using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.EventSystems;

public class MenuController : MonoBehaviour
{
	private static bool sceneIsRestarting = false;
	private static MenuController instance;

	private bool firstRaceIsStarted = false, raceIsStopped = true;
	[SerializeField]
	private TextMeshProUGUI generationText, populationNumberText, generationTimeText, obstaclesText, startRaceButtonText;
	[SerializeField]
	private Slider populationNumberSlider, generationTimeSlider, obstaclesSlider;
	[SerializeField]
	private Button hideSensorsButton, showPlayerButton, startRaceButton, resetPopulationButton, graphicsButton, audioButton;
	[SerializeField]
	private Toggle wall1Toggle, wall2Toggle;
	[SerializeField]
	private Canvas evolutionInfoCanvas, resetPopulationCanvas, exitCanvas;
	private Animator evolutionInfoPanelAnimator, resetPopulationPanelAnimator, exitPanelAnimator;
	[SerializeField]
	private CarsController carsController;
	[SerializeField]
	private ObstaclesController obstaclesController;
	[SerializeField]
	private GameObject player;
	[SerializeField]
	private PostProcessProfile postProcessProfile;
	private Bloom bloom;


	public static MenuController Instance { get { return instance; } }

	public TextMeshProUGUI GenerationText { get { return generationText; } }


	private void Awake()
	{
		Init();
	}

	private void Update()
	{
		#if !UNITY_WEBGL
		if(Input.GetKeyDown(KeyCode.Escape) && !sceneIsRestarting)
		{
			ExitButtonClicked();
		}

		if(Input.GetKeyDown(KeyCode.Space) && !sceneIsRestarting)
		{
			StartRaceButtonClicked();
		}
		#else
		if((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space)) && !sceneIsRestarting)
		{
			StartRaceButtonClicked();
		}
		#endif
	}

	#if UNITY_EDITOR
	private void OnDestroy()
	{
		bloom.intensity.value = 3;
	}
	#endif

	private void Init()
	{
		instance = this;
		evolutionInfoPanelAnimator = evolutionInfoCanvas.GetComponentInChildren<Animator>();
		resetPopulationPanelAnimator = resetPopulationCanvas.GetComponentInChildren<Animator>();
		exitPanelAnimator = exitCanvas.GetComponentInChildren<Animator>();

		bloom = postProcessProfile.GetSetting<Bloom>();
		bloom.intensity.value = 3;

		#if UNITY_ANDROID
		showPlayerButton.interactable = false;
		#endif

		if(Settings.IsPotatoGraphics)
		{
			graphicsButton.GetComponentInChildren<TextMeshProUGUI>().text = "Potato";
		}
		else
		{
			graphicsButton.GetComponentInChildren<TextMeshProUGUI>().text = "Ultra";
		}

		if(sceneIsRestarting)
		{
			if(!Settings.IsAudioMuted)
			{
				audioButton.GetComponentInChildren<TextMeshProUGUI>().text = "On";
			}

			SensorVisualizer.VisualizeSensors = true;
			sceneIsRestarting = false;
			StartCoroutine(StartSceneEffectAfterPopulationReset());
		}
	}

	public static bool IsPointerOverUI()
	{
		#if !UNITY_ANDROID || UNITY_EDITOR
		if(EventSystem.current.IsPointerOverGameObject())
		{
			return true;
		}
		#else
		if(Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began &&
			EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId))
		{
			return true;
		}
		#endif
		else
		{
			return false;
		}
	}

	public void ShowEvolutionInfo(bool enable)
	{
		evolutionInfoCanvas.enabled = enable;
		evolutionInfoPanelAnimator.Play("WindowPopUp", 0, 0);
	}

