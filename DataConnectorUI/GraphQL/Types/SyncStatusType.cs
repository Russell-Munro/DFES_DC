using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using UDC.Common.Data.Models.Configuration;
using UDC.Common.Database.Data.Models.Database;
using UDC.DataConnectorCore.Models;

namespace DataConnectorUI.GraphQL.Types
{
    public class SyncStatusType : ObjectGraphType<SyncStatus>
    {
        public SyncStatusType()
        {
            Field(x => x.ExecutionStatus, type: typeof(StringGraphType));
            Field(x => x.TagsCreated, type: typeof(IntGraphType));
            Field(x => x.TagsUpdated, type: typeof(IntGraphType));
            Field(x => x.TagsSkipped, type: typeof(IntGraphType));
            Field(x => x.TagsDeleted, type: typeof(IntGraphType));
            Field(x => x.ContainersCreated, type: typeof(IntGraphType));
            Field(x => x.ContainersUpdated, type: typeof(IntGraphType));
            Field(x => x.ContainersSkipped, type: typeof(IntGraphType));
            Field(x => x.ContainersDeleted, type: typeof(IntGraphType));
            Field(x => x.ObjectsCreated, type: typeof(IntGraphType));
            Field(x => x.ObjectsUpdated, type: typeof(IntGraphType));
            Field(x => x.ObjectsSkipped, type: typeof(IntGraphType));
            Field(x => x.ObjectsDeleted, type: typeof(IntGraphType));
            //Field(x => x.SyncTimeElapsed, type: typeof(TimeSpan));
            Field(x => x.TagsCreated, type: typeof(IntGraphType));
            Field(x => x.SyncLog, type: typeof(ListGraphType<SyncLogEntryType>));



    }
    }
}
