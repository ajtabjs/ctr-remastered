using UnityEngine;
using UnityEngine.UI;

namespace Paladin.SafeAreaPlugin
{
	[RequireComponent(typeof(Image))]
	public class OverlayActivator : MonoBehaviour
	{
		private const string IPHONE_OVERLAY_ASSETNAME = "iPhoneXOverlay";

		private Image image;

		private void Awake()
		{
		}

		private void Update()
		{
		}
	}
}
