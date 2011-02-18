using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Facebook
{
	public class PostViewController: UIViewController
	{
		Post post;
		
		public PostViewController (Post p) : base()
		{
			post = p;
		}
		
		public UITextView textView;
		public UIWebView webView;
		
		public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
			// no XIB !
			webView = new UIWebView()
			{
				ScalesPageToFit = false
			};
			webView.LoadHtmlString(FormatText(), new NSUrl());
			
			// Set the web view to fit the width of the app.
            webView.SizeToFit();

            // Reposition and resize the receiver
            webView.Frame = new RectangleF (0, 0, this.View.Bounds.Width, this.View.Bounds.Height);

            // Add the table view as a subview
            this.View.AddSubview(webView);
			
		}		
		/// <summary>
		/// Format the restaurant text for UIWebView
		/// </summary>
		private string FormatText()
		{
			StringBuilder sb = new StringBuilder();
			
			sb.Append(@"<style>
body,b,p{font-family:Helvetica;font-size:14px}
</style>");

//sb.Append("<span style='font-size:28px;font-weight:bold;'>" + post.@from.name + "</span>" + Environment.NewLine);
//sb.Append("<span style='float:right;size:10px;color:#555555;'>" + post. + "</span>" + Environment.NewLine);

if (!String.IsNullOrEmpty(post.icon))
	sb.Append("<img src='"+ post.icon +"' align='right' />" + Environment.NewLine);

sb.Append("<p>"+ post.name +"</p>" + Environment.NewLine);
sb.Append("<p>"+ post.caption +"</p>" + Environment.NewLine);
sb.Append("<p>"+ post.description +"</p>" + Environment.NewLine);


sb.Append("<br/>" + post.message + "<br/><br/>" + Environment.NewLine);
			
if (!String.IsNullOrEmpty(post.picture))
	sb.Append("<img src='"+ post.picture +"' />" + Environment.NewLine);
			

if (post.likes > 0 )
	sb.Append("<br /><i>likes: "+ post.likes +"</i>" + Environment.NewLine);

if (!String.IsNullOrEmpty(post.attribution))
	sb.Append("<i>via "+ post.attribution +"</i>" + Environment.NewLine);

//sb.Append("<div style='background-color:#FECF7F;padding:8px;'>" + Environment.NewLine);
//sb.Append("<div style='size:10px'><b>HOURS</b></div>" + Environment.NewLine);
//sb.Append(rest.Hours.Replace("\n","<br/>") + "<br/>" + Environment.NewLine);
//sb.Append("<div style='size:10px;padding:10 0 0 0;'><b>CARD TYPES ACCEPTED</b></div>" + Environment.NewLine);
//sb.Append(rest.CreditCards.Replace("\n","<br/>") + "<br/>" + Environment.NewLine);
//sb.Append("<div style='size:10px;padding:10 0 0 0;'><b>CHEF</b></div>" + Environment.NewLine);
//sb.Append(rest.Chef + "<br/>" + Environment.NewLine);
//sb.Append("</div>" + Environment.NewLine);
sb.Append("<br/>");
//sb.Append("<br/>");
			
			return sb.ToString();
		}
	}
}

