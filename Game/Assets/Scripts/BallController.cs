﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour {
	public enum Walk_Direction {Right, Left};
	public float vJumpHeight = 2f;
	public bool vCanMove = true;

	public float vDistanceGround = 2f;
	public float vJumpSpeed = 4f;
	public Quaternion rotation;
	public float speed = 0.6f;
	public float vWalkSpeed = 4f;
	public float JumpForce = 400f;
	public bool CanWalkOnPlateform = false;		

	private float vCenterDist;
	private float vLeftDist;
	private float vRightDist;
	private GameObject vCurPlanet;
	private bool IsJumping = false;
	private Rigidbody2D myRigidBody;
	private float vRotateSpeed = 10f;	
	private GameObject vLeftObj, vRightObj;
	private SpriteRenderer myRenderer;
	private PlanetCollider vPlanetCollider;
	// Use this for initialization
	void Start () {
		myRigidBody = GetComponent<Rigidbody2D> ();
		myRenderer = GetComponent<SpriteRenderer> ();
		vCurPlanet = null;
		//keep the rotation in mind
		Vector3 vOriginalRotation = transform.rotation.eulerAngles;

		//rotate object to be normal
		transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));

		//create left probe
		vLeftObj = Instantiate(Resources.Load("Probe") as GameObject);
		vLeftObj.transform.position = myRenderer.bounds.center + new Vector3(myRenderer.bounds.extents.x/2, 0f, 0f);
		vLeftObj.transform.parent = transform;
		//vLeftObj.hideFlags = HideFlags.HideInHierarchy;

		//create left probe
		vRightObj = Instantiate(Resources.Load("Probe") as GameObject);
		vRightObj.transform.position = myRenderer.bounds.center + new Vector3(-myRenderer.bounds.extents.x/2, 0f, 0f);
		vRightObj.transform.parent = transform;
		//vRightObj.hideFlags = HideFlags.HideInHierarchy;
		//then rotate to the original rotation
		transform.rotation = Quaternion.Euler(vOriginalRotation);

		if (CanJumpOnOtherPlanets) {
			IsReadyToChange = false;
			GameObject vCircleCollider = Instantiate(Resources.Load("CircleCollider") as GameObject);
			vCircleCollider.transform.parent = transform;
			vCircleCollider.transform.localPosition = new Vector3 (0f, 0f, 0f);
			vPlanetCollider = vCircleCollider.GetComponent<PlanetCollider> ();
			//vCircleCollider.hideFlags = HideFlags.HideInHierarchy;
		}
	}

	private Vector3 pos;
	public bool IsAutoWalking = false;
	public bool IsPlayer = true;
	public bool CanJump = true;
	private bool IsWalking;
	public Walk_Direction WalkingDirection = Walk_Direction.Right;
	private float vElapsedHeight = 0f;
	private bool IsReadyToChange = false;
	public bool CanJumpOnOtherPlanets = true;	
	// Update is called once per frame
	void Update () {
		//check if this character can move freely or it's disabled
		if (vCanMove) {
			pos = Vector3.zero;

			//check if going RIGHT
			if ((IsPlayer && Input.GetAxis ("Horizontal") > 0 && !Input.GetButtonUp ("Horizontal")) || (IsAutoWalking && WalkingDirection == Walk_Direction.Right)) {
				pos += Vector3.right * vWalkSpeed * Time.deltaTime;
				WalkingDirection = Walk_Direction.Right;
			}

			//check if going LEFT
			if ((IsPlayer && Input.GetAxis ("Horizontal") < 0 && !Input.GetButtonUp ("Horizontal")) || (IsAutoWalking && WalkingDirection == Walk_Direction.Left)) {
				pos += Vector3.left * vWalkSpeed * Time.deltaTime;
				WalkingDirection = Walk_Direction.Left;
			}

//			//check if JUMP
			if (IsPlayer && Input.GetAxis ("Vertical") > 0 && !IsJumping && CanJump) {
				IsJumping = true;
				CanJump = false;
				vElapsedHeight = 0f;
				IsReadyToChange = true;

				//check if there is a nearby planet if JUMP and CAN change planets is activated
				if (CanJumpOnOtherPlanets)
					CheckIfNearbyPlanet ();
			}

			//check if the character is walking
			if (pos != Vector3.zero)
				IsWalking = true;
			else
				IsWalking = false;

			//ONLY show walking animation when moving!
			if (IsWalking) {
				//move
				transform.Translate (pos);
//
//				//increase time
//				elapseanimation += Time.deltaTime;
//				if (elapseanimation >= animationSpeed) {
//					UpdateCharacterAnimation ();
//					elapseanimation = 0f;
//				}
			}


		}
	}
	
	void CheckIfNearbyPlanet()
	{
		bool vFound = false;

		foreach (GameObject vPlanet in vPlanetCollider.vPlanetList)
			//Debug.Log (vPlanetCollider);
			if (vPlanet != vCurPlanet && !vFound && vPlanet != transform.gameObject) {
				Debug.Log ("in");
				//we found a planet. we transfert to the first one not in the range.
				vFound = true;
				IsReadyToChange = false; //cannot change planet without making another jump. Prevent the character to be stuck between 2 planets
				//change the planet
				vCurPlanet = vPlanet;

				//make sure the character scale isn't changed between planets
				transform.parent = vCurPlanet.transform;
			}
	}

	void FixedUpdate () {

		keepItDownDirectionPointToPlanet ();

	}

	void keepItDownDirectionPointToPlanet() {
		Vector3 fwd = (Vector2)vLeftObj.transform.TransformDirection(-Vector3.up);

		//initialise variables
		vLeftDist = 99f;
		vRightDist = 99f;
		vCenterDist = 0f;

		Vector3 vlpos = vRightObj.transform.position;

		//left ray
		RaycastHit2D[] hitAlll = Physics2D.RaycastAll(vlpos, (Vector2)(fwd));

		//get the first planet in range and make it's own
		if (vCurPlanet == null) {
			foreach (RaycastHit2D hit in hitAlll) {
				if (hit.transform.tag == "Planet" && vCurPlanet == null && hit.transform.gameObject != transform.gameObject)
					vCurPlanet = hit.transform.gameObject;
			}
		}

		foreach (RaycastHit2D hit in hitAlll){
			if (vCurPlanet == hit.transform.gameObject) {
				Debug.DrawRay (vlpos, (Vector3)hit.point - vlpos, Color.blue);	
				vLeftDist = Vector3.Distance (vlpos, (Vector3)hit.point);
			//	Debug.Log (vLeftDist);
			}
		}

		Vector3 vrpos = vLeftObj.transform.position;

		//right ray
		RaycastHit2D[] hitAllr = Physics2D.RaycastAll(vrpos, (Vector2)(fwd));
		foreach (RaycastHit2D hit in hitAllr)
			if (vCurPlanet == hit.transform.gameObject) {
				Debug.DrawRay (vrpos, (Vector3)hit.point-vrpos, Color.blue);	
				vRightDist = Vector3.Distance(vrpos,(Vector3)hit.point);
				//Debug.Log (vRightDist);
			}

		//center to be able to have some kind of gravity
		RaycastHit2D[] hitAllc = Physics2D.RaycastAll(myRenderer.bounds.center, (Vector2)(fwd));
		bool vFoundGround = false;
		foreach (RaycastHit2D hit in hitAllc) {
			if (vCurPlanet == hit.transform.gameObject || ((hit.transform.tag == "Plateform" || hit.transform.tag == "Pushable")&& CanWalkOnPlateform)) {
				Debug.DrawRay (myRenderer.bounds.center, (Vector3)hit.point - transform.position, Color.blue);	
				if (Vector3.Distance (myRenderer.bounds.center, (Vector3)hit.point) < vCenterDist || !vFoundGround) {
					vCenterDist = Vector3.Distance (myRenderer.bounds.center, (Vector3)hit.point);
					vFoundGround = true;
				}
			}
		}
		//check if the Left or Right Probe ARE inside the collider which mean they cannot know the distance. 
		//So we just rotate them to get a distance
		if (vLeftDist == 0f) vRightDist = 99f;
		if (vRightDist == 0f) vLeftDist = 99f;

		//make sure if we doesn't find anything below the character, make him rotate until we find the floor!
		if (vLeftDist == 99f && vRightDist == 99f) {
			vLeftDist = 0f;
		}

		//check if we rotate the character
		if (vLeftDist != vRightDist) {

			//check we rotate at which speed.
			float vDiff = vLeftDist-vRightDist;
			if (vDiff < 0)
				vDiff *= -1;

			//here we calculate how fast we must rotate the character
			if (vDiff < 0.2f)
				vRotateSpeed = 30f;				//small rotation to be smooth and be able to have the same exact position between Left and Right
			else if (vDiff >= 0.2f && vDiff < 0.4f)
				vRotateSpeed = 80f;				//need to turn a little bit faster
			else
				vRotateSpeed = 400f;			//we must turn VERY quick because it's a big corner

			//rotate the character in the direction it's going
			if (vLeftDist < vRightDist)
				RotateObj ("Left");
			else
				RotateObj ("Right");
		}

		//always keep the same distance on this new field
		if (vCenterDist > vDistanceGround && !IsJumping) {
			//walk
			transform.Translate (-Vector3.up * vJumpSpeed * Time.deltaTime);
		} else if (IsJumping) {	//going to the jumpheight
			//make him going UP
			transform.Translate (Vector3.up * vJumpSpeed * Time.deltaTime);

			//increase jump time
			vElapsedHeight += Time.deltaTime * vJumpSpeed;

			//check if we jumped enought
			if (vElapsedHeight >= vJumpHeight) {
				IsJumping = false;
				IsReadyToChange = false;
			}
			//			myRigidBody.AddForce(transform.up * 500f);
			//			IsJumping = false;
			//			IsReadyToChange = false;
		} else if (!CanJump && Input.GetAxis ("Vertical") == 0) {
			CanJump = true;
		}
		else if (vCenterDist < (vDistanceGround-0.1f) && !IsJumping){
			//make him going UP
			transform.Translate (Vector3.up * vJumpSpeed * Time.deltaTime);
		}

	}
	void RotateObj(string vDirection)
	{
		//initialise variable
		float RotateByAngle = 0f;

		//check which direction we are rotating
		if (vDirection == "Right")
			RotateByAngle = Time.deltaTime*vRotateSpeed;
		else
			RotateByAngle = -Time.deltaTime*vRotateSpeed;

		//rotate
		Vector3 temp = transform.rotation.eulerAngles;
		temp.x = 0f;
		temp.y = 0f;
		temp.z += RotateByAngle;
		transform.rotation = Quaternion.Euler(temp);
	}

	void MoveRight () {
		Vector3 position = transform.position;
		position += (vLeftObj.transform.position - myRenderer.bounds.center) * vWalkSpeed * Time.deltaTime;
		transform.position = position;
	}
	void MoveLeft () {
		Vector2 position = transform.position;
		position.x -= speed;
		transform.position = position;
	}
	void MoveUp () {
		Vector2 position = transform.position;
		position.y += speed;
		transform.position = position;
	}
	void MoveDown () {
		Vector2 position = transform.position;
		position.y -= speed;
		transform.position = position;
	}
	void OnCollisionEnter2D(Collision2D col) {
		if (col.gameObject.tag == "Left") {
			Physics2D.gravity = new Vector2 (-9.8f, 0f);
			myRigidBody.MoveRotation (-90f);
		}
		if (col.gameObject.tag == "Bottom") {
			Physics2D.gravity = new Vector2 (0f, -9.8f);
			myRigidBody.MoveRotation (0f);
		}
		if (col.gameObject.tag == "Right") {
			Physics2D.gravity = new Vector2 (9.8f, 0f);
			myRigidBody.MoveRotation (90f);
		}
		if (col.gameObject.tag == "Top") {
			Physics2D.gravity = new Vector2 (0f, 9.8f);
			myRigidBody.MoveRotation (180f);
		}
	}
}
