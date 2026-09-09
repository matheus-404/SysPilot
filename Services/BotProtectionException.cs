using System;

namespace SysPilot.Services
{

    public sealed class BotProtectionException : Exception
    {
        public BotProtectionException(string message) : base(message) { }
    }
}
