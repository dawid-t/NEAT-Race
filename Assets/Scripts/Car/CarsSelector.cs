using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Globalization;

public class CarsSelector : MonoBehaviour
{
	private static CarsSelector instance;

	private bool selectionIsAvailable = true, selectedCarsIsLocked = false;
	private Transform[] selectedCars = new Transform[1];
	private CarBrain[] selectedCarsBrains = new CarBrain[1];
	private TextMeshPro[] carsFitnessText = new TextMeshPro[1];
	private NumberFormatInfo numberFormatInfo;


	public static CarsSelector Instance { get { return instance; } }

	public bool SelectionIsAvailable { get { return selectionIsAvailable; } set { selectionIsAvailable = value; } }


	private void Awake()
	{
		instance = this;
		numberFormatInfo = new NumberFormatInfo { NumberGroupSeparator = " "};
	}

	private void Update()
	{
		SelectCar();
		UpdateCarsFitnessText();
	}

	/// <summary>
	/// Select the car on which mouse hovered over.
	/// </summary>
	private void SelectCar()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		int layerMask = LayerMask.GetMask("Car");

		if(Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)) // Select car.
		{
			if(selectionIsAvailable && CanSelectNewCar(hit.transform))
			{
				UnselectLastCars();
				ResetSelectedCarsDataArrays();

				GetCarComponents(0, hit.transform);
				ShowCarFitness(0);
				EnableOutlineMesh(0, true);
			}
		}
		else if(Input.GetMouseButtonDown(0) && Physics.Raycast(ray) && !MenuController.IsPointerOverUI()) // Unselect car.
		{
			ResetSelection();
		}
	}

	/// <summary>
	/// Select all cars in a given Species. Set 'lockCars' to true if want to lock cars and bypass the former lock.
	/// </summary>
	public void SelectSpecies(Species species, bool lockCars)
	{
		List<CarBrain> carBrainsList = species.BrainsList;

		// Lock the "mouse hover" selection if lockCars is true. Otherwise don't override the selectedCarsIsLocked value:
		if(lockCars)
		{
			selectedCarsIsLocked = lockCars;
		}

		// Return when:
		if(!selectionIsAvailable || // Selection is not available,
			selectedCarsIsLocked && !lockCars || // Selected cars are locked and you don't want to lock these cars,
			carBrainsList[0] == selectedCarsBrains[0] && carBrainsList.Count == selectedCarsBrains.Length) // Same Species is selected.
		{
			return;
		}

		// Select the new Species:
		UnselectLastCars();
		selectedCars = new Transform[carBrainsList.Count];
		selectedCarsBrains = new CarBrain[carBrainsList.Count];
		carsFitnessText = new TextMeshPro[carBrainsList.Count];

		for(int i = 0; i < carBrainsList.Count; i++)
		{
			GetCarComponents(i, carBrainsList[i].transform);
			ShowCarFitness(i);
			EnableOutlineMesh(i, true);
		}
	}

	private bool CanSelectNewCar(Transform car)
	{
		if(Input.GetMouseButtonDown(0))
		{
			selectedCarsIsLocked = true;
		}

		return ((car != selectedCars[0] || selectedCars.Length > 1) && // Can't be the same car, or is selected the whole Species.
			(!selectedCarsIsLocked || Input.GetMouseButtonDown(0)));
	}

	private void UnselectLastCars()
	{
		for(int i = 0; i < selectedCars.Length; i++)
		{
			if(selectedCars[i] != null)
			{
				EnableCarFitness(i, false);
				EnableOutlineMesh(i, false);
			}
		}
	}

	private void GetCarComponents(int carIndex, Transform car)
	{
		selectedCars[carIndex] = car;
		selectedCarsBrains[carIndex] = car.GetComponent<CarBrain>();
		carsFitnessText[carIndex] = car.GetComponentInChildren<TextMeshPro>(true);
	}

	private void ShowCarFitness(int carIndex)
	{
		EnableCarFitness(carIndex, true);
		Color textColor = selectedCarsBrains[carIndex].CrashedCarMaterial.GetColor("_BaseColor");
		carsFitnessText[carIndex].color = new Color(textColor.r, textColor.g, textColor.b, 1);
	}

	private void EnableCarFitness(int carIndex, bool enable)
	{
		carsFitnessText[carIndex].gameObject.SetActive(enable);
		if(enable)
		{
			carsFitnessText[carIndex].GetComponent<Animator>().Play("FitnessUpdate", 0, 0);
		}
	}

	private void EnableOutlineMesh(int carIndex, bool enable)
	{
		Transform vehicleMeshes = selectedCars[carIndex].GetChild(0);
		if(enable) // Enable one version of the outline mesh.
		{
			if(!Settings.IsPotatoGraphics)
			{
				vehicleMeshes.GetChild(2).gameObject.SetActive(enable);
			}
			else
			{
				vehicleMeshes.GetChild(3).gameObject.SetActive(enable);
			}
		}
		else // Disable all versions of the outline mesh.
		{
			vehicleMeshes.GetChild(2).gameObject.SetActive(enable);
			vehicleMeshes.GetChild(3).gameObject.SetActive(enable);
		}
	}

	private void UpdateCarsFitnessText()
	{
		for(int i = 0; i < selectedCars.Length; i++)
		{
			if(selectedCarsBrains[i] != null)
			{
				carsFitnessText[i].text = "Fitness: " + selectedCarsBrains[i].Fitness.ToString("N0", numberFormatInfo);
				carsFitnessText[i].rectTransform.rotation = Quaternion.Euler(90, -selectedCars[i].rotation.y, 0);
			}
		}
	}

	private void ResetSelectedCarsDataArrays()
	{
		selectedCars = new Transform[1];
		selectedCarsBrains = new CarBrain[1];
		carsFitnessText = new TextMeshPro[1];
	}

	public void ReselectCars() // It is used for graphics changes.
	{
		for(int i = 0; i < selectedCars.Length; i++)
		{
			if(selectedCars[i] != null)
			{
				EnableOutlineMesh(i, false);
				EnableOutlineMesh(i, true);
			}
		}
	}

	public void ResetSelection()
	{
		UnselectLastCars();
		selectedCarsIsLocked = false;
		ResetSelectedCarsDataArrays();
	}
}
