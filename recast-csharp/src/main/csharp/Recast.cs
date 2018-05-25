namespace Recast
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public class RcContext
    {
    }

    public static class RecastLibrary
    {
        [DllImport("librecastwrapper")]
        [return: MarshalAs(UnmanagedType.LPStruct)]
        public static extern RcContext rcContext_create();
    }
}