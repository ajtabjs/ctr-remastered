using System;
using System.Linq;
using Opencoding.CommandHandlerSystem;
using UnityEngine;

namespace Opencoding.Console.Demo
{
	internal class AdvancedCommandHandlerDemo : MonoBehaviour
	{
		private static int _value;

		private void Awake()
		{
			CommandHandlers.RegisterCommandHandlers(typeof(AdvancedCommandHandlerDemo));
			CommandHandlers.RegisterCommandHandler<string, int>(DynamicallyRegisteredMethod, null, null, (Type[])null, true);
			CommandHandlers.RegisterCommandHandler<int, string[]>(DynamicParamsMethod1, "DynamicParamsMethod1", null, (Type[])null, true);
			CommandHandlers.RegisterCommandHandler<Color, string[]>(DynamicParamsMethod2, "DynamicParamsMethod2", "Method that tests dynamic parameters with overridden types", new Type[]
			{
				typeof(int),
				typeof(string)
			}, true);
			CommandHandlers.RegisterCommandHandler<Color, string[]>(DynamicParamsMethod2, "DynamicParamsMethod3", "Method that tests dynamic parameters with overridden full parameter infos", new ParamInfo[]
			{
				new ParamInfo
				{
					Type = typeof(int),
					Name = "magicNumber"
				},
				new ParamInfo
				{
					Type = typeof(string),
					Name = "cake",
					IsOptional = true,
					DefaultValue = "victoria",
					AutoCompleteOptions = new string[]
					{
						"carrot",
						"coffee",
						"red velvet",
						"victoria"
					}
				}
			}, true);
			CommandHandlers.RegisterCommandHandler<int>(GetDynamicValue, SetDynamicValue, "DynamicValue", null, true);
			CommandHandlers.RegisterCommandHandler<object[]>(MultipleCommandHandler, "MultiCommand1", null, new Type[]
			{
				typeof(int),
				typeof(string)
			}, true);
			CommandHandlers.RegisterCommandHandler<object[]>(MultipleCommandHandler, "MultiCommand2", null, new Type[]
			{
				typeof(Color)
			}, true);
			CommandHandlers.RegisterCommandHandler<string>(delegate(string value)
			{
				Debug.Log("Call lambda command with value " + value);
			}, "LambdaCommand", null, null, true);
			CommandHandlers.DefaultCommandHandler = DefaultCommandHandler;
		}

		[ConsoleCommand]
		private static void ParamsMethodTest(int number, params string[] args)
		{
			Debug.Log("Dynamic method called with args: " + number + "  then " + string.Join(",", args));
		}

		private static void DynamicallyRegisteredMethod(string aString, int aNumber)
		{
			Debug.Log("DynamicallyRegisteredMethod called with " + aString + " and " + aNumber);
		}

		private static void DynamicParamsMethod1(int number, params string[] args)
		{
			Debug.Log("Dynamic params method called with args: " + number + "  then " + string.Join(",", args));
		}

		private static void DynamicParamsMethod2(Color color, params object[] args)
		{
			Debug.Log("Dynamic params method called with color " + color + " and args: " + string.Join(",", args.Select((object x) => x.ToString()).ToArray()));
		}

		private static int GetDynamicValue()
		{
			return _value;
		}

		private static void SetDynamicValue(int value)
		{
			_value = value;
		}

		private static void MultipleCommandHandler(params object[] args)
		{
			if (CommandHandlers.CurrentExecutingCommand == "MultiCommand1")
			{
				Debug.Log("MultiCommand1 executed with params " + string.Join(",", args.Select((object x) => x.ToString()).ToArray()));
			}
			else if (CommandHandlers.CurrentExecutingCommand == "MultiCommand2")
			{
				Debug.Log("MultiCommand2 executed with params " + string.Join(",", args.Select((object x) => x.ToString()).ToArray()));
			}
		}

		private bool DefaultCommandHandler(string commandName, string[] args)
		{
			bool flag = commandName == "ADefaultCommand";
			if (flag)
			{
				Debug.Log("Handling a default command with parameters " + string.Join(",", args));
			}
			return flag;
		}
	}
}
