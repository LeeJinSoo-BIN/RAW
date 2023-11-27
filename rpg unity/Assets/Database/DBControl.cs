using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Data;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.MemoryProfiler;
using System.Linq.Expressions;
using System.Threading;

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

    public static async Task<bool> ConnectToDB()
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(builder.ConnectionString))
            {
                await connection.OpenAsync();
                return true;
            }
        }
        catch (MySqlException ex)
        {
            Debug.LogException(ex);
            return false;
        }
    }
}
    
    


public class AccountDB : MonoBehaviour
{
    public static async Task<int> Login(string loginId, string loginPw)
    {
        int status = 0;
        try
        {
            using (MySqlConnection conn = new MySqlConnection(DBSetting.builder.ConnectionString))
            {

                await conn.OpenAsync();

                using (MySqlCommand command = conn.CreateCommand())
                {

                    command.CommandText = string.Format(
                        "SELECT password " +
                        "FROM user " +
                        "WHERE user_id = '{0}';", loginId
                    );

                    using (MySqlDataReader userAccount = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        if (await userAccount.ReadAsync())
                        {
                            if (loginPw == Convert.ToString(userAccount["password"]))
                                status = 1;
                            else 
                                status = 0;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            status = -1;
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
        List<Color> colors;

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

                            using (MySqlConnection conn2 = new MySqlConnection(DBSetting.builder.ConnectionString))
                            {
                                conn2.Open();

                                maxHealth = (float)GetCharacterStat(conn2, userId, characterNum, "hp");
                                maxMana = (float)GetCharacterStat(conn2, userId, characterNum, "mp");
                                recoverManaPerThreeSec = (float)GetCharacterStat(conn2, userId, characterNum, "recover_mp");
                                power = (float)GetCharacterStat(conn2, userId, characterNum, "power");
                                criticalDamage = (float)GetCharacterStat(conn2, userId, characterNum, "critical_dmg");
                                criticalPercent = (float)GetCharacterStat(conn2, userId, characterNum, "critical_percent");
                                healPercent = (float)GetCharacterStat(conn2, userId, characterNum, "heal_percent");

                                conn2.Close();
                            }
                                

                            equipment = SelectEquipment(userId, characterNum);
                            colors = SelectColor(userId, characterNum);

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
                                colors = colors,
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
                        InsertCharacterStat(conn, userId, characterNum, statName, GetCharacterStdStat(conn, characterId, statName));

                    foreach (int id in equipmentId)
                    {
                        int equipmentNum = InsertEquipment(conn, userId, characterNum, id);
                        InsertEquipmentSlot(conn, userId, characterNum, id, equipmentNum);
                    }

                    foreach (KeyValuePair<string, Color> color in colors)
                        InsertColor(conn, userId, characterNum, color.Key, color.Value);
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

    private static double GetCharacterStdStat(MySqlConnection conn, int characterId, string statName)
    {
        double stat = 0;

        try
        {
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

        return stat;
    }

    private static double GetCharacterStat(MySqlConnection conn, string userId, int characterNum, string statName)
    {
        double stat = 0;

        try
        {
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

        return stat;
    }

    private static int InsertCharacterStat(MySqlConnection conn, string userId, int characterNum, string statName, double stat)
    {
        int res;

        try
        {
            using (MySqlCommand command = conn.CreateCommand())
            {
                int statCode = StatDB.GetStatCode(conn, statName);

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

        return res;
    }

    private static int InsertEquipment(MySqlConnection conn, string userId, int characterNum, int equipmentId)
    {
        int equipmentNum = 1;
        int reinforce = 1;

        try
        {
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
                    "INSERT INTO character_equipment (user_id, character_num, equipment_id, equipment_num, reinforce) " +
                    "VALUES ('{0}', {1}, {2}, {3}, {4});",
                    userId, characterNum, equipmentId, equipmentNum, reinforce
                );

                command.ExecuteNonQuery();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            equipmentNum = -1;
        }

        return equipmentNum;
    }

    private static int InsertEquipmentSlot(MySqlConnection conn, string userId, int characterNum, int equipmentId, int equipmentNum)
    {
        int res = 0;

        string equipmentType;

        try
        {
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

        return res;
    }

    private static int InsertColor(MySqlConnection conn, string userId, int characterNum, string colorType, Color color)
    {
        int res;

        try
        {
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

        InventoryItem equipment;

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

                            equipment = new()
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

    public static List<Color> SelectColor(string userId, int characterNum)
    {
        List<Color> colors = new();

        List<string> colorType = new() { "hair", "left eye", "right eye", "mustache" };

        float red, green, blue;

        using (MySqlConnection conn = new MySqlConnection(DBSetting.builder.ConnectionString))
        {
            try
            {
                conn.Open();

                using (MySqlCommand command = conn.CreateCommand())
                {
                    foreach (string type in colorType)
                    {
                        command.CommandText = string.Format(
                            "SELECT red, green, blue " +
                            "FROM character_color " +
                            "WHERE user_id = '{0}' " +
                            "AND character_num = {1} " +
                            "AND color_type = '{2}'",
                            userId, characterNum, type
                        );

                        using (MySqlDataReader characterColor = command.ExecuteReader())
                        {
                            characterColor.Read();

                            red = (float)Convert.ToDouble(characterColor["red"]);
                            green = (float)Convert.ToDouble(characterColor["green"]);
                            blue = (float)Convert.ToDouble(characterColor["blue"]);

                            characterColor.Close();
                        }

                        Color color = new() { r = red, g = green, b = blue };

                        colors.Add(color);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                colors = null;
            }
            finally
            {
                conn.Close();
            }
        }

        return colors;
    }
}

public class StatDB : MonoBehaviour
{
    public static int GetStatCode(MySqlConnection conn, string statName)
    {
        int statCode = 0;

        try
        {
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

        return statCode;
    }
}