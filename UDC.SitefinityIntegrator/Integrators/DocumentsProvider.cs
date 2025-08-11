using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using UDC.Common;
using UDC.Common.Data.Models;
using UDC.Common.Data.Models.Configuration;
using UDC.Common.Interfaces;
using UDC.SitefinityIntegrator.Data;

using static UDC.Common.Constants;
using static UDC.SitefinityIntegrator.Data.PlatformIO;

namespace UDC.SitefinityIntegrator.Integrators
{
    public class DocumentsProvider : IIntegrator
    {
        public Guid IntegratorID { get; set; }
        public String Name { get; set; }

        public Boolean SupportsRead { get; set; }
        public Boolean SupportsWrite { get; set; }

        public Boolean SupportsContainers { get; set; }
        public Boolean SupportsMetaTags { get; set; }

        public PlatformCfg PlatformConfig { get; set; }
        public List<Dictionary<String, Object>> Settings { get; set; }

        public DocumentsProvider()
        {
            Init();
        }
        public DocumentsProvider(PlatformCfg cfg)
        {
            Init();

            this.PlatformConfig = cfg;
        }
        private void Init()
        {
            this.IntegratorID = new Guid("5f49ff89-604f-4e29-935e-bd463dff354e");
            this.Name = "Documents Provider";

            this.SupportsRead = true;
            this.SupportsWrite = true;

            this.SupportsContainers = true;
            this.SupportsMetaTags = true;
        }

