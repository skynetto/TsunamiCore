using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Tsunami.Core
{
    using AlertHandle = HandleRef;

    public class Alert : IDisposable
    {
        #region PInvoke
        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern void Alert_Destroy(AlertHandle handle);

        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern void Alert_What_Get(AlertHandle handle, StringBuilder str, int size);

        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern void Alert_Message_Get(AlertHandle handle, StringBuilder str, int size);

        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern int Alert_Type_Get(AlertHandle handle);

        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern int Alert_Category_Get(AlertHandle handle);

        #endregion PInvoke

        protected AlertHandle handle;
        private StringBuilder sb = new StringBuilder(256);

        public Alert(IntPtr h)
        {
            handle = new HandleRef(this, h);
        }

        private string what;
        public string What
        {
            get
            {
                if (String.IsNullOrEmpty(what))
                {
                    Alert_What_Get(handle, sb, sb.Capacity);
                    what = sb.ToString();
                }
                return what;
            }
        }

        private string message;
        public string Message
        {
            get
            {
                if (String.IsNullOrEmpty(message))
                {
                    Alert_Message_Get(handle, sb, sb.Capacity);
                    message = sb.ToString();
                }
                return message;
            }
        }

        private int type;
        public int Type
        {
            get
            {
                type = Alert_Type_Get(handle);
                return type;
            }
        }

        private AlertMask category;
        public AlertMask Category
        {
            get
            {
                category = (AlertMask)Alert_Category_Get(handle);
                return category;
            }
        }

        public void Dispose()
        {
            CleanUp();
            GC.SuppressFinalize(this);
        }

        private void CleanUp()
        {
            Alert_Destroy(handle);
            handle = new HandleRef(this, IntPtr.Zero);
        }

        ~Alert()
        {
            CleanUp();
        }
    }
}
