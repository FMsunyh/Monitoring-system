using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using Microsoft.Win32;
using System.Net;
using System.Management;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using Log.Properties;

namespace Log
{
    public partial class Form1 : Form
    {
        private const int WM_QUEYENDSESSION = 0x11;
        private const int WM_POWERBROADCAST = 0x218;

        private string m_ProcName = Process.GetCurrentProcess().ProcessName;

        private string m_ExecutName = "test";
        private string m_ExecutablePath = null;

        private string m_HostName = null;
        private IPAddress[] m_IpAdressList = null;
        private ManagementClass m_Mc = null;
        private ManagementObjectCollection m_Moc = null;
        private string m_Mac = null;
        private string m_StartTime;
        private string m_ShutDownTime;
        private string m_RunSpan;

        private readonly string m_TempfileName = Application.StartupPath + "\\Listener.exe";

        SqlParameter[] m_Prams = new SqlParameter[6];

        bool m_IsConn = false;

        public Form1()
        {
            InitializeComponent();


            m_IsConn = SqlServer.SqlObject.iSConnection();
            while (m_IsConn == false)
            {
                m_IsConn = SqlServer.SqlObject.iSConnection();
                Thread.Sleep(1800000);
            }

            // 初始化
            Init();

            //设置开机启动
            SetAutoRun(m_ExecutablePath, true);

            //创建一条记录
            CreateLog();
        }

        private static bool ProcInstance()
        {
            int pid;
            if ((pid = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool ListenerInstance()
        {
            int pid;
            if ((pid = Process.GetProcessesByName("Listener").Length) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void StartListener()
        {
            try
            {
                System.Diagnostics.Process.Start(GetListener());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }

        }

        private void Init()
        {
            //初始化表中的6个属性
            m_HostName = Dns.GetHostName();
            m_IpAdressList = Dns.GetHostAddresses(m_HostName);
            m_Mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            m_Moc = m_Mc.GetInstances();
            foreach (ManagementObject mo in m_Moc)
            {
                if (mo["IPEnabled"].ToString() == "True")
                    m_Mac = mo["MacAddress"].ToString();
            }

            m_StartTime = DateTime.Now.ToString();
            m_ShutDownTime = "null";
            m_RunSpan = DateTime.Now.ToString();

            //定时更新数据
            timer1.Interval = 600000;
            timer1.Enabled = true;

            //执行程序地址
            m_ExecutablePath = System.Windows.Forms.Application.ExecutablePath;

            this.WindowState = FormWindowState.Minimized;
            this.Visible = false;
            this.notifyIcon1.Visible = true;
        }

        private string GetListener()
        {
            if (File.Exists(m_TempfileName))
            {
                return m_TempfileName;
            }

            using (FileStream stream = new FileStream(m_TempfileName, FileMode.OpenOrCreate))
            {
                byte[] bs = Resources.Listener;
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(bs);

                writer.Close();
                stream.Close();
            }

            return m_TempfileName;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (ProcInstance() == false || ListenerInstance() == false)
            {
                StartListener();
            }

            this.Visible = false;
            this.notifyIcon1.Visible = true;
        }

        private void SetAutoRun(string fileName, bool isAutoRun)
        {
            RegistryKey reg = null;
            try
            {
                if (!System.IO.File.Exists(fileName))
                    throw new Exception("执行文件不存在!");
                fileName = "\"" + fileName + "\"";
                reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (reg == null)
                    reg = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
                if (isAutoRun)
                    reg.SetValue(m_ExecutName, fileName);
                else
                    reg.SetValue(m_ExecutName, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(null, ex.ToString(), "错误提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (reg != null)
                    reg.Close();
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                //确认注销、重新启动、关机前广播此消息
                case WM_QUEYENDSESSION:
                    {
                        UpdateShutDownTime();
                        break;
                    }

                case WM_POWERBROADCAST:
                    {
                        UpdateShutDownTime();
                        break;
                    }
            }
            base.WndProc(ref m);
        }

        private void CreateLog()
        {
            SqlCommand cmd = new SqlCommand();
            string sql = "insert into T_LogInfo values (@hostname, @mac, @ip, @starttime, @shutDowntime, @runspan)";

            m_Prams[0] = new SqlParameter("@hostname", SqlDbType.VarChar, 50);
            m_Prams[0].Value = this.m_HostName;

            m_Prams[1] = new SqlParameter("@mac", SqlDbType.VarChar, 128);
            m_Prams[1].Value = this.m_Mac;

            m_Prams[2] = new SqlParameter("@ip", SqlDbType.VarChar, 128);
            m_Prams[2].Value = this.m_IpAdressList[0].ToString();

            m_Prams[3] = new SqlParameter("@starttime", SqlDbType.VarChar, 128);
            m_Prams[3].Value = this.m_StartTime;

            m_Prams[4] = new SqlParameter("@shutDowntime", SqlDbType.VarChar, 128);
            m_Prams[4].Value = this.m_ShutDownTime;

            m_Prams[5] = new SqlParameter("@runspan", SqlDbType.VarChar, 128);
            m_Prams[5].Value = this.m_RunSpan;

            if (m_Prams != null)
            {
                foreach (SqlParameter parameter in m_Prams)
                    cmd.Parameters.Add(parameter);
            }

            cmd.CommandText = sql;
            SqlServer.SqlObject.ExecuteCmd(cmd);
        }

        private void UpdateRunSpan()
        {
            string sql;
            SqlCommand cmd;
            SqlParameter para;

            sql = "update T_LogInfo set RunSpan = @RunSpan where id in(select MAX(Id) from T_LogInfo)";
            cmd = new SqlCommand();
            para = new SqlParameter("@RunSpan", SqlDbType.VarChar, 128);
            para.Value = DateTime.Now.ToString();

            cmd.Parameters.Add(para);
            cmd.CommandText = sql;
            SqlServer.SqlObject.ExecuteCmd(cmd);
        }

        private void UpdateShutDownTime()
        {
            string sql;
            SqlCommand cmd;
            SqlParameter para;

            sql = "update T_LogInfo set ShutDownTime = @ShutDownTime where id in(select MAX(Id) from T_LogInfo)";
            cmd = new SqlCommand();
            para = new SqlParameter("@ShutDownTime", SqlDbType.VarChar, 128);
            para.Value = DateTime.Now.ToString();

            cmd.Parameters.Add(para);
            cmd.CommandText = sql;
            SqlServer.SqlObject.ExecuteCmd(cmd);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateRunSpan();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            this.Visible = false;
            this.notifyIcon1.Visible = true;
        }
    }
}