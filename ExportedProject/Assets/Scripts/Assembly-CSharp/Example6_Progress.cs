using System;
using System.Linq;
using HovelHouse.CloudKit;
using UnityEngine;

public class Example6_Progress : MonoBehaviour
{
	private int numFiles = 5;

	private CKRecord[] records = new CKRecord[5];

	private CKDatabase database;

	private void Start()
	{
		Run();
	}

	private void Run()
	{
		Debug.Log("Example 6 - Per Record Progress");
		database = CKContainer.DefaultContainer().PrivateCloudDatabase;
		for (int i = 0; i < numFiles; i++)
		{
			CKRecord cKRecord = new CKRecord("BigFile");
			byte[] bytes = new byte[1000000];
			cKRecord.SetBuffer(bytes, "bytes");
			records[i] = cKRecord;
		}
		CKModifyRecordsOperation cKModifyRecordsOperation = new CKModifyRecordsOperation(records, null);
		cKModifyRecordsOperation.Configuration.QualityOfService = NSQualityOfService.UserInitiated;
		cKModifyRecordsOperation.PerRecordProgressBlock = OnPerRecordProgress;
		cKModifyRecordsOperation.PerRecordCompletionBlock = OnPerRecordComplete;
		cKModifyRecordsOperation.ModifyRecordsCompletionBlock = OnRecordsSaved;
		database.AddOperation(cKModifyRecordsOperation);
	}

	private void OnRecordsSaved(CKRecord[] savedRecords, CKRecordID[] deletedRecordIds, NSError error)
	{
		if (error != null)
		{
			Debug.LogError(error.LocalizedDescription);
		}
		else
		{
			Debug.Log("All records saved");
			DeleteRecords();
		}
	}

	private void OnPerRecordComplete(CKRecord record, NSError error)
	{
		if (error != null)
		{
			Debug.LogError(error.LocalizedDescription);
			return;
		}
		Debug.Log(string.Format("{0} has finished uploading", record.RecordID.RecordName));
	}

	private void OnPerRecordProgress(CKRecord record, double progress)
	{
		Debug.Log(string.Format("{0} {1}% complete", record.RecordID.RecordName, Math.Round(progress * 100.0)));
	}

	private void DeleteRecords()
	{
		Debug.Log("Cleaning Up");
		CKModifyRecordsOperation cKModifyRecordsOperation = new CKModifyRecordsOperation(null, records.Select((CKRecord r) => r.RecordID).ToArray());
		cKModifyRecordsOperation.Configuration.QualityOfService = NSQualityOfService.UserInitiated;
		cKModifyRecordsOperation.ModifyRecordsCompletionBlock = OnRecordsDeleted;
		database.AddOperation(cKModifyRecordsOperation);
	}

	private void OnRecordsDeleted(CKRecord[] savedRecords, CKRecordID[] deletedRecordIds, NSError error)
	{
		if (error != null)
		{
			Debug.LogError(error.LocalizedDescription);
		}
		else
		{
			Debug.Log(string.Format("Sucessfully deleted {0} records", deletedRecordIds.Length));
		}
		Debug.Log("Done");
	}
}
