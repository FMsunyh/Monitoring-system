using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;
using Log.Properties;

namespace Log
{
    public class SqlServer
    {
        public static string ConnectionString = Settings.Default.ConnectionString;
        //public static string ConnectionString2 = @"Data Source=VBTDS9OSYBWSXSD\SQLEXPRESS;Initial Catalog=LogData;Integrated Security=True;Pooling=False";

        public static SqlServer SqlObject
        {
            get
            {
                return GreateSqlServer.SqlInstance;
            }
        }
        private SqlServer() { }

        class GreateSqlServer
        {
            static GreateSqlServer() { }

            internal static readonly SqlServer SqlInstance = new SqlServer();
        }


        public bool iSConnection()
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(SqlServer.ConnectionString);
                conn.Open();
                conn.Close();
                return true;
            }
            catch 
            {
                return false;
            }
        }

        /// <summary>
        /// 创建一个SqlConnection
        /// </summary>
        /// <returns> 返回 conn </returns>
        public SqlConnection GetConn()
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(SqlServer.ConnectionString);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return conn;
        }

        public SqlCommand GetCommand(string sql, SqlConnection conn)
        {
            SqlCommand cmd = null;
            try
            {
                cmd = new SqlCommand(sql, conn);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return cmd;

        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="smd"></param>
        /// <returns></returns>
        public bool ExecuteCmd(SqlCommand cmd)
        {
            cmd.Connection = GetConn();
            try
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                return true;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Clone();
                    cmd.Dispose();
                }
                if (cmd.Connection != null)
                {
                    cmd.Connection.Close();
                    cmd.Connection.Dispose();
                }
            }
        }
    }
}
