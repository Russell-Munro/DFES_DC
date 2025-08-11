using GraphQL.Types;
using UDC.Common.Data.Models;
using UDC.Common.Interfaces;

namespace DataConnectorUI.GraphQL.Types
{
    public class IntegratorType : ObjectGraphType<IIntegrator>
    {
        public IntegratorType()
        {
            Field(x => x.Name, type: typeof(StringGraphType));
            Field(x => x.IntegratorID, type: typeof(StringGraphType));
            Field(x => x.PlatformConfig, type: typeof(StringGraphType));
        }
    }
}