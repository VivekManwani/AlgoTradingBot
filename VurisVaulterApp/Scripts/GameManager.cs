using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
	
	// Use this for initialization
	void Start () {

		DontDestroyOnLoad (gameObject);
	}

}
