namespace OneCard
{
    using CacheeServer;
    using ClsSecurity;

   // using LibClsAttDevice;
    using LibDoorManage;
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Data.SqlClient;
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;
   // using hsopPlatform;
   // using HS.Platform;
    using System.Collections;
    using System.Diagnostics;
    public class Form1 : Form
    { 
        private Button button1;
        private Button button2;
        private int category__ = 0;
        private Thread CheckLineThread;
        private IContainer components = null;
        public DataTable DoorConnTable = new DataTable();
        public bool IsDownDoorLog = false;
        public bool ISLinkMoc = false;
        public bool ISStop = false;
        public bool IsUpInfo = false;
        private int keys = 0;
      //  private LibClsAttDevice libClsAtt;
        private LibDoorManage LibDM = new LibDoorManage();
        private string Message = "";
        private string message_ = "";
        private string resultFlag_ = "true";
        private string resultMessage_ = "";
        private Thread StartAutoTransDataThread;
       // public void autoDownloadRecordInTimelist()
        private Thread StartautoDownloadRecordInTimelist;
        private string subject_ = "";
       // private CmsPassport pst;
        public Form1()
        {
            this.InitializeComponent();
            this.button2.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.CheckLineThread = new Thread(new ThreadStart(this.CheckLines_));
            this.CheckLineThread.IsBackground = true;
            this.StartAutoTransDataThread = new Thread(new ThreadStart(this.StartAutoTransDatas));
            this.StartAutoTransDataThread.IsBackground = true;
            this.StartautoDownloadRecordInTimelist = new Thread(new ThreadStart(this.autoDownloadRecordInTimelist));
            this.StartautoDownloadRecordInTimelist.IsBackground = true;
            this.CheckLineThread.Start();
            this.StartAutoTransDataThread.Start();
            this.button1.Enabled = false;
            this.button2.Enabled = true;
        }


        private void button2_Click(object sender, EventArgs e)
        {
            foreach (DataRow row in this.DoorConnTable.Rows)
            {
                try
                {
                    IntPtr ptr = (IntPtr) Convert.ToInt32(row["ConnValues"].ToString());
                    this.LibDM.M_Disconnect(ptr);
                    clsDBManager.UpdateStatu(false, row["ID"].ToString());
                    this.StartAutoTransDataThread.Abort();
                    this.CheckLineThread.Abort();
                }
                catch (Exception)
                {
                }
            }
            this.ISStop = true;
            this.button1.Enabled = true;
            this.button2.Enabled = false;
        }

