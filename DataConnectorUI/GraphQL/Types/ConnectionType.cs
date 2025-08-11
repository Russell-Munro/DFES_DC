using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using UDC.Common.Database.Data.Models.Database;

namespace DataConnectorUI.GraphQL.Types
{
    public class ConnectionType: ObjectGraphType<UDC.Common.Database.Data.Models.Database.Connection>
    {
        public ConnectionType()
        {
            Field(x => x.Id);
            Field(x => x.Name, type: typeof(StringGraphType));
            Field(x => x.DestinationPlatformCfg, type: typeof(StringGraphType)); 
            Field(x => x.SourcePlatformCfg, type:typeof(StringGraphType)); 
            Field(x => x.Enabled, type:typeof(BooleanGraphType));
            Field(x => x.DateCreated,type:typeof(DateTimeGraphType));
            Field(x => x.LastUpdated,type:typeof(DateTimeGraphType));
            Field(x => x.ConnectionRules,type:typeof(ListGraphType<ConnectionRuleType>),nullable:true);

            Field(x => x.JsonSourcePlatformCfg,type:typeof(PlatformCfgType));
            Field(x => x.JsonDestinationPlatformCfg, type: typeof(PlatformCfgType));


        }
    }
}
