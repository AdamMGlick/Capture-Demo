using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;

// Controls general game features such as game progress, saving, loading
public class GameControl : MonoBehaviour {

	// Stores the active gameControl script
	public static GameControl gameControl;

	// Stores all savegame data
	public static GameData gameData = new GameData();

	// Total number of levels in the game
	public static int numLevels = 12;

	// Stores the highest level completed
	public static int highestLevelComplete;

	// Stores level progression data
	static SimpleLevelState[] levelStateArray;

	// Stores references to the level selection buttons
	static Selectable[] levelButtonArray;

	// Stores button panel gameObject holding level selection buttons
	static Transform levelButtonPanel;

	// Level selection button colors
	static Color levelCompletedColor = new Color32 (109, 213, 131, 255);
	static Color levelUnlockedColor = new Color32 (255, 255, 255, 255);
	static Color mouseOverColor = new Color32 (255, 255, 255, 255);

	// Keeps track of whether or not the Start() function has run yet
	static bool startHasRun;

	void Start(){

		// Indicate that the Start() function has run
		startHasRun = true;

		// Initialize level selection button array
		levelButtonArray = new Selectable[numLevels];

		// Initialize level progressiond data array
		levelStateArray = new SimpleLevelState[numLevels];

		// If the current scene is the main menu scene
		if(SceneManager.GetActiveScene().name == "Main"){

			// Initialize storage of level selection buttons in script
			initializeLevelButtons ();

			// Load save file
			Load ();
		}
	}
		
	void Awake () {

		// If we don't have a gameControl yet, this will be it
		if (gameControl == null) {

			// This gameObject should persist through the different scenes
			DontDestroyOnLoad (gameObject);

			// Save the reference to this gameControl
			gameControl = this;
		} 
		// If we already have a gameControl, delete this one
		else if (gameControl != this) {
			Destroy (gameObject);
		}

		// Only after start is run (and everything is initialized)
		if(startHasRun){

			// If the current scene is the main menu scene
			if(SceneManager.GetActiveScene().name == "Main"){
				// Re-initialize storage of level selection buttons in script
				initializeLevelButtons ();
				// Update level selection buttons based on level progression data
				updateLevelButtons ();
			}
		}
	}

	// Saves gamedata class to gamesave.dat
	public static void  Save(){
		// Insitialize binary formatter and save file
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream saveFile = File.Create(Application.persistentDataPath + "/gameSave.dat");

		// Save script game data to gameData class
		gameData.levelStateArray = levelStateArray;
		gameData.highestLevelComplete = highestLevelComplete;

		// Serialize data and close save file
		bf.Serialize (saveFile, gameData);
		saveFile.Close ();
	}

	// Delete the save file - activated via UI button in MainMenuControl.CS
	public static void deleteSave(){

		// Delete the save file
		File.Delete (Application.persistentDataPath + "/gameSave.dat");

		// Load game to reset game with no save
		Load ();

		Debug.Log ("Save file deleted.");
	}

	// Attempts to load a savegame from a file
	static void Load(){

		// If we have a save game, load it
		if (File.Exists (Application.persistentDataPath + "/gameSave.dat")) {

			// initialize binary formatter and save file
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream saveFile = File.Open (Application.persistentDataPath + "/gameSave.dat", FileMode.Open);

			// Pull gameData object out of save file
			GameData data = (GameData)bf.Deserialize (saveFile);

			// Save data from file to script gameData object
			gameData = data;

			// Close save file
			saveFile.Close ();

			Debug.Log ("Savegame loaded from file.");

			// Update levelStateArray variable with newly loaded data
			levelStateArray = gameData.levelStateArray;
			highestLevelComplete = gameData.highestLevelComplete;
		} 
		// If there is no save game, initiate save data
		else {
			Debug.Log ("No Savegame file found.");

			// Loop for each level number
			for (int i = 0; i < numLevels; i++) {

				// Temporarily hold the level state data for ith level
				SimpleLevelState temp = new SimpleLevelState ();

				// Save level number to level state object
				temp.levelNumber = i+1;

				// Change level 1 to unlocked
				if (i == 1-1) {
					temp.levelUnlocked = true;
				}

				// Commit temp to ith levelStateArray position
				levelStateArray [i] = temp;
			}
		}
		// Update level buttons
		updateLevelButtons ();
	}

