using UnityEngine;

namespace Opencoding.Console.Demo
{
	internal class DemoBall : MonoBehaviour
	{
		private BallGameController _ballGameController;

		[SerializeField]
		private Color[] _ballColors;

		public int BallColorIndex { get; private set; }

		public void Setup(BallGameController ballGameController)
		{
			_ballGameController = ballGameController;
			BallColorIndex = Random.Range(0, _ballColors.Length);
			GetComponent<SpriteRenderer>().color = _ballColors[BallColorIndex];
			ballGameController.RegisterBall(this);
		}

		private void OnMouseUp()
		{
			_ballGameController.DestroyAdjacentMatchingBalls(this);
		}

		private void OnDestroy()
		{
			if (_ballGameController != null)
			{
				_ballGameController.BallCollected(this);
			}
		}
	}
}
