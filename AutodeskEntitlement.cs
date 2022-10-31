using Autodesk.WebServices;
using log4net;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace $safeprojectname$
{
    internal class AutodeskEntitlement
    {
        public static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static bool ValidUser(Inventor.Application InvApp, string appId)
        {
            //Steps to get the user id
            string userName;
            string userId = "";

            //Force Login to Autodesk SSO = hack for Inventor 2020+
            InvApp.Login();

            try
            {
                userId = WebServicesUtils_18Plus.GetUserId();
                //MessageBox.Show("User Id = " + _userID);                
            }
            catch
            {
                userId = WebServicesUtils.GetUserId(out userName);
            }

            //Not logged in with Autodesk Id, hence we can not get user id
            if (userId.Equals(""))
            {
                return false;
            }

            //Check for online entitlement
            RestClient client = new RestClient("https://apps.autodesk.com");
            RestRequest req = new RestRequest("webservices/checkentitlement");

            // Set request parameters-
            req.Method = Method.GET;
            req.AddParameter("userid", userId);
            req.AddParameter("appid", appId);

            Logger.Info($"App ID {appId}");

            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            IRestResponse<EntitlementResponse> resp = client.Execute<EntitlementResponse>(req);

            if (resp.Data != null && resp.Data.IsValid)
            {
                //User has downloaded the App from the store and hence is a valid user...
                Logger.Info($"User {resp.Data.UserId} is valid");
                return true;
            }
            else
            {
                //Not a valid user. Entitlement check failed.  
                Logger.Info($"User {resp.Data.UserId} is invalid");
                return false;
            }
        }
    }

    class WebServicesUtils_18Plus
    {
        public static string GetUserId()
        {
            //string username = "";
            CWebServicesManager mgr = new CWebServicesManager();
            bool isInitialize = mgr.Initialize();

            if (isInitialize == true)
            {
                string userId = "";
                mgr.GetUserId(ref userId);
                string username = "";
                mgr.GetLoginUserName(ref username);
                return userId;
            }
            else
            {
                return "";
            }
        }
    }

    class WebServicesUtils
    {

        [DllImport("AdWebServices", EntryPoint = "GetUserId", CharSet = CharSet.Unicode)]
        private static extern int AdGetUserId(StringBuilder userid, int buffersize);

        [DllImport("AdWebServices", EntryPoint = "IsWebServicesInitialized")]
        private static extern bool AdIsWebServicesInitialized();

        [DllImport("AdWebServices", EntryPoint = "InitializeWebServices")]
        private static extern void AdInitializeWebServices();

        [DllImport("AdWebServices", EntryPoint = "IsLoggedIn")]
        private static extern bool AdIsLoggedIn();

        [DllImport("AdWebServices", EntryPoint = "GetLoginUserName", CharSet = CharSet.Unicode)]
        private static extern int AdGetLoginUserName(StringBuilder username, int buffersize);

        internal static string _GetUserId()
        {
            int buffersize = 128; //should be long enough for userid
            StringBuilder sb = new StringBuilder(buffersize);
            int len = AdGetUserId(sb, buffersize);
            sb.Length = len;
            return sb.ToString();
        }

        internal static string _GetUserName()
        {
            int buffersize = 128; //should be long enough for username
            StringBuilder sb = new StringBuilder(buffersize);
            int len = AdGetLoginUserName(sb, buffersize);
            sb.Length = len;
            return sb.ToString();
        }

        public static string GetUserId(out string userName)
        {
            AdInitializeWebServices();

            if (!AdIsWebServicesInitialized())
            {
                throw new Exception("Could not initialize the web services component.");
            }

            if (!AdIsLoggedIn())
            {
                throw new Exception("User is not logged in.");
            }

            string userId = _GetUserId();
            if (userId == "")
            {
                throw new Exception("Could not get user id.");
            }

            userName = _GetUserName();
            if (userName == "")
            {
                throw new Exception("Could not get user name.");
            }

            return userId;
        }
    }

    [Serializable]
    internal class EntitlementResponse
    {
        public string UserId { get; set; }
        public string AppId { get; set; }
        public bool IsValid { get; set; }
        public string Message { get; set; }
    }
}
