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
		Run();
	}

	private void Run()
	{
		Debug.Log("Example 7 - Account Status");
		Token = NSFileManager.DefaultManager.UbiquityIdentityToken;
		Unsubscriber = ICloudNotifications.AddIdentityDidChangeObserver(OnCloudIdentityChanged);
		CKContainer.AddAccountChangedNotificationObserver(AccountStatusChanged);
		Debug.Log("Account status notification handlers attached. Sign in or out of iCloud ot update.");
		Debug.Log("Waiting for change in account status...");
	}

	private void AccountStatusChanged(NSNotification obj)
	{
		Debug.Log("CKContainer account status changed notification. Fetching account status...");
		CKContainer.DefaultContainer.AccountStatusWithCompletionHandler(OnAccountStatusComplete);
	}

	private void OnAccountStatusComplete(CKAccountStatus accountStatus, NSError error)
	{
		if (error != null)
		{
			Debug.LogError(error.LocalizedDescription);
		}
		else if (!currentAccountStatus.HasValue || currentAccountStatus.Value != accountStatus)
		{
			CKAccountStatus? value = currentAccountStatus;
			currentAccountStatus = accountStatus;
			Debug.Log(string.Format("Account status changed from '{0}' to '{1}'", value, currentAccountStatus));
		}
	}

	private void OnCloudIdentityChanged(NSNotification obj)
	{
		Debug.Log("OnCloudIdentityChanged");
		bool flag = Token == NSFileManager.DefaultManager.UbiquityIdentityToken;
		Debug.Log(flag ? "The cloud identity has not changed" : "The cloud identity has chnaged");
		CKContainer.RemoveAccountChangedNotificationObserver(Unsubscriber);
	}
}
