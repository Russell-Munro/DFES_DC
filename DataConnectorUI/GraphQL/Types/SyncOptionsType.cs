using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using UDC.Common;
using UDC.Common.Data.Models.Configuration;
using UDC.Common.Database.Data.Models.Database;

namespace DataConnectorUI.GraphQL.Types
{
    public class SyncOptionsType : ObjectGraphType<SyncOptions>
    {
        public SyncOptionsType()
        {
            //Field(x => x.NullAction, type:typeof(IntGraphType));
            Field(x => x.MutuallyExclusive, type: typeof(BooleanGraphType));
            Field(x => x.AlwaysUpdateFromSrc, type: typeof(BooleanGraphType));
            Field<IntGraphType>("nullAction", resolve: e => (int) e.Source.NullAction);
            Field<NullActionsType>("nullActionLabel", resolve: e => e.Source.NullAction);

            /*Field<IntGraphType>("nullAction", resolve: (e) =>
                {
                    return Enum.GetName(typeof(Constants.NullActions), e.Source.NullAction);
                });*/

        }
    }
}
