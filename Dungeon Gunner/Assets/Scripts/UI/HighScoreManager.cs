using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
// 파일 저장을 위한 System.IO
// 그리고 c# 바이너리 포맷터가 필요


public class HighScoreManager : SingletoneMonobehaviour<HighScoreManager>
{
    private HighScores highScores = new HighScores();

    protected override void Awake()
    {
        base.Awake();

        // 저장된 점수 불러오기
        LoadScores();
    }

    /// <summary>
    /// Load Scores From Disk
    /// </summary>
    private void LoadScores()
    {
        BinaryFormatter bf = new BinaryFormatter();

        if (File.Exists(Application.persistentDataPath + "/DungeonGunnerHighScores.dat"))
        {
            ClearScoreList();

            FileStream file = File.OpenRead(Application.persistentDataPath + "/DungeonGunnerHighScores.dat");

            highScores = (HighScores)bf.Deserialize(file);

            file.Close();
        }
    }

    /// <summary>
    /// Clear all scores
    /// </summary>
    private void ClearScoreList()
    {
        highScores.scoreList.Clear();
    }

    /// <summary>
    /// Add score to high scores list
    /// </summary>
    public void AddScore(Score score, int rank)
    {
        highScores.scoreList.Insert(rank - 1, score);


        // Maintain the maximum number of score to save
        if (highScores.scoreList.Count > Settings.numberOfHighScoresToSave)
        {
            highScores.scoreList.RemoveAt(Settings.numberOfHighScoresToSave);
        }

        SaveScore();
    }

    /// <summary>
    /// Save scores to disk
    /// </summary>
    private void SaveScore()
    {
        BinaryFormatter bf = new BinaryFormatter();

        FileStream file = File.Create(Application.persistentDataPath + "/DungeonGunnerHighScores.dat");

        bf.Serialize(file, highScores);

        file.Close();

    }

    /// <summary>
    /// Get Highscores
    /// </summary>
    public HighScores GetHighScores()
    {
        return highScores;
    }

    /// <summary>
    /// Return the reank of the playerScore compared to the other high scores (return 0 if the score isn't higher than any in the high scores list)
    /// </summary>
    public int GetRank(long playerScore)
    {
        // If there are no scores currently in the list - then this score must be ranked 1 - then return
        if (highScores.scoreList.Count == 0) return 1;

        int index = 0;

        // Loop through scores in list to find the rank of this score
        for (int i = 0; i < highScores.scoreList.Count; i++)
        {
            index++;

            if (playerScore >= highScores.scoreList[i].playerScore)
            {
                return index;
            }
        }

        if (highScores.scoreList.Count < Settings.numberOfHighScoresToSave)
            return (index + 1);

        return 0;
    }

}
