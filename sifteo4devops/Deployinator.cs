using Sifteo;
using System;
using System.Collections.Generic;

namespace sifteo4devops
{
     public class Deployinator : BaseApp
     {
          Jenkins Jenkins;
          static Config Config;

          private DateTime LastCycleJobs;
          private DateTime LastSelectedJob;

          int LastJob = 0;

          Cube SelectedCube;
          int CurrentFade = 0;

          Dictionary<Cube, JenkinsJob> CubeJobs;
		Dictionary<Cube, DoomGuy> CubeDooms;

          ButtonEventHandler OnSelectButton;

          override public void Setup()
          {
               Config = new Config();
               Jenkins = new Jenkins(Config.BaseJenkinsURL);
               CubeJobs = new Dictionary<Cube, JenkinsJob>();
			CubeDooms = new Dictionary<Cube, DoomGuy>();
               OnSelectButton = new ButtonEventHandler(SelectButtonClick);
               for ( int i = 0 ; i < this.CubeSet.Count ; i ++ )
                    {
                         this.CubeSet[i].ButtonEvent += OnSelectButton;
					CubeDooms.Add(this.CubeSet[i], new DoomGuy());
					this.CubeSet[i].FillScreen(Color.White);
                    }
          }

          public void SelectButtonClick(Cube c, bool pressed)
          {
			if ( pressed && SelectedCube == c )
				{
					Log.Debug(c.UniqueId + " unselected");
					this.SelectedCube = null;
					this.CurrentFade = 0;
				}

               else if ( pressed && SelectedCube == null )
                    {
                         Log.Debug(c.UniqueId + " pressed eh");
                         if ( this.CubeJobs.ContainsKey(c) )
                              {
                                   Log.Debug(c.UniqueId + " selected");
                                   this.SelectedCube = c;
                              }
                    }
          }

          override public void Tick()
          {
               if ( SelectedCube == null )
                    {
                         TimeSpan span = DateTime.Now - LastCycleJobs;
                         if ( span.Seconds > Config.CycleEvery )
                              {
                                   CycleJobs();
                                   LastCycleJobs = DateTime.Now;
                              }
                    }
               else
                    {
                         TimeSpan span = DateTime.Now - LastSelectedJob;
                         if ( span.Milliseconds > 100 )
                              {
                                   if ( CurrentFade >= 255 )
                                        {
                                             CurrentFade = 0;
                                        }
                                   int c = CurrentFade;
                                   Color col = new Color(255, c, c);
                                   DrawJobCube(this.SelectedCube, CubeJobs[SelectedCube], col);
                                   this.SelectedCube.Paint();
                                   CurrentFade += 32;
                              }
                    }
          }

          static void Main(string[] args)
          {
               new Deployinator().Run();
          }

          private void CycleJobs()
          {
               CubeSet CycleCubes = this.CubeSet;
               int ActiveCubes = CycleCubes.Count;
               int ActiveJobs = this.Jenkins.Count();
               Log.Debug("updating " + ActiveJobs.ToString() + "(" + this.LastJob.ToString() + ") jobs across " + ActiveCubes.ToString() + " cubes");
			
			Log.Debug(this.LastJob.ToString() + " vs " + ActiveJobs.ToString() );
			if ( this.LastJob >= ActiveJobs )
				{
					this.LastJob = 0;
				}
			int JobInc = this.LastJob;
		
               CubeJobs.Clear();
               for ( int i = 0; i < ActiveCubes ; i ++ )
                    {
                         Cube c = CycleCubes[i];
                         if ( i + this.LastJob >= ActiveJobs )
                              {
                                   DrawBlankCube(c);
                              }
                         else
                              {
                                   JenkinsJob j = Jenkins.Job(i + this.LastJob);
                                   Log.Debug("updating job " + j.GetName() + " on " + c.UniqueId);
                                   DrawJobCube(c, j, Config.EnabledJobColor);
                                   CubeJobs.Add(c, j);
                                   JobInc += 1;
                              }
                         c.Paint();
                    }
			this.LastJob = JobInc;
          }

          private void DrawBlankCube(Cube c)
          {
			c.FillScreen(Color.White);
               c.Image("difficulties", 0, 16, 0, 0);
			CubeDooms[c].Draw(c, DoomGuy.Face.None, DoomGuy.FaceStatus.None, 0, 0);
          }

          private void DrawJobCube(Cube c, JenkinsJob j, Color Col)
          {
			c.FillScreen(Color.White);
               Util.DrawString(c, 5, 5, "Job:" + j.GetName());
               Util.DrawString(c, 5, 15, "Score:" + j.GetScore().ToString());
               Util.DrawString(c, 5, 25, "Last Success:" + j.GetLastSuccess().ToString());
               Util.DrawString(c, 5, 35, "Last Fail:" + j.GetLastFail().ToString());
			if ( j.GetScore() == 100 )
				{
					CubeDooms[c].Draw(c, DoomGuy.Face.Health1, DoomGuy.FaceStatus.Normal, 5, 50);
				}
			else if ( j.GetScore() >= 80 )
				{
					CubeDooms[c].Draw(c, DoomGuy.Face.Health2, DoomGuy.FaceStatus.Normal, 5, 50);
				}
			else if ( j.GetScore() >= 50 )
				{
					CubeDooms[c].Draw(c, DoomGuy.Face.Health3, DoomGuy.FaceStatus.Normal, 5, 50);
				}
			else if ( j.GetScore() >= 30 )
				{
					CubeDooms[c].Draw(c, DoomGuy.Face.Health4, DoomGuy.FaceStatus.Normal, 5, 50);
				}
			else if ( j.GetScore() >= 0 )
				{
					CubeDooms[c].Draw(c, DoomGuy.Face.Health5, DoomGuy.FaceStatus.Normal, 5, 50);					
				}
			else
				{
					CubeDooms[c].Draw(c, DoomGuy.Face.GameOver, DoomGuy.FaceStatus.Normal, 5, 50);
				}
          }
     }
}