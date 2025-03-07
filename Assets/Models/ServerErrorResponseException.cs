using System;

namespace Assets.Models
{
    public class ServerErrorResponseException: Exception
    {
        public int ErrorCode { get; private set; }

        public ServerErrorResponseException(int errorCode, string message) : base(message)
        {
            this.ErrorCode = errorCode;
        }
    }
}
