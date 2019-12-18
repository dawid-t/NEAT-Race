using System.Collections.Generic;
using UnityEngine;

public class Genome
{
	private int biasNodeIndex, maxNodeLayer = 3;
	private List<Node> nodesList = new List<Node>();
	private List<NodesConnection> nodesConnectionsList = new List<NodesConnection>();


	public int BiasNodeIndex { get { return biasNodeIndex; } }
	public int MaxNodeLayer { get { return maxNodeLayer; } }
	public List<Node> NodesList { get { return nodesList; } }
	public List<NodesConnection> NodesConnectionsList { get { return nodesConnectionsList; } }


	/// <summary>
	/// Create default Genome with 2 layers (input and output layers).
	/// </summary>
	public Genome(int inNodesCount, int outNodesCount)
	{
		// Create Nodes:
		for(int i = 0; i < inNodesCount; i++)
		{
			nodesList.Add(new Node(i+1, 1)); // Input Nodes.
		}

		for(int i = 0; i < outNodesCount; i++)
		{
			nodesList.Add(new Node(i+inNodesCount+1, maxNodeLayer)); // Output Nodes.
		}

		biasNodeIndex = inNodesCount+outNodesCount;
		nodesList.Add(new Node(biasNodeIndex+1, 1)); // Bias Node.
		nodesList[biasNodeIndex].Activation = 1; // Bias always has an activation value of 1.

		// Create connections between created input and output Nodes (without the Bias Node):
		for(int i = 0; i < outNodesCount; i++)
		{
			for(int j = 0; j < inNodesCount; j++)
			{
				nodesConnectionsList.Add(new NodesConnection(true, nodesList[j], nodesList[inNodesCount+i]));
			}
		}

		// Create connections between created Bias Node and output Nodes:
		for(int i = inNodesCount; i < biasNodeIndex; i++)
		{
			NodesConnection connection = new NodesConnection(true, nodesList[biasNodeIndex], nodesList[i]);
			connection.Weight = 1; // On beginning set weight to 1 in the Bias connections.
			nodesConnectionsList.Add(connection);
		}
	}

	/// <summary>
	/// Add Node in hidden layer, between input and output layers. Can't add Node without at least 1 existing NodesConnection.
	/// </summary>
	public void AddNewNode()
	{
		if(nodesConnectionsList.Count == 0)
		{
			return;
		}

		// Get from random NodesConnection inNode and outNode. Don't disable the Bias connection:
		int randomConnectionIndex = Random.Range(0, nodesConnectionsList.Count);
		int numberOfRandomBiasConnections = 0;
		while(nodesConnectionsList[randomConnectionIndex].InNode == nodesList[biasNodeIndex]) // Draw again if is connected with the Bias Node.
		{
			randomConnectionIndex = Random.Range(0, nodesConnectionsList.Count);

			// After 50 draws cancel adding the new Node. We don't want to freeze the game / wait too long:
			numberOfRandomBiasConnections++;
			if(numberOfRandomBiasConnections >= 50)
			{
				return;
			}
		}

		Node inNode = nodesConnectionsList[randomConnectionIndex].InNode;
		Node outNode = nodesConnectionsList[randomConnectionIndex].OutNode;
		Node newHiddenNode = new Node(nodesList.Count+1, inNode.Layer+1);

		// If newHiddenNode's layer is equal to outNode's layer then shift all Nodes' layers greater or equal to newHiddenNode's layer:
		if(newHiddenNode.Layer == outNode.Layer)
		{
			for(int i = 0; i < nodesList.Count; i++)
			{
				Node node = nodesList[i];
				if(newHiddenNode.Layer <= node.Layer)
				{
					node.Layer++;
					if(maxNodeLayer < node.Layer)
					{
						maxNodeLayer = node.Layer;
					}
				}
			}
		}
		nodesList.Add(newHiddenNode);

		// Disable this random NodesConnection and add new 2 NodesConnections between newHiddenNode:
		nodesConnectionsList[randomConnectionIndex].Enabled = false;
		NodesConnection connection1 = new NodesConnection(true, inNode, newHiddenNode);
		NodesConnection connection2 = new NodesConnection(true, newHiddenNode, outNode);

		// First connection weight have to be 1. Second connection weight have to be equal to the weight of the disabled connection:
		connection1.Weight = 1;
		connection2.Weight = nodesConnectionsList[randomConnectionIndex].Weight;
		nodesConnectionsList.Add(connection1);
		nodesConnectionsList.Add(connection2);

		// Connect Bias Node to the newHiddenNode with 1 weight:
		NodesConnection newBiasConnection = new NodesConnection(true, nodesList[biasNodeIndex], newHiddenNode);
		newBiasConnection.Weight = 1;
		nodesConnectionsList.Add(newBiasConnection);
	}

