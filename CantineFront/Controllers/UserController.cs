using CantineBack.Models;
using CantineBack.Models.Dtos;
using CantineFront.Helpers;
using CantineFront.Models;
using CantineFront.Models.Request;
using CantineFront.ServiceFactory;
using CantineFront.Services;
using CantineFront.Static;
using CantineFront.Utils;
using CantineFront.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.ComponentModel;

namespace CantineFront.Controllers
{

    public class UserController : Controller
    {
        UserViewModel UserVM;
        private readonly IHttpClientFactory _httpClientFactory;
        IOptions<AppSettings> _appSettings;
        public UserController(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
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
        [Authorize(Policy = "AdminPolicy")]
        [HttpGet]
        public async Task<JsonResult> GetUsers()
        {
            var url = ApiUrlGeneric.ReadAllURL<User>();
            var httpClient = _httpClientFactory.CreateClient("ClearanceApi");
            HttpResponseMessage response = await httpClient.GetAsync(url);
            var apiResponse = await ApiResultParser<User>.ParseList(response);
            return Json(apiResponse);
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public async Task<JsonResult> GetUsersServerSide()
        {

            var listUsersSessions = HttpContext.Session.GetListObjectFromSession<User>("ListUsers");
            if (listUsersSessions==null || !(listUsersSessions?.Any()??false))
            {

                var url = ApiUrlGeneric.ReadAllURL<User>();
                var httpClient = _httpClientFactory.CreateClient("ClearanceApi");
                HttpResponseMessage response = await httpClient.GetAsync(url);
                var apiResponse = await ApiResultParser<User>.ParseList(response);

                if (apiResponse.Data != null && apiResponse.Success)
                {
                    listUsersSessions = apiResponse.Data;
                    HttpContext.Session.SetListInSession("ListUsers", listUsersSessions);
                }


            }

            int totalRecord = 0;
            int filterRecord = 0;
            var draw = Request.Form["draw"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
            if (pageSize == -1)
            {
                pageSize = listUsersSessions?.Count ?? 0; 
            }
            int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
            var data = listUsersSessions;
            //get total count of data in table
            totalRecord = data.Count();
            // search data when search value found
            if (!string.IsNullOrEmpty(searchValue))
            {
                data = data.Where(x => (x.Matricule?.ToLower()?.Contains(searchValue.ToLower())??false) 
                || (x.Nom?.ToLower().Contains(searchValue.ToLower())??false) 
                || (x.Email?.ToLower().Contains(searchValue.ToLower())??false) 
                || (x.Telephone?.ToLower().Contains(searchValue.ToLower())??false) 
                || (x.Prenom?.ToLower().Contains(searchValue.ToLower())??false) 
                || (x.Profile?.ToString().ToLower().Contains(searchValue.ToLower())??false)
                
                ).ToList();
            }
            // get total count of records after search
            filterRecord = data.Count();
            //sort data
            //  if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection)) data = data.OrderBy(sortColumn + " " + sortColumnDirection);
            //pagination
            var empList = data.Skip(skip).Take(pageSize).ToList();
            var returnObj = new
            {
                draw = draw,
                recordsTotal = totalRecord,
                recordsFiltered = filterRecord,
                data = empList
            };





            return Json(returnObj);
        }












        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> Create()
        {
            UserVM = new UserViewModel();
            var url = ApiUrlGeneric.ReadAllURL<Department>();
            var urlEnt = ApiUrlGeneric.ReadAllURL<Entreprise>();
            var apiResponse = await ApiService<Department>.CallGetList(_httpClientFactory, url);
            var apiResponseEnt = await ApiService<Entreprise>.CallGetList(_httpClientFactory, urlEnt);
            UserVM.Departments = apiResponse?.Data;
            UserVM.Entreprises = apiResponseEnt?.Data;

            return View(UserVM);
        }

        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> List()
        {
            var url = ApiUrlGeneric.ReadAllURL<User>();
            var httpClient = _httpClientFactory.CreateClient("ClearanceApi");
            HttpResponseMessage response = await httpClient.GetAsync(url);
            var apiResponse = await ApiResultParser<User>.ParseList(response);

            if (apiResponse.Data != null && apiResponse.Success)
            {

                HttpContext.Session.SetListInSession("ListUsers", apiResponse.Data);
            }

            return View();
        }
        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public async Task<JsonResult> CreateUser(UserCURequest userRequest)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<User>.ModelStateError(ModelState));
            }
            var url = ApiUrlGeneric.CreateURL<User>();

            var apiResponse = await ApiService<User>.CallApiPost(_httpClientFactory, url, userRequest);

            bool success = apiResponse.Data != null;

            string msg = success ? "Utilisateur crée avec succès!" : (apiResponse.Message ?? "Une erreur a été rencontrée.");
            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });
        }

        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> Update(int? id)
        {

            if (id.HasValue)
            {
                if (UserVM == null) UserVM = new UserViewModel();
                var url = String.Format(ApiUrlGeneric.ReadOneURL<User>(), id);
                var httpClient = _httpClientFactory.CreateClient("ClearanceApi");
                HttpResponseMessage response = await httpClient.GetAsync(url);
                var apiResponse = await ApiResultParser<User>.Parse(response);
                var urlEnt = ApiUrlGeneric.ReadAllURL<Entreprise>();
                var apiResponseEnt = await ApiService<Entreprise>.CallGetList(_httpClientFactory, urlEnt);

                var categorie = apiResponse.Data;
                if (categorie != null)
                {
                    UserVM.User = categorie;
                    //Get Categories
                    url = ApiUrlGeneric.ReadAllURL<Department>();
                    response = await httpClient.GetAsync(url);
                    var catApiResponse = await ApiResultParser<Department>.ParseList(response);
                    UserVM.Departments = catApiResponse.Data;
                    UserVM.Entreprises = apiResponseEnt?.Data;

                    return View(UserVM);
                }
                else
                {
                    return NotFound();
                }
            }

            return NotFound();
        }
        [Authorize(Policy = "AdminPolicy")]
        [HttpPost] 
        public async Task<JsonResult> UpdateUser(UserCURequest userRequest)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<User>.ModelStateError(ModelState));
            }


            var url = String.Format(ApiUrlGeneric.UpdateURL<User>(), userRequest.Id);

            var apiResponse = await ApiService<ApiMessage>.CallApiPut(_httpClientFactory, url, userRequest);

            bool success = apiResponse.StatusCode == System.Net.HttpStatusCode.OK;
            string msg = success ? apiResponse.Data?.Message ?? "Utilisateur modifié avec succès"
                                 : (apiResponse.Message ?? "Une erreur a été rencontrée.");

            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });

        }
        public async Task<JsonResult> UpdateProfileUser(UserProfilDto userRequest)
        {
                if (!ModelState.IsValid)
                {
                    return Json(ModelErrorHandler<User>.ModelStateError(ModelState));
                }


            var url = String.Format(ApiUrlGeneric.UpdateUserProfile, userRequest.Id);

            var apiResponse = await ApiService<ApiMessage>.CallApiPost(_httpClientFactory, url, userRequest);

            bool success = apiResponse.StatusCode == System.Net.HttpStatusCode.OK;
            string msg = success ? apiResponse.Data?.Message ?? "Profile modifié avec succès"
                                 : (apiResponse.Message ?? "Une erreur a été rencontrée.");

            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });

        }


        [Authorize(Policy = "AdminPolicy")]
        [HttpPatch]
        public async Task<JsonResult> RenitializePassword(int id)
        {
            var url = String.Format(ApiUrlGeneric.RenitializePasswordURL, id);

            var apiResponse = await ApiService<Article>.CallApiPut(_httpClientFactory, url, new { });

            bool success = apiResponse.StatusCode == System.Net.HttpStatusCode.NoContent;
            string msg = success ? "Opération effectuée avec succès!" : "Une erreur a été rencontrée!";
            apiResponse.Success = success;
            apiResponse.Message = msg;

            return Json(apiResponse);
        }
        [Authorize(Policy = "AdminPolicy")]
        [HttpPut]
        public async Task<JsonResult> GenerateAllQrCode()
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<User>.ModelStateError(ModelState));
            }


            var url = ApiUrlGeneric.GenerateAllQrCodeURL;

            var apiResponse = await ApiService<String>.CallApiPut(_httpClientFactory, url, null);

            bool success = apiResponse.StatusCode == System.Net.HttpStatusCode.NoContent;
            string msg = success ? "Opération effectuée avec succès!" : (apiResponse.Message ?? "Une erreur a été rencontrée.");

            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });




        }
        [Authorize(Policy = "AdminPolicy")]
        [HttpPut]
        public async Task<JsonResult> SendAllQrCode()
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<User>.ModelStateError(ModelState));
            }


            var url = ApiUrlGeneric.SendAllQrCodeURL;

            var apiResponse = await ApiService<String>.CallApiPut(_httpClientFactory, url, null);

            bool success = apiResponse.StatusCode == System.Net.HttpStatusCode.NoContent;
            string msg = success ? "Opération effectuée avec succès!" : (apiResponse.Message ?? "Une erreur a été rencontrée.");

            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });

        }
        [Authorize(Policy = "AdminPolicy")]
        [HttpPut]
        public async Task<JsonResult> SendUserQrCode(int id)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<User>.ModelStateError(ModelState));
            }


            var url =String.Format( ApiUrlGeneric.SendUserQrCodeURL,id);

            var apiResponse = await ApiService<String>.CallApiPut(_httpClientFactory, url, null);

            bool success = apiResponse.StatusCode == System.Net.HttpStatusCode.NoContent;
            string msg = success ? "Opération effectuée avec succès!" : (apiResponse.Message ?? "Une erreur a été rencontrée.");

            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });




        }


        [Authorize(Policy = "AdminPolicy")]
        [HttpPut]
        public async Task<JsonResult> RechargerCompte(int userId, int montant)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<User>.ModelStateError(ModelState));
            }


            var url = String.Format(ApiUrlGeneric.RechargerCompteURL, userId, montant);

            var apiResponse = await ApiService<UserReadDto>.CallApiPut(_httpClientFactory, url, null);

            bool success = apiResponse.Data != null;
          
            string msg = success ? "Opération effectuée avec succès!" : (apiResponse.Message ?? "Une erreur a été rencontrée.");
            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });




        }        
        [Authorize(Policy = "AdminPolicy")]
        [HttpPut]
        public async Task<JsonResult> RechargerMultipleCompte(List<int>? userIds, int montant)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<User>.ModelStateError(ModelState));
            }


            var url = ApiUrlGeneric.RechargerMultiCompteURL;

            var apiResponse = await ApiService<String>.CallApiPost(_httpClientFactory, url, new RechargeComptesRequest{ UserIds=userIds,Montant=montant });

            bool success = apiResponse.StatusCode == System.Net.HttpStatusCode.NoContent;
            string msg = success ? "Opération effectuée avec succès!" : (apiResponse.Message ?? "Une erreur a été rencontrée.");

            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });



        }
        [Authorize(Policy = "AdminPolicy")]
        [HttpPut]
        public async Task<JsonResult> ResetPasswordMultiAccounts(List<int>? userIds)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<User>.ModelStateError(ModelState));
            }


            var url = ApiUrlGeneric.ResetPasswordMultiAccountsURL;

            var apiResponse = await ApiService<String>.CallApiPost(_httpClientFactory, url, new RechargeComptesRequest { UserIds = userIds, Montant = 0 });

            bool success = apiResponse.StatusCode == System.Net.HttpStatusCode.NoContent;
            string msg = success ? "Opération effectuée avec succès!" : (apiResponse.Message ?? "Une erreur a été rencontrée.");

            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });



        }


        [Authorize(Policy = "AdminPolicy")]
        [HttpDelete]
        public async Task<JsonResult> DeleteUser(int id)
        {
            var url = String.Format(ApiUrlGeneric.DeleteURL<User>(), id);

            var apiResponse = await ApiService<String>.CallApiDelete(_httpClientFactory, url);
            apiResponse.Success = apiResponse.StatusCode == System.Net.HttpStatusCode.NoContent;
            return Json(apiResponse);
        }
        [Authorize(Policy = "UserPolicy")]
        [HttpGet]
        public async Task<JsonResult> GetCurrentUser()
        {

            var url = String.Format(ApiUrlGeneric.ReadOneURL<User>(), HttpContext.Session.GetInt32("UserId"));
            var httpClient = _httpClientFactory.CreateClient("ClearanceApi");
            HttpResponseMessage response = await httpClient.GetAsync(url);
            var apiResponse = await ApiResultParser<User>.Parse(response);
            return Json(apiResponse);
        }
        [HttpGet]
        public async Task<JsonResult> GetUserByQrCode(string qrCode)
        {
            if (!string.IsNullOrEmpty(qrCode))
            {
                var decryptInfo = DPWorldEncryption.SecurityManager.DecryptAES(qrCode) ?? String.Empty;
                // QrCodeStaff? qrCodeStaff=JsonConvert.DeserializeObject<QrCodeStaff>(decryptInfo!);
                if (!String.IsNullOrWhiteSpace(decryptInfo))
                {
                    string matricule = decryptInfo.Substring(0, decryptInfo.Length - 1);
                    //Get User By matricule
                    var url = String.Format(ApiUrlGeneric.GetUserByMatriculeURL, matricule);

                    var apiResponse = await ApiService<User>.CallGet(_httpClientFactory, url);
                    if(apiResponse.StatusCode==System.Net.HttpStatusCode.OK && apiResponse.Data == null)
                    {
                        apiResponse.Message = "QRCode inconnu ou inexistant";
                    }
                    else if(apiResponse.StatusCode==System.Net.HttpStatusCode.NotFound) {
                        apiResponse.Message = "QRCode inconnu ou inexistant";
                    }
                    
                    return Json(apiResponse);
                }


            }

            return Json(null);


        }
        //[Authorize(Policy = "UserPolicy")]
        [Authorize]
        [HttpGet]
        public async Task<JsonResult> GetUserSolde()
        {

            var url = String.Format(ApiUrlGeneric.GetSoldeURL, HttpContext.Session.GetInt32("UserId"));

            var httpClient = _httpClientFactory.CreateClient("ClearanceApi");

            HttpResponseMessage response = await httpClient.GetAsync(url);

            HttpContent content = response.Content;
            string result = await content.ReadAsStringAsync();

            _ = int.TryParse(result, out int value);

            return Json(new { StatusCode = response.StatusCode, Data = value });
        }
        [Authorize]
        [HttpGet]
        public  JsonResult GetUserQrCode()
        {


            return Json(new {Data = HttpContext.Session.GetString("UserQRCode") });
        }

    }
}
