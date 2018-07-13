using System;

namespace Improbable.GeographicLib
{
	public class GeographicException : Exception
    {
		public GeographicException() { }      

        public GeographicException(string message)
			: base(message) { }

        public GeographicException(string message, Exception innerException)
			: base(message, innerException) { }
    }
}
