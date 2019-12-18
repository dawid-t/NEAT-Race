using System.Collections.Generic;

public class Node
{
	private int number, layer;
	private float activation;
	private List<NodesConnection> inConnectionsList = new List<NodesConnection>(); // NodesConnections from left side of this Node.
	private List<NodesConnection> outConnectionsList = new List<NodesConnection>(); // NodesConnections from right side of this Node.


	public int Number { get { return number; } set { number = value; } }
	public int Layer { get { return layer; } set { layer = value; } }
	public float Activation { get { return activation; } set { activation = value; } }
	public List<NodesConnection> InConnectionsList { get { return inConnectionsList; } }
	public List<NodesConnection> OutConnectionsList { get { return outConnectionsList; } }


	/// <summary>
	/// Create node / neuron.
	/// </summary>
	public Node(int number, int layer)
	{
		this.number = number;
		this.layer = layer;
		activation = 0;
	}
}
