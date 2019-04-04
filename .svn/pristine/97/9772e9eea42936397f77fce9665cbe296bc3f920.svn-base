Imports System.Threading
Imports MySql.Data.MySqlClient
Imports NSN.GNOCN.Tools.Logs
Imports NSN.GNOCN.Tools.SQL.DbLayer
Imports System.Text.RegularExpressions
Imports System.IO
Imports ScriptingSSH

Namespace NSN.GNOCN.Tools.AWE.NSS


    Public Class clsPSProcess

        Dim ParsingLogSGSN As String = "D:\DCM\PACO_DCM\SGSN\" + System.DateTime.Now.ToString("dd-MM-yyyy") + "\"
        Dim ParsingLogGGSN As String = "D:\DCM\PACO_DCM\GGSN\" + System.DateTime.Now.ToString("dd-MM-yyyy") + "\"
        Dim ParsingLogCMD As String = "D:\DCM\PACO_DCM\CMD\" + System.DateTime.Now.ToString("dd-MM-yyyy") + "\"
        Private objSourcedb As Global.NSN.GNOCN.Tools.SQL.DbLayer.DbConnectLayer
        Private objOracleDb As Global.NSN.GNOCN.Tools.SQL.DbLayer.DbConnectLayer
        Private oExceptionLog As clsExceptionLogs
        Private oFileLog As clsFileLogs
        Private ErrorMessageString As String = String.Empty
        Private _client As String = String.Empty
        Private _CircleName As String = String.Empty
        Private _OSSName As String = String.Empty
        Private _vendor As String = String.Empty
        Private _NEType As String = String.Empty
        Private _ThreadID As String = String.Empty
        Private _conString As String = String.Empty
        Private _ossConstring As String = String.Empty
        Private connectionstringg As String = String.Empty
        Private _routerName As String = String.Empty

        Private ChargeingProfile_list As ArrayList = Nothing
        Private priXMLfilelist As ArrayList = Nothing
        Private secXMLfilelist As ArrayList = Nothing
        Private ipandEC As ArrayList = Nothing
        Private APNList As ArrayList = Nothing
        Private TelNetSession As ScriptingSSH.ScriptingSSH
        Private LOG As String = String.Empty
        Dim ErrMsg As String

        Public Property RouterName As String
            Get
                Return _routerName
            End Get
            Set(ByVal value As String)
                _routerName = value
            End Set
        End Property
        Public Enum NERouterConnectionStatus
            Inprogress
            Failed
            Success
        End Enum
        Public Property Client As String
            Get
                Return _client
            End Get
            Set(ByVal value As String)
                _client = value
            End Set
        End Property
        Public Property OSSConString As String
            Get
                Return _ossConstring
            End Get
            Set(ByVal value As String)
                _ossConstring = value
            End Set
        End Property
        Public Property ConString As String
            Get
                Return _conString
            End Get
            Set(ByVal value As String)
                _conString = value
            End Set
        End Property
        Public Property ThreadID As String
            Get
                Return _ThreadID
            End Get
            Set(ByVal value As String)
                _ThreadID = value
            End Set
        End Property
        Public Property CircleName As String
            Get
                Return _CircleName
            End Get
            Set(ByVal value As String)
                _CircleName = value
            End Set
        End Property
        Public Property OSSName As String
            Get
                Return _OSSName
            End Get
            Set(ByVal value As String)
                _OSSName = value
            End Set
        End Property
        Public Property Vendor As String
            Get
                Return _vendor
            End Get
            Set(ByVal value As String)
                _vendor = value
            End Set
        End Property
        Public Property NE_Type As String
            Get
                Return _NEType
            End Get
            Set(ByVal value As String)
                _NEType = value
            End Set
        End Property
        Sub New()
            Try

            Catch ex As Exception

            End Try
        End Sub
        Sub New(ByVal ConnectionString)
            'Dim constr As String = String.Empty
            'Dim constr1 As String = String.Empty
            'constr = System.Configuration.ConfigurationManager.AppSettings("Conn").ToString()
            'constr = System.Configuration.ConfigurationManager.AppSettings("OSSCon").ToString()
            objSourcedb = New Global.NSN.GNOCN.Tools.SQL.DbLayer.DbConnectLayer(ConnectionString, DbType.MySqlType)

            'objOracleDb = New NSN.GNOCN.Tools.SQL.DbLayer.DbConnectLayer(constr1, DbType.OracleType)
            oExceptionLog = New clsExceptionLogs(_CircleName)
            oFileLog = New clsFileLogs(_CircleName)
            connectionstringg = ConnectionString
        End Sub
        ''' <summary>
        ''' StartMain function is the entry point for fetching data.
        ''' </summary>
        ''' <remarks></remarks>


        Public Function AutoUpdateNSEIMapping()
            Try
                If _vendor.Trim = "" Or _CircleName.Trim = "" Then
                    'Throw New Exception("Inalide Vendor / Circle ")
                    Exit Function
                End If
                AutoUpdateNSEIMapping(_vendor, _CircleName, _OSSName, _NEType, _routerName)
            Catch ex As Exception
                'Call oExceptionLog.WriteExceptionErrorToFile("AutoUpdateNSEIMapping()", ex.Message, "Error while processing circle: " & _CircleName, ex.Message)
            End Try
        End Function

        Public Sub AutoUpdateNSEIMapping(ByVal strVendor As String, ByVal strGCirle As String, ByVal strCircle As String, ByVal strNEType As String, ByVal _routerName As String)
            _vendor = strVendor
            _CircleName = strGCirle
            _OSSName = strCircle
            _NEType = strNEType
            Try

                FetchData(_CircleName, _OSSName, _routerName)

            Catch ex As Exception
                'Call oExceptionLog.WriteExceptionErrorToFile("AutoUpdateNSEIMapping2()", ex.Message, "Error while processing circle: " & _CircleName, ex.Message)
            Finally

            End Try
        End Sub

        Private Function FetchData(ByVal strCircle As String, ByVal strOSS As String, ByVal routerName As String) As String
            Try
                Dim Dt As DataTable = Nothing
                Dim RtrLoginCtr As Integer = 0
                Dim routercount As Integer = 0
                Dim CurrentDateTime As String = String.Empty
                Dim errorlog As String = String.Empty
                Dim RouterLoginError As String = String.Empty
                Dim ErrorMsg As String = String.Empty
                Dim AvoidableNode As String = String.Empty
                Dim IsRouterLoginSuccessfull As Boolean = False
                CurrentDateTime = Date.Now.ToString("yyyy-MM-dd HH:mm:ss")
                'Login to Router for specified Client and circle

                Dt = objSourcedb.ExecuteSQL("SELECT Router_Id, Rtr_Name, Rtr_IP, Rtr_Port, Rtr_UserName, Rtr_Password, Rtr_Prompt, Rtr_ActiveFlag, Circle FROM Router_Master where Node_Type='" & _NEType & "'  and vendor='" & Trim(_vendor) & "' and Circle ='" & Trim(_CircleName) & "' and  Rtr_Name='" & routerName.ToString() & "' and Rtr_ActiveFlag is True")

                If (Not Dt Is Nothing AndAlso Dt.Rows.Count > 0) Then

                    If (_NEType = "CMD") Or (_NEType = "GGSN") Or (_NEType = "SGSN") Then
                        Try

                            If GetServer(TelNetSession, Dt(0)("Rtr_IP").ToString(), Dt(0)("Rtr_UserName").ToString(), Dt(0)("Rtr_Password").ToString(), Dt(0)("Rtr_Port").ToString(), 60) = True Then

                                EntereNETOCollectData(TelNetSession, strCircle, routerName)

                                TelNetSession.Disconnect()
                                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.router_master set  REMARKS='Disconnect' where Rtr_IP='" + Dt(0)("Rtr_IP").ToString() + "' and domain='PS' and node_type='" + _NEType + "' and Rtr_Name='" + _routerName + "' and circle='" + _CircleName + "' and Rtr_ActiveFlag is true")
                            End If

                        Catch ex As Exception
                            'throw
                        Finally
                            TelNetSession.Disconnect()
                        End Try

                    End If

                End If

                Return Nothing
            Catch ex As Exception


                'Throw New Exception("Error in FetchData method " & ex.Message)
                Return Nothing
            End Try
        End Function


        ' Change by arvind on the basis of manish code at 08042014            *************************Change Start**************

        Public Function GetServer(ByRef SessioName As ScriptingSSH.ScriptingSSH, ByVal IP As String, ByVal UserName As String, ByVal password As String, ByVal port As Integer, ByVal TimeOut As Integer) As Boolean
            Try
                Dim Attempt As Integer = 0
Retry_Router:   SessioName = New ScriptingSSH.ScriptingSSH(IP, port, UserName, password, TimeOut)
                If (SessioName.Connect() = True) Then
                    objSourcedb.ExecuteNonQuery("update awe_paco_dcm.router_master set  Status='Success' where Rtr_IP='" + IP.ToString + "' and domain='PS' and node_type='" + _NEType + "' and Rtr_Name='" + _routerName + "' and circle='" + _CircleName + "' and Rtr_ActiveFlag is true")
                    Return True
                Else
                    If (Attempt <= 5) Then
                        System.Threading.Thread.Sleep(60 * 5 * 1000)
                        Attempt += 1
                        Dim RouterPath As String = "D:\DCM\PACO_DCM\" + NE_Type + "\" + System.DateTime.Now.ToString("dd-MM-yyyy") + "\" + "ROUTER FAILED\"
                        If (Not (Directory.Exists(RouterPath))) Then
                            Directory.CreateDirectory(RouterPath)
                        End If
                        File.AppendAllText(RouterPath + _CircleName + ".txt", " ROUTER LOGIN FAILLED " + IP + "   " + System.DateTime.Now.ToString() + Environment.NewLine)
                        GoTo Retry_Router
                    End If
                    objSourcedb.ExecuteNonQuery("update awe_paco_dcm.router_master set  Status='Not Success',ERRORMSG='ROUTER LOGIN FAILLED AFTER 5 ATTEMPTED' where Rtr_IP='" + IP.ToString + "' and domain='PS' and node_type='" + _NEType + "' and Rtr_Name='" + _routerName + "' and circle='" + _CircleName + "' and Rtr_ActiveFlag is true")
                    'REMARKS='NOT OK'
                    BackupUpload(IP.ToString)
                    Return False
                End If


            Catch ex As Exception
                Dim RouterPath As String = "D:\DCM\PACO_DCM\" + NE_Type + "\" + System.DateTime.Now.ToString("dd-MM-yyyy") + "\" + "ROUTER FAILED\"
                If (Not (Directory.Exists(RouterPath))) Then
                    Directory.CreateDirectory(RouterPath)
                End If
                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.router_master set  Status='Not Success',ERRORMSG='Router is not login',REMARKS='CRITICAL PROBLEM' where Rtr_IP='" + IP.ToString + "' and domain='PS' and node_type='" + _NEType + "' and Rtr_Name='" + _routerName + "' and circle='" + _CircleName + "' and Rtr_ActiveFlag is true")
                File.AppendAllText(RouterPath + _CircleName + ".txt", " ROUTER LOGIN Problem.... " + IP + "   " + ex.Message + "   " + System.DateTime.Now.ToString() + Environment.NewLine)

            End Try

        End Function
        Public Function BackupUpload(ByVal IP As String)
            Dim dt_circle As DataTable = Nothing
            Dim Dt_Element As DataTable = Nothing
            Dim NE_Element As String = String.Empty
            Dim NEIP As String = String.Empty
            Dim NodeType As String = String.Empty
            'Status='Not Success' and ERRORMSG='ROUTER LOGIN FAILLED AFTER 5 ATTEMPTED' and
            dt_circle = objSourcedb.ExecuteSQL("select * from awe_paco_dcm.router_master where   Rtr_IP='" + IP + "' and domain='PS' and node_type='" + _NEType + "' and Rtr_Name='" + _routerName + "' and circle='" + _CircleName + "' and Rtr_ActiveFlag is true") 'REMARKS='NOT OK'
            If (dt_circle.Rows.Count > 0) Then
                If _NEType = "SGSN" Then
                    Dt_Element = objSourcedb.ExecuteSQL("Select NE_NetAct_Name, NE_Name, NE_IP, NE_UserName, NE_Password,Convert(fetchedTime,Char(50)) as fetchedTime,DelayMinute,LoginCriteria,Node_Type,ProtoCol,NE_Prompt  FROM ne_master Where vendor='" & Trim(_vendor) & "' and Circle ='" & Trim(_CircleName) & "' and OSS ='" & Trim(_OSSName) & "' and Node_Type='" & _NEType & "' and Rtr_Name='" & RouterName & "'")
                ElseIf _NEType = "GGSN" Then
                    Dt_Element = objSourcedb.ExecuteSQL("Select NE_NetAct_Name, NE_Name, NE_IP, NE_UserName, NE_Password,Convert(fetchedTime,Char(50)) as fetchedTime,DelayMinute,LoginCriteria,Node_Type,ProtoCol,NE_Prompt  FROM ne_master Where vendor='" & Trim(_vendor) & "' and Circle ='" & Trim(_CircleName) & "' and OSS ='" & Trim(_OSSName) & "' and Node_Type='" & _NEType & "' and Rtr_Name='" & RouterName & "'")
                ElseIf _NEType = "CMD" Then
                    Dt_Element = objSourcedb.ExecuteSQL("Select NE_NetAct_Name, NE_Name, NE_IP, NE_UserName, NE_Password,Convert(fetchedTime,Char(50)) as fetchedTime,DelayMinute,LoginCriteria,Node_Type,ProtoCol,NE_Prompt  FROM ne_master Where vendor='" & Trim(_vendor) & "' and Circle ='" & Trim(_CircleName) & "' and OSS ='" & Trim(_OSSName) & "' and Node_Type='" & _NEType & "' and Rtr_Name='" & RouterName & "'")
                End If

                If (Not Dt_Element Is Nothing AndAlso Dt_Element.Rows.Count > 0) Then
                    For t As Integer = 0 To Dt_Element.Rows.Count - 1
                        Try

                            NE_Element = Trim(Dt_Element.Rows(t)("NE_Name").ToString())
                            NEIP = Trim(Dt_Element.Rows(t)("NE_IP").ToString())
                            NodeType = Trim(Dt_Element.Rows(t)("Node_Type").ToString())

                            If (_NEType = "SGSN") Then
                                'Delete data in current table's if data is exists.
                                objSourcedb.ExecuteNonQuery("delete from awe_paco_dcm.tblsgsn_nsvci where circle='" + _CircleName + "' and ne_name='" + NE_Element + "'")
                                objSourcedb.ExecuteNonQuery("delete from awe_paco_dcm.tblcoremap_up_apndata where circle='" + _CircleName + "' and ne_name='" + NE_Element + "'")
                                'Insert data in current table's to backup table
                                objSourcedb.ExecuteNonQuery("INSERT INTO awe_paco_dcm.tblsgsn_nsvci SELECT VENDOR, CIRCLE, '', NE_NAME, '', MCC, MNC, LAC, RAC, CI, COMP_UNIT, PAPU_IP, NSEI, NSVC_ID, NSVC_NAME, OP_STATE, DLCI_UDP_PORT, CIR_RDW, BEARER_ID_RSW, BEARER_NAME_RPNBR, PCM_PCU_IP, TS_1, TS_2 FROM awe_paco_dcm.tblsgsn_nsvci_backup  where circle='" + _CircleName + "' and ne_name='" + NE_Element + "'")
                                objSourcedb.ExecuteNonQuery("INSERT INTO awe_paco_dcm.tblcoremap_up_apndata SELECT * FROM awe_paco_dcm.tblcoremap_up_apndata_backup  where circle='" + _CircleName + "' and ne_name='" + NE_Element + "'")
                                'update status
                                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set REMARKS='SGSN Data Uploaded' where Circle='" + _CircleName + "' and NE_Name='" + NE_Element + "' and NE_IP='" + NEIP + "' and Node_Type='" + NodeType + "' and Domain='PS' and Rtr_Name='" + RouterName + "' and NE_ActiveFlag is true")
                            ElseIf (_NEType = "GGSN") Then
                                'Delete data in current table's if data is exists.
                                objSourcedb.ExecuteNonQuery("delete FROM awe_paco_dcm.tblcoremap_update where GGSN_NAME='" + NE_Element + "'")
                                'Insert data in current table's to backup table
                                objSourcedb.ExecuteNonQuery("insert into awe_paco_dcm.tblcoremap_update SELECT VENDOR, CIRCLE, OSS, NE_NAME, COMP_UNIT, PAPU_IP, PRIMARY_DNS, SECONDARY_DNS, APN, GGSN_SERVER_GW_IP, GGSN_BLADE, GGSN_NAME, IMSI_MISSION, PRI_DIAMETER, PRI_SERVER_IP, '', SEC_DIAMETER, SEC_SERVER_IP, '', '', '', '', '' FROM awe_paco_dcm.tblcoremap_update_backup where GGSN_NAME='" + NE_Element + "'")
                                'update status
                                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set REMARKS='GGSN Data Uploaded' where Circle='" + _CircleName + "' and NE_Name='" + NE_Element + "' and NE_IP='" + NEIP + "' and Node_Type='" + NodeType + "' and Domain='PS' and Rtr_Name='" + RouterName + "' and NE_ActiveFlag is true")
                            ElseIf (_NEType = "CMD") Then
                                Dim dt_pri As DataTable = Nothing
                                Dim dt_sec As DataTable = Nothing
                                Dim SERVER_IP As String = String.Empty
                                Dim EC As String = String.Empty
                                Dim IN_NAME As String = String.Empty
                                Dim IN_IP As String = String.Empty

                                If _CircleName = "KOL" Then
                                    dt_pri = objSourcedb.ExecuteSQL("select  distinct PRI_SERVER_IP,EC_DETAIL,PRIMARY_IN_NAME,PRIMARY_IN_IP from awe_paco_dcm.tblcoremap_update_backup where CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG') and EC_DETAIL LIKE '" + NE_Element + "%' and PRI_SERVER_IP <> ''")
                                    dt_sec = objSourcedb.ExecuteSQL("select  distinct SEC_SERVER_IP,SEC_EC,SECONDARY_IN_NAME,SECONDARY_IN_IP from awe_paco_dcm.tblcoremap_update_backup where CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG') and SEC_EC LIKE '" + NE_Element + "%'and SEC_SERVER_IP <> ''")
                                Else
                                    dt_pri = objSourcedb.ExecuteSQL("select  distinct PRI_SERVER_IP,EC_DETAIL,PRIMARY_IN_NAME,PRIMARY_IN_IP from tblcoremap_update_backup where CIRCLE in ('MP','MAH','MUM','GUJ') and EC_DETAIL LIKE '" + NE_Element + "%' and PRI_SERVER_IP <> ''")
                                    dt_sec = objSourcedb.ExecuteSQL("select  distinct SEC_SERVER_IP,SEC_EC,SECONDARY_IN_NAME,SECONDARY_IN_IP from tblcoremap_update_backup where CIRCLE in ('MP','MAH','MUM','GUJ') and SEC_EC LIKE '" + NE_Element + "%'and SEC_SERVER_IP <> ''")
                                End If

                                If (dt_pri.Rows.Count > 0) Then
                                    For i As Integer = 0 To dt_pri.Rows.Count - 1
                                        Try
                                            SERVER_IP = Trim(dt_pri.Rows(i)("PRI_SERVER_IP").ToString())
                                            EC = Trim(dt_pri.Rows(i)("EC_DETAIL").ToString())
                                            IN_NAME = Trim(dt_pri.Rows(i)("PRIMARY_IN_NAME").ToString())
                                            IN_IP = Trim(dt_pri.Rows(i)("PRIMARY_IN_IP").ToString())
                                            If (_CircleName = "KOL") Then
                                                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.tblcoremap_update set EC_DETAIL='" + EC + "',PRIMARY_IN_NAME='" + IN_NAME + "',PRIMARY_IN_IP='" + IN_IP + "' where PRI_SERVER_IP='" + SERVER_IP + "' and CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG')")
                                            Else
                                                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.tblcoremap_update set EC_DETAIL='" + EC + "',PRIMARY_IN_NAME='" + IN_NAME + "',PRIMARY_IN_IP='" + IN_IP + "' where PRI_SERVER_IP='" + SERVER_IP + "' and CIRCLE in ('MP','MAH','MUM','GUJ')")
                                            End If
                                        Catch exx As Exception

                                        End Try
                                    Next
                                End If

                                If (dt_sec.Rows.Count > 0) Then
                                    For i As Integer = 0 To dt_sec.Rows.Count - 1
                                        Try
                                            SERVER_IP = Trim(dt_sec.Rows(i)("SEC_SERVER_IP").ToString())
                                            EC = Trim(dt_sec.Rows(i)("SEC_EC").ToString())
                                            IN_NAME = Trim(dt_sec.Rows(i)("SECONDARY_IN_NAME").ToString())
                                            IN_IP = Trim(dt_sec.Rows(i)("SECONDARY_IN_IP").ToString())
                                            If (_CircleName = "KOL") Then
                                                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.tblcoremap_update set SEC_EC='" + EC + "',SECONDARY_IN_NAME='" + IN_NAME + "',SECONDARY_IN_IP='" + IN_IP + "' where SEC_SERVER_IP='" + SERVER_IP + "' and CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG')")
                                            Else
                                                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.tblcoremap_update set SEC_EC='" + EC + "',SECONDARY_IN_NAME='" + IN_NAME + "',SECONDARY_IN_IP='" + IN_IP + "' where SEC_SERVER_IP='" + SERVER_IP + "' and CIRCLE in ('MP','MAH','MUM','GUJ')")
                                            End If
                                        Catch exx As Exception

                                        End Try
                                    Next
                                End If
                                'update status
                                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set REMARKS='CMD Data Uploaded' where Circle='" + _CircleName + "' and NE_Name='" + NE_Element + "' and NE_IP='" + NEIP + "' and Node_Type='" + NodeType + "' and Domain='PS' and Rtr_Name='" + RouterName + "' and NE_ActiveFlag is true")
                            End If
                            'End If
                        Catch ex As Exception

                        End Try
                    Next
                    objSourcedb.ExecuteNonQuery("update awe_paco_dcm.router_master set  REMARKS=' All " + _NEType + " Data have been Uploaded' where Rtr_IP='" + IP.ToString + "' and domain='PS' and node_type='" + _NEType + "' and Rtr_Name='" + _routerName + "' and circle='" + _CircleName + "' and Rtr_ActiveFlag is true")
                End If
            Else
                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.router_master set  Status='Not Success',ERRORMSG='ROUTER LOGIN FAILLED AFTER 5 ATTEMPTED',REMARKS='Router detail not available' where Rtr_IP='" + IP.ToString + "' and domain='PS' and node_type='" + _NEType + "' and Rtr_Name='" + _routerName + "' and circle='" + _CircleName + "' and Rtr_ActiveFlag is true")
            End If


        End Function
        Public Function EntereNETOCollectData(ByRef SessionName As ScriptingSSH.ScriptingSSH, ByVal circle As String, ByVal routerName As String)
            Dim CMDPrompt As Integer = 0
            Dim Dt As New DataTable
            Dim dtCmd As New DataTable
            Dim NE_Name As String = String.Empty
            Dim NEIP As String = String.Empty
            Dim NEUSERNAME As String = String.Empty
            Dim NEPASSWORD As String = String.Empty
            Dim NEPROMPT As String = String.Empty
            Dim ErrMsg As String = String.Empty
            Dim blnReturnValue As Boolean = True
            Dim chkErrorinLogin As Boolean = False
            Dim NodeType As String = String.Empty
            Dim NeAttempt As Integer = 0
            Dim count As Integer = 0


            Try
                'Getting NE information for the specified circle

                If _NEType = "SGSN" Then
                    Dt = objSourcedb.ExecuteSQL("Select NE_NetAct_Name, NE_Name, NE_IP, NE_UserName, NE_Password,Convert(fetchedTime,Char(50)) as fetchedTime,DelayMinute,LoginCriteria,Node_Type,ProtoCol,NE_Prompt  FROM ne_master Where vendor='" & Trim(_vendor) & "' and Circle ='" & Trim(_CircleName) & "' and OSS ='" & Trim(_OSSName) & "' and Node_Type='" & _NEType & "' and Rtr_Name='" & routerName & "'")
                ElseIf _NEType = "GGSN" Then
                    Dt = objSourcedb.ExecuteSQL("Select NE_NetAct_Name, NE_Name, NE_IP, NE_UserName, NE_Password,Convert(fetchedTime,Char(50)) as fetchedTime,DelayMinute,LoginCriteria,Node_Type,ProtoCol,NE_Prompt  FROM ne_master Where vendor='" & Trim(_vendor) & "' and Circle ='" & Trim(_CircleName) & "' and OSS ='" & Trim(_OSSName) & "' and Node_Type='" & _NEType & "' and Rtr_Name='" & routerName & "'")
                ElseIf _NEType = "CMD" Then
                    Dt = objSourcedb.ExecuteSQL("Select NE_NetAct_Name, NE_Name, NE_IP, NE_UserName, NE_Password,Convert(fetchedTime,Char(50)) as fetchedTime,DelayMinute,LoginCriteria,Node_Type,ProtoCol,NE_Prompt  FROM ne_master Where vendor='" & Trim(_vendor) & "' and Circle ='" & Trim(_CircleName) & "' and OSS ='" & Trim(_OSSName) & "' and Node_Type='" & _NEType & "' and Rtr_Name='" & routerName & "'")
                End If

                If (Not Dt Is Nothing AndAlso Dt.Rows.Count > 0) Then
                    For t As Integer = 0 To Dt.Rows.Count - 1
                        Try
                            NeAttempt = 0
                            LOG = ""
                            SessionName.timeout = 100

                            ''SessionName.ClearSessionLog()
