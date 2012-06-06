using System;
using Sifteo;

namespace sifteo4devops
{
     public class Util
     {
		
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
}