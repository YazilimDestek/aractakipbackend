using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HesapCo.Entity.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using VehicleTracking.Api.Security;
using VehicleTracking.Entity.Persistence;
using Swashbuckle.AspNetCore.Swagger;
using System.Reflection;
using Swashbuckle.Swagger;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Text;
using System.Security.Claims;
using Newtonsoft.Json;

namespace VehicleTracking.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddSwaggerGen(c => 
                {
                    c.CustomSchemaIds(x => x.FullName);
                    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Api Docs", Version = "v1", Description = "Araç takip api" });
                }
            );
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddControllers();
            services.AddDbContext<VehicleTrackingDbContext>(options => options.UseNpgsql(Configuration.GetConnectionString("Default")));
            // ----- AUTHENTICATION
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = "192.168.2.70",
                            ValidAudience = "192.168.2.70",
                            IssuerSigningKey = JwtSecurityKey.Create("CINIGAZ2019-92223K-324957-K3596U")
                        };
                
                });
            services.AddAuthentication(options =>
            {
                // We check the cookie to condirm that we are authenticated
                options.DefaultAuthenticateScheme = "OAuthClientCookie";
                // When we sign in we will deal out a cookie
                options.DefaultSignInScheme = "OAuthClientCookie";
                // Use this check if we are allowed to do something
                options.DefaultChallengeScheme = "OAuthServer";
            })
                .AddCookie("OAuthClientCookie")
                .AddOAuth("OAuthServer", options =>
                {
                    options.ClientId = "client_id";
                    options.ClientSecret = "client_secret";
                    options.CallbackPath = "/OAuth/Callback";
                    options.AuthorizationEndpoint = "http://localhost:19729/";  // Oauth_Server_Path_wil_be_come_over_here
                    options.TokenEndpoint = "http://localhost:19729/";   // Oauth_Server_Path_wil_be_come_over_here

                    options.SaveTokens = true;

                    options.Events = new OAuthEvents()
                    {
                        OnCreatingTicket = context =>
                        {
                            var accessToken = context.AccessToken;
                            var base64Payload = accessToken.Split('.')[1];
                            var bytes = Convert.FromBase64String(base64Payload);
                            var jsonPayload = Encoding.ASCII.GetString(bytes);
                            var claims = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonPayload);
                            foreach (var claim in claims)
                            {
                                context.Identity.AddClaim(new Claim(claim.Key, claim.Value));
                            }
                            return Task.CompletedTask;
                        }
                    };

                });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllHeaders",
                      builder =>
                      {
                          builder.AllowAnyOrigin()
                                 .AllowAnyHeader()
                                 .AllowAnyMethod();
                      });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, VehicleTrackingDbContext context)
        {
            app.UseCors("AllowAllHeaders");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("swagger/v1/swagger.json", "API V1");
                    c.RoutePrefix = string.Empty;
                }
            );

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            VehicleTrackingDbInitializer.Initialize(context);
        }
    }
}
