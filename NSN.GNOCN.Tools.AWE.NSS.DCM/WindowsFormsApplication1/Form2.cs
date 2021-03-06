﻿using System;
using System.Data;
using System.Windows.Forms;
using System.IO;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using NSN.GNOCN.Tools.AWE.NSS;

namespace WindowsFormsApplication1
{
    public partial class Form2 : Form
    {
        public MySqlConnection conn;
        public string group_name = string.Empty;
        public string _ne_name;
        public string _vendor;
        public string _NEType;
        public string CircleName;
        public string _NE_MAP;
        StreamReader sr;

        public Form2()
        {
            InitializeComponent();
            conn = new MySqlConnection("Data Source=93.183.30.215;database=awe_dcm;User Id=root;Password=tiger;allow zero datetime=true;Persist Security Info=true; default command timeout = 2000");

            _vendor = "NSN";
            CircleName = "WB";
            group_name = "HLR_MSC";
            _NE_MAP = "MSS MAP";
            _NEType = "MSC";
            _ne_name = "AND_MSC";

        }

        private void button1_Click(object sender, EventArgs e)
        {
            clsCSParsingProcess objParse1 = new clsCSParsingProcess();
            objParse1.group_name = "group3";
            objParse1.NeMap = new string[1];
            objParse1.NeMap[0] = "MSS MAP";
            objParse1.StartParsing();


            //clsCSGetRawProcess obj = new clsCSGetRawProcess();
            //obj.nodeTypeMain = new string[3];
            //obj.nodeTypeMain[0] = "MSS";
            //obj.nodeTypeMain[1] = "GCS";
            //obj.nodeTypeMain[2] = "MGW";
            //obj.groupName = "group3";
            //obj.StartRawGeneration();

            //string Data;
            //string PATH = @"D:\CS_DCM\DCM_RAW\20_06_2014\WB\MSC\ZQRI\AND_MSC.txt";
            //string sCommandName = "ZQRI";
            //sr = new StreamReader(PATH);

            //Data = sr.ReadToEnd();

            //ParsefromTextFile(Data, CircleName, _NE_MAP, _NEType, sCommandName);
        }
        public bool SQLQuery(ref MySqlConnection MYSQL_connection, ref DataTable DTCombo, string querystr)
        {
            DTCombo = new DataTable();
            MySqlDataAdapter ADAPTCombo = new MySqlDataAdapter();
            MySqlCommand mMySqlCommand = new MySqlCommand();


            mMySqlCommand.CommandText = querystr;
            mMySqlCommand.CommandType = CommandType.Text;
            mMySqlCommand.Connection = MYSQL_connection;

            if (MYSQL_connection.State != ConnectionState.Open)
            {
                conn.Open();
            }

            if (MYSQL_connection.State != ConnectionState.Open)
            {
                conn.Open();
            }
            ADAPTCombo.ReturnProviderSpecificTypes = true;
            mMySqlCommand.Connection = MYSQL_connection;
            ADAPTCombo.SelectCommand = mMySqlCommand;
            try
            {
                ADAPTCombo.Fill(DTCombo);
                MYSQL_connection.Close();
                conn.Close();
                return true;

            }
            catch (MySqlException ex)
            {
                conn.Close();
                return false;
            }
        }
        public bool ExecuteSQLQuery(ref MySqlConnection MYSQL_connection, string querystr)
        {
            MySqlCommand mMySqlCommand = new MySqlCommand();

            mMySqlCommand.CommandText = querystr;
            mMySqlCommand.CommandType = CommandType.Text;
            mMySqlCommand.Connection = MYSQL_connection;


            if (MYSQL_connection.State != ConnectionState.Open)
            {
                conn.Open();
            }

            if (MYSQL_connection.State != ConnectionState.Open)
            {
                conn.Open();
            }
            try
            {
                mMySqlCommand.Connection = MYSQL_connection;
                mMySqlCommand.ExecuteNonQuery();
                MYSQL_connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                return false;
            }
        }

