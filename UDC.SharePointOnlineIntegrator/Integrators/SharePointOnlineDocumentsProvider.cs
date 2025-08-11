using System;
using System.Collections.Generic;
using UDC.Common.Data.Models;
using UDC.Common.Data.Models.Configuration;
using UDC.Common.Interfaces;
using Equ.SharePoint.GraphService;

namespace UDC.SharePointOnlineIntegrator.Integrators
{
    /// <summary>
    /// Placeholder integrator for SharePoint Online document operations using Microsoft Graph.
    /// All integration logic will be implemented in subsequent iterations.
    /// </summary>
    public class SharePointOnlineDocumentsProvider : IIntegrator
    {
        private readonly IGraphService _graphService;

        public SharePointOnlineDocumentsProvider()
        {
            Init();
        }

        public SharePointOnlineDocumentsProvider(IGraphService graphService)
        {
            _graphService = graphService;
            Init();
        }

        private void Init()
        {
            IntegratorID = Guid.NewGuid();
            Name = "SharePoint Online Documents Provider";
            SupportsRead = true;
            SupportsWrite = false;
            SupportsContainers = true;
            SupportsMetaTags = false;
        }

        public Guid IntegratorID { get; set; }
        public string Name { get; set; }
        public bool SupportsRead { get; set; }
        public bool SupportsWrite { get; set; }
        public bool SupportsContainers { get; set; }
        public bool SupportsMetaTags { get; set; }
        public PlatformCfg PlatformConfig { get; set; }
        public List<Dictionary<string, object>> Settings { get; set; }

        public List<SyncContainer> GetContainers() => throw new NotImplementedException();
        public SyncContainer GetContainerTree(string id) => throw new NotImplementedException();
        public string SaveContainer(SyncContainer container, bool isRoot, string rootContainerId) => throw new NotImplementedException();
        public bool DeleteContainers(List<string> ids) => throw new NotImplementedException();
        public List<SyncField> GetFields(string rootContainerId) => throw new NotImplementedException();
        public List<SyncObject> GetObjects(string containerId, List<SyncField> fields, bool includeBinary = false) => throw new NotImplementedException();
        public List<SyncObject> GetObjects(List<string> ids, List<SyncField> fields, bool includeBinary = false) => throw new NotImplementedException();
        public SyncObject GetObject(string id, List<SyncField> fields, bool includeBinary = false) => throw new NotImplementedException();
        public string SaveObject(SyncObject syncObj) => throw new NotImplementedException();
        public bool DeleteObjects(List<string> ids) => throw new NotImplementedException();
        public List<SyncTag> GetMetaTagsList() => throw new NotImplementedException();
        public SyncTag GetMetaTagTree(string id) => throw new NotImplementedException();
        public string SaveMetaTag(SyncTag tag, bool isRoot, string rootTagId) => throw new NotImplementedException();
        public bool DeleteMetaTags(List<string> ids) => throw new NotImplementedException();
        public void RunPostSyncTasks() { }
    }
}
