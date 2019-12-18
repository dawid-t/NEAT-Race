using System.Collections.Generic;
using UnityEngine;


public class NodesConnection
{
	private static List<InnovationData> createdInnovationsList = new List<InnovationData>(); // History of created unique NodesConnections.

	private bool enabled;
	private int innovation;
	private float weight;
	private Node inNode, outNode;


	public bool Enabled { get { return enabled; } set { enabled = value; } }
	public int Innovation { get { return innovation; } }
	public float Weight { get { return weight; } set { weight = value; } }
	public Node InNode { get { return inNode; } }
	public Node OutNode { get { return outNode; } }


	private struct InnovationData
	{
		private int innovation, inNodeNumber, outNodeNumber;


		public int Innovation { get { return innovation; } }
		public int InNodeNumber { get { return inNodeNumber; } }
		public int OutNodeNumber { get { return outNodeNumber; } }


		public InnovationData(int innovation, int inNodeNumber, int outNodeNumber)
		{
			this.innovation = innovation;
			this.inNodeNumber = inNodeNumber;
			this.outNodeNumber = outNodeNumber;
		}
	}


	/// <summary>
	/// Create connection between 2 Nodes. NodesConnection's weight is random between -1 and 1.
	/// </summary>
	public NodesConnection(bool enabled, Node inNode, Node outNode)
	{
		this.enabled = enabled;
		this.inNode = inNode;
		this.outNode = outNode;
		weight = Random.Range(-1f, 1f);

		// Set innovation. Find the old one with same data if exists, otherwise create a new one:
		bool innovationExists = false;
		foreach(InnovationData innovationData in createdInnovationsList)
		{
			if(inNode.Number == innovationData.InNodeNumber && outNode.Number == innovationData.OutNodeNumber)
			{
				innovation = innovationData.Innovation;
				innovationExists = true;
				break;
			}
		}

		if(!innovationExists)
		{
			innovation = createdInnovationsList.Count+1;
			createdInnovationsList.Add(new InnovationData(innovation, inNode.Number, outNode.Number));
		}

		AddThisConnectionToNodeLists();
	}

	/// <summary>
	/// Add to appropriate list in inNode and outNode this NodesConnection.
	/// </summary>
	private void AddThisConnectionToNodeLists()
	{
		inNode.OutConnectionsList.Add(this); // InNode is on left side from this connection, so it is a outConnection for this Node.
		outNode.InConnectionsList.Add(this); // OutNode is on right side from this connection, so it is a inConnection for this Node.
	}

	/// <summary>
	/// When Genome is cloned from the best one, update the inNode and outNode reference to cloned Nodes and their NodesConnections lists.
	/// </summary>
	public void UpdateAfterCloning(List<Node> nodesList)
	{
		bool inNodeFound = false;
		bool outNodeFound = false;
		foreach(Node node in nodesList)
		{
			if(inNode.Number == node.Number)
			{
				inNode = node;
				inNodeFound = true;
			}
			else if(outNode.Number == node.Number)
			{
				outNode = node;
				outNodeFound = true;
			}

			if(inNodeFound && outNodeFound)
			{
				break;
			}
		}

		AddThisConnectionToNodeLists();
	}

	public static void ResetStaticValues()
	{
		createdInnovationsList = new List<InnovationData>();
	}
}
