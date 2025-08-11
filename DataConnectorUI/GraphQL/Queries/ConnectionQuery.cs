using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Types; 
using GraphQL; 

using UDC.Common; 
using UDC.Common.Interfaces; 
using UDC.DataConnectorCore; 

using DataConnectorUI.GraphQL.Types; 
using DataConnectorUI.Repositories; 
using DataConnectorUI.Services; 


namespace DataConnectorUI.GraphQL.Queries
{
    /// <summary>
    /// Represents the root Query type for the GraphQL API.
    /// Defines all top-level fields for fetching data related to connections, rules, logs, etc.
    /// </summary>
    public class ConnectionQuery : ObjectGraphType
    {
        public ConnectionQuery(ConnectionRepository connectionRepository, AuthSessionService authSessionService)
        {
            //Field<StringGraphType>("hello")
            // .Description("A simple field to test the API.")
            // .Resolve(context => "Hello World!");

                    

                // Field: connections
                // Returns a list of ConnectionType, optionally filtered by ID.
                Field<ListGraphType<ConnectionType>>(
                    "connections") // Use the overload that takes only the name
                    .Description("Returns a list of connections, optionally filtered by ID.")
                    .Arguments(new QueryArguments(new List<QueryArgument>
                    {
                        new QueryArgument<IntGraphType> // Assuming connection IDs are integers
                        {
                            Name = "id",
                            Description = "ID of the connection to retrieve."
                        }
                    }))
                    .Resolve(context => // Use .Resolve() for the resolver
                    {
                        // Authentication check
                        if (!authSessionService.IsAuthenticated())
                        {
                            context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                            return null;
                        }

                        var query = connectionRepository.GetQuery();
                        var connectionId = context.GetArgument<int?>("id");

                        if (connectionId.HasValue)
                            return connectionRepository.GetQuery().Where(r => r.Id == connectionId.Value);

                        return query;
                    });

                // Field: connectionRules
                // Returns a list of ConnectionRuleType, optionally filtered by ID.
                Field<ListGraphType<ConnectionRuleType>>("connectionRules")
                    .Description("Returns a list of connection rules, optionally filtered by ID.")
                    .Arguments(new QueryArguments(new List<QueryArgument>
                    {
                        new QueryArgument<IntGraphType> // Assuming connection rule IDs are integers
                        {
                            Name = "id",
                            Description = "ID of the connection rule to retrieve."
                        }
                    }))
                    .Resolve(context =>
                    {
                        if (!authSessionService.IsAuthenticated())
                        {
                            context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                            return null;
                        }
                        var query = connectionRepository.GetConnectionRules();

                        var connectionRuleId = context.GetArgument<int?>("id");
                        if (connectionRuleId.HasValue)
                            return connectionRepository.GetConnectionRules().Where(r => r.Id == connectionRuleId.Value)
                            .ToList();

                        return query;
                    });

                // Field: currentconnection
                // Returns a single ConnectionType, filtered by ID.
                Field<ConnectionType>("currentconnection")
                    .Description("Returns a single connection, filtered by ID.")
                    .Arguments(new QueryArguments(new List<QueryArgument>
                    {
                        new QueryArgument<IntGraphType> // Assuming connection IDs are integers
                        {
                            Name = "id",
                            Description = "ID of the current connection."
                        }
                    }))
                    .Resolve(context =>
                    {
                        if (!authSessionService.IsAuthenticated())
                        {
                            context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                            return null;
                        }
                        var connectionId = context.GetArgument<int?>("id");

                        if (!connectionId.HasValue)
                        {
                            context.Errors.Add(new ExecutionError("Connection ID is required for 'currentconnection'."));
                            return null;
                        }

                        return connectionRepository.GetQuery().FirstOrDefault(r => r.Id == connectionId.Value);
                    });

                // Field: currentrule
                // Returns a single ConnectionRuleType, filtered by ID.
                Field<ConnectionRuleType>("currentrule")
                    .Description("Returns a single connection rule, filtered by ID.")
                    .Arguments(new QueryArguments(new List<QueryArgument>
                    {
                        new QueryArgument<IntGraphType> // Assuming rule IDs are integers
                        {
                            Name = "id",
                            Description = "ID of the current rule."
                        }
                    }))
                    .Resolve(context =>
                    {
                        if (!authSessionService.IsAuthenticated())
                        {
                            context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                            return null;
                        }
                        var ruleId = context.GetArgument<int?>("id");

                        if (!ruleId.HasValue)
                        {
                            context.Errors.Add(new ExecutionError("Rule ID is required for 'currentrule'."));
                            return null;
                        }

                        return connectionRepository.GetConnectionRules().FirstOrDefault(r => r.Id == ruleId.Value);
                    });

                // Field: sourceFields
                // Returns a list of FieldType.
                Field<ListGraphType<GraphQL.Types.FieldType>>("sourceFields")
                    .Description("Returns a list of source fields for a given connection and container.")
                    .Arguments(new QueryArguments(new List<QueryArgument>
                    {
                        new QueryArgument<IntGraphType> { Name = "connectionId", Description = "ID of the connection." },
                        new QueryArgument<StringGraphType> { Name = "containerId", Description = "ID of the source container." }
                    }))
                    .Resolve(context =>
                    {
                        if (!authSessionService.IsAuthenticated())
                        {
                            context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                            return null;
                        }
                        var connectionId = context.GetArgument<int>("connectionId");
                        var containerId = context.GetArgument<string>("containerId");

                        if (!string.IsNullOrEmpty(containerId))
                        {
                            return connectionRepository.GetSourceFields(connectionId, containerId);
                        }
                        return null;
                    });

                // Field: destinationFields
                // Returns a list of FieldType.
                Field<ListGraphType<GraphQL.Types.FieldType>>("destinationFields")
                    .Description("Returns a list of destination fields for a given connection and container.")
                    .Arguments(new QueryArguments(new List<QueryArgument>
                    {
                        new QueryArgument<IntGraphType> { Name = "connectionId", Description = "ID of the connection." },
                        new QueryArgument<StringGraphType> { Name = "containerId", Description = "ID of the destination container." }
                    }))
                    .Resolve(context =>
                    {
                        if (!authSessionService.IsAuthenticated())
                        {
                            context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                            return null;
                        }
                        var connectionId = context.GetArgument<int>("connectionId");
                        var containerId = context.GetArgument<string>("containerId");

                        if (!string.IsNullOrEmpty(containerId))
                        {
                            return connectionRepository.GetDestinationFields(connectionId, containerId);
                        }
                        return null;
                    });

                // Field: sourceContainers
                // Returns a list of SyncContainerType.
                Field<ListGraphType<SyncContainerType>>("sourceContainers")
                    .Description("Returns a list of source containers for a given connection.")
                    .Arguments(new QueryArguments(new List<QueryArgument>
                    {
                        new QueryArgument<IntGraphType> { Name = "connectionId", Description = "ID of the connection." }
                    }))
                    .Resolve(context =>
                    {
                        if (!authSessionService.IsAuthenticated())
                        {
                            context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                            return null;
                        }
                        var connectionId = context.GetArgument<int>("connectionId");
                        return connectionRepository.GetSourceContainers(connectionId);
                    });

                // Field: destinationContainers
                // Returns a list of SyncContainerType.
                Field<ListGraphType<SyncContainerType>>("destinationContainers")
                    .Description("Returns a list of destination containers for a given connection.")
                    .Arguments(new QueryArguments(new List<QueryArgument>
                    {
                        new QueryArgument<IntGraphType> { Name = "connectionId", Description = "ID of the connection." }
                    }))
                    .Resolve(context =>
                    {
                        if (!authSessionService.IsAuthenticated())
                        {
                            context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                            return null;
                        }
                        var connectionId = context.GetArgument<int>("connectionId");
                        return connectionRepository.GetDestinationContainers(connectionId);
                    });

                // Field: platforms
                // Returns a list of PlatformType.
                Field<ListGraphType<PlatformType>>("platforms")
                    .Description("Returns a list of supported platforms.")
                    .Resolve(context =>
                    {
                        if (!authSessionService.IsAuthenticated())
                        {
                            context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                            return null;
                        }
                        return ProviderHelpers.GetPlatformInstances();
                    });

                // Field: integrators
                // Returns a list of IntegratorType.
                Field<ListGraphType<IntegratorType>>("integrators")
                    .Description("Returns a list of all integrators.")
                    .Resolve(context =>
                    {
                        if (!authSessionService.IsAuthenticated())
                        {
                            context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                            return null;
                        }
                        return ProviderHelpers.GetIntegratorInstances();
                    });

                // Field: sourceIntegrators
                // Returns a list of IntegratorType, filtered by platformID.
                Field<ListGraphType<IntegratorType>>("sourceIntegrators")
                    .Description("Returns a list of source integrators for a given platform.")
                    .Arguments(new QueryArguments(new List<QueryArgument>
                    {
                        new QueryArgument<StringGraphType> { Name = "platformID", Description = "ID of the platform." }
                    }))
                    .Resolve(context =>
                    {
                        if (!authSessionService.IsAuthenticated())
                        {
                            context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                            return null;
                        }
                        String platformID = context.GetArgument<String>("platformID");
                        Guid platformGuid = GeneralHelpers.parseGUID(platformID);
                        IPlatform objTargetPlatform = ProviderHelpers.GetPlatform(platformGuid);

                        if (objTargetPlatform != null)
                        {
                            return ProviderHelpers.GetSrcIntegrators(objTargetPlatform);
                        }
                        return null;
                    });

                // Field: destinationIntegrators
                // Returns a list of IntegratorType, filtered by platformID.
                Field<ListGraphType<IntegratorType>>("destinationIntegrators")
                    .Description("Returns a list of destination integrators for a given platform.")
                    .Arguments(new QueryArguments(new List<QueryArgument>
                    {
                        new QueryArgument<StringGraphType> { Name = "platformID", Description = "ID of the platform." }
                    }))
                    .Resolve(context =>
                    {
                        if (!authSessionService.IsAuthenticated())
                        {
                            context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                            return null;
                        }
                        String platformID = context.GetArgument<String>("platformID");
                        Guid platformGuid = GeneralHelpers.parseGUID(platformID);
                        IPlatform objTargetPlatform = ProviderHelpers.GetPlatform(platformGuid);

                        if (objTargetPlatform != null)
                        {
                            return ProviderHelpers.GetDestIntegrators(objTargetPlatform);
                        }
                        return null;
                    });

                // Field: logCount
                // Returns an integer count of logs, optionally filtered by connectionRuleId.
                Field<IntGraphType>("logCount")
                    .Description("Returns the count of logs, optionally filtered by connection rule ID.")
                    .Arguments(new QueryArguments(new List<QueryArgument>
                    {
                        new QueryArgument<StringGraphType> { Name = "connectionRuleId", Description = "ID of the connection rule." }
                    }))
                    .Resolve(context =>
                    {
                        if (!authSessionService.IsAuthenticated())
                        {
                            context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                            return null;
                        }
                        var connectionRuleId = context.GetArgument<string>("connectionRuleId");

                        if (!string.IsNullOrEmpty(connectionRuleId))
                        {
                            return connectionRepository.GetLogCount(connectionRuleId);
                        }
                        return connectionRepository.GetLogCount();
                    });

                // Field: logs
                // Returns a list of DataConnectorLogType, with pagination.
                Field<ListGraphType<DataConnectorLogType>>("logs")
                    .Description("Returns a paginated list of data connector logs.")
                    .Arguments(new QueryArguments(new List<QueryArgument>
                    {
                        new QueryArgument<StringGraphType> { Name = "connectionRuleId", Description = "ID of the connection rule." },
                        new QueryArgument<IntGraphType> { Name = "pageSize", Description = "Number of logs per page." },
                        new QueryArgument<IntGraphType> { Name = "pageNo", Description = "Page number (0-indexed)." }
                    }))
                    .Resolve(context =>
                    {
                        if (!authSessionService.IsAuthenticated())
                        {
                            context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                            return null;
                        }
                        var connectionRuleId = context.GetArgument<string>("connectionRuleId");

                        int pageSize = context.GetArgument<int>("pageSize") > 0 ? context.GetArgument<int>("pageSize") : 10;
                        int pageNo = context.GetArgument<int>("pageNo");

                        if (!string.IsNullOrEmpty(connectionRuleId))
                        {
                            return connectionRepository.GetLogs(connectionRuleId, pageSize, pageNo);
                        }

                        return connectionRepository.GetLogs(pageSize, pageNo);
                    });

                // Field: nullActions
                // Returns a list of NullActionsType.
                Field<ListGraphType<NullActionsType>>("nullActions")
                    .Description("Returns a list of null actions.")
                    .Resolve(context =>
                    {
                        if (!authSessionService.IsAuthenticated())
                        {
                            context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                            return null;
                        }
                        return connectionRepository.GetNullActions();
                    });

                // Field: log (singular)
                // Returns a single DataConnectorLogType.
                Field<DataConnectorLogType>("log")
                    .Description("Returns a single data connector log by ID.")
                    .Arguments(new QueryArguments(new List<QueryArgument>
                    {
                        new QueryArgument<StringGraphType> { Name = "dataConnectorLogID", Description = "ID of the data connector log." }
                    }))
                    .Resolve(context =>
                    {
                        if (!authSessionService.IsAuthenticated())
                        {
                            context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                            return null;
                        }
                        var dataConnectorLogID = context.GetArgument<string>("dataConnectorLogID");

                        // Assuming GetLog returns a single item. If it returns IEnumerable, use .FirstOrDefault()
                        return connectionRepository.GetLog(dataConnectorLogID);
                    });

                // Field: postSyncTasks
                // Returns a list of PostSyncTaskCfgType.
                Field<ListGraphType<PostSyncTaskCfgType>>("postSyncTasks")
                    .Description("Returns a list of supported post-sync tasks for a given platform.")
                    .Arguments(new QueryArguments(new List<QueryArgument>
                    {
                        new QueryArgument<StringGraphType> { Name = "platformId", Description = "ID of the platform." }
                    }))
                    .Resolve(context =>
                    {
                        if (!authSessionService.IsAuthenticated())
                        {
                            context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                            return null;
                        }
                        var platformId = context.GetArgument<string>("platformId");
                        Guid platformGuid = GeneralHelpers.parseGUID(platformId);
                        IPlatform objTargetPlatform = ProviderHelpers.GetPlatform(platformGuid);
                        List<IPostSyncTask> arrPostSyncTasks = ProviderHelpers.GetSupportedPostSyncTasks(objTargetPlatform);
                        return arrPostSyncTasks;
                    });
            }
        }
}
