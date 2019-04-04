//This Class library is build for SSH client using Secure black Box 
//Secure Black Box Leicence key is properity of Nokia Siemense networks

//INSTANT C# NOTE: Formerly VB project-level imports:
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using SBSSHClient;
using SBSSHUtils;
using SBSSHCommon;
using SBSimpleSSH;
using SBSSHKeyStorage;
using System.Text;
using System.Runtime.Remoting.Messaging;

namespace ScriptingSSH
{
    public class ScriptingSSH
    {

        public TElSimpleSSHClient client = new TElSimpleSSHClient();
        public string ServerName = "NoName";
        public string LOG = "";
        public int timeout = 30;
        private System.Text.StringBuilder strFullLog = new System.Text.StringBuilder();
        private System.Text.StringBuilder strWorkingData = new System.Text.StringBuilder(); // Holds everything received from the server since our last processing
        private byte[] m_byBuff = new byte[32767];
        private TElSSHMemoryKeyStorage Key_Storage = new TElSSHMemoryKeyStorage();
        private string NewLineChar = "\r";


        public delegate Int32 RecieveData();

        public ScriptingSSH(string Address, int Port, string User_name, string Pass_word, int CommandTimeout)
        {
            //Me.address = Address
            client = new TElSimpleSSHClient();
            client.Address = Address;
            client.Port = Port;
            client.Username = User_name;
            client.Password = Pass_word;
            timeout = CommandTimeout;
            SBUtils.Unit.SetLicenseKey(SBUtils.Unit.BytesOfString("184B4EC05D9FF7847A5D2E320BBBE02459A6B654370E360720A03574465EB43DB4DE16303976FFE976CEA790A4121B22A26499520FC9DA651B9EFB1AE0ADF0A355C78A0542B04174638736CA7E453430F1A00EEDE5B66FAEEEEC962D4A14F946403AD8816E7A9A569684A3A00B2C6D381C4C0351BD3BA9087A80037CD1A18D503CC629E2C8DE45A4DB75E92138DEFF2EDE1C9B054837345C3EE397BC7B0AEDDCD1B7D2B62BDFF1C205CF32BC58F2E9854E1DAE62C18B56C3784B36443464E16944F240D888FACA9DACF29FF8704398B87AD61E6F45C46BCBA067AD74783C9EE89C5EB440CC65BBA14D0A027D60DDE23F48DC8D72AD4502F57762D20BBEE420E4"));
            client.Versions = 0 | SBSSHCommon.Unit.sbSSH2 | SBSSHCommon.Unit.sbSSH1;
            //client.AuthenticationTypes = SBSSHConstants.Unit.SSH_AUTH_TYPE_PASSWORD | SBSSHConstants.Unit.SSH_AUTH_TYPE_KEYBOARD;
            client.AuthenticationTypes = SBSSHConstants.Unit.SSH_AUTH_TYPE_PASSWORD | SBSSHConstants.Unit.SSH_AUTH_TYPE_KEYBOARD & ~SBSSHConstants.Unit.SSH_AUTH_TYPE_PUBLICKEY;
            Key_Storage.Clear();
            client.KeyStorage = Key_Storage;
            client.OnKeyValidate += this.m_Client_OnKeyValidate;

        }

        #region        Additional
        private System.Text.RegularExpressions.Regex RegexPattern = new System.Text.RegularExpressions.Regex("\\[01;\\d+m");
        private string CleanDisplay(string input)
        {
            input = input.Replace("(0x (B", "|");
            input = input.Replace("(0 x(B", "|");
            input = input.Replace(")0=>", "");
            input = input.Replace("[0m>", "");
            input = input.Replace("7[7m", "[");
            input = input.Replace("[0m*8[7m", "]");
            input = input.Replace("[0m", "");
            input = input.Replace("[00m", "");
            input = input.Replace("[01", "");
            input = input.Replace("[m", "");
            input = input.Replace("\0", "");
            input = input.Replace("\r\0", "");
            input = RegexPattern.Replace(input, "");
            return input;
        }
        #endregion

        public bool Connect(string WaitForPrompt)
        {

            try
            {
                // If the connect worked, setup a callback to start listening for incoming data
                // Dim recieveData As New AsyncCallback(AddressOf OnRecievedData)
                client.Open();

                RecieveData recievedata = new RecieveData(Recieve);
                recievedata.BeginInvoke(new AsyncCallback(OnRecievedData), recievedata);
                // Threading.Thread.Sleep(5000)
                try
                {
                    this.WaitFor(WaitForPrompt);
                }
                catch
                {
                    throw new Exception("Prompt Not Returned");
                }

                return true;
            }
            catch (Exception ex)
            {
                writelog(System.Environment.NewLine + "------54----------" + ex.Message + System.Environment.NewLine + ex.Source + "----------------");
                return false;
            }

        }

