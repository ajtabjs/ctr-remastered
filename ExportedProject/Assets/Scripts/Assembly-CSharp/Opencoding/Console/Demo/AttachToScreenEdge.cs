using UnityEngine;

namespace Opencoding.Console.Demo
{
	internal class AttachToScreenEdge : MonoBehaviour
	{
		private enum Edge
		{
			LEFT = 0,
			RIGHT = 1,
			BOTTOM = 2
		}

		[SerializeField]
		private Edge _edge;

		private void Start()
		{
		}
	}
}
