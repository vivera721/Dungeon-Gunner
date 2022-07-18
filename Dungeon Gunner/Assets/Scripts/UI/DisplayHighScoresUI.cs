using UnityEngine;

public class DisplayHighScoresUI : MonoBehaviour
{
    #region Header OBJECT REFERENCES
    [Space(10)]
    [Header("OBJECT REFERENCES")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with the child Content gameobject Transform component")]
    #endregion
    [SerializeField] private Transform contentAnchorTransform;


    private void Start()
    {
        DisplayScores();
    }

    /// <summary>
    /// Display Scores
    /// </summary>
    private void DisplayScores()
    {
        HighScores highScores = HighScoreManager.Instance.GetHighScores();
        GameObject scoreGameobject;

        int rank = 0;
        foreach (Score score in highScores.scoreList)
        {
            rank++;

            // Instantiate scores gameobject
            scoreGameobject = Instantiate(GameResources.Instance.scorePrefab, contentAnchorTransform);

            ScorePrefab scorePrefab = scoreGameobject.GetComponent<ScorePrefab>();

            // Populate
            scorePrefab.rankTMP.text = rank.ToString();
            scorePrefab.nameTMP.text = score.playerName;
            scorePrefab.levelTMP.text = score.levelDescription;
            scorePrefab.scoreTMP.text = score.playerScore.ToString("###,###0");

        }

        // Add blank line
        // Instantiate scores gameobject
        scoreGameobject = Instantiate(GameResources.Instance.scorePrefab, contentAnchorTransform);
    }

}
