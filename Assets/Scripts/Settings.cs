using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// Game settings. Object of this class is invoking first in the scene (except TextMeshPro).
/// </summary>
public class Settings : MonoBehaviour
{
	private static bool isFirstStart = true, isPaused = false, isPotatoGraphics = false, isAudioMuted = true;
	private static Settings instance;

	[SerializeField]
	private PostProcessLayer postProcessLayer;
	[SerializeField]
	private PostProcessProfile postProcessProfile;
	[SerializeField]
	private RenderPipelineAsset lowQualityPipeline, highQualityPipeline, androidHighQualityPipeline;
	[SerializeField]
	private Light directionalLight;
	[SerializeField]
	private CarsSound carsSound;
	[SerializeField]
	private GameObject playerCar;
	private GameObject[] aiCars;


	public static bool IsPaused { get { return isPaused; } }
	public static bool IsPotatoGraphics { get { return isPotatoGraphics; } }
	public static bool IsAudioMuted { get { return isAudioMuted; } }
	public static Settings Instance { get { return instance; } }


	private void Awake()
	{
		instance = this;

		if(isFirstStart)
		{
			#if UNITY_ANDROID
			isPotatoGraphics = true;
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
			#endif

			isFirstStart = false;
		}

		#if UNITY_ANDROID && !UNITY_EDITOR
		directionalLight.intensity = 1.5f;
		instance.postProcessProfile.GetSetting<Bloom>().fastMode.value = true;
		instance.postProcessLayer.subpixelMorphologicalAntialiasing.quality = SubpixelMorphologicalAntialiasing.Quality.Low;
		#endif

		if(isPotatoGraphics)
		{
			ChangeGraphics(true);
		}
		if(isAudioMuted)
		{
			MuteAudio(true);
		}
	}

	/// <summary>
	/// Pause or unpause the game.
	/// </summary>
	public static void SetPause()
	{
		isPaused = !isPaused;
		SetPause(isPaused);
	}

	/// <summary>
	/// Pause or unpause the game.
	/// </summary>
	public static void SetPause(bool pauseGame)
	{
		isPaused = pauseGame;
		Time.timeScale = (isPaused) ? 0 : 1;

		// Audio settings:
		bool isAudioMutedTmp = isAudioMuted; // Save 'isAudioMuted' current value.
		bool muteAudio = (pauseGame) ? true : isAudioMuted; // If game is paused mute the audio.
															// Otherwise if game is resumed and 'isAudioMuted' is false unmute the audio.
		MuteAudio(muteAudio); // This method can changes the 'isAudioMuted' value, but we don't want it.
		isAudioMuted = isAudioMutedTmp; // Set old 'isAudioMuted' value.
	}

	/// <summary>
	/// Change the game graphics (potato or ultra).
	/// </summary>
	public static void ChangeGraphics()
	{
		isPotatoGraphics = !isPotatoGraphics;
		ChangeGraphics(isPotatoGraphics);
	}

	/// <summary>
	/// Change the game graphics (potato or ultra).
	/// </summary>
	private static void ChangeGraphics(bool setPotatoGraphics)
	{
		isPotatoGraphics = setPotatoGraphics;
		if(isPotatoGraphics)
		{
			instance.postProcessLayer.antialiasingMode = PostProcessLayer.Antialiasing.None;
			GraphicsSettings.renderPipelineAsset = instance.lowQualityPipeline;
		}
		else
		{
			instance.postProcessLayer.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;

			#if !UNITY_ANDROID
			GraphicsSettings.renderPipelineAsset = instance.highQualityPipeline;
			#else
			GraphicsSettings.renderPipelineAsset = instance.androidHighQualityPipeline;
			#endif
		}

		instance.postProcessProfile.GetSetting<Bloom>().active = !isPotatoGraphics;
		ChangeCarsGraphics();
		CarsSelector.Instance?.ReselectCars();
	}

	public static void ChangeCarsGraphics()
	{
		MeshRenderer[] playerMeshRenderers = instance.playerCar.GetComponentsInChildren<MeshRenderer>(true);
		playerMeshRenderers[0].gameObject.SetActive(!isPotatoGraphics);
		playerMeshRenderers[1].gameObject.SetActive(isPotatoGraphics);

		if(instance.aiCars != null)
		{
			foreach(GameObject car in instance.aiCars)
			{
				MeshRenderer[] aiMeshRenderers = car.GetComponentsInChildren<MeshRenderer>(true);
				aiMeshRenderers[0].gameObject.SetActive(!isPotatoGraphics);
				aiMeshRenderers[1].gameObject.SetActive(isPotatoGraphics);
			}
		}
	}

	public static void SetAICars(GameObject[] cars)
	{
		instance.aiCars = cars;
	}

	/// <summary>
	/// Mute or unmute the main audio.
	/// </summary>
	public static void MuteAudio()
	{
		isAudioMuted = !isAudioMuted;
		MuteAudio(isAudioMuted);
	}

	/// <summary>
	/// Mute or unmute the main audio.
	/// </summary>
	public static void MuteAudio(bool mute)
	{
		isAudioMuted = mute;

		if(isPaused) // Mute if game is paused.
		{
			mute = true;
		}

		if(mute)
		{
			// Add here background sound.
			instance.carsSound.StopSound();
		}
		else
		{
			// Add here background sound.
			instance.carsSound.PlaySound();
		}

		AudioSource[] audioSources = instance.carsSound.GetComponents<AudioSource>();
		foreach(AudioSource audioSource in audioSources)
		{
			audioSource.mute = mute;
		}
	}
}
