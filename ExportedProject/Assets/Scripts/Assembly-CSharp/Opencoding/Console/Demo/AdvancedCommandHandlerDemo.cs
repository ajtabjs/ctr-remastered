using Opencoding.CommandHandlerSystem;
using UnityEngine;

namespace Opencoding.Console.Demo
{
	internal class AdvancedCommandHandlerDemo : MonoBehaviour
	{
		private static int _value;

		private void Awake()
		{
		}

		[ConsoleCommand]
		private static void ParamsMethodTest(int number, params string[] args)
		{
		}

		private static void DynamicallyRegisteredMethod(string aString, int aNumber)
		{
		}

		private static void DynamicParamsMethod1(int number, params string[] args)
		{
		}

		private static void DynamicParamsMethod2(Color color, params object[] args)
		{
		}

		private static int GetDynamicValue()
		{
			return 0;
		}

		private static void SetDynamicValue(int value)
		{
		}

		private static void MultipleCommandHandler(params object[] args)
		{
		}

		private bool DefaultCommandHandler(string commandName, string[] args)
		{
			return false;
		}
	}
}
