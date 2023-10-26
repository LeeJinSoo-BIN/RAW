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
            try
            {
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {

                    command.CommandText = string.Format("SELECT password FROM user WHERE user_id = '{0}';", loginId);

                    using (MySqlDataReader userAccount = command.ExecuteReader())
                    {
                        if (userAccount.Read())
                        {
                            if (loginPw == Convert.ToString(userAccount["password"]))
                                return 1;
                        }

                        return 0;
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

    public static List<CharacterSpec> SelectCharacter(string userId)
    {
        using (MySqlConnection conn = new MySqlConnection(builder.ConnectionString))
        {
            try
            {
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {
                    command.CommandText = string.Format("SELECT * FROM user_character WHERE user_id = '{0}';", userId);

                    using (MySqlDataReader userCharacter = command.ExecuteReader())
                    {
                        List<CharacterSpec> characterSpec = new List<CharacterSpec>();

                        while(userCharacter.Read())
                        {
                            CharacterSpec spec = new CharacterSpec
                            {
                                nickName        = Convert.ToString(userCharacter["nickname"]),
                                characterLevel  = Convert.ToInt32(userCharacter["level"]),
                                lastTown        = Convert.ToString(userCharacter["last_town"]),
                                maxInventoryNum = Convert.ToInt32(userCharacter["max_inventory"]),
                                exp             = Convert.ToInt32(userCharacter["exp"])
                                
                            };

                            characterSpec.Add(spec);
                        }

                        return (characterSpec);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                return null;
            }
        }
    }

    public static bool CheckDuplicateNickName(string nickName)
    {
        using (MySqlConnection conn = new MySqlConnection(builder.ConnectionString))
        {
            try
            {
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {
                    command.CommandText = string.Format("SELECT EXISTS (SELECT * FROM user_character WHERE nickname = '{0}' LIMIT 1) AS success", nickName);

                    using (MySqlDataReader user_character = command.ExecuteReader())
                    {
                        user_character.Read();
                        if (Convert.ToInt32(user_character[0]) == 1)
                            return true;

                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                return true;
            }
        }
    }
}
