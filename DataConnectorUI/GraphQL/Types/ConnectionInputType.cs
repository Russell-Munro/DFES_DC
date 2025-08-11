using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using UDC.Common;

namespace DataConnectorUI.GraphQL.Types
{
    public class ConnectionInputType: InputObjectGraphType<UDC.Common.Database.Data.Models.Database.Connection>
    {
        public ConnectionInputType()
        {
            Name = "connectionInput";
            Field<StringGraphType>("name");
            Field<IntGraphType>("id");
            Field<BooleanGraphType>("enabled");
            Field<StringGraphType>("sourcePlatformCfg");
            Field<StringGraphType>("destinationPlatformCfg");

        }
    }
}
