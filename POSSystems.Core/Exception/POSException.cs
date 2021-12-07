using System;

namespace POSSystems.Core.Exceptions
{
    public class POSException : Exception
    {
        public POSException(string usermessage) : base() => UserMessage = usermessage;

        public POSException(string usermessage, string logMessage) : base(logMessage) => UserMessage = usermessage;

        public string UserMessage { get; set; }
    }
}