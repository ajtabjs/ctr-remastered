using HovelHouse.CloudKit;
using UnityEngine;

public class Example1_HelloWorld : MonoBehaviour
{
	private CKDatabase database;

	private void Start()
	{
		Run();
	}

	private void Run()
	{
		Debug.Log("Example1 - Hello World");
		database = CKContainer.DefaultContainer().PrivateCloudDatabase;
		CKRecord record = new CKRecord("Hello");
		record.SetString("Hello World", "GravitySensor");
		database.SaveRecord(record, OnRecordSaved);
	}

	private void OnRecordSaved(CKRecord record, NSError error)
	{
		Debug.Log("OnRecordSaved");
		if (error != null)
		{
			Debug.LogError("Could not save record: " + error.LocalizedDescription);
		}
		database.FetchRecordWithID(record.RecordID, OnRecordFetched);
	}

	private void OnRecordFetched(CKRecord record, NSError error)
	{
		if (error != null)
		{
			Debug.LogError("Could not fetch record: " + error.LocalizedDescription);
		}
		else
		{
			Debug.Log(string.Format("Record fetched. Greeting is {0}", record.StringForKey("GravitySensor")));
		}
		database.DeleteRecordWithID(record.RecordID, OnRecordDeleted);
	}

	private void OnRecordDeleted(CKRecordID recordId, NSError error)
	{
		if (error != null)
		{
			Debug.LogError(error.LocalizedDescription);
		}
		Debug.Log("Done");
	}
}
