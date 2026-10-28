using Opencoding.CommandHandlerSystem;
using Opencoding.Console;
using UnityEngine;

namespace Opencoding.Demo.CommandSecurity
{
	public class ExampleCommands : MonoBehaviour
	{
		private void Awake()
		{
			CommandHandlers.RegisterCommandHandlers(this);
		}

		[ConsoleCommand(Description = "This is a secured command")]
		[SecureCommandHandler]
		private void SecureCommand()
		{
			Debug.Log("This is a secure command");
		}

		[ConsoleCommand(Description = "This is an insecure command - anyone can execute it")]
		private void InsecureCommand()
		{
			Debug.Log("This is an insecure command - you don't need a password to execute it");
		}

		private void OnGUI()
		{
			GUILayout.BeginArea(new Rect(10f, 10f, Screen.width - 20, Screen.height - 20));
			GUIStyle style = new GUIStyle(GUI.skin.label)
			{
				alignment = TextAnchor.MiddleCenter,
				fontSize = ((Screen.dpi > 0f) ? ((int)(Screen.dpi * 0.1f)) : 20),
				wordWrap = true
			};
			GUIStyleState gUIStyleState = new GUIStyleState();
			gUIStyleState.textColor = Color.black;
			style.normal = gUIStyleState;
			if (DebugConsole.Instance != null && !DebugConsole.IsVisible)
			{
				GUILayout.Label("This scene provides a demo of how you can implement a very basic security system so users can't execute every command. This is useful for public betas etc.\n\nThe default password is 'password'.", style, GUILayout.ExpandWidth(true));
				GUILayout.FlexibleSpace();
				GUILayout.Label("To open the console, press the 'tilde' key (~).", style, GUILayout.ExpandWidth(true));
			}
			GUILayout.EndArea();
		}
	}
}
