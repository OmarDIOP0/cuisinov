using CantineBack.Models;
using CantineFront.Identity;
using CantineFront.ServiceFactory;
using CantineFront.Services;
using CantineFront.Static;
using CantineFront.Utils;
using CantineFront.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Data;

namespace CantineFront.Controllers
{
    public class EmplacementController : Controller
    {
        EmplacementViewModel EmplacementVM;
        private readonly IHttpClientFactory _httpClientFactory;
        IOptions<AppSettings> _appSettings;
        public EmplacementController(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
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
        public async Task<JsonResult> GetEmplacements()
        {

            var url = ApiUrlGeneric.ReadAllURL<Emplacement>();
            var httpClient = _httpClientFactory.CreateClient("ClearanceApi");
            HttpResponseMessage response = await httpClient.GetAsync(url);
            var apiResponse = await ApiResultParser<Emplacement>.ParseList(response);
            return Json(apiResponse);
        }
        public async Task<JsonResult> GetEmplacementsByEntreprise(int entrepriseId)
        {

            var url = ApiUrlGeneric.ReadAllURL<Emplacement>();
            var httpClient = _httpClientFactory.CreateClient("ClearanceApi");
            HttpResponseMessage response = await httpClient.GetAsync(url);
            var apiResponse = await ApiResultParser<Emplacement>.ParseList(response);
            return Json(apiResponse);
        }
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<IActionResult> Create()
        {
            var urlEntreprise = ApiUrlGeneric.ReadAllURL<Entreprise>();
            var apiResponseListEntreprises = await ApiService<Entreprise>.CallGetList(_httpClientFactory, urlEntreprise);
            ViewBag.Entreprises = apiResponseListEntreprises?.Data;

            return View();
        }

        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public IActionResult List()
        {


            return View();
        }
        [HttpPost]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<JsonResult> CreateEmplacement(Emplacement catRequest)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<Emplacement>.ModelStateError(ModelState));
            }



            var url = ApiUrlGeneric.CreateURL<Emplacement>();

            var apiResponse = await ApiService<Emplacement>.CallApiPost(_httpClientFactory, url, catRequest);

            bool success = apiResponse.Data != null;
            string msg = success ? "Emplacement crée avec succès!" : "Une erreur a été rencontrée!";

            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });



        }

        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<IActionResult> Update(int? id)
        {

            if (id.HasValue)
            {
                if (EmplacementVM == null) EmplacementVM = new EmplacementViewModel();
                var url = String.Format(ApiUrlGeneric.ReadOneURL<Emplacement>(), id);
                var httpClient = _httpClientFactory.CreateClient("ClearanceApi");
                HttpResponseMessage response = await httpClient.GetAsync(url);
                var apiResponse = await ApiResultParser<Emplacement>.Parse(response);
                var urlEntreprise = ApiUrlGeneric.ReadAllURL<Entreprise>();
                var apiResponseListEntreprises = await ApiService<Entreprise>.CallGetList(_httpClientFactory, urlEntreprise);

                var categorie = apiResponse.Data;
                if (categorie != null)
                {
                    EmplacementVM.Emplacement = categorie;
                    EmplacementVM.Entreprises = apiResponseListEntreprises?.Data;
                    return View(EmplacementVM);
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
        public async Task<JsonResult> UpdateEmplacement(Emplacement catRequest)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<Emplacement>.ModelStateError(ModelState));
            }


            var url = String.Format(ApiUrlGeneric.UpdateURL<Emplacement>(), catRequest.Id);

            var apiResponse = await ApiService<String>.CallApiPut(_httpClientFactory, url, catRequest);

            bool success = apiResponse.StatusCode == System.Net.HttpStatusCode.NoContent;
            string msg = success ? "Emplacement modifiée avec succès!" : "Une erreur a été rencontrée!";

            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });
        }

        [HttpDelete]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<JsonResult> DeleteEmplacement(int id)
        {
            var url = String.Format(ApiUrlGeneric.DeleteURL<Emplacement>(), id);

            var apiResponse = await ApiService<String>.CallApiDelete(_httpClientFactory, url);
            apiResponse.Success = apiResponse.StatusCode == System.Net.HttpStatusCode.NoContent;
            return Json(apiResponse);
        }
    }
}
