using Opencoding.CommandHandlerSystem;
using UnityEngine;

namespace Opencoding.Demo.CommandSecurity
{
	public class ConsoleCommandSecurity : MonoBehaviour
	{
		private bool _authenticated;

		private CommandSecurityPopup _commandSecurityPopup;

		public const string PASSWORD = "password";

		public bool _SavePassword;

		private void Awake()
		{
		}

		private void OnGUI()
		{
		}

		private bool BeforeCommandExecuted(CommandHandler commandHandler, string[] strings)
		{
			return false;
		}

		public void PopupClosed(bool authenticated)
		{
		}
	}
}
