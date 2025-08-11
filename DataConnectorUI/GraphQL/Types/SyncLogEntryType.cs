using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using UDC.Common;
using UDC.Common.Data.Models.Configuration;
using UDC.Common.Database.Data.Models.Database;
using UDC.DataConnectorCore.Models;

namespace DataConnectorUI.GraphQL.Types
{
    public class SyncLogEntryType : ObjectGraphType<SyncLogEntry>
    {
        public SyncLogEntryType()
        {
            Field(x => x.Source, type: typeof(StringGraphType));
            Field(x => x.SourceDesc, type: typeof(StringGraphType));
            Field(x => x.Msg, type: typeof(StringGraphType));
            Field(x => x.Exception, type: typeof(StringGraphType));
            Field(x => x.TimeStamp, type: typeof(DateTimeGraphType));
            Field<StringGraphType>("logType", resolve: e => ((Constants.LogTypes)e.Source.LogType).ToString());
            Field<StringGraphType>("logAction", resolve: e => ((Constants.LogActions)e.Source.LogAction).ToString());
            Field<StringGraphType>("logResult", resolve: e => ((Constants.LogResults)e.Source.LogResult).ToString());

    }
    }
}
