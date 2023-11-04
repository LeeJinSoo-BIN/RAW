using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;
using MySql.Data.MySqlClient;

public class DBSetting : MonoBehaviour
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
        int status = 0;

        using (MySqlConnection conn = new MySqlConnection(DBSetting.builder.ConnectionString))
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
                                status = 1;
                        }

                        userAccount.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                status = -1;
            }
            finally
            {
                conn.Close();
            }
        }

        return status;
    }

    public static int Register(string loginId, string loginPw)
    {
        int status;

        using (MySqlConnection conn = new MySqlConnection(DBSetting.builder.ConnectionString))
        {
            try
            {
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {
                    command.CommandText = string.Format("INSERT INTO user (user_id, password) VALUES ('{0}', '{1}');", loginId, loginPw);

                    status = command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                status = -1;
            }
            finally
            {
                conn.Close();
            }
        }

        return status;
    }

    public static List<CharacterSpec> SelectCharacter(string userId)
    {
        List<CharacterSpec> characterSpec = new List<CharacterSpec>();

        using (MySqlConnection conn = new MySqlConnection(DBSetting.builder.ConnectionString))
        {
            try
            {
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {
                    command.CommandText = string.Format("SELECT * FROM user_character WHERE user_id = '{0}';", userId);

                    using (MySqlDataReader userCharacter = command.ExecuteReader())
                    {
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

                        userCharacter.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                characterSpec = null;
            }
            finally
            {
                conn.Close();
            }
        }

        return characterSpec;
    }

    public static bool CheckDuplicateNickName(string nickName)
    {
        bool chk = false;

        using (MySqlConnection conn = new MySqlConnection(DBSetting.builder.ConnectionString))
        {
            try
            {
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {
                    command.CommandText = string.Format("SELECT EXISTS (SELECT * FROM user_character WHERE nickname = '{0}' LIMIT 1) AS chk;", nickName);

                    using (MySqlDataReader userCharacter = command.ExecuteReader())
                    {
                        userCharacter.Read();

                        chk = Convert.ToInt32(userCharacter[0]) >= 1;

                        userCharacter.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                chk = true;
            }
            finally
            {
                conn.Close();
            }
        }

        return chk;
    }
}

public class CharacterDB : MonoBehaviour
{
    private static string userId = DataBase.Instance.defaultAccountInfo.accountId;

    public static int CreateCharacter(string nickname, string roll)
    {
        int status = 0;

        using (MySqlConnection conn = new MySqlConnection(DBSetting.builder.ConnectionString))
        {
            try
            {
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {

                    command.CommandText = string.Format("SELECT * FROM character_std WHERE roll = '{0}';", roll);

                    int characterId;
                    int maxInventoryNum;
                    int characterNum;

                    using (MySqlDataReader characterStd = command.ExecuteReader())
                    {
                        characterStd.Read();

                        characterId = Convert.ToInt32(characterStd["character_id"]);
                        maxInventoryNum = Convert.ToInt32(characterStd["max_inventory"]);

                        characterStd.Close();
                    }

                    command.CommandText = string.Format(
                        "INSERT INTO user_character (user_id, character_num, character_id, nickname, level, exp, last_town, max_inventory) " +
                        "VALUES ('{0}', {1}, {2}, '{3}', {4}, {5}, '{6}', {7})",
                        userId, 0, characterId, nickname, 1, 0, "Pallet Town", maxInventoryNum
                    );

                    command.ExecuteNonQuery();

                    StatDB.InsertCharacterStat(userId, 1, "hp", StatDB.GetCharacterStdStat(characterId, "hp"));
                    StatDB.InsertCharacterStat(userId, 1, "mp", StatDB.GetCharacterStdStat(characterId, "mp"));
                    StatDB.InsertCharacterStat(userId, 1, "recover_mp", StatDB.GetCharacterStdStat(characterId, "recover_mp"));
                    StatDB.InsertCharacterStat(userId, 1, "power", StatDB.GetCharacterStdStat(characterId, "power"));
                    StatDB.InsertCharacterStat(userId, 1, "critical_dmg", StatDB.GetCharacterStdStat(characterId, "critical_dmg"));
                    StatDB.InsertCharacterStat(userId, 1, "critical_percent", StatDB.GetCharacterStdStat(characterId, "critical_percent"));
                    StatDB.InsertCharacterStat(userId, 1, "heal_percent", StatDB.GetCharacterStdStat(characterId, "heal_percent"));

                    status = 1;
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                status = -1;
            }
            finally
            {
                conn.Close();
            }
        }

        return status;
    }
}

public class StatDB : MonoBehaviour
{
    public static double GetCharacterStdStat(int characterId, string statName)
    {
        double stat = 0;

        using (MySqlConnection conn = new MySqlConnection(DBSetting.builder.ConnectionString))
        {
            try
            {
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {
                    int statCode = GetStatCode(statName);

                    command.CommandText = string.Format("SELECT stat FROM character_std_stat WHERE character_id = {0} AND stat_code = {1};", characterId, statCode);

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
            finally
            {
                conn.Close();
            }
        }

        return stat;
    }

    public static int InsertCharacterStat(string user_id, int character_num, string statName, double stat)
    {
        int res;
        using (MySqlConnection conn = new(DBSetting.builder.ConnectionString))
        {
            try
            {
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {
                    int statCode = GetStatCode(statName);

                    command.CommandText = string.Format("INSERT INTO character_stat (user_id, character_num, stat_code, stat) VALUES ('{0}', {1}, {2}, {3});", user_id, character_num, statCode, stat);

                    res = command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                res = -1;
            }
            finally
            {
                conn.Close();
            }
        }

        return res;
    }

    public static int GetStatCode(string statName)
    {
        int statCode = 0;

        using (MySqlConnection conn = new MySqlConnection(DBSetting.builder.ConnectionString))
        {
            try
            {
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {
                    command.CommandText = string.Format("SELECT stat_code FROM stat WHERE stat_name = '{0}';", statName);

                    using (MySqlDataReader statCodeReader = command.ExecuteReader())
                    {
                        statCodeReader.Read();

                        statCode = Convert.ToInt32(statCodeReader["stat_code"]);

                        statCodeReader.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                statCode = -1;
            }
            finally
            {
                conn.Close();
            }
        }

        return statCode;
    }
}