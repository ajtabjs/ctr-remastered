using System.Collections.Generic;
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

		private readonly HashSet<DemoBall> _balls;

		public int Level { get; private set; }

		public int BallsDestroyedThisLevel { get; private set; }

		[ConsoleCommand]
		public int TurnsLeft
		{
			get
			{
				return 0;
			}
			private set
			{
			}
		}

		[ConsoleCommand]
		public int InitialBalls { get; private set; }

		public int TotalTurns { get; private set; }

		public int BallTargetThisLevel { get; private set; }

		public bool GameIsOver { get; private set; }

		private void Start()
		{
		}

		private void OnDestroy()
		{
		}

		[ConsoleCommand]
		private void ExportLogToMemoryStream()
		{
		}

		private void SetupDebugConsole()
		{
		}

		private void BeginGame()
		{
		}

		public void RegisterBall(DemoBall demoBall)
		{
		}

		public void BallCollected(DemoBall ball)
		{
		}

		private void BeginRound()
		{
		}

		private void EndOfRound()
		{
		}

		[ConsoleCommand(Description = "Spawn the specified number of balls")]
		private void SpawnBalls(int ballsToSpawn)
		{
		}

		public void DestroyAdjacentMatchingBalls(DemoBall demoBall)
		{
		}

		private void FindAdjacentMatchingBalls(HashSet<DemoBall> matchedBalls, int colorIndex, float limitSquared)
		{
		}

		[ConsoleCommand]
		private void EndGame()
		{
		}

		[ConsoleCommand(Description = "Restarts the game")]
		public void Restart()
		{
		}
	}
}
