using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Net;
using System.Threading.Tasks;

using ODataService;

using UDC.Common;
using UDC.Common.Data.Models;

namespace UDC.SharePointIntegrator.Data
{
    //Create Wrappers / Type Converters for our Native OData -> Dictionary Conversions..
    public class PlatformIO_REST
    {
        public String EndPointURL { get; set; }
        public String ServiceUsername { get; set; }
        public String ServicePassword { get; set; }
        public String ServiceDomain { get; set; }

        public PlatformIO_REST()
        {
            this.EndPointURL = AppSettings.GetValue("SharePoint:EndPointURL");
            this.ServiceUsername = AppSettings.GetValue("SharePoint:ServiceUsername");
            this.ServicePassword = AppSettings.GetValue("SharePoint:ServicePassword");
            this.ServiceDomain = AppSettings.GetValue("SharePoint:ServiceDomain");
        }
        public PlatformIO_REST(String endPointURL, String serviceUsername, String servicePassword, String serviceDomain)
        {
            this.EndPointURL = endPointURL;
            this.ServiceUsername = serviceUsername;
            this.ServicePassword = servicePassword;
            this.ServiceDomain = serviceDomain;
        }

        private ApiData GetContext()
        {
            ApiData objApi = new ApiData(new Uri(this.EndPointURL + "/_api"));
            NetworkCredential objCredentials = new NetworkCredential(this.ServiceUsername, this.ServicePassword, this.ServiceDomain);
            
            objApi.Credentials = objCredentials;

            return objApi;
        }
        private void LoadProperty(ApiData api, Object entity, String propertyName)
        {
            TaskFactory objTFactory = new TaskFactory();
            QueryOperationResponse objResult = objTFactory.FromAsync(api.BeginLoadProperty(entity, propertyName, null, null), obj => api.EndLoadProperty(obj)).Result;

            objResult = null;
            objTFactory = null;
        }

        public List<Dictionary<String, Object>> GetLists()
        {
            List<Dictionary<String, Object>> retVal = null;
            ApiData objApi = GetContext();
            DataServiceQuery<List> objQuery = objApi.Lists.Expand("Fields,Field");
            TaskFactory<IEnumerable<List>> objTFactory = new TaskFactory<IEnumerable<List>>();
            IEnumerable<List> arrLists = objTFactory.FromAsync(objQuery.BeginExecute(null, null), obj => objQuery.EndExecute(obj)).Result;

            if (arrLists != null)
            {
                retVal = new List<Dictionary<String, Object>>();

                foreach (List objList in arrLists)
                {
                    Dictionary<String, Object> objDestList = new Dictionary<String, Object>();

                    objDestList.Add("ListId", objList.Id);
                    objDestList.Add("ListTitle", objList.Title);
                    objDestList.Add("Description", objList.Description);
                    objDestList.Add("EntityType", objList.EntityTypeName);
                    objDestList.Add("LastItemDeletedDate", objList.LastItemDeletedDate);
                    objDestList.Add("LastItemModifiedDate", objList.LastItemModifiedDate);
                    objDestList.Add("ItemCount", objList.ItemCount);

                    List<String> arrFields = new List<String>();
                    if(objList.Fields != null)
                    {
                        foreach(Field objSrcField in objList.Fields)
                        {
                            arrFields.Add(objSrcField.InternalName + " - " + objSrcField.TypeAsString + " - " + objSrcField.FieldTypeKind + " - " + objSrcField.EntityPropertyName);
                        }
                    }
                    objDestList.Add("Fields", arrFields);

                    retVal.Add(objDestList);
                }
            }

            arrLists = null;
            objTFactory = null;
            objQuery = null;
            objApi = null;

            return retVal;
        }
        public APIActionResult SaveList(Object targetList)
        {
            return new APIActionResult();
        }
        public APIActionResult DeleteList(Object targetList)
        {
            return new APIActionResult();
        }

        public List<Object> GetDocuments()
        {
            return new List<Object>();
        }
        public APIActionResult SaveDocument(Object targetDoc)
        {
            return new APIActionResult();
        }
        public APIActionResult DeleteDocument(Object targetDoc)
        {
            return new APIActionResult();
        }

