using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// CarBrain has Genome / Neuron Network, and thinks about decisions which have to make and fitness.
/// </summary>
public class CarBrain : MonoBehaviour
{
	private static readonly int inNodesNumber = 5, outNodesNumber = 2;

	private bool carIsCrashed = false, passedNewCheckpoint = false;
	[SerializeField]
	private int fitness = 0, lastPassedCheckpointIndex = -1;
	private Genome genome;
	private CarMovement carMovement;
	private CarsSound carsSound;
	[SerializeField]
	private Transform sensor;
	private SensorVisualizer sensorVisualizer;
	private MeshRenderer vehicleMeshRenderer, potatoVehicleMeshRenderer;
	private Material carMaterial, crashedCarMaterial, potatoCarMaterial, crashedPotatoCarMaterial;
	[SerializeField]
	private Animator fitnessAnimator;
	private Coroutine selfDestructionCoroutine;


	public static int InNodesNumber { get { return inNodesNumber; } }
	public static int OutNodesNumber { get { return outNodesNumber; } }

	public bool CarIsCrashed { get { return carIsCrashed; } set { carIsCrashed = value; } }
	public int Fitness { get { return fitness; } set { fitness = value; } }
	public Genome Genome { get { return genome; } }
	public SensorVisualizer SensorVisualizer { get { return sensorVisualizer; } }
	public Material CarMaterial { get { return carMaterial; } set { carMaterial = value; } }
	public Material CrashedCarMaterial { get { return crashedCarMaterial; } set { crashedCarMaterial = value; } }
	public Material PotatoCarMaterial { get { return potatoCarMaterial; } set { potatoCarMaterial = value; } }
	public Material CrashedPotatoCarMaterial { get { return crashedPotatoCarMaterial; } set { crashedPotatoCarMaterial = value; } }


	private void Awake()
	{
		carMovement = GetComponent<CarMovement>();
		carsSound = CarsController.Instance.GetComponent<CarsSound>();
		sensorVisualizer = sensor.GetComponentInChildren<SensorVisualizer>();

		MeshRenderer[] carMeshRenderers = GetComponentsInChildren<MeshRenderer>(true);
		vehicleMeshRenderer = carMeshRenderers[0];
		potatoVehicleMeshRenderer = carMeshRenderers[1];

		genome = new Genome(inNodesNumber, outNodesNumber);
	}

