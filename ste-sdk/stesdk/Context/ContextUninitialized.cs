using System;

namespace Improbable.Context
{
    public class ContextUninitialized: Exception
    {
        public ContextUninitialized(string message)
            : base(message)
        {
        }
    }   
}