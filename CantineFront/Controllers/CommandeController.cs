using AutoMapper;
using CantineBack.Models;
using CantineBack.Models.Dtos;
using CantineBack.Models.Enums;
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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Data;
using System.Net.Http;

namespace CantineFront.Controllers
{
    public class CommandeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _mapper;
       
        public CommandeController(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings,IMapper mapper)
        {
          
                _httpClientFactory = httpClientFactory;
            _mapper=mapper;
        }
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public IActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public IActionResult List()
        {
            ViewBag.MaxDate = DateTime.Now.AddHours(1).ToString("yyyy-MM-ddTHH:mm");
            ViewBag.StartDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-ddT00:00");
            ViewBag.EndDate = DateTime.Now.AddHours(5).ToString("yyyy-MM-ddTHH:mm");
            return View();
        }
        [Authorize(Roles = IdentityData.AdminOrUserRoles)]
        public IActionResult MyCommands()
        {
            ViewBag.MaxDate = DateTime.Now.AddHours(1).ToString("yyyy-MM-ddTHH:mm");
            ViewBag.StartDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-ddT00:00");
            ViewBag.EndDate = DateTime.Now.AddHours(5).ToString("yyyy-MM-ddTHH:mm");
            return View();
        }
        [HttpPost]
        [Authorize]
        public async Task<JsonResult> CreateCommande(PostCommandDto commandRequest )
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<Article>.ModelStateError(ModelState));
            }
            // commandRequest.UserId = HttpContext.Session.GetInt32("UserId")??1;

            //Supprimer les articles avec quantité=0
            commandRequest.LigneCommands = commandRequest.LigneCommands?.Where(a => a.Quantite > 0).ToList();
           if( (commandRequest.LigneCommands?.Count() ?? 0) <= 0)
            {
                return Json(new FormResponse { Success = false, Object =null, Message = "Commande invalide , aucune ligne de commande n'est définie." });
            }

            var url = ApiUrlGeneric.CreateURL<Commande>();

            var apiResponse = await ApiService<Commande>.CallApiPost(_httpClientFactory, url, commandRequest);


            bool success = (apiResponse.Data?.Id??0)>0 ;
            string msg = success ? "Commande enregistrée avec succès!" :(apiResponse.Message?? "Une erreur a été rencontrée!");
            if(success)
            {
                HttpContext.Session.SetListInSession<Article>("ArticlesCart",null);
                HttpContext.Session.SetInt32("CartBadgeCount", 0);

            }
            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });



        }
        [HttpPost]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<JsonResult> CreateCommandeByPartner(string? QrCode, string PaymentMethodeCode, PostCommandDto commandRequest)
        {

            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<PostCommandDto>.ModelStateError(ModelState));
            }

            // commandRequest.UserId = HttpContext.Session.GetInt32("UserId")??1;

            //Supprimer les articles avec quantité=0
            commandRequest.LigneCommands = commandRequest.LigneCommands?.Where(a => a.Quantite > 0).ToList();
            if ((commandRequest.LigneCommands?.Count() ?? 0) <= 0)
            {
                return Json(new FormResponse { Success = false, Object = null, Message = "Commande invalide , aucune ligne de commande n'est définie." });
            }
            User user = null;

            if (PaymentMethodeCode.ToUpper() == "QRCODE")
            {
                string matricule = String.Empty;
                var decryptInfo = DPWorldEncryption.SecurityManager.DecryptAES(QrCode) ?? String.Empty;
                // QrCodeStaff? qrCodeStaff=JsonConvert.DeserializeObject<QrCodeStaff>(decryptInfo!);
                if (!String.IsNullOrWhiteSpace(decryptInfo))
                {
                    matricule = decryptInfo.Substring(0, decryptInfo.Length - 1);

                }

                if (String.IsNullOrWhiteSpace(matricule))
                {
                    return Json(new FormResponse { Success = false, Message = "Utilisateur inconnu ou inexistant! Le paiement par solde nécessite l'identification du client" });
                }
                //Descript qrCode
                //Get User By matricule

                var urlGetUser = String.Format(ApiUrlGeneric.GetUserByMatriculeURL, matricule);

                var apiResponseGetUser = await ApiService<User>.CallGet(_httpClientFactory, urlGetUser);
                user = apiResponseGetUser.Data;
                if(user == null)
                {
                    return Json(new FormResponse { Success = false, Message = "Utilisateur inconnu ou inexistant! Le paiement par solde nécessite l'identification du client" });

                }

                commandRequest.UserId = user.Id;

            }










            //if (!ModelState.IsValid)
            //{
            //    return Json(ModelErrorHandler<Article>.ModelStateError(ModelState));
            //}
            //// commandRequest.UserId = HttpContext.Session.GetInt32("UserId")??1;

            ////Supprimer les articles avec quantité=0
            //commandRequest.LigneCommands = commandRequest.LigneCommands?.Where(a => a.Quantite > 0).ToList();
            //if ((commandRequest.LigneCommands?.Count() ?? 0) <= 0)
            //{
            //    return Json(new FormResponse { Success = false, Object = null, Message = "Commande invalide , aucune ligne de commande n'est définie." });
            //}

            var url = ApiUrlGeneric.CreateURL<Commande>();

            var apiResponse = await ApiService<Commande>.CallApiPost(_httpClientFactory, url, commandRequest);


            bool success = (apiResponse.Data?.Id ?? 0) > 0;
            string msg = success ? "Commande enregistrée avec succès!" : (apiResponse.Message ?? "Une erreur a été rencontrée!");
            if (success)
            {
                HttpContext.Session.SetListInSession<Article>("ArticlesCart", null);
                HttpContext.Session.SetInt32("CartBadgeCount", 0);

            }
            return Json(new FormResponse { Success = success, Object = apiResponse.Data, Message = msg });



        }

        [HttpPost]
       
        public async Task<JsonResult> PostLocalCommande(string? QrCode,string PaymentMethodeCode , PostCommandDto commandRequest)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelErrorHandler<PostCommandDto>.ModelStateError(ModelState));
            }
            User user = null;

            if (PaymentMethodeCode.ToUpper() == "QRCODE")
            {
                string matricule = String.Empty;
                var decryptInfo = DPWorldEncryption.SecurityManager.DecryptAES(QrCode) ?? String.Empty;
                // QrCodeStaff? qrCodeStaff=JsonConvert.DeserializeObject<QrCodeStaff>(decryptInfo!);
                if (!String.IsNullOrWhiteSpace(decryptInfo))
                {
                    matricule = decryptInfo.Substring(0, decryptInfo.Length - 1);

                }

                if (String.IsNullOrWhiteSpace(matricule))
                {
                    return Json(new FormResponse { Success = false, Message = "Utilisateur inconnu ou inexistant!" });
                }
                //Descript qrCode
                //Get User By matricule

                var url = String.Format(ApiUrlGeneric.GetUserByMatriculeURL, matricule);

                var apiResponse = await ApiService<User>.CallGet(_httpClientFactory, url);
                user = apiResponse.Data;
            }



            var listArticlesCart = HttpContext.Session.GetListObjectFromSession<Article>("ArticlesCart");
            var artcileCartToShopKeeper = listArticlesCart.GroupBy(d => d.Id)
                                  .Select(
                                      g =>
                                      {

                                          var a = new ArticleBooking
                                          {
                                              Id = g.Key,
                                             
                                          };
                                          a = _mapper.Map<ArticleBooking>(g.First());
                                          a.Quantite = commandRequest.LigneCommands?.Where(lc => lc.ArticleId == g.Key)?.FirstOrDefault()?.Quantite ?? 0;
                                          //a.Image=String.Empty;
                                          if (a.Categorie != null)
                                          {
                                              a.Categorie.ArticlesNavigation = null;
                                          }
                                        
                                          return a;
                                      }

                                  ).ToList();

            int montantTotal = 0;
            //Supprimer les articles avec quantité=0
            artcileCartToShopKeeper = artcileCartToShopKeeper.Where(a => a.Quantite > 0).ToList();
            if (artcileCartToShopKeeper != null)
            {
                foreach (var item in artcileCartToShopKeeper)
                {
                    montantTotal += item.PrixDeVente * item.Quantite;
                }

            }

            bool success = (artcileCartToShopKeeper?.Count ?? 0) > 0;
            string msg = success ? "Commande envoyée au gérant de la boutique!" : "Une erreur a été rencontrée!";
            if (success)
            {
                HttpContext.Session.SetListInSession<Article>("ArticlesCart", null);
                HttpContext.Session.SetInt32("CartBadgeCount", 0);

            }
            return Json(new FormResponse { Success = success, Object = new { Articles=artcileCartToShopKeeper, MontantTotal= montantTotal ,PaymentMethod=new{Id=commandRequest.PaymentMethodId,Code=PaymentMethodeCode }, User=user}, Message = msg });



        }



        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<JsonResult> GetPendingCommandes()
        {

            var url = String.Format(ApiUrlGeneric.ReadCommandesByStateURL, CommandStateEnum.Pending);
            var apiResponse = await ApiService<Commande>.CallGetList(_httpClientFactory, url);
            return Json(apiResponse);
        }
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<JsonResult> GetAllCommandes(DateTime? startDate = null, DateTime? endDate = null)
        {
            var url = String.Empty;
            if (startDate == null || endDate == null)
            {
                 url = String.Format(ApiUrlGeneric.ReadAllURL<Commande>());
            }
            else
            {
                 url = String.Format(ApiUrlGeneric.ReadAllCommandesFilterURL,null, startDate?.ToString("yyyy-MM-ddTHH:mm"), endDate?.ToString("yyyy-MM-ddTHH:mm"));
            }

            var apiResponse = await ApiService<CommandeReadDto>.CallGetList(_httpClientFactory, url);
            return Json(apiResponse);
        }

        [Authorize(Roles = IdentityData.AdminOrUserRoles)]
        public async Task<JsonResult> GetMyCommandes(DateTime? startDate = null, DateTime? endDate = null)
        {
            var url = String.Empty;
            
                url = String.Format(ApiUrlGeneric.GetMyCommandsURL, HttpContext.Session.GetInt32("UserId"), startDate?.ToString("yyyy-MM-ddTHH:mm"), endDate?.ToString("yyyy-MM-ddTHH:mm"));
            
         

            var apiResponse = await ApiService<CommandeReadDto>.CallGetList(_httpClientFactory, url);
            return Json(apiResponse);
        }
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<JsonResult> GetPendingCommandsCount()
        {
            var url = String.Empty;

            url = String.Format(ApiUrlGeneric.GetPendingCommandesCountURL);



            var apiResponse = await ApiService<String>.CallGet(_httpClientFactory, url);
            return Json(apiResponse);
        }


        [HttpPost]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<JsonResult> Delivery(int id)
        {
            var url = string.Format(ApiUrlGeneric.ChangeStateCommandeURL, id);
            var apiResponse = await ApiService<string>.CallApiPut(_httpClientFactory, url, new { });
            apiResponse.Success = apiResponse.StatusCode == System.Net.HttpStatusCode.NoContent;
            var status = apiResponse.Success ? "success" : "error";
            apiResponse.Message = apiResponse.Success
                ? "Commande livrée avec succès."
                : (apiResponse.Message ?? "Une erreur a été rencontrée lors de la livraison de la commande.");
            return Json(new { apiResponse.Success, Status = status, apiResponse.Message, apiResponse.Data });
        }

        [HttpPost]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public async Task<JsonResult> Reject(int commandId,string motif)
        {
            var url = String.Format(ApiUrlGeneric.RejectCommandeURL, commandId);

            var apiResponse = await ApiService<String>.CallApiPost(_httpClientFactory, url,new RejectCommandRequest {Reason=motif,Id=commandId});

            apiResponse.Success = apiResponse.StatusCode == System.Net.HttpStatusCode.NoContent;
            apiResponse.Message = apiResponse.Success ? "Commande rejetée" : (apiResponse.Message ?? "Une erreur a été rencontrée!");
            return Json(new FormResponse { Success = apiResponse.Success, Object = apiResponse.Success, Message = apiResponse.Message });
          
        }

        [HttpPost]
        [Authorize(Roles = IdentityData.AdminOrUserRoles)]
        public async Task<JsonResult> FeedBack(int commandId, string feedback,int? rate)
        {
            var url = String.Format(ApiUrlGeneric.CommandeFeedBackURL, commandId);
            var rateRequest = new CommandRatingRequest { CustomerFeedback = feedback, Rate = rate, Id = commandId };
            var apiResponse = await ApiService<String>.CallApiPost(_httpClientFactory, url, rateRequest);

            apiResponse.Success = apiResponse.StatusCode == System.Net.HttpStatusCode.NoContent;
            apiResponse.Message = apiResponse.Success ? "Votre avis a été envoyé." : (apiResponse.Message ?? "Une erreur a été rencontrée!");
            return Json(new FormResponse { Success = apiResponse.Success, Object = rateRequest, Message = apiResponse.Message });

        }

        [Authorize(Roles = IdentityData.AdminOrUserRoles)]
        [HttpDelete]
        public async Task<JsonResult> DeleteCommande(int id)
        {
            var url = String.Format(ApiUrlGeneric.DeleteURL<Commande>(), id);

            var apiResponse = await ApiService<String>.CallApiDelete(_httpClientFactory, url);

            apiResponse.Success = apiResponse.StatusCode == System.Net.HttpStatusCode.NoContent;
            apiResponse.Message = apiResponse.Success ? "Commande supprimée avec succés." : (apiResponse.Message ?? "Une erreur a été rencontrée!");
            return Json(apiResponse);
        }



        //[ChildActionOnly]
        [Authorize(Roles = IdentityData.AdminOrGerantUserRoles)]
        public PartialViewResult InvoicePartial(SoldAmountReport data)
        {


            return PartialView("_InvoicePartial", data);
        }


        [HttpPost]
        [Authorize(Roles = IdentityData.AdminOrUserRoles)]
        public async Task<JsonResult> GenerateReport(SoldAmountRequest soldAmountRequest)
        {
            if(!(soldAmountRequest.PaymentMethods?.Any()??false))
            {
                return Json(new ApiResponse<SoldAmountReport> {Message="Veuillez sélectionner au moins un moyen de paiement.",Success=false });
            }

            var url = String.Format(ApiUrlGeneric.GenerateSoldAmountReportURL);
            
            var apiResponse = await ApiService<SoldAmountReport>.CallApiPost(_httpClientFactory, url, soldAmountRequest);

            apiResponse.Success = apiResponse.Data !=null;
            apiResponse.Message = apiResponse.Success ? "Données générées avec succès." : (apiResponse.Message ?? "Une erreur a été rencontrée!");
            return Json(apiResponse);

        }

    }
}
