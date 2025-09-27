using DPWorldMobile.ServiceFactory;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CantineFront.ServiceFactory
{
    public class ApiResultParser<T> where T : class
    {



        public static async Task<ApiResponse<T>> Parse(HttpResponseMessage response)
        {

            HttpContent content = response.Content;
            string result = await content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {

                ProblemDetails? pblm = JsonConvert.DeserializeObject<ProblemDetails>(result);
                return new ApiResponse<T>
                {
                    Data = null,
                    Message = pblm?.Detail ?? "Une erreur a été rencontrée.",
                    StatusCode = response.StatusCode,
                    Success = false
                };
            }
      
   
            return new ApiResponse<T>
            {
                Data = JsonConvert.DeserializeObject<T>(result),
                Message = "OK",
                StatusCode = response.StatusCode,
                Success = true
            };
        }


        public static async Task<ApiResponse<List<T>>> ParseList(HttpResponseMessage response)
        {
            HttpContent content = response.Content;
            string result = await content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {

                ProblemDetails? pblm = JsonConvert.DeserializeObject<ProblemDetails>(result);
                return new  ApiResponse<List<T>>
                {
                    Data = null,
                    Message = pblm?.Detail ?? "Une erreur a été rencontrée.",
                    StatusCode = response.StatusCode,
                    Success = false
                };
            }


            return new ApiResponse<List<T>>
            {
                Data = JsonConvert.DeserializeObject<List<T>>(result),
                Message = "OK",
                StatusCode = response.StatusCode,
                Success = true
            };
        }
    }
}
