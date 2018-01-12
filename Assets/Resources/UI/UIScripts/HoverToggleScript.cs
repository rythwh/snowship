using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverToggleScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	private GameObject mouseOverObj;

	public void Initialize(GameObject newMouseOverObj) {
		mouseOverObj = newMouseOverObj;
		mouseOverObj.SetActive(false);
	}

	public void OnPointerEnter(PointerEventData eventData) {
		mouseOverObj.SetActive(true);
	}

	public void OnPointerExit(PointerEventData eventData) {
		mouseOverObj.SetActive(false);
	}
}
