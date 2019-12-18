using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarBrainEvolution
{
	private CarsController carsController;
	private List<Species> speciesList = new List<Species>();

	public List<Species> SpeciesList { get { return speciesList; } }


	public CarBrainEvolution(CarsController carsController)
	{
		this.carsController = carsController;
	}

	public void StartEvolution(CarBrain[] brains)
	{
		// Divide cars into the species:
		Speciation(brains);

		// Eliminate weak Species after their immunity time or if they are not improving:
		EliminateWeakSpecies();

		// "Destroy" weak cars (in this algorithm modify them for optimization (pooling)) and leave the best in each Species:
		List<CarBrain> bestBrainsList;
		List<CarBrain> weakerBrainsList;
		Selection(brains, out bestBrainsList, out weakerBrainsList);

		// Reproduce the best cars from each Species:
		Crossover(brains, bestBrainsList, weakerBrainsList);

		// Mutate the cars' brains:
		Mutate(brains);

		// Change the cars' colors to suit the Species:
		UpdateCarsColors();

		// Update the Species UI & reset the Genome UI:
		SpeciesUI.Instance.UpdateSpeciesData(speciesList);
		GenomeUI.Instance.ClearGenomePanel(true);

		// Start the next generation race:
		carsController.StartRace();
	}

	#region Evolution:
	private void Speciation(CarBrain[] brains)
	{
		// Clear Species:
		for(int speciesIndex = 0; speciesIndex < speciesList.Count; speciesIndex++)
		{
			speciesList[speciesIndex].BrainsList.Clear();
		}

		// Add a similar CarBrain into existing Species:
		for(int brainIndex = 0; brainIndex < brains.Length; brainIndex++)
		{
			bool speciesFound = false;
			for(int speciesIndex = 0; speciesIndex < speciesList.Count; speciesIndex++)
			{
				if(AreGenomesSimilar(brains[brainIndex].Genome, speciesList[speciesIndex].BestGenomePattern))
				{
					speciesList[speciesIndex].BrainsList.Add(brains[brainIndex]);
					speciesFound = true;
					break;
				}
			}

			// If Species is not found, create a new one and add into it a CarBrain:
			if(!speciesFound)
			{
				speciesList.Add(new Species(brains[brainIndex]));
			}
		}
	}

	private bool AreGenomesSimilar(Genome genome1, Genome genome2)
	{
		// Original formula for checking the Genomes compatibility: d = c1*E/N + c2*D/N + c3*W
		// c1, c2 = genes / connections coefficient,
		// E, D = Excess and Disjoint genes without match,
		// c3 = weights coefficient,
		// W = average difference between weights.

		float compatibilityThreshold = 4;
		float connectionsCoefficient = 1.5f; // c1 = c2 (excess and disjoint genes are count together here).
		float weightsCoefficient = 0; // c3.
		int brain1Size = genome1.NodesConnectionsList.Count;
		int brain2Size = genome2.NodesConnectionsList.Count;

		// Get matching between normal, disjoint and excess genes / connections, and average difference between weights:
		int innovationMatching = 0;
		float averageDiffBetweenWeights = 0; // W.
		for(int i = 0; i < brain1Size; i++)
		{
			for(int j = 0; j < brain2Size; j++)
			{
				if(genome1.NodesConnectionsList[i].Innovation == genome2.NodesConnectionsList[j].Innovation)
				{
					innovationMatching++;
					averageDiffBetweenWeights += Math.Abs(genome1.NodesConnectionsList[i].Weight - genome2.NodesConnectionsList[j].Weight);
					break;
				}
			}
		}

		// Get difference between disjoint and excess genes (genes which don't match):
		float innovationDifference = (brain1Size + brain2Size) - 2*innovationMatching; // E = D (Excess and Disjoint genes are count together here).

		// Get the larger Genome size and normalize this value to 1 if is smaller than 20:
		int largerGenomeNormalizer = (brain1Size > brain2Size) ? brain1Size-20 : brain2Size-20;
		largerGenomeNormalizer = (largerGenomeNormalizer < 1) ? 1 : largerGenomeNormalizer;

		// Get compatibility value. Formula is a little modified (c1 = c2 and E = D so: d = c1*E/N + c3*W):
		float compatibility = (connectionsCoefficient * innovationDifference/largerGenomeNormalizer) +
			(weightsCoefficient * averageDiffBetweenWeights);

		return (compatibilityThreshold > compatibility);
	}

	private void EliminateWeakSpecies()
	{
		if(speciesList.Count <= 1)
		{
			// If exists only 1 Species then update only its best fitness:
			if(speciesList.Count == 1)
			{
				speciesList[0].UpdateBestFitness();
			}
			return;
		}

		for(int i = 0; i < speciesList.Count; i++)
		{
			if(speciesList[i].IsSpeciesImprovementBadly())
			{
				speciesList[i].ReleaseColor();
				speciesList.RemoveAt(i);
				i--;
			}
		}
	}

	private void Selection(CarBrain[] brains, out List<CarBrain> bestBrainsList, out List<CarBrain> weakerBrainsList)
	{
		// Find the best CarBrain in each Species:
		bestBrainsList = new List<CarBrain>();
		for(int speciesIndex = 0; speciesIndex < speciesList.Count; speciesIndex++)
		{
			CarBrain bestBrain = speciesList[speciesIndex].BrainsList[0];
			for(int brainIndex = 1; brainIndex < speciesList[speciesIndex].BrainsList.Count; brainIndex++)
			{
				CarBrain nextBrain = speciesList[speciesIndex].BrainsList[brainIndex];
				if(bestBrain.Fitness < nextBrain.Fitness)
				{
					bestBrain = nextBrain;
				}
			}
			bestBrainsList.Add(bestBrain);
		}

		// Get the weaker cars and set them into temporary list (later they Genomes will be modified):
		weakerBrainsList = new List<CarBrain>();
		for(int i = 0; i < brains.Length; i++)
		{
			bool isBestBrain = false;
			for(int j = 0; j < bestBrainsList.Count; j++)
			{
				if(brains[i] == bestBrainsList[j])
				{
					isBestBrain = true;
					break;
				}
			}

			if(!isBestBrain)
			{
				weakerBrainsList.Add(brains[i]);
			}
		}
	}

	private void Crossover(CarBrain[] brains, List<CarBrain> bestBrainsList, List<CarBrain> weakerBrainsList)
	{
		// Reproduce / clone the best cars in each Species (instead of cross better half of random cars):
		int speciesMaxSize = brains.Length / speciesList.Count;
		int rest = brains.Length - speciesMaxSize*speciesList.Count;
		int weakerBrainsIndex = 0;

		for(int speciesIndex = 0; speciesIndex < speciesList.Count; speciesIndex++)
		{
			// Clear carsBrainsList in Species and add again the best car into this list:
			speciesList[speciesIndex].BrainsList.Clear();
			speciesList[speciesIndex].BrainsList.Add(bestBrainsList[speciesIndex]); // speciesList count is equal to bestCarsBrainsList count.

			// Add weaker cars into the Species and modifiy their Genomes, cloning Genome from the best car:
			bool restAdded = false;
			for(int i = 1; i < speciesMaxSize; i++)
			{
				CarBrain modifiedBrain = weakerBrainsList[weakerBrainsIndex];
				modifiedBrain.Genome.CloneGenome(speciesList[speciesIndex].BrainsList[0].Genome);
				speciesList[speciesIndex].BrainsList.Add(modifiedBrain);
				weakerBrainsIndex++;

				// Add to the list extra car:
				if(!restAdded && rest > 0)
				{
					i--;
					rest--;
					restAdded = true;
				}
			}
		}
	}

	private void Mutate(CarBrain[] brains)
	{
		System.Random rand = new System.Random();

		for(int brainIndex = 0; brainIndex < brains.Length; brainIndex++)
		{
			// 80% chance for each Genome to mutate their weights in the NodesConnections:
			if(rand.Next(0, 100) < 80)
			{
				for(int connectionIndex = 0; connectionIndex < brains[brainIndex].Genome.NodesConnectionsList.Count; connectionIndex++)
				{
					// 10% chance for entirely change the weight and 90% for a little change:
					if(rand.Next(0, 100) < 10)
					{
						brains[brainIndex].Genome.NodesConnectionsList[connectionIndex].Weight = (float)rand.NextDouble()*2-1;
					}
					else
					{
						NodesConnection connection = brains[brainIndex].Genome.NodesConnectionsList[connectionIndex];

						float additionalValue = connection.Weight/10;
						additionalValue = (rand.Next(0, 2) == 0) ? additionalValue : -additionalValue;

						float weight = connection.Weight;
						weight += additionalValue;
						if(weight > 1)
						{
							weight = 1;
						}
						else if(weight < -1)
						{
							weight = -1;
						}

						connection.Weight = weight;
					}
				}
			}

			// 5% chance for each Genome to add a new NodesConnection:
			if(rand.Next(0, 100) < 5)
			{
				brains[brainIndex].Genome.AddNewNodesConnection();
			}

			// 2% chance for each Genome to add a new Node:
			if(rand.Next(0, 100) < 2)
			{
				brains[brainIndex].Genome.AddNewNode();
			}
		}
	}
	#endregion Evolution.

	#region Other:
	private void UpdateCarsColors()
	{
		foreach(Species species in speciesList)
		{
			species.UpdateCarsColor();
		}
	}
	#endregion Other.
}
