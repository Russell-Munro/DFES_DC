using System;
using UDC.Common.Data.Models;
using static UDC.Common.Constants;

namespace SchedulingAgent.Models
{
    public class SocketResponse : APIResponse
    {
        public SocketFrameType socketFrameType { get; set; }

        public SocketResponse() { }
        public SocketResponse(SocketFrameType SocketFrameType, Int32 ExitCode, String Message) : base(ExitCode, Message)
        {
            this.socketFrameType = SocketFrameType;
        }
        public SocketResponse(SocketFrameType SocketFrameType, Int32 ExitCode, String Message, Object Data) : base(ExitCode, Message, Data)
        {
            this.socketFrameType = SocketFrameType;
        }
    }
}