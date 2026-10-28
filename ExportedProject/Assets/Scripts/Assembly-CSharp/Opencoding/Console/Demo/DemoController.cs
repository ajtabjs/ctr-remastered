using System.Collections.Generic;
using System.IO;
using Opencoding.CommandHandlerSystem;
using Opencoding.Shared.Utils;
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
				return Physics.gravity.y;
			}
			set
			{
				Physics.gravity = new Vector3(0f, value, 0f);
			}
		}

		private void Awake()
		{
			CommandHandlers.RegisterCommandHandlers(this);
			DebugConsole.SaveFileProvider += () => new SaveFileData[]
			{
				new SaveFileData("save.json", "{\"test\": 42}", SaveFileDataType.JSON),
				new SaveFileData("info.txt", "This is some plain-text data", SaveFileDataType.TEXT)
			};
			DebugConsole.GameInfoProvider += () => new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("Number of balls created", _numberOfBallsCreated.ToString()),
				new KeyValuePair<string, string>("Sky color", Camera.main.backgroundColor.ToString()),
				new KeyValuePair<string, string>("Gravity", Physics.gravity.ToString())
			};
		}

		private void Start()
		{
			DebugConsole.CompleteLogEmailPreprocessor += delegate(Email email)
			{
				File.WriteAllText(Application.persistentDataPath + "/save.dat", "Imaginary Save File");
				email.Attachments.Add(new Email.Attachment(Application.persistentDataPath + "/save.dat", "application/octet-stream"));
				email.Message += "\n\nYour imaginary save file is attached!";
				return true;
			};
		}

		private void OnGUI()
		{
			GUILayout.BeginArea(new Rect(10f, 10f, Screen.width - 20, Screen.height - 20));
			GUILayout.FlexibleSpace();
			GUIStyle gUIStyle = new GUIStyle(GUI.skin.label);
			gUIStyle.alignment = TextAnchor.MiddleCenter;
			gUIStyle.fontSize = ((Screen.dpi > 0f) ? ((int)(Screen.dpi * 0.1f)) : 20);
			gUIStyle.wordWrap = true;
			GUIStyleState gUIStyleState = new GUIStyleState();
			gUIStyleState.textColor = Color.black;
			gUIStyle.normal = gUIStyleState;
			if (!_isConsoleVisible)
			{
				GUILayout.Label("To open the console, press the 'tilde' key (~).", gUIStyle, GUILayout.ExpandWidth(true));
			}
			GUILayout.EndArea();
		}

		private void Update()
		{
			Debug.Log("Test" + new string(' ', 65535));
			_isConsoleVisible = DebugConsole.Instance != null && DebugConsole.IsVisible;
		}

		private void OnDestroy()
		{
			CommandHandlers.UnregisterCommandHandlers(this);
		}

		[ConsoleCommand(Description = "This command creates a sphere just above the ground")]
		private void CreateBall()
		{
			Debug.Log("Created ball");
			GameObject gameObject = Object.Instantiate(_sphere, Vector3.up, Quaternion.identity);
			gameObject.SetActive(true);
			_numberOfBallsCreated++;
		}

		[ConsoleCommand(Description = "You can enter Vectors by entering numbers separated by commas (e.g. 1,2,1)")]
		private void CreateBallAtPosition(Vector3 position)
		{
			GameObject gameObject = Object.Instantiate(_sphere, position, Quaternion.identity);
			gameObject.SetActive(true);
			_numberOfBallsCreated++;
		}

		[ConsoleCommand(Description = "Change the colour of the sky. Predefined named colours are available.")]
		private void SetSkyColor(Color color)
		{
			Camera.main.backgroundColor = color;
		}

		[ConsoleCommand(Description = "Throws an exception to test how the console handles displays it.")]
		private void ThrowException()
		{
			throw new System.InvalidOperationException("This is a test exception");
		}

		[ConsoleCommand]
		private void LoremIpsum()
		{
			Debug.Log("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus facilisis, nisl id venenatis lacinia, diam metus eleifend felis, ac aliquam lacus sapien eget justo. Donec auctor consequat posuere. Integer efficitur blandit accumsan. Cras vehicula dictum sapien, sed ultricies leo laoreet nec. Integer egestas ex tempus metus vulputate lacinia. Cras ut rutrum leo, quis ornare tellus. Sed dolor odio, volutpat a dignissim a, eleifend vel urna. Nam pellentesque enim ac mauris eleifend, et ultrices augue mollis. Etiam eget neque facilisis, tempor odio nec, feugiat turpis.\n\nEtiam nunc augue, finibus id consequat id, iaculis ac diam. Pellentesque condimentum dolor id quam finibus, id ultrices felis finibus. Proin faucibus nibh vitae sapien aliquet, sit amet sagittis nunc viverra. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; In vestibulum eleifend pulvinar. Phasellus at congue erat. Fusce at orci egestas, congue est posuere, facilisis ante. Donec semper justo non porta hendrerit. Cras interdum, ipsum non interdum vulputate, sem ipsum imperdiet felis, eget euismod orci tortor vitae nunc.\n\nNam ornare tempor nisi sed posuere. Etiam gravida porttitor fringilla. Donec id dui lacinia, elementum magna et, posuere metus. Proin sed neque risus. Curabitur nunc tortor, rhoncus ac varius a, pellentesque sit amet elit. Duis tempor rhoncus leo, eu lacinia ligula consectetur in. Praesent ut pellentesque ligula. Curabitur tempus mauris lobortis semper hendrerit. Proin mattis, quam nec elementum placerat, arcu erat posuere lacus, ac lobortis est magna quis neque. Sed at mauris sed libero facilisis mattis. Maecenas eget elementum ex. Maecenas vitae sollicitudin est, a elementum sem. Fusce hendrerit felis sit amet pellentesque dapibus.\n\nNullam at maximus quam. Etiam id metus accumsan, condimentum enim sed, malesuada est. Nulla ut urna consequat, porttitor ligula at, malesuada turpis. Quisque egestas lectus non pretium tincidunt. Ut erat ligula, dignissim a pellentesque nec, convallis in orci. Vivamus eu efficitur turpis. Pellentesque luctus ex nec feugiat pharetra. Duis cursus nulla non quam sollicitudin dictum.\n\nMaecenas mauris mauris, tempus sit amet ligula eget, convallis congue arcu. Morbi vel posuere ante. Integer justo odio, tristique ac lorem at, congue congue velit. Maecenas mattis magna lacus, eget rhoncus quam gravida eu. Vestibulum vitae tristique tellus. Etiam sit amet ligula quis arcu tempor laoreet eu eu erat. Nunc ultrices ligula ipsum. Ut eget purus imperdiet, laoreet neque ac, ultrices nunc. Morbi urna nisi, maximus vitae gravida vel, luctus et turpis. Etiam id vestibulum ligula. Quisque tempus sit amet nulla sit amet fermentum. Aenean luctus enim id ex suscipit, eu maximus tortor accumsan. Sed aliquam ex sit amet erat aliquet, vitae finibus ipsum laoreet. Nam luctus in tortor id tristique. Phasellus placerat neque a nibh scelerisque dignissim. Donec in urna ipsum.");
		}

		[ConsoleCommand(Description = "This is a method that demonstrates that optional arguments work as you would expect")]
		private void OptionalArgumentExample(float c = 50f)
		{
		}

		[ConsoleCommand]
		private void LoadLevel([Autocomplete(typeof(DemoController), "LevelAutocomplete")] string levelName)
		{
			Debug.Log("This is a fake command to demonstrate auto-completion. Imagine you've loaded the level '" + levelName + "'!");
		}

		public static IEnumerable<string> LevelAutocomplete()
		{
			return new string[]
			{
				"Map",
				"MagneticFieldSensor",
				"Level1",
				"Level2",
				"Level3",
				"Intro Movie",
				"Game Over Screen"
			};
		}
	}
}