	/// <summary>
	/// Add enabled NodesConnection between random Nodes.
	/// </summary>
	public void AddNewNodesConnection()
	{
		int[] nodesNumberInLayers = GetLayersCapacity();
		if(IsMaxConnectionsNumber(nodesNumberInLayers))
		{
			return;
		}

		// Get random index of Node. If Node has max out NodeConnections then check the next one.
		int randomNodeIndex = Random.Range(0, nodesList.Count);
		for(int i = randomNodeIndex; i < nodesList.Count; i++)
		{
			// Max output connections number for this Node:
			int maxOutConnectionsNumber = 0;
			for(int j = nodesList[i].Layer; j < maxNodeLayer; j++) // Minimum value of layer is 1.
			{
				maxOutConnectionsNumber += nodesNumberInLayers[j]; // Index 1 in this array is number of Nodes in layer 2, etc...
			}

			if(nodesList[i].OutConnectionsList.Count < maxOutConnectionsNumber)
			{
				// We got our inNode. Now find the outNode:
				Node inNode = nodesList[i];

				for(int j = 0; j < nodesList.Count; j++)
				{
					// Search outNode in the higher layer:
					if(inNode.Layer < nodesList[j].Layer)
					{
						// Check all connections with this outNode. If it is not connected with found inNode then it is our outNode:
						bool nodesAreConnected = false;
						for(int k = 0; k < nodesList[j].InConnectionsList.Count; k++)
						{
							if(nodesList[j].InConnectionsList[k].InNode == inNode)
							{
								nodesAreConnected = true;
							}
						}

						if(!nodesAreConnected)
						{
							Node outNode = nodesList[j];
							nodesConnectionsList.Add(new NodesConnection(true, inNode, outNode));
							return;
						}
					}
				}
			}
			else if(i == nodesList.Count-1) // Reset the iterator. Start search from the beginning of the list.
			{
				i = -1;
			}
		}
	}

	/// <summary>
	/// Get number of Nodes in each layer.
	/// </summary>
	private int[] GetLayersCapacity()
	{
		int[] nodesNumberInLayers = new int[maxNodeLayer];
		foreach(Node node in nodesList)
		{
			nodesNumberInLayers[node.Layer-1]++;
		}
		return nodesNumberInLayers;
	}

	private int GetMaxConnectionsNumber(int[] nodesNumberInLayers, int layer = 1)
	{
		// Minimum layer value is 1:
		if(layer < 1)
		{
			layer = 1;
		}

		// Get the max number of NodesConnections which can exists with present number of Nodes.
		// Multiply the number of Nodes in one layer with all Nodes in the next layers:
		int maxConnectionsNumber = 0;
		for(int i = layer-1; i < nodesNumberInLayers.Length-1; i++)
		{
			for(int j = 1+i; j < nodesNumberInLayers.Length; j++)
			{
				maxConnectionsNumber += nodesNumberInLayers[i]*nodesNumberInLayers[j]; // Index 0 in this array is number of Nodes in layer 1, etc...
			}
		}

		return maxConnectionsNumber;
	}

	private bool IsMaxConnectionsNumber(int[] nodesNumberInLayers)
	{
		int maxConnectionsNumber = GetMaxConnectionsNumber(nodesNumberInLayers);

		// Don't add NodesConnection if input / output number of Nodes is 0 or number of NodesConnections is reached maximum:
		if(nodesNumberInLayers[0] == 0 || nodesNumberInLayers[maxNodeLayer-1] == 0 || nodesConnectionsList.Count == maxConnectionsNumber)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Set data as in another Genome.
	/// </summary>
	public void CloneGenome(Genome betterGenome)
	{
		nodesList.Clear();
		nodesConnectionsList.Clear();

		biasNodeIndex = betterGenome.biasNodeIndex;
		maxNodeLayer = betterGenome.maxNodeLayer;

		foreach(Node betterNode in betterGenome.NodesList)
		{
			nodesList.Add(new Node(betterNode.Number, betterNode.Layer));
		}
		nodesList[biasNodeIndex].Activation = 1; // Bias always has an activation value of 1.

		foreach(NodesConnection betterConnection in betterGenome.NodesConnectionsList)
		{
			Node inNode = null;
			Node outNode = null;
			foreach(Node node in nodesList)
			{
				if(betterConnection.InNode.Number == node.Number)
				{
					inNode = node;
				}
				else if(betterConnection.OutNode.Number == node.Number)
				{
					outNode = node;
				}

				if(inNode != null && outNode != null)
				{
					break;
				}
			}

			NodesConnection connection = new NodesConnection(betterConnection.Enabled, inNode, outNode);
			connection.Weight = betterConnection.Weight;
			nodesConnectionsList.Add(connection);
		}
	}
}
