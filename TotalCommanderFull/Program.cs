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
         //HideConsoleWindow();
         string buttonName;
         for (int x = 1; x <= 3; x++) //add images to list
         {
            Images.imagesList.Add(new Images(Path.Combine(Application.StartupPath, $"{Images.buttonNames[x - 1]}.png"), new Point(650, 460), new Point(700, 500)));
         }
         //start total commander process:
         Process process = new Process();
         process.StartInfo.FileName = @"c:\program files\totalcmd\TOTALCMD64.EXE";
         process.Start();
         process.WaitForInputIdle();
         //System.Threading.Thread.Sleep(2500);
         int repetition = 0;
         Point imagePosition;
         while (!Images.RecognizeImage(out buttonName, out imagePosition) && repetition < 25) //2,5 sec waiting (25x100ms)
         {
            repetition++;
            System.Threading.Thread.Sleep(250); //do-odladit
         }
         ClickCorrectButton(buttonName, imagePosition);

         #region oldComment
         //while (!iconImage.ImageCheck(iconImage.startSearch, iconImage.endSearch) && repetition < 10) //2 seconds checking
         //{ repetition++; Thread.Sleep(200); }
         //total commander check 1, 2, 3 and click button by this (margin by sample) [if not in small rectangle, test big rectangle after, then test full screen, then off]
         //when get sample LMBclick on margin position (from sample) and getCursorBackToPreviousPosition ... ;
         //at last close this program - easy
         #endregion

      }

      private static void ClickCorrectButton(string buttonName, Point imagePosition)
      {
         Point lastCursorPosition = Cursor.Position;
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
                  SendLeftMouse(new Point(imagePosition.X - xMargin, imagePosition.Y + yMargin));
                  break;
               }
            case "two":
               {
                  SendLeftMouse(new Point(imagePosition.X + xMargin, imagePosition.Y + yMargin));
                  break;
               }
            case "three":
               {
                  SendLeftMouse(new Point(imagePosition.X + xMargin, imagePosition.Y + yMargin));
                  break;
               }
         }
         Cursor.Position = lastCursorPosition;
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

      #region Console showing
      //hide console:
      [DllImport("kernel32.dll")]
      static extern IntPtr GetConsoleWindow();
      [DllImport("user32.dll")]
      static extern bool HideWindow(IntPtr hWnd, int nCmdShow);
      const int SW_HIDE = 0;
      const int SW_SHOW = 1;
      /// <summary>
      /// Show or hide console window.
      /// </summary>
      /// <param name="type">Type "hide" to hide console window instead of show.</param>
      public static void HideConsoleWindow(string type = "hide")
      {
         var handle = GetConsoleWindow(); //for hide console window
         var showing = type.ToLower() == "hide" ? SW_HIDE : SW_SHOW;
         HideWindow(handle, showing); //hide console window
      }

      //determine if console window is visible:
      [DllImport("user32.dll")]
      [return: MarshalAs(UnmanagedType.Bool)]
      static extern bool IsWindowVisible(IntPtr hWnd);
      #endregion

   }
}