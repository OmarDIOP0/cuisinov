using CantineBack.Models;
using CantineFront.Identity;
using CantineFront.ServiceFactory;
using CantineFront.Services;
using CantineFront.Static;
using CantineFront.Utils;
using CantineFront.ViewModels;
using DPWorldMobile.ServiceFactory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Data;

namespace CantineFront.Controllers
{
    public class DepartmentController : Controller
    {
        DepartmentViewModel DepartmentVM;
        private readonly IHttpClientFactory _httpClientFactory;
        IOptions<AppSettings> _appSettings;
        public DepartmentController(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
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
        public async Task<JsonResult> GetDepartments()
        {

            var url = ApiUrlGeneric.ReadAllURL<Department>();
            var httpClient = _httpClientFactory.CreateClient("ClearanceApi");
            HttpResponseMessage response = await httpClient.GetAsync(url);
            var apiResponse = await ApiResultParser<Department>.ParseList(response);
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
        public async Task<JsonResult> CreateDepartment(Department catRequest)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<Department>.ModelStateError(ModelState));
            }
            var url = ApiUrlGeneric.CreateURL<Department>();
            var apiResponse = await ApiService<Department>.CallApiPost(_httpClientFactory, url, catRequest);
            bool success = apiResponse.Data != null;
            string msg = success ? "Department crée avec succès!" : "Une erreur a été rencontrée!";
            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });
        }

        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<IActionResult> Update(int? id)
        {

            if (id.HasValue)
            {
                if (DepartmentVM == null) DepartmentVM = new DepartmentViewModel();
                var url = String.Format(ApiUrlGeneric.ReadOneURL<Department>(), id);
                var httpClient = _httpClientFactory.CreateClient("ClearanceApi");
                HttpResponseMessage response = await httpClient.GetAsync(url);
                var apiResponse = await ApiResultParser<Department>.Parse(response);
                var urlEntreprise = ApiUrlGeneric.ReadAllURL<Entreprise>();
                var apiResponseListEntreprises = await ApiService<Entreprise>.CallGetList(_httpClientFactory, urlEntreprise);

                var categorie = apiResponse.Data;
                if (categorie != null)
                {
                    DepartmentVM.Department = categorie;
                    DepartmentVM.Entreprises = apiResponseListEntreprises?.Data;
                    return View(DepartmentVM);
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
        public async Task<JsonResult> UpdateDepartment(Department catRequest)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<Department>.ModelStateError(ModelState));
            }


            var url = String.Format(ApiUrlGeneric.UpdateURL<Department>(), catRequest.Id);

            var apiResponse = await ApiService<String>.CallApiPut(_httpClientFactory, url, catRequest);

            bool success = apiResponse.StatusCode == System.Net.HttpStatusCode.NoContent;
            string msg = success ? "Department modifiée avec succès!" : "Une erreur a été rencontrée!";

            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });

        }

        [HttpDelete]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<JsonResult> DeleteDepartment(int id)
        {
            var url = String.Format(ApiUrlGeneric.DeleteURL<Department>(), id);

            var apiResponse = await ApiService<String>.CallApiDelete(_httpClientFactory, url);
            apiResponse.Success = apiResponse.StatusCode == System.Net.HttpStatusCode.NoContent;
            return Json(apiResponse);
        }
    }
}
