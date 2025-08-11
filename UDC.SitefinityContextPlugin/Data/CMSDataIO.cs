using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using System.Web;
using System.IO;

using Telerik.Sitefinity;
using Telerik.Sitefinity.Modules.Pages;
using Telerik.Sitefinity.Pages.Model;
using Telerik.Sitefinity.Security;
using Telerik.Sitefinity.Security.Claims;
using Telerik.Sitefinity.Security.Model;
using Telerik.Sitefinity.Frontend.GridSystem;
using Telerik.Sitefinity.Modules.Forms;
using Telerik.Sitefinity.Forms.Model;
using Telerik.Sitefinity.Newsletters.Model;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Modules.Newsletters;
using Telerik.Sitefinity.Modules.Libraries;
using Telerik.Sitefinity.Services.Notifications;
using Telerik.Sitefinity.Services;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Data.Linq.Dynamic;
using Telerik.Sitefinity.Taxonomies.Model;
using Telerik.Sitefinity.Taxonomies;
using Telerik.Sitefinity.Utilities.TypeConverters;
using Telerik.Sitefinity.Multisite;
using Telerik.Sitefinity.Publishing;
using Telerik.Sitefinity.Publishing.Configuration;
using Telerik.Sitefinity.Publishing.Web.Services;
using Telerik.Sitefinity.Publishing.Model;
using Telerik.Sitefinity.Data.Metadata;
using Telerik.Sitefinity.Metadata.Model;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Versioning;
using Telerik.Sitefinity.Versioning.Model;

using UDC.Common;
using UDC.Common.Data.Models;
using UDC.SitefinityContextPlugin.Models;

using Newtonsoft.Json.Linq;

namespace UDC.SitefinityContextPlugin.Data
{
    public class CMSDataIO
    {
        #region Page Helpers...
        public static PageNode GetPageNodeById(String id)
        {
            return GetPageNodeById(new Guid(id));
        }
        public static PageNode GetPageNodeById(Guid id)
        {
            PageManager objPageManager = PageManager.GetManager();
            PageNode objNode = objPageManager.GetPageNodes().FirstOrDefault(n => n.Id == id);

            objPageManager = null;

            return objNode;
        }

        public static String GetPageNodeUrlById(String id)
        {
            return GetPageNodeUrlById(new Guid(id));
        }
        public static String GetPageNodeUrlById(Guid id)
        {
            PageManager objPageManager = PageManager.GetManager();
            PageNode objNode = objPageManager.GetPageNodes().FirstOrDefault(n => n.Id == id);
            if (objNode != null)
            {
                return objNode.GetUrl().Replace("~", "");
            }

            objPageManager = null;

            return String.Empty;
        }

        public static PageData GetPageData(Guid id)
        {
            PageManager objPageManager = PageManager.GetManager();
            PageData objPageData = objPageManager.GetPageData(id);

            objPageManager = null;

            return objPageData;
        }

        public static PageNode GetCurrentPageNode()
        {
            PageNode objPageNode = null;
            SiteMapNode currentNode = SiteMap.CurrentNode;
            if (currentNode != null)
            {
                PageManager objPageManager = PageManager.GetManager();
                Guid pageId = new Guid(currentNode.Key);
                objPageNode = objPageManager.GetPageNode(pageId);

                objPageManager = null;
            }

            currentNode = null;

            return objPageNode;
        }

        public static IQueryable<PageNode> GetChildPageNodesByParentId(String id)
        {
            var isGuid = Guid.TryParse(id, out var guidOut);
            if (!isGuid)
            {
                return null;
            }
            return GetChildPageNodesByParentId(guidOut);
        }
        public static IQueryable<PageNode> GetChildPageNodesByParentId(Guid id)
        {
            var pageManager = PageManager.GetManager();
            return pageManager.GetPageNodes().Where(n => n.ParentId == id);
        }

        public static String GetCurrentPageTemplate()
        {
            var result = "";
            try
            {
                result = GetCurrentPageNode().GetPageData().Template.Name.ToLower().Trim().Replace(" ", "");
            }
            catch (Exception ex)
            {
            }
            return result;
        }
        public static String GetCurrentPageTemplateClass()
        {
            return "template-" + GetCurrentPageTemplate();
        }

        public static Boolean CurrentIsHomepage()
        {
            var result = false;
            try
            {
                result = GetCurrentPageTemplate() == "homepage";
            }
            catch (Exception ex)
            {
            }
            return result;
        }
        #endregion

        #region Page Control Helpers...
        public static List<ControlData> GetPageControls(ref PageData pageData)
        {
            List<ControlData> arrRetVal = new List<ControlData>();
            IList<PageControl> arrPageControls = null;

            if (pageData != null)
            {
                arrPageControls = pageData.Controls;
                if (arrPageControls != null && arrPageControls.Count > 0)
                {
                    arrRetVal.AddRange(arrPageControls);
                }
            }

            return arrRetVal;
        }
        public static List<ControlData> GetTemplateControls(ref PageData pageData)
        {
            List<ControlData> arrRetVal = new List<ControlData>();
            IList<TemplateControl> arrTemplateControls = null;

            if (pageData != null && pageData.Template != null)
            {
                arrTemplateControls = pageData.Template.Controls;
                if (arrTemplateControls != null && arrTemplateControls.Count > 0)
                {
                    arrRetVal.AddRange(arrTemplateControls);
                }
            }

            return arrRetVal;
        }
        public static List<ControlData> GetAllPageControls(ref PageData pageData)
        {
            List<ControlData> arrRetVal = new List<ControlData>();
            IList<TemplateControl> arrTemplateControls = null;
            IList<PageControl> arrPageControls = null;

            if (pageData != null && pageData.Template != null)
            {
                arrTemplateControls = pageData.Template.Controls;
                arrPageControls = pageData.Controls;

                if (arrTemplateControls != null && arrTemplateControls.Count > 0)
                {
                    arrRetVal.AddRange(arrTemplateControls);
                }
                if (arrPageControls != null && arrPageControls.Count > 0)
                {
                    arrRetVal.AddRange(arrPageControls);
                }
            }

            return arrRetVal;
        }
        public static List<GridControl> GetGridControlInstances(ref List<ControlData> targetControls)
        {
            List<GridControl> arrRetVal = null;

            if (targetControls != null && targetControls.Count > 0)
            {
                PageManager objPageManager = PageManager.GetManager();

                foreach (ControlData objCtl in targetControls)
                {
                    Object objCtlInstance = objPageManager.LoadObject(objCtl);
                    if (objCtlInstance != null && objCtlInstance is GridControl)
                    {
                        if (arrRetVal == null) { arrRetVal = new List<GridControl>(); }
                        arrRetVal.Add((GridControl)objCtlInstance);
                    }
                }

                objPageManager = null;
            }

            return arrRetVal;
        }

        public static List<Dictionary<String, Object>> FindWidgetInstances(String typeName, bool searchTemplate)
        {
            List<Dictionary<String, Object>> objRetVal = new List<Dictionary<String, Object>>();
            PageManager pageManager = PageManager.GetManager();
            List<PageNode> arrPageNodes = pageManager.GetPageNodes().ToList();

            if (arrPageNodes != null)
            {
                foreach (PageNode objNode in arrPageNodes)
                {
                    if (!objNode.IsBackend)
                    {
                        PageData objPageData = objNode.GetPageData();
                        List<ControlData> arrControls = null;

                        if (searchTemplate)
                        {
                            arrControls = GetAllPageControls(ref objPageData);
                        }
                        else
                        {
                            arrControls = GetPageControls(ref objPageData);
                        }

                        if (arrControls != null)
                        {
                            List<String> arrCtls = new List<String>();

                            if (!String.IsNullOrEmpty(typeName))
                            {
                                arrControls = arrControls.Where(obj => obj.Caption.ToLower().IndexOf(typeName.ToLower()) > -1).ToList();
                            }
                            foreach (ControlData objCtl in arrControls)
                            {
                                arrCtls.Add(objCtl.Caption);
                            }
                            if (arrControls.Count > 0)
                            {
                                Dictionary<String, Object> objDest = new Dictionary<String, Object>();

                                objDest.Add("Page", objNode.GetFullPath("/"));
                                objDest.Add("InstanceCount", arrControls.Count);
                                objDest.Add("Instances", arrCtls);

                                objRetVal.Add(objDest);
                            }

                            arrCtls = null;
                        }

                        arrControls = null;
                        objPageData = null;
                    }
                }
            }

            arrPageNodes = null;
            pageManager = null;

            return objRetVal;
        }
        public static List<Dictionary<String, Object>> FindFormInstances(String formName, String formTitle, Guid formId, bool searchTemplate)
        {
            List<Dictionary<String, Object>> objRetVal = new List<Dictionary<String, Object>>();
            PageManager pageManager = PageManager.GetManager();
            List<PageNode> arrPageNodes = pageManager.GetPageNodes().ToList();

            if (arrPageNodes != null)
            {
                foreach (PageNode objNode in arrPageNodes)
                {
                    if (!objNode.IsBackend)
                    {
                        PageData objPageData = objNode.GetPageData();
                        List<ControlData> arrControls = null;

                        if (searchTemplate)
                        {
                            arrControls = GetAllPageControls(ref objPageData);
                        }
                        else
                        {
                            arrControls = GetPageControls(ref objPageData);
                        }
                        if (arrControls != null)
                        {
                            List<String> arrCtls = new List<String>();

                            arrControls = arrControls.Where(obj => obj.Caption.ToLower().IndexOf("form") > -1).ToList();

                            foreach (ControlData objCtl in arrControls)
                            {
                                Telerik.Sitefinity.Mvc.Proxy.MvcControllerProxy objCtlInstance = (Telerik.Sitefinity.Mvc.Proxy.MvcControllerProxy)pageManager.LoadObject(objCtl);
                                Telerik.Sitefinity.Mvc.Proxy.ControllerSettings objCtlSetting = (Telerik.Sitefinity.Mvc.Proxy.ControllerSettings)objCtlInstance.Settings;

                                if (objCtlSetting.Values["Model"].ToString() == "Telerik.Sitefinity.Frontend.Forms.Mvc.Models.FormModel")
                                {
                                    Telerik.Sitefinity.Frontend.Forms.Mvc.Models.FormModel objFormModel = (Telerik.Sitefinity.Frontend.Forms.Mvc.Models.FormModel)objCtlSetting.Values["Model"];

                                    Boolean blnAdd = false;
                                    if (formId == Guid.Empty && String.IsNullOrEmpty(formName) && String.IsNullOrEmpty(formTitle))
                                    {
                                        blnAdd = true;
                                    }
                                    else
                                    {
                                        if (formId != Guid.Empty && formId == objFormModel.FormId)
                                        {
                                            blnAdd = true;
                                        }
                                        if (!String.IsNullOrEmpty(formName) && formName.ToLower() == objFormModel.FormData.Name.ToLower())
                                        {
                                            blnAdd = true;
                                        }
                                        if (!String.IsNullOrEmpty(formTitle) && formTitle.ToLower() == objFormModel.FormData.Title.ToLower())
                                        {
                                            blnAdd = true;
                                        }
                                    }
                                    if (blnAdd)
                                    {
                                        arrCtls.Add(objFormModel.FormId + " : " + objFormModel.FormData.Name + " : " + objFormModel.FormData.Title);
                                    }

                                    objFormModel = null;
                                }

                                objCtlSetting = null;
                                objCtlInstance = null;
                            }
                            if (arrCtls.Count > 0)
                            {
                                Dictionary<String, Object> objDest = new Dictionary<String, Object>();

                                objDest.Add("Page", objNode.GetFullPath("/"));
                                objDest.Add("InstanceCount", arrControls.Count);
                                objDest.Add("Instances", arrCtls);

                                objRetVal.Add(objDest);
                            }

                            arrCtls = null;
                        }

                        arrControls = null;
                        objPageData = null;
                    }
                }
            }

            arrPageNodes = null;
            pageManager = null;

            return objRetVal;
        }
        #endregion