        public bool Connect()
        {

            try
            {
                // If the connect worked, setup a callback to start listening for incoming data
                // Dim recieveData As New AsyncCallback(AddressOf OnRecievedData)
                client.Open();

                RecieveData recievedata = new RecieveData(Recieve);
                recievedata.BeginInvoke(new AsyncCallback(OnRecievedData), recievedata);
                // Threading.Thread.Sleep(5000)

                return true;
            }
            catch (Exception ex)
            {
                writelog(System.Environment.NewLine + "------54----------" + ex.Message + System.Environment.NewLine + ex.Source + "----------------");
                return false;
            }

        }

        public bool Disconnect()
        {
            //INSTANT C# TODO TASK: The 'On Error Resume Next' statement is not converted by Instant C#:
            //On Error Resume Next
            try
            {
                client.Close();
                //client.Dispose();
                GC.Collect();
                return true;
            }
            catch
            {
                GC.Collect();
                return false;
                //client.Dispose();
            }
        }

        /// <summary>
        /// Recieve Delegate to collect Data
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private int Recieve()
        {
            long lngStart = System.DateTime.Now.AddSeconds(this.timeout).Ticks;
            long lngCurTime = 0;

            bool CanRecv = true;
            while (CanRecv)
            {
                // Timeout logic
                lngCurTime = System.DateTime.Now.Ticks;
                if (lngCurTime > lngStart)
                {
                    return -1;
                }

                System.Threading.Thread.Sleep(5);
                try
                {
                    CanRecv = client.CanReceive(0);
                }
                catch (Exception ex)
                {
                    writelog(System.Environment.NewLine + "------83----------" + ex.Message + System.Environment.NewLine + ex.Source.ToString() + "----------------");
                    return -1;
                }

                if (CanRecv == true)
                {
                    try
                    {
                        int sz = client.ReceiveData(m_byBuff, 0, 32766, true);
                        if (sz > 0)
                        {
                            return sz;
                        }
                    }
                    catch (Exception ex)
                    {
                        writelog(System.Environment.NewLine + "------94----------" + ex.Message + System.Environment.NewLine + ex.Source.ToString() + "----------------");
                        return -1;
                    }
                }
                System.Threading.Thread.Sleep(10);
            }
            return -1;
        }

        /// <summary>
        /// Callback function for recieve data
        /// </summary>
        /// <param name="result"></param>
        /// <remarks></remarks>
        private void OnRecievedData(IAsyncResult result)
        {

            var resultClass = (AsyncResult)result;
            RecieveData d = (RecieveData)resultClass.AsyncDelegate;

            Int32 sz = d.EndInvoke(result);

            if (sz > 0)
            {
                
                string sRecieved = CleanDisplay(Encoding.ASCII.GetString(m_byBuff, 0, sz));
                // Write out the data
                //If sRecieved.IndexOf("[c") <> -1 Then
                //    Negotiate(1)
                //End If
                //If sRecieved.IndexOf("[6n") <> -1 Then
                //    Negotiate(2)
                //End If

                this.strWorkingData.Append(sRecieved.ToLower());
                this.strFullLog.Append(sRecieved);
                this.LOG = this.LOG + sRecieved;
                //Console.Write(sRecieved);

                //here take some rest
                System.Threading.Thread.Sleep(10);

                //create a new delegate
                RecieveData recievedata = new RecieveData(Recieve);
                recievedata.BeginInvoke(new AsyncCallback(OnRecievedData), recievedata);
            }
            else
            {
                try
                {
                    if (client.Active) //here if client is still active then
                    {
                        //here take some rest
                        System.Threading.Thread.Sleep(10);
                        RecieveData recievedata = new RecieveData(Recieve);
                        recievedata.BeginInvoke(new AsyncCallback(OnRecievedData), recievedata);
                    }
                }
                catch (Exception ex)
                {
                    writelog(System.Environment.NewLine + "------139----------" + ex.Message + System.Environment.NewLine + ex.Source.ToString() + "----------------");
                }
            }

        }

        /// <summary>
        /// Waits for a specific string to be found in the stream from the server.
        /// Once that string is found, sends a message to the server
        /// </summary>
        /// <param name="WaitFor">The string to be found in the server stream</param>
        /// <param name="Message">The message to send to the server</param>
        /// <returns>Returns true once the string has been found, and the message has been sent</returns>
        public bool WaitAndSend(string WaitFor, string Message)
        {
            this.WaitFor(WaitFor);
            SendMessage(Message);
            return true;
        }

