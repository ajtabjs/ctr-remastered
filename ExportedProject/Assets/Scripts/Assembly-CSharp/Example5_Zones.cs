using HovelHouse.CloudKit;
using UnityEngine;

public class Example5_Zones : MonoBehaviour
{
	private CKDatabase database;

	private CKRecordZone customZone;

	private void Start()
	{
	}

	private void Run()
	{
	}

	private void OnRecordZoneCreated(CKRecordZone zone, NSError error)
	{
	}

	private void OnRecordsSaved(CKRecord[] savedRecords, CKRecordID[] deletedRecordIds, NSError error)
	{
	}

	private void DeleteZone()
	{
	}

	private void OnZoneDeleted(CKRecordZone[] savedZones, CKRecordZoneID[] deletedZoneIds, NSError error)
	{
	}
}
