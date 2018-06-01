using System.Runtime.InteropServices;

namespace Recast
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FindPathResult
    {
        public uint status;
        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.MaxPathLength)]
        public uint[] path;
        
        public int pathCount;
    }
}