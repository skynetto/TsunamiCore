using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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
        public static extern IntPtr Session_AddTorrent(AddTorrentParamsHandle handle);

        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern void Session_AsyncAddTorrent(AddTorrentParamsHandle handle);

        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr Session_FindTorrent(Sha1HashHandle handle);

        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern void Session_SetCallback([MarshalAs(UnmanagedType.FunctionPtr)] HandleCallback callbackPointer);

        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern void Session_ClearCallback();

        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern void Alert_What_Get(IntPtr handle, StringBuilder str, int size);

        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern void Alert_Destroy(IntPtr handle);
        #endregion PInvoke

        private static Dictionary<Type, Action<Object>> Alert2Func = new Dictionary<Type, Action<Object>>();
        static StringBuilder alertNameFromHandle = new StringBuilder(20);
        private static HandleCallback handler = new HandleCallback(HandleAlertCallback);

        public Session()
        {
            Session_Create();
            Alert2Func[typeof(torrent_added_alert)] = a => OnTorrentAddAlert((torrent_added_alert)a);
            Alert2Func[typeof(read_piece_alert)] = a => OnReadPieceAlert((read_piece_alert)a);
            
            Session_SetCallback(handler);
        }

        public void LoadTorrent(string filename)
        {
            using (AddTorrentParams atp = new AddTorrentParams())
            using (TorrentInfo ti = new TorrentInfo(filename))
            {
                atp.ti = ti;
                atp.SavePath = @"C:\Download";
                //atp.Flags &= ~Core.ATPFlags.flag_auto_managed; // remove auto managed flag
                //atp.Flags &= ~Core.ATPFlags.flag_paused; // remove pause on added torrent
                // atp.Flags &= ~Core.ATPFlags.flag_use_resume_save_path; // 
                Session_AsyncAddTorrent(atp.Handle);
            }
        }

        public TorrentHandle FindTorrent(Sha1Hash hash)
        {
            return new TorrentHandle(Session_FindTorrent(hash.GetHandle()));
        }

        private static void HandleAlertCallback(IntPtr alertHandle)
        {
            try
            {
                Alert_What_Get(alertHandle, alertNameFromHandle, alertNameFromHandle.Capacity);
                Type type = Type.GetType("Tsunami.Core." + alertNameFromHandle.ToString(), true);
                using (Alert alertTypeClass = (Alert)Activator.CreateInstance(type, alertHandle))
                {
                    Action<Object> run;
                    if (Alert2Func.TryGetValue(alertTypeClass.GetType(), out run))
                    {
                        run(alertTypeClass);
                    }
                }
            }
            catch (TypeLoadException)
            {
                Alert_Destroy(alertHandle);
            }
        }

        private static void OnTorrentAddAlert(torrent_added_alert a)
        {
            Console.WriteLine("Progress = {0}", a.What);
        }

        private static void OnReadPieceAlert(read_piece_alert a)
        {
            Console.WriteLine("Progress = {0}", a.buffer);
        }

        public void Dispose()
        {
            CleanUp();
            GC.SuppressFinalize(this);
        }

        private void CleanUp()
        {
            Session_ClearCallback();
            Session_Destroy();
        }

        ~Session()
        {
            CleanUp();
        }
    }
}
