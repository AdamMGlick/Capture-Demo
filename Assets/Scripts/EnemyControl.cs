using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Controls the AI enemy faction's gameplay and strategy
public class EnemyControl : MonoBehaviour {

	// Affects chance of AI moving against player vs nuetral cells
	//public float AI_Aggressiveness = 0.3f;

	// Determines the steepness of the move probability based on cell fullness
	[Range(1,50)]
	public float moveSteepness = 15.0f;

	// Number of ticks before AI considers making a move
	public int AIThinkNumTicks = 20;
	int thinkTicks = 0; // Will count number of ticks for AI to think

	// AI score weights
	public float distanceWeight = 3.0f;
	public float droneCapacityWeight = 1.0f;
	public float droneProductionWeight = 1.0f;
	public float enemyFactionWeight = 1.5f;
	public float sizeDifferenceWeight = 1.0f;

	// Cycles through enemy cells from which the enemy makes a move
	int cellNumCycle;

	// List holding the AI knowledge of the current game's factory cells
	public List<FactoryCellInfo> cellInfoList = new List<FactoryCellInfo>();

	// Array holding the factory cells in the game
	public GameObject[] cellArray;

	// Array holding the distances between each pair of factory cells
	float[][] distanceArray;

	// Stores a reference to this level's control script
	LevelControl levelControl;

	// Use this for initialization
	void Start () {
		
		// Initialize level control script reference
		levelControl = GetComponent<LevelControl> ();

		// Populate factory cell array
		cellArray = GameObject.FindGameObjectsWithTag ("FactoryCell");

		// Initialize first dimension of distance array
		distanceArray = new float[cellArray.Length][];

		// loop for each cell in this level, initializing cell information
		for (int i = 0; i < cellArray.Length; i++) {

			// Initialize second dimension of distance array ** will potentially truncate subsequent arrays to avoid duplicate distances
			distanceArray [i] = new float[cellArray.Length];

			// Loop for each cell again to calculate the distance between each pair of cells
			for (int j = 0; j < cellArray.Length; j++) {

				// Store calculated distance between cell i and cell j
				distanceArray [i][j] = Vector3.Distance (cellArray [i].transform.position, cellArray [j].transform.position);
			}

			// Initialize and populate static information for cell i, add to cellInfoList
			FactoryCellInfo tempCellInfo = new FactoryCellInfo(i, cellArray, distanceArray);
			cellInfoList.Add (tempCellInfo);

			// tell FactoryCellControl scripts their cell numbers
			tempCellInfo.cellControlScript.cellNum = i;
		}
	}
		
	void FixedUpdate () {
		
		// If the game is not paused
		if (!levelControl.checkPause ()) {
			// advance think ticks counter
			thinkTicks++;

			// Check if this tick is a think tick
			if (thinkTicks == AIThinkNumTicks && !levelControl.levelEnd) {

				// AI thinks about making a move from cell cellNum
				think ();

				thinkTicks = 0; // reset think ticks counter
			}
		}
	}

	// Return whether this cell should dump its drones
	bool shouldDumpDrones( int cellNum){

		// Get cell drone information
		float numDrones = (float)cellInfoList [cellNum].cellControlScript.numDrones;
		float maxDrones = (float)cellInfoList [cellNum].cellControlScript.maxNumDrones;

		// Determine proportion of time drones should be launched
		float propDrones = Mathf.Pow((numDrones / maxDrones), moveSteepness);

		// Returns true based on the proportion of time drones should be launched
		return Random.value < propDrones;
	}
		
	// Main function controlling AI move decision making
	void think(){

		// Think about moves in two ways:
		//  - Capture Cells - "Does this not owned cell have a high enough value to try to capture it?"
		//			if so, make a move to it from the cell with the highest outgoing score (based on num drones and dist)
		//  - Dump drones - "Does this owned cell have enough drones to make a move?"
		// 			if so, make a move to the highest value cell from this one
		// Capture cells is the goal of the game and takes precedence. Dump drones operates after capture cells has 
		// used whatever drones it needed

		//captureCellsThink ();

		dumpDronesThink ();

	}

	/*
	// Controls AI moves based off of capturing desirable enemy or nuetral cells
	void captureCellsThink (){
		
		// Look at all cell numbers in current section
		for (int i = 0; i < cellInfoList.Count; i++){

			// If the current cellNum belongs to an enemy cell
			if (cellInfoList [i].cellControlScript.faction == Faction.Enemy) {

				// Reinforce cell with drones from other cells if necessary

			} 
			// if the current cellNum belongs to a player or neutral cell
			else if(cellInfoList [i].cellControlScript.faction == Faction.Player){

				// 1 If real time score is above threshold to make move
				// 1.5 distinguish between capture move and non-capture move
				// 2 Record offensive score for enemy cells
				// 3 Make move from offensive cell with best score (above some minimum)
				// repeat 2 and 3 until no offensive scores are above the minimum
			}
		}
	}
	*/

