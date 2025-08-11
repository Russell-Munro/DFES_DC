using DataConnectorUI.GraphQL.Mutations;
using DataConnectorUI.GraphQL.Queries;
using GraphQL.Types; // Required for Schema, ObjectGraphType
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataConnectorUI.GraphQL.Schemas
{
    public class ConnectionSchema: Schema
    {
        public ConnectionSchema(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Query = serviceProvider.GetRequiredService<ConnectionQuery>();
            Mutation = serviceProvider.GetRequiredService<ConnectionMutation>();
        }
    }
}
