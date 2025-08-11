using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using GraphQL.Types;
using UDC.Common.Data.Models;
using UDC.Common.Database.Data.Models.Database;
using UDC.Common.Interfaces;

namespace DataConnectorUI.GraphQL.Types
{
    public class PostSyncTaskCfgType : ObjectGraphType<IPostSyncTask>
    {
        public PostSyncTaskCfgType()
        {

            Field(x => x.PostSyncTaskID, type: typeof(StringGraphType));
            Field(x => x.Name, type:typeof(StringGraphType));
            Field(x => x.Schema, type: typeof(StringGraphType));


        }
    }
}