        #region User Helpers...
        public static User GetUserById(Guid userId)
        {
            UserManager objUserManager = UserManager.GetManager();
            User objUser = objUserManager.GetUser(userId);

            //objUserManager.EmailExists()
            //objUserManager.GetUserByEmail()
            //objUserManager.UserExists()
            //objUserManager.ValidateUser()
            //objUserManager.SaveChanges();

            objUserManager = null;

            return objUser;
        }
        public static User GetUserByEmail(String email)
        {
            UserManager objUserManager = UserManager.GetManager();
            User objUser = objUserManager.GetUserByEmail(email);

            objUserManager = null;

            return objUser;
        }
        public static User GetUserByUsername(String username)
        {
            UserManager objUserManager = UserManager.GetManager();
            User objUser = objUserManager.GetUser(username);

            objUserManager = null;

            return objUser;
        }
        
        public static User GetCurrentUser()
        {
            User retVal = null;
            SitefinityIdentity objIdentity = ClaimsManager.GetCurrentIdentity();
            //Guid objUserId = SecurityManager.GetCurrentUserId();

            //if(objUserId != Guid.Empty)
            //{
            //    retVal = GetUserById(objUserId);
            //}

            if (objIdentity != null)
            {
                if (objIdentity.UserId != Guid.Empty)
                {
                    retVal = GetUserById(objIdentity.UserId);
                }
            }
            objIdentity = null;

            return retVal;
        }

        public static Boolean IsAuthenticated()
        {
            Boolean retVal = false;
            SitefinityIdentity objIdentity = ClaimsManager.GetCurrentIdentity();

            if (objIdentity != null)
            {
                Boolean blnIsApproved = false;
                User objCurrUser = null;

                if (objIdentity.UserId != Guid.Empty)
                {
                    objCurrUser = GetUserById(objIdentity.UserId);
                    if (objCurrUser != null)
                    {
                        blnIsApproved = objCurrUser.IsApproved;
                    }
                }
                objCurrUser = null;

                retVal = objIdentity.IsAuthenticated && blnIsApproved;
            }
            objIdentity = null;

            return retVal;
        }
        public static Boolean IsBackendUser()
        {
            Boolean retVal = false;
            SitefinityIdentity objIdentity = ClaimsManager.GetCurrentIdentity();

            if (objIdentity != null)
            {
                retVal = (objIdentity.IsAuthenticated && objIdentity.IsBackendUser);

            }
            objIdentity = null;

            return retVal;
        }
        public static Boolean IsAccountEnabled()
        {
            Boolean retVal = false;
            User objCurrUser = GetCurrentUser();

            if (objCurrUser != null)
            {
                retVal = objCurrUser.IsApproved;
            }
            objCurrUser = null;

            return retVal;
        }

        //Profile Helpers
        public static SitefinityProfile GetUserProfile(User user)
        {
            UserProfileManager objProfileManager = UserProfileManager.GetManager();
            SitefinityProfile objProfile = objProfileManager.GetUserProfile<SitefinityProfile>(user);

            objProfileManager = null;

            return objProfile;
        }
        public static SitefinityProfile GetUserProfile(Guid userId)
        {
            User objUser = GetUserById(userId);
            UserProfileManager objProfileManager = UserProfileManager.GetManager();
            SitefinityProfile objProfile = objProfileManager.GetUserProfile<SitefinityProfile>(objUser);

            objUser = null;
            objProfileManager = null;

            return objProfile;
        }
        public static SitefinityProfile GetCurrentUserProfile()
        {
            SitefinityProfile objProfile = null;
            User objCurrUser = GetCurrentUser();

            if (objCurrUser != null)
            {
                objProfile = GetUserProfile(objCurrUser);
            }

            objCurrUser = null;

            return objProfile;
        }
        public static Boolean SaveUserProfile(Guid userId, Dictionary<String, Object> profile)
        {
            Boolean retVal = false;
            UserProfileManager objProfileManager = UserProfileManager.GetManager();
            User targetUser = GetUserById(userId);
            SitefinityProfile objSFProfile = null;

            if (targetUser != null && profile != null)
            {
                objSFProfile = objProfileManager.GetUserProfile<SitefinityProfile>(targetUser);
                if (profile != null && profile.Keys.Count > 0)
                {
                    foreach (String key in profile.Keys)
                    {
                        if (objSFProfile.DoesFieldExist(key))
                        {
                            objSFProfile.SetValue(key, profile[key]);
                        }
                    }
                    objProfileManager.SaveChanges();

                    retVal = true;
                }
            }

            objSFProfile = null;
            targetUser = null;
            objProfileManager = null;

            return retVal;
        }

        //Role Helpers
        public static Role GetRoleById(Guid roleId)
        {
            RoleManager objRoleManager = RoleManager.GetManager();
            Role objRole = objRoleManager.GetRole(roleId);

            objRoleManager = null;

            return objRole;
        }
        public static Role GetRoleByName(String roleName)
        {
            RoleManager objRoleManager = RoleManager.GetManager();
            List<Role> arrRoles = objRoleManager.GetRoles().ToList();
            Role objRole = null;

            if (arrRoles != null && arrRoles.Count > 0)
            {
                objRole = arrRoles.Where(obj => obj.Name.ToLower() == roleName.ToLower()).FirstOrDefault();
            }

            arrRoles = null;
            objRoleManager = null;

            return objRole;
        }
        public static RoleInfo GetRoleByNameAsRoleInfo(String roleName)
        {
            RoleManager objRoleManager = RoleManager.GetManager();
            RoleInfo objRoleInfo = null;
            Role objRole = GetRoleByName(roleName);

            if (objRole != null)
            {
                objRoleInfo = new RoleInfo();
                objRoleInfo.Id = objRole.Id;
                objRoleInfo.Name = objRole.Name;
                objRoleInfo.Provider = objRoleManager.Provider.Name;
            }
            objRole = null;
            objRoleManager = null;

            return objRoleInfo;
        }
        public static Role CreateRole(String roleName)
        {
            RoleManager objRoleManager = RoleManager.GetManager();
            Role targetRole = GetRoleByName(roleName);

            if(targetRole == null)
            {
                Boolean propCurrVal = objRoleManager.Provider.SuppressSecurityChecks;
                objRoleManager.Provider.SuppressSecurityChecks = true;

                targetRole = objRoleManager.CreateRole(roleName);
                objRoleManager.SaveChanges();

                objRoleManager.Provider.SuppressSecurityChecks = propCurrVal;
            }

            objRoleManager = null;

            return targetRole;
        }
        public static List<Role> GetUserRoles(Guid userId)
        {
            RoleManager objRoleManager = RoleManager.GetManager();
            List<Role> arrRoles = objRoleManager.GetRolesForUser(userId).ToList();

            objRoleManager = null;

            return arrRoles;
        }
        public static Boolean IsUserInRole(Guid userId, String roleName)
        {
            Boolean retVal = false;
            RoleManager objRoleManager = RoleManager.GetManager();

            retVal = objRoleManager.IsUserInRole(userId, roleName);

            objRoleManager = null;

            return retVal;
        }
        public static Boolean AddUserToRole(Guid userId, String roleName)
        {
            Boolean retVal = false;
            RoleManager objRoleManager = RoleManager.GetManager();
            User targetUser = GetUserById(userId);
            Role targetRole = GetRoleByName(roleName);

            if (targetUser != null && targetRole != null)
            {
                if (!objRoleManager.IsUserInRole(userId, roleName))
                {
                    Boolean propCurrVal = objRoleManager.Provider.SuppressSecurityChecks;
                    objRoleManager.Provider.SuppressSecurityChecks = true;

                    objRoleManager.AddUserToRole(targetUser, targetRole);
                    objRoleManager.SaveChanges();

                    objRoleManager.Provider.SuppressSecurityChecks = propCurrVal;
                }
                retVal = true;
            }

            targetRole = null;
            targetUser = null;
            objRoleManager = null;

            return retVal;
        }
        public static Boolean RemoveUserFromRole(Guid userId, String roleName)
        {
            Boolean retVal = false;
            RoleManager objRoleManager = RoleManager.GetManager();
            User targetUser = GetUserById(userId);
            Role targetRole = GetRoleByName(roleName);

            if (targetUser != null && targetRole != null)
            {
                if (objRoleManager.IsUserInRole(userId, roleName))
                {
                    Boolean propCurrVal = objRoleManager.Provider.SuppressSecurityChecks;
                    objRoleManager.Provider.SuppressSecurityChecks = true;

                    objRoleManager.RemoveUserFromRole(targetUser, targetRole);
                    objRoleManager.SaveChanges();

                    objRoleManager.Provider.SuppressSecurityChecks = propCurrVal;
                }
                retVal = true;
            }

            targetRole = null;
            targetUser = null;
            objRoleManager = null;

            return retVal;
        }
        public static Boolean RemoveUserFromAllRoles(Guid userId)
        {
            Boolean retVal = false;
            List<Role> arrCurrRoles = GetUserRoles(userId);

            if (arrCurrRoles != null)
            {
                foreach (Role objRole in arrCurrRoles)
                {
                    RemoveUserFromRole(userId, objRole.Name);
                }
                retVal = true;
            }
            arrCurrRoles = null;

            return retVal;
        }

