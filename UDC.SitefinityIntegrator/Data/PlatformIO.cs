using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using UDC.Common;
using UDC.Common.Data;
using UDC.Common.Data.Models;
using UDC.Common.Data.Models.Configuration;

namespace UDC.SitefinityIntegrator.Data
{
    //Basically an API Client Wrapper to Interface with the API Controller within the SF Context Plugin
    public class PlatformIO
    {
        public String EndPointURL { get; set; }
        public String ServiceUsername { get; set; }
        public String ServicePassword { get; set; }

        public PlatformIO()
        {
            LoadSettings();
        }
        public PlatformIO(String endPointURL, String serviceUsername, String servicePassword, String serviceDomain)
        {
            this.EndPointURL = endPointURL;
            this.ServiceUsername = serviceUsername;
            this.ServicePassword = servicePassword;
        }
        public PlatformIO(PlatformCfg cfg)
        {
            if (cfg != null)
            {
                this.EndPointURL = cfg.EndPointURL;
                this.ServiceUsername = cfg.ServiceUsername;
                this.ServicePassword = cfg.ServicePassword;
            }
            else
            {
                LoadSettings();
            }
        }

        private void LoadSettings()
        {
            this.EndPointURL = AppSettings.GetValue("Sitefinity:EndPointURL");
            this.ServiceUsername = AppSettings.GetValue("Sitefinity:ServiceUsername");
            this.ServicePassword = AppSettings.GetValue("Sitefinity:ServicePassword");
        }
        private Boolean ValidateSettings()
        {
            Boolean blnRetVal = true;
            if (String.IsNullOrEmpty(this.EndPointURL))
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
            return blnRetVal;
        }

        private APIResponse SendApiRequest(String methodName, String parameterStr, Object postParams, WebAPIClient.HttpMethod method)
        {
            APIResponse objAPIResponse = null;

            if (ValidateSettings())
            {
                WebAPIClient objWebAPIClient = null;
                String svcURL = this.EndPointURL + "/udc-api/sfcontext/{0}?{1}";
                Int32 retryCount = 30;
                Int32 retry = 0;

                objWebAPIClient = new WebAPIClient();

                while (retry < retryCount)
                {
                    objAPIResponse = objWebAPIClient.SendRequest(String.Format(svcURL, methodName, parameterStr), this.ServiceUsername, this.ServicePassword, true, postParams, method);
                    retry++;
                    if (objAPIResponse.exitCode == 2)
                    {
                        //SF Initialising or Auth Error... Wait it out / Retry...
                        System.Threading.Thread.Sleep(2000);
                    }
                    else
                    {
                        retry = retryCount;
                    }
                }
                
                objWebAPIClient = null;
            }
            else
            {
                throw new Exception("Platform not configured! Please check settings and try again.");
            }

            return objAPIResponse;
        }
        private APIResponse SendApiRequest(String methodName, String parameterStr)
        {
            return SendApiRequest(methodName, parameterStr, null, WebAPIClient.HttpMethod.GET);
        }
        private APIResponse SendApiPost(String methodName, String parameterStr, Object postParams)
        {
            return SendApiRequest(methodName, parameterStr, postParams, WebAPIClient.HttpMethod.POST);
        }

        #region Taxonomy Methods...
        public APIResponse GetAllHierarchicalTaxonomies()
        {
            return SendApiRequest("GetAllHierarchicalTaxonomies", "");
        }
        public APIResponse GetHierarchicalTaxonomyTree()
        {
            return SendApiRequest("GetHierarchicalTaxonomyTree", "");
        }
        public APIResponse GetHierarchicalTaxonomyTree(Guid taxonomyGuid)
        {
            return SendApiRequest("GetHierarchicalTaxonomyTree", "taxonomyGuid=" + taxonomyGuid.ToString());
        }
        public APIResponse GetHierarchicalTaxonomy(Guid taxonomyGuid)
        {
            return SendApiRequest("GetHierarchicalTaxonomy", "taxonomyGuid=" + taxonomyGuid.ToString());
        }
        public APIResponse DeleteTaxon(Guid taxonGuid)
        {
            return SendApiRequest("DeleteTaxon", "taxonGuid=" + taxonGuid.ToString());
        }
        public APIResponse DeleteTaxons(List<Guid> taxonGuids)
        {
            GetDocumentArgs objArgs = new GetDocumentArgs();
            objArgs.guidList = JsonConvert.SerializeObject(taxonGuids);
            return SendApiPost("DeleteTaxons", "", objArgs);
        }
        public APIResponse DeleteChildTaxons(Guid taxonomyGuid)
        {
            return SendApiRequest("DeleteChildTaxons", "taxonomyGuid=" + taxonomyGuid.ToString());
        }
        public APIResponse SaveTaxonomy(TaxonomyFormData data)
        {
            return SendApiPost("SaveTaxonomy", "", data);
        }
        public class TaxonomyFormData
        {
            public String taxonomyGuid { get; set; }
            public String Name { get; set; }
            public Int32 TaxonomyType { get; set; }
        }
        public APIResponse SaveTaxon(TaxonFormData data)
        {
            return SendApiPost("SaveTaxon", "", data);
        }
        public class TaxonFormData
        {
            public String taxonGuid { get; set; }
            public String parentTaxonomyGuid { get; set; }
            public String parentTaxonGuid { get; set; }
            public String Name { get; set; }
            public Int32 TaxonType { get; set; }
        }
        #endregion