ReAttempt:                  NE_Name = Trim(Dt.Rows(t)("NE_Name").ToString())
                            NEIP = Trim(Dt.Rows(t)("NE_IP").ToString())
                            NEUSERNAME = Trim(Dt.Rows(t)("NE_UserName").ToString())
                            NEPASSWORD = Trim(Dt.Rows(t)("NE_Password").ToString())
                            NodeType = Trim(Dt.Rows(t)("Node_Type").ToString())
                            NEPROMPT = Trim(Dt.Rows(t)("NE_Prompt").ToString())

                            Dim LogPath As String = "D:\DCM\PACO_DCM\" + _NEType + "\" + System.DateTime.Now.ToString("dd-MM-yyyy") + "\" + _CircleName + "\" + NE_Name + "\"
                            If (Not (Directory.Exists(LogPath))) Then
                                Directory.CreateDirectory(LogPath)
                            End If

                            If (_NEType = "GGSN") Or (_NEType = "CMD") Then
                                If Trim(Dt.Rows(t)("Protocol").ToString()) = "SSH" Then
                                    Dim ipssh As String = "ssh -l " + NEUSERNAME + " " + NEIP

                                    SessionName.SendAndWait(ipssh, "Password: ")
                                    SessionName.SendAndWait(NEPASSWORD, "$|#|<|> ", "|", False)

                                Else
                                    SessionName.SendAndWait(NEIP, "login: ")
                                    SessionName.SendAndWait(NEUSERNAME, "password: ")
                                    SessionName.SendAndWait(NEPASSWORD, "$|#|>|< ", "|", False)
                                End If
                            Else
                                SessionName.SendAndWait(NEIP, "ENTER USERNAME < ")
                                SessionName.SendAndWait(NEUSERNAME, "ENTER PASSWORD < ")
                                SessionName.SendAndWait(NEPASSWORD, "$|#|>|< ", "|", False)
                            End If

                            If (_NEType = "CMD") Then
                                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  Status='Success' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and NE_IP='" + NEIP + "' and Node_Type='" + NodeType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
                                CMDfunctioning(TelNetSession, _CircleName, NE_Name, NEPROMPT, _NEType, LogPath, routerName)

                            ElseIf (_NEType = "GGSN") Then

                                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  Status='Success' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and NE_IP='" + NEIP + "' and Node_Type='" + NodeType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
                                GGSNfunctioning(TelNetSession, _CircleName, NE_Name, NEPROMPT, _NEType, LogPath, routerName)

                            ElseIf (_NEType = "SGSN") Then
                                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  Status='Success' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and NE_IP='" + NEIP + "' and Node_Type='" + NodeType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
                                SGSNfunctioning(TelNetSession, _CircleName, NE_Name, NEPROMPT, _NEType, LogPath, routerName, NEPASSWORD)
                                SessionName.SendAndWait("ZZZZ;", "#|>|<|$", "|", False)
                                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set REMARKS='OK' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and NE_IP='" + NEIP + "' and Node_Type='" + NodeType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
                            End If

                        Catch ex As Exception
                            SessionName.SendMessage(Char.ConvertFromUtf32(25), True)
                            System.Threading.Thread.Sleep(5000)
                            If (NeAttempt <= 5) Then
                                NeAttempt += 1
                                System.Threading.Thread.Sleep(1000 * 60 * 1)
                                Dim LoginPath As String = "D:\DCM\PACO_DCM\" + _NEType + "\" + System.DateTime.Now.ToString("dd-MM-yyyy") + "\" + _CircleName + "\" + "ErrorLog" + "\"
                                If (Not (Directory.Exists(LoginPath))) Then
                                    Directory.CreateDirectory(LoginPath)
                                End If
                                File.AppendAllText(LoginPath + NE_Name + ".txt", SessionName.SessionLog + Environment.NewLine + Environment.NewLine + ex.Message + Environment.NewLine)
                                SessionName.ClearSessionLog()
                                GoTo ReAttempt
                            Else
                                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  Status='Not Success',ERRORMSG='NE LOGIN FAILLED AFTER 5 ATTEMPTED',REMARKS='NOT OK' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and NE_IP='" + NEIP + "' and Node_Type='" + NodeType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
                                If (_NEType = "SGSN") Then
                                    'Delete data in current table's if data is exists.
                                    objSourcedb.ExecuteNonQuery("delete from awe_paco_dcm.tblsgsn_nsvci where circle='" + _CircleName + "' and ne_name='" + NE_Name + "'")
                                    objSourcedb.ExecuteNonQuery("delete from awe_paco_dcm.tblcoremap_up_apndata where circle='" + _CircleName + "' and ne_name='" + NE_Name + "'")
                                    'Insert data in current table's to backup table
                                    objSourcedb.ExecuteNonQuery("INSERT INTO awe_paco_dcm.tblsgsn_nsvci SELECT VENDOR, CIRCLE, '', NE_NAME, '', MCC, MNC, LAC, RAC, CI, COMP_UNIT, PAPU_IP, NSEI, NSVC_ID, NSVC_NAME, OP_STATE, DLCI_UDP_PORT, CIR_RDW, BEARER_ID_RSW, BEARER_NAME_RPNBR, PCM_PCU_IP, TS_1, TS_2 FROM awe_paco_dcm.tblsgsn_nsvci_backup  where circle='" + _CircleName + "' and ne_name='" + NE_Name + "'")
                                    objSourcedb.ExecuteNonQuery("INSERT INTO awe_paco_dcm.tblcoremap_up_apndata SELECT * FROM awe_paco_dcm.tblcoremap_up_apndata_backup  where circle='" + _CircleName + "' and ne_name='" + NE_Name + "'")
                                    'update status
                                    objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set REMARKS='SGSN Data Uploaded' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and NE_IP='" + NEIP + "' and Node_Type='" + NodeType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
                                ElseIf (_NEType = "GGSN") Then
                                    'Delete data in current table's if data is exists.
                                    objSourcedb.ExecuteNonQuery("delete FROM awe_paco_dcm.tblcoremap_update where GGSN_NAME='" + NE_Name + "'")
                                    'Insert data in current table's to backup table
                                    objSourcedb.ExecuteNonQuery("insert into awe_paco_dcm.tblcoremap_update SELECT VENDOR, CIRCLE, OSS, NE_NAME, COMP_UNIT, PAPU_IP, PRIMARY_DNS, SECONDARY_DNS, APN, GGSN_SERVER_GW_IP, GGSN_BLADE, GGSN_NAME, IMSI_MISSION, PRI_DIAMETER, PRI_SERVER_IP, '', SEC_DIAMETER, SEC_SERVER_IP, '', '', '', '', '' FROM awe_paco_dcm.tblcoremap_update_backup where GGSN_NAME='" + NE_Name + "'")
                                    'update status
                                    objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set REMARKS='GGSN Data Uploaded' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and NE_IP='" + NEIP + "' and Node_Type='" + NodeType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
                                ElseIf (_NEType = "CMD") Then
                                    Dim dt_pri As DataTable = Nothing
                                    Dim dt_sec As DataTable = Nothing
                                    Dim SERVER_IP As String = String.Empty
                                    Dim EC As String = String.Empty
                                    Dim IN_NAME As String = String.Empty
                                    Dim IN_IP As String = String.Empty

                                    If _CircleName = "KOL" Then
                                        dt_pri = objSourcedb.ExecuteSQL("select  distinct PRI_SERVER_IP,EC_DETAIL,PRIMARY_IN_NAME,PRIMARY_IN_IP from awe_paco_dcm.tblcoremap_update_backup where CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG') and EC_DETAIL LIKE '" + NE_Name + "%' and PRI_SERVER_IP <> ''")
                                        dt_sec = objSourcedb.ExecuteSQL("select  distinct SEC_SERVER_IP,SEC_EC,SECONDARY_IN_NAME,SECONDARY_IN_IP from awe_paco_dcm.tblcoremap_update_backup where CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG') and SEC_EC LIKE '" + NE_Name + "%'and SEC_SERVER_IP <> ''")
                                    Else
                                        dt_pri = objSourcedb.ExecuteSQL("select  distinct PRI_SERVER_IP,EC_DETAIL,PRIMARY_IN_NAME,PRIMARY_IN_IP from tblcoremap_update_backup where CIRCLE in ('MP','MAH','MUM','GUJ') and EC_DETAIL LIKE '" + NE_Name + "%' and PRI_SERVER_IP <> ''")
                                        dt_sec = objSourcedb.ExecuteSQL("select  distinct SEC_SERVER_IP,SEC_EC,SECONDARY_IN_NAME,SECONDARY_IN_IP from tblcoremap_update_backup where CIRCLE in ('MP','MAH','MUM','GUJ') and SEC_EC LIKE '" + NE_Name + "%'and SEC_SERVER_IP <> ''")
                                    End If

                                    If (dt_pri.Rows.Count > 0) Then
                                        For i As Integer = 0 To dt_pri.Rows.Count - 1
                                            Try
                                                SERVER_IP = Trim(dt_pri.Rows(i)("PRI_SERVER_IP").ToString())
                                                EC = Trim(dt_pri.Rows(i)("EC_DETAIL").ToString())
                                                IN_NAME = Trim(dt_pri.Rows(i)("PRIMARY_IN_NAME").ToString())
                                                IN_IP = Trim(dt_pri.Rows(i)("PRIMARY_IN_IP").ToString())
                                                If (_CircleName = "KOL") Then
                                                    objSourcedb.ExecuteNonQuery("update awe_paco_dcm.tblcoremap_update set EC_DETAIL='" + EC + "',PRIMARY_IN_NAME='" + IN_NAME + "',PRIMARY_IN_IP='" + IN_IP + "' where PRI_SERVER_IP='" + SERVER_IP + "' and CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG')")
                                                Else
                                                    objSourcedb.ExecuteNonQuery("update awe_paco_dcm.tblcoremap_update set EC_DETAIL='" + EC + "',PRIMARY_IN_NAME='" + IN_NAME + "',PRIMARY_IN_IP='" + IN_IP + "' where PRI_SERVER_IP='" + SERVER_IP + "' and CIRCLE in ('MP','MAH','MUM','GUJ')")
                                                End If
                                            Catch exx As Exception

                                            End Try
                                        Next
                                    End If

                                    If (dt_sec.Rows.Count > 0) Then
                                        For i As Integer = 0 To dt_sec.Rows.Count - 1
                                            Try
                                                SERVER_IP = Trim(dt_sec.Rows(i)("SEC_SERVER_IP").ToString())
                                                EC = Trim(dt_sec.Rows(i)("SEC_EC").ToString())
                                                IN_NAME = Trim(dt_sec.Rows(i)("SECONDARY_IN_NAME").ToString())
                                                IN_IP = Trim(dt_sec.Rows(i)("SECONDARY_IN_IP").ToString())
                                                If (_CircleName = "KOL") Then
                                                    objSourcedb.ExecuteNonQuery("update awe_paco_dcm.tblcoremap_update set SEC_EC='" + EC + "',SECONDARY_IN_NAME='" + IN_NAME + "',SECONDARY_IN_IP='" + IN_IP + "' where SEC_SERVER_IP='" + SERVER_IP + "' and CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG')")
                                                Else
                                                    objSourcedb.ExecuteNonQuery("update awe_paco_dcm.tblcoremap_update set SEC_EC='" + EC + "',SECONDARY_IN_NAME='" + IN_NAME + "',SECONDARY_IN_IP='" + IN_IP + "' where SEC_SERVER_IP='" + SERVER_IP + "' and CIRCLE in ('MP','MAH','MUM','GUJ')")
                                                End If
                                            Catch exx As Exception

                                            End Try
                                        Next
                                    End If
                                    'update status
                                    objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set REMARKS='CMD Data Uploaded' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and NE_IP='" + NEIP + "' and Node_Type='" + NodeType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
                                End If
                            End If
                        End Try
                    Next
                End If

            Catch ex As Exception
                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  Status='Not Success',ERRORMSG='Query is not execuet',REMARKS='CRITICAL PROBLEM' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and NE_IP='" + NEIP + "' and Node_Type='" + NodeType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
            End Try
        End Function
        Public Function SGSNfunctioning(ByRef SessionName As ScriptingSSH.ScriptingSSH, ByVal Circle As String, ByVal NE_Name As String, ByVal NEPROMPT As String, ByVal Ne_type As String, ByVal LogPath As String, ByVal routerName As String, ByVal Password As String)
            Dim MCC_MNC_DATA As String = String.Empty
            Dim MCC As String = String.Empty
            Dim MNC As String = String.Empty
            Dim count As Integer = 0
            Try



                '*************************Start Fetching data using Command: ZEJL:NSEI=0&&65535:;*******************************************
                Dim GIGBtxt_ZEJL() As String = Nothing
                LOG = ""
                SessionName.ClearSessionLog()
                SessionName.timeout = 360
                SessionName.SendAndWait("ZEJL:NSEI=0&&65535:;", NEPROMPT)
                LOG = SessionName.SessionLog
                File.AppendAllText(LogPath + NE_Name + ".txt", LOG + Environment.NewLine)
                SessionName.ClearSessionLog()
                If (LOG.Contains("/*** YOUR PASSWORD HAS EXPIRED ***/") Or (LOG.Contains("/*** COMMAND NOT AUTHORIZED ***/"))) Then
                    objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  ERRORMSG='YOUR PASSWORD HAS EXPIRED' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and Node_Type='" + _NEType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
                    SessionName.SendAndWait("ZIAG;", "OLD PASSWORD:")
                    SessionName.SendAndWait(Password, "NEW PASSWORD:")
                    SessionName.SendAndWait(Password, "VERIFICATION:")
                    SessionName.SendAndWait(Password, NEPROMPT)
                    LOG += SessionName.SessionLog

                    Dim RouterPath As String = "D:\DCM\PACO_DCM\" + Ne_type + "\" + System.DateTime.Now.ToString("dd-MM-yyyy") + "\" + "NE FAILED\"
                    If (Not (Directory.Exists(RouterPath))) Then
                        Directory.CreateDirectory(RouterPath)
                    End If
                    File.AppendAllText(RouterPath + NE_Name + ".txt", LOG + Environment.NewLine)
                    LOG = ""
                    objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  ERRORMSG='Your password has been Restored' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and Node_Type='" + _NEType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
                    SessionName.timeout = 360
                    SessionName.SendAndWait("ZEJL:NSEI=0&&65535:;", NEPROMPT)
                    LOG = SessionName.SessionLog
                    File.AppendAllText(LogPath + NE_Name + ".txt", LOG + Environment.NewLine)
                End If
                GIGBtxt_ZEJL = Regex.Split(LOG, "\n")
                ParseData_ZEJL(GIGBtxt_ZEJL, _CircleName, NE_Name, LogPath)
                '************************Start Executing Command: ZFWO:NSVCI=0&&65535:;*************************************************
                Dim GIGBtxt_ZFWO() As String = Nothing
                LOG = ""
                SessionName.ClearSessionLog()
                SessionName.timeout = 360
                SessionName.SendAndWait("ZFWO:NSVCI=0&&65535:;", NEPROMPT)
                LOG = SessionName.SessionLog
                File.AppendAllText(LogPath + NE_Name + ".txt", LOG + Environment.NewLine)
                GIGBtxt_ZFWO = Regex.Split(LOG, "\n")
                ParseData_ZFWO(GIGBtxt_ZFWO, _CircleName, NE_Name, LogPath)
                '************************Start Executing Command: ZFUI:;*************************************************
                Dim GIGBtxt_ZFUI() As String = Nothing
                LOG = ""
                SessionName.ClearSessionLog()
                SessionName.SendAndWait("ZFUI:;", NEPROMPT)
                LOG = SessionName.SessionLog
                File.AppendAllText(LogPath + NE_Name + ".txt", LOG + Environment.NewLine)
                GIGBtxt_ZFUI = Regex.Split(LOG, "\n")
                ParseData_ZFUI(GIGBtxt_ZFUI, _CircleName, NE_Name, LogPath)
                '************************Start Executing Command: ZKAI:;*************************************************
                '' NOTE:- Command use for SGSN and GGSN
                Dim GIGBtxt_ZKAI() As String = Nothing
                LOG = ""
                SessionName.ClearSessionLog()
                SessionName.SendAndWait("ZKAI:;", NEPROMPT)
                LOG = SessionName.SessionLog
                File.AppendAllText(LogPath + NE_Name + ".txt", LOG + Environment.NewLine)
                GIGBtxt_ZKAI = Regex.Split(LOG, "\n")
                ParseData_ZKAI(GIGBtxt_ZKAI, _CircleName, NE_Name, LogPath)
                '************************************************************************************************************
                Dim GIGBtxt_ZE6I() As String = Nothing
                LOG = ""
                SessionName.ClearSessionLog()
                SessionName.SendAndWait("ZE6I:;", NEPROMPT)
                LOG = SessionName.SessionLog
                File.AppendAllText(LogPath + NE_Name + ".txt", LOG + Environment.NewLine)
                GIGBtxt_ZE6I = Regex.Split(LOG, "\n")
                ParseData_ZE6I(GIGBtxt_ZE6I, _CircleName, NE_Name, LogPath)
                '***********************Use for GGSN ****************************************************************************
                'Primary DNS IP and Secondry DNS IP data updated for GGSN in tblCoremap table
                'NOTE:- Command use for GGSN
                Dim GIGBtxt_ZQRJ() As String = Nothing
                LOG = ""
                SessionName.ClearSessionLog()
                SessionName.SendAndWait("ZQRJ:;", NEPROMPT)
                LOG = SessionName.SessionLog
                File.AppendAllText(LogPath + NE_Name + ".txt", LOG + Environment.NewLine)
                GIGBtxt_ZQRJ = Regex.Split(LOG, "\n")
                ParseData_ZQRJ(GIGBtxt_ZQRJ, _CircleName, NE_Name, LogPath)


                Dim GIGBtxt_ZWVI() As String = Nothing
                LOG = ""
                SessionName.ClearSessionLog()
                SessionName.SendAndWait("ZWVI::;", NEPROMPT) 'NOTE:- Command use for GGSN 
                LOG = SessionName.SessionLog
                File.AppendAllText(LogPath + NE_Name + ".txt", LOG + Environment.NewLine)
                GIGBtxt_ZWVI = Regex.Split(LOG, "\n")
                MCC_MNC_DATA = ParseData_ZWVI(GIGBtxt_ZWVI, _CircleName, NE_Name, LogPath)

                Dim FindMCCMNC() As String = Nothing
                FindMCCMNC = Regex.Split(MCC_MNC_DATA, "MANISH")
                '************************Start Executing Command: ZQRX*************************************************ZZQRX:PAPU,0:IOCPE,3:PING:HOST="AIRTELFUN.COM.MNC031.MCC404.GPRS",;
                'NOTE:- Command use for GGSN and data insert into tblcoremap_up_apndata table

                Dim dtcomp As DataTable = Nothing
                Dim DtAPN As DataTable = Nothing
                Dim PAPU_INDEX As String = String.Empty
                Dim ZQRX_COMMAND As String = String.Empty
                Dim COMP_UNIT As String = String.Empty
                Dim COMP_UNIT_GBU As String = String.Empty
                Dim IOCPE_FIX_VAL As Integer = 2
                Dim APN As String = String.Empty
                Dim GGSNcircle As String = String.Empty
                Dim GGSN_NE_NAME As String = String.Empty
                MCC = FindMCCMNC(0).Trim
                MNC = FindMCCMNC(1).Trim
                count = 0
                'Note :- Here working for APN And GGSN Name
                Dim dtGGSN As DataTable = Nothing
                dtGGSN = objSourcedb.ExecuteSQL("SELECT distinct circle FROM awe_paco_dcm.apn_detail")
                For Each drcircle As DataRow In dtGGSN.Rows

                    GGSNcircle = drcircle(0).ToString()
                    If (GGSNcircle = "MUM") Then
                        If ((_CircleName = "MUM") Or (_CircleName = "MP") Or (_CircleName = "MAH") Or (_CircleName = "GUJ")) Then
                            DtAPN = objSourcedb.ExecuteSQL("SELECT * FROM awe_paco_dcm.apn_detail where circle='" & GGSNcircle.ToString() & "'")
                            Exit For
                        End If
                    End If
                    If (GGSNcircle = "KOL") Then
                        If ((_CircleName = "KOL") Or (_CircleName = "ORI") Or (_CircleName = "BIHAR") Or (_CircleName = "WB") Or (_CircleName = "ASSAM") Or _CircleName = "SHILONG") Then
                            DtAPN = objSourcedb.ExecuteSQL("SELECT * FROM awe_paco_dcm.apn_detail where circle='" & GGSNcircle.ToString() & "'")
                            Exit For
                        End If
                    End If
                Next

                '********************************************************************************************************************************************************************
                '' ( ATAKA NE names--------------------------------------------------------------------------------------------------------------------------------------------------------)
                If ((Not ((NE_Name = "NS_ASSAM") And (_CircleName = "ASSAM"))) And (Not ((NE_Name = "SGSN04" Or NE_Name = "SGSN05") And (_CircleName = "MAH"))) And (Not ((NE_Name = "SGSN4_RJT") And (_CircleName = "GUJ"))) And (Not ((NE_Name = "SGN5_901235") And (_CircleName = "BIHAR"))) And (Not ((NE_Name = "SGSN4") And (_CircleName = "MP"))) And (Not ((NE_Name = "SGSN2MUM") And (_CircleName = "MUM"))) And (Not ((NE_Name = "FINS_ROWB_1") And (_CircleName = "WB")))) Then
                    'DtAPN = objSourcedb.ExecuteSQL("SELECT * FROM awe_paco_dcm.apn_detail where circle='" & GGSNcircle.ToString() & "' order by GGSN_NAME asc ")

                    If (DtAPN.Rows.Count > 0) Then
                        For Each drAPN As DataRow In DtAPN.Rows
                            APN = ""
                            GGSN_NE_NAME = ""
                            System.Threading.Thread.Sleep(1000)
                            APN = drAPN(0).ToString.Trim
                            'For a As Integer = 0 To APNList.Count - 1
                            'If (APN = APNList(a).ToString()) Then
                            dtcomp = Nothing
                            dtcomp = objSourcedb.ExecuteSQL("SELECT COMP_UNIT,CAST(SUBSTRING_INDEX(COMP_UNIT,'-','-1') AS UNSIGNED) AS PAPU_INDEX FROM awe_paco_dcm.tblcoremap where CIRCLE='" & _CircleName & "' AND NE_NAME='" & NE_Name & "' AND VENDOR='" & _vendor & "' order by COMP_UNIT asc")

                            If (dtcomp.Rows.Count > 0) Then
                                For i As Integer = 0 To dtcomp.Rows.Count - 1
                                    System.Threading.Thread.Sleep(1000)
                                    COMP_UNIT_GBU = IIf(IsDBNull(dtcomp.Rows(i)("COMP_UNIT")), "", dtcomp.Rows(i)("COMP_UNIT")).ToString()
                                    If (COMP_UNIT_GBU.Contains("PAPU")) Then
                                        Dim GIGBtxt_ZQRX() As String = Nothing
                                        LOG = ""
                                        ZQRX_COMMAND = ""
                                        PAPU_INDEX = ""
                                        COMP_UNIT = ""
                                        SessionName.ClearSessionLog()
                                        PAPU_INDEX = IIf(IsDBNull(dtcomp.Rows(i)("PAPU_INDEX")), "", dtcomp.Rows(i)("PAPU_INDEX")).ToString()
                                        COMP_UNIT = IIf(IsDBNull(dtcomp.Rows(i)("COMP_UNIT")), "", dtcomp.Rows(i)("COMP_UNIT")).ToString()
                                        ZQRX_COMMAND = GetCommand(_CircleName, NE_Name, PAPU_INDEX, IOCPE_FIX_VAL.ToString(), MCC, MNC, APN, LogPath)

                                        Try
ZQRX:                                       count += 1
                                            SessionName.timeout = 60
                                            SessionName.SendAndWait(ZQRX_COMMAND, NEPROMPT)
                                            LOG = SessionName.SessionLog
                                            File.AppendAllText(LogPath + NE_Name + ".txt", LOG + Environment.NewLine)
                                            If (LOG.Contains("COMMAND EXECUTED")) Then

                                                GIGBtxt_ZQRX = Regex.Split(LOG, "\n")
                                                ParseData_ZQRX(GIGBtxt_ZQRX, _CircleName, NE_Name, COMP_UNIT, APN, GGSN_NE_NAME, LogPath)
                                                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  ERRORMSG='' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and Node_Type='" + _NEType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
                                            End If
                                        Catch ex As Exception
                                            objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  ERRORMSG='COMMAND LABLE PROBLEM(NON ATKA-ZQRX:PAPU)' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and Node_Type='" + _NEType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
                                            SessionName.SendMessage(Char.ConvertFromUtf32(25), True)
                                            System.Threading.Thread.Sleep(100)
                                            File.AppendAllText(LogPath + NE_Name + ".txt", SessionName.SessionLog + Environment.NewLine + ex.Message + Environment.NewLine)
                                            ZQRX_COMMAND = ""
                                            SessionName.ClearSessionLog()
                                            If (count > 2) Then
                                                System.Threading.Thread.Sleep(2000)
                                                GoTo NextAPN
                                            End If
                                            If (count < 2) Then
                                                IOCPE_FIX_VAL += 1
                                                ZQRX_COMMAND = GetCommand(_CircleName, NE_Name, PAPU_INDEX, IOCPE_FIX_VAL.ToString(), MCC, MNC, APN, LogPath)
                                                System.Threading.Thread.Sleep(2000)
                                                GoTo ZQRX
                                            End If
                                        End Try
                                    End If
                                Next
                            End If
                            'End If
                            'Next
                            System.Threading.Thread.Sleep(2000)
NextAPN:                Next
                    End If
                Else ' Here Use Atka(GBU)

                    'DtAPN = objSourcedb.ExecuteSQL("SELECT * FROM awe_paco_dcm.apn_detail where circle='" & GGSNcircle.ToString() & "' order by GGSN_NAME asc ")
                    If (DtAPN.Rows.Count > 0) Then
                        For Each drAPN As DataRow In DtAPN.Rows
                            APN = ""
                            GGSN_NE_NAME = ""
                            APN = drAPN(0).ToString.Trim
                            'GGSN_NE_NAME = drAPN(1).ToString

                            'For a As Integer = 0 To APNList.Count - 1
                            '    If (APN = APNList(a).ToString()) Then

                            dtcomp = objSourcedb.ExecuteSQL("SELECT COMP_UNIT,CAST(SUBSTRING_INDEX(COMP_UNIT,'-','-1') AS UNSIGNED) AS PAPU_INDEX FROM awe_paco_dcm.tblcoremap where CIRCLE='" & _CircleName & "' AND NE_NAME='" & NE_Name & "' AND VENDOR='" & _vendor & "' order by COMP_UNIT asc")

                            If (dtcomp.Rows.Count > 0) Then
                                For i As Integer = 0 To dtcomp.Rows.Count - 1

                                    Dim GIGBtxt_ZQRX() As String = Nothing
                                    LOG = ""
                                    ZQRX_COMMAND = ""
                                    PAPU_INDEX = ""
                                    COMP_UNIT = ""
                                    SessionName.ClearSessionLog()
                                    PAPU_INDEX = IIf(IsDBNull(dtcomp.Rows(i)("PAPU_INDEX")), "", dtcomp.Rows(i)("PAPU_INDEX")).ToString()
                                    COMP_UNIT = IIf(IsDBNull(dtcomp.Rows(i)("COMP_UNIT")), "", dtcomp.Rows(i)("COMP_UNIT")).ToString()
                                    ZQRX_COMMAND = ATKAGetCommand(_CircleName, NE_Name, PAPU_INDEX, IOCPE_FIX_VAL.ToString(), MCC, MNC, APN, LogPath)

                                    Try
                                        SessionName.timeout = 60
                                        SessionName.SendAndWait(ZQRX_COMMAND, "<|>|#|$", "|", False)
                                        LOG = SessionName.SessionLog
                                        File.AppendAllText(LogPath + NE_Name + ".txt", LOG + Environment.NewLine)
                                        If (LOG.Contains("COMMAND EXECUTED")) Then

                                            GIGBtxt_ZQRX = Regex.Split(LOG, "\n")
                                            ParseData_ZQRX(GIGBtxt_ZQRX, _CircleName, NE_Name, COMP_UNIT, APN, GGSN_NE_NAME, LogPath)
                                            objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  ERRORMSG='' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and Node_Type='" + _NEType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
                                        End If
                                    Catch ex As Exception
                                        SessionName.SendMessage(Char.ConvertFromUtf32(25), True)
                                        System.Threading.Thread.Sleep(100)
                                        File.AppendAllText(LogPath + NE_Name + ".txt", SessionName.SessionLog + Environment.NewLine + ex.Message + Environment.NewLine)
                                        ZQRX_COMMAND = ""
                                        SessionName.ClearSessionLog()
                                        objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  ERRORMSG='COMMAND LABLE PROBLEM(ATKA-ZQRX:PAPU)' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and Node_Type='" + _NEType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
                                        System.Threading.Thread.Sleep(2000)
                                        GoTo NextAPNGBU

                                    End Try

                                Next
                            End If
                            'End If
                            'Next
