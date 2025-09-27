using System;
using System.Collections.Generic;
using System.Configuration;

using System.Linq;
using System.Reflection.PortableExecutable;
using System.Web;

namespace CantineFront.Helpers
{
    public class LoginManager
    {
        public static Tuple<bool,List<string>> AuthenticateUser(string username, string password)
        {
            try
            {
                //if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                //{
                //    string activeDirectoryServer = ConfigurationManager.AppSettings["domain"].ToString();

                //    DirectoryEntry Ldap = new DirectoryEntry("LDAP://" + activeDirectoryServer, username, password);

                //    if (Ldap.Guid == null)
                //    {
                //        return new Tuple<bool, List<string>>(false,null);
                //    }

                //    var groups = LoginManager.GetGroups(username);
                //   // string group = groups.FirstOrDefault(g => g.Name.StartsWith("CCPO_"))?.Name;
                //    var backofficeProfils = groups.Where(g => g.Name.StartsWith("DPW_BO_"))?.Select(g => g.Name).ToList();
                //    backofficeProfils.Add("DPW_BO_COMMERCIAL");
                //    //group =  CCPO_CLERK |CCPO_SUPERVISOR |CCPO_SENIOR_OPERATOR 
                //    // return new Tuple<bool, string>(!String.IsNullOrWhiteSpace(group), group);
                //    return new Tuple<bool, List<string>>(true, backofficeProfils);
                //    //return true;
                //}
                // return false;
                return new Tuple<bool, List<string>>(false, null);
            }
            catch (Exception ex)
            {
          //     Logger.Error(ex.Message);
                // return false;
                return new Tuple<bool, List<string>>(false, null);
            }
        }


    }


   
}