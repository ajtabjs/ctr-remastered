using HovelHouse.CloudKit;
using UnityEngine;

public class Example3_Querying : MonoBehaviour
{
	private CKDatabase database;

	private string[] names;

	private CKRecord[] recordsToSearch;

	private void Start()
	{
	}

	private void Run()
	{
	}

	private void OnRecordsSaved(CKRecord[] savedRecords, CKRecordID[] deletedRecordIds, NSError error)
	{
	}

	private void RunQuery()
	{
	}

	private void OnQueryComplete(CKRecord[] records, NSError error)
	{
	}

	private void OnRecordsDeleted(CKRecord[] savedRecords, CKRecordID[] deletedRecordIds, NSError error)
	{
	}
}
