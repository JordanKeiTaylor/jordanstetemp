using System;
using Microsoft.Win32.SafeHandles;

namespace Recast
{
    public class PolyMesh : SafeHandleZeroOrMinusOneIsInvalid
    {
        public PolyMesh(IntPtr handle) : base(true)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            RecastLibrary.polymesh_delete(handle);
            return true;
        }
    }
}