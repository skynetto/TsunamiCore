using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Tsunami.Core
{
    using EntryHandle = HandleRef;

    public class Entry : IDisposable
    {
        #region PInvoke
        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr Entry_Create();

        [DllImport("TsunamiBridge", CallingConvention = CallingConvention.StdCall)]
        public static extern void Entry_Destroy(EntryHandle h);

        #endregion PInvoke

        EntryHandle handle;

        public Entry()
        {
            IntPtr h = Entry_Create();
            handle = new HandleRef(this, h);
        }

        public void Dispose()
        {
            CleanUp();
            GC.SuppressFinalize(this);
        }

        private void CleanUp()
        {
            Entry_Destroy(handle);
            handle = new HandleRef(this, IntPtr.Zero);
        }

        ~Entry()
        {
            CleanUp();
        }
    }
}
