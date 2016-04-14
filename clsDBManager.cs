
namespace CacheeServer
{


    using ClsSecurity;
    using System;
    using System.Data;
    using System.Data.Odbc;
    using System.Data.SqlClient;
    using System.IO;
    using System.Net;
    using System.Net.Mail;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;

    public class clsDBManager
    {
        public static SetUserInfo SetUserInfo_;

        public static bool Chage_CDPUpdateTime(int keys__, int UpdateTime__, ref string message__)
        {
            bool flag;
            SqlConnection connection = new SqlConnection(GetSqlconn());
            SqlCommand command = new SqlCommand("Chage_CDPUpdateTime", connection);
            command.CommandType = CommandType.StoredProcedure;
            SqlParameter parameter = command.Parameters.Add("@keys_", SqlDbType.BigInt);
            SqlParameter parameter2 = command.Parameters.Add("@UpdateTime_", SqlDbType.BigInt);
            SqlParameter parameter3 = command.Parameters.Add("@message_", SqlDbType.NVarChar, 50);
            parameter3.Direction = ParameterDirection.Output;
            parameter.Value = keys__;
            parameter2.Value = UpdateTime__;
            try
            {
                command.CommandTimeout = 0xe10;
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                command.ExecuteNonQuery();
                message__ = parameter3.Value.ToString();
                flag = true;
            }
            catch (Exception exception)
            {
                message__ = exception.Message;
                flag = false;
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
            return flag;
        }

        public static void CreateTXT(string str)
        {
            try
            {
                FileStream stream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + DateTime.Now.ToString("yyyyMMdd") + "Log.txt", FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(stream);
                writer.Flush();
                writer.BaseStream.Seek(0L, SeekOrigin.End);
                writer.Write(str + "\t");
                writer.Write("\r\n");
                writer.Flush();
                writer.Close();
                writer.Dispose();
            }
            catch (Exception)
            {
            }
        }

        public static bool DBAll(ref string serverName, ref string DBs, ref string userName, ref string Pwd, ref string yz)
        {
            bool flag = true;
            serverName = "";
            DBs = "";
            userName = "";
            Pwd = "";
            yz = "";
            Exists();
            XmlDocument document = new XmlDocument();
            document.Load(File_Name());
            XmlElement documentElement = document.DocumentElement;
            foreach (XmlNode node in documentElement.ChildNodes)
            {
                try
                {
                    string str = node.Attributes.GetNamedItem("id").Value;
                    if (str != null)
                    {
                        if (!(str == "serverName"))
                        {
                            if (str == "DBs")
                            {
                                goto Label_00EF;
                            }
                            if (str == "userName")
                            {
                                goto Label_010D;
                            }
                            if (str == "Pwd")
                            {
                                goto Label_012B;
                            }
                            if (str == "yz")
                            {
                                goto Label_0149;
                            }
                        }
                        else
                        {
                            serverName =    ClsSecurity.clsSecurity.TripDesDecrypt(node.Attributes.GetNamedItem("value").Value);
                           
                        }
                    }
                    goto Label_0172;
                Label_00EF:
                    DBs = clsSecurity.TripDesDecrypt(node.Attributes.GetNamedItem("value").Value);
                    goto Label_0172;
                Label_010D:
                    userName = clsSecurity.TripDesDecrypt(node.Attributes.GetNamedItem("value").Value);
                    goto Label_0172;
                Label_012B:
                    Pwd = clsSecurity.TripDesDecrypt(node.Attributes.GetNamedItem("value").Value);
                    goto Label_0172;
                Label_0149:
                    yz = clsSecurity.TripDesDecrypt(node.Attributes.GetNamedItem("value").Value);
                }
                catch
                {
                    flag = false;
                }
            Label_0172:;
            }
            return flag;
        }

        public static int ExecuteSql(SqlCommand sqlcom, SqlConnection sqlcon, ref string message, ref int keys)
        {
            int num = 0;
            using (SqlConnection connection = sqlcon)
            {
                using (SqlCommand command = sqlcom)
                {
                    try
                    {
                        SqlParameter parameter = command.Parameters.Add("@judgenum", SqlDbType.Int);
                        SqlParameter parameter2 = command.Parameters.Add("@msg", SqlDbType.NVarChar, 0xbb8);
                        sqlcom.CommandTimeout = 0xe10;
                        parameter.Direction = ParameterDirection.Output;
                        parameter2.Direction = ParameterDirection.Output;
                        if (connection.State == ConnectionState.Closed)
                        {
                            connection.Open();
                        }
                        num = command.ExecuteNonQuery();
                        keys = Convert.ToInt32(parameter.Value);
                        message = parameter2.Value.ToString();
                    }
                    catch (SqlException exception)
                    {
                        connection.Close();
                        connection.Dispose();
                        message = exception.Message;
                        keys = -1;
                        return num;
                    }
                    finally
                    {
                        connection.Close();
                        connection.Dispose();
                    }
                    return num;
                }
            }
        }

        public static void Exists()
        {
            if (!System.IO.File.Exists(File_Name()))
            {
                file_xml(File_Name());
            }
        }

        public static string File_Name()
        {
            return (AppDomain.CurrentDomain.BaseDirectory + "XMLFile1.xml");
        }

        public static void file_xml(string fileName)
        {
            if (!Directory.Exists(fileName))
            {
                XmlTextWriter writer = new XmlTextWriter(fileName, Encoding.UTF8);
                writer.WriteStartDocument();
                writer.WriteStartElement("root");
                writer.WriteStartElement("serverName");
                writer.WriteAttributeString("id", "serverName");
                writer.WriteAttributeString("value", "");
                writer.WriteEndElement();
                writer.WriteStartElement("DBs");
                writer.WriteAttributeString("id", "DBs");
                writer.WriteAttributeString("value", "");
                writer.WriteEndElement();
                writer.WriteStartElement("userName");
                writer.WriteAttributeString("id", "userName");
                writer.WriteAttributeString("value", "");
                writer.WriteEndElement();
                writer.WriteStartElement("Pwd");
                writer.WriteAttributeString("id", "Pwd");
                writer.WriteAttributeString("value", "");
                writer.WriteEndElement();
                writer.WriteStartElement("yz");
                writer.WriteAttributeString("id", "yz");
                writer.WriteAttributeString("value", "");
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Close();
            }
        }

        public static DataTable GetCSV_(string fileurl, string fileName, ref string message__)
        {
            string str = "Driver={Microsoft Text Driver (*.txt; *.csv)};Dbq=";
            OdbcConnection selectConnection = new OdbcConnection(str + fileurl + ";Extensions=asc,csv,tab,txt;");
            DataTable dataTable = new DataTable();
            try
            {
                new OdbcDataAdapter("select * from " + fileName, selectConnection).Fill(dataTable);
                message__ = "OK";
            }
            catch (Exception exception)
            {
                message__ = exception.Message;
            }
            return dataTable;
        }

        public static string Getdatarq(string NowDate)
        {
            string[] strArray = NowDate.Split(" ".ToCharArray());
            if (strArray.Length == 2)
            {
                return strArray[0].ToString();
            }
            return NowDate;
        }

        public static bool GetDBs(string yz, string serverName, string userName, string Pwd, string DBs, ref string errMsg)
        {
            bool flag = true;
            string connectionString = "";
            if (yz == "1")
            {
                connectionString = " Data Source=" + serverName + ";Initial Catalog=" + DBs + ";Integrated Security=True";
            }
            else if (yz == "2")
            {
                connectionString = "Data Source=" + serverName + ";Initial Catalog=" + DBs + ";Persist Security Info=True;User ID=" + userName + ";Password=" + Pwd;
            }
            SqlConnection connection = new SqlConnection(connectionString);
            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                errMsg = "数据库连接成功！";
                connection.Close();
                connection.Dispose();
            }
            catch (SqlException exception)
            {
                errMsg = exception.Message;
                flag = false;
                connection.Close();
                connection.Dispose();
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
            return flag;
        }

        public static string GetSqlconn()
        {
            string str = "";
            string serverName = "";
            string dBs = "";
            string userName = "";
            string pwd = "";
            string yz = "";
            if (DBAll(ref serverName, ref dBs, ref userName, ref pwd, ref yz))
            {
                if (yz == "1")
                {
                    return (" Data Source=" + serverName + ";Initial Catalog=" + dBs + ";Integrated Security=True");
                }
                if (yz == "2")
                {
                    str = "Data Source=" + serverName + ";Initial Catalog=" + dBs + ";Persist Security Info=True;User ID=" + userName + ";Password=" + pwd;
                }
            }
            return str;
        }

        public static int Input_int(string InputValue)
        {
            try
            {
                return Convert.ToInt32(InputValue);
            }
            catch
            {
                return 0;
            }
        }

        public static void Insert_DownUser(DataTable UserTable, string MachineNo_, int rows_, ref int rrows_, ref string msg_)
        {
            rrows_ = 1;
            SqlConnection connection = new SqlConnection(GetSqlconn());
            SqlCommand command = new SqlCommand("Insert_DownUser", connection);
            command.CommandType = CommandType.StoredProcedure;
            try
            {
                command.CommandTimeout = 0xe10;
                SqlParameter parameter = command.Parameters.Add("@MachineCode", SqlDbType.NVarChar);
                SqlParameter parameter2 = command.Parameters.Add("@RegNo", SqlDbType.NVarChar);
                SqlParameter parameter3 = command.Parameters.Add("@Name", SqlDbType.NVarChar);
                SqlParameter parameter4 = command.Parameters.Add("@CardNo", SqlDbType.NVarChar);
                SqlParameter parameter5 = command.Parameters.Add("@UserPwd", SqlDbType.NVarChar);
                SqlParameter parameter6 = command.Parameters.Add("@Privilege", SqlDbType.Int);
                SqlParameter parameter7 = command.Parameters.Add("@Enable", SqlDbType.Int);
                SqlParameter parameter8 = command.Parameters.Add("@rows", SqlDbType.Int);
                int num = 0;
                connection.Open();
                foreach (DataRow row in UserTable.Rows)
                {
                    try
                    {
                        parameter.Value = MachineNo_;
                        parameter2.Value = row["RegNo"].ToString();
                        parameter3.Value = row["Name"].ToString();
                        parameter4.Value = row["CardNo"].ToString();
                        parameter5.Value = row["UserPwd"].ToString();
                        parameter6.Value = DBExecute.Input_int(row["Privilege"].ToString());
                        if (Convert.ToBoolean(row["Enable"].ToString()))
                        {
                            parameter7.Value = 1;
                        }
                        else
                        {
                            parameter7.Value = -1;
                        }
                        parameter8.Value = rows_;
                        command.ExecuteNonQuery();
                        num++;
                    }
                    catch
                    {
                        continue;
                    }
                }
                msg_ = num.ToString();
            }
            catch (SqlException exception)
            {
                rrows_ = -1;
                msg_ = exception.Message.ToString();
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }

        public static string Insert_Job(string UserSS__, string deptcode_, int deptChild_, int Tyle_, bool fg_, bool exec_, string MachineNo_, int MainIndex_)
        {
            string message;
            SqlConnection connection = new SqlConnection(GetSqlconn());
            SqlCommand command = new SqlCommand("Insert_Job", connection);
            command.CommandType = CommandType.StoredProcedure;
            SqlParameter parameter = command.Parameters.Add("@UserSS_", SqlDbType.NVarChar);
            SqlParameter parameter2 = command.Parameters.Add("@deptcode", SqlDbType.NVarChar);
            SqlParameter parameter3 = command.Parameters.Add("@deptChild", SqlDbType.Int);
            SqlParameter parameter4 = command.Parameters.Add("@Tyle", SqlDbType.Int);
            SqlParameter parameter5 = command.Parameters.Add("@fg", SqlDbType.Bit);
            SqlParameter parameter6 = command.Parameters.Add("@exec", SqlDbType.Bit);
            SqlParameter parameter7 = command.Parameters.Add("@MachineNo", SqlDbType.NVarChar);
            SqlParameter parameter8 = command.Parameters.Add("@MainIndex", SqlDbType.Int);
            try
            {
                command.CommandTimeout = 0xe10;
                parameter.Value = UserSS__;
                parameter2.Value = deptcode_;
                parameter3.Value = deptChild_;
                parameter4.Value = Tyle_;
                parameter5.Value = fg_;
                parameter6.Value = exec_;
                parameter7.Value = MachineNo_;
                parameter8.Value = MainIndex_;
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                command.ExecuteNonQuery();
                message = "OK";
            }
            catch (Exception exception)
            {
                message = exception.Message;
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
            return message;
        }

        public static int Insert_LogMain(string subject_, string resultMessage_, bool resultFlag_, int type_, ref string mess_)
        {
            int num = 0;
            SqlConnection connection = new SqlConnection(GetSqlconn());
            SqlCommand command = new SqlCommand("InsertLogMain", connection);
            command.CommandType = CommandType.StoredProcedure;
            SqlParameter parameter = command.Parameters.Add("@subject", SqlDbType.NVarChar);
            SqlParameter parameter2 = command.Parameters.Add("@resultMessage", SqlDbType.NVarChar);
            SqlParameter parameter3 = command.Parameters.Add("@resultFlag", SqlDbType.NVarChar);
            SqlParameter parameter4 = command.Parameters.Add("@type", SqlDbType.Int);
            SqlParameter parameter5 = command.Parameters.Add("@msg", SqlDbType.NVarChar, 500);
            parameter5.Direction = ParameterDirection.Output;
            parameter.Value = subject_;
            parameter2.Value = resultMessage_.Replace("'", " ");
            parameter3.Value = resultFlag_.ToString();
            parameter4.Value = type_;
            try
            {
                command.CommandTimeout = 0xe10;
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                command.ExecuteNonQuery();
                num = Convert.ToInt32(parameter5.Value.ToString());
                mess_ = "OK";
            }
            catch (SqlException exception)
            {
                num = 0;
                mess_ = exception.Message;
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
            return num;
        }

        public static bool Insert_UpdateCDPUserInfo(DataTable CDPTable, ref string message_, int MainIndex_)
        {
            bool flag = true;
            message_ = "OK";
            SqlCommand command = new SqlCommand();
            command.CommandType = CommandType.Text;
            string str = "";
            command.CommandTimeout = 0xe10;
            if (CDPTable.Rows.Count > 0)
            {
                str = ((((str + "INSERT INTO [UpdateCDPUserInfo] " + " ([usercode],[cname],[ename],[status],[time_rz],[time_jrts],[ssjt],[ssfr] ") + " ,[costcenter],[dept1],[dept2],[dept3],[dept4],[gw],[head],[type_yg] " + " ,[gsz],[enable_gsz],[zl],[type_kq],[card],[gender],[time_cs],[age] ") + " ,[hf],[xl],[jg],[mz],[sfz],[email],[jtdz],[jtdh],[sj],[gzdd] " + "  ,[gl_lsgs],[gl_dqgs],[gl_lssh],[time_lz],[remark],[deptAll],MainIndex) VALUES ") + "(@usercode,@cname,@ename,@status,@time_rz,@time_jrts,@ssjt,@ssfr,@costcenter,@dept1" + ",@dept2,@dept3,@dept4,@gw,@head,@type_yg,@gsz,@enable_gsz,@zl,@type_kq,@card,@gender") + ",@time_cs,@age,@hf,@xl,@jg,@mz,@sfz,@email,@jtdz,@jtdh,@sj,@gzdd" + ",@gl_lsgs,@gl_dqgs,@gl_lssh,@time_lz,@remark,@deptAll,@MainIndex) ";
                command.CommandText = str;
                SqlConnection connection = new SqlConnection(GetSqlconn());
                command.Connection = connection;
                int num = 0;
                try
                {
                    try
                    {
                        SqlParameter parameter = command.Parameters.Add("@usercode", SqlDbType.NVarChar);
                        SqlParameter parameter2 = command.Parameters.Add("@cname", SqlDbType.NVarChar);
                        SqlParameter parameter3 = command.Parameters.Add("@ename", SqlDbType.NVarChar);
                        SqlParameter parameter4 = command.Parameters.Add("@status", SqlDbType.NVarChar);
                        SqlParameter parameter5 = command.Parameters.Add("@time_rz", SqlDbType.NVarChar);
                        SqlParameter parameter6 = command.Parameters.Add("@time_jrts", SqlDbType.NVarChar);
                        SqlParameter parameter7 = command.Parameters.Add("@deptAll", SqlDbType.NVarChar);
                        SqlParameter parameter8 = command.Parameters.Add("@ssjt", SqlDbType.NVarChar);
                        SqlParameter parameter9 = command.Parameters.Add("@ssfr", SqlDbType.NVarChar);
                        SqlParameter parameter10 = command.Parameters.Add("@costcenter", SqlDbType.NVarChar);
                        SqlParameter parameter11 = command.Parameters.Add("@dept1", SqlDbType.NVarChar);
                        SqlParameter parameter12 = command.Parameters.Add("@dept2", SqlDbType.NVarChar);
                        SqlParameter parameter13 = command.Parameters.Add("@dept3", SqlDbType.NVarChar);
                        SqlParameter parameter14 = command.Parameters.Add("@dept4", SqlDbType.NVarChar);
                        SqlParameter parameter15 = command.Parameters.Add("@gw", SqlDbType.NVarChar);
                        SqlParameter parameter16 = command.Parameters.Add("@head", SqlDbType.NVarChar);
                        SqlParameter parameter17 = command.Parameters.Add("@type_yg", SqlDbType.NVarChar);
                        SqlParameter parameter18 = command.Parameters.Add("@gsz", SqlDbType.NVarChar);
                        SqlParameter parameter19 = command.Parameters.Add("@enable_gsz", SqlDbType.NVarChar);
                        SqlParameter parameter20 = command.Parameters.Add("@zl", SqlDbType.NVarChar);
                        SqlParameter parameter21 = command.Parameters.Add("@type_kq", SqlDbType.NVarChar);
                        SqlParameter parameter22 = command.Parameters.Add("@card", SqlDbType.NVarChar);
                        SqlParameter parameter23 = command.Parameters.Add("@gender", SqlDbType.NVarChar);
                        SqlParameter parameter24 = command.Parameters.Add("@time_cs", SqlDbType.NVarChar);
                        SqlParameter parameter25 = command.Parameters.Add("@age", SqlDbType.NVarChar);
                        SqlParameter parameter26 = command.Parameters.Add("@hf", SqlDbType.NVarChar);
                        SqlParameter parameter27 = command.Parameters.Add("@xl", SqlDbType.NVarChar);
                        SqlParameter parameter28 = command.Parameters.Add("@jg", SqlDbType.NVarChar);
                        SqlParameter parameter29 = command.Parameters.Add("@mz", SqlDbType.NVarChar);
                        SqlParameter parameter30 = command.Parameters.Add("@sfz", SqlDbType.NVarChar);
                        SqlParameter parameter31 = command.Parameters.Add("@email", SqlDbType.NVarChar);
                        SqlParameter parameter32 = command.Parameters.Add("@jtdz", SqlDbType.NVarChar);
                        SqlParameter parameter33 = command.Parameters.Add("@jtdh", SqlDbType.NVarChar);
                        SqlParameter parameter34 = command.Parameters.Add("@sj", SqlDbType.NVarChar);
                        SqlParameter parameter35 = command.Parameters.Add("@gzdd", SqlDbType.NVarChar);
                        SqlParameter parameter36 = command.Parameters.Add("@gl_lsgs", SqlDbType.Float);
                        SqlParameter parameter37 = command.Parameters.Add("@gl_dqgs", SqlDbType.Float);
                        SqlParameter parameter38 = command.Parameters.Add("@gl_lssh", SqlDbType.Float);
                        SqlParameter parameter39 = command.Parameters.Add("@time_lz", SqlDbType.NVarChar);
                        SqlParameter parameter40 = command.Parameters.Add("@remark", SqlDbType.NVarChar);
                        SqlParameter parameter41 = command.Parameters.Add("@MainIndex", SqlDbType.NVarChar);
                        if (connection.State == ConnectionState.Closed)
                        {
                            connection.Open();
                        }
                        foreach (DataRow row in CDPTable.Rows)
                        {
                            try
                            {
                                parameter.Value = row["员工工号"].ToString();
                                parameter2.Value = row["中文姓名"].ToString();
                                parameter3.Value = row["英文姓名"].ToString();
                                parameter4.Value = row["状态"].ToString();
                                parameter5.Value = row["入职日期"].ToString();
                                parameter6.Value = row["加入泰森日期"].ToString();
                                parameter7.Value = (row["所属集团"].ToString() + "," + row["所属法人"].ToString() + "," + row["一级部门(中)"].ToString() + "," + row["二级部门(中)"].ToString() + "," + row["三级部门(中)"].ToString() + "," + row["四级部门(中)"].ToString()).Trim(new char[] { ',' });
                                parameter8.Value = row["所属集团"].ToString();
                                parameter9.Value = row["所属法人"].ToString();
                                parameter10.Value = row["成本中心代码"].ToString();
                                parameter11.Value = row["一级部门(中)"].ToString();
                                parameter12.Value = row["二级部门(中)"].ToString();
                                parameter13.Value = row["三级部门(中)"].ToString();
                                parameter14.Value = row["四级部门(中)"].ToString();
                                parameter15.Value = row["岗位(中)"].ToString();
                                parameter16.Value = row["直接主管姓名"].ToString();
                                parameter17.Value = row["用工类别"].ToString();
                                parameter18.Value = row["工时制"].ToString();
                                parameter19.Value = row["是否约定有效工时"].ToString();
                                parameter20.Value = row["职类"].ToString();
                                parameter21.Value = row["考勤类别"].ToString();
                                parameter22.Value = "";
                                parameter23.Value = row["性别"].ToString();
                                parameter24.Value = row["出生日期"].ToString();
                                parameter25.Value = row["年龄"].ToString();
                                parameter26.Value = row["婚否状态"].ToString();
                                parameter27.Value = row["学历"].ToString();
                                parameter28.Value = row["籍贯"].ToString();
                                parameter29.Value = row["民族"].ToString();
                                parameter30.Value = row["身份证号码"].ToString();
                                parameter31.Value = row["电子邮件"].ToString();
                                parameter32.Value = row["家庭地址"].ToString();
                                parameter33.Value = row["家庭电话"].ToString();
                                parameter34.Value = row["手机"].ToString();
                                parameter35.Value = row["工作地点"].ToString();
                                parameter36.Value = DBExecute.Input_double(row["历史公司工龄"].ToString());
                                parameter37.Value = DBExecute.Input_double(row["当前公司工龄"].ToString());
                                parameter38.Value = DBExecute.Input_double(row["历史社会工龄"].ToString());
                                parameter39.Value = row["离职日期"].ToString();
                                parameter40.Value = row["员工备注"].ToString();
                                parameter41.Value = MainIndex_;
                                num++;
                                command.ExecuteNonQuery();
                            }
                            catch (Exception exception)
                            {
                                message_ = exception.Message;
                                return false;
                            }
                        }
                        return flag;
                    }
                    catch (Exception exception2)
                    {
                        flag = false;
                        message_ = exception2.Message;
                    }
                    return flag;
                }
                finally
                {
                    connection.Close();
                    connection.Dispose();
                }
                return flag;
            }
            flag = false;
            message_ = "数据源为空";
            return flag;
        }

        public static string insertJob(int type_, int category__)
        {
            try
            {
                DataTable table = Select_HR_Machines("", "", "true", 1, "");
                if (table.Rows.Count > 0)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        Insert_Job("-1", "-1", -1, type_, false, false, row["ID"].ToString(), category__);
                    }
                }
                return "OK";
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public static void InsertLogMachineUp(int MainIndex__, string title1, string Message1, string ys1)
        {
            Message1 = Message1.Replace("'", " ");
            string str = "INSERT INTO [Log_MachineUp]([ID],[subject],[resultMessage],[resultFlag]) VALUES";
            DBExecute.ExecuteSql(string.Concat(new object[] { str, " (", MainIndex__, ",'", title1, "','", Message1, "','", ys1, "')" }), GetSqlconn());
        }

        public static void K_CheckInOutCopy(int num_, int RecordType_, ref int key, ref string msgg)
        {
            SqlConnection connection = new SqlConnection(GetSqlconn());
            SqlCommand command = new SqlCommand("K_CheckInOutCopy", connection);
            command.CommandType = CommandType.StoredProcedure;
            SqlParameter parameter = command.Parameters.Add("@num", SqlDbType.BigInt);
            SqlParameter parameter2 = command.Parameters.Add("@RecordType", SqlDbType.Int);
            SqlParameter parameter3 = command.Parameters.Add("@Rcount", SqlDbType.Int);
            parameter3.Direction = ParameterDirection.Output;
            parameter.Value = num_;
            parameter2.Value = RecordType_;
            key = 0;
            msgg = "";
            try
            {
                command.CommandTimeout = 0xe10;
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                command.ExecuteNonQuery();
                key = Convert.ToInt32(parameter3.Value.ToString());
            }
            catch (SqlException exception)
            {
                key = -1;
                msgg = exception.Message;
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }

        public static DataTable K_Select_Door(string ID_, string MacID_, string Name_, string AreaID_)
        {
            DataTable table = new DataTable();
            SqlConnection connection = new SqlConnection(GetSqlconn());
            SqlCommand sqlcom = new SqlCommand("K_Select_Door", connection);
            sqlcom.CommandType = CommandType.StoredProcedure;
            SqlParameter parameter = sqlcom.Parameters.Add("@ID", SqlDbType.NVarChar);
            SqlParameter parameter2 = sqlcom.Parameters.Add("@MacID", SqlDbType.NVarChar);
            SqlParameter parameter3 = sqlcom.Parameters.Add("@Name", SqlDbType.NVarChar);
            SqlParameter parameter4 = sqlcom.Parameters.Add("@AreaID", SqlDbType.NVarChar);
            parameter.Value = ID_;
            parameter2.Value = MacID_;
            parameter3.Value = Name_;
            parameter4.Value = AreaID_;
            return DBExecute.Query(sqlcom, connection);
        }

        public static DataTable K_Select_UpLoad(int type_)
        {
            DataTable table = new DataTable();
            SqlConnection connection = new SqlConnection(GetSqlconn());
            SqlCommand sqlcom = new SqlCommand("K_Select_UpLoad", connection);
            sqlcom.CommandType = CommandType.StoredProcedure;
            return DBExecute.Query(sqlcom, connection);
        }

        public static DataTable K_SelectUp(int Type_, int MacId_)
        {
            DataTable table = new DataTable();
            SqlConnection connection = new SqlConnection(GetSqlconn());
            SqlCommand sqlcom = new SqlCommand("K_SelectUp", connection);
            sqlcom.CommandTimeout = 150;
            sqlcom.CommandType = CommandType.StoredProcedure;
            SqlParameter parameter = sqlcom.Parameters.Add("@Type", SqlDbType.Int);
            SqlParameter parameter2 = sqlcom.Parameters.Add("@MacId", SqlDbType.Int);
            parameter.Value = Type_;
            parameter2.Value = MacId_;
            return DBExecute.Query(sqlcom, connection);
        }

        public static void K_Update_UpLoad(string Id_, string Notes_, ref int key, ref string msgg)
        {
            SqlConnection connection = new SqlConnection(GetSqlconn());
            SqlCommand sqlcom = new SqlCommand("K_Update_UpLoad", connection);
            sqlcom.CommandType = CommandType.StoredProcedure;
            SqlParameter parameter = sqlcom.Parameters.Add("@Id", SqlDbType.NVarChar);
            SqlParameter parameter2 = sqlcom.Parameters.Add("@Notes", SqlDbType.NVarChar);
            parameter.Value = Id_;
            parameter2.Value = Notes_;
            ExecuteSql(sqlcom, connection, ref msgg, ref key);
        }

        public static int MaxNum()
        {
            try
            {
                return Convert.ToInt32(DBExecute.Query("select isnull(max(num),0)+1 from K_CheckInOut_Tem ", GetSqlconn()).Rows[0][0].ToString());
            }
            catch
            {
                return 0;
            }
        }

        public static string MocConn(string ip, int port, string pwd)
        {
            try
            {
                return string.Concat(new object[] { "protocol=TCP,ipaddress=", ip, ",port=", port, ",timeout=2000,passwd=", pwd });
            }
            catch
            {
                return "";
            }
        }

        public static void NEW_Insert_HR_CHECKINOUT(DataTable AttTable, string SENSORID_, ref int judgenum_, ref string msg_)
        {
            SqlConnection connection = new SqlConnection(GetSqlconn());
            SqlCommand command = new SqlCommand("Insert_HR_CHECKINOUT", connection);
            command.CommandType = CommandType.StoredProcedure;
            try
            {
                SqlParameter parameter = command.Parameters.Add("@UserCode", SqlDbType.NVarChar);
                SqlParameter parameter2 = command.Parameters.Add("@CHECKTIME", SqlDbType.DateTime);
                SqlParameter parameter3 = command.Parameters.Add("@CHECKTYPE", SqlDbType.NVarChar);
                SqlParameter parameter4 = command.Parameters.Add("@VERIFYCODE", SqlDbType.Int);
                SqlParameter parameter5 = command.Parameters.Add("@SENSORID", SqlDbType.NVarChar);
                SqlParameter parameter6 = command.Parameters.Add("@OperateCode", SqlDbType.NVarChar);
                SqlParameter parameter7 = command.Parameters.Add("@bz", SqlDbType.NVarChar);
                SqlParameter parameter8 = command.Parameters.Add("@judgenum", SqlDbType.Int);
                SqlParameter parameter9 = command.Parameters.Add("@msg", SqlDbType.NVarChar, 200);
                parameter8.Direction = ParameterDirection.Output;
                parameter9.Direction = ParameterDirection.Output;
                string str = "";
                string str2 = "";
                int num = 0;
                string str3 = "";
                int num2 = 0;
                connection.Open();
                foreach (DataRow row in AttTable.Rows)
                {
                    try
                    {
                        str = SENSORID_;
                        str2 = row["RegNo"].ToString();
                        num = DBExecute.R_Int(row["verifyMode"].ToString());
                        str3 = row["INOutMode"].ToString();
                        DateTime time = DBExecute.StartDateTime(row["AttDatatime"].ToString());
                        parameter.Value = str2.ToString();
                        parameter2.Value = time;
                        parameter3.Value = "";
                        parameter4.Value = num;
                        parameter5.Value = str;
                        parameter7.Value = "";
                        parameter6.Value = "";
                        command.ExecuteNonQuery();
                        judgenum_ = Convert.ToInt32(parameter8.Value.ToString().Trim());
                        if (judgenum_ != -1)
                        {
                            num2++;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
                msg_ = num2.ToString();
                judgenum_ = 1;
            }
            catch (SqlException exception)
            {
                judgenum_ = -1;
                msg_ = exception.Message.ToString();
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }

        public static DataTable Select_HR_Machines(string MachineAlias_, string ID_, string Enabled_, int Type_, string AreaID_)
        {
            DataTable dataTable = new DataTable();
            SqlConnection connection = new SqlConnection(GetSqlconn());
            SqlCommand selectCommand = new SqlCommand("Select_HR_Machines", connection);
            selectCommand.CommandType = CommandType.StoredProcedure;
            SqlParameter parameter = selectCommand.Parameters.Add("@MachineAlias", SqlDbType.NVarChar);
            SqlParameter parameter2 = selectCommand.Parameters.Add("@ID", SqlDbType.NVarChar);
            SqlParameter parameter3 = selectCommand.Parameters.Add("@Enabled", SqlDbType.NVarChar);
            SqlParameter parameter4 = selectCommand.Parameters.Add("@Type", SqlDbType.Int);
            SqlParameter parameter5 = selectCommand.Parameters.Add("@AreaID", SqlDbType.NVarChar);
            parameter.Value = MachineAlias_;
            parameter2.Value = ID_;
            parameter3.Value = Enabled_;
            parameter4.Value = Type_;
            parameter5.Value = AreaID_;
            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter(selectCommand);
                connection.Open();
                adapter.Fill(dataTable);
            }
            catch (Exception)
            {
                connection.Close();
            }
            finally
            {
                connection.Close();
            }
            return dataTable;
        }

        public static DataTable Select_SmallUser(string UserSS__, int Tyle_, string deptcode_, int deptChild_, string userKeys_)
        {
            DataTable table = new DataTable();
            SqlConnection connection = new SqlConnection(GetSqlconn());
            SqlCommand sqlcom = new SqlCommand("Select_SmallUser", connection);
            sqlcom.CommandType = CommandType.StoredProcedure;
            SqlParameter parameter = sqlcom.Parameters.Add("@UserSS_", SqlDbType.NVarChar);
            SqlParameter parameter2 = sqlcom.Parameters.Add("@Tyle", SqlDbType.Int);
            SqlParameter parameter3 = sqlcom.Parameters.Add("@deptcode", SqlDbType.NVarChar);
            SqlParameter parameter4 = sqlcom.Parameters.Add("@userKeys", SqlDbType.NVarChar);
            SqlParameter parameter5 = sqlcom.Parameters.Add("@deptChild", SqlDbType.Int);
            parameter.Value = UserSS__;
            parameter2.Value = Tyle_;
            parameter3.Value = deptcode_;
            parameter5.Value = deptChild_;
            parameter4.Value = userKeys_;
            return DBExecute.Query(sqlcom, connection);
        }

        public static string SelectAutoSet()
        {
            string message = "";
            string selectCommandText = "";
            selectCommandText = selectCommandText + "SELECT [IsAutoUpload],[Uploadmode],convert(varchar,[UploadTime],108)as [UploadTime],[UploadTimeGap],[DownAtt],[IsEmail],[ToEmail],[FromEmail],[AddCDP],[AddCachee],[LoginCDP],[IsDoorUpload],[IsConsumeUpload]  FROM [Cachee_AutoSetting] " + " where [TheIndex] = 1";
            SqlConnection selectConnection = new SqlConnection(GetSqlconn());
            SqlDataAdapter adapter = new SqlDataAdapter(selectCommandText, selectConnection);
            DataTable dataTable = new DataTable();
            try
            {
                if (selectConnection.State == ConnectionState.Closed)
                {
                    selectConnection.Open();
                }
                adapter.Fill(dataTable);
                if (dataTable.Rows.Count != 1)
                {
                    return message;
                }
                SetUserInfo_.IsAutoUpload = Convert.ToBoolean(dataTable.Rows[0]["IsAutoUpload"].ToString());
                SetUserInfo_.Uploadmode = Convert.ToInt32(dataTable.Rows[0]["Uploadmode"].ToString());
                SetUserInfo_.UploadTime = dataTable.Rows[0]["UploadTime"].ToString();
                SetUserInfo_.UploadTimeGap = Convert.ToInt32(dataTable.Rows[0]["UploadTimeGap"].ToString());
                SetUserInfo_.DownAtt = Convert.ToBoolean(dataTable.Rows[0]["DownAtt"].ToString());
                SetUserInfo_.IsEmail = Convert.ToBoolean(dataTable.Rows[0]["IsEmail"].ToString());
                SetUserInfo_.ToEmail = dataTable.Rows[0]["ToEmail"].ToString();
                SetUserInfo_.FromEmail = dataTable.Rows[0]["FromEmail"].ToString();
                SetUserInfo_.AddCachee = dataTable.Rows[0]["AddCachee"].ToString();
                SetUserInfo_.AddCDP = dataTable.Rows[0]["AddCDP"].ToString();
                SetUserInfo_.IsConsumeUpload = Convert.ToBoolean(dataTable.Rows[0]["IsConsumeUpload"].ToString());
                SetUserInfo_.IsDoorUpload = Convert.ToBoolean(dataTable.Rows[0]["IsDoorUpload"].ToString());
                SetUserInfo_.LoginCDP = dataTable.Rows[0]["LoginCDP"].ToString();
                if (!string.IsNullOrEmpty(SetUserInfo_.LoginCDP))
                {
                    SetUserInfo_.LoginCDP = clsSecurity.TripDesDecrypt(SetUserInfo_.LoginCDP);
                    string[] strArray = SetUserInfo_.LoginCDP.Split(new char[] { '%' });
                    if (strArray.Length == 3)
                    {
                        SetUserInfo_.LoginAdd = strArray[0].ToString();
                        SetUserInfo_.LoginUser = strArray[1].ToString();
                        SetUserInfo_.LoginPassd = strArray[2].ToString();
                    }
                }
                string[] strArray2 = dataTable.Rows[0]["FromEmail"].ToString().Split(new char[] { ';' });
                if (strArray2.Length == 3)
                {
                    SetUserInfo_.FromEmail_ = strArray2[0].ToString();
                    SetUserInfo_.FromUser = strArray2[1].ToString();
                    SetUserInfo_.FromPsd = strArray2[2].ToString();
                }
                return "OK";
            }
            catch (Exception exception)
            {
                message = exception.Message;
            }
            finally
            {
                selectConnection.Close();
                selectConnection.Dispose();
            }
            return message;
        }

        public static string SendEmail(string to, string from, string subject, string body, string userName, string password, string smtpHost)
        {
            try
            {
                MailAddress address = new MailAddress(from);
                MailAddress address2 = new MailAddress(to);
                MailMessage message = new MailMessage(from, to);
                message.Subject = subject;
                message.IsBodyHtml = true;
                message.Body = body;
                SmtpClient client = new SmtpClient(smtpHost);
                client.Credentials = new NetworkCredential(userName, password);
                client.Send(message);
                return "OK";
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public static string SetUserInfo_CDP(int MainIndex_)
        {
            string message;
            SqlConnection connection = new SqlConnection(GetSqlconn());
            SqlCommand command = new SqlCommand("SetUserInfo_CDP", connection);
            command.CommandType = CommandType.StoredProcedure;
            SqlParameter parameter = command.Parameters.Add("@MainIndex", SqlDbType.Int);
            try
            {
                command.CommandTimeout = 0xe10;
                parameter.Value = MainIndex_;
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                command.ExecuteNonQuery();
                message = "OK";
            }
            catch (Exception exception)
            {
                message = exception.Message;
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
            return message;
        }

        public static string SetUserInfo_CDP_xf(int MainIndex_)
        {
            string message;
            SqlConnection connection = new SqlConnection(GetSqlconn());
            SqlCommand command = new SqlCommand("SetUserInfo_CDP_xf", connection);
            command.CommandType = CommandType.StoredProcedure;
            SqlParameter parameter = command.Parameters.Add("@MainIndex", SqlDbType.Int);
            try
            {
                command.CommandTimeout = 0xe10;
                parameter.Value = MainIndex_;
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                command.ExecuteNonQuery();
                message = "OK";
            }
            catch (Exception exception)
            {
                message = exception.Message;
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
            return message;
        }

        public static bool textboxyh(string input)
        {
            Regex regex = new Regex("^[^'‘’]+$");
            return regex.IsMatch(input);
        }

        public static string UpdateStatu(bool Type, string ID)
        {
            try
            {
                if (DBExecute.Query("select 1 from HR_Machines where [ID] = '" + ID + "' and [statue]= '" + Type.ToString() + "' ", GetSqlconn()).Rows.Count == 0)
                {
                    DBExecute.ExecuteSql("UPDATE [HR_Machines]  SET [statue]= '" + Type.ToString() + "'  where [ID] = '" + ID + "'", GetSqlconn());
                }
                return "";
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public static string WriteApplicationDatabaseConnectionString(string serverName, string userName, string Pwd, string DBs, string yz)
        {
            string errMsg = "";
            if (GetDBs(yz, serverName, userName, Pwd, DBs, ref errMsg))
            {
                Exists();
                XmlDocument document = new XmlDocument();
                try
                {
                    document.Load(File_Name());
                    XmlElement documentElement = document.DocumentElement;
                    foreach (XmlNode node in documentElement.ChildNodes)
                    {
                        string str3 = node.Attributes.GetNamedItem("id").Value;
                        if (str3 != null)
                        {
                            if (!(str3 == "serverName"))
                            {
                                if (str3 == "DBs")
                                {
                                    goto Label_00E9;
                                }
                                if (str3 == "userName")
                                {
                                    goto Label_0107;
                                }
                                if (str3 == "Pwd")
                                {
                                    goto Label_0125;
                                }
                                if (str3 == "yz")
                                {
                                    goto Label_0143;
                                }
                            }
                            else
                            {
                                node.Attributes.GetNamedItem("value").Value = clsSecurity.TripDESCrypt(serverName);
                            }
                        }
                        goto Label_0162;
                    Label_00E9:
                        node.Attributes.GetNamedItem("value").Value = clsSecurity.TripDESCrypt(DBs);
                        goto Label_0162;
                    Label_0107:
                        node.Attributes.GetNamedItem("value").Value = clsSecurity.TripDESCrypt(userName);
                        goto Label_0162;
                    Label_0125:
                        node.Attributes.GetNamedItem("value").Value = clsSecurity.TripDESCrypt(Pwd);
                        goto Label_0162;
                    Label_0143:
                        node.Attributes.GetNamedItem("value").Value = clsSecurity.TripDESCrypt(yz);
                    Label_0162:;
                    }
                    document.Save(File_Name());
                    return "保存成功！";
                }
                catch (SqlException exception)
                {
                    return exception.Message;
                }
            }
            return errMsg;
        }
    }
}

