using System;
using MonoTouch.Foundation;
using System.Collections.Generic;

namespace Facebook
{
	[MonoTouch.Foundation.Preserve]
	public class Post
	{
		public string id;
		public string message;
		public Person @from;
		public string picture;
		public string link;
		public string source;
		public string type;
		public string attribution;
		public string name;
		public string caption;
		public string description;
		public string created_time;
		public string updated_time;
		public string icon;
		public List<Action> actions;
		public int likes;
	}
	[MonoTouch.Foundation.Preserve]
	public class Person
	{
		public string name;
		public string id;
	}
	[MonoTouch.Foundation.Preserve]
	public class Action
	{
		public string name;
		public string link;
	}
	[MonoTouch.Foundation.Preserve]
	public class Posts
	{
		public List<Post> data;
	}
}

