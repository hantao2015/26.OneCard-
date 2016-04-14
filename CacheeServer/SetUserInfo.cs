namespace CacheeServer
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct SetUserInfo
    {
        public bool IsAutoUpload;
        public bool IsDoorUpload;
        public bool IsConsumeUpload;
        public int Uploadmode;
        public string UploadTime;
        public int UploadTimeGap;
        public bool DownAtt;
        public bool IsEmail;
        public string ToEmail;
        public string FromEmail;
        public string AddCDP;
        public string AddCachee;
        public string FromEmail_;
        public string FromUser;
        public string FromPsd;
        public string LoginCDP;
        public string LoginAdd;
        public string LoginUser;
        public string LoginPassd;
    }
}

