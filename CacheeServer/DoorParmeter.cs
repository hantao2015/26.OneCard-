namespace CacheeServer
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct DoorParmeter
    {
        public string MacID;
        public string MacName;
        public string IP;
        public int Port;
        public string PassWord;
        public int Status;
    }
}

