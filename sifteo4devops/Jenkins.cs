using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;

namespace sifteo4devops
{
	
	public class JenkinsJob
	{
		protected string Name;
		protected int LastSuccess;
		protected int LastFail;
		protected string Icon;
		
		private string URL;
		
		public JenkinsJob(string BaseURL, string Name)
		{
			this.Name = Name;
			this.URL = BaseURL + "/jobs/" + Name + "/api/json";
		}
		
		public bool Refresh(string BaseURL)
		{
			HttpWebRequest JobReq = (HttpWebRequest)WebRequest.Create("http://www.google.com/");
			return false;
		}
		
	}

	public class Jenkins
	{
		private List<JenkinsJob> Jobs;
		private string BaseURL;

		public Jenkins (string BaseURL)
		{
			this.BaseURL = BaseURL;
		}
	}
}