NextAPNGBU:             Next

                    End If

                End If

                '**************************************************************************************************************************************************

            Catch ex As Exception
                File.AppendAllText(LogPath + NE_Name + ".txt", System.DateTime.Now.ToString() + Environment.NewLine + "COMMAND LABLE PROBLEM  " + ex.Message + Environment.NewLine + Environment.NewLine)
                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  ERRORMSG='COMMAND LABLE PROBLEM' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and Node_Type='" + _NEType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
            End Try

        End Function
        '           *************************Change End**************



        '        Public Function GetServer(ByRef SessioName As ScriptingSSH.ScriptingSSH, ByVal IP As String, ByVal UserName As String, ByVal password As String, ByVal port As Integer, ByVal TimeOut As Integer) As Boolean
        '            Try
        '                Dim Attempt As Integer = 0
        'Retry_Router:   SessioName = New ScriptingSSH.ScriptingSSH(IP, port, UserName, password, TimeOut)
        '                If (SessioName.Connect() = True) Then
        '                    objSourcedb.ExecuteNonQuery("update awe_paco_dcm.router_master set  Status='Success' where Rtr_IP='" + IP.ToString + "' and domain='PS' and node_type='" + _NEType + "' and Rtr_Name='" + _routerName + "' and circle='" + _CircleName + "' and Rtr_ActiveFlag is true")
        '                    Return True
        '                Else
        '                    If (Attempt <= 5) Then
        '                        System.Threading.Thread.Sleep(60 * 5 * 1000)
        '                        Attempt += 1
        '                        Dim RouterPath As String = "D:\DCM\PACO_DCM\" + NE_Type + "\" + System.DateTime.Now.ToString("dd-MM-yyyy") + "\" + "ROUTER FAILED\"
        '                        If (Not (Directory.Exists(RouterPath))) Then
        '                            Directory.CreateDirectory(RouterPath)
        '                        End If
        '                        File.AppendAllText(RouterPath + _CircleName + ".txt", " ROUTER LOGIN FAILLED " + IP + "   " + System.DateTime.Now.ToString() + Environment.NewLine)
        '                        GoTo Retry_Router
        '                    End If
        '                    objSourcedb.ExecuteNonQuery("update awe_paco_dcm.router_master set  Status='Not Success',ERRORMSG='ROUTER LOGIN FAILLED AFTER 5 ATTEMPTED',REMARKS='NOT OK' where Rtr_IP='" + IP.ToString + "' and domain='PS' and node_type='" + _NEType + "' and Rtr_Name='" + _routerName + "' and circle='" + _CircleName + "' and Rtr_ActiveFlag is true")
        '                    Return False
        '                End If


        '            Catch ex As Exception
        '                Dim RouterPath As String = "D:\DCM\PACO_DCM\" + NE_Type + "\" + System.DateTime.Now.ToString("dd-MM-yyyy") + "\" + "ROUTER FAILED\"
        '                If (Not (Directory.Exists(RouterPath))) Then
        '                    Directory.CreateDirectory(RouterPath)
        '                End If
        '                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.router_master set  Status='Not Success',ERRORMSG='Router is not login',REMARKS='CRITICAL PROBLEM' where Rtr_IP='" + IP.ToString + "' and domain='PS' and node_type='" + _NEType + "' and Rtr_Name='" + _routerName + "' and circle='" + _CircleName + "' and Rtr_ActiveFlag is true")
        '                File.AppendAllText(RouterPath + _CircleName + ".txt", " ROUTER LOGIN Problem.... " + IP + "   " + ex.Message + "   " + System.DateTime.Now.ToString() + Environment.NewLine)

        '            End Try

        '        End Function
        '        Public Function EntereNETOCollectData(ByRef SessionName As ScriptingSSH.ScriptingSSH, ByVal circle As String, ByVal routerName As String)
        '            Dim CMDPrompt As Integer = 0
        '            Dim Dt As New DataTable
        '            Dim dtCmd As New DataTable
        '            Dim NE_Name As String = String.Empty
        '            Dim NEIP As String = String.Empty
        '            Dim NEUSERNAME As String = String.Empty
        '            Dim NEPASSWORD As String = String.Empty
        '            Dim NEPROMPT As String = String.Empty
        '            Dim ErrMsg As String = String.Empty
        '            Dim blnReturnValue As Boolean = True
        '            Dim chkErrorinLogin As Boolean = False
        '            Dim NodeType As String = String.Empty
        '            Dim NeAttempt As Integer = 0
        '            Dim count As Integer = 0


        '            Try
        '                'Getting NE information for the specified circle

        '                If _NEType = "SGSN" Then
        '                    Dt = objSourcedb.ExecuteSQL("Select NE_NetAct_Name, NE_Name, NE_IP, NE_UserName, NE_Password,Convert(fetchedTime,Char(50)) as fetchedTime,DelayMinute,LoginCriteria,Node_Type,ProtoCol,NE_Prompt  FROM ne_master Where vendor='" & Trim(_vendor) & "' and Circle ='" & Trim(_CircleName) & "' and OSS ='" & Trim(_OSSName) & "' and Node_Type='" & _NEType & "' and Rtr_Name='" & routerName & "'")
        '                ElseIf _NEType = "GGSN" Then
        '                    Dt = objSourcedb.ExecuteSQL("Select NE_NetAct_Name, NE_Name, NE_IP, NE_UserName, NE_Password,Convert(fetchedTime,Char(50)) as fetchedTime,DelayMinute,LoginCriteria,Node_Type,ProtoCol,NE_Prompt  FROM ne_master Where vendor='" & Trim(_vendor) & "' and Circle ='" & Trim(_CircleName) & "' and OSS ='" & Trim(_OSSName) & "' and Node_Type='" & _NEType & "' and Rtr_Name='" & routerName & "'")
        '                ElseIf _NEType = "CMD" Then
        '                    Dt = objSourcedb.ExecuteSQL("Select NE_NetAct_Name, NE_Name, NE_IP, NE_UserName, NE_Password,Convert(fetchedTime,Char(50)) as fetchedTime,DelayMinute,LoginCriteria,Node_Type,ProtoCol,NE_Prompt  FROM ne_master Where vendor='" & Trim(_vendor) & "' and Circle ='" & Trim(_CircleName) & "' and OSS ='" & Trim(_OSSName) & "' and Node_Type='" & _NEType & "' and Rtr_Name='" & routerName & "'")
        '                End If

        '                If (Not Dt Is Nothing AndAlso Dt.Rows.Count > 0) Then
        '                    For t As Integer = 0 To Dt.Rows.Count - 1
        '                        Try
        '                            NeAttempt = 0
        '                            LOG = ""
        '                            SessionName.timeout = 100

        '                            ''SessionName.ClearSessionLog()
        'ReAttempt:                  NE_Name = Trim(Dt.Rows(t)("NE_Name").ToString())
        '                            NEIP = Trim(Dt.Rows(t)("NE_IP").ToString())
        '                            NEUSERNAME = Trim(Dt.Rows(t)("NE_UserName").ToString())
        '                            NEPASSWORD = Trim(Dt.Rows(t)("NE_Password").ToString())
        '                            NodeType = Trim(Dt.Rows(t)("Node_Type").ToString())
        '                            NEPROMPT = Trim(Dt.Rows(t)("NE_Prompt").ToString())

        '                            Dim LogPath As String = "D:\DCM\PACO_DCM\" + _NEType + "\" + System.DateTime.Now.ToString("dd-MM-yyyy") + "\" + _CircleName + "\" + NE_Name + "\"
        '                            If (Not (Directory.Exists(LogPath))) Then
        '                                Directory.CreateDirectory(LogPath)
        '                            End If

        '                            If (_NEType = "GGSN") Or (_NEType = "CMD") Then
        '                                If Trim(Dt.Rows(t)("Protocol").ToString()) = "SSH" Then
        '                                    Dim ipssh As String = "ssh -l " + NEUSERNAME + " " + NEIP

        '                                    SessionName.SendAndWait(ipssh, "Password: ")
        '                                    SessionName.SendAndWait(NEPASSWORD, "$|#|<|> ", "|", False)

        '                                Else
        '                                    SessionName.SendAndWait(NEIP, "login: ")
        '                                    SessionName.SendAndWait(NEUSERNAME, "password: ")
        '                                    SessionName.SendAndWait(NEPASSWORD, "$|#|>|< ", "|", False)
        '                                End If
        '                            Else
        '                                SessionName.SendAndWait(NEIP, "ENTER USERNAME < ")
        '                                SessionName.SendAndWait(NEUSERNAME, "ENTER PASSWORD < ")
        '                                SessionName.SendAndWait(NEPASSWORD, "$|#|>|< ", "|", False)
        '                            End If

        '                            If (_NEType = "CMD") Then
        '                                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  Status='Success' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and NE_IP='" + NEIP + "' and Node_Type='" + NodeType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
        '                                CMDfunctioning(TelNetSession, _CircleName, NE_Name, NEPROMPT, _NEType, LogPath, routerName)

        '                            ElseIf (_NEType = "GGSN") Then

        '                                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  Status='Success' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and NE_IP='" + NEIP + "' and Node_Type='" + NodeType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
        '                                GGSNfunctioning(TelNetSession, _CircleName, NE_Name, NEPROMPT, _NEType, LogPath, routerName)

        '                            ElseIf (_NEType = "SGSN") Then
        '                                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  Status='Success' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and NE_IP='" + NEIP + "' and Node_Type='" + NodeType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
        '                                SGSNfunctioning(TelNetSession, _CircleName, NE_Name, NEPROMPT, _NEType, LogPath, routerName)
        '                                SessionName.SendAndWait("ZZZZ;", "#|>|<|$", "|", False)
        '                                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set REMARKS='OK' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and NE_IP='" + NEIP + "' and Node_Type='" + NodeType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
        '                            End If

        '                        Catch ex As Exception
        '                            SessionName.SendMessage(Char.ConvertFromUtf32(25), True)
        '                            System.Threading.Thread.Sleep(5000)
        '                            If (NeAttempt <= 5) Then
        '                                NeAttempt += 1
        '                                System.Threading.Thread.Sleep(1000 * 60 * 1)
        '                                Dim LoginPath As String = "D:\DCM\PACO_DCM\" + _NEType + "\" + System.DateTime.Now.ToString("dd-MM-yyyy") + "\" + _CircleName + "\" + "ErrorLog" + "\"
        '                                If (Not (Directory.Exists(LoginPath))) Then
        '                                    Directory.CreateDirectory(LoginPath)
        '                                End If
        '                                File.AppendAllText(LoginPath + NE_Name + ".txt", SessionName.SessionLog + Environment.NewLine + Environment.NewLine + ex.Message + Environment.NewLine)
        '                                SessionName.ClearSessionLog()
        '                                GoTo ReAttempt
        '                            Else
        '                                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  Status='Not Success',ERRORMSG='NE LOGIN FAILLED AFTER 5 ATTEMPTED',REMARKS='NOT OK' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and NE_IP='" + NEIP + "' and Node_Type='" + NodeType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
        '                                If (_NEType = "SGSN") Then
        '                                    'Delete data in current table's if data is exists.
        '                                    objSourcedb.ExecuteNonQuery("delete from awe_paco_dcm.tblsgsn_nsvci where circle='" + _CircleName + "' and ne_name='" + NE_Name + "'")
        '                                    objSourcedb.ExecuteNonQuery("delete from awe_paco_dcm.tblcoremap_up_apndata where circle='" + _CircleName + "' and ne_name='" + NE_Name + "'")
        '                                    'Insert data in current table's to backup table
        '                                    objSourcedb.ExecuteNonQuery("INSERT INTO awe_paco_dcm.tblsgsn_nsvci SELECT VENDOR, CIRCLE, '', NE_NAME, '', MCC, MNC, LAC, RAC, CI, COMP_UNIT, PAPU_IP, NSEI, NSVC_ID, NSVC_NAME, OP_STATE, DLCI_UDP_PORT, CIR_RDW, BEARER_ID_RSW, BEARER_NAME_RPNBR, PCM_PCU_IP, TS_1, TS_2 FROM awe_paco_dcm.tblsgsn_nsvci_backup  where circle='" + _CircleName + "' and ne_name='" + NE_Name + "'")
        '                                    objSourcedb.ExecuteNonQuery("INSERT INTO awe_paco_dcm.tblcoremap_up_apndata SELECT * FROM awe_paco_dcm.tblcoremap_up_apndata_backup  where circle='" + _CircleName + "' and ne_name='" + NE_Name + "'")
        '                                    'update status
        '                                    objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set REMARKS='SGSN Data Uploaded' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and NE_IP='" + NEIP + "' and Node_Type='" + NodeType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
        '                                ElseIf (_NEType = "GGSN") Then
        '                                    'Delete data in current table's if data is exists.
        '                                    objSourcedb.ExecuteNonQuery("delete FROM awe_paco_dcm.tblcoremap_update where GGSN_NAME='" + NE_Name + "'")
        '                                    'Insert data in current table's to backup table
        '                                    objSourcedb.ExecuteNonQuery("insert into awe_paco_dcm.tblcoremap_update SELECT VENDOR, CIRCLE, OSS, NE_NAME, COMP_UNIT, PAPU_IP, PRIMARY_DNS, SECONDARY_DNS, APN, GGSN_SERVER_GW_IP, GGSN_BLADE, GGSN_NAME, IMSI_MISSION, PRI_DIAMETER, PRI_SERVER_IP, '', SEC_DIAMETER, SEC_SERVER_IP, '', '', '', '', '' FROM awe_paco_dcm.tblcoremap_update_backup where GGSN_NAME='" + NE_Name + "'")
        '                                    'update status
        '                                    objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set REMARKS='GGSN Data Uploaded' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and NE_IP='" + NEIP + "' and Node_Type='" + NodeType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
        '                                ElseIf (_NEType = "CMD") Then
        '                                    Dim dt_pri As DataTable = Nothing
        '                                    Dim dt_sec As DataTable = Nothing
        '                                    Dim SERVER_IP As String = String.Empty
        '                                    Dim EC As String = String.Empty
        '                                    Dim IN_NAME As String = String.Empty
        '                                    Dim IN_IP As String = String.Empty

        '                                    If _CircleName = "KOL" Then
        '                                        dt_pri = objSourcedb.ExecuteSQL("select  distinct PRI_SERVER_IP,EC_DETAIL,PRIMARY_IN_NAME,PRIMARY_IN_IP from awe_paco_dcm.tblcoremap_update_backup where CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG') and EC_DETAIL LIKE '" + NE_Name + "%' and PRI_SERVER_IP <> ''")
        '                                        dt_sec = objSourcedb.ExecuteSQL("select  distinct SEC_SERVER_IP,SEC_EC,SECONDARY_IN_NAME,SECONDARY_IN_IP from awe_paco_dcm.tblcoremap_update_backup where CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG') and SEC_EC LIKE '" + NE_Name + "%'and SEC_SERVER_IP <> ''")
        '                                    Else
        '                                        dt_pri = objSourcedb.ExecuteSQL("select  distinct PRI_SERVER_IP,EC_DETAIL,PRIMARY_IN_NAME,PRIMARY_IN_IP from tblcoremap_update_backup where CIRCLE in ('MP','MAH','MUM','GUJ') and EC_DETAIL LIKE '" + NE_Name + "%' and PRI_SERVER_IP <> ''")
        '                                        dt_sec = objSourcedb.ExecuteSQL("select  distinct SEC_SERVER_IP,SEC_EC,SECONDARY_IN_NAME,SECONDARY_IN_IP from tblcoremap_update_backup where CIRCLE in ('MP','MAH','MUM','GUJ') and SEC_EC LIKE '" + NE_Name + "%'and SEC_SERVER_IP <> ''")
        '                                    End If

        '                                    If (dt_pri.Rows.Count > 0) Then
        '                                        For i As Integer = 0 To dt_pri.Rows.Count - 1
        '                                            Try
        '                                                SERVER_IP = Trim(dt_pri.Rows(i)("PRI_SERVER_IP").ToString())
        '                                                EC = Trim(dt_pri.Rows(i)("EC_DETAIL").ToString())
        '                                                IN_NAME = Trim(dt_pri.Rows(i)("PRIMARY_IN_NAME").ToString())
        '                                                IN_IP = Trim(dt_pri.Rows(i)("PRIMARY_IN_IP").ToString())
        '                                                If (_CircleName = "KOL") Then
        '                                                    objSourcedb.ExecuteNonQuery("update awe_paco_dcm.tblcoremap_update set EC_DETAIL='" + EC + "',PRIMARY_IN_NAME='" + IN_NAME + "',PRIMARY_IN_IP='" + IN_IP + "' where PRI_SERVER_IP='" + SERVER_IP + "' and CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG')")
        '                                                Else
        '                                                    objSourcedb.ExecuteNonQuery("update awe_paco_dcm.tblcoremap_update set EC_DETAIL='" + EC + "',PRIMARY_IN_NAME='" + IN_NAME + "',PRIMARY_IN_IP='" + IN_IP + "' where PRI_SERVER_IP='" + SERVER_IP + "' and CIRCLE in ('MP','MAH','MUM','GUJ')")
        '                                                End If
        '                                            Catch exx As Exception

        '                                            End Try
        '                                        Next
        '                                    End If

        '                                    If (dt_sec.Rows.Count > 0) Then
        '                                        For i As Integer = 0 To dt_sec.Rows.Count - 1
        '                                            Try
        '                                                SERVER_IP = Trim(dt_sec.Rows(i)("SEC_SERVER_IP").ToString())
        '                                                EC = Trim(dt_sec.Rows(i)("SEC_EC").ToString())
        '                                                IN_NAME = Trim(dt_sec.Rows(i)("SECONDARY_IN_NAME").ToString())
        '                                                IN_IP = Trim(dt_sec.Rows(i)("SECONDARY_IN_IP").ToString())
        '                                                If (_CircleName = "KOL") Then
        '                                                    objSourcedb.ExecuteNonQuery("update awe_paco_dcm.tblcoremap_update set SEC_EC='" + EC + "',SECONDARY_IN_NAME='" + IN_NAME + "',SECONDARY_IN_IP='" + IN_IP + "' where SEC_SERVER_IP='" + SERVER_IP + "' and CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG')")
        '                                                Else
        '                                                    objSourcedb.ExecuteNonQuery("update awe_paco_dcm.tblcoremap_update set SEC_EC='" + EC + "',SECONDARY_IN_NAME='" + IN_NAME + "',SECONDARY_IN_IP='" + IN_IP + "' where SEC_SERVER_IP='" + SERVER_IP + "' and CIRCLE in ('MP','MAH','MUM','GUJ')")
        '                                                End If
        '                                            Catch exx As Exception

        '                                            End Try
        '                                        Next
        '                                    End If
        '                                    'update status
        '                                    objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set REMARKS='CMD Data Uploaded' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and NE_IP='" + NEIP + "' and Node_Type='" + NodeType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
        '                                End If
        '                            End If
        '                        End Try
        '                    Next
        '                End If

        '            Catch ex As Exception
        '                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  Status='Not Success',ERRORMSG='Query is not execuet',REMARKS='CRITICAL PROBLEM' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and NE_IP='" + NEIP + "' and Node_Type='" + NodeType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
        '            End Try
        '        End Function

        Public Function CMDfunctioning(ByRef SessionName As ScriptingSSH.ScriptingSSH, ByVal Circle As String, ByVal NE_Name As String, ByVal NEPROMPT As String, ByVal Ne_type As String, ByVal LogPath As String, ByVal routerName As String)
            Dim CMDPrompt As String = String.Empty
            Try

                '---------------------------------------------------------------------------------------------------------------------------------------------------------------------
                CMDPrompt = SessionName.SendAndWait("cat /etc/hosts", "#|>|<|$|--More--", "|", False)
                LOG = SessionName.SessionLog

                If (CMDPrompt = 3) Then
                    LOG = SessionName.SessionLog
                Else
AgainCMD:           CMDPrompt = SessionName.SendAndWait(" ", "#|>|<|$|--More--", "|", False)
                    LOG = SessionName.SessionLog
                    If (CMDPrompt = 4) Then
                        GoTo AgainCMD
                    End If
                End If
                System.Threading.Thread.Sleep(1000)
                LOG = SessionName.SessionLog
                Dim CAT_ETC_HOST_CMD() As String = Regex.Split(LOG, "\n")
                File.AppendAllText(LogPath + NE_Name + ".txt", System.DateTime.Now.ToString() + Environment.NewLine + LOG + Environment.NewLine + Environment.NewLine)
                CATETCHOSTCMD(CAT_ETC_HOST_CMD, _CircleName, NE_Name, LogPath)
                LOG = ""
                SessionName.ClearSessionLog()
                CMDPrompt = SessionName.SendAndWait("cd /etc/opt/cmd/Mediate/cfg", "#|$|>|< ", "|", False)
                System.Threading.Thread.Sleep(1000)
                CMDPrompt = SessionName.SendAndWait("ls -lrt Bharti_LookupData*", "#|$|>|< ", "|", False)
                System.Threading.Thread.Sleep(1000)
                LOG = SessionName.SessionLog
                Dim Bharti_LookUPdata() As String = Regex.Split(LOG, "\n")
                File.AppendAllText(LogPath + NE_Name + ".txt", System.DateTime.Now.ToString() + Environment.NewLine + LOG + Environment.NewLine + Environment.NewLine)
                Bhartilookupdata(Bharti_LookUPdata, _CircleName, NE_Name, LogPath)
                LOG = ""
                SessionName.ClearSessionLog()
                '*******************************************************Primary XML file arrayList*****************************************
                Dim XMLcommand As String = String.Empty
                Dim pricatBhartiloopup() As String = Nothing
                For k As Integer = 0 To priXMLfilelist.Count - 1 'pri XML file array list
                    LOG = ""
                    SessionName.ClearSessionLog()
                    XMLcommand = "cat " & priXMLfilelist(k).ToString()
RePRI_EC:           SessionName.SendAndWait(XMLcommand, "#|$|>|< ", "|", False)
                    SessionName.SendAndWait(" ", "#|$|>|< ", "|", False)
                    LOG = SessionName.SessionLog
                    If (LOG.Contains("</LookupData>")) Then
                        pricatBhartiloopup = Regex.Split(LOG, "\n")
                        File.AppendAllText(LogPath + NE_Name + ".txt", "PrimaryData : -" + System.DateTime.Now.ToString() + Environment.NewLine + LOG + Environment.NewLine + Environment.NewLine)
                        IMSILISTData(pricatBhartiloopup, _CircleName, NE_Name, "UseforPRIvalue", LogPath)
                        System.Threading.Thread.Sleep(1000)
                    Else
                        System.Threading.Thread.Sleep(1000)
                        SessionName.ClearSessionLog()
                        LOG = ""
                        GoTo RePRI_EC
                    End If

                Next

                Dim seccatBhartiloopup() As String = Nothing
                SessionName.SendAndWait("cd /etc/opt/cmd/Mediate/cfg", "$|#|>|< ", "|", False)
                For z As Integer = 0 To secXMLfilelist.Count - 1 'sec XML file array list
                    LOG = ""
                    SessionName.ClearSessionLog()
                    XMLcommand = "cat " & secXMLfilelist(z).ToString()
ReSEC_EC:           SessionName.SendAndWait(XMLcommand, "$|#|>|< ", "|", False)
                    SessionName.SendAndWait(" ", "#|$|>|< ", "|", False)
                    LOG = SessionName.SessionLog
                    If (LOG.Contains("</LookupData>")) Then
                        seccatBhartiloopup = Regex.Split(LOG, "\n")
                        File.AppendAllText(LogPath + NE_Name + ".txt", "SecondaryData:- " + System.DateTime.Now.ToString() + Environment.NewLine + LOG + Environment.NewLine + Environment.NewLine)
                        IMSILISTData(seccatBhartiloopup, _CircleName, NE_Name, "UseforSECvalue", LogPath)
                        System.Threading.Thread.Sleep(1000)
                    Else
                        System.Threading.Thread.Sleep(1000)
                        SessionName.ClearSessionLog()
                        LOG = ""
                        GoTo ReSEC_EC
                    End If
                Next

                SessionName.SendAndWait("exit", ">|<|$|#", "|", False) ''eixt from NE
                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  REMARKS='OK' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and Node_Type='" + _NEType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")

            Catch ex As Exception
                SessionName.SendMessage(Char.ConvertFromUtf32(25), True)
                File.AppendAllText(LogPath + NE_Name + ".txt", System.DateTime.Now.ToString() + Environment.NewLine + "COMMAND LABLE PROBLEM  " + ex.Message + Environment.NewLine + Environment.NewLine)
                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  ERRORMSG='COMMAND LABLE PROBLEM' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and Node_Type='" + _NEType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
            End Try
        End Function
        Public Function GGSNfunctioning(ByRef SessionName As ScriptingSSH.ScriptingSSH, ByVal Circle As String, ByVal NE_Name As String, ByVal NEPROMPT As String, ByVal Ne_type As String, ByVal LogPath As String, ByVal routerName As String)
            Try



                Dim CMDPrompt As String = String.Empty
                Dim dtAPN As DataTable = Nothing
                Dim APN As String = String.Empty
                Dim APNListItem As String = String.Empty
                System.Threading.Thread.Sleep(1000)

                'NOTE :- Plz do not remove commect code  this data use for GGSN APN data 

                '******************************************************APN Detail*****************************************************
                'Try
                '    SessionName.SendAndWait("fsclish", "#|>|<|$|--More--", "|", False)
                '    Dim GIGBtxt_TAB() As String = Nothing
                '    LOG = ""
                '    SessionName.ClearSessionLog()
                '    SessionName.SendAndWaitTAB("show ng session-profile ", "#|>|<|$", "|", False)
                '    SessionName.SendAndWait(" ", "#|>|<|$", "|", False)
                '    SessionName.SendAndWait(" ", "#|>|<|$", "|", False)
                '    LOG = SessionName.SessionLog
                '    File.AppendAllText(LogPath + NE_Name + ".txt", System.DateTime.Now.ToString() + Environment.NewLine + LOG + Environment.NewLine + Environment.NewLine)
                '    GIGBtxt_TAB = Regex.Split(LOG, "\n")
                '    ParseData_TAB(GIGBtxt_TAB, _CircleName, NE_Name)
                '    ' ''Note  :- Here Disconnect sessioon and exit from ne
                '    'SessionName.SendAndWait("exit;", "#|>|<|$", "|", False)
                '    'SessionName.SendAndWait("exit;", "#|>|<|$", "|", False)
                '    '        TelNetSession.Disconnect()
                'Catch ex As Exception
                'End Try
                '    '**********************************************************************************************************************************
                '    ''Note :- Here we are going  to login in the SGSN
                '    APNdataUseforSGSN(Circle, NE_Name, Ne_type, LogPath, routerName)
                '************************************************************************************************************************************
                'SessionName.SendAndWait("fsclish", "#|>|<|$|--More--", "|", False)
                dtAPN = objSourcedb.ExecuteSQL("select distinct APN, GGSN_NAME FROM awe_paco_dcm.tblcoremap_up_apndata where GGSN_NAME='" & NE_Name.ToString() & "'")
                If (dtAPN.Rows.Count > 0) Then
                    For Each drAPN As DataRow In dtAPN.Rows
                        System.Threading.Thread.Sleep(1000)
                        APN = ""

                        APN = drAPN(0).ToString()
                        'For i As Integer = 0 To APNList.Count - 1
                        '    APNListItem = ""
                        '    APNListItem = APNList(i).ToString
                        '    If (APNListItem.Contains(APN)) Then
                        '        Exit For
                        '    Else
                        '        APNListItem = ""
                        '    End If
                        'Next
                        'If (Not (APNListItem = "")) Then
                        SessionName.SendAndWait("fsclish", "#|>|<|$|--More--", "|", False)

                        Dim ACCESS_POINT() As String = Nothing
                        LOG = ""
                        SessionName.ClearSessionLog()
                        SessionName.SendAndWait("show ng access-point " + APN, "#|>|<|$", "|", False)
                        LOG = SessionName.SessionLog
                        File.AppendAllText(LogPath + NE_Name + ".txt", System.DateTime.Now.ToString() + Environment.NewLine + LOG + Environment.NewLine + Environment.NewLine)
                        ACCESS_POINT = Regex.Split(LOG, "\n")
                        Dim SP_NAME As String = ParseData_SPname(ACCESS_POINT, _CircleName, NE_Name, LogPath)

                        If (Not (SP_NAME = "")) Then

                            Dim GIGBtxt_CMD1() As String = Nothing
                            LOG = ""
                            SessionName.ClearSessionLog()
                            CMDPrompt = SessionName.SendAndWait("show ng session-profile " & SP_NAME, "#|>|<|$|--More--", "|", False)
                            LOG = SessionName.SessionLog
                            If (CMDPrompt = 4) Then