        public void ParseData_ZNRI_MSS(string output, string _CircleName, string NE_Type, string NET)   // change here for root set 
        {
            string link_name = string.Empty;
            string MSS_SPC = string.Empty;
            string temp = string.Empty;
            string[] tebihar_local;
            string[] temp1;
            string SqlStr = string.Empty;
            DataTable dt = new DataTable();
            string[] cmnd_data = Regex.Split(output, "\r\n");

            for (int i = 0; i <= cmnd_data.Length - 1; i++)
            {
                try
                {
                    if (cmnd_data[i].ToString().Trim().Contains("SP CODE H/D") && cmnd_data[i].ToString().Trim().Contains("NET"))
                    {
                        i = i + 2;

                        if (cmnd_data[i].Contains(NET) && cmnd_data[i].Contains("OWN SP"))
                        {
                            temp = cmnd_data[i].Trim();
                            temp = Regex.Replace(temp.ToString().Trim(), " {2,}", "~");
                            temp1 = Regex.Split(temp, "~");
                            MSS_SPC = temp1[1].Trim().ToString();

                            if (MSS_SPC != "")
                            {
                                if (NE_Type == "HLR")
                                {

                                    SQLQuery(ref conn, ref dt, SqlStr);
                                    //ExecuteSQLQuery(ref conn, "Update hlr_" + _CircleName + " set HLR_SPC='" + MSS_SPC + "'  where NE_NAME='" + _ne_name.Trim() + "' and NODE_Type='" + NE_Type + "' and Circle='" + _CircleName + "' and Vendor='" + _vendor.Trim() + "' AND CGR_NET = '" + NET + "'");
                                }
                                else if (NE_Type == "MSS_LINKS")
                                {

                                    SQLQuery(ref conn, ref dt, SqlStr);
                                    //ExecuteSQLQuery(ref conn, "Update mss_link_" + _CircleName + "_" + group_name + " set MSS_SPC='" + MSS_SPC + "'  where NE_NAME='" + _ne_name.Trim() + "' and NODE_Type='" + NE_Type + "' and Circle='" + _CircleName + "' and Vendor='" + _vendor.Trim() + "' AND CGR_NET = '" + NET + "'");

                                }
                                else if (NE_Type == "MSC")
                                {

                                    SQLQuery(ref conn, ref dt, SqlStr);
                                    //ExecuteSQLQuery(ref conn, "update msc_" + _CircleName + " set MSS_SPC = '" + MSS_SPC + "' where NE_NAME = '" + _ne_name.Trim() + "' AND NODE_TYPE = '" + NE_Type + "' AND circle = '" + _CircleName + "' AND CGR_NET = '" + NET + "' and Vendor='" + _vendor.Trim() + "' ");
                                }

                                else
                                    // for mss

                                    SQLQuery(ref conn, ref dt, SqlStr);
                                //ExecuteSQLQuery(ref conn, "update mss_mgw_map_" + _CircleName + "_" + group_name + " set MSS_SPC = '" + MSS_SPC + "' where NE_NAME = '" + _ne_name.Trim() + "' AND NODE_TYPE = '" + NE_Type + "' AND circle = '" + _CircleName + "' AND CGR_NET = '" + NET + "' and Vendor='" + _vendor.Trim() + "' ");

                            }
                        }
                        else
                        {
                            tebihar_local = cmnd_data[i].Trim().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            MSS_SPC = tebihar_local[1].Trim().ToString();
                            link_name = tebihar_local[2].Trim().ToString();
                            if (MSS_SPC != "")
                            {

                                if (NE_Type == "HLR")
                                {
                                    SqlStr = "select distinct * from (select VENDOR,CIRCLE,NE_NAME,ELEMENT_IP,CGR_NET,HLR_C_NO from  hlr_" + _CircleName + " where CGR_NET='" + NET + "' and NE_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "')A";
                                    dt.Clear();
                                    SQLQuery(ref conn, ref dt, SqlStr);
                                    if (dt.Rows.Count >= 1)
                                    {

                                        SqlStr = "insert into hlr_" + _CircleName + " (VENDOR,CIRCLE,NE_NAME,ELEMENT_IP,CGR_NET,HLR_C_NO,CGR_SPC,LINKSET) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["NE_NAME"] + "','" + dt.Rows[0]["ELEMENT_IP"] + "','" + dt.Rows[0]["CGR_NET"] + "','" + dt.Rows[0]["HLR_C_NO"] + "','" + MSS_SPC + "','" + link_name + "')";
                                        //ExecuteSQLQuery(ref conn, SqlStr);
                                    }

                                }
                                else if (NE_Type == "MSS_LINKS")
                                {
                                    SqlStr = "select distinct * from (select VENDOR,CIRCLE,NE_NAME,MSS,ELEMENT_IP,CGR_NET from  mss_link_" + _CircleName + "_" + group_name + " where CGR_NET='" + NET + "' and NE_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' )A";
                                    dt.Clear();
                                    SQLQuery(ref conn, ref dt, SqlStr);
                                    if (dt.Rows.Count >= 1)
                                    {

                                        SqlStr = "insert into mss_link_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,NE_NAME,ELEMENT_IP,CGR_NET,CGR_SPC,LINKSET) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["NE_NAME"] + "','" + dt.Rows[0]["ELEMENT_IP"] + "','" + dt.Rows[0]["CGR_NET"] + "','" + MSS_SPC + "','" + link_name + "')";
                                        //ExecuteSQLQuery(ref conn, SqlStr);
                                    }


                                }
                                else if (NE_Type == "MSC")
                                {
                                    SqlStr = "select distinct * from (select VENDOR,CIRCLE,NE_NAME,MCC,MNC,MSS,ELEMENT_IP,CGR_NET from  msc_" + _CircleName + " where CGR_NET='" + NET + "' and NE_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' and mcc<> '' and mnc <> '')A";
                                    SQLQuery(ref conn, ref dt, SqlStr);
                                    if (dt.Rows.Count >= 1)
                                    {

                                        SqlStr = "insert into msc_" + _CircleName + " (VENDOR,CIRCLE,NE_NAME,MCC,MNC,MSS,ELEMENT_IP,CGR_NET,CGR_SPC,LINKSET) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["NE_NAME"] + "','" + dt.Rows[0]["MCC"] + "','" + dt.Rows[0]["MNC"] + "','" + dt.Rows[0]["MSS"] + "','" + dt.Rows[0]["ELEMENT_IP"] + "','" + dt.Rows[0]["CGR_NET"] + "','" + MSS_SPC + "','" + link_name + "')";
                                        //ExecuteSQLQuery(ref conn, SqlStr);
                                    }

                                }

                                else
                                    // for mss
                                    SqlStr = "select distinct * from (select VENDOR,CIRCLE,NE_NAME,MCC,MNC,MSS,ELEMENT_IP,CGR_NET from  mss_mgw_map_" + _CircleName + "_" + group_name + " where CGR_NET='" + NET + "' and NE_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' and mcc<> '' and mnc <> '')A";
                                SQLQuery(ref conn, ref dt, SqlStr);
                                if (dt.Rows.Count >= 1)
                                {

                                    SqlStr = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,NE_NAME,MCC,MNC,MSS,ELEMENT_IP,CGR_NET,MSS_SPC,LINKSET) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["NE_NAME"] + "','" + dt.Rows[0]["MCC"] + "','" + dt.Rows[0]["MNC"] + "','" + dt.Rows[0]["MSS"] + "','" + dt.Rows[0]["ELEMENT_IP"] + "','" + dt.Rows[0]["CGR_NET"] + "','" + MSS_SPC + "','" + link_name + "')";
                                    //ExecuteSQLQuery(ref conn, SqlStr);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                    //if (!Directory.Exists(parsing_error_path))
                    //{
                    //    Directory.CreateDirectory(parsing_error_path);
                    //}

                    //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZNRI_MSS ");
                    //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                    //continue;
                }

            }

        }


        public void ParsefromTextFile(string txtData, string CircleName, string NE_MAP, string NE_Type, string CommandName)
        {
            try
            {
                switch (NE_MAP)
                {
                    case "MSS_LINKS":
                        NE_Type = "MSS_LINKS";
                        if (NE_Type == "MSS_LINKS")
                        {
                            if (CommandName == "ZQNI")
                            {
                                //Update_MSS_LINK_Details();
                                string[] GIGBtxt_ZQNI = Regex.Split(txtData, "\n");
                                //ParseData_ZQNI(txtData, CircleName, NE_Type);
                            }

                            if (CommandName == "ZNRI_NA0" || CommandName == "ZNRI_NA1")
                            {
                                string NA = string.Empty;
                                if (CommandName == "ZNRI_NA0")
                                {
                                    NA = "NA0";
                                }
                                else if (CommandName == "ZNRI_NA1")
                                {
                                    NA = "NA1";
                                }
                                ExecuteSQLQuery(ref conn, "delete FROM mss_link_" + CircleName + "_" + group_name + " where  CGR_SPC is null and GENERIC_NAME is null and CGR_NET is null and ET is null and LINK is null and LINKSET is null and SLC is null");
                                //ParseData_ZNRI_MSS(txtData, CircleName, NE_Type, NA);
                            }

                            if (CommandName == "ZNEL")
                            {
                                string[] GIGBtxt_ZNEL = Regex.Split(txtData, "\n");
                                //ParseData_ZNEL(txtData, CircleName, NE_Type);
                            }


                            if (CommandName == "ZNSI_NA0" || CommandName == "ZNSI_NA1")
                            {
                                string NA = string.Empty;
                                if (CommandName == "ZNSI_NA0")
                                {
                                    NA = "NA0";
                                }
                                else if (CommandName == "ZNSI_NA1")
                                {
                                    NA = "NA1";
                                }
                                //ParseData_ZNSI(txtData, CircleName, NE_Type, NA);
                            }


                        }

                        break;

                    case "MSS MAP":
                        if (NE_Type == "MSS")
                        {
                            if (CommandName == "ZWVI")
                            {
                                //ParseData_ZWVI(txtData.Trim(), CircleName, NE_Type);
                            }

                            if (CommandName == "ZQNI")
                            {
                                //ParseData_ZQNI(txtData, CircleName, NE_Type);
                            }

                            if (CommandName == "ZQRI")
                            {
                                ParseData_ZQRI(txtData, CircleName, NE_Type);
                            }

                            if (CommandName == "ZNRI_NA0" || CommandName == "ZNRI_NA1")
                            {
                                string NA = string.Empty;
                                if (CommandName == "ZNRI_NA0")
                                {
                                    NA = "NA0";
                                }
                                else if (CommandName == "ZNRI_NA1")
                                {
                                    NA = "NA1";
                                }
                                ParseData_ZNRI_MSS(txtData, CircleName, NE_Type, NA);
                            }

                            if (CommandName == "ZNSI_NA0" || CommandName == "ZNSI_NA1")
                            {
                                string NA = string.Empty;
                                if (CommandName == "ZNSI_NA0")
                                {
                                    NA = "NA0";
                                }
                                else if (CommandName == "ZNSI_NA1")
                                {
                                    NA = "NA1";
                                }
                                //ParseData_mss_ZNSI(txtData, CircleName, NE_Type, NA);
                            }


                            if (CommandName == "ZJGI")
                            {
                                //ParseData_ZJGI(txtData, CircleName, NE_Type);
                            }

                            if (CommandName == "ZRCI")
                            {
                                //ParseData_ZRCI_PRINT_5(txtData, CircleName, NE_Type);
                            }

                            if (CommandName == "ZEDO")
                            {
                                //ParseData_ZEDO(txtData, CircleName, NE_Type);
                            }

                            if (CommandName == "ZNEL")
                            {
                                //ParseData_mss_ZNEL(txtData, CircleName, NE_Type);
                            }
                        }

                        else if (NE_Type == "MGW_extra_et")
                        {
                            if (CommandName == "ZNSI_NA0" || CommandName == "ZNSI_NA1")
                            {
                                string NA = string.Empty;
                                if (CommandName == "ZNSI_NA0")
                                {
                                    NA = "NA0";
                                }
                                else if (CommandName == "ZNSI_NA1")
                                {
                                    NA = "NA1";
                                }
                                //MGW_EXTRA_ET_ZNSI(txtData, CircleName, "MGW", NA);
                            }

                            if (CommandName == "ZNRI_NA0")
                            {
                                string[] GIGBtxt_ZNRI_NA0 = Regex.Split(txtData, "\n");
                                //ZNRI_MGW_EXTRA_ET(GIGBtxt_ZNRI_NA0, CircleName, "MGW");
                            }

                            if (CommandName == "ZNRI_NA1")
                            {
                                string[] GIGBtxt_ZNRI_NA1 = Regex.Split(txtData, "\n");
                                //ZNRI_MGW_EXTRA_ET(GIGBtxt_ZNRI_NA1, CircleName, "MGW");
                            }

                            if (CommandName == "ZW7N")
                            {
                                string[] GIGBtxt_ZW7N = Regex.Split(txtData, "\n");
                                //ParseData_ZW7N_Extra_et(GIGBtxt_ZW7N, CircleName, "MGW");
                            }

                            if (CommandName == "ZUSI_IWS1E")
                            {
                                //ZUSI_IWS1E_EXTRA_ET(txtData, CircleName, "MGW");
                            }

                            if (CommandName == "ZUSI_IWSEP")
                            {
                                //ZUSI_IWS1E_EXTRA_ET(txtData, CircleName, "MGW");
                            }

                            if (CommandName == "ZUSI_NIWU")
                            {
                                //ZUSI_NIWU_EXTRA_ET(txtData, CircleName, "MGW");
                            }
                        }

                        else if (NE_Type == "MGW")
                        {
                            if (CommandName == "ZJVI")
                            {

                                //ParseData_ZJVI_1(txtData, CircleName, NE_Type);
                            }

                            if (CommandName == "ZW7N")
                            {
                                string[] GIGBtxt_ZW7N = Regex.Split(txtData, "\n");
                                //ParseData_ZW7N(GIGBtxt_ZW7N, CircleName, NE_Type);
                            }

                            if (CommandName == "ZNRI_NA0")
                            {

                                string[] GIGBtxt_ZNRI_NA0 = Regex.Split(txtData, "\n");

                                ParseData_ZNRI_MGW(GIGBtxt_ZNRI_NA0, CircleName, NE_Type, "NA0");

                            }

                            if (CommandName == "ZNRI_NA1")
                            {
                                string[] GIGBtxt_ZNRI_NA1 = Regex.Split(txtData, "\n");
                                ParseData_ZNRI_MGW(GIGBtxt_ZNRI_NA1, CircleName, NE_Type, "NA1");
                            }

                            if (CommandName == "ZJVI_2")
                            {
                                //ParseData_ZJVI_2(txtData, CircleName, NE_Type);
                            }

                            if (CommandName == "ZRCI")
                            {
                                //ParseData_ZRCI_PRINT_4(txtData, CircleName, NE_Type);
                            }
                            if (CommandName == "ZUSI_IWS1E")
                            {
                                //ParseData_ZUSI_IWS1E(txtData, CircleName, NE_Type);
                            }

                            if (CommandName == "ZUSI_IWSEP")
                            {
                                //ParseData_ZUSI_IWS1E(txtData, CircleName, NE_Type);
                            }

                            if (CommandName == "ZUSI_NIWU")
                            {
                                //ParseData_ZUSI_NIWU(txtData, CircleName, NE_Type);
                            }

                            if (CommandName == "ZR2O")
                            {
                                //ParseData_ZR2O(txtData, CircleName, NE_Type);
                            }

                            if (CommandName == "ZNEL")
                            {
                                //ParseData_ZNEL(txtData, CircleName, NE_Type);
                            }

                            if (CommandName == "ZNSI_NA0" || CommandName == "ZNSI_NA1")
                            {
                                string NA = string.Empty;
                                if (CommandName == "ZNSI_NA0")
                                {
                                    NA = "NA0";
                                }
                                else if (CommandName == "ZNSI_NA1")
                                {
                                    NA = "NA1";
                                }
                                //ParseData_ZNSI(txtData, CircleName, NE_Type, NA);
                            }

                        }

                        else if (NE_Type == "MGW_ATCA")
                        {

                            // For ATCA                         
                            if (CommandName == "VMGW_MGW")
                            {
                                //ParseData_VMGW_MGW(txtData, CircleName, NE_Type);
                            }
                            else if (CommandName == "TDM_ATER")
                            {
                                //ParseData_TDM_ATER(txtData, CircleName, NE_Type);
                            }
                            else if (CommandName == "LICENSE_TARGET")
                            {
                                //ParseData_LICENSE_TARGET(txtData, CircleName, NE_Type);
                            }
                            else if (CommandName == "SIGNALING_SS7_OWN")
                            {
                                //ParseData_SIGNALING_SS7_OWN(txtData, CircleName, NE_Type);
                            }
                            else if (CommandName == "TDMMGU")
                            {

                            }
                            else if (CommandName == "SIGNALING_SS7_LINK")
                            {
                                //ParseData_SIGNALING_SS7_LINK(txtData, CircleName, NE_Type);
                            }
                            else if (CommandName == "TDM_CIRCUITGROUP")
                            {
                                //ParseData_TDM_CIRCUITGROUP(txtData, CircleName, NE_Type);
                            }
                            // END ATCA COMMANDs
                        }

                        break;

                    case "MSC MAP":

                        if (NE_Type == "MSC_EXTRA_ET")
                        {
                            if (CommandName == "ZNSI_NA0" || CommandName == "ZNSI_NA1")
                            {
                                string NA = string.Empty;
                                if (CommandName == "ZNSI_NA0")
                                {
                                    NA = "NA0";
                                }
                                else if (CommandName == "ZNSI_NA1")
                                {
                                    NA = "NA1";
                                }
                                //ParseData_ZNSI_MSC_extra_et(txtData, CircleName, NE_Type, NA);
                            }
                        }

                        else
                        {
                            if (CommandName == "ZWVI")
                            {
                                //ParseData_ZWVI(txtData, CircleName, NE_Type);
                            }
                            if (CommandName == "ZQNI")
                            {
                                //ParseData_ZQNI(txtData, CircleName, NE_Type);
                            }
                            if (CommandName == "ZQRI")
                            {
                                //ParseData_ZQRI(txtData, CircleName, NE_Type);
                            }


                            if (CommandName == "ZNRI_NA0" || CommandName == "ZNRI_NA1")
                            {
                                string NA = string.Empty;
                                if (CommandName == "ZNRI_NA0")
                                {
                                    NA = "NA0";
                                }
                                else if (CommandName == "ZNRI_NA1")
                                {
                                    NA = "NA1";
                                }
                                //ParseData_ZNRI_MSS(txtData, CircleName, NE_Type, NA);
                            }

                            if (CommandName == "ZNSI_NA0" || CommandName == "ZNSI_NA1")
                            {
                                string NA = string.Empty;
                                if (CommandName == "ZNSI_NA0")
                                {
                                    NA = "NA0";
                                }
                                else if (CommandName == "ZNSI_NA1")
                                {
                                    NA = "NA1";
                                }
                                //ParseData_ZNSI(txtData, CircleName, NE_Type, NA);
                            }
                            if (CommandName == "ZNEL")
                            {
                                //ParseData_ZNEL(txtData, CircleName, NE_Type);
                            }
                            if (CommandName == "ZRCI")
                            {
                                //ParseData_MSC_ZRCI_PRINT_5(txtData, CircleName, NE_Type);
                            }
                            if (CommandName == "ZEDO")
                            {
                                //ParseData_ZEDO(txtData, CircleName, NE_Type);
                            }
                        }
                        break;


                    case "HLR MAP":

                        if (CommandName == "ZQNI")
                        {
                            //UpdateHLRDetails();
                            string[] GIGBtxt_ZQNI = Regex.Split(txtData, "\n");
                            //ParseData_ZQNI(txtData, CircleName, NE_Type);
                        }

                        if (CommandName == "ZNRI_NA0" || CommandName == "ZNRI_NA1")
                        {
                            string NA = string.Empty;
                            if (CommandName == "ZNRI_NA0")
                            {
                                NA = "NA0";
                            }
                            else if (CommandName == "ZNRI_NA1")
                            {
                                NA = "NA1";
                            }
                            ExecuteSQLQuery(ref conn, "delete FROM hlr_" + CircleName + " where  CGR_SPC is null and GENERIC_NAME is null and CGR_NET is null and ET is null and LINK is null and LINKSET is null and SLC is null");
                            //ParseData_ZNRI_MSS(txtData, CircleName, NE_Type, NA);
                        }

                        if (CommandName == "ZNEL")
                        {
                            string[] GIGBtxt_ZNEL = Regex.Split(txtData, "\n");
                            //ParseData_ZNEL(txtData, CircleName, NE_Type);
                        }


                        if (CommandName == "ZNSI_NA0" || CommandName == "ZNSI_NA1")
                        {
                            string NA = string.Empty;
                            if (CommandName == "ZNSI_NA0")
                            {
                                NA = "NA0";
                            }
                            else if (CommandName == "ZNSI_NA1")
                            {
                                NA = "NA1";
                            }
                            //ParseData_ZNSI(txtData, CircleName, NE_Type, NA);
                        }


                        break;


                    case "DESTINATION INFO":
                        if (_NEType == "MSS" || _NEType == "MSC")
                        {
                            if (CommandName == "ZRRI")
                            {
                                //parse_ZRRI(txtData, CircleName);
                            }

                            if (CommandName == "ZRNI")
                            {
                                //parse_ZRNI(txtData, CircleName);
                            }

                            if (CommandName == "ZRIL")
                            {
                                //parse_ZRIL(txtData, CircleName);
                            }
                        }


                        break;


                    case "ASSOCIATION INFO":
                        if (_NEType == "MSS")
                        {
                            if (CommandName == "ZNSI_NA0" || CommandName == "ZNSI_NA1")
                            {
                                string NA = string.Empty;
                                if (CommandName == "ZNSI_NA0")
                                {
                                    NA = "NA0";
                                }
                                else if (CommandName == "ZNSI_NA1")
                                {
                                    NA = "NA1";
                                }
                                //association_ZNSI_NA1(txtData, CircleName, NA);
                            }
                            if (CommandName == "ZOYI")
                            {
                                //association_ZOYI(txtData, CircleName);
                                ExecuteSQLQuery(ref conn, "delete from association_info_" + CircleName + " where circle = '" + CircleName + "' and node = '" + _ne_name + "' and `SOURCE_IP_1` IS NULL ");
                            }

                            if (CommandName == "ZRCI")
                            {
                                //ASSOCIATION_ZRCI(txtData, CircleName);
                            }
                        }

                        else if (_NEType == "MGW")
                        {
                            if (CommandName == "ZNSI_NA0" || CommandName == "ZNSI_NA1")
                            {
                                string NA = string.Empty;
                                if (CommandName == "ZNSI_NA0")
                                {
                                    NA = "NA0";
                                }
                                else if (CommandName == "ZNSI_NA1")
                                {
                                    NA = "NA1";
                                }
                                //association_ZNSI_NA1(txtData, CircleName, NA);
                            }

                            else if (CommandName == "ZRCI")
                            {
                                string[] GIGBtxt_ZRCI_PRINT_4 = Regex.Split(txtData, "\n");
                                //ASSOCIATION_ZRCI(txtData, CircleName);
                            }

                            else if (CommandName == "ZOYI")
                            {
                                //association_ZOYI(txtData, CircleName);
                                ExecuteSQLQuery(ref conn, "delete from association_info_" + CircleName + " where circle = '" + CircleName + "' and node = '" + _ne_name + "' and `SOURCE_IP_1` IS NULL ");
                            }
                        }
                        break;

                }
            }
            catch (Exception ex)
            {
                //File.WriteAllText(@"E:\CS_DCM\DCM_RAW\" + @"PARSING_ERROR\" + DateTime.Now.ToString("dd_MM_yyyy") + @"\ParsingError.txt", "Circle : " + _CircleName + " Element : " + _ne_name.Trim() + " Type : " + NE_Type + "\r\n" + ex + "\r\n");
            }
        }
        public void ParseData_ZQRI(string output, string CircleName, string NE_Type)
        {
            string sqlstr = string.Empty;
            string element_ip = string.Empty;
            string[] temp1;
            string required_output = string.Empty;
            string value = string.Empty;
            try
            {
                string data = output.Remove(0, output.IndexOf("-------"));
                string[] temp = Regex.Split(data, "\r\n");

                for (int i = 0; i <= temp.Length - 1; i++)
                {
                    try
                    {

                        value = Regex.Replace(temp[i].Trim(), " {2,}", "~");
                        temp1 = Regex.Split(value, "~");

                        for (int j = 0; j <= temp1.Length - 1; j++)
                        {
                            if (temp1[j].Trim() == "L")
                            {
                                if (j == 0)
                                {
                                    element_ip = temp1[j + 1];   // for atca mss
                                }
                                else
                                {
                                    element_ip = temp1[j - 1];
                                }

                                break;
                            }

                        }

                        if (element_ip != "")
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                        //if (!Directory.Exists(parsing_error_path))
                        //{
                        //    Directory.CreateDirectory(parsing_error_path);
                        //}

                        //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZQRI ");
                        //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        //ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                        //oExceptionLog.WriteExceptionErrorToFile("ParseData_ZQRI_SUBwithConinue()", ErrorMsg, "", ref FileError);

                        //ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'","") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                        //ExecuteSQLQuery(ref conn, ErrorQuery);
                        continue;
                    }


                }

                if (_NEType == "MSC")
                {
                    sqlstr = "Update msc_" + CircleName + " Set ELEMENT_IP='" + element_ip.Trim() + "' Where NE_NAME='" + _ne_name.Trim() + "' and NODE_Type='" + NE_Type + "' and Circle='" + CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "'";
                    ExecuteSQLQuery(ref conn, sqlstr);
                }
                else
                {
                    sqlstr = "Update mss_mgw_map_" + CircleName + "_" + group_name + "  Set ELEMENT_IP='" + element_ip.Trim() + "' Where NE_NAME='" + _ne_name.Trim() + "' and NODE_Type='" + NE_Type + "' and Circle='" + CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "'";
                    ExecuteSQLQuery(ref conn, sqlstr);
                }
            }
            catch (Exception ex)
            {
                //ErrorMsg = "@" + System.DateTime.Now.ToString() + "[" + _ne_name + "]: " + ex.Message.ToString();
                //oExceptionLog.WriteExceptionErrorToFile("ParseData_ZQRI()", ErrorMsg, "", ref FileError);

                //ErrorQuery = "update dcm_node_failure_details1 set ErrorMsg = '" + ErrorMsg.Replace("'","") + "' where Circle = '" + _CircleName + "' and Element='" + _ne_name + "'";
                //ExecuteSQLQuery(ref conn, ErrorQuery);
            }
        }
        public void ParseData_ZNRI_MGW(string[] GIGBtxt, string _CircleName, string NE_Type, string NET)
        {
            string[] temp;
            string[] temp1;
            string SqlStr = string.Empty;
            string MGW_spc = string.Empty;
            DataTable dt = new DataTable();

            for (int i = 0; i < GIGBtxt.Length; i++)
            {
                try
                {
                    if (GIGBtxt[i].ToString().Trim().Contains("SP CODE H/D") && GIGBtxt[i].ToString().Trim().Contains("NET"))
                    {
                        i = i + 2;

                        if (GIGBtxt[i].Contains(NET) && !GIGBtxt[i].Contains("OWN SP"))
                        {

                            temp = GIGBtxt[i].Trim().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            MGW_spc = temp[1].Trim().ToString();

                            SqlStr = "select distinct * from (select VENDOR,CIRCLE,MGW_NAME,NE_NAME,MCC,MNC,MSS,ELEMENT_IP,CGR_NET,MGW_C_NO from  mss_mgw_map_" + _CircleName + "_" + group_name + " where CGR_NET='" + NET + "' and MGW_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "' and mcc<> '' and mnc <> '')A";
                            SQLQuery(ref conn, ref dt, SqlStr);
                            if (dt.Rows.Count >= 1)
                            {

                                SqlStr = "insert into mss_mgw_map_" + _CircleName + "_" + group_name + " (VENDOR,CIRCLE,MGW_NAME,NE_NAME,MCC,MNC,MSS,ELEMENT_IP,CGR_NET,MGW_C_NO,MGW_SPC) values ('" + dt.Rows[0]["VENDOR"] + "','" + dt.Rows[0]["CIRCLE"] + "','" + dt.Rows[0]["MGW_NAME"] + "','" + dt.Rows[0]["NE_NAME"] + "','" + dt.Rows[0]["MCC"] + "','" + dt.Rows[0]["MNC"] + "','" + dt.Rows[0]["MSS"] + "','" + dt.Rows[0]["ELEMENT_IP"] + "','" + dt.Rows[0]["CGR_NET"] + "','" + dt.Rows[0]["MGW_C_NO"] + "','" + MGW_spc + "')";
                                ExecuteSQLQuery(ref conn, SqlStr);
                            }
                        }

                        if (GIGBtxt[i].Contains(NET) && GIGBtxt[i].Contains("OWN SP"))
                        {
                            temp1 = GIGBtxt[i].Trim().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            MGW_spc = temp1[1].Trim().ToString();

                            if (MGW_spc != "")
                            {
                                SqlStr = "Update mss_mgw_map_" + _CircleName + "_" + group_name + " set MGW_SPC='" + MGW_spc.Trim() + "' Where CGR_NET='" + NET + "' and MGW_Name='" + _ne_name.Trim() + "'  and Circle='" + _CircleName.Trim() + "' and Vendor='" + _vendor.Trim() + "'";
                                ExecuteSQLQuery(ref conn, SqlStr);
                            }

                            MGW_spc = "";

                        }
                    }

                }
                catch (Exception ex)
                {
                    //string parsing_error_path = @errorFolderPath + DateTime.Now.ToString("dd_MM_yyyy") + @"\" + _CircleName + @"\" + _ne_name + @"\" + NE_Type + @"\";
                    //if (!Directory.Exists(parsing_error_path))
                    //{
                    //    Directory.CreateDirectory(parsing_error_path);
                    //}

                    //File.AppendAllText(parsing_error_path + "parsingError.txt", " error while parsing in function ParseData_ZNRI_MGW ");
                    //File.AppendAllText(parsing_error_path + "parsingError.txt", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                    continue;
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {

            clsTransferToNSSFromCS.DCMDataMigration(System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString().ToUpper());
            //clsCSParsingProcess ook = new clsCSParsingProcess();
            //ook._CircleName = "ori";
            //ook.group_name = "group1";
            //ook.Inser_data_into_final_table("ori_group1");
            //clsCSParsingProcess objParse1 = new clsCSParsingProcess();
            //objParse1.group_name = "HLR_MSC";
            //objParse1.NeMap = new string[4];
            //objParse1.NeMap[0] = "MSC MAP";
            //objParse1.NeMap[1] = "HLR MAP";
            //objParse1.NeMap[2] = "ASSOCIATION INFO";
            //objParse1.NeMap[3] = "DESTINATION INFO";
            //objParse1.StartParsing();

            clsCSGetRawProcess obj = new clsCSGetRawProcess();
            obj.nodeTypeMain = new string[3];
            //obj.nodeTypeMain[0] = "MSS";
            //obj.nodeTypeMain[1] = "GCS";
            obj.nodeTypeMain[0] = "MGW";
            obj.groupName = "group1";
            obj.StartRawGeneration();

            clsCSGetRawProcess obj1 = new clsCSGetRawProcess();

            obj1.nodeTypeMain = new string[3];
            obj1.nodeTypeMain[0] = "MSS";
            obj1.nodeTypeMain[1] = "GCS";
            obj1.nodeTypeMain[2] = "MGW";
            obj1.groupName = "group2";
            obj1.StartRawGeneration();

            clsCSGetRawProcess obj3 = new clsCSGetRawProcess();

            obj3.nodeTypeMain = new string[3];
            obj3.nodeTypeMain[0] = "MSS";
            obj3.nodeTypeMain[1] = "GCS";
            obj3.nodeTypeMain[2] = "MGW";
            obj3.groupName = "group3";
            obj3.StartRawGeneration();



            //clsCSParsingProcess objParse1 = new clsCSParsingProcess();
            //objParse1.group_name = "group3";
            //objParse1.NeMap = new string[1];
            //objParse1.NeMap[0] = "MSS MAP";
            //objParse1.StartParsing();
        }


        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void CS_timer_Tick(object sender, EventArgs e)
        {
            clsCSprocess objCSprocess = new clsCSprocess();
            String Circle = String.Empty;
            String Vendor = String.Empty;
            Circle = System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString().ToUpper();
            Vendor = System.Configuration.ConfigurationManager.AppSettings["Vendor"].ToString().ToUpper();
            try
            {
                CS_timer.Enabled = false;
                if (System.DateTime.Now.ToString("HH:mm") == System.Configuration.ConfigurationManager.AppSettings["CSStartTime"].ToString())
                {

                    // For Starting Raw Generation
                    objCSprocess.StartMain();


                    //ExecuteNonQuery("Update tblProcessUpdate Set EndTime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',Status='COMPLETED' Where DCMProcess='CS' and Circle='" + Circle + "' and Vendor='" + Vendor + "'");


                    //}
                    //    }

                    //}
                }


            }
            catch (Exception ex)
            {

                ExecuteSQLQuery(ref conn, "Update tblProcessUpdate Set EndTime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',Status='NOT COMPLETED',ErrorMsg='" + ex.Message.ToString() + "' Where DCMProcess='CS' and Circle='" + Circle + "'  and Vendor='" + Vendor + "'");
            }
            finally
            {
                CS_timer.Enabled = true;
            }
        }
        private void tmGroup1_Tick(object sender, EventArgs e)
        {
            try
            {

                OnTimedEventGroup2.Enabled = false;
                ////string date1 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                ////int d1 = Int32.Parse(date1.Substring(11, 2));

                String Circle = String.Empty;
                String Vendor = String.Empty;
                String Group = String.Empty;
                Circle = System.Configuration.ConfigurationManager.AppSettings["CircleName"].ToString().ToLower();
                Vendor = System.Configuration.ConfigurationManager.AppSettings["Vendor"].ToString().ToUpper();
                Group = Circle + "_group" + 1;
                ////int startTime = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["ParsingStartTime"]);
                ////int rangeTime = startTime + 4;
                bool isProcessStart = false;
                if (System.DateTime.Now.ToString("HH:mm") == System.Configuration.ConfigurationManager.AppSettings["ParsingStartTime"].ToString())
                {
                    if (isProcessStart == false)
                    {
                        DataTable dt = ExecuteSQL("Select * from parsing_completion_details Where circle='" + Group + "' and Status='NOT COMPLETED' and START_TIME='0000-00-00 00:00:00' ");
                        if (dt != null)
                        {
                            if (dt.Rows.Count > 0)
                            {
                                dt = null;
                                dt = ExecuteSQL("Select * from raw_generation_details Where circle='" + Group + "' and Status='NOT COMPLETED'");
                                if (dt != null)
                                {
                                    if (dt.Rows.Count == 0)
                                    {
                                        //if (dt.Rows[0]["Status"].ToString() == "COMPLETED")
                                        //{
                                        isProcessStart = true;
                                        clsCSParsingProcess objParse1 = new clsCSParsingProcess();

                                        objParse1.group_name = "group1";
                                        objParse1.NeMap = new string[1];
                                        objParse1.NeMap[0] = "MSS MAP";
                                        objParse1.StartParsing();


                                        objParse1 = null;

                                        isProcessStart = false;
                                        //}
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }
            finally
            {
                OnTimedEventGroup2.Enabled = true;
            }


        }
        String Constring = System.Configuration.ConfigurationSettings.AppSettings["CSDBConstring"].ToString();
        private System.Data.DataTable ExecuteSQL(String Query)
        {
            MySqlDataAdapter da = null;
            System.Data.DataTable Dt = null;
            try
            {
                if (Dt == null)
                {
                    Dt = new DataTable();
                }
                da = new MySqlDataAdapter(Query, Constring);
                da.Fill(Dt);
                return Dt;
            }
            catch (Exception ex)
            {
                //throw ex;
                return null;
            }
        }

        private Int32 ExecuteNonQuery(String Query)
        {
            MySqlConnection Conn = null;
            MySqlCommand Command = null;
            int result = 0;
            try
            {
                Conn = new MySqlConnection(Constring);
                // Command.CommandTimeout = 20000;
                Command = new MySqlCommand(Query, Conn);
                Command.CommandTimeout = 20000;
                Conn.Open();
                result = Command.ExecuteNonQuery();
                Conn.Close();
                return result;
            }
            catch (Exception ex)
            {
                //throw ex;
                return result;
            }
            finally
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                Command.Dispose();
                Conn.Dispose();
                if (Conn != null)
                {
                    Conn = null;
                }

                if (Command != null)
                {
                    Command = null;
                }

            }
        }

        private void CS_timer_Tick_1(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            //clsCSParsingProcess objParse1 = new clsCSParsingProcess();
            //objParse1.group_name = "group2";
            //objParse1.NeMap = new string[1];
            //objParse1.NeMap[0] = "MSS MAP";
            //objParse1.StartParsing();

            //clsCSParsingProcess objParse2 = new clsCSParsingProcess();
            //objParse2.group_name = "group1";
            //objParse2.NeMap = new string[1];
            //objParse2.NeMap[0] = "HLR MAP";
            //objParse2.StartParsing();

            clsCSParsingProcess objParse3 = new clsCSParsingProcess();
            objParse3.group_name = "group2";
            objParse3.NeMap = new string[1];
            //objParse3.NeMap[0] = "MSS MAP";
            objParse3.NeMap[0] = "ASSOCIATION INFO";
            objParse3.StartParsing();

            //clsCSGetRawProcess obj = new clsCSGetRawProcess();
            //obj.nodeTypeMain = new string[3];
            //obj.nodeTypeMain[0] = "MSS";
            //obj.nodeTypeMain[1] = "GCS";
            //obj.nodeTypeMain[2] = "MGW";
            //obj.groupName = "group1";
            //obj.StartRawGeneration();
        }

        private void btnSqlUploadCS_Click(object sender, EventArgs e)
        {

            clsTransferToNSSFromCS obj = new clsTransferToNSSFromCS();  

           
        }
    }

}

