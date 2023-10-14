using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using MySql.Data.MySqlClient;

public class DBControl : MonoBehaviour
{
    public static MySqlConnection SqlConn;

    static string ipAddress = "svc.sel4.cloudtype.app";
    static string port = "30541";
    static string db_id = "root";
    static string db_pw = "binary01!";
    static string db_name = "raw";

    string strConn = string.Format("server={0};port={1};uid={2};pwd={3};database={4};charset=utf8 ;", ipAddress, port, db_id, db_pw, db_name);

    private void Awake()
    {
        try
        {
            Debug.Log("DB connected");
            SqlConn = new MySqlConnection(strConn);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    private void OnApplicationQuit()
    {
        SqlConn.Close();
    }
}
