using UnityEngine;
using System.Collections;

// Controls pause menu interaction during level play
public class PauseMenuControl : MonoBehaviour {

	// Stores reference to this level's LevelControl script
	LevelControl levelControl;

	// Stores reference to UI gameObjects
	GameObject optionsMenu, levelUI;

	// Use this for initialization
	void Start () {

		// Save references to levelControl script, options menu UI objects
		levelControl = GameObject.Find ("Level Control").GetComponent <LevelControl>();
		optionsMenu = transform.Find ("Options").gameObject;
		levelUI = transform.Find ("LevelUI").gameObject;

		// Ensure options menu is not displaying
		optionsMenu.SetActive(false);
	}

	void Update(){

		// Check for escape (or phone back button) input
		if(Input.GetKeyDown(KeyCode.Escape)){

			// If the game is paused, switch to "game" state
			if(levelControl.checkPause()){
				switchLevelState("Game");
			}
			// If not, switch to "options" state
			else{
				switchLevelState("Options");
			}
		}

	}

	// Function used for buttons to switch UI states given desired state name
	public void switchLevelState(string newState){
		
		switch (newState) {

		// Switch to Options Menu
		case "Options":
			
			// Pause game
			levelControl.setPause (true);
			// enable options menu
			optionsMenu.SetActive (true);
			// Turn off level UI
			levelUI.SetActive (false);

			break;

		// Switch to gameplay
		case "Game":
			
			// unpause game
			levelControl.setPause(false);
			// disable options menu
			optionsMenu.SetActive(false);
			// Turn on level UI
			levelUI.SetActive (true);

			break;		
		}
	}

	// Function used for buttons triggering debug actions
	public void special(string specialAction){

		switch (specialAction) {

		// Trigger instant level win
		case "win":
			switchLevelState ("Game");
			levelControl.win ();
			break;

		// Trigger instant level lose
		case "lose":
			switchLevelState ("Game");
			levelControl.lose ();
			break;

		}

	}
}