        //Claims Mapping Helpers
        public static Boolean IsRoleClaimMappedToCurrentUser(RoleInfo roleInfo)
        {
            Boolean retVal = false;
            List<RoleInfo> arrCurrRoles = ClaimsManager.GetSitefinityRoles().ToList();

            if (roleInfo != null)
            {
                if (arrCurrRoles != null && arrCurrRoles.Count > 0)
                {
                    foreach (RoleInfo objCurrMapping in arrCurrRoles)
                    {
                        if (roleInfo.Id == objCurrMapping.Id)
                        {
                            retVal = true;
                            break;
                        }
                    }
                }
            }
            arrCurrRoles = null;

            return retVal;
        }
        public static Boolean AddRoleClaimToCurrentUser(String roleName)
        {
            Boolean retVal = false;
            RoleInfo objRoleInfo = GetRoleByNameAsRoleInfo(roleName);
            SitefinityPrincipal objPrincipal = ClaimsManager.GetCurrentPrincipal();
            ClaimsIdentity objIdentity = objPrincipal.Identities.First();

            if (objRoleInfo != null && objIdentity != null)
            {
                if (!IsRoleClaimMappedToCurrentUser(objRoleInfo))
                {
                    ClaimsManager.SetSitefinityRoles(objIdentity, new List<RoleInfo>() { objRoleInfo });
                    retVal = true;
                }
            }

            objIdentity = null;
            objPrincipal = null;
            objRoleInfo = null;

            return retVal;
        }
        public static Boolean RemoveRoleClaimFromCurrentUser(String roleName)
        {
            Boolean retVal = false;
            RoleInfo objRoleInfo = GetRoleByNameAsRoleInfo(roleName);
            SitefinityPrincipal objPrincipal = ClaimsManager.GetCurrentPrincipal();
            ClaimsIdentity objIdentity = objPrincipal.Identities.First();

            if (objRoleInfo != null && objIdentity != null)
            {
                if (IsRoleClaimMappedToCurrentUser(objRoleInfo))
                {
                    List<RoleInfo> arrRoles = ClaimsManager.GetSitefinityRoles().ToList();
                    if (arrRoles != null)
                    {
                        foreach (RoleInfo objRole in arrRoles)
                        {
                            if (objRole.Name.ToLower() == roleName.ToLower())
                            {
                                arrRoles.Remove(objRole);
                                break;
                            }
                        }
                    }
                    ClaimsManager.SetSitefinityRoles(objIdentity, arrRoles);
                    retVal = true;
                }
            }

            objIdentity = null;
            objPrincipal = null;
            objRoleInfo = null;

            return retVal;
        }

        //Authentication Helpers
        public static Boolean AuthenticateUser(String username, String password, Boolean isPersistent)
        {
            Boolean retVal = false;
            User user;
            UserLoggingReason result = SecurityManager.AuthenticateUser(UserManager.GetDefaultProviderName(), username, password, isPersistent, out user);

            if (result == UserLoggingReason.Success || result == UserLoggingReason.UserAlreadyLoggedIn)
            {
                retVal = true;
            }

            return retVal;
        }

        // FIRE-817: SecurityManager.SkipAuthenticationAndLogin is no longer supported in 14.4
        //public static Boolean AuthenticateUser(String username, Boolean isPersistent)
        //{
        //    String baseURL = "https://" + HttpContext.Current.Request.Url.DnsSafeHost;
        //    Boolean retVal = false;
        //    UserLoggingReason result = SecurityManager.SkipAuthenticationAndLogin(UserManager.GetDefaultProviderName(), username, isPersistent, baseURL + "/logon", baseURL + "/home");

        //    if (result == UserLoggingReason.Success)
        //    {
        //        retVal = true;
        //    }

        //    return retVal;
        //}
        #endregion

        #region Form Helpers...
        public static FormDescription GetFormById(String Id)
        {
            return GetFormById(new Guid(Id));
        }
        public static FormDescription GetFormById(Guid Id)
        {
            FormDescription retVal = null;
            FormsManager formManager = FormsManager.GetManager();

            retVal = formManager.GetForms().Where(f => f.Id == Id).SingleOrDefault();
            formManager = null;

            return retVal;
        }
        public static FormDescription GetFormByName(String formName)
        {
            FormDescription retVal = null;
            FormsManager formManager = FormsManager.GetManager();

            retVal = formManager.GetFormByName(formName);
            formManager = null;

            return retVal;
        }
        public static List<FormDescription> GetAllForms()
        {
            List<FormDescription> retVal = null;
            FormsManager formManager = FormsManager.GetManager();

            retVal = formManager.GetForms().ToList<FormDescription>();
            formManager = null;

            return retVal;
        }

        public static List<FormEntry> GetFormResponses(Guid formId)
        {
            List<FormEntry> retVal = null;
            FormsManager formManager = FormsManager.GetManager();
            FormDescription objForm = GetFormById(formId);

            if (objForm != null)
            {
                retVal = formManager.GetFormEntries(new FormDescription(objForm.Name)).ToList();
            }
            objForm = null;
            formManager = null;

            return retVal;
        }
        public static List<FormEntry> GetFormResponses(String formName)
        {
            List<FormEntry> retVal = null;
            FormsManager formManager = FormsManager.GetManager();
            FormDescription objForm = GetFormByName(formName);

            if (objForm != null)
            {
                retVal = formManager.GetFormEntries(new FormDescription(objForm.Name)).ToList();
            }
            objForm = null;
            formManager = null;

            return retVal;
        }

        public static List<ISubscriberRequest> GetListSubscribers(ServiceContext serviceContext, Guid listId)
        {
            List<ISubscriberRequest> retVal = null;
            INotificationService objNotifSvc = SystemManager.GetNotificationService();

            retVal = objNotifSvc.GetSubscribers(serviceContext, listId, null).ToList<ISubscriberRequest>();
            objNotifSvc = null;

            return retVal;
        }
        public static ISubscriberRequest GetSubscriber(ServiceContext serviceContext, Guid subscriberId)
        {
            ISubscriberRequest retVal = null;
            INotificationService objNotifSvc = SystemManager.GetNotificationService();

            retVal = objNotifSvc.GetSubscriber(serviceContext, subscriberId);
            objNotifSvc = null;

            return retVal;
        }

        public static List<Subscriber> GetNewsMailingListSubscribers(Guid listId)
        {
            List<Subscriber> retVal = null;
            NewslettersManager listManager = NewslettersManager.GetManager();
            MailingList mailingList = listManager.GetMailingLists().Where(obj => obj.Id == listId).FirstOrDefault();

            if (mailingList != null)
            {
                retVal = mailingList.Subscribers().ToList();
            }
            mailingList = null;
            listManager = null;

            return retVal;
        }
        public static Subscriber GetNewsMailingListSubscriber(Guid subscriberId)
        {
            Subscriber retVal = null;
            NewslettersManager listManager = NewslettersManager.GetManager();

            retVal = listManager.GetSubscribers().Where(s => s.Id == subscriberId).FirstOrDefault();
            listManager = null;

            return retVal;
        }
        #endregion

        #region Dynamic Module Helpers...
        public static String GetDynamicProviderByModuleName(String moduleName)
        {
            //Sf -> Settings -> Advanced -> DynamicModules -> Providers...
            MultisiteContext objMSC = new MultisiteContext();
            String retVal = objMSC.CurrentSite.GetDefaultProvider(moduleName).ProviderName;

            objMSC = null;

            return retVal;
        }

