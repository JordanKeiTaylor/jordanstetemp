using System;
using System.Runtime.InteropServices;

namespace Improbable.Recast.Types
{
    public struct NavMeshDataResult
    {
        public IntPtr data;
        public int size;

        public byte[] GetData()
        {
            byte[] bytes = new byte[size];
            Marshal.Copy(data, bytes, 0, size);

            return bytes;
        }
    }
}