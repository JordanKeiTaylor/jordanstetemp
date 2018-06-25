using Improbable.Collections;

namespace Improbable
{
    public class ContextStatus
    {
        public static readonly ContextStatus ExitError = new ContextStatus("ExitError", 1);
        public static readonly ContextStatus DispatcherDisconnected = new ContextStatus("DispatcherDisconnected", 2);

        private static readonly Map<int, ContextStatus> _codeMap;

        static ContextStatus()
        {
            _codeMap = new Map<int, ContextStatus>
            {
                {1, ExitError}, 
                {2, DispatcherDisconnected}
            };
        }

        public static ContextStatus FromCode(int code)
        {
            return _codeMap[code];
        }
        
        private string _name;
        private int _code;

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