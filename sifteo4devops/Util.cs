using System;
using Sifteo;
using System.Threading;

namespace sifteo4devops
{
     class DoomGuyState
     {
          public int PicY;
          public Cube Cube;
          public int X;
          public int Y;
     }
	class SirenState
	{
		public int Step;
		public int X;
		public int Y;
	}
     public class Util
     {
		public static bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

          public static void DrawString(Cube c, int x, int y, String s)
          {
               int cur_x = x, cur_y = y;
               for(int i = 0; i < s.Length; ++i)
                    {
                         char ascii = s[i];
                         if(s[i] == '\n')
                              {
                                   cur_y += 10;
                                   cur_x = x;
                              }

                         else if(s[i] == ' ')
                              cur_x += 6;

                         else
                              {

                                   c.Image("xterm610", cur_x, cur_y, (ascii % 16) * 6, (ascii / 16) * 10, 6, 10, 1, 0);
                                   cur_x += 6;
                              }
                    }

          }
     }

     public class DoomGuy
     {
          public enum FaceStatus
          {
               Grin, Normal, Angry, Surprise, Left, Right, None
          };
          public enum Face
          {
               Health1, Health2, Health3, Health4, Health5, GameOver, OhYes, None
          };

          private int FaceAnimState = -1;
          private Timer FaceAnimTimer;
          private TimerCallback FaceAnimCallback;

          private Face CurrentFace;

		public void Reset()
		{
			this.CurrentFace = Face.None;
			this.FaceAnimState = -1;
			if ( this.FaceAnimTimer != null )
				{
					this.FaceAnimTimer.Dispose();
				}
		}

          private void FaceAnim(Object Odgs)
          {
               DoomGuyState dgs = (DoomGuyState) Odgs;
               int PicY = dgs.PicY;
               int BaseX = 52;
               int PicH = 67;
               int PicW = 53;
               int PicX = 0;
               if ( this.FaceAnimState == 0 )
                    {
                         PicX = BaseX * 3 ;
                         this.FaceAnimState = 1;
                    }
               else if ( FaceAnimState == 1 )
                    {
                         PicX = BaseX * 4;
                         this.FaceAnimState = 2;
                    }
               else if ( FaceAnimState == 2 )
                    {
                         PicX =  BaseX * 5;
                         this.FaceAnimState = 3;
                    }
               else if ( FaceAnimState == 3 )
                    {
                         PicX =  BaseX * 4;
                         this.FaceAnimState = 0;
                    }
               DrawFace(dgs.Cube, dgs.X, dgs.Y, PicX, PicY, PicW, PicH);
          }

          public void Draw(Cube c, Face F, FaceStatus FS, int X, int Y)
          {
               int BaseY = 67;
               int BaseX = 53;
               int PicX = 0;
               int PicY = 0;
               int PicH = 67;
               int PicW = 53;

			this.Reset();

               if ( F != Face.None )
                    {
                         if ( F == Face.Health1 )
                              {
                                   PicY = 0;
                              }
                         else if ( F == Face.Health2 )
                              {
                                   PicY = BaseY;
                              }
                         else if ( F == Face.Health3 )
                              {
                                   PicY = BaseY * 2;
                              }
                         else if ( F == Face.Health4 )
                              {
                                   PicY = BaseY * 3;
                              }
                         else if ( F == Face.Health5 )
                              {
                                   PicY = BaseY * 4;
                              }
                         else if ( F == Face.GameOver )
                              {
                                   PicY = 331;
                                   PicX = 0;
                              }
                         else if ( F == Face.OhYes )
                              {
                                   PicY = 331;
                                   PicX = BaseX * 1;
                              }
                         else
                              {
                                   throw new System.ArgumentException("Invalid Argument Face");
                              }
                         if ( F != Face.GameOver && F != Face.OhYes )
                              {
                                   if ( FS == FaceStatus.Normal )
                                        {
                                             if (this.FaceAnimState == -1 || this.CurrentFace != F )
                                                  {
                                                       this.FaceAnimState = 0;
                                                       this.FaceAnimCallback = this.FaceAnim;
                                                       this.CurrentFace = F;
                                                       DoomGuyState dgs = new DoomGuyState();
                                                       dgs.PicY = PicY;
                                                       dgs.Cube = c;
                                                       dgs.X = X;
                                                       dgs.Y = Y;
                                                       this.FaceAnimTimer = new Timer(this.FaceAnimCallback, dgs, 0, 500);
                                                  }
                                             return;
                                        }
                                   else if ( FS == FaceStatus.Grin )
                                        {
                                             PicX = 1;
                                        }
                                   else if ( FS == FaceStatus.Angry )
                                        {
                                             PicX = 1 + BaseX;
                                        }
                                   else if ( FS == FaceStatus.Surprise )
                                        {
                                             PicX = 1 + ( BaseX * 2);
                                        }
                                   else if ( FS == FaceStatus.Left )
                                        {
                                             PicX = 313;
                                             PicW = 60;
                                        }
                                   else if ( FS == FaceStatus.Right )
                                        {
                                             PicX = 373;
                                             PicW = 60;
                                        }
                                   else
                                        {
                                             throw new System.ArgumentException("invalid FaceStatus");
                                        }
                              }

                         DrawFace(c, X, Y, PicX, PicY, PicW, PicH);
                    }
			this.CurrentFace = F;
          }

          private static void DrawFace(Cube c, int X, int Y, int PicX, int PicY, int PicW, int PicH)
          {
//               Log.Debug("drawing on " + c.UniqueId + " at " + X.ToString() + "." + Y.ToString() + ", " + PicW.ToString() + "x" + PicH.ToString()  + ", from " + PicX.ToString() + "." + PicY.ToString());
               c.Image("doomfaces", X, Y, PicX, PicY, PicW, PicH, 1, 0);
               c.Paint();
          }
     }
}