using System.Net;

namespace DPWorldMobile.ServiceFactory
{
    public class ApiResponse<T> where T:class
    {
        public HttpStatusCode StatusCode { get; set; }
        public string? Message { get; set; }
        public string? Status => Success ?(Data!=null?"sucess":"error") : "error";
        public bool Success { get; set; }
        public T? Data { get; set; }

        public IDictionary<string, string[]>? Errors { get; set; }
    }
}