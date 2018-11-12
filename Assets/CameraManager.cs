using System;
using UnityEngine;

public class CameraManager : BaseManager {

	public GameObject cameraGO;
	public Camera cameraComponent;

	public override void Awake() {
		cameraGO = GameObject.Find("Camera");
		cameraComponent = cameraGO.GetComponent<Camera>();
	}

	private readonly int minOrthoSize = 1;
	private readonly int maxOrthoSize = 20;

	public int GetMinOrthoSize() {
		return minOrthoSize;
	}

	public int GetMaxOrthoSize() {
		return maxOrthoSize;
	}

	public Vector2 GetCameraPosition() {
		return cameraGO.transform.position;
	}

	public void SetCameraPosition(Vector2 position) {
		cameraGO.transform.position = position;
	}

	public float GetCameraZoom() {
		return cameraComponent.orthographicSize;
	}

	public void SetCameraZoom(float newOrthoSize) {
		cameraComponent.orthographicSize = newOrthoSize;
	}

	public override void Update() {
		if (GameManager.tileM.mapState == TileManager.MapState.Generated && !GameManager.uiM.pauseMenu.activeSelf && !GameManager.uiM.playerTyping) {
			cameraGO.transform.Translate(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * cameraComponent.orthographicSize * Time.deltaTime);
			if (Input.GetMouseButton(2)) {
				cameraGO.transform.Translate(new Vector2(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y")) * cameraComponent.orthographicSize * Time.deltaTime);
			}
			cameraGO.transform.position = new Vector2(Mathf.Clamp(cameraGO.transform.position.x, 0, GameManager.colonyM.colony.map.mapData.mapSize), Mathf.Clamp(cameraGO.transform.position.y, 0, GameManager.colonyM.colony.map.mapData.mapSize));

			if (!GameManager.uiM.IsPointerOverUI()) {
				cameraComponent.orthographicSize -= Mathf.Clamp(Input.GetAxis("Mouse ScrollWheel") + (Input.GetAxis("KeyboardZoom") / 10f), -1f, 1f) * cameraComponent.orthographicSize * Time.deltaTime * 100;
				if (GameManager.debugM.debugMode) {
					cameraComponent.orthographicSize = Mathf.Clamp(cameraComponent.orthographicSize, minOrthoSize, maxOrthoSize * 25);
				} else {
					cameraComponent.orthographicSize = Mathf.Clamp(cameraComponent.orthographicSize, minOrthoSize, maxOrthoSize);
				}
			}
		}
	}
}