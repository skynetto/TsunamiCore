using System;

using System.Runtime.InteropServices;

namespace Tsunami.Core
{
    using TorrentStatusHandle = HandleRef;

    public class TorrentStatus : IDisposable
    {
        #region PInvoke
        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TorrentStatus_Create();

        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern void TorrentStatus_Destroy(TorrentStatusHandle handle);

        #endregion PInvoke

        TorrentStatusHandle handle;


        public TorrentStatus()
        {
            IntPtr h = TorrentStatus_Create();
            handle = new HandleRef(this, h);
        }

        public TorrentStatus(IntPtr h)
        {
            handle = new HandleRef(this, h);
        }

        public void Dispose()
        {
            CleanUp();
            GC.SuppressFinalize(this);
        }

        private void CleanUp()
        {
            TorrentStatus_Destroy(handle);
            handle = new HandleRef(this, IntPtr.Zero);
        }

        ~TorrentStatus()
        {
            CleanUp();
        }
    }
}
