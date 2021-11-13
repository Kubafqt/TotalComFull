﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;

namespace TotalCommanderFull
{
   class Images : IDisposable
   {
      private Bitmap sample;
      private string samplePath;
      private Point startSearch;
      private static int startX;
      private static int startY;
      private Point endSearch;
      private Point checkScreenStart;
      public static string[] buttonNames = new string[] { "one", "two", "three" };

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
      public Images(string samplePath, Point startSearch, Point endSearch, Point checkScreenStart)
      {
         this.samplePath = samplePath;
         this.startSearch = startSearch;
         this.endSearch = endSearch;
         this.checkScreenStart = checkScreenStart;
         startX = startSearch.X;
         startY = startSearch.Y;
         sample = (Bitmap)Image.FromFile(samplePath);
         //lastPosition = new Point(-1, -1);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public static bool RecognizeImage(out string imageButtonID, out Point ImagePosition)
      {
         int imageNumberID = 0;
         string resolution = $"CONCAT({Screen.PrimaryScreen.Bounds.Width}, 'x', {Screen.PrimaryScreen.Bounds.Height})";
         Point lastPos = imagesList[0].startSearch.X != 0 ? DB_Access.GetLastPosition(resolution) : new Point(-1, -1);
         for (int x = 0; x < imagesList.Count; x++)
         {
            using (Images sample = imagesList[x])
            {
               if (sample.ImageCheck(sample.startSearch, sample.endSearch, sample.checkScreenStart, ref lastPos))
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
         ImagePosition = lastPos;
         return false;
      }

      /// <summary>
      /// Check if sample images is appearing on exact screenshot.
      /// </summary>
      /// <param name="startSearch">start searching point on display screen</param>
      /// <param name="endSearch">end searching point on display screen</param>
      /// <returns>True: sample image is appearing on exact screenshot, False: sample image is not appearing on exact screenshot.</returns>
      public bool ImageCheck(Point startSearch, Point endSearch, Point checkScreenStart, ref Point lastPos)
      {
         string resolution = $"CONCAT({Screen.PrimaryScreen.Bounds.Width}, 'x', {Screen.PrimaryScreen.Bounds.Height})";
         using (Bitmap screen = ExactScreenshot(startSearch, endSearch))
         {
            for (int x = checkScreenStart.X; x != checkScreenStart.X - 1; x++)
            {
               for (int y = checkScreenStart.Y; y != checkScreenStart.Y - 1 /*(sample.Height - 1)*/; y++)
               {
                  if (sample.GetPixel(0, 0) == screen.GetPixel(x, y) && IsInnerImage(x, y, sample, screen))
                  {
                     //lastPosition = new Point(x, y);
                     lastPos = new Point(x, y); //lastPosition;
                     Point newLastPos = new Point(-1, -1);
                     screen.Dispose();
                     if ((startSearch.X == 0 || endSearch.X == Screen.PrimaryScreen.Bounds.Width))
                     {
                        Point newStartSearch = new Point(lastPos.X - 10, lastPos.Y - 10);
                        Point newEndSearch = new Point(lastPos.X + 10 + sample.Width, lastPos.Y + 10 + sample.Height);
                        Point differenceStartSearch = new Point(newStartSearch.X - startSearch.X, newStartSearch.Y - startSearch.Y);
                        newLastPos = new Point(lastPos.X - differenceStartSearch.X, lastPos.Y - differenceStartSearch.Y);
                        //bool update = 
                        DB_Access.SavePoints(resolution, newLastPos, newStartSearch, newEndSearch);
                     }
                     else if (lastPos != DB_Access.GetLastPosition(resolution))
                     {
                        newLastPos = newLastPos.X == -1 ? lastPos : newLastPos;
                        DB_Access.UpdateLastPos(newLastPos, resolution);
                     }
                     return true; //sample is on screen
                  }
                  if (x == screen.Width - (sample.Width - 1))
                  {
                     x = 0;
                  }
                  if (y == screen.Height - (sample.Height - 1))
                  {
                     y = 0;
                  }
               }
            }
            lastPos = new Point(-1, -1);
            screen.Dispose();
            return false;
         }
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

   }
}