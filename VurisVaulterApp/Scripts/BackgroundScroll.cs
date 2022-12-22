using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroll : MonoBehaviour {

	public float speed;
	public PlayerController thePlayer;
	
	// Update is called once per frame
	void Update ()
	{

		if (PlayerPrefs.GetInt ("ContinueBool") == 0 || thePlayer.timeLeft < 0) 
		{
			Vector2 offset = new Vector2 (Time.time * speed, 0);

			GetComponent<Renderer> ().material.mainTextureOffset = offset;
		}
	}
}