        public static Boolean DynamicContentHasChildren(String providerName, Guid itemId, Type itemType)
        {
            DynamicModuleManager objDynamicModuleManager = DynamicModuleManager.GetManager(providerName);
            DynamicContent objDynamicContent = objDynamicModuleManager.GetDataItem(itemType, itemId);
            Boolean retVal = objDynamicModuleManager.HasChildItems(objDynamicContent);

            objDynamicContent = null;
            objDynamicModuleManager = null;

            return retVal;
        }
        public static List<DynamicContent> GetDynamicContentItems(String providerName, String typeName)
        {
            DynamicModuleManager objDynamicModuleManager = DynamicModuleManager.GetManager(providerName);
            Type objDynamicType = TypeResolutionService.ResolveType(typeName);
            List<DynamicContent> arrRetVal = null;

            arrRetVal = (List<DynamicContent>)objDynamicModuleManager.GetDataItems(objDynamicType).ToList();

            objDynamicType = null;
            objDynamicModuleManager = null;

            return arrRetVal;
        }
        public static DynamicContent GetDynamicContentItem(String providerName, String typeName, Guid id)
        {
            DynamicModuleManager objDynamicModuleManager = DynamicModuleManager.GetManager(providerName);
            Type objDynamicType = TypeResolutionService.ResolveType(typeName);
            DynamicContent objRetVal = null;

            objRetVal = (DynamicContent)objDynamicModuleManager.GetItemOrDefault(objDynamicType, id);

            objDynamicType = null;
            objDynamicModuleManager = null;

            return objRetVal;
        }
        public static DynamicContent GetDynamicContentItemByCustomFieldID(String providerName, String typeName, String customField, String idValue)
        {
            DynamicModuleManager objDynamicModuleManager = DynamicModuleManager.GetManager(providerName);
            Type objDynamicType = TypeResolutionService.ResolveType(typeName);
            DynamicContent objRetVal = null;

            objRetVal = objDynamicModuleManager.GetDataItems(objDynamicType).ToList().Where(obj => obj.GetValue(customField) != null && obj.GetValue(customField).ToString() == idValue && obj.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Master).FirstOrDefault();

            objDynamicType = null;
            objDynamicModuleManager = null;

            return objRetVal;
        }

        public static APIActionResult SaveDynamicContentItem(String providerName, String typeName, Guid id, Dictionary<String, SFCustomFieldValue> fieldValueCollection)
        {
            APIActionResult objRetVal = null;
            DynamicModuleManager objDynamicModuleManager = DynamicModuleManager.GetManager(providerName);
            Type objDynamicType = TypeResolutionService.ResolveType(typeName);

            objRetVal = SaveDynamicContentItem(ref objDynamicModuleManager, ref objDynamicType, id, fieldValueCollection);

            objDynamicModuleManager = null;
            objDynamicType = null;

            return objRetVal;
        }
        public static APIActionResult SaveDynamicContentItem(ref DynamicModuleManager dynamicModuleManager, ref Type dynamicType, Guid id, Dictionary<String, SFCustomFieldValue> fieldValueCollection)
        {
            APIActionResult objRetVal = new APIActionResult(Guid.Empty, APIActionResult.APIActions.NoAction);
            DynamicContent objDynamicContent = null;
            DynamicContent objCheckedOutItem = null;

            objDynamicContent = (DynamicContent)dynamicModuleManager.GetItemOrDefault(dynamicType, id);
            if (objDynamicContent == null)
            {
                objDynamicContent = dynamicModuleManager.CreateDataItem(dynamicType);
                objRetVal.APIAction = APIActionResult.APIActions.Created;
            }
            else
            {
                objRetVal.APIAction = APIActionResult.APIActions.Updated;
                objDynamicContent = dynamicModuleManager.Lifecycle.GetMaster(objDynamicContent) as DynamicContent;
            }

            objCheckedOutItem = dynamicModuleManager.Lifecycle.CheckOut(objDynamicContent) as DynamicContent;

            if (fieldValueCollection != null && fieldValueCollection.Keys.Count > 0)
            {
                foreach (String key in fieldValueCollection.Keys)
                {
                    if (objCheckedOutItem.DoesFieldExist(key))
                    {
                        if (fieldValueCollection[key] != null)
                        {
                            switch (fieldValueCollection[key].FieldType)
                            {
                                case SFCustomFieldValue.FieldTypes.Taxonomy:
                                    Guid[] arrTaxonIDs = (Guid[])fieldValueCollection[key].FieldValue;
                                    if (fieldValueCollection[key].IsMutuallyExclusive)
                                    {
                                        objCheckedOutItem.Organizer.Clear(key);
                                    }
                                    if (arrTaxonIDs != null && arrTaxonIDs.Length > 0)
                                    {
                                        List<Guid> arrTaxonIDsToRemove = new List<Guid>();
                                        foreach (Guid objTaxonID in arrTaxonIDs)
                                        {
                                            if (objCheckedOutItem.Organizer.TaxonExists(key, objTaxonID))
                                            {
                                                arrTaxonIDsToRemove.Add(objTaxonID);
                                            }
                                        }
                                        if (arrTaxonIDsToRemove.Count > 0)
                                        {
                                            objCheckedOutItem.Organizer.RemoveTaxa(key, arrTaxonIDsToRemove.ToArray());
                                        }
                                        objCheckedOutItem.Organizer.AddTaxa(key, arrTaxonIDs);

                                        arrTaxonIDsToRemove = null;
                                    }
                                    arrTaxonIDs = null;
                                    break;
                                default:
                                    objCheckedOutItem.SetValue(key, fieldValueCollection[key].FieldValue);
                                    break;
                            }
                        }
                    }
                }
            }

            if (objRetVal.APIAction == APIActionResult.APIActions.Created)
            {
                //Generate Unique URL Name
                objCheckedOutItem.UrlName = Guid.NewGuid().ToString();
            }
            objDynamicContent = dynamicModuleManager.Lifecycle.CheckIn(objCheckedOutItem) as DynamicContent;
            objDynamicContent.SetWorkflowStatus(dynamicModuleManager.Provider.ApplicationName, "Published");
            dynamicModuleManager.Lifecycle.Publish(objDynamicContent);
            dynamicModuleManager.SaveChanges();

            objRetVal.Id = objDynamicContent.Id;
            objRetVal.DataObject = objDynamicContent;

            return objRetVal;
        }
        public static APIActionResult DeleteDynamicContentItem(String providerName, String typeName, Guid id)
        {
            APIActionResult objRetVal = null;
            DynamicModuleManager objDynamicModuleManager = DynamicModuleManager.GetManager(providerName);
            Type objDynamicType = TypeResolutionService.ResolveType(typeName);

            objRetVal = DeleteDynamicContentItem(ref objDynamicModuleManager, ref objDynamicType, id);

            objDynamicType = null;
            objDynamicModuleManager = null;

            return objRetVal;
        }
        public static APIActionResult DeleteDynamicContentItem(ref DynamicModuleManager dynamicModuleManager, ref Type dynamicType, Guid id)
        {
            APIActionResult objRetVal = new APIActionResult(Guid.Empty, APIActionResult.APIActions.NoAction);
            DynamicContent objDynamicContent = null;

            objDynamicContent = (DynamicContent)dynamicModuleManager.GetItemOrDefault(dynamicType, id);
            if (objDynamicContent != null)
            {
                dynamicModuleManager.DeleteDataItem(dynamicType, objDynamicContent.Id);
                if (String.IsNullOrEmpty(dynamicModuleManager.TransactionName))
                {
                    dynamicModuleManager.SaveChanges();
                }
                objRetVal.APIAction = APIActionResult.APIActions.Deleted;
            }

            objDynamicContent = null;

            return objRetVal;
        }
        #endregion

