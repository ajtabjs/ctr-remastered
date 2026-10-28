using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExampleHub : MonoBehaviour
{
	private void Start()
	{
		Button[] componentsInChildren = GetComponentsInChildren<Button>();
		componentsInChildren[0].onClick.AddListener(delegate
		{
			SceneManager.LoadScene("Example1_HelloWorld");
		});
		componentsInChildren[1].onClick.AddListener(delegate
		{
			SceneManager.LoadScene("Example2_BasicDatabaseOperations");
		});
		componentsInChildren[2].onClick.AddListener(delegate
		{
			SceneManager.LoadScene("Example3_Querying");
		});
		componentsInChildren[3].onClick.AddListener(delegate
		{
			SceneManager.LoadScene("Example4_CKAsset");
		});
		componentsInChildren[4].onClick.AddListener(delegate
		{
			SceneManager.LoadScene("Example5_Zones");
		});
		componentsInChildren[5].onClick.AddListener(delegate
		{
			SceneManager.LoadScene("Example6_Progress");
		});
		componentsInChildren[6].onClick.AddListener(delegate
		{
			SceneManager.LoadScene("Example7_AccountStatus");
		});
		componentsInChildren[7].onClick.AddListener(delegate
		{
			SceneManager.LoadScene("Example8_KeyValueStore");
		});
		componentsInChildren[8].onClick.AddListener(delegate
		{
			SceneManager.LoadScene("Example9_Subscriptions");
		});
	}

	private void Update()
	{
	}
}
