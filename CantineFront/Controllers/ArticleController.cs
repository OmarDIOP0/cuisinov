using CantineBack.Models;
using CantineBack.Models.Dtos;
using CantineBack.Models.Enums;
using CantineFront.Helpers;
using CantineFront.Identity;
using CantineFront.Models.Request;
using CantineFront.ServiceFactory;
using CantineFront.Services;
using CantineFront.Static;
using CantineFront.Utils;
using CantineFront.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Data;
using System.Net.Http;
using System.Text;

namespace CantineFront.Controllers
{
    public class ArticleController : Controller
    {
        ArticleViewModel ArticleVM;
        private readonly IHttpClientFactory _httpClientFactory;
        IOptions<AppSettings> _appSettings;
        public ArticleController(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
        {
            try
            {
                _httpClientFactory = httpClientFactory;
                _appSettings = appSettings;
            }
            catch (Exception ex)
            {

            }

        }
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<JsonResult> GetArticles(int? categorieId)
        {

            var url =String.Format(  ApiUrlGeneric.ReadArticlesByCategoryURL, categorieId);
            var apiResponse = await ApiService<Article>.CallGetList(_httpClientFactory, url);
            return Json(apiResponse);
        }
        public async Task<JsonResult> GetMenu(int? categorieId)
        {

            var url = String.Format( ApiUrlGeneric.GetMenuArticlesURL,categorieId);
            var apiResponse = await ApiService<Article>.CallGetList(_httpClientFactory, url);
            return Json(apiResponse);
        }
    
    public async Task<JsonResult> GetArticleImage(int id)
    {
            ArticleImageDto? image=null;
            var ListImages = HttpContext.Session.GetListObjectFromSession<ArticleImageDto>("ImagesArticles");
            if (ListImages != null)
            {

                 image=ListImages.Where(a=>a.Id==id).FirstOrDefault();

                if(image ==null)
                {

                    var url = String.Format(ApiUrlGeneric.ReadOneURL<Article>(), id);
                    var httpClient = _httpClientFactory.CreateClient("ClearanceApi");
                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    var apiResponse = await ApiResultParser<Article>.Parse(response);

                    var article = apiResponse.Data;
                    if(article != null)
                    {
                        image = new ArticleImageDto { Id = article.Id, Image = article.Image };
                        HttpContext.Session.SetAddToListInSession<ArticleImageDto>("ImagesArticles",image);
                    }
                }

            }
            else
            {
     
                var apiResponseList = await ApiService<ArticleImageDto>.CallGetList(_httpClientFactory, ApiUrlGeneric.GetArticleImagesURL);
                HttpContext.Session.SetListInSession<ArticleImageDto>("ImagesArticles",apiResponseList.Data);

                 image = apiResponseList.Data?.Where(a => a.Id == id).FirstOrDefault();
            }

          
        return Json(image);
    }


    [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<IActionResult> Create()
        {
            ArticleVM = new ArticleViewModel();
            var url =  ApiUrlGeneric.ReadAllURL<Categorie>();
            var apiResponse = await ApiService<Categorie>.CallGetList(_httpClientFactory, url);
            var urlEntreprise = ApiUrlGeneric.ReadAllURL<Entreprise>();
            var apiResponseListEntreprises = await ApiService<Entreprise>.CallGetList(_httpClientFactory, urlEntreprise);
            ArticleVM.Categories = apiResponse?.Data;
            ArticleVM.Entreprises = apiResponseListEntreprises?.Data;

            return View(ArticleVM);
        }
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public IActionResult List()
        {

            return View();
        }
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        [HttpPost]
        public async Task<JsonResult> CreateArticle(ArticleCURequest articleRequest, IFormFile Image)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<Article>.ModelStateError(ModelState));
            }


            if (Image != null)
            {
                if (Image.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        Image.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        articleRequest.Image = fileBytes;
                    }
                }
            }
            var url =  ApiUrlGeneric.CreateURL<Article>();

            var apiResponse = await ApiService<Article>.CallApiPost(_httpClientFactory, url, articleRequest);
        

            bool success = apiResponse.Data != null;
           
