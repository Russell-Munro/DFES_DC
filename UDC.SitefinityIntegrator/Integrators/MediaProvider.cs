using System;
using System.Collections.Generic;

using UDC.Common.Data.Models;
using UDC.Common.Data.Models.Configuration;
using UDC.Common.Interfaces;
using UDC.SitefinityIntegrator.Data;

namespace UDC.SitefinityIntegrator.Integrators
{
    //Demo / POC Integrator Class... Not actually Implemented...
    public class MediaProvider //: IIntegrator
    {
        public Guid IntegratorID { get; set; }
        public String Name { get; set; }

        public Boolean SupportsRead { get; set; }
        public Boolean SupportsWrite { get; set; }

        public Boolean SupportsContainers { get; set; }
        public Boolean SupportsMetaTags { get; set; }

        public PlatformCfg PlatformConfig { get; set; }
        public List<Dictionary<String, Object>> Settings { get; set; }

        public MediaProvider()
        {
            this.IntegratorID = new Guid("5d4c290c-1c97-4bf1-80ba-a1466027424a");
            this.Name = "Media Provider";

            this.SupportsRead = true;
            this.SupportsWrite = true;

            this.SupportsContainers = true;
            this.SupportsMetaTags = true;
        }

        public List<SyncContainer> GetContainers()
        {
            PlatformIO objPlatformIO = new PlatformIO();

            objPlatformIO = null;

            return new List<SyncContainer>();
        }
        public String SaveContainer(SyncContainer container, Boolean isRoot, String rootContainerId)
        {
            return "";
        }
        public Boolean DeleteContainers(List<String> ids)
        {
            return false;
        }

        public List<SyncField> GetFields(String rootContainerId)
        {
            return new List<SyncField>();
        }

        public List<SyncObject> GetObjects(String containerId, List<SyncField> fields, Boolean includeBinary = false)
        {
            return new List<SyncObject>();
        }
        public List<SyncObject> GetObjects(List<String> ids, List<SyncField> fields, Boolean includeBinary = false)
        {
            return new List<SyncObject>();
        }
        public SyncObject GetObject(String id, List<SyncField> fields, Boolean includeBinary = false)
        {
            return new SyncObject();
        }
        public String SaveObject(SyncObject syncObj)
        {
            return "";
        }
        public Boolean DeleteObjects(List<String> ids)
        {
            return false;
        }

        public List<SyncTag> GetMetaTagsList()
        {
            return new List<SyncTag>();
        }
        public SyncTag GetMetaTagTree(String id)
        {
            return new SyncTag();
        }
        public String SaveMetaTag(SyncTag tag, Boolean isRoot, String rootTagId)
        {
            return "";
        }
        public Boolean DeleteMetaTags(List<String> ids)
        {
            return true;
        }

        public SyncContainer GetContainerTree(String id)
        {
            return new SyncContainer();
        }
        public void RunPostSyncTasks()
        {
        }
    }
}