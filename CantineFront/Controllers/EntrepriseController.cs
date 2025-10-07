using CantineBack.Models;
using CantineFront.Identity;
using CantineFront.ServiceFactory;
using CantineFront.Services;
using CantineFront.Static;
using CantineFront.Utils;
using CantineFront.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CantineFront.Controllers
{
    public class EntrepriseController : Controller
    {
        EntrepriseViewModel EntrepriseVM;
        private readonly IHttpClientFactory _httpClientFactory;
        IOptions<AppSettings> _appSettings;
        public EntrepriseController(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
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
        public ActionResult List()
        {
            return View();
        }
        public ActionResult Create()
        {
            return View();
        }
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<IActionResult> Update(int? id)
        {

            if (id.HasValue)
            {
                if (EntrepriseVM == null) EntrepriseVM = new EntrepriseViewModel();
                var url = String.Format(ApiUrlGeneric.ReadOneURL<Entreprise>(), id);
                var httpClient = _httpClientFactory.CreateClient("ClearanceApi");
                HttpResponseMessage response = await httpClient.GetAsync(url);
                var apiResponse = await ApiResultParser<Entreprise>.Parse(response);

                var categorie = apiResponse.Data;
                if (categorie != null)
                {
                    EntrepriseVM.Entreprise = categorie;
                    return View(EntrepriseVM);
                }
                else
                {
                    return NotFound();
                }
            }

            return NotFound();
        }
        [HttpGet]
        public async Task<JsonResult> GetEntreprises()
        {

            var url = ApiUrlGeneric.ReadAllURL<Entreprise>();
            var httpClient = _httpClientFactory.CreateClient("ClearanceApi");
            HttpResponseMessage response = await httpClient.GetAsync(url);
            var apiResponse = await ApiResultParser<Entreprise>.ParseList(response);
            return Json(apiResponse);
        }

        [HttpPost]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<JsonResult> CreateEntreprise(Entreprise request)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<Entreprise>.ModelStateError(ModelState));
            }
            var url = ApiUrlGeneric.CreateURL<Entreprise>();
            var apiResponse = await ApiService<Entreprise>.CallApiPost(_httpClientFactory, url, request);
            bool success = apiResponse.Data != null;
            string msg = success ? "Entreprise crée avec succès!" : "Une erreur a été rencontrée!";
            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });
        }

        [HttpPost]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<JsonResult> UpdateEntreprise(Entreprise request)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<Entreprise>.ModelStateError(ModelState));
            }


            var url = String.Format(ApiUrlGeneric.UpdateURL<Entreprise>(), request.Id);

            var apiResponse = await ApiService<String>.CallApiPut(_httpClientFactory, url, request);

            bool success = apiResponse.StatusCode == System.Net.HttpStatusCode.OK
             || apiResponse.StatusCode == System.Net.HttpStatusCode.NoContent;

            string msg = success ? "Entreprise modifiée avec succès!" : "Une erreur a été rencontrée!";

            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });
        }
        [HttpDelete]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<JsonResult> DeleteEntreprise(int id)
        {
            var url = String.Format(ApiUrlGeneric.DeleteURL<Entreprise>(), id);

            var apiResponse = await ApiService<String>.CallApiDelete(_httpClientFactory, url);
            apiResponse.Success = apiResponse.StatusCode == System.Net.HttpStatusCode.OK
                     || apiResponse.StatusCode == System.Net.HttpStatusCode.NoContent;

            return Json(apiResponse);
        }
    }
}
