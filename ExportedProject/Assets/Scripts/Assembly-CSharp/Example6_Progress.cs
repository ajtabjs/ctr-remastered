using HovelHouse.CloudKit;
using UnityEngine;

public class Example6_Progress : MonoBehaviour
{
	private int numFiles;

	private CKRecord[] records;

	private CKDatabase database;

	private void Start()
	{
	}

	private void Run()
	{
	}

	private void OnRecordsSaved(CKRecord[] savedRecords, CKRecordID[] deletedRecordIds, NSError error)
	{
	}

	private void OnPerRecordComplete(CKRecord record, NSError error)
	{
	}

	private void OnPerRecordProgress(CKRecord record, double progress)
	{
	}

	private void DeleteRecords()
	{
	}

	private void OnRecordsDeleted(CKRecord[] savedRecords, CKRecordID[] deletedRecordIds, NSError error)
	{
	}
}
