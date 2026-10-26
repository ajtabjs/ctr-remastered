using System.Collections.Generic;
using Opencoding.CommandHandlerSystem;
using UnityEngine;

namespace Opencoding.Console.Demo
{
	public class DemoController : MonoBehaviour
	{
		[SerializeField]
		private GameObject _sphere;

		private int _numberOfBallsCreated;

		private bool _isConsoleVisible;

		[ConsoleCommand(Description = "This command creates a sphere just above the ground")]
		private bool Test { get; set; }

		[ConsoleCommand(Description = "This is a property that changes the strength of gravity")]
		private float Gravity
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		private void Awake()
		{
		}

		private void Start()
		{
		}

		private void OnGUI()
		{
		}

		private void Update()
		{
		}

		private void OnDestroy()
		{
		}

		[ConsoleCommand(Description = "This command creates a sphere just above the ground")]
		private void CreateBall()
		{
		}

		[ConsoleCommand(Description = "You can enter Vectors by entering numbers separated by commas (e.g. 1,2,1)")]
		private void CreateBallAtPosition(Vector3 position)
		{
		}

		[ConsoleCommand(Description = "Change the colour of the sky. Predefined named colours are available.")]
		private void SetSkyColor(Color color)
		{
		}

		[ConsoleCommand(Description = "Throws an exception to test how the console handles displays it.")]
		private void ThrowException()
		{
		}

		[ConsoleCommand]
		private void LoremIpsum()
		{
		}

		[ConsoleCommand(Description = "This is a method that demonstrates that optional arguments work as you would expect")]
		private void OptionalArgumentExample(float c = 50f)
		{
		}

		[ConsoleCommand]
		private void LoadLevel([Autocomplete(typeof(DemoController), "LevelAutocomplete")] string levelName)
		{
		}

		public static IEnumerable<string> LevelAutocomplete()
		{
			return null;
		}
	}
}
