using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverToggleScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	private GameObject mouseOverObj;
	private bool disableWithNoChildren;

	public void Initialize(GameObject newMouseOverObj, bool disableWithNoChildren) {
		mouseOverObj = newMouseOverObj;
		mouseOverObj.SetActive(false);

		this.disableWithNoChildren = disableWithNoChildren;
	}

	public void OnPointerEnter(PointerEventData eventData) {
		if (disableWithNoChildren) {
			if (mouseOverObj.transform.childCount <= 0) {
				return;
			}
		}
		mouseOverObj.SetActive(true);
	}

	public void OnPointerExit(PointerEventData eventData) {
		mouseOverObj.SetActive(false);
	}
}