        public List<Dictionary<String, Object>> GetTags()
        {
            List<Dictionary<String, Object>> retVal = null;
            
            return retVal;
        }
        public APIActionResult SaveTag(Object targetTag)
        {
            return new APIActionResult();
        }
        public APIActionResult DeleteTag(Object targetTag)
        {
            return new APIActionResult();
        }

        public List<Dictionary<String, Object>> GetTreeStructure(Guid listID)
        {
            List<Dictionary<String, Object>> retVal = null;
            ApiData objApi = GetContext();
            DataServiceQuery<List> objQuery = objApi.Lists.AddQueryOption("$filter", "Id eq guid'" + listID.ToString() + "'").Expand("RootFolder,Folder");

            TaskFactory<IEnumerable<List>> objTFactory = new TaskFactory<IEnumerable<List>>();
            IEnumerable<List> arrLists = objTFactory.FromAsync(objQuery.BeginExecute(null, null), obj => objQuery.EndExecute(obj)).Result;

            if (arrLists != null)
            {
                retVal = new List<Dictionary<String, Object>>();
                foreach (List objList in arrLists)
                {
                    Dictionary<String, Object> objFolderContents = GetFolderContents(objApi, objList.RootFolder);
                    
                    objFolderContents.Add("ListId", objList.Id);
                    objFolderContents.Add("ListTitle", objList.Title);
                    objFolderContents.Add("ItemCount", objList.ItemCount);

                    retVal.Add(objFolderContents);
                }
            }

            arrLists = null;
            objTFactory = null;
            objQuery = null;
            objApi = null;

            return retVal;
        }
        public Dictionary<String, Object> GetFolderContents(ApiData objApi, Folder parentFolder)
        {
            Dictionary<String, Object> retVal = new Dictionary<String, Object>();
            List<Dictionary<String, Object>> arrDestFolders = new List<Dictionary<String, Object>>();
            List<Dictionary<String, Object>> arrDestItems = new List<Dictionary<String, Object>>();

            LoadProperty(objApi, parentFolder, "Folders");
            LoadProperty(objApi, parentFolder, "Files");

            if (parentFolder.Folders != null)
            {
                foreach(Folder objSrcFolder in parentFolder.Folders)
                {
                    Dictionary<String, Object> objDestFolder = new Dictionary<String, Object>();

                    objDestFolder.Add("Id", objSrcFolder.ServerRelativeUrl);
                    objDestFolder.Add("ParentId", objSrcFolder.ServerRelativeUrl);
                    objDestFolder.Add("Path", objSrcFolder.ServerRelativeUrl);
                    objDestFolder.Add("Title", objSrcFolder.Name);

                    if (objSrcFolder.ItemCount > 0)
                    {
                        //Recurse to next level...
                        Dictionary<String, Object> arrRecursiveCall = GetFolderContents(objApi, objSrcFolder);
                        GeneralHelpers.appendDictionary(ref objDestFolder, ref arrRecursiveCall);
                    }

                    arrDestFolders.Add(objDestFolder);
                }
            }
            if (parentFolder.Files != null)
            {
                foreach (File objSrcFile in parentFolder.Files)
                {
                    Dictionary<String, Object> objDestItem = new Dictionary<String, Object>();

                    objDestItem.Add("Id", objSrcFile.ServerRelativeUrl);
                    objDestItem.Add("FolderId", parentFolder.ServerRelativeUrl);
                    objDestItem.Add("Path", objSrcFile.ServerRelativeUrl);
                    objDestItem.Add("Title", objSrcFile.Title);
                    objDestItem.Add("FileName", objSrcFile.Name);
                    objDestItem.Add("Extension", System.IO.Path.GetExtension(objSrcFile.Name));
                    objDestItem.Add("TotalSize", objSrcFile.Length);
                    objDestItem.Add("LastModified", objSrcFile.TimeLastModified);
                    objDestItem.Add("DateCreated", objSrcFile.TimeCreated);

                    arrDestItems.Add(objDestItem);
                }
            }

            retVal.Add("Folders", arrDestFolders);
            retVal.Add("Documents", arrDestItems);

            return retVal;
        }
    }
}