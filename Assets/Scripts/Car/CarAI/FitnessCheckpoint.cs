using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// FitnessCheckpoint is for calculate cars fitness.
/// </summary>
public class FitnessCheckpoint : MonoBehaviour
{
	private static int lastCheckpointIndex = 0;
	private static List<FitnessCheckpoint> fitnessCheckpointsList;

	[SerializeField]
	private int checkpointIndex = 0;
	private Material material;
	private Color originalColor;
	private Animator animator;
	private ParticleSystem.MainModule particleMain;


	public int CheckpointIndex { get { return checkpointIndex; } }
	public static int LastCheckpointIndex { get { return lastCheckpointIndex; } }


	private void Awake()
	{
		Init();
		SetCheckpointLastIndex();
		AddInstanceToList();
	}

	private void OnDestroy()
	{
		fitnessCheckpointsList = null; // Destroy the list when scene is restarting ("Reset Population" button).
	}

	private void Init()
	{
		MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
		material = new Material(meshRenderer.sharedMaterial); // Create a copy of the material.
		meshRenderer.sharedMaterial = material;

		originalColor = material.GetColor("_BaseColor");
		animator = GetComponentInChildren<Animator>();
		particleMain = GetComponentInChildren<ParticleSystem>().main;
	}

	private void SetCheckpointLastIndex()
	{
		if(lastCheckpointIndex < checkpointIndex)
		{
			lastCheckpointIndex = checkpointIndex;
		}
	}

	private void AddInstanceToList()
	{
		if(fitnessCheckpointsList == null)
		{
			fitnessCheckpointsList = new List<FitnessCheckpoint>();
		}
		fitnessCheckpointsList.Add(this);
	}

	public void ReactToTouchedCar(Color color)
	{
		material.SetColor("_BaseColor", color);
		particleMain.startColor = new ParticleSystem.MinMaxGradient(new Color(color.r, color.g, color.b, 0.5f));
		PlayTriggerAnimation();
	}

	public void PlayTriggerAnimation()
	{
		animator.Play("CheckpointTrigger", 0, 0);
	}

	public static void ResetColors()
	{
		foreach(FitnessCheckpoint fitnessCheckpoint in fitnessCheckpointsList)
		{
			fitnessCheckpoint.ResetColor();
		}
	}

	private void ResetColor()
	{
		material.SetColor("_BaseColor", originalColor);
		particleMain.startColor = new ParticleSystem.MinMaxGradient(new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f));
	}
}
