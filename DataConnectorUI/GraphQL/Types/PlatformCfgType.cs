using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using UDC.Common.Data.Models;
using UDC.Common.Data.Models.Configuration;
using UDC.Common.Database.Data.Models.Database;
using UDC.Common.Interfaces;

namespace DataConnectorUI.GraphQL.Types
{
    public class PlatformCfgType : ObjectGraphType<PlatformCfg>
    {
        public PlatformCfgType()
        {
            Field(x => x.EndPointURL, type: typeof(StringGraphType));
            Field(x => x.ServiceDomain, type: typeof(StringGraphType));
            Field(x => x.IntegratorID, type: typeof(StringGraphType));
            Field(x => x.ServicePassword, type: typeof(StringGraphType));
            Field(x => x.ServiceUsername, type: typeof(StringGraphType));
            Field(x => x.PlatformID, type: typeof(StringGraphType));

        }
    }
}
