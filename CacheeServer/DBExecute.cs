namespace CacheeServer
{
    using System;
    using System.Data;
    using System.Data.SqlClient;

    public class DBExecute
    {
        public static int ExecuteSql(SqlCommand sqlcom, SqlConnection sqlcon)
        {
            int num = 0;
            using (SqlConnection connection = sqlcon)
            {
                using (SqlCommand command = sqlcom)
                {
                    try
                    {
                        command.CommandTimeout = 0xe10;
                        if (connection.State == ConnectionState.Closed)
                        {
                            connection.Open();
                        }
                        num = command.ExecuteNonQuery();
                    }
                    catch (SqlException)
                    {
                        connection.Close();
                        connection.Dispose();
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

        public static int ExecuteSql(string SQLString, string connectionString)
        {
            int num2;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        command.CommandTimeout = 0xe10;
                        connection.Open();
                        return command.ExecuteNonQuery();
                    }
                    catch (Exception exception)
                    {
                        connection.Close();
                        throw new Exception(exception.Message);
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
            return num2;
        }

        public static string Inpupt_string(string InputValue)
        {
            if (!string.IsNullOrEmpty(InputValue))
            {
                return InputValue;
            }
            return "";
        }

        public static double Input_double(string InputValue)
        {
            try
            {
                return Convert.ToDouble(InputValue);
            }
            catch
            {
                return 0.0;
            }
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

        public static DataTable Query(SqlCommand sqlcom, SqlConnection sqlcon)
        {
            using (SqlConnection connection = sqlcon)
            {
                DataTable dataTable = new DataTable();
                try
                {
                    if (connection.State == ConnectionState.Closed)
                    {
                        connection.Open();
                    }
                    new SqlDataAdapter(sqlcom).Fill(dataTable);
                }
                catch (SqlException)
                {
                    connection.Close();
                    connection.Dispose();
                }
                finally
                {
                    connection.Close();
                    connection.Dispose();
                }
                return dataTable;
            }
        }

        public static DataTable Query(string SQLString, string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                DataTable dataTable = new DataTable();
                try
                {
                    connection.Open();
                    new SqlDataAdapter(SQLString, connection).Fill(dataTable);
                }
                catch (Exception exception)
                {
                    connection.Close();
                    throw new Exception(exception.Message);
                }
                finally
                {
                    connection.Close();
                }
                return dataTable;
            }
        }

        public static int R_Int(string text)
        {
            int num = -1;
            if (string.IsNullOrEmpty(text))
            {
                return num;
            }
            try
            {
                return Convert.ToInt32(text);
            }
            catch
            {
                return -1;
            }
        }

        public static DateTime StartDateTime(string text)
        {
            DateTime time = new DateTime();
            if (!string.IsNullOrEmpty(text))
            {
                try
                {
                    return Convert.ToDateTime(text);
                }
                catch
                {
                    return DateTime.Parse("1910-01-01");
                }
            }
            return DateTime.Parse("1910-01-01");
        }
    }
}

