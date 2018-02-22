using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

	GameObject cameraFocus;

	Camera cam;

	// Stores a reference to the collider defining the bounds within which the main camera can be moved
	Collider camBoundCollider;

	float focusXOffset;

	// Pinch to zoom functionality based on https://unity3d.com/learn/tutorials/topics/mobile-touch/pinch-zoom
	public float zoomSpeed = 0.02f;        // The rate of change of the orthographic size of the main camera view
	public float minZoom = 2f;		// Minimum camera zoom (in orthographic size)
	public float maxZoom = 8f;		// Maximum camera zoom (in orthographic size)

	public float camLerpSpeed = 0.5f;   // Speed at which camera lerps toward focus when there is one

	// Use this for initialization
	void Start () {
		cameraFocus = null;

		cam = transform.GetComponent<Camera> ();

		// Store reference to the collider defining camera movement bounds
		camBoundCollider = GameObject.Find ("Camera Bounds").GetComponent<BoxCollider> ();
	}
	
	// Update is called once per frame
	void Update () {
		camFollowOffset ();

		// Scroll wheel zoom just for debugging QoL
		if (Input.GetAxis ("Mouse ScrollWheel") < 0) {
			cam.orthographicSize = Mathf.Min(cam.orthographicSize+1, maxZoom+5);
		}
		if (Input.GetAxis ("Mouse ScrollWheel") > 0) {
			cam.orthographicSize = Mathf.Max(cam.orthographicSize-1, minZoom);
		}
	}

	// Lerps the camera position toward its focus
	void camFollowOffset (){

		// If a camera focus exists
		if (cameraFocus != null) {

			// Calculate camera position with focus position and offset (modified by zoom in ClinicInput.cs)
			Vector3 targetPos = cameraFocus.transform.position;
			targetPos = new Vector3 ( targetPos.x + focusXOffset, targetPos.y, transform.position.z);

			//Lerp camera toward focus + offset position
			transform.position = Vector3.Lerp (transform.position, targetPos, camLerpSpeed);

			if (Vector3.Distance (transform.position, targetPos) < 0.001f) {
				cameraFocus = null;
			}
		}
	}

	// Sets cameraFocus to the specified gameObject parameter
	public void setCamFollow (Transform focus, float xOffset){
		cameraFocus = focus.gameObject;
		focusXOffset = xOffset;
	}

	public float mouseDrag(Vector3 dragStartWorld){

		// Current camera position in world space
		Vector3 camPosWorld = transform.position;

		// Current mouse position in world space
		Vector3 dragPosWorld = cam.ScreenToWorldPoint (Input.mousePosition);

		// Effective mouse drag move in world space
		Vector3 camMove = dragStartWorld - dragPosWorld;

		// Record the magnitude of mouse movement
		float mouseMoveMagn = Vector3.Magnitude (camMove);

		// Record new camera position based on the inverse of the drag movement on both axes
		Vector3 newCamPos = new Vector3(camPosWorld.x + camMove.x, camPosWorld.y + camMove.y,  camPosWorld.z);

		// Record new camera movement position on only the X axis
		Vector3 newCamPosX = new Vector3 (newCamPos.x, camPosWorld.y, camPosWorld.z);

		// If only the X axis portion of the potential move is within bounds
		if(camBoundsCheck(newCamPosX)){

			// Move the camera the appropriate amount on the X axis
			transform.position = newCamPosX;

			// Update camPosWorld for potential Y axis movement
			camPosWorld = transform.position;
		}

		// Record new camera movement position on only the Y axis
		Vector3 newCamPosY = new Vector3 (camPosWorld.x, newCamPos.y, camPosWorld.z);

		// If only the Y axis portion of the potential move is within bounds
		if (camBoundsCheck (newCamPosY)){
			
			// Move the camera the appropriate amount on the Y axis
			transform.position = newCamPosY;
		}
		return mouseMoveMagn;
	}

	// Return (bool) if the given position parameter is within camBoundCollider
	bool camBoundsCheck(Vector3 potentialPosition){

		// Return if potentialPosition is within the camera bounds collider
		return camBoundCollider.bounds.Contains (potentialPosition);
	}

	public void pinchZoom(){
		// Store both touches.
		Touch touchZero = Input.GetTouch(0);
		Touch touchOne = Input.GetTouch(1);

		// Find the position in the previous frame of each touch.
		Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
		Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

		// Find the magnitude of the vector (the distance) between the touches in each frame.
		float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
		float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

		// Find the difference in the distances between each frame.
		float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

		// ... change the orthographic size based on the change in distance between the touches.
		cam.orthographicSize += deltaMagnitudeDiff * zoomSpeed;

		// Make sure the orthographic size stays within zoom bounds
		cam.orthographicSize = Mathf.Max(cam.orthographicSize, minZoom);
		cam.orthographicSize = Mathf.Min(cam.orthographicSize, maxZoom);
	}
}
