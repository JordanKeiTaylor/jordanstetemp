namespace Improbable.Context.Exception
{
    public class ContextInitializationFailedException : System.Exception
    {
        public ContextInitializationFailedException(string message)
            : base(message)
        {
        }
    }   
}