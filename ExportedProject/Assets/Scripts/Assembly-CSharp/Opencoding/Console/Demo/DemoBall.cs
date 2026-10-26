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
		}

		private void OnMouseUp()
		{
		}

		private void OnDestroy()
		{
		}
	}
}
