using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
	private TextMeshProUGUI fpsCounterText;
	private float deltaTime = 0.0f;


	private void Awake()
	{
		fpsCounterText = GetComponent<TextMeshProUGUI>();
	}

	private void Update()
	{
		if(!Settings.IsPaused)
		{
			deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
			float fps = 1.0f / deltaTime;
			fpsCounterText.text = "FPS: "+Mathf.Ceil(fps);
		}
	}
}
