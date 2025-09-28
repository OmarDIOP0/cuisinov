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
            if (_accessor.HttpContext == null)
                return await base.SendAsync(request, cancellationToken);

            var session = _accessor.HttpContext.Session;
            var token = session.GetString("access_token");
            var refreshToken = session.GetString("refresh_token");
            var expireAt = session.GetCustomObjectFromSession<DateTime?>("expire_at") ?? DateTime.Now;

            if (string.IsNullOrWhiteSpace(token) || DateTime.UtcNow >= expireAt.AddSeconds(-30))
            {
                if (!string.IsNullOrWhiteSpace(refreshToken))
                {
                    var refreshRequest = new RefreshTokenRequest
                    {
                        AccessToken = token ?? string.Empty,
                        RefreshToken = refreshToken
                    };

                    var httpClient = _httpClientFactory.CreateClient();
                    var response = await httpClient.PostAsJsonAsync(ApiUrlGeneric.RefreshTokenURL, refreshRequest, cancellationToken);

                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        session.Clear();
                        await _accessor.HttpContext.SignOutAsync();
                    }
                    else if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            var apiResponse = await ApiResultParser<Token>.Parse(response);
                            if (apiResponse.Data != null)
                            {
                                token = apiResponse.Data.AccessToken;
                                session.SetString("access_token", token);
                                session.SetString("refresh_token", apiResponse.Data.RefreshToken);
                                session.SetObjectInSession("expire_at", apiResponse.Data.ExpireAt);
                            }
                        }
                        catch
                        {
                            session.Clear();
                            await _accessor.HttpContext.SignOutAsync();
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }

    }
}