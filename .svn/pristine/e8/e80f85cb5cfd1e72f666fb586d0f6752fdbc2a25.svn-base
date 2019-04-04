Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Data
Imports System.Data.Common

Namespace NSN.GNOCN.Tools.SQL.DbLayer




    Public Enum DbType
        SqlType = 1
        MySqlType = 2
        OracleType = 3
    End Enum

    Public Interface IDbFactory

        Function CreateInstanceDataAdapter(ByVal Dbtype As DbType) As DbDataAdapter
        'DbDataAdapter CreateInstanceDataAdapter(DbType Dbtype,DbCommand cmd);
        'DbDataAdapter CreateInstanceDataAdapter(DbType Dbtype,string cmdText,DbConnection con);
        Function CreateInstanceDataAdapter(ByVal Dbtype As DbType, ByVal cmdText As String, ByVal conString As String) As DbDataAdapter
        Function CreateInstanceDbCommandBuilder(ByVal Dbtype As DbType) As DbCommandBuilder
        Function CreateInstanceDbCommandBuilder(ByVal Dbtype As DbType, ByVal da As DbDataAdapter) As DbCommandBuilder
        Function CreateInstanceDbCommand(ByVal Dbtype As DbType) As DbCommand
        Function CreateInstanceDbCommand(ByVal Dbtype As DbType, ByVal cmdText As String, ByVal con As DbConnection) As DbCommand
        Function CreateInstanceDbConnection(ByVal Dbtype As DbType) As DbConnection
        Function CreateInstanceDbConnection(ByVal Dbtype As DbType, ByVal strConString As String) As DbConnection
        'DbParameterCollection CreateInstanceDbParameterCollection(DbType Dbtype);
        Function CreateInstanceDbParameter(ByVal Dbtype As DbType) As DbParameter
        ' void CreateInstanceDbTransaction(ref IDbTransaction obj, DbType Dbtype);
 End Interface
End Namespace