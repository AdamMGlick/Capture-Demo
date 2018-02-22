using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

// Holds functions for scene navigation via UI buttons
public class ButtonOnClick : MonoBehaviour {

	// Stores the current scene index
	public int sceneIndex;

	void Start(){
		// Save the scen index of the current scene
		sceneIndex = SceneManager.GetActiveScene ().buildIndex;
	}

	// BUTTON - Loads the scene with the scene index given
	public void LoadScene (int level) {
		SceneManager.LoadScene (level);
	}

	// BUTTON - Loads a scene adjacent to the current one based on an index offset value
	public void LoadSceneAdj (int offset) {
		SceneManager.LoadScene (sceneIndex+offset);
	}
}