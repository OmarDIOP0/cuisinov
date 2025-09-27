using CantineBack.Models;
using CantineBack.Models.Dtos;
using CantineFront.Helpers;
using CantineFront.ServiceFactory;
using CantineFront.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace CantineFront.Hubs
{
    public sealed class BackendApiAuthenticationHttpClientHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHttpClientFactory _httpClientFactory;
       // private readonly ILogger<BackendApiAuthenticationHttpClientHandler> _logger;

        public BackendApiAuthenticationHttpClientHandler(IHttpContextAccessor accessor, IHttpClientFactory httpClientFactory )
        {
            _accessor = accessor;
            _httpClientFactory = httpClientFactory;
         
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //var expat = await _accessor.HttpContext.GetTokenAsync("expires_at");
            //var dataExp = DateTime.Parse(expat, null, DateTimeStyles.RoundtripKind);
            //if ((dataExp - DateTime.Now).TotalMinutes < 10)
            //{
            //    //SNIP GETTING A NEW TOKEN IF ITS ABOUT TO EXPIRE
            //}

            if(_accessor.HttpContext == null) {

                return await base.SendAsync(request, cancellationToken);
            }
            var token = _accessor.HttpContext.Session.GetString("access_token");
            var refresh_token = _accessor.HttpContext.Session.GetString("refresh_token");
            var expire_at = _accessor.HttpContext.Session.GetCustomObjectFromSession<DateTime?>("expire_at") ?? DateTime.Now;

            if (String.IsNullOrWhiteSpace(token) || (DateTime.Now >= expire_at))
            {

                if (!String.IsNullOrWhiteSpace(token) && !String.IsNullOrWhiteSpace(refresh_token))
                {

                    var refreshTokenRequest = new RefreshTokenRequest  {
                        AccessToken = token!,
                        RefreshToken = refresh_token! };

                    var httpClient=new HttpClient();    
                    HttpResponseMessage response = await httpClient.PostAsJsonAsync(ApiUrlGeneric.RefreshTokenURL, refreshTokenRequest);

                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest || response?.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        //Refresh Token is Expired Or Invalid client request.
                       
                        
                        _accessor.HttpContext.Session.Clear();
                        await _accessor.HttpContext.SignOutAsync();

                    }
                    else
                    {

                        try
                        {
                            var apiResponse = await ApiResultParser<Token>.Parse(response!);
                            if (apiResponse.Data != null)
                            {
                                _accessor.HttpContext.Session.SetString("access_token", apiResponse.Data.AccessToken);
                                _accessor.HttpContext.Session.SetString("refresh_token", apiResponse.Data.RefreshToken);
                                _accessor.HttpContext.Session.SetObjectInSession("expire_at", apiResponse.Data.ExpireAt);

                            }
                            
                        }
                        catch (Exception ex)
                        {
                            
                            _accessor.HttpContext.Session.Clear();
                            await _accessor.HttpContext.SignOutAsync();
                            //throw;
                        }
                        
                       
                    }
                   


                }


            }


            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await base.SendAsync(request, cancellationToken);
            // Use the token to make the call.

        }
    }
}