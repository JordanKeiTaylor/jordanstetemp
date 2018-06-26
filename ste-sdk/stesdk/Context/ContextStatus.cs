using Improbable.Collections;

namespace Improbable.Context
{
    public class ContextStatus
    {
        public static readonly ContextStatus ErrorExit = new ContextStatus("ExitError", 1);
        public static readonly ContextStatus DispatcherDisconnected = new ContextStatus("DispatcherDisconnected", 2);

        private static readonly Map<int, ContextStatus> CodeMap;

        static ContextStatus()
        {
            CodeMap = new Map<int, ContextStatus>
            {
                {1, ErrorExit}, 
                {2, DispatcherDisconnected}
            };
        }

        public static ContextStatus FromCode(int code)
        {
            return CodeMap[code];
        }
        
        private readonly string _name;
        private readonly int _code;

        private ContextStatus(string name, int code)
        {
            _name = name;
            _code = code;
        }

        public string GetName()
        {
            return _name;
        }

        public int GetCode()
        {
            return _code;
        }
    }
}