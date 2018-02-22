using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Controls the functions of an individual level such as win and lose conditions
public class LevelControl : MonoBehaviour {

	public int levelNum;

	// Keep track of enemy and player quantities during gameplay
	int numEnemyCells, numPlayerCells, numEnemyDrones, numPlayerDrones;

	// Bool for if the level is over
	public bool levelEnd = false;

	// Bool for if the level is paused
	public bool isPaused = false;

	GameObject levelPauseMenu, mainMenuButton, nextLevelButton, restartLevelButton;
	Text levelText;

	// Text displayed to player on level end
	string winText = "Section Cleared."; 
	string loseText = "Section Failed.";

	void Start(){

		// Save references to level pause text
		levelPauseMenu = GameObject.Find ("LevelPauseMenu");
		levelText = GameObject.Find ("LevelText").GetComponent<Text>();

		// LevelNum currently corresponds to scene buildIndex (main menu has build index 0)
		levelNum = SceneManager.GetActiveScene ().buildIndex;

		// Save references to in-level pause menu buttons
		//mainMenuButton = GameObject.Find ("MainMenuButton");
		nextLevelButton = GameObject.Find ("NextLevelButton");
		restartLevelButton = GameObject.Find ("RestartLevelButton");

		// Ensure pause menu is not displaying
		levelPauseMenu.SetActive (false);
	}

	// Functions to increment and decrement player and enemy quantities, check level end
	public void addEnemyCell(){
		numEnemyCells++;
	}
	public void removeEnemyCell(){
		numEnemyCells--;
		checkWin ();
	}
	public void addPlayerCell(){
		numPlayerCells++;
	}
	public void removePlayerCell(){
		numPlayerCells--;
		checkLose();
	}
	public void addEnemyDrone(){
		numEnemyDrones++;
	}
	public void removeEnemyDrone(){
		numEnemyDrones--;
		checkWin();
	}
	public void addPlayerDrone(){
		numPlayerDrones++;
	}
	public void removePlayerDrone(){
		numPlayerDrones--;
		checkLose();
	}

	// If there are no enemy cells or drones, the player wins
	void checkWin(){
		if (numEnemyCells == 0 && numEnemyDrones == 0) {
			// player wins
			win();
		}
	}

	// If there are no player cells or drones, the player loses
	void checkLose(){
		if (numPlayerCells == 0 && numPlayerDrones == 0) {
			// player loses
			lose();
		}
	}

	// End of game result where player wins
	public void win(){

		// Text displayed to player on win
		levelText.text = winText;

		// Turn on level end UI
		levelPauseMenu.SetActive (true);

		// If this is not the final level
		if (levelNum < GameControl.numLevels) {
			// turn on next level button, turn off restart level button
			nextLevelButton.SetActive (true);
			restartLevelButton.SetActive (false);
		} else {
			//nextLevelButton.SetActive (false);
			restartLevelButton.SetActive (true);
		}

		// change levelEnd bool to reflect the level ending
		levelEnd = true;

		Debug.Log ("Section Cured");

		// Tell GameControl script that this level has been completed (for progress saving)
		GameControl.completedLevel (levelNum);


	}

	// End of game result where player loses
	public void lose(){
		// Text displayed to player on lose
		levelText.text = loseText;

		// Turn on level end UI
		levelPauseMenu.SetActive (true);

		// turn off next level button, turn on restart level button
		nextLevelButton.SetActive (false);
		restartLevelButton.SetActive (true);

		// change levelEnd bool to reflect the level ending
		levelEnd = true;

		Debug.Log ("Section Lost");
	}

	// Returns true if the level is paused, false otherwise
	public bool checkPause(){
		return isPaused;
	}

	// Sets pause bool to true or false based on argument
	public void setPause(bool setTo){
		isPaused = setTo;
	}
}
