using System;
using System.Collections.Generic;
using System.Linq;
using UDC.Common;
using UDC.Common.Data.Models;
using UDC.Common.Data.Models.Configuration;
using UDC.Common.Interfaces;
using UDC.SharePointOnlineIntegrator.Data;
using Equ.SharePoint.GraphService;

namespace UDC.SharePointOnlineIntegrator.Integrators
{
    public class SharePointOnlineDocumentsProvider : IIntegrator
    {
        private readonly IGraphService _graphService;

        public SharePointOnlineDocumentsProvider()
        {
            Init();
        }

        public SharePointOnlineDocumentsProvider(PlatformCfg cfg)
        {
            Init();
            this.PlatformConfig = cfg;
        }

        public SharePointOnlineDocumentsProvider(IGraphService graphService)
        {
            _graphService = graphService;
            Init();
        }

        public SharePointOnlineDocumentsProvider(PlatformCfg cfg, IGraphService graphService)
        {
            _graphService = graphService;
            Init();
            this.PlatformConfig = cfg;
        }

        private void Init()
        {
            this.IntegratorID = new Guid("b6ee0926-1c61-4d77-99fa-c8272968b85a");
            this.Name = "Documents Provider";

            this.SupportsRead = true;
            this.SupportsWrite = false;

            this.SupportsContainers = true;
            this.SupportsMetaTags = true;
        }

        public Guid IntegratorID { get; set; }
        public String Name { get; set; }

        public Boolean SupportsRead { get; set; }
        public Boolean SupportsWrite { get; set; }

        public Boolean SupportsContainers { get; set; }
        public Boolean SupportsMetaTags { get; set; }

        public PlatformCfg PlatformConfig { get; set; }
        public List<Dictionary<String, Object>> Settings { get; set; }

        public List<SyncContainer> GetContainers()
        {
            List<SyncContainer> arrContainers = null;
            PlatformIO objPlatformIO = null;

            if (_graphService != null)
            {
                objPlatformIO = new PlatformIO(_graphService);
                var arrSrcLists = objPlatformIO.GetLists();
                if (arrSrcLists != null)
                {
                    arrContainers = new List<SyncContainer>();
                    foreach (Dictionary<String, Object> srcList in arrSrcLists)
                    {
                        SyncContainer objDestContainer = new SyncContainer();
                        TypeConverters.ConvertSyncContainer(srcList, ref objDestContainer);
                        arrContainers.Add(objDestContainer);
                    }
                }
            }

            objPlatformIO = null;
            return arrContainers;
        }
        public SyncContainer GetContainerTree(String rootContainerId)
        {
            SyncContainer objRootContainer = null;
            PlatformIO objPlatformIO = null;

            if (_graphService != null)
            {
                objPlatformIO = new PlatformIO(_graphService);
                Guid listGuid = GeneralHelpers.parseGUID(rootContainerId);
                Dictionary<String, Object> objSrcList = objPlatformIO.GetListTreeStructure(listGuid);

                if (objSrcList != null)
                {
                    objRootContainer = new SyncContainer();
                    TypeConverters.ConvertSyncContainer(objSrcList, ref objRootContainer);

                    objRootContainer.SyncContainers = GetSubContainers(objSrcList);
                    objRootContainer.SyncObjects = GetContainerObjects(objSrcList);
                }
            }

            objPlatformIO = null;
            return objRootContainer;
        }
        private List<SyncContainer> GetSubContainers(Dictionary<String, Object> parent)
        {
            List<SyncContainer> arrRetVal = null;

            if (parent != null && parent.ContainsKey("Folders") && parent["Folders"] != null)
            {
                List<Dictionary<String, Object>> arrFolders = (parent["Folders"]) as List<Dictionary<String, Object>>;

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
            }

            return arrRetVal;
        }
        private List<SyncObject> GetContainerObjects(Dictionary<String, Object> parent)
        {
            List<SyncObject> arrRetVal = null;

            if (parent != null && parent.ContainsKey("Documents") && parent["Documents"] != null)
            {
                List<Dictionary<String, Object>> arrDocs = (parent["Documents"]) as List<Dictionary<String, Object>>;

                if (arrDocs != null && arrDocs.Count > 0)
                {
                    arrRetVal = new List<SyncObject>();

                    foreach (Dictionary<String, Object> srcDoc in arrDocs)
                    {
                        SyncObject objDestDoc = new SyncObject();
                        TypeConverters.ConvertSyncObject(srcDoc, ref objDestDoc, null);

                        arrRetVal.Add(objDestDoc);
                    }
                }
            }

            return arrRetVal;
        }

