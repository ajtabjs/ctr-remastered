using UnityEngine;
using UnityEngine.UI;

namespace Opencoding.Console.Demo
{
	internal class BallGameHUD : MonoBehaviour
	{
		[SerializeField]
		private Text _levelLabel;

		[SerializeField]
		private Text _turnsLabel;

		[SerializeField]
		private Text _targetLabel;

		private BallGameController _ballGameController;

		public void Setup(BallGameController ballGameController)
		{
		}

		private void Update()
		{
		}
	}
}
