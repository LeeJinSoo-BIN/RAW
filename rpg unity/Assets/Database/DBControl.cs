using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using MySql.Data.MySqlClient;

public class DBControl : MonoBehaviour
{
    public static MySqlConnection SqlConn;

    static string ipAddress = "34.64.184.142";
    static string db_id = "root";
    static string db_pw = "binary01!";
    static string db_name = "raw";

    string strConn = string.Format("server={0};uid={1};pwd={2};database={3};charset=utf8 ;", ipAddress, db_id, db_pw, db_name);

    private void Awake()
    {
        try
        {
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
