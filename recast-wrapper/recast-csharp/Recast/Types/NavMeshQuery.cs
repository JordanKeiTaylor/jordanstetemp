using System;
using Microsoft.Win32.SafeHandles;

namespace Recast
{
    public class NavMeshQuery : SafeHandleZeroOrMinusOneIsInvalid
    {
        public NavMeshQuery(IntPtr handle) : base(true)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            RecastLibrary.navmesh_query_delete(handle);
            return true;
        }
    }
}