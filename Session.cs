using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Tsunami.Core
{
    using AddTorrentParamsHandle = HandleRef;
    using Sha1HashHandle = HandleRef;
    using AlertHandle = HandleRef;
    public class Session : IDisposable
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void HandleCallback(IntPtr alertHandle);

        #region PInvoke
        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern void Session_Create();

        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern void Session_Destroy();

        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern void Session_AsyncAddTorrent(AddTorrentParamsHandle handle);

        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr Session_FindTorrent(Sha1HashHandle handle);

        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern void Session_SetCallback([MarshalAs(UnmanagedType.FunctionPtr)] HandleCallback callbackPointer);

        #endregion PInvoke

        public Session()
        {
            Session_Create();
            Session_SetCallback(callback);
        }

        public void LoadTorrent(string filename)
        {
            using (AddTorrentParams atp = new AddTorrentParams())
            using (TorrentInfo ti = new TorrentInfo(filename))
            {
                atp.ti = ti;
                atp.SavePath = @"C:\Download";
                atp.Flags &= ~ATPFlags.flag_paused; // remove pause on added torrent
                atp.Flags &= ~ATPFlags.flag_use_resume_save_path; // 
                //Debug.WriteLine(atp.Trackers);
                Session_AsyncAddTorrent(atp.GetHandle());
                
            }
        }
        
        TorrentHandle FindTorrent(Sha1Hash hash)
        {
            return new TorrentHandle(Session_FindTorrent(hash.GetHandle()));
        }

        HandleCallback callback = (alertHandle) =>
        {
            using (Alert a = new Alert(alertHandle))
            {
                Console.WriteLine("Progress = {0}", a.Category.ToString());
            }
        };

        public void Dispose()
        {
            CleanUp();
            GC.SuppressFinalize(this);
        }

        private void CleanUp()
        {
           Session_Destroy();
        }

        ~Session()
        {
            CleanUp();
        }
    }
}