        public List<SyncContainer> GetContainers()
        {
            //Get Doc Libs...
            PlatformIO objPlatformIO = new PlatformIO(this.PlatformConfig);
            APIResponse objAPIResponse = objPlatformIO.GetLibraries();
            List<SyncContainer> arrContainers = null;

            if(objAPIResponse != null)
            {
                if(objAPIResponse.exitCode == 0)
                {
                    if(objAPIResponse.data != null && objAPIResponse.data is JArray)
                    {
                        List<Dictionary<String, Object>> arrLibraries = ((JArray)objAPIResponse.data).ToObject<List<Dictionary<String, Object>>>();
                        if (arrLibraries != null && arrLibraries.Count > 0)
                        {
                            arrContainers = new List<SyncContainer>();

                            foreach(Dictionary<String, Object> srcLib in arrLibraries)
                            {
                                SyncContainer objDestLib = new SyncContainer();
                                TypeConverters.ConvertSyncContainer(srcLib, ref objDestLib);
                                arrContainers.Add(objDestLib);
                            }
                        }
                        arrLibraries = null;
                    }
                }
                else
                {
                    throw new Exception("The SitefinityContextPlugin returned an error while serving this request. The error was: " + objAPIResponse.message);
                }
            }

            objAPIResponse = null;
            objPlatformIO = null;

            return arrContainers;
        }
        public SyncContainer GetContainerTree(String rootContainerId)
        {
            //Get Library... Hierarchical Contents...
            PlatformIO objPlatformIO = new PlatformIO(this.PlatformConfig);
            APIResponse objAPIResponse = null;
            SyncContainer objRootContainer = null;
            Guid libGuid = GeneralHelpers.parseGUID(rootContainerId);

            if (libGuid != Guid.Empty)
            {
                objAPIResponse = objPlatformIO.GetLibraryTreeStructure(libGuid);
                if (objAPIResponse != null)
                {
                    if (objAPIResponse.exitCode == 0)
                    {
                        if (objAPIResponse.data != null && objAPIResponse.data is JObject)
                        {
                            Dictionary<String, Object> objRootFolder = ((JObject)objAPIResponse.data).ToObject<Dictionary<String, Object>>();
                            if (objRootFolder != null)
                            {
                                objRootContainer = new SyncContainer();
                                TypeConverters.ConvertSyncContainer(objRootFolder, ref objRootContainer);

                                objRootContainer.SyncContainers = GetSubContainers(objRootFolder);
                                objRootContainer.SyncObjects = GetContainerObjects(objRootFolder);
                            }
                            objRootFolder = null;
                        }
                    }
                    else
                    {
                        throw new Exception("The SitefinityContextPlugin returned an error while serving this request. The error was: " + objAPIResponse.message);
                    }
                }
            }

            objAPIResponse = null;
            objPlatformIO = null;

            return objRootContainer;
        }
        private List<SyncContainer> GetSubContainers(Dictionary<String, Object> parent)
        {
            List<SyncContainer> arrRetVal = null;

            if (parent["Folders"] != null && parent["Folders"] is JArray)
            {
                List<Dictionary<String, Object>> arrFolders = ((JArray)parent["Folders"]).ToObject<List<Dictionary<String, Object>>>();

                if (arrFolders != null && arrFolders.Count > 0)
                {
                    arrRetVal = new List<SyncContainer>();

                    foreach (Dictionary<String, Object> srcFolder in arrFolders)
                    {
                        SyncContainer objDestContainer = new SyncContainer();
                        TypeConverters.ConvertSyncContainer(srcFolder, ref objDestContainer);
                        
                        objDestContainer.SyncContainers = GetSubContainers(srcFolder);
                        objDestContainer.SyncObjects = GetContainerObjects(srcFolder);

                        arrRetVal.Add(objDestContainer);
                    }
                }

                arrFolders = null;
            }

            return arrRetVal;
        }
        private List<SyncObject> GetContainerObjects(Dictionary<String, Object> parent)
        {
            List<SyncObject> arrRetVal = null;

            if (parent["Documents"] != null && parent["Documents"] is JArray)
            {
                List<Dictionary<String, Object>> arrDocs = ((JArray)parent["Documents"]).ToObject<List<Dictionary<String, Object>>>();

                if (arrDocs != null && arrDocs.Count > 0)
                {
                    arrRetVal = new List<SyncObject>();

                    foreach (Dictionary<String, Object> srcDoc in arrDocs)
                    {
                        SyncObject objDestDoc = new SyncObject();
                        TypeConverters.ConvertSyncObject(srcDoc, ref objDestDoc);

                        arrRetVal.Add(objDestDoc);
                    }
                }

                arrDocs = null;
            }

            return arrRetVal;
        }
        public String SaveContainer(SyncContainer container, Boolean isRoot, String rootContainerId)
        {
            String returnedId = "";
            PlatformIO objPlatformIO = new PlatformIO(this.PlatformConfig);
            APIResponse objAPIResponse = null;
            Exception objEx = null;

            if (isRoot)
            {
                LibraryFormData objLibData = new LibraryFormData();

                objLibData.LibGuid = container.Id;
                objLibData.Title = container.Name;

                objAPIResponse = objPlatformIO.SaveLibrary(objLibData);

                objLibData = null;
            }
            else
            {
                FolderFormData objFolderData = new FolderFormData();

                objFolderData.folderGuid = container.Id;
                objFolderData.libGuid = rootContainerId;
                objFolderData.parentFolderGuid = container.parentId;

                objFolderData.Title = container.Name;
                objFolderData.Description = ""; //Not mapped for now...

                objAPIResponse = objPlatformIO.SaveFolder(objFolderData);
                objFolderData = null;
            }

            if (objAPIResponse != null)
            {
                if (objAPIResponse.exitCode == 0)
                {
                    if(objAPIResponse.data is JObject)
                    {
                        APIActionResult objResult = ((JObject)objAPIResponse.data).ToObject<APIActionResult>();
                        if(objResult != null && (objResult.APIAction == APIActionResult.APIActions.Created || objResult.APIAction == APIActionResult.APIActions.Updated))
                        {
                            returnedId = objResult.Id.ToString();
                        }
                        else
                        {
                            objEx = new Exception("The SitefinityContextPlugin was not able to fulfill saving this item. The Api Action returned was: " + objResult.APIAction.ToString());
                        }
                    }
                }
                else
                {
                    objEx = new Exception("The SitefinityContextPlugin returned an error while serving this request. The error was: " + objAPIResponse.message);
                }
            }

            objAPIResponse = null;
            objPlatformIO = null;

            if (objEx != null)
            {
                throw objEx;
            }

            return returnedId;
        }
        public Boolean DeleteContainers(List<String> ids)
        {
            PlatformIO objPlatformIO = new PlatformIO(this.PlatformConfig);
            APIResponse objAPIResponse = null;
            Boolean retVal = false;
            List<Guid> arrContainerGuids = null;

            if (ids != null)
            {
                arrContainerGuids = new List<Guid>();
                foreach (String containerId in ids)
                {
                    if (GeneralHelpers.parseGUID(containerId) != Guid.Empty)
                    {
                        arrContainerGuids.Add(GeneralHelpers.parseGUID(containerId));
                    }
                }
            }
            if (arrContainerGuids != null)
            {
                objAPIResponse = objPlatformIO.DeleteFolders(arrContainerGuids);
                if (objAPIResponse != null)
                {
                    if (objAPIResponse.exitCode == 0)
                    {
                        if (objAPIResponse.data != null && objAPIResponse.data is JObject)
                        {
                            APIActionResult objAPIActionResult = ((JObject)objAPIResponse.data).ToObject<APIActionResult>();
                            if (objAPIActionResult.APIAction == APIActionResult.APIActions.Deleted)
                            {
                                retVal = true;
                            }
                            objAPIActionResult = null;
                        }
                    }
                    else
                    {
                        throw new Exception("The SitefinityContextPlugin returned an error while serving this request. The error was: " + objAPIResponse.message);
                    }
                }
            }

            arrContainerGuids = null;
            objAPIResponse = null;
            objPlatformIO = null;

            return retVal;
        }

