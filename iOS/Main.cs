using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

using MonoTouch.Facebook.Authorization;

using System.Text;

using Newtonsoft.Json;

/*
Facebook doco

http://developers.facebook.com/docs/opengraph/
http://developers.facebook.com/docs/reference/api/user/

http://developers.facebook.com/docs/reference/api/ 
 
To create an app
http://www.facebook.com/developers/createapp.php
*/

// iOS stuff
// http://developers.facebook.com/docs/guides/mobile
//

// Binding to MT
// http://forums.monotouch.net/yaf_postst702.aspx
// http://btouch-library.googlecode.com/files/FacebookConnect.zip
// http://code.google.com/p/btouch-library/source/browse/trunk/FacebookConnect/FacebookConnect.cs
//

// Json.NET
// http://www.brettnagy.com/post/2009/11/21/Using-JsonNET-with-MonoTouch.aspx
// 
namespace Facebook
{
	/// <summary>
	/// Example using Facebook
	/// </summary>
	public class Application
	{
		static void Main (string[] args)
		{ UIApplication.Main (args, null, "AppDelegate"); }
	}

	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{		
		public UINavigationController NavController;
		UIWindow window;		
		UITableViewController table;	// news table
		UIViewController usvc;			// update status input
		UIBarButtonItem statusButton;	// [+] button
		
		//string clientId = "";// TODO: You must create a Facebook application to get an Id -- http://www.facebook.com/developers/createapp.php
		
		string token;

		public AppDelegate()
		{
			Current = this;
		}

		public static AppDelegate Current
		{
			get; private set;
		}

		// This method is invoked when the application has loaded its UI and its ready to run
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			window = new UIWindow (UIScreen.MainScreen.Bounds);	
			window.BackgroundColor = UIColor.White;

			table = new UITableViewController();
			
			NavController = new UINavigationController();
			
			// Add the [+] button
			statusButton = new UIBarButtonItem (UIBarButtonSystemItem.Add);
			statusButton.Clicked += delegate 
			{
				usvc = new UpdateStatusViewController();
				NavController.PushViewController(usvc, true);
			};
			table.NavigationItem.SetRightBarButtonItem (statusButton, false);


			FacebookAuthorizationViewController fac = 
				new FacebookAuthorizationViewController(
					  clientId 
					, new string[] {"read_stream", "publish_stream"} //, "user_groups"}
					, FbDisplayType.Touch);

			fac.AccessToken += 
			delegate(string accessToken, DateTime expires) 
			{
				token = accessToken;

			    //Logged in, got your accessToken here and when it expires!
			    NavController.PopViewControllerAnimated(true);
			  
			    //Do something else here (eg: Save the accessToken and expiry date to be used in your Graph API calls)
				Console.WriteLine("### Get json");
				System.Net.WebClient wc = new System.Net.WebClient();

				var b = wc.DownloadData(
					new Uri("https://graph.facebook.com/me/home?access_token=" + token)
				);
				var s = Encoding.UTF8.GetString(b);

				//Console.WriteLine("### Output json");
				Console.WriteLine(s);

				// http://www.brettnagy.com/post/2009/11/21/Using-JsonNET-with-MonoTouch.aspx
				var posts = JsonConvert.DeserializeObject<Posts>( s );
				
//				foreach(var p in posts.data)
//				{
//					Console.WriteLine("name: " + p.from.name);
//				}
				
				table.Title = "Facebook";
				table.TableView.Source = new TableViewSource(posts);

			};

			NavController.PushViewController(table, false);
			NavController.PushViewController(fac, true);
			NavController.NavigationBar.TintColor = new UIColor(0.27f,0.52f,0.73f,1f);

			window.AddSubview(NavController.View);

			window.MakeKeyAndVisible ();
			return true;
		}

		/// <summary>
		/// Update Facebook status
		/// </summary>
		public bool UpdateStatus (string status)
		{
			System.Net.WebClient wc = new System.Net.WebClient();
			var result = wc.UploadString
						("https://graph.facebook.com/me/feed?access_token=" + token
						, "message=" + status);

			// Expect the result to be a json string like this
			// {"id":"689847836_129432987125279"}
			return result.IndexOf ("id") > 0;
		}
		
		// This method is required in iPhoneOS 3.0
		public override void OnActivated (UIApplication application)
		{
		}
		
		


		
		/// <summary>
		/// Extends the new UITableViewSource in MonoTouch 1.2 (4-Nov-09)
		/// </summary>
		private class TableViewSource : UITableViewSource
		{
            static NSString kCellIdentifier = new NSString ("MyIdentifier");
			private Posts posts;
			
            public TableViewSource (Posts p)
            {
				if (p == null)
					this.posts = new Posts {data = new List<Post>()};
				else
	                this.posts = p;
            }
			
			public override int RowsInSection (UITableView tableview, int section)
            {
                return posts.data.Count;
            }

            public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
            {
                UITableViewCell cell = tableView.DequeueReusableCell (kCellIdentifier);
                if (cell == null)
                {
                    cell = new UITableViewCell (UITableViewCellStyle.Subtitle, kCellIdentifier);
                }
                cell.TextLabel.Text = posts.data[indexPath.Row].@from.name;
				cell.DetailTextLabel.Text = posts.data[indexPath.Row].message;
				cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
                return cell;
            }
			
			/// <summary>
			/// Display selected post details
			/// </summary>
			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
            {
				var uivc = new PostViewController(posts.data[indexPath.Row]);
				uivc.Title = posts.data[indexPath.Row].@from.name;
				AppDelegate.Current.NavController.PushViewController(uivc,true);
			}
		}
	}
}