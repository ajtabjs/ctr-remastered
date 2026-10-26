using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using HovelHouse.CloudKit;
using UnityEngine;

public class Example9_Subscriptions : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CStart_003Ed__2 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Example9_Subscriptions _003C_003E4__this;

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
		public _003CStart_003Ed__2(int _003C_003E1__state)
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

	private const string recordType = "SubscriptionRecordType";

	private CKDatabase database;

	[IteratorStateMachine(typeof(_003CStart_003Ed__2))]
	private IEnumerator Start()
	{
		return null;
	}

	private void OnSubscriptionsFetched(CKSubscription[] subscriptions, NSError error)
	{
	}

	private void OnQueryNotification(CKNotification notification)
	{
	}

	private void CreateSubscription()
	{
	}

	private void OnSubscriptionModified(CKSubscription[] savedSubscriptions, string[] deletedSubscriptionIds, NSError error)
	{
	}

	private void OnSubscriptionSaved(CKSubscription subscription, NSError error)
	{
	}
}
