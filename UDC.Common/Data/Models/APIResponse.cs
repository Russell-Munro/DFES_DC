using System;

namespace UDC.Common.Data.Models
{
    public class APIResponse
    {
        public Int32 exitCode { get; set; }
        public String message { get; set; }
        public Object data { get; set; }

        public APIResponse() {  }
        public APIResponse(Int32 ExitCode, String Message)
        {
            exitCode = ExitCode;
            message = Message;
        }
        public APIResponse(Int32 ExitCode, String Message, Object Data)
        {
            exitCode = ExitCode;
            message = Message;
            data = Data;
        }
    }
}