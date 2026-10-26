using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using HovelHouse.CloudKit;
using UnityEngine;

public class Example8_KeyValueStore : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CSetRandomValues_003Ed__5 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Example8_KeyValueStore _003C_003E4__this;

		public float timeDelay;

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
		public _003CSetRandomValues_003Ed__5(int _003C_003E1__state)
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

	private NSUbiquitousKeyValueStore store;

	private readonly string[] strings;

	private void Start()
	{
	}

	private void Run()
	{
	}

	private void OnKeyStoreChanged(long arg1, string[] arg2)
	{
	}

	[IteratorStateMachine(typeof(_003CSetRandomValues_003Ed__5))]
	private IEnumerator SetRandomValues(float timeDelay)
	{
		return null;
	}
}
