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

namespace CantineFront.Controllers
{
    public class PaymentMethodController : Controller
    {
        PaymentViewModel PaymentMethodVM;
        private readonly IHttpClientFactory _httpClientFactory;
        IOptions<AppSettings> _appSettings;
        public PaymentMethodController(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
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
        public async Task<JsonResult> GetPaymentMethods()
        {

            var url = ApiUrlGeneric.ReadAllURL<PaymentMethod>();
            var httpClient = _httpClientFactory.CreateClient("ClearanceApi");
            HttpResponseMessage response = await httpClient.GetAsync(url);
            var apiResponse = await ApiResultParser<PaymentMethod>.ParseList(response);
            return Json(apiResponse);
        }
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
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
        public async Task<JsonResult> CreatePaymentMethod(PaymentMethod catRequest)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<PaymentMethod>.ModelStateError(ModelState));
            }
            var url = ApiUrlGeneric.CreateURL<PaymentMethod>();
            var apiResponse = await ApiService<PaymentMethod>.CallApiPost(_httpClientFactory, url, catRequest);

            bool success = apiResponse.Data != null;
            string msg = success ? "PaymentMethod crée avec succès!" : "Une erreur a été rencontrée!";

            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });



        }

        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<IActionResult> Update(int? id)
        {

            if (id.HasValue)
            {
                if (PaymentMethodVM == null) PaymentMethodVM = new PaymentViewModel();
                var url = String.Format(ApiUrlGeneric.ReadOneURL<PaymentMethod>(), id);
                var httpClient = _httpClientFactory.CreateClient("ClearanceApi");
                HttpResponseMessage response = await httpClient.GetAsync(url);
                var apiResponse = await ApiResultParser<PaymentMethod>.Parse(response);

                var categorie = apiResponse.Data;
                if (categorie != null)
                {
                    PaymentMethodVM.PaymentMethod = categorie;
                    return View(PaymentMethodVM);
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
        public async Task<JsonResult> UpdatePaymentMethod(PaymentMethod catRequest)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<PaymentMethod>.ModelStateError(ModelState));
            }


            var url = String.Format(ApiUrlGeneric.UpdateURL<PaymentMethod>(), catRequest.Id);

            var apiResponse = await ApiService<String>.CallApiPut(_httpClientFactory, url, catRequest);

            bool success = apiResponse.StatusCode == System.Net.HttpStatusCode.NoContent;
            string msg = success ? "PaymentMethod modifiée avec succès!" : "Une erreur a été rencontrée!";

            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });
        }

        [HttpDelete]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<JsonResult> DeletePaymentMethod(int id)
        {
            var url = String.Format(ApiUrlGeneric.DeleteURL<PaymentMethod>(), id);

            var apiResponse = await ApiService<String>.CallApiDelete(_httpClientFactory, url);
            apiResponse.Success = apiResponse.StatusCode == System.Net.HttpStatusCode.NoContent;
            return Json(apiResponse);
        }
    }
}
