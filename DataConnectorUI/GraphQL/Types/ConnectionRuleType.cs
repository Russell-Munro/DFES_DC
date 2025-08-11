using GraphQL.Types;
using DataConnectorUI.Repositories;
using UDC.Common.Data.Models;
using UDC.Common.Data.Models.Configuration;
using UDC.Common.Database.Data.Models.Database;

namespace DataConnectorUI.GraphQL.Types
{
    public class ConnectionRuleType : ObjectGraphType<ConnectionRule>
    {
        public ConnectionRuleType()
        {
            Name = "ConnectionRule";

            // Direct property fields (no resolver needed)
            Field(x => x.Id);
            Field(x => x.Name);
            Field(x => x.Enabled, type: typeof(BooleanGraphType));
            Field(x => x.DateCreated, type: typeof(DateTimeGraphType));
            Field(x => x.LastUpdated, type: typeof(DateTimeGraphType));
            Field(x => x.SyncIntervalCron, type: typeof(StringGraphType));
            Field(x => x.LastExecuted, type: typeof(DateTimeGraphType));
            Field(x => x.SourceContainerCfg, type: typeof(StringGraphType));
            Field(x => x.DestinationContainerCfg, type: typeof(StringGraphType));
            Field(x => x.LastExecutedStatus, type: typeof(StringGraphType));
            Field(x => x.FieldMappings, type: typeof(StringGraphType));
            Field(x => x.connectionID);

            // Custom/resolver fields - use FieldBuilder with .Resolve(...)
            Field<ListGraphType<FieldMappingType>>("jsonFieldMappings")
                .Resolve(context => context.Source.JsonFieldMappings);

            Field<IntegratorCfgType>("jsonSourceContainerCfg")
                .Resolve(context => context.Source.JsonSourceContainerCfg);

            Field<IntegratorCfgType>("jsonDestinationContainerCfg")
                .Resolve(context => context.Source.JsonDestinationContainerCfg);

            Field<StringGraphType>("sourceContainerCfgContainerId")
                .Resolve(context => context.Source.JsonSourceContainerCfg?.ContainerID);

            Field<StringGraphType>("destinationContainerCfgContainerId")
                .Resolve(context => context.Source.JsonDestinationContainerCfg?.ContainerID);

            Field<StringGraphType>("sourceContainerLabel")
                .Resolve(context =>
                {
                    var repo = context.RequestServices.GetService(typeof(ConnectionRepository)) as ConnectionRepository;
                    var src = context.Source;
                    return src != null && src.JsonSourceContainerCfg != null
                        ? repo?.GetSourceContainer(src.connectionID, src.JsonSourceContainerCfg.ContainerID)?.Name
                        : null;
                });

            Field<StringGraphType>("destinationContainerLabel")
                .Resolve(context =>
                {
                    var repo = context.RequestServices.GetService(typeof(ConnectionRepository)) as ConnectionRepository;
                    var src = context.Source;
                    return src != null && src.JsonDestinationContainerCfg != null
                        ? repo?.GetDestinationContainer(src.connectionID, src.JsonDestinationContainerCfg.ContainerID)?.Name
                        : null;
                });

            Field<ListGraphType<StringGraphType>>("sourcePostSyncTasks")
                .Resolve(context =>
                    !string.IsNullOrWhiteSpace(context.Source.SourcePostSyncTasks)
                        ? Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(context.Source.SourcePostSyncTasks)
                        : new List<string>()
                );

            Field<ListGraphType<StringGraphType>>("destinationPostSyncTasks")
                .Resolve(context =>
                    !string.IsNullOrWhiteSpace(context.Source.DestinationPostSyncTasks)
                        ? Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(context.Source.DestinationPostSyncTasks)
                        : new List<string>()
                );
        }
    }
}
