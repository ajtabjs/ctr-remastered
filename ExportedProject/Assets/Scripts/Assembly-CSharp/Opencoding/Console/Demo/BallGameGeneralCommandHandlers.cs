using System.Collections.Generic;
using Opencoding.CommandHandlerSystem;
using UnityEngine;

namespace Opencoding.Console.Demo
{
	internal static class BallGameGeneralCommandHandlers
	{
		[ConsoleCommand(Description = "This is a property that changes the strength of gravity")]
		private static float Gravity
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public static void Initialize()
		{
		}

		[ConsoleCommand(Description = "Change the colour of the background. Predefined named colours are available.")]
		private static void SetBackgroundColor(Color color)
		{
		}

		[ConsoleCommand(Description = "This command is an example of custom auto-completion. It doesn't do anything.")]
		private static void LoadLevel([Autocomplete(typeof(DemoController), "LevelAutocomplete")] string levelName)
		{
		}

		public static IEnumerable<string> LevelAutocomplete()
		{
			return null;
		}

		[ConsoleCommand]
		private static void ChangeColor([Autocomplete(typeof(BallGameGeneralCommandHandlers), "ColorAutoComplete")] string color)
		{
		}

		public static IEnumerable<string> ColorAutoComplete()
		{
			return null;
		}

		[ConsoleCommand]
		private static void ChangeText([Autocomplete(typeof(BallGameGeneralCommandHandlers), "TextAutoComplete")] string text)
		{
		}

		public static IEnumerable<string> TextAutoComplete()
		{
			return null;
		}
	}
}
