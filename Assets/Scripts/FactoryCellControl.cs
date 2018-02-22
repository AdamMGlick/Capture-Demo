using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public enum Faction {Player, Neutral, Enemy};

// Controls all functions of the factory cell, including most drone move functionality
public class FactoryCellControl : MonoBehaviour{

	// Holds faction of this cell
	public Faction faction = Faction.Neutral;

	// Stats of this factory cell regarding its performance
	[Header("Factory Cell Stats")]
	[Range(10,50)]
	public int maxNumDrones = 10;
	[Range(25,200)]
	public int produceDroneNumTicks = 100; // in fixedUpdate ticks

	// Used to count fixedUpdates since last drone was produced
	int produceDroneCounter = 0;

	// Default range in which this cell can initiate drone moves
	public float droneRange = 5f;
	float sizeToRangeRatio = 0.35f; // Multiplied by maximum drones in a cell to get its max move range

	[Header("Real Time Attributes")]
	public int numDrones; // Stores current number of drones
	public int cellNum; // Stores unique integer identifier for this cell - set and used in EnemyCellControl.cs

	// Store references to control scripts
	MoveControl inputControl;
	LevelControl levelControl;
	EnemyControl enemyControl;

	// Stores the parent game object of the drone position game objects for this cell
	Transform droneList;

	// Stores individual drone position game objects for this cell
	public Transform[] droneArray;

	// Delay between individual drones launching in milliseconds
	public int droneLaunchDelay = 250;

	// Store cell highlight mesh renderers for blue and red highlights
	public MeshRenderer highlightBlue, highlightRed; // Could totally just change the color of a single component but this is fine
	public float highlightThickness = 0.6f;

	// Stores this cell's renderer component
	Renderer thisRenderer;

	// Booleans denoting whether or not a drone launch is in progress, whether or not the faction was recently changed
	bool islaunching, factionChanged;

	// Use this for initialization
	void Start () {

		// If a number of drones are not present (specified in editory) the cell should have max drones
		if (numDrones == 0) {
			numDrones = maxNumDrones;
		}

		// Initialize level script variables for easy access
		inputControl = GameObject.Find ("Level Control").GetComponent <MoveControl>();
		levelControl = GameObject.Find ("Level Control").GetComponent <LevelControl>();
		enemyControl = GameObject.Find ("Level Control").GetComponent <EnemyControl>();

		// Store reference to this cell's renderer
		thisRenderer = GetComponent<Renderer> ();

		// Update (initialize) faction info
		factionUpdate (faction);

		// Initialize cell highlight object for easy access
		highlightBlue = transform.Find ("Highlight Blue").gameObject.GetComponent <MeshRenderer> ();
		highlightRed = transform.Find ("Highlight Red").gameObject.GetComponent <MeshRenderer> ();

		// Use object scales to provide uniform highlight thickness among cells
		Vector3 h = highlightBlue.transform.localScale;
		h = new Vector3 ( (highlightThickness / transform.localScale.x) + 1f, h.y, (highlightThickness / transform.localScale.z)+ 1f);
		highlightBlue.transform.localScale = h;
		highlightRed.transform.localScale = h;

		// Drone move range is the defined ratio multiplied by the drone capacity
		droneRange = maxNumDrones * sizeToRangeRatio;

		// Initialize list of drones in cell
		droneList = transform.Find ("Drone List");

		// Mathematically generate drone position gameobjects at appropriate locations
		generateDronePositions (maxNumDrones);

		// Initialize array of drone position transforms
		droneArray = new Transform[droneList.childCount];

		// Fill drone Array by displaying the drone mesh to indicate a drone in each appropriate position
		for(int i = 0; i < droneArray.Length; i++){
			droneArray [i] = droneList.GetChild (i);
			if (i < numDrones) {
				droneArray [i].GetComponentInChildren<MeshRenderer> ().enabled = true;
			}
		}
	}

