using UnityEngine;
using System.Collections;

// Controls drone behavior once launched
public class DroneControl : MonoBehaviour {

	// Stores the faction that this drone belongs to
	public Faction faction;

	// Stores this drones target that it is moving to
	public Transform targetCell;

	// Stores a reference to the target cell's control script
	FactoryCellControl targetCellControl;

	// Stores a reference to this level's control script
	LevelControl levelControl;

	// Stores the original docked position of this drone
	public Vector3 startPosition;

	// This drone's travel speed
	public float droneTravelSpeed = 1f;

	//public int droneAge = 0;

	// Use this for initialization
	void Start () {

		// Initialize target cell control script and level control script
		targetCellControl = targetCell.GetComponent<FactoryCellControl> ();
		levelControl = GameObject.Find ("Level Control").GetComponent <LevelControl>();

		// Add to appropriate drone count
		if (faction == Faction.Enemy) {
			levelControl.addEnemyDrone ();
		} else if (faction == Faction.Player) {
			levelControl.addPlayerDrone ();
		}
	}

	void FixedUpdate () {
		// If the game is not paused
		if (!levelControl.checkPause ()) {
			
			// If the drone is within the boundaries of the target cell
			if (Vector3.Distance (transform.position, targetCell.position) < targetCell.localScale.x / 2) {
				interactWithCell (); // Interact with the cell appropriately
			} else {
				travelToCell (); // Keep moving towards the target cell
			}
			//droneAge++;
		}
	}

	// Change the display color of the drone based on its faction
	public void changeFactionColor(Material newFactionColor){
		transform.Find ("DroneFactionColor").GetComponent<Renderer> ().material = newFactionColor;
	}

	// Moves a drone toward its target cell
	void travelToCell(){

		// Drone looks at destination and goes straight there 
		transform.LookAt (targetCell.position); // maybe make this happen less often in favor of looking not directly at dest
		transform.Translate (0f, 0f, droneTravelSpeed/1000);
	}

	// Takes care of the drones interaction with the target cell
	void interactWithCell(){
		// if cell is friendly, add self to drones of cell
		if (targetCellControl.faction == faction){
			targetCellControl.produceDrone ();
		}
		// if cell is hostile,  "attack" cell (subtracting a hostile drone from the cell)
		else if(targetCellControl.faction != faction){

			// Tell target cell that one of its drones was destroyed
			targetCellControl.droneDestroyed(faction);
		}

		// Subtract from appropriate drone count
		if (faction == Faction.Enemy) {
			levelControl.removeEnemyDrone ();
		} else if (faction == Faction.Player) {
			levelControl.removePlayerDrone ();
		}

		// Destroy drone
			Destroy(gameObject);
	}
}
