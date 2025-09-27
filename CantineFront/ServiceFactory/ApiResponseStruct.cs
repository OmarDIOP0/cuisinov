using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace DPWorldMobile.ServiceFactory
{   
    public class ApiResponseStruct<T> where T : struct
    {        
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
        public T Data { get; set; }
    }
}