        #region Taxonomy Helpers...
        public static List<HierarchicalTaxonomy> GetAllHierarchicalTaxonomies()
        {
            TaxonomyManager objTaxonomyManager = TaxonomyManager.GetManager();
            List<HierarchicalTaxonomy> arrRetVal = null;

            arrRetVal = objTaxonomyManager.GetTaxonomies<HierarchicalTaxonomy>().ToList();

            objTaxonomyManager = null;

            return arrRetVal;
        }
        public static List<Dictionary<String, Object>> GetHierarchicalTaxonomyTree()
        {
            List<Dictionary<String, Object>> retVal = new List<Dictionary<String, Object>>();
            List<HierarchicalTaxonomy> arrSrcTaxons = GetAllHierarchicalTaxonomies();

            if(arrSrcTaxons != null)
            {
                foreach (HierarchicalTaxonomy objSrcTaxon in arrSrcTaxons)
                {
                    Dictionary<String, Object> arrDestTaxonomy = new Dictionary<String, Object>();
                    List<Dictionary<String, Object>> arrDestTaxa = new List<Dictionary<String, Object>>();
                    List<Taxon> arrSubTaxons = objSrcTaxon.Taxa.Where(obj => obj.ParentId != Guid.Empty).ToList();

                    TypeConverters.ConvertTaxonomy(objSrcTaxon, ref arrDestTaxonomy);

                    foreach (Taxon objRootTaxon in objSrcTaxon.Taxa.Where(obj => obj.ParentId == Guid.Empty))
                    {
                        Dictionary<String, Object> objDestTaxon = new Dictionary<String, Object>();

                        TypeConverters.ConvertTaxon(objRootTaxon, ref objDestTaxon);
                        objDestTaxon.Add("Taxa", GetTreeSubTaxons(objRootTaxon, ref arrSubTaxons));
                        arrDestTaxa.Add(objDestTaxon);
                    }
                    arrDestTaxonomy.Add("Taxa", arrDestTaxa);

                    arrSubTaxons = null;

                    retVal.Add(arrDestTaxonomy);
                }
            }

            arrSrcTaxons = null;

            return retVal;
        }
        public static Dictionary<String, Object> GetHierarchicalTaxonomyTree(Guid taxonomyId)
        {
            Dictionary<String, Object> retVal = new Dictionary<String, Object>();
            HierarchicalTaxonomy objSrcTaxonomy = GetHierarchicalTaxonomy(taxonomyId);

            if (objSrcTaxonomy != null)
            {
                List<Dictionary<String, Object>> arrDestTaxa = new List<Dictionary<String, Object>>();
                List<Taxon> arrSubTaxons = objSrcTaxonomy.Taxa.Where(obj => obj.ParentId != Guid.Empty).ToList();

                TypeConverters.ConvertTaxonomy(objSrcTaxonomy, ref retVal);

                foreach (Taxon objRootTaxon in objSrcTaxonomy.Taxa.Where(obj => obj.ParentId == Guid.Empty))
                {
                    Dictionary<String, Object> objDestTaxon = new Dictionary<String, Object>();

                    TypeConverters.ConvertTaxon(objRootTaxon, ref objDestTaxon);
                    objDestTaxon.Add("Taxa", GetTreeSubTaxons(objRootTaxon, ref arrSubTaxons));
                    arrDestTaxa.Add(objDestTaxon);
                }
                retVal.Add("Taxa", arrDestTaxa);

                arrSubTaxons = null;
            }

            objSrcTaxonomy = null;

            return retVal;
        }
        private static List<Dictionary<String, Object>> GetTreeSubTaxons(Taxon taxon, ref List<Taxon> subTaxons)
        {
            List<Dictionary<String, Object>> arrRetVal = new List<Dictionary<String, Object>>();

            foreach (Taxon objSrcTaxon in subTaxons.Where(obj => obj.ParentId == taxon.Id))
            {
                Dictionary<String, Object> objDestTaxon = new Dictionary<String, Object>();
                TypeConverters.ConvertTaxon(objSrcTaxon, ref objDestTaxon);

                objDestTaxon.Add("Taxa", GetTreeSubTaxons(objSrcTaxon, ref subTaxons));

                arrRetVal.Add(objDestTaxon);
            }

            return arrRetVal;
        }

        public static HierarchicalTaxonomy GetHierarchicalTaxonomy(Guid id)
        {
            TaxonomyManager objTaxonomyManager = TaxonomyManager.GetManager();
            HierarchicalTaxonomy objRetVal = null;

            objRetVal = objTaxonomyManager.GetTaxonomy<HierarchicalTaxonomy>(id);

            objTaxonomyManager = null;

            return objRetVal;
        }
        public static HierarchicalTaxonomy GetHierarchicalTaxonomy(String taxonomyName)
        {
            TaxonomyManager objTaxonomyManager = TaxonomyManager.GetManager();
            HierarchicalTaxonomy objRetVal = null;

            objRetVal = objTaxonomyManager.GetTaxonomies<HierarchicalTaxonomy>().SingleOrDefault(t => t != null && t.Name.ToLower() == taxonomyName.ToLower());

            objTaxonomyManager = null;

            return objRetVal;
        }

        public enum TaxonTypes
        {
            HierarchicalTaxon = 1,
            FlatTaxon = 2
        }
        public static APIActionResult SaveTaxonomy(Guid id, String taxonomyName, TaxonTypes taxonomyType)
        {
            APIActionResult objRetVal = new APIActionResult(Guid.Empty, APIActionResult.APIActions.NoAction);
            TaxonomyManager objTaxonomyManager = TaxonomyManager.GetManager();
            Taxonomy objTaxonomy = null;
            String spTaxonomyUrlName = TypeConverters.ConvertUrlSafe(taxonomyName);

            using (ElevatedModeRegion elevatedModeRegion = new ElevatedModeRegion(objTaxonomyManager))
            {
                if (id != Guid.Empty)
                {
                    objTaxonomy = objTaxonomyManager.GetTaxonomy<Taxonomy>(id);
                }
                if (objTaxonomy == null)
                {
                    if (taxonomyType == TaxonTypes.HierarchicalTaxon)
                    {
                        objTaxonomy = objTaxonomyManager.CreateTaxonomy<HierarchicalTaxonomy>();
                    }
                    else
                    {
                        objTaxonomy = objTaxonomyManager.CreateTaxonomy<FlatTaxonomy>();
                    }
                    objRetVal.APIAction = APIActionResult.APIActions.Created;
                }
                else
                {
                    objRetVal.APIAction = APIActionResult.APIActions.Updated;
                }
                objTaxonomy.Title = taxonomyName;
                objTaxonomy.Name = spTaxonomyUrlName.Replace("-", "").Replace(" ", "");

                objTaxonomyManager.SaveChanges();
            }

            objRetVal.Id = objTaxonomy.Id;
            objRetVal.DataObject = objTaxonomy;

            objTaxonomy = null;
            objTaxonomyManager = null;

            return objRetVal;
        }
        public static APIActionResult SaveTaxon(Guid id, Guid parentTaxonomyId, Guid? parentTaxonId, String taxonName, TaxonTypes taxonType)
        {
            APIActionResult objRetVal = new APIActionResult(Guid.Empty, APIActionResult.APIActions.NoAction);
            TaxonomyManager objTaxonomyManager = TaxonomyManager.GetManager();
            Taxonomy objTaxonomy = objTaxonomyManager.GetTaxonomy<Taxonomy>(parentTaxonomyId);
            String spTaxonUrlName = TypeConverters.ConvertUrlSafe(taxonName);

            if (objTaxonomy != null)
            {
                Taxon objTaxon = null;

                using (ElevatedModeRegion elevatedModeRegion = new ElevatedModeRegion(objTaxonomyManager))
                {
                    if (id != Guid.Empty)
                    {
                        objTaxon = objTaxonomyManager.GetTaxon<Taxon>(id);
                    }
                    if (objTaxon == null)
                    {
                        if (taxonType == TaxonTypes.HierarchicalTaxon)
                        {
                            objTaxon = objTaxonomyManager.CreateTaxon<HierarchicalTaxon>();
                        }
                        else
                        {
                            objTaxon = objTaxonomyManager.CreateTaxon<FlatTaxon>();
                        }

                        if (parentTaxonId != null && parentTaxonId.HasValue && objTaxon is HierarchicalTaxon)
                        {
                            objTaxon.ParentId = parentTaxonId.Value;
                        }
                        objTaxon.Taxonomy = objTaxonomy;
                        objTaxon.Title = taxonName;
                        objTaxon.UrlName = spTaxonUrlName;
                        objTaxon.Name = spTaxonUrlName.Replace("-", "").Replace(" ", "");
                        objTaxonomy.Taxa.Add(objTaxon);

                        objRetVal.APIAction = APIActionResult.APIActions.Created;
                    }
                    else
                    {
                        if (parentTaxonId != null && parentTaxonId.HasValue && objTaxon is HierarchicalTaxon)
                        {
                            objTaxon.ParentId = parentTaxonId.Value;
                        }
                        objTaxon.Name = spTaxonUrlName.Replace("-", "").Replace(" ", "");
                        objTaxon.Title = taxonName;
                        objTaxon.UrlName = spTaxonUrlName;

                        objRetVal.APIAction = APIActionResult.APIActions.Updated;
                    }
                    objTaxonomyManager.SaveChanges();
                }

                objRetVal.Id = objTaxon.Id;
                objRetVal.DataObject = objTaxon;
            }

            objTaxonomy = null;
            objTaxonomyManager = null;

            return objRetVal;
        }

        public static APIActionResult DeleteTaxon(Guid id)
        {
            APIActionResult objRetVal = new APIActionResult(Guid.Empty, APIActionResult.APIActions.NoAction);
            TaxonomyManager objTaxonomyManager = TaxonomyManager.GetManager();
            Taxon objTaxon = objTaxonomyManager.GetTaxon<Taxon>(id);

            if(objTaxon != null)
            {
                using (ElevatedModeRegion elevatedModeRegion = new ElevatedModeRegion(objTaxonomyManager))
                {
                    objTaxonomyManager.Delete(objTaxon);
                    objTaxonomyManager.SaveChanges();
                    objRetVal.APIAction = APIActionResult.APIActions.Deleted;
                }
            }

            objTaxon = null;
            objTaxonomyManager = null;

            return objRetVal;
        }
        public static APIActionResult DeleteTaxons(List<Guid> ids)
        {
            APIActionResult objRetVal = new APIActionResult(Guid.Empty, APIActionResult.APIActions.NoAction);
            TaxonomyManager objTaxonomyManager = TaxonomyManager.GetManager();

            if (ids != null)
            {
                using (ElevatedModeRegion elevatedModeRegion = new ElevatedModeRegion(objTaxonomyManager))
                {
                    foreach (Guid taxonId in ids)
                    {
                        Taxon objTaxon = objTaxonomyManager.GetTaxon<Taxon>(taxonId);
                        if (objTaxon != null)
                        {
                            objTaxonomyManager.Delete(objTaxon);
                        }
                        objTaxon = null;
                    }
                    objTaxonomyManager.SaveChanges();
                }
                objRetVal.APIAction = APIActionResult.APIActions.Deleted;
            }

            objTaxonomyManager = null;

            return objRetVal;
        }
        public static APIActionResult DeleteChildTaxons(Guid taxonomyId)
        {
            APIActionResult objRetVal = new APIActionResult(Guid.Empty, APIActionResult.APIActions.NoAction);
            TaxonomyManager objTaxonomyManager = TaxonomyManager.GetManager();
            Taxonomy objTaxonomy = objTaxonomyManager.GetTaxonomy<Taxonomy>(taxonomyId);

            if (objTaxonomy != null)
            {
                List<Taxon> arrSubTaxons = objTaxonomy.Taxa.ToList();

                if(arrSubTaxons != null && arrSubTaxons.Count > 0)
                {
                    using (ElevatedModeRegion elevatedModeRegion = new ElevatedModeRegion(objTaxonomyManager))
                    {
                        foreach (Taxon objTaxon in arrSubTaxons)
                        {
                            objTaxonomyManager.Delete(objTaxon);
                        }
                        objTaxonomyManager.SaveChanges();
                        objRetVal.APIAction = APIActionResult.APIActions.Deleted;
                    }
                }

                arrSubTaxons = null;
            }

            objTaxonomy = null;
            objTaxonomyManager = null;

            return objRetVal;
        }
        #endregion