AgainCMD:                       CMDPrompt = SessionName.SendAndWait(" ", "#|>|<|$|--More--", "|", False)
                                LOG = SessionName.SessionLog
                                If (CMDPrompt = 4) Then
                                    GoTo AgainCMD
                                End If
                            End If

                            LOG = SessionName.SessionLog
                            GIGBtxt_CMD1 = Regex.Split(LOG, "\n")
                            File.AppendAllText(LogPath + NE_Name + ".txt", System.DateTime.Now.ToString() + Environment.NewLine + LOG + Environment.NewLine + Environment.NewLine)
                            Dim CHARGING_PROFILE As String = ParseData_CMD1(GIGBtxt_CMD1, _CircleName, NE_Name, LogPath)

                            For peofile As Integer = 0 To ChargeingProfile_list.Count - 1 '' this loop use for tow charging profile


                                Dim GIGBtxt_CMD2() As String = Nothing
                                LOG = ""
                                SessionName.ClearSessionLog()
                                CMDPrompt = SessionName.SendAndWait("show ng charging charging-profile " & ChargeingProfile_list(peofile), "#|>|<|$|--More--", "|", False)
                                If (CMDPrompt = 4) Then
AgainCMD1:                          CMDPrompt = SessionName.SendAndWait(" ", "#|>|<|$|--More--", "|", False)
                                    LOG = SessionName.SessionLog
                                    If (CMDPrompt = 4) Then
                                        GoTo AgainCMD1
                                    End If
                                End If
                                LOG = SessionName.SessionLog
                                GIGBtxt_CMD2 = Regex.Split(LOG, "\n")
                                File.AppendAllText(LogPath + NE_Name + ".txt", System.DateTime.Now.ToString() + Environment.NewLine + LOG + Environment.NewLine + Environment.NewLine)
                                Dim DIAMETER_PROFILE As String = ParseData_CMD2(GIGBtxt_CMD2, _CircleName, NE_Name, LogPath)

                                TelNetSession.timeout = 1000 * 60 * 10

                                Dim GIGBtxt_CMD3() As String = Nothing
                                LOG = ""
                                SessionName.ClearSessionLog()

                                CMDPrompt = SessionName.SendAndWait("show ng diameter-profile " & DIAMETER_PROFILE.Trim, "#|>|<|$|--More--", "|", False)
                                If (CMDPrompt = 4) Then
AgainCMD2:                          CMDPrompt = SessionName.SendAndWait(" ", "#|>|<|$|--More--", "|", False)
                                    LOG = SessionName.SessionLog
                                    If (CMDPrompt = 4) Then
                                        GoTo AgainCMD2
                                    End If
                                End If

                                LOG = SessionName.SessionLog
                                GIGBtxt_CMD3 = Regex.Split(LOG, "\n")
                                File.AppendAllText(LogPath + NE_Name + ".txt", System.DateTime.Now.ToString() + Environment.NewLine + LOG + Environment.NewLine + Environment.NewLine)
                                ParseData_CMD3(GIGBtxt_CMD3, _CircleName, NE_Name, APN, LogPath) ' data add store in db
                                System.Threading.Thread.Sleep(1000)
                            Next
                            Dim dtdiameter As DataTable = Nothing
                            If _CircleName = "KOL" Then
                                dtdiameter = objSourcedb.ExecuteSQL("select DISTINCT PRI_DIAMETER,SEC_DIAMETER from tblcoremap_update Where CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG') and VENDOR='" & _vendor & "' and GGSN_Name='" & NE_Name & "' and APN='" & APN.ToString() & "'")
                            Else
                                dtdiameter = objSourcedb.ExecuteSQL("select DISTINCT PRI_DIAMETER,SEC_DIAMETER from tblcoremap_update Where CIRCLE in ('MP','MAH','MUM','GUJ') and VENDOR='" & _vendor & "' and GGSN_Name='" & NE_Name & "' and APN='" & APN.ToString() & "'")
                            End If




                            Dim PRI_DIAMETER As String = String.Empty
                            Dim SEC_DIAMETER As String = String.Empty
                            If dtdiameter.Rows.Count > 0 Then

                                For i As Integer = 0 To dtdiameter.Rows.Count - 1
                                    System.Threading.Thread.Sleep(1000)
                                    Try
                                        PRI_DIAMETER = ""
                                        SEC_DIAMETER = ""
                                        PRI_DIAMETER = IIf(IsDBNull(dtdiameter.Rows(i)("PRI_DIAMETER")), "", dtdiameter.Rows(i)("PRI_DIAMETER"))
                                        SEC_DIAMETER = IIf(IsDBNull(dtdiameter.Rows(i)("SEC_DIAMETER")), "", dtdiameter.Rows(i)("SEC_DIAMETER"))
                                        If PRI_DIAMETER <> "" And SEC_DIAMETER <> "" Then


                                            Dim GIGBtxt_CMD4() As String = Nothing
                                            Dim GIGBtxt_CMD5() As String = Nothing
                                            'Primary Server IP
                                            LOG = ""
                                            SessionName.ClearSessionLog()
                                            CMDPrompt = SessionName.SendAndWait("show ng diameter-server " & PRI_DIAMETER.Trim, "#|>|<|$|--More--", "|", False)
                                            If (CMDPrompt = 4) Then
AgainCMD3:                                      CMDPrompt = SessionName.SendAndWait(" ", "#|>|<|$|--More--", "|", False)
                                                LOG = SessionName.SessionLog
                                                If (CMDPrompt = 4) Then
                                                    GoTo AgainCMD3
                                                End If
                                            End If
                                            LOG = SessionName.SessionLog
                                            GIGBtxt_CMD4 = Regex.Split(LOG, "\n")
                                            File.AppendAllText(LogPath + NE_Name + ".txt", System.DateTime.Now.ToString() + Environment.NewLine + LOG + Environment.NewLine + Environment.NewLine)
                                            ParseData_CMD4(GIGBtxt_CMD4, _CircleName, NE_Name, PRI_DIAMETER.Trim, APN.ToString(), LogPath)


                                            'Secondry Server IP
                                            LOG = ""
                                            SessionName.ClearSessionLog()
                                            CMDPrompt = SessionName.SendAndWait("show ng diameter-server " & SEC_DIAMETER.Trim, "#|>|<|$|--More--", "|", False)
                                            If (CMDPrompt = 4) Then
