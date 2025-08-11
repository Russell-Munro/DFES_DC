using GraphQL.Types;
using UDC.Common.Data.Models;
using UDC.Common.Data.Models.Configuration;
using UDC.Common.Interfaces;

namespace DataConnectorUI.GraphQL.Types
{
    public class IntegratorCfgType : ObjectGraphType<IntegratorCfg>
    {
        public IntegratorCfgType()
        {
            Field(x => x.ContainerID, type: typeof(StringGraphType));
        }
    }
}