	// Initializes storage of level selection buttons in script
	static void initializeLevelButtons(){

		// Store reference to panel holding level selection buttons
		levelButtonPanel = GameObject.Find("Canvas").transform.Find("LevelSelect").Find ("LevelButtonPanel");

		// Save references to each levelbutton in buttonArray
		for (int i = 0; i < numLevels; i++) {

			// Save reference to the current button to the array
			levelButtonArray [i] = levelButtonPanel.Find ("Level" + (i + 1) + "Button").GetComponent<Selectable> ();

			// Change highlightedColor for each button
			ColorBlock temp = levelButtonArray [i].colors;
			temp.highlightedColor = mouseOverColor;
			levelButtonArray [i].colors = temp;
		}
	}

	// Updates level selection buttons based on current levelStateArray
	static void updateLevelButtons(){

		Debug.Log ("Level buttons have been updated.");

		// Call updateButton for every level button
		for (int i = 0; i < numLevels; i++){
			updateButton (levelButtonArray [i], levelStateArray [i].levelUnlocked, levelStateArray [i].levelCompleted);
		}
	}

	// Sets interactable based on level unlock, sets button colors based on level completion
	static void updateButton(Selectable levelButton, bool levelUnlocked, bool levelCompleted){

		// Change level button interactable based on unlock
		levelButton.interactable = levelUnlocked;

		// If this level is completed
		if (levelCompleted) {
			ColorBlock temp = levelButton.colors;
			temp.normalColor = levelCompletedColor; // Change color to indicate completed level
			temp.highlightedColor = levelCompletedColor; // Change mouseover color to match
			levelButton.colors = temp; 
		} 
		// If this level is not completed
		else {
			ColorBlock temp = levelButton.colors;
			temp.normalColor = levelUnlockedColor; // Change color to indicate not completed level
			temp.highlightedColor = levelUnlockedColor; // Change mouseover color to match
			levelButton.colors = temp; 
		}
	}

	// Update appropriate levelStateArray values when a level is completed
	// Called from LevelControl.cs when an individual level is completed
	public static void completedLevel (int levelNum){

		// Change level to completed in level state array
		levelStateArray [levelNum - 1].levelCompleted = true;

		// If not the last level, change next level to unlocked in level state array
		if (levelNum < numLevels) {
			levelStateArray [levelNum].levelUnlocked = true;
		}

		// Update highest completed level
		highestLevelComplete = levelNum + 1;
	}

	// DEBUG function to print levelStateArray contents to console
	public static void printLevelStateArray(){
		
		for (int i = 0; i < numLevels; i++) {
			Debug.Log ("#" + levelStateArray [i].levelNumber + ", unl = " + levelStateArray [i].levelUnlocked + 
				", compl = " + levelStateArray [i].levelCompleted);
		}
	}
}

// Class that stores all game save data
[Serializable]
public class GameData{
	
	// Array that stores level progression data
	public SimpleLevelState[] levelStateArray;
	public int highestLevelComplete;
}

[Serializable]
// Class that stores a single level's progression data
public class SimpleLevelState{
	public int levelNumber; // Stores level number identifier
	public bool levelUnlocked; // Stores whether or not level is unlocked
	public bool levelCompleted; // Stores whether or not level is completed
	public int score; // Stores the score the level has earned

	// Constructor
	public SimpleLevelState(){
		levelUnlocked = false; // Level is not unlocked by default
		levelCompleted = false; // Level is not completed by default
		score = 0; // Level earned no score by default
	}
}