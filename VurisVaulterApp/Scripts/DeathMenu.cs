using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathMenu : MonoBehaviour {

	public Text scoreText;
	public Text highScoreText;

	private float highScoreCount;
	private float scoreCount;

	void Start()
	{
		if (PlayerPrefs.HasKey ("HighScore")) 
		{
			highScoreCount = PlayerPrefs.GetFloat ("HighScore");
		}

		scoreCount = PlayerPrefs.GetFloat ("ContinueScore");

		scoreText.text = "Score: " + Mathf.Round(scoreCount);
		highScoreText.text = "High Score: " + Mathf.Round(highScoreCount);
	}

	public void PlayAgain()
	{
		GameObject.Find ("Background music").GetComponent<AudioSource> ().Play ();
		SceneManager.LoadScene (1);
	}

	public void RestartWithSameScore()
	{
		PlayerPrefs.SetInt("ContinueBool", 1);
		GameObject.Find ("Background music").GetComponent<AudioSource> ().Play ();
		SceneManager.LoadScene (1);
	}
}
