using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {

	public float moveSpeed;
	public float jumpForce;

	bool isOnGround;

	private Rigidbody2D myRigidBody;

	private Animator myAnimator;

	private bool canDoubleJump;

	public LayerMask whatIsGround;
	public Transform groundCheck;
	public float groundCheckRadius;

	public Transform ContinuePoint;

	public ScoreManager theScoreManager;

	public float timeLeft = 3.0f;

	public AudioSource jumpSound;

	// Use this for initialization
	void Start () 
	{
		myRigidBody = GetComponent<Rigidbody2D> ();
		myAnimator = GetComponent<Animator> ();

		if (PlayerPrefs.GetInt ("ContinueBool") == 1) 
		{
			gameObject.transform.position = ContinuePoint.position;

		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (PlayerPrefs.GetInt ("ContinueBool") == 1) {
			timeLeft -= Time.deltaTime;
			if (timeLeft < 0) {
				MoveForward ();

				isOnGround = Physics2D.OverlapCircle (groundCheck.position, groundCheckRadius, whatIsGround);

				if (Input.GetMouseButtonDown(0)) {
					if (!isOnGround && canDoubleJump) {
						jumpSound.Play ();
						jump ();
						canDoubleJump = false;

					}

					if (isOnGround) {
						jumpSound.Play ();
						jump ();

					}
				}

				if (isOnGround) {
					canDoubleJump = true;
				}

				myAnimator.SetFloat ("Speed", myRigidBody.velocity.x);
				myAnimator.SetBool ("Grounded", isOnGround);
			}
		} else {
			
			MoveForward ();

			isOnGround = Physics2D.OverlapCircle (groundCheck.position, groundCheckRadius, whatIsGround);

			if (Input.GetMouseButtonDown(0)) {
				if (!isOnGround && canDoubleJump) {
					jumpSound.Play ();
					jump ();
					canDoubleJump = false;

				}

				if (isOnGround) {
					jumpSound.Play ();
					jump ();

				}
			}

			if (isOnGround) {
				canDoubleJump = true;
			}

			myAnimator.SetFloat ("Speed", myRigidBody.velocity.x);
			myAnimator.SetBool ("Grounded", isOnGround);
		}
	}

	// Functions defined by me ----------------------------------------------------------------------------------------

	private void MoveForward()
	{
		myRigidBody.velocity = new Vector2 (moveSpeed, myRigidBody.velocity.y);
	}

	private void OnCollisionEnter2D(Collision2D coll)
	{

		if (coll.gameObject.tag == "killbox") 
		{
			GameObject.Find ("Background music").GetComponent<AudioSource> ().Stop ();
			PlayerPrefs.SetFloat ("ContinueScore", theScoreManager.scoreCount);
			PlayerPrefs.SetInt("ContinueBool", 0);
			GameObject.FindObjectOfType<SceneLoader>().transitionAnim.SetTrigger ("start");
			SceneManager.LoadScene (2);
		}

	}

	private void jump()
	{
		myRigidBody.velocity = new Vector2 (myRigidBody.velocity.x, jumpForce);
		isOnGround = false;
	}
}
