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
        {
            h = new TorrentHandle(Alert_TorrentAlert_TorrentHandle_Get(handle));
        }

        private TorrentHandle h;
        public TorrentHandle Handle
        {
            get { return h; }
            set { h = value; }
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
        {
            pid = new Sha1Hash(Alert_PeerAlert_Pid_Get(handle));
        }

        // TODO ip
        private Sha1Hash pid;
        public Sha1Hash Pid
        {
            get
            {
                return pid;
            }
            set
            {
                pid = value;
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
        {
            sb = new StringBuilder(256);
            Alert_TrackerAlert_Url_Get(handle, sb, sb.Capacity);
            url = sb.ToString();
        }

        static private StringBuilder sb;
        public string url { get; set; }
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

    ///<summary>
    ///This alert is posted when the asynchronous read operation initiated by a
    ///call to torrent_handle::read_piece() is completed. If the read failed,
    ///the torrent is paused and an error state is set and the buffer member of
    ///the alert is 0. If successful, buffer points to a buffer containing all
    ///the data of the piece. piece is the piece index that was read. size is
    ///the number of bytes that was read.
    ///</summary>
    public class read_piece_alert : torrent_alert, IDisposable
    {
        #region PInvoke
        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern uint Alert_ReadPieceAlert_Buffer_Size_Get(AlertHandle handle);

        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern void Alert_ReadPieceAlert_Buffer_Get(AlertHandle handle, [Out] byte[] buff, uint size);

        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern int Alert_ReadPieceAlert_Piece_Get(AlertHandle handle);
        #endregion PInvoke

        public read_piece_alert(IntPtr alert)
        : base(alert)
        {
            size = Alert_ReadPieceAlert_Buffer_Size_Get(handle);
            buffer = new byte[size];
            Alert_ReadPieceAlert_Buffer_Get(handle, buffer, size);
            piece = Alert_ReadPieceAlert_Piece_Get(handle);
        }

        public byte[] buffer { get; set; }
        public int piece { get; set; }
        public uint size { get; set; }
    }

    ///<summary>
    ///This is posted whenever an individual file completes its download. i.e.
    ///All pieces overlapping this file have passed their hash check.
    ///</summary>
    public class file_completed_alert : torrent_alert, IDisposable
    {
        #region PInvoke
        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern int Alert_FileCompletedAlert_Index_Get(AlertHandle handle);
        #endregion PInvoke

        public file_completed_alert(IntPtr alert)
            : base(alert)
        {
            index = Alert_FileCompletedAlert_Index_Get(handle);
        }

        ///<summary>The index of the file that completed.</summary>
        private int index;
        public int Index
        {
            get { return index; }
            set { index = value; }
        }
    }

    ///<summary>
    ///This is posted as a response to a torrent_handle::rename_file() call, 
    ///if the rename operation succeeds.
    ///</summary>
    public class file_renamed_alert : torrent_alert, IDisposable
    {
        #region PInvoke
        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern void Alert_FileRenamedAlert_Name_Get(AlertHandle handle,StringBuilder sb, int size);

        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern int Alert_FileRenamedAlert_Index_Get(AlertHandle handle);
        #endregion PInvoke

        public file_renamed_alert(IntPtr alert)
            :base(alert)
        {
            sb = new StringBuilder(256);
            Alert_FileRenamedAlert_Name_Get(handle, sb, sb.Capacity);
            name = sb.ToString();
            index = Alert_FileRenamedAlert_Index_Get(handle);
        }

        static private StringBuilder sb;

        ///<summary>The new name of the file.</summary>
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        
        ///<summary>The index of the file that was renamed.</summary>
        private int index;
        public int Index
        {
            get { return index; }
            set { index = value; }
        }
    }

    ///<summary>
    ///This is posted as a response to a torrent_handle::rename_file() call,
    ///if the rename operation failed.
    ///</summary>
    public class file_rename_failed_alert : torrent_alert, IDisposable
    {
        #region PInvoke
        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern int Alert_FileRenameFailedAlert_Index_Get(AlertHandle handle);

        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr Alert_FileRenameFailedAlert_Error_Get(AlertHandle handle);
        #endregion PInvoke


        public file_rename_failed_alert(IntPtr alert)
            :base(alert)
        {
            index = Alert_FileRenameFailedAlert_Index_Get(handle);
            error = new ErrorCode(Alert_FileRenameFailedAlert_Error_Get(handle));
        }

        ///<summary>The index of the file that was supposed to be renamed.</summary>
        private int index;
        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        private ErrorCode error;
        public ErrorCode Error
        {
            get { return error; }
            set { error = value; }
        }
    }


    /// <summary>
    /// This alert is generated when a limit is reached that might have a
    /// negative impact on upload or download rate performance.
    /// </summary>
    public class performance_alert : torrent_alert, IDisposable
    {
        #region PInvoke
        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern uint Alert_PerformanceAlert_WarningCode_Get(AlertHandle handle);
        #endregion PInvoke

        public performance_alert(IntPtr alert)
            :base(alert)
        {
            warning_code = (performance_warning_t)Alert_PerformanceAlert_WarningCode_Get(handle);
        }

        private performance_warning_t warning_code;
        public performance_warning_t WarningCode
        {
            get { return warning_code; }
            set { warning_code = value; }
        }
        
    }
}

