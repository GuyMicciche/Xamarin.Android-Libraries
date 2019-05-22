using Android.App;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Views;
using Java.IO;
using System.Collections.Generic;
using System.Linq;

using DialogFragment = Android.Support.V4.App.DialogFragment;

namespace MaterialDialogs.Example
{
	/// <summary>
	/// @author Aidan Follestad (afollestad)
	/// </summary>
	public class FolderSelectorDialog : DialogFragment, MaterialDialog.IListCallback
	{

		private File parentFolder;
		private File[] parentContents;
		private bool canGoUp = true;
		private IFolderSelectCallback mCallback;
        private MaterialDialog.ButtonCallback mButtonCallback;

		private class MyButtonCallback : MaterialDialog.ButtonCallback
		{
            private FolderSelectorDialog dialog;
            public MyButtonCallback(FolderSelectorDialog dialog)
			{
                this.dialog = dialog;
			}

			public override void OnPositive(MaterialDialog materialDialog)
			{
				materialDialog.Dismiss();
                dialog.mCallback.OnFolderSelection(dialog.parentFolder);
			}

			public override void OnNegative(MaterialDialog materialDialog)
			{
				materialDialog.Dismiss();
			}
		}

		public interface IFolderSelectCallback
		{
			void OnFolderSelection(File folder);
		}

		public FolderSelectorDialog()
		{
            mButtonCallback = new MyButtonCallback(this);

			parentFolder = Environment.ExternalStorageDirectory;
			parentContents = ListFiles();
		}

		internal virtual string[] ContentsArray
		{
			get
			{
				string[] results = new string[parentContents.Length + (canGoUp ? 1 : 0)];
				if (canGoUp)
				{
					results[0] = "...";
				}
				for (int i = 0; i < parentContents.Length; i++)
				{
					results[canGoUp ? i + 1 : i] = parentContents[i].Name;
				}
				return results;
			}
		}

		internal virtual File[] ListFiles()
		{
			File[] contents = parentFolder.ListFiles();
			List<File> results = new List<File>();
			foreach (File fi in contents)
			{
				if (fi.IsDirectory)
				{
					results.Add(fi);
				}
			}
			results.Sort(new FolderSorter());
			return results.ToArray();
		}

		public override Dialog OnCreateDialog(Bundle savedInstanceState)
		{
			return (new MaterialDialog.Builder(Activity)).SetTitle(parentFolder.AbsolutePath).SetItems(ContentsArray).SetItemsCallback(this).SetCallback(mButtonCallback).SetAutoDismiss(false).SetPositiveText(Resource.String.choose).SetNegativeText(Android.Resource.String.Cancel).Build();
		}

		public void OnSelection(MaterialDialog materialDialog, View view, int i, string s)
		{
			if (canGoUp && i == 0)
			{
				parentFolder = parentFolder.ParentFile;
				canGoUp = parentFolder.Parent != null;
			}
			else
			{
				parentFolder = parentContents[canGoUp ? i - 1 : i];
				canGoUp = true;
			}
			parentContents = ListFiles();
			MaterialDialog dialog = (MaterialDialog) Dialog;
			dialog.SetTitle(parentFolder.AbsolutePath);
			dialog.SetItems(ContentsArray);
		}

		public override void OnAttach(Activity activity)
		{
			base.OnAttach(activity);
			mCallback = (IFolderSelectCallback)activity;
		}

		public virtual void Show(ActionBarActivity context)
		{
			Show(context.SupportFragmentManager, "FOLDER_SELECTOR");
		}

		private class FolderSorter : IComparer<File>
		{
			public virtual int Compare(File lhs, File rhs)
			{
				return lhs.Name.CompareTo(rhs.Name);
			}
		}
	}
}