using UnityEngine;

[System.Serializable]
public static class SaveFile
{
    private const string KEY_SAVEPOINT = "Save_Point";
    private const string KEY_JUMPAMOUNT = "Jumps";
    private const string KEY_PLAYERLIFE = "Player_Life";
    private const string KEY_LUMS = "Lums";
    private const string KEY_PUZZLE_CAGE = "Puzzle_Cage";
    private const string KEY_PUZZLE_BUTTERFLY = "Puzzle_Butterfly";
    private const string KEY_PUZZLE_STATUE = "Puzzle_Statue";
    private const string KEY_PUZZLE_FLOWER = "Puzzle_Flower";
    private const string KEY_PUZZLE_BOAT = "Puzzle_Boat";
    private const string KEY_PUZZLE_THIEF = "Puzzle_Thief";

    [System.Serializable]
    public class Data
    {
        public string savePoint = "";
        public int jumps = -1;
        public int life = -1;
        public int lums = -1;

        public Data(string savePoint, int jumps, int life, int lums)
        {
            this.savePoint = savePoint;
            this.jumps = jumps;
            this.life = life;
            this.lums = lums;
        }
        public Data() { }
        public bool IsEmpty ()
        {
            return savePoint.Length == 0;
        }
    }

    public static Data Load (int saveFile)
    {
        Data data = new Data();

        data.savePoint = PlayerPrefs.GetString(saveFile + "_" + KEY_SAVEPOINT, "");

        data.jumps = PlayerPrefs.GetInt(saveFile + "_" + KEY_JUMPAMOUNT, -1);
        data.life = PlayerPrefs.GetInt(saveFile + "_" + KEY_PLAYERLIFE, -1);
        data.lums = PlayerPrefs.GetInt(saveFile + "_" + KEY_LUMS, -1);

        return data;
    }
    public static void Save (Data data, int saveFile) 
    {
        PlayerPrefs.SetString(saveFile + "_" + KEY_SAVEPOINT, data.savePoint);
        PlayerPrefs.SetInt(saveFile + "_" + KEY_JUMPAMOUNT, data.jumps);
        PlayerPrefs.SetInt(saveFile + "_" + KEY_PLAYERLIFE, data.life);
        PlayerPrefs.SetInt(saveFile + "_" + KEY_LUMS, data.lums);
    }
}
