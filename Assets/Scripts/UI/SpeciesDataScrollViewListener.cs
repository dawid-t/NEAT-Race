using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpeciesDataScrollViewListener : MonoBehaviour, IPointerClickHandler
{
	public void OnPointerClick(PointerEventData eventData)
	{
		CarsSelector.Instance.ResetSelection();
		GenomeUI.Instance.ClearGenomePanel(true);
	}
}
