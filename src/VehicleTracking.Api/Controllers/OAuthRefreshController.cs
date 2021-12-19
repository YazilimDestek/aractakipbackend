using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace VehicleTracking.Api.Controllers
{
    public class OAuthRefreshController : Controller
    {
        private readonly IHttpClientFactory _httpclientFactory;

        public OAuthRefreshController(IHttpClientFactory httpClientFactory)
        {
            _httpclientFactory = httpClientFactory;

        }

        [Authorize]
        public async Task<ActionResult> Client()
        {
            var serverResponse = await AccessTokenRefreshWrapper(() => SecuredGetRequest("Server_path_will_be_come_here"));

            var apiResponse = await AccessTokenRefreshWrapper(() => SecuredGetRequest("Api_path_will_be_come_here"));

            return Ok();
        }

        private async Task<HttpResponseMessage> SecuredGetRequest(string url)
        {
            var client = _httpclientFactory.CreateClient();
            var token = await HttpContext.GetTokenAsync("access_token");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer{token}");
            return await client.GetAsync(url);
        }

        public async Task<HttpResponseMessage> AccessTokenRefreshWrapper(Func<Task<HttpResponseMessage>> initialRequest)
        {
            var response = await initialRequest();

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await RefreshAccessToken();
                response = await initialRequest();
            }

            return response;
        }

        private async Task RefreshAccessToken()
        {

            var refreshToken = await HttpContext.GetTokenAsync("refresh_token");

            var refreshTokenClient = _httpclientFactory.CreateClient();

            var requestData = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "Server_path_will_be_come_here")
            {
                Content = new FormUrlEncodedContent(requestData)
            };

            var basicCredentials = "username:password";
            var encodedCredentials = Encoding.ASCII.GetBytes(basicCredentials);
            var base64Credentials = Convert.ToBase64String(encodedCredentials);

            request.Headers.Add("Authortization", $"Basic{base64Credentials}");

            var response = await refreshTokenClient.SendAsync(request);

            var responseString = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

            var newAccessToken = responseData.GetValueOrDefault("access_token");
            var newRefreshToken = responseData.GetValueOrDefault("refresh_token");

            var authInfo = await HttpContext.AuthenticateAsync("OAuthClientCookie");

            authInfo.Properties.UpdateTokenValue("access_token", newAccessToken);
            authInfo.Properties.UpdateTokenValue("refresh_token", newRefreshToken);

            await HttpContext.SignInAsync("OAuthClientCookie", authInfo.Principal, authInfo.Properties);
        }

    }
}
