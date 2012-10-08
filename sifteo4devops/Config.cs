using System;
using Sifteo;
using Nini.Config;

namespace sifteo4devops
{

  public class SetupCube
  {
    public enum Screen
    {
      Exit, DeployJob, JenkinsJobs, ZenossGroups
    };
    public bool Active = false;

    private Cube c;
    public Cube Cube
    {
      set
        {
          if ( value == null )
            {
              this.c.ButtonEvent -= this.DoButton;
              this.c.TiltEvent -= this.OnTilt;
	      this.Active = false;
              this.LoadingTime = 0;
	      this.CurrentScreen = Screen.Exit;
	      Log.Debug("reset setupcube");
            }
          else
            {
	      Log.Debug("new setupcube");
              this.LastLoadingTime = DateTime.Now;
              value.ButtonEvent += this.OnButton;
              value.TiltEvent += this.OnTilt;
              this.Tick();
            }
          this.c = value;
        }
      get
        {
          if ( this.c == null )
            {
              return null;
            }
          else
            {
              return this.c;
            }
        }
    }

    private int LoadingTime = 5;
    private DateTime LastLoadingTime;
    private Screen CurrentScreen = Screen.Exit;

    private ButtonEventHandler OnButton;
    private TiltEventHandler OnTilt;

    private int Element = 0;

    public SetupCube()
    {
      OnButton = new ButtonEventHandler(DoButton);
      OnTilt = new TiltEventHandler(DoTilt);
    }

    public void Loading()
    {
      TimeSpan setupspan = DateTime.Now - this.LastLoadingTime;
      if ( setupspan.Seconds > 5 )
        {
          this.Active = true;
          this.Redraw();
        }
      else
        {
          if ( setupspan.Seconds > this.LoadingTime + 1)
            {
              int color = 50 * this.LoadingTime;
              Color col = new Color(255, color, color);
              this.c.FillScreen(col);
              Util.DrawString(c, 5, 25, "Hold For Setup" + (5 - this.LoadingTime).ToString() + "s");
              Util.DrawString(c, 5, 35, "To enter Setup Mode");
              this.c.Paint();
              this.LoadingTime += 1;
            }
        }
    }
    public bool Tick()
    {
      if ( this.c != null )
        {
          if ( ! this.Active )
            {
              this.Loading();
            }
          return true;
        }
      else
        {
          return false;
        }
    }

    public void DoButton(Cube c, bool pressed)
    {
      if ( CurrentScreen == Screen.Exit )
        {
          if ( pressed )
            {
              c = null;
            }
        }
    }

    public void DoTilt(Cube c, int x, int y, int z)
    {

    }

    private void Redraw()
    {
      Color col = new Color(255, 255, 255);
      this.c.FillScreen(col);
      Util.DrawString(c, 5, 5, "@~+~~ Config");
      if ( CurrentScreen == Screen.Exit )
        {
          Util.DrawString(c, 5, 15, "Click to Exit");
        }
      else if ( CurrentScreen == Screen.DeployJob )
        {
          if ( this.Element < Deployinator.Jenkins.Count() && this.Element >= 0 )
            {
              this.Element = 0;
            }
          JenkinsJob j = Deployinator.Jenkins.Job(this.Element);
          Util.DrawString(c, 5, 15, "Click to select");
          Util.DrawString(c, 5, 25, j.GetName());
        }
    }
  }

  public class Config
  {
    private string _DeployJob;
    public string DeployJob
    {
      get
        {
          return _DeployJob;
        }
      set
        {
          this._DeployJob = value;
        }
    }

    private bool _Danger;
    public bool Danger
    {
      get
        {
          return _Danger;
        }
      set
        {
          this._Danger = value;
        }
    }

    private string _JenkinsUrl;
    public string JenkinsUrl
    {
      get
        {
          return _JenkinsUrl;
        }
    }

    private string _ZenossUrl;
    public string ZenossUrl
    {
      get
        {
          return _ZenossUrl;
        }
    }
    private string _ZenossUser;
    public string ZenossUser
    {
      get
        {
          return _ZenossUser;
        }
    }
    private string _ZenossPass;
    public string ZenossPass
    {
      get
        {
          return _ZenossPass;
        }
    }

    private int _CycleEvery;
    public int CycleEvery
    {
      get
        {
          return _CycleEvery;
        }
      set
        {
          this._CycleEvery = value;
        }
    }

    private int _ScoreEvery;
    public int ScoreEvery
    {
      get
        {
          return _ScoreEvery;
        }
      set
        {
          this._ScoreEvery = value;
        }
    }
    

    public Config()
    {
      IConfigSource source = new IniConfigSource("deployinator.ini");
      string d = source.Configs["Displays"].Get("Danger");
      if ( d == "true" )
        {
          this._Danger = true;
        }
      else
        {
          this._Danger = false;
        }
      this._JenkinsUrl = source.Configs["Jenkins"].Get("URL");
      this._ZenossUrl = source.Configs["Zenoss"].Get("URL");
      this._ZenossUser = source.Configs["Zenoss"].Get("User");
      this._ZenossPass = source.Configs["Zenoss"].Get("Pass");
      this._CycleEvery = Convert.ToInt32(source.Configs["Displays"].Get("CycleEvery"));
      this._ScoreEvery = Convert.ToInt32(source.Configs["Displays"].Get("ScoreEvery"));
      this._DeployJob = source.Configs["Displays"].Get("DeployJob");

      for ( int i = 0 ; i < source.Configs.Count ; i++ )
        {
          Log.Debug(source.Configs[i].ToString());
        }
    }
  }
}