        public bool CheckLines(string DoorID)
        {
            clsDBManager.CreateTXT("开始更新编号为：" + DoorID + "的设备状态，时间" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            bool flag = false;
            try
            {
                foreach (DataRow row in this.DoorConnTable.Rows)
                {
                    if (row["ID"].ToString() == DoorID)
                    {
                        lock (this)
                        {
                            IntPtr ptr = (IntPtr) Convert.ToInt32(row["ConnValues"].ToString());
                            try
                            {
                                if (ptr != IntPtr.Zero)
                                {
                                    this.LibDM.M_Disconnect(ptr);
                                }
                                else
                                {
                                    clsDBManager.CreateTXT("句柄值丢失，设备号：" + DoorID + "，时间" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                    //Application.Exit();
                                }
                            }
                            catch (Exception)
                            {
                            }
                            DoorParmeter parmeter = new DoorParmeter();
                            parmeter.IP = row["IP"].ToString();
                            parmeter.MacID = row["ID"].ToString();
                            parmeter.MacName = row["MachineAlias"].ToString();
                            parmeter.PassWord = row["CommPassword"].ToString();
                            parmeter.Port = Convert.ToInt32(row["Port"].ToString());
                            clsDBManager.CreateTXT("开始连接编号为：" + DoorID + "的设备，时间" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            this.LibDM.h = IntPtr.Zero;
                            string strip = clsDBManager.MocConn(parmeter.IP, parmeter.Port, parmeter.PassWord);
                            this.keys = this.LibDM.M_Connect(clsDBManager.MocConn(parmeter.IP, parmeter.Port, parmeter.PassWord));
                            clsDBManager.CreateTXT("结束连接编号为：" + DoorID + "的设备，状态为：" + this.keys.ToString() + "strip:" + strip + "，时间" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            if (this.keys >= 0)
                            {
                                clsDBManager.UpdateStatu(true, parmeter.MacID);
                                clsDBManager.CreateTXT("更新编号为：" + DoorID + "的设备状态成功，时间" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                flag = true;
                                for (int i = 0; i < this.DoorConnTable.Rows.Count; i++)
                                {
                                    if (this.DoorConnTable.Rows[i]["ID"].ToString() == DoorID)
                                    {
                                        this.DoorConnTable.Rows[i]["ConnValues"] = this.LibDM.h.ToString();
                                        clsDBManager.CreateTXT("编号为：" + DoorID + "的设备连接值已匹配，时间" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                        goto Label_03F2;
                                    }
                                }
                            }
                            else
                            {
                                clsDBManager.UpdateStatu(false, parmeter.MacID);
                            }
                        }
                        goto Label_03F2;
                    }
                }
            }
            catch (Exception exception2)
            {
                clsDBManager.CreateTXT("更新编号为：" + DoorID + "的设备状态出错，错误信息：" + exception2.ToString() + "，时间" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                //Application.Exit();
            }
        Label_03F2:
            clsDBManager.CreateTXT("结束更新编号为：" + DoorID + "的设备状态，时间" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            return flag;
        }

        public void CheckLines_()
        {
            clsDBManager.CreateTXT("首次启动服务开始加载设备信息");
            this.LoadDoorConnection();
            clsDBManager.CreateTXT("加载设备信息成功，进入循环检查任务模块中");
            while (true)
            {
                Thread.Sleep(0x3e8);
                this.Upload();
                Thread.Sleep(0x7d0);
                Thread.Sleep(0x7d0);
                foreach (DataRow row in this.DoorConnTable.Rows)
                {
                    this.CheckLines(row["ID"].ToString());
                    Thread.Sleep(200);
                }
            }
        }
        public void logRealsun(DataTable dt, string source, string type, string mchid)
        {
            //Int64  resid = 0;
            //Hashtable hs = new Hashtable();
            //CmsTableParam cp = new CmsTableParam();
            // switch (source) 
            // {
            //     default:
            //         break;
            //     case "namelist":
            //         resid = 493817252015;
            //         for (int k = 0; k < dt.Rows.Count; k++)
            //         {
            //            hs.Clear ();
            //            hs.Add("CardNo", dt.Rows[k][0]);
            //            hs.Add("Pin", dt.Rows[k][1]);
            //            hs.Add("Groups", dt.Rows[k][3]);
            //            hs.Add("StartTime", dt.Rows[k][4]);
            //            hs.Add("EndTime", dt.Rows[k][5]);
            //            hs.Add("types", type);
            //            hs.Add("mchid", mchid);
            //            try
            //            {

            //                CmsTable.AddRecord(ref pst, resid, ref hs, ref cp);
            //            }
            //            catch (Exception ex)
            //            {
            //                SLog.Err("namelist Error", ref ex);
            //            }
            //         }
            //         break;
            //     case "authoritylist":
            //         resid=493817710407;
            //         for (int k = 0; k < dt.Rows.Count; k++)
            //         {
            //             hs.Clear();
                       
            //             hs.Add("Pin", dt.Rows[k][0]);
            //             hs.Add("AuthorizeTimezoneId", dt.Rows[k][1]);
            //             hs.Add("AuthorizeDoorId", dt.Rows[k][2]);
            //             hs.Add("types", type);
            //             hs.Add("mchid", mchid);
            //             try
            //             {

            //                 CmsTable.AddRecord(ref pst, resid, ref hs, ref cp);
            //             }
            //             catch (Exception ex)
            //             {
            //                 SLog.Err("authoritylist Error", ref ex);
            //             }
            //         }
            //         break;
            // }
        }
        public DataTable Diff(int type1, int type2, DataTable table1, DataTable table2)
        {
            DataTable table = table1.Clone();
            if (type1 == 1)
            {
                foreach (DataRow row in table2.Rows)
                {
                    if (type2 == 1)
                    {
                        if (table1.Select(string.Concat(new object[] { "[CardNo]='", row[0], "' and [Pin]='", row[1], "' and [Group]='", row[3], "' and [StartTime]='", row[4], "' and [EndTime]='", row[5], "'" })).Length == 0)
                        {
                            table.Rows.Add(new object[] { row[0].ToString(), row[1].ToString(), row[2].ToString(), row[3].ToString(), row[4].ToString(), row[5].ToString() });
                        }
                    }
                    else if ((type2 == 2) && (table1.Select(string.Concat(new object[] { "[Pin]='", row[0], "' and [AuthorizeTimezoneId]='", row[1], "' and [AuthorizeDoorId]='", row[2], "'" })).Length == 0))
                    {
                        table.Rows.Add(new object[] { row[0].ToString(), row[1].ToString(), row[2].ToString() });
                    }
                }
                return table;
            }
            if (type1 == 2)
            {
                foreach (DataRow row in table1.Rows)
                {
                    if (type2 == 1)
                    {
                        if (table2.Select(string.Concat(new object[] { "[CardNo]='", row[0], "' and [Pin]='", row[1], "' and [Group]='", row[3], "' and [StartTime]='", row[4], "' and [EndTime]='", row[5], "'" })).Length == 0)
                        {
                            table.Rows.Add(new object[] { row[0].ToString(), row[1].ToString(), row[2].ToString(), row[3].ToString(), row[4].ToString(), row[5].ToString() });
                        }
                    }
                    else if ((type2 == 2) && (table2.Select(string.Concat(new object[] { "[Pin]='", row[0], "' and [AuthorizeTimezoneId]='", row[1], "' and [AuthorizeDoorId]='", row[2], "'" })).Length == 0))
                    {
                        table.Rows.Add(new object[] { row[0].ToString(), row[1].ToString(), row[2].ToString() });
                    }
                }
            }
            return table;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        public string DownRecord(bool AllDown, IntPtr OneIntPtr_, int MachineID,bool del=true)

        {
            try
            {
                if (OneIntPtr_ != IntPtr.Zero)
                {
                    this.keys = 0;
                    int num = 0;
                    DataTable recordTable = new DataTable();
                    this.keys = this.LibDM.M_GetDeviceData(5, 0, "", AllDown, ref recordTable, OneIntPtr_);
                    // this.LibDM.M_DeleteDeviceData(
                    if (this.keys >= 0)
                    {
                        if (recordTable.Rows.Count > 0)
                        {
                            num = clsDBManager.MaxNum();
                            recordTable.Columns.Add("num", typeof(string));
                            recordTable.Columns.Add("SENSORID", typeof(string));
                            foreach (DataRow row in recordTable.Rows)
                            {
                                row["num"] = num.ToString();
                                row["SENSORID"] = MachineID.ToString();
                            }
                            SqlBulkCopy copy = new SqlBulkCopy(clsDBManager.GetSqlconn(), SqlBulkCopyOptions.UseInternalTransaction);
                            copy.DestinationTableName = "K_CheckInOut_Tem";
                            copy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Cardno", "Cardno"));
                            copy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Pin", "Pin"));
                            copy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Verified", "Verified"));
                            copy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("DoorID", "DoorID"));
                            copy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("EventType", "EventType"));
                            copy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("InOutState", "InOutState"));
                            copy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Time_second", "Time_second"));
                            copy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("num", "num"));
                            copy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("SENSORID", "SENSORID"));
                            copy.WriteToServer(recordTable);
                            clsDBManager.K_CheckInOutCopy(num, 2, ref this.keys, ref this.Message);
                            if (del)
                            { this.LibDM.M_DeleteDeviceData(5, 0, "", OneIntPtr_); }
                            // this.LibDM.M_DeleteDeviceData(5, 0, "", OneIntPtr_);

                            return string.Concat(new object[] { "共下载记录：", recordTable.Rows.Count, "条，最新记录：", this.keys.ToString(), "条" });
                        }
                        return "无记录";
                    }
                    return ("下载记录失败， Error：" + this.keys.ToString());
                }

            }
            catch (Exception ex)
            {

                return  "下载记录失败， Error："+ex.InnerException.Message+ex.Source;
            }
           
           // Application.Exit();
            return "下载记录失败， Error：连接句柄值为空";
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(72, 35);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "启动";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(199, 35);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "停止";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(363, 96);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "门禁自动服务程序20210321";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        public void LoadDoorConnection()
        {
            this.ISLinkMoc = true;
            this.DoorConnTable = new DataTable();
            this.DoorConnTable.Rows.Clear();
            this.DoorConnTable.Columns.Clear();
            this.DoorConnTable.Columns.Add(new DataColumn("ID", typeof(string)));
            this.DoorConnTable.Columns.Add(new DataColumn("IP", typeof(string)));
            this.DoorConnTable.Columns.Add(new DataColumn("MachineAlias", typeof(string)));
            this.DoorConnTable.Columns.Add(new DataColumn("Port", typeof(string)));
            this.DoorConnTable.Columns.Add(new DataColumn("CommPassword", typeof(string)));
            this.DoorConnTable.Columns.Add(new DataColumn("ConnValues", typeof(string)));
            DataTable table = clsDBManager.Select_HR_Machines_new("", "", "true", 2, "");
            clsDBManager.CreateTXT("Select_HR_MachinesNew：");
            if (table.Rows.Count > 0)
            {
                clsDBManager.CreateTXT("Select_HR_Machines："+ table.Rows.Count.ToString());
                foreach (DataRow row in table.Rows)
                {
                    DoorParmeter parmeter = new DoorParmeter();
                    parmeter.IP = row["IP"].ToString();
                    parmeter.MacID = row["ID"].ToString();
                    parmeter.MacName = row["MachineAlias"].ToString();
                    parmeter.PassWord = "";
                    if (!string.IsNullOrEmpty(row["CommPassword"].ToString()))
                    {
                        parmeter.PassWord = clsSecurity.TripDesDecrypt(row["CommPassword"].ToString());
                    }
                    parmeter.Port = Convert.ToInt32(row["Port"].ToString());
                    DataRow row2 = this.DoorConnTable.NewRow();
                    clsDBManager.CreateTXT("startconnect：" + parmeter.MacID.ToString());
                    row2["ID"] = parmeter.MacID.ToString();
                    row2["IP"] = parmeter.IP.ToString();
                    row2["MachineAlias"] = parmeter.MacName.ToString();
                    row2["Port"] = parmeter.Port.ToString();
                    row2["CommPassword"] = parmeter.PassWord.ToString();
                    this.LibDM.h = IntPtr.Zero;
                    string add = clsDBManager.MocConn(parmeter.IP, parmeter.Port, parmeter.PassWord);
                    clsDBManager.CreateTXT("connecting：" + add);
                    this.keys = this.LibDM.M_Connect(clsDBManager.MocConn(parmeter.IP, parmeter.Port, parmeter.PassWord));
                    if (this.keys >= 0)
                    {
                        clsDBManager.UpdateStatu(true, parmeter.MacID);
                        clsDBManager.CreateTXT("online：" + parmeter.MacID.ToString()+"h:"+this.LibDM.h.ToString() );
                        row2["ConnValues"] = this.LibDM.h.ToString();
                    }
                    else
                    {
                        clsDBManager.UpdateStatu(false, parmeter.MacID);
                        clsDBManager.CreateTXT("offline：" + parmeter.MacID.ToString());
                        clsDBManager.CreateTXT("offline h：" + this.LibDM.h.ToString());
                        row2["ConnValues"] = this.LibDM.h.ToString();
                    }
                 
                    this.DoorConnTable.Rows.Add(row2);
                }
            }
            // autoDownloadRecordInTimelist();
            clsDBManager.CreateTXT("StartautoDownloadRecordInTimelist start"  );
            this.StartautoDownloadRecordInTimelist.Start();
            this.ISLinkMoc = false;
        }
        public void autoDownloadRecordInTimelist()
        {
            clsDBManager.CreateTXT("StartautoDownloadRecordInTimelist start2");
            while (true)
            {
                clsDBManager.CreateTXT("StartautoDownloadRecordInTimelist start3");
                if ((DateTime.Now.Hour >= 6 && DateTime.Now.Hour < 16) || (DateTime.Now.Hour >= 16 && DateTime.Now.Hour <= 23))
                {
                    foreach (DataRow row in this.DoorConnTable.Rows)
                    {
                        clsDBManager.CreateTXT("StartautoDownloadRecordInTimelist start4");
                        if (Convert.ToInt32(row["ID"]) == 3766)
                        {
                            clsDBManager.CreateTXT("StartautoDownloadRecordInTimelist start5");
                            clsDBManager.CreateTXT("id:"+ row["ID"].ToString()+ "ConnValues:" + row["ConnValues"].ToString());
                            string str=this.DownRecord(true, (IntPtr)Convert.ToInt32(row["ConnValues"].ToString()), Convert.ToInt32(row["ID"].ToString()),false);
                           
                            clsDBManager.CreateTXT(str);
                            Thread.Sleep(60 * 1000);
                        }
                    }
                }
            }


        }
        public void LoadRearcd()
        {
            foreach (DataRow row in this.DoorConnTable.Rows)
            {
                if (this.IsDownDoorLog)
                {
                    try
                    {
                        this.DownRecord(true, (IntPtr) Convert.ToInt32(row["ConnValues"].ToString()), Convert.ToInt32(row["ID"].ToString()));
                        Thread.Sleep(200);
                    }
                    catch
                    {
                    }
                    finally
                    {
                        if (DBExecute.Query("SELECT [id] FROM [dbo].[SystemSet] where [id] = '7' and " + Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmm")) + "-[values] > 6", clsDBManager.GetSqlconn()).Rows.Count > 0)
                        {
                            this.IsDownDoorLog = false;
                        }
                        else
                        {
                            this.IsDownDoorLog = true;
                        }
                    }
                }
                else if (DBExecute.Query("SELECT [id] FROM [dbo].[SystemSet] where [id] = '7' and " + Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmm")) + "-[values] > 6", clsDBManager.GetSqlconn()).Rows.Count > 0)
                {
                    this.IsDownDoorLog = false;
                }
                else
                {
                    this.IsDownDoorLog = true;
                }
            }
        }

        public void Log(int category__, string subject_, string resultMessage_, string resultFlag_)
        {
            clsDBManager.InsertLogMachineUp(category__, subject_, resultMessage_, resultFlag_);
        }

        public void StartAutoTransData()
        {
            clsDBManager.SelectAutoSet();
            DateTime now = DateTime.Now;
            this.ISStop = false;
            while (!this.ISStop)
            {
                Thread.Sleep(0x7d0);
                try
                {
                    if (clsDBManager.SetUserInfo_.Uploadmode == 1)
                    {
                        try
                        {
                            string[] strArray = clsDBManager.SetUserInfo_.UploadTime.Split(new char[] { ';' });
                            foreach (string str in strArray)
                            {
                                DateTime time2 = Convert.ToDateTime(str);
                                if ((time2.Hour == DateTime.Now.Hour) && (time2.Minute == DateTime.Now.Minute))
                                {
                                    this.StartServer();
                                    Thread.Sleep(0xea60);
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                    else if (clsDBManager.SetUserInfo_.Uploadmode == 2)
                    {
                        TimeSpan span = (TimeSpan) (DateTime.Now - now);
                        if (span.TotalMinutes >= clsDBManager.SetUserInfo_.UploadTimeGap)
                        {
                            this.StartServer();
                            now = DateTime.Now;
                        }
                    }
                    clsDBManager.SelectAutoSet();
                }
                catch (Exception exception2)
                {
                    clsDBManager.Insert_LogMain("同步数据服务-1", exception2.Message, false, 2, ref this.Message);
                }
            }
        }

        public void StartServer()
        {
            clsDBManager.SelectAutoSet();
            string str = "";
            int num = 0;
            num = clsDBManager.Insert_LogMain("执行同步数据任务-4", "同步服务自动执行任务-同步设备数据", true, 1, ref str);
            if (num != 0)
            {
                if (clsDBManager.SetUserInfo_.IsDoorUpload)
                {
                    DBExecute.ExecuteSql("INSERT INTO [K_UpLoad] ([MacId],[LogId],[Stime],[Type]) SELECT [ID],'" + num.ToString() + "',getdate(),'2' FROM [dbo].[HR_Machines] where [Type]=2", clsDBManager.GetSqlconn());
                }
            }
            else
            {
                clsDBManager.Insert_LogMain("执行同步数据任务-5", str, false, 1, ref this.Message);
            }
        }
       
        public void Upload()
        {
            while (true)
            {
                DataTable table = clsDBManager.K_Select_UpLoad(2);
                if (table.Rows.Count <= 0)
                {
                    return;
                }
                try
                {
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        clsDBManager.CreateTXT("----START 此次同步开始 日志编码：" + table.Rows[i]["Id"].ToString() + "；发生时间：" + DateTime.Now.ToString() + " ----；");
                      
                       
                        for (int j = 0; j < this.DoorConnTable.Rows.Count; j++)
                        {
                            try
                            {
                                if (this.DoorConnTable.Rows[j]["ID"].ToString() == table.Rows[i]["MacId"].ToString())
                                {
                                    IntPtr ptr = (IntPtr) Convert.ToInt32(this.DoorConnTable.Rows[j]["ConnValues"].ToString());
                                    clsDBManager.CreateTXT("开始执行任务，设备ID：" + this.DoorConnTable.Rows[j]["ID"].ToString().ToString() + "；");
                                    this.category__ = Convert.ToInt32(table.Rows[i]["LogId"].ToString());
                                    clsDBManager.CreateTXT("取得主日志标识，ID：" + table.Rows[i]["LogId"].ToString() + "；");
                                    DataTable table2 = clsDBManager.Select_HR_Machines("", this.DoorConnTable.Rows[j]["ID"].ToString().ToString(), "true", 2, "");
                                    string str = table2.Rows[0]["MachineAlias"].ToString();
                                    clsDBManager.CreateTXT("查询设备名称，设备名称：" + table2.Rows[0]["MachineAlias"].ToString() + "；");
                                    this.keys = 0;
                                    DataTable table3 = new DataTable();
                                    DataTable table4 = new DataTable();
                                    DataTable recordTable = new DataTable();
                                    DataTable table6 = new DataTable();
                                    DataTable table7 = new DataTable();
                                    clsDBManager.CreateTXT("开始上传人员卡号；");
                                    int num3 = this.LibDM.M_GetDeviceData(1, 0, "", true, ref table7, ptr);
                                    if ((num3 == -2) || (table7.Columns.Count == 0))
                                    {

                                        this.Log(this.category__, "同步门禁设备信息", "连接设备失败，句柄值为空，请稍后再试，或者先同步其他门禁设备数据", "false");
                                       // Application.Exit();
                                        break;

                                    }
                                    clsDBManager.CreateTXT("结束查询设备中人员卡号，共：" + table7.Columns.Count.ToString() + "列；" + table7.Rows.Count.ToString() + "行；结果：" + num3.ToString());
                                    table3 = clsDBManager.K_SelectUp(1, Convert.ToInt32(this.DoorConnTable.Rows[j]["ID"].ToString()));
                                    clsDBManager.CreateTXT("结束查询系统中人员卡号，共：" + table3.Columns.Count.ToString() + "列；" + table3.Rows.Count.ToString() + "行；");
                                    table6 = this.Diff(2, 1, table7, table3);
                                    clsDBManager.CreateTXT("结束比对设备中多余的卡信息，共：" + table6.Rows.Count.ToString() + "条；");
                                    if (table6.Rows.Count >= 500)
                                    {
                                        this.keys = this.LibDM.M_DeleteDeviceData(1, 0, "", ptr);
                                        clsDBManager.CreateTXT("结束删除设备中卡信息，由于数量大于50条，批量清空了，共清空：" + table6.Rows.Count.ToString() + "条数据；");
                                        if (this.keys < 0)
                                        {
                                            
                                            return;
                                        }
                                        if (table3.Rows.Count > 0)
                                        {
                                            this.keys = this.LibDM.M_SetDeviceData(1, table3, ptr);
                                            clsDBManager.CreateTXT("结束上传人员信息到设备中，共：" + table3.Rows.Count.ToString() + "条；");
                                        }
                                        if (this.keys < 0)
                                        {
                                            this.resultFlag_ = "false";
                                            this.resultMessage_ = "设备名称为：" + str + " ； - 上传人员信息失败；Error" + this.keys.ToString();
                                        }
                                        else
                                        {
                                            this.resultFlag_ = "true";
                                            this.resultMessage_ = string.Concat(new object[] { "设备名称为：", str, " ； - 上传人员信息成功； - 用户总数：", table3.Rows.Count, "个，新增用户：", table3.Rows.Count, "个，删除用户：", table6.Rows.Count, "个" });

                                        }
                                        this.subject_ = "门禁 ---> 上传人员卡号";
                                        this.Log(this.category__, this.subject_, this.resultMessage_, this.resultFlag_);
                                    }
                                    else
                                    {
                                        foreach (DataRow row in table6.Rows)
                                        {
                                            this.keys = this.LibDM.M_DeleteDeviceData(1, 3, row[0].ToString(), ptr);
                                            if (this.keys < 0)
                                            {
                                                
                                                return;
                                            }
                                        }
                                        clsDBManager.CreateTXT("结束删除设备中多余的卡信息，共：" + table6.Rows.Count.ToString() + "条；");
                                        recordTable = this.Diff(1, 1, table7, table3);
                                        clsDBManager.CreateTXT("结束比对系统中新增的卡号信息，共：" + recordTable.Rows.Count.ToString() + "条；");
                                        if (recordTable.Rows.Count > 0)
                                        {
                                            this.keys = this.LibDM.M_SetDeviceData(1, recordTable, ptr);
                                            clsDBManager.CreateTXT("结束上传人员信息到设备中，共：" + recordTable.Rows.Count.ToString() + "条；");
                                        }
                                        if (this.keys < 0)
                                        {
                                            this.resultFlag_ = "false";
                                            this.resultMessage_ = "设备名称为：" + str + " ； - 上传人员信息失败；Error" + this.keys.ToString();
                                        }
                                        else
                                        {
                                            this.resultFlag_ = "true";
                                            this.resultMessage_ = string.Concat(new object[] { "设备名称为：", str, " ； - 上传人员信息成功； - 用户总数：", table3.Rows.Count, "个，新增用户：", recordTable.Rows.Count, "个，删除用户：", table6.Rows.Count, "个" });
                                            if (table.Rows[i]["Type"].ToString() == "3")
                                            {
                                                if (recordTable.Rows.Count > 0)
                                                {
                                                    logRealsun(recordTable, "namelist", "新增", table.Rows[i]["MacId"].ToString());
                                                }
                                                if (table6.Rows.Count > 0)
                                                {
                                                    logRealsun(table6, "namelist", "删除", table.Rows[i]["MacId"].ToString());
                                                }
                                            }
                                        }
                                        this.subject_ = "门禁 ---> 上传人员卡号";
                                        this.Log(this.category__, this.subject_, this.resultMessage_, this.resultFlag_);
                                    }
                                    clsDBManager.CreateTXT("开始查询设备中人员权限表；");
                                    num3 = this.LibDM.M_GetDeviceData(2, 0, "", true, ref table7, ptr);
                                    if ((num3 == -2) || (table7.Columns.Count == 0))
                                    {
                                       
                                        this.Log(this.category__, "同步门禁设备信息2", "连接设备失败，句柄值为空，请稍后再试，或者先同步其他门禁设备数据", "false");
                                        //Application.Exit();
                                     
                                        break;
                                    }
                                    clsDBManager.CreateTXT("结束查询设备中人员权限信息，共：" + table7.Columns.Count.ToString() + "列；" + table7.Rows.Count.ToString() + "行；结果：" + num3.ToString());
                                    table3 = clsDBManager.K_SelectUp(2, Convert.ToInt32(this.DoorConnTable.Rows[j]["ID"].ToString()));
                                    clsDBManager.CreateTXT("结束查询系统中人员权限信息，共：" + table3.Rows.Count.ToString() + "条；");
                                    table6 = this.Diff(2, 2, table7, table3);
                                    clsDBManager.CreateTXT("结束比对设备中需要删除的人员权限信息，共：" + table6.Rows.Count.ToString() + "条；");
                                    if (table6.Rows.Count >= 1000)
                                    {
                                        this.keys = this.LibDM.M_DeleteDeviceData(2, 0, "", ptr);
                                        clsDBManager.CreateTXT("结束删除设备中权限信息，由于数量大于500条，批量清空了，共清空：" + table6.Rows.Count.ToString() + "条数据；");
                                        if (this.keys < 0)
                                        {
                                           
                                            return;
                                        }
                                        if (table3.Rows.Count > 0)
                                        {
                                            this.keys = this.LibDM.M_SetDeviceData(2, table3, ptr);
                                            clsDBManager.CreateTXT("结束上传人员权限信息到设备中，共：" + table3.Rows.Count.ToString() + "条；");
                                        }
                                        if (this.keys < 0)
                                        {
                                            this.resultFlag_ = "false";
                                            this.resultMessage_ = "设备名称为：" + str + " ； - 上传人员权限失败；Error" + this.keys.ToString();
                                        }
                                        else
                                        {
                                            this.resultFlag_ = "true";
                                            this.resultMessage_ = string.Concat(new object[] { "设备名称为：", str, " ； - 上传人员权限成功； - 用户授权总数：", table3.Rows.Count, "个，新增用户授权：", table3.Rows.Count, "个，删除用户授权：", table6.Rows.Count, "个" });
                                        }
                                        this.subject_ = "门禁 ---> 上传人员权限";
                                        this.Log(this.category__, this.subject_, this.resultMessage_, this.resultFlag_);
                                    }
                                    else
                                    {
                                        foreach (DataRow row in table6.Rows)
                                        {
                                            this.keys = this.LibDM.M_DeleteDeviceData(2, 1, row[0].ToString(), ptr);
                                            if (this.keys < 0)
                                            {
                                               
                                                return;
                                            }
                                        }
                                        clsDBManager.CreateTXT("结束删除设备中多余的人员权限信息，共：" + table6.Rows.Count.ToString() + "条；");
                                        recordTable = this.Diff(1, 2, table7, table3);
                                        clsDBManager.CreateTXT("结束比对系统中新增的人员权限信息，共：" + recordTable.Rows.Count.ToString() + "条；");
                                        if (recordTable.Rows.Count > 0)
                                        {
                                            this.keys = this.LibDM.M_SetDeviceData(2, recordTable, ptr);
                                            clsDBManager.CreateTXT("结束上传人员权限信息到设备中，共：" + recordTable.Rows.Count.ToString() + "条；");
                                        }
                                        if (this.keys < 0)
                                        {
                                            this.resultFlag_ = "false";
                                            this.resultMessage_ = "设备名称为：" + str + " ； - 上传人员权限失败；Error" + this.keys.ToString();
                                        }
                                        else
                                        {
                                            this.resultFlag_ = "true";
                                            this.resultMessage_ = string.Concat(new object[] { "设备名称为：", str, " ； - 上传人员权限成功； - 用户授权总数：", table3.Rows.Count, "个，新增用户授权：", recordTable.Rows.Count, "个，删除用户授权：", table6.Rows.Count, "个" });
                                            if (table.Rows[i]["Type"].ToString() == "3")
                                            {
                                           
                                                 if (recordTable.Rows.Count > 0)
                                                    {
                                                        logRealsun(recordTable, "authoritylist", "新增", table.Rows[i]["MacId"].ToString());
                                                    }
                                                 if (table6.Rows.Count > 0)
                                                    {
                                                        logRealsun(table6, "authoritylist", "删除", table.Rows[i]["MacId"].ToString());
                                                    }
                                            }
                                        }
                                        this.subject_ = "门禁 ---> 上传人员权限";
                                        this.Log(this.category__, this.subject_, this.resultMessage_, this.resultFlag_);
                                    }
                                    clsDBManager.CreateTXT("开始查询门禁参数信息；");
                                    table4 = clsDBManager.K_Select_Door("", table2.Rows[0]["ID"].ToString(), "", "");
                                    clsDBManager.CreateTXT("结束查询门禁参数信息，共：" + table4.Rows.Count.ToString() + "条；");
                                    for (int k = 0; k < table4.Rows.Count; k++)
                                    {
                                        table3 = new DataTable();
                                        table3.Columns.Add(new DataColumn("id", typeof(string)));
                                        table3.Columns.Add(new DataColumn("dates", typeof(string)));
                                        DataRow row2 = table3.NewRow();
                                        row2[0] = "Door" + table4.Rows[k]["门编号"].ToString() + "ForcePassWord";
                                        row2[1] = table4.Rows[k]["胁迫密码"].ToString();
                                        table3.Rows.Add(row2);
                                        ////
                                        row2 = table3.NewRow();
                                        row2[0] = "Door" + table4.Rows[k]["门编号"].ToString() + "VerifyType";
                                        if (table4.Rows[k]["验证方式"].ToString() == "1")
                                        { row2[1] = "0"; }
                                        else
                                        { row2[1] = "11";
                                          clsDBManager.CreateTXT("Door" + table4.Rows[k]["门编号"].ToString() + "VerifyType=11");
                                        }
                                       
                                        
                                        table3.Rows.Add(row2);
                                        ////
                                        row2 = table3.NewRow();
                                        row2[0] = "Door" + table4.Rows[k]["门编号"].ToString() + "SupperPassWord";
                                        row2[1] = table4.Rows[k]["紧急状态密码"].ToString();
                                        table3.Rows.Add(row2);
                                        row2 = table3.NewRow();
                                        row2[0] = "Door" + table4.Rows[k]["门编号"].ToString() + "Drivertime";
                                        row2[1] = table4.Rows[k]["锁驱动时长"].ToString();
                                        table3.Rows.Add(row2);
                                        row2 = table3.NewRow();
                                        row2[0] = "Door" + table4.Rows[k]["门编号"].ToString() + "Intertime";
                                        row2[1] = table4.Rows[k]["刷卡间隔"].ToString();
                                        table3.Rows.Add(row2);
                                        row2 = table3.NewRow();
                                        row2[0] = "Door" + table4.Rows[k]["门编号"].ToString() + "SensorType";
                                        row2[1] = table4.Rows[k]["门磁类型"].ToString();
                                        table3.Rows.Add(row2);
                                        row2 = table3.NewRow();
                                        row2[0] = "Door" + table4.Rows[k]["门编号"].ToString() + "ValidTZ";
                                        row2[1] = table4.Rows[k]["T_ID"].ToString();
                                        table3.Rows.Add(row2);
                                        row2 = table3.NewRow();
                                        row2[0] = "Door" + table4.Rows[k]["门编号"].ToString() + "KeepOpenTimeZone";
                                        row2[1] = table4.Rows[k]["K_ID"].ToString();
                                        table3.Rows.Add(row2);
                                        row2 = table3.NewRow();
                                        row2[0] = "DateTime";
                                        row2[1] = DateTime.Now.ToString();
                                        table3.Rows.Add(row2);
                                        this.keys = this.LibDM.M_SetDeviceParam(ref table3, ptr);
                                        if (this.keys < 0)
                                        {
                                            this.resultFlag_ = "false";
                                            this.resultMessage_ = "设备名称为：" + str + " ；门名称为：" + table4.Rows[k]["门名称"].ToString() + "； - 上传门参数信息失败；Error" + this.keys.ToString();
                                        }
                                        else
                                        {
                                            this.resultFlag_ = "true";
                                            this.resultMessage_ = "设备名称为：" + str + "；门名称为：" + table4.Rows[k]["门名称"].ToString() + " ； - 上传门参数信息成功";
                                        }
                                        this.subject_ = "门禁 ---> 上传门参数信息";
                                        this.Log(this.category__, this.subject_, this.resultMessage_, this.resultFlag_);
                                    }
                                    clsDBManager.CreateTXT("结束上传门禁参数信息，共：" + table4.Rows.Count.ToString() + "条；");
                                    clsDBManager.CreateTXT("开始删除门禁时间段信息；");
                                    this.LibDM.M_DeleteDeviceData(4, 0, "", ptr);
                                    clsDBManager.CreateTXT("结束删除门禁时间段信息；");
                                    table3 = clsDBManager.K_SelectUp(3, Convert.ToInt32(this.DoorConnTable.Rows[j]["ID"].ToString()));
                                    clsDBManager.CreateTXT("结束查询门禁时间段信息，共：" + table3.Rows.Count.ToString() + " 条；");
                                    clsDBManager.CreateTXT("开始上传门禁时间段信息；");
                                    this.keys = this.LibDM.M_SetDeviceData(4, table3, ptr);
                                    if (this.keys < 0)
                                    {
                                        this.resultFlag_ = "false";
                                        this.resultMessage_ = "设备名称为：" + str + " ； - 上传时间段信息失败；Error" + this.keys.ToString();
                                    }
                                    else
                                    {
                                        this.resultFlag_ = "true";
                                        this.resultMessage_ = string.Concat(new object[] { "设备名称为：", str, " ； - 上传数据成功； - 时间段总数：", table3.Rows.Count, "个" });
                                    }
                                    clsDBManager.CreateTXT(this.resultMessage_);
                                    this.subject_ = "门禁 ---> 上传时间段信息";
                                    this.Log(this.category__, this.subject_, this.resultMessage_, this.resultFlag_);
                                    if (table.Rows[i]["Type"].ToString() == "2")
                                    {
                                        clsDBManager.CreateTXT("开始下载门禁记录；");
                                        string str2 = this.DownRecord(false, ptr, Convert.ToInt32(this.DoorConnTable.Rows[j]["ID"].ToString()));
                                        clsDBManager.CreateTXT("完成下载记录，执行结果：" + str2 + " ；");
                                        this.Log(this.category__, "门禁 ---> 下载门禁记录", "设备名称为：" + str + " ；" + str2, this.resultFlag_);
                                    }
                                    clsDBManager.CreateTXT("----END 此次同步结束 日志编码：" + table.Rows[i]["Id"].ToString() + "；发生时间：" + DateTime.Now.ToString() + " ----；");
                                 
                                    Thread.Sleep(200);
                                }
                            }
                            catch (Exception exception)
                            {
                                this.DoorConnTable.Rows[j]["ConnValues"] = 0;
                                this.category__ = clsDBManager.Insert_LogMain("执行门禁同步数据任务2", "同步设备信息发生错误，错误消息：" + exception.Message, false, 2, ref this.message_);
                               // Application.Exit();
                            }
                        }
                        clsDBManager.K_Update_UpLoad(table.Rows[i]["Id"].ToString(), "", ref this.keys, ref this.Message);
                        clsDBManager.CreateTXT("结束更新任务表状态，ID：" + table.Rows[i]["Id"].ToString() + "；");
                    }
                }
                catch (Exception exception2)
                {
                    this.category__ = clsDBManager.Insert_LogMain("执行门禁同步数据任务1", "同步设备信息发生错误，错误消息：" + exception2.Message, false, 2, ref this.message_);
                   // Application.Exit();
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button1.PerformClick();
          //  HsopCmsEnvironment.InitForClientApplication(Application.StartupPath, "RealsunFormj.log", true );
          //  pst = CmsPassport.GenerateCmsPassportBySysuser();

            //if (System.Diagnostics.Process.GetProcessesByName("OneCardAutoGuard").Length <= 0)
            //{
            //    System.Diagnostics.Process.Start("OneCardAutoGuard.exe");
            //}
     


        }
        public void StartAutoTransDatas()
        {
            clsDBManager.SelectAutoSet();
            DateTime now = DateTime.Now;
           
            while (true)
            {
                Thread.Sleep(0x7d0);
                try
                {
                    DataTable table = new DataTable();
                    string inputValue = "";
                    inputValue = "1";
                    if (inputValue == "1")
                    {
                        if (clsDBManager.SetUserInfo_.Uploadmode == 1)
                        {
                            try
                            {
                                string[] strArray = clsDBManager.SetUserInfo_.UploadTime.Split(new char[] { ';' });
                                foreach (string str2 in strArray)
                                {
                                    DateTime time2 = Convert.ToDateTime(str2);
                                    if ((time2.Hour == DateTime.Now.Hour) && (time2.Minute == DateTime.Now.Minute))
                                    {
                                        StartServerS();
                                        Thread.Sleep(0xea60);
                                    }
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                        else if (clsDBManager.SetUserInfo_.Uploadmode == 2)
                        {
                            TimeSpan span = (TimeSpan)(DateTime.Now - now);
                            if (span.TotalMinutes >= clsDBManager.SetUserInfo_.UploadTimeGap)
                            {
                                StartServerS();
                                now = DateTime.Now;
                            }
                        }
                        clsDBManager.SelectAutoSet();
                    }
       
                }
                catch (Exception exception2)
                {
                    clsDBManager.Insert_LogMain("同步数据服务-1", exception2.Message, false, 2, ref this.Message);
                }
            }
        }

        public void StartServerS()
        {
            clsDBManager.SelectAutoSet();
            string str = "";
            int num = 0;
            num = clsDBManager.Insert_LogMain("执行同步数据任务-4", "同步服务自动执行任务-同步设备数据", true, 1, ref str);
            if (num != 0)
            {
                if (clsDBManager.SetUserInfo_.DownAtt)
                {
                    clsDBManager.insertJob(3, num);
                }
                if (clsDBManager.SetUserInfo_.IsAutoUpload)
                {
                    clsDBManager.Insert_Job("-1", "-1", -1, 5, false, false, "", num);
                }
                if (clsDBManager.SetUserInfo_.DownAtt)
                {
                    clsDBManager.insertJob(1, num);
                }
                if (clsDBManager.SetUserInfo_.DownAtt)
                {
                    clsDBManager.insertJob(4, num);
                }
                if (clsDBManager.SetUserInfo_.IsConsumeUpload)
                {
                    DBExecute.ExecuteSql("INSERT INTO [IF_Parameter]([MOC_No],[LogId],[P_Type],[P_Synchronous],[P_Date]) select [MOC_No],'" + num.ToString() + "','3','True',getdate() from [X_MOC]", clsDBManager.GetSqlconn());
                    DBExecute.ExecuteSql("INSERT INTO [IF_Parameter]([MOC_No],[LogId],[P_Type],[P_Synchronous],[P_Date]) select [MOC_No],'" + num.ToString() + "','2','True',getdate() from [X_MOC]", clsDBManager.GetSqlconn());
                    DBExecute.ExecuteSql("INSERT INTO [IF_Parameter]([MOC_No],[LogId],[P_Type],[P_Synchronous],[P_Date]) select [MOC_No],'" + num.ToString() + "','1','True',getdate() from [X_MOC]", clsDBManager.GetSqlconn());
                }
                if (clsDBManager.SetUserInfo_.IsDoorUpload)
                {
                    DBExecute.ExecuteSql("INSERT INTO [K_UpLoad] ([MacId],[LogId],[Stime],[Type]) SELECT [ID],'" + num.ToString() + "',getdate(),'2' FROM [dbo].[HR_Machines] where [Type]=2", clsDBManager.GetSqlconn());
                }
                
            }
            else
            {
                clsDBManager.Insert_LogMain("执行同步数据任务-5", str, false, 1, ref this.Message);
            }
        }


    }
}

