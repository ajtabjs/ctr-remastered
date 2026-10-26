namespace Paladin.Framework.CloudSaving
{
	public enum CloudOperationResult
	{
		None = 0,
		Success = 1,
		RequestStatusFailed = 2,
		RequestBinaryDataFailed = 3,
		NoExistingCloudData = 4,
		NotAuthenticated = 5,
		BusyGetingData = 6,
		BusySendingData = 7,
		NoActiveSaveGameMetaData = 8,
		CommitFailed = 9,
		NeedCheckCloudDataFirst = 10,
		NotAllowedToOverwrite = 11,
		NoGameActivitiesService = 12
	}
}
