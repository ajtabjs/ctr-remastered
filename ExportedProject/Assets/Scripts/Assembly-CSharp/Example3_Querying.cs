using System.Linq;
using HovelHouse.CloudKit;
using UnityEngine;

public class Example3_Querying : MonoBehaviour
{
	private CKDatabase database;

	private string[] names;

	private CKRecord[] recordsToSearch;

	private void Start()
	{
		Run();
	}

	private void Run()
	{
		Debug.Log("Example 3 - Querying");
		database = CKContainer.DefaultContainer.PrivateCloudDatabase;
		names = new string[5]
		{
			"Alice",
			"Bob",
			"Charles",
			"Danni",
			"Exavier"
		};
		recordsToSearch = names.Select(delegate(string name)
		{
			CKRecord cKRecord = new CKRecord("Pen");
			cKRecord.SetString(name, "name");
			return cKRecord;
		}).ToArray();
		Debug.Log("Creating records");
		CKModifyRecordsOperation cKModifyRecordsOperation = new CKModifyRecordsOperation(recordsToSearch, null);
		cKModifyRecordsOperation.Configuration.QualityOfService = NSQualityOfService.UserInitiated;
		cKModifyRecordsOperation.ModifyRecordsCompletionBlock = OnRecordsSaved;
		database.AddOperation(cKModifyRecordsOperation);
		CKFetchRecordsOperation cKFetchRecordsOperation = new CKFetchRecordsOperation(recordsToSearch.Select((CKRecord x) => x.RecordID).ToArray());
		cKFetchRecordsOperation.FetchRecordsCompletionHandler = delegate
		{
			Debug.Log("Fetch records complete");
		};
		database.AddOperation(cKFetchRecordsOperation);
	}

	private void OnRecordsSaved(CKRecord[] savedRecords, CKRecordID[] deletedRecordIds, NSError error)
	{
		if (error != null)
		{
			Debug.LogError(error.LocalizedDescription);
			return;
		}
		Debug.Log(string.Format("Saved {0} records with names: {1}", savedRecords.Length, string.Join(",", savedRecords.Select((CKRecord record) => record.RecordID.RecordName))));
		RunQuery();
	}

	private void RunQuery()
	{
		string text = names[Random.Range(0, names.Length)];
		string text2 = string.Format("name = '{0}'", text);
		Debug.Log(string.Format("Running query with predicate ({0})", text2));
		NSPredicate predicate = NSPredicate.PredicateWithFormat(text2);
		CKQuery query = new CKQuery("Pen", predicate);
		database.PerformQuery(query, null, OnQueryComplete);
	}

	private void OnQueryComplete(CKRecord[] records, NSError error)
	{
		Debug.Log("OnQueryComplete");
		if (error != null)
		{
			Debug.LogError(error.LocalizedDescription);
			return;
		}
		CKRecord cKRecord = records.FirstOrDefault();
		if (cKRecord != null)
		{
			Debug.Log(string.Format("found record: '{0}' with name: '{1}'", cKRecord.RecordID.RecordName, cKRecord.StringForKey("name")));
		}
		CKRecordID[] recordIDs = recordsToSearch.Select((CKRecord record) => record.RecordID).ToArray();
		Debug.Log("Cleaning up, deleting the records we created....");
		CKModifyRecordsOperation cKModifyRecordsOperation = new CKModifyRecordsOperation(null, recordIDs);
		cKModifyRecordsOperation.ModifyRecordsCompletionBlock = OnRecordsDeleted;
		cKModifyRecordsOperation.Configuration.QualityOfService = NSQualityOfService.UserInitiated;
		database.AddOperation(cKModifyRecordsOperation);
	}

	private void OnRecordsDeleted(CKRecord[] savedRecords, CKRecordID[] deletedRecordIds, NSError error)
	{
		Debug.Log("Records deleted");
		if (error != null)
		{
			Debug.LogError(error.LocalizedDescription);
		}
		else
		{
			Debug.Log("Deleted records");
			Debug.Log(string.Format("Deleted records: {0}", string.Join(",", deletedRecordIds.Select((CKRecordID id) => id.RecordName).ToArray())));
		}
		Debug.Log("Done");
	}
}
