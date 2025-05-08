namespace SosyalMedya_Web.Models
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }

    public class ApiDataResponse<T> : ApiResponse
    {
        public new T Data { get; set; }
    }

    public class ApiListDataResponse<T> : ApiResponse
    {
        public new List<T> Data { get; set; }
    }
} 