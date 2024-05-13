using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

public static class PlayerHandleData
{
    public static void Save(PlayerBlob player)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/player.em";
        FileStream stream = new FileStream(path, FileMode.Create);

        PlayerDataTest data = new PlayerDataTest(player);

        formatter.Serialize(stream, data);

        stream.Close();
    }

    public static PlayerDataTest Load(PlayerBlob player)
    {
        string path = Application.persistentDataPath + "/player.em";
        if(!File.Exists(path))
        {
            Save(player);
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Open);

        PlayerDataTest data = (PlayerDataTest)formatter.Deserialize(stream);

        stream.Close();

        return data;
    }
}
