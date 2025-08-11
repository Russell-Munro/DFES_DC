using System;
using GraphQL.Types;
using GraphQL;
using UDC.Common.Database.Data.Models.Database;
using DataConnectorUI.GraphQL.Types;
using DataConnectorUI.Repositories;
using DataConnectorUI.Services;

namespace DataConnectorUI.GraphQL.Mutations
{
    public class ConnectionMutation: ObjectGraphType
    {
        public ConnectionMutation(ConnectionRepository repository, AuthSessionService authSessionService)
        {
            Field<ConnectionType>("createConnection",
                resolve: context =>
                {
                    if (!authSessionService.IsAuthenticated())
                    {
                        throw new Exception("Unauthorised");
                    }
                    return repository.CreateConnection();
                });
            Field<ConnectionType>("updateConnection",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<ConnectionInputType>> { Name = "connection" }),
                resolve: context =>
                {
                    if (!authSessionService.IsAuthenticated())
                    {
                        throw new Exception("Unauthorised");
                    }
                    var connection = context.GetArgument<Connection>("connection");
                    return repository.UpdateConnection(connection);
                });
            Field<ConnectionType>("deleteConnection",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<ConnectionInputType>> { Name = "connection" }),
                resolve: context =>
                {
                    if (!authSessionService.IsAuthenticated())
                    {
                        throw new Exception("Unauthorised");
                    }
                    var connection = context.GetArgument<Connection>("connection");
                    return repository.DeleteConnection(connection);
                });
            Field<ConnectionRuleType>("updateRule",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<ConnectionRuleInputType>> { Name = "connectionRule" }),
                resolve: context =>
                {
                    if (!authSessionService.IsAuthenticated())
                    {
                        throw new Exception("Unauthorised");
                    }
                    var connectionRule = context.GetArgument<ConnectionRule>("connectionRule");
                    return repository.UpdateConnectionRule(connectionRule);
                });
            Field<ConnectionRuleType>("createConnectionRule",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "connectionId" }),
                resolve: context =>
                {
                    if (!authSessionService.IsAuthenticated())
                    {
                        throw new Exception("Unauthorised");
                    }
                    var connectionId = context.GetArgument<Int64>("connectionId");
                    return repository.CreateConnectionRule(connectionId);
                });
            Field<ConnectionRuleType>("deleteConnectionRule",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "connectionRuleId" }),
                resolve: context =>
                {
                    if (!authSessionService.IsAuthenticated())
                    {
                        throw new Exception("Unauthorised");
                    }
                    var connectionRuleId = context.GetArgument<Int64>("connectionRuleId");
                    return repository.DeleteConnectionRule(connectionRuleId);
                });
        }
    }
}