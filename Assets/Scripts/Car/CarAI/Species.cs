using System.Collections.Generic;
using UnityEngine;

public class Species
{
	private static int allCreatedSpeciesSum = 0, bestSpeciesFitness = 0;
	private static List<Color> usedSpeciesColorsList = new List<Color>();

	private readonly int maxStaleness = 20;
	private int speciesNumber, staleness = 0, bestFitness = 0;
	private Genome bestGenomePattern;
	private List<CarBrain> brainsList = new List<CarBrain>();
	private Color speciesColor;


	public int SpeciesNumber { get { return speciesNumber; } }
	public int BestFitness { get { return bestFitness; } }
	public Genome BestGenomePattern { get { return bestGenomePattern; } }
	public List<CarBrain> BrainsList { get { return brainsList; } }
	public Color SpeciesColor { get { return speciesColor; } }


	/// <summary>
	/// Create new Species which will have reference list to the cars with similar Genome.
	/// </summary>
	public Species(CarBrain firstBrain)
	{
		allCreatedSpeciesSum++;
		speciesNumber = allCreatedSpeciesSum;
		PickColor();

		brainsList.Add(firstBrain);
		bestGenomePattern = new Genome(CarBrain.InNodesNumber, CarBrain.OutNodesNumber);
		bestGenomePattern.CloneGenome(firstBrain.Genome);
	}

	/// <summary>
	/// Update the best Species fitness and Genome pattern.
	/// </summary>
	public void UpdateBestFitness()
	{
		int bestFitness = int.MinValue;
		CarBrain bestBrain = null;

		foreach(CarBrain brain in brainsList)
		{
			if(bestFitness < brain.Fitness)
			{
				bestFitness = brain.Fitness;
				bestBrain = brain;
			}
		}
		this.bestFitness = bestFitness;

		if(bestBrain != null)
		{
			bestGenomePattern.CloneGenome(bestBrain.Genome);
		}
	}

	#region Species survival:
	/// <summary>
	/// In each generation execute it to check improvement of this Species.<para/>
	/// Returns "true" if is improvement badly. Otherwise returns "false".
	/// </summary>
	public bool IsSpeciesImprovementBadly()
	{
		bool isBadly = false;
		if(brainsList.Count > 0)
		{
			isBadly = CheckStaleness();
			UpdateBestSpeciesFitness();
		}
		else
		{
			isBadly = true; // Empty Species is a dead Species.
		}

		return isBadly;
	}

	/// <summary>
	/// Check if fitness of Species is improvement. If not ("staleness" value exceed the "maxStaleness"), returns true.<para/>
	/// Otherwise returns "false".
	/// </summary>
	private bool CheckStaleness()
	{
		bool isStaleness = true;
		int lastBestFitness = bestFitness;
		UpdateBestFitness();

		if(lastBestFitness < bestFitness)
		{
			isStaleness = false;
		}

		if(isStaleness)
		{
			staleness++;
			if(staleness > maxStaleness)
			{
				return true; // Remove the staleness Species.
			}
		}
		else
		{
			staleness--;
			if(staleness < 0)
			{
				staleness = 0;
			}
		}

		return false;
	}

	/// <summary>
	/// If this Species has the best fitness then update static value "bestSpeciesFitness".
	/// </summary>
	private void UpdateBestSpeciesFitness()
	{
		if(bestSpeciesFitness < bestFitness)
		{
			bestSpeciesFitness = bestFitness;
		}
	}
	#endregion Species survival.

	#region Species colors:
	private void PickColor()
	{
		Color[] speciesColorsPool = new Color[] { Color.yellow, Color.blue, Color.green, Color.cyan, Color.red, Color.white, Color.magenta, Color.grey,
			new Color(1, 0.6f, 0), new Color(0.4f, 0.2f, 0), new Color(1, 0.314f, 0.314f), new Color(0.4f, 0.4f, 0.2f), new Color(0.6f, 0, 0.6f),
			new Color(0.6f, 1, 0.6f), new Color(0.4f, 0, 1), new Color(0.4f, 0.4f, 0.6f), new Color(0, 0, 0.4f), new Color(0, 0.6f, 0.8f),
			new Color(0, 0.6f, 0.6f), new Color(0.6f, 0.8f, 1), new Color(0.6f, 1, 0.4f), new Color(0.4f, 0.4f, 0.2f), new Color(1, 1, 0.8f)};

		// If we have some not used color from the pool, choose one:
		if(speciesColorsPool.Length > usedSpeciesColorsList.Count)
		{
			foreach(Color colorFromPool in speciesColorsPool)
			{
				bool colorInUse = false;
				foreach(Color existingColor in usedSpeciesColorsList)
				{
					if(colorFromPool.Equals(existingColor))
					{
						colorInUse = true;
						break;
					}
				}

				if(!colorInUse)
				{
					speciesColor = colorFromPool;
					usedSpeciesColorsList.Add(speciesColor);
					break;
				}
			}
		}
		else
		{
			// Set some random color:
			speciesColor = new Color(Random.Range(0, 256), Random.Range(0, 256), Random.Range(0, 256));
		}
	}

	public void UpdateCarsColor()
	{
		Material newCarMaterial = new Material(CarsController.Instance.CarMaterial);
		newCarMaterial.SetColor("_BaseColor", speciesColor*8);
		Material newPotatoCarMaterial = new Material(CarsController.Instance.PotatoCarMaterial);
		newPotatoCarMaterial.SetColor("_BaseColor", speciesColor*8);

		Material newCrashedCarMaterial = new Material(CarsController.Instance.CrashedCarMaterial);
		newCrashedCarMaterial.SetColor("_BaseColor", speciesColor);
		Material newCrashedPotatoCarMaterial = new Material(CarsController.Instance.CrashedPotatoCarMaterial);
		newCrashedPotatoCarMaterial.SetColor("_BaseColor", speciesColor);

		Material newSensorVisualizerMaterial = new Material(brainsList[0].SensorVisualizer.LinesMaterial);
		Color linesColor = speciesColor;
		linesColor.a = 0.5f;
		newSensorVisualizerMaterial.SetColor("_BaseColor", linesColor);
		newSensorVisualizerMaterial.SetColor("_EmissionColor", linesColor);

		foreach(CarBrain brain in brainsList)
		{
			MeshRenderer[] carMeshRenderers = brain.GetComponentsInChildren<MeshRenderer>(true);
			carMeshRenderers[0].sharedMaterial = newCarMaterial;
			carMeshRenderers[1].sharedMaterial = newPotatoCarMaterial;

			brain.CarMaterial = newCarMaterial;
			brain.CrashedCarMaterial = newCrashedCarMaterial;
			brain.PotatoCarMaterial = newPotatoCarMaterial;
			brain.CrashedPotatoCarMaterial = newCrashedPotatoCarMaterial;

			brain.SensorVisualizer.LinesMaterial = newSensorVisualizerMaterial;
		}
	}

	public void ReleaseColor()
	{
		usedSpeciesColorsList.Remove(speciesColor);
	}
	#endregion Species colors.

	public static void ResetStaticValues()
	{
		allCreatedSpeciesSum = 0;
		bestSpeciesFitness = 0;
		usedSpeciesColorsList = new List<Color>();
	}
}
