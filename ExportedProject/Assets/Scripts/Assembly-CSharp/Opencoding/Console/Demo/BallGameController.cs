using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Opencoding.CommandHandlerSystem;
using UnityEngine;

namespace Opencoding.Console.Demo
{
	internal class BallGameController : MonoBehaviour
	{
		private int _turnsLeft;

		[SerializeField]
		private GameObject _ballPrefab;

		[SerializeField]
		private BallSpawner[] _ballSpawners;

		[SerializeField]
		private BallGameGameOverUI _gameOverUI;

		[SerializeField]
		private BallGameHUD _hud;

		private readonly HashSet<DemoBall> _balls = new HashSet<DemoBall>();

		public int Level { get; private set; }

		public int BallsDestroyedThisLevel { get; private set; }

		[ConsoleCommand]
		public int TurnsLeft
		{
			get
			{
				return _turnsLeft;
			}
			private set
			{
				if (TotalTurns < value)
				{
					throw new InvalidOperationException("TurnsLeft can't be greater than TotalTurns");
				}
				_turnsLeft = value;
			}
		}

		[ConsoleCommand]
		public int InitialBalls { get; private set; }

		public int TotalTurns { get; private set; }

		public int BallTargetThisLevel { get; private set; }

		public bool GameIsOver { get; private set; }

		private void Start()
		{
			Application.targetFrameRate = 60;
			InitialBalls = 80;
			SetupDebugConsole();
			BallGameGeneralCommandHandlers.Initialize();
			CommandHandlers.RegisterCommandHandlers(this);
			_gameOverUI.Setup(this);
			_hud.Setup(this);
			_ballPrefab.SetActive(false);
			BallSpawner[] ballSpawners = _ballSpawners;
			for (int i = 0; i < ballSpawners.Length; i++)
			{
				ballSpawners[i].Setup(this, _ballPrefab);
			}
			BeginGame();
		}

		private void OnDestroy()
		{
			CommandHandlers.UnregisterCommandHandlers(this);
		}

		[ConsoleCommand]
		private void ExportLogToMemoryStream()
		{
			MemoryStream memoryStream = new MemoryStream();
			DebugConsole.Instance.ExportLog(memoryStream, true, delegate
			{
				Debug.Log(Encoding.UTF8.GetString(memoryStream.ToArray()));
				memoryStream.Close();
			});
		}

		private void SetupDebugConsole()
		{
			DebugConsole.SaveFileProvider += () => new SaveFileData[]
			{
				new SaveFileData("save.txt", BallGameSaveSystem.AsString(), SaveFileDataType.TEXT),
				new SaveFileData("data.json", "{\"health\": 42, \"player_name\": \"Player17\"}", SaveFileDataType.JSON)
			};
			DebugConsole.GameInfoProvider += () => new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("Current level", Level.ToString()),
				new KeyValuePair<string, string>("Turns left", _turnsLeft.ToString()),
				new KeyValuePair<string, string>("Is game over?", GameIsOver.ToString())
			};
			CommandHandlers.BeforeCommandParsedHook += delegate(string input)
			{
				if (input.StartsWith("-"))
				{
					Debug.Log("Dash prefixed command entered: " + input.Substring(1));
					return true;
				}
				return false;
			};
		}

		private void BeginGame()
		{
			GameIsOver = false;
			Level = 1;
			SpawnBalls(InitialBalls);
			TotalTurns = 15 - Level;
			_turnsLeft = 15 - Level;
			BallsDestroyedThisLevel = 0;
			BallTargetThisLevel = 7 * Level + 5;
		}

		public void RegisterBall(DemoBall demoBall)
		{
			_balls.Add(demoBall);
		}

		public void BallCollected(DemoBall ball)
		{
			_balls.Remove(ball);
		}

		private void BeginRound()
		{
			TotalTurns = 15 - Level;
			_turnsLeft = 15 - Level;
			BallsDestroyedThisLevel = 0;
			BallTargetThisLevel = 7 * Level + 5;
		}

		private void EndOfRound()
		{
			Debug.Log("Round " + Level + " over");
			Level++;
			BallGameSaveSystem.MaxLevelReached = Mathf.Max(BallGameSaveSystem.MaxLevelReached, Level);
			SpawnBalls(Mathf.Clamp(16 - 2 * Level, 0, 20) + 18);
			TotalTurns = 15 - Level;
			_turnsLeft = 15 - Level;
			BallsDestroyedThisLevel = 0;
			BallTargetThisLevel = 7 * Level + 5;
		}

		[ConsoleCommand(Description = "Spawn the specified number of balls")]
		private void SpawnBalls(int ballsToSpawn)
		{
			Debug.Log(string.Format("Spawning {0} balls", ballsToSpawn));
			BallSpawner[] ballSpawners = _ballSpawners;
			for (int i = 0; i < ballSpawners.Length; i++)
			{
				ballSpawners[i].SpawnBalls(ballsToSpawn / _ballSpawners.Length);
			}
		}

		public void DestroyAdjacentMatchingBalls(DemoBall demoBall)
		{
			if (DebugConsole.IsVisible)
			{
				return;
			}
			if (GameIsOver)
			{
				return;
			}
			Debug.Log("Clicked on ball with color " + demoBall.BallColorIndex);
			HashSet<DemoBall> hashSet = new HashSet<DemoBall>();
			hashSet.Add(demoBall);
			int count;
			do
			{
				count = hashSet.Count;
				FindAdjacentMatchingBalls(hashSet, demoBall.BallColorIndex, 1.21f);
			}
			while (count != hashSet.Count);
			Debug.Log(string.Format("Destroying {0} balls", count));
			foreach (DemoBall item in hashSet)
			{
				UnityEngine.Object.Destroy(item.gameObject);
				BallsDestroyedThisLevel++;
				BallGameSaveSystem.BallsCollected++;
			}
			if (BallsDestroyedThisLevel >= BallTargetThisLevel)
			{
				EndOfRound();
				return;
			}
			TurnsLeft = _turnsLeft - 1;
			if (_turnsLeft == 0)
			{
				EndGame();
			}
		}

		private void FindAdjacentMatchingBalls(HashSet<DemoBall> matchedBalls, int colorIndex, float limitSquared)
		{
			foreach (DemoBall ball in _balls)
			{
				if (matchedBalls.Contains(ball) || ball.BallColorIndex != colorIndex)
				{
					continue;
				}
				foreach (DemoBall matchedBall in matchedBalls)
				{
					if ((matchedBall.transform.position - ball.transform.position).sqrMagnitude < limitSquared)
					{
						matchedBalls.Add(ball);
						break;
					}
				}
			}
		}

		[ConsoleCommand]
		private void EndGame()
		{
			Debug.Log("Game is over");
			_gameOverUI.gameObject.SetActive(true);
			GameIsOver = true;
			BallGameSaveSystem.Save();
		}

		[ConsoleCommand(Description = "Restarts the game")]
		public void Restart()
		{
			Debug.Log("Restarting game");
			foreach (DemoBall ball in _balls)
			{
				UnityEngine.Object.Destroy(ball.gameObject);
			}
			BeginGame();
		}
	}
}