        #region Document Helpers...
        public static List<DocumentLibrary> GetLibraries()
        {
            List <DocumentLibrary> retVal = null;
            LibrariesManager objLibManager = LibrariesManager.GetManager();

            retVal = objLibManager.GetDocumentLibraries().ToList();

            objLibManager = null;

            return retVal;
        }
        public static DocumentLibrary GetLibrary(Guid id)
        {
            DocumentLibrary retVal = null;
            LibrariesManager objLibManager = LibrariesManager.GetManager();

            retVal = objLibManager.GetDocumentLibrary(id);

            objLibManager = null;

            return retVal;
        }
        public static APIActionResult SaveLibrary(Guid id, String title, Dictionary<String,Object> fieldValues)
        {
            APIActionResult objRetVal = new APIActionResult(Guid.Empty, APIActionResult.APIActions.NoAction);
            DocumentLibrary objLibrary = null;
            LibrariesManager objLibManager = LibrariesManager.GetManager();

            using (ElevatedModeRegion elevatedModeRegion = new ElevatedModeRegion(objLibManager))
            {
                if (id != Guid.Empty)
                {
                    objLibrary = objLibManager.GetDocumentLibraries().Where(obj => obj.Id == id).FirstOrDefault();
                }
                if (objLibrary == null)
                {
                    if (id != Guid.Empty)
                    {
                        objLibrary = objLibManager.CreateDocumentLibrary(id);
                    }
                    else
                    {
                        objLibrary = objLibManager.CreateDocumentLibrary();
                    }
                    objLibrary.DateCreated = DateTime.UtcNow;
                    objRetVal.APIAction = APIActionResult.APIActions.Created;
                }
                else
                {
                    objRetVal.APIAction = APIActionResult.APIActions.Updated;
                }
                objLibrary.UrlName = TypeConverters.ConvertUrlSafe(title);
                objLibrary.Title = title;
                objLibrary.LastModified = DateTime.UtcNow;

                if (fieldValues != null && fieldValues.Count > 0)
                {
                    foreach (String key in fieldValues.Keys)
                    {
                        if (objLibrary.DoesFieldExist(key))
                        {
                            objLibrary.SetValue(key, fieldValues[key]);
                        }
                    }
                }

                objLibManager.RecompileAndValidateUrls(objLibrary);
                objLibManager.SaveChanges();
            }

            objRetVal.Id = objLibrary.Id;
            
            objLibrary = null;
            objLibManager = null;

            return objRetVal;
        }
        public static APIActionResult DeleteLibrary(Guid id)
        {
            APIActionResult objRetVal = new APIActionResult(Guid.Empty, APIActionResult.APIActions.NoAction);
            DocumentLibrary objLibrary = null;
            LibrariesManager objLibManager = LibrariesManager.GetManager();

            objLibrary = objLibManager.GetDocumentLibrary(id);
            if(objLibrary != null)
            {
                using (ElevatedModeRegion elevatedModeRegion = new ElevatedModeRegion(objLibManager))
                {
                    objLibManager.DeleteLibrary(objLibrary);
                    objLibManager.SaveChanges();
                    objRetVal.APIAction = APIActionResult.APIActions.Deleted;
                }
            }

            objLibrary = null;
            objLibManager = null;

            return objRetVal;
        }

        public static List<IFolder> GetFolders(Guid rootFolderId)
        {
            List<IFolder> retVal = null;
            IFolder objRootFolder = GetFolder(rootFolderId);
            LibrariesManager objLibManager = LibrariesManager.GetManager();

            if(objRootFolder != null)
            {
                retVal = objLibManager.GetAllFolders(objRootFolder).ToList();
            }

            objLibManager = null;

            return retVal;
        }
        public static IFolder GetFolder(Guid id)
        {
            IFolder retVal = null;
            LibrariesManager objLibManager = LibrariesManager.GetManager();

            retVal = objLibManager.GetFolder(id);

            objLibManager = null;

            return retVal;
        }
        public static APIActionResult SaveFolder(Guid id, Guid libraryId, Guid parentFolderId, String title, String description)
        {
            APIActionResult objRetVal = new APIActionResult(Guid.Empty, APIActionResult.APIActions.NoAction);
            LibrariesManager objLibManager = LibrariesManager.GetManager();
            Library objLibrary = null;
            IFolder objParentFolder = null;
            IFolder objFolder = null;

            using (ElevatedModeRegion elevatedModeRegion = new ElevatedModeRegion(objLibManager))
            {
                if (libraryId != Guid.Empty)
                {
                    objLibrary = objLibManager.GetDocumentLibrary(libraryId);
                }
                if (parentFolderId != Guid.Empty)
                {
                    objParentFolder = objLibManager.GetFolder(parentFolderId);
                }
                if (id != Guid.Empty)
                {
                    objFolder = objLibManager.GetFolder(id);
                }

                if (objFolder == null)
                {
                    //Create
                    if (objParentFolder != null)
                    {
                        objFolder = objLibManager.CreateFolder(objParentFolder);
                    }
                    else if (objLibrary != null)
                    {
                        objFolder = objLibManager.CreateFolder(objLibrary);
                    }
                    objRetVal.APIAction = APIActionResult.APIActions.Created;
                }
                else
                {
                    //Update
                    objRetVal.APIAction = APIActionResult.APIActions.Updated;
                }
                objFolder.Title = title;
                objFolder.Description = description;
                objFolder.UrlName = TypeConverters.ConvertUrlSafe(title);

                objLibManager.SaveChanges();
            }

            objRetVal.Id = objFolder.Id;

            objFolder = null;
            objParentFolder = null;
            objLibrary = null;
            objLibManager = null;

            return objRetVal;
        }
        public static APIActionResult DeleteFolder(Guid id)
        {
            APIActionResult objRetVal = new APIActionResult(Guid.Empty, APIActionResult.APIActions.NoAction);
            LibrariesManager objLibManager = LibrariesManager.GetManager();
            IFolder objFolder = null;

            objFolder = objLibManager.GetFolder(id);
            if(objFolder != null)
            {
                using (ElevatedModeRegion elevatedModeRegion = new ElevatedModeRegion(objLibManager))
                {
                    objLibManager.Delete(objFolder);
                    objLibManager.SaveChanges();
                    objRetVal.APIAction = APIActionResult.APIActions.Deleted;
                }
            }

            objFolder = null;
            objLibManager = null;

            return objRetVal;
        }
        public static APIActionResult DeleteFolders(List<Guid> ids)
        {
            APIActionResult objRetVal = new APIActionResult(Guid.Empty, APIActionResult.APIActions.NoAction);
            LibrariesManager objLibManager = LibrariesManager.GetManager();

            if (ids != null)
            {
                using (ElevatedModeRegion elevatedModeRegion = new ElevatedModeRegion(objLibManager))
                {
                    foreach (Guid folderId in ids)
                    {
                        IFolder objFolder = objLibManager.GetFolder(folderId);
                        if (objFolder != null)
                        {
                            try
                            {
                                objLibManager.Delete(objFolder);
                            }
                            catch (Exception ex) { }
                        }
                        objFolder = null;
                    }
                    objLibManager.SaveChanges();
                }
                objRetVal.APIAction = APIActionResult.APIActions.Deleted;
            }

            objLibManager = null;

            return objRetVal;
        }

