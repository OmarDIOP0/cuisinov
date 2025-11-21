using DPWorldMobile.ServiceFactory;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CantineFront.ServiceFactory
{
    public class ApiResultParser<T> where T : class
    {

        // Retourne true si la chaîne commence par un objet ou un tableau JSON
        private static bool IsJson(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            var first = s.TrimStart()[0];
            return first == '{' || first == '[';
        }

        private static string ExtractProblemMessage(string content, string? mediaType)
        {
            // Si le contenu est JSON on tente de désérialiser ProblemDetails
            if (IsJson(content) || (mediaType != null && mediaType.Contains("json")))
            {
                try
                {
                    var pblm = JsonConvert.DeserializeObject<ProblemDetails>(content);
                    if (pblm != null && !string.IsNullOrWhiteSpace(pblm.Detail))
                        return pblm.Detail;
                }
                catch
                {
                    // ignore - fall back to raw content
                }
            }
            // Si ce n'est pas du JSON ou si la désérialisation a échoué, renvoyer le texte brut (trimmed)
            return string.IsNullOrWhiteSpace(content) ? "Une erreur a été rencontrée." : content.Trim();
        }

        public static async Task<ApiResponse<T>> Parse(HttpResponseMessage response)
        {
            HttpContent content = response.Content;
            string result = await content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                var message = ExtractProblemMessage(result, content.Headers.ContentType?.MediaType);
                return new ApiResponse<T>
                {
                    Data = null,
                    Message = message ?? "Une erreur a été rencontrée.",
                    StatusCode = response.StatusCode,
                    Success = false
                };
            }

            if (!response.IsSuccessStatusCode)
            {
                // Pour d'autres codes d'erreur, retourner le message brut si non JSON
                var msg = ExtractProblemMessage(result, content.Headers.ContentType?.MediaType);
                return new ApiResponse<T>
                {
                    Data = null,
                    Message = msg,
                    StatusCode = response.StatusCode,
                    Success = false
                };
            }

            try
            {
                var data = JsonConvert.DeserializeObject<T>(result);
                return new ApiResponse<T>
                {
                    Data = data,
                    Message = "OK",
                    StatusCode = response.StatusCode,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    Data = null,
                    Message = $"Erreur de désérialisation JSON: {ex.Message}",
                    StatusCode = response.StatusCode,
                    Success = false
                };
            }
        }


        public static async Task<ApiResponse<List<T>>> ParseList(HttpResponseMessage response)
        {
            HttpContent content = response.Content;
            string result = await content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                var message = ExtractProblemMessage(result, content.Headers.ContentType?.MediaType);
                return new ApiResponse<List<T>>
                {
                    Data = null,
                    Message = message ?? "Une erreur a été rencontrée.",
                    StatusCode = response.StatusCode,
                    Success = false
                };
            }

            if (!response.IsSuccessStatusCode)
            {
                var msg = ExtractProblemMessage(result, content.Headers.ContentType?.MediaType);
                return new ApiResponse<List<T>>
                {
                    Data = null,
                    Message = msg,
                    StatusCode = response.StatusCode,
                    Success = false
                };
            }

            try
            {
                var data = JsonConvert.DeserializeObject<List<T>>(result);
                return new ApiResponse<List<T>>
                {
                    Data = data,
                    Message = "OK",
                    StatusCode = response.StatusCode,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<T>>
                {
                    Data = null,
                    Message = $"Erreur de désérialisation JSON: {ex.Message}",
                    StatusCode = response.StatusCode,
                    Success = false
                };
            }
        }
    }
}