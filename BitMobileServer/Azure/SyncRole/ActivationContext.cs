using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Runtime.InteropServices;
using System.IO;

namespace SyncRole
{
    public class ActivationContext
    {
        // Activation Context API Functions 

        [DllImport("Kernel32.dll", SetLastError = true)]
        private extern static IntPtr CreateActCtx(ref ACTCTX actctx);

        // Activation context structure 
        private struct ACTCTX
        {
            public int cbSize;
            public uint dwFlags;
            public string lpSource;
            public ushort wProcessorArchitecture;
            public ushort wLangId;
            public string lpAssemblyDirectory;
            public string lpResourceName;
            public string lpApplicationName;
        }

        private const int ACTCTX_FLAG_ASSEMBLY_DIRECTORY_VALID = 0x004;
        private const int ACTCTX_FLAG_SET_PROCESS_DEFAULT = 0x00000010;
        private IntPtr m_hActCtx = (IntPtr)0;
        public const UInt32 ERROR_SXS_PROCESS_DEFAULT_ALREADY_SET = 14011;

        /// <summary>
        /// Explicitly load a manifest and create the process-default activation 
        /// context. It takes effect immediately and stays there until the process exits. 
        /// </summary>
        static public void CreateActivationContext()
        {
            string rootFolder = AppDomain.CurrentDomain.BaseDirectory;
            string manifestPath = Path.Combine(rootFolder, "webapp.manifest");
            UInt32 dwError = 0;

            // Build the activation context information structure 
            ACTCTX info = new ACTCTX();
            info.cbSize = Marshal.SizeOf(typeof(ACTCTX));
            info.dwFlags = ACTCTX_FLAG_SET_PROCESS_DEFAULT;
            info.lpSource = manifestPath;
            if (null != rootFolder && "" != rootFolder)
            {
                info.lpAssemblyDirectory = rootFolder;
                info.dwFlags |= ACTCTX_FLAG_ASSEMBLY_DIRECTORY_VALID;
            }

            dwError = 0;

            // Create the activation context 
            IntPtr result = CreateActCtx(ref info);
            if (-1 == result.ToInt32())
            {
                dwError = (UInt32)Marshal.GetLastWin32Error();
            }

            if (-1 == result.ToInt32() && ActivationContext.ERROR_SXS_PROCESS_DEFAULT_ALREADY_SET != dwError)
            {
                string err = string.Format("Cannot create process-default win32 sxs context, error={0} manifest={1}", dwError, manifestPath);
                ApplicationException ex = new ApplicationException(err);
                throw ex;
            }
        }
    }
}

