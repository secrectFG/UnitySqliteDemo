using System.Collections.Generic;
using System.Threading.Tasks;
using Mono.Data.Sqlite;
using UnityEngine;

public class SqliteTools
{
    public static async Task CreateDB<T>(string path)
    {
        await Task.Run(() =>
        {
            try
            {
                //判断文件是否存在
                if (System.IO.File.Exists(path))
                {
                    return;
                }
                //如果目录不存在，创建目录
                if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(path)))
                {
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                }
                SqliteConnection.CreateFile(path);
                SqliteConnection dbconn = new SqliteConnection("URI=file:" + path);
                dbconn.Open();
                SqliteCommand dbcmd = dbconn.CreateCommand();
                //根据data的类型，生成sql语句
                var tablename = typeof(T).Name;
                string sqlQuery = "CREATE TABLE " + tablename + " (";
                var fields = typeof(T).GetFields();
                for (int i = 0; i < fields.Length; i++)
                {
                    var field = fields[i];
                    sqlQuery += field.Name + " " + field.FieldType.Name + ",";
                }
                sqlQuery = sqlQuery.Substring(0, sqlQuery.Length - 1);
                sqlQuery += ")";

                dbcmd.CommandText = sqlQuery;
                dbcmd.ExecuteNonQuery();
                dbcmd.Dispose();
                dbconn.Close();
                Debug.Log("CreateDB Success");
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        });
    }
    public static async Task WriteTable<T>(string path, List<T> dataList)
    {
        await Task.Run(() =>
        {
            try
            {
                SqliteConnection dbconn = new SqliteConnection("URI=file:" + path);
                dbconn.Open();
                SqliteCommand dbcmd = dbconn.CreateCommand();
                //根据data的类型，生成sql语句
                var tablename = typeof(T).Name;

                var fields = typeof(T).GetFields();
                foreach (var data in dataList)
                {
                    string sqlQuery = "INSERT INTO " + tablename + " (";
                    for (int i = 0; i < fields.Length; i++)
                    {
                        var field = fields[i];
                        sqlQuery += field.Name + ",";
                    }
                    sqlQuery = sqlQuery.Substring(0, sqlQuery.Length - 1);
                    sqlQuery += ") VALUES (";
                    for (int i = 0; i < fields.Length; i++)
                    {
                        var field = fields[i];
                        sqlQuery += "@" + field.Name + ",";
                    }
                    sqlQuery = sqlQuery.Substring(0, sqlQuery.Length - 1);
                    sqlQuery += ")";

                    dbcmd.CommandText = sqlQuery;
                    for (int i = 0; i < fields.Length; i++)
                    {
                        var field = fields[i];
                        dbcmd.Parameters.AddWithValue("@" + field.Name, field.GetValue(data));
                    }
                    dbcmd.ExecuteNonQuery();
                }
                // string sqlQuery = "INSERT INTO " + tablename + " (";
                // for (int i = 0; i < fields.Length; i++)
                // {
                //     var field = fields[i];
                //     sqlQuery += field.Name + ",";
                // }
                // sqlQuery = sqlQuery.Substring(0, sqlQuery.Length - 1);
                // sqlQuery += ") VALUES ";
                // var values = "";
                // foreach (var data in dataList)
                // {
                //     values += "(";
                //     for (int i = 0; i < fields.Length; i++)
                //     {
                //         var field = fields[i];
                //         values += "@" + field.Name + ",";
                //     }
                //     values = values.Substring(0, values.Length - 1);
                //     values += "),";
                // }
                // values = values.Substring(0, values.Length - 1);
                // sqlQuery += values;

                // Debug.Log(sqlQuery);

                // dbcmd.CommandText = sqlQuery;
                // foreach (var data in dataList)
                // {
                //     for (int i = 0; i < fields.Length; i++)
                //     {
                //         var field = fields[i];
                //         dbcmd.Parameters.AddWithValue("@" + field.Name, field.GetValue(data));
                //         Debug.Log("@" + field.Name + " = " + field.GetValue(data));
                //     }
                // }
                // dbcmd.ExecuteNonQuery();
                dbcmd.Dispose();
                dbconn.Close();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        });
    }

    public static async Task<List<T>> ReadTable<T>(string path, string filter = "")
    {
        return await Task.Run(() =>
        {
            try
            {
                SqliteConnection dbconn = new SqliteConnection("URI=file:" + path);
                dbconn.Open();
                SqliteCommand dbcmd = dbconn.CreateCommand();
                //根据data的类型，生成sql语句
                var tablename = typeof(T).Name;
                string sqlQuery = "SELECT * FROM " + tablename;
                if (filter != "")
                {
                    sqlQuery += " WHERE " + filter;
                }
                dbcmd.CommandText = sqlQuery;
                SqliteDataReader reader = dbcmd.ExecuteReader();
                var dataList = new List<T>();
                while (reader.Read())
                {
                    var data = System.Activator.CreateInstance<T>();
                    var fields = typeof(T).GetFields();
                    for (int i = 0; i < fields.Length; i++)
                    {
                        var field = fields[i];
                        if (field.FieldType == typeof(string))
                        {
                            field.SetValue(data, reader.GetString(i));
                        }
                        else if (field.FieldType == typeof(int))
                        {
                            field.SetValue(data, reader.GetInt32(i));
                        }
                        else if (field.FieldType == typeof(float))
                        {
                            field.SetValue(data, reader.GetFloat(i));
                        }
                        else if (field.FieldType == typeof(double))
                        {
                            field.SetValue(data, reader.GetDouble(i));
                        }
                        else if (field.FieldType == typeof(bool))
                        {
                            field.SetValue(data, reader.GetBoolean(i));
                        }
                    }
                    dataList.Add(data);
                }
                reader.Close();
                dbcmd.Dispose();
                dbconn.Close();
                return dataList;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        });
    }

    public static async Task DeleteFromTable<T>(string path, string filter = "")
    {
        await Task.Run(() =>
        {
            try
            {
                SqliteConnection dbconn = new SqliteConnection("URI=file:" + path);
                dbconn.Open();
                SqliteCommand dbcmd = dbconn.CreateCommand();
                var tablename = typeof(T).Name;
                string sqlQuery = "DELETE FROM " + tablename;
                if (filter != "")
                {
                    sqlQuery += " WHERE " + filter;
                }
                dbcmd.CommandText = sqlQuery;
                dbcmd.ExecuteNonQuery();
                dbcmd.Dispose();
                dbconn.Close();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        });
    }
}