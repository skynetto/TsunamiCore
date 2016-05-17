using System;
using System.Runtime.InteropServices;

namespace Tsunami.Core
{
    using TorrentInfoHandle = HandleRef;

    public class TorrentInfo : IDisposable
    {
        #region PInvoke
        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TorrentInfo_Create(string filePath);

        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern void TorrentInfo_Destroy(TorrentInfoHandle h);
        
        #endregion PInvoke

        TorrentInfoHandle handle;

        public TorrentInfo(string filePath)
        {
            IntPtr h = TorrentInfo_Create(filePath);
            handle = new HandleRef(this, h);
        }

        public TorrentInfo(IntPtr h)
        {
            handle = new HandleRef(this, h);
        }

        public TorrentInfoHandle Handle
        {
            get
            {
                return handle;
            }
            set
            {
                handle = value;
            }
        }

        public void Dispose()
        {
            CleanUp();
            GC.SuppressFinalize(this);
        }

        private void CleanUp()
        {
            TorrentInfo_Destroy(handle);
            handle = new HandleRef(this, IntPtr.Zero);
        }

        ~TorrentInfo()
        {
            CleanUp();
        }
    }
}
