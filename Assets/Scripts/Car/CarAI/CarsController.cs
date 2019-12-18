using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarsController : MonoBehaviour
{
	private static bool raceIsStopped = true;
	private static int drivingCars = 0;
	private static CarsController instance;

	private int populationNumber = 50, generationLifeTime = 30;
	private int generationNumber = 0;
	private CarBrain[] population;
	[SerializeField]
	private GameObject carPrefab;
	[SerializeField]
	private Material carMaterial, crashedCarMaterial, potatoCarMaterial, crashedPotatoCarMaterial;
	[SerializeField]
	private Transform spawnPoint;
	private CarBrainEvolution carBrainEvolution;
	private Coroutine endGenerationCoroutine, startEvolutionCoroutine, startDissolveEffectCoroutine, endDissolveEffectCoroutine;


	public static bool RaceIsStopped { get { return raceIsStopped; } }
	public static CarsController Instance { get { return instance; } }

	public int GenerationNumber { get { return generationNumber; } }
	public int PopulationNumber { get { return populationNumber; } set { populationNumber = value; } }
	public int GenerationLifeTime { get { return generationLifeTime; } set { generationLifeTime = value; } }
	public Material CarMaterial { get { return carMaterial; } }
	public Material CrashedCarMaterial { get { return crashedCarMaterial; } }
	public Material PotatoCarMaterial { get { return potatoCarMaterial; } }
	public Material CrashedPotatoCarMaterial { get { return crashedPotatoCarMaterial; } }


	private void Awake()
	{
		instance = this;
		carBrainEvolution = new CarBrainEvolution(this);
	}

	/// <summary>
	/// Invoke when you want to start first race.
	/// </summary>
	public void StartFirstRace()
	{
		SpawnCars();
		StartRace();
	}

	private void SpawnCars()
	{
		// Create copies of materials because we don't want to change settings in the original materials:
		Material copyOfDefaultCarMaterial = new Material(carMaterial);
		Material copyOfCrashedCarMaterial = new Material(crashedCarMaterial);
		Material copyOfDefaultPotatoCarMaterial = new Material(potatoCarMaterial);
		Material copyOfCrashedPotatoCarMaterial = new Material(crashedPotatoCarMaterial);

		// Create cars and give them reference to materials:
		GameObject carsContainer = new GameObject("Cars");
		population = new CarBrain[populationNumber];
		GameObject[] cars = new GameObject[populationNumber];

		for(int i = 0; i < populationNumber; i++)
		{
			GameObject car = Instantiate(carPrefab);
			car.name = "Car-"+i;
			car.transform.parent = carsContainer.transform;
			population[i] = car.GetComponent<CarBrain>();
			cars[i] = car;

			MeshRenderer[] carMeshRenderers = car.GetComponentsInChildren<MeshRenderer>(true);
			carMeshRenderers[0].sharedMaterial = copyOfDefaultCarMaterial;
			carMeshRenderers[1].sharedMaterial = copyOfDefaultPotatoCarMaterial;

			population[i].CarMaterial = copyOfDefaultCarMaterial;
			population[i].CrashedCarMaterial = copyOfCrashedCarMaterial;
			population[i].PotatoCarMaterial = copyOfDefaultPotatoCarMaterial;
			population[i].CrashedPotatoCarMaterial = copyOfCrashedPotatoCarMaterial;
		}

		// Add cars array to object which will control the cars sound:
		CarsSound carsSound = GetComponent<CarsSound>();
		carsSound.SetCarsMovements(cars);
		if(!Settings.IsAudioMuted)
		{
			carsSound.PlaySound();
		}

		// Add cars array and change the cars graphics to low if 'Settings.IsPotatoGraphics' is true:
		Settings.SetAICars(cars);
		if(Settings.IsPotatoGraphics)
		{
			Settings.ChangeCarsGraphics();
		}
	}

	/// <summary>
	/// Invoke when you want to start next generation race.
	/// </summary>
	public void StartRace()
	{
		RestartCars();
		drivingCars = populationNumber;
		raceIsStopped = false;
		generationNumber++;
		endGenerationCoroutine = StartCoroutine(EndGeneration());
	}

	private void RestartCars()
	{
		for(int i = 0; i < population.Length; i++)
		{
			population[i].transform.position = spawnPoint.position;
			population[i].transform.rotation = Quaternion.identity;
			population[i].Restart();
		}

		CarsSelector.Instance.SelectionIsAvailable = true;
		GenomeUI.Instance.DrawingGenomeIsAvailable = true;
		MenuController.Instance.ShowEvolutionInfo(false);
		endDissolveEffectCoroutine = StartCoroutine(EndDissolveEffect());
	}

	private IEnumerator EndGeneration()
	{
		MenuController menuController = MenuController.Instance;

		// Generation & time counter:
		for(int i = 0; i < generationLifeTime; i++)
		{
			menuController.GenerationText.text = "Generation: "+generationNumber+" ("+(generationLifeTime-i)+"s)";
			yield return new WaitForSeconds(1);
		}
		menuController.GenerationText.text = "Generation: "+generationNumber+" (0s)";

		drivingCars = 0;
		raceIsStopped = true;
		StopCars();

		if(startEvolutionCoroutine == null)
		{
			startEvolutionCoroutine = StartCoroutine(StartEvolution());
		}
	}

	private void StopCars()
	{
		for(int i = 0; i < populationNumber; i++)
		{
			population[i].StopCar();
		}
	}

	public static void CarIsCrashed()
	{
		drivingCars--;
		if(drivingCars < 1)
		{
			drivingCars = 0;
			raceIsStopped = true;
			instance.StopCoroutine(instance.endGenerationCoroutine);

			if(instance.startEvolutionCoroutine == null)
			{
				instance.startEvolutionCoroutine = instance.StartCoroutine(instance.StartEvolution());
			}
		}
	}

	private IEnumerator StartEvolution()
	{
		// UI & visual effects:
		CarsSelector carsSelector = CarsSelector.Instance;
		carsSelector.SelectionIsAvailable = false;
		carsSelector.ResetSelection();
		GenomeUI.Instance.DrawingGenomeIsAvailable = false;
		MenuController.Instance.ShowEvolutionInfo(true);
		FitnessCheckpoint.ResetColors();

		startDissolveEffectCoroutine = StartCoroutine(StartDissolveEffect());
		yield return new WaitForSeconds(1);

		// Create new generation:
		carBrainEvolution.StartEvolution(population);
		startEvolutionCoroutine = null;
	}

	public static void ResetStaticValues()
	{
		raceIsStopped = true;
		drivingCars = 0;
		instance = null;
	}

	#region Visual effects:
	public IEnumerator StartDissolveEffectOnSceneRestarting()
	{
		// Stop EndDissolveEffect coroutine:
		if(endDissolveEffectCoroutine != null)
		{
			StopCoroutine(endDissolveEffectCoroutine);
			endDissolveEffectCoroutine = null;
		}

		// Stop StartDissolveEffect coroutine:
		if(startDissolveEffectCoroutine != null)
		{
			StopCoroutine(startDissolveEffectCoroutine);
			startDissolveEffectCoroutine = null;
		}

		// Get unique car materials / species materials:
		Material[] speciesMaterials = GetSpeciesMaterials();

		// Dissolve:
		float dissolveEffect = speciesMaterials[0].GetFloat("_Dissolve");
		for(int i = (int)(dissolveEffect*50); i <= 50; i++)
		{
			ChangeDissolveEffect(speciesMaterials, i/50f);
			yield return new WaitForSecondsRealtime(0.01f);
		}
	}
	private IEnumerator StartDissolveEffect()
	{
		// Stop EndDissolveEffect coroutine:
		if(endDissolveEffectCoroutine != null)
		{
			StopCoroutine(endDissolveEffectCoroutine);
			endDissolveEffectCoroutine = null;
		}

		// Get unique car materials / species materials:
		Material[] speciesMaterials = GetSpeciesMaterials();

		// Dissolve:
		for(int i = 0; i <= 50; i++)
		{
			ChangeDissolveEffect(speciesMaterials, i/50f);
			yield return new WaitForSeconds(0.01f);
		}
	}

	private IEnumerator EndDissolveEffect()
	{
		// Stop StartDissolveEffect coroutine:
		if(startDissolveEffectCoroutine != null)
		{
			StopCoroutine(startDissolveEffectCoroutine);
			startDissolveEffectCoroutine = null;
		}

		// Get unique car materials / species materials:
		Material[] speciesMaterials = GetSpeciesMaterials();

		// Undo dissolve:
		for(int i = 25; i >= 0; i--)
		{
			ChangeDissolveEffect(speciesMaterials, i/25f);
			yield return new WaitForSeconds(0.01f);
		}
	}

	private Material[] GetSpeciesMaterials()
	{
		Material[] speciesMaterials;
		if(carBrainEvolution.SpeciesList.Count == 0) // Species not exists yet (first generation).
		{
			speciesMaterials = new Material[4];
			speciesMaterials[0] = population[0].CarMaterial;
			speciesMaterials[1] = population[0].CrashedCarMaterial;
			speciesMaterials[2] = population[0].PotatoCarMaterial;
			speciesMaterials[3] = population[0].CrashedPotatoCarMaterial;
		}
		else // Species exists.
		{
			int speciesCount = carBrainEvolution.SpeciesList.Count;
			speciesMaterials = new Material[speciesCount*4];

			for(int i = 0; i < speciesCount; i++)
			{
				speciesMaterials[i*4] = carBrainEvolution.SpeciesList[i].BrainsList[0].CarMaterial;
				speciesMaterials[i*4+1] = carBrainEvolution.SpeciesList[i].BrainsList[0].CrashedCarMaterial;
				speciesMaterials[i*4+2] = carBrainEvolution.SpeciesList[i].BrainsList[0].PotatoCarMaterial;
				speciesMaterials[i*4+3] = carBrainEvolution.SpeciesList[i].BrainsList[0].CrashedPotatoCarMaterial;
			}
		}

		return speciesMaterials;
	}

	private void ChangeDissolveEffect(Material[] dissolveMaterials, float dissolveEffect)
	{
		foreach(Material material in dissolveMaterials)
		{
			material.SetFloat("_Dissolve", dissolveEffect);
		}
	}
	#endregion Visual effects.
}
