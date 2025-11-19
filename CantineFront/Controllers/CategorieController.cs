using CantineBack.Models;
using CantineFront.Identity;
using CantineFront.Models.Response;
using CantineFront.ServiceFactory;
using CantineFront.Services;
using CantineFront.Static;
using CantineFront.Utils;
using CantineFront.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace CantineFront.Controllers
{
    public class CategorieController : Controller
    {
        CategorieViewModel CategorieVM;
        private readonly IHttpClientFactory _httpClientFactory;
        IOptions<AppSettings> _appSettings;
        public CategorieController(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
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
        [HttpGet]
        public async Task<JsonResult> GetAllCategories() {

            var url = String.Format(ApiUrlGeneric.GetAllCategories);
            var httpClient = _httpClientFactory.CreateClient("ClearanceApi");
            HttpResponseMessage response = await httpClient.GetAsync(url);
            var apiResponse = await ApiResultParser<Categorie>.ParseList(response);
            return Json(apiResponse);
        }
        [HttpGet]
        public async Task<JsonResult> GetCategories() {

            var url = ApiUrlGeneric.ReadAllURL<Categorie>();
            var httpClient = _httpClientFactory.CreateClient("ClearanceApi");
            HttpResponseMessage response = await httpClient.GetAsync(url);
            var apiResponse = await ApiResultParser<Categorie>.ParseList(response);
            return Json(apiResponse);
        }

        [Authorize(Roles ="ADMIN,GERANT")]
        public IActionResult Create()
        {

            return View();
        }

        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public IActionResult List()
        {


            return View();
        }
        [HttpPost]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<JsonResult> CreateCategorie(Categorie catRequest)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<Categorie>.ModelStateError(ModelState));
            }


         
            var url = ApiUrlGeneric.CreateURL<Categorie>();

            var apiResponse = await ApiService<Categorie>.CallApiPost(_httpClientFactory, url, catRequest);

            bool success = apiResponse.Data != null;
            string msg = success ? "Categorie crée avec succès!" : "Une erreur a été rencontrée!";

            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });



        }

        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<IActionResult> Update(int? id)
    {

        if (id.HasValue)
        {
            if (CategorieVM == null) CategorieVM = new CategorieViewModel();
            var url = String.Format(ApiUrlGeneric.ReadOneURL<Categorie>(), id);
            var httpClient = _httpClientFactory.CreateClient("ClearanceApi");
            HttpResponseMessage response = await httpClient.GetAsync(url);
            var apiResponse = await ApiResultParser<Categorie>.Parse(response);

            var categorie = apiResponse.Data;
            if (categorie != null)
            {
                CategorieVM.Categorie = categorie;
                return View(CategorieVM);
            }
            else
            {
                return NotFound();
            }
        }

        return NotFound();
    }
        [HttpPost]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<JsonResult> UpdateCategorie(Categorie catRequest)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<Categorie>.ModelStateError(ModelState));
            }


            var url = String.Format(ApiUrlGeneric.UpdateURL<Categorie>(), catRequest.Id);

            var apiResponse = await ApiService<String>.CallApiPut(_httpClientFactory, url, catRequest);

            bool success = apiResponse.StatusCode == System.Net.HttpStatusCode.NoContent   ;
            string msg = success ? "Categorie modifiée avec succès!" : "Une erreur a été rencontrée!";

            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });




        }

        [HttpDelete]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<JsonResult> DeleteCategorie(int id)
        {
            var url = String.Format(ApiUrlGeneric.DeleteURL<Categorie>(), id);

            var apiResponse = await ApiService<String>.CallApiDelete(_httpClientFactory, url);
            apiResponse.Success = apiResponse.StatusCode == System.Net.HttpStatusCode.NoContent;
            return Json(apiResponse);
        }

    }
}
