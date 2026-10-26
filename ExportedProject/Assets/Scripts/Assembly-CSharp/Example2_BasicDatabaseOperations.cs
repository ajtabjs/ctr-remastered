using HovelHouse.CloudKit;
using UnityEngine;

public class Example2_BasicDatabaseOperations : MonoBehaviour
{
	private CKDatabase database;

	private CKRecord record;

	private void Start()
	{
	}

	private void Run()
	{
	}

	private void OnRecordSaved(CKRecord record, NSError error)
	{
	}

	private void FetchRecord(string recordName)
	{
	}

	private void OnRecordFetched(CKRecord record, NSError error)
	{
	}

	private void ModifyRecord()
	{
	}

	private void OnRecordModified(CKRecord record, NSError error)
	{
	}

	private void OnRecordDeleted(CKRecordID recordId, NSError error)
	{
	}
}
