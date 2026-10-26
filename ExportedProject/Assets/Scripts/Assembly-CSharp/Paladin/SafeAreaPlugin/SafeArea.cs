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
			return default(Rect);
		}

		public static Rect GetNormalisedRect()
		{
			return default(Rect);
		}
	}
}
