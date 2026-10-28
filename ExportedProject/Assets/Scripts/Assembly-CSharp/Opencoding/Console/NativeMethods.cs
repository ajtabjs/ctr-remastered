using System;
using Opencoding.Shared.Utils;
using UnityEngine;

namespace Opencoding.Console
{
	internal static class NativeMethods
	{
		public static void SendEmail(Email email)
		{
			throw new InvalidOperationException("Emailing is not supported on this platform. Please contact support@opencoding.net and I'll do my best to support it!");
		}

		public static void CopyTextToClipboard(string text)
		{
			TextEditor textEditor = new TextEditor();
			textEditor.text = text;
			textEditor.SelectAll();
			textEditor.Copy();
		}

		public static float GetNativeScreenScaleFactor()
		{
			return 1f;
		}
	}
}
