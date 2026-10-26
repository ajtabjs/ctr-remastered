using HovelHouse.CloudKit;
using UnityEngine;

public class Example7_AccountStatus : MonoBehaviour
{
	private Unsubscriber unsub;

	private UbiquityIdentityToken Token;

	private NSUbiquitousKeyValueStore store;

	private CKAccountStatus? currentAccountStatus;

	private Unsubscriber Unsubscriber;

	private void Start()
	{
	}

	private void Run()
	{
	}

	private void AccountStatusChanged(NSNotification obj)
	{
	}

	private void OnAccountStatusComplete(CKAccountStatus accountStatus, NSError error)
	{
	}

	private void OnCloudIdentityChanged(NSNotification obj)
	{
	}
}
