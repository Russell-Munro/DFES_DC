using System;
using System.Collections.Generic;

using UDC.Common.Data.Models;
using UDC.Common.Data.Models.Configuration;

namespace UDC.Common.Interfaces
{
    public interface IIntegrator
    {
        Guid IntegratorID { get; set; }
        String Name { get; set; }

        Boolean SupportsRead { get; set; }
        Boolean SupportsWrite { get; set; }

        Boolean SupportsContainers { get; set; }
        Boolean SupportsMetaTags { get; set; }

        PlatformCfg PlatformConfig { get; set; }
        List<Dictionary<String, Object>> Settings { get; set; }

        List<SyncContainer> GetContainers();
        SyncContainer GetContainerTree(String id);
        String SaveContainer(SyncContainer container, Boolean isRoot, String rootContainerId);
        Boolean DeleteContainers(List<String> ids);

        List<SyncField> GetFields(String rootContainerId);

        List<SyncObject> GetObjects(String containerId, List<SyncField> fields, Boolean includeBinary = false);
        List<SyncObject> GetObjects(List<String> ids, List<SyncField> fields, Boolean includeBinary = false);
        SyncObject GetObject(String id, List<SyncField> fields, Boolean includeBinary = false);
        String SaveObject(SyncObject syncObj);
        Boolean DeleteObjects(List<String> ids);

        List<SyncTag> GetMetaTagsList();
        SyncTag GetMetaTagTree(String id);
        String SaveMetaTag(SyncTag tag, Boolean isRoot, String rootTagId);
        Boolean DeleteMetaTags(List<String> ids);

        void RunPostSyncTasks();
    }
}