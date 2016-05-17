using System;
using System.Text;
using System.Runtime.InteropServices;

namespace Tsunami.Core
{
    using ErrorCodeHandle = HandleRef;

    public class ErrorCode : IDisposable
    {
        #region PInvoke
        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr ErrorCode_Create();

        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr ErrorCode_Destroy(ErrorCodeHandle handle);

        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern void ErrorCode_Message_Get(ErrorCodeHandle handle, StringBuilder str, int size);

        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern int ErrorCode_Value_Get(ErrorCodeHandle handle);
        #endregion PInvoke

        ErrorCodeHandle handle;
        StringBuilder sb = new StringBuilder(256);

        public ErrorCode()
        {
            IntPtr h = ErrorCode_Create();
            handle = new HandleRef(this, h);
            ErrorCode_Message_Get(handle, sb, sb.Capacity);
            message = sb.ToString();
            Value = ErrorCode_Value_Get(handle);
        }

        public ErrorCode(IntPtr h)
        {
            handle = new HandleRef(this, h);
            ErrorCode_Message_Get(handle, sb, sb.Capacity);
            message = sb.ToString();
            Value = ErrorCode_Value_Get(handle);
        }

        private string message;
        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        public int Value
        {
            get; set;
        }

        public void Dispose()
        {
            CleanUp();
            GC.SuppressFinalize(this);
        }

        private void CleanUp()
        {
            ErrorCode_Destroy(handle);
            handle = new HandleRef(this, IntPtr.Zero);
        }

        ~ErrorCode()
        {
            CleanUp();
        }
    }
}