	// Controls AI moves based on using drones before generated drones are lost at the cell's drone capacity
	void dumpDronesThink(){

		// Look at all cell numbers in current section
		for (int i = 0; i < cellInfoList.Count; i++){

			// If the current cellNum belongs to an enemy cell and shouldMakeMove() == true
			if (cellInfoList [i].cellControlScript.faction == Faction.Enemy && shouldDumpDrones(i)) {

				// perform a move from this cell
				AI_PerformMove(cellInfoList [i].factoryCellGameObject, AI_ChooseDestCell(i));
			} 
		}
	}

	// Determine the highest scoring eligible destination cell for a move from cell cellNum
	public GameObject AI_ChooseDestCell(int cellNum){

		// Initialize current best move score save
		float currentBestCellMoveScore = 0;

		// Initialize reference to current best destination cell
		GameObject currentBestDestCell = null;

		// Loop for every cell in the game
		foreach (FactoryCellInfo fCellInfo in cellInfoList) {

			// If this cell is a valid move
			if (fCellInfo.cellFaction != Faction.Enemy) {

				// Check its score against the current best score
				if (currentBestCellMoveScore <= fCellInfo.getStaticScore(cellNum, cellInfoList[cellNum].maxNumDrones) ) {

					// Store current winning move score and corresponding destination cell
					currentBestCellMoveScore = fCellInfo.getStaticScore(cellNum, cellInfoList[cellNum].maxNumDrones);
					currentBestDestCell = fCellInfo.factoryCellGameObject;
				}
			}
		}
		// Return best scoring destination cell
		return currentBestDestCell;
	}

	// Perform a move between origin and dest cell
	public void AI_PerformMove(GameObject AI_OriginCellGO, GameObject AI_DestCellGO){

		// Save origin and destination cell transform references
		Transform AI_OriginCell = AI_OriginCellGO.transform;
		Transform AI_DestCell = AI_DestCellGO.transform;

		// Save factoryCellControl script for origin cell
		FactoryCellControl originCellControl = AI_OriginCell.GetComponent<FactoryCellControl> ();

		// Launch drones, save number of drones launched
		int numDronesSent = originCellControl.launchDrones (AI_DestCell);
	}
}

/// <summary>
/// Stores information about a factory cell for the purposes of AI move evaluation
/// </summary>
[System.Serializable]
public class FactoryCellInfo{
	EnemyControl enemyControl;

	// Static cell information
	public GameObject factoryCellGameObject;
	public FactoryCellControl cellControlScript;
	public Faction cellFaction;
	public int maxNumDrones;
	int produceDroneNumTicks;
	float playerFactionBonus;

	// Score combining the static cell information. designed so a cellValue of 1 should be the maximum
	public float cellValue;

	// an array to hold the distance from this cell to all others
	public float[] distanceFromThisCellArray;

	// Dynamic/ realtime cell information
	int numDrones;
	int numIncomingFriendly;
	int numIncomingHostile;

	float realtimeValue;

	// Constructor which initializes static cell information and calculates static score
	public FactoryCellInfo(int cellNum, GameObject[] cellArray, float[][] distanceArray){
		enemyControl = GameObject.Find ("Level Control").GetComponent <EnemyControl>();

		// Store static cell info
		factoryCellGameObject = cellArray[cellNum];
		cellControlScript = factoryCellGameObject.transform.GetComponent<FactoryCellControl> ();
		cellFaction = cellControlScript.faction;
		maxNumDrones = cellControlScript.maxNumDrones;
		produceDroneNumTicks = cellControlScript.produceDroneNumTicks;

		// Set playerFaction bonus weight when cell faction is player
		if (cellFaction == Faction.Player) {
			playerFactionBonus = enemyControl.enemyFactionWeight;
		} 
		// Set isPlayerFaction to give no bonus weight when cell not player
		else {
			playerFactionBonus = 1;
		}

		// Calculate static cell value
		cellValue = playerFactionBonus * (((maxNumDrones * enemyControl.droneCapacityWeight/10f) + 
			(enemyControl.droneProductionWeight*200f/ produceDroneNumTicks)))/2;

		// Store distance array of distances between this cell and all others
		distanceFromThisCellArray = distanceArray [cellNum];


	}

	public float getStaticScore(int otherCellNum, int originCellNumDrones){


		// Static cell score comprised of cell value, inverse distance between origin and potential dest, 
		//  ratio of origin to dest cell size
		return cellValue / (distanceFromThisCellArray [otherCellNum] * enemyControl.distanceWeight) 
			* (enemyControl.sizeDifferenceWeight*originCellNumDrones/maxNumDrones);
	}

	public void updateFaction(Faction newFaction){
		cellFaction = newFaction;

		// Update playerFaction bonus weight when cell faction is player
		if (newFaction == Faction.Player) {
			playerFactionBonus = enemyControl.enemyFactionWeight;
		}
		// Update isPlayerFaction to give no bonus weight when cell not player
		else {
			playerFactionBonus = 1;
		}
	}
}