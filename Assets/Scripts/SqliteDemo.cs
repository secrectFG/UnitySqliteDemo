using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using UnityEngine.UI;
using System.Threading.Tasks;
public class SqliteDemo : MonoBehaviour
{

    public Text text;

    private void Start()
    {
        var dbpath = Application.streamingAssetsPath + "/test.db";
        // 如果目录不存在，创建目录
        if (!System.IO.Directory.Exists(Application.streamingAssetsPath))
        {
            System.IO.Directory.CreateDirectory(Application.streamingAssetsPath);
        }
        string conn = "URI=file:" + dbpath;
        // 判断文件是否存在
        if (!System.IO.File.Exists(dbpath))
        {
            CreateDB(dbpath);
            InsertData(conn);
        }

        SqliteConnection dbconn = new SqliteConnection(conn);
        dbconn.Open();
        SqliteCommand dbcmd = dbconn.CreateCommand();
        string sqlQuery = "SELECT * FROM test";
        dbcmd.CommandText = sqlQuery;
        SqliteDataReader reader = dbcmd.ExecuteReader();
        while (reader.Read())
        {
            string name = reader.GetString(0);
            int age = reader.GetInt32(1);
            Log("name = " + name + " age = " + age);
        }
        reader.Close();
        dbcmd.Dispose();
        dbconn.Close();

        _ = Test();
    }


    class Person
    {
        public int id;
        public string name;
        public int age;
    }

    async Task Test()
    {
        await SqliteTools.CreateDB<Person>(Application.streamingAssetsPath + "/test2.db");
        var list = await SqliteTools.ReadTable<Person>(Application.streamingAssetsPath + "/test2.db");
        if (list.Count == 0)
        {
            await SqliteTools.WriteTable(Application.streamingAssetsPath + "/test2.db", new List<Person>(){
           new Person(){id = 1, name = "张三", age = 18},
           new Person(){id = 2, name = "李四", age = 20},
           new Person(){id = 3, name = "王五", age = 22},
           new Person(){id = 4, name = "赵六", age = 24},
           new Person(){id = 5, name = "田七", age = 26},
        });
            list = await SqliteTools.ReadTable<Person>(Application.streamingAssetsPath + "/test2.db");
            Log("\nlist count = " + list.Count);
            foreach (var item in list)
            {
                Log("id = " + item.id + " name = " + item.name + " age = " + item.age);
            }
        }

        list = await SqliteTools.ReadTable<Person>(Application.streamingAssetsPath + "/test2.db", "age > 20");
        Log("\nage >20. list count = " + list.Count);
        foreach (var item in list)
        {
            Log("id = " + item.id + " name = " + item.name + " age = " + item.age);
        }
    }

    void InsertData(string path)
    {
        SqliteConnection dbconn = new SqliteConnection(path);
        dbconn.Open();
        SqliteCommand dbcmd = dbconn.CreateCommand();
        string sqlQuery = "INSERT INTO test (name, age) VALUES ('张三', 18), ('李四', 20), ('王五', 22), ('赵六', 24), ('田七', 26)";
        dbcmd.CommandText = sqlQuery;
        dbcmd.ExecuteNonQuery();
        dbcmd.Dispose();
        dbconn.Close();
    }

    void CreateDB(string path)
    {
        SqliteConnection.CreateFile(path);
        SqliteConnection dbconn = new SqliteConnection("URI=file:" + path);
        dbconn.Open();
        SqliteCommand dbcmd = dbconn.CreateCommand();
        string sqlQuery = "CREATE TABLE test (name VARCHAR(20), age INT)";
        dbcmd.CommandText = sqlQuery;
        dbcmd.ExecuteNonQuery();
        dbcmd.Dispose();
        dbconn.Close();
    }

    void Log(string msg)
    {
        text.text += msg + "\n";
    }
}
