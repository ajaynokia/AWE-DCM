using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form4 : Form
    {

        public ScriptingSSH.ScriptingSSH TelnetSession = null;
        public Form4()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {           
            if (TelnetSession.IsConnected() == false)
            {
                TelnetSession.Connect();
            }
            listBox1.Items.Add("Trying to connect to Router.");
            if (TelnetSession.IsConnected() == false)
            {
                listBox1.Items.Add("Router Login Failed.");
            }
            //TelnetSession.ClearSessionLog();
            //if (TelnetSession.IsConnected() == true)
            //{
            else
            {
                listBox1.Items.Add("Router is Connected.");
                if (Convert.ToInt32(txtNePort.Text.ToString()) == 22)
                {
                    try
                    {
                        TelnetSession.SendAndWait("ssh -l " + txtNeUser.Text.ToString().Trim() + " " + txtNEIP.Text.ToString().Trim(), txtPattern.Text.ToString().Trim(), "|", false);

                    }
                    catch (Exception ex)
                    {
                        listBox1.Items.Add("Error while giving ssh -l user IP: " + ex.Message.ToString());
                        listBox1.Items.Add(TelnetSession.SessionLog.ToString());

                        CreateErrorMessage(ex);

                    }
                    try
                    {
                        TelnetSession.SendAndWait(txtNEPwd.Text.ToString().Trim(), txtPattern.Text.ToString().Trim(), "|", false);
                    }
                    catch (Exception ex)
                    {
                        listBox1.Items.Add("Error while giving password: " + ex.Message.ToString());
                        listBox1.Items.Add(TelnetSession.SessionLog.ToString());
                        CreateErrorMessage(ex);
                    }
                }
            }
            //}
            if (Convert.ToInt32(txtNePort.Text.ToString()) == 23)
            {
                try
                {
                    TelnetSession.SendAndWait("telnet " + txtNEIP.Text.ToString().Trim(), txtPattern.Text.ToString().Trim(), "|", false);
                }
                catch (Exception ex)
                {
                    listBox1.Items.Add("Error while giving IP: " + ex.Message.ToString());
                    listBox1.Items.Add(TelnetSession.SessionLog.ToString());
                }
                try
                {
                    TelnetSession.SendAndWait(txtNeUser.Text.ToString().Trim(), txtPattern.Text.ToString().Trim(), "|", false);
                }
                catch (Exception ex)
                {
                    listBox1.Items.Add("Error while giving User Name: " + ex.Message.ToString());
                    listBox1.Items.Add(TelnetSession.SessionLog.ToString());
                }
                try
                {
                    TelnetSession.SendAndWait(txtNEPwd.Text.ToString().Trim(), txtPattern.Text.ToString().Trim(), "|", false);
                }
                catch (Exception ex)
                {
                    listBox1.Items.Add("Error while giving password: " + ex.Message.ToString());
                    listBox1.Items.Add(TelnetSession.SessionLog.ToString());
                }
            }

        }

        private void btncommand_Click(object sender, EventArgs e)
        {
            if (TelnetSession.IsConnected() == false)
            {
                TelnetSession.Connect();
            }
            try
            {
                TelnetSession.SendAndWait(txtcommand.Text.ToString().Trim(), txtPattern.Text.ToString().Trim(), "|", false);
                listBox1.Items.Add(TelnetSession.SessionLog.ToString());
            }
            catch (Exception ex)
            {
                listBox1.Items.Add("Error while giving command: " + ex.Message.ToString());
                listBox1.Items.Add(TelnetSession.SessionLog.ToString());
                //CreateErrorMessage(ex);                
            }

        }

        private void Form4_Load(object sender, EventArgs e)
        {
            TelnetSession = new ScriptingSSH.ScriptingSSH(txtRtrIP.Text.ToString().Trim(), Convert.ToInt32(txRtrPort.Text.ToString()), txtRtrUser.Text.ToString(), txtRtrPwd.Text.ToString(), 300);
            
        }

        public static string CreateErrorMessage(Exception serviceException)
        {
            StringBuilder messageBuilder = new StringBuilder();

            try
            {
                messageBuilder.Append("The Exception is:-");

                messageBuilder.Append("Exception :: " + serviceException.ToString());
                if (serviceException.InnerException != null)
                {
                    messageBuilder.Append("InnerException :: " + serviceException.InnerException.ToString());
                }
                return messageBuilder.ToString();
            }
            catch
            {
                messageBuilder.Append("Exception:: Unknown Exception.");
                return messageBuilder.ToString();
            }

        }

        private void Logs(string Data)
        {
            try
            {
                if (System.Configuration.ConfigurationManager.AppSettings["IsLogEnable"].ToString() == "Y")
                {
                    string Path = @"" + System.Configuration.ConfigurationManager.AppSettings["ErrorLogPath"].ToString();
                    if (!System.IO.Directory.Exists(Path))
                    {
                        System.IO.Directory.CreateDirectory(Path);
                    }

                    System.IO.StreamWriter s = new System.IO.StreamWriter(Path + "\\Auto_FLT_SMS_Error.log", true);
                    s.Write("\r\n" + System.DateTime.Now.ToString("HH:mm") + ": " + Data);
                    s.Close();
                    s.Dispose();
                }
            }
            catch
            {

            }
        }
    }
}