        /// <summary>
        /// Waits for a specific string to be found in the stream from the server
        /// </summary>
        /// <param name="DataToWaitFor">The string to wait for</param>
        /// <returns>Always returns 0 once the string has been found</returns>
        public int WaitFor(string DataToWaitFor)
        {

            // Get the starting time
            long lngStart = System.DateTime.Now.AddSeconds(this.timeout).Ticks;
            long lngCurTime = 0;
            //  Dim start_index As UInt64 = 0
            //  Dim End_index As UInt64 = 0
            string LN = "";
            // Dim L As Integer = 0
            while (LN.IndexOf(DataToWaitFor.ToLower()) == -1)
            {
                // Timeout logic
                lngCurTime = System.DateTime.Now.Ticks;
                if (lngCurTime > lngStart)
                {
                    throw new Exception("Timed Out waiting for : " + DataToWaitFor);
                }
                System.Threading.Thread.Sleep(5);

                if (LN.IndexOf("idle too long; timed out") != -1)
                {
                    //intReturn = -2
                    if (LN.IndexOf(DataToWaitFor.ToLower()) != -1)
                    {
                        return 0;
                    }
                    strWorkingData = new StringBuilder();
                    throw new Exception("Connection Terminated forcefully");
                }

                //  L = strWorkingData.Length
                LN = strWorkingData.ToString(0, strWorkingData.Length);

                // End_index = If(LN.Length < 50, 0, LN.Length - 50)
                strWorkingData.Remove(0, ((LN.Length < 50) ? 0 : LN.Length - 50)); //CLIPPING OF LN FROM WORKING DATA

            }
            strWorkingData.Length = 0;
            return 0;
        }

        /// <summary>
        /// Waits for one of several possible strings to be found in the stream from the server
        /// </summary>
        /// <param name="DataToWaitFor">A delimited list of strings to wait for</param>
        /// <param name="BreakCharacter">The character to break the delimited string with</param>
        /// <returns>The index (zero based) of the value in the delimited list which was matched</returns>
        public int WaitFor(string DataToWaitFor, string BreakCharacter)
        {
            // Get the starting time
            long lngStart = System.DateTime.Now.AddSeconds(this.timeout).Ticks;
            long lngCurTime = 0;
            //  Dim start_index As UInt64 = 0
            //  Dim End_index As UInt64 = 0
            string LN = "";

            string[] Breaks = DataToWaitFor.Split(BreakCharacter.ToCharArray());
            int intReturn = -1;

            while (intReturn == -1)
            {
                // Timeout logic
                lngCurTime = System.DateTime.Now.Ticks;
                if (lngCurTime > lngStart)
                {
                    throw new Exception("Timed Out waiting for : " + DataToWaitFor);
                }

                System.Threading.Thread.Sleep(5);
                for (int i = 0; i < Breaks.Length; i++)
                {
                    if (LN.IndexOf(Breaks[i].ToLower()) != -1)
                    {
                        intReturn = i;
                    }
                }

                if (LN.IndexOf("idle too long; timed out") != -1)
                {
                    intReturn = 0;
                    for (int i = 0; i < Breaks.Length; i++)
                    {
                        if (strWorkingData.ToString().ToLower().IndexOf(Breaks[i].ToLower()) != -1)
                        {
                            return i;
                        }
                    }
                    throw new Exception("Connection Terminated forcefully");
                }

                //start_index = If(End_index - 50 < 0, 0, End_index - 50)
                //End_index = strWorkingData.Length
                //LN = strWorkingData.ToString.Substring(start_index, End_index - start_index).ToLower()

                // L = strWorkingData.Length
                LN = strWorkingData.ToString(0, strWorkingData.Length);
                // End_index = If(LN.Length < 50, 0, LN.Length - 50)
                strWorkingData.Remove(0, ((LN.Length < 50) ? 0 : LN.Length - 50)); //CLIPPING OF LN FROM WORKING DATA
            }
            strWorkingData.Length = 0;
            return intReturn;

        }

        /// <summary>
        /// Sends a message to the server, and waits until the designated
        /// response is received
        /// </summary>
        /// <param name="Message">The message to send to the server</param>
        /// <param name="WaitFor">The response to wait for</param>
        /// <returns>True if the process was successful</returns>
        public int SendAndWait(string Message, string WaitFor)
        {
            strWorkingData.Length = 0;
            SendMessage(Message);
            this.WaitFor(WaitFor);
            return 0;
        }

