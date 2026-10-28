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
			Camera main = Camera.main;
			float orthographicSize = main.orthographicSize;
			float num = main.aspect * orthographicSize;
			switch (_edge)
			{
			case Edge.LEFT:
				transform.position = new Vector3(0f - num - 7.5f, 0f, 0f);
				transform.localScale = new Vector3(orthographicSize * 2f, 1f, 1f);
				break;
			case Edge.RIGHT:
				transform.position = new Vector3(num + 7.5f, 0f, 0f);
				transform.localScale = new Vector3(orthographicSize * 2f, 1f, 1f);
				break;
			case Edge.BOTTOM:
				transform.position = new Vector3(0f, 0f - orthographicSize - 7.5f, 0f);
				transform.localScale = new Vector3(num * 2f, 1f, 1f);
				break;
			}
		}
	}
}
