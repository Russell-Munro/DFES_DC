using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

using UDC.Common;
using UDC.Common.Data.Models;
using UDC.Common.Data.Models.Configuration;
using UDC.Common.Interfaces;
using UDC.SharePointIntegrator.Data;

namespace UDC.SharePointIntegrator.Integrators
{
    public class SharePointDocumentsProvider : IIntegrator
    {
        public Guid IntegratorID { get; set; }
        public String Name { get; set; }

        public Boolean SupportsRead { get; set; }
        public Boolean SupportsWrite { get; set; }

        public Boolean SupportsContainers { get; set; }
        public Boolean SupportsMetaTags { get; set; }

        public PlatformCfg PlatformConfig { get; set; }
        public List<Dictionary<String, Object>> Settings { get; set; }

        public SharePointDocumentsProvider()
        {
            Init();
        }
        public SharePointDocumentsProvider(PlatformCfg cfg)
        {
            Init();

            this.PlatformConfig = cfg;
        }
        private void Init()
        {
            this.IntegratorID = new Guid("fcbd7cf3-36dc-4a8e-97c0-eb9f3b0665cc");
            this.Name = "Documents Provider";

            this.SupportsRead = true;
            this.SupportsWrite = false;

            this.SupportsContainers = true;
            this.SupportsMetaTags = true;
        }

