using System;
using System.Text;
using HovelHouse.CloudKit;
using UnityEngine;

public class Example2_BasicDatabaseOperations : MonoBehaviour
{
	private CKDatabase database;

	private CKRecord record;

	private void Start()
	{
		Run();
	}

	private void Run()
	{
		Debug.Log("Example 2 - Basic Database Operations");
		database = CKContainer.DefaultContainer().PrivateCloudDatabase;
		record = new CKRecord("MyType");
		record.SetString("AmbientTemperatureSensor", "MyField");
		Debug.Log("Saving a record");
		database.SaveRecord(record, OnRecordSaved);
	}

	private void OnRecordSaved(CKRecord record, NSError error)
	{
		Debug.Log("OnRecordSaved");
		if (error != null)
		{
			Debug.LogError(error.LocalizedDescription);
			return;
		}
		Debug.Log(string.Format("record saved with name: '{0}'", record.RecordID.RecordName));
		FetchRecord(record.RecordID.RecordName);
	}

	private void FetchRecord(string recordName)
	{
		CKRecordID recordID = new CKRecordID(recordName);
		database.FetchRecordWithID(recordID, OnRecordFetched);
	}

	private void OnRecordFetched(CKRecord record, NSError error)
	{
		if (error != null)
		{
			Debug.LogError(error.LocalizedDescription);
			return;
		}
		Debug.Log(string.Format("Record fetched", Array.Empty<object>()));
		Debug.Log("CreationDate: " + record.CreationDate);
		Debug.Log("ModificationDate: " + record.ModificationDate);
		Debug.Log("RecordChangeTag: " + record.RecordChangeTag);
		Debug.Log("Parent: " + record.Parent);
		Debug.Log("Type: " + record.RecordType);
		Debug.Log("Share: " + record.Share);
		Debug.Log("CreatorUserRecordID: " + record.CreatorUserRecordID);
		Debug.Log("Last Modified User Record Id: " + record.LastModifiedUserRecordID);
		ModifyRecord();
	}

	private void ModifyRecord()
	{
		record.SetInt(Random.Range(0, int.MaxValue), "MyNumber");
		record.SetDouble(Random.Range(0f, float.MaxValue), "MyNumber");
		byte[] bytes = Encoding.ASCII.GetBytes("Test Buffer");
		record.SetBuffer(bytes, "TestBufferKey");
		database.SaveRecord(record, OnRecordModified);
	}

	private void OnRecordModified(CKRecord record, NSError error)
	{
		if (error != null)
		{
			Debug.LogError(error.LocalizedDescription);
			return;
		}
		Debug.Log(string.Format("record '{0}' MyNumber is:{1}", record.RecordID.RecordName, record.StringForKey("MyField")));
		Debug.Log("RecordChangeTag: " + record.RecordChangeTag);
		database.DeleteRecordWithID(record.RecordID, OnRecordDeleted);
	}

	private void OnRecordDeleted(CKRecordID recordId, NSError error)
	{
		if (error != null)
		{
			Debug.LogError(error.LocalizedDescription);
		}
		else
		{
			Debug.Log(string.Format("record: '{0}' deleted", recordId.RecordName));
		}
		Debug.Log("Done");
	}
}
