using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;

using UDC.Common;
using UDC.Common.Interfaces;
using UDC.Common.Data;
using UDC.Common.Data.Models;
using UDC.Common.Database.Data.Models.Database;

using DataConnectorUI.Controllers.Filters;

using UDC.DataConnectorCore;
using static UDC.DataConnectorCore.ProviderHelpers;
using Newtonsoft.Json;
using UDC.Common.Database.Logging;
using UDC.Common.Database.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DataConnectorUI.Controllers.Api
{

    [ServiceFilter(typeof(AdminAuthFilter))]
    [Route("api/AdminApi")]
    public class AdminApiController : ApiController
    {
        private DatabaseContext _databaseContext;

        public AdminApiController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        [HttpGet]
        [Route("GetConnections")]
        public async Task<Object> GetConnections(Int32 pageSize, Int32 pageNo)
        {
            APIResponse objRetVal = new APIResponse(0, "Success");

            try
            {
                if (pageSize > 0)
                { 
                    objRetVal.data = await _databaseContext.Connections
                        .Skip((pageNo - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync(); 
                }
                else
                { 
                    objRetVal.data = await _databaseContext.Connections.ToListAsync(); 
                }
            }
            catch (Exception ex)
            {
                objRetVal.message = "An error occurred while trying to serve the request! " + ex.Message + " -- " + ex.StackTrace;
                objRetVal.exitCode = 1;
            }

            return objRetVal;
        }

        [HttpGet]
        [Route("GetConnection")]
        public async Task<Object> GetConnection(Int64 connectionID)
        {
            APIResponse objRetVal = new APIResponse(0, "Success");

            try
            {
                objRetVal.data = await _databaseContext.Connections
                    .FirstOrDefaultAsync(obj => obj.Id == connectionID);
            }
            catch (Exception ex)
            {
                objRetVal.message = "An error occurred while trying to serve the request! " + ex.Message + " -- " + ex.StackTrace;
                objRetVal.exitCode = 1;
            }

            return objRetVal;
        }

        [HttpPost]
        [Route("SaveConnection")]
        public async Task<Object> SaveConnection([FromBody]ConnectionFormData formData)
        {
            APIResponse objRetVal = new APIResponse(0, "Success"); 

            try
            {
                var saveId = GeneralHelpers.parseInt64(formData.Id);
                Connection objEntry = await _databaseContext.Connections
                    .FirstOrDefaultAsync(obj => obj.Id == saveId);

                if(objEntry == null)
                {
                    objEntry = new Connection();
                    objEntry.DateCreated = DateTime.UtcNow;
                    await _databaseContext.Connections.AddAsync(objEntry);
                }

                objEntry.Name = GeneralHelpers.parseString(formData.Name);
                objEntry.SourcePlatformCfg = GeneralHelpers.parseString(formData.SourcePlatformCfg);
                objEntry.DestinationPlatformCfg = GeneralHelpers.parseString(formData.DestinationPlatformCfg);
                objEntry.Enabled = GeneralHelpers.parseBool(formData.Enabled);
                objEntry.LastUpdated = DateTime.UtcNow;

                await _databaseContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                objRetVal.message = "An error occurred while trying to serve the request! " + ex.Message + " -- " + ex.StackTrace;
                objRetVal.exitCode = 1;
            }

            return objRetVal;
        }

        public class ConnectionFormData
        {
            public long Id {get;set;}
            public String Name { get; set; }
            public String SourcePlatformCfg { get; set; }
            public String DestinationPlatformCfg { get; set; }
            public String Enabled { get; set; }
        }

        [HttpGet]
        [Route("DeleteConnection")]
        public async Task<Object> DeleteConnection(Int64 connectionID)
        {
            APIResponse objRetVal = new APIResponse(0, "Success");

            try
            {
                Connection objEntity = await _databaseContext.Connections
                    .FirstOrDefaultAsync(obj => obj.Id == connectionID);
                if(objEntity != null)
                {
                    _databaseContext.Connections.Remove(objEntity);
                    await _databaseContext.SaveChangesAsync();
                }
                objEntity = null;
            }
            catch (Exception ex)
            {
                objRetVal.message = "An error occurred while trying to serve the request! " + ex.Message + " -- " + ex.StackTrace;
                objRetVal.exitCode = 1;
            }

            return objRetVal;
        }

        [HttpGet]
        [Route("DisableConnection")]
        public async Task<Object> DisableConnection(Int64 connectionID, Boolean disabled)
        {
            APIResponse objRetVal = new APIResponse(0, "Success");

            try
            {
                Connection objEntity = await _databaseContext.Connections
                    .FirstOrDefaultAsync(obj => obj.Id == connectionID);
                if(objEntity != null)
                {
                    objEntity.Enabled = disabled;
                    await _databaseContext.SaveChangesAsync();
                }
                objEntity = null;
            }
            catch (Exception ex)
            {
                objRetVal.message = "An error occurred while trying to serve the request! " + ex.Message + " -- " + ex.StackTrace;
                objRetVal.exitCode = 1;
            }

            return objRetVal;
        }

        [HttpGet]
        [Route("GetRules")]
        public async Task<Object> GetRules(Int64 connectionID, Int32 pageSize, Int32 pageNo)
        {
            APIResponse objRetVal = new APIResponse(0, "Success");

            try
            {
                if (pageSize > 0)
                {
                    objRetVal.data = await _databaseContext.ConnectionRules
                        .Where(obj => obj.connectionID == connectionID)
                        .Skip((pageNo - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();
                }
                else
                {
                    objRetVal.data = await _databaseContext.ConnectionRules
                        .Where(obj => obj.connectionID == connectionID)
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                objRetVal.message = "An error occurred while trying to serve the request! " + ex.Message + " -- " + ex.StackTrace;
                objRetVal.exitCode = 1;
            }

            return objRetVal;
        }

        [HttpGet]
        [Route("GetRule")]
        public async Task<Object> GetRule(Int64 ruleID)
        {
            APIResponse objRetVal = new APIResponse(0, "Success");

            try
            {
                objRetVal.data = await _databaseContext.ConnectionRules
                    .FirstOrDefaultAsync(obj => obj.Id == ruleID);
            }
            catch (Exception ex)
            {
                objRetVal.message = "An error occurred while trying to serve the request! " + ex.Message + " -- " + ex.StackTrace;
                objRetVal.exitCode = 1;
            }

            return objRetVal;
        }

        [HttpPost]
        [Route("SaveRule")]
        public async Task<Object> SaveRule([FromBody]ConnectionRuleFormData formData)
        {
            APIResponse objRetVal = new APIResponse(0, "Success");

            try
            {
                var ruleID = GeneralHelpers.parseInt64(formData.Id);
                ConnectionRule objEntry = await _databaseContext.ConnectionRules
                    .FirstOrDefaultAsync(obj => obj.Id == ruleID);

                if (objEntry == null)
                {
                    objEntry = new ConnectionRule();
                    objEntry.DateCreated = DateTime.UtcNow;
                    await _databaseContext.ConnectionRules.AddAsync(objEntry);
                }

                objEntry.connectionID = GeneralHelpers.parseInt64(formData.connectionID);
                objEntry.Name = GeneralHelpers.parseString(formData.Name);
                objEntry.SyncIntervalCron = GeneralHelpers.parseString(formData.SyncIntervalCron);
                objEntry.SourceContainerCfg = GeneralHelpers.parseString(formData.SourceContainerCfg);
                objEntry.DestinationContainerCfg = GeneralHelpers.parseString(formData.DestinationContainerCfg);
                objEntry.FieldMappings = GeneralHelpers.parseString(formData.FieldMappings);
                objEntry.Enabled = GeneralHelpers.parseBool(formData.Enabled);
                objEntry.LastUpdated = DateTime.UtcNow;

                await _databaseContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                objRetVal.message = "An error occurred while trying to serve the request! " + ex.Message + " -- " + ex.StackTrace;
                objRetVal.exitCode = 1;
            }

            return objRetVal;
        }

        public class ConnectionRuleFormData
        {
            public long Id {get;set;}
            public String Name { get; set; }
            public String connectionID { get; set; }
            public String SyncIntervalCron { get; set; }
            public String SourceContainerCfg { get; set; }
            public String DestinationContainerCfg { get; set; }
            public String FieldMappings { get; set; }
            public String Enabled { get; set; }
        }

        [HttpGet]
        [Route("DeleteRule")]
        public async Task<Object> DeleteRule(Int64 ruleID)
        {
            APIResponse objRetVal = new APIResponse(0, "Success");

            try
            {
                ConnectionRule objEntity = await _databaseContext.ConnectionRules
                    .FirstOrDefaultAsync(obj => obj.Id == ruleID);
                if (objEntity != null)
                {
                    _databaseContext.ConnectionRules.Remove(objEntity);
                    await _databaseContext.SaveChangesAsync();
                }
                objEntity = null;
            }
            catch (Exception ex)
            {
                objRetVal.message = "An error occurred while trying to serve the request! " + ex.Message + " -- " + ex.StackTrace;
                objRetVal.exitCode = 1;
            }

            return objRetVal;
        }

        [HttpGet]
        [Route("DisableRule")]
        public async Task<Object> DisableRule(Int64 ruleID, Boolean disabled)
        {
            APIResponse objRetVal = new APIResponse(0, "Success");

            try
            {
                ConnectionRule objEntity = await _databaseContext.ConnectionRules
                    .FirstOrDefaultAsync(obj => obj.Id == ruleID);
                if (objEntity != null)
                {
                    objEntity.Enabled = disabled;
                    await _databaseContext.SaveChangesAsync();
                }
                objEntity = null;
            }
            catch (Exception ex)
            {
                objRetVal.message = "An error occurred while trying to serve the request! " + ex.Message + " -- " + ex.StackTrace;
                objRetVal.exitCode = 1;
            }

            return objRetVal;
        }

        [HttpGet]
        [Route("GetPlatforms")]
        public Object GetPlatforms()
        {
            APIResponse objRetVal = new APIResponse(0, "Success");
            
            try
            {
                objRetVal.data = ProviderHelpers.GetPlatformInstances();
            }
            catch (Exception ex)
            {
                objRetVal.message = "An error occurred while trying to serve the request! " + ex.Message + " -- " + ex.StackTrace;
                objRetVal.exitCode = 1;
            }

            return objRetVal;
        }

        [HttpGet]
        [Route("GetIntegrators")]
        public Object GetIntegrators(String platformID)
        {
            APIResponse objRetVal = new APIResponse(0, "Success");

            try
            {
                Guid platformGuid = GeneralHelpers.parseGUID(platformID);

                if(platformGuid != Guid.Empty)
                {
                    IPlatform objTargetPlatform = ProviderHelpers.GetPlatform(platformGuid);

                    if (objTargetPlatform != null)
                    {
                        Dictionary<String, Object> objIntegrators = new Dictionary<String, Object>();
                        List<IIntegrator> arrSrcIntegrators = ProviderHelpers.GetSrcIntegrators(objTargetPlatform);
                        List<IIntegrator> arrDestIntegrators = ProviderHelpers.GetDestIntegrators(objTargetPlatform);

                        objIntegrators.Add("Sources", arrSrcIntegrators);
                        objIntegrators.Add("Destinations", arrDestIntegrators);

                        objRetVal.data = objIntegrators;

                        arrDestIntegrators = null;
                        arrSrcIntegrators = null;
                    }

                    objTargetPlatform = null;
                }
            }
            catch (Exception ex)
            {
                objRetVal.message = "An error occurred while trying to serve the request! " + ex.Message + " -- " + ex.StackTrace;
                objRetVal.exitCode = 1;
            }

            return objRetVal;
        }

        [HttpGet]
        [Route("GetPostSyncTasks")]
        public Object GetPostSyncTasks(String platformID)
        {
            APIResponse objRetVal = new APIResponse(0, "Success");

            try
            {
                Guid platformGuid = GeneralHelpers.parseGUID(platformID);

                if (platformGuid != Guid.Empty)
                {
                    IPlatform objTargetPlatform = ProviderHelpers.GetPlatform(platformGuid);

                    if (objTargetPlatform != null)
                    {
                        Dictionary<String, Object> objPostSyncTasks = new Dictionary<String, Object>();
                        List<IPostSyncTask> arrPostSyncTasks = ProviderHelpers.GetSupportedPostSyncTasks(objTargetPlatform);

                        objPostSyncTasks.Add("PostSyncTasks", arrPostSyncTasks);

                        objRetVal.data = objPostSyncTasks;

                        arrPostSyncTasks = null;
                    }

                    objTargetPlatform = null;
                }
            }
            catch (Exception ex)
            {
                objRetVal.message = "An error occurred while trying to serve the request! " + ex.Message + " -- " + ex.StackTrace;
                objRetVal.exitCode = 1;
            }

            return objRetVal;
        }

        [HttpGet]
        [Route("GetSrcContainers")]
        public Object GetSrcContainers(Int64 connectionID)
        {
            APIResponse objRetVal = new APIResponse(0, "Success");

            try
            {
                IIntegrator objTargetIntegrator = ProviderHelpers.GetIntegrator(connectionID, Targets.Source);
                if (objTargetIntegrator != null)
                {
                    objRetVal.data = objTargetIntegrator.GetContainers();
                }
                objTargetIntegrator = null;
            }
            catch (Exception ex)
            {
                objRetVal.message = "An error occurred while trying to serve the request! " + ex.Message + " -- " + ex.StackTrace;
                objRetVal.exitCode = 1;
            }

            return objRetVal;
        }

        [HttpGet]
        [Route("GetDestContainers")]
        public Object GetDestContainers(Int64 connectionID)
        {
            APIResponse objRetVal = new APIResponse(0, "Success");

            try
            {
                IIntegrator objTargetIntegrator = ProviderHelpers.GetIntegrator(connectionID, Targets.Destination);
                if (objTargetIntegrator != null)
                {
                    objRetVal.data = objTargetIntegrator.GetContainers();
                }
                objTargetIntegrator = null;
            }
            catch (Exception ex)
            {
                objRetVal.message = "An error occurred while trying to serve the request! " + ex.Message + " -- " + ex.StackTrace;
                objRetVal.exitCode = 1;
            }

            return objRetVal;
        }

        [HttpGet]
        [Route("GetSrcObjFields")]
        public Object GetSrcObjFields(Int64 connectionID, String containerId)
        {
            APIResponse objRetVal = new APIResponse(0, "Success");

            try
            {
                IIntegrator objTargetIntegrator = ProviderHelpers.GetIntegrator(connectionID, Targets.Source);
                if (objTargetIntegrator != null)
                {
                    objRetVal.data = objTargetIntegrator.GetFields(containerId);
                }
                objTargetIntegrator = null;
            }
            catch (Exception ex)
            {
                objRetVal.message = "An error occurred while trying to serve the request! " + ex.Message + " -- " + ex.StackTrace;
                objRetVal.exitCode = 1;
            }

            return objRetVal;
        }

        [HttpGet]
        [Route("GetDestObjFields")]
        public Object GetDestObjFields(Int64 connectionID, String containerId)
        {
            APIResponse objRetVal = new APIResponse(0, "Success");

            try
            {
                IIntegrator objTargetIntegrator = ProviderHelpers.GetIntegrator(connectionID, Targets.Destination);
                if (objTargetIntegrator != null)
                {
                    objRetVal.data = objTargetIntegrator.GetFields(containerId);
                }
                objTargetIntegrator = null;
            }
            catch (Exception ex)
            {
                objRetVal.message = "An error occurred while trying to serve the request! " + ex.Message + " -- " + ex.StackTrace;
                objRetVal.exitCode = 1;
            }

            return objRetVal;
        }

        [HttpGet]
        [Route("GetLogs")]
        public async Task<Object> GetLogs(Int64? connectionRuleID, Int32? typeFilter, Int32? resultFilter, DateTime? startDate, DateTime? endDate, Int32 pageSize, Int32 pageNo)
        {
            APIResponse objRetVal = new APIResponse(0, "Success");

            try
            {
                var arrData = _databaseContext.DataConnectorLogs.AsQueryable();

                if(connectionRuleID != null && connectionRuleID.HasValue)
                {
                    arrData = arrData.Where(obj => obj.connectionRuleID == connectionRuleID.Value);
                }
                if (resultFilter != null && resultFilter.HasValue)
                {
                    arrData = arrData.Where(obj => obj.Result == resultFilter.Value);
                }
                if (typeFilter != null && typeFilter.HasValue)
                {
                    arrData = arrData.Where(obj => obj.LogType == typeFilter.Value);
                }
                if (startDate != null && startDate.HasValue)
                {
                    arrData = arrData.Where(obj => obj.DateCreated >= startDate.Value);
                }
                if (endDate != null && endDate.HasValue)
                {
                    arrData = arrData.Where(obj => obj.DateCreated <= endDate.Value);
                }
                if (pageSize > 0)
                {
                    arrData = arrData.Skip((pageNo - 1) * pageSize).Take(pageSize);
                }
                objRetVal.data = await arrData.ToListAsync();

                arrData = null;
            }
            catch (Exception ex)
            {
                objRetVal.message = "An error occurred while trying to serve the request! " + ex.Message + " -- " + ex.StackTrace;
                objRetVal.exitCode = 1;
            }

            return objRetVal;
        }
    }
}