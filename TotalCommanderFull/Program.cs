using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;

namespace TotalCommanderFull
{
   class Program
   {
      static void Main(string[] args)
      {
         //add images to list:
         for (int x = 1; x <= 3; x++) 
         {
            Images.imagesList.Add(new Images(Path.Combine(Application.StartupPath, $"{Images.buttonNames[x - 1]}.png"), new Point(650, 460), new Point(700, 500)));
         }
         //start total commander process:
         Process process = new Process();
         process.StartInfo.FileName = @"c:\program files\totalcmd\TOTALCMD64.EXE";
         process.Start();
         process.WaitForInputIdle();
         //recognize image:
         string buttonName;
         int repetition = 0;
         Point imagePosition;
         while (!Images.RecognizeImage(out buttonName, out imagePosition) && repetition < 30) //3 sec waiting (30x100ms)
         {
            repetition++;
            System.Threading.Thread.Sleep(100);
         }
         ClickCorrectButton(buttonName, imagePosition);

         //next - settings for images, search location, ... - second winform app -> db or file for both programs

      }

      private static void ClickCorrectButton(string buttonName, Point imagePosition)
      {
         int xMargin = 110;
         int yMargin = 32;
         switch (buttonName)
         {
            default:
               {
                  Console.WriteLine("Žádný image sample nenalezen.");
                  break;
               }
            case "one": //potom asi dictionary<string,point>
               {
                  LMBclick(new Point(imagePosition.X - xMargin, imagePosition.Y + yMargin), true);
                  break;
               }
            case "two":
               {
                  LMBclick(new Point(imagePosition.X, imagePosition.Y + yMargin), true);
                  break;
               }
            case "three":
               {
                  LMBclick(new Point(imagePosition.X + xMargin, imagePosition.Y + yMargin), true);
                  break;
               }
         }
      }

      #region HandleMouse
      [DllImport("user32.dll")]
      public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
      public enum MouseEventFlags
      {
         LEFTDOWN = 0x00000002,
         LEFTUP = 0x00000004,
         MIDDLEDOWN = 0x00000020,
         MIDDLEUP = 0x00000040,
         RIGHTDOWN = 0x00000008,
         RIGHTUP = 0x00000010
      }

      /// <summary>
      /// Send left mouse click on selected position.
      /// </summary>
      /// <param name="p">Point for position to send left mouse click.</param>
      public static void SendLeftMouse(Point p)
      {
         Cursor.Position = p;
         LMBclick();
      }

      /// <summary>
      /// Send left mouse click.
      /// </summary>
      public static void LMBclick()
      { mouse_event((int)MouseEventFlags.LEFTDOWN | (int)MouseEventFlags.LEFTUP, Cursor.Position.X, Cursor.Position.Y, 0, 0); }

      /// <summary>
      /// Send left mouse click on selected position and take back cursor to last position if selected.
      /// </summary>
      /// <param name="point">Point for position to send left mouse click.</param>
      /// <param name="lastPos">Take back cursor to lastPosition before left mouse click. True: take back cursor to last position, Default: false (not take back cursor to last position).</param>
      public static void LMBclick(Point point, bool lastPos = false)
      {
         Point lastPosition = Cursor.Position;
         Cursor.Position = point;
         mouse_event((int)MouseEventFlags.LEFTDOWN | (int)MouseEventFlags.LEFTUP, Cursor.Position.X, Cursor.Position.Y, 0, 0);
         if (lastPos)
         {
            Cursor.Position = lastPosition;
         }
      }
      /// <summary>
      /// Send middle mouse click.
      /// </summary>
      public static void MMBclick()
      { mouse_event((int)MouseEventFlags.MIDDLEDOWN | (int)MouseEventFlags.MIDDLEUP, Cursor.Position.X, Cursor.Position.Y, 0, 0); }
      /// <summary>
      /// Send right mouse click.
      /// </summary>
      public static void RMBClick()
      { mouse_event((int)MouseEventFlags.RIGHTDOWN | (int)MouseEventFlags.RIGHTUP, Cursor.Position.X, Cursor.Position.Y, 0, 0); }
      #endregion

   }
}