namespace QueryFirst
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;

    public interface IScaffoldInsert
    {

        List<ScaffoldInsertResults> Execute(string table_name);
        IEnumerable<ScaffoldInsertResults> Execute(string table_name, IDbConnection conn);
        ScaffoldInsertResults GetOne(string table_name);
        ScaffoldInsertResults GetOne(string table_name, IDbConnection conn);
        System.String ExecuteScalar(string table_name);
        System.String ExecuteScalar(string table_name, IDbConnection conn);
        ScaffoldInsertResults Create(IDataRecord record);
        int ExecuteNonQuery(string table_name);
        int ExecuteNonQuery(string table_name, IDbConnection conn);
    }
    public partial class ScaffoldInsert : IScaffoldInsert
    {
        public virtual int ExecuteNonQuery(string table_name)
        {
            using (IDbConnection conn = QfRuntimeConnection.GetConnection())
            {
                conn.Open();
                return ExecuteNonQuery(table_name, conn);
            }
        }
        public virtual int ExecuteNonQuery(string table_name, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            cmd.CommandText = getCommandText();
            AddAParameter(cmd, "VarChar", "@table_name", table_name, 776);
            return cmd.ExecuteNonQuery();
        }
        private string getCommandText()
        {
            Stream strm = typeof(ScaffoldInsertResults).Assembly.GetManifestResourceStream("QueryFirst.InsertsAndUpdates.ScaffoldInsert.sql");
            string queryText = new StreamReader(strm).ReadToEnd();
//Comments inverted at runtime in debug, pre-build in release
queryText = queryText.Replace("-- designTime", "/*designTime");
queryText = queryText.Replace("-- endDesignTime", "endDesignTime*/");
queryText = queryText.Replace("--designTime", "/*designTime");
queryText = queryText.Replace("--endDesignTime", "endDesignTime*/");
            return queryText;
        }
        private void AddAParameter(IDbCommand Cmd, string DbType, string DbName, object Value, int Length)
        {
            var dbType = (SqlDbType)System.Enum.Parse(typeof(SqlDbType), DbType);
            SqlParameter myParam;
            if (Length != 0)
            {
                myParam = new SqlParameter(DbName, dbType, Length);
            }
            else
            {
                myParam = new SqlParameter(DbName, dbType);
            }
            myParam.Value = Value != null ? Value : DBNull.Value;
            Cmd.Parameters.Add(myParam);
        }
        public virtual List<ScaffoldInsertResults> Execute(string table_name)
        {
            using (IDbConnection conn = QfRuntimeConnection.GetConnection())
            {
                conn.Open();
                return Execute(table_name, conn).ToList();
            }
        }
        public virtual IEnumerable<ScaffoldInsertResults> Execute(string table_name, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            cmd.CommandText = getCommandText();
            AddAParameter(cmd, "VarChar", "@table_name", table_name, 776);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    yield return Create(reader);
                }
            }
        }
        public virtual ScaffoldInsertResults GetOne(string table_name)
        {
            using (IDbConnection conn = QfRuntimeConnection.GetConnection())
            {
                conn.Open();
                return GetOne(table_name, conn);
            }
        }
        public virtual ScaffoldInsertResults GetOne(string table_name, IDbConnection conn)
        {
            var all = Execute(table_name, conn);
            using (IEnumerator<ScaffoldInsertResults> iter = all.GetEnumerator())
            {
                iter.MoveNext();
                return iter.Current;
            }
        }
        public virtual System.String ExecuteScalar(string table_name)
        {
            using (IDbConnection conn = QfRuntimeConnection.GetConnection())
            {
                conn.Open();
                return ExecuteScalar(table_name, conn);
            }
        }
        public virtual System.String ExecuteScalar(string table_name, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            cmd.CommandText = getCommandText();
            AddAParameter(cmd, "VarChar", "@table_name", table_name, 776);
            var result = cmd.ExecuteScalar();
            if (result == DBNull.Value)
                return null;
            else
                return (System.String)result;
        }
        public virtual ScaffoldInsertResults Create(IDataRecord record)
        {
            var returnVal = CreatePoco(record);
            if (record[0] != null && record[0] != DBNull.Value)
                returnVal.MyInsertStatement = (string)record[0];
            returnVal.OnLoad();
            return returnVal;
        }
    }
    public partial class ScaffoldInsertResults
    {
        public string MyInsertStatement; //(varchar null)
    }
}