        public List<SyncContainer> GetContainers()
        {
            PlatformIO objPlatformIO = new PlatformIO(this.PlatformConfig);
            List<SyncContainer> arrContainers = null;
            List<Dictionary<String, Object>> arrSrcLists = objPlatformIO.GetLists();

            if (arrSrcLists != null && arrSrcLists.Count > 0)
            {
                arrContainers = new List<SyncContainer>();
                foreach (Dictionary<String, Object> srcList in arrSrcLists)
                {
                    SyncContainer objDestContainer = new SyncContainer();
                    TypeConverters.ConvertSyncContainer(srcList, ref objDestContainer);
                    arrContainers.Add(objDestContainer);
                }
            }

            arrSrcLists = null;
            objPlatformIO = null;

            return arrContainers;
        }
        public SyncContainer GetContainerTree(String rootContainerId)
        {
            PlatformIO objPlatformIO = new PlatformIO(this.PlatformConfig);
            SyncContainer objRootContainer = null;
            Guid listGuid = GeneralHelpers.parseGUID(rootContainerId);
            Dictionary<String, Object> objSrcList = objPlatformIO.GetListTreeStructure(listGuid);

            if(objSrcList != null)
            {
                objRootContainer = new SyncContainer();
                TypeConverters.ConvertSyncContainer(objSrcList, ref objRootContainer);

                objRootContainer.SyncContainers = GetSubContainers(objSrcList);
                objRootContainer.SyncObjects = GetContainerObjects(objSrcList);
            }

            objSrcList = null;
            objPlatformIO = null;

            return objRootContainer;
        }
        private List<SyncContainer> GetSubContainers(Dictionary<String, Object> parent)
        {
            List<SyncContainer> arrRetVal = null;

            if (parent["Folders"] != null)
            {
                List<Dictionary<String, Object>> arrFolders = (List<Dictionary<String, Object>>)parent["Folders"];

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

            if (parent["Documents"] != null)
            {
                List<Dictionary<String, Object>> arrDocs = (List<Dictionary<String, Object>>)parent["Documents"];

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

                arrDocs = null;
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
            PlatformIO objPlatformIO = new PlatformIO(this.PlatformConfig);
            List<SyncField> arrRetVal = null;
            Guid listGuid = GeneralHelpers.parseGUID(rootContainerId);
            Dictionary<String, Object> objSrcList = objPlatformIO.GetList(listGuid);

            if (objSrcList != null)
            {
                if (objSrcList["Fields"] != null)
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

                    arrFields = null;
                }
            }

            objSrcList = null;
            objPlatformIO = null;

            return arrRetVal;
        }

        public List<SyncObject> GetObjects(String containerId, List<SyncField> fields, Boolean includeBinary = false)
        {
            PlatformIO objPlatformIO = new PlatformIO(this.PlatformConfig);
            List<SyncObject> arrRetVal = null;
            Guid listGuid = GeneralHelpers.parseGUID(containerId);
            List<Dictionary<String, Object>> arrSrcFiles = null;

            if(listGuid != Guid.Empty)
            {
                //*** List<String> fields = new List<String>() { "" }; //Need to work out how to handle fields... We'll deal with this once we parse cfg from DBase...
                arrSrcFiles = objPlatformIO.GetDocuments(listGuid, includeBinary, null);
                if(arrSrcFiles != null)
                {
                    arrRetVal = new List<SyncObject>();
                    foreach (Dictionary<String, Object> srcFile in arrSrcFiles)
                    {
                        SyncObject objDestFile = new SyncObject();
                        TypeConverters.ConvertSyncObject(srcFile, ref objDestFile, null);

                        arrRetVal.Add(objDestFile);
                    }
                }
            }

            arrSrcFiles = null;
            objPlatformIO = null;

            return arrRetVal;
        }
        public List<SyncObject> GetObjects(List<String> docIds, List<SyncField> fields, Boolean includeBinary = false)
        {
            PlatformIO objPlatformIO = new PlatformIO(this.PlatformConfig);
            List<SyncObject> arrRetVal = null;
            List<Dictionary<String, Object>> arrSrcFiles = null;

            //*** List<String> fields = new List<String>() { "" }; //Need to work out how to handle fields... We'll deal with this once we parse cfg from DBase...
            arrSrcFiles = objPlatformIO.GetDocuments(docIds, includeBinary, null);
            if (arrSrcFiles != null)
            {
                arrRetVal = new List<SyncObject>();
                foreach (Dictionary<String, Object> srcFile in arrSrcFiles)
                {
                    SyncObject objDestFile = new SyncObject();
                    TypeConverters.ConvertSyncObject(srcFile, ref objDestFile, null);

                    arrRetVal.Add(objDestFile);
                }
            }

            arrSrcFiles = null;
            objPlatformIO = null;

            return arrRetVal;
        }
        public SyncObject GetObject(String id, List<SyncField> fields, Boolean includeBinary = false)
        {
            PlatformIO objPlatformIO = new PlatformIO(this.PlatformConfig);
            SyncObject objRetVal = null;
            Dictionary<String, Object> objSrcFile = null;
            List<String> arrRequiredFields = null;

            if(fields != null)
            {
                arrRequiredFields = fields.Select(obj => obj.Key).ToList();
            }
            objSrcFile = objPlatformIO.GetDocument(id, includeBinary, arrRequiredFields);
            if (objSrcFile != null)
            {
                objRetVal = new SyncObject();
                TypeConverters.ConvertSyncObject(objSrcFile, ref objRetVal, arrRequiredFields);
            }

            arrRequiredFields = null;
            objSrcFile = null;
            objPlatformIO = null;

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
            PlatformIO objPlatformIO = new PlatformIO(this.PlatformConfig);
            List<SyncTag> arrRetVal = null;
            List<Dictionary<String, Object>> arrSrcTermSets = objPlatformIO.GetTermSets(false);

            if (arrSrcTermSets != null && arrSrcTermSets.Count > 0)
            {
                arrRetVal = new List<SyncTag>();
                foreach (Dictionary<String, Object> srcTag in arrSrcTermSets)
                {
                    SyncTag objDestTag = new SyncTag();
                    TypeConverters.ConvertSyncTag(srcTag, ref objDestTag);
                    arrRetVal.Add(objDestTag);
                }
            }

            arrSrcTermSets = null;
            objPlatformIO = null;

            return arrRetVal;
        }
        public SyncTag GetMetaTagTree(String id)
        {
            PlatformIO objPlatformIO = new PlatformIO(this.PlatformConfig);
            SyncTag objRetVal = null;
            Dictionary<String, Object> objSrcTermSet = null;
            Guid termSetGuid = GeneralHelpers.parseGUID(id);

            if(termSetGuid != Guid.Empty)
            {
                objSrcTermSet = objPlatformIO.GetTermSet(termSetGuid, true);
                if(objSrcTermSet != null)
                {
                    objRetVal = new SyncTag();
                    TypeConverters.ConvertSyncTag(objSrcTermSet, ref objRetVal);
                    objRetVal.SyncTags = GetChildMetaTags(objSrcTermSet);
                }
            }

            objSrcTermSet = null;
            objPlatformIO = null;

            return objRetVal;
        }
        private List<SyncTag> GetChildMetaTags(Dictionary<String, Object> parent)
        {
            List<SyncTag> arrRetVal = null;

            if (parent["Terms"] != null)
            {
                List<Dictionary<String, Object>> arrTerms = (List<Dictionary<String, Object>>)parent["Terms"];

                if (arrTerms != null && arrTerms.Count > 0)
                {
                    arrRetVal = new List<SyncTag>();

                    foreach (Dictionary<String, Object> srcTerm in arrTerms)
                    {
                        SyncTag objDestTag = new SyncTag();
                        TypeConverters.ConvertSyncTag(srcTerm, ref objDestTag);

                        objDestTag.SyncTags = GetChildMetaTags(srcTerm);

                        arrRetVal.Add(objDestTag);
                    }
                }

                arrTerms = null;
            }

            return arrRetVal;
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