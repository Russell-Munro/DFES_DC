using System;
using GraphQL.Types;
using UDC.Common.Data.Models;
using UDC.Common.Interfaces;

namespace DataConnectorUI.GraphQL.Types
{
    public class IntegratorType : ObjectGraphType<IIntegrator>
    {
        public IntegratorType()
        {
            // The unique identifier is a Guid. Use IdGraphType to properly
            // expose it as a GraphQL ID scalar rather than relying on string
            // serialisation which was causing runtime errors.
            Field<IdGraphType>(
                "integratorID",
                resolve: context =>
                {
                    // Return null if the source or its ID is missing.
                    var id = context.Source?.IntegratorID;

                    // Guid.Empty indicates the identifier has not been assigned.
                    return id.HasValue && id.Value != Guid.Empty ? id : null;
                });

            // Name of the integrator.
            Field(x => x.Name, type: typeof(StringGraphType));

            // Platform-specific configuration represented as a string blob.
            Field(x => x.PlatformConfig, type: typeof(StringGraphType));
        }
    }
}
