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
    public static CharacterSpec CreateCharacter(string nickname, string roll)
    {
        CharacterSpec spec = new();

        spec.nickName = nickname;
        spec.roll = roll;
        spec.characterLevel = 1;
        spec.exp = 0;
        spec.lastTown = "Pallet Town";

        using (MySqlConnection conn = new MySqlConnection(DBSetting.builder.ConnectionString))
        {
            try
            {
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {

                    command.CommandText = string.Format("SELECT * FROM character_std WHERE roll = '{0}';", roll);

                    int characterId;

                    using (MySqlDataReader characterStd = command.ExecuteReader())
                    {
                        characterStd.Read();

                        characterId             = Convert.ToInt32(characterStd["character_id"]);
                        spec.maxInventoryNum    = Convert.ToInt32(characterStd["max_inventory"]);

                        characterStd.Close();
                    }

                    spec.maxHealth              = (float)StatDB.GetCharacterStdStat(characterId, "hp");
                    spec.maxMana                = (float)StatDB.GetCharacterStdStat(characterId, "mp");
                    spec.recoverManaPerThreeSec = (float)StatDB.GetCharacterStdStat(characterId, "recover_mp");
                    spec.power                  = (float)StatDB.GetCharacterStdStat(characterId, "power");
                    spec.criticalDamage         = (float)StatDB.GetCharacterStdStat(characterId, "critical_dmg");
                    spec.criticalPercent        = (float)StatDB.GetCharacterStdStat(characterId, "critical_percent");
                    spec.healPercent            = (float)StatDB.GetCharacterStdStat(characterId, "heal_percent");
                }

                InsertCharacter(spec);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                spec = null;
            }
            finally
            {
                conn.Close();
            }
        }

        return spec;
    }

    public static int InsertCharacter(CharacterSpec spec)
    {
        int res;

        using (MySqlConnection conn = new MySqlConnection(DBSetting.builder.ConnectionString))
        {
            try
            {
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {
                    command.CommandText = string.Format(
                        "INSERT INTO user_character (user_id, character_num, character_id, nickname, level, exp, last_town, max_inventory) " +
                        "VALUES ('{0}', '{1}', '{2}', '{3}', {4}, {5}, '{6}', {7})",
                        DataBase.Instance.defaultAccountInfo.accountId, 0, 1, spec.nickName, spec.characterLevel, spec.exp, spec.lastTown, spec.maxInventoryNum
                    );

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

    public static int InsertCharacterStat(string statType, string statName, double stat)
    {
        int res;
        using (MySqlConnection conn = new(DBSetting.builder.ConnectionString))
        {
            try
            {
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {
                    int statCode = StatDB.GetStatCode(statName);

                    command.CommandText = string.Format("INSERT INTO '{0}' (user_id, character_num, stat_code, stat) VALUES ({1}, {2});", statType, statCode, stat);

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