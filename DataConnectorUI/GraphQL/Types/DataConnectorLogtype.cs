using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using DataConnectorUI.Repositories;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UDC.Common;
using UDC.Common.Database.Data.Models.Database;
using UDC.DataConnectorCore.Models;

namespace DataConnectorUI.GraphQL.Types
{
    public class DataConnectorLogType : ObjectGraphType<DataConnectorLog>
    {

        public DataConnectorLogType(ConnectionRepository connectionRepository)
        {


            Field(x => x.Id, type: typeof(IntGraphType));
            Field(x => x.connectionRuleID, type: typeof(IntGraphType));
            Field<StringGraphType>("connectionRuleName", resolve: e => connectionRepository.GetConnectionRule(e.Source.connectionRuleID).Name);

            //Field<StringGraphType(x => x.LogType, type: typeof(IntGraphType));
            Field<StringGraphType>("logType", resolve: e => ((Constants.LogTypes)e.Source.LogType).ToString());
            Field<StringGraphType>("logAction", resolve: e => ((Constants.LogActions)e.Source.Action).ToString());
            Field<StringGraphType>("logResult", resolve: e => ((Constants.LogResults)e.Source.Result).ToString());

            //Field(x => x.Result, type: typeof(IntGraphType));
            Field(x => x.Source, type: typeof(StringGraphType));
            Field(x => x.Message, type: typeof(StringGraphType));
            //Field(x => x.Data, type: typeof(StringGraphType));
            //Field<StringGraphType>("Data",resolve:e=> JsonConvert.DeserializeObject(e.Source.Data));


            Field<StringGraphType>("SyncTimeElapsed", resolve: e =>
            {
                //object stats = JsonConvert.DeserializeObject(e.Source.Data);


                JObject data = JObject.Parse(e.Source.Data);
                string timeElapsed = data.GetValue("SyncTimeElapsed")?.ToString();
                if (!string.IsNullOrEmpty(timeElapsed))
                {
                    TimeSpan ts = TimeSpan.Parse(timeElapsed);
                    return GetReadableTimeSpan(ts);
                }

                return null;

            });


            Field<StringGraphType>("Stats", resolve: e =>
            {
                //object stats = JsonConvert.DeserializeObject(e.Source.Data);


                JObject stats = JObject.Parse(e.Source.Data);
                stats.Remove("SyncLog");

                return stats;
            });
            Field<ListGraphType<SyncLogEntryType>>("SyncLog", resolve: e =>
            {
                //object stats = JsonConvert.DeserializeObject(e.Source.Data);

                JToken data = JObject.Parse(e.Source.Data);

                var syncLogsRaw = JsonConvert.SerializeObject(data.SelectToken("SyncLog"));
                IEnumerable<SyncLogEntry> syncLogs = JsonConvert.DeserializeObject<IEnumerable<SyncLogEntry>>(syncLogsRaw);

                return syncLogs;

            });

            Field(x => x.DateCreated, type: typeof(DateTimeGraphType));


        }

        public string GetReadableTimeSpan(TimeSpan value)
        {
            string duration;

            if (value.TotalMinutes < 1)
                duration = value.Seconds + " Seconds";
            else if (value.TotalHours < 1)
                duration = value.Minutes + " Minutes, " + value.Seconds + " Seconds";
            else if (value.TotalDays < 1)
                duration = value.Hours + " Hours, " + value.Minutes + " Minutes";
            else
                duration = value.Days + " Days, " + value.Hours + " Hours";

            if (duration.StartsWith("1 Seconds") || duration.EndsWith(" 1 Seconds"))
                duration = duration.Replace("1 Seconds", "1 Second");

            if (duration.StartsWith("1 Minutes") || duration.EndsWith(" 1 Minutes"))
                duration = duration.Replace("1 Minutes", "1 Minute");

            if (duration.StartsWith("1 Hours") || duration.EndsWith(" 1 Hours"))
                duration = duration.Replace("1 Hours", "1 Hour");

            if (duration.StartsWith("1 Days"))
                duration = duration.Replace("1 Days", "1 Day");

            return duration;
        }
    }
}