AgainCMD4:                                      CMDPrompt = SessionName.SendAndWait(" ", "#|>|<|$|--More--", "|", False)
                                                LOG = SessionName.SessionLog
                                                If (CMDPrompt = 4) Then
                                                    GoTo AgainCMD4
                                                End If
                                            End If
                                            LOG = SessionName.SessionLog
                                            GIGBtxt_CMD5 = Regex.Split(LOG, "\n")
                                            File.AppendAllText(LogPath + NE_Name + ".txt", System.DateTime.Now.ToString() + Environment.NewLine + LOG + Environment.NewLine + Environment.NewLine)
                                            ParseData_CMD5(GIGBtxt_CMD5, _CircleName, NE_Name, SEC_DIAMETER.Trim, APN.ToString(), LogPath)
                                        End If

                                    Catch ex As Exception
                                        File.AppendAllText(LogPath + NE_Name + ".txt", System.DateTime.Now.ToString() + Environment.NewLine + "COMMAND LABLE PROBLEM  " + ex.Message + Environment.NewLine + Environment.NewLine)
                                        objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set ERRORMSG='COMMAND LABLE PROBLEM' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and Node_Type='" + _NEType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
                                    End Try
                                Next
                            End If
                            'End If
                        End If
                    Next
                End If
                SessionName.SendAndWait("exit", "#|>|<|$", "|", False)
                SessionName.SendAndWait("exit", "#|>|<|$", "|", False) ''eixt from NE
                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  REMARKS='OK' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and Node_Type='" + _NEType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
            Catch ex As Exception
                SessionName.SendMessage(Char.ConvertFromUtf32(25), True)
                File.AppendAllText(LogPath + NE_Name + ".txt", System.DateTime.Now.ToString() + Environment.NewLine + "COMMAND LABLE PROBLEM  " + SessionName.SessionLog + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine)
                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set ERRORMSG='COMMAND LABLE PROBLEM' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and Node_Type='" + _NEType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
            End Try
        End Function
        '        Public Function SGSNfunctioning(ByRef SessionName As ScriptingSSH.ScriptingSSH, ByVal Circle As String, ByVal NE_Name As String, ByVal NEPROMPT As String, ByVal Ne_type As String, ByVal LogPath As String, ByVal routerName As String)
        '            Dim MCC_MNC_DATA As String = String.Empty
        '            Dim MCC As String = String.Empty
        '            Dim MNC As String = String.Empty
        '            Dim count As Integer = 0
        '            Try



        '                '*************************Start Fetching data using Command: ZEJL:NSEI=0&&65535:;*******************************************
        '                Dim GIGBtxt_ZEJL() As String = Nothing
        '                LOG = ""
        '                SessionName.ClearSessionLog()
        '                SessionName.timeout = 360
        '                SessionName.SendAndWait("ZEJL:NSEI=0&&65535:;", NEPROMPT)
        '                LOG = SessionName.SessionLog
        '                File.AppendAllText(LogPath + NE_Name + ".txt", LOG + Environment.NewLine)
        '                If (LOG.Contains("/*** YOUR PASSWORD HAS EXPIRED ***/") Or (LOG.Contains("/*** COMMAND NOT AUTHORIZED ***/"))) Then
        '                    objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  ERRORMSG='YOUR PASSWORD HAS EXPIRED' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and Node_Type='" + _NEType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
        '                End If
        '                GIGBtxt_ZEJL = Regex.Split(LOG, "\n")
        '                ParseData_ZEJL(GIGBtxt_ZEJL, _CircleName, NE_Name, LogPath)
        '                '************************Start Executing Command: ZFWO:NSVCI=0&&65535:;*************************************************
        '                Dim GIGBtxt_ZFWO() As String = Nothing
        '                LOG = ""
        '                SessionName.ClearSessionLog()
        '                SessionName.timeout = 360
        '                SessionName.SendAndWait("ZFWO:NSVCI=0&&65535:;", NEPROMPT)
        '                LOG = SessionName.SessionLog
        '                File.AppendAllText(LogPath + NE_Name + ".txt", LOG + Environment.NewLine)
        '                GIGBtxt_ZFWO = Regex.Split(LOG, "\n")
        '                ParseData_ZFWO(GIGBtxt_ZFWO, _CircleName, NE_Name, LogPath)
        '                '************************Start Executing Command: ZFUI:;*************************************************
        '                Dim GIGBtxt_ZFUI() As String = Nothing
        '                LOG = ""
        '                SessionName.ClearSessionLog()
        '                SessionName.SendAndWait("ZFUI:;", NEPROMPT)
        '                LOG = SessionName.SessionLog
        '                File.AppendAllText(LogPath + NE_Name + ".txt", LOG + Environment.NewLine)
        '                GIGBtxt_ZFUI = Regex.Split(LOG, "\n")
        '                ParseData_ZFUI(GIGBtxt_ZFUI, _CircleName, NE_Name, LogPath)
        '                '************************Start Executing Command: ZKAI:;*************************************************
        '                '' NOTE:- Command use for SGSN and GGSN
        '                Dim GIGBtxt_ZKAI() As String = Nothing
        '                LOG = ""
        '                SessionName.ClearSessionLog()
        '                SessionName.SendAndWait("ZKAI:;", NEPROMPT)
        '                LOG = SessionName.SessionLog
        '                File.AppendAllText(LogPath + NE_Name + ".txt", LOG + Environment.NewLine)
        '                GIGBtxt_ZKAI = Regex.Split(LOG, "\n")
        '                ParseData_ZKAI(GIGBtxt_ZKAI, _CircleName, NE_Name, LogPath)
        '                '************************************************************************************************************
        '                Dim GIGBtxt_ZE6I() As String = Nothing
        '                LOG = ""
        '                SessionName.ClearSessionLog()
        '                SessionName.SendAndWait("ZE6I:;", NEPROMPT)
        '                LOG = SessionName.SessionLog
        '                File.AppendAllText(LogPath + NE_Name + ".txt", LOG + Environment.NewLine)
        '                GIGBtxt_ZE6I = Regex.Split(LOG, "\n")
        '                ParseData_ZE6I(GIGBtxt_ZE6I, _CircleName, NE_Name, LogPath)
        '                '***********************Use for GGSN ****************************************************************************
        '                'Primary DNS IP and Secondry DNS IP data updated for GGSN in tblCoremap table
        '                'NOTE:- Command use for GGSN
        '                Dim GIGBtxt_ZQRJ() As String = Nothing
        '                LOG = ""
        '                SessionName.ClearSessionLog()
        '                SessionName.SendAndWait("ZQRJ:;", NEPROMPT)
        '                LOG = SessionName.SessionLog
        '                File.AppendAllText(LogPath + NE_Name + ".txt", LOG + Environment.NewLine)
        '                GIGBtxt_ZQRJ = Regex.Split(LOG, "\n")
        '                ParseData_ZQRJ(GIGBtxt_ZQRJ, _CircleName, NE_Name, LogPath)


        '                Dim GIGBtxt_ZWVI() As String = Nothing
        '                LOG = ""
        '                SessionName.ClearSessionLog()
        '                SessionName.SendAndWait("ZWVI::;", NEPROMPT) 'NOTE:- Command use for GGSN 
        '                LOG = SessionName.SessionLog
        '                File.AppendAllText(LogPath + NE_Name + ".txt", LOG + Environment.NewLine)
        '                GIGBtxt_ZWVI = Regex.Split(LOG, "\n")
        '                MCC_MNC_DATA = ParseData_ZWVI(GIGBtxt_ZWVI, _CircleName, NE_Name, LogPath)

        '                Dim FindMCCMNC() As String = Nothing
        '                FindMCCMNC = Regex.Split(MCC_MNC_DATA, "MANISH")
        '                '************************Start Executing Command: ZQRX*************************************************ZZQRX:PAPU,0:IOCPE,3:PING:HOST="AIRTELFUN.COM.MNC031.MCC404.GPRS",;
        '                'NOTE:- Command use for GGSN and data insert into tblcoremap_up_apndata table

        '                Dim dtcomp As DataTable = Nothing
        '                Dim DtAPN As DataTable = Nothing
        '                Dim PAPU_INDEX As String = String.Empty
        '                Dim ZQRX_COMMAND As String = String.Empty
        '                Dim COMP_UNIT As String = String.Empty
        '                Dim COMP_UNIT_GBU As String = String.Empty
        '                Dim IOCPE_FIX_VAL As Integer = 2
        '                Dim APN As String = String.Empty
        '                Dim GGSNcircle As String = String.Empty
        '                Dim GGSN_NE_NAME As String = String.Empty
        '                MCC = FindMCCMNC(0).Trim
        '                MNC = FindMCCMNC(1).Trim
        '                count = 0
        '                'Note :- Here working for APN And GGSN Name
        '                Dim dtGGSN As DataTable = Nothing
        '                dtGGSN = objSourcedb.ExecuteSQL("SELECT distinct circle FROM awe_paco_dcm.apn_detail")
        '                For Each drcircle As DataRow In dtGGSN.Rows

        '                    GGSNcircle = drcircle(0).ToString()
        '                    If (GGSNcircle = "MUM") Then
        '                        If ((_CircleName = "MUM") Or (_CircleName = "MP") Or (_CircleName = "MAH") Or (_CircleName = "GUJ")) Then
        '                            DtAPN = objSourcedb.ExecuteSQL("SELECT * FROM awe_paco_dcm.apn_detail where circle='" & GGSNcircle.ToString() & "'")
        '                            Exit For
        '                        End If
        '                    End If
        '                    If (GGSNcircle = "KOL") Then
        '                        If ((_CircleName = "KOL") Or (_CircleName = "ORI") Or (_CircleName = "BIHAR") Or (_CircleName = "WB") Or (_CircleName = "ASSAM") Or _CircleName = "SHILONG") Then
        '                            DtAPN = objSourcedb.ExecuteSQL("SELECT * FROM awe_paco_dcm.apn_detail where circle='" & GGSNcircle.ToString() & "'")
        '                            Exit For
        '                        End If
        '                    End If
        '                Next

        '                '********************************************************************************************************************************************************************
        '                '' ( ATAKA NE names--------------------------------------------------------------------------------------------------------------------------------------------------------)
        '                If ((Not ((NE_Name = "NS_ASSAM") And (_CircleName = "ASSAM"))) And (Not ((NE_Name = "SGSN04" Or NE_Name = "SGSN05") And (_CircleName = "MAH"))) And (Not ((NE_Name = "SGSN4_RJT") And (_CircleName = "GUJ"))) And (Not ((NE_Name = "SGN5_901235") And (_CircleName = "BIHAR"))) And (Not ((NE_Name = "SGSN4") And (_CircleName = "MP"))) And (Not ((NE_Name = "SGSN2MUM") And (_CircleName = "MUM"))) And (Not ((NE_Name = "FINS_ROWB_1") And (_CircleName = "WB")))) Then
        '                    'DtAPN = objSourcedb.ExecuteSQL("SELECT * FROM awe_paco_dcm.apn_detail where circle='" & GGSNcircle.ToString() & "' order by GGSN_NAME asc ")

        '                    If (DtAPN.Rows.Count > 0) Then
        '                        For Each drAPN As DataRow In DtAPN.Rows
        '                            APN = ""
        '                            GGSN_NE_NAME = ""
        '                            System.Threading.Thread.Sleep(1000)
        '                            APN = drAPN(0).ToString.Trim
        '                            'For a As Integer = 0 To APNList.Count - 1
        '                            'If (APN = APNList(a).ToString()) Then
        '                            dtcomp = Nothing
        '                            dtcomp = objSourcedb.ExecuteSQL("SELECT COMP_UNIT,CAST(SUBSTRING_INDEX(COMP_UNIT,'-','-1') AS UNSIGNED) AS PAPU_INDEX FROM awe_paco_dcm.tblcoremap where CIRCLE='" & _CircleName & "' AND NE_NAME='" & NE_Name & "' AND VENDOR='" & _vendor & "' order by COMP_UNIT asc")

        '                            If (dtcomp.Rows.Count > 0) Then
        '                                For i As Integer = 0 To dtcomp.Rows.Count - 1
        '                                    System.Threading.Thread.Sleep(1000)
        '                                    COMP_UNIT_GBU = IIf(IsDBNull(dtcomp.Rows(i)("COMP_UNIT")), "", dtcomp.Rows(i)("COMP_UNIT")).ToString()
        '                                    If (COMP_UNIT_GBU.Contains("PAPU")) Then
        '                                        Dim GIGBtxt_ZQRX() As String = Nothing
        '                                        LOG = ""
        '                                        ZQRX_COMMAND = ""
        '                                        PAPU_INDEX = ""
        '                                        COMP_UNIT = ""
        '                                        SessionName.ClearSessionLog()
        '                                        PAPU_INDEX = IIf(IsDBNull(dtcomp.Rows(i)("PAPU_INDEX")), "", dtcomp.Rows(i)("PAPU_INDEX")).ToString()
        '                                        COMP_UNIT = IIf(IsDBNull(dtcomp.Rows(i)("COMP_UNIT")), "", dtcomp.Rows(i)("COMP_UNIT")).ToString()
        '                                        ZQRX_COMMAND = GetCommand(_CircleName, NE_Name, PAPU_INDEX, IOCPE_FIX_VAL.ToString(), MCC, MNC, APN, LogPath)

        '                                        Try
        'ZQRX:                                       count += 1
        '                                            SessionName.timeout = 60
        '                                            SessionName.SendAndWait(ZQRX_COMMAND, NEPROMPT)
        '                                            LOG = SessionName.SessionLog
        '                                            File.AppendAllText(LogPath + NE_Name + ".txt", LOG + Environment.NewLine)
        '                                            If (LOG.Contains("COMMAND EXECUTED")) Then

        '                                                GIGBtxt_ZQRX = Regex.Split(LOG, "\n")
        '                                                ParseData_ZQRX(GIGBtxt_ZQRX, _CircleName, NE_Name, COMP_UNIT, APN, GGSN_NE_NAME, LogPath)
        '                                                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  ERRORMSG='' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and Node_Type='" + _NEType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
        '                                            End If
        '                                        Catch ex As Exception
        '                                            objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  ERRORMSG='COMMAND LABLE PROBLEM(NON ATKA-ZQRX:PAPU)' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and Node_Type='" + _NEType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
        '                                            SessionName.SendMessage(Char.ConvertFromUtf32(25), True)
        '                                            System.Threading.Thread.Sleep(100)
        '                                            File.AppendAllText(LogPath + NE_Name + ".txt", SessionName.SessionLog + Environment.NewLine + ex.Message + Environment.NewLine)
        '                                            ZQRX_COMMAND = ""
        '                                            SessionName.ClearSessionLog()
        '                                            If (count > 2) Then
        '                                                System.Threading.Thread.Sleep(2000)
        '                                                GoTo NextAPN
        '                                            End If
        '                                            If (count < 2) Then
        '                                                IOCPE_FIX_VAL += 1
        '                                                ZQRX_COMMAND = GetCommand(_CircleName, NE_Name, PAPU_INDEX, IOCPE_FIX_VAL.ToString(), MCC, MNC, APN, LogPath)
        '                                                System.Threading.Thread.Sleep(2000)
        '                                                GoTo ZQRX
        '                                            End If
        '                                        End Try
        '                                    End If
        '                                Next
        '                            End If
        '                            'End If
        '                            'Next
        '                            System.Threading.Thread.Sleep(2000)
        'NextAPN:                Next
        '                    End If
        '                Else ' Here Use Atka(GBU)

        '                    'DtAPN = objSourcedb.ExecuteSQL("SELECT * FROM awe_paco_dcm.apn_detail where circle='" & GGSNcircle.ToString() & "' order by GGSN_NAME asc ")
        '                    If (DtAPN.Rows.Count > 0) Then
        '                        For Each drAPN As DataRow In DtAPN.Rows
        '                            APN = ""
        '                            GGSN_NE_NAME = ""
        '                            APN = drAPN(0).ToString.Trim
        '                            'GGSN_NE_NAME = drAPN(1).ToString

        '                            'For a As Integer = 0 To APNList.Count - 1
        '                            '    If (APN = APNList(a).ToString()) Then

        '                            dtcomp = objSourcedb.ExecuteSQL("SELECT COMP_UNIT,CAST(SUBSTRING_INDEX(COMP_UNIT,'-','-1') AS UNSIGNED) AS PAPU_INDEX FROM awe_paco_dcm.tblcoremap where CIRCLE='" & _CircleName & "' AND NE_NAME='" & NE_Name & "' AND VENDOR='" & _vendor & "' order by COMP_UNIT asc")

        '                            If (dtcomp.Rows.Count > 0) Then
        '                                For i As Integer = 0 To dtcomp.Rows.Count - 1

        '                                    Dim GIGBtxt_ZQRX() As String = Nothing
        '                                    LOG = ""
        '                                    ZQRX_COMMAND = ""
        '                                    PAPU_INDEX = ""
        '                                    COMP_UNIT = ""
        '                                    SessionName.ClearSessionLog()
        '                                    PAPU_INDEX = IIf(IsDBNull(dtcomp.Rows(i)("PAPU_INDEX")), "", dtcomp.Rows(i)("PAPU_INDEX")).ToString()
        '                                    COMP_UNIT = IIf(IsDBNull(dtcomp.Rows(i)("COMP_UNIT")), "", dtcomp.Rows(i)("COMP_UNIT")).ToString()
        '                                    ZQRX_COMMAND = ATKAGetCommand(_CircleName, NE_Name, PAPU_INDEX, IOCPE_FIX_VAL.ToString(), MCC, MNC, APN, LogPath)

        '                                    Try
        '                                        SessionName.timeout = 60
        '                                        SessionName.SendAndWait(ZQRX_COMMAND, "<|>|#|$", "|", False)
        '                                        LOG = SessionName.SessionLog
        '                                        File.AppendAllText(LogPath + NE_Name + ".txt", LOG + Environment.NewLine)
        '                                        If (LOG.Contains("COMMAND EXECUTED")) Then

        '                                            GIGBtxt_ZQRX = Regex.Split(LOG, "\n")
        '                                            ParseData_ZQRX(GIGBtxt_ZQRX, _CircleName, NE_Name, COMP_UNIT, APN, GGSN_NE_NAME, LogPath)
        '                                            objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  ERRORMSG='' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and Node_Type='" + _NEType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
        '                                        End If
        '                                    Catch ex As Exception
        '                                        SessionName.SendMessage(Char.ConvertFromUtf32(25), True)
        '                                        System.Threading.Thread.Sleep(100)
        '                                        File.AppendAllText(LogPath + NE_Name + ".txt", SessionName.SessionLog + Environment.NewLine + ex.Message + Environment.NewLine)
        '                                        ZQRX_COMMAND = ""
        '                                        SessionName.ClearSessionLog()
        '                                        objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  ERRORMSG='COMMAND LABLE PROBLEM(ATKA-ZQRX:PAPU)' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and Node_Type='" + _NEType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
        '                                        System.Threading.Thread.Sleep(2000)
        '                                        GoTo NextAPNGBU

        '                                    End Try

        '                                Next
        '                            End If
        '                            'End If
        '                            'Next
        'NextAPNGBU:             Next

        '                    End If

        '                End If

        '                '**************************************************************************************************************************************************

        '            Catch ex As Exception
        '                File.AppendAllText(LogPath + NE_Name + ".txt", System.DateTime.Now.ToString() + Environment.NewLine + "COMMAND LABLE PROBLEM  " + ex.Message + Environment.NewLine + Environment.NewLine)
        '                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.ne_master set  ERRORMSG='COMMAND LABLE PROBLEM' where Circle='" + _CircleName + "' and NE_Name='" + NE_Name + "' and Node_Type='" + _NEType + "' and Domain='PS' and Rtr_Name='" + routerName + "' and NE_ActiveFlag is true")
        '            End Try

        '        End Function


        Public Function ParseData_SPname(ByVal ACCESS_POINT() As String, ByVal circle_name As String, ByVal element_name As String, ByVal errorPath As String)
            Dim SP_Name As String = String.Empty
            Dim finding() As String = Nothing
            Try

                For i As Integer = 0 To ACCESS_POINT.Length - 1
                    If (ACCESS_POINT(i).ToString().Contains("session-profile-name")) Then
                        finding = ACCESS_POINT(i).Split("=")
                        SP_Name = finding(1).ToString.Trim

                    End If

                Next
                Return SP_Name
            Catch ex As Exception
                File.AppendAllText(errorPath + "parsingError.txt", System.DateTime.Now.ToString() + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine)
            End Try

        End Function


        Public Function IMSILISTData(ByVal catBhartilookup() As String, ByVal circle_name As String, ByVal element_name As String, ByVal privacy As String, ByVal errorPath As String) As Boolean
            Dim IMSIipAddress As String = String.Empty
            Dim IPAddressName As String = String.Empty
            Dim IMSIipAddressUNIC As String = String.Empty
            Dim IMSI As String = String.Empty
            Dim FindResult() As String = Nothing
            Dim FindResult1() As String = Nothing
            Dim EC As String = String.Empty
            Dim IMSIfind() As String = Nothing
            Dim ECvalue As String = String.Empty
            Dim dtEC As DataTable = Nothing

            Try

                For m As Integer = 0 To catBhartilookup.Length - 1

                    If (catBhartilookup(m).Contains("Bharti_LookupData_")) Then
                        FindResult = Split(catBhartilookup(m), "_")
                        FindResult1 = Split(FindResult(2), ".")
                        EC = FindResult1(0).ToUpper().ToString().Trim()
                        FindResult = Nothing
                        FindResult1 = Nothing
                        'If privacy = "UseforPRIvalue" Then
                        '    dtEC = objSourcedb.ExecuteSQL("select distinct EC_DETAIL FROM awe_paco_dcm.tblcoremap_update where EC_DETAIL like '%" + EC + "' and CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG') and vendor='" & _vendor & "'")
                        'Else
                        '    dtEC = objSourcedb.ExecuteSQL("select distinct EC_DETAIL FROM awe_paco_dcm.tblcoremap_update where EC_DETAIL like '%" + EC + "' and CIRCLE in ('MP','MAH','MUM','GUJ') and vendor='" & _vendor & "'")
                        'End If
                        If EC <> "" Then
                            If _CircleName = "KOL" Then
                                If privacy = "UseforPRIvalue" Then
                                    dtEC = objSourcedb.ExecuteSQL("select distinct EC_DETAIL FROM awe_paco_dcm.tblcoremap_update where EC_DETAIL like '%" + element_name + "_" + EC + "' and CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG') and vendor='" & _vendor & "'")
                                ElseIf privacy = "UseforSECvalue" Then
                                    dtEC = objSourcedb.ExecuteSQL("select distinct SEC_EC FROM awe_paco_dcm.tblcoremap_update where SEC_EC like '%" + element_name + "_" + EC + "' and CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG') and vendor='" & _vendor & "'")
                                End If
                            Else
                                If privacy = "UseforPRIvalue" Then
                                    dtEC = objSourcedb.ExecuteSQL("select distinct EC_DETAIL FROM awe_paco_dcm.tblcoremap_update where EC_DETAIL like '%" + element_name + "_" + EC + "' and CIRCLE in ('MP','MAH','MUM','GUJ') and vendor='" & _vendor & "'")
                                ElseIf privacy = "UseforSECvalue" Then
                                    dtEC = objSourcedb.ExecuteSQL("select distinct SEC_EC FROM awe_paco_dcm.tblcoremap_update where SEC_EC like '%" + element_name + "_" + EC + "' and CIRCLE in ('MP','MAH','MUM','GUJ') and vendor='" & _vendor & "'")
                                End If
                            End If
                        End If

                        ECvalue = dtEC.Rows(0)(0).ToString()
                    End If


                    If catBhartilookup(m).IndexOf("IMSI IPAddress") <> -1 Then
                        FindResult = Split(catBhartilookup(m), ">")
                        '----------------------------------------------MISI data----------------------------
                        'IMSI = (FindResult(1).Substring(0, FindResult(1).LastIndexOf("</"))) / 10
                        'If (IMSI.Contains(".")) Then
                        '    IMSIfind = Nothing
                        '    IMSIfind = IMSI.Split(".")
                        '    IMSI = IMSIfind(0).ToString.Trim
                        'End If

                        IMSI = FindResult(1).Substring(0, FindResult(1).LastIndexOf("</"))


                        '-------------------------------------------------------------------------------------
                        FindResult1 = Split(FindResult(0), " ")
                        For k As Integer = 0 To FindResult1.Length - 1
                            If FindResult1(k).Contains("IPAddress=") Then
                                IMSIipAddress = FindResult1(k).Trim.Substring(11, FindResult1(k).Length - 12)
                                ' Dim IMSIIPAddressSplit() As String = IMSIipAddress.Split(":")
                                ' IMSIipAddressUNIC = IMSIIPAddressSplit(0)
                            ElseIf FindResult1(k).Contains("IPAddressName=") Then
                                IPAddressName = FindResult1(k).Trim.Substring(15, FindResult1(k).Length - 16)
                            End If
                        Next
                        If IMSI <> "" Then


                            If _CircleName = "KOL" Then
                                'If privacy = "UseforPRIvalue" Then
                                '    objSourcedb.ExecuteNonQuery("update tblcoremap_update set primary_in_name='" & IPAddressName.Trim & "',primary_in_ip='" & IMSIipAddress.Trim & "' where IMSI_MISSION='" & IMSI.Trim & "' and CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG') and vendor='" & _vendor & "'")
                                'ElseIf privacy = "UseforSECvalue" Then
                                '    objSourcedb.ExecuteNonQuery("update tblcoremap_update set secondary_in_name='" & IPAddressName.Trim & "',secondary_in_ip='" & IMSIipAddress.Trim & "' where IMSI_MISSION='" & IMSI.Trim & "' and CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG') and vendor='" & _vendor & "'")
                                'End If

                                If privacy = "UseforPRIvalue" Then
                                    objSourcedb.ExecuteNonQuery("update tblcoremap_update set primary_in_name='" & IPAddressName.Trim & "',primary_in_ip='" & IMSIipAddress.Trim & "' where EC_DETAIL='" & ECvalue.ToString & "' and CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG') and vendor='" & _vendor & "' and imsi_mission like '" + IMSI.ToString + "%" + "'")
                                ElseIf privacy = "UseforSECvalue" Then
                                    objSourcedb.ExecuteNonQuery("update tblcoremap_update set secondary_in_name='" & IPAddressName.Trim & "',secondary_in_ip='" & IMSIipAddress.Trim & "' where SEC_EC='" & ECvalue.ToString & "' and CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG') and vendor='" & _vendor & "' and imsi_mission like '" + IMSI.ToString + "%" + "'")
                                End If
                                IMSI = ""
                                IMSIipAddress = ""
                                IPAddressName = ""
                            Else
                                'If privacy = "UseforPRIvalue" Then
                                '    objSourcedb.ExecuteNonQuery("update tblcoremap_update set primary_in_name='" & IPAddressName.Trim & "',primary_in_ip='" & IMSIipAddress.Trim & "' where IMSI_MISSION='" & IMSI.Trim & "' and CIRCLE in ('MP','MAH','MUM','GUJ') and vendor='" & _vendor & "'")
                                'ElseIf privacy = "UseforSECvalue" Then
                                '    objSourcedb.ExecuteNonQuery("update tblcoremap_update set secondary_in_name='" & IPAddressName.Trim & "',secondary_in_ip='" & IMSIipAddress.Trim & "' where IMSI_MISSION='" & IMSI.Trim & "' and CIRCLE in ('MP','MAH','MUM','GUJ') and vendor='" & _vendor & "'")
                                'End If

                                If privacy = "UseforPRIvalue" Then
                                    objSourcedb.ExecuteNonQuery("update tblcoremap_update set primary_in_name='" & IPAddressName.Trim & "',primary_in_ip='" & IMSIipAddress.Trim & "' where EC_DETAIL='" & ECvalue.ToString & "' and CIRCLE in ('MP','MAH','MUM','GUJ') and vendor='" & _vendor & "' and imsi_mission like '" + IMSI.ToString + "%" + "'")
                                ElseIf privacy = "UseforSECvalue" Then
                                    objSourcedb.ExecuteNonQuery("update tblcoremap_update set secondary_in_name='" & IPAddressName.Trim & "',secondary_in_ip='" & IMSIipAddress.Trim & "' where SEC_EC='" & ECvalue.ToString & "' and CIRCLE in ('MP','MAH','MUM','GUJ') and vendor='" & _vendor & "' and imsi_mission like '" + IMSI.ToString + "%" + "'")
                                End If
                                IMSI = ""
                                IMSIipAddress = ""
                                IPAddressName = ""
                            End If

                        End If
                    End If

                Next




                Return True
            Catch ex As Exception
                File.AppendAllText(errorPath + "parsingError.txt", System.DateTime.Now.ToString() + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine)
                Return False
            End Try
        End Function
        Public Function Bhartilookupdata(ByVal Bhartilookup() As String, ByVal circle_name As String, ByVal element_name As String, ByVal errorPath As String) As Boolean
            Dim pri_ec As String = String.Empty
            Dim sec_ec As String = String.Empty
            Dim pri_ecs() As String = Nothing
            Dim sec_ecs() As String = Nothing
            Dim splitSECXmlfile() As String = Nothing
            Dim getSECec() As String = Nothing
            Dim priXmlfile As String = String.Empty
            Dim secXmlfile As String = String.Empty
            Dim splitPRIXmlfile() As String = Nothing
            Dim getPRIec() As String = Nothing
            Dim ReplacedData As String = String.Empty
            Dim dtPriEC As DataTable = Nothing
            Dim dtSecEC As DataTable = Nothing
            Dim secBharti() As String = Nothing
            Dim priBharti() As String = Nothing
            Dim SplitEC() As String = Nothing
            priXMLfilelist = Nothing
            secXMLfilelist = Nothing

            priXMLfilelist = New ArrayList
            secXMLfilelist = New ArrayList

            Try
                If _CircleName = "KOL" Then
                    'dtEC = objSourcedb.ExecuteSQL("select distinct ifnull(ec_detail,'') ec_detail,ifnull(sec_ec,'') sec_ec from tblcoremap_update where Vendor='" & _vendor & "' and CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG')  and IMSI_Mission <> '' ")
                    dtPriEC = objSourcedb.ExecuteSQL("select distinct substring_index(ec_detail,'_',-1)as ec_detail from awe_paco_dcm.tblcoremap_update where Vendor='" & _vendor & "' and CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG') and ec_detail like '" + element_name + "%" + "' and ec_detail <> ''")
                    dtSecEC = objSourcedb.ExecuteSQL("select distinct substring_index(sec_ec,'_',-1)as sec_ec from awe_paco_dcm.tblcoremap_update where Vendor='" & _vendor & "' and CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG') and sec_ec like '" + element_name + "%" + "' and sec_ec <> ''")
                Else
                    dtPriEC = objSourcedb.ExecuteSQL("select distinct substring_index(ec_detail,'_',-1)as ec_detail from awe_paco_dcm.tblcoremap_update where Vendor='" & _vendor & "' and CIRCLE in ('MP','MAH','MUM','GUJ') and ec_detail like '" + element_name + "%" + "' and ec_detail <> ''")
                    dtSecEC = objSourcedb.ExecuteSQL("select distinct substring_index(sec_ec,'_',-1)as sec_ec from awe_paco_dcm.tblcoremap_update where Vendor='" & _vendor & "' and CIRCLE in ('MP','MAH','MUM','GUJ') and sec_ec like '" + element_name + "%" + "' and sec_ec <> ''")
                    'dtEC = objSourcedb.ExecuteSQL("select distinct ifnull(ec_detail,'') ec_detail,ifnull(sec_ec,'') sec_ec from tblcoremap_update where Vendor='" & _vendor & "' and CIRCLE in ('MP','MAH','MUM','GUJ')  and IMSI_Mission <> '' ")
                End If

                For i As Integer = 0 To Bhartilookup.Length - 1
                    Dim count As Integer = 0
                    count = i + 1
                    ' If (Bhartilookup(i).Trim <> "") Then

                    If (Bhartilookup(i).Contains("Bharti_LookupData_")) Then

                        For j As Integer = 0 To dtSecEC.Rows.Count - 1
                            sec_ec = ""
                            sec_ec = dtSecEC.Rows(j)("sec_ec").ToString()
                            'Bharti_LookupData_
                            If sec_ec <> "" Then
                                If (Bhartilookup(i).ToUpper.Contains(sec_ec)) Then
                                    'If (Bhartilookup(i).Contains("ec22")) Then
                                    '    Dim aaaaaaaaaaaaa As String = String.Empty
                                    'End If
                                    'secXmlfile = Bhartilookup(i).Substring(Bhartilookup(i).LastIndexOf("Bharti_LookupData_"), 26).Trim
                                    secBharti = Regex.Split(Bhartilookup(i).Trim, "Bharti_LookupData")
                                    If (Not (secBharti(1).Contains("xml_"))) Then
                                        secXmlfile = "Bharti_LookupData" + secBharti(1)
                                        If (Not (secBharti(1).Contains("xml."))) Then
                                            splitSECXmlfile = secXmlfile.Split("_")
                                            getSECec = splitSECXmlfile(2).Split(".")
                                            If (getSECec(0) = sec_ec.ToLower()) Then
                                                If (Not (secXmlfile.Contains("bkp"))) Then
                                                    SplitEC = Nothing
                                                    SplitEC = secXmlfile.Split("_")
                                                    If (SplitEC.Length = 3) Then
                                                        secXMLfilelist.Add(secXmlfile)
                                                    End If
                                                End If
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        Next

                        For k As Integer = 0 To dtPriEC.Rows.Count - 1
                            pri_ec = ""
                            pri_ec = dtPriEC.Rows(k)("ec_detail").ToString()
                            If pri_ec <> "" Then
                                If (Bhartilookup(i).ToUpper.Contains(pri_ec)) Then
                                    'priXmlfile = Bhartilookup(i).Substring(Bhartilookup(i).LastIndexOf("Bharti_LookupData_"), 29).Trim
                                    priBharti = Regex.Split(Bhartilookup(i), "Bharti_LookupData")
                                    If (Not (priBharti(1).Contains("xml_"))) Then
                                        priXmlfile = "Bharti_LookupData" + priBharti(1)
                                        If (Not (priBharti(1).Contains("xml."))) Then
                                            splitPRIXmlfile = priXmlfile.Split("_")
                                            getPRIec = splitPRIXmlfile(2).Split(".")
                                            If (getPRIec(0) = pri_ec.ToLower()) Then
                                                If (Not (priXmlfile.Contains("bkp"))) Then
                                                    SplitEC = Nothing
                                                    SplitEC = priXmlfile.Split("_")
                                                    If (SplitEC.Length = 3) Then
                                                        priXMLfilelist.Add(priXmlfile)
                                                    End If
                                                End If
                                            End If
                                        End If
                                    End If
                                End If
                            End If

                        Next
                    End If
                    ' End If
                Next
                Return True
            Catch ex As Exception
                File.AppendAllText(errorPath + "parsingError.txt", System.DateTime.Now.ToString() + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine)

                Return False
            End Try

        End Function



        Public Function CATETCHOSTCMD(ByVal CATetchost() As String, ByVal circle_name As String, ByVal element_name As String, ByVal errorPath As String) As Boolean

            Dim IPandEC As String = String.Empty
            Dim PRI_EC As String = String.Empty
            Dim SEC_EC As String = String.Empty
            Dim pri_ip As String = String.Empty
            Dim sec_ip As String = String.Empty
            Dim PRI_ECwithNEname As String = String.Empty
            Dim SEC_ECwithNEname As String = String.Empty
            Dim findEC() As String = Nothing
            Dim dt As DataTable = Nothing
            Dim dtip As DataTable = Nothing
            Try
                'Dim dtip As DataTable = objSourcedb.ExecuteSQL("select  distinct PRI_SERVER_IP, SEC_SERVER_IP from tblcoremap where circle='" & _CircleName & "'  and Vendor='" & _vendor & "' and pri_server_ip <> '';")
                If _CircleName = "KOL" Then
                    dtip = objSourcedb.ExecuteSQL("select  distinct PRI_SERVER_IP, SEC_SERVER_IP from tblcoremap_update where Vendor='" & _vendor & "' and CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG')  and pri_server_ip <> '' ")
                Else
                    dtip = objSourcedb.ExecuteSQL("select  distinct PRI_SERVER_IP, SEC_SERVER_IP from tblcoremap_update where Vendor='" & _vendor & "' and CIRCLE in ('MP','MAH','MUM','GUJ')  and pri_server_ip <> '' ")
                End If


                For i As Integer = 0 To CATetchost.Length - 1
                    ' oFileLog.WriteDataToFile(CATetchost(i), circle_name, element_name, "CMD")
                    If CATetchost(i).Trim <> "" Then
                        If CATetchost(i).ToString().ToUpper().Contains("EC") And (Not CATetchost(i).Contains("ec")) Then

                            If (dtip.Rows.Count > 0) Then
                                For j As Integer = 0 To dtip.Rows.Count - 1

                                    pri_ip = IIf(IsDBNull(dtip.Rows(j)("PRI_SERVER_IP")), "", dtip.Rows(j)("PRI_SERVER_IP"))
                                    sec_ip = IIf(IsDBNull(dtip.Rows(j)("SEC_SERVER_IP")), "", dtip.Rows(j)("SEC_SERVER_IP"))
                                    If (Not (pri_ip = "")) Then
                                        If (CATetchost(i).Contains(pri_ip)) Then
                                            PRI_EC = CATetchost(i).Substring(CATetchost(i).LastIndexOf("EC"), 4).Trim
                                            PRI_ECwithNEname = element_name + "_" + PRI_EC
                                            If _CircleName = "KOL" Then
                                                'objSourcedb.ExecuteNonQuery("update tblcoremap_update set EC_DETAIL='" & PRI_ECwithNEname & "' where pri_server_ip='" & pri_ip & "' and circle='" & _CircleName & "'")
                                                objSourcedb.ExecuteNonQuery("update tblcoremap_update set EC_DETAIL='" & PRI_ECwithNEname & "' where pri_server_ip='" & pri_ip & "' and CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG') ")
                                            Else
                                                objSourcedb.ExecuteNonQuery("update tblcoremap_update set EC_DETAIL='" & PRI_ECwithNEname & "' where pri_server_ip='" & pri_ip & "' and CIRCLE in ('MP','MAH','MUM','GUJ') ")
                                            End If
                                        End If
                                    End If
                                    If (Not (sec_ip = "")) Then
                                        If (CATetchost(i).Contains(sec_ip)) Then
                                            SEC_EC = CATetchost(i).Substring(CATetchost(i).LastIndexOf("EC"), 4).Trim
                                            SEC_ECwithNEname = element_name + "_" + SEC_EC
                                            If _CircleName = "KOL" Then
                                                'objSourcedb.ExecuteNonQuery("update tblcoremap_update set SEC_EC='" & SEC_ECwithNEname & "' where sec_server_ip='" & sec_ip & "' and circle='" & _CircleName & "'")
                                                objSourcedb.ExecuteNonQuery("update tblcoremap_update set SEC_EC='" & SEC_ECwithNEname & "' where sec_server_ip='" & sec_ip & "' and CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG') ")
                                            Else
                                                objSourcedb.ExecuteNonQuery("update tblcoremap_update set SEC_EC='" & SEC_ECwithNEname & "' where sec_server_ip='" & sec_ip & "' and CIRCLE in ('MP','MAH','MUM','GUJ') ")
                                            End If
                                        End If
                                    End If
                                Next
                            End If
                        End If
                    End If
                Next
                Return True
            Catch ex As Exception
                File.AppendAllText(errorPath + "paesing.txt", System.DateTime.Now.ToString() + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine)
                Return False
            End Try
        End Function


        Public Function ParseData_TAB(ByVal GIGBTAB() As String, ByVal Circle_Name As String, ByVal Element_Name As String)
            Dim findRuselt() As String = Nothing
            Dim findRuseltSP() As String = Nothing
            Dim APN As String = String.Empty
            Dim Flag As Boolean = False
            APNList = Nothing ' this is a ArrayList
            APNList = New ArrayList
            Try

                If (GIGBTAB.Length > 0) Then
                    For i As Integer = 0 To GIGBTAB.Length
                        APN = ""
                        findRuselt = Nothing
                        findRuseltSP = Nothing
                        Flag = False
                        If (GIGBTAB(i).Contains("[X]")) Then
                            findRuselt = GIGBTAB(i).Split(New String() {" "}, StringSplitOptions.RemoveEmptyEntries)
                            If (findRuselt.Length > 3) Then
                                If (findRuselt(1).Contains("sp-")) Then
                                    'findRuseltSP = Regex.Split(findRuselt(1), "sp-")
                                    'APN = findRuseltSP(1).ToString().Trim
                                    APN = findRuselt(1).ToString().Trim

                                    If (APN.ToLower.Contains("test")) Then
                                        Flag = False
                                    Else
                                        Flag = True
                                    End If
                                Else
                                    APN = findRuselt(1).ToString().Trim
                                    If (APN.ToLower.Contains("test")) Then
                                        Flag = False
                                    Else
                                        Flag = True
                                    End If
                                End If
                                If (Flag = True) Then
                                    APNList.Add(APN)
                                    ' objSourcedb.ExecuteNonQuery("insert into awe_paco_dcm.apn_detail(APN_NAME,GGSN_NAME,CIRCLE) values('" + APN.ToString() + "','" + Element_Name.ToString() + "','" + Circle_Name.ToString() + "') ")
                                End If
                            End If
                        End If
                    Next

                End If
            Catch ex As Exception

            End Try

        End Function

        Public Function ParseData_CMD5(ByVal GIGBtxt() As String, ByVal Circle_Name As String, ByVal Element_Name As String, ByVal sec_diameter As String, ByVal APN As String, ByVal errorPath As String) As Boolean
            Dim Vendor As String = _vendor
            Dim SEC_SERVER_IP As String = String.Empty
            Dim updateflag As Boolean = False
            Dim FindResult() As String = Nothing
            Try
                For m As Integer = 0 To GIGBtxt.Length - 1

                    'oFileLog.WriteDataToFile(GIGBtxt(m), Circle_Name, Element_Name, "CMD5")
                    If GIGBtxt(m).Trim <> "" Then
                        If GIGBtxt(m).IndexOf("server-ip-address") <> -1 Then
                            FindResult = Nothing
                            FindResult = Split(GIGBtxt(m), " ")
                            SEC_SERVER_IP = FindResult(2).Trim
                            If _CircleName = "KOL" Then
                                objSourcedb.ExecuteNonQuery("Update tblcoremap_update Set SEC_SERVER_IP='" & SEC_SERVER_IP & "' Where VENDOR='" & Vendor & "' and CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG')  and GGSN_NAME='" & Element_Name & "' and SEC_DIAMETER='" & sec_diameter & "' and APN='" & APN.ToString() & "'")
                            Else
                                objSourcedb.ExecuteNonQuery("Update tblcoremap_update Set SEC_SERVER_IP='" & SEC_SERVER_IP & "' Where VENDOR='" & Vendor & "' and CIRCLE in ('MP','MAH','MUM','GUJ')  and GGSN_NAME='" & Element_Name & "' and SEC_DIAMETER='" & sec_diameter & "' and APN='" & APN.ToString() & "'")
                            End If

                            Exit For
                        End If
                    End If
                Next
            Catch ex As Exception
                File.AppendAllText(errorPath + "parsingError.txt", System.DateTime.Now.ToString() + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine)
            End Try
        End Function
        Public Function ParseData_CMD4(ByVal GIGBtxt() As String, ByVal Circle_Name As String, ByVal Element_Name As String, ByVal pri_diameter As String, ByVal APN As String, ByVal errorPath As String) As Boolean
            Dim Vendor As String = _vendor
            Dim PRI_SERVER_IP As String = String.Empty
            Dim updateflag As Boolean = False
            Dim FindResult() As String = Nothing
            Try
                For m As Integer = 0 To GIGBtxt.Length - 1

                    'oFileLog.WriteDataToFile(GIGBtxt(m), Circle_Name, Element_Name, "CMD4")
                    If GIGBtxt(m).Trim <> "" Then
                        If GIGBtxt(m).IndexOf("server-ip-address") <> -1 Then
                            FindResult = Nothing
                            FindResult = Split(GIGBtxt(m), " ")
                            PRI_SERVER_IP = FindResult(2).Trim
                            If _CircleName = "KOL" Then
                                objSourcedb.ExecuteNonQuery("Update tblcoremap_update Set PRI_SERVER_IP='" & PRI_SERVER_IP & "' Where VENDOR='" & Vendor & "' and CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG') and GGSN_NAME='" & Element_Name & "' and PRI_DIAMETER='" & pri_diameter & "' and APN='" & APN.ToString() & "' ")
                            ElseIf _CircleName = "MUM" Then
                                objSourcedb.ExecuteNonQuery("Update tblcoremap_update Set PRI_SERVER_IP='" & PRI_SERVER_IP & "' Where VENDOR='" & Vendor & "' and CIRCLE in ('MP','MAH','MUM','GUJ') and GGSN_NAME='" & Element_Name & "' and PRI_DIAMETER='" & pri_diameter & "'  and APN='" & APN.ToString() & "' ")
                            End If

                            Exit For
                        End If
                    End If
                Next
            Catch ex As Exception
                File.AppendAllText(errorPath + "parsingError.txt", System.DateTime.Now.ToString() + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine)
            End Try
        End Function
        Public Function ParseData_CMD3(ByVal GIGBtxt() As String, ByVal Circle_Name As String, ByVal Element_Name As String, ByVal APNdata As String, ByVal errorPath As String) As String()
            Dim IMSI_MISSION As String = String.Empty
            Dim PRI_DIA As String = String.Empty
            Dim SEC_DIA As String = String.Empty
            'Dim RETURN_RESULT As New Hashtable
            'Dim recordList As New ArrayList()
            Dim FindResult() As String = Nothing
            Dim updateflag As Boolean = False
            Dim Vendor As String = _vendor
            Dim Circle As String
            Dim NE_Name As String
            Dim Comp_Unit As String = String.Empty
            Dim Papu_Ip As String = String.Empty
            Dim pri_dns As String = String.Empty
            Dim sec_dns As String = String.Empty
            Dim apn As String = String.Empty
            Dim BLAG_NAME As String = String.Empty
            Dim NG_NAME As String = String.Empty
            Dim ggsn_server_gw_ip As String = String.Empty
            Dim isfilegenerate As Boolean = False
            Dim k As Integer = 0
            Dim dt As DataTable
            Try
                If _CircleName = "KOL" Then
                    dt = objSourcedb.ExecuteSQL("Select VENDOR, CIRCLE,  NE_NAME, COMP_UNIT, PAPU_IP, PRIMARY_DNS, SECONDARY_DNS, APN, GGSN_SERVER_GW_IP,GGSN_BLADE,GGSN_NAME from tblcoremap_up_apndata Where CIRCLE in ('KOL','BIHAR','ORI','WB','ASSAM','SHILONG') and VENDOR='" & _vendor & "' and GGSN_NAME='" & Element_Name & "'AND APN='" & APNdata.ToString & "'")
                Else
                    dt = objSourcedb.ExecuteSQL("Select VENDOR, CIRCLE,  NE_NAME, COMP_UNIT, PAPU_IP, PRIMARY_DNS, SECONDARY_DNS, APN, GGSN_SERVER_GW_IP,GGSN_BLADE,GGSN_NAME from tblcoremap_up_apndata Where CIRCLE in ('MP','MAH','MUM','GUJ') and VENDOR='" & _vendor & "' and GGSN_NAME='" & Element_Name & "'AND APN='" & APNdata.ToString & "'")
                End If
                If dt.Rows.Count > 0 Then
                    For i As Integer = 0 To dt.Rows.Count - 1
                        System.Threading.Thread.Sleep(1000)
                        Vendor = IIf(IsDBNull(dt.Rows(i)("Vendor")), "", dt.Rows(i)("Vendor"))
                        Circle = IIf(IsDBNull(dt.Rows(i)("CIRCLE")), "", dt.Rows(i)("CIRCLE"))
                        NE_Name = IIf(IsDBNull(dt.Rows(i)("NE_NAME")), "", dt.Rows(i)("NE_NAME"))
                        Comp_Unit = IIf(IsDBNull(dt.Rows(i)("COMP_UNIT")), "", dt.Rows(i)("COMP_UNIT"))
                        Papu_Ip = IIf(IsDBNull(dt.Rows(i)("PAPU_IP")), "", dt.Rows(i)("PAPU_IP"))
                        pri_dns = IIf(IsDBNull(dt.Rows(i)("PRIMARY_DNS")), "", dt.Rows(i)("PRIMARY_DNS"))
                        sec_dns = IIf(IsDBNull(dt.Rows(i)("SECONDARY_DNS")), "", dt.Rows(i)("SECONDARY_DNS"))
                        apn = IIf(IsDBNull(dt.Rows(i)("APN")), "", dt.Rows(i)("APN"))
                        ggsn_server_gw_ip = IIf(IsDBNull(dt.Rows(i)("GGSN_SERVER_GW_IP")), "", dt.Rows(i)("GGSN_SERVER_GW_IP"))
                        BLAG_NAME = IIf(IsDBNull(dt.Rows(i)("GGSN_BLADE")), "", dt.Rows(i)("GGSN_BLADE"))
                        NG_NAME = IIf(IsDBNull(dt.Rows(i)("GGSN_NAME")), "", dt.Rows(i)("GGSN_NAME"))

                        For m As Integer = 0 To GIGBtxt.Length - 1


                            If GIGBtxt(m).Trim <> "" Then
                                If ((GIGBtxt(m).Contains("diameter-imsi-servers")) Or (GIGBtxt(m).Contains("diameter-msisdn-servers"))) Then
                                    'imsi MISSION
                                    FindResult = Nothing
                                    FindResult = Split(GIGBtxt(m), "=")
                                    IMSI_MISSION = FindResult(1).Trim

                                ElseIf ((GIGBtxt(m).Contains("primary-diameter-server")) And (Not (GIGBtxt(m).Contains("default-primary-diameter-server")))) Then
                                    FindResult = Nothing
                                    FindResult = Split(GIGBtxt(m), "=")
                                    PRI_DIA = FindResult(1).Trim

                                ElseIf ((GIGBtxt(m).Contains("secondary-diameter-server")) And (Not (GIGBtxt(m).Contains("default-secondary-diameter-server")))) Then
                                    'SEC_DIAMETER
                                    FindResult = Nothing
                                    FindResult = Split(GIGBtxt(m), "=")
                                    SEC_DIA = FindResult(1).Trim
                                    updateflag = True
                                End If

                            End If
                            If updateflag Then
                                objSourcedb.ExecuteNonQuery("Insert into tblcoremap_update(VENDOR, CIRCLE,  NE_NAME, COMP_UNIT, PAPU_IP, PRIMARY_DNS, SECONDARY_DNS, APN, GGSN_SERVER_GW_IP,GGSN_BLADE,GGSN_NAME,IMSI_MISSION ,PRI_DIAMETER, SEC_DIAMETER)" &
                                " Values ('" & Vendor & "', '" & Circle & "','" & NE_Name & "','" & Comp_Unit & "', '" & Papu_Ip & "','" & pri_dns & "','" & sec_dns & "', '" & apn & "','" & ggsn_server_gw_ip & "','" & BLAG_NAME.ToString() & "','" & NG_NAME.ToString() & "','" & IMSI_MISSION & "', '" & PRI_DIA & "','" & SEC_DIA & "')")
                                IMSI_MISSION = ""
                                PRI_DIA = ""
                                SEC_DIA = ""
                                updateflag = False
                            End If
                        Next
                    Next
                End If
                Return Nothing
            Catch ex As Exception
                File.AppendAllText(errorPath + "parsingError.txt", System.DateTime.Now.ToString() + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine)
            End Try
        End Function



        Public Function ParseData_ZE6I(ByVal GIGBtxt() As String, ByVal Circle_Name As String, ByVal Element_Name As String, ByVal errorPath As String)
            Dim MCC As String = String.Empty
            Dim MNC As String = String.Empty
            Dim STATE As String = String.Empty
            Dim PAPU_INDEX As String = String.Empty
            Dim MCC_code() As String = Nothing
            Dim MNC_code() As String = Nothing
            Dim RNC_State() As String = Nothing
            Dim papu() As String = Nothing
            Dim NeNmae As String = String.Empty
            Dim Flag As String = True
            Dim DT As DataTable = Nothing
            Dim RNCFind() As String = Nothing
            Dim RNC_NAME As String = String.Empty
            Dim PAPU_Length As String = String.Empty
            Try

                For i As Integer = 0 To GIGBtxt.Length - 1
                    If (GIGBtxt(i).Trim <> "") Then
                        If (GIGBtxt(i).Contains("SGSN")) Then
                            If (Flag = True) Then
                                NeNmae = GIGBtxt(i).Substring(10, 15).Trim
                                Flag = False
                            End If
                        ElseIf (GIGBtxt(i).Contains("MOBILE COUNTRY CODE.......")) Then
                            MCC_code = Regex.Split(GIGBtxt(i), ":")
                            MCC = MCC_code(1).Trim

                        ElseIf (GIGBtxt(i).Contains("MOBILE NETWORK CODE.......")) Then
                            MNC_code = Regex.Split(GIGBtxt(i), ":")
                            MNC = MNC_code(1).Trim

                        ElseIf (GIGBtxt(i).Contains("RNC NAME.......")) Then
                            RNCFind = Regex.Split(GIGBtxt(i), ":")
                            RNC_NAME = RNCFind(1).Trim

                        ElseIf (GIGBtxt(i).Contains("RNC ADMINISTRATIVE STATE.......")) Then
                            RNC_State = Regex.Split(GIGBtxt(i), ":")
                            STATE = RNC_State(1).Trim

                        ElseIf (GIGBtxt(i).Contains("PAPU UNIT INDEX.......")) Then
                            papu = Regex.Split(GIGBtxt(i), ":")
                            PAPU_INDEX = papu(1).Trim
                            PAPU_Length = PAPU_INDEX.Length
                            If (PAPU_Length = 1) Then
                                PAPU_INDEX = "PAPU-0" + PAPU_INDEX
                            ElseIf (PAPU_Length = 2) Then
                                PAPU_INDEX = "PAPU-" + PAPU_INDEX
                            End If


                            'DT = objSourcedb.ExecuteSQL("select distinct papu_ip from tblsgsn_nsvci where comp_unit='" & PAPU_INDEX & "' and circle='" & Circle_Name & "' and ne_name='" & NeNmae & "' and vendor='" & _vendor & "'")
                            'If (DT.Rows.Count > 0) Then
                            '    Dim ip As String = DT(0)("papu_ip")

                            Dim sqlstr As String = "VENDOR, CIRCLE, OSS, NE_NAME, BSC, MCC, MNC, COMP_UNIT, OP_STATE"
                            sqlstr = "Insert into tblsgsn_nsvci(" & sqlstr & ") " +
                                                   " values('" & _vendor & "','" & _CircleName & "',' ','" & NeNmae & "','" & RNC_NAME & "','" & MCC & "','" & MNC & "','" & PAPU_INDEX & "','" & STATE & "')"
                            If (objSourcedb.ExecuteNonQuery(sqlstr)) = False Then

                            End If
                            'End If

                        End If

                    End If

                Next
            Catch ex As Exception
                File.AppendAllText(errorPath + "parsingError.txt", System.DateTime.Now.ToString() + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine)
            End Try

        End Function
        Public Function ParseData_CMD2(ByVal GIGBtxt() As String, ByVal Circle_Name As String, ByVal Element_Name As String, ByVal errorPath As String) As String
            Dim DIAMETER_PROFILE As String = String.Empty
            Dim FindResult() As String = Nothing
            Try
                For m As Integer = 0 To GIGBtxt.Length - 1

                    'oFileLog.WriteDataToFile(GIGBtxt(m), Circle_Name, Element_Name, "CMD2")
                    If GIGBtxt(m).Trim <> "" Then
                        If GIGBtxt(m).IndexOf("ocs-diameter-profile") <> -1 Then
                            FindResult = Nothing
                            FindResult = Split(GIGBtxt(m), " ")
                            DIAMETER_PROFILE = FindResult(2)
                            'Exit For
                        End If
                    End If
                Next
                Return DIAMETER_PROFILE
            Catch ex As Exception
                File.AppendAllText(errorPath + "parsingError.txt", System.DateTime.Now.ToString() + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine)
            End Try
        End Function
        Public Function ParseData_CMD1(ByVal GIGBtxt() As String, ByVal Circle_Name As String, ByVal Element_Name As String, ByVal errorPath As String) As String
            Dim CHARGE_PROFILE As String = String.Empty
            Dim FindResult() As String = Nothing
            ChargeingProfile_list = Nothing
            ChargeingProfile_list = New ArrayList

            Try
                For m As Integer = 0 To GIGBtxt.Length - 1

                    'oFileLog.WriteDataToFile(GIGBtxt(m), Circle_Name, Element_Name, "CMD1")
                    If GIGBtxt(m).Trim <> "" Then
                        If GIGBtxt(m).IndexOf("charging-profile =") <> -1 Then
                            'PREPAID
                            FindResult = Nothing
                            FindResult = GIGBtxt(m).Trim.Split("=")
                            CHARGE_PROFILE = FindResult(1).Trim.ToString()
                            ChargeingProfile_list.Add(CHARGE_PROFILE).ToString()
                            'Exit For
                        End If
                    End If
                Next
                Return CHARGE_PROFILE
            Catch ex As Exception
                File.AppendAllText(errorPath + "parsingError.txt", System.DateTime.Now.ToString() + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine)
            End Try

        End Function

        Public Function ParseData_ZQRJ(ByVal GIGBtxt() As String, ByVal Circle_Name As String, ByVal Element_Name As String, ByVal errorPath As String) As Boolean
            Dim Vendor As String = _vendor
            Dim NE_Name As String = String.Empty
            Dim PRIMARY_DNS_IP As String = String.Empty
            Dim SECONDARY_DNS_IP As String = String.Empty
            Dim flag As Boolean = False
            Dim FindResult() As String = Nothing
            Try


                For m As Integer = 0 To GIGBtxt.Length - 1

                    'change by manish
                    If GIGBtxt(m).IndexOf("SGSN") <> -1 Then

                        NE_Name = GIGBtxt(m).Substring(10, 15)

                    ElseIf GIGBtxt(m).IndexOf("PRIMARY DNS SERVER") <> -1 Then
                        FindResult = Regex.Split(GIGBtxt(m), ":")
                        PRIMARY_DNS_IP = FindResult(1).Trim.ToString()

                    ElseIf GIGBtxt(m).IndexOf("SECONDARY DNS SERVER") <> -1 Then
                        FindResult = Nothing
                        FindResult = Regex.Split(GIGBtxt(m), ":")
                        SECONDARY_DNS_IP = FindResult(1).Trim.ToString()
                        flag = True
                    End If
                    If flag Then
                        objSourcedb.ExecuteNonQuery("Update tblCoreMap Set PRIMARY_DNS='" & PRIMARY_DNS_IP & "', SECONDARY_DNS='" & SECONDARY_DNS_IP & "' Where VENDOR='" & Vendor & "' and CIRCLE='" & Circle_Name & "' and NE_NAME='" & NE_Name & "'")
                        Exit For
                    End If
                Next
            Catch ex As Exception
                File.AppendAllText(errorPath + "parsingError.txt", System.DateTime.Now.ToString() + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine)
            End Try
        End Function
        Public Function ParseData_ZQRX(ByVal GIGBtxt() As String, ByVal Circle_Name As String, ByVal Element_Name As String, ByVal COMP_UNIT As String, ByVal APN As String, ByVal GGSN_NE_NAME As String, ByVal errorPath As String) As Boolean
            Dim ErrorMsg As String = String.Empty
            Dim PAPU_IP As String = String.Empty
            Dim NEName As String = String.Empty
            Dim PRIMARY_DNS As String = String.Empty
            Dim SECONDARY_DNS As String = String.Empty
            Dim GGGSN_GateWay_IP As String = String.Empty
            Dim dtMaster As DataTable = Nothing
            Dim dtMAP As DataTable = Nothing
            Dim FindResult1() As String = Nothing
            Dim FindResult2() As String = Nothing
            Dim GGSN_NG_NAME As String = String.Empty
            Dim BLAD_NAME As String = String.Empty
            Dim Flag As String = False
            Try

                For i As Integer = 0 To GIGBtxt.Length - 1
                    If (GIGBtxt(i).Trim <> "") Then
                        If (GIGBtxt(i).Contains("PING")) And ((Not (GIGBtxt(i).Contains("PING,"))) And (Not (GIGBtxt(i).Contains("PING:HOST"))) And (Not (GIGBtxt(i).Contains("PING Statistics----")))) Then
                            FindResult1 = GIGBtxt(i).Split("):")
                            FindResult2 = FindResult1(0).Split("(")
                            GGGSN_GateWay_IP = FindResult2(1).Trim
                            Flag = True
                            Exit For
                        ElseIf (GIGBtxt(i).ToLower().Contains("cannot resolve")) Then
                            File.AppendAllText(errorPath + "parsingError.txt", System.DateTime.Now.ToString() + Environment.NewLine + "ZQRX:PAPU(This command is not completed execute.Plz check commands logs.)" + Environment.NewLine + Environment.NewLine)
                            GGGSN_GateWay_IP = "N/A"
                            Flag = True
                            Exit For

                        ElseIf (GIGBtxt(i).Contains("SGSN")) Then
                            NEName = GIGBtxt(i).Substring(10, 15).Trim

                        End If
                    End If
                Next

                If (Flag = True) Then
                    dtMAP = objSourcedb.ExecuteSQL("SELECT * FROM awe_paco_dcm.map_sheet where GW_IP_ADDRESS='" & GGGSN_GateWay_IP.ToString & "'")
                    If (dtMAP.Rows.Count > 0) Then
                        GGSN_NG_NAME = dtMAP.Rows(0)("NG_NAME").ToString.Trim
                        BLAD_NAME = dtMAP.Rows(0)("BLAD_NAME").ToString.Trim
                    Else
                        GGSN_NG_NAME = "N/A"
                        BLAD_NAME = "N/A"
                    End If


                    'objSourcedb.ExecuteNonQuery("update tblmaster_details set GGSN_SERVER_GW_IP='" & GGGSN_GateWay_IP & "' where circle='" & Circle_Name & "' and Ne_name='" & NEName & "';")
                    ' dtMaster = objSourcedb.ExecuteSQL("Select MCC, MNC, APN from tblmaster_details Where  VENDOR='" & _vendor & "' and CIRCLE='" & Circle_Name & "' and NE_NAME='" & NEName & "' ")
                    dtMaster = objSourcedb.ExecuteSQL("Select * from tblCoreMap Where  VENDOR='" & _vendor & "' and CIRCLE='" & Circle_Name & "' and NE_NAME='" & NEName & "' and COMP_UNIT='" & COMP_UNIT.ToString() & "' ")
                    If dtMaster.Rows.Count > 0 Then
                        PRIMARY_DNS = dtMaster.Rows(0)("PRIMARY_DNS").ToString.Trim
                        SECONDARY_DNS = dtMaster.Rows(0)("SECONDARY_DNS").ToString.Trim
                        PAPU_IP = dtMaster.Rows(0)("PAPU_IP").ToString.Trim
                        objSourcedb.ExecuteNonQuery("insert into tblcoremap_up_apndata(VENDOR, CIRCLE, OSS, NE_NAME, COMP_UNIT, PAPU_IP, PRIMARY_DNS, SECONDARY_DNS, APN, GGSN_SERVER_GW_IP,GGSN_BLADE,GGSN_NAME) values('" + _vendor + "','" + Circle_Name + "','','" + NEName + "','" + COMP_UNIT.ToString() + "','" + PAPU_IP.ToString() + "','" + PRIMARY_DNS + "','" + SECONDARY_DNS + "','" + APN + "','" + GGGSN_GateWay_IP + "','" + BLAD_NAME.ToString() + "','" + GGSN_NG_NAME.ToString() + "')")
                    End If
                End If

            Catch ex As Exception
                File.AppendAllText(errorPath + "parsingError.txt", System.DateTime.Now.ToString() + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine)
            End Try
        End Function

        Private Function GetCommand(ByVal Circle_Name As String, ByVal Element_Name As String, ByVal PAPU_INDEX As String, ByVal valIOCPE As String, ByVal MCC As String, ByVal MNC As String, ByVal APN As String, ByVal errorPath As String) As String
            Try
                'ZZQRX:PAPU,0:IOCPE,3:PING:HOST="AIRTELFUN.COM.MNC031.MCC404.GPRS",;
                Dim Vendor As String = _vendor
                Dim APNvalue As String = String.Empty
                Dim MCCvalue As String = String.Empty
                Dim MNCvalue As String = String.Empty
                Dim PAPUID As String = String.Empty

                MCCvalue = MCC
                MNCvalue = MNC


                If MCCvalue.Length < 3 Then
                    MCCvalue = "MCC0" & MCC
                Else
                    MCCvalue = "MCC" & MCCvalue
                End If
                If MNCvalue.Length < 3 Then
                    MNCvalue = "MNC0" & MNCvalue
                Else
                    MNCvalue = "MNC" & MNCvalue
                End If
                APNvalue = APN
                Dim Host As String = String.Empty
                PAPUID = PAPU_INDEX.ToString()
                Host = Chr(34) & APNvalue & "." & MNCvalue & "." & MCCvalue & "." & "GPRS" & Chr(34)
                Dim cmd As String = "ZQRX:PAPU," & PAPUID & ":IOCPE," & valIOCPE & ":PING:HOST=" & Host.Trim & ",;"
                Return cmd
                'End If
            Catch ex As Exception
                File.AppendAllText(errorPath + "CommandError.txt", "Command is not making proper....." + System.DateTime.Now.ToString() + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine)
            End Try
        End Function


        Private Function ATKAGetCommand(ByVal Circle_Name As String, ByVal Element_Name As String, ByVal PAPU_INDEX As String, ByVal valIOCPE As String, ByVal MCC As String, ByVal MNC As String, ByVal APN As String, ByVal errorPath As String) As String
            Try
                'ZZQRX:PAPU,0:IOCPE,3:PING:HOST="AIRTELFUN.COM.MNC031.MCC404.GPRS",;
                Dim Vendor As String = _vendor
                Dim APNvalue As String = String.Empty
                Dim MCCvalue As String = String.Empty
                Dim MNCvalue As String = String.Empty
                Dim PAPUID As String = String.Empty

                MCCvalue = MCC
                MNCvalue = MNC


                If MCCvalue.Length < 3 Then
                    MCCvalue = "MCC0" & MCC
                Else
                    MCCvalue = "MCC" & MCCvalue
                End If
                If MNCvalue.Length < 3 Then
                    MNCvalue = "MNC0" & MNCvalue
                Else
                    MNCvalue = "MNC" & MNCvalue
                End If
                APNvalue = APN
                Dim Host As String = String.Empty
                Dim cmd As String = String.Empty
                PAPUID = PAPU_INDEX.ToString()
                Host = Chr(34) & APNvalue & "." & MNCvalue & "." & MCCvalue & "." & "GPRS" & Chr(34)
                If ((Circle_Name = "MAH") And (Element_Name = "SGSN04" Or Element_Name = "SGSN05")) Then
                    cmd = "ZQRX:GBU," & PAPUID & "::PING:HOST=" & Host.Trim & ",;"
                ElseIf ((Circle_Name = "MP") And (Element_Name = "SGSN4")) Then
                    cmd = "ZQRX:GBU," & PAPUID & "::PING:HOST=" & Host.Trim & ",;"
                ElseIf ((Circle_Name = "GUJ") And (Element_Name = "SGSN4_RJT")) Then
                    cmd = "ZQRX:GBU," & PAPUID & "::PING:HOST=" & Host.Trim & ",;"
                Else
                    cmd = "ZQRX:PAPU," & PAPUID & "::PING:HOST=" & Host.Trim & ",;"
                End If

                Return cmd
                'End If
            Catch ex As Exception
                File.AppendAllText(errorPath + "C0mmandError.txt", "ATAKA Command is not making...." + System.DateTime.Now.ToString() + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine)
            End Try
        End Function


        Public Function ParseData_ZWVI(ByVal GIGBtxt() As String, ByVal Circle_Name As String, ByVal Element_Name As String, ByVal errorPath As String) As String
            Dim ErrorMsg As String = String.Empty
            Dim NE_Name As String = Element_Name
            Dim APN As String = String.Empty
            Dim NE_Name_ZWVI As String = String.Empty
            Dim MCC As String = String.Empty
            Dim MNC As String = String.Empty
            Dim updateflag As Boolean = False
            Dim FindResult() As String = Nothing
            Dim dtAPN As DataTable = Nothing
            Dim MCC_MNC As String = String.Empty
            Dim My_circle As String = String.Empty
            Try
                For m As Integer = 0 To GIGBtxt.Length - 1

                    If GIGBtxt(m).Trim <> "" Then
                        If GIGBtxt(m).IndexOf("SGSN") <> -1 Then
                            FindResult = Split(GIGBtxt(m), " ")
                            For k As Integer = 0 To FindResult.Length - 1
                                If FindResult(k).Trim.ToString() <> "" Then
                                    If k > 1 Then
                                        NE_Name_ZWVI = FindResult(k).Trim.ToString()
                                        Exit For
                                    End If
                                End If
                            Next
                        ElseIf GIGBtxt(m).IndexOf("MOBILE COUNTRY CODE") <> -1 Then
                            MCC = GIGBtxt(m + 1).Trim.ToString()
                        ElseIf GIGBtxt(m).IndexOf("MOBILE NETWORK CODE") <> -1 Then
                            MNC = GIGBtxt(m + 1).Trim.ToString()
                            updateflag = True
                        End If

                        'Note :- Plz Do not delete comment data
                        If updateflag = True Then

                            If ((NE_Name_ZWVI = "SGSN1RAJ") And (Circle_Name = "GUJ")) Then
                                Dim RAJ As String = "RAJSTAN"
                                Circle_Name = RAJ
                            End If
                            '    dtAPN = objSourcedb.ExecuteSQL("select APN_NAME from awe_paco_dcm.apn_detail where circle='" + _CircleName + "' and GGSN_NAME='" + GGSN_NE_NAME.ToString() + "'")
                            '    For Each drAPN As DataRow In dtAPN.Rows
                            '        APN = drAPN(0).ToString()
                            objSourcedb.ExecuteNonQuery("Insert into tblMaster_Details(VENDOR, CIRCLE, NE_NAME, MCC, MNC) Values('" & _vendor & "' ,'" & Circle_Name & "','" & NE_Name_ZWVI & "','" & MCC & "','" & MNC & "')")
                            updateflag = False
                            Exit For
                            '        APN = ""
                            '    Next

                            '    Exit For
                        End If
                    End If
                Next
                Return MCC + "MANISH" + MNC
            Catch ex As Exception
                File.AppendAllText(errorPath + "parsingError.txt", System.DateTime.Now.ToString() + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine)
                Return False
            End Try
        End Function

        Public Function ParseData_ZKAI(ByVal GIGBtxt() As String, ByVal Circle_Name As String, ByVal Element_Name As String, ByVal errorPath As String) As Boolean
            Dim Vendor As String = _vendor
            Dim ErrorMsg As String = String.Empty
            Dim CIRCLE As String = _CircleName
            Dim NE_Name As String = String.Empty
            Dim PAPU_ID As String = String.Empty
            Dim PAPU_IP As String = String.Empty
            Dim dt As DataTable = Nothing
            Dim FindResult() As String
            Dim flag As Boolean = False
            Dim GBflag As Boolean = False
            Dim cnt As Integer = 0
            Dim PAPU_IP_GN As String = String.Empty
            Dim Sqlstr As String = String.Empty



            Try

                For m As Integer = 0 To GIGBtxt.Length - 1

                    If GIGBtxt(m).Trim <> "" Then

                        If GIGBtxt(m).IndexOf("----") = -1 Then
                            If flag = True Then
                                If GIGBtxt(m).IndexOf("PAPU-") <> -1 Then
                                    PAPU_ID = GIGBtxt(m).Trim.ToString()
                                    dt = objSourcedb.ExecuteSQL("Select Comp_Unit from tblsgsn_nsvci Where Comp_Unit='" & PAPU_ID & "' and vendor='" & Vendor & "' and Circle='" & CIRCLE & "' and NE_Name='" & NE_Name & "'")
                                    If dt.Rows.Count > 0 Then
                                        GBflag = True
                                    End If

                                ElseIf GIGBtxt(m).IndexOf("GBU-") <> -1 Then
                                    PAPU_ID = GIGBtxt(m).Trim.ToString
                                    dt = objSourcedb.ExecuteSQL("Select Comp_Unit from tblsgsn_nsvci Where Comp_Unit='" & PAPU_ID & "' and vendor='" & Vendor & "' and Circle='" & CIRCLE & "' and NE_Name='" & NE_Name & "'")
                                    If dt.Rows.Count > 0 Then
                                        GBflag = True
                                    End If
                                ElseIf GIGBtxt(m).IndexOf("GB") <> -1 Then
                                    If GBflag = True Then
                                        FindResult = Nothing

                                        'changeing by manish
                                        FindResult = GIGBtxt(m).Split(New String() {" "}, StringSplitOptions.RemoveEmptyEntries)
                                        PAPU_IP = FindResult(1).Trim.ToString()


                                        objSourcedb.ExecuteNonQuery("Update tblsgsn_nsvci set PAPU_IP='" & PAPU_IP & "' Where Comp_Unit='" & PAPU_ID & "' and vendor='" & Vendor & "' and Circle='" & CIRCLE & "' and NE_Name='" & NE_Name & "'")
                                        PAPU_ID = ""
                                        PAPU_IP = ""
                                        GBflag = False
                                    End If
                                ElseIf GIGBtxt(m).IndexOf("GN") <> -1 Then  'changeing by manish, GN basically use for GGSN Data(PAPU_IP)

                                    If GBflag = True Then
                                        If (Not (PAPU_ID = "")) Then
                                            FindResult = Nothing


                                            FindResult = GIGBtxt(m).Split(New String() {" "}, StringSplitOptions.RemoveEmptyEntries)
                                            PAPU_IP_GN = FindResult(1).Trim.ToString()

                                            Try

                                                Sqlstr = ""
                                                Sqlstr = "Insert into tblCoreMap(VENDOR, CIRCLE, OSS, NE_NAME, COMP_UNIT, PAPU_IP) VALUES('" & Vendor & "','" & Circle_Name & "',' ','" & NE_Name & "','" & PAPU_ID & "','" & PAPU_IP_GN & "') "
                                                objSourcedb.ExecuteNonQuery(Sqlstr)
                                            Catch ex As Exception
                                                File.AppendAllText(errorPath + "parsingError.txt", "GN IP Not insert in db for GGSN" + System.DateTime.Now.ToString() + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine)
                                            End Try
                                            PAPU_IP_GN = ""
                                        End If
                                    End If


                                End If
                            End If
                            If GIGBtxt(m).IndexOf("UNIT") <> -1 And GIGBtxt(m).IndexOf("APP ID") <> -1 And GIGBtxt(m).IndexOf("IPV4") <> -1 Then
                                flag = True
                            ElseIf GIGBtxt(m).IndexOf("COMMAND EXECUTED") <> -1 Then
                                flag = False
                            ElseIf (GIGBtxt(m).IndexOf("SGSN") <> -1) Then
                                NE_Name = GIGBtxt(m).Substring(10, 15).Trim.ToString()
                            End If
                        End If
                    End If
                Next
            Catch ex As Exception
                File.AppendAllText(errorPath + "parsingError.txt", System.DateTime.Now.ToString() + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine)
                ' MsgBox(ex.Message)
            End Try
        End Function
        Public Function ParseData_ZEJL(ByVal GIGBtxt() As String, ByVal Circle_Name As String, ByVal Element_Name As String, ByVal errorPath As String) As String
            Dim ErrorMsg As String = String.Empty
            Dim NE_Name As String = String.Empty
            Dim BSC As String = String.Empty
            Dim NSEI_ID As String = String.Empty
            Dim PAPU_ID As String = String.Empty
            Dim MCC As String = String.Empty
            Dim MNC As String = String.Empty
            Dim LAC As String = String.Empty
            Dim RAC As String = String.Empty
            Dim CI As String = String.Empty
            Dim COMP_UNIT As String = String.Empty
            Dim NSEI_IDfind() As String = Nothing
            Dim COMP_UNITfind() As String = Nothing
            Dim RACfind() As String = Nothing
            Dim CIfind() As String = Nothing
            Dim MCCfind() As String = Nothing
            Dim MNCfind() As String = Nothing
            Dim LACfind() As String = Nothing


            Dim FindResult() As String
            Dim FindResult1() As String
            Dim flag As Boolean = False
            Dim cnt As Integer = 0
            ' oFileLog = New clsFileLogs(_CircleName)
            Try
                For m As Integer = 0 To GIGBtxt.Length - 1

                    If (GIGBtxt(m).Trim <> "") Then

                        If GIGBtxt(m).Contains("NSEI-") Then ' changing by manish
                            flag = True
                            NSEI_IDfind = Regex.Split(GIGBtxt(m), "-")
                            NSEI_ID = NSEI_IDfind(1).Trim.ToString()
                            If (NSEI_ID = "01218") Then
                                Dim id As String = String.Empty
                            End If
                        ElseIf GIGBtxt(m).IndexOf("SGSN") <> -1 Then
                            NE_Name = GIGBtxt(m).Substring(10, 15).Trim

                        ElseIf GIGBtxt(m).IndexOf("PAPU-") <> -1 Then
                            If GIGBtxt(m).Trim.Length > 7 Then
                                'FindResult = Nothing
                                FindResult = Split(GIGBtxt(m).Trim, "MCC") ' changing by manish
                                'COMP_UNITfind = Regex.Split(FindResult(0), "-")
                                'COMP_UNIT = COMP_UNITfind(1).Trim
                                COMP_UNIT = FindResult(0).Trim
                            Else
                                'COMP_UNITfind = Regex.Split(GIGBtxt(m), "-")
                                'COMP_UNIT = COMP_UNITfind(1).Trim   'PAPU-ID is COMP UNIT
                                COMP_UNIT = GIGBtxt(m).Trim.ToString()
                            End If
                        ElseIf GIGBtxt(m).IndexOf("GBU-") <> -1 Then
                            If GIGBtxt(m).Trim.Length > 7 Then
                                FindResult = Nothing 'now peace of code under testing 
                                cnt = 0
                                MCC = ""
                                MNC = ""
                                LAC = ""
                                FindResult = Split(GIGBtxt(m).Trim, "MCC")
                                'COMP_UNITfind = Regex.Split(FindResult(0), "-")
                                'COMP_UNIT = COMP_UNITfind(1).Trim
                                COMP_UNIT = FindResult(0).Trim.ToString()

                                FindResult1 = Split(FindResult(1).Trim, " ")
                                For k As Integer = 0 To FindResult1.Length - 1
                                    If FindResult1(k).Trim.ToString <> "" Then
                                        cnt += 1
                                        If cnt = 1 Then
                                            MCC = FindResult1(k).Trim.Substring(1, FindResult1(0).Length - 1)
                                        ElseIf cnt = 2 Then
                                            MNC = FindResult1(k).Trim.Substring(4)
                                        ElseIf cnt = 3 Then
                                            LAC = FindResult1(k).Trim.Substring(4)
                                        End If
                                    End If
                                Next

                            Else
                                'COMP_UNITfind = Regex.Split(GIGBtxt(m), "-")' changing by manish
                                'COMP_UNIT = COMP_UNITfind(1).Trim   'PAPU-ID is COMP UNIT
                                COMP_UNIT = GIGBtxt(m).Trim.ToString()
                            End If

                        ElseIf GIGBtxt(m).IndexOf("MCC-") <> -1 And GIGBtxt(m).IndexOf("MNC-") <> -1 And GIGBtxt(m).IndexOf("LAC-") <> -1 Then
                            FindResult = Nothing
                            FindResult = Split(GIGBtxt(m), " ")
                            For i As Integer = 0 To FindResult.Length - 1
                                If (FindResult(i).Trim <> "") Then
                                    If (FindResult(i).Contains("MCC-")) Then
                                        MCCfind = Regex.Split(FindResult(i), "-")
                                        MCC = MCCfind(1).Trim

                                    ElseIf (FindResult(i).Contains("MNC-")) Then
                                        MNCfind = Regex.Split(FindResult(i), "-")
                                        MNC = MNCfind(1).Trim

                                    ElseIf (FindResult(i).Contains("LAC-")) Then
                                        LACfind = Regex.Split(FindResult(i), "-")
                                        LAC = LACfind(1).Trim
                                        flag = True
                                    End If
                                End If
                            Next

                        ElseIf GIGBtxt(m).IndexOf("RAC-") <> -1 Then
                            RACfind = Regex.Split(GIGBtxt(m), "-")
                            RAC = RACfind(1).Trim
                            flag = True
                        ElseIf GIGBtxt(m).IndexOf("CI-") <> -1 Then
                            If flag = True Then
                                FindResult = Nothing
                                FindResult = Split(GIGBtxt(m).Trim, " ")
                                If GIGBtxt(m).Contains("STATE-WO") Then
                                    CIfind = Regex.Split(FindResult(0), "-")
                                    CI = CIfind(1).Trim

                                    If UpdateSGSN(_OSSName, NE_Name, NSEI_ID, Val(MCC), Val(MNC), Val(LAC), Val(RAC), Val(CI), COMP_UNIT, ErrorMsg) = False Then

                                    End If
                                    flag = False
                                End If
                            End If
                        End If
                    End If
                Next
                Return NE_Name
            Catch ex As Exception
                File.AppendAllText(errorPath + "parsingError.txt", System.DateTime.Now.ToString() + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine)

                Return Nothing
            End Try
        End Function
        Public Function ParseData_ZFWO(ByVal GIGBtxt() As String, ByVal Circle_Name As String, ByVal Element_Name As String, ByVal errorPath As String) As Boolean
            Dim ErrorMsg As String = String.Empty
            Dim Vendor As String = _vendor
            Dim CIRCLE As String = _CircleName
            Dim NE_Name As String = String.Empty
            Dim NSEI_ID As String = String.Empty
            Dim NSEI_IDfind() As String = Nothing
            Dim NSEI_ID_ZEJL As String = String.Empty
            Dim NSVC_ID As String = String.Empty
            Dim NSVC_NAME As String = String.Empty
            Dim OP_STATE As String = String.Empty
            Dim DLCI_UDP_PORT As String = String.Empty
            Dim CIR_RDW As String = String.Empty
            Dim BEARERID_RSW As String = String.Empty
            Dim BEARERNAME_RPNBR As String = String.Empty
            Dim PCM_PCU_IP As String = String.Empty
            Dim dtNSEI As DataTable = Nothing
            Dim DATA As String = String.Empty

            Dim FindResult() As String = Nothing
            Dim flag As Boolean = False
            Dim flag2 As Boolean = False
            Dim cnt As Integer = 0
            Try
                For m As Integer = 0 To GIGBtxt.Length - 1
                    'changeing by Manish 
                    If GIGBtxt(m).Trim <> "" Then
                        'If GIGBtxt(m).Contains("REMOTE IP") Then
                        '    Dim kal As String = String.Empty
                        'End If
                        'If (GIGBtxt(m).Contains("CHANNEL")) Then
                        '    Dim lak As String = String.Empty
                        'End If

                        If GIGBtxt(m).IndexOf("NSEI") <> -1 Then
                            flag = False
                            FindResult = Regex.Split(GIGBtxt(m), " ")
                            NSEI_IDfind = Regex.Split(FindResult(0), "-")
                            NSEI_ID = NSEI_IDfind(1).Trim
                            dtNSEI = objSourcedb.ExecuteSQL("Select NSEI from tblsgsn_nsvci Where NSEI='" & NSEI_ID & "' and vendor='" & Vendor & "' and Circle='" & CIRCLE & "' and NE_Name='" & NE_Name & "'")

                            'If (NSEI_ID_ZEJL = "07766") Then
                            '    Dim mm As String = String.Empty
                            'End If
                            If dtNSEI.Rows.Count > 0 Then
                                NSEI_ID_ZEJL = dtNSEI.Rows(0)(0)
                                If NSEI_ID_ZEJL <> "" Then
                                    flag = True
                                    NSEI_ID_ZEJL = ""
                                End If
                            End If

                        ElseIf GIGBtxt(m).IndexOf("SGSN") <> -1 Then
                            'change by manish
                            NE_Name = GIGBtxt(m).Substring(10, 15).Trim

                        End If

                        If flag Then
                            If GIGBtxt(m).IndexOf("ID") <> -1 And GIGBtxt(m).IndexOf("STA") <> -1 And GIGBtxt(m).IndexOf("STATE") <> -1 And GIGBtxt(m).IndexOf("UDP") <> -1 And GIGBtxt(m).IndexOf("RPNBR") <> -1 And GIGBtxt(m).IndexOf("REMOTE IP") <> -1 Then
                                'cnt = 0
                                FindResult = Nothing
                                'changeing by manish
                                FindResult = GIGBtxt(m + 2).Split(New String() {" "}, StringSplitOptions.RemoveEmptyEntries)
                                'FindResult = Split(GIGBtxt(m + 2).Trim, " ")
                                'For k = 0 To FindResult.Length - 1
                                '    If FindResult(k).Trim.ToString() <> "" Then
                                '        cnt += 1
                                '        If cnt = 1 Then

                                NSVC_ID = FindResult(0).Trim.ToString() 'NSVC ID 
                                ''ElseIf cnt = 2 Then
                                NSVC_NAME = FindResult(1).Trim.ToString() 'NSVC Name
                                'ElseIf cnt = 4 Then
                                OP_STATE = FindResult(3).Trim.ToString() ' OP state
                                'ElseIf cnt = 5 Then
                                DLCI_UDP_PORT = FindResult(4).Trim.ToString() ' UDP
                                'ElseIf cnt = 6 Then
                                CIR_RDW = FindResult(5).Trim.ToString() 'RDW  
                                'ElseIf cnt = 7 Then
                                BEARERID_RSW = FindResult(6).Trim.ToString() 'RSW  
                                'ElseIf cnt = 8 Then
                                BEARERNAME_RPNBR = FindResult(7).Trim.ToString() 'RPNBR
                                'ElseIf cnt = 9 Then
                                PCM_PCU_IP = FindResult(8).Trim.ToString() 'REMOTE IP ENDPOINT


                                UpdateSGSN(_vendor, _CircleName, NE_Name, NSEI_ID, NSVC_ID, NSVC_NAME, OP_STATE, DLCI_UDP_PORT, CIR_RDW, BEARERID_RSW, BEARERNAME_RPNBR, PCM_PCU_IP, "")
                                NSEI_ID = ""
                                NSVC_ID = ""
                                NSVC_NAME = ""
                                OP_STATE = ""
                                DLCI_UDP_PORT = ""
                                CIR_RDW = ""
                                BEARERID_RSW = ""
                                BEARERNAME_RPNBR = ""
                                PCM_PCU_IP = ""

                            End If
                            If GIGBtxt(m).IndexOf("NS-VC") <> -1 And GIGBtxt(m).IndexOf("ADM") <> -1 And GIGBtxt(m).IndexOf("OP") <> -1 And GIGBtxt(m).IndexOf("DLCI") <> -1 And GIGBtxt(m).IndexOf("BEARER") <> -1 Then

                                FindResult = Nothing
                                FindResult = GIGBtxt(m + 3).Split(New String() {" "}, StringSplitOptions.RemoveEmptyEntries) ' change by manish

                                'changeing by manish
                                NSVC_ID = FindResult(0).Trim.ToString() 'NSVCI ID
                                NSVC_NAME = FindResult(1).Trim.ToString() 'NSVCI NAME
                                OP_STATE = FindResult(3).Trim.ToString() ' OP STATE
                                DLCI_UDP_PORT = FindResult(4).Trim.ToString() 'DLCI
                                CIR_RDW = FindResult(6).Trim.ToString() 'CIR
                                BEARERID_RSW = Val(FindResult(8).Trim.ToString()) ' BEARER ID
                                BEARERNAME_RPNBR = FindResult(9).Trim.ToString() ' Name


                                UpdateSGSN(_vendor, _CircleName, NE_Name, NSEI_ID, NSVC_ID, NSVC_NAME, OP_STATE, DLCI_UDP_PORT, CIR_RDW, BEARERID_RSW, BEARERNAME_RPNBR, PCM_PCU_IP, "")
                                NSEI_ID = ""
                                NSVC_ID = ""
                                NSVC_NAME = ""
                                OP_STATE = ""
                                DLCI_UDP_PORT = ""
                                CIR_RDW = ""
                                BEARERID_RSW = ""
                                BEARERNAME_RPNBR = ""
                                PCM_PCU_IP = ""
                            End If
                        End If

                    End If
                Next
            Catch ex As Exception
                File.AppendAllText(errorPath + "parsingError.txt", System.DateTime.Now.ToString() + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine)
                'oExceptionLog.WriteExceptionErrorToFile("ParseData()", ex.Message, " Element Name: " & NE_Name, "")
            End Try
        End Function
        Public Function ParseData_ZFUI(ByVal GIGBtxt() As String, ByVal Circle_Name As String, ByVal Element_Name As String, ByVal errorPath As String) As Boolean
            Dim Vendor As String = _vendor
            Dim ErrorMsg As String = String.Empty
            Dim CIRCLE As String = Circle_Name
            Dim NE_Name As String = String.Empty
            Dim BEARERID_RSW As String = String.Empty
            Dim BEARERNAME_RPNBR As String = String.Empty
            Dim PCM_PCU_IP As String = String.Empty
            Dim TS1 As String = String.Empty
            Dim TS2 As String = String.Empty
            Dim dt As DataTable = Nothing
            Dim Flag As Boolean = False
            Dim BtimeF() As String = Nothing

            Dim FindResult() As String


            Try
                For m As Integer = 0 To GIGBtxt.Length - 1
                    'change by manish
                    If GIGBtxt(m).Trim <> "" Then

                        If (GIGBtxt(m).Contains("ID")) And (GIGBtxt(m).Contains("BEARER NAME")) And (GIGBtxt(m).Contains("SLOTS")) Then
                            Flag = False

                        ElseIf (GIGBtxt(m).Contains("----")) Then
                            Flag = True
                        ElseIf (GIGBtxt(m).Contains("COMMAND EXECUTED")) Then
                            Flag = False

                        ElseIf (Flag = True) Then
                            FindResult = GIGBtxt(m).Split(New String() {" "}, StringSplitOptions.RemoveEmptyEntries)

                            BEARERID_RSW = FindResult(0).Trim.ToString() ' change by manish
                            'Dim mk As Integer = BEARERID_RSW.Length
                            'If (BEARERID_RSW.Length = 3) Then
                            '    BEARERID_RSW = "00" + BEARERID_RSW
                            'ElseIf (BEARERID_RSW.Length = 4) Then
                            '    BEARERID_RSW = "0" + BEARERID_RSW
                            'End If

                            BEARERNAME_RPNBR = FindResult(1).Trim.ToString()

                            BtimeF = Split(FindResult(5), "-")
                            TS1 = BtimeF(0).Trim.ToString()
                            TS2 = BtimeF(1).Trim.ToString()

                            dt = objSourcedb.ExecuteSQL("Select NSEI from tblsgsn_nsvci Where BEARER_ID_RSW='" & BEARERID_RSW & "' and BEARER_NAME_RPNBR='" & BEARERNAME_RPNBR & "' and vendor='" & Vendor & "' and Circle='" & CIRCLE & "' and NE_Name='" & NE_Name & "'")

                            If dt.Rows.Count > 0 Then
                                objSourcedb.ExecuteNonQuery("Update tblsgsn_nsvci set TS_1='" & TS1 & "',TS_2='" & TS2 & "' Where BEARER_ID_RSW='" & BEARERID_RSW & "' and BEARER_NAME_RPNBR='" & BEARERNAME_RPNBR & "' and vendor='" & Vendor & "' and Circle='" & CIRCLE & "' and NE_Name='" & NE_Name & "'")
                            End If
                        End If

                        If GIGBtxt(m).IndexOf("SGSN") <> -1 Then
                            NE_Name = GIGBtxt(m).Substring(10, 15).Trim.ToString() 'change by manish
                        End If

                    End If
                Next
            Catch ex As Exception
                File.AppendAllText(errorPath + "parsingError.txt", System.DateTime.Now.ToString() + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine)
                ' MsgBox(ex.Message)
            End Try
        End Function
        Private Function UpdateSGSN(ByVal strOSS As String, ByVal NE_Name As String, ByVal NSEI_ID As String, ByVal MCC As Integer, ByVal MNC As Integer, ByVal LAC As Integer, ByVal RAC As Integer, ByVal CI As Integer, ByVal COMP_UNIT As String, ByRef ErrorMsg As String) As Boolean
            Try
                Dim sqlstr As String = "VENDOR, CIRCLE, OSS, NE_NAME , NSEI , MCC, MNC, LAC, RAC, CI, COMP_UNIT"
                sqlstr = "Insert into tblsgsn_nsvci(" & sqlstr & ") " & _
                                       " values('" & _vendor & "','" & _CircleName & "','" & strOSS & "','" & NE_Name & "','" & NSEI_ID & "','" & MCC & "','" & MNC & "','" & LAC & "','" & RAC & "','" & CI & "','" & COMP_UNIT & "')"
                If sqlstr <> "" Then
                    If objSourcedb.ExecuteNonQuery(sqlstr) = False Then
                        ErrorMsg = "Could'nt save record.Please check MYSQL query!"
                        Return False
                    End If
                End If
                Return True
            Catch ex As Exception
                ErrorMsg = ex.Message
                Return False
            End Try
        End Function
        Private Function UpdateSGSN(ByVal Vendor As String, ByVal Circle As String, ByVal NE_Name As String, ByVal NSEI As String, ByVal NSVC_ID As String, ByVal NSVC_NAME As String, ByVal OP_STATE As String, ByVal DLCI_UDP_PORT As String, ByVal CIR_RDW As String, ByVal BEARERID_RSW As String, ByVal BEARERNAME_RPNBR As String, ByVal PCM_PCU_IP As String, ByRef ErrorMsg As String) As Boolean
            Try
                Dim sqlstr As String = ""
                If PCM_PCU_IP <> "" Then
                    sqlstr = "Update tblsgsn_nsvci set " & _
                                       "NSVC_ID='" & NSVC_ID & "',NSVC_NAME='" & NSVC_NAME & "',OP_STATE='" & OP_STATE & "',DLCI_UDP_PORT='" & DLCI_UDP_PORT & "',CIR_RDW='" & CIR_RDW & "',BEARER_ID_RSW='" & BEARERID_RSW & "',BEARER_NAME_RPNBR='" & BEARERNAME_RPNBR & "',PCM_PCU_IP='" & PCM_PCU_IP & "'" & _
                                       "Where VENDOR='" & Vendor & "' and CIRCLE='" & Circle & "' and NE_NAME='" & NE_Name & "' and NSEI='" & NSEI & "'"
                Else
                    sqlstr = "Update tblsgsn_nsvci set " & _
                                       "NSVC_ID='" & NSVC_ID & "',NSVC_NAME='" & NSVC_NAME & "',OP_STATE='" & OP_STATE & "',DLCI_UDP_PORT='" & DLCI_UDP_PORT & "',CIR_RDW='" & CIR_RDW & "',BEARER_ID_RSW='" & BEARERID_RSW & "',BEARER_NAME_RPNBR='" & BEARERNAME_RPNBR & "'" & _
                                       "Where VENDOR='" & Vendor & "' and CIRCLE='" & Circle & "' and NE_NAME='" & NE_Name & "' and NSEI='" & NSEI & "'"
                End If

                If sqlstr <> "" Then
                    If objSourcedb.ExecuteNonQuery(sqlstr) = False Then
                        ErrorMsg = "Could'nt save record.Please check MYSQL query!"
                        Return False
                    End If
                End If
                Return True
            Catch ex As Exception
                ErrorMsg = ex.Message
                Return False
            End Try
        End Function



        Private Function UpdateND111(ByVal strCircle As String, ByVal strOSS As String, ByVal strVendor As String) As Boolean
            Dim DT As New DataTable
            Dim DT1 As New DataTable
            Dim DT2 As New DataTable
            Dim adap As MySqlDataAdapter
            Dim SqlStrWB_KOL As String = String.Empty
            Dim SqlStr1 As String = String.Empty
            Dim SqlStrUpdate As String = String.Empty
            Dim con As New MySqlConnection(connectionstringg)
            Try
                Dim SqlStr As String = String.Empty


                If strCircle = "WB" Or strCircle = "KOL" Then
                    'Note:-changing by manish
                    '----------------------------------------------------------------------------------------------------------------------------------------------
                    ' for - (WB & KOL)
                    'SqlStr = "SELECT S.NSEI as NSEI,S.NE_NAME,N.BSC_NAME as BSC_Name,N.Circle as CIRCLE, N.OSS as OSS,N.VENDOR FROM tblsgsn_nsvci S" & _
                    '          " Inner Join tblnd111 N" & _
                    '            " ON" & _
                    '            " cast(S.NSEI as unsigned ) = N.NSEI" & _
                    '            " AND S.LAC = N.LAC" & _
                    '            " And S.RAC = N.RAC" & _
                    '            " And ((S.Circle = 'KOL' OR S.Circle = 'WB') And (N.Circle = 'KOL' OR N.Circle = 'WB'))" & _
                    '            " And S.Vendor = N.Vendor" & _
                    '            " Where (S.Circle = 'KOL' OR N.OSS  = 'KOL') OR (S.Circle = 'WB' OR N.OSS  = 'WB')  AND N.VENDOR  = 'NSN' ;"
                    '---------------------------------------------------------------------------------------------------------------------------------------------------
                    SqlStr = "SELECT S.NSEI as NSEI,S.NE_NAME,N.BSC_NAME as BSC_Name,N.Circle as CIRCLE, N.OSS as OSS,N.VENDOR FROM tblsgsn_nsvci S" & _
                             " Inner Join tblnd111 N" & _
                               " ON" & _
                               " cast(S.NSEI as unsigned ) = N.NSEI" & _
                               " AND S.LAC = N.LAC" & _
                               " And S.RAC = N.RAC" & _
                               " And ((S.Circle  in ('KOL','WB','ASSAM')) And (N.Circle  in ('KOL','WB')))" & _
                               " And S.Vendor = N.Vendor" & _
                               " Where (S.Circle in ('KOL','WB','ASSAM','SHILONG') OR N.OSS  IN ('KOL','WB')) AND N.VENDOR  = 'NSN' ;"
                    DT1 = objSourcedb.ExecuteSQL(SqlStr)

                    If DT1.Rows.Count > 0 Then
                        For Each drs As DataRow In DT1.Rows
                            SqlStr1 = String.Empty
                            'SqlStr1 = "Select NSEI, NE_NAME, BSC, CIRCLE, OSS, VENDOR from tblsgsn_nsvci Where NSEI='" & drs("NSEI").ToString.Trim & "' and NE_NAME='" & drs("NE_NAME").ToString.Trim & "' and ((CIRCLE= 'KOL' OR OSS = 'KOL') OR (CIRCLE= 'WB' OR OSS = 'WB')) and Vendor = '" & strVendor.ToString.Trim & "'"
                            SqlStr1 = "Select NSEI, NE_NAME, BSC, CIRCLE, OSS, VENDOR from tblsgsn_nsvci Where NSEI='" & drs("NSEI").ToString.Trim & "' and NE_NAME='" & drs("NE_NAME").ToString.Trim & "' and (CIRCLE IN ('KOL','WB','ASSAM','SHILONG')) and Vendor = '" & strVendor.ToString.Trim & "'"
                            DT2 = Nothing
                            DT2 = objSourcedb.ExecuteSQL(SqlStr1)
                            If DT2.Rows.Count > 0 Then
                                ' For Each dr As DataRow In DT2.Rows
                                SqlStrWB_KOL = String.Empty
                                ''NOTE : -- Here Circle variable is not use for updating data because here circle(KOL&WB) are combine so that circle variabe have one value at the same time but database(DT1) have two cicle name at the same time.  
                                'SqlStrWB_KOL = "Update tblsgsn_nsvci set BSC='" & drs("BSC_Name").ToString.Trim & "', OSS ='" & drs("OSS").ToString.Trim & "' Where  NSEI='" & drs("NSEI").ToString.Trim & "' and NE_NAME='" & drs("NE_NAME").ToString.Trim & "' and Circle IN ('KOL','WB')  and Vendor = '" & strVendor.ToString.Trim & "'"
                                SqlStrWB_KOL = "Update tblsgsn_nsvci set BSC='" & drs("BSC_Name").ToString.Trim & "', OSS ='" & drs("OSS").ToString.Trim & "' Where  NSEI='" & drs("NSEI").ToString.Trim & "' and NE_NAME='" & drs("NE_NAME").ToString.Trim & "' and Circle IN ('KOL','WB','ASSAM','SHILONG')  and Vendor = '" & strVendor.ToString.Trim & "'"
                                objSourcedb.ExecuteNonQuery(SqlStrWB_KOL)
                                '   Next
                            Else
                                '                'SqlStr = "Insert into tbl_paco_node_detail(NSVCI_ID, SGSN_NAME, BSC_NAME, CIRCLE, OSS, VENDOR) Values('" & drs("NSVCI_ID").ToString.Trim & "','" & drs("SGSN_NAME").ToString.Trim & "','" & drs("BSC_NAME").ToString.Trim & "','" & drs("CIRCLE").ToString.Trim & "','" & drs("OSS").ToString.Trim & "','" & drs("VENDOR").ToString.Trim & "')"
                                '                'objSourcedb.ExecuteNonQuery(SqlStr)
                            End If

                            '            If strOSS = "WB" And drs("SGSN_NAME") = "SGSN2" Then
                            '                SqlStr = "Select NSVCI_ID, SGSN_NAME, BSC_NAME, CIRCLE, OSS, VENDOR from tbl_paco_node_detail Where NSVCI_ID='" & drs("NSVCI_ID") & "' and Circle= 'KOL' and OSS = 'KOL' and Vendor = '" & strVendor & "'"

                            '                If objSourcedb.ExecuteSQL(SqlStr).Rows.Count > 0 Then
                            '                    SqlStr = "Update tbl_paco_node_detail set NSVCI_ID='" & drs("NSVCI_ID") & "', SGSN_NAME='" & drs("SGSN_Name") & "', BSC_NAME='" & drs("BSC_Name") & "', CIRCLE='KOL', OSS ='KOL', VENDOR='" & drs("VENDOR") & "'  Where  NSVCI_ID='" & drs("NSVCI_ID") & "' and Circle = 'KOL' and OSS = 'KOL' and Vendor = '" & strVendor & "'"
                            '                Else
                            '                    SqlStr = "Insert into tbl_paco_node_detail(NSVCI_ID, SGSN_NAME, BSC_NAME, CIRCLE, OSS, VENDOR) Values('" & drs("NSVCI_ID") & "','" & drs("SGSN_NAME") & "','" & drs("BSC_NAME") & "','KOL','KOL','" & drs("VENDOR") & "')"

                        Next
                    End If
                Else
                    '**************************************************************************************** FOR All CIRCLE*************************************************************************************************
                    SqlStr = "SELECT S.NSEI as NSEI,S.NE_NAME,N.BSC_NAME as BSC_Name,N.Circle as CIRCLE, N.OSS as OSS,N.VENDOR FROM tblsgsn_nsvci S  Inner Join tblnd111 N ON cast(S.NSEI as unsigned) = N.NSEI  AND S.LAC = N.LAC And S.RAC = N.RAC And S.RAC = N.RAC And S.Vendor = N.Vendor And S.Circle = N.Circle Where S.Circle = '" & strCircle.ToString.Trim & "' AND N.OSS  = '" & strOSS.ToString.Trim & "' AND N.VENDOR  = '" & strVendor.ToString.Trim & "'"
                    DT1 = objSourcedb.ExecuteSQL(SqlStr)
                    If DT1.Rows.Count > 0 Then
                        For Each drs As DataRow In DT1.Rows

                            SqlStr1 = String.Empty
                            SqlStr1 = "Select NSEI, NE_NAME, BSC, CIRCLE, OSS, VENDOR from tblsgsn_nsvci Where NSEI='" & drs("NSEI").ToString.Trim & "' and NE_NAME='" & drs("NE_NAME").ToString.Trim & "' and Circle= '" & strCircle.ToString.Trim & "'  and Vendor = '" & strVendor.ToString.Trim & "'"

                            If objSourcedb.ExecuteSQL(SqlStr1).Rows.Count > 0 Then

                                SqlStrUpdate = String.Empty
                                SqlStrUpdate = "Update tblsgsn_nsvci set BSC='" & drs("BSC_Name").ToString.Trim & "', OSS ='" & drs("OSS").ToString.Trim & "' Where  NSEI='" & drs("NSEI").ToString.Trim & "' and NE_NAME='" & drs("NE_NAME").ToString.Trim & "' and Circle = '" & strCircle.ToString.Trim & "' and Vendor = '" & strVendor.ToString.Trim & "'"
                                objSourcedb.ExecuteNonQuery(SqlStrUpdate)
                            Else
                                'SqlStr = "Insert into tbl_paco_node_detail(NSVCI_ID, SGSN_NAME, BSC_NAME, CIRCLE, OSS, VENDOR) Values('" & drs("NSVCI_ID").ToString.Trim & "','" & drs("SGSN_NAME").ToString.Trim & "','" & drs("BSC_NAME").ToString.Trim & "','" & drs("CIRCLE").ToString.Trim & "','" & drs("OSS").ToString.Trim & "','" & drs("VENDOR").ToString.Trim & "')"
                                'objSourcedb.ExecuteNonQuery(SqlStr)
                            End If

                        Next


                    End If
                    '**************************************************************************************************************************************************************************************************
                End If



                'SqlStr = "Select NSVCI_ID, SGSN_NAME, BSC_NAME, CIRCLE, OSS, VENDOR from tbl_paco_node_detail "
                'SqlStr = "Select NSVCI_ID, SGSN_NAME, BSC_NAME, CIRCLE_Name, VENDOR from tbl_paco_node_details Where Circle_NAme = '" & strOSS & "' AND Vendor = '" & strVendor & "'"
                ''DT2 = objSourcedb.ExecuteSQL(SqlStr)
                'adap = New MySqlDataAdapter(SqlStr, con)
                'adap.Fill(DT2)

                'DT = DT2.Clone
                'Dim keys(0) As DataColumn
                'Dim dc As DataColumn = DT2.Columns(0)
                'keys(0) = dc
                'DT2.PrimaryKey = keys

                'Dim dr As DataRow = Nothing
                'For Each drs As DataRow In DT1.Rows
                '    If Not DT2.Rows.Contains(drs.Item(0)) Then
                '        dr = DT.NewRow
                '        dr(0) = drs(0)
                '        dr(1) = drs(1)
                '        dr(2) = drs(2)
                '        dr(3) = drs(3)
                '        dr(4) = drs(4)
                '        dr(5) = drs(5)
                '        DT.Rows.Add(dr)
                '    End If
                'Next
                'Dim CmdBuilder As New MySqlCommandBuilder(adap)
                'adap.Update(DT)
                Return True
            Catch ex As Exception
                'Call oExceptionLog.WriteExceptionErrorToFile("AutoUpdateNSEIMapping2()", ex.Message, "Error while processing circle: " & _CircleName, ex.Message)
                Return False
            Finally
                If con.State = ConnectionState.Open Then
                    con.Close()
                End If
                adap = Nothing
                con = Nothing
            End Try
        End Function
        Private Sub ArchieveToRaw(ByVal BSC As String)
            Try
                Dim SP_Name As String = "Alarm_Insertion_Distribution"
                Dim Vendor_Name As String = System.Configuration.ConfigurationManager.AppSettings("Vendor").ToString()
                Dim SqlParam As New ArrayList
                SqlParam(0) = "VendorName"
                SqlParam(1) = "CircleName"
                objSourcedb.ExecuteProcedure(SP_Name, SqlParam)
            Catch ex As Exception
                oFileLog.WriteEventToFile("[" & Now.ToString("HH:mm:ss") & "][ BSC : " & BSC & "] " & ex.Message & "", BSC, _ThreadID)
            End Try
        End Sub
        Private Function SetProgressStatusRouter(ByVal strRouterID As String, ByVal strStatus As NERouterConnectionStatus, ByVal strErrMsg As String, ByVal strRemark As String)

            Try
                If strRouterID.Trim = "" Then
                    If objSourcedb.ExecuteSQL("Select Status from NE_Master Where Status<>''").Rows.Count = 0 Then
                        objSourcedb.ExecuteNonQuery("Update Router_Master set Status = '',Remarks='', ErrorMsg ='' Where vendor='" & Trim(_vendor) & "' and Circle = '" & Trim(_CircleName) & "';")
                    End If
                Else
                    objSourcedb.ExecuteNonQuery("Update Router_Master set Status = '" & strStatus.ToString() & "',Remarks='" & strRemark & "', ErrorMsg ='" & strErrMsg & "' Where vendor='" & Trim(_vendor) & "' and Circle = '" & Trim(_CircleName) & "' and Router_Id =" & strRouterID & ";")
                    If strStatus = NERouterConnectionStatus.Failed Then
                        Dim ErrorMessageString As String = ""
                        Dim LogTiime As String = Now.ToString("yyyy-MM-dd HH:mm:ss")
                        objSourcedb.ExecuteNonQuery("Insert Into tblerrorlog(ExceptionLog,LogTime) Values('" & strErrMsg & "','" & LogTiime & "');")
                        oExceptionLog.WriteExceptionErrorToFile("???", strErrMsg, strErrMsg, ErrorMessageString)
                    End If
                End If

                Return True
            Catch ex As Exception
                'TBD
                Return False
            End Try
        End Function
        Private Function SetProgressStatusNE(ByVal strNEType As String, ByVal strNEID As String, ByVal strStatus As NERouterConnectionStatus, ByVal strErrMsg As String, ByVal strRemark As String)
            Try
                If strNEID.Trim = "" Then
                    objSourcedb.ExecuteNonQuery("Update ne_master set Status = '',Remarks='', ErrorMsg ='' Where vendor='" & Trim(_vendor) & "' and Circle = '" & Trim(_CircleName) & "' and OSS = '" & Trim(_OSSName) & "' And Node_Type = '" & strNEType & "' ;")
                Else
                    objSourcedb.ExecuteNonQuery("Update ne_master set Status = '" & strStatus.ToString() & "',Remarks='" & strRemark.Replace("'", "") & "', ErrorMsg ='" & strErrMsg.Replace("'", "") & "' Where vendor='" & Trim(_vendor) & "' and Circle = '" & Trim(_CircleName) & "' and OSS = '" & Trim(_OSSName) & "' And Node_Type = '" & strNEType & "' and NE_NAME ='" & strNEID & "';")
                    If strStatus = NERouterConnectionStatus.Failed Then
                        Dim ErrorMessageString As String = ""
                        Dim LogTiime As String = Now.ToString("yyyy-MM-dd HH:mm:ss")
                        objSourcedb.ExecuteNonQuery("Insert Into tblerrorlog(ExceptionLog,LogTime) Values('" & strErrMsg & "','" & LogTiime & "');")
                        oExceptionLog.WriteExceptionErrorToFile("???", strErrMsg, strErrMsg, ErrorMessageString)
                    End If
                End If
                Return True
            Catch ex As Exception
                'TBD
                Return False
            End Try
        End Function

        Public Function ND111() As Boolean
            _CircleName = String.Empty
            _vendor = System.Configuration.ConfigurationManager.AppSettings("Vendor").ToString()
            _client = System.Configuration.ConfigurationManager.AppSettings("Client").ToString()

            Dim _clientConString As String = String.Empty
            Dim dtClient As DataTable = Nothing
            _clientConString = System.Configuration.ConfigurationManager.AppSettings("PSDBConstring").ToString()
            Dim objProcess As New clsPSProcess(_clientConString)
            objSourcedb = New DbConnectLayer(_clientConString, DbType.MySqlType)
            Try

                dtClient = objSourcedb.ExecuteSQL("SELECT distinct circle FROM awe_paco_dcm.router_master where domain='PS' and node_type='SGSN'")


                For Each dr As DataRow In dtClient.Rows

                    _NEType = "SGSN"
                    _CircleName = dr("circle")

                    objProcess.NE_Type = _NEType
                    objProcess.CircleName = _CircleName
                    objProcess.Vendor = _vendor


                    '---------------------------------------------------------------------------------------------------------------------------------------
                    If (_CircleName <> "") And (_vendor <> "") And (_NEType = "SGSN") Then
                        AutoUpdateND111Mapping()
                    End If
                    '----------------------------------------------------------------------------------------------------------------------------------------

                Next
            Catch ex As Exception

                Dim ND111path As String = "D:\DCM\PACO_DCM\ND111\ND111_CIRCLE\" + System.DateTime.Now.ToString("dd-MM-yyyy") + "\"
                If (Not (Directory.Exists(ND111path))) Then
                    Directory.CreateDirectory(ND111path)
                End If
                File.AppendAllText(ND111path + _CircleName + ".txt", "Data is not selecting for ND111 , Plz ckeck ND111() function " + ex.Message + " " + System.DateTime.Now.ToString() + Environment.NewLine)
                ' MsgBox("ND111 Query is not runing")
            End Try

        End Function
        Public Function AutoUpdateND111Mapping() As Boolean
            Try
                If _vendor.Trim = "" Or _CircleName.Trim = "" Then
                    'Throw New Exception("Inalide Vendor / Group Circel / Circle")
                    Exit Function
                End If
                Dim dtOSS As DataTable = Nothing
                dtOSS = objSourcedb.ExecuteSQL("Select distinct c.circle_name,c.OSS_ConString from Circle_Master c inner join NE_Master n on c.Group_Circle=n.circle  Where c.Group_Circle='" & _CircleName & "' order by c.circle_name")
                If (dtOSS.Rows.Count > 0) Then
                    Dim i As Integer = 0
                    For Each dr As DataRow In dtOSS.Rows
                        _OSSName = dr("Circle_Name")
                        _ossConstring = dr("OSS_ConString").ToString()
