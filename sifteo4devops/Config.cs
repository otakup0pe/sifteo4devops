using System;
using Sifteo;

namespace sifteo4devops
{
	public class Config
	{
		public string BaseJenkinsURL = "http://monolith.eghetto.intra:8080/";
		public string BaseZenossURL = "http://neuromancer.catghetto.home:8080/";
		public int CycleEvery = 5;

		public static Color DisabledJobColor = new Color(255, 0, 0);
		public static Color EnabledJobColor = new Color(0, 255, 0);

	}
}