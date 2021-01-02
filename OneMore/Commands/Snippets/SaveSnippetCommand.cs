﻿//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Win = System.Windows;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class SaveSnippetCommand : Command
	{

		public SaveSnippetCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await SingleThreaded.Invoke(() =>
			{
				Win.Clipboard.Clear();
			});

			using (var one = new OneNote(out var page, out _))
			{
				if (page.GetTextCursor() != null)
				{
					UIHelper.ShowMessage(Resx.SaveSnippet_NeedSelection);
					return;
				}

				// since the Hotkey message loop is watching all input, explicitly setting
				// focus on the OneNote main window provides a direct path for SendKeys
				Native.SetForegroundWindow(one.WindowHandle);
				SendKeys.SendWait("^(c)");
			}

			var html = await SingleThreaded.Invoke(() =>
			{
				if (Win.Clipboard.ContainsText(Win.TextDataFormat.Html))
					return Win.Clipboard.GetText(Win.TextDataFormat.Html);
				else
					return null;
			});

			if (html == null || html.Length == 0)
			{
				return;
			}

			using (var dialog = new SaveSnippetDialog())
			{
				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				await new SnippetsProvider().Save(html, dialog.SnippetName);

				ribbon.InvalidateControl("ribFavoritesMenu");
			}
		}
	}
}
