using System;
using System.Web;
using System.Collections.Generic;
using System.Reflection;

using Telerik.Sitefinity.Security.Model;

using Newtonsoft.Json;

using UDC.Common;
using UDC.Common.Data.Models;

using UDC.SitefinityContextPlugin.Configuration;
using UDC.SitefinityContextPlugin.Data;

namespace UDC.SitefinityContextPlugin.Handlers
{
    public class DataConnectorUIHandler : BaseHandler
    {
        public DataConnectorUIHandler()
        {

        }

        override protected void ExecMain(HttpContext context)
        {
            Boolean blnAuthorised = false;
            String targetURL = "";
            String appSecret = "";
            String token = "";

            if (CMSDataIO.IsAuthenticated())
            {
                if (CMSDataIO.IsBackendUser())
                {
                    User objUser = CMSDataIO.GetCurrentUser();

                    if (objUser != null)
                    {
                        Boolean blnIsConnectionAdmin = CMSDataIO.IsUserInRole(objUser.Id, Constants.RoleConnectionAdmin);
                        Boolean blnIsSyncManager = CMSDataIO.IsUserInRole(objUser.Id, Constants.RoleSyncManager);

                        if (blnIsConnectionAdmin || blnIsSyncManager)
                        {
                            DataConnectorConfig objDataConnectorConfig = Telerik.Sitefinity.Configuration.Config.Get<DataConnectorConfig>();
                            
                            targetURL = objDataConnectorConfig.AdminUIUrl.Value;
                            appSecret = objDataConnectorConfig.SharedKey.Value;
                            
                            objDataConnectorConfig = null;

                            Dictionary<String, Object> objToken = new Dictionary<String, Object>();

                            objToken.Add("Salt_1", KeyGenerator.GetUniqueKey(64));
                            objToken.Add("UserId", objUser.Id);
                            objToken.Add("Username", objUser.UserName);
                            objToken.Add("Email", objUser.Email);

                            objToken.Add("IsConnectionAdmin", blnIsConnectionAdmin);
                            objToken.Add("IsSyncManager", blnIsSyncManager);

                            String referringPage = "";
                            String assemblyName = "Sitefinity";
                            String versionInfo = "13.3.X";
                            
                            if (HttpContext.Current.Request.UrlReferrer != null)
                            {
                                referringPage = HttpContext.Current.Request.UrlReferrer.ToString();
                            }

                            //Dynamically get Sitefinity Version Info straight from Core Assembly
                            Assembly objAssembly = null;
                            try
                            {
                                objAssembly = Assembly.Load("Telerik.Sitefinity");
                                if(objAssembly != null)
                                {
                                    Version version = objAssembly.GetName().Version;
                                    assemblyName = objAssembly.GetName().Name;
                                    if (version != null)
                                    {
                                        versionInfo = version.ToString();
                                    }
                                    version = null;
                                }
                            }
                            catch(Exception ex){}
                            objAssembly = null;

                            objToken.Add("ReferringApplication", assemblyName + " - " + versionInfo);
                            objToken.Add("ReferringApplicationURL", referringPage);

                            objToken.Add("TokenDate", DateTime.UtcNow);
                            objToken.Add("Salt_2", KeyGenerator.GetUniqueKey(64));

                            token = JsonConvert.SerializeObject(objToken);

                            objToken = null;

                            blnAuthorised = true;
                        }
                    }

                    objUser = null;
                }
            }
            if (blnAuthorised)
            {
                //Do Redirect with Token Payload in Http Header Here...
                Boolean blnValidToken = false;
                String authPayload = "";

                if (!String.IsNullOrEmpty(token))
                {
                    try
                    {
                        authPayload = Cryptor.Encrypt(token, appSecret);
                    }
                    catch (Exception ex) { }
                    if (!String.IsNullOrEmpty(authPayload))
                    {
                        blnValidToken = true;
                    }
                }
                if (blnValidToken)
                {
                    Dictionary<String, String> objParams = new Dictionary<String, String>();
                    objParams.Add("AuthToken", authPayload);
                    RedirectWithData(objParams, targetURL);
                }
                else
                {
                    context.Response.ContentType = "application/json";
                    context.Response.Write(JsonConvert.SerializeObject(new APIResponse(1, "Invalid Token")));
                }
            }
            else
            {
                context.Response.ContentType = "application/json";
                context.Response.Write(JsonConvert.SerializeObject(new APIResponse(1, "Invalid Token")));
            }
        }
    }
}