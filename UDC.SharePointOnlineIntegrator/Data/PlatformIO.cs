using System;
using System.Collections.Generic;
using System.Linq;

using UDC.Common;
using UDC.Common.Data.Models.Configuration;
using Equ.SharePoint.GraphService;

namespace UDC.SharePointOnlineIntegrator.Data
{
    // Wrapper / Type Converter helper for SharePoint Online via Microsoft Graph
    public class PlatformIO
    {
        public String Domain { get; set; }
        public String ServiceUsername { get; set; }
        public String ServicePassword { get; set; }
        public Dictionary<String, String> AdditionalConfigs { get; private set; }

        private IGraphService _graphService;

        public PlatformIO()
        {
            AdditionalConfigs = new Dictionary<String, String>();
            LoadSettings();
            EnsureAdditionalConfigsValid();
        }

        public PlatformIO(String domain, String serviceUsername, String servicePassword, String additionalConfigs)
        {
            this.Domain = domain;
            this.ServiceUsername = serviceUsername;
            this.ServicePassword = servicePassword;
            this.AdditionalConfigs = GeneralHelpers.ParseAdditionalConfigs(additionalConfigs);
            EnsureAdditionalConfigsValid();
        }

        public PlatformIO(PlatformCfg cfg)
        {
            if (cfg != null)
            {
                this.Domain = cfg.ServiceDomain;
                this.ServiceUsername = cfg.ServiceUsername;
                this.ServicePassword = cfg.ServicePassword;
                this.AdditionalConfigs = GeneralHelpers.ParseAdditionalConfigs(cfg.AdditionalConfigs);
                EnsureAdditionalConfigsValid();
            }
            else
            {
                this.AdditionalConfigs = new Dictionary<String, String>();
                LoadSettings();
                EnsureAdditionalConfigsValid();
            }
        }


        private void EnsureAdditionalConfigsValid()
        {
            if (this.AdditionalConfigs == null ||
                !this.AdditionalConfigs.ContainsKey("tenantId") ||
                !this.AdditionalConfigs.ContainsKey("sitePath") ||
                !this.AdditionalConfigs.ContainsKey("driveName"))
            {
                throw new Exception("Additional configs must contain tenantId, sitePath and driveName.");
            }
        }

        private void LoadSettings()
        {
            this.Domain = AppSettings.GetValue("SharePointOnline:EndPointURL");
            this.ServiceUsername = AppSettings.GetValue("SharePointOnline:ServiceUsername");
            this.ServicePassword = AppSettings.GetValue("SharePointOnline:ServicePassword");
            String addCfg = AppSettings.GetValue("SharePointOnline:AdditionalConfigs");
            this.AdditionalConfigs = GeneralHelpers.ParseAdditionalConfigs(addCfg);
        }
        private Boolean ValidateSettings()
        {
            Boolean blnRetVal = true;
            if (String.IsNullOrEmpty(this.Domain))
            {
                blnRetVal = false;
            }
            if (String.IsNullOrEmpty(this.ServiceUsername))
            {
                blnRetVal = false;
            }
            if (String.IsNullOrEmpty(this.ServicePassword))
            {
                blnRetVal = false;
            }
            if (this.AdditionalConfigs == null ||
                !this.AdditionalConfigs.ContainsKey("tenantId") ||
                !this.AdditionalConfigs.ContainsKey("sitePath") ||
                !this.AdditionalConfigs.ContainsKey("driveName"))
            {
                blnRetVal = false;
            }
            return blnRetVal;
        }

        private IGraphService GetGraphService()
        {
            if (this._graphService == null)
            {
                if (!ValidateSettings())
                {
                    throw new Exception("Platform not configured! Please check settings and try again.");
                }

                String tenantId = this.AdditionalConfigs.ContainsKey("tenantId") ? this.AdditionalConfigs["tenantId"] : null;
                String sitePath = this.AdditionalConfigs.ContainsKey("sitePath") ? this.AdditionalConfigs["sitePath"] : null;
                String driveName = this.AdditionalConfigs.ContainsKey("driveName") ? this.AdditionalConfigs["driveName"] : null;

                this._graphService = new GraphService(tenantId, this.ServiceUsername, this.ServicePassword, this.Domain, sitePath, driveName);
            }

            return this._graphService;
        }

        public List<Dictionary<String, Object>> GetLists()
        {
            List<Dictionary<String, Object>> arrRetVal = null;
            IGraphService graphService = GetGraphService();

            var arrSrcLists = AsyncHelper.RunSync(() => graphService.GetListsAsync());
            if (arrSrcLists != null)
            {
                arrRetVal = arrSrcLists.ToList();
            }

            return arrRetVal;
        }

        public Dictionary<String, Object> GetList(Guid listId)
        {
            IGraphService graphService = GetGraphService();
            return AsyncHelper.RunSync(() => graphService.GetListAsync(listId));
        }
        public List<Dictionary<String, Object>> GetDocuments(Guid listId, Boolean includeBinary, List<String> fields)
        {
            List<Dictionary<String, Object>> arrRetVal = null;
            IGraphService graphService = GetGraphService();

            var arrSrcFiles = AsyncHelper.RunSync(() => graphService.GetDocumentsAsync(listId, fields));
            if (arrSrcFiles != null)
            {
                arrRetVal = arrSrcFiles.ToList();
            }

            return arrRetVal;
        }

        public List<Dictionary<String, Object>> GetDocuments(List<String> docIds, Boolean includeBinary, List<String> fields)
        {
            List<Dictionary<String, Object>> arrRetVal = null;
            List<Dictionary<String, Object>> arrSrcFiles = GetDocuments(Guid.Empty, includeBinary, fields);

            if (arrSrcFiles != null)
            {
                arrRetVal = arrSrcFiles.Where(doc =>
                {
                    String id = GeneralHelpers.parseString(doc["Id"]);
                    return docIds == null || docIds.Contains(id);
                }).ToList();
            }

            return arrRetVal;
        }
        public Dictionary<String, Object> GetDocument(String docId, Boolean includeBinary, List<String> fields)
        {
            List<Dictionary<String, Object>> arrDocs = GetDocuments(new List<String>() { docId }, includeBinary, fields);
            if (arrDocs != null && arrDocs.Count > 0)
            {
                return arrDocs[0];
            }
            return null;
        }
        public List<Dictionary<String, Object>> GetTermSets(Boolean includeChildren)
        {
            List<Dictionary<String, Object>> arrRetVal = null;
            IGraphService graphService = GetGraphService();

            var arrSrcTermSets = AsyncHelper.RunSync(() => graphService.GetTermSetsAsync());
            if (arrSrcTermSets != null)
            {
                arrRetVal = arrSrcTermSets.ToList();
            }

            return arrRetVal;
        }

        public Dictionary<String, Object> GetTermSet(Guid id, Boolean includeChildren)
        {
            IGraphService graphService = GetGraphService();
            var arrTermSets = AsyncHelper.RunSync(() => graphService.GetTermSetsAsync());
            Dictionary<String, Object> objRetVal = arrTermSets?.FirstOrDefault(ts => GeneralHelpers.parseGUID(ts["Id"].ToString()) == id);

            if (objRetVal != null && includeChildren)
            {
                var arrTerms = AsyncHelper.RunSync(() => graphService.GetTermsAsync(id));
                objRetVal["Terms"] = arrTerms?.ToList();
            }

            return objRetVal;
        }
    }
}

