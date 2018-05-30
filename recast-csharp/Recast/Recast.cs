namespace Recast
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct RcContext
    {
    }

    public static class RecastLibrary
    {
        const string Library = "librecastwrapper";

        [DllImport(Library)]
        [return: MarshalAs(UnmanagedType.LPStruct)]
        public static extern RcContext rcContext_create();
    }
}