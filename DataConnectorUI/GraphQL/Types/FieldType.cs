using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using GraphQL.Types;
using UDC.Common.Data.Models;
using UDC.Common.Database.Data.Models.Database;

namespace DataConnectorUI.GraphQL.Types
{
    public class FieldType : ObjectGraphType<SyncField>
    {
        public FieldType()
        {

            Field<StringGraphType>("id",
                resolve: x => (string.IsNullOrEmpty(x.Source.Id) ? $"ootb-{x.Source.Key}" : x.Source.Id));
 
            Field(x => x.Key, type: typeof(StringGraphType));
            Field<StringGraphType>("title",
                resolve: x => (string.IsNullOrEmpty(x.Source.Title) ? x.Source.Key : x.Source.Title));



        }
    }
}
