using Opencoding.CommandHandlerSystem;
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
		}

		public void OnGUI()
		{
		}

		private void CreateSkin()
		{
		}

		private void WindowFunc(int id)
		{
		}
	}
}
