using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shows paths of scans of the sensor. Each car has 1 sensor which does multiple scans in the CarBrain class.
/// </summary>
public class SensorVisualizer : MonoBehaviour
{
	private static bool visualizeSensors = true;
	private static List<SensorVisualizer> sensorsVisualizersList;

	private bool sensorsCanBeShown;
	[SerializeField]
	private MeshRenderer vehicleMeshRenderer;
	private Material linesMaterial;
	private LineRenderer[] sensorVisualizers;


	public static bool VisualizeSensors {
		get { return visualizeSensors; }
		set
		{
			VisualizeSensorsChanged(visualizeSensors, value);
			visualizeSensors = value;
		} }

	public Material LinesMaterial { get { return linesMaterial; } set { linesMaterial = value; } }


	private void Awake()
	{
		sensorVisualizers = GetComponentsInChildren<LineRenderer>();
		linesMaterial = GetComponentInChildren<LineRenderer>().sharedMaterial;
		AddInstanceToList();
	}

	private void OnDestroy()
	{
		sensorsVisualizersList = null; // Destroy the list when scene is restarting ("Reset Population" button).
	}

	private void AddInstanceToList()
	{
		if(sensorsVisualizersList == null)
		{
			sensorsVisualizersList = new List<SensorVisualizer>();
		}
		sensorsVisualizersList.Add(this);
	}

	/// <summary>
	/// If sensors can be visualized ('SensorVisualizer.VisualizeSensors' is true) update their new material and enable/disable them.
	/// </summary>
	public void ShowSensors(bool enable)
	{
		sensorsCanBeShown = enable;
		if(visualizeSensors)
		{
			UpdateSensorsVisualization(enable);
		}
	}

	private void UpdateSensorsVisualization(bool enable)
	{
		foreach(LineRenderer visualizer in sensorVisualizers)
		{
			visualizer.sharedMaterial = linesMaterial;
			visualizer.enabled = (enable && sensorsCanBeShown);
		}
	}

	/// <summary>
	/// Update visualized/rendered scan's path of the sensor.
	/// </summary>
	public void SetVisualizationData(int sensorIndex, Vector3 sensorHitPoint)
	{
		if(visualizeSensors)
		{
			if(sensorIndex > sensorVisualizers.Length)
			{
				Debug.LogWarning("Index of sensor visualizers is out of range!");
				return;
			}

			sensorVisualizers[sensorIndex].SetPositions(new Vector3[] { transform.position, sensorHitPoint });
		}
	}

	private static void VisualizeSensorsChanged(bool oldValue, bool newValue)
	{
		if(oldValue != newValue && sensorsVisualizersList != null)
		{
			foreach(SensorVisualizer sensorVisualizer in sensorsVisualizersList)
			{
				sensorVisualizer.UpdateSensorsVisualization(newValue);
			}
		}
	}
}
