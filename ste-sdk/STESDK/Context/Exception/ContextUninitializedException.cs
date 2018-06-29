namespace Improbable.Context.Exception
{
    public class ContextUninitializedException : System.Exception
    {
        public ContextUninitializedException(string message)
            : base(message)
        {
        }
    }   
}