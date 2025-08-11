using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using UDC.Common;
using UDC.Common.Database.Data.Models.Database;

namespace DataConnectorUI.GraphQL.Types
{
    public class ConnectionRuleInputType: InputObjectGraphType<ConnectionRule>
    {
        public ConnectionRuleInputType()
        {
            Name = "connectionRuleInput";
            Field<NonNullGraphType<StringGraphType>>("name");
            Field<NonNullGraphType<StringGraphType>>("destinationContainerCfg");

            Field<NonNullGraphType<IntGraphType>>("connectionID");
            Field<NonNullGraphType<BooleanGraphType>>("enabled");
            Field<NonNullGraphType<IntGraphType>>("id");
            Field<NonNullGraphType<StringGraphType>>("sourceContainerCfg"); 
            Field<NonNullGraphType<StringGraphType>>("syncIntervalCron");
            Field<NonNullGraphType<StringGraphType>>("fieldMappings");
            Field<NonNullGraphType<StringGraphType>>("sourcePostSyncTasks");
            Field<NonNullGraphType<StringGraphType>>("destinationPostSyncTasks");
        }
    }
}