        #region Library Methods...
        public APIResponse GetLibraryTreeStructure(Guid rootLibId)
        {
            return SendApiRequest("GetLibraryTreeStructure", "rootLibId=" + rootLibId.ToString());
        }
        public APIResponse GetLibraries()
        {
            return SendApiRequest("GetLibraries", "");
        }
        public APIResponse GetLibrary(Guid libGuid)
        {
            return SendApiRequest("GetLibrary", "libGuid=" + libGuid.ToString());
        }
        public APIResponse DeleteLibrary(Guid libGuid)
        {
            return SendApiRequest("DeleteLibrary", "libGuid=" + libGuid.ToString());
        }
        public APIResponse SaveLibrary(LibraryFormData data)
        {
            return SendApiPost("SaveLibrary", "", data);
        }
        public class LibraryFormData
        {
            public String LibGuid { get; set; }
            public String Title { get; set; }
        }
        #endregion

        #region Folder Methods...
        public APIResponse GetFolders(Guid rootFolderGuid)
        {
            return SendApiRequest("GetFolders", "rootFolderGuid=" + rootFolderGuid.ToString());
        }
        public APIResponse GetFolder(Guid folderGuid)
        {
            return SendApiRequest("GetFolder", "folderGuid=" + folderGuid.ToString());
        }
        public APIResponse DeleteFolder(Guid folderGuid)
        {
            return SendApiRequest("DeleteFolder", "folderGuid=" + folderGuid.ToString());
        }
        public APIResponse DeleteFolders(List<Guid> folderGuids)
        {
            GetDocumentArgs objArgs = new GetDocumentArgs();
            objArgs.guidList = JsonConvert.SerializeObject(folderGuids);
            return SendApiPost("DeleteFolders", "", objArgs);
        }
        public APIResponse SaveFolder(FolderFormData data)
        {
            return SendApiPost("SaveFolder", "", data);
        }
        public class FolderFormData
        {
            public String folderGuid { get; set; }
            public String libGuid { get; set; }
            public String parentFolderGuid { get; set; }

            public String Title { get; set; }
            public String Description { get; set; }
        }
        #endregion

        #region Document Methods...
        public APIResponse GetDocumentCustomFields()
        {
            return SendApiRequest("GetDocumentCustomFields", "");
        }
        public APIResponse GetDocuments(Guid libGuid)
        {
            return SendApiRequest("GetDocuments", "libGuid=" + libGuid.ToString());
        }
        public APIResponse GetDocuments(List<Guid> docGuids)
        {
            GetDocumentArgs objArgs = new GetDocumentArgs();
            objArgs.guidList = JsonConvert.SerializeObject(docGuids);
            return SendApiPost("GetDocuments", "", objArgs);
        }
        public class GetDocumentArgs
        {
            public String guidList { get; set; }
        }
        public APIResponse GetDocumentsByFolder(Guid folderGuid)
        {
            return SendApiRequest("GetDocumentsByFolder", "folderGuid=" + folderGuid.ToString());
        }
        public APIResponse GetDocument(Guid Id)
        {
            return SendApiRequest("GetDocument", "Id=" + Id.ToString());
        }
        public APIResponse GetDocumentBinary(Guid Id)
        {
            return SendApiRequest("GetDocumentBinary", "Id=" + Id.ToString());
        }
        public APIResponse DeleteDocument(Guid docGuid)
        {
            return SendApiRequest("DeleteDocument", "docGuid=" + docGuid.ToString());
        }
        public APIResponse DeleteDocuments(List<Guid> docGuids)
        {
            GetDocumentArgs objArgs = new GetDocumentArgs();
            objArgs.guidList = JsonConvert.SerializeObject(docGuids);
            return SendApiPost("DeleteDocuments", "", objArgs);
        }
        public APIResponse SaveDocument(DocumentFormData data)
        {
            return SendApiPost("SaveDocument", "", data);
        }
        public class DocumentFormData
        {
            public String DocGuid { get; set; }
            public String LibGuid { get; set; }
            public String FolderGuid { get; set; }

            public String Title { get; set; }
            public String Filename { get; set; }

            public String FieldValues { get; set; }

            public Byte[] FileData { get; set; }

            public DateTime DateCreated { get; set; }
            public DateTime LastModified { get; set; }
        }
        #endregion

        public APIResponse RunSearchIndex(String indexName)
        {
            return SendApiRequest("RunSearchIndex", "indexName=" + indexName);
        }
        public APIResponse PurgeDocumentRevisionHistories(Guid libGuid)
        {
            return SendApiRequest("PurgeDocumentRevisionHistories", "libGuid=" + libGuid.ToString());
        }
        public APIResponse PurgeOrhpanedSFChunks()
        {
            return SendApiRequest("PurgeOrhpanedSFChunks", "");
        }
    }
}