﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;

namespace jigsaw.Model
{
    public class MySQLDriver
    {
        #region SQL Commands
        private static String GET_TABLES =
            @"SELECT table_name, table_rows as size
            FROM INFORMATION_SCHEMA.tables
            WHERE tables.table_schema = 'jigsawtest';";
        private static String GET_FK =
            @"SELECT A.table_name, referenced_table_name as foreign_key
            FROM INFORMATION_SCHEMA.tables as A natural join INFORMATION_SCHEMA.key_column_usage
            WHERE A.table_schema = 'jigsawtest' and referenced_table_name is not NULL";
        #endregion

        private static String CONNECTION_STRING = "server=127.0.0.1;userid=root;database=jigsawtest";
        private static String TABLE_NAME = "table_name";
        private static String SIZE = "size";
        private static String FOREIGN_KEY= "foreign_key";

        private static MySQLDriver instance;

        private MySQLDriver()
        {
            
        }

        public static MySQLDriver getInstance() 
        {
            if (instance == null)
            {
                instance = new MySQLDriver();
            }

            return instance;
        }

        public List<Table> GetSchema()
        {
            List<Table> result = new List<Table>();
            Dictionary<string, Table> instantiated = new Dictionary<string, Table>();

		    MySqlConnection sqlConn = new MySqlConnection(CONNECTION_STRING);
		    MySqlCommand command = sqlConn.CreateCommand();
		    MySqlDataReader reader = null;
            command.CommandText = GET_TABLES;

            try
            {
                sqlConn.Open();
                reader = command.ExecuteReader();

                //get tables
                while (reader.Read())
                {
                    Table t = new Table(reader.GetString(TABLE_NAME), reader.GetInt32(SIZE));
                    result.Add(t);
                    instantiated.Add(reader.GetString(TABLE_NAME), t);
                }

                //get foreign keys
                reader.Close();
                command.CommandText = GET_FK;
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Table table = instantiated[reader.GetString(TABLE_NAME)];
                    Table referencedTable = instantiated[reader.GetString(FOREIGN_KEY)];
                    table.ForeignKey.Add(referencedTable);
                }
                reader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                sqlConn.Close();
            }
            return result;
        }

        public Table JoinTables(Table a, Table b)
        {
            Table result = null;

            String query = String.Format("SELECT * FROM {0} NATURAL JOIN {1};", a.Name, b.Name);
            String newName = String.Format("{0}_and_{1}", a.Name, b.Name);
            
            MySqlConnection sqlConn = new MySqlConnection(CONNECTION_STRING);
            MySqlCommand command = sqlConn.CreateCommand();
            MySqlDataReader reader = null;
            command.CommandText = query;

            try
            {
                sqlConn.Open();
                reader = command.ExecuteReader();

                result = new Table(newName, 0);
                result.Columns = getColumnNames(reader.GetSchemaTable());
                while (reader.Read())
                {
                    object[] values = new object[result.Columns.Count];
                    reader.GetValues(values);
                    result.Data.Add(values);
                }

                result.Size = result.Data.Count;
                result.From.Add(a);
                result.From.Add(b);

                //get foreign keys
                reader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                sqlConn.Close();
            }
            return result;
        }

        private List<String> getColumnNames(DataTable schemaTable)
        {
            List<String> result = new List<string>();
            foreach (DataRow row in schemaTable.Rows)
            {
                result.Add((String)row["ColumnName"]);
            }
            return result;
        }

        public bool isValidJoin(Table a, Table b)
        {
            String query = String.Format("SELECT COUNT(*) as size FROM {0} NATURAL JOIN {1};", a.Name, b.Name);
            bool result = false;

            MySqlConnection sqlConn = new MySqlConnection(CONNECTION_STRING);
            MySqlCommand command = sqlConn.CreateCommand();
            MySqlDataReader reader = null;
            command.CommandText = query;

            try
            {
                sqlConn.Open();
                reader = command.ExecuteReader();

                reader.Read();
                if (reader.GetInt32(SIZE) > 0)
                {
                    result = true;
                }

                //get foreign keys
                reader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                sqlConn.Close();
            }

            return result;
        }
    }
}
