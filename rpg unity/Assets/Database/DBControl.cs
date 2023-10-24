using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;
using MySql.Data.MySqlClient;

public class AccountDB : MonoBehaviour
{
    static MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
    {
        Server = "svc.sel4.cloudtype.app",
        Port = 30541,
        Database = "raw",
        UserID = "root",
        Password = "binary01!",
        CharacterSet = "utf8",
    };

    public static int Login(string loginId, string loginPw)
    {
        using (MySqlConnection conn = new MySqlConnection(builder.ConnectionString))
        {
            int loginStatus = 0;

            try
            {
                Debug.Log("DB Connect");
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {

                    command.CommandText = string.Format("SELECT * FROM user WHERE user_id = '{0}';", loginId);

                    using (MySqlDataReader userAccount = command.ExecuteReader())
                    {
                        while (userAccount.Read())
                        {
                            if (loginId == (string)userAccount["user_id"] && loginPw == (string)userAccount["password"])
                            {
                                loginStatus = 1;
                                break;
                            }
                        }

                        return loginStatus;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                return -1;
            }
        }
    }

    public static int Register(string loginId, string loginPw)
    {
        using (MySqlConnection conn = new MySqlConnection(builder.ConnectionString))
        {
            try
            {
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {
                    command.CommandText = string.Format("INSERT INTO user (user_id, password) VALUES ('{0}', '{1}');", loginId, loginPw);

                    return (command.ExecuteNonQuery());
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                return -1;
            }
        }
    }
}
