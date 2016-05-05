using System;
using System.Runtime.InteropServices;

namespace Tsunami.Core
{
    using TorrentHandleHandle = HandleRef;
    public class TorrentHandle : IDisposable
    {
        #region PInvoke
        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TorrentHandle_Create();

        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern void TorrentHandle_Destroy(TorrentHandleHandle h);

        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern bool TorrentHandle_IsValid(TorrentHandleHandle h);

        #endregion PInvoke

        TorrentHandleHandle handle;

        public TorrentHandle()
        {
            IntPtr h = TorrentHandle_Create();
            handle = new HandleRef(this, h);
        }

        public TorrentHandle(IntPtr h)
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
            TorrentHandle_Destroy(handle);
            handle = new HandleRef(this, IntPtr.Zero);
        }

        public bool IsValid()
        {
            return TorrentHandle_IsValid(handle);
        }

        ~TorrentHandle()
        {
            CleanUp();
        }
    }
}
