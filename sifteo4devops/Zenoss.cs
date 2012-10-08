using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using JsonFx.Json;
using Sifteo;
using System.Text;

namespace sifteo4devops
{
  public class Zenoss
  {
    private Timer RefreshTimer;
    private TimerCallback RefreshCallback;

    protected List<ZenossGroup> Groups;

    public Zenoss()
    {
      this.Groups = new List<ZenossGroup>();
      RefreshCallback = this.Refresh;
      this.RefreshTimer = new Timer(RefreshCallback, null, 0, 300 * 1000);
    }

    public ZenossGroup Group(int i)
    {
      return Groups[i];
    }

    public void Refresh(Object state)
    {
      if ( ! this.Request() )
        {
          Log.Error("unable to refresh zenoss groups");
        }
      /*else
        {
        Log.Debug("Refreshed " + this.Groups.Count.ToString() + " Zenoss groups");
        }*/
    }

    public int Count()
    {
      return this.Groups.Count;
    }

    public bool Request()
    {
      try
        {
          string Thing = "[{\"action\":\"DeviceRouter\",\"method\":\"getGroups\",\"data\":[],\"type\":\"rpc\",\"tid\":21}]";
          byte[] Body = Encoding.ASCII.GetBytes(Thing);
          HttpWebRequest ZenReq = (HttpWebRequest)WebRequest.Create(Deployinator.Config.ZenossUrl + "zport/dmd/device_router");
          string authInfo = Deployinator.Config.ZenossUser + ":" + Deployinator.Config.ZenossPass;
          ZenReq.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(authInfo)));
          ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(Util.AcceptAllCertifications);
          ZenReq.Method = "POST";
          ZenReq.ContentLength = Body.Length;
          ZenReq.ContentType = "application/json";
          ZenReq.Expect = "";
          System.IO.Stream DStream = ZenReq.GetRequestStream();
          DStream.Write(Body, 0, Body.Length);

          //Log.Debug("making request of " + Thing);
          HttpWebResponse ZenResp = (HttpWebResponse)ZenReq.GetResponse();

          if (ZenResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
              System.IO.Stream ResponseStream = ZenResp.GetResponseStream();
              System.IO.StreamReader StreamReader = new System.IO.StreamReader(ResponseStream);
              return ParseZenossGroupsJSON(StreamReader.ReadToEnd());
            }
          else
            {
              Log.Error("non ok status return " + ZenResp.StatusCode.ToString());
              return false;
            }
        }
      catch ( System.Exception e )
        {
          Log.Error("something awful happened " + e.ToString());
          return false;
        }
    }

    private bool ParseZenossGroupsJSON(String Json)
    {
      //Log.Debug("parsing json " + Json);
      JsonReader Reader = new JsonReader();
      //Log.Debug("what " + Reader.ToString());
      try
        {
          Dictionary<string, Object> JsonDict = Reader.Read<Dictionary<string, Object>>(Json);
          if ( JsonDict.ContainsKey("result") )
            {
              Log.Debug("ok then " + JsonDict["result"].ToString());
              Dictionary<string, Object> JsonResults = (Dictionary<string, Object>) JsonDict["result"];
              Object[] JsonGroups = (Object[]) JsonResults["groups"];

              if ( JsonGroups.Length >= 0 )
                {
                  for ( int i = 0 ; i < JsonGroups.Length ; i++ )
                    {
                      Dictionary <string, Object> GroupJson = (Dictionary<string, Object>) JsonGroups[i];
                      string GroupName = (string) GroupJson["name"];
                      if ( ! GroupContains(GroupName, Groups) )
                        {
                          ZenossGroup g = new ZenossGroup(GroupName);
                          Groups.Add(g);
                        }
                    }
                  return true;
                }
            }
          else
            {
              Log.Error("invalid json?");
            }
        }
      catch (System.Exception e )
        {
          Log.Error("something awful happened " + e.ToString());
        }
      return false;
    }

    private bool GroupContains(string CheckName, List<ZenossGroup> Groups)
    {
      for ( int i = 0 ; i < Groups.Count ; i ++ )
        {
          if ( CheckName == Groups[i].Name )
            {
              return true;
            }
        }
      return false;
    }

    ~Zenoss()
    {
      RefreshTimer.Dispose();
    }


  }

  public class ZenossGroup
  {
    private Timer RefreshTimer;
    private TimerCallback RefreshCallback;

    private string _Name;
    public string Name
    {
      get
        {
          return this._Name;
        }
    }
    protected string State;
    protected int Info;
    protected int Warning;
    protected int Error;
    protected int DeviceCount;
    protected int MaintenanceCount;

    public bool Problems()
    {
      if ( this.Warning == 0 && this.Error == 0 )
	{
	  return false;
	}
      else
	{
	  return true;
	}
    }
    public ZenossGroup(string Name)
    {
      this._Name = Name;
      RefreshCallback = this.Refresh;
      //Log.Debug("new zenoss group " + Name);
      this.RefreshTimer = new Timer(RefreshCallback, null, 0, 60 * 1000);
    }

    public void Refresh(Object state)
    {
      if ( ! this.Request() )
        {
          Log.Error("unable to refresh zenoss group " + this.Name);
        }
      /*else
        {
        Log.Debug("Refreshed Zenoss group " + this.Name + " with " + this.DeviceCount.ToString() + " devices");
        }*/
    }

    public bool Request()
    {
      try
        {
          string Thing = "[{\"action\":\"DeviceRouter\",\"method\":\"getDevices\",\"data\":[{\"keys\":[\"name\", \"productionState\", \"events\"], \"params\":\"{}\",\"uid\":\"/zport/dmd/Groups" + this._Name + "\"}],\"type\":\"rpc\",\"tid\":21}]";
          byte[] Body = Encoding.ASCII.GetBytes(Thing);
          HttpWebRequest ZenReq = (HttpWebRequest)WebRequest.Create(Deployinator.Config.ZenossUrl + "zport/dmd/device_router");
          string authInfo = Deployinator.Config.ZenossUser + ":" + Deployinator.Config.ZenossPass;
          ZenReq.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(authInfo)));
          ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(Util.AcceptAllCertifications);
          ZenReq.Method = "POST";
          ZenReq.ContentLength = Body.Length;
          ZenReq.ContentType = "application/json";
          ZenReq.Expect = "";
          System.IO.Stream DStream = ZenReq.GetRequestStream();
          DStream.Write(Body, 0, Body.Length);

          //Log.Debug("sending " + Thing);
          HttpWebResponse ZenResp = (HttpWebResponse)ZenReq.GetResponse();

          if (ZenResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
              System.IO.Stream ResponseStream = ZenResp.GetResponseStream();
              System.IO.StreamReader StreamReader = new System.IO.StreamReader(ResponseStream);
              ParseZenossGroupJSON(StreamReader.ReadToEnd());
              return true;
            }
          else
            {
              return false;
            }
        }
      catch ( System.Exception e )
        {
          Log.Error("something awful happened " + e.ToString());
          return false;
        }
    }

    private void ParseZenossGroupJSON(String Json)
    {
      //Log.Debug("Results were " + Json);
      JsonReader Reader = new JsonReader();
      try
        {
          Dictionary<string, Object> JsonDict = Reader.Read<Dictionary<string, Object>>(Json);
          Dictionary<string, Object> JsonResults = (Dictionary<string, Object>) JsonDict["result"];
          //Log.Debug("has result");
          Object[] Devs = (Object[]) JsonResults["devices"];
          //Log.Debug("has devices");

          this.DeviceCount = 0;
          this.MaintenanceCount = 0;
          this.Info = 0;
          this.Warning = 0;
          this.Error = 0;


          if ( Devs.Length == 0 )
            {
              //                                   Log.Debug("no devices");
            }
          else
            {
              //                                 Log.Debug(Devs.Length.ToString() + " devices");
              for ( int i = 0 ; i < Devs.Length ; i++ )
                {
                  Dictionary<string, Object> DevJson = (Dictionary<string, Object>) Devs[i];
                  string DevState = (string) DevJson["productionState"];
                  this.DeviceCount = this.DeviceCount + 1;
                  if ( DevState == "maintenance" )
                    {
                      this.MaintenanceCount = this.MaintenanceCount + 1;
                    }
                  Dictionary<string, Object> DevEvents = (Dictionary<string, Object>) DevJson["events"];
                  this.Info = this.Info + ( int ) DevEvents["info"];
                  this.Warning = this.Warning + ( int ) DevEvents["warning"];
                  this.Error = this.Error + ( int ) DevEvents["error"];
                  this.Error = this.Error + ( int ) DevEvents["critical"];
                }
            }
        }
      catch ( System.Exception e )
        {
          //                         Log.Debug( " something awful happened " + e.ToString());
        }
    }

    public void Draw(Cube c, DoomGuy g)
    {
      c.FillScreen(Color.White);
      Util.DrawString(c, 5, 5, "Group:" + this._Name);
      Util.DrawString(c, 5, 15, "Devices: " + this.DeviceCount.ToString() + " (" + this.MaintenanceCount.ToString() + ")");
      Util.DrawString(c, 5, 25, "Info:" + this.Info.ToString());
      Util.DrawString(c, 5, 35, "Warn:" + this.Warning.ToString());
      Util.DrawString(c, 5, 45, "Error:" + this.Error.ToString());

      c.Image("zenoss", 5, 60, 0, 0, 32, 42);

      if ( this.DeviceCount == 0 )
        {
          g.Draw(c, DoomGuy.Face.GameOver, DoomGuy.FaceStatus.Normal, 50, 60);
        }
      else
        {

          if ( this.Error >= 10 )
            {
              g.Draw(c, DoomGuy.Face.Health5, DoomGuy.FaceStatus.Normal, 50, 60);
            }
          else if ( this.Error >= 1 )
            {
              g.Draw(c, DoomGuy.Face.Health4, DoomGuy.FaceStatus.Normal, 50, 60);
            }
          else if ( this.Warning >= 10 )
            {
              g.Draw(c, DoomGuy.Face.Health3, DoomGuy.FaceStatus.Normal, 50, 60);
            }
          else if ( this.Warning >= 1 )
            {
              g.Draw(c, DoomGuy.Face.Health2, DoomGuy.FaceStatus.Normal, 50, 60);
            }
          else
            {
              g.Draw(c, DoomGuy.Face.Health1, DoomGuy.FaceStatus.Normal, 50, 60);
            }
        }
    }

    ~ZenossGroup()
    {
      RefreshTimer.Dispose();
    }

  }
}
