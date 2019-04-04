Imports MySql.Data.MySqlClient
Imports System.Data.SqlClient

Namespace NSN.GNOCN.Tools.AWE.NSS


    Public Class clsTransferToNSSFromPS

        'public static MySqlConnection con21 = new MySqlConnection("Data Source=93.183.30.154;database=awe_paco_dcm;User Id=root;Password=tiger;allow zero datetime=true;Persist Security Info=true;Connection Timeout = 5000;");
        Public Shared con21 As MySqlConnection = New MySqlConnection(System.Configuration.ConfigurationSettings.AppSettings("PSDBConstring").ToString())
        Shared cmd As MySqlCommand = Nothing
        Shared da As MySqlDataAdapter = Nothing

        Public Shared conS As SqlConnection = New SqlConnection(System.Configuration.ConfigurationSettings.AppSettings("SQLDBConstring").ToString())
        Shared cmdS As SqlCommand = Nothing
        Shared daS As SqlDataAdapter = Nothing

        Public Shared Sub dcm_update()

            Try
                If (Not conS.State = ConnectionState.Open) Then
                    conS.Open()
                End If
                cmdS = New SqlCommand("delete from tblsgsn_nsvci_temp", conS)
                cmdS.ExecuteNonQuery()

                Dim dt As DataTable = New DataTable()
                cmd = New MySqlCommand("select * from awe_paco_dcm.tblsgsn_nsvci", con21)
                If (Not con21.State = ConnectionState.Open) Then
                    con21.Open()

                End If
                da = New MySqlDataAdapter(cmd)
                da.Fill(dt)
                Dim command As String = "insert into tblsgsn_nsvci_temp values"
                Dim rows As String = String.Empty
                Dim count As Integer = 0
                For Each dr As DataRow In dt.Rows

                    count = count + 1
                    rows += "('" & dr(0).ToString() & "','" + dr(1).ToString() + "','" + dr(2).ToString() + "','" + dr(3).ToString() + "','" + dr(4).ToString() + "','" + dr(5).ToString() + "','" + dr(6).ToString() + "','" + dr(7).ToString() + "','" + dr(8).ToString() + "','" + dr(9).ToString() + "','" + dr(10).ToString() + "','" + dr(11).ToString() + "','" + dr(12).ToString() + "','" + dr(13).ToString() + "','" + dr(14).ToString() + "','" + dr(15).ToString() + "','" + dr(16).ToString() + "','" + dr(17).ToString() + "','" + dr(18).ToString() + "','" + dr(19).ToString() + "','" + dr(20).ToString() + "','" + dr(21).ToString() + "','" + dr(22).ToString() + "'),"
                    If (count <= 1000) Then
                        cmdS = New SqlCommand("insert into tblsgsn_nsvci_temp values" + rows.TrimEnd(",").Trim, conS)
                        cmdS.ExecuteNonQuery()
                        rows = String.Empty
                        count = 0
                    End If

                Next
                If (Not rows = String.Empty) Then
                    cmdS = New SqlCommand("insert into tblsgsn_nsvci_temp values" + rows.TrimEnd(",").Trim, conS)
                    cmdS.ExecuteNonQuery()
                End If
                cmdS = New SqlCommand("delete from tblsgsn_nsvci", conS)
                cmdS.ExecuteNonQuery()
                cmdS = New SqlCommand("insert into tblsgsn_nsvci select * from tblsgsn_nsvci_temp", conS)
                cmdS.ExecuteNonQuery()
            Catch ex As Exception
                'Throw ex
            Finally
                If conS.State = ConnectionState.Open Then
                    conS.Close()
                End If
                If con21.State = ConnectionState.Open Then
                    con21.Close()
                End If
                System.Threading.Thread.Sleep(60 * 1000 * 2)
            End Try
            
        End Sub

    End Class
End Namespace