	// Unity always does this if it's here
	void Update(){

	}

	// Use FixedUpdate to produce drones based on number of ticks to produce a drone int
	void FixedUpdate () {

		// If the game is not paused
		if (!levelControl.checkPause ()) {
			produceDroneCounter++; // Add to count of fixedUpdates since last drone was produced

			// If we are not in the middle of launching drones
			if (!islaunching) {
				factionChanged = false; // it is safe to switch off faction change bool
			}

			// Nuetral factory cells produce drones slower than player/ enemy ones
			if (faction == Faction.Neutral) {
				// If the produceDroneCounter is on a production tick and the level has not ended
				if (produceDroneCounter >= produceDroneNumTicks * 3 && !levelControl.levelEnd) {
					produceDrone ();
					produceDroneCounter = 0; // Reset drone production counter
				}
			} else if (faction != Faction.Neutral) {
				// If the produceDroneCounter is on a production tick and the level has not ended
				if (produceDroneCounter >= produceDroneNumTicks && !levelControl.levelEnd) {
					produceDrone ();
					produceDroneCounter = 0; // Reset drone production counter

				}
			}

			// If the level is ended, unhighlight this cell
			if (levelControl.levelEnd) {
				highlightBlue.enabled = false;
				highlightRed.enabled = false;
			}
		}
	}

	/// <summary>
	/// Generates all drone docking positions for this factory cell
	/// </summary>
	void generateDronePositions ( int numPositions){

		// The angle between drone dock positions for this cell
		float thetaSegment = (2*Mathf.PI / numPositions);

		// Loop for each drone docking position
		for (int i = 0; i < numPositions; i++) {
			// Store X and Z coordinate of the current drone docking position
			float xCoord, zCoord;

			// calculate x, z coords of current drone on factory cell (0,0) is center of cell)
			xCoord = -0.5f * Mathf.Cos(thetaSegment*i);
			zCoord = 0.5f * Mathf.Sin(thetaSegment*i);

			// place new drone prefab at these coords under Drone List child
			GameObject newDronePosition = (GameObject)Instantiate(Resources.Load("DronePosition"));

			// Make this new drone position a child of the drone list object
			newDronePosition.transform.SetParent (droneList);

			// Set this new drone position object to the position calculated above
			newDronePosition.transform.localPosition = new Vector3 (xCoord, 0f, zCoord);
		}
	}

	// Causes the cell to display one more drone if possible
	public void produceDrone(){

		// If this cell does not already have maximum drones
		if (numDrones < maxNumDrones) {
			numDrones++; // Add to drone count

			// Turn on the appropriate drone position's drone mesh renderer
			droneArray [numDrones - 1].GetComponentInChildren<MeshRenderer>().enabled = true;
		}
	}

	// ------ AS ORIGIN CELL ------------

	/// <summary>
	/// Launches all of this factory cell's drones to the target transform
	/// </summary>
	/// <returns>The number of drones sent</returns>
	/// <param name="target">Target/destination cell.</param>
	public int launchDrones(Transform target){

		// Indicate that a drone launch is in progress
		islaunching = true;

		// Record the number of drones that will comprise this launch
		int sentNumDrones = numDrones;

		// If this cell has more than 0 drones
		if (numDrones > 0) {

			// Launch drones with delay between launches
			StartCoroutine(droneLaunchWait (target, sentNumDrones));
		}

		// Return the number of drones launched
		return sentNumDrones;
	}

