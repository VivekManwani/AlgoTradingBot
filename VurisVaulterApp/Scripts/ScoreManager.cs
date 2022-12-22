using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour {

	public Text scoreText;
	public Text highScoreText;

	public float scoreCount;
	public float highScoreCount;

	public PlayerController thePlayer;

	public Text countDownText;

	public ObjectPooler spikePool;

	// Use this for initialization
	void Start () {
		if (PlayerPrefs.HasKey ("HighScore")) 
		{
			highScoreCount = PlayerPrefs.GetFloat ("HighScore");
		}

		if (PlayerPrefs.GetInt ("ContinueBool") == 1) 
		{
			scoreCount = PlayerPrefs.GetFloat ("ContinueScore");
		}

	}
	
	// Update is called once per frame
	void Update () 
	{

		if (thePlayer.timeLeft > 0 && PlayerPrefs.GetInt ("ContinueBool") == 1) {
			countDownText.enabled = true;
			countDownText.text = Mathf.Round (thePlayer.timeLeft).ToString();
		} else {
			countDownText.enabled = false;
		}

		if (scoreCount > highScoreCount) 
		{
			highScoreCount = scoreCount;
			PlayerPrefs.SetFloat ("HighScore", highScoreCount);
		}

		scoreText.text = Mathf.Round (scoreCount).ToString ();
	}

	public void AddScore(int pointsToAdd)
	{
		scoreCount += pointsToAdd;

	}
}
