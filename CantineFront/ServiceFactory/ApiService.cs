using CantineBack.Models;
using CantineFront.Models.Request;
using DPWorldMobile.ServiceFactory;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace CantineFront.ServiceFactory
{
    public class ApiService<T> where T : class
    {

        /// <summary>
        /// Call Api Post
        /// </summary>
        /// <param name="url"></param>
        /// <param name="ob"></param>
        /// <returns></returns>
        public static async Task<ApiResponse<T>> CallApiPost(IHttpClientFactory _httpClientFactory, string url, object ob)
        {

            var httpClient = _httpClientFactory.CreateClient("ClearanceApi");
            var json = JsonConvert.SerializeObject(ob);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync(url, data);
            var obj = await ApiResultParser<T>.Parse(response);
            return obj;

        }
        /// <summary>
        /// Call Api Post
        /// </summary>
        /// <param name="url"></param>
        /// <param name="ob"></param>
        /// <returns></returns>
        public static async Task<ApiResponse<T>> CallApiPut(IHttpClientFactory _httpClientFactory, string url, object ob)
        {

            var httpClient = _httpClientFactory.CreateClient("ClearanceApi");
            var json = JsonConvert.SerializeObject(ob);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response ;
            if (ob != null) {
                 response = await httpClient.PutAsync(url, data);
            }
            else
            {
                 response = await httpClient.PutAsync(url, null);
            }
          
            var obj = await ApiResultParser<T>.Parse(response);
            return obj;

        }

        /// <summary>
        /// Call Api Post
        /// </summary>
        /// <param name="url"></param>
        /// <param name="ob"></param>
        /// <returns></returns>
        public static async Task<ApiResponse<T>> CallApiPatch(IHttpClientFactory _httpClientFactory, string url, object ob)
        {

            var httpClient = _httpClientFactory.CreateClient("ClearanceApi");
            var json = JsonConvert.SerializeObject(ob);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response;
            if (ob != null)
            {
                response = await httpClient.PatchAsync(url, data);
            }
            else
            {
                response = await httpClient.PatchAsync(url, null);
            }

            var obj = await ApiResultParser<T>.Parse(response);
            return obj;

        }
        /// <summary>
        /// Call Api Post
        /// </summary>
        /// <param name="url"></param>
        /// <param name="ob"></param>
        /// <returns></returns>
        public static async Task<ApiResponse<T>> CallApiDelete(IHttpClientFactory _httpClientFactory, string url)
        {

            var httpClient = _httpClientFactory.CreateClient("ClearanceApi");
            HttpResponseMessage response = await httpClient.DeleteAsync(url);
            var obj = await ApiResultParser<T>.Parse(response);
            return obj;

        }
        /// <summary>
        /// Call Api Post
        /// </summary>
        /// <param name="url"></param>
        /// <param name="ob"></param>
        /// <returns></returns>
        public static async Task<ApiResponse<List<T>>> CallGetList(IHttpClientFactory _httpClientFactory, string url)
        {

            var httpClient = _httpClientFactory.CreateClient("ClearanceApi");
    
            HttpResponseMessage response = await httpClient.GetAsync(url);
            return await ApiResultParser<T>.ParseList(response);
            
        }

        public static async Task<ApiResponse<T>> CallGet(IHttpClientFactory _httpClientFactory, string url)
        {

            var httpClient = _httpClientFactory.CreateClient("ClearanceApi");

            HttpResponseMessage response = await httpClient.GetAsync(url);
            return await ApiResultParser<T>.Parse(response);

        }
    }
}