        public List<SyncField> GetFields(String rootContainerId)
        {
            PlatformIO objPlatformIO = new PlatformIO(this.PlatformConfig);
            APIResponse objAPIResponse = objPlatformIO.GetDocumentCustomFields();
            List<SyncField> arrSyncFields = new List<SyncField>();

            //Add our known document properties...
            arrSyncFields.Add(new SyncField("", "Id", "Id", "System.Guid", FieldDataTypes.Guid, "", false));
            arrSyncFields.Add(new SyncField("", "Title", "Title", "System.String", FieldDataTypes.String, "", true));
            arrSyncFields.Add(new SyncField("", "Description", "Description", "System.String", FieldDataTypes.String, "", true));
            arrSyncFields.Add(new SyncField("", "Author", "Author", "System.String", FieldDataTypes.String, "", true));
            arrSyncFields.Add(new SyncField("", "MimeType", "MimeType", "System.String", FieldDataTypes.String, "", true));
            arrSyncFields.Add(new SyncField("", "TotalSize", "TotalSize", "System.Int64", FieldDataTypes.Integer, "", false));

            arrSyncFields.Add(new SyncField("", "DateCreated", "DateCreated", "System.DateTime", FieldDataTypes.DateTime, "", false));
            arrSyncFields.Add(new SyncField("", "LastModified", "LastModified", "System.DateTime", FieldDataTypes.DateTime, "", false));
            arrSyncFields.Add(new SyncField("", "ExpirationDate", "ExpirationDate", "System.DateTime", FieldDataTypes.DateTime, "", true));
            arrSyncFields.Add(new SyncField("", "PublicationDate", "PublicationDate", "System.DateTime", FieldDataTypes.DateTime, "", true));

            //Parse / Append any Dynamic Meta Data Fields
            if (objAPIResponse != null)
            {
                if (objAPIResponse.exitCode == 0)
                {
                    if (objAPIResponse.data != null && objAPIResponse.data is JArray)
                    {
                        List<Dictionary<String, Object>> arrCustFields = ((JArray)objAPIResponse.data).ToObject<List<Dictionary<String, Object>>>();
                        if (arrCustFields != null && arrCustFields.Count > 0)
                        {
                            foreach (Dictionary<String, Object> srcLib in arrCustFields)
                            {
                                SyncField objDestField = new SyncField();
                                TypeConverters.ConvertSyncField(srcLib, ref objDestField);
                                objDestField.Writable = true;
                                arrSyncFields.Add(objDestField);
                            }
                        }
                        arrCustFields = null;
                    }
                }
                else
                {
                    throw new Exception("The SitefinityContextPlugin returned an error while serving this request. The error was: " + objAPIResponse.message);
                }
            }

            objAPIResponse = null;
            objPlatformIO = null;

            return arrSyncFields;
        }
        public List<SyncObject> GetObjects(String containerId, List<SyncField> fields, Boolean includeBinary = false)
        {
            //Get Docs...
            PlatformIO objPlatformIO = new PlatformIO(this.PlatformConfig);
            APIResponse objAPIResponse = null;
            List<SyncObject> arrSyncObjects = null;
            Guid containerGuid = GeneralHelpers.parseGUID(containerId);

            if(containerGuid != Guid.Empty)
            {
                objAPIResponse = objPlatformIO.GetDocuments(containerGuid);
                if (objAPIResponse != null)
                {
                    if (objAPIResponse.exitCode == 0)
                    {
                        if (objAPIResponse.data != null && objAPIResponse.data is JArray)
                        {
                            List<Dictionary<String, Object>> arrDocuments = ((JArray)objAPIResponse.data).ToObject<List<Dictionary<String, Object>>>();
                            if (arrDocuments != null && arrDocuments.Count > 0)
                            {
                                arrSyncObjects = new List<SyncObject>();

                                foreach (Dictionary<String, Object> srcDoc in arrDocuments)
                                {
                                    SyncObject objDestDoc = new SyncObject();
                                    TypeConverters.ConvertSyncObject(srcDoc, ref objDestDoc);

                                    if (includeBinary)
                                    {
                                        Guid docGuid = GeneralHelpers.parseGUID(objDestDoc.Id);
                                        if(docGuid != Guid.Empty)
                                        {
                                            APIResponse objBinaryResp = objPlatformIO.GetDocumentBinary(docGuid);
                                            if (objBinaryResp != null)
                                            {
                                                objDestDoc.BinaryPayload = (Byte[])objBinaryResp.data;
                                            }
                                            objBinaryResp = null;
                                        }
                                    }

                                    arrSyncObjects.Add(objDestDoc);
                                }
                            }
                            arrDocuments = null;
                        }
                    }
                    else
                    {
                        throw new Exception("The SitefinityContextPlugin returned an error while serving this request. The error was: " + objAPIResponse.message);
                    }
                }
            }

            objAPIResponse = null;
            objPlatformIO = null;

            return arrSyncObjects;
        }
        public List<SyncObject> GetObjects(List<String> docIds, List<SyncField> fields, Boolean includeBinary = false)
        {
            //Get Docs...
            PlatformIO objPlatformIO = new PlatformIO(this.PlatformConfig);
            APIResponse objAPIResponse = null;
            List<SyncObject> arrSyncObjects = null;
            List<Guid> arrDocGuids = null;

            if(docIds != null)
            {
                arrDocGuids = new List<Guid>();
                foreach (String docId in docIds)
                {
                    if(GeneralHelpers.parseGUID(docId) != Guid.Empty)
                    {
                        arrDocGuids.Add(GeneralHelpers.parseGUID(docId));
                    }
                }
            }
            if (arrDocGuids != null)
            {
                objAPIResponse = objPlatformIO.GetDocuments(arrDocGuids);
                if (objAPIResponse != null)
                {
                    if (objAPIResponse.exitCode == 0)
                    {
                        if (objAPIResponse.data != null && objAPIResponse.data is JArray)
                        {
                            List<Dictionary<String, Object>> arrDocuments = ((JArray)objAPIResponse.data).ToObject<List<Dictionary<String, Object>>>();
                            if (arrDocuments != null && arrDocuments.Count > 0)
                            {
                                arrSyncObjects = new List<SyncObject>();

                                foreach (Dictionary<String, Object> srcDoc in arrDocuments)
                                {
                                    SyncObject objDestDoc = new SyncObject();
                                    TypeConverters.ConvertSyncObject(srcDoc, ref objDestDoc);

                                    if (includeBinary)
                                    {
                                        Guid docGuid = GeneralHelpers.parseGUID(objDestDoc.Id);
                                        if (docGuid != Guid.Empty)
                                        {
                                            APIResponse objBinaryResp = objPlatformIO.GetDocumentBinary(docGuid);
                                            if (objBinaryResp != null)
                                            {
                                                objDestDoc.BinaryPayload = (Byte[])objBinaryResp.data;
                                            }
                                            objBinaryResp = null;
                                        }
                                    }

                                    arrSyncObjects.Add(objDestDoc);
                                }
                            }
                            arrDocuments = null;
                        }
                    }
                    else
                    {
                        throw new Exception("The SitefinityContextPlugin returned an error while serving this request. The error was: " + objAPIResponse.message);
                    }
                }
            }

            objAPIResponse = null;
            objPlatformIO = null;

            return arrSyncObjects;
        }
        public SyncObject GetObject(String id, List<SyncField> fields, Boolean includeBinary = false)
        {
            //Get Doc...
            PlatformIO objPlatformIO = new PlatformIO(this.PlatformConfig);
            APIResponse objAPIResponse = null;
            SyncObject objSyncObject = null;
            Guid docGuid = GeneralHelpers.parseGUID(id);

            if (docGuid != Guid.Empty)
            {
                objAPIResponse = objPlatformIO.GetDocument(docGuid);
                if (objAPIResponse != null)
                {
                    if (objAPIResponse.exitCode == 0)
                    {
                        if (objAPIResponse.data != null && objAPIResponse.data is JObject)
                        {
                            Dictionary<String, Object> objDocument = ((JObject)objAPIResponse.data).ToObject<Dictionary<String, Object>>();
                            if (objDocument != null)
                            {
                                objSyncObject = new SyncObject();
                                TypeConverters.ConvertSyncObject(objDocument, ref objSyncObject);

                                if (includeBinary)
                                {
                                    APIResponse objBinaryResp = objPlatformIO.GetDocumentBinary(docGuid);
                                    if (objBinaryResp != null)
                                    {
                                        objSyncObject.BinaryPayload = (Byte[])objBinaryResp.data;
                                    }
                                    objBinaryResp = null;
                                }
                            }
                            objDocument = null;
                        }
                    }
                    else
                    {
                        throw new Exception("The SitefinityContextPlugin returned an error while serving this request. The error was: " + objAPIResponse.message);
                    }
                }
            }

            objAPIResponse = null;
            objPlatformIO = null;

            return objSyncObject;
        }
        public String SaveObject(SyncObject syncObj)
        {
            String returnedId = "";
            PlatformIO objPlatformIO = new PlatformIO(this.PlatformConfig);
            APIResponse objAPIResponse = null;
            Exception objEx = null;

            //This will take some work to get our Dynamic Properties List and Types Mapped up...
            DocumentFormData objDocumentData = new DocumentFormData();

            objDocumentData.DocGuid = syncObj.Id;
            objDocumentData.LibGuid = syncObj.rootContainerId;
            objDocumentData.FolderGuid = syncObj.containerId;

            objDocumentData.Title = syncObj.Title;
            objDocumentData.Filename = syncObj.FileName;
            objDocumentData.FieldValues = JsonConvert.SerializeObject(syncObj.Properties);
            objDocumentData.FileData = syncObj.BinaryPayload;

            objDocumentData.DateCreated = syncObj.DateCreated;
            objDocumentData.LastModified = syncObj.LastUpdated;

            objAPIResponse = objPlatformIO.SaveDocument(objDocumentData);
            objDocumentData = null;

            if (objAPIResponse != null)
            {
                if (objAPIResponse.exitCode == 0)
                {
                    if (objAPIResponse.data is JObject)
                    {
                        APIActionResult objResult = ((JObject)objAPIResponse.data).ToObject<APIActionResult>();
                        if (objResult != null && (objResult.APIAction == APIActionResult.APIActions.Created || objResult.APIAction == APIActionResult.APIActions.Updated))
                        {
                            returnedId = objResult.Id.ToString();
                        }
                        else
                        {
                            objEx = new Exception("The SitefinityContextPlugin was not able to fulfill saving this item. The Api Action returned was: " + objResult.APIAction.ToString());
                        }
                    }
                }
                else
                {
                    objEx = new Exception("The SitefinityContextPlugin returned an error while serving this request. The error was: " + objAPIResponse.message + "\n" + GeneralHelpers.parseString(objAPIResponse.data));
                }
            }

            objAPIResponse = null;
            objPlatformIO = null;

            if(objEx != null)
            {
                throw objEx;
            }

            return returnedId;
        }
        public Boolean DeleteObjects(List<String> ids)
        {
            PlatformIO objPlatformIO = new PlatformIO(this.PlatformConfig);
            APIResponse objAPIResponse = null;
            Boolean retVal = false;
            List<Guid> arrDocGuids = null;

            if (ids != null)
            {
                arrDocGuids = new List<Guid>();
                foreach (String docId in ids)
                {
                    if (GeneralHelpers.parseGUID(docId) != Guid.Empty)
                    {
                        arrDocGuids.Add(GeneralHelpers.parseGUID(docId));
                    }
                }
            }
            if (arrDocGuids != null)
            {
                objAPIResponse = objPlatformIO.DeleteDocuments(arrDocGuids);
                if (objAPIResponse != null)
                {
                    if (objAPIResponse.exitCode == 0)
                    {
                        if (objAPIResponse.data != null && objAPIResponse.data is JObject)
                        {
                            APIActionResult objAPIActionResult = ((JObject)objAPIResponse.data).ToObject<APIActionResult>();
                            if (objAPIActionResult.APIAction == APIActionResult.APIActions.Deleted)
                            {
                                retVal = true;
                            }
                            objAPIActionResult = null;
                        }
                    }
                    else
                    {
                        throw new Exception("The SitefinityContextPlugin returned an error while serving this request. The error was: " + objAPIResponse.message);
                    }
                }
            }

            arrDocGuids = null;
            objAPIResponse = null;
            objPlatformIO = null;

            return retVal;
        }

