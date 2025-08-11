using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using UDC.Common;
using UDC.Common.Database.AppState;
using UDC.Common.Interfaces;
using UDC.Common.Data;
using UDC.Common.Data.Models;
using UDC.Common.Database.Data.Models.Database;
using UDC.Common.Data.Models.Configuration;

using UDC.DataConnectorCore.Models;
using static UDC.Common.Constants;
using UDC.Common.Database.Data;
using Microsoft.Extensions.DependencyInjection;

namespace UDC.DataConnectorCore
{
    public class SyncService
    {
        public Boolean TestMode { get; set; }
        public Int32 TestSyncObjectLimit { get; set; }
        public LogVerbosityLevels LogVerbosity { get; set; }

        private Int32 currSyncObjectCount = 0;

        public event EventHandler<SyncStateUpdatedEventArgs> SyncStateUpdated;

        public SyncService()
        {
            this.LogVerbosity = LogVerbosityLevels.ChangesOnly;
            Init();
        }
        public SyncService(LogVerbosityLevels logVerbosity)
        {
            this.LogVerbosity = logVerbosity;
            Init();
        }
        public void Init()
        {
            this.TestMode = GeneralHelpers.parseBool(UDC.Common.AppSettings.GetValue("TestMode"));
            this.TestSyncObjectLimit = GeneralHelpers.parseInt32(UDC.Common.AppSettings.GetValue("TestSyncObjectLimit"));
        }

