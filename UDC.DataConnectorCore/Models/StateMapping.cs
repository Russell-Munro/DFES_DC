using System;

namespace UDC.DataConnectorCore.Models
{
    public class StateMapping
    {
        public String SrcId { get; set; }
        public String DestId { get; set; }

        public String SrcLabel { get; set; }
        public String DestLabel { get; set; }

        public StateMapping()
        {

        }
        public StateMapping(String srcId, String destId)
        {
            this.SrcId = srcId;
            this.DestId = destId;
        }
        public StateMapping(String srcId, String destId, String srcLabel, String destLabel)
        {
            this.SrcId = srcId;
            this.DestId = destId;

            this.SrcLabel = srcLabel;
            this.DestLabel = destLabel;
        }
    }
}