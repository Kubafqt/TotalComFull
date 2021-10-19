using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
//using System.Windows.Forms;

namespace TotalCommanderFull
{
   class Images : IDisposable
   {
      private string samplePath;
      private Bitmap sample;
      private Point lastPosition;
      private Point startSearch;
      private static int startX;
      private static int startY;
      private Point endSearch;
      public static string[] buttonNames = new string[] { "one", "two", "three" };
      //+save last position

      public static List<Images> imagesList = new List<Images>();

      #region dispose
      private readonly IntPtr unmanagedResource;
      private readonly SafeHandle managedResource;

      public void Dispose()
      {
         this.Dispose(true);
         GC.SuppressFinalize(this);
      }

      protected virtual void Dispose(bool isManualDisposing)
      {
         ReleaseUnmanagedResource(unmanagedResource);
         if (isManualDisposing)
         {
            ReleaseManagedResources(managedResource);
         }
         // Release
      }

      private void ReleaseManagedResources(SafeHandle safeHandle)
      {
         if (safeHandle != null)
         {
            safeHandle.Dispose();
         }
      }

      private void ReleaseUnmanagedResource(IntPtr intPtr)
      {
         Marshal.FreeHGlobal(intPtr);
      }
      #endregion

      /// <summary>
      /// constructor
      /// </summary>
      /// <param name="samplePath">path to sample image</param>
      /// <param name="startSearch">start searching point in screenshot of sample image</param>
      /// <param name="endSearch">end searching point in screenshot of sample image</param>
      public Images(string samplePath, Point startSearch, Point endSearch)
      {
         this.samplePath = samplePath;
         this.startSearch = startSearch;
         this.endSearch = endSearch;
         startX = startSearch.X;
         startY = startSearch.Y;
         sample = (Bitmap)Image.FromFile(samplePath);
         lastPosition = new Point(-1, -1);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public static bool RecognizeImage(out string imageButtonID, out Point ImagePosition)
      {
         int imageNumberID = 0;
         Point lastPos = new Point(-1, -1);
         ImagePosition = lastPos;
         for (int x = 0; x < imagesList.Count; x++)
         {
            using (Images sample = imagesList[x])
            {
               if (sample.ImageCheck(sample.startSearch, sample.endSearch, ref lastPos))
               {
                  imageButtonID = buttonNames[imageNumberID];
                  ImagePosition = new Point(lastPos.X + startX, lastPos.Y + startY);
                  sample.Dispose();
                  return true; //getted image
               }
               sample.Dispose();
               imageNumberID++;
            }
         }
         imageButtonID = string.Empty;
         lastPos = new Point(-1, -1);
         return false;
      }

      /// <summary>
      /// Check if sample images is appearing on exact screenshot.
      /// </summary>
      /// <param name="startSearch">start searching point on display screen</param>
      /// <param name="endSearch">end searching point on display screen</param>
      /// <returns>True: sample image is appearing on exact screenshot, False: sample image is not appearing on exact screenshot.</returns>
      public bool ImageCheck(Point startSearch, Point endSearch, ref Point lastPos)
      {
         try
         {
            using (Bitmap screen = ExactScreenshot(startSearch, endSearch))
            {
               if (lastPosition.X == -1)
               {
                  for (int x = 0; x < screen.Width - (sample.Width - 1); x++)
                  {
                     for (int y = 0; y < screen.Height - (sample.Height - 1); y++)
                     {
                        if (sample.GetPixel(0, 0) == screen.GetPixel(x, y) && IsInnerImage(x, y, sample, screen))
                        {
                           lastPosition = new Point(x, y);
                           lastPos = lastPosition;
                           screen.Dispose();
                           return true; //sample is on screen
                        }
                     }
                  }
                  lastPos = new Point(-1, -1);
                  screen.Dispose();
                  return false;
               }
               else //lastPosition point of sample image in exact screenshot is saved
               {
                  if (!(sample.GetPixel(0, 0) == screen.GetPixel(lastPosition.X, lastPosition.Y) && IsInnerImage(lastPosition.X, lastPosition.Y, sample, screen)))
                  {
                     lastPosition = new Point(-1, -1);
                     ImageCheck(startSearch, endSearch, ref lastPos); //go again and check whole screen - if not appears nowhere - return false;
                  }
                  screen.Dispose();
                  return true;
               }
            }
         }
         catch (Exception e)
         {
            Console.WriteLine($"Images.ImageCheck error: {e.GetType()}");
            return false;
         }
      }

      /// <summary>
      /// Get screenshot of exact area on screen.
      /// </summary>
      /// <param name="startScreen">start point of screen (left, up)</param>
      /// <param name="endScreen">end point of screen (right, down)</param>
      /// <returns>screenshot bitmap</returns>
      private Bitmap ExactScreenshot(Point startScreen, Point endScreen)
      {
         Size size = new Size(endScreen.X - startScreen.X, endScreen.Y - startScreen.Y);
         Bitmap screenshot = new Bitmap(size.Width, size.Height);
         Graphics gfx = Graphics.FromImage(screenshot);
         gfx.CopyFromScreen(startScreen.X, startScreen.Y, 0, 0, size);
         return screenshot;
      }

      /// <summary>
      /// Check if sample is inner image of screen, when first pixel is matched
      /// </summary>
      private bool IsInnerImage(int left, int top, Bitmap sample, Bitmap screen)
      {
         for (int x = 0; x < sample.Width; x++)
         {
            for (int y = 0; y < sample.Height; y++)
            {
               if (sample.GetPixel(x, y) != screen.GetPixel(left + x, top + y))
               {
                  return false; //sample is not inner image of screen
               }
            }
         }
         return true; //sample is inner image of screen
      }
   }
}