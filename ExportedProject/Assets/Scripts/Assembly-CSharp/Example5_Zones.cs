using System.Linq;
using HovelHouse.CloudKit;
using UnityEngine;

public class Example5_Zones : MonoBehaviour
{
	private CKDatabase database;

	private CKRecordZone customZone;

	private void Start()
	{
		Run();
	}

	private void Run()
	{
		Debug.Log("Example 5 - Zones");
		customZone = new CKRecordZone("GameCenterUser1");
		database = CKContainer.DefaultContainer().PrivateCloudDatabase;
		database.SaveRecordZone(customZone, OnRecordZoneCreated);
	}

	private void OnRecordZoneCreated(CKRecordZone zone, NSError error)
	{
		Debug.Log(string.Format("Created record zone with name {0}", zone.ZoneID.ZoneName));
		CKRecord record = new CKRecord("SaveGame", customZone.ZoneID);
		CKRecord record2 = new CKRecord("SaveGame", new CKRecordID("MySave", customZone.ZoneID));
		CKModifyRecordsOperation cKModifyRecordsOperation = new CKModifyRecordsOperation(new CKRecord[2] { record, record2 }, null);
		cKModifyRecordsOperation.Configuration.QualityOfService = NSQualityOfService.UserInitiated;
		cKModifyRecordsOperation.ModifyRecordsCompletionBlock = OnRecordsSaved;
		database.AddOperation(cKModifyRecordsOperation);
	}

	private void OnRecordsSaved(CKRecord[] savedRecords, CKRecordID[] deletedRecordIds, NSError error)
	{
		if (error != null)
		{
			Debug.LogError(error.LocalizedDescription);
			return;
		}
		Debug.Log(string.Format("Saved {0} records to zone: {1}", savedRecords.Length, savedRecords.First().RecordID.ZoneID.ZoneName));
		DeleteZone();
	}

	private void DeleteZone()
	{
		Debug.Log("Deleting Record Zone...");
		CKModifyRecordZonesOperation cKModifyRecordZonesOperation = new CKModifyRecordZonesOperation(null, new CKRecordZoneID[1] { customZone.ZoneID });
		cKModifyRecordZonesOperation.Configuration.QualityOfService = NSQualityOfService.UserInitiated;
		cKModifyRecordZonesOperation.ModifyRecordZonesCompletionHandler = OnZoneDeleted;
		database.AddOperation(cKModifyRecordZonesOperation);
	}

	private void OnZoneDeleted(CKRecordZone[] savedZones, CKRecordZoneID[] deletedZoneIds, NSError error)
	{
		if (error != null)
		{
			Debug.LogError(error.LocalizedDescription);
		}
		else
		{
			Debug.Log(string.Format("zone {0} successfully deleted", deletedZoneIds[0].ZoneName));
		}
		Debug.Log("Done");
	}
}
