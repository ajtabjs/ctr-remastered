using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TVOSSubmitButton : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		if (!Input.GetKeyDown(KeyCode.Joystick1Button14))
		{
			return;
		}
		EventSystem current = EventSystem.current;
		if (current != null && current.currentSelectedGameObject != null)
		{
			Button component = current.currentSelectedGameObject.GetComponent<Button>();
			if (component != null)
			{
				component.onClick.Invoke();
			}
		}
	}
}
