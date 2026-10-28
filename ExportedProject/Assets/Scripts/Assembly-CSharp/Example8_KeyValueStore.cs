using System.Collections;
using HovelHouse.CloudKit;
using UnityEngine;

public class Example8_KeyValueStore : MonoBehaviour
{
	private NSUbiquitousKeyValueStore store;

	private readonly string[] strings = new string[5] { "Aries", "Leo", "Cancer", "Pisces", "Scorpio" };

	private void Start()
	{
		Run();
	}

	private void Run()
	{
		Debug.Log("Example 8 - Key Value Store");
		Debug.Log("Open this example on another client to start receiving KVS changed Notifications");
		store = NSUbiquitousKeyValueStore.DefaultStore;
		store.AddDidChangeExternallyNotificationObserver(OnKeyStoreChanged);
		StartCoroutine(SetRandomValues(30f));
	}

	private void OnKeyStoreChanged(long arg1, string[] arg2)
	{
		Debug.Log(string.Format("Keystore has changed keys: ({0})", string.Join(",", arg2)));
		for (int i = 0; i < arg2.Length; i++)
		{
			string text = arg2[i];
			if (text == "BoolKey")
			{
				Debug.Log(string.Format("{0}:{1}", text, store.BoolForKey(text)));
			}
			else if (text == "DoubleKey")
			{
				Debug.Log(string.Format("{0}:{1}", text, store.DoubleForKey(text)));
			}
			else if (text == "StringKey")
			{
				Debug.Log(string.Format("{0}:{1}", text, store.StringForKey(text)));
			}
			else
			{
				Debug.Log(string.Format("{0}:{1}", text, null));
			}
		}
	}

	private IEnumerator SetRandomValues(float timeDelay)
	{
		while (true)
		{
			Debug.Log("Setting new random values...");
			store.SetBool(Random.Range(0, 1) > 0, "BoolKey");
			store.SetDouble(Random.Range(0f, 1f), "DoubleKey");
			store.SetString(strings[Random.Range(0, strings.Length)], "StringKey");
			store.Synchronize();
			Debug.Log("Waiting for " + timeDelay + "seconds...");
			yield return new WaitForSecondsRealtime(timeDelay);
			Debug.Log("done waiting");
		}
	}
}
