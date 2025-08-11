using System;
using System.Collections.Generic;
using System.Linq;
//using System.Runtime.InteropServices.WindowsRuntime; //is nessessary?
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DataConnectorUI.GraphQL.Types;
using DataConnectorUI.Models;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json;
using UDC.Common;
using UDC.Common.Database.AppState;
using UDC.Common.Data;
using UDC.Common.Data.Models;
using UDC.Common.Data.Models.Configuration;
using UDC.Common.Database.Data.Models.Database;
using UDC.Common.Interfaces;
using UDC.DataConnectorCore;
using UDC.DataConnectorCore.Models;
using UDC.Common.Database.Data;

namespace DataConnectorUI.Repositories
{
    public class ConnectionRepository : IHttpContextAccessor
    {
        private readonly DatabaseContext _myDbContext;

        public ConnectionRepository(DatabaseContext myHotelDbContext)
        {
            _myDbContext = myHotelDbContext;

        }



        public async Task<List<T>> GetAll<T>()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Connection, ConnectionModel>();
            });

            return await GetQuery().ProjectTo<T>(config).ToListAsync();
        }


        public Connection CreateConnection()
        {

            var emptyPlatformCfg = new PlatformCfg();
            Connection connection = new Connection
            {
                Enabled = false,
                DateCreated = DateTime.Now,
                LastUpdated = DateTime.Now,
                ConnectionRules = new List<ConnectionRule>(),
                Name = "My new connection",
                SourcePlatformCfg = JsonConvert.SerializeObject(emptyPlatformCfg),
                DestinationPlatformCfg = JsonConvert.SerializeObject(emptyPlatformCfg)
            };
            _myDbContext.Connections.Add(connection);
            _myDbContext.SaveChanges();
            return connection;
        }

        public Connection UpdateConnection(Connection newConnection)
        {

            Connection connection = _myDbContext.Connections.Single(x => x.Id == newConnection.Id);

            connection.LastUpdated = DateTime.Now;
            connection.Name = newConnection.Name;
            connection.Enabled = newConnection.Enabled;
            connection.SourcePlatformCfg = newConnection.SourcePlatformCfg;
            connection.DestinationPlatformCfg = newConnection.DestinationPlatformCfg;

            _myDbContext.SaveChanges();
            return connection;
        }

        public Connection DeleteConnection(Connection connection)
        {
            List<ConnectionRule> rules = _myDbContext.ConnectionRules.Where(x => x.connectionID == connection.Id).ToList();
            List<Int64> ruleIDs = null;
            List<DataConnectorLog> logs = null;

            if (rules != null)
            {
                ruleIDs = rules.Select(obj => obj.Id).ToList();
                logs = _myDbContext.DataConnectorLogs.Where(x => ruleIDs.Contains((Int64)x.connectionRuleID)).ToList();

                if(logs != null)
                {
                    _myDbContext.DataConnectorLogs.RemoveRange(logs);
                }
                _myDbContext.ConnectionRules.RemoveRange(rules);
            }

            _myDbContext.Connections.Remove(connection);
            _myDbContext.SaveChanges();

            logs = null;
            ruleIDs = null;
            rules = null;
            
            return null;
        }



        public async Task<IEnumerable<Connection>> GetAll()
        {
            return await _myDbContext
                .Connections
                .Include(x => x.Id)
                .Include(x => x.Name)
                .Include(x => x.SourcePlatformCfg)
                .Include(x => x.DestinationPlatformCfg)
                .Include(x => x.Enabled)
                .ToListAsync();
        }

        public Connection Get(int id)
        {
            return GetQuery().Single(x => x.Id == id);
        }

        public IIncludableQueryable<Connection, List<ConnectionRule>> GetQuery()
        {
            return _myDbContext
                .Connections
                .Include(x => x.ConnectionRules);
        }


        public DbSet<ConnectionRule> GetConnectionRules()
        {
            return _myDbContext
                .ConnectionRules;
        }

        public ConnectionRule GetConnectionRule(long? connectionRuleId)
        {
            return _myDbContext.ConnectionRules.Single(x => x.Id == connectionRuleId);
        }

        public IOrderedEnumerable<SyncField> GetSourceFields(Int64 connectionId, string containerId)
        {
            IIntegrator objTargetIntegrator = ProviderHelpers.GetIntegrator(connectionId, ProviderHelpers.Targets.Source);
            return objTargetIntegrator?.GetFields(containerId).OrderBy(field => !string.IsNullOrEmpty(field.Title) ? field.Title : field.Key);
        }
        public IOrderedEnumerable<SyncField> GetDestinationFields(Int64 connectionId, string containerId)
        {
            IIntegrator objTargetIntegrator = ProviderHelpers.GetIntegrator(connectionId, ProviderHelpers.Targets.Destination);
            return objTargetIntegrator?.GetFields(containerId).OrderBy(field => !string.IsNullOrEmpty(field.Title) ? field.Title : field.Key);
        }
        public List<SyncContainer> GetSourceContainers(int connectionId)
        {
            IIntegrator objTargetIntegrator = ProviderHelpers.GetIntegrator(connectionId, ProviderHelpers.Targets.Source);
            return objTargetIntegrator?.GetContainers();
        }

        public SyncContainer GetSourceContainer(long connectionId, string containerId)
        {
            if (!string.IsNullOrEmpty(containerId))
            {
                IIntegrator objTargetIntegrator = ProviderHelpers.GetIntegrator(connectionId, ProviderHelpers.Targets.Source);
                var q = objTargetIntegrator?.GetContainers();

                return objTargetIntegrator?.GetContainers().FirstOrDefault(x => x.Id.ToString() == containerId);
            }

            return null;
        }

        public List<SyncContainer> GetDestinationContainers(int connectionId)
        {
            IIntegrator objTargetIntegrator = ProviderHelpers.GetIntegrator(connectionId, ProviderHelpers.Targets.Destination);
            return objTargetIntegrator?.GetContainers();
        }

        public SyncContainer GetDestinationContainer(long connectionId, string containerId)
        {
            if (!string.IsNullOrEmpty(containerId))
            {
                IIntegrator objTargetIntegrator = ProviderHelpers.GetIntegrator(connectionId, ProviderHelpers.Targets.Destination);
                var q = objTargetIntegrator?.GetContainers();
                return objTargetIntegrator?.GetContainers().FirstOrDefault(x => x.Id.ToString() == containerId);
            }
            return null;

        }

        public object UpdateConnectionRule(ConnectionRule newConnectionRule)
        {
            ConnectionRule connectionRule = _myDbContext.ConnectionRules.Single(x => x.Id == newConnectionRule.Id);

            if (connectionRule != null && connectionRule.FieldMappings != newConnectionRule.FieldMappings)
            {
                //If fieldmappings have changed, set dirty to sync all metadata regardless of LastModified evaluation...
                AppStateUtility.SetMetaDataOnlyOverride(newConnectionRule.Id, true);
            }

            connectionRule.Name = newConnectionRule.Name;
            connectionRule.DestinationContainerCfg = newConnectionRule.DestinationContainerCfg;
            connectionRule.Enabled = newConnectionRule.Enabled;
            connectionRule.SourceContainerCfg = newConnectionRule.SourceContainerCfg;
            connectionRule.SyncIntervalCron = newConnectionRule.SyncIntervalCron;
            connectionRule.FieldMappings = newConnectionRule.FieldMappings;
            connectionRule.LastUpdated = DateTime.Now;
            connectionRule.SourceContainerCfg = newConnectionRule.SourceContainerCfg;
            connectionRule.DestinationPostSyncTasks = newConnectionRule.DestinationPostSyncTasks;

            //_myDbContext.ConnectionRules.Update(newConnectionRule);
            _myDbContext.SaveChanges();
            
            return connectionRule;
        }

        public object DeleteConnectionRule(Int64 connectionRuleId)
        {

            var logs = _myDbContext.DataConnectorLogs.Where(x => x.connectionRuleID == connectionRuleId);
            _myDbContext.DataConnectorLogs.RemoveRange(logs);

            var connectionRules = _myDbContext.ConnectionRules.Where(x => x.Id == connectionRuleId);
            _myDbContext.ConnectionRules.RemoveRange(connectionRules);

            _myDbContext.SaveChanges();

            //Don't clutter state table...
            AppStateUtility.DeleteMetaDataOnlyOverride(connectionRuleId);

            return null;
        }

        public ConnectionRule CreateConnectionRule(Int64 connectionId)
        {

            ConnectionRule connectionRule = new ConnectionRule
            {
                Name = "My new connection rule",
                connectionID = connectionId,
                DateCreated = DateTime.Now,
                LastUpdated = DateTime.Now,
                SourceContainerCfg = JsonConvert.SerializeObject(new SyncContainer()),
                DestinationContainerCfg = JsonConvert.SerializeObject(new SyncContainer()),
                SyncIntervalCron = "0 0 0 ? * * *",
                LastExecutedStatus = JsonConvert.SerializeObject(new SyncStatus()),
                FieldMappings = JsonConvert.SerializeObject(new List<SyncFieldMapping>()),
                Enabled = false

            };
            //            Connection connection = new Connection {Name = "foo2"};
            _myDbContext.ConnectionRules.Add(connectionRule);
            _myDbContext.SaveChanges();
            return connectionRule;
        }

        public IQueryable<DataConnectorLog> GetLogs(string connectionRuleId, Int32 pageSize, Int32 pageNo)
        {
            //return _myDbContext.DataConnectorLogs;
            var arrData = _myDbContext.DataConnectorLogs.Where(x => x.connectionRuleID.ToString() == connectionRuleId).OrderByDescending(x => x.DateCreated);
            if (pageSize >= 0)
            {
                arrData.Skip((pageNo - 1) * pageSize).Take(pageSize);
            }
            return arrData;
        }

        public List<DataConnectorLog> GetLogs(Int32 pageSize, Int32 pageNo)


        {
            var arrData = _myDbContext.DataConnectorLogs.OrderByDescending(x => x.DateCreated).AsQueryable<DataConnectorLog>();

            if (pageSize >= 0)
            {
                arrData = arrData.Skip((pageNo) * pageSize).Take(pageSize);
            }

            //objRetVal.data = arrData.ToList();

            return arrData.ToList();
        }


        public int GetLogCount()
        {
            return _myDbContext.DataConnectorLogs.Count();
        }

        public int GetLogCount(string connectionRuleId)
        {
            return _myDbContext.DataConnectorLogs.Count(x => x.connectionRuleID.ToString() == connectionRuleId);
        }

        public IQueryable<DataConnectorLog> GetLog(string dataConnectorLogID)
        {
            //return _myDbContext.DataConnectorLogs;
            return _myDbContext.DataConnectorLogs.Where(x => x.Id.ToString() == dataConnectorLogID);
        }


        public HttpContext HttpContext { get; set; }


        public Array GetNullActions()
        {

            Array values = Enum.GetValues(typeof(Constants.NullActions));
            return values;
        }
    }
}