	private void FixedUpdate()
	{
		if(!CarsController.RaceIsStopped && !carIsCrashed)
		{
			ThinkAndMakeDecision();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		FitnessCheckpoint fitnessCheckpoint;
		if(!CarsController.RaceIsStopped && !carIsCrashed && (fitnessCheckpoint = other.GetComponent<FitnessCheckpoint>()) != null)
		{
			CalculateFitnessFromCheckpoint(fitnessCheckpoint);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if(!CarsController.RaceIsStopped && !carIsCrashed && collision.gameObject.CompareTag("Obstacle"))
		{
			CarCrash();
		}
	}

	public void StopCar()
	{
		carMovement.Speed = 0;
		sensorVisualizer.ShowSensors(false);
		StopCoroutine(selfDestructionCoroutine);
	}

	private void CarCrash()
	{
		carIsCrashed = true;
		CarsController.CarIsCrashed();
		StopCar();
		vehicleMeshRenderer.sharedMaterial = crashedCarMaterial;
		potatoVehicleMeshRenderer.sharedMaterial = crashedPotatoCarMaterial;

		if(!Settings.IsAudioMuted)
		{
			carsSound.PlayCrashSound();
		}
	}

	private IEnumerator SelfDestructionDueToStupidity()
	{
		int lastFitness = fitness;
		passedNewCheckpoint = false; // Some cars can drive in a circle so check if they pass the new checkpoint.
		yield return new WaitForSeconds(10);

		if(fitness < lastFitness+1000 || !passedNewCheckpoint)
		{
			CarCrash();
		}
		else
		{
			selfDestructionCoroutine = StartCoroutine(SelfDestructionDueToStupidity());
		}
	}

	public void Restart()
	{
		fitness = 0;
		lastPassedCheckpointIndex = -1;
		carIsCrashed = false;
		sensorVisualizer.ShowSensors(true);
		selfDestructionCoroutine = StartCoroutine(SelfDestructionDueToStupidity());
	}

	#region Calculate the decisions about speed and direction:
	private void ThinkAndMakeDecision()
	{
		float[] distanceFromObstacle = GetDistanceFromObstacles(); // Scan the environment.
		FeedForward(distanceFromObstacle); // Think about decision.
		ExecuteDecision(); // Make a decision.
	}

	private float[] GetDistanceFromObstacles()
	{
		float[] distanceFromObstacle = new float[inNodesNumber];
		RaycastHit hit;
		LayerMask layerMask = ~(1 << LayerMask.NameToLayer("Ignore Raycast") | 1 << LayerMask.NameToLayer("Car"));

		// Forward:
		if(Physics.Raycast(sensor.position, transform.forward, out hit, Mathf.Infinity, layerMask))
		{
			distanceFromObstacle[0] = hit.distance;
			sensorVisualizer.SetVisualizationData(0, hit.point);
		}

		// Left:
		if(Physics.Raycast(sensor.position, -transform.right, out hit, Mathf.Infinity, layerMask))
		{
			distanceFromObstacle[1] = hit.distance;
			sensorVisualizer.SetVisualizationData(1, hit.point);
		}

		// Right:
		if(Physics.Raycast(sensor.position, transform.right, out hit, Mathf.Infinity, layerMask))
		{
			distanceFromObstacle[2] = hit.distance;
			sensorVisualizer.SetVisualizationData(2, hit.point);
		}

		// Forward left:
		if(Physics.Raycast(sensor.position, transform.forward + -transform.right, out hit, Mathf.Infinity, layerMask))
		{
			distanceFromObstacle[3] = hit.distance;
			sensorVisualizer.SetVisualizationData(3, hit.point);
		}

		// Forward right:
		if(Physics.Raycast(sensor.position, transform.forward + transform.right, out hit, Mathf.Infinity, layerMask))
		{
			distanceFromObstacle[4] = hit.distance;
			sensorVisualizer.SetVisualizationData(4, hit.point);
		}

		return distanceFromObstacle;
	}

	/// <summary>
	/// Calculate the activation value in each Node.
	/// </summary>
	private void FeedForward(float[] distanceFromObstacle)
	{
		// Change activations in the input Nodes:
		for(int nodeIndex = 0; nodeIndex < inNodesNumber; nodeIndex++)
		{
			genome.NodesList[nodeIndex].Activation = distanceFromObstacle[nodeIndex];
		}

		// Calculate activations first in the hidden Nodes (if exists) and next in the output Nodes:
		for(int nodeIndex = genome.NodesList.Count-1; nodeIndex >= inNodesNumber; nodeIndex--)
		{
			// Don't calculate activation for the Bias Node:
			if(genome.BiasNodeIndex == nodeIndex)
			{
				continue;
			}

			Node node = genome.NodesList[nodeIndex]; // "Our" Node which will have recalculate the activation value.
			float newNodeActivation = 0;
			for(int connectionIndex = 0; connectionIndex < node.InConnectionsList.Count; connectionIndex++)
			{
				// If NodesConnection is enabled & if "our" Node is output of the NodesConnection then calculate the new activation for this Node:
				if(node.InConnectionsList[connectionIndex].Enabled && node.InConnectionsList[connectionIndex].OutNode == node)
				{
					Node inNode = node.InConnectionsList[connectionIndex].InNode; // Node which is connected with "our" Node from the left side.
					float connectionValue = inNode.Activation * node.InConnectionsList[connectionIndex].Weight;

					// If is connected with Bias Node then subtract instead of adding:
					if(inNode != genome.NodesList[genome.BiasNodeIndex])
					{
						newNodeActivation += connectionValue;
					}
					else
					{
						newNodeActivation -= connectionValue;
					}
				}
			}

			// Pass activation value through the ReLU function (instead of the "old" Sigmoid function):
			//newNodeActivation = (newNodeActivation > 0) ? newNodeActivation : 0; // ReLU(x) = max(0, x).

			// Pass activation value through the TanH function:
			newNodeActivation = (float)System.Math.Tanh(newNodeActivation);

			// Set new activation value for Node:
			genome.NodesList[nodeIndex].Activation = newNodeActivation;
		}
	}

	private void ExecuteDecision()
	{
		float firstOutputNodeValue = genome.NodesList[inNodesNumber].Activation;
		float secondOutputNodeValue = genome.NodesList[inNodesNumber+1].Activation;

		carMovement.Speed = firstOutputNodeValue;
		carMovement.Direction = secondOutputNodeValue;

		CalculateFitnessFromSpeed(firstOutputNodeValue);
	}
	#endregion Calculate the decisions about speed and direction.

	#region Calculate the fitness:
	private void CalculateFitnessFromCheckpoint(FitnessCheckpoint fitnessCheckpoint)
	{
		int checkpointIndex = fitnessCheckpoint.CheckpointIndex;
		int lastCheckpointIndex = FitnessCheckpoint.LastCheckpointIndex;

		if(lastPassedCheckpointIndex == checkpointIndex)
		{
			return;
		}
		else if(lastPassedCheckpointIndex == checkpointIndex-1 || // Moving forward to higher indexed checkpoint.
			lastPassedCheckpointIndex == lastCheckpointIndex && checkpointIndex == 0) // Moving forward from last to first checkpoint.
		{
			fitness += 500;
			passedNewCheckpoint = true;
			if(fitnessAnimator.gameObject.activeInHierarchy)
			{
				fitnessAnimator.Play("FitnessUpdate", 0, 0);
			}
			fitnessCheckpoint.ReactToTouchedCar(crashedCarMaterial.GetColor("_BaseColor"));
		}
		else if(lastPassedCheckpointIndex == checkpointIndex+1 || // Moving back to lower indexed checkpoint.
			checkpointIndex == lastCheckpointIndex && lastPassedCheckpointIndex <= 0) // Moving back from first (or finish line) to last checkpoint.
		{
			fitness -= 500;
			passedNewCheckpoint = false;
			if(fitnessAnimator.gameObject.activeInHierarchy)
			{
				fitnessAnimator.Play("FitnessUpdate", 0, 0);
			}
			fitnessCheckpoint.PlayTriggerAnimation();
		}

		lastPassedCheckpointIndex = checkpointIndex;
	}

	private void CalculateFitnessFromSpeed(float speed)
	{
		fitness += (int)(speed*5);
	}
	#endregion Calculate the fitness.
}
