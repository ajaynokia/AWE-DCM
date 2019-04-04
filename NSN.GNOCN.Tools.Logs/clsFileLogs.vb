Imports System.IO
Imports System.Windows.Forms

Namespace NSN.GNOCN.Tools.Logs
    Public Class clsFileLogs
        Shared _strFileNameRemark As String
        Public Sub New(ByVal strFileNameRemark As String)
            _strFileNameRemark = strFileNameRemark
        End Sub

        Public Sub WriteLogToFile(ByVal pWriteCustomErrorMessage As String)
            Try
                Dim pFileNamePathString As String = Application.StartupPath & "\" & "log" & "\" & "Log_" & Now().ToString("yyyy_MM_dd") & "_" & _strFileNameRemark & ".log"
                WriteLogToFile(pFileNamePathString, pWriteCustomErrorMessage)
            Catch ex As Exception
                '' not able to handel this exception...
            End Try
        End Sub
        Public Sub WriteDataToFile(ByVal Data As String, ByVal pStringData As String, Optional pStringData1 As String = "")
            Dim ExceptionMessageString As String = String.Empty
            Dim pFileNamePathString As String = String.Empty
            If pStringData1 <> "" Then
                pFileNamePathString = Application.StartupPath & "\" & "Data" & "\" & Now.ToString("yyyy_MM_dd") & "\" & "Data_" & pStringData & "_" & pStringData1 & " .log"
            Else
                pFileNamePathString = Application.StartupPath & "\" & "Data" & "\" & Now.ToString("yyyy_MM_dd") & "\" & "Data_" & pStringData & "_" & pStringData1 & " .log"
            End If

            If Not System.IO.Directory.Exists(Path.GetDirectoryName(pFileNamePathString)) Then
                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(pFileNamePathString))
            End If
            Try
                Dim ObjectFileStream As New FileStream(pFileNamePathString, FileMode.Append, FileAccess.Write, FileShare.None)
                Dim ObjectStreamWriter As New StreamWriter(ObjectFileStream)
                ExceptionMessageString = Data
                ObjectStreamWriter.WriteLine(ExceptionMessageString)
                If Not ObjectStreamWriter Is Nothing Then
                    ObjectStreamWriter.Flush()
                    ObjectStreamWriter.Close()
                End If
                If Not ObjectFileStream Is Nothing Then
                    ObjectFileStream.Close()
                End If
            Catch exError As Exception

            End Try
        End Sub
        Public Sub WriteLogDataToFile(ByVal FolderPath As String, ByVal Data As String)
            Dim ExceptionMessageString As String = String.Empty
            Dim pFileNamePathString As String = String.Empty

            pFileNamePathString = FolderPath

            If Not System.IO.Directory.Exists(Path.GetDirectoryName(pFileNamePathString)) Then
                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(pFileNamePathString))
            End If
            Try
                Dim ObjectFileStream As New FileStream(pFileNamePathString, FileMode.Append, FileAccess.Write, FileShare.None)
                Dim ObjectStreamWriter As New StreamWriter(ObjectFileStream)
                ExceptionMessageString = Data
                ObjectStreamWriter.WriteLine(ExceptionMessageString)
                If Not ObjectStreamWriter Is Nothing Then
                    ObjectStreamWriter.Flush()
                    ObjectStreamWriter.Close()
                End If
                If Not ObjectFileStream Is Nothing Then
                    ObjectFileStream.Close()
                End If
            Catch exError As Exception

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
                LogMessageString = pWriteCustomErrorMessage
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
        Public Sub WriteEventToFile(ByVal Data As String, ByVal pStringData As String, Optional ByVal pStringData1 As String = "")
            Dim ExceptionMessageString As String
            Dim pFileNamePathString As String
            If pStringData1 <> "" Then
                pFileNamePathString = Application.StartupPath & "\" & "Events" & "\" & Now.ToString("yyyy-MM-dd") & "\" & "Event_" & pStringData & "_" & pStringData1 & " .log"
            Else
                pFileNamePathString = Application.StartupPath & "\" & "Events" & "\" & Now.ToString("yyyy-MM-dd") & "\" & "Event_" & pStringData & " .log"
            End If

            If Not System.IO.Directory.Exists(Path.GetDirectoryName(pFileNamePathString)) Then
                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(pFileNamePathString))
            End If
            Try
                Dim ObjectFileStream As New FileStream(pFileNamePathString, FileMode.Append, FileAccess.Write, FileShare.None)
                Dim ObjectStreamWriter As New StreamWriter(ObjectFileStream)
                ExceptionMessageString = Data
                ObjectStreamWriter.WriteLine(ExceptionMessageString)
                If Not ObjectStreamWriter Is Nothing Then
                    ObjectStreamWriter.Flush()
                    ObjectStreamWriter.Close()
                End If
                If Not ObjectFileStream Is Nothing Then
                    ObjectFileStream.Close()
                End If
            Catch exError As Exception

            End Try
        End Sub
    End Class

End Namespace