        public static List<Document> GetDocuments(Guid libraryId)
        {
            List<Document> retVal = null;
            LibrariesManager objLibManager = LibrariesManager.GetManager();
            DocumentLibrary objLib = null;
            
            objLib = objLibManager.GetDocumentLibrary(libraryId);
            if (objLib != null)
            {
                retVal = objLib.Documents().Where(obj => obj.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Master).ToList();
            }

            objLibManager = null;

            return retVal;
        }
        public static List<Document> GetDocuments(List<Guid> ids)
        {
            List<Document> retVal = null;
            LibrariesManager objLibManager = LibrariesManager.GetManager();

            retVal = objLibManager.GetDocuments().Where(obj => ids.Contains(obj.Id) && obj.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Master).ToList();
            
            objLibManager = null;

            return retVal;
        }
        public static List<Document> GetDocumentsByFolder(Guid folderId)
        {
            List<Document> retVal = null;
            LibrariesManager objLibManager = LibrariesManager.GetManager();
            IFolder objFolder = null;

            objFolder = objLibManager.GetFolder(folderId);
            if (objFolder != null)
            {
                retVal = objLibManager.GetDocuments().Where(obj => obj.FolderId == objFolder.Id && obj.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Master).ToList();
            }

            objFolder = null;
            objLibManager = null;

            return retVal;
        }
        public static Document GetDocument(Guid id)
        {
            Document retVal = null;
            LibrariesManager objLibManager = LibrariesManager.GetManager();

            retVal = objLibManager.GetDocument(id);

            return retVal;
        }
        public static Byte[] GetDocumentBinary(Guid id)
        {
            Byte[] retVal = null;
            LibrariesManager objLibManager = LibrariesManager.GetManager();
            Document objDoc = null;

            objDoc = objLibManager.GetDocument(id);

            if (objDoc != null)
            {
                using (Stream objStream = objLibManager.Download(objDoc))
                {
                    retVal = new Byte[objStream.Length];
                    objStream.Read(retVal, 0, retVal.Length);
                }
            }
            return retVal;
        }
        public static Stream GetDocumentStream(Guid id)
        {
            Stream retVal = null;
            LibrariesManager objLibManager = LibrariesManager.GetManager();
            Document objDoc = null;

            objDoc = objLibManager.GetDocument(id);

            if (objDoc != null)
            {
                retVal = objLibManager.Download(objDoc);
            }
            return retVal;
        }
        public static APIActionResult SaveDocument(Guid id, Guid libraryId, Guid folderId, String title, String fileName, DateTime dateCreated, DateTime lastModified, Dictionary<String, Object> fieldValues, Byte[] fileData)
        {
            APIActionResult objRetVal = new APIActionResult(Guid.Empty, APIActionResult.APIActions.NoAction);
            Document objDocument = null;
            Document objDocTemp = null;
            DocumentLibrary objLibrary = null;
            IFolder objFolder = null;
            DateTime? publicationDate = null;
            LibrariesManager objLibManager = LibrariesManager.GetManager();

            using (ElevatedModeRegion elevatedModeRegion = new ElevatedModeRegion(objLibManager))
            {
                if (libraryId != Guid.Empty)
                {
                    objLibrary = objLibManager.GetDocumentLibrary(libraryId);
                }
                if (folderId != Guid.Empty)
                {
                    objFolder = objLibManager.GetFolder(folderId);
                }
                if (id != Guid.Empty)
                {
                    objDocument = objLibManager.GetDocuments().Where(obj => obj.Id == id && obj.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Master).FirstOrDefault();
                }
                if (objDocument == null)
                {
                    objDocument = objLibManager.CreateDocument();
                    objRetVal.APIAction = APIActionResult.APIActions.Created;
                }
                else
                {
                    objRetVal.APIAction = APIActionResult.APIActions.Updated;
                }

                objDocTemp = objLibManager.Lifecycle.CheckOut(objDocument) as Document;
                if(objRetVal.APIAction == APIActionResult.APIActions.Created)
                {
                    objDocTemp.DateCreated = (dateCreated != DateTime.MinValue ? dateCreated : DateTime.UtcNow);
                    objDocTemp.UrlName = TypeConverters.ConvertUrlSafe(title);
                }

                objDocTemp.LastModified = (lastModified != DateTime.MinValue ? lastModified : DateTime.UtcNow);
                publicationDate = objDocTemp.LastModified;
                objDocTemp.Title = title;
                objDocTemp.MediaFileUrlName = TypeConverters.ConvertUrlSafe(fileName);

                if (objLibrary != null)
                {
                    objDocTemp.Library = objLibrary;
                }
                if (objFolder != null)
                {
                    objDocTemp.FolderId = objFolder.Id;
                }

                if (fieldValues != null && fieldValues.Count > 0)
                {
                    List<MetaField> arrCustomFields = GetDocumentCustomFieldsRaw();

                    foreach (String key in fieldValues.Keys)
                    {
                        if(key.ToLower() == "publicationdate")
                        {
                            //Mapping overrides default / last modified...
                            publicationDate = (DateTime)fieldValues[key];
                        }

                        if (objDocTemp.DoesFieldExist(key))
                        {
                            if(fieldValues[key] != null)
                            {
                                MetaField objFld = arrCustomFields.Where(obj => obj.FieldName.ToLower() == key.ToLower()).FirstOrDefault();
                                if (objFld != null)
                                {
                                    if(objFld.TaxonomyId != null && objFld.TaxonomyId != Guid.Empty)
                                    {
                                        //Is Taxonomy...
                                        List<String> arrTaxonIDs = null;
                                        List<Guid> arrTaxonGuids = null;

                                        if (fieldValues[key] is JArray)
                                        {
                                            arrTaxonIDs = ((JArray)fieldValues[key]).ToObject<List<String>>();
                                        }
                                        else
                                        {
                                            arrTaxonIDs = (List<String>)fieldValues[key];
                                        }
                                        if(arrTaxonIDs != null && arrTaxonIDs.Count > 0)
                                        {
                                            arrTaxonGuids = new List<Guid>();
                                            foreach (String taxonId in arrTaxonIDs)
                                            {
                                                arrTaxonGuids.Add(GeneralHelpers.parseGUID(taxonId));
                                            }
                                        }
                                        objDocTemp.Organizer.Clear(key);
                                        if (arrTaxonGuids != null && arrTaxonGuids.Count > 0)
                                        {
                                            List<Guid> arrTaxonIDsToRemove = new List<Guid>();
                                            foreach (Guid objTaxonGuid in arrTaxonGuids)
                                            {
                                                if (objDocTemp.Organizer.TaxonExists(key, objTaxonGuid))
                                                {
                                                    arrTaxonIDsToRemove.Add(objTaxonGuid);
                                                }
                                            }
                                            if (arrTaxonIDsToRemove.Count > 0)
                                            {
                                                objDocTemp.Organizer.RemoveTaxa(key, arrTaxonIDsToRemove.ToArray());
                                            }
                                            objDocTemp.Organizer.AddTaxa(key, arrTaxonGuids.ToArray());

                                            arrTaxonIDsToRemove = null;
                                        }

                                        arrTaxonIDs = null;
                                    }
                                    else
                                    {
                                        if (objFld.ClrType.IndexOf("Telerik.Sitefinity.Model.Lstring") > -1)
                                        {
                                            objDocTemp.SetString(key, new Lstring(fieldValues[key].ToString()));
                                        }
                                        else
                                        {
                                            //Some other field type...
                                            objDocTemp.SetValue(key, fieldValues[key]);
                                        }
                                    }
                                }
                                else
                                {
                                    if(fieldValues[key] is String)
                                    {
                                        objDocTemp.SetString(key, new Lstring(fieldValues[key].ToString()));
                                    }
                                    else
                                    {
                                        objDocTemp.SetValue(key, fieldValues[key]);
                                    }
                                }
                                objFld = null;
                            }
                        }
                    }
                    arrCustomFields = null;
                }
                if(String.IsNullOrEmpty(objDocTemp.Title))
                {
                    //Title can't be null if overwritten by dynamically mapped props above...
                    objDocTemp.Title = title;
                }
                if (fileData != null)
                {
                    MemoryStream objStream = new MemoryStream(fileData);
                    objLibManager.Upload(objDocTemp, objStream, Path.GetExtension(fileName));
                    objStream = null;
                }

                objLibManager.RecompileAndValidateUrls(objDocTemp);
                objDocument = objLibManager.Lifecycle.CheckIn(objDocTemp) as Document;

                if(publicationDate != null && publicationDate.HasValue && publicationDate.Value != DateTime.MinValue)
                {
                    //SF Seems to convert this date, so our database hack below fixes this...
                    objLibManager.Lifecycle.PublishWithSpecificDate(objDocument, publicationDate.Value);
                }
                else
                {
                    objLibManager.Lifecycle.Publish(objDocument);
                }
                objDocument.SetWorkflowStatus(objLibManager.Provider.ApplicationName, "Published");

                objLibManager.SaveChanges();

                //Database Hacks to be able override DateCreated / LastModified / PublicationDate Values...
                if(dateCreated != DateTime.MinValue || lastModified != DateTime.MinValue)
                {
                    DatabaseIO objSFDB = new DatabaseIO(true);
                    List<Tuple<String, Object>> arrParams = new List<Tuple<String, Object>>();
                    if (dateCreated != DateTime.MinValue)
                    {
                        arrParams.Add(Tuple.Create<String, Object>("id", objDocument.Id));
                        arrParams.Add(Tuple.Create<String, Object>("date_created", dateCreated));
                        objSFDB.UpdateSQL("UPDATE sf_media_content SET date_created=@date_created WHERE content_id = @id OR original_content_id = @id;", arrParams.ToArray());
                    }
                    if (lastModified != DateTime.MinValue)
                    {
                        arrParams.Clear();
                        arrParams.Add(Tuple.Create<String, Object>("id", objDocument.Id));
                        arrParams.Add(Tuple.Create<String, Object>("last_modified", lastModified));
                        objSFDB.UpdateSQL("UPDATE sf_media_content SET last_modified=@last_modified, publication_date=@last_modified WHERE content_id = @id OR original_content_id = @id;", arrParams.ToArray());
                    }
                    arrParams = null;
                    objSFDB = null;
                }
            }

            objRetVal.Id = objDocument.Id;

            objFolder = null;
            objLibrary = null;
            objDocTemp = null;
            objDocument = null;
            objLibManager = null;

            return objRetVal;
        }
        public static APIActionResult DeleteDocument(Guid id)
        {
            APIActionResult objRetVal = new APIActionResult(Guid.Empty, APIActionResult.APIActions.NoAction);
            Document objDocument = null;
            LibrariesManager objLibManager = LibrariesManager.GetManager();

            if (id != Guid.Empty)
            {
                objDocument = objLibManager.GetDocuments().Where(obj => obj.Id == id && obj.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Master).FirstOrDefault();
            }
            if (objDocument != null)
            {
                using (ElevatedModeRegion elevatedModeRegion = new ElevatedModeRegion(objLibManager))
                {
                    objLibManager.DeleteDocument(objDocument);
                    objLibManager.SaveChanges();
                    objRetVal.APIAction = APIActionResult.APIActions.Deleted;
                }
            }

            objDocument = null;
            objLibManager = null;

            return objRetVal;
        }
        public static APIActionResult DeleteDocuments(List<Guid> ids)
        {
            APIActionResult objRetVal = new APIActionResult(Guid.Empty, APIActionResult.APIActions.NoAction);
            LibrariesManager objLibManager = LibrariesManager.GetManager();

            if (ids != null)
            {
                using (ElevatedModeRegion elevatedModeRegion = new ElevatedModeRegion(objLibManager))
                {
                    foreach (Guid docId in ids)
                    {
                        Document objDocument = objLibManager.GetDocuments().Where(obj => obj.Id == docId && obj.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Master).FirstOrDefault();
                        if (objDocument != null)
                        {
                            try
                            {
                                objLibManager.DeleteDocument(objDocument);
                            }
                            catch (Exception ex) { }
                        }
                        objDocument = null;
                    }
                    objLibManager.SaveChanges();
                }
                objRetVal.APIAction = APIActionResult.APIActions.Deleted;
            }

            objLibManager = null;

            return objRetVal;
        }

