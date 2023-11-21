using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
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

                    command.CommandText = string.Format(
                        "SELECT password " +
                        "FROM user " +
                        "WHERE user_id = '{0}';",
                        loginId
                    );

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
                    command.CommandText = string.Format(
                        "INSERT INTO user (user_id, password) " +
                        "VALUES ('{0}', '{1}');",
                        loginId, loginPw
                    );

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
                    command.CommandText = string.Format(
                        "SELECT EXISTS " +
                        "(SELECT * FROM user_character " +
                        "WHERE nickname = '{0}' " +
                        "LIMIT 1) " +
                        "AS chk;", nickName
                    );

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
    public static List<CharacterSpec> SelectCharacter(string userId)
    {
        List<CharacterSpec> characterSpec = new List<CharacterSpec>();

        int characterId;
        int characterNum;

        string nickName;
        string roll;
        int characterLevel;
        int exp;
        string lastTown;

        float maxHealth;
        float maxMana;
        float recoverManaPerThreeSec;
        float power;
        float criticalDamage;
        float criticalPercent;
        float healPercent;

        int maxInventoryNum;

        List<InventoryItem> equipment;

        using (MySqlConnection conn = new MySqlConnection(DBSetting.builder.ConnectionString))
        {
            try
            {
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {
                    command.CommandText = string.Format(
                        "SELECT * " +
                        "FROM user_character " +
                        "WHERE user_id = '{0}'" +
                        "ORDER BY character_num",
                        userId
                    );

                    using (MySqlDataReader userCharacter = command.ExecuteReader())
                    {
                        while (userCharacter.Read())
                        {
                            nickName = Convert.ToString(userCharacter["nickname"]);
                            lastTown = Convert.ToString(userCharacter["last_town"]);
                            characterLevel = Convert.ToInt32(userCharacter["level"]);
                            maxInventoryNum = Convert.ToInt32(userCharacter["max_inventory"]);
                            exp = Convert.ToInt32(userCharacter["exp"]);
                            characterId = Convert.ToInt32(userCharacter["character_id"]);
                            characterNum = Convert.ToInt32(userCharacter["character_num"]);

                            roll = GetRollName(characterId);
                            
                            maxHealth = (float)GetCharacterStat(userId, characterNum, "hp");
                            maxMana = (float)GetCharacterStat(userId, characterNum, "mp");
                            recoverManaPerThreeSec = (float)GetCharacterStat(userId, characterNum, "recover_mp");
                            power = (float)GetCharacterStat(userId, characterNum, "power");
                            criticalDamage = (float)GetCharacterStat(userId, characterNum, "critical_dmg");
                            criticalPercent = (float)GetCharacterStat(userId, characterNum, "critical_percent");
                            healPercent = (float)GetCharacterStat(userId, characterNum, "heal_percent");

                            equipment = SelectEquipment(userId, characterNum);
                            Debug.Log(equipment.Count);

                            CharacterSpec spec = new()
                            {
                                nickName = nickName,
                                roll = roll,
                                characterLevel = characterLevel,
                                exp = exp,
                                lastTown = lastTown,
                                maxInventoryNum = maxInventoryNum,
                                maxHealth = maxHealth,
                                maxMana = maxMana,
                                recoverManaPerThreeSec = recoverManaPerThreeSec,
                                power = power,
                                criticalDamage = criticalDamage,
                                criticalPercent = criticalPercent,
                                healPercent = healPercent,
                                equipment = equipment,
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

    public static int CreateCharacter(string nickname, string roll, List<int> equipmentId, Dictionary<string, Color> colors)
    {
        List<string> statNames = new List<string>() { "hp", "mp", "recover_mp", "power", "critical_dmg", "critical_percent", "heal_percent" };

        int status = 0;

        string userId = DataBase.Instance.defaultAccountInfo.accountId;
        string lastTown = "Pallet Town";
        int characterId;
        int maxInventoryNum;
        int characterNum = 1;
        int level = 1;
        int exp = 0;

        using (MySqlConnection conn = new MySqlConnection(DBSetting.builder.ConnectionString))
        {
            try
            {
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {

                    command.CommandText = string.Format(
                        "SELECT * FROM character_std " +
                        "WHERE roll = '{0}';",
                        roll
                    );

                    using (MySqlDataReader characterStd = command.ExecuteReader())
                    {
                        characterStd.Read();

                        characterId = Convert.ToInt32(characterStd["character_id"]);
                        maxInventoryNum = Convert.ToInt32(characterStd["max_inventory"]);

                        characterStd.Close();
                    }

                    command.CommandText = string.Format(
                        "SELECT character_num " +
                        "FROM user_character " +
                        "WHERE user_id = '{0}' " +
                        "ORDER BY character_num " +
                        "DESC",
                        userId
                    );
                    
                    using (MySqlDataReader userCharacter = command.ExecuteReader())
                    {
                        if (userCharacter.Read())
                            characterNum = Convert.ToInt32(userCharacter["character_num"]) + 1;

                        userCharacter.Close();
                    }

                    command.CommandText = string.Format(
                        "INSERT INTO user_character (user_id, character_num, character_id, nickname, level, exp, last_town, max_inventory) " +
                        "VALUES ('{0}', {1}, {2}, '{3}', {4}, {5}, '{6}', {7})",
                        userId, characterNum, characterId, nickname, level, exp, lastTown, maxInventoryNum
                    );

                    command.ExecuteNonQuery();


                    foreach (string statName in statNames)
                        InsertCharacterStat(userId, characterNum, statName, GetCharacterStdStat(characterId, statName));

                    foreach (int id in equipmentId)
                    {
                        int equipmentNum = InsertEquipment(userId, characterNum, id);
                        Debug.Log(equipmentNum);
                        InsertEquipmentSlot(userId, characterNum, id, equipmentNum);
                    }

                    foreach (KeyValuePair<string, Color> color in colors)
                        InsertColor(userId, characterNum, color.Key, color.Value);
                }

                status = 1;
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

    private static double GetCharacterStdStat(int characterId, string statName)
    {
        double stat = 0;

        using (MySqlConnection conn = new MySqlConnection(DBSetting.builder.ConnectionString))
        {
            try
            {
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {
                    command.CommandText = string.Format(
                        "SELECT stat " +
                        "FROM character_std_stat " +
                        "WHERE character_id = {0} " +
                        "AND stat_code = " +
                        "(SELECT stat_code " +
                        "FROM stat " +
                        "WHERE stat_name = '{1}')",
                        characterId, statName
                    );

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

    private static double GetCharacterStat(string userId, int characterNum, string statName)
    {
        double stat = 0;

        using (MySqlConnection conn = new MySqlConnection(DBSetting.builder.ConnectionString))
        {
            try
            {
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {
                    command.CommandText = string.Format(
                        "SELECT stat FROM character_stat " +
                        "WHERE user_id = '{0}' " +
                        "AND character_num = {1} " +
                        "AND stat_code = " +
                        "(SELECT stat_code " +
                        "FROM stat " +
                        "WHERE stat_name = '{2}')",
                        userId, characterNum, statName
                    );

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

    private static int InsertCharacterStat(string userId, int characterNum, string statName, double stat)
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

                    command.CommandText = string.Format(
                        "INSERT INTO character_stat (user_id, character_num, stat_code, stat) " +
                        "VALUES ('{0}', {1}, {2}, {3});",
                        userId, characterNum, statCode, stat
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

    private static int InsertEquipment(string userId, int characterNum, int equipmentId)
    {
        int equipmentNum = 1;

        using (MySqlConnection conn = new(DBSetting.builder.ConnectionString))
        {
            try
            {
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {
                    command.CommandText = string.Format(
                        "SELECT equipment_num " +
                        "FROM character_equipment " +
                        "WHERE user_id = '{0}' " +
                        "AND character_num = {1} " +
                        "AND equipment_id = {2} " +
                        "ORDER BY equipment_num DESC",
                        userId, characterNum, equipmentId
                    );

                    using (MySqlDataReader characterEquipment = command.ExecuteReader())
                    {
                        if (characterEquipment.Read())
                            equipmentNum = Convert.ToInt32(characterEquipment["equipment_num"]) + 1;

                        characterEquipment.Close();
                    }

                    command.CommandText = string.Format(
                        "INSERT INTO character_equipment (user_id, character_num, equipment_id, equipment_num) " +
                        "VALUES ('{0}', {1}, {2}, {3});",
                        userId, characterNum, equipmentId, equipmentNum
                    );

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                equipmentNum = -1;
            }
            finally
            {
                conn.Close();
            }
        }

        return equipmentNum;
    }

    private static int InsertEquipmentSlot(string userId, int characterNum, int equipmentId, int equipmentNum)
    {
        int res = 0;

        string equipmentType;

        using (MySqlConnection conn = new(DBSetting.builder.ConnectionString))
        {
            try
            {
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {
                    command.CommandText = string.Format(
                        "SELECT type " +
                        "FROM equipment " +
                        "WHERE equipment_id = {0} ",
                        equipmentId
                    );

                    using (MySqlDataReader characterEquipment = command.ExecuteReader())
                    {
                        characterEquipment.Read();

                        equipmentType = Convert.ToString(characterEquipment["type"]);

                        characterEquipment.Close();
                    }

                    command.CommandText = string.Format(
                        "INSERT INTO equipment_slot (user_id, character_num, equipment_id, equipment_num, slot_type) " +
                        "VALUES ('{0}', {1}, {2}, {3}, '{4}');",
                        userId, characterNum, equipmentId, equipmentNum, equipmentType
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

    private static int InsertColor(string userId, int characterNum, string colorType, Color color)
    {
        int res;

        using (MySqlConnection conn = new(DBSetting.builder.ConnectionString))
        {
            try
            {
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {
                    command.CommandText = string.Format(
                        "INSERT INTO character_color (user_id, character_num, color_type, red, green, blue) " +
                        "VALUES ('{0}', {1}, '{2}', {3}, {4}, {5});",
                        userId, characterNum, colorType, color.r, color.g, color.b
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

    public static string GetRollName(int characterId)
    {
        string roll;

        using (MySqlConnection conn = new MySqlConnection(DBSetting.builder.ConnectionString))
        {
            try
            {
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {
                    command.CommandText = string.Format(
                        "SELECT roll " +
                        "FROM character_std " +
                        "WHERE character_id = {0}",
                        characterId
                    );

                    using (MySqlDataReader characterStd = command.ExecuteReader())
                    {
                        characterStd.Read();

                        roll = Convert.ToString(characterStd["roll"]);

                        characterStd.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                roll = "";
            }
            finally
            {
                conn.Close();
            }
        }

        return roll;
    }

    public static List<InventoryItem> SelectEquipment(string userId, int characterNum)
    {
        List<InventoryItem> equipments = new();

        string itemName;
        int count = 1;
        int reinforce;

        using (MySqlConnection conn = new MySqlConnection(DBSetting.builder.ConnectionString))
        {
            try
            {
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {
                    command.CommandText = string.Format(
                        "SELECT e.name, c.reinforce " +
                        "FROM equipment_slot s " +
                        "LEFT JOIN character_equipment c " +
                        "ON s.user_id = c.user_id " +
                        "AND s.character_num = c.character_num " +
                        "AND s.equipment_id = c.equipment_id " +
                        "AND s.equipment_num = c.equipment_num " +
                        "LEFT JOIN equipment e " +
                        "ON s.equipment_id = e.equipment_id " +
                        "WHERE s.user_id = '{0}' " +
                        "AND s.character_num = {1}",
                        userId, characterNum
                    );

                    using (MySqlDataReader equipmentReader = command.ExecuteReader())
                    {
                        while (equipmentReader.Read())
                        {
                            itemName = Convert.ToString(equipmentReader["name"]);
                            reinforce = Convert.ToInt32(equipmentReader["reinforce"]);

                            InventoryItem equipment = new()
                            {
                                itemName = itemName,
                                reinforce = reinforce,
                                count = count,
                            };

                            equipments.Add(equipment);
                        }

                        equipmentReader.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                equipments = null;
            }
            finally
            {
                conn.Close();
            }
        }

        return equipments;
    }
}

public class StatDB : MonoBehaviour
{
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
                    command.CommandText = string.Format(
                        "SELECT stat_code " +
                        "FROM stat " +
                        "WHERE stat_name = '{0}';",
                        statName
                    );

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