using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

	private TileManager tileM;
	private DebugManager debugM;

	private void GetScriptReferences() {
		tileM = GetComponent<TileManager>();
		debugM = GetComponent<DebugManager>();
	}

	public GameObject cameraGO;
	public Camera cameraComponent;

	private int minOrthoSize = 1;
	private int maxOrthoSize = 20;

	public int GetMinOrthoSize() {
		return minOrthoSize;
	}

	public int GetMaxOrthoSize() {
		return maxOrthoSize;
	}

	void Awake() {
		GetScriptReferences();

		cameraGO = GameObject.Find("Camera");
		cameraComponent = cameraGO.GetComponent<Camera>();
	}

	/* Called by GM.TileManager after it gets the mapSize */
	public void SetCameraPosition(Vector2 position) {
		cameraGO.transform.position = position;
	}

	public void SetCameraZoom(float newOrthoSize) {
		cameraComponent.orthographicSize = newOrthoSize;
	}

	void Update() {

		if (tileM.generated) {
			cameraGO.transform.Translate(new Vector2(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical")) * cameraComponent.orthographicSize * Time.deltaTime);
			cameraGO.transform.position = new Vector2(Mathf.Clamp(cameraGO.transform.position.x,0,tileM.map.mapData.mapSize),Mathf.Clamp(cameraGO.transform.position.y,0,tileM.map.mapData.mapSize));
		}

		if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
			cameraComponent.orthographicSize -= Mathf.Clamp(Input.GetAxis("Mouse ScrollWheel") + (Input.GetAxis("KeyboardZoom") / 10f),-1f,1f) * cameraComponent.orthographicSize * Time.deltaTime * 100;
			if (debugM.debugMode) {
				cameraComponent.orthographicSize = Mathf.Clamp(cameraComponent.orthographicSize,minOrthoSize,maxOrthoSize*25);
			} else {
				cameraComponent.orthographicSize = Mathf.Clamp(cameraComponent.orthographicSize,minOrthoSize,maxOrthoSize);
			}
		}
	}
}