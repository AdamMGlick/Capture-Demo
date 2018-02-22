using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

// Controls the UI of the game main menu
public class MainMenuControl : MonoBehaviour {

	// Store main menu UI game objects
	GameObject homeMenu, levelSelectMenu, optionsMenu, exitPrompt;

	// Enum that stores the different main menu states
	enum MenuState {homeMenu, levelSelectMenu, optionsMenu, exitPrompt};

	// Stores the current menu state
	MenuState currentMenuState;

	// Use this for initialization
	void Start () {
		
		if (SceneManager.GetActiveScene ().name == "Main") {
			// Store references to the main menu UI game objects
			homeMenu = transform.Find ("HomeMenu").gameObject;
			levelSelectMenu = transform.Find ("LevelSelect").gameObject;
			optionsMenu = transform.Find ("Options").gameObject;
			exitPrompt = transform.Find ("ExitPrompt").gameObject;

			// Game starts on the main menu
			toggleMenus (homeMenu);
		}
	}

	void Update(){

		if (SceneManager.GetActiveScene ().name == "Main") {
			// Check for escape (or phone back button) input
			if (Input.GetKeyDown (KeyCode.Escape)) {
				// If the exit prompt is displayed at the time of escape pressed, exit game
				if (currentMenuState == MenuState.exitPrompt) {
					exitGame ();
				}
			// If the current menuState is not the main menu, exit to the main menu
			else if (currentMenuState != MenuState.homeMenu) {
					switchMainMenu ("HomeMenu");
				}
			// If the exit prompt is not displayed, and we are at the main menu, display the exit prompt
			else {
					switchMainMenu ("ExitPrompt");
				}
			}
		}
	}

	// Load the scene of the highest unlocked level
	public void playHighestLevel(){

		// If no level is completed, highest level is 0 and we load level 1
		if (GameControl.highestLevelComplete != 0)
			SceneManager.LoadScene (GameControl.highestLevelComplete);
		else {
			SceneManager.LoadScene (1);
		}
	}

	// Exit the game and save
	public void exitGame(){
		Debug.Log ("Bye bye.");
		GameControl.Save ();
		Application.Quit ();
	}
		
	// Takes string as input describing menu to switch to, switches to it
	public void switchMainMenu(string menuName){

		switch (menuName) {

		case "HomeMenu":
			toggleMenus (homeMenu);
			currentMenuState = MenuState.homeMenu;
			break;
		case "LevelSelect":
			toggleMenus (levelSelectMenu);
			currentMenuState = MenuState.levelSelectMenu;
			break;		
		case "Options":
			toggleMenus (optionsMenu);
			currentMenuState = MenuState.optionsMenu;
			break;
		case "ExitPrompt":
			toggleMenus (exitPrompt);
			currentMenuState = MenuState.exitPrompt;
			break;

		}
	}

	// Turns all menus off except that of the given game object
	void toggleMenus(GameObject activeMenu){

		// Turn all menus off
		homeMenu.SetActive (false);
		levelSelectMenu.SetActive (false);
		optionsMenu.SetActive (false);
		exitPrompt.SetActive (false);

		// Turn only the appropriate menu back on
		activeMenu.SetActive (true);
	}

	// Deletes save game file
	public void restartGame(){
		GameControl.deleteSave ();
	}

	// DEBUG function to display contents of levelStateArray (game progress info)
	public void printLevelStateArray(){
		GameControl.printLevelStateArray ();
	}
}
