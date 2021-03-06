﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CnControls;
using UnityEngine.UI;

public class dialogueHolder : MonoBehaviour {

	public string dialogue = "Hi,Bunny...I need Cherry";
	public DialogueManager dMAn;
	public PlayerController bunny;
	bool showwin;
	bool showmessage;
    public GameObject Panel2;
    public int scene;

    
	// Use this for initialization
	void Start () {
		showwin = false;
		showmessage = false;
		dMAn = (DialogueManager)FindObjectOfType(typeof(DialogueManager));
		Debug.Log (dMAn);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerStay2D(Collider2D col) {
		if (col.transform.tag == "Player") {
			bunny = col.GetComponent<PlayerController> ();
			if (bunny.findTarget && showwin == false) {
                PlayerPrefs.SetFloat(scene+"",PlayerController.curHealth);
                showwin = true;
                Panel2.SetActive(true);
			}
			else if (!bunny.findTarget && showmessage == false) {
				showmessage = true;
				dMAn.ShowBox (dialogue);
			}
		}
	}


}