	// Coroutine for launching drones with a delay in between each
	IEnumerator droneLaunchWait(Transform target, int sentNumDrones){

		// If the game is not paused
		if (!levelControl.checkPause ()) {
			
			// Iterate for the number of drones that the cell has at the start
			for (int i = sentNumDrones - 1; i >= 0; i--) {

				// if the faction was changed, the launch is interrupted
				if (factionChanged) {
					Debug.Log ("Drone Launch stopped by faction change");
					factionChanged = false; // switch off faction changed bool
					break;
				}

				// If we do not have 0 drones
				if (numDrones != 0) {
					
					// Launch the next drone (in the highest numbered position)
					launchNextDrone (target, numDrones - 1);

					// Remove the visual for the docked drone
					droneArray [numDrones - 1].GetComponentInChildren<MeshRenderer> ().enabled = false;

					// Tell the cell it has one less drone
					numDrones--;

					// Wait for the launch delay
					yield return new WaitForSeconds (droneLaunchDelay / 1000f);
				}
			}
		}

		// Indicate that the launch has concluded
		islaunching = false;
	}

	/// <summary>
	/// Launch individual drones from location of next available drone (going down the list)
	/// </summary>
	public void launchNextDrone(Transform target, int droneNum){

		// If the level is over, do nothing
		if (levelControl.levelEnd)
			return;

		// Store reference to drone dock positions array
		Transform droneSpawnPosition = droneArray [droneNum];

		// Create a new gameobject that will be the drone that travels to the target cell
		GameObject droneTravellingGO = (GameObject)Instantiate (Resources.Load ("DroneTravelling"));

		// Store a reference to the new travelling drone's transform
		Transform newDroneTravelling = droneTravellingGO.transform;

		// Store a reference to the droneControl script
		DroneControl newDroneTravellingControl = newDroneTravelling.GetComponent<DroneControl> ();

		// Move drone to spawn position
		newDroneTravelling.transform.position = droneSpawnPosition.position;

		// Set start position and dest cell in DroneControl script
		newDroneTravellingControl.startPosition = newDroneTravelling.transform.position;

		// Tell the new travelling drone what its target cell is
		newDroneTravellingControl.targetCell = target;

		// Set drone faction to that of this cell
		newDroneTravellingControl.faction = faction;

		// Set drone faction color
		newDroneTravellingControl.changeFactionColor(thisRenderer.material);
	}

	// Select starting cell
	void OnMouseDown(){

		// If the game is not paused
		if (!levelControl.checkPause ()) {
			// If the level is over, do nothing
			if (levelControl.levelEnd)
				return;

			// Player may only control a cell of Faction "Player"
			if (faction == Faction.Player) {

				//Debug.Log ("OnMouseDown "+ gameObject.name);


				// highlight cell, tell inputControl that we are in a turn, that this is the origin cell
				highlightBlue.enabled = true;
				inputControl.setMove (true);
				inputControl.originCell = transform;
				inputControl.originCellControl = this;
			}
		}
	}

	// ------ AS DESTINATION CELL ------------

	/// <summary>
	/// Attempts to remove a drone from the cell
	/// </summary>
	/// <returns><c>true</c>, if drone was removed successfully, <c>false</c> if there was no drone to remove.</returns>
	public void droneDestroyed(Faction droneFaction){

		// If this cell has any drones
		if (numDrones > 0) {
			numDrones--; // Remove one drone from the drone count

			// Stop displaying one of the docked drones
			droneArray [numDrones].GetComponentInChildren<MeshRenderer>().enabled = false;
		}
		// If there are no more drones
		else if (numDrones == 0) {

			// Update to new faction (that of the drone that did the attack)
			factionUpdate (droneFaction);

		}
	}