        public class SyncStateUpdatedEventArgs : EventArgs
        {
            public SyncLogEntry SyncState { get; set; }
        }
        protected virtual void OnSyncStateUpdated(SyncStateUpdatedEventArgs e)
        {
            EventHandler<SyncStateUpdatedEventArgs> handler = SyncStateUpdated;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        private void RaiseSyncStateUpdated(SyncLogEntry syncLogEntry)
        {
            SyncStateUpdatedEventArgs args = new SyncStateUpdatedEventArgs();
            args.SyncState = syncLogEntry;
            OnSyncStateUpdated(args);
        }
        private void AppendSyncLog(SyncLogEntry logEntry, ref List<SyncLogEntry> syncLog)
        {
            syncLog.Add(logEntry);
            RaiseSyncStateUpdated(logEntry);
        }

        public SyncStatus ExecuteRuleSync(Int64 ruleId)
        {
            SyncStatus objSyncStatus = new SyncStatus();
            List<SyncLogEntry> arrSyncLog = new List<SyncLogEntry>();

            this.currSyncObjectCount = 0;

            try
            {
                AppendSyncLog(new SyncLogEntry(LogTypes.Trace, LogActions.Starting, LogResults.Undetermined, "ExecuteRuleSync", "Executing Rule", "Synchronisation Started", null, null), ref arrSyncLog);

                var objDB = ServiceLocator.Instance.GetRequiredService<DatabaseContext>();

                ConnectionRule objCurrRule = objDB.ConnectionRules.Include(obj => obj.Connection).Where(obj => obj.Id == ruleId).FirstOrDefault();

                    if (objCurrRule != null && GeneralHelpers.parseBool(objCurrRule.Enabled))
                    {
                        if (objCurrRule.Connection != null && !String.IsNullOrEmpty(objCurrRule.Connection.SourcePlatformCfg) && !String.IsNullOrEmpty(objCurrRule.Connection.DestinationPlatformCfg))
                        {
                            if(GeneralHelpers.parseBool(objCurrRule.Connection.Enabled))
                            {
                                PlatformCfg objSrcPlatformCfg = JsonConvert.DeserializeObject<PlatformCfg>(objCurrRule.Connection.SourcePlatformCfg);
                                PlatformCfg objDestPlatformCfg = JsonConvert.DeserializeObject<PlatformCfg>(objCurrRule.Connection.DestinationPlatformCfg);
                                
                                if (objSrcPlatformCfg != null && objDestPlatformCfg != null)
                                {
                                    if (!String.IsNullOrEmpty(objCurrRule.SourceContainerCfg) && !String.IsNullOrEmpty(objCurrRule.DestinationContainerCfg))
                                    {
                                        IntegratorCfg objSrcIntegratorCfg = JsonConvert.DeserializeObject<IntegratorCfg>(objCurrRule.SourceContainerCfg);
                                        IntegratorCfg objDestIntegratorCfg = JsonConvert.DeserializeObject<IntegratorCfg>(objCurrRule.DestinationContainerCfg);

                                        if (objSrcIntegratorCfg != null && objDestIntegratorCfg != null)
                                        {
                                            if (!String.IsNullOrEmpty(objCurrRule.FieldMappings))
                                            {
                                                List<SyncFieldMapping> arrFieldMappings = JsonConvert.DeserializeObject<List<SyncFieldMapping>>(objCurrRule.FieldMappings);

                                                IIntegrator objSrcProvider = ProviderHelpers.GetIntegrator(new Guid(objSrcPlatformCfg.IntegratorID), objSrcPlatformCfg);
                                                IIntegrator objDestProvider = ProviderHelpers.GetIntegrator(new Guid(objDestPlatformCfg.IntegratorID), objDestPlatformCfg);

                                                if (objSrcProvider != null && objDestProvider != null)
                                                {
                                                    if (!String.IsNullOrEmpty(objSrcIntegratorCfg.ContainerID) && !String.IsNullOrEmpty(objDestIntegratorCfg.ContainerID))
                                                    {
                                                        Dictionary<StateMappingKeys, List<StateMapping>> arrGlobalStateMapping = null;
                                                        Dictionary<StateMappingKeys, List<StateMapping>> arrStateMapping = null;
                                                        List<SyncField> srcPlatformFields = null;
                                                        List<SyncField> destPlatformFields = null;
                                                        List<SyncFieldMapping> arrTaxonomiesToSync = new List<SyncFieldMapping>();
                                                        Dictionary<String, SyncTag> arrSrcTaxonomyLookupCache = new Dictionary<String, SyncTag>();

                                                        try
                                                        {
                                                            AppendSyncLog(new SyncLogEntry(LogTypes.Trace, LogActions.Running, LogResults.Undetermined, "ExecuteRuleSync", "Getting object fields from source and destination platforms", "", null, null), ref arrSyncLog);
                                                            srcPlatformFields = objSrcProvider.GetFields(objSrcIntegratorCfg.ContainerID);
                                                            destPlatformFields = objDestProvider.GetFields(objDestIntegratorCfg.ContainerID);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            objSyncStatus.Errors += 1;
                                                            AppendSyncLog(new SyncLogEntry(LogTypes.Error, LogActions.Stopped, LogResults.Critical, "ExecuteRuleSync", "Getting object fields from source and destination platforms",
                                                                                                "The target platform returned an error. The sync will now abort.",
                                                                                                ex.Message, ex.StackTrace), ref arrSyncLog);
                                                        }
                                                        if (srcPlatformFields != null && destPlatformFields != null)
                                                        {
                                                            if (!String.IsNullOrEmpty(objCurrRule.SrcDestObjSyncState))
                                                            {
                                                                arrStateMapping = JsonConvert.DeserializeObject<Dictionary<StateMappingKeys, List<StateMapping>>>(objCurrRule.SrcDestObjSyncState);
                                                            }
                                                            else
                                                            {
                                                                arrStateMapping = new Dictionary<StateMappingKeys, List<StateMapping>>();
                                                                arrStateMapping.Add(StateMappingKeys.ContainerMappings, new List<StateMapping>());
                                                                arrStateMapping.Add(StateMappingKeys.ObjectMappings, new List<StateMapping>());
                                                            }
                                                            if (!String.IsNullOrEmpty(objCurrRule.Connection.GlobalSrcDestObjSyncState))
                                                            {
                                                                arrGlobalStateMapping = JsonConvert.DeserializeObject<Dictionary<StateMappingKeys, List<StateMapping>>>(objCurrRule.Connection.GlobalSrcDestObjSyncState);
                                                            }
                                                            else
                                                            {
                                                                arrGlobalStateMapping = new Dictionary<StateMappingKeys, List<StateMapping>>();
                                                                arrGlobalStateMapping.Add(StateMappingKeys.TagMappings, new List<StateMapping>());
                                                            }

                                                            //Resolve SyncField Mappings from originating platforms for extra info and validate Existance...
                                                            if (arrFieldMappings != null && arrFieldMappings.Count > 0)
                                                            {
                                                                AppendSyncLog(new SyncLogEntry(LogTypes.Trace, LogActions.Running, LogResults.Undetermined, "ExecuteRuleSync", "Resolving SyncField Mappings against originating platforms", "", null, null), ref arrSyncLog);
                                                                foreach (SyncFieldMapping objFldMapping in arrFieldMappings)
                                                                {
                                                                    SyncField objSrcField = null;
                                                                    SyncField objDestField = null;

                                                                    if (objFldMapping.SrcField != null)
                                                                    {
                                                                        objSrcField = srcPlatformFields.Where(obj => (!String.IsNullOrEmpty(obj.Id) && obj.Id == objFldMapping.SrcField.Id) || obj.Key.ToLower() == objFldMapping.SrcField.Key.ToLower()).FirstOrDefault();
                                                                    }
                                                                    if (objFldMapping.DestField != null)
                                                                    {
                                                                        objDestField = destPlatformFields.Where(obj => (!String.IsNullOrEmpty(obj.Id) && obj.Id == objFldMapping.DestField.Id) || obj.Key.ToLower() == objFldMapping.DestField.Key.ToLower()).FirstOrDefault();
                                                                    }
                                                                    if (objSrcField != null && objDestField != null)
                                                                    {
                                                                        if (!arrSrcTaxonomyLookupCache.ContainsKey(objSrcField.LinkedLookupId))
                                                                        {
                                                                            SyncTag objSrcRootTag = null;
                                                                            try
                                                                            {
                                                                                AppendSyncLog(new SyncLogEntry(LogTypes.Trace, LogActions.Running, LogResults.Undetermined, "ExecuteRuleSync", "Getting target tag hierarchy from source platform", "", null, null), ref arrSyncLog);
                                                                                objSrcRootTag = objSrcProvider.GetMetaTagTree(objSrcField.LinkedLookupId);
                                                                                arrSrcTaxonomyLookupCache.Add(objSrcField.LinkedLookupId, objSrcRootTag);
                                                                            }
                                                                            catch (Exception ex2)
                                                                            {
                                                                                objSyncStatus.Errors += 1;
                                                                                AppendSyncLog(new SyncLogEntry(LogTypes.Error, LogActions.Running, LogResults.Failed, "ExecuteRuleSync", "Populating tag cache from source platform",
                                                                                                        "The target platform returned an error.",
                                                                                                        ex2.Message, ex2.StackTrace), ref arrSyncLog);
                                                                            }
                                                                            objSrcRootTag = null;
                                                                        }
                                                                        if (objSrcField.FieldDataType == Constants.FieldDataTypes.Taxonomy && objDestField.FieldDataType == Constants.FieldDataTypes.Taxonomy)
                                                                        {
                                                                            arrTaxonomiesToSync.Add(new SyncFieldMapping(objSrcField, objDestField));
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        objSyncStatus.Warnings += 1;
                                                                        AppendSyncLog(new SyncLogEntry(LogTypes.Warning, LogActions.Running, LogResults.Undetermined, "ExecuteRuleSync", "Resolving SyncField Mappings against originating platforms",
                                                                            "Could not find mapping fields in source or destination platforms, check these fields exist in originating platforms. This mapping will be ignored. " + objFldMapping.SrcField.Key + "-> " + objFldMapping.DestField.Key,
                                                                            null, null), ref arrSyncLog);
                                                                    }

                                                                    objFldMapping.SrcField = objSrcField;
                                                                    objFldMapping.DestField = objDestField;

                                                                    objDestField = null;
                                                                    objSrcField = null;
                                                                }
                                                            }

                                                            //Get FileSystem Tree Containers / Objects from Src && Dest Platforms
                                                            SyncContainer srcRootContainer = null;
                                                            SyncContainer destRootContainer = null;

                                                            try
                                                            {
                                                                //Sync The Actual Taxons
                                                                AppendSyncLog(new SyncLogEntry(LogTypes.Trace, LogActions.Running, LogResults.Undetermined, "ExecuteRuleSync", "Syncing mapped tags to destination platform", "", null, null), ref arrSyncLog);
                                                                if (arrTaxonomiesToSync != null && arrTaxonomiesToSync.Count > 0)
                                                                {
                                                                    foreach (SyncFieldMapping objTaxonMapping in arrTaxonomiesToSync)
                                                                    {
                                                                        SyncHierarchichalTagTree(ref objSrcProvider, ref objDestProvider, ref arrGlobalStateMapping, ref arrSrcTaxonomyLookupCache, objTaxonMapping, ref arrSyncLog, ref objSyncStatus);
                                                                    }

                                                                    //Persist state data
                                                                    UpdateStateMappingData(objCurrRule.Id, ref arrGlobalStateMapping, true);
                                                                }

                                                                AppendSyncLog(new SyncLogEntry(LogTypes.Trace, LogActions.Running, LogResults.Undetermined, "ExecuteRuleSync", "Getting container trees from source and destination platforms", "", null, null), ref arrSyncLog);
                                                                srcRootContainer = objSrcProvider.GetContainerTree(objSrcIntegratorCfg.ContainerID);
                                                                destRootContainer = objDestProvider.GetContainerTree(objDestIntegratorCfg.ContainerID);

                                                                if (srcRootContainer != null && destRootContainer != null)
                                                                {
                                                                    //Sync Containers
                                                                    AppendSyncLog(new SyncLogEntry(LogTypes.Trace, LogActions.Running, LogResults.Undetermined, "ExecuteRuleSync", "Syncing containers to destination platform", "", null, null), ref arrSyncLog);
                                                                    SyncHierarchichalContainerTree(ref objSrcProvider, ref objDestProvider, ref srcRootContainer, ref destRootContainer, ref arrStateMapping, ref arrSyncLog, ref objSyncStatus);

                                                                    //Persist state data
                                                                    UpdateStateMappingData(objCurrRule.Id, ref arrStateMapping, false);

                                                                    //Sync Objects
                                                                    Boolean blnMetaDataOnlyOverride = AppStateUtility.GetMetaDataOnlyOverride(ruleId, objDB);
                                                                    AppendSyncLog(new SyncLogEntry(LogTypes.Trace, LogActions.Running, LogResults.Undetermined, "ExecuteRuleSync", "Syncing objects to destination platform", "", null, null), ref arrSyncLog);
                                                                    SyncContainerObjects(objCurrRule.Id, ref objSrcProvider, ref objDestProvider, ref srcRootContainer, ref destRootContainer, ref arrStateMapping, ref arrGlobalStateMapping, ref arrFieldMappings, ref arrSrcTaxonomyLookupCache, ref arrSyncLog, ref objSyncStatus, blnMetaDataOnlyOverride);
                                                                    AppStateUtility.SetMetaDataOnlyOverride(objCurrRule.Id, false);

                                                                    //Persist state data
                                                                    UpdateStateMappingData(objCurrRule.Id, ref arrStateMapping, false);

                                                                    //Run Post Sync Tasks
                                                                    try
                                                                    {
                                                                        AppendSyncLog(new SyncLogEntry(LogTypes.Trace, LogActions.Running, LogResults.Undetermined, "ExecuteRuleSync", "Running post sync tasks on destination platform", "", null, null), ref arrSyncLog);
                                                                        RunPostSyncTasks(ref objCurrRule, ref objSrcPlatformCfg, ref objDestPlatformCfg, ref srcRootContainer, ref destRootContainer, ref arrSyncLog, ref objSyncStatus);
                                                                    }
                                                                    catch(Exception ex)
                                                                    {
                                                                        objSyncStatus.Errors += 1;
                                                                        AppendSyncLog(new SyncLogEntry(LogTypes.Error, LogActions.Running, LogResults.Failed, "ExecuteRuleSync", "Running post sync tasks on destination platform",
                                                                                                            "The target platform returned an error.",
                                                                                                            ex.Message, ex.StackTrace), ref arrSyncLog);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    objSyncStatus.Warnings += 1;
                                                                    AppendSyncLog(new SyncLogEntry(LogTypes.Warning, LogActions.Running, LogResults.Undetermined, "ExecuteRuleSync", "Validating root containers",
                                                                                                    "Could not get Container Tree from either Source or Destination platform with the supplied mapping. Containers and Objects will not be synced. Check rule configuration.",
                                                                                                    null, null), ref arrSyncLog);
                                                                }
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                objSyncStatus.Errors += 1;
                                                                AppendSyncLog(new SyncLogEntry(LogTypes.Error, LogActions.Stopped, LogResults.Critical, "ExecuteRuleSync", "Syncing mapped tags, containers and objects to destination platform",
                                                                                                    "The target platform returned an error.",
                                                                                                    ex.Message, ex.StackTrace), ref arrSyncLog);
                                                            }

                                                            destRootContainer = null;
                                                            srcRootContainer = null;
                                                        }

                                                        arrSrcTaxonomyLookupCache = null;
                                                        arrTaxonomiesToSync = null;
                                                        destPlatformFields = null;
                                                        srcPlatformFields = null;

                                                        //Persist state data
                                                        if (arrStateMapping != null || arrGlobalStateMapping != null)
                                                        {
                                                            objDB.Entry(objCurrRule).Reload();
                                                            if (arrStateMapping != null)
                                                            {
                                                                objCurrRule.SrcDestObjSyncState = JsonConvert.SerializeObject(arrStateMapping);
                                                            }
                                                            if (arrGlobalStateMapping != null)
                                                            {
                                                                objCurrRule.Connection.GlobalSrcDestObjSyncState = JsonConvert.SerializeObject(arrGlobalStateMapping);
                                                            }
                                                            objDB.SaveChanges();
                                                        }

                                                        arrStateMapping = null;
                                                        arrGlobalStateMapping = null;
                                                        objCurrRule = null;
                                                    }
                                                    else
                                                    {
                                                        objSyncStatus.Warnings += 1;
                                                        AppendSyncLog(new SyncLogEntry(LogTypes.Warning, LogActions.Stopped, LogResults.Critical, "ExecuteRuleSync", "Validating Rule Configuration Container ID",
                                                                        "No container set for source or destination platforms. This rule cannot be run. Check rule configuration.",
                                                                        null, null), ref arrSyncLog);
                                                    }
                                                }
                                                else
                                                {
                                                    objSyncStatus.Warnings += 1;
                                                    AppendSyncLog(new SyncLogEntry(LogTypes.Warning, LogActions.Stopped, LogResults.Critical, "ExecuteRuleSync", "Validating Configured Integrators",
                                                                        "Could not resolve an instance of either Source or the Destination Providers by the Ids supplied. This rule cannot be run. Check rule configuration.",
                                                                        null, null), ref arrSyncLog);
                                                }

                                                objDestProvider = null;
                                                objSrcProvider = null;

                                                arrFieldMappings = null;
                                            }
                                            else
                                            {
                                                objSyncStatus.Warnings += 1;
                                                AppendSyncLog(new SyncLogEntry(LogTypes.Warning, LogActions.Stopped, LogResults.Critical, "ExecuteRuleSync", "Validating Field Mappings",
                                                                            "No field mappings set. This rule cannot be run. Check rule configuration.",
                                                                            null, null), ref arrSyncLog);
                                            }
                                        }
                                        else
                                        {
                                            objSyncStatus.Warnings += 1;
                                            AppendSyncLog(new SyncLogEntry(LogTypes.Warning, LogActions.Stopped, LogResults.Critical, "ExecuteRuleSync", "Initialising Integrator Configurations",
                                                                            "Could not initialise Integrator Configuration. Possibly invalid format. This rule cannot be run. Check rule configuration.",
                                                                            null, null), ref arrSyncLog);
                                        }
                                    }
                                    else
                                    {
                                        objSyncStatus.Warnings += 1;
                                        AppendSyncLog(new SyncLogEntry(LogTypes.Warning, LogActions.Stopped, LogResults.Critical, "ExecuteRuleSync", "Validating Source and Dest Container Configurations",
                                                                            "Either SourceContainerCfg or DestContainerCfg has not been set. This rule cannot be run. Check rule configuration.",
                                                                            null, null), ref arrSyncLog);
                                    }
                                }
                                else
                                {
                                    objSyncStatus.Warnings += 1;
                                    AppendSyncLog(new SyncLogEntry(LogTypes.Warning, LogActions.Stopped, LogResults.Critical, "ExecuteRuleSync", "Parsing Source and Destination Connection Information",
                                                                            "Could not parse the configured connection information. Possibly invalid format. This rule cannot be run. Check connection configuration.",
                                                                            null, null), ref arrSyncLog);
                                }

                                objDestPlatformCfg = null;
                                objSrcPlatformCfg = null;
                            }
                            else
                            {
                                objSyncStatus.Warnings += 1;
                                AppendSyncLog(new SyncLogEntry(LogTypes.Warning, LogActions.Stopped, LogResults.Critical, "ExecuteRuleSync", "Validating Connection",
                                                                            "The connection for this rule is disabled. This rule will be run. Check connection configuration.",
                                                                            null, null), ref arrSyncLog);
                            }
                        }
                        else
                        {
                            objSyncStatus.Warnings += 1;
                            AppendSyncLog(new SyncLogEntry(LogTypes.Warning, LogActions.Stopped, LogResults.Critical, "ExecuteRuleSync", "Validating Source and Destination Connection Information",
                                                                        "Either SourcePlatformCfg or DestinationPlatformCfg has not been set. This rule cannot be run. Check connection configuration.",
                                                                        null, null), ref arrSyncLog);
                        }
                    }
                    else
                    {
                        objSyncStatus.Warnings += 1;
                        AppendSyncLog(new SyncLogEntry(LogTypes.Warning, LogActions.Stopped, LogResults.Critical, "ExecuteRuleSync", "Validating Rule",
                                                                            "This rule was either disabled, deleted or no longer exists for some othe reason.",
                                                                            null, null), ref arrSyncLog);
                    }

                    objCurrRule = null;
                
            }
            catch(Exception ex)
            {
                objSyncStatus.Errors += 1;
                AppendSyncLog(new SyncLogEntry(LogTypes.Error, LogActions.Stopped, LogResults.Critical, "ExecuteRuleSync", "Executing Rule",
                                                                        "A critical error occurred while executing this rule. The sync has been aborted.",
                                                                        ex.Message, ex.StackTrace), ref arrSyncLog);
            }
            
            //Calculate Total Sync Time...
            objSyncStatus.SyncTimeElapsed = CalculateTimeElapsed(ref arrSyncLog);

            String strSyncResultMessage = "";
            Int32 intCritical = GetJobStateCount(LogResults.Critical, ref arrSyncLog);
            LogResults objLastResult = GetJobState(ref arrSyncLog);

            if(intCritical > 0)
            {
                strSyncResultMessage = "Synchronisation Failed";
            }
            else if (objSyncStatus.Errors > 0)
            {
                strSyncResultMessage = "Synchronisation Completed with Errors";
            }
            else if(objSyncStatus.Warnings > 0)
            {
                strSyncResultMessage = "Synchronisation Completed with Warnings";
            }
            else
            {
                strSyncResultMessage = "Synchronisation Completed";
            }

            objSyncStatus.ExecutionStatus = strSyncResultMessage;
            try
            {
                UpdateLastExecutionStatus(ruleId, objSyncStatus);
            }
            catch (Exception ex) { }

            AppendSyncLog(new SyncLogEntry(LogTypes.Trace, LogActions.Stopped, objLastResult, "ExecuteRuleSync", "Executing Rule", strSyncResultMessage, null, JsonConvert.SerializeObject(objSyncStatus)), ref arrSyncLog);
            
            objSyncStatus.SyncLog = arrSyncLog;

            return objSyncStatus;
        }
        public String TestSystemConnectivity(Int64 ruleId)
        {
            String retVal = "";

            var objDB = ServiceLocator.Instance.GetRequiredService<DatabaseContext>();
            ConnectionRule objCurrRule = objDB.ConnectionRules.Include(obj => obj.Connection).Where(obj => obj.Id == ruleId).FirstOrDefault();

                if(objCurrRule != null)
                {
                    PlatformCfg objSrcPlatformCfg = JsonConvert.DeserializeObject<PlatformCfg>(objCurrRule.Connection.SourcePlatformCfg);
                    PlatformCfg objDestPlatformCfg = JsonConvert.DeserializeObject<PlatformCfg>(objCurrRule.Connection.DestinationPlatformCfg);
                    IntegratorCfg objSrcIntegratorCfg = JsonConvert.DeserializeObject<IntegratorCfg>(objCurrRule.SourceContainerCfg);
                    IntegratorCfg objDestIntegratorCfg = JsonConvert.DeserializeObject<IntegratorCfg>(objCurrRule.DestinationContainerCfg);

                    IIntegrator objSrcProvider = ProviderHelpers.GetIntegrator(new Guid(objSrcPlatformCfg.IntegratorID), objSrcPlatformCfg);
                    IIntegrator objDestProvider = ProviderHelpers.GetIntegrator(new Guid(objDestPlatformCfg.IntegratorID), objDestPlatformCfg);

                    List<SyncTag> arrSrcTags = null;
                    List<SyncTag> arrDestTags = null;

                    //Get FileSystem Tree Containers / Objects from Src && Dest Platforms
                    SyncContainer srcRootContainer = null;
                    SyncContainer destRootContainer = null;

                    retVal += "Fetching tags from source system...\n";
                    try
                    {
                        arrSrcTags = objSrcProvider.GetMetaTagsList();
                        if(arrSrcTags != null && arrSrcTags.Count > 0)
                        {
                            retVal += "Tags fetched from source system: " + arrSrcTags.Count + "\n";
                        }
                        else
                        {
                            retVal += "Didn't seem to get any tags from the source system...\n";
                        }
                    }
                    catch (Exception ex)
                    {
                        retVal += "Splat! " + ex.Message + "\n";
                    }
                    retVal += "Fetching tags from dest system...\n";
                    try
                    {
                        arrDestTags = objDestProvider.GetMetaTagsList();
                        if (arrDestTags != null && arrDestTags.Count > 0)
                        {
                            retVal += "Tags fetched from dest system: " + arrDestTags.Count + "\n";
                        }
                        else
                        {
                            retVal += "Didn't seem to get any tags from the dest system...\n";
                        }
                    }
                    catch (Exception ex)
                    {
                        retVal += "Splat! " + ex.Message + "\n";
                    }

                    retVal += "Fetching container tree from source system...\n";
                    try
                    {
                        srcRootContainer = objSrcProvider.GetContainerTree(objSrcIntegratorCfg.ContainerID);
                        if (srcRootContainer != null)
                        {
                            retVal += "Container fetched from source system: " + srcRootContainer.Id + "\n";
                        }
                        else
                        {
                            retVal += "Didn't seem to get any container data from the source system...\n";
                        }
                    }
                    catch (Exception ex)
                    {
                        retVal += "Splat! " + ex.Message + "\n";
                    }
                    retVal += "Fetching container tree from dest system...\n";
                    try
                    {
                        destRootContainer = objDestProvider.GetContainerTree(objDestIntegratorCfg.ContainerID);
                        if (destRootContainer != null)
                        {
                            retVal += "Container tree fetched from dest system: " + destRootContainer.Id + "\n";
                        }
                        else
                        {
                            retVal += "Didn't seem to get any container data from the dest system...\n";
                        }
                    }
                    catch (Exception ex)
                    {
                        retVal += "Splat! " + ex.Message + "\n";
                    }

                    srcRootContainer = null;
                    destRootContainer = null;

                    arrDestTags = null;
                    arrSrcTags = null;

                    objDestProvider = null;
                    objSrcProvider = null;

                    objDestIntegratorCfg = null;
                    objSrcIntegratorCfg = null;
                    objDestPlatformCfg = null;
                    objSrcPlatformCfg = null;
                }
                else
                {
                    retVal += "No Such Rule...";
                }

                objCurrRule = null;
            

            return retVal;
        }

        private LogResults GetJobState(ref List<SyncLogEntry> syncLog)
        {
            LogResults retVal = LogResults.Undetermined;
            SyncLogEntry objLast = syncLog.LastOrDefault();

            retVal = objLast.LogResult;

            objLast = null;

            return retVal;
        }
        private Int32 GetJobStateCount(LogResults logResult, ref List<SyncLogEntry> syncLog)
        {
            Int32 retVal = 0;
            retVal = syncLog.Where(obj => obj.LogResult == logResult).Count();
            return retVal;
        }
        private TimeSpan CalculateTimeElapsed(ref List<SyncLogEntry> syncLog)
        {
            TimeSpan retVal = TimeSpan.FromSeconds(0);
            SyncLogEntry objFirst = syncLog.FirstOrDefault();
            SyncLogEntry objLast = syncLog.LastOrDefault();

            if (objFirst != null && objLast != null)
            {
                retVal = objLast.TimeStamp.Subtract(objFirst.TimeStamp);
            }

            objLast = null;
            objFirst = null;

            return retVal;
        }
        private void UpdateLastExecutionStatus(Int64 ruleId, SyncStatus syncStatus)
        {
            if(ruleId > 0)
            {
                var objDB = ServiceLocator.Instance.GetRequiredService<DatabaseContext>();
                ConnectionRule objCurrRule = objDB.ConnectionRules.Include(obj => obj.Connection).Where(obj => obj.Id == ruleId).FirstOrDefault();

                if(objCurrRule != null)
                {
                    objCurrRule.LastExecutedStatus = JsonConvert.SerializeObject(syncStatus);
                    objCurrRule.LastExecuted = DateTime.UtcNow;
                    objCurrRule.LastExecuted = DateTime.UtcNow;
                    objDB.SaveChanges();
                }
                objCurrRule = null;
                
            }
        }
        private void UpdateStateMappingData(Int64 ruleId, ref Dictionary<StateMappingKeys, List<StateMapping>> stateMappings, Boolean isGlobal)
        {
            if (ruleId > 0)
            {
                var objDB = ServiceLocator.Instance.GetRequiredService<DatabaseContext>();

                ConnectionRule objCurrRule = objDB.ConnectionRules.Include(obj => obj.Connection).Where(obj => obj.Id == ruleId).FirstOrDefault();

                if (objCurrRule != null && stateMappings != null)
                {
                    if(!isGlobal)
                    {
                        objCurrRule.SrcDestObjSyncState = JsonConvert.SerializeObject(stateMappings);
                    }
                    else
                    {
                        objCurrRule.Connection.GlobalSrcDestObjSyncState = JsonConvert.SerializeObject(stateMappings);
                    }
                    objDB.SaveChanges();
                }
                objCurrRule = null;
            }
        }

        #region SyncTags
        private void SyncHierarchichalTagTree(ref IIntegrator srcProvider, ref IIntegrator destProvider, ref Dictionary<StateMappingKeys, List<StateMapping>> globalStateMapping, ref Dictionary<String, SyncTag> srcTaxonomyLookupCache, SyncFieldMapping mapping, ref List<SyncLogEntry> syncLog, ref SyncStatus syncStatus)
        {
            SyncTag objSrcRootTag = null;
            SyncTag objDestRootTag = destProvider.GetMetaTagTree(mapping.DestField.LinkedLookupId);
            List<String> arrDestDeletions = new List<String>();

            if (srcTaxonomyLookupCache.ContainsKey(mapping.SrcField.LinkedLookupId))
            {
                //Get from cache...
                objSrcRootTag = srcTaxonomyLookupCache[mapping.SrcField.LinkedLookupId];
            }
            else
            {
                //Try get from Src...
                objSrcRootTag = srcProvider.GetMetaTagTree(mapping.SrcField.LinkedLookupId);
            }
            if (objSrcRootTag != null && objDestRootTag != null)
            {
                List<String> arrSrcTagIds = new List<String>();
                Int32 intTotalTags = 0;
                Int32 intCurrTag = 0;

                GetRecursiveTagIDs(objDestRootTag, ref arrDestDeletions);
                GetRecursiveTagIDs(objSrcRootTag, ref arrSrcTagIds);
                intTotalTags = arrSrcTagIds.Count;
                SyncSubTags(ref destProvider, objSrcRootTag, objDestRootTag, true, objDestRootTag.Id, ref globalStateMapping, ref arrDestDeletions, ref syncLog, ref syncStatus, ref intTotalTags, ref intCurrTag);

                AppendSyncLog(new SyncLogEntry(LogTypes.Trace, LogActions.Running, LogResults.Undetermined, "SyncHierarchichalTagTree", "Deleting redundant tags in destination platform",
                                                                        arrDestDeletions.Count + " tags will be deleted in destination platform. Check data for specific Ids.",
                                                                        null, JsonConvert.SerializeObject(arrDestDeletions)), ref syncLog);
                syncStatus.TagsDeleted += arrDestDeletions.Count;

                if (arrDestDeletions.Count > 0)
                {
                    try
                    {
                        //Delete from Dest Platform...
                        destProvider.DeleteMetaTags(arrDestDeletions);
                    }
                    catch(Exception ex)
                    {
                        syncStatus.Errors += 1;
                        AppendSyncLog(new SyncLogEntry(LogTypes.Error, LogActions.Running, LogResults.Failed, "SyncHierarchichalTagTree", "Deleting redundant tags in destination platform",
                                                                        "The destination platform returned an error.",
                                                                        ex.Message, ex.StackTrace), ref syncLog);
                    }
                    
                    //Tidy Up State Management Obj
                    List<StateMapping> arrOldMappings = globalStateMapping[StateMappingKeys.TagMappings].Where(obj => arrDestDeletions.Contains(obj.DestId)).ToList();
                    if(arrOldMappings != null)
                    {
                        foreach(StateMapping oldMapping in arrOldMappings)
                        {
                            globalStateMapping[StateMappingKeys.TagMappings].Remove(oldMapping);
                        }
                    }
                    arrOldMappings = null;
                }

                arrSrcTagIds = null;
            }
            else
            {
                syncStatus.Warnings += 1;
                AppendSyncLog(new SyncLogEntry(LogTypes.Warning, LogActions.Running, LogResults.Undetermined, "SyncHierarchichalTagTree", "Validating root tags",
                                                                        "Could not get Tag Tree from either Source or Destination platform with the supplied mapping. Skipping...",
                                                                        null, null), ref syncLog);
            }

            arrDestDeletions = null;
            objDestRootTag = null;
            objSrcRootTag = null;
        }
        private void SyncSubTags(ref IIntegrator destProvider, SyncTag parentSrcTag, SyncTag parentDestTag, Boolean isRootTag, String rootDestTagId, ref Dictionary<StateMappingKeys, List<StateMapping>> globalStateMapping, ref List<String> destDeletions, ref List<SyncLogEntry> syncLog, ref SyncStatus syncStatus, ref Int32 totalTags, ref Int32 currTag)
        {
            if (parentSrcTag.SyncTags != null && parentSrcTag.SyncTags.Count > 0)
            {
                foreach (SyncTag childSrcTag in parentSrcTag.SyncTags)
                {
                    Boolean requiresSave = false;
                    SyncTag destTag = null;
                    StateMapping objMapping = globalStateMapping[StateMappingKeys.TagMappings].Where(obj => obj.SrcId == childSrcTag.Id).FirstOrDefault();
                    EntityStates objEntityState = EntityStates.None;

                    currTag++;

                    if (objMapping != null && parentDestTag.SyncTags != null)
                    {
                        //We have a state mapping... Check if the destination actually exists... Use that with existing ID...
                        destTag = parentDestTag.SyncTags.Where(obj => obj.Id == objMapping.DestId).FirstOrDefault();
                    }
                    if(destTag == null)
                    {
                        //No existing mapping or Tag deleted in destination...
                        destTag = new SyncTag();
                        objEntityState = EntityStates.New;
                        requiresSave = true;
                    }
                    else
                    {
                        requiresSave = CompareSyncTags(childSrcTag, destTag);
                        objEntityState = (requiresSave ? EntityStates.Changed : EntityStates.Existing);
                    }
                    if(requiresSave)
                    {
                        if (!isRootTag)
                        {
                            destTag.parentId = parentDestTag.Id;
                        }
                        destTag.Name = childSrcTag.Name;
                        try
                        {
                            //Save in Dest Platform...
                            destTag.Id = destProvider.SaveMetaTag(destTag, false, rootDestTagId);
                            switch (objEntityState)
                            {
                                case EntityStates.New:
                                    syncStatus.TagsCreated += 1;
                                    break;
                                case EntityStates.Existing:
                                    syncStatus.TagsUpdated += 1;
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            syncStatus.Errors += 1;
                            syncStatus.TagsSkipped += 1;
                            AppendSyncLog(new SyncLogEntry(LogTypes.Error, LogActions.Running, LogResults.Failed, "SyncSubTags", "Saving tag in destination platform",
                                                                            "The destination platform returned an error.",
                                                                            ex.Message, ex.StackTrace), ref syncLog);
                        }
                    }
                    else
                    {
                        syncStatus.TagsSkipped += 1;
                    }
                    if (this.LogVerbosity == LogVerbosityLevels.All || objEntityState == EntityStates.Changed)
                    {
                        AppendSyncLog(new SyncLogEntry(LogTypes.Trace, LogActions.Running, LogResults.Undetermined, "SyncSubTags", "Saving tag " + currTag.ToString() + " of " + totalTags.ToString() + " in destination platform",
                                                                        objEntityState.ToString() + " destTag.Id: " + destTag.Id + " (" + (requiresSave ? "'" + destTag.Name + "' Vs '" + childSrcTag.Name + "'" : "") + ")",
                                                                        null, JsonConvert.SerializeObject(new Dictionary<String, Int32>() { { "Current", currTag }, { "Total", totalTags } })), ref syncLog);
                    }

                    //Maintain StateMapping...
                    if (objMapping != null)
                    {
                        globalStateMapping[StateMappingKeys.TagMappings].Remove(objMapping);
                    }
                    if (objMapping == null)
                    {
                        objMapping = new StateMapping();
                    }
                    objMapping.SrcId = childSrcTag.Id;
                    objMapping.DestId = destTag.Id;
                    objMapping.SrcLabel = childSrcTag.Name;
                    objMapping.DestLabel = destTag.Name;
                    globalStateMapping[StateMappingKeys.TagMappings].Add(objMapping);

                    destDeletions.Remove(destTag.Id);

                    //Recursive call...
                    SyncSubTags(ref destProvider, childSrcTag, destTag, false, rootDestTagId, ref globalStateMapping, ref destDeletions, ref syncLog, ref syncStatus, ref totalTags, ref currTag);

                    objMapping = null;
                    destTag = null;
                }
            }
        }
        private void GetRecursiveTagIDs(SyncTag parentTag, ref List<String> idList)
        {
            if (parentTag.SyncTags != null && parentTag.SyncTags.Count > 0)
            {
                foreach (SyncTag childTag in parentTag.SyncTags)
                {
                    idList.Add(childTag.Id);
                    GetRecursiveTagIDs(childTag, ref idList);
                }
            }
        }
        private void GetRecursiveTagLabelLookups(SyncTag parentTag, ref Dictionary<String, String> labelLookupList)
        {
            if (parentTag.SyncTags != null && parentTag.SyncTags.Count > 0)
            {
                foreach (SyncTag childTag in parentTag.SyncTags)
                {
                    labelLookupList.Add(childTag.Id, childTag.Name);
                    GetRecursiveTagLabelLookups(childTag, ref labelLookupList);
                }
            }
        }
        private Boolean CompareSyncTags(SyncTag srcTag, SyncTag destTag)
        {
            Boolean retVal = false;

            if (srcTag.Name.Replace(" ","").ToLower() != destTag.Name.Replace(" ", "").ToLower())
            {
                retVal = true;
            }
            
            return retVal;
        }
        #endregion

        #region SyncContainers
        private void SyncHierarchichalContainerTree(ref IIntegrator srcProvider, ref IIntegrator destProvider, ref SyncContainer srcRootContainer, ref SyncContainer destRootContainer, ref Dictionary<StateMappingKeys, List<StateMapping>> stateMapping, ref List<SyncLogEntry> syncLog, ref SyncStatus syncStatus)
        {
            List<String> arrDestDeletions = new List<String>();

            if (srcRootContainer != null && destRootContainer != null)
            {
                List<String> arrSrcContainerIds = new List<String>();
                Int32 intTotalContainers = 0;
                Int32 intCurrContainer = 0;

                GetRecursiveContainerIDs(destRootContainer, ref arrDestDeletions);
                GetRecursiveContainerIDs(srcRootContainer, ref arrSrcContainerIds);
                intTotalContainers = arrSrcContainerIds.Count;
                SyncSubContainers(ref destProvider, srcRootContainer, destRootContainer, true, destRootContainer.Id, ref stateMapping, ref arrDestDeletions, ref syncLog, ref syncStatus, ref intTotalContainers, ref intCurrContainer);

                AppendSyncLog(new SyncLogEntry(LogTypes.Trace, LogActions.Running, LogResults.Undetermined, "SyncHierarchichalContainerTree", "Deleting redundant containers in destination platform",
                                                                        arrDestDeletions.Count + " containers will be deleted in destination platform. Check data for specific Ids.",
                                                                        null, JsonConvert.SerializeObject(arrDestDeletions)), ref syncLog);
                syncStatus.ContainersDeleted += arrDestDeletions.Count;
                if (arrDestDeletions.Count > 0)
                {
                    try
                    {
                        //Delete from Dest Platform...
                        destProvider.DeleteContainers(arrDestDeletions);
                    }
                    catch (Exception ex)
                    {
                        syncStatus.Errors += 1;
                        AppendSyncLog(new SyncLogEntry(LogTypes.Error, LogActions.Running, LogResults.Failed, "SyncHierarchichalContainerTree", "Deleting redundant containers in destination platform",
                                                                        "The destination platform returned an error.",
                                                                        ex.Message, ex.StackTrace), ref syncLog);
                    }

                    //Tidy Up State Management Obj
                    List<StateMapping> arrOldMappings = stateMapping[StateMappingKeys.ContainerMappings].Where(obj => arrDestDeletions.Contains(obj.DestId)).ToList();
                    if (arrOldMappings != null)
                    {
                        foreach (StateMapping oldMapping in arrOldMappings)
                        {
                            stateMapping[StateMappingKeys.ContainerMappings].Remove(oldMapping);
                        }
                    }
                    arrOldMappings = null;
                }

                arrSrcContainerIds = null;
            }

            arrDestDeletions = null;
        }
        private void SyncSubContainers(ref IIntegrator destProvider, SyncContainer parentSrcContainer, SyncContainer parentDestContainer, Boolean isRootContainer, String rootDestContainerId, ref Dictionary<StateMappingKeys, List<StateMapping>> stateMapping, ref List<String> destDeletions, ref List<SyncLogEntry> syncLog, ref SyncStatus syncStatus, ref Int32 totalContainers, ref Int32 currContainer)
        {
            if (parentSrcContainer.SyncContainers != null && parentSrcContainer.SyncContainers.Count > 0)
            {
                foreach (SyncContainer childSrcContainer in parentSrcContainer.SyncContainers)
                {
                    Boolean requiresSave = false;
                    SyncContainer destContainer = null;
                    StateMapping objMapping = stateMapping[StateMappingKeys.ContainerMappings].Where(obj => obj.SrcId == childSrcContainer.Id).FirstOrDefault();
                    EntityStates objEntityState = EntityStates.None;

                    currContainer++;

                    if (objMapping != null && parentDestContainer.SyncContainers != null)
                    {
                        //We have a state mapping... Check if the destination actually exists... Use that with existing ID...
                        destContainer = parentDestContainer.SyncContainers.Where(obj => obj.Id == objMapping.DestId).FirstOrDefault();
                    }
                    if (destContainer == null)
                    {
                        //No existing mapping or Container deleted in destination...
                        destContainer = new SyncContainer();
                        objEntityState = EntityStates.New;
                        requiresSave = true;
                    }
                    else
                    {
                        requiresSave = CompareSyncContainers(childSrcContainer, destContainer);
                        objEntityState = (requiresSave ? EntityStates.Changed : EntityStates.Existing);
                    }
                    if(requiresSave)
                    {
                        if (!isRootContainer)
                        {
                            destContainer.parentId = parentDestContainer.Id;
                        }
                        destContainer.Name = childSrcContainer.Name;
                        try
                        {
                            //Save in Dest Platform...
                            destContainer.Id = destProvider.SaveContainer(destContainer, false, rootDestContainerId);
                            switch (objEntityState)
                            {
                                case EntityStates.New:
                                    syncStatus.ContainersCreated += 1;
                                    break;
                                case EntityStates.Existing:
                                    syncStatus.ContainersUpdated += 1;
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            syncStatus.Errors += 1;
                            syncStatus.ContainersSkipped += 1;
                            AppendSyncLog(new SyncLogEntry(LogTypes.Error, LogActions.Running, LogResults.Failed, "SyncSubContainers", "Saving container in destination platform",
                                                                            "The destination platform returned an error.",
                                                                            ex.Message, ex.StackTrace), ref syncLog);
                        }
                    }
                    else
                    {
                        syncStatus.ContainersSkipped += 1;
                    }
                    if (this.LogVerbosity == LogVerbosityLevels.All || objEntityState == EntityStates.Changed)
                    {
                        AppendSyncLog(new SyncLogEntry(LogTypes.Trace, LogActions.Running, LogResults.Undetermined, "SyncSubContainers", "Saving container " + currContainer.ToString() + " of " + totalContainers.ToString() + " in destination platform",
                                                                        objEntityState.ToString() + " destContainer.Id: " + destContainer.Id + " (" + (requiresSave ? "'" + destContainer.Name + "' Vs '" + childSrcContainer.Name + "'" : "") + ")",
                                                                        null, JsonConvert.SerializeObject(new Dictionary<String, Int32>() { { "Current", currContainer }, { "Total", totalContainers } })), ref syncLog);
                    }

                    //Maintain StateMapping...
                    if (objMapping != null)
                    {
                        stateMapping[StateMappingKeys.ContainerMappings].Remove(objMapping);
                    }
                    if (objMapping == null)
                    {
                        objMapping = new StateMapping();
                    }
                    objMapping.SrcId = childSrcContainer.Id;
                    objMapping.DestId = destContainer.Id;
                    stateMapping[StateMappingKeys.ContainerMappings].Add(objMapping);

                    destDeletions.Remove(destContainer.Id);

                    //Recursive call...
                    SyncSubContainers(ref destProvider, childSrcContainer, destContainer, false, rootDestContainerId, ref stateMapping, ref destDeletions, ref syncLog, ref syncStatus, ref totalContainers, ref currContainer);

                    objMapping = null;
                    destContainer = null;
                }
            }
        }
        private void GetRecursiveContainerIDs(SyncContainer parentContainer, ref List<String> idList)
        {
            if (parentContainer.SyncContainers != null && parentContainer.SyncContainers.Count > 0)
            {
                foreach (SyncContainer childContainer in parentContainer.SyncContainers)
                {
                    idList.Add(childContainer.Id);
                    GetRecursiveContainerIDs(childContainer, ref idList);
                }
            }
        }
        private String GenerateContainerPath(SyncContainer targetContainer)
        {
            String retVal = "";
            SyncContainer tmpContainer = targetContainer;

            while (tmpContainer != null)
            {
                retVal += tmpContainer.Name + "\\";
                tmpContainer = tmpContainer.Parent;
            }

            return retVal;
        }
        private Boolean CompareSyncContainers(SyncContainer srcContainer, SyncContainer destContainer)
        {
            Boolean retVal = false;

            if (srcContainer.Name != destContainer.Name)
            {
                retVal = true;
            }
            
            return retVal;
        }
        #endregion

        #region SyncObjects
        private void SyncContainerObjects(Int64 ruleId, ref IIntegrator srcProvider, ref IIntegrator destProvider, ref SyncContainer srcRootContainer, ref SyncContainer destRootContainer, ref Dictionary<StateMappingKeys, List<StateMapping>> stateMapping, ref Dictionary<StateMappingKeys, List<StateMapping>> globalStateMapping, ref List<SyncFieldMapping> fieldMappings, ref Dictionary<String, SyncTag> srcTaxonomyLookupCache, ref List<SyncLogEntry> syncLog, ref SyncStatus syncStatus, Boolean metaDataOnlyOverride)
        {
            List<String> arrDestDeletions = new List<String>();

            if (srcRootContainer != null && destRootContainer != null)
            {
                List<String> arrSrcObjectIds = new List<String>();
                Int32 intTotalObjects = 0;
                Int32 intCurrObject = 0;
                
                try
                {
                    //Get fresh copy of container tree for object sync...
                    AppendSyncLog(new SyncLogEntry(LogTypes.Trace, LogActions.Running, LogResults.Undetermined, "SyncContainerObjects", "Getting updated root container from destination platform", "", null, null), ref syncLog);
                    destRootContainer = destProvider.GetContainerTree(destRootContainer.Id);
                }
                catch (Exception ex)
                {
                    syncStatus.Errors += 1;
                    AppendSyncLog(new SyncLogEntry(LogTypes.Error, LogActions.Running, LogResults.Failed, "SyncContainerObjects", "Getting updated root container from destination platform",
                                                                    "The destination platform returned an error.",
                                                                    ex.Message, ex.StackTrace), ref syncLog);
                }

                GetRecursiveObjectIDs(destRootContainer, ref arrDestDeletions);
                GetRecursiveObjectIDs(srcRootContainer, ref arrSrcObjectIds);
                intTotalObjects = arrSrcObjectIds.Count;

                SyncSubContainerObjects(ruleId, ref srcProvider, ref destProvider, srcRootContainer, destRootContainer, true, destRootContainer.Id, ref stateMapping, ref globalStateMapping, ref arrDestDeletions, ref fieldMappings, ref srcTaxonomyLookupCache, ref syncLog, ref syncStatus, ref intTotalObjects, ref intCurrObject, metaDataOnlyOverride);

                AppendSyncLog(new SyncLogEntry(LogTypes.Trace, LogActions.Running, LogResults.Undetermined, "SyncContainerObjects", "Deleting redundant objects in destination platform",
                                                                        arrDestDeletions.Count + " objects will be deleted in destination platform. Check data for specific Ids.",
                                                                        null, JsonConvert.SerializeObject(arrDestDeletions)), ref syncLog);
                syncStatus.ObjectsDeleted += arrDestDeletions.Count;
                if (arrDestDeletions.Count > 0)
                {
                    try
                    {
                        //Delete from Dest Platform...
                        destProvider.DeleteObjects(arrDestDeletions);
                    }
                    catch (Exception ex)
                    {
                        syncStatus.Errors += 1;
                        AppendSyncLog(new SyncLogEntry(LogTypes.Error, LogActions.Running, LogResults.Failed, "SyncContainerObjects", "Deleting redundant objects in destination platform",
                                                                        "The destination platform returned an error.",
                                                                        ex.Message, ex.StackTrace), ref syncLog);
                    }
                    
                    //Tidy Up State Management Obj
                    List<StateMapping> arrOldMappings = stateMapping[StateMappingKeys.ObjectMappings].Where(obj => arrDestDeletions.Contains(obj.DestId)).ToList();
                    if (arrOldMappings != null)
                    {
                        foreach (StateMapping oldMapping in arrOldMappings)
                        {
                            stateMapping[StateMappingKeys.ObjectMappings].Remove(oldMapping);
                        }
                    }
                    arrOldMappings = null;
                }
                arrSrcObjectIds = null;
            }

            arrDestDeletions = null;
        }
        private void SyncSubContainerObjects(Int64 ruleId, ref IIntegrator srcProvider, ref IIntegrator destProvider, SyncContainer parentSrcContainer, SyncContainer parentDestContainer, Boolean isRootContainer, String rootDestContainerId, ref Dictionary<StateMappingKeys, List<StateMapping>> stateMapping, ref Dictionary<StateMappingKeys, List<StateMapping>> globalStateMapping, ref List<String> destDeletions, ref List<SyncFieldMapping> fieldMappings, ref Dictionary<String, SyncTag> srcTaxonomyLookupCache, ref List<SyncLogEntry> syncLog, ref SyncStatus syncStatus, ref Int32 totalObjects, ref Int32 currObject, Boolean metaDataOnlyOverride)
        {
            if (parentSrcContainer.SyncObjects != null && parentSrcContainer.SyncObjects.Count > 0)
            {
                //Pawn SyncObjects to Sync...
                foreach(SyncObject childSrcObject in parentSrcContainer.SyncObjects)
                {
                    Boolean requiresSave = false;
                    SyncObject destObject = null;
                    StateMapping objMapping = stateMapping[StateMappingKeys.ObjectMappings].Where(obj => obj.SrcId == childSrcObject.Id).FirstOrDefault();
                    EntityStates objEntityState = EntityStates.None;

                    currObject++;

                    if (objMapping != null && parentDestContainer.SyncObjects != null)
                    {
                        //We have a state mapping... Check if the destination actually exists... Use that with existing ID...
                        destObject = parentDestContainer.SyncObjects.Where(obj => obj.Id == objMapping.DestId).FirstOrDefault();
                    }
                    if (destObject == null)
                    {
                        //No existing mapping or Object deleted in destination...
                        destObject = new SyncObject();
                        objEntityState = EntityStates.New;
                        requiresSave = true;
                    }
                    else
                    {
                        requiresSave = CompareSyncObjects(childSrcObject, destObject);
                        objEntityState = (requiresSave ? EntityStates.Changed : EntityStates.Existing);
                    }
                    if (requiresSave || metaDataOnlyOverride)
                    {
                        List<SyncField> srcSyncFields = fieldMappings.Where(obj => obj.SrcField != null).Select(obj => obj.SrcField).ToList();
                        SyncObject objFullSrcObj = null;

                        try
                        {
                            objFullSrcObj = srcProvider.GetObject(childSrcObject.Id, srcSyncFields, !this.TestMode);
                        }
                        catch(Exception ex)
                        {
                            syncStatus.Errors += 1;
                            syncStatus.ObjectsSkipped += 1;
                            String strDesc = GenerateContainerPath(parentSrcContainer) + childSrcObject.FileName;
                            AppendSyncLog(new SyncLogEntry(LogTypes.Error, LogActions.Running, LogResults.Failed, "SyncSubContainerObjects", "Fetching full object from source platform [" + strDesc + "]",
                                                                            "The source platform returned an error.",
                                                                            ex.Message, ex.StackTrace), ref syncLog);
                        }
                        if (objFullSrcObj != null)
                        {
                            if (!isRootContainer)
                            {
                                destObject.containerId = parentDestContainer.Id;
                            }
                            destObject.rootContainerId = rootDestContainerId;

                            destObject.Name = (!String.IsNullOrEmpty(objFullSrcObj.Name) ? objFullSrcObj.Name : objFullSrcObj.Title);
                            destObject.Title = (!String.IsNullOrEmpty(objFullSrcObj.Title) ? objFullSrcObj.Title : objFullSrcObj.Name);
                            destObject.FileName = objFullSrcObj.FileName;
                            destObject.DateCreated = objFullSrcObj.DateCreated;
                            destObject.LastUpdated = objFullSrcObj.LastUpdated;

                            if (fieldMappings != null && objFullSrcObj.Properties != null)
                            {
                                Dictionary<String, Object> arrDestProps = new Dictionary<String, Object>();
                                foreach (SyncFieldMapping objFldMapping in fieldMappings)
                                {
                                    if (objFldMapping.SrcField != null && objFldMapping.DestField != null)
                                    {
                                        if (objFullSrcObj.Properties.ContainsKey(objFldMapping.SrcField.Key))
                                        {
                                            if (objFullSrcObj.Properties[objFldMapping.SrcField.Key] != null)
                                            {
                                                switch (objFldMapping.SrcField.FieldDataType)
                                                {
                                                    case Constants.FieldDataTypes.Taxonomy:
                                                        //Resolve Taxonomies Between Src and Dest Systems...
                                                        if(objFullSrcObj.Properties[objFldMapping.SrcField.Key] is List<String>)
                                                        {
                                                            List<String> arrSrcTaxonIDs = (List<String>)objFullSrcObj.Properties[objFldMapping.SrcField.Key];
                                                            switch (objFldMapping.DestField.FieldDataType)
                                                            {
                                                                case Constants.FieldDataTypes.Taxonomy:
                                                                    //Supported Taxonomy -> Taxonomy

                                                                    //////////////////////////////////////////////
                                                                    //objFldMapping.Options.MutuallyExclusive
                                                                    //////////////////////////////////////////////

                                                                    List<String> arrDestTaxonIDs = null;
                                                                    foreach (String srcTaxonId in arrSrcTaxonIDs)
                                                                    {
                                                                        //Lookup State Obj for Dest equivelent...
                                                                        StateMapping objTagMapping = globalStateMapping[StateMappingKeys.TagMappings].Where(obj => obj.SrcId == srcTaxonId.ToString()).FirstOrDefault();

                                                                        if (objTagMapping != null && !String.IsNullOrEmpty(objTagMapping.DestId))
                                                                        {
                                                                            if (arrDestTaxonIDs == null)
                                                                            {
                                                                                arrDestTaxonIDs = new List<String>();
                                                                            }
                                                                            arrDestTaxonIDs.Add(objTagMapping.DestId);
                                                                        }
                                                                        else
                                                                        {
                                                                            syncStatus.Warnings += 1;
                                                                            AppendSyncLog(new SyncLogEntry(LogTypes.Warning, LogActions.Running, LogResults.Undetermined, "SyncSubContainerObjects", "Looking for equivelent taxon in destination platform",
                                                                                "Could not resolve taxon mapping from src to dest platforms [src." + objFldMapping.SrcField.Key + "] to [dest." + objFldMapping.DestField.Key + "]. Perhaps the Tag Sync didn't execute or failed. srcTaxonId: " + srcTaxonId,
                                                                                null, null), ref syncLog);
                                                                        }

                                                                        objTagMapping = null;
                                                                    }
                                                                    //Set the dest value...
                                                                    GeneralHelpers.addUpdateDictionary(ref arrDestProps, objFldMapping.DestField.Key, arrDestTaxonIDs);

                                                                    arrDestTaxonIDs = null;

                                                                    break;
                                                                case Constants.FieldDataTypes.String:
                                                                    //Supported Taxonomy -> String

                                                                    //Use Src Labels...
                                                                    List<String> arrLabels = new List<String>();
                                                                    SyncTag objSrcRootTag = null;
                                                                    Dictionary<String, String> objLabelLookupList = new Dictionary<String, String>();
                                                                    
                                                                    if(srcTaxonomyLookupCache.ContainsKey(objFldMapping.SrcField.LinkedLookupId))
                                                                    {
                                                                        objSrcRootTag = srcTaxonomyLookupCache[objFldMapping.SrcField.LinkedLookupId];
                                                                    }
                                                                    if (objSrcRootTag != null)
                                                                    {
                                                                        GetRecursiveTagLabelLookups(objSrcRootTag, ref objLabelLookupList);
                                                                        foreach (String srcTaxonId in arrSrcTaxonIDs)
                                                                        {
                                                                            String strSrcTaxonLabel = objLabelLookupList[srcTaxonId];
                                                                            if(!String.IsNullOrEmpty(strSrcTaxonLabel))
                                                                            {
                                                                                arrLabels.Add(strSrcTaxonLabel);
                                                                            }
                                                                            strSrcTaxonLabel = null;
                                                                        }
                                                                    }
                                                                    GeneralHelpers.addUpdateDictionary(ref arrDestProps, objFldMapping.DestField.Key, GeneralHelpers.stringListToCSV(arrLabels));

                                                                    objLabelLookupList = null;
                                                                    objSrcRootTag = null;
                                                                    arrLabels = null;
                                                                    break;
                                                                default:
                                                                    //Not Supported...
                                                                    syncStatus.Warnings += 1;
                                                                    AppendSyncLog(new SyncLogEntry(LogTypes.Warning, LogActions.Running, LogResults.Undetermined, "SyncSubContainerObjects", "Evaluating taxonomy field mapping conversion",
                                                                                "Conversion Not Supported [src." + objFldMapping.SrcField.Key + " (" + objFldMapping.SrcField.FieldDataType.ToString() + ")] to [dest." + objFldMapping.DestField.Key + " (" + objFldMapping.DestField.FieldDataType.ToString() + ")]",
                                                                                null, null), ref syncLog);
                                                                    break;
                                                            }
                                                            arrSrcTaxonIDs = null;
                                                        }
                                                        break;
                                                    case Constants.FieldDataTypes.Integer:
                                                    case Constants.FieldDataTypes.Decimal:
                                                    case Constants.FieldDataTypes.Boolean:
                                                    case Constants.FieldDataTypes.String:
                                                    case Constants.FieldDataTypes.Guid:
                                                    case Constants.FieldDataTypes.DateTime:
                                                    case Constants.FieldDataTypes.Binary:
                                                        //These types don't need any resolution or translation as they are primative types... Dest system should handle these...
                                                        GeneralHelpers.addUpdateDictionary(ref arrDestProps, objFldMapping.DestField.Key, objFullSrcObj.Properties[objFldMapping.SrcField.Key]);
                                                        break;
                                                    default:
                                                        //Not Supported...
                                                        syncStatus.Warnings += 1;
                                                        AppendSyncLog(new SyncLogEntry(LogTypes.Warning, LogActions.Running, LogResults.Undetermined, "SyncSubContainerObjects", "Evaluating field mapping conversion",
                                                                                "Type Not Supported [src." + objFldMapping.SrcField.Key + " (" + objFldMapping.SrcField.FieldDataType.ToString() + ")] to [dest." + objFldMapping.DestField.Key + " (" + objFldMapping.DestField.FieldDataType.ToString() + ")]",
                                                                                null, null), ref syncLog);
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                if(objFldMapping.Options != null)
                                                {
                                                    switch(objFldMapping.Options.NullAction)
                                                    {
                                                        case NullActions.AllowNull:
                                                            GeneralHelpers.addUpdateDictionary(ref arrDestProps, objFldMapping.DestField.Key, null);
                                                            break;
                                                        case NullActions.SetDefault:
                                                            switch (objFldMapping.SrcField.FieldDataType)
                                                            {
                                                                case Constants.FieldDataTypes.Integer:
                                                                    GeneralHelpers.addUpdateDictionary(ref arrDestProps, objFldMapping.DestField.Key, 0);
                                                                    break;
                                                                case Constants.FieldDataTypes.Decimal:
                                                                    GeneralHelpers.addUpdateDictionary(ref arrDestProps, objFldMapping.DestField.Key, 0.0f);
                                                                    break;
                                                                case Constants.FieldDataTypes.Boolean:
                                                                    GeneralHelpers.addUpdateDictionary(ref arrDestProps, objFldMapping.DestField.Key, false);
                                                                    break;
                                                                case Constants.FieldDataTypes.String:
                                                                    GeneralHelpers.addUpdateDictionary(ref arrDestProps, objFldMapping.DestField.Key, "");
                                                                    break;
                                                                case Constants.FieldDataTypes.Guid:
                                                                    GeneralHelpers.addUpdateDictionary(ref arrDestProps, objFldMapping.DestField.Key, Guid.Empty);
                                                                    break;
                                                                case Constants.FieldDataTypes.DateTime:
                                                                    GeneralHelpers.addUpdateDictionary(ref arrDestProps, objFldMapping.DestField.Key, DateTime.MinValue);
                                                                    break;
                                                            }
                                                            break;
                                                        case NullActions.Skip:
                                                            //Skip, no action...
                                                            break;
                                                        default:
                                                            //Same as Skip, no action...
                                                            break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                destObject.Properties = arrDestProps;
                            }

                            if(!this.TestMode)
                            {
                                if(requiresSave)
                                {
                                    destObject.BinaryPayload = objFullSrcObj.BinaryPayload;
                                    destObject.SizeBytes = objFullSrcObj.SizeBytes;

                                    if (objFullSrcObj.BinaryPayload != null && objFullSrcObj.BinaryPayload.Length > 0)
                                    {
                                        syncStatus.BinaryTransferedBytes += objFullSrcObj.BinaryPayload.Length;
                                    }
                                }
                            }
                            else
                            {
                                Byte[] arrBinaryPayload = Encoding.ASCII.GetBytes("Running in test mode. All synchronised objects will contain this payload. To synchronise actual data disable test mode.");

                                destObject.BinaryPayload = arrBinaryPayload;
                                destObject.SizeBytes = arrBinaryPayload.Length;

                                arrBinaryPayload = null;
                            }

                            this.currSyncObjectCount++;

                            try
                            {
                                //Save in Dest Platform...
                                destObject.Id = destProvider.SaveObject(destObject);
                                switch (objEntityState)
                                {
                                    case EntityStates.New:
                                        syncStatus.ObjectsCreated += 1;
                                        break;
                                    case EntityStates.Existing:
                                        syncStatus.ObjectsUpdated += 1;
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                syncStatus.Errors += 1;
                                syncStatus.ObjectsSkipped += 1;
                                String strDesc = GenerateContainerPath(parentSrcContainer) + destObject.FileName + " - " + GeneralHelpers.prettyFileSize(destObject.BinaryPayload != null ? destObject.BinaryPayload.Length.ToString() : "0");
                                AppendSyncLog(new SyncLogEntry(LogTypes.Error, LogActions.Running, LogResults.Failed, "SyncSubContainerObjects", "Saving object in destination platform [" + strDesc + "]",
                                                                                "The destination platform returned an error.",
                                                                                ex.Message, ex.StackTrace), ref syncLog);
                            }
                        }
                        
                        objFullSrcObj = null;
                        srcSyncFields = null;
                    }
                    else
                    {
                        syncStatus.ObjectsSkipped += 1;
                    }
                    if (this.LogVerbosity == LogVerbosityLevels.All || objEntityState == EntityStates.Changed)
                    {
                        AppendSyncLog(new SyncLogEntry(LogTypes.Trace, LogActions.Running, LogResults.Undetermined, "SyncSubContainerObjects", "Saving object " + currObject.ToString() + " of " + totalObjects.ToString() + " in destination platform",
                                                                        objEntityState.ToString() + " destObject.Id: " + destObject.Id,
                                                                        null, JsonConvert.SerializeObject(new Dictionary<String, Int32>() { { "Current", currObject }, { "Total", totalObjects } })), ref syncLog);
                    }

                    //Maintain StateMapping...
                    if (objMapping != null)
                    {
                        stateMapping[StateMappingKeys.ObjectMappings].Remove(objMapping);
                    }
                    if (objMapping == null)
                    {
                        objMapping = new StateMapping();
                    }
                    objMapping.SrcId = childSrcObject.Id;
                    objMapping.DestId = destObject.Id;
                    stateMapping[StateMappingKeys.ObjectMappings].Add(objMapping);

                    destDeletions.Remove(destObject.Id);

                    //Periodically Persist State Mapping...
                    if (currObject > 0 && currObject % 50 == 0)
                    {
                        UpdateStateMappingData(ruleId, ref stateMapping, false);
                    }

                    objMapping = null;
                    destObject = null;
                }
            }
            if (parentSrcContainer.SyncContainers != null && parentSrcContainer.SyncContainers.Count > 0)
            {
                if(!this.TestMode || (this.TestMode && this.currSyncObjectCount <= this.TestSyncObjectLimit))
                {
                    //Recurse Call to all children...
                    foreach (SyncContainer childSrcContainer in parentSrcContainer.SyncContainers)
                    {
                        StateMapping objMapping = stateMapping[StateMappingKeys.ContainerMappings].Where(obj => obj.SrcId == childSrcContainer.Id).FirstOrDefault();
                        SyncContainer destContainer = null;

                        if (objMapping != null && parentDestContainer.SyncContainers != null)
                        {
                            //We have a state mapping... Check if the destination actually exists... Use that with existing ID...
                            destContainer = parentDestContainer.SyncContainers.Where(obj => obj.Id == objMapping.DestId).FirstOrDefault();
                        }
                        if (destContainer != null)
                        {
                            //Recursive call
                            SyncSubContainerObjects(ruleId, ref srcProvider, ref destProvider, childSrcContainer, destContainer, false, rootDestContainerId, ref stateMapping, ref globalStateMapping, ref destDeletions, ref fieldMappings, ref srcTaxonomyLookupCache, ref syncLog, ref syncStatus, ref totalObjects, ref currObject, metaDataOnlyOverride);
                        }
                        else
                        {
                            syncStatus.Warnings += 1;
                            AppendSyncLog(new SyncLogEntry(LogTypes.Warning, LogActions.Running, LogResults.Undetermined, "SyncSubContainerObjects", "Getting reference to destination platform container from StateMappings",
                                                                            "Could not find reference to destination platform container from StateMappings. This should not happen unless the container failed to create when containers were synced.",
                                                                            null, null), ref syncLog);
                        }

                        destContainer = null;
                        objMapping = null;
                    }
                }
            }
        }
        private void GetRecursiveObjectIDs(SyncContainer parentContainer, ref List<String> idList)
        {
            if(parentContainer.SyncObjects != null && parentContainer.SyncObjects.Count > 0)
            {
                foreach (SyncObject childObject in parentContainer.SyncObjects)
                {
                    idList.Add(childObject.Id);
                }
            }
            if (parentContainer.SyncContainers != null && parentContainer.SyncContainers.Count > 0)
            {
                foreach (SyncContainer childContainer in parentContainer.SyncContainers)
                {
                    GetRecursiveObjectIDs(childContainer, ref idList);
                }
            }
        }
        private Boolean CompareSyncObjects(SyncObject srcObject, SyncObject destObject)
        {
            //Could incorporate some Sync Options Here to determine what is dirty state...
            Boolean retVal = false;

            //if (srcObject.Name != destObject.Name)
            //{
            //    retVal = true;
            //}
            //if (srcObject.FileName != destObject.FileName)
            //{
            //    retVal = true;
            //}
            if (srcObject.DateCreated != destObject.DateCreated)
            {
                retVal = true;
            }
            if (srcObject.LastUpdated != destObject.LastUpdated)
            {
                retVal = true;
            }
            if (srcObject.SizeBytes != destObject.SizeBytes)
            {
                retVal = true;
            }

            return retVal;
        }
        #endregion

        #region PostSyncTasks
        private void RunPostSyncTasks(ref ConnectionRule currRule, ref PlatformCfg srcPlatformCfg, ref PlatformCfg dstPlatformCfg, ref SyncContainer srcRootContainer, ref SyncContainer destRootContainer, ref List<SyncLogEntry> syncLog, ref SyncStatus syncStatus)
        {
            if (currRule != null && srcPlatformCfg != null&& dstPlatformCfg != null)
            {
                if (srcPlatformCfg != null && !String.IsNullOrEmpty(srcPlatformCfg.PlatformID) && GeneralHelpers.parseGUID(srcPlatformCfg.PlatformID) != Guid.Empty)
                {
                    Guid srcPlatformId = GeneralHelpers.parseGUID(srcPlatformCfg.PlatformID);

                    if (!String.IsNullOrEmpty(currRule.SourcePostSyncTasks))
                    {
                        List<PostSyncTask> arrSrcTasks = JsonConvert.DeserializeObject<List<PostSyncTask>>(currRule.SourcePostSyncTasks);

                        if (arrSrcTasks != null)
                        {
                            IPlatform objSrcPlatform = ProviderHelpers.GetPlatform(srcPlatformId);
                            if (objSrcPlatform != null)
                            {
                                foreach (PostSyncTask srcTask in arrSrcTasks)
                                {
                                    Guid srcTaskId = GeneralHelpers.parseGUID(srcTask.PostSyncTaskID);
                                    if(srcTaskId != Guid.Empty)
                                    {
                                        IPostSyncTask objSrcTask = ProviderHelpers.GetPostSyncTask(objSrcPlatform, srcTaskId);

                                        if(objSrcTask != null)
                                        {
                                            try
                                            {
                                                objSrcTask.TargetContainer = srcRootContainer;
                                                objSrcTask.SetConfig(srcPlatformCfg, srcTask.Cfg);
                                                AppendSyncLog(new SyncLogEntry(LogTypes.Trace, LogActions.Running, LogResults.Undetermined, "RunPostSyncTasks", "Running Source Platform Post Sync Task: " + srcTaskId.ToString() + " (" + objSrcTask.GetTaskInstanceDescription() + ")", "", null, null), ref syncLog);
                                                objSrcTask.RunTask();
                                            }
                                            catch (Exception ex)
                                            {
                                                syncStatus.Errors += 1;
                                                AppendSyncLog(new SyncLogEntry(LogTypes.Error, LogActions.Running, LogResults.Failed, "RunPostSyncTasks", "Running Source Platform Post Sync Task: " + srcTaskId.ToString() + " (" + objSrcTask.GetTaskInstanceDescription() + ")",
                                                                                                "The source platform returned an error.",
                                                                                                ex.Message, ex.StackTrace), ref syncLog);
                                            }
                                        }
                                        else
                                        {
                                            AppendSyncLog(new SyncLogEntry(LogTypes.Warning, LogActions.Running, LogResults.Failed, "RunPostSyncTasks", "Creating Instance of Destination Post Sync Task",
                                                                            "Could not resolve an instance of the Destination Post Sync Task with the Id supplied. This task will not be run. Check configuration.",
                                                                            null, null), ref syncLog);
                                        }

                                        objSrcTask = null;
                                    }
                                    else
                                    {
                                        AppendSyncLog(new SyncLogEntry(LogTypes.Warning, LogActions.Running, LogResults.Failed, "RunPostSyncTasks", "Validating Source Post Sync Task",
                                                                            "The specified Source Post Sync Task has an invalid ID. This sync task will not be run. Check configuration.",
                                                                            null, null), ref syncLog);
                                    }
                                }
                            }
                            else
                            {
                                AppendSyncLog(new SyncLogEntry(LogTypes.Warning, LogActions.Running, LogResults.Failed, "RunPostSyncTasks", "Creating Instance of Source Platform Provider",
                                                                            "Could not resolve an instance of Source Platform with the Id supplied. No post sync tasks will be run against the source provider. Check configuration.",
                                                                            null, null), ref syncLog);
                            }
                            objSrcPlatform = null;
                        }
                        else
                        {
                            AppendSyncLog(new SyncLogEntry(LogTypes.Trace, LogActions.Running, LogResults.Undetermined, "RunPostSyncTasks", "Retrieving Source Provider Post Sync Tasks",
                                                                            "No post sync tasks have been configured to run against the source provider.",
                                                                            null, null), ref syncLog);
                        }

                        arrSrcTasks = null;
                    }
                    else
                    {
                        AppendSyncLog(new SyncLogEntry(LogTypes.Trace, LogActions.Running, LogResults.Undetermined, "RunPostSyncTasks", "Retrieving Source Provider Post Sync Tasks",
                                                                            "No post sync tasks have been configured to run against the source provider.",
                                                                            null, null), ref syncLog);
                    }
                }
                else
                {
                    AppendSyncLog(new SyncLogEntry(LogTypes.Warning, LogActions.Running, LogResults.Failed, "RunPostSyncTasks", "Validating Source Platform Provider",
                                                                            "Source Platform is not configured. No post sync tasks will be run against the source provider. Check configuration.",
                                                                            null, null), ref syncLog);
                }
                
                if (dstPlatformCfg != null && !String.IsNullOrEmpty(dstPlatformCfg.PlatformID) && GeneralHelpers.parseGUID(dstPlatformCfg.PlatformID) != Guid.Empty)
                {
                    Guid dstPlatformId = GeneralHelpers.parseGUID(dstPlatformCfg.PlatformID);

                    if (!String.IsNullOrEmpty(currRule.DestinationPostSyncTasks))
                    {
                        List<PostSyncTask> arrDstTasks = JsonConvert.DeserializeObject<List<PostSyncTask>>(currRule.DestinationPostSyncTasks);

                        if (arrDstTasks != null)
                        {
                            IPlatform objDstPlatform = ProviderHelpers.GetPlatform(dstPlatformId);
                            if (objDstPlatform != null)
                            {
                                foreach (PostSyncTask dstTask in arrDstTasks)
                                {
                                    Guid dstTaskId = GeneralHelpers.parseGUID(dstTask.PostSyncTaskID);
                                    if (dstTaskId != Guid.Empty && !String.IsNullOrEmpty(dstTask.Cfg))
                                    {
                                        IPostSyncTask objDstTask = ProviderHelpers.GetPostSyncTask(objDstPlatform, dstTaskId);

                                        if (objDstTask != null)
                                        {
                                            try
                                            {
                                                objDstTask.TargetContainer = destRootContainer;
                                                objDstTask.SetConfig(dstPlatformCfg, dstTask.Cfg);
                                                AppendSyncLog(new SyncLogEntry(LogTypes.Trace, LogActions.Running, LogResults.Undetermined, "RunPostSyncTasks", "Running Destination Platform Post Sync Task: " + dstTaskId.ToString() + " (" + objDstTask.GetTaskInstanceDescription() + ")", "", null, null), ref syncLog);
                                                objDstTask.RunTask();
                                            }
                                            catch (Exception ex)
                                            {
                                                syncStatus.Errors += 1;
                                                AppendSyncLog(new SyncLogEntry(LogTypes.Error, LogActions.Running, LogResults.Failed, "RunPostSyncTasks", "Running Destination Platform Post Sync Task: " + dstTaskId.ToString() + " (" + objDstTask.GetTaskInstanceDescription() + ")",
                                                                                                "The destination platform returned an error.",
                                                                                                ex.Message, ex.StackTrace), ref syncLog);
                                            }
                                        }
                                        else
                                        {
                                            AppendSyncLog(new SyncLogEntry(LogTypes.Warning, LogActions.Running, LogResults.Failed, "RunPostSyncTasks", "Creating Instance of Destination Post Sync Task",
                                                                            "Could not resolve an instance of the Destination Post Sync Task with the Id supplied. This task will not be run. Check configuration.",
                                                                            null, null), ref syncLog);
                                        }

                                        objDstTask = null;
                                    }
                                    else
                                    {
                                        AppendSyncLog(new SyncLogEntry(LogTypes.Warning, LogActions.Running, LogResults.Failed, "RunPostSyncTasks", "Validating Destination Post Sync Task",
                                                                            "The specified Destination Post Sync Task is not configured. This sync task will not be run. Check configuration.",
                                                                            null, null), ref syncLog);
                                    }
                                }
                            }
                            else
                            {
                                AppendSyncLog(new SyncLogEntry(LogTypes.Warning, LogActions.Running, LogResults.Failed, "RunPostSyncTasks", "Creating Instance of Destination Platform Provider",
                                                                            "Could not resolve an instance of Destination Platform with the Id supplied. No post sync tasks will be run against the destination provider. Check configuration.",
                                                                            null, null), ref syncLog);
                            }
                            objDstPlatform = null;
                        }
                        else
                        {
                            AppendSyncLog(new SyncLogEntry(LogTypes.Trace, LogActions.Running, LogResults.Undetermined, "RunPostSyncTasks", "Retrieving Destination Provider Post Sync Tasks",
                                                                            "No post sync tasks have been configured to run against the destination provider.",
                                                                            null, null), ref syncLog);
                        }

                        arrDstTasks = null;
                    }
                    else
                    {
                        AppendSyncLog(new SyncLogEntry(LogTypes.Trace, LogActions.Running, LogResults.Undetermined, "RunPostSyncTasks", "Retrieving Destination Provider Post Sync Tasks",
                                                                            "No post sync tasks have been configured to run against the destination provider.",
                                                                            null, null), ref syncLog);
                    }
                }
                else
                {
                    AppendSyncLog(new SyncLogEntry(LogTypes.Warning, LogActions.Running, LogResults.Failed, "RunPostSyncTasks", "Validating Destination Platform Provider",
                                                                            "Destination Platform is not configured. No post sync tasks will be run against the destination provider. Check configuration.",
                                                                            null, null), ref syncLog);
                }
            }
        }
        #endregion
    }
}