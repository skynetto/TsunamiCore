using System;
using System.Runtime.InteropServices;

namespace Tsunami.Core
{
    using Sha1HashHandle = HandleRef;

    public class Sha1Hash : IDisposable
    {
        #region PInvoke
        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr Sha1Hash_Create();

        #endregion PInvoke

        Sha1HashHandle handle;

        public Sha1Hash()
        {
            IntPtr h = Sha1Hash_Create(); 
            handle = new HandleRef(this, h);
        }

        public Sha1Hash(IntPtr h)
        {
            handle = new HandleRef(this, h);
        }

        public Sha1HashHandle GetHandle()
        {
            return handle;
        }

        public void Dispose()
        {
            CleanUp();
            GC.SuppressFinalize(this);
        }

        private void CleanUp()
        {
            
        }

        ~Sha1Hash()
        {
            CleanUp();
        }
    }
}
