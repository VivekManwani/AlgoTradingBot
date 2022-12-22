using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {
	
	public Animator transitionAnim;

	IEnumerator LoadScene(int sceneNumber)
	{
		transitionAnim.SetTrigger ("start");
		yield return new WaitForSeconds(1.5f);
		SceneManager.LoadScene (sceneNumber);
	}

	public void LoadPlayScene()
	{
		StartCoroutine (LoadScene (1));
	}

	public void LoadInstructionsScene()
	{
		SceneManager.LoadScene (2);
	}
}
