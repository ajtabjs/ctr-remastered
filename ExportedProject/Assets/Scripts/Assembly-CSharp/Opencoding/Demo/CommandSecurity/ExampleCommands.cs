using Opencoding.CommandHandlerSystem;
using UnityEngine;

namespace Opencoding.Demo.CommandSecurity
{
	public class ExampleCommands : MonoBehaviour
	{
		private void Awake()
		{
		}

		[ConsoleCommand(Description = "This is a secured command")]
		[SecureCommandHandler]
		private void SecureCommand()
		{
		}

		[ConsoleCommand(Description = "This is an insecure command - anyone can execute it")]
		private void InsecureCommand()
		{
		}

		private void OnGUI()
		{
		}
	}
}
