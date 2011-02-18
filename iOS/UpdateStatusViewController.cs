using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Facebook
{
	public class UpdateStatusViewController : UIViewController
	{
		public UpdateStatusViewController ()
		{
		}
		
		UITextView TextBox;
		UILabel Label;
		UIBarButtonItem BarButton;

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
				
			NavigationItem.Title = "Update Status";

			BarButton = new UIBarButtonItem(UIBarButtonSystemItem.Save);
			BarButton.Clicked += delegate 
			{
				Console.WriteLine("Sharing status...");
				AppDelegate.Current.UpdateStatus(TextBox.Text);
				this.NavigationController.PopViewControllerAnimated(true);
			};
			NavigationItem.SetRightBarButtonItem (BarButton, false);

			Label = new UILabel();
			Label.Frame = new System.Drawing.RectangleF(10, 5, 300, 30);
			Label.Text = "What's happening?";

			TextBox = new UITextView();
			TextBox.Frame = new System.Drawing.RectangleF(10,35, 300, 150);
			
			TextBox.Font = UIFont.FromName ("Helvetica", 14f);
			TextBox.TextAlignment = UITextAlignment.Left;
			TextBox.ScrollsToTop = true;
			TextBox.BackgroundColor = UIColor.LightGray;

			TextBox.AutocapitalizationType = UITextAutocapitalizationType.None;
			TextBox.AutocorrectionType = UITextAutocorrectionType.Default;
			TextBox.KeyboardAppearance = UIKeyboardAppearance.Default;
			TextBox.KeyboardType = UIKeyboardType.Default;

			Add(Label);
			Add(TextBox);
		}
	}
}

