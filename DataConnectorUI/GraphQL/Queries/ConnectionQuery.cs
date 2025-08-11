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
            // connections
            Field<ListGraphType<ConnectionType>>("connections")
                .Description("Returns a list of connections, optionally filtered by ID.")
                .Arguments(new QueryArguments(
                    new QueryArgument<IdGraphType> { Name = "id", Description = "ID of the connection to retrieve." }
                ))
                .Resolve(context =>
                {
                    if (!authSessionService.IsAuthenticated())
                    {
                        context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                        return null;
                    }

                    var query = connectionRepository.GetQuery();
                    var idStr = context.GetArgument<string>("id");

                    if (!string.IsNullOrEmpty(idStr) && int.TryParse(idStr, out var id))
                        return query.Where(r => r.Id == id);

                    return query;
                });

            // connectionRules
            Field<ListGraphType<ConnectionRuleType>>("connectionRules")
                .Description("Returns a list of connection rules, optionally filtered by ID.")
                .Arguments(new QueryArguments(
                    new QueryArgument<IdGraphType> { Name = "id", Description = "ID of the connection rule to retrieve." }
                ))
                .Resolve(context =>
                {
                    if (!authSessionService.IsAuthenticated())
                    {
                        context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                        return null;
                    }

                    var query = connectionRepository.GetConnectionRules();
                    var idStr = context.GetArgument<string>("id");

                    if (!string.IsNullOrEmpty(idStr) && int.TryParse(idStr, out var id))
                        return query.Where(r => r.Id == id).ToList();

                    return query;
                });

            // currentconnection
            Field<ConnectionType>("currentconnection")
                .Description("Returns a single connection, filtered by ID.")
                .Arguments(new QueryArguments(
                    new QueryArgument<IdGraphType> { Name = "id", Description = "ID of the current connection." }
                ))
                .Resolve(context =>
                {
                    if (!authSessionService.IsAuthenticated())
                    {
                        context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                        return null;
                    }

                    var idStr = context.GetArgument<string>("id");
                    if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out var id))
                    {
                        context.Errors.Add(new ExecutionError("Connection ID is required for 'currentconnection'."));
                        return null;
                    }

                    return connectionRepository.GetQuery().FirstOrDefault(r => r.Id == id);
                });

            // currentrule
            Field<ConnectionRuleType>("currentrule")
                .Description("Returns a single connection rule, filtered by ID.")
                .Arguments(new QueryArguments(
                    new QueryArgument<IdGraphType> { Name = "id", Description = "ID of the current rule." }
                ))
                .Resolve(context =>
                {
                    if (!authSessionService.IsAuthenticated())
                    {
                        context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                        return null;
                    }

                    var idStr = context.GetArgument<string>("id");
                    if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out var id))
                    {
                        context.Errors.Add(new ExecutionError("Rule ID is required for 'currentrule'."));
                        return null;
                    }

                    return connectionRepository.GetConnectionRules().FirstOrDefault(r => r.Id == id);
                });

            // sourceFields
            Field<ListGraphType<GraphQL.Types.FieldType>>("sourceFields")
                .Description("Returns a list of source fields for a given connection and container.")
                .Arguments(new QueryArguments(
                    new QueryArgument<IdGraphType> { Name = "connectionId", Description = "ID of the connection." },
                    new QueryArgument<StringGraphType> { Name = "containerId", Description = "ID of the source container." }
                ))
                .Resolve(context =>
                {
                    if (!authSessionService.IsAuthenticated())
                    {
                        context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                        return null;
                    }

                    var connectionIdStr = context.GetArgument<string>("connectionId");
                    var containerId = context.GetArgument<string>("containerId");
                    if (string.IsNullOrEmpty(containerId)) return null;

                    if (int.TryParse(connectionIdStr, out var connectionId))
                        return connectionRepository.GetSourceFields(connectionId, containerId);

                    context.Errors.Add(new ExecutionError("Invalid connectionId."));
                    return null;
                });

            // destinationFields
            Field<ListGraphType<GraphQL.Types.FieldType>>("destinationFields")
                .Description("Returns a list of destination fields for a given connection and container.")
                .Arguments(new QueryArguments(
                    new QueryArgument<IdGraphType> { Name = "connectionId", Description = "ID of the connection." },
                    new QueryArgument<StringGraphType> { Name = "containerId", Description = "ID of the destination container." }
                ))
                .Resolve(context =>
                {
                    if (!authSessionService.IsAuthenticated())
                    {
                        context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                        return null;
                    }

                    var connectionIdStr = context.GetArgument<string>("connectionId");
                    var containerId = context.GetArgument<string>("containerId");
                    if (string.IsNullOrEmpty(containerId)) return null;

                    if (int.TryParse(connectionIdStr, out var connectionId))
                        return connectionRepository.GetDestinationFields(connectionId, containerId);

                    context.Errors.Add(new ExecutionError("Invalid connectionId."));
                    return null;
                });

            // sourceContainers
            Field<ListGraphType<SyncContainerType>>("sourceContainers")
                .Description("Returns a list of source containers for a given connection.")
                .Arguments(new QueryArguments(
                    new QueryArgument<IdGraphType> { Name = "connectionId", Description = "ID of the connection." }
                ))
                .Resolve(context =>
                {
                    if (!authSessionService.IsAuthenticated())
                    {
                        context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                        return null;
                    }

                    var connectionIdStr = context.GetArgument<string>("connectionId");
                    if (int.TryParse(connectionIdStr, out var connectionId))
                        return connectionRepository.GetSourceContainers(connectionId);

                    context.Errors.Add(new ExecutionError("Invalid connectionId."));
                    return null;
                });

            // destinationContainers
            Field<ListGraphType<SyncContainerType>>("destinationContainers")
                .Description("Returns a list of destination containers for a given connection.")
                .Arguments(new QueryArguments(
                    new QueryArgument<IdGraphType> { Name = "connectionId", Description = "ID of the connection." }
                ))
                .Resolve(context =>
                {
                    if (!authSessionService.IsAuthenticated())
                    {
                        context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                        return null;
                    }

                    var connectionIdStr = context.GetArgument<string>("connectionId");
                    if (int.TryParse(connectionIdStr, out var connectionId))
                        return connectionRepository.GetDestinationContainers(connectionId);

                    context.Errors.Add(new ExecutionError("Invalid connectionId."));
                    return null;
                });

            // platforms
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

            // integrators
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

            // sourceIntegrators
            Field<ListGraphType<IntegratorType>>("sourceIntegrators")
                .Description("Returns a list of source integrators for a given platform.")
                .Arguments(new QueryArguments(
                    new QueryArgument<IdGraphType> { Name = "platformID", Description = "ID of the platform." }
                ))
                .Resolve(context =>
                {
                    if (!authSessionService.IsAuthenticated())
                    {
                        context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                        return null;
                    }

                    string platformID = context.GetArgument<string>("platformID");
                    Guid platformGuid = GeneralHelpers.parseGUID(platformID);
                    IPlatform objTargetPlatform = ProviderHelpers.GetPlatform(platformGuid);

                    if (objTargetPlatform != null)
                        return ProviderHelpers.GetSrcIntegrators(objTargetPlatform);

                    return null;
                });

            // destinationIntegrators
            Field<ListGraphType<IntegratorType>>("destinationIntegrators")
                .Description("Returns a list of destination integrators for a given platform.")
                .Arguments(new QueryArguments(
                    new QueryArgument<IdGraphType> { Name = "platformID", Description = "ID of the platform." }
                ))
                .Resolve(context =>
                {
                    if (!authSessionService.IsAuthenticated())
                    {
                        context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                        return null;
                    }

                    string platformID = context.GetArgument<string>("platformID");
                    Guid platformGuid = GeneralHelpers.parseGUID(platformID);
                    IPlatform objTargetPlatform = ProviderHelpers.GetPlatform(platformGuid);

                    if (objTargetPlatform != null)
                        return ProviderHelpers.GetDestIntegrators(objTargetPlatform);

                    return null;
                });

            // logCount
            Field<IntGraphType>("logCount")
                .Description("Returns the count of logs, optionally filtered by connection rule ID.")
                .Arguments(new QueryArguments(
                    new QueryArgument<IdGraphType> { Name = "connectionRuleId", Description = "ID of the connection rule." }
                ))
                .Resolve(context =>
                {
                    if (!authSessionService.IsAuthenticated())
                    {
                        context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                        return null;
                    }
                    var connectionRuleId = context.GetArgument<string>("connectionRuleId");

                    if (!string.IsNullOrEmpty(connectionRuleId))
                        return connectionRepository.GetLogCount(connectionRuleId);

                    return connectionRepository.GetLogCount();
                });

            // logs
            Field<ListGraphType<DataConnectorLogType>>("logs")
                .Description("Returns a paginated list of data connector logs.")
                .Arguments(new QueryArguments(
                    new QueryArgument<IdGraphType> { Name = "connectionRuleId", Description = "ID of the connection rule." },
                    new QueryArgument<IntGraphType> { Name = "pageSize", Description = "Number of logs per page." },
                    new QueryArgument<IntGraphType> { Name = "pageNo", Description = "Page number (0-indexed)." }
                ))
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
                        return connectionRepository.GetLogs(connectionRuleId, pageSize, pageNo);

                    return connectionRepository.GetLogs(pageSize, pageNo);
                });

            // nullActions
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

            // log
            Field<DataConnectorLogType>("log")
                .Description("Returns a single data connector log by ID.")
                .Arguments(new QueryArguments(
                    new QueryArgument<StringGraphType> { Name = "dataConnectorLogID", Description = "ID of the data connector log." }
                ))
                .Resolve(context =>
                {
                    if (!authSessionService.IsAuthenticated())
                    {
                        context.Errors.Add(new ExecutionError("Unauthorised: Authentication required."));
                        return null;
                    }
                    var dataConnectorLogID = context.GetArgument<string>("dataConnectorLogID");
                    return connectionRepository.GetLog(dataConnectorLogID);
                });

            // postSyncTasks
            Field<ListGraphType<PostSyncTaskCfgType>>("postSyncTasks")
                .Description("Returns a list of supported post-sync tasks for a given platform.")
                .Arguments(new QueryArguments(
                    new QueryArgument<IdGraphType> { Name = "platformId", Description = "ID of the platform." }
                ))
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
