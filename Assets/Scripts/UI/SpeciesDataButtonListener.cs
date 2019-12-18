using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SpeciesDataButtonListener : MonoBehaviour, IPointerEnterHandler
{
	private Species species;


	public Species Species { get { return species; } set { species = value; } }


	public void OnPointerEnter(PointerEventData eventData)
	{
		CarsSelector.Instance.SelectSpecies(species, false);
		GenomeUI.Instance.ShowSpeciesBestGenome(species, false);
	}
}
