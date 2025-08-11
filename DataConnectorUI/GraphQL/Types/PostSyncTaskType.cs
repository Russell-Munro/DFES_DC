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
    public class PostSyncTaskType : ObjectGraphType<PostSyncTask>
    {
        public PostSyncTaskType()
        {


            Field(x => x.PostSyncTaskID, type: typeof(StringGraphType));
            Field(x => x.PlatformID, type:typeof(StringGraphType));
            Field(x => x.Cfg, type: typeof(StringGraphType));


        }
    }
}
