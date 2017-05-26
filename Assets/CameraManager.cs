using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

	private GameObject cameraGO;
	public Camera cameraComponent;

	private GameObject gm;
	private TileManager tm;

	void Awake() {
		cameraGO = GameObject.Find("Camera");
		cameraComponent = cameraGO.GetComponent<Camera>();

		gm = GameObject.Find("GM");
		tm = gm.GetComponent<TileManager>();
	}

	/* Called by GM.TileManager after it gets the mapSize */
	public void SetCameraPosition(Vector2 position) {
		cameraGO.transform.position = position;
	}

	public void SetCameraZoom(float newOrthoSize) {
		cameraComponent.orthographicSize = newOrthoSize;
	}

	void Update() {
		cameraGO.transform.Translate(new Vector2(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical")) * cameraComponent.orthographicSize * Time.deltaTime);
		cameraGO.transform.position = new Vector2(Mathf.Clamp(cameraGO.transform.position.x,0,tm.mapSize),Mathf.Clamp(cameraGO.transform.position.y,0,tm.mapSize));

		cameraComponent.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * cameraComponent.orthographicSize * Time.deltaTime * 100;
		cameraComponent.orthographicSize = Mathf.Clamp(cameraComponent.orthographicSize,1,20); // 1,20
	}
}
