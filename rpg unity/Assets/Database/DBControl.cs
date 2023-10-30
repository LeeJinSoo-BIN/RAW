using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;
using MySql.Data.MySqlClient;

public class DB : MonoBehaviour
{
    public static MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
    {
        Server = "svc.sel4.cloudtype.app",
        Port = 30541,
        Database = "raw",
        UserID = "root",
        Password = "binary01!",
        CharacterSet = "utf8",
    };
}

public class AccountDB : MonoBehaviour
{

    public static int Login(string loginId, string loginPw)
    {
        using (MySqlConnection conn = new MySqlConnection(DB.builder.ConnectionString))
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
        using (MySqlConnection conn = new MySqlConnection(DB.builder.ConnectionString))
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
        using (MySqlConnection conn = new MySqlConnection(DB.builder.ConnectionString))
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
        using (MySqlConnection conn = new MySqlConnection(DB.builder.ConnectionString))
        {
            try
            {
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {
                    command.CommandText = string.Format("SELECT EXISTS (SELECT * FROM user_character WHERE nickname = '{0}' LIMIT 1) AS chk", nickName);

                    using (MySqlDataReader userCharacter = command.ExecuteReader())
                    {
                        userCharacter.Read();
                        if (Convert.ToInt32(userCharacter[0]) == 1)
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

    public static CharacterSpec CreateCharacter(string nickname, string roll)
    {
        CharacterSpec spec = new();

        spec.nickName = nickname;
        spec.roll = roll;
        spec.characterLevel = 1;
        spec.lastTown = "Pallet Town";

        using (MySqlConnection conn = new MySqlConnection(DB.builder.ConnectionString))
        {
            try
            {
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {

                    command.CommandText = string.Format("SELECT * FROM character_std WHERE roll = '{0}'", roll);

                    int characterId;

                    using (MySqlDataReader characterStd = command.ExecuteReader())
                    {
                        characterStd.Read();

                        characterId = Convert.ToInt32(characterStd["id"]);
                        spec.maxInventoryNum = Convert.ToInt32(characterStd["max_inventory"]);

                        characterStd.Close();
                    }

                    spec.maxHealth                  = (float) StatDB.getStat("character_std_stat", "hp");
                    spec.maxMana                    = (float) StatDB.getStat("character_std_stat", "mp");
                    spec.recoverManaPerThreeSec     = (float) StatDB.getStat("character_std_stat", "recover_mp");
                    spec.power                      = (float) StatDB.getStat("character_std_stat", "power");
                    spec.criticalDamage             = (float) StatDB.getStat("character_std_stat", "critical_dmg");
                    spec.criticalPercent            = (float) StatDB.getStat("character_std_stat", "critical_percent");
                    spec.healPercent                = (float) StatDB.getStat("character_std_stat", "heal_percent");
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                spec = null;
            }

            conn.Close();
        }

        return spec;
    }
}


public class StatDB : MonoBehaviour
{
    public static double getStat(string statType, string statName)
    {
        double stat = 0;

        using (MySqlConnection conn = new MySqlConnection(DB.builder.ConnectionString))
        {
            try
            {

                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {
                    command.CommandText = string.Format("SELECT stat_code FROM stat WHERE stat_name = '{0}'", statName);

                    int code;

                    using (MySqlDataReader statCodeReader = command.ExecuteReader())
                    {
                        statCodeReader.Read();

                        code = Convert.ToInt32(statCodeReader["stat_code"]);

                        statCodeReader.Close();
                    }

                    command.CommandText = string.Format("SELECT stat FROM {0} WHERE stat_code = {1}", statType, code);


                    using (MySqlDataReader statReader = command.ExecuteReader())
                    {
                        statReader.Read();

                        stat = Convert.ToDouble(statReader["stat"]);

                        statReader.Close();
                    }
                }

            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }

            conn.Close();
        }

        return stat;
    }
}