#region UI events:
	public void PopulationNumberSliderValueChanged()
	{
		int value = (int)populationNumberSlider.value;
		carsController.PopulationNumber = value;
		populationNumberText.text = "Population number: " + value;
	}

	public void GenerationTimeSliderValueChanged()
	{
		int value = (int)generationTimeSlider.value;
		carsController.GenerationLifeTime = value;
		generationTimeText.text = "Generation time: " + value + "s";
	}

	public void ObstaclesSliderValueChanged()
	{
		int value = (int)obstaclesSlider.value;
		obstaclesController.ChangeObstaclesNumber(value);
		obstaclesText.text = "Obstacles: " + value;
	}

	public void WallToggleChanged(bool isFirstWall)
	{
		obstaclesController.ChangeWallActivationStatus(isFirstWall);
	}

	public void HideSensorsButtonClicked()
	{
		SensorVisualizer.VisualizeSensors = !SensorVisualizer.VisualizeSensors;
		if(SensorVisualizer.VisualizeSensors)
		{
			hideSensorsButton.GetComponentInChildren<TextMeshProUGUI>().text = "Hide Sensors";
		}
		else
		{
			hideSensorsButton.GetComponentInChildren<TextMeshProUGUI>().text = "Show Sensors";
		}
	}

	public void ShowPlayerButtonClicked()
	{
		player.SetActive(!player.activeInHierarchy);
		if(!player.activeInHierarchy)
		{
			showPlayerButton.GetComponentInChildren<TextMeshProUGUI>().text = "Show Player";
		}
		else
		{
			showPlayerButton.GetComponentInChildren<TextMeshProUGUI>().text = "Hide Player";
		}
	}

	public void StartRaceButtonClicked()
	{
		if(raceIsStopped)
		{
			if(!firstRaceIsStarted)
			{
				populationNumberSlider.interactable = false;
				Image[] sliderChildrenImages = populationNumberSlider.GetComponentsInChildren<Image>();
				sliderChildrenImages[0].color = new Color32(0xAB, 0xAB, 0xAB, 0xff);
				sliderChildrenImages[1].color = new Color32(0x7F, 0x20, 0x20, 0xff);

				hideSensorsButton.interactable = true;
				resetPopulationButton.interactable = true;

				carsController.StartFirstRace();
				firstRaceIsStarted = true;
			}

			Settings.SetPause(false);
			startRaceButtonText.text = "Pause Race";
			raceIsStopped = false;
		}
		else
		{
			Settings.SetPause(true);
			startRaceButtonText.text = "Resume Race";
			raceIsStopped = true;
		}
	}

	public void ResetPopulationButtonClicked()
	{
		if(!resetPopulationCanvas.enabled)
		{
			ExitCancelButtonClicked();
			resetPopulationCanvas.enabled = true;
			resetPopulationPanelAnimator.Play("WindowPopUp", 0, 0);
		}
		else
		{
			ResetPopulationCancelButtonClicked();
		}
	}

	public void ResetPopulationConfirmButtonClicked()
	{
		if(!sceneIsRestarting)
		{
			StartRaceButtonClicked();
			ResetPopulationCancelButtonClicked();
			startRaceButton.interactable = false;
			resetPopulationButton.interactable = false;
			ShowEvolutionInfo(false);
			sceneIsRestarting = true;

			NodesConnection.ResetStaticValues();
			CarsController.ResetStaticValues();
			Species.ResetStaticValues();

			SensorVisualizer.VisualizeSensors = false;
			StartCoroutine(carsController.StartDissolveEffectOnSceneRestarting());
			StartCoroutine(EndSceneEffectBeforePopulationReset());
		}
	}

	public void ResetPopulationCancelButtonClicked()
	{
		resetPopulationCanvas.enabled = false;
	}

	public void ExitButtonClicked()
	{
		if(resetPopulationCanvas.enabled)
		{
			ResetPopulationCancelButtonClicked();
		}
		else if(!exitCanvas.enabled)
		{
			exitCanvas.enabled = true;
			exitPanelAnimator.Play("WindowPopUp", 0, 0);
		}
		else
		{
			ExitCancelButtonClicked();
		}
	}

	public void ExitConfirmButtonClicked()
	{
		Settings.SetPause(true);
		Application.Quit();
	}

	public void ExitCancelButtonClicked()
	{
		exitCanvas.enabled = false;
	}

	public void ChangeGraphics()
	{
		Settings.ChangeGraphics();
		if(Settings.IsPotatoGraphics)
		{
			graphicsButton.GetComponentInChildren<TextMeshProUGUI>().text = "Potato";
		}
		else
		{
			graphicsButton.GetComponentInChildren<TextMeshProUGUI>().text = "Ultra";
		}
	}

	public void MuteAudio()
	{
		Settings.MuteAudio();
		if(Settings.IsAudioMuted)
		{
			audioButton.GetComponentInChildren<TextMeshProUGUI>().text = "Off";
		}
		else
		{
			audioButton.GetComponentInChildren<TextMeshProUGUI>().text = "On";
		}
	}
#endregion UI events.

	private IEnumerator EndSceneEffectBeforePopulationReset()
	{
		bloom.active = true; // Turn on bloom effect (when is set "potato" graphics then bloom is disabled, so enable it for a while).
		CarsSound carsSound = carsController.GetComponent<CarsSound>();
		carsSound.StopSound();

		for(int i = 4; i <= 60; i++)
		{
			bloom.intensity.value = i;
			yield return new WaitForSecondsRealtime(0.02f);
		}

		// Reset scene:
		Settings.SetPause(false);
		carsSound.StopSound(); // Stop again the engine sound because above method unmuted the audio.
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	private IEnumerator StartSceneEffectAfterPopulationReset()
	{
		bloom.active = true; // Turn on bloom effect (when is set "potato" graphics then bloom is disabled, so enable it for a while).

		for(int i = 59; i >= 3; i--)
		{
			bloom.intensity.value = i;
			yield return new WaitForSecondsRealtime(0.02f);
		}

		// Turn off bloom if is set "potato" graphics.
		if(Settings.IsPotatoGraphics)
		{
			bloom.active = false;
		}
	}
}
