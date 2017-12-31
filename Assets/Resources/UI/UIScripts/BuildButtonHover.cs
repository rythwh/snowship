using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	private GameObject requiredResourcesPanel;

	void Awake() {
		requiredResourcesPanel = transform.Find("RequiredResources-Panel").gameObject;
		requiredResourcesPanel.SetActive(false);
	}

	public void OnPointerEnter(PointerEventData eventData) {
		requiredResourcesPanel.SetActive(true);
	}

	public void OnPointerExit(PointerEventData eventData) {
		requiredResourcesPanel.SetActive(false);
	}
}
