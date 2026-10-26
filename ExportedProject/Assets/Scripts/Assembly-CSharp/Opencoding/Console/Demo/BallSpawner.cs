using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Opencoding.Console.Demo
{
	internal class BallSpawner : MonoBehaviour
	{
		[CompilerGenerated]
		private sealed class _003CSpawnBallCoroutine_003Ed__5 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public BallSpawner _003C_003E4__this;

			public int count;

			private int _003Ci_003E5__2;

			object IEnumerator<object>.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			[DebuggerHidden]
			public _003CSpawnBallCoroutine_003Ed__5(int _003C_003E1__state)
			{
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
			}

			private bool MoveNext()
			{
				return false;
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
			}
		}

		private GameObject _ballPrefab;

		private BallGameController _ballGameController;

		[SerializeField]
		private float _xFraction;

		private void Awake()
		{
		}

		public void SpawnBalls(int count)
		{
		}

		[IteratorStateMachine(typeof(_003CSpawnBallCoroutine_003Ed__5))]
		private IEnumerator SpawnBallCoroutine(int count)
		{
			return null;
		}

		public void Setup(BallGameController ballGameController, GameObject ballPrefab)
		{
		}
	}
}
