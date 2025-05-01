namespace SosyalMedya_Web.Models
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class ApiDataResponse<T> : ApiResponse
    {
        public T Data { get; set; }
    }

    public class ApiListDataResponse<T> : ApiResponse
    {
        public List<T> Data { get; set; }
    }
} 