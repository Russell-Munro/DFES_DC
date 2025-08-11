using System;
using System.Collections.Generic;

namespace UDC.DataConnectorCore.Models
{
    public class SyncStatus
    {
        public String ExecutionStatus { get; set; }

        public Int32 TagsCreated { get; set; }
        public Int32 TagsUpdated { get; set; }
        public Int32 TagsSkipped { get; set; }
        public Int32 TagsDeleted { get; set; }

        public Int32 ContainersCreated { get; set; }
        public Int32 ContainersUpdated { get; set; }
        public Int32 ContainersSkipped { get; set; }
        public Int32 ContainersDeleted { get; set; }

        public Int32 ObjectsCreated { get; set; }
        public Int32 ObjectsUpdated { get; set; }
        public Int32 ObjectsSkipped { get; set; }
        public Int32 ObjectsDeleted { get; set; }

        public Int64 BinaryTransferedBytes { get; set; }

        public Int32 Warnings { get; set; }
        public Int32 Errors { get; set; }

        public TimeSpan SyncTimeElapsed { get; set; }

        public List<SyncLogEntry> SyncLog { get; set; }

        public SyncStatus()
        {
            this.ExecutionStatus = "";

            this.TagsCreated = 0;
            this.TagsUpdated = 0;
            this.TagsSkipped = 0;
            this.TagsDeleted = 0;

            this.ContainersCreated = 0;
            this.ContainersUpdated = 0;
            this.ContainersSkipped = 0;
            this.ContainersDeleted = 0;

            this.ObjectsCreated = 0;
            this.ObjectsUpdated = 0;
            this.ObjectsSkipped = 0;
            this.ObjectsDeleted = 0;

            this.BinaryTransferedBytes = 0;

            this.Warnings = 0;
            this.Errors = 0;

            this.SyncTimeElapsed = TimeSpan.FromSeconds(0);
        }
    }
}