using System.Collections;
using UnityEngine;

namespace Opencoding.Console.Demo
{
	internal class BallSpawner : MonoBehaviour
	{
		private GameObject _ballPrefab;

		private BallGameController _ballGameController;

		[SerializeField]
		private float _xFraction;

		private void Awake()
		{
			float orthographicSize = Camera.main.orthographicSize;
			float aspect = Camera.main.aspect;
			Vector3 position = transform.position;
			float num = orthographicSize * aspect;
			position.x = num * 2f * _xFraction - num;
			transform.position = position;
		}

		public void SpawnBalls(int count)
		{
			StartCoroutine(SpawnBallCoroutine(count));
		}

		private IEnumerator SpawnBallCoroutine(int count)
		{
			for (int i = 0; i < count; i++)
			{
				GameObject gameObject = Object.Instantiate(_ballPrefab, transform.position, Quaternion.identity);
				gameObject.GetComponent<DemoBall>().Setup(_ballGameController);
				gameObject.SetActive(true);
				gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-3, 3), Random.Range(-2, -4));
				yield return new WaitForSeconds(0.1f);
			}
		}

		public void Setup(BallGameController ballGameController, GameObject ballPrefab)
		{
			_ballGameController = ballGameController;
			_ballPrefab = ballPrefab;
		}
	}
}
