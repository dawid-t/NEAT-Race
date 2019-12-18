using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeciesUI : MonoBehaviour
{
	private static SpeciesUI instance;

	[SerializeField]
	private TextMeshProUGUI speciesDataText;
	[SerializeField]
	private GameObject speciesDataScrollViewContent, speciesDataButtonPrefab;
	private List<Button> speciesDataButtonsList = new List<Button>();


	public static SpeciesUI Instance { get { return instance; } }


	private void Awake()
	{
		instance = this;
	}

	public void UpdateSpeciesData(List<Species> speciesList)
	{
		// Update the SpeciesDataButton or add a new one:
		for(int i = 0; i < speciesList.Count; i++)
		{
			bool isInList = false;
			string updatedText = speciesList[i].SpeciesNumber + ". Size: " + speciesList[i].BrainsList.Count +
				", Best fitness: " + speciesList[i].BestFitness;

			for(int j = 0; j < speciesDataButtonsList.Count; j++)
			{
				TextMeshProUGUI buttonText = speciesDataButtonsList[j].GetComponentInChildren<TextMeshProUGUI>();
				if(buttonText.text.StartsWith(speciesList[i].SpeciesNumber+"."))
				{
					buttonText.text = updatedText;
					isInList = true;
					break;
				}
			}

			if(!isInList)
			{
				AddSpeciesDataButtonToScrollView(updatedText, speciesList[i]);
			}
		}

		// Remove the SpeciesDataButton of removed Species:
		for(int i = 0; i < speciesDataButtonsList.Count; i++)
		{
			bool isInList = false;
			TextMeshProUGUI buttonText = speciesDataButtonsList[i].GetComponentInChildren<TextMeshProUGUI>();

			for(int j = 0; j < speciesList.Count; j++)
			{
				if(buttonText.text.StartsWith(speciesList[j].SpeciesNumber+"."))
				{
					isInList = true;
					break;
				}
			}

			if(!isInList)
			{
				speciesDataButtonsList[i].onClick.RemoveAllListeners();
				Destroy(speciesDataButtonsList[i].gameObject);
				speciesDataButtonsList.RemoveAt(i);
				i--;
			}
		}

		speciesDataText.text = "Species: "+speciesList.Count;
	}

	private void AddSpeciesDataButtonToScrollView(string text, Species species)
	{
		Button newSpeciesButton = Instantiate(speciesDataButtonPrefab, speciesDataScrollViewContent.transform).GetComponent<Button>();

		// Change the button color:
		ColorBlock newButtonColorBlock = newSpeciesButton.colors;
		newButtonColorBlock.normalColor = species.SpeciesColor;
		newSpeciesButton.colors = newButtonColorBlock;

		// Change the button text:
		TextMeshProUGUI buttonText = newSpeciesButton.GetComponentInChildren<TextMeshProUGUI>();
		buttonText.text = text;

		// Set up and add listeners to the button:
		newSpeciesButton.GetComponent<SpeciesDataButtonListener>().Species = species; // OnPointerEnter event.
		newSpeciesButton.onClick.AddListener(() => SpeciesDataButtonClicked(species)); // OnClick event.

		speciesDataButtonsList.Add(newSpeciesButton);
	}

	private void SpeciesDataButtonClicked(Species species)
	{
		CarsSelector.Instance.SelectSpecies(species, true);
		GenomeUI.Instance.ShowSpeciesBestGenome(species, true);
	}

	public void SpeciesDataScrollViewClicked()
	{
		CarsSelector.Instance.ResetSelection();
		GenomeUI.Instance.ClearGenomePanel(true);
	}
}
