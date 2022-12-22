using System.Collections;
using UnityEngine;

public class PickUpPoints : MonoBehaviour {

	public int scoreToGive;
	private ScoreManager thesScoreManager;

	private AudioSource pointsSound;


	// Use this for initialization
	void Start () {
		thesScoreManager = FindObjectOfType<ScoreManager> ();

		pointsSound = GameObject.Find ("Coin Sound Effect").GetComponent<AudioSource> ();
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.name == "Player") 
		{
			pointsSound.Play ();
			thesScoreManager.AddScore (scoreToGive);
			gameObject.SetActive (false);
		}
	}
}
