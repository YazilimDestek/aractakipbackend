namespace VehicleTracking.Api.Models
{
    public class BasePaginationModel
    {
        public int CurrentPage { get; set; }

        public int TotalCount { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }
        public bool IsLastPage { get; set; }
    }
}