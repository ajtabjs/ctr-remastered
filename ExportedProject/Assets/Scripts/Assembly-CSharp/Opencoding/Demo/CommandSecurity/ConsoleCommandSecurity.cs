using Opencoding.CommandHandlerSystem;
using UnityEngine;

namespace Opencoding.Demo.CommandSecurity
{
	public class ConsoleCommandSecurity : MonoBehaviour
	{
		internal bool _authenticated;

		internal CommandSecurityPopup _commandSecurityPopup;

		public const string PASSWORD = "password";

		public bool _SavePassword;

		private void Awake()
		{
			CommandHandlers.BeforeCommandExecutedHook += BeforeCommandExecuted;
			Object.DontDestroyOnLoad(gameObject);
			if (_SavePassword && PlayerPrefs.GetString("Opencoding.Console.ConsoleCommandSecurity.Password") == "password")
			{
				_authenticated = true;
			}
		}

		private void OnGUI()
		{
			if (_commandSecurityPopup != null)
			{
				_commandSecurityPopup.OnGUI();
			}
		}

		private bool BeforeCommandExecuted(CommandHandler commandHandler, string[] strings)
		{
			if (_authenticated)
			{
				return true;
			}
			if (commandHandler.MemberInfo.GetCustomAttributes(typeof(SecureCommandHandlerAttribute), false).Length != 0)
			{
				if (_commandSecurityPopup != null)
				{
					return false;
				}
				_commandSecurityPopup = new CommandSecurityPopup(this, commandHandler, strings);
				return false;
			}
			return true;
		}

		public void PopupClosed(bool authenticated)
		{
			_commandSecurityPopup = null;
			_authenticated = authenticated;
		}
	}
}
