using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.IO;

namespace TotalCommanderFull
{
   class DB_Access
   {
      static readonly string connString = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\"))}Database.mdf;Integrated Security = True; Connect Timeout = 30";

      /// <summary>
      /// 
      /// </summary>
      /// <param name="resolution"></param>
      /// <returns></returns>
      public static bool ResolutionExistInDb(string resolution)
      {
         resolution = resolution.Replace("CONCAT(", string.Empty).Replace(", '", string.Empty).Replace("', ", string.Empty).Replace(")", string.Empty);
         SqlConnection connection = new SqlConnection(connString);
         string selectCmdText = "SELECT Resolution FROM ImagePointTable;";
         SqlCommand selectCommand = new SqlCommand(selectCmdText, connection);
         connection.Open();
         SqlDataReader reader = selectCommand.ExecuteReader();
         ResolutionAppendToTextFile();
         while (reader.Read())
         {
            if (resolution == (string)reader["Resolution"])
            {
               connection.Close();
               return true;
            }
         }
         connection.Close();
         return false;
      }

      /// <summary>
      /// 
      /// </summary>
      public static void ResolutionAppendToTextFile()
      {
         string resolution = $"{Screen.PrimaryScreen.Bounds.Width}x{Screen.PrimaryScreen.Bounds.Height}";
         string filePath = Path.Combine(Application.StartupPath, "resSaved.txt");
         List<string> fileLines = new List<string>(File.ReadAllLines(filePath));
         if (!fileLines.Contains(resolution))
         {
            File.AppendAllText(filePath, resolution + "\n");
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="resolution"></param>
      /// <param name="startSearch"></param>
      /// <param name="endSearch"></param>
      public static void GetSearchPoints(string resolution, out Point startSearch, out Point endSearch, out Point checkScreenStart)
      {
         //default points location:
         startSearch = new Point(-1, -1);
         endSearch = new Point(-1, -1);
         checkScreenStart = new Point(-1, -1);
         //get points from db:
         SqlConnection connection = new SqlConnection(connString);
         string selectCmdText = $"SELECT startSearch, endSearch, LastPosition FROM ImagePointTable WHERE Resolution = {resolution};";
         SqlCommand selectCommand = new SqlCommand(selectCmdText, connection);
         connection.Open();
         SqlDataReader reader = selectCommand.ExecuteReader();
         while (reader.Read())
         {
            string startSearchString = (string)reader["StartSearch"];
            string endSearchString = (string)reader["EndSearch"];
            string checkScreenStartString = (string)reader["LastPosition"];
            string[] startSearchSplit = startSearchString.Split(';');
            string[] endSearchSplit = endSearchString.Split(';');
            string[] checkScreenStartSplit = checkScreenStartString.Split(';');
            startSearch = new Point(int.Parse(startSearchSplit[0]), int.Parse(startSearchSplit[1]));
            endSearch = new Point(int.Parse(endSearchSplit[0]), int.Parse(endSearchSplit[1]));
            int x = int.Parse(checkScreenStartSplit[0]);
            int y = int.Parse(checkScreenStartSplit[1]);
            checkScreenStart = new Point(x - 2 > 0 ? x - 2 : 0, y - 2 > 0 ? y - 2 : 0);
         }
         connection.Close();
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="resolution"></param>
      /// <param name="LastPosition"></param>
      /// <param name="startSearch"></param>
      /// <param name="endSearch"></param>
      public static void SavePoints(string resolution, Point LastPosition, Point startSearch, Point endSearch)
      {
         string cmdText = !ResolutionExistInDb(resolution) ? 
            $"INSERT INTO ImagePointTable VALUES({resolution}, CONCAT({startSearch.X}, ';', {startSearch.Y}), CONCAT({endSearch.X}, ';', {endSearch.Y}), CONCAT({LastPosition.X}, ';', {LastPosition.Y}));" : 
            $"UPDATE ImagePointTable SET StartSearch = CONCAT({startSearch.X}, ';', {startSearch.Y}), EndSearch = CONCAT({endSearch.X}, ';', {endSearch.Y}), LastPosition = CONCAT({LastPosition.X}, ';', {LastPosition.Y}) WHERE Resolution = {resolution};";
         ExecuteNonQuery(cmdText);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="resolution"></param>
      /// <returns></returns>
      public static Point GetLastPosition(string resolution)
      {
         SqlConnection connection = new SqlConnection(connString);
         string selectCmdText = $"SELECT LastPosition FROM ImagePointTable WHERE Resolution = {resolution};";
         SqlCommand selectCommand = new SqlCommand(selectCmdText, connection);
         connection.Open();
         SqlDataReader reader = selectCommand.ExecuteReader();
         while (reader.Read())
         {
            string lastPositionString = (string)reader["LastPosition"];
            string[] lastPositionSplit = lastPositionString.Split(';');
            connection.Close();
            return new Point(int.Parse(lastPositionSplit[0]), int.Parse(lastPositionSplit[1]));
         }
         connection.Close();
         return new Point(-1, -1);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="lastPos"></param>
      /// <param name="resolution"></param>
      public static void UpdateLastPos(Point lastPos, Point lastPositionFromDB, string resolution)
      {
         if (lastPositionFromDB.X != -1) //exist in db
         {
            string cmdText = $"UPDATE ImagePointTable SET LastPosition = CONCAT({lastPos.X}, ';', {lastPos.Y}) WHERE Resolution = {resolution};";
            ExecuteNonQuery(cmdText);
         }
         else //not exist in db
         {
            string cmdText = $"INSERT INTO ImagePointTable (LastPosition) VALUES (CONCAT({lastPos.X}, ';', {lastPos.Y})) WHERE Resolution = {resolution};";
            ExecuteNonQuery(cmdText);
         }
      }

      /// <summary>
      /// Execute SQL command
      /// </summary>
      /// <param name="cmdText">command text</param>
      private static void ExecuteNonQuery(string cmdText)
      {
         SqlConnection connection = new SqlConnection(connString);
         SqlCommand cmd = new SqlCommand(cmdText, connection);
         connection.Open();
         cmd.ExecuteNonQuery();
         connection.Close();
      }

   }
}