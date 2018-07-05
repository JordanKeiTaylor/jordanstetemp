using System;

namespace Improbable.Navigation.Api
{
    public class NavigationException : Exception
    {
        public NavigationException(string message) : base(message)
        {
            
        }
    }
}