        public List<SyncTag> GetMetaTagsList()
        {
            //Get Taxonomies List...
            PlatformIO objPlatformIO = new PlatformIO(this.PlatformConfig);
            APIResponse objAPIResponse = objPlatformIO.GetAllHierarchicalTaxonomies();
            List<SyncTag> arrTags = null;

            if (objAPIResponse != null)
            {
                if (objAPIResponse.exitCode == 0)
                {
                    if (objAPIResponse.data != null && objAPIResponse.data is JArray)
                    {
                        List<Dictionary<String, Object>> arrTaxonomies = ((JArray)objAPIResponse.data).ToObject<List<Dictionary<String, Object>>>();
                        if (arrTaxonomies != null && arrTaxonomies.Count > 0)
                        {
                            arrTags = new List<SyncTag>();

                            foreach (Dictionary<String, Object> srcTaxonomy in arrTaxonomies)
                            {
                                SyncTag objDestTaxonomy = new SyncTag();
                                TypeConverters.ConvertSyncTag(srcTaxonomy, ref objDestTaxonomy);
                                arrTags.Add(objDestTaxonomy);
                            }
                        }
                        arrTaxonomies = null;
                    }
                }
                else
                {
                    throw new Exception("The SitefinityContextPlugin returned an error while serving this request. The error was: " + objAPIResponse.message);
                }
            }

            objAPIResponse = null;
            objPlatformIO = null;

            return arrTags;
        }
        public SyncTag GetMetaTagTree(String id)
        {
            //Get Taxonomies List... Hierarchical Mappings...
            PlatformIO objPlatformIO = new PlatformIO(this.PlatformConfig);
            APIResponse objAPIResponse = null;
            SyncTag objRootTag = null;
            Guid taxonomyGuid = GeneralHelpers.parseGUID(id);

            if(taxonomyGuid != Guid.Empty)
            {
                objAPIResponse = objPlatformIO.GetHierarchicalTaxonomyTree(taxonomyGuid);
                if (objAPIResponse != null)
                {
                    if (objAPIResponse.exitCode == 0)
                    {
                        if (objAPIResponse.data != null && objAPIResponse.data is JObject)
                        {
                            Dictionary<String, Object> objRootTaxonomy = ((JObject)objAPIResponse.data).ToObject<Dictionary<String, Object>>();
                            if (objRootTaxonomy != null)
                            {
                                objRootTag = new SyncTag();
                                TypeConverters.ConvertSyncTag(objRootTaxonomy, ref objRootTag);
                                objRootTag.SyncTags = GetChildMetaTags(objRootTaxonomy);
                            }
                            objRootTaxonomy = null;
                        }
                    }
                    else
                    {
                        throw new Exception("The SitefinityContextPlugin returned an error while serving this request. The error was: " + objAPIResponse.message);
                    }
                }
            }

            objAPIResponse = null;
            objPlatformIO = null;

            return objRootTag;
        }
        private List<SyncTag> GetChildMetaTags(Dictionary<String, Object> parent)
        {
            List<SyncTag> arrRetVal = null;

            if (parent["Taxa"] != null && parent["Taxa"] is JArray)
            {
                List<Dictionary<String, Object>> arrTaxons = ((JArray)parent["Taxa"]).ToObject<List<Dictionary<String, Object>>>();

                if(arrTaxons != null && arrTaxons.Count > 0)
                {
                    arrRetVal = new List<SyncTag>();

                    foreach (Dictionary<String, Object> srcTaxon in arrTaxons)
                    {
                        SyncTag objDestTag = new SyncTag();
                        TypeConverters.ConvertSyncTag(srcTaxon, ref objDestTag);
                        objDestTag.SyncTags = GetChildMetaTags(srcTaxon);
                        arrRetVal.Add(objDestTag);
                    }
                }

                arrTaxons = null;
            }

            return arrRetVal;
        }
        public String SaveMetaTag(SyncTag tag, Boolean isRoot, String rootTagId)
        {
            String returnedId = "";
            PlatformIO objPlatformIO = new PlatformIO(this.PlatformConfig);
            APIResponse objAPIResponse = null;
            Exception objEx = null;

            if (isRoot)
            {
                TaxonomyFormData objTaxonomyData = new TaxonomyFormData();

                objTaxonomyData.taxonomyGuid = tag.Id;
                objTaxonomyData.Name = tag.Name;
                objTaxonomyData.TaxonomyType = 1; //Always HierarchicalTaxonomy

                objAPIResponse = objPlatformIO.SaveTaxonomy(objTaxonomyData);

                objTaxonomyData = null;
            }
            else
            {
                TaxonFormData objTaxonData = new TaxonFormData();

                objTaxonData.taxonGuid = tag.Id;
                objTaxonData.parentTaxonomyGuid = rootTagId;
                objTaxonData.parentTaxonGuid = tag.parentId;
                objTaxonData.Name = tag.Name;
                objTaxonData.TaxonType = 1; //Always HierarchicalTaxon

                objAPIResponse = objPlatformIO.SaveTaxon(objTaxonData);
                objTaxonData = null;
            }

            if (objAPIResponse != null)
            {
                if (objAPIResponse.exitCode == 0)
                {
                    if (objAPIResponse.data is JObject)
                    {
                        APIActionResult objResult = ((JObject)objAPIResponse.data).ToObject<APIActionResult>();
                        if (objResult != null && (objResult.APIAction == APIActionResult.APIActions.Created || objResult.APIAction == APIActionResult.APIActions.Updated))
                        {
                            returnedId = objResult.Id.ToString();
                        }
                        else
                        {
                            objEx = new Exception("The SitefinityContextPlugin was not able to fulfill saving this item. The Api Action returned was: " + objResult.APIAction.ToString());
                        }
                    }
                }
                else
                {
                    objEx = new Exception("The SitefinityContextPlugin returned an error while serving this request. The error was: " + objAPIResponse.message);
                }
            }

            objAPIResponse = null;
            objPlatformIO = null;

            if (objEx != null)
            {
                throw objEx;
            }

            return returnedId;
        }
        public Boolean DeleteMetaTags(List<String> tagIds)
        {
            PlatformIO objPlatformIO = new PlatformIO(this.PlatformConfig);
            APIResponse objAPIResponse = null;
            Boolean retVal = false;
            List<Guid> arrTaxonGuids = null;

            if (tagIds != null)
            {
                arrTaxonGuids = new List<Guid>();
                foreach (String tagId in tagIds)
                {
                    if (GeneralHelpers.parseGUID(tagId) != Guid.Empty)
                    {
                        arrTaxonGuids.Add(GeneralHelpers.parseGUID(tagId));
                    }
                }
            }
            if (arrTaxonGuids != null)
            {
                objAPIResponse = objPlatformIO.DeleteTaxons(arrTaxonGuids);
                if (objAPIResponse != null)
                {
                    if (objAPIResponse.exitCode == 0)
                    {
                        if (objAPIResponse.data != null && objAPIResponse.data is JObject)
                        {
                            APIActionResult objAPIActionResult = ((JObject)objAPIResponse.data).ToObject<APIActionResult>();
                            if(objAPIActionResult.APIAction == APIActionResult.APIActions.Deleted)
                            {
                                retVal = true;
                            }
                            objAPIActionResult = null;
                        }
                    }
                    else
                    {
                        throw new Exception("The SitefinityContextPlugin returned an error while serving this request. The error was: " + objAPIResponse.message);
                    }
                }
            }

            arrTaxonGuids = null;
            objAPIResponse = null;
            objPlatformIO = null;

            return retVal;
        }

        public void RunPostSyncTasks()
        {
        }
    }
}