        public static List<Dictionary<String, Object>> GetDocumentCustomFields()
        {
            List<Dictionary<String, Object>> arrRetVal = new List<Dictionary<String, Object>>();
            MetadataManager objMetaManager = MetadataManager.GetManager();
            MetaType objType = objMetaManager.GetMetaType(typeof(Document));

            if(objType != null)
            {
                foreach(MetaField objFld in objType.Fields)
                {
                    Dictionary<String, Object> objDst = new Dictionary<String, Object>();
                    TypeConverters.ConvertMetaField(objFld, ref objDst);
                    arrRetVal.Add(objDst);
                }
            }
            objType = null;

            objMetaManager = null;

            return arrRetVal;
        }
        public static List<MetaField> GetDocumentCustomFieldsRaw()
        {
            List<MetaField> arrRetVal = null;
            MetadataManager objMetaManager = MetadataManager.GetManager();
            MetaType objType = objMetaManager.GetMetaType(typeof(Document));

            if (objType != null)
            {
                arrRetVal = objType.Fields.ToList();
            }
            objType = null;

            objMetaManager = null;

            return arrRetVal;
        }
        public static MetaField GetDocumentCustomField(String fieldName)
        {
            MetaField retVal = null;
            MetadataManager objMetaManager = MetadataManager.GetManager();
            MetaType objType = objMetaManager.GetMetaType(typeof(Document));

            if (objType != null)
            {
                retVal = objType.Fields.Where(obj => obj.FieldName.ToLower() == fieldName.ToLower()).FirstOrDefault();
            }
            objType = null;

            objMetaManager = null;

            return retVal;
        }

        public static Dictionary<String, Object> GetLibraryTreeStructure(Guid rootFolderId)
        {
            Dictionary<String, Object> retVal = new Dictionary<String, Object>();
            LibrariesManager objLibManager = LibrariesManager.GetManager();
            IFolder objRootFolder = objLibManager.GetFolder(rootFolderId);

            if (objRootFolder != null)
            {
                TypeConverters.ConvertDocumentLibrary(objRootFolder, ref retVal);
                Dictionary<String, Object> objContents = GetFolderContents(ref objLibManager, objRootFolder, true);
                GeneralHelpers.appendDictionary(ref retVal, ref objContents);
            }

            objRootFolder = null;
            objLibManager = null;

            return retVal;
        }
        public static Dictionary<String, Object> GetFolderContents(ref LibrariesManager objLibManager, IFolder parentFolder, Boolean isRoot)
        {
            Dictionary<String, Object> retVal = new Dictionary<String, Object>();

            List<IFolder> arrSrcFolders = objLibManager.GetChildFolders(parentFolder).ToList();
            List<MediaContent> arrSrcItems = null;

            List<Dictionary<String, Object>> arrDestFolders = null;
            List<Dictionary<String, Object>> arrDestItems = null;

            if(isRoot)
            {
                arrSrcItems = objLibManager.GetChildItems(parentFolder).Where(obj => obj.FolderId == null && obj.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Master).ToList();
            }
            else
            {
                arrSrcItems = objLibManager.GetChildItems(parentFolder).Where(obj => obj.FolderId == parentFolder.Id && obj.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Master).ToList();
            }

            if (arrSrcFolders != null)
            {
                arrDestFolders = new List<Dictionary<String, Object>>();
                foreach (IFolder objSrcFolder in arrSrcFolders)
                {
                    Dictionary<String, Object> objDestFolder = new Dictionary<String, Object>();
                    TypeConverters.ConvertDocumentLibrary(objSrcFolder, ref objDestFolder);
                    
                    //Recurse to next level...
                    Dictionary<String, Object> arrRecursiveCall = GetFolderContents(ref objLibManager, objSrcFolder, false);
                    GeneralHelpers.appendDictionary(ref objDestFolder, ref arrRecursiveCall);

                    arrDestFolders.Add(objDestFolder);
                }
            }
            if (arrSrcItems != null)
            {
                arrDestItems = new List<Dictionary<String, Object>>();
                foreach (MediaContent objSrcItem in arrSrcItems)
                {
                    if (objSrcItem.GetType().ToString().IndexOf("Model.Document") > -1)
                    {
                        Dictionary<String, Object> objDestItem = new Dictionary<String, Object>();
                        TypeConverters.ConvertDocument((Document)objSrcItem, ref objDestItem);
                        arrDestItems.Add(objDestItem);
                    }
                }
            }

            arrSrcItems = null;
            arrSrcFolders = null;

            retVal.Add("Folders", arrDestFolders);
            retVal.Add("Documents", arrDestItems);

            return retVal;
        }

        public static Boolean PurgeDocumentRevisionHistories(Guid libraryId)
        {
            Boolean retVal = false;
            LibrariesManager objLibManager = LibrariesManager.GetManager();
            DocumentLibrary objLib = null;

            objLib = objLibManager.GetDocumentLibrary(libraryId);
            if (objLib != null)
            {
                List<Document> arrDocs = objLib.Documents().Where(obj => obj.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Master).ToList();
                if(arrDocs != null && arrDocs.Count > 0)
                {
                    VersionManager objVerManager = VersionManager.GetManager();
                    foreach(Document objDoc in arrDocs)
                    {
                        List<Change> arrChanges = objVerManager.GetItemVersionHistory(objDoc.Id).Where(x => !x.IsLastPublishedVersion).ToList();
                        foreach(Change objChange in arrChanges)
                        {
                            objVerManager.DeleteChange(objChange.Id);
                        }
                        arrChanges = null;
                        objVerManager.SaveChanges();
                    }
                    objVerManager = null;
                }
                arrDocs = null;
            }
            objLibManager = null;

            return retVal;
        }
        #endregion

        #region Search Index Helpers...
        public static List<String> GetSearchIndexes()
        {
            List<String> retVal = null;
            PublishingManager objPublishingManager = PublishingManager.GetManager(PublishingConfig.SearchProviderName);
            List<PipeSettings> arrPipeSettings = objPublishingManager.GetPipeSettings().Where(obj => obj.PipeName.ToLower() == "searchindex" && obj.IsActive == true && obj.IsInbound == false).ToList();

            if (arrPipeSettings != null && arrPipeSettings.Count() > 0)
            {
                retVal = new List<String>();
                foreach (PipeSettings obj in arrPipeSettings)
                {
                    if (obj != null && obj is SearchIndexPipeSettings)
                    {
                        retVal.Add(((SearchIndexPipeSettings)obj).Title); //CatalogName
                    }
                }
            }

            arrPipeSettings = null;
            objPublishingManager = null;

            return retVal;
        }
        public static Boolean RunSearchIndex(String indexName)
        {
            Boolean blnRetVal = false;
            PublishingManager objPublishingManager = PublishingManager.GetManager(PublishingConfig.SearchProviderName);
            PublishingPoint objPublishingPoint = objPublishingManager.GetPublishingPoints().Where(p => p.Name.ToLower() == indexName.ToLower()).FirstOrDefault();
            PublishingAdminService objPublishingAdminService = new PublishingAdminService();

            if (objPublishingPoint != null)
            {
                try
                {
                    blnRetVal = objPublishingAdminService.ReindexSearchContent(PublishingConfig.SearchProviderName, objPublishingPoint.Id.ToString());
                }
                catch (Exception ex)
                { }
            }

            objPublishingAdminService = null;
            objPublishingPoint = null;
            objPublishingManager = null;

            return blnRetVal;
        }
        #endregion

        #region Other Maintenance...
        public static Boolean PurgeOrhpanedSFChunks()
        {
            Boolean blnRetVal = false;
            DatabaseIO objSFDB = new DatabaseIO(true);
            String strSQL = @"DELETE FROM sf_chunks 
                                WHERE file_id IN(
	                                SELECT ch.file_id 
	                                FROM sf_chunks ch 
	                                WHERE ch.file_id NOT IN(
		                                SELECT mfl.file_id 
		                                FROM sf_media_content mc
		                                JOIN sf_media_file_links mfl 
		                                ON mc.content_id = mfl.content_id
		                                JOIN sf_chunks ch 
		                                ON ch.file_id = mfl.file_id)
	                                AND ch.file_id NOT IN(
		                                SELECT ch.file_id 
		                                FROM sf_media_thumbnails mt
		                                JOIN sf_media_content mc 
		                                ON mc.content_id = mt.content_id
		                                JOIN sf_chunks ch 
		                                ON mt.file_id = ch.file_id)
                                );";
            objSFDB.UpdateSQL(strSQL);

            strSQL = null;
            objSFDB = null;

            return blnRetVal;
        }
        #endregion
    }
}