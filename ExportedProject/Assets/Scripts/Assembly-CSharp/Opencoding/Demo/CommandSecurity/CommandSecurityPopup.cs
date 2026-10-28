using Opencoding.CommandHandlerSystem;
using Opencoding.Shared.Utils;
using UnityEngine;

namespace Opencoding.Demo.CommandSecurity
{
	internal class CommandSecurityPopup
	{
		private readonly ConsoleCommandSecurity _consoleCommandSecurity;

		private readonly CommandHandler _commandToExecute;

		private readonly string[] _parameters;

		private string _enteredPassword;

		private float _incorrectPasswordEnteredTimeout;

		private GUISkin _skin;

		private float _screenScaleFactor;

		public CommandSecurityPopup(ConsoleCommandSecurity consoleCommandSecurity, CommandHandler commandToExecute, string[] parameters)
		{
			_enteredPassword = string.Empty;
			_consoleCommandSecurity = consoleCommandSecurity;
			_commandToExecute = commandToExecute;
			_parameters = parameters;
		}

		public void OnGUI()
		{
			if (_skin == null)
			{
				CreateSkin();
			}
			float num = ((Screen.dpi == 0f) ? 140f : Screen.dpi) / 140f;
			int num2 = (int)Mathf.Min(_screenScaleFactor * 300f, Screen.width);
			int num3 = (int)(num * 140f);
			GUI.ModalWindow(typeof(CommandSecurityPopup).GetHashCode(), new Rect(Screen.width / 2 - num2 / 2, Screen.height / 2 - num3 / 2, num2, num3), WindowFunc, "", _skin);
		}

		private void CreateSkin()
		{
			float num = ((Screen.dpi == 0f) ? 140f : Screen.dpi) / 140f;
			_screenScaleFactor = num;
			_skin = Object.Instantiate(GUI.skin);
			GUIStyle gUIStyle = new GUIStyle(GUI.skin.window);
			gUIStyle.normal.background = UIUtilities.CreateTexture(new Color(0.2f, 0.2f, 0.2f, 1f));
			gUIStyle.padding = new RectOffset((int)(10f * _screenScaleFactor), (int)(10f * _screenScaleFactor), (int)(_screenScaleFactor * 20f), (int)(10f * _screenScaleFactor));
			gUIStyle.fontSize = (int)(_screenScaleFactor * 13f);
			_skin.window = gUIStyle;
			int fontSize = (int)(num * 13f);
			_skin.label.fontSize = fontSize;
			_skin.button.fontSize = fontSize;
			_skin.textField.fontSize = fontSize;
		}

		private void WindowFunc(int id)
		{
			GUISkin skin = GUI.skin;
			GUI.skin = _skin;
			GUILayout.Label("This command requires a password to execute.");
			Color color = GUI.color;
			GUI.color = Color.red;
			GUILayout.Label((_incorrectPasswordEnteredTimeout <= Time.realtimeSinceStartup) ? " " : "Incorrect password entered, try again.");
			GUI.color = color;
			GUILayout.BeginHorizontal();
			GUILayout.Label("Password:", GUILayout.Width(_screenScaleFactor * 100f));
			_enteredPassword = GUI.PasswordField(GUILayoutUtility.GetRect(100f * _screenScaleFactor, 10000f * _screenScaleFactor, 0f, _screenScaleFactor * 20f, GUI.skin.textField), _enteredPassword, '*');
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Cancel"))
			{
				_consoleCommandSecurity._commandSecurityPopup = null;
				_consoleCommandSecurity._authenticated = false;
			}
			if (GUILayout.Button("Login"))
			{
				if (_enteredPassword == "password")
				{
					Debug.Log("Logged in using password");
					_consoleCommandSecurity._commandSecurityPopup = null;
					_consoleCommandSecurity._authenticated = true;
					if (_consoleCommandSecurity._SavePassword)
					{
						PlayerPrefs.SetString("Opencoding.Console.ConsoleCommandSecurity.Password", _enteredPassword);
						PlayerPrefs.Save();
					}
					_commandToExecute.Invoke(_parameters);
				}
				else
				{
					_incorrectPasswordEnteredTimeout = Time.realtimeSinceStartup + 3f;
				}
			}
			GUILayout.EndHorizontal();
			GUI.skin = skin;
		}
	}
}
