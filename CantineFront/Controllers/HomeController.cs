using CantineBack.Models;
using CantineBack.Models.Dtos;
using CantineFront.Helpers;
using CantineFront.Identity;
using CantineFront.Models;
using CantineFront.Models.Request;
using CantineFront.ServiceFactory;
using CantineFront.Services;
using CantineFront.Static;
using CantineFront.Utils;
using DPWorldMobile.ServiceFactory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http;

namespace CantineFront.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        IHttpClientFactory _httpClientFactory;
        public HomeController(IHttpClientFactory httpClientFactory, ILogger<HomeController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int categorieId)
        {
            ViewBag.CategorieId = categorieId;
            if (HttpContext.Session.GetListObjectFromSession<PaymentMethod>("PaymentMethods") == null)
            {
                var urlPaymentMethods = ApiUrlGeneric.ReadAllURL<PaymentMethod>();
                var apiPaymentMethods = await ApiService<PaymentMethod>.CallGetList(_httpClientFactory, urlPaymentMethods);
                if (apiPaymentMethods.Data != null)
                {

                    HttpContext.Session.SetListInSession("PaymentMethods", apiPaymentMethods.Data);
                }

            }
            return View();
        }
        public async Task<IActionResult> Menu()
        {
            if (HttpContext.Session.GetListObjectFromSession<PaymentMethod>("PaymentMethods")==null)
            {
                var urlPaymentMethods = ApiUrlGeneric.ReadAllURL<PaymentMethod>();
                var apiPaymentMethods = await ApiService<PaymentMethod>.CallGetList(_httpClientFactory, urlPaymentMethods);
                if (apiPaymentMethods.Data != null)
                {

                    HttpContext.Session.SetListInSession("PaymentMethods", apiPaymentMethods.Data);
                }

            }

            return View();
        }



        [Authorize(Roles = "USER")]
        public IActionResult Book()
        {
            return View();
        }
        
        public IActionResult BookShop()
        {
            return View();
        }
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public IActionResult BookValidation()
        {
            return View();
        }
        public IActionResult About()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Profil()
        {
            return View();  
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public JsonResult AddArticle(Article articleCart)
        {
            try
            {
                if (articleCart != null)
                {
                    var cartBadgeCount = HttpContext.Session.SetAddToListInSession<Article>("ArticlesCart", articleCart);
                    HttpContext.Session.SetInt32("CartBadgeCount", cartBadgeCount);
                    return Json(new FormResponse { Success = true, Message = "Article ajouté avec succès!", Object = cartBadgeCount });
                }

                return Json(new FormResponse { Success = false, Message = "Une erreur a été rencontrée , article is null" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(new FormResponse { Success = false, Message = "Une erreur a été rencontrée!" });
            }

        }
        [HttpDelete]
        public JsonResult DeleteAllArticleCart()
        {
            try
            {
             
                    int cartBadgeCount = HttpContext.Session.SetListInSession<Article>("ArticlesCart", new List<Article>());
                    HttpContext.Session.SetInt32("CartBadgeCount", 0);

                    return Json(new FormResponse { Success = true, Message = "Panier vidé avec succès!", Object = 0 });
            
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(new FormResponse { Success = false, Message = "Une erreur a été rencontrée!" });
            }


        }
        [HttpGet]
        public async Task<JsonResult> GetCurrentUser()
        {

            var url = String.Format(ApiUrlGeneric.ReadOneURL<User>(), HttpContext.Session.GetInt32("UserId"));
            var httpClient = _httpClientFactory.CreateClient("ClearanceApi");
            HttpResponseMessage response = await httpClient.GetAsync(url);
            var apiResponse = await ApiResultParser<User>.Parse(response);
            return Json(apiResponse);
        }
        [HttpPost]
        public async Task<JsonResult> ResetPassword([FromBody] UserResetPwdRequest userResetPwdRequest)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<UserResetPwdRequest>.ModelStateError(ModelState));
            }
            var url = ApiUrlGeneric.ResetPasswordURL;

            var apiResponse = await ApiService<UserResetPwdRequest>.CallApiPost(_httpClientFactory, url, userResetPwdRequest);

            bool success = apiResponse.StatusCode == System.Net.HttpStatusCode.NoContent;
            if (success)
            {
                await ApiService<String>.CallApiPost(_httpClientFactory, ApiUrlGeneric.RevokeTokenURL, null);
            }
            string msg = success ? "Mot de passe modifié avec succès!" : (apiResponse.Message ?? "Une erreur a été rencontrée.");

            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });
        }

        [HttpDelete]
        public JsonResult DeleteArticleCart(int id)
        {
            try
            {
                if (id > 0)
                {
                    var listArticlesCart = HttpContext.Session.GetListObjectFromSession<Article>("ArticlesCart");
                    if (listArticlesCart == null)
                    {
                        return Json(new FormResponse { Success = true, Message = "Aucun article n'est disponible dans le panier!", Object = 0 });
                    }
                    listArticlesCart = listArticlesCart?.Where(a => a.Id != id)?.ToList();

                    int cartBadgeCount = HttpContext.Session.SetListInSession<Article>("ArticlesCart", listArticlesCart);
                    HttpContext.Session.SetInt32("CartBadgeCount", cartBadgeCount);

                    return Json(new FormResponse { Success = true, Message = "L'article a été supprimé du panier avec succès!", Object = cartBadgeCount });
                }

                return Json(new FormResponse { Success = false, Message = "Une erreur a été rencontrée , article is null" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(new FormResponse { Success = false, Message = "Une erreur a été rencontrée!" });
            }

        }
    }
}