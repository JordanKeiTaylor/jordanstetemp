using System;

namespace Improbable.sandbox.Navigation.Api
{
    public class NoPathFoundException : Exception
    {
        /// <summary>
        /// Exception used by the Navigation framework API to signal that a valid path is not
        /// possible.
        /// </summary>
        /// <param name="message">Message of the exception.</param>
        public NoPathFoundException(string message) : base(message) { }
    }
}