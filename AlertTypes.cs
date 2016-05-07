using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Tsunami.Core
{

    using AlertHandle = HandleRef;

    /// <summary>
    /// This is a base class for alerts that are associated with a specific
    /// torrent. It contains a handle to the torrent.
    /// </summary>
    public class torrent_alert : Alert, IDisposable
    {

        #region PInvoke
        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr Alert_TorrentAlert_TorrentHandle_Get(AlertHandle handle);
        #endregion PInvoke

        public torrent_alert(IntPtr alert)
            : base(alert)
        { }

        public TorrentHandle Handle
        {
            get
            {
                return Handle;
            }
            private set
            {
                Handle = new TorrentHandle(Alert_TorrentAlert_TorrentHandle_Get(handle));
            }
        }
    }

    /// <summary>
    /// The peer alert is a base class for alerts that refer to a specific
    /// peer. It includes all the information to identify the peer. i.e. ip and
    /// peer-id.
    /// </summary>
    public class peer_alert : torrent_alert , IDisposable
    {
        #region PInvoke
        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr Alert_PeerAlert_Pid_Get(AlertHandle handle);
        #endregion PInvoke

        public peer_alert(IntPtr alert)
            : base(alert)
        { }

        // TODO ip
        public Sha1Hash Pid
        {
            get
            {
                return Pid;
            }
            private set
            {
                Pid = new Sha1Hash(Alert_PeerAlert_Pid_Get(handle));
            }
        }
    }

    /// <summary>
    /// This is a base class used for alerts that are associated with a
    /// specific tracker. It derives from torrent_alert since a tracker is also
    /// associated with a specific torrent.
    /// </summary>
    public class tracker_alert : torrent_alert , IDisposable
    {

        #region PInvoke
        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern void Alert_TrackerAlert_Url_Get(AlertHandle handle, StringBuilder str, int size);
        #endregion PInvoke

        public tracker_alert(IntPtr alert)
            : base(alert)
        { }

        private StringBuilder sb = new StringBuilder(256);
        public string Url
        {
            get
            {
                return Url;
            }
            private set
            {
                Alert_TrackerAlert_Url_Get(handle, sb, sb.Capacity);
                Url = sb.ToString();
            }
        }
    }

    ///<summary>
    ///The torrent_added_alert is posted once every time a torrent is
    ///successfully added. It doesn't contain any members of its own, 
    ///ut inherits the torrent handle from its base class. It's posted
    ///when the status_notification bit is set in the alert_mask.
    ///</summary>
    public class torrent_added_alert : torrent_alert, IDisposable
    {
        public torrent_added_alert(IntPtr alert)
        : base(alert)
        { }
    }

    ///<summary>
    ///The torrent_removed_alert is posted whenever a torrent is removed.
    ///Since the torrent handle in its baseclass will always be invalid (since
    ///the torrent is already removed) it has the info hash as a member, to
    ///identify it. It's posted when the status_notification bit is set in the
    ///alert_mask.
    ///</summary>
    public class torrent_removed_alert : torrent_alert, IDisposable
    {
        public torrent_removed_alert(IntPtr alert)
        : base(alert)
        { }
    }
}
