using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclesController : MonoBehaviour
{
	private int lastObstaclesNumber = 0;
	[SerializeField]
	private GameObject wall1, wall2;
	[SerializeField]
	private GameObject[] obstacles, wheelsOnWall1, wheelsOnWall2;


	private void Start()
	{
		DisableStaticObstacles();
	}

	/// <summary>
	/// If you want decrease the amount of batches with static GameObjects then those GameObjects has to be active on scene start.<para/>
	/// Disable them after start in the Start() method because we don't want them on the beginning.
	/// </summary>
	private void DisableStaticObstacles()
	{
		lastObstaclesNumber = obstacles.Length;
		ChangeObstaclesNumber(0);
		ChangeWallActivationStatus(true);
		ChangeWallActivationStatus(false);
	}

	public void ChangeObstaclesNumber(int obstaclesNumber)
	{
		if(lastObstaclesNumber < obstaclesNumber)
		{
			for(int i = lastObstaclesNumber; i < obstaclesNumber; i++)
			{
				obstacles[i].SetActive(true);
			}
		}
		else if(lastObstaclesNumber > obstaclesNumber)
		{
			for(int i = obstaclesNumber; i < lastObstaclesNumber; i++)
			{
				obstacles[i].SetActive(false);
			}
		}
		lastObstaclesNumber = obstaclesNumber;
	}

	public void ChangeWallActivationStatus(bool isFirstWall)
	{
		if(isFirstWall)
		{
			wall1.SetActive(!wall1.activeInHierarchy);
			for(int i = 0; i < wheelsOnWall1.Length; i++)
			{
				wheelsOnWall1[i].SetActive(!wheelsOnWall1[i].activeInHierarchy);
			}
		}
		else
		{
			wall2.SetActive(!wall2.activeInHierarchy);
			for(int i = 0; i < wheelsOnWall2.Length; i++)
			{
				wheelsOnWall2[i].SetActive(!wheelsOnWall2[i].activeInHierarchy);
			}
		}
	}
}
