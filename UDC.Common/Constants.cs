using System;

namespace UDC.Common
{
    public static class Constants
    {
        public enum LogVerbosityLevels
        {
            All = 1,
            ChangesOnly = 2
        }
        public enum LogTypes
        {
            Trace = 1,
            Warning = 2,
            Error = 3
        }
        public enum LogActions
        {
            NoAction = 0,
            Starting = 1,
            Running = 2,
            Stopped = 3
        }
        public enum LogResults
        {
            Undetermined = 0,
            Success = 1,
            Failed = 2,
            Critical = 3
        }

        public enum EntityStates
        {
            None = 0,
            New = 1,
            Existing = 2,
            Changed = 3
        }
        public enum StateMappingKeys
        {
            TagMappings = 1,
            ContainerMappings = 2,
            ObjectMappings = 3
        }

        public enum FieldDataTypes
        {
            String = 1,
            Integer = 2,
            Decimal = 3,
            DateTime = 4,
            Boolean = 5,
            Guid = 6,
            Taxonomy = 7,
            Binary = 8
        }
        public enum NullActions
        {
            SetDefault = 1,
            AllowNull = 2,
            Skip = 3
        }

        public enum SocketFrameType
        {
            CommandResponse = 1,
            SyncStateUpdate = 2,
            SyncStats = 3
        }
    }
}