        /// <summary>
        /// Throw Send Text and wait for required serch option
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="WaitFor"></param>
        /// <param name="BreakCharacter"></param>
        /// <param name="SuppressCarriegeReturn"></param>
        /// <returns></returns>
        /// <remarks></remarks>

        public int SendAndWait(string Message, string WaitFor, string BreakCharacter)
        {
            return SendAndWait(Message, WaitFor, BreakCharacter, false);
        }

        //INSTANT C# NOTE: Overloaded method(s) are created above to convert the following method having optional parameters:
        //ORIGINAL LINE: Public Function SendAndWait(ByVal Message As String, ByVal WaitFor As String, ByVal BreakCharacter As String, Optional ByVal SuppressCarriegeReturn As Boolean = false) As Integer
        public int SendAndWait(string Message, string WaitFor, string BreakCharacter, bool SuppressCarriegeReturn)
        {
            SendMessage(Message, SuppressCarriegeReturn);
            int t = this.WaitFor(WaitFor, BreakCharacter);
            return t;
        }

        /// <summary>
        /// SendMessage to client
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="SuppressCarriageReturn"></param>
        /// <remarks></remarks>

        public void SendMessage(string Message)
        {
            SendMessage(Message, false);
        }

        //INSTANT C# NOTE: Overloaded method(s) are created above to convert the following method having optional parameters:
        //ORIGINAL LINE: Public Sub SendMessage(ByVal Message As String, Optional ByVal SuppressCarriageReturn As Boolean = false)
        public void SendMessage(string Message, bool SuppressCarriageReturn)
        {
            if (!SuppressCarriageReturn)
            {
                DoSend(Message + NewLineChar);
            }
            else
            {
                DoSend(Message);
            }
        }

        /// <summary>
        /// Throw text in client
        /// </summary>
        /// <param name="strText"></param>
        /// <remarks></remarks>
        private void DoSend(string strText)
        {
            try
            {
                //here we are setting working data to "" 
                this.strWorkingData.Length = 0;
                this.client.SendText(strText);
            }
            catch (Exception ers)
            {
                //MessageBox.Show("ERROR IN RESPOND OPTIONS");
            }
        }


        /// <summary>
        /// Check Connetion is if live
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsConnected()
        {
            try
            {
                return client.Active;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// A full log of session activity
        /// </summary>
        public string SessionLog
        {
            get
            {
                string ln = this.strFullLog.ToString();
                return ln;
            }
        }

        /// <summary>
        /// Clears all data in the session log
        /// </summary>
        public void ClearSessionLog()
        {
            this.strFullLog.Length = 0;
            this.strWorkingData.Length = 0;
        }

        public string Set_NewLine_Char
        {
            get
            {
                return NewLineChar;
            }
            set
            {
                if (value == "\r")
                {
                    NewLineChar = "\r";
                }
                else
                {
                    NewLineChar = "\r" + "\n";
                }
            }
        }

        #region        connection

        private void m_Client_OnKeyValidate(object Sender, SBSSHKeyStorage.TElSSHKey ServerKey, ref bool Validate)
        {
            //System.Console.WriteLine("Server key received")
            Validate = true; // NEVER do this. You MUST check the key somehow
        }

        #endregion

        #region        obsolete
        private void Negotiate(int WhichPart)
        {
            StringBuilder x = null;
            string neg = null;
            if (WhichPart == 1)
            {
                x = new StringBuilder();
                x.Append((char)(27));
                x.Append((char)(91));
                x.Append((char)(63));
                x.Append((char)(49));
                x.Append((char)(59));
                x.Append((char)(50));
                x.Append((char)(99));
                neg = x.ToString();
            }
            else
            {

                x = new StringBuilder();
                x.Append((char)(27));
                x.Append((char)(91));
                x.Append((char)(50));
                x.Append((char)(52));
                x.Append((char)(59));
                x.Append((char)(56));
                x.Append((char)(48));
                x.Append((char)(82));
                neg = x.ToString();
            }
            SendMessage(neg, true);
        }
        #endregion

        private void writelog(string msg)
        {
            //INSTANT C# TODO TASK: The 'On Error Resume Next' statement is not converted by Instant C#:
            //On Error Resume Next
            System.IO.StreamWriter s = new System.IO.StreamWriter("ScriptingSSH_Error.log", true);
            s.Write(msg);
            s.Close();
            s.Dispose();
        }


    }



} //end of root namespace