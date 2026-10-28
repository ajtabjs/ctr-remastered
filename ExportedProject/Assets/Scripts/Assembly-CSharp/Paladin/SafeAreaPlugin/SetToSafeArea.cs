using UnityEngine;

namespace Paladin.SafeAreaPlugin
{
	[ExecuteInEditMode]
	public class SetToSafeArea : MonoBehaviour
	{
		private RectTransform rectTransform;

		private Rect lastSafeArea;

		private void Start()
		{
			rectTransform = GetComponent<RectTransform>();
		}

		private void Update()
		{
			Rect rect = SafeArea.GetRect();
			if (rect != lastSafeArea)
			{
				ApplySafeArea(rect);
			}
		}

		private void ApplySafeArea(Rect area)
		{
			rectTransform.anchorMin = new Vector2(area.x / (float)Screen.width, area.y / (float)Screen.height);
			rectTransform.anchorMax = new Vector2((area.x + area.width) / (float)Screen.width, (area.y + area.height) / (float)Screen.height);
			lastSafeArea = area;
		}
	}
}
