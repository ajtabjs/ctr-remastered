using System.Collections;
using System.Linq;
using HovelHouse.CloudKit;
using UnityEngine;

public class Example9_Subscriptions : MonoBehaviour
{
	private const string recordType = "SubscriptionRecordType";

	private CKDatabase database;

	private IEnumerator Start()
	{
		Debug.Log("Example 9 - Subscriptions");
		Debug.Log("Requesting notification token");
		ICloudNotifications.RequestNotificationToken(delegate(byte[] tokenBytes, NSError error)
		{
			if (error != null)
			{
				Debug.LogError("Failed to get push notification token: " + error.LocalizedDescription);
			}
			else
			{
				Debug.LogError("Got push notification token");
			}
		});
		ICloudNotifications.SetRemoteNotificationHandler(delegate
		{
			Debug.Log("remote notification handler called");
		});
		database = CKContainer.DefaultContainer().PrivateCloudDatabase;
		database.FetchAllSubscriptionsWithCompletionHandler(OnSubscriptionsFetched);
		ICloudNotifications.SetRemoteNotificationHandler(OnQueryNotification);
		yield break;
	}

	private void OnSubscriptionsFetched(CKSubscription[] subscriptions, NSError error)
	{
		if (error != null)
		{
			Debug.LogError(error.LocalizedDescription);
			return;
		}
		if (subscriptions.Length == 0)
		{
			Debug.Log("No subscriptions detected - making a new one");
			CKRecord record = new CKRecord("SubscriptionRecordType");
			database.SaveRecord(record, delegate(CKRecord savedRecord, NSError saveError)
			{
				if (saveError != null)
				{
					Debug.LogError(saveError.LocalizedDescription);
				}
				else
				{
					CreateSubscription();
				}
			});
			return;
		}
		Debug.LogFormat("You have the following subscriptions: {0}", new object[]
		{
			string.Join(",", subscriptions.Select((CKSubscription sub) => sub.SubscriptionID.ToString()).ToArray())
		});
	}

	private void OnQueryNotification(CKNotification notification)
	{
		Debug.Log(string.Format("Recieved notification: {0} {1}", notification.NotificationID, notification.NotificationType));
		if (notification is CKQueryNotification cKQueryNotification)
		{
			Debug.Log("Notification Reason: " + cKQueryNotification.QueryNotificationReason);
			Debug.Log("RecordID: " + cKQueryNotification.RecordID.RecordName);
		}
	}

	private void CreateSubscription()
	{
		CKDatabase cKDatabase = CKContainer.DefaultContainer().PrivateCloudDatabase;
		NSPredicate predicate = NSPredicate.PredicateWithValue(true);
		CKNotificationInfo cKNotificationInfo = new CKNotificationInfo();
		CKQuerySubscription cKQuerySubscription = new CKQuerySubscription("SubscriptionRecordType", predicate, CKQuerySubscriptionOptions.FiresOnRecordCreation | CKQuerySubscriptionOptions.FiresOnRecordUpdate | CKQuerySubscriptionOptions.FiresOnRecordDeletion);
		cKQuerySubscription.NotificationInfo = cKNotificationInfo;
		cKDatabase.SaveSubscription(cKQuerySubscription, OnSubscriptionSaved);
	}

	private void OnSubscriptionModified(CKSubscription[] savedSubscriptions, string[] deletedSubscriptionIds, NSError error)
	{
		if (error != null)
		{
			Debug.LogError(error.LocalizedDescription);
			return;
		}
		Debug.Log(string.Format("Saved {0} subscriptions with ids: ({1})", savedSubscriptions.Length, string.Join(",", savedSubscriptions.Select((CKSubscription x) => x.SubscriptionID))));
	}

	private void OnSubscriptionSaved(CKSubscription subscription, NSError error)
	{
		if (error == null)
		{
			Debug.Log(string.Format("subscription saved with id: {0} of type: {1}", subscription.SubscriptionID, subscription.SubscriptionType));
		}
		else
		{
			Debug.LogError("error saving subscription: " + error.LocalizedDescription);
		}
	}
}
