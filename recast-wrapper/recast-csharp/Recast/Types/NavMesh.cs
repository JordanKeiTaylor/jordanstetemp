using System;
using Microsoft.Win32.SafeHandles;

namespace Recast
{
    public class NavMesh : SafeHandleZeroOrMinusOneIsInvalid
    {
        public NavMesh(IntPtr handle) : base(true)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            RecastLibrary.navmesh_delete(handle);
            return true;
        }
    }
}