Imports NSN.GNOCN.Tools.AWE.NSS
Imports NSN.GNOCN.Tools.SQL.DbLayer
Public Class Form1

    Private objSourcedbPS As Global.NSN.GNOCN.Tools.SQL.DbLayer.DbConnectLayer
    Private PSConstring As String = System.Configuration.ConfigurationSettings.AppSettings("PSDBConstring").ToString()
    Private WithEvents tmrPACO As System.Timers.Timer
    Private WithEvents tmrSQLUploadPS As System.Timers.Timer
    Private WithEvents tmrSQLUploadCS As System.Timers.Timer
    Private objSourcedbCS As Global.NSN.GNOCN.Tools.SQL.DbLayer.DbConnectLayer

    Private CSConstring As String = System.Configuration.ConfigurationSettings.AppSettings("CSDBConstring").ToString()
    Protected Sub OnStart(ByVal args() As String)
        Try
            'LogEvent("AWE DCM PACO Sercice getting started...", EventLogEntryType.Information)

            'Debugger.Launch()
            objSourcedbPS = New Global.NSN.GNOCN.Tools.SQL.DbLayer.DbConnectLayer(PSConstring, DbType.MySqlType)
            objSourcedbCS = New Global.NSN.GNOCN.Tools.SQL.DbLayer.DbConnectLayer(CSConstring, DbType.MySqlType)

            'tmrPACO = New System.Timers.Timer

            'tmrPACO.Interval = 10000

            'tmrPACO.AutoReset = True

            'tmrPACO.Enabled = True

            'tmrPACO.Start()

            'tmrSQLUploadPS = New System.Timers.Timer

            'tmrSQLUploadPS.Interval = 10000

            'tmrSQLUploadPS.AutoReset = True

            'tmrSQLUploadPS.Enabled = True

            'tmrSQLUploadPS.Start()

            tmrSQLUploadCS = New System.Timers.Timer

            tmrSQLUploadCS.Interval = 10000

            tmrSQLUploadCS.AutoReset = True

            tmrSQLUploadCS.Enabled = True

            tmrSQLUploadCS.Start()

            ' LogEvent("AWE DCM PACO Upload service started successfully...", EventLogEntryType.Information)
        Catch ex As Exception
            'LogEvent("AWE DCM PACO Upload service [Error while starting the service: " & ex.Message & "]", EventLogEntryType.Error)
        End Try
    End Sub
    Public Sub SQLUploaCSdMainFunction(ByRef errormsg As String, ByVal circle As String)
        Try
            clsTransferToNSSFromCS.DCMDataMigration(circle)
        Catch ex As Exception

        End Try
    End Sub
    Dim errormsg As String
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        objSourcedbCS = New Global.NSN.GNOCN.Tools.SQL.DbLayer.DbConnectLayer(CSConstring, DbType.MySqlType)
        Dim dtcircle As DataTable = Nothing
        Dim dt As DataTable = Nothing
        dtcircle = objSourcedbCS.ExecuteSQL("SELECT distinct circle FROM tblprocessupdate")
        If dtcircle IsNot Nothing Then
            If dtcircle.Rows.Count > 0 Then
                For Each dr As DataRow In dtcircle.Rows
                    dt = Nothing
                    dt = objSourcedbCS.ExecuteSQL("SELECT CIRCLE FROM tblprocessupdate where status='NOT COMPLETED' and circle = '" & dr("Circle").ToString.Trim & "'")
                    If dt IsNot Nothing Then
                        If dt.Rows.Count > 0 Then
                            objSourcedbCS.ExecuteNonQuery("Update tblprocessupdate Set status='In Progress',starttime='" & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & "',EndTime='0000-00-00 00:00:00' Where circle='" & dr("Circle").ToString.Trim & "'")
                            SQLUploaCSdMainFunction(errormsg, dr("Circle").ToString.Trim)
                            objSourcedbCS.ExecuteNonQuery("Update tblprocessupdate Set status='COMPLETED',EndTime='" & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & "' Where circle='" & dr("Circle").ToString.Trim & "'")
                        End If
                    End If
                Next
            End If
        End If
    End Sub
End Class
