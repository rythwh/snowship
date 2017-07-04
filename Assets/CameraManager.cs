using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

	public GameObject cameraGO;
	public Camera cameraComponent;

	private TileManager tileM;

	void Awake() {
		cameraGO = GameObject.Find("Camera");
		cameraComponent = cameraGO.GetComponent<Camera>();

		GameObject GM = GameObject.Find("GM");
		tileM = GM.GetComponent<TileManager>();
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
			if (tileM.debugMode) {
				cameraComponent.orthographicSize = Mathf.Clamp(cameraComponent.orthographicSize,1,500);
			} else {
				cameraComponent.orthographicSize = Mathf.Clamp(cameraComponent.orthographicSize,1,20);
			}
		}
	}
}