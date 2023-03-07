namespace Catel.Fody
{
    using System;

    public class WeavingException : Exception
    {
        public WeavingException(string message)
            : base(message)
        {
        }
    }
}