yy:
                        Try
                            If i <= 5 Then
                                If _ossConstring.Trim <> "" Then
                                    GetND111(_ossConstring)
                                End If
                            Else
                                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.circle_master set Connection_String=' NOT Success', PT4_Profile='ND111 query is not execute after 5 attempted' where  OSS_ConString='" + _ossConstring + "' and Circle_Name='" + _OSSName + "' and Group_Circle='" + _CircleName + "'")
                                'if query is not execute after 5 attempted then delete data in current table if data is exist.
                                objSourcedb.ExecuteNonQuery("delete FROM awe_paco_dcm.tblnd111  where circle='" + _CircleName + "' and OSS='" + _OSSName + "'")
                                'Insert data into current table to backup table
                                objSourcedb.ExecuteNonQuery("insert into awe_paco_dcm.tblnd111 SELECT * FROM awe_paco_dcm.tblnd111_backup where circle='" + _CircleName + "' and OSS='" + _OSSName + "'")
                                'update staus
                                objSourcedb.ExecuteNonQuery("update awe_paco_dcm.circle_master set Task_Profile='DATA Uploaded' where  OSS_ConString='" + _ossConstring + "' and Circle_Name='" + _OSSName + "' and Group_Circle='" + _CircleName + "'")
                                UpdateND111(_CircleName, _OSSName, _vendor)
                                Return False
                            End If
                            i = 0
                            UpdateND111(_CircleName, _OSSName, _vendor)
                            objSourcedb.ExecuteNonQuery("update awe_paco_dcm.circle_master set Connection_String='Success', PT4_Profile='', Task_Profile='OK' where  OSS_ConString='" + _ossConstring + "' and Circle_Name='" + _OSSName + "' and Group_Circle='" + _CircleName + "'")
                        Catch ex As Exception
                            i += 1
                            System.Threading.Thread.Sleep(1000 * 60 * 1) ''when query fail then sleep one min. 

                            Dim ND111path As String = "D:\DCM\PACO_DCM\ND111\" + System.DateTime.Now.ToString("dd-MM-yyyy") + "\"
                            If (Not (Directory.Exists(ND111path))) Then
                                Directory.CreateDirectory(ND111path)
                            End If
                            File.AppendAllText(ND111path + _OSSName + ".txt", i.ToString() + " :- Query is not execute....., Plz ckeck ND111 Query " + ex.Message + " " + System.DateTime.Now.ToString() + Environment.NewLine)

                            GoTo yy
                        End Try
                    Next
                    Return True
                    'ORA-12170: TNS:Connect timeout occurred
                End If
            Catch ex As Exception

                'Throw ex

            End Try
        End Function

        Private Function GetND111(ByVal OSSConnectionString As String) As Boolean
            Try
                Dim SqlStr As String = String.Empty

                'SqlStr = "Select plmn_name,bsc_name,la_id_lac,ra_id_rac,nsei From (" & _
                '         " SELECT plmn.co_name plmn_name,bsc.co_name bsc_name,bcf.co_name bcf_name,bts.co_name bts_name,bsc.co_object_instance bsc_id,bcf.co_object_instance bcf_id,bsc.co_int_id bsc_int_id" & _
                '         " ,c_bts.segment_id segment_id,TO_NUMBER(SUBSTR(bts.co_object_instance,1,3)) bts_id,la_id_lac,ra_id_rac,cell_id,trx.co_object_instance trx_id,trx.co_gid trx_co_gid,bts.co_gid bts_co_gid" & _
                '         " ,DECODE(o_bcf_st.stateinmode1,'0','N','1','U','2','S','3','L') bcf_state,DECODE(o_bts_st.stateinmode1,'0','N','1','U','2','S','3','L') bts_state,DECODE(o_trx_st.stateinmode1,'0','N','1','U','2','S','3','L') trx_state" & _
                '         " ,bsic_bcc bcc,bsic_ncc ncc,c_trx.initial_frequency freq,DECODE(c_bts.hopping_mode,0,'None',1,'Baseband',2,'Rf ('||TO_CHAR(c_bts.used_mobile_alloc_list_id)||')','???') list_idt" & _
                '         " ,DECODE(c_bts.hopping_mode,0,'None',1,'Baseband',2,TO_CHAR(c_bts.used_mobile_alloc_list_id),'???') list_id" & _
                '         " ,decode(c_bts.hopping_mode,0,'None',1,to_char(c_bts.used_mobile_alloc_list_id),2,to_char(c_bts.used_mobile_alloc_list_id),'???') mal_id,DECODE(c_bts.hopping_mode,0,'None',1,'Baseband',2,'Rf','???') hopping" & _
                '         " ,DECODE(c_bts.USED_MOBILE_ALLOC_ID_USED,0,'--',TO_CHAR(c_bts.used_mobile_alloc_list_id,'9990')) MA_list_id,c_bts.maio_offset,c_bts.maio_step,'       ' tsl0_type" & _
                '         " '       ' tsl1_type,c_bts.nsei nsei,c_trx.dap_id dap_id" & _
                '         " FROM  " & _
                '         " roh_c_trx c_trx,roh_c_bts c_bts,roh_c_bcf c_bcf,utp_common_objects trx,utp_common_objects bts,utp_common_objects bcf,utp_common_objects bsc,utp_common_objects plmn" & _
                '         " ,object_modestates o_bcf_st,object_modestates o_bts_st,object_modestates o_trx_st" & _
                '         " WHERE" & _
                '         " c_trx.valid_finish_time > sysdate  and c_bts.valid_finish_time > sysdate and c_bcf.valid_finish_time > sysdate and bsc.co_oc_id = 3" & _
                '         " and bcf.co_oc_id = 27 and bts.co_oc_id = 4 and trx.co_oc_id=24 and plmn.co_oc_id=16 " & _
                '         " AND trx.co_gid = c_trx.co_gid       AND bts.co_gid = c_bts.co_gid AND bcf.co_gid = c_bcf.co_gid AND trx.co_parent_gid = bts.co_gid AND bts.co_parent_gid = bcf.co_gid" & _
                '         " AND bcf.co_parent_gid = bsc.co_gid  AND bsc.co_parent_gid = plmn.co_gid AND o_bcf_st.int_id = bcf.co_int_id AND o_bts_st.int_id = bts.co_int_id" & _
                '         " AND o_trx_st.int_id = trx.co_int_id " & _
                '         " AND DECODE(c_bts.hopping_mode,0,'None',1,'Baseband',2,'Rf ('||TO_CHAR(c_bts.used_mobile_alloc_list_id)||')','???') != '1=1' " & _
                '         " ) ND111 Group By plmn_name,bsc_name,la_id_lac,ra_id_rac,nsei"


                'SqlStr = "Select plmn_name,bsc_name,la_id_lac,ra_id_rac,nsei From (" & _
                '         " SELECT " & _
                '         " plmn.co_name plmn_name" & _
                '         "  ,bsc.co_name bsc_name" & _
                '                        " ,bcf.co_name bcf_name" & _
                '                        " ,bts.co_name bts_name " & _
                '                       "  ,bsc.co_object_instance bsc_id " & _
                '                       "  ,bcf.co_object_instance bcf_id " & _
                '                       "  ,bsc.co_int_id bsc_int_id " & _
                '                       "  ,c_bts.segment_id segment_id " & _
                '                        " ,TO_NUMBER(SUBSTR(bts.co_object_instance,1,3)) bts_id " & _
                '                        " ,la_id_lac " & _
                '                        " ,ra_id_rac " & _
                '                        " ,cell_id " & _
                '                        " ,trx.co_object_instance trx_id " & _
                '                        " ,trx.co_gid trx_co_gid " & _
                '                        " ,bts.co_gid bts_co_gid " & _
                '                        " ,DECODE(o_bcf_st.stateinmode1,'0','N','1','U','2','S','3','L') bcf_state " & _
                '                        " ,DECODE(o_bts_st.stateinmode1,'0','N','1','U','2','S','3','L') bts_state " & _
                '                        " ,DECODE(o_trx_st.stateinmode1,'0','N','1','U','2','S','3','L') trx_state " & _
                '                        " ,bsic_bcc bcc " & _
                '                       "  ,bsic_ncc ncc " & _
                '                        " ,c_trx.initial_frequency freq " & _
                '                        " ,DECODE(c_bts.hopping_mode,0,'None',1,'Baseband',2,'Rf ('||TO_CHAR(c_bts.used_mobile_alloc_list_id)||')','???') list_idt " & _
                '                       "  ,DECODE(c_bts.hopping_mode,0,'None',1,'Baseband',2,TO_CHAR(c_bts.used_mobile_alloc_list_id),'???') list_id " & _
                '                       "  ,decode(c_bts.hopping_mode,0,'None',1,to_char(c_bts.used_mobile_alloc_list_id),2,to_char(c_bts.used_mobile_alloc_list_id),'???') mal_id " & _
                '                       "  ,DECODE(c_bts.hopping_mode,0,'None',1,'Baseband',2,'Rf','???') hopping " & _
                '                       "  ,DECODE(c_bts.USED_MOBILE_ALLOC_ID_USED,0,'--',TO_CHAR(c_bts.used_mobile_alloc_list_id,'9990')) MA_list_id " & _
                '                       "  ,c_bts.maio_offset " & _
                '                       "  ,c_bts.maio_step " & _
                '                      "   ,'       ' tsl0_type " & _
                '                      "   ,'       ' tsl1_type " & _
                '                      "   ,c_bts.nsei nsei " & _
                '                       "  ,c_trx.dap_id dap_id " & _
                '                        "  FROM " & _
                '                       "  roh_c_trx c_trx " & _
                '                      "   ,roh_c_bts c_bts " & _
                '                       "  ,roh_c_bcf c_bcf " & _
                '                       "  ,utp_common_objects trx " & _
                '                       "  ,utp_common_objects bts " & _
                '                       "  ,utp_common_objects bcf " & _
                '                       "  ,utp_common_objects bsc " & _
                '                       "   ,utp_common_objects plmn " & _
                '                       "   ,object_modestates o_bcf_st " & _
                '                       "  ,object_modestates o_bts_st " & _
                '                       "  ,object_modestates o_trx_st " & _
                '                        "  WHERE " & _
                '                        "  c_trx.valid_finish_time > sysdate " & _
                '                        "  and c_bts.valid_finish_time > sysdate " & _
                '                        "  and c_bcf.valid_finish_time > sysdate " & _
                '                        "  and bsc.co_oc_id = 3 " & _
                '                        "  and bcf.co_oc_id = 27 " & _
                '                        " and bts.co_oc_id = 4 " & _
                '                       "  and trx.co_oc_id=24 " & _
                '                       "  and plmn.co_oc_id=16 " & _
                '                       "   AND trx.co_gid = c_trx.co_gid " & _
                '                       "   AND bts.co_gid = c_bts.co_gid " & _
                '                       "   AND bcf.co_gid = c_bcf.co_gid " & _
                '                       "   AND trx.co_parent_gid = bts.co_gid " & _
                '                       "   AND bts.co_parent_gid = bcf.co_gid " & _
                '                       "   AND bcf.co_parent_gid = bsc.co_gid " & _
                '                       "   AND bsc.co_parent_gid = plmn.co_gid " & _
                '                       "   AND o_bcf_st.int_id = bcf.co_int_id " & _
                '                       "   AND o_bts_st.int_id = bts.co_int_id " & _
                '                       "   AND o_trx_st.int_id = trx.co_int_id " & _
                '                       "   AND DECODE(c_bts.hopping_mode,0,'None',1,'Baseband',2,'Rf ('||TO_CHAR(c_bts.used_mobile_alloc_list_id)||')','???') != '1=1'  " & _
                '                       "  ) ND111  " & _
                '                       "  Where nsei >= 0 " & _
                '                       "  Group By  " & _
                '                       "    plmn_name, " & _
                '                       "  bsc_name, " & _
                '                       "  la_id_lac, " & _
                '                       "   ra_id_rac, " & _
                '                       "  nsei "


                SqlStr = "Select plmn_name,bsc_name,la_id_lac,ra_id_rac,nsei From (" & _
                         " SELECT " & _
                         " plmn.co_name plmn_name" & _
                         "  ,bsc.co_name bsc_name" & _
                                        " ,bcf.co_name bcf_name" & _
                                        " ,bts.co_name bts_name " & _
                                       "  ,bsc.co_object_instance bsc_id " & _
                                       "  ,bcf.co_object_instance bcf_id " & _
                                       "  ,bsc.co_int_id bsc_int_id " & _
                                       "  ,c_bts.segment_id segment_id " & _
                                        " ,TO_NUMBER(SUBSTR(bts.co_object_instance,1,3)) bts_id " & _
                                        " ,la_id_lac " & _
                                        " ,ra_id_rac " & _
                                        " ,cell_id " & _
                                        " ,trx.co_object_instance trx_id " & _
                                        " ,trx.co_gid trx_co_gid " & _
                                        " ,bts.co_gid bts_co_gid " & _
                                        " ,DECODE(o_bcf_st.stateinmode1,'0','N','1','U','2','S','3','L') bcf_state " & _
                                        " ,DECODE(o_bts_st.stateinmode1,'0','N','1','U','2','S','3','L') bts_state " & _
                                        " ,DECODE(o_trx_st.stateinmode1,'0','N','1','U','2','S','3','L') trx_state " & _
                                        " ,bsic_bcc bcc " & _
                                       "  ,bsic_ncc ncc " & _
                                        " ,c_trx.initial_frequency freq " & _
                                        " ,DECODE(c_bts.hopping_mode,0,'None',1,'Baseband',2,'Rf ('||TO_CHAR(c_bts.used_mobile_alloc_list_id)||')','???') list_idt " & _
                                       "  ,DECODE(c_bts.hopping_mode,0,'None',1,'Baseband',2,TO_CHAR(c_bts.used_mobile_alloc_list_id),'???') list_id " & _
                                       "  ,decode(c_bts.hopping_mode,0,'None',1,to_char(c_bts.used_mobile_alloc_list_id),2,to_char(c_bts.used_mobile_alloc_list_id),'???') mal_id " & _
                                       "  ,DECODE(c_bts.hopping_mode,0,'None',1,'Baseband',2,'Rf','???') hopping " & _
                                       "  ,DECODE(c_bts.USED_MOBILE_ALLOC_ID_USED,0,'--',TO_CHAR(c_bts.used_mobile_alloc_list_id,'9990')) MA_list_id " & _
                                       "  ,c_bts.maio_offset " & _
                                       "  ,c_bts.maio_step " & _
                                      "   ,'       ' tsl0_type " & _
                                      "   ,'       ' tsl1_type " & _
                                      "   ,c_bts.nsei nsei " & _
                                       "  ,c_trx.dap_id dap_id " & _
                                        "  FROM " & _
                                       "  roh_c_trx c_trx " & _
                                      "   ,roh_c_bts c_bts " & _
                                       "  ,roh_c_bcf c_bcf " & _
                                       "  ,utp_common_objects trx " & _
                                       "  ,utp_common_objects bts " & _
                                       "  ,utp_common_objects bcf " & _
                                       "  ,utp_common_objects bsc " & _
                                       "   ,utp_common_objects plmn " & _
                                       "   ,object_modestates o_bcf_st " & _
                                       "  ,object_modestates o_bts_st " & _
                                       "  ,object_modestates o_trx_st " & _
                                        "  WHERE " & _
                                        "   bsc.co_oc_id = 3 " & _
                                        "  and bcf.co_oc_id = 27 " & _
                                        " and bts.co_oc_id = 4 " & _
                                       "  and trx.co_oc_id=24 " & _
                                       "  and plmn.co_oc_id=16 " & _
                                       "   AND trx.co_gid = c_trx.co_gid " & _
                                       "   AND bts.co_gid = c_bts.co_gid " & _
                                       "   AND bcf.co_gid = c_bcf.co_gid " & _
                                       "   AND trx.co_parent_gid = bts.co_gid " & _
                                       "   AND bts.co_parent_gid = bcf.co_gid " & _
                                       "   AND bcf.co_parent_gid = bsc.co_gid " & _
                                       "   AND bsc.co_parent_gid = plmn.co_gid " & _
                                       "   AND o_bcf_st.int_id = bcf.co_int_id " & _
                                       "   AND o_bts_st.int_id = bts.co_int_id " & _
                                       "   AND o_trx_st.int_id = trx.co_int_id " & _
                                       "   AND DECODE(c_bts.hopping_mode,0,'None',1,'Baseband',2,'Rf ('||TO_CHAR(c_bts.used_mobile_alloc_list_id)||')','???') != '1=1'  " & _
                                       "  ) ND111  " & _
                                       "  Where nsei >= 0 " & _
                                       "  Group By  " & _
                                       "    plmn_name, " & _
                                       "  bsc_name, " & _
                                       "  la_id_lac, " & _
                                       "   ra_id_rac, " & _
                                       "  nsei "

                Dim DtND As DataTable = Nothing
                objOracleDb = New Global.NSN.GNOCN.Tools.SQL.DbLayer.DbConnectLayer(OSSConnectionString, DbType.OracleType)
                DtND = objOracleDb.ExecuteSQL(SqlStr)

                If (DtND.Rows.Count > 0) Then
                    ' here  delete tblnd111 table 
                    ' objSourcedb.ExecuteNonQuery("delete from tblnd111 where circle='" & _CircleName.ToString() & "' and oss='" & _OSSName.ToString() & "' and vendor='" & _vendor.ToString() & "'")
                    For Each dr As DataRow In DtND.Rows

                        Dim SqlStr111 As String = String.Empty
                        SqlStr111 = "Insert into tblnd111(VENDOR, CIRCLE, OSS, PLMN_NAME, BSC_NAME, LAC, RAC, NSEI)" & _
                                 " Values('" & _vendor & "','" & _CircleName & "','" & _OSSName & "','" & dr("plmn_name") & "','" & dr("bsc_name") & "','" & dr("la_id_lac") & "','" & dr("ra_id_rac") & "','" & dr("nsei").ToString & "')"

                        objSourcedb.ExecuteNonQuery(SqlStr111)
                    Next
                End If

            Catch ex As Exception
                'Throw ex
            End Try
        End Function


    End Class
End Namespace
