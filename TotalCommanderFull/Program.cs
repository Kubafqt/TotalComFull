using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace TotalCommanderFull
{
   class Program
   {
      /// <summary>
      /// 
      /// </summary>
      static void Main(string[] args)
      {
         Point startSearch, endSearch, checkScreenStart;
         GetSearchPoints(out startSearch, out endSearch, out checkScreenStart);
         //start total commander process:
         Process process = new Process();
         process.StartInfo.FileName = @"c:\program files\totalcmd\TOTALCMD64.EXE";
         process.Start();
         process.WaitForInputIdle();
         //test if resolution is saved (in db):
         string resolution = $"{Screen.PrimaryScreen.Bounds.Width}x{Screen.PrimaryScreen.Bounds.Height}";
         string[] fileLines = File.ReadAllLines(Path.Combine(Application.StartupPath, "resSaved.txt"));
         List<string> fileLinesList = new List<string>(fileLines); //faster access than db
         int sleepTime = !fileLinesList.Contains(resolution) ? 2500 : 0;
         System.Threading.Thread.Sleep(sleepTime);
         //recognize image:
         int repetition = 0;
         int maxRepetition = 30;
         string buttonName;
         Point imagePosition;
         RecognizeImage:
         //add images to list:
         for (int x = 1; x <= 3; x++)
         {
            //then load from db
            Images.imagesList.Add(new Images(Application.StartupPath + $"\\images\\{Images.buttonNames[x - 1]}.png", startSearch, endSearch, checkScreenStart)); 
         }
         while (!Images.RecognizeImage(out buttonName, out imagePosition) && repetition < maxRepetition) //3 sec waiting (30x100ms)
         {
            repetition++;
            System.Threading.Thread.Sleep(100);
         }
         if (imagePosition.X == -1)
         {
            Images.imagesList.Clear();
            GetSearchPoints(out startSearch, out endSearch, out checkScreenStart, false);
            maxRepetition = 10;
            goto RecognizeImage;
         }
         ClickCorrectButton(buttonName, imagePosition);
         //next - settings for images, search location, ... - second winform app -> db or file for both programs
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="startSearch"></param>
      /// <param name="endSearch"></param>
      private static void GetSearchPoints(out Point startSearch, out Point endSearch, out Point checkScreenStart, bool testGetFromDB = true)
      {
         string resolution = $"CONCAT({Screen.PrimaryScreen.Bounds.Width}, 'x', {Screen.PrimaryScreen.Bounds.Height})";
         startSearch = new Point(-1, -1);
         endSearch = new Point(-1, -1);
         checkScreenStart = new Point(-1, -1);
         if (testGetFromDB)
         {
            DB_Access.GetSearchPoints(resolution, out startSearch, out endSearch, out checkScreenStart);
         }
         startSearch = startSearch.X != -1 ? startSearch : new Point(0, 0);
         endSearch = endSearch.X != -1 ? endSearch : new Point(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
         checkScreenStart = checkScreenStart.X != -1 ? checkScreenStart : new Point(endSearch.X / 2 - startSearch.X / 2 - 20, endSearch.Y / 2 - startSearch.Y / 2);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="buttonName"></param>
      /// <param name="imagePosition"></param>
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
         System.Threading.Thread.Sleep(6);
         Cursor.Position = point;
         System.Threading.Thread.Sleep(7);
         mouse_event((int)MouseEventFlags.LEFTDOWN | (int)MouseEventFlags.LEFTUP, Cursor.Position.X, Cursor.Position.Y, 0, 0);
         System.Threading.Thread.Sleep(7);
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