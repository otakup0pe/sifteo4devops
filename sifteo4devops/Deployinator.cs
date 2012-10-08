using Sifteo;
using System;
using System.Collections.Generic;

namespace sifteo4devops
{
  public class Deployinator : BaseApp
  {

    public static Jenkins Jenkins;
    public static Zenoss Zenoss;
    public static Config Config;

    private DateTime LastCycleJobs;
    private DateTime LastScoreCalc;
    private DateTime LastSelectedJob;

    private SetupCube SetupCube = null;

    private List<string> FlippedCubes;
    private Dictionary<string, DateTime> ButtonPressed;

    int LastJob = 0;
    int LastGroup = 0;

    int Score = 0;

    Dictionary<Cube, Object> Displays;
    Dictionary<Cube, DoomGuy> CubeDooms;

    ButtonEventHandler OnButton;
    FlipEventHandler OnFlip;
    TiltEventHandler OnTilt;

    override public void Setup()
    {
      SetupCube = new SetupCube();
      Displays = new Dictionary<Cube, Object>();
      CubeDooms = new Dictionary<Cube, DoomGuy>();
      FlippedCubes = new List<string>();
      ButtonPressed = new Dictionary<string, DateTime>();
      OnButton = new ButtonEventHandler(DoButton);
      OnFlip = new FlipEventHandler(DoFlip);
      OnTilt = new TiltEventHandler(DoTilt);
      for ( int i = 0 ; i < this.CubeSet.Count ; i ++ )
        {
          this.CubeSet[i].ButtonEvent += OnButton;
          this.CubeSet[i].TiltEvent += OnTilt;
          this.CubeSet[i].FlipEvent += OnFlip;
          CubeDooms.Add(this.CubeSet[i], new DoomGuy());
          this.CubeSet[i].FillScreen(Color.White);
        }
    }

    public void DoButton(Cube c, bool pressed)
    {
      if ( FlippedCubes.Count == this.CubeSet.Count - 1 )
        {
          if ( pressed )
            {
	      CubeDooms[c].Pause = true;
              SetupCube.Cube = c;
            }
          else
            {
              if ( ! SetupCube.Active )
                {
		  CubeDooms[c].Pause = false;
                  SetupCube.Cube = null;
                }
            }
        }
      else
        {
          if ( SetupCube.Cube != null )
            {
	      CubeDooms[c].Pause = false;
              SetupCube.Cube = null;
            }
        }
      if ( pressed )
        {
          if ( ! this.ButtonPressed.ContainsKey(c.UniqueId) )
            {
              this.ButtonPressed.Add(c.UniqueId, DateTime.Now);
            }
        }
      else
        {
          if ( this.ButtonPressed.ContainsKey(c.UniqueId) )
            {
              this.ButtonPressed.Remove(c.UniqueId);
            }
        }
      Log.Debug("A CLICK -> " + c.UniqueId.ToString() + " : " + pressed.ToString());
    }

    public void DoFlip(Cube c, bool isUp)
    {

      if ( ! isUp )
        {
          if ( ! FlippedCubes.Contains(c.UniqueId) )
            {
              FlippedCubes.Add(c.UniqueId);
            }
        }
      else
        {
          if ( FlippedCubes.Contains(c.UniqueId) )
            {
	      if ( SetupCube.Active )
		{
		  CubeDooms[SetupCube.Cube].Pause = false;
		  SetupCube.Cube = null;
		}
              FlippedCubes.Remove(c.UniqueId);
            }
        }
      Log.Debug("A FLIP -> " + c.UniqueId + " : " + isUp.ToString() + " " + FlippedCubes.Count.ToString() + " flipped");
    }

    public void DoTilt(Cube c, int x, int y, int z)
    {
      Log.Debug("A TILT -> " + c.UniqueId.ToString() + " : " + x.ToString() + "," + y.ToString() + "," + z.ToString());
    }

    override public void Tick()
    {
      TimeSpan span = DateTime.Now - LastCycleJobs;
      if ( SetupCube.Cube != null )
        {
          if ( this.FlippedCubes.Count == this.CubeSet.Count - 1 && this.ButtonPressed.Count == 1 )
            {
              SetupCube.Tick();
            }
        }
      else
        {
          if ( span.Seconds > Config.CycleEvery )
            {
              CycleJobs();
              LastCycleJobs = DateTime.Now;
            }
        }
      this.CalculateScore();
    }

    private void CalculateScore()
    {
      TimeSpan span = DateTime.Now - LastScoreCalc;
      if ( span.Seconds > Config.ScoreEvery )
        {
          int _iscore = 0;
          for ( int i = 0 ; i < Jenkins.Count() ; i ++ )
            {
              if ( Jenkins.Job(i).GetScore() == 100 )
                {
                  _iscore += 10;
                }
              else if ( Jenkins.Job(i).GetScore() >= 80 )
                {
                  _iscore += 2;
                }
            }
          for ( int i = 0 ; i < Zenoss.Count() ; i ++ )
            {
              if ( Zenoss.Group(i).Problems() )
                {
                  _iscore += 10;
                }
            }
          this.Score += _iscore;
          Log.Debug("Adding " + _iscore.ToString() + " for a new high score of " + Score.ToString());
          LastScoreCalc = DateTime.Now;
        }
    }

    static void Main(string[] args)
    {
      Config = new Config();
      Jenkins = new Jenkins();
      Zenoss = new Zenoss();

      new Deployinator().Run();
    }

    private void CycleJobs()
    {
      Log.Debug("updating jenkins jobs : " + Deployinator.Jenkins.Count().ToString() + "(" + this.LastJob.ToString() + ") zenoss groups : " + Deployinator.Zenoss.Count().ToString() + "(" + this.LastGroup.ToString() + ") across " + this.CubeSet.Count.ToString() + " cubes");

      if ( this.LastJob >= Deployinator.Jenkins.Count() )
        {
          this.LastJob = 0;
        }
      if ( this.LastGroup >= Deployinator.Zenoss.Count() )
        {
          this.LastGroup = 0;
        }

      Displays.Clear();

      for ( int i = 0; i < this.CubeSet.Count ; i ++ )

        {
          Cube c = this.CubeSet[i];
          if ( ! FlippedCubes.Contains(c.UniqueId) )
            {
              DrawCube(c);
            }
        }
    }

    private void DrawCube(Cube c)
    {
      CubeDooms[c].Reset();
      Random r = new Random();
      int b = r.Next(2);
      if ( b == 0 && this.LastJob < Deployinator.Jenkins.Count() )
        {
          JenkinsJob j = Deployinator.Jenkins.Job(this.LastJob);
          j.Draw(c, CubeDooms[c]);
          this.LastJob = this.LastJob + 1;
          this.Displays[c] = j;
        }
      else if ( b == 1 && this.LastGroup < Deployinator.Zenoss.Count() )
        {
          ZenossGroup k = Deployinator.Zenoss.Group(this.LastGroup);
          k.Draw(c, CubeDooms[c]);
          this.LastGroup = this.LastGroup + 1;
          this.Displays[c] = k;
        }
      else
        {
          DrawBlankCube(c);
        }
      c.Paint();

    }
    private void DrawBlankCube(Cube c)
    {
      c.FillScreen(Color.White);
      c.Image("difficulties", 0, 16, 0, 0);
    }
  }
}