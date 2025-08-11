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
    public class PlatformType : ObjectGraphType<IPlatform>
    {
        public PlatformType()
        {
            Field<IdGraphType>(
                "platformID",
                resolve: context =>
                {
                    // Return null if the source or its ID is missing.
                    var id = context.Source?.PlatformID;

                    // Guid.Empty indicates the identifier has not been assigned.
                    return id.HasValue && id.Value != Guid.Empty ? id : null;
                });
            //Field(x => x.PlatformID, type: typeof(StringGraphType));
            Field(x => x.Name, type: typeof(StringGraphType));

        }
    }
}
