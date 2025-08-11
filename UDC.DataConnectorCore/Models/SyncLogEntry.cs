using System;
using Newtonsoft.Json;
using static UDC.Common.Constants;

namespace UDC.DataConnectorCore.Models
{
    public class DocStats
    {
        public int Total { get; set; }
        public int Current { get; set; }
    }

    public class SyncLogEntry
    {
        public LogTypes LogType { get; set; }
        public LogActions LogAction { get; set; }
        public LogResults LogResult { get; set; }

        public String Source { get; set; }
        public String SourceDesc { get; set; }
        public String Msg { get; set; }
        public String Exception { get; set; }
        public String Data { get; set; }
        public DateTime TimeStamp { get; set; }

        public Object DocStats
        {
            get
            {
                try{
                if(this.Data != null){
                    return JsonConvert.DeserializeObject<DocStats>(this.Data);    
                }
                }catch{
                return null;

                }

                return null;
            }
        }

        public SyncLogEntry()
        {

        }
        public SyncLogEntry(String source, String sourceDesc, String msg, String exception, String data)
        {
            this.LogType = LogTypes.Trace;
            this.Source = source;
            this.SourceDesc = sourceDesc;
            this.Msg = msg;
            this.Exception = exception;
            this.Data = data;
            this.TimeStamp = DateTime.UtcNow;
        }
        public SyncLogEntry(LogTypes logType, String source, String sourceDesc, String msg, String exception, String data)
        {
            this.LogType = logType;
            this.Source = source;
            this.SourceDesc = sourceDesc;
            this.Msg = msg;
            this.Exception = exception;
            this.Data = data;
            this.TimeStamp = DateTime.UtcNow;
        }
        public SyncLogEntry(LogTypes logType, LogActions logAction, LogResults logResult, String source, String sourceDesc, String msg, String exception, String data)
        {
            this.LogType = logType;
            this.LogAction = logAction;
            this.LogResult = logResult;

            this.Source = source;
            this.SourceDesc = sourceDesc;
            this.Msg = msg;
            this.Exception = exception;
            this.Data = data;
            this.TimeStamp = DateTime.UtcNow;
        }
    }
}