	// Updates faction, updates faction counts in LevelControlScript
	void factionUpdate(Faction newFaction){

		// Reset drone production
		produceDroneCounter = 0;

		// This cell is converting to player
		if (newFaction == Faction.Player) {

			// Change visual to look like player
			thisRenderer.material = (Material)Instantiate(Resources.Load("Materials/PlayerMaterial"));

			// Add to player cell count
			levelControl.addPlayerCell();

			// If this cell was an enemy cell before
			if (faction == Faction.Enemy) {
				// Subtract from enemy cell count
				levelControl.removeEnemyCell();
			}
			// Indicate that the faction has been changed
			factionChanged = true;
		} 
		// This cell is converting to enemy
		else if (newFaction == Faction.Enemy) {

			// Change visual to look like enemy
			thisRenderer.material = (Material)Instantiate(Resources.Load("Materials/EnemyMaterial"));

			// Add to enemy cell count
			levelControl.addEnemyCell();

			// If the enemy captured a player cell
			if (faction == Faction.Player) {
				// Subtract from player cell count
				levelControl.removePlayerCell ();
			}
			// Indicate that the faction has been changed
			factionChanged = true;
		}
			
		// Complete faction update by updating faction variable
		faction = newFaction;

		// If this cell number is in the cellInfoList of EnemyControl.cs
		if (enemyControl.cellInfoList.Count > cellNum) {
			// Update faction change in enemyControlScript (AI game information)
			enemyControl.cellInfoList [cellNum].cellFaction = newFaction;
		}
	}

	// ---------- Mouse Controls ----

	// Mouse over destination cell
	void OnMouseEnter(){
		
		// If the game is not paused
		if (!levelControl.checkPause ()) {
			
			// If the level is over, do nothing
			if (levelControl.levelEnd)
				return;
			
			// If we're currently in a move and this cell is not the origin cell, highlight this cell
			if (inputControl.checkMove () == true && inputControl.originCell != transform) {

				// Store this cell as potential destCell
				inputControl.destCell = transform;
				inputControl.destCellControl = this;

				// If the origin cell and this cell are in the origin cell's drone range
				if (isInRangeOfOriginCell (false)) {

					// Highlight the cell blue to indicate a valid move
					highlightBlue.enabled = true;
				} else {
					// Otherwise, highlight red to indicate out of range
					highlightRed.enabled = true;
				}
			} 
		}

	}

	// Mouse exit destination cell (without selecting)
	void OnMouseExit(){
		
		// If the game is not paused
		if (!levelControl.checkPause ()) {
			
			// If we're currently in a move and this cell is not the origin cell
			if (inputControl.checkMove () == true && inputControl.originCell != transform) {

				// Blank out the move information in InputControl.cs
				inputControl.destCell = null;

				// Unlighlight this cell
				highlightBlue.enabled = false;
				highlightRed.enabled = false;
			}
		}
	}
		
	// Release mouse on destination cell (this function is triggered for the origin cell only)
	void OnMouseUp(){
		// If the game is not paused
		if (!levelControl.checkPause ()) {
			
			// If the level is over, do nothing
			if (levelControl.levelEnd)
				return;
		

			// If this is a valid move - destCell is not empty, checkMove returns true, origin and dest cells are different, 
			// origin and dest cells are within the origin cell's drone range
			if (inputControl.destCell != null && inputControl.checkMove () == true && inputControl.originCell != inputControl.destCell &&
			   isInRangeOfOriginCell (true)) {

				// perform move through InputControl.cs
				inputControl.performMove ();
			}

			// Terminate the move in InputControl.cs
			inputControl.endMove ();
		}
	}

	// Returns true if the attempted move is within the origin cell's range, false otherwise
	bool isInRangeOfOriginCell(bool moveAttempted){

		// Save positions of origin and destination cells
		Vector3 originPos = inputControl.originCell.position;
		Vector3 destPos = inputControl.destCell.position;

		// Save origin cell's range for easier reference
		float origRange = inputControl.originCellControl.droneRange;

		// set inRange bool to true when the distance between the cells is less than the origin cell's range
		bool inRange = Vector3.Distance (originPos, destPos) <= origRange;

		// If a move is attempted for an out of range move, display console message
		if (!inRange && moveAttempted) {
			Debug.Log ("Move out of range: range = " + origRange + " distance = " + Vector3.Distance (originPos, destPos));
		}

		// Return whether or not the move is in the origin cell's range
		return inRange;
	}
}
