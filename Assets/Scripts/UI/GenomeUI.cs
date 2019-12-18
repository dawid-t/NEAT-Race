using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GenomeUI : MonoBehaviour
{
	private static GenomeUI instance;

	private bool drawingGenomeIsAvailable = true, lastBrainIsLocked = false;
	[SerializeField]
	private TextMeshProUGUI genomeDataText;
	private TextMeshProUGUI[] nodesActivationTexts;
	private Image genomeDataPanelImage;
	[SerializeField]
	private GameObject genomeDataPanel, genomeDataNodePrefab, genomeDataNodesConnectionPrefab;
	[SerializeField]
	private Transform genomeDataPanelNodes, genomeDataPanelNodesConnections, genomeDataPanelNodesConnectionInfo;
	private Queue<GameObject> genomeDataNodesQueue = new Queue<GameObject>(), genomeDataNodesConnectionsQueue = new Queue<GameObject>();
	private CarBrain lastBrain;
	private Coroutine drawBestGenomeFromSpeciesCoroutine;
	private Animator genomeDataTextAnimator;


	public static GenomeUI Instance { get { return instance; } }

	public bool DrawingGenomeIsAvailable { get { return drawingGenomeIsAvailable; } set { drawingGenomeIsAvailable = value; } }
	public Transform GenomeDataPanelNodesConnectionInfo { get { return genomeDataPanelNodesConnectionInfo; } }


	private void Awake()
	{
		instance = this;
		genomeDataPanelImage = genomeDataPanel.GetComponent<Image>();
		genomeDataTextAnimator = genomeDataText.GetComponent<Animator>();
	}

	private void Update()
	{
		if(Input.GetMouseButtonDown(0))
		{
			NodesConnectionUIListener.DeactivateLastShowedInfo();
		}
		ShowGenome();
		UpdateGenomeData();
	}

	/// <summary>
	/// Show a car's Genome after hover mouse cursor on it.
	/// </summary>
	private void ShowGenome()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		int layerMask = LayerMask.GetMask("Car");

		if(Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)) // Show Genome.
		{
			CarBrain brain;
			if(drawingGenomeIsAvailable && CanDrawGenome(brain = hit.transform.GetComponent<CarBrain>()))
			{
				StopDrawBestGenomeFromSpeciesCoroutine();
				DrawGenome(brain);
			}
		}
		else if(Input.GetMouseButtonDown(0) && Physics.Raycast(ray) && !MenuController.IsPointerOverUI()) // Hide Genome.
		{
			ClearGenomePanel(true);
		}
	}

	#region Draw the best current Species' Genome:
	/// <summary>
	/// Select best Genome in a given Species. Set 'lockBrain' to true if want to lock Species and bypass the former lock.
	/// </summary>
	public void ShowSpeciesBestGenome(Species species, bool lockBrain)
	{
		// Lock the "mouse hover" selection if lockBrain is true. Otherwise don't override the lastBrainIsLocked value:
		if(lockBrain)
		{
			lastBrainIsLocked = lockBrain;
			genomeDataText.fontStyle = FontStyles.Underline;
		}

		// Return when:
		if(!drawingGenomeIsAvailable || // Drawing is not available,
			lastBrainIsLocked && !lockBrain || // Selected Genome is locked and you don't want to lock this Genome,
			species.BrainsList[0] == lastBrain) // Same Genome is selected.
		{
			return;
		}

		// Start selecting the best current Genome from given Species, every 1s:
		StopDrawBestGenomeFromSpeciesCoroutine();
		drawBestGenomeFromSpeciesCoroutine = StartCoroutine(DrawBestGenomeFromSpecies(species));
	}

	private IEnumerator DrawBestGenomeFromSpecies(Species species)
	{
		while(true)
		{
			CarBrain bestBrain = species.BrainsList[0];
			for(int i = 1; i < species.BrainsList.Count; i++)
			{
				if(bestBrain.Fitness < species.BrainsList[i].Fitness)
				{
					bestBrain = species.BrainsList[i];
				}
			}

			if(bestBrain != lastBrain)
			{
				DrawGenome(bestBrain);
			}
			yield return new WaitForSeconds(1);
		}
	}

	private void StopDrawBestGenomeFromSpeciesCoroutine()
	{
		if(drawBestGenomeFromSpeciesCoroutine != null)
		{
			StopCoroutine(drawBestGenomeFromSpeciesCoroutine);
		}
	}
	#endregion Draw the best current Species' Genome.

	private bool CanDrawGenome(CarBrain brain)
	{
		if(Input.GetMouseButtonDown(0))
		{
			lastBrainIsLocked = true;
			genomeDataText.fontStyle = FontStyles.Underline;
		}

		return (brain != lastBrain && (!lastBrainIsLocked || Input.GetMouseButtonDown(0)));
	}

	private void DrawGenome(CarBrain brain)
	{
		ClearGenomePanel(false);
		lastBrain = brain;

		Color textColor = brain.CrashedCarMaterial.GetColor("_BaseColor");
		genomeDataText.color = new Color(textColor.r, textColor.g, textColor.b, 1);
		genomeDataTextAnimator.Play("WindowPopUp2", 0, 0);

		GameObject[] nodesUI = DrawNodes(brain);
		DrawNodesConnections(brain, nodesUI);
	}

	private GameObject[] DrawNodes(CarBrain brain)
	{
		int nodesNumber = brain.Genome.NodesList.Count;
		Vector2 genomeDataPanelSize = genomeDataPanel.GetComponent<RectTransform>().rect.size;

		#region Calculate the offset for UI Nodes in the genomeDataPanel:
		// Input Nodes:
		float inNodesOffsetY = genomeDataPanelSize.y / (CarBrain.InNodesNumber+1); // +1 because of Bias Node.
		int inNodesOffsetMultiplier = 1;

		// Output Nodes:
		float outNodesOffsetY = genomeDataPanelSize.y / CarBrain.OutNodesNumber;
		int outNodesOffsetMultiplier = 1;

		// Hidden Nodes:
		int hiddenLayersNumber = brain.Genome.MaxNodeLayer - 2;
		int[] hiddenNodesNumberInLayers = new int[hiddenLayersNumber];
		for(int i = 0; i < nodesNumber; i++)
		{
			Node node = brain.Genome.NodesList[i];
			if(node.Layer != 1 && node.Layer != brain.Genome.MaxNodeLayer)
			{
				hiddenNodesNumberInLayers[node.Layer-2]++;
			}
		}

		float[] hiddenNodesOffsetsY = new float[hiddenLayersNumber];
		int[] hiddenNodesOffsetMultipliers = new int[hiddenLayersNumber]; // Different multiplier for each hidden layer.
		for(int i = 0; i < hiddenLayersNumber; i++)
		{
			hiddenNodesOffsetsY[i] = genomeDataPanelSize.y / hiddenNodesNumberInLayers[i];
			hiddenNodesOffsetMultipliers[i] = 1;
		}

		float hiddenNodeOffsetX = genomeDataPanelSize.x / brain.Genome.MaxNodeLayer;
		#endregion Calculate the offset for UI Nodes in the genomeDataPanel.

		GameObject[] nodesUI = new GameObject[nodesNumber];
		nodesActivationTexts = new TextMeshProUGUI[nodesNumber];
		for(int i = 0; i < nodesNumber; i++)
		{
			Node node = brain.Genome.NodesList[i];
			GameObject nodeUI = GetGenomeDataUIElementFromPool(genomeDataNodesQueue, genomeDataNodePrefab, genomeDataPanelNodes);
			nodesUI[i] = nodeUI;

			RectTransform rectTransform = nodeUI.GetComponent<RectTransform>();
			TextMeshProUGUI[] nodeTexts = nodeUI.GetComponentsInChildren<TextMeshProUGUI>();
			nodeTexts[0].text = node.Number+"";
			nodeTexts[1].text = node.Activation.ToString("F");
			nodesActivationTexts[i] = nodeTexts[1];

			if(node.Layer == 1) // Draw Nodes in the first layer.
			{
				float x = 10;
				float y = (CarBrain.InNodesNumber > 1) ? -inNodesOffsetY*inNodesOffsetMultiplier + inNodesOffsetY/2 : inNodesOffsetY/2;
				rectTransform.anchoredPosition = new Vector2(x, y);
				inNodesOffsetMultiplier++;
			}
			else if(node.Layer == brain.Genome.MaxNodeLayer) // Draw Nodes in the last layer.
			{
				float x = genomeDataPanelSize.x - 10;
				float y = (CarBrain.OutNodesNumber > 1) ? -outNodesOffsetY*outNodesOffsetMultiplier + outNodesOffsetY/2 : outNodesOffsetY/2;
				rectTransform.anchoredPosition = new Vector2(x, y);
				outNodesOffsetMultiplier++;
			}
			else // Draw Nodes in the hidden layers.
			{
				float x = hiddenNodeOffsetX/2 + hiddenNodeOffsetX*(node.Layer-1);
				float y;
				if(hiddenNodesNumberInLayers[node.Layer-2] > 1)
				{
					y = -hiddenNodesOffsetsY[node.Layer-2]*hiddenNodesOffsetMultipliers[node.Layer-2] + hiddenNodesOffsetsY[node.Layer-2]/2;
				}
				else
				{
					y = -hiddenNodesOffsetsY[node.Layer-2]/2;
				}

				rectTransform.anchoredPosition = new Vector2(x, y);
				hiddenNodesOffsetMultipliers[node.Layer-2]++; // Hidden layers are beginning from 2 layer so "[node.Layer-2]".
			}
		}

		return nodesUI;
	}

	private void DrawNodesConnections(CarBrain brain, GameObject[] nodesUI)
	{
		int nodesConnectionsNumber = brain.Genome.NodesConnectionsList.Count;

		for(int i = 0; i < nodesConnectionsNumber; i++)
		{
			NodesConnection connection = brain.Genome.NodesConnectionsList[i];
			GameObject connectionUI = GetGenomeDataUIElementFromPool(genomeDataNodesConnectionsQueue, genomeDataNodesConnectionPrefab,
				genomeDataPanelNodesConnections);

			RectTransform rectTransform = connectionUI.GetComponent<RectTransform>();
			TextMeshProUGUI connectionNumberText = connectionUI.GetComponentInChildren<TextMeshProUGUI>(true);
			connectionNumberText.text = connection.Weight.ToString("F");

			Vector2 startPoint = Vector2.zero;
			foreach(GameObject nodeUI in nodesUI)
			{
				TextMeshProUGUI nodeNumberText = nodeUI.GetComponentInChildren<TextMeshProUGUI>();
				if(nodeNumberText.text.Equals(connection.InNode.Number.ToString()))
				{
					startPoint = nodeUI.GetComponent<RectTransform>().anchoredPosition;
					break;
				}
			}

			Vector2 endPoint = Vector2.zero;
			foreach(GameObject nodeUI in nodesUI)
			{
				TextMeshProUGUI nodeNumberText = nodeUI.GetComponentInChildren<TextMeshProUGUI>();
				if(nodeNumberText.text.Equals(connection.OutNode.Number.ToString()))
				{
					endPoint = nodeUI.GetComponent<RectTransform>().anchoredPosition;
					break;
				}
			}

			// Position:
			Vector2 connectionUIPosition = (endPoint + startPoint)/2;
			rectTransform.anchoredPosition = connectionUIPosition;

			// Rotation:
			Vector2 distance = endPoint - startPoint;
			float z = Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg;
			rectTransform.rotation = Quaternion.AngleAxis(z, Vector3.forward);

			// Width:
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, distance.magnitude);

			// Color:
			float hsvColorPercent = (connection.Weight+1)/2 * 0.32f; // 0% (min) = red, 16% (mid) = yellow, 32% (max) = green.
			connectionUI.GetComponent<Image>().color = Color.HSVToRGB(hsvColorPercent, 1, 1);
		}
	}

	// For better optimization use pooling instead of creating and destroying UI gameObjects used to draw the Genome:
	private GameObject GetGenomeDataUIElementFromPool(Queue<GameObject> queue, GameObject prefab, Transform prefabParent)
	{
		GameObject uiElement;
		if(queue.Count > 0)
		{
			uiElement = queue.Dequeue();
			uiElement.SetActive(true);
		}
		else
		{
			uiElement = Instantiate(prefab, prefabParent);
		}
		return uiElement;
	}

	/// <summary>
	/// Reset the genomeDataPanel view. Disable the Nodes and NodesConnections UI elements and add them to the pool.
	/// </summary>
	public void ClearGenomePanel(bool unlockLastBrain)
	{
		// Deactivate NodesConnection weight, reset the panel colors and lastBrain data:
		NodesConnectionUIListener.DeactivateLastShowedInfo();
		genomeDataText.color = Color.white;
		genomeDataPanelImage.color = new Color(1, 1, 1, 0.39f);

		lastBrain = null;
		nodesActivationTexts = null;
		if(unlockLastBrain)
		{
			lastBrainIsLocked = false;
			genomeDataText.fontStyle = FontStyles.Normal;
			StopDrawBestGenomeFromSpeciesCoroutine();
		}

		// Add Nodes to the pool:
		genomeDataNodesQueue.Clear();
		int childrenNumberInPanel = genomeDataPanelNodes.childCount;

		for(int i = 0; i < childrenNumberInPanel; i++)
		{
			GameObject child = genomeDataPanelNodes.GetChild(i).gameObject;
			child.SetActive(false);
			genomeDataNodesQueue.Enqueue(child);
		}

		// Add NodesConnections to the pool:
		genomeDataNodesConnectionsQueue.Clear();
		childrenNumberInPanel = genomeDataPanelNodesConnections.childCount;

		for(int i = 0; i < childrenNumberInPanel; i++)
		{
			GameObject child = genomeDataPanelNodesConnections.GetChild(i).gameObject;
			child.SetActive(false);
			genomeDataNodesConnectionsQueue.Enqueue(child);
		}
	}

	/// <summary>
	/// Update Nodes activation values in genomeDataPanel and it's background color.
	/// </summary>
	private void UpdateGenomeData()
	{
		if(lastBrain != null)
		{
			for(int i = 0; i < nodesActivationTexts.Length; i++)
			{
				// Value:
				float activation = lastBrain.Genome.NodesList[i].Activation;
				nodesActivationTexts[i].text = activation.ToString("F");

				// Color (from red, through yellow, to green):
				float hsvColorPercent;

				// Input Nodes have range from 0 to infinity instead from -1 to 1.
				// So for their values: 0 = red & from 5 to infinity = green.
				if(i < CarBrain.InNodesNumber)
				{
					hsvColorPercent = (activation/5 * 0.32f);
					hsvColorPercent = (hsvColorPercent > 0.32f) ? 0.32f : hsvColorPercent; // Max (0.32f) is green.
				}
				else
				{
					// 0% (min) = red, 16% (mid) = yellow, 32% (max) = green. 32% = 115 in HSV:
					hsvColorPercent = ((activation+1)/2 * 0.32f);
				}

				nodesActivationTexts[i].transform.parent.GetComponent<Image>().color = Color.HSVToRGB(hsvColorPercent, 1, 1);
			}

			if(!lastBrain.CarIsCrashed)
			{
				genomeDataPanelImage.color = new Color(1, 1, 1, 0.39f);
			}
			else
			{
				genomeDataPanelImage.color = new Color(1, 0.5f, 0.5f, 0.39f);
			}
		}
	}
}