        public String SaveContainer(SyncContainer container, Boolean isRoot, String rootContainerId)
        {
            throw new NotImplementedException("This Integrator does not support writing back to the platform.");
        }
        public Boolean DeleteContainers(List<String> ids)
        {
            throw new NotImplementedException("This Integrator does not support writing back to the platform.");
        }

        public List<SyncField> GetFields(String rootContainerId)
        {
            List<SyncField> arrRetVal = null;
            PlatformIO objPlatformIO = null;

            if (_graphService != null)
            {
                objPlatformIO = new PlatformIO(_graphService);
                Guid listGuid = GeneralHelpers.parseGUID(rootContainerId);
                Dictionary<String, Object> objSrcList = objPlatformIO.GetList(listGuid);

                if (objSrcList != null && objSrcList.ContainsKey("Fields") && objSrcList["Fields"] != null)
                {
                    List<Dictionary<String, Object>> arrFields = (List<Dictionary<String, Object>>)objSrcList["Fields"];

                    if (arrFields != null && arrFields.Count > 0)
                    {
                        arrRetVal = new List<SyncField>();
                        foreach (Dictionary<String, Object> srcField in arrFields)
                        {
                            SyncField objDestField = new SyncField();
                            TypeConverters.ConvertSyncField(srcField, ref objDestField);

                            arrRetVal.Add(objDestField);
                        }
                    }
                }
            }

            objPlatformIO = null;
            return arrRetVal;
        }

        public List<SyncObject> GetObjects(String containerId, List<SyncField> fields, Boolean includeBinary = false)
        {
            List<SyncObject> arrRetVal = null;
            PlatformIO objPlatformIO = null;

            if (_graphService != null)
            {
                objPlatformIO = new PlatformIO(_graphService);
                Guid listGuid = GeneralHelpers.parseGUID(containerId);
                List<String> arrRequiredFields = null;

                if (fields != null)
                {
                    arrRequiredFields = fields.Select(obj => obj.Key).ToList();
                }

                var arrSrcFiles = objPlatformIO.GetDocuments(listGuid, includeBinary, arrRequiredFields);
                if (arrSrcFiles != null)
                {
                    arrRetVal = new List<SyncObject>();
                    foreach (Dictionary<String, Object> srcFile in arrSrcFiles)
                    {
                        SyncObject objDestFile = new SyncObject();
                        TypeConverters.ConvertSyncObject(srcFile, ref objDestFile, arrRequiredFields);

                        arrRetVal.Add(objDestFile);
                    }
                }
            }

            objPlatformIO = null;
            return arrRetVal;
        }
        public List<SyncObject> GetObjects(List<String> docIds, List<SyncField> fields, Boolean includeBinary = false)
        {
            List<SyncObject> arrRetVal = null;
            PlatformIO objPlatformIO = null;

            if (_graphService != null)
            {
                objPlatformIO = new PlatformIO(_graphService);
                List<String> arrRequiredFields = null;

                if (fields != null)
                {
                    arrRequiredFields = fields.Select(obj => obj.Key).ToList();
                }

                var arrSrcFiles = objPlatformIO.GetDocuments(docIds, includeBinary, arrRequiredFields);
                if (arrSrcFiles != null)
                {
                    arrRetVal = new List<SyncObject>();
                    foreach (Dictionary<String, Object> srcFile in arrSrcFiles)
                    {
                        SyncObject objDestFile = new SyncObject();
                        TypeConverters.ConvertSyncObject(srcFile, ref objDestFile, arrRequiredFields);

                        arrRetVal.Add(objDestFile);
                    }
                }
            }

            objPlatformIO = null;
            return arrRetVal;
        }
        public SyncObject GetObject(String id, List<SyncField> fields, Boolean includeBinary = false)
        {
            SyncObject objRetVal = null;

            List<SyncObject> arrFiles = GetObjects(new List<String>() { id }, fields, includeBinary);
            if (arrFiles != null && arrFiles.Count > 0)
            {
                objRetVal = arrFiles[0];
            }

            return objRetVal;
        }
        public String SaveObject(SyncObject syncObj)
        {
            throw new NotImplementedException("This Integrator does not support writing back to the platform.");
        }
        public Boolean DeleteObjects(List<String> ids)
        {
            throw new NotImplementedException("This Integrator does not support writing back to the platform.");
        }