            string msg = success ? "Article crée avec succès!" : (apiResponse.Message ?? "Une erreur a été rencontrée.");
            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });



        }


        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<IActionResult> Update(int? id)
        {

            if (id.HasValue)
            {
                if (ArticleVM == null) ArticleVM = new ArticleViewModel();
                var url = String.Format( ApiUrlGeneric.ReadOneURL<Article>(), id);
                var httpClient = _httpClientFactory.CreateClient("ClearanceApi");
                HttpResponseMessage response = await httpClient.GetAsync(url);
                var apiResponse = await ApiResultParser<Article>.Parse(response);
                var urlEntreprise = ApiUrlGeneric.ReadAllURL<Entreprise>();
                var apiResponseListEntreprises = await ApiService<Entreprise>.CallGetList(_httpClientFactory, urlEntreprise);

                var article = apiResponse.Data;
                if (article != null)
                {
                    //Get Categories
                    url =  ApiUrlGeneric.ReadAllURL<Categorie>();
                    response = await httpClient.GetAsync(url);
                    var catApiResponse = await ApiResultParser<Categorie>.ParseList(response);
                    ArticleVM.Categories = catApiResponse.Data;
                    ArticleVM.Article = article;
                    ArticleVM.Entreprises = apiResponseListEntreprises?.Data;
                    if (article.Image != null)
                        ArticleVM.ImageBase64 = $"{Convert.ToBase64String(article.Image!)}";
                    return View(ArticleVM);
                }
                else
                {
                    return NotFound();
                }
            }

            return NotFound();
        }
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        [HttpPost]
        public async Task<JsonResult> UpdateArticle(ArticleCURequest articleRequest, IFormFile? Image)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<Article>.ModelStateError(ModelState));
            }
            if (Image != null)
            {
                if (Image.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        Image.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        articleRequest.Image = fileBytes;
                    }
                }
            }
            else
            {
                articleRequest.Image = Convert.FromBase64String(articleRequest.ImageBase64);

            }

            var url = String.Format(ApiUrlGeneric.UpdateURL<Article>(), articleRequest.Id);

            var apiResponse = await ApiService<String>.CallApiPut(_httpClientFactory, url, articleRequest);

            bool success = apiResponse.StatusCode==System.Net.HttpStatusCode.NoContent;
     
            string msg = success ? "Article modifiée avec succès!" : (apiResponse.Message ?? "Une erreur a été rencontrée.");
            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });




        }


        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        [HttpDelete]
        public async Task<JsonResult> DeleteArticle(int id)
        {
            var url = String.Format(ApiUrlGeneric.DeleteURL<Article>(), id);

            var apiResponse = await ApiService<String>.CallApiDelete(_httpClientFactory, url);

            apiResponse.Success=apiResponse.StatusCode== System.Net.HttpStatusCode.NoContent;

            return Json(apiResponse);
        }

        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        [HttpPatch]
        public async Task<JsonResult> UpdateStatusArticle(int id)
        {
            var url = String.Format(ApiUrlGeneric.UpdateArticleStatusURL, id);

            var apiResponse = await ApiService<Article>.CallApiPut(_httpClientFactory, url,new {});
            return Json(apiResponse);
        }
        [Authorize(Policy = IdentityData.AdminPolicy)]
        [HttpPatch]
        public async Task<JsonResult> ApproveArticle(int id)
        {
            var url = String.Format(ApiUrlGeneric.ApproveArticleStatusURL, id);

            var apiResponse = await ApiService<Article>.CallApiPut(_httpClientFactory, url, new { });
            return Json(apiResponse);
        }


        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        [HttpPatch]
        public async Task<JsonResult> Mouvement(int articleId, int quantite,MouvementEnum mouvement)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<User>.ModelStateError(ModelState));
            }


            var url = String.Format(ApiUrlGeneric.MouvementArticleURL, articleId, quantite,mouvement);

            var apiResponse = await ApiService<ArticleCommandReadDto>.CallApiPut(_httpClientFactory, url, null);

            bool success = apiResponse.Data != null;
            string msg = success ? "Opération effectuée avec succès!" :( apiResponse.Message??"Une erreur a été rencontrée.");

            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });




        }


    }
}
