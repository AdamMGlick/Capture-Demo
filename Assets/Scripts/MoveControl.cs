using UnityEngine;
using System.Collections;

// Coordinates the two factory cells involved in a move
public class MoveControl : MonoBehaviour {

	// Stores whether or not the player is currently making a move
	bool isMove;

	// Stores transforms for destination and origin cell of a move
	public Transform destCell, originCell;

	// Origin control script is set by origin cell upon starting a potential move, dest set in performMove() below
	public FactoryCellControl originCellControl, destCellControl;

	// Performs all actions that have to do with a move
	public void performMove(){

		// Save factoryCellControl script for dest cell
		destCellControl = destCell.GetComponent<FactoryCellControl> ();
		originCellControl = originCell.GetComponent<FactoryCellControl> ();

		//Unhighlight destination cell since the move is set
		//destCellControl.highlightBlue.enabled = false;
		//originCellControl.highlightBlue.enabled = false;

		// Launch drones, save number of drones launched
		int numDronesSent = originCellControl.launchDrones (destCell);
		endMove ();

	}

	// Returns level to non-move state
	public void endMove(){

		//Unhighlight all

		originCellControl.highlightBlue.enabled = false;
		originCellControl.highlightRed.enabled = false;

		if (destCellControl != null) {
			destCellControl.highlightBlue.enabled = false;
			destCellControl.highlightRed.enabled = false;
		}

		// Blank out cell values
		destCell = null;
		originCell = null;

		// We are no longer in a move
		isMove = false;
	}
		
	// Checks if a move is in progress
	public bool checkMove(){
		return isMove;
	}

	// Changes value denoting wether or not a move is in progress
	public void setMove(bool setMoveTo){
		isMove = setMoveTo;
	}
}
