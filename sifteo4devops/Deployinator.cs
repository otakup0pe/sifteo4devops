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

          ButtonEventHandler OnSelectButton;

          override public void Setup()
          {
               Config = new Config();
               Jenkins = new Jenkins(Config.BaseJenkinsURL);
               CubeJobs = new Dictionary<Cube, JenkinsJob>();
               OnSelectButton = new ButtonEventHandler(SelectButtonClick);
               for ( int i = 0 ; i < this.CubeSet.Count ; i ++ )
                    {
                         this.CubeSet[i].ButtonEvent += OnSelectButton;
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
               int JobInc = this.LastJob;
               CubeJobs.Clear();
               for ( int i = 0; i < ActiveCubes ; i ++ )
                    {
                         Cube c = CycleCubes[i];
                         if ( i + this.LastJob >= ActiveJobs )
                              {
                                   DrawBlankCube(c);
                                   JobInc = 0;
                              }
                         else
                              {
                                   JenkinsJob j = Jenkins.Job(i + this.LastJob);
                                   Log.Debug("updating job " + j.GetName() + " on " + c.UniqueId);
                                   DrawJobCube(c, j, Config.EnabledJobColor);
                                   CubeJobs.Add(c, j);
                                   JobInc = i;
                              }
                         c.Paint();
                    }
               if ( ActiveCubes < ActiveJobs )
                    {
                         this.LastJob = JobInc + 1;
                    }
          }

          private void DrawBlankCube(Cube c)
          {
               c.FillScreen(Color.White);
               c.Image("difficulties", 0, 16, 0, 0);
          }

          private void DrawJobCube(Cube c, JenkinsJob j, Color Col)
          {
               c.FillScreen(Col);
               Util.DrawString(c, 0, 0, "Job:" + j.GetName());
               Util.DrawString(c, 0, 10, "Score:" + j.GetScore().ToString());
               Util.DrawString(c, 0, 20, "Last Success:" + j.GetLastSuccess().ToString());
               Util.DrawString(c, 0, 30, "Last Fail:" + j.GetLastFail().ToString());
          }
     }
}