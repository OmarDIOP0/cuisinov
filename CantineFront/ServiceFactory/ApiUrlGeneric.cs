using CantineFront.Static;
using CantineFront.Models;

using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.AccessControl;

namespace CantineFront.Services
{
    public class ApiUrlGeneric  
    {
        public ApiUrlGeneric()
        {
                
        }
        public static AppSettings AppSettings { get; set; }

        public ApiUrlGeneric(IOptions<AppSettings> appSettings)
        {
             AppSettings = appSettings.Value;

        }
        public static string CreateURL<T>() =>AppSettings.ApiUrl + $"/api/{ typeof(T).Name}s"; //; { get { return  _appSettings.ApiUrl + $"/api/{ typeof(T).Name}s" ; } }
        public static  string UpdateURL<T>() => AppSettings.ApiUrl + $"/api/{typeof(T).Name}s" + "/{0}";  //{ get { return  _appSettings.ApiUrl + $"/api/{ typeof(T).Name}s"  + "/{0}"; } }
        public static  string ReadOneURL<T>() => AppSettings.ApiUrl + $"/api/{typeof(T).Name}s" + "/{0}"; // { get { return _appSettings.ApiUrl + $"/api/{ typeof(T).Name}s"  + "/{0}"; } }
        public static  string ReadAllURL<T>() => AppSettings.ApiUrl + $"/api/{typeof(T).Name}s"; // { get { return _appSettings.ApiUrl + $"/api/{ typeof(T).Name}s" ; } }
        public static string ReadArticlesByCategoryURL { get { return AppSettings.ApiUrl + "/api/Articles?categorieId={0}" ; } }
        public static string GetAllCategories { get { return AppSettings.ApiUrl + "/api/Categories/GetAllCategories"; } }
        public static string UpdateUserProfile { get { return AppSettings.ApiUrl + "/api/Users/UpdateProfileUser"; } }
        public static string GetArticleImagesURL { get { return AppSettings.ApiUrl + "/api/Articles/GetArticleImages"; } }
        public static string MouvementArticleURL { get { return AppSettings.ApiUrl + "/api/Articles/Mouvement/{0}?quantite={1}&mouvement={2}"; } }
        public static string ReadCommandesByStateURL { get { return AppSettings.ApiUrl + "/api/Commandes?state={0}" ; } }
        public static string ReadAllCommandesFilterURL => AppSettings.ApiUrl + "/api/Commandes?state={0}&startDate={1}&endDate={2}"; // { get { return _appSettings.ApiUrl + $"/api/{ typeof(T).Name}s" ; } }
        public static string ChangeStateCommandeURL { get { return AppSettings.ApiUrl + "/api/Commandes/ChangeState/{0}"; } }
        public static string RejectCommandeURL { get { return AppSettings.ApiUrl + "/api/Commandes/Reject/{0}"; } }
        public static string CommandeFeedBackURL { get { return AppSettings.ApiUrl + "/api/Commandes/CommandFeedBack/{0}"; } }
        public static string GenerateSoldAmountReportURL { get { return AppSettings.ApiUrl + "/api/Commandes/GetSoldAmountByPaymentMethod"; } }
        public static string GetMyCommandsURL { get { return AppSettings.ApiUrl + "/api/Commandes/GetMyCommands/{0}?startDate={1}&endDate={2}"; } }
        public static string GetPendingCommandesCountURL { get { return AppSettings.ApiUrl + "/api/Commandes/GetPendingCommandesCount"; } }
        public static string GetMenuArticlesURL { get { return AppSettings.ApiUrl + "/api/Articles/GetMenu?categorieId={0}"; } }
        public static string UpdateArticleStatusURL { get { return AppSettings.ApiUrl + "/api/Articles/UpdateStatus/{0}"; } }
        public static string ApproveArticleStatusURL { get { return AppSettings.ApiUrl + "/api/Articles/Approve/{0}"; } }
        public static string AuthenticateURL { get { return AppSettings.ApiUrl + "/api/Users/Authenticate?username={0}&password={1}"; } }
        public static string RegisterURL { get { return AppSettings.ApiUrl + "/api/Users/Register"; } }
        public static string GetUserByMatriculeURL { get { return AppSettings.ApiUrl + "/api/Users/UserByMatricule?matricule={0}"; } }
        public static string GenerateAllQrCodeURL { get { return AppSettings.ApiUrl + "/api/Users/GenerateAllQrCode"; } }
        public static string SendAllQrCodeURL { get { return AppSettings.ApiUrl + "/api/Users/SendAllQrCode"; } }
        public static string SendUserQrCodeURL { get { return AppSettings.ApiUrl + "/api/Users/SendUserQrCode/{0}"; } }
        public static string RechargerCompteURL { get { return AppSettings.ApiUrl + "/api/Users/RechargerCompte/{0}?montant={1}"; } }
        public static string RechargerMultiCompteURL { get { return AppSettings.ApiUrl + "/api/Users/RechargerMultiCompte"; } }
        public static string GetSoldeURL { get { return AppSettings.ApiUrl + "/api/Users/GetSolde/{0}"; } }
        public static string ResetPasswordURL { get { return AppSettings.ApiUrl + "/api/Users/ResetPassword"; } }
        public static string ForgotPasswordURL { get { return AppSettings.ApiUrl + "/api/Users/ForgotPassword"; } }
        public static string ForgetPasswordURL { get { return AppSettings.ApiUrl + "/api/Users/ForgetPassword/{0}"; } }
        public static string ResetPasswordMultiAccountsURL { get { return AppSettings.ApiUrl + "/api/Users/ResetPasswordMultiAccounts"; } }
        public static string RenitializePasswordURL { get { return AppSettings.ApiUrl + "/api/Users/RenitializePassword/{0}"; } }
        public static string RefreshTokenURL { get { return AppSettings.ApiUrl + "/api/Token/refresh"; } }
        public static string RevokeTokenURL { get { return AppSettings.ApiUrl + "/api/Token/revoke"; } }
        public static string DeleteURL<T>() => AppSettings.ApiUrl + $"/api/{typeof(T).Name}s" + "/{0}"; // { get { return  _appSettings.ApiUrl + $"/api/{ typeof(T).Name}s"  + "/{0}"; } }

    }
}