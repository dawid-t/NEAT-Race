using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CarsSound : MonoBehaviour
{
	private bool calculateSoundSettings = false;
	private float updatedVolume = 0, updatedPitch = 0;
	[SerializeField]
	private AudioSource engineAudioSource, crashAudioSource;
	[SerializeField]
	private CarMovement playerCarMovement;
	private CarMovement[] carsMovements;
	private Coroutine playSoundCoroutine;


	private void Awake()
	{
		PlayPlayerCarSound();
	}

	private void PlayPlayerCarSound()
	{
		carsMovements = new CarMovement[1];
		carsMovements[0] = playerCarMovement;
		if(!Settings.IsAudioMuted)
		{
			PlaySound();
		}
	}

	public void SetCarsMovements(GameObject[] cars)
	{
		carsMovements = new CarMovement[cars.Length+1];
		for(int i = 0; i < cars.Length; i++)
		{
			carsMovements[i] = cars[i].GetComponent<CarMovement>();
		}
		carsMovements[cars.Length] = playerCarMovement;
	}

	public void PlaySound()
	{
		if(carsMovements == null)
		{
			return;
		}

		engineAudioSource.Play();

		#if !UNITY_WEBGL
		calculateSoundSettings = true;
		new Thread(CalculateSoundSettings).Start();

		StopSoundCoroutine();
		playSoundCoroutine = StartCoroutine(SetSoundSettings());
		#else
		StopSoundCoroutine();
		playSoundCoroutine = StartCoroutine(CalculateAndSetSoundSettings());
		#endif
	}

	#region Multi-thread methods (not for WebGL):
	private void CalculateSoundSettings()
	{
		while(calculateSoundSettings)
		{
			int stoppedCarsNumber = 0;
			float maxCarSpeed = 0; // The highest speed of car in the existing cars group.
			foreach(CarMovement carMovement in carsMovements)
			{
				float carSpeed = Mathf.Abs(carMovement.Speed);
				if(carSpeed == 0)
				{
					stoppedCarsNumber++;
				}
				if(maxCarSpeed < carSpeed)
				{
					maxCarSpeed = carSpeed;
				}
			}

			updatedVolume = 0.6f - (float)stoppedCarsNumber/(carsMovements.Length*2);

			if(stoppedCarsNumber < carsMovements.Length)
			{
				float newPitch = maxCarSpeed*1.5f;
				updatedPitch = (newPitch > 0.5f) ? newPitch : 0.5f;
			}
			else
			{
				updatedPitch = 0;
			}

			Thread.Sleep(200);
		}
	}

	private IEnumerator SetSoundSettings()
	{
		while(true)
		{
			engineAudioSource.volume = updatedVolume;
			engineAudioSource.pitch = updatedPitch;

			yield return new WaitForSeconds(0.02f);
		}
	}
	#endregion Multi-thread methods (not for WebGL).

	#region Single-thread methods (for WebGL):
	private IEnumerator CalculateAndSetSoundSettings()
	{
		while(true)
		{
			int stoppedCarsNumber = 0;
			float maxCarSpeed = 0; // The highest speed of car in the existing cars group.
			foreach(CarMovement carMovement in carsMovements)
			{
				float carSpeed = Mathf.Abs(carMovement.Speed);
				if(carSpeed == 0)
				{
					stoppedCarsNumber++;
				}
				if(maxCarSpeed < carSpeed)
				{
					maxCarSpeed = carSpeed;
				}
			}

			engineAudioSource.volume = 0.6f - (float)stoppedCarsNumber/(carsMovements.Length*2);

			if(stoppedCarsNumber < carsMovements.Length)
			{
				float newPitch = maxCarSpeed*1.5f;
				engineAudioSource.pitch = (newPitch > 0.5f) ? newPitch : 0.5f;
			}
			else
			{
				engineAudioSource.pitch = 0;
			}

			yield return new WaitForSeconds(0.02f);
		}
	}
	#endregion Single-thread methods (for WebGL).

	private void StopSoundCoroutine()
	{
		if(playSoundCoroutine != null)
		{
			StopCoroutine(playSoundCoroutine);
			playSoundCoroutine = null;
		}
	}

	public void StopSound()
	{
		calculateSoundSettings = false;
		StopSoundCoroutine();

		engineAudioSource.Stop();
		engineAudioSource.volume = 0.6f;
		engineAudioSource.pitch = 1;
	}

	public void PlayCrashSound()
	{
		crashAudioSource.PlayOneShot(crashAudioSource.clip);
	}
}
