using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using JsonFx.Json;
using Sifteo;

namespace sifteo4devops
{

     public class JenkinsJob
     {
          protected string Name;
          protected int LastSuccess;
          protected int LastFail;
          protected int Score;

          private Timer RefreshTimer;
          private TimerCallback RefreshCallback;

		public void Draw(Cube c, DoomGuy g)
		{
			c.FillScreen(Color.White);
               Util.DrawString(c, 5, 5, "Job:" + this.Name);
               Util.DrawString(c, 5, 15, "Score:" + this.Score.ToString());
               Util.DrawString(c, 5, 25, "Last Success:" + this.LastSuccess.ToString());
			Util.DrawString(c, 5, 35, "Last Fail:" + this.LastFail.ToString());

			c.Image("jenkins", 5, 60, 0, 0, 32, 44);
 			if ( this.Score == 100 )
				{
					g.Draw(c, DoomGuy.Face.Health1, DoomGuy.FaceStatus.Normal, 50, 60);
				}
			else if ( this.Score >= 80 )
				{
					g.Draw(c, DoomGuy.Face.Health2, DoomGuy.FaceStatus.Normal, 50, 60);
				}
			else if ( this.Score >= 60 )
				{
					g.Draw(c, DoomGuy.Face.Health3, DoomGuy.FaceStatus.Normal, 50, 60);
				}
			else if ( this.Score >= 30 )
				{
					g.Draw(c, DoomGuy.Face.Health4, DoomGuy.FaceStatus.Normal, 50, 60);
				}
			else if ( this.Score > 0 )
				{
					g.Draw(c, DoomGuy.Face.Health5, DoomGuy.FaceStatus.Normal, 50, 60);					
				}
			else
				{
					g.Draw(c, DoomGuy.Face.GameOver, DoomGuy.FaceStatus.Normal, 50, 60);
				}
		}
		
          public JenkinsJob(string Name)
          {
               this.Name = Name;
               RefreshCallback = this.Refresh;
               Log.Info("New jenkins job " + Name);
               this.RefreshTimer = new Timer(RefreshCallback, null, 0, 60 * 1000);
          }

          public string GetName()
          {
               return this.Name;
          }
		
		public int GetScore()
		{
			return this.Score;
		}

		public int GetLastSuccess()
		{
			return this.LastSuccess;
		}

		public int GetLastFail()
		{
			return this.LastFail;
		}

          public void Refresh(Object State)
          {
               if ( ! this.Request() )
                    {
                         Log.Error("unable to refresh jenkins job " + Name);
                    }
               else
                    {
                         Log.Debug("Job:" + this.Name + " Score:" + this.Score.ToString());
                    }

          }

          public bool Request()
          {
               HttpWebRequest JobReq = (HttpWebRequest)WebRequest.Create(Deployinator.Config.JenkinsUrl + "job/" + this.Name + "/api/json");
               HttpWebResponse JobResp = (HttpWebResponse)JobReq.GetResponse();
               if (JobResp.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                         System.IO.Stream ResponseStream = JobResp.GetResponseStream();
                         System.IO.StreamReader StreamReader = new System.IO.StreamReader(ResponseStream);
                         ParseJobJSON(StreamReader.ReadToEnd());
                         return true;
                    }
               else
                    {
                         return false;
                    }
          }

          private void ParseJobJSON(String Json)
          {
               JsonReader Reader = new JsonReader();
               Dictionary<string, Object> JsonDict = Reader.Read<Dictionary<string, Object>>(Json);
               Object[] HealthReport = (Object[]) JsonDict["healthReport"];
               if ( HealthReport.Length == 0 )
                    {
                         this.Score = 0;
                    }
               else
                    {
                         Dictionary<string, Object> JsonHealth = (Dictionary<string, Object>) HealthReport[0];
                         if ( JsonHealth.ContainsKey("score") )
                              {
                                   this.Score = (int) JsonHealth["score"];
                              }
                         else
                              {
                                   this.Score = 0;
                              }
                    }
               this.LastSuccess = ExtractJobJSON("lastSuccessfulBuild", JsonDict);
               this.LastFail = ExtractJobJSON("lastFailedBuild", JsonDict);
          }

          private int ExtractJobJSON(string BuildType, Dictionary<string, Object> Json)
          {
               if ( ! Json.ContainsKey(BuildType) )
                    {
                         return 0;
                    }
               if ( Json[BuildType] == null )
                    {
                         return 0;
                    }
               Dictionary<string, Object> BuildJson = (Dictionary<string, Object>) Json[BuildType];
               if ( BuildJson != null )
                    {
                         if ( BuildJson["number"] == null )
                              {
                                   return 0;
                              }
                         else
                              {
                                   return (int) BuildJson["number"];
                              }
                    }
               else
                    {
                         return 0;
                    }

          }

          ~JenkinsJob()
          {
               RefreshTimer.Dispose();
          }
     }

     public class Jenkins
     {
          private List<JenkinsJob> Jobs;

          private Timer RefreshTimer;
          private TimerCallback RefreshCallback;

		public int Count()
		{
			return Jobs.Count;
		}

		public JenkinsJob Job(int i)
		{
			return Jobs[i];
		}

          public Jenkins ()
          {
               this.Jobs = new List<JenkinsJob>();
               RefreshCallback = this.Refresh;
               this.RefreshTimer = new Timer(RefreshCallback, null, 0, 300 * 1000);
          }

          public void Refresh(Object State)
          {
               List<string> JobList = this.JobList();
               for ( int i = 0 ; i < JobList.Count ; i++ )
                    {
                         String Job = JobList[i];
                         if ( ! isJob(Job) )
                              {
                                   this.Jobs.Add(new JenkinsJob(Job));
                              }
                    }

          }

          private bool isJob(String Name)
          {
               for ( int i = 0 ; i < this.Jobs.Count ; i++ )
                    {
                         if ( this.Jobs[i].GetName() == Name )
                              {
                                   return true;
                              }
                    }
               return false;
          }

          private List<string> JobList()
          {
               List<string> Jobs = new List<string>();
               string URL = Deployinator.Config.JenkinsUrl + "/api/json";
               Log.Info("requesting " + URL);
               HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(URL);
               HttpWebResponse Resp = (HttpWebResponse)Req.GetResponse();
               if ( Resp.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                         System.IO.Stream ResponseStream = Resp.GetResponseStream();
                         System.IO.StreamReader StreamReader = new System.IO.StreamReader(ResponseStream);
                         JsonReader Reader = new JsonReader();
                         String Json = StreamReader.ReadToEnd();
                         Dictionary<string, Object> JsonDict = Reader.Read<Dictionary<string, Object>>(Json);
                         Dictionary<string, Object>[] JsonJobs = (Dictionary<string, Object>[]) JsonDict["jobs"];
                         for ( int i = 0 ; i < JsonJobs.Length ; i++ )
                              {
                                   Dictionary<string, Object> JsonJob = (Dictionary<string, Object>) JsonJobs[i];
                                   Jobs.Add((string)JsonJob["name"]);
                              }
                    }
               else
                    {
                         Log.Info("returned non 200 " + Resp.StatusCode.ToString());
                    }
               return Jobs;
          }

          ~Jenkins()
          {
               RefreshTimer.Dispose();
          }

     }
}
