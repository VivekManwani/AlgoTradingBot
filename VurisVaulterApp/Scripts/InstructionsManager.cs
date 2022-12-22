using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionsManager : MonoBehaviour {

	public GameObject instructionsImage;

	public void BackButton(){
		instructionsImage.SetActive (false);
	}

	public void InstructionsPage()
	{
		instructionsImage.SetActive (true);
	}
}
