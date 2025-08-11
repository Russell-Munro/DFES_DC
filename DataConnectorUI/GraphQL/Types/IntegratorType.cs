using GraphQL.Types;
using UDC.Common.Data.Models;
using UDC.Common.Interfaces;

namespace DataConnectorUI.GraphQL.Types
{
    public class IntegratorType : ObjectGraphType<IIntegrator>
    {
        public IntegratorType()
        {
            Field<IdGraphType>(
                "integratorID",
                resolve: context =>
                {
                    // Return null if the source or its ID is missing.
                    var id = context.Source?.IntegratorID;

                    // Guid.Empty indicates the identifier has not been assigned.
                    return id.HasValue && id.Value != Guid.Empty ? id : null;
                });

            Field(x => x.Name, type: typeof(StringGraphType));
            
            Field(x => x.PlatformConfig, type: typeof(StringGraphType));
        }
    }
}