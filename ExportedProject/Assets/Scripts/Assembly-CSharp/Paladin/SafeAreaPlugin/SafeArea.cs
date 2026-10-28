using UnityEngine;

namespace Paladin.SafeAreaPlugin
{
	public static class SafeArea
	{
		private const float TOP_SAFEAREA_ADJUST_FRACTION = 0.2f;

		private static int screenWidth;

		private static int screenHeight;

		private static Rect cachedRect;

		public static Rect GetRect()
		{
			if (screenHeight != Screen.height || screenWidth != Screen.width)
			{
				screenWidth = Screen.width;
				screenHeight = Screen.height;
				cachedRect = Screen.safeArea;
				cachedRect.height += (1f - (cachedRect.height + cachedRect.y) / (float)Screen.height) * 0.2f * (float)Screen.height;
			}
			return cachedRect;
		}

		public static Rect GetNormalisedRect()
		{
			Rect rect = GetRect();
			return new Rect(rect.x / (float)Screen.width, rect.y / (float)Screen.height, rect.width / (float)Screen.width, rect.height / (float)Screen.height);
		}
	}
}
