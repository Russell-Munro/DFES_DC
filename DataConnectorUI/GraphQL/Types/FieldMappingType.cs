using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using UDC.Common.Data.Models;
using UDC.Common.Database.Data.Models.Database;

namespace DataConnectorUI.GraphQL.Types
{
    public class FieldMappingType : ObjectGraphType<SyncFieldMapping>
    {
        public FieldMappingType()
        {

            Field(x => x.SrcField,type:typeof(FieldType));
            Field(x => x.DestField, type: typeof(FieldType));
            Field(x => x.Options, type: typeof(SyncOptionsType));


        }
    }
}
