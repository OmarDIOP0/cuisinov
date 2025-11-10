using AutoMapper.Internal;
using CantineBack.Models;
using CantineBack.Models.Dtos;
using CantineBack.Models.Enums;
using CantineFront.Helpers;
using CantineFront.Models.Request;
using CantineFront.ServiceFactory;
using CantineFront.Services;
using CantineFront.Static;
using CantineFront.Utils;
using DPWorldMobile.ServiceFactory;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Security.Claims;

namespace CantineFront.Controllers
{
    public class LoginController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LoginController(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
        {

            _httpClientFactory = httpClientFactory;

        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }

       

        [HttpPost]
        public async Task<JsonResult> Register(UserCURequest userRequest)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<User>.ModelStateError(ModelState));
            }
            
            var url = String.Format(ApiUrlGeneric.RegisterURL);
            var apiResponse = await ApiService<UserRegisterResponse>.CallApiPost(_httpClientFactory, url, userRequest);

            bool success = apiResponse.Data != null;

            string msg = success ? "Utilisateur crée avec succès!" : (apiResponse.Message ?? "Une erreur a été rencontrée.");
            if (success)
            {
                UserReadDto user = apiResponse.Data!.User!;
                string token = apiResponse.Data!.Token!;
                string refreshToken = apiResponse.Data!.RefreshToken!;
                DateTime expireAt = apiResponse.Data!.TokenExpireAt!;
                if (user != null)
                {
                    HttpContext.Session.SetString("access_token", token);
                    HttpContext.Session.SetString("refresh_token", refreshToken);
                    HttpContext.Session.SetObjectInSession("expire_at", expireAt);
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    HttpContext.Session.SetString("Username", user.Login);
                    HttpContext.Session.SetInt32("EntrepriseId", user.EntrepriseId ?? 0);
                    HttpContext.Session.SetInt32("Solde", user.Solde);
                    HttpContext.Session.SetString("IsAdmin", (user.Profile == "ADMIN").ToString());
                    HttpContext.Session.SetString("IsGerant", (user.Profile == "GERANT").ToString());
                    HttpContext.Session.SetString("Logged", "true");

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Prenom+ " "+user.Nom),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Role, user.Profile ?? "")
                    };
                    claims.Add(new Claim(ClaimTypes.Role, user.Profile!.ToUpper()));

                    await HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies", ClaimTypes.Name, ClaimTypes.Role)));



                    var urlEmplacements = ApiUrlGeneric.ReadAllURL<Emplacement>();
                    var urlPaymentMethods = ApiUrlGeneric.ReadAllURL<PaymentMethod>();
                    var tasks = new Tuple<Task<ApiResponse<List<Emplacement>>>, Task<ApiResponse<List<PaymentMethod>>>>(
                        ApiService<Emplacement>.CallGetList(_httpClientFactory, urlEmplacements),
                        ApiService<PaymentMethod>.CallGetList(_httpClientFactory, urlPaymentMethods)
                        );
                    await Task.WhenAll(tasks.Item1,
                  tasks.Item2
                  );


                    var apiEmplacements = tasks.Item1.Result;
                    var apiPaymentMethods = tasks.Item2.Result;

                    var entrepriseId = user.EntrepriseId ?? 0;
                    var filteredEmplacements = new List<Emplacement>();

                    if (apiEmplacements.Data != null)
                    {
                        if(entrepriseId > 0)
                        {
                             filteredEmplacements = apiEmplacements.Data
                              .Where(e => !e.EntrepriseId.HasValue || e.EntrepriseId == entrepriseId)
                              .ToList();
                            HttpContext.Session.SetListInSession("Emplacements", filteredEmplacements);
                        }
                        else
                        {
                            HttpContext.Session.SetListInSession("Emplacements", apiEmplacements.Data);
                        }
                    }

                    if (apiPaymentMethods.Data != null)
                    {

                        HttpContext.Session.SetListInSession("PaymentMethods", apiPaymentMethods.Data);
                    }
                }
                else
                {
                    return Json(new
                    {
                        Message = "Utilisateur introuvable dans le système!",
                        Success = false
                    });
                }

            }
            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });
        }
        [HttpPost]
        public async Task<JsonResult> SignIn(string username, string password)
        {

            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {

                Tuple<bool, List<string>> res = new Tuple<bool, List<string>>(true, new List<string>()); 
                bool resetPassword = false;
                var url = String.Format(ApiUrlGeneric.AuthenticateURL, username, password);
                var apiResponse = await ApiService<UserLoginResponse>.CallApiPost(_httpClientFactory, url,null);
                bool success = apiResponse.Data?.User != null && !String.IsNullOrWhiteSpace(apiResponse.Data?.Token);
                if (success)
                {
                    UserReadDto user = apiResponse.Data!.User!;
                    string token = apiResponse.Data!.Token!;
                    string refreshToken = apiResponse.Data!.RefreshToken!;
                    DateTime expireAt = apiResponse.Data!.TokenExpireAt!;
                    if(user != null)
                    {
                        resetPassword = user.ResetPassword ?? false;
                        HttpContext.Session.SetString("access_token", token);
                        HttpContext.Session.SetString("refresh_token", refreshToken);
                        HttpContext.Session.SetObjectInSession("expire_at", expireAt);
                        HttpContext.Session.SetInt32("UserId", user.Id);
                       
                        HttpContext.Session.SetString("Username", username);
                        HttpContext.Session.SetInt32("Solde", user.Solde);
                        HttpContext.Session.SetInt32("EntrepriseId", user.EntrepriseId ?? 0);
                        HttpContext.Session.SetString("IsAdmin", (user.Profile == "ADMIN").ToString());
                        HttpContext.Session.SetString("IsGerant", (user.Profile == "GERANT").ToString());
                        HttpContext.Session.SetString("Logged", "true");

                        var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Login),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Role, "USER")
                    };
                        claims.Add(new Claim(ClaimTypes.Role, user.Profile!.ToUpper()));

                        await HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies", ClaimTypes.Name, ClaimTypes.Role)));



                        var urlEmplacements = ApiUrlGeneric.ReadAllURL<Emplacement>();
                        var urlPaymentMethods = ApiUrlGeneric.ReadAllURL<PaymentMethod>();
                        var tasks = new Tuple<Task<ApiResponse<List<Emplacement>>>, Task<ApiResponse<List<PaymentMethod>>>>(
                            ApiService<Emplacement>.CallGetList(_httpClientFactory, urlEmplacements),
                            ApiService<PaymentMethod>.CallGetList(_httpClientFactory, urlPaymentMethods)
                            );
                        await Task.WhenAll(tasks.Item1,
              tasks.Item2
              );
                     

                        var apiEmplacements = tasks.Item1.Result;
                        var apiPaymentMethods = tasks.Item2.Result;

                        var entrepriseId = user.EntrepriseId ?? 0;
                        var filteredEmplacements = new List<Emplacement>();
                        if (apiEmplacements.Data != null)
                        {
                            if (entrepriseId > 0)
                            {
                                filteredEmplacements = apiEmplacements.Data
                                 .Where(e => !e.EntrepriseId.HasValue || e.EntrepriseId == entrepriseId)
                                 .ToList();
                                HttpContext.Session.SetListInSession("Emplacements", filteredEmplacements);
                            }
                            else
                            {
                                HttpContext.Session.SetListInSession("Emplacements", apiEmplacements.Data);
                            }
                        }

                        if (apiPaymentMethods.Data != null)
                        {

                            HttpContext.Session.SetListInSession("PaymentMethods", apiPaymentMethods.Data);
                        }
                    }
                    else
                    {
                        return Json(new
                        {
                            Message = "Utilisateur introuvable dans le système!",
                            Success = false
                        });
                    }

                }
                string resetPasswordMessage = resetPassword ? " Veuillez configurer votre nouveau mot de passe.":String.Empty;
                return Json(new
                {   ResetPassword= success&&resetPassword,
                    Message = success ? ("Connexion réussie!"+ resetPasswordMessage) : "Échec de la connexion, nom d'utilisateur ou mot de passe incorrect !",
                    Success = success
                });
            }
            return Json(new
            {
                Message = "username or password incorrect!",
                Success = false
            });
        }

        [Authorize]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Logout()
        {

            var apiResponse = await ApiService<String>.CallApiPost(_httpClientFactory, ApiUrlGeneric.RevokeTokenURL, null);
            HttpContext.Session.SetInt32("UserId", 0);
            HttpContext.Session.SetString("IsAdmin", "false");
            HttpContext.Session.SetString("IsGerant", "false");
            HttpContext.Session.SetString("Logged", "false");

            HttpContext.Session.SetString("Username", String.Empty);
            HttpContext.Session.SetString("Matricule", String.Empty);

            HttpContext.Session.SetInt32("Solde", 0);

            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<JsonResult> ResetPassword(UserResetPwdRequest userResetPwdRequest)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<Categorie>.ModelStateError(ModelState));
            }
            var url = ApiUrlGeneric.ResetPasswordURL;

            var apiResponse = await ApiService<Categorie>.CallApiPost(_httpClientFactory, url, userResetPwdRequest);


            bool success = apiResponse.StatusCode == System.Net.HttpStatusCode.NoContent;
            if (success)
            {
                await ApiService<String>.CallApiPost(_httpClientFactory, ApiUrlGeneric.RevokeTokenURL, null);
            }
            string msg = success ? "Mot de passe modifié avec succès!" : (apiResponse.Message ?? "Une erreur a été rencontrée.");

            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });
        }
        [HttpPost]
        public async Task<JsonResult> ForgotPassword(UserResetPwdRequest userResetPwdRequest)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<Categorie>.ModelStateError(ModelState));
            }
            var url = ApiUrlGeneric.ResetPasswordURL;

            var apiResponse = await ApiService<Categorie>.CallApiPost(_httpClientFactory, url, userResetPwdRequest);


            bool success = apiResponse.StatusCode == System.Net.HttpStatusCode.NoContent;
            if (success)
            {
                await ApiService<String>.CallApiPost(_httpClientFactory, ApiUrlGeneric.RevokeTokenURL, null);
            }
            string msg = success ? "Mot de passe modifié avec succès!" : (apiResponse.Message ?? "Une erreur a été rencontrée.");

            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });
        }
    }
    }
