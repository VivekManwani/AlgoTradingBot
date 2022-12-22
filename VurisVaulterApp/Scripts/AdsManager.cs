using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.SceneManagement;

public class AdsManager : MonoBehaviour, IUnityAdsListener {

	string googlePlay_ID = "3908261";
	bool testMode = false;
	string myPlacementId_rewardedVideo = "rewardedVideo";
	string myPlacementId_Video = "video";
	//string myPlacementId_Banner = "bannerPlacement";

	// Use this for initialization
	void Start () {
		Advertisement.AddListener (this);
		Advertisement.Initialize (googlePlay_ID, testMode);
		//StartCoroutine(ShowBannerWhenInitialized());
	}

	public void ShowRewardedVideo() {
		// Check if UnityAds ready before calling Show method:
		if (Advertisement.IsReady(myPlacementId_rewardedVideo)) {
			//GameObject.Find ("Background music").GetComponent<AudioSource> ().Stop();
			Advertisement.Banner.Hide ();
			//Debug.Log ("Continue AD");
			Advertisement.Show(myPlacementId_rewardedVideo);
		}
		else {
			Debug.Log("Rewarded video is not ready at the moment! Please try again later!");
		}
	}

	public void ShowVideoAD(){
		if (Advertisement.IsReady(myPlacementId_Video)) {
			//GameObject.Find ("Background music").GetComponent<AudioSource> ().Stop();
			Advertisement.Banner.Hide ();
			//Debug.Log ("Restart AD");
			Advertisement.Show(myPlacementId_Video);
		}
		else {
			Debug.Log("Video is not ready at the moment! Please try again later!");
		}
	}

	// Implement IUnityAdsListener interface methods:
	public void OnUnityAdsDidFinish (string placementId, ShowResult showResult) {
		// Define conditional logic for each ad completion status:
		if (showResult == ShowResult.Finished) {
			
			// Reward the user for watching the ad to completion.
			if (placementId == myPlacementId_Video)
				Destroy (GameObject.Find("Background music"));
				SceneManager.LoadScene (0);
				//Debug.Log ("Restart button");
			if (placementId == myPlacementId_rewardedVideo)
				//Debug.Log ("Continue button");
				GameObject.FindObjectOfType<DeathMenu> ().RestartWithSameScore ();



		} else if (showResult == ShowResult.Skipped) {
			
			// Do not reward the user for skipping the ad.
			if (placementId == myPlacementId_rewardedVideo)
				GameObject.Find ("Score Manager").GetComponent<ScoreManager> ().scoreText.text = "You did not watch the full video";
			if (placementId == myPlacementId_Video)
				Destroy (GameObject.Find("Background music"));
				SceneManager.LoadScene (0);
			
		} else if (showResult == ShowResult.Failed) {
			
			if (placementId == myPlacementId_rewardedVideo)
				GameObject.Find ("Score Manager").GetComponent<ScoreManager> ().scoreText.text = "Video failed, check you internet!";
			if (placementId == myPlacementId_Video)
				Destroy (GameObject.Find("Background music"));
				SceneManager.LoadScene (0);

		}
	}

	public void OnUnityAdsReady (string placementId) {
		// If the ready Placement is rewarded, show the ad:
		if (placementId == myPlacementId_rewardedVideo) {
			// Optional actions to take when the placement becomes ready(For example, enable the rewarded ads button)
		}
	}

	public void OnUnityAdsDidError (string message) {
		// Log the error.
	}

	public void OnUnityAdsDidStart (string placementId) {
		// Optional actions to take when the end-users triggers an ad.
	} 
/*
	// Banner Ad function
	IEnumerator ShowBannerWhenInitialized () {
		while (!Advertisement.isInitialized) {
			yield return new WaitForSeconds(0.5f);
		}
		Advertisement.Banner.SetPosition (BannerPosition.BOTTOM_CENTER);
		Advertisement.Banner.Show (myPlacementId_Banner);
	} */
}
