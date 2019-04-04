Imports System.IO
Imports System.Windows.Forms
Imports System.Configuration
Namespace NSN.GNOCN.Tools.Logs

    Public Class DebugLogs
        Shared _strFileNameRemark As String
        Public Sub New(ByVal strFileNameRemark As String)
            _strFileNameRemark = strFileNameRemark
        End Sub
        Public Sub WriteDebugLog(ByVal pWriteCustomErrorMessage As String)
            Try
                If System.Configuration.ConfigurationManager.AppSettings("EnableDebugLogs").ToString().ToUpper() = "YES" Then
                    'Dim pFileNamePathString As String = Application.StartupPath & "\" & "log" & "\" & "Log_" & Now().ToString("yyyy_MM_dd") & "_" & _strFileNameRemark & ".log"
                    Dim pFileNamePathString As String = "D:\GNOCTools\GNOCTools_Prod\" & "log" & "\" & "Log_" & Now().ToString("yyyy_MM_dd") & "_" & _strFileNameRemark & ".log"
                    WriteLogToFile(pFileNamePathString, pWriteCustomErrorMessage)
                End If
            Catch ex As Exception
                '' not able to handel this exception...
            End Try
        End Sub

        Private Sub WriteLogToFile(ByVal pFileNamePathString As String, _
                                            ByVal pWriteCustomErrorMessage As String)

            Static intRecCount As Integer = 0

            If intRecCount > 2 Then
                Exit Sub
            Else
                intRecCount = intRecCount + 1
            End If

            Dim LogMessageString As String
            If Not System.IO.Directory.Exists(Path.GetDirectoryName(pFileNamePathString)) Then
                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(pFileNamePathString))
            End If
            Try
                Dim ObjectFileStream As New FileStream(pFileNamePathString, FileMode.Append, FileAccess.Write, FileShare.None)
                Dim ObjectStreamWriter As New StreamWriter(ObjectFileStream)
                LogMessageString = "[" & DateTime.Now.ToString() & "]: " & pWriteCustomErrorMessage & vbCrLf

                ObjectStreamWriter.WriteLine(LogMessageString)
                If Not ObjectStreamWriter Is Nothing Then
                    ObjectStreamWriter.Flush()
                    ObjectStreamWriter.Close()
                End If
                If Not ObjectFileStream Is Nothing Then
                    ObjectFileStream.Close()
                End If
            Catch exError As Exception
                'pErrorMessageString = "Unable to write to Application Log File. Contact your Application Administrator." & exError.Message
                Dim pFileNamePathStringError As String = Application.StartupPath & "\" & "log" & "\" & "Log_" & Now().ToString("yyyy_MM_dd") & "_" & _strFileNameRemark & "_" & TimeOfDay.Ticks & " .log"
                WriteLogToFile(pFileNamePathStringError, pWriteCustomErrorMessage)
            End Try
        End Sub

    End Class

End Namespace