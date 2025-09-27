using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CantineFront.Models.Response
{
    public class PostPutResponse<T> where T :class
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Object { get; set; }
        public IDictionary<string, string[]> Errors { get; set; }
    }
}