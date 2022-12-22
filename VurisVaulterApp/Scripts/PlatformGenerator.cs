using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformGenerator : MonoBehaviour {

	public GameObject thePlatform;
	public Transform generationPoint;
	private float distanceBetween;

	public float distanceBetweenMax;
	public float distanceBetweenMin;

	private int platformSelector;
	private float[] platformWidths;

	public ObjectPooler[] theObectPools;

	private float minHeight;
	private float maxHeight;
	public Transform maxHeightPoint;
	public float maxHeightChange;
	private float heightChange;

	public float randomSpikeThreshold;
	public ObjectPooler spikePool;
	public ObjectPooler rectanglePointsPool;


	// Use this for initialization
	void Start () {
		platformWidths = new float[theObectPools.Length];

		for (int i = 0; i < theObectPools.Length; i++) 
		{
			platformWidths[i] = theObectPools[i].pooledObject.GetComponent<BoxCollider2D> ().size.x;
		}

		minHeight = transform.position.y;
		maxHeight = maxHeightPoint.position.y;

		
	}
	
	// Update is called once per frame
	void Update () {

		if (transform.position.x < generationPoint.position.x) 
		{
			distanceBetween = Random.Range (distanceBetweenMin, distanceBetweenMax);

			platformSelector = Random.Range (0, theObectPools.Length);

			heightChange = transform.position.y + Random.Range(maxHeightChange, -maxHeightChange);

			if (heightChange > maxHeight) 
			{
				heightChange = maxHeight;

			} else if (heightChange < minHeight) 
			{
				heightChange = minHeight;
			}

			transform.position = new Vector3 (transform.position.x + (platformWidths[platformSelector]/2) + distanceBetween, heightChange, transform.position.z);

			GameObject newPlatform = theObectPools[platformSelector].GetPooledObject();

			newPlatform.transform.position = transform.position;
			newPlatform.transform.rotation = transform.rotation;
			newPlatform.SetActive (true);

			if (Random.Range (0f, 100f) < randomSpikeThreshold) 
			{
				GameObject newSpike = spikePool.GetPooledObject ();
				GameObject newPointsRectangle = rectanglePointsPool.GetPooledObject ();

				float spikeXPosition = Random.Range (-platformWidths [platformSelector] / 2f + 0.5f, platformWidths [platformSelector] / 2f - 0.5f);

				Vector3 spikePosition = new Vector3 (spikeXPosition, 1f, 0f);

				newSpike.transform.position = transform.position + spikePosition;
				newSpike.transform.rotation = transform.rotation;
				newSpike.SetActive (true);

				newPointsRectangle.transform.position = transform.position + spikePosition;
				newPointsRectangle.transform.rotation = transform.rotation;
				newPointsRectangle.SetActive (true);

			}

			transform.position = new Vector3 (transform.position.x + (platformWidths[platformSelector]/2), transform.position.y, transform.position.z);
		}
	}
}
