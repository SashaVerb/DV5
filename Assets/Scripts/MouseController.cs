using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour {

	public float jetpackForce = 75.0f;
	public float forwardMovementSpeed = 3.0f;

	private bool grounded;
	public LayerMask groundCheckLayerMask;
	Animator animator;

	public ParticleSystem jetpack;

	private bool dead = false;

	private uint coins = 0;

	public Texture2D coinIconTexture;
	public AudioClip coinCollectSound;
	public AudioSource jetpackAudio;
	public AudioSource footstepsAudio;

	public ParallaxScroll parallax;

	public Rigidbody2D rigidBody;
	void Start () {
		animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody2D>();

    }

	void FixedUpdate () 
	{
        bool jetpackActive = Input.GetButton("Fire1");

		jetpackActive = jetpackActive && !dead;

		if (jetpackActive)
		{
            rigidBody.AddForce(new Vector2(0, jetpackForce));
		}

		if (!dead)
		{
			Vector2 newVelocity = GetComponent<Rigidbody2D>().velocity;
			newVelocity.x = forwardMovementSpeed;
			GetComponent<Rigidbody2D>().velocity = newVelocity;
		}

		AdjustJetpack(jetpackActive);
		AdjustFootstepsAndJetpackSound(jetpackActive);

		parallax.offset = transform.position.x;
	}

	void OnGUI()
	{
		DisplayCoinsCount();
		DisplayRestartButton ();
	}

	void DisplayCoinsCount()
	{
		Rect coinIconRect = new Rect(10, 10, 32, 32);
		GUI.DrawTexture(coinIconRect, coinIconTexture);                         

		GUIStyle style = new GUIStyle();
		style.fontSize = 30;
		style.fontStyle = FontStyle.Bold;
		style.normal.textColor = Color.yellow;

		Rect labelRect = new Rect(coinIconRect.xMax, coinIconRect.y, 60, 32);
		GUI.Label(labelRect, coins.ToString(), style);
	}

	void DisplayRestartButton()
	{
		if (dead && grounded)
		{
			Rect buttonRect = new Rect(Screen.width * 0.35f, Screen.height * 0.45f, Screen.width * 0.30f, Screen.height * 0.1f);
			if (GUI.Button(buttonRect, "Tap to restart!"))
			{
				Application.LoadLevel (Application.loadedLevelName);
			};
		}
	}
    private void OnCollisionEnter2D(Collision2D collision)
    {
		if (((1 << collision.gameObject.layer) & groundCheckLayerMask.value) != 0)
		{
			UpdateGroundedStatus(true);
        }
	}

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundCheckLayerMask.value) != 0 && rigidBody.velocity.y > 0)
        {
            UpdateGroundedStatus(false);
        }
    }

    void UpdateGroundedStatus(bool groundedStatus)
	{
        //1
        grounded = groundedStatus;
		//2
		animator.SetBool("grounded", groundedStatus);
	}

	void AdjustJetpack (bool jetpackActive)
    {
		if (grounded)
		{
			jetpack.Stop();
		}
		else
		{
			jetpack.Play();
		}

		jetpack.emissionRate = jetpackActive ? 300.0f : 75.0f; 

	}

	void OnTriggerEnter2D(Collider2D collider)
	{
		if (collider.gameObject.CompareTag("Coins"))
			CollectCoin(collider);
		else
			HitByLaser(collider);
	}

	void HitByLaser(Collider2D laserCollider)
	{
		if (!dead)
			laserCollider.gameObject.GetComponent<AudioSource>().Play();
		dead = true;
		GetComponent<Animator> ().SetBool ("dead", true);
	}

	void CollectCoin(Collider2D coinCollider)
	{
		AudioSource.PlayClipAtPoint(coinCollectSound, transform.position);
		coins++;

		Destroy(coinCollider.gameObject);
	}

	void AdjustFootstepsAndJetpackSound(bool jetpackActive)    
	{
		footstepsAudio.enabled = !dead && grounded;

		jetpackAudio.enabled =  !dead && !grounded;
		jetpackAudio.volume = jetpackActive ? 1.0f : 0.5f;        
	}
}
