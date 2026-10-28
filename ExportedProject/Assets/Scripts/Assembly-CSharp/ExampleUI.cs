using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExampleUI : MonoBehaviour
{
	public Text text;

	public Button BackButton;

	private void Start()
	{
		BackButton.onClick.AddListener(OnBackClick);
	}

	private void Update()
	{
	}

	private void OnEnable()
	{
		Application.logMessageReceived += HandleLog;
	}

	private void OnDisable()
	{
		Application.logMessageReceived -= HandleLog;
	}

	private void OnBackClick()
	{
		SceneManager.LoadScene("_Examples_");
	}

	private void HandleLog(string condition, string stackTrace, LogType type)
	{
		text.text = text.text + condition + "\n";
	}
}