        public List<SyncTag> GetMetaTagsList()
        {
            List<SyncTag> arrRetVal = null;
            PlatformIO objPlatformIO = null;

            if (_graphService != null)
            {
                objPlatformIO = new PlatformIO(_graphService);
                var arrSrcTermSets = objPlatformIO.GetTermSets(false);
                if (arrSrcTermSets != null)
                {
                    arrRetVal = new List<SyncTag>();
                    foreach (Dictionary<String, Object> srcTag in arrSrcTermSets)
                    {
                        SyncTag objDestTag = new SyncTag();
                        ConvertSyncTag(srcTag, ref objDestTag);
                        arrRetVal.Add(objDestTag);
                    }
                }
            }

            objPlatformIO = null;
            return arrRetVal;
        }
        public SyncTag GetMetaTagTree(String id)
        {
            SyncTag objRetVal = null;
            PlatformIO objPlatformIO = null;

            if (_graphService != null)
            {
                objPlatformIO = new PlatformIO(_graphService);
                Guid termSetGuid = GeneralHelpers.parseGUID(id);
                Dictionary<String, Object> objSrcTermSet = objPlatformIO.GetTermSet(termSetGuid, true);

                if (objSrcTermSet != null)
                {
                    objRetVal = new SyncTag();
                    ConvertSyncTag(objSrcTermSet, ref objRetVal);
                    objRetVal.SyncTags = GetChildMetaTags(objSrcTermSet);
                }
            }

            objPlatformIO = null;
            return objRetVal;
        }
        private List<SyncTag> GetChildMetaTags(Dictionary<String, Object> parent)
        {
            List<SyncTag> arrRetVal = null;

            if (parent != null && parent.ContainsKey("Terms") && parent["Terms"] != null)
            {
                List<Dictionary<String, Object>> arrTerms = (List<Dictionary<String, Object>>)parent["Terms"];

                if (arrTerms != null && arrTerms.Count > 0)
                {
                    arrRetVal = new List<SyncTag>();

                    foreach (Dictionary<String, Object> srcTerm in arrTerms)
                    {
                        SyncTag objDestTag = new SyncTag();
                        ConvertSyncTag(srcTerm, ref objDestTag);

                        objDestTag.SyncTags = GetChildMetaTags(srcTerm);

                        arrRetVal.Add(objDestTag);
                    }
                }
            }

            return arrRetVal;
        }
        private void ConvertSyncTag(Dictionary<String, Object> src, ref SyncTag dest)
        {
            dest.Id = GeneralHelpers.parseString(src["Id"]);
            if (src.ContainsKey("parentId"))
            {
                dest.parentId = GeneralHelpers.parseString(src["parentId"]);
            }
            dest.Name = GeneralHelpers.parseString(src["Name"]);
        }
        public String SaveMetaTag(SyncTag tag, Boolean isRoot, String rootTagId)
        {
            throw new NotImplementedException("This Integrator does not support writing back to the platform.");
        }
        public Boolean DeleteMetaTags(List<String> ids)
        {
            throw new NotImplementedException("This Integrator does not support writing back to the platform.");
        }

        public void RunPostSyncTasks()
        {
        }
    }
}
