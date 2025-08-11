using System;

using UDC.Common.Database.Data;
using UDC.Common.Database.Data.Models.Database;

using static UDC.Common.Constants;

namespace UDC.Common.Database.Logging
{
    public class Logger
    {
        public static void Write(LogTypes logType, LogActions logAction, LogResults result, String source, String message, String data)
        {
            Write(null, logType, logAction, result, source, message, data);
        }
        public static void Write(Int64? ruleId, String source, String message, String data)
        {
            Write(ruleId, LogTypes.Trace, LogActions.NoAction, LogResults.Undetermined, source, message, data);
        }
        public static void Write(Int64? ruleId, LogTypes logType, LogActions logAction, LogResults result, String source, String message, String data)
        {
            using (DatabaseContext objDB = new DatabaseContext())
            {
                DataConnectorLog objEntry = new DataConnectorLog();

                objEntry.connectionRuleID = ruleId;
                objEntry.LogType = ((Int32)logType);
                objEntry.Action = ((Int32)logAction);
                objEntry.Result = ((Int32)result);
                objEntry.Source = source;
                objEntry.Message = message;
                objEntry.Data = data;

                objEntry.DateCreated = DateTime.UtcNow;

                objDB.DataConnectorLogs.Add(objEntry);
                objDB.SaveChanges();

                objEntry = null;
            }
        }
    }
}