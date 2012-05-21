using Sifteo;
using System;

namespace sifteo4devops
{
  public class Deployinator : BaseApp
  {
	Jenkins jenkins;
    override public void Setup()
    {
			jenkins = new Jenkins();
    }

    override public void Tick()
    {
      Log.Debug("HelloWorldApp.Tick()");
      // override BaseApp.FrameRate to change the frequency of Tick()
    }
  }
}