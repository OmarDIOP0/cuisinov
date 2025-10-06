using CantineBack.Models;
using CantineFront.ServiceFactory;
using CantineFront.Services;
using CantineFront.Static;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CantineFront.Controllers
{
    public class EntrepriseController : Controller
    {
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

        public ActionResult Update(int id)
        {
            return View();
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
    }
}
