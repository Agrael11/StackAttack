using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackAttack
{
    internal static class Logger
    {
        public enum Levels { Info, Warn, Error, Fatal };
        public static Levels StopAt = Levels.Fatal;
        public static Levels ConsoleLogAt = Levels.Info;
        public static Levels FileLogAt = Levels.Warn;
        public static string LogFile = "current.log";
        private static string KeepLog = "";
        public static bool KeepOld = true;
        public static int Repeat = 3;
        private static int Repeated = 0;
        private static string LastLog = "";

        public static void Log(Levels level, string info)
        {
            if (Repeat != -1 && LastLog == info)
            {
                Repeated++;
                if (Repeat < Repeated)
                    return;
            }
            if (LastLog != info)
            {
                Repeated = 0;
            }
            LastLog = info;
            DateTime time = DateTime.Now;
            string curDate = $"{time.Day.ToString().PadLeft(2, '0')}/" +
                $"{time.Month.ToString().PadLeft(2, '0')}/" +
                $"{time.Year.ToString().PadLeft(4, '0')} " +
                $"{time.Hour.ToString().PadLeft(2, '0')}:" +
                $"{time.Minute.ToString().PadLeft(2, '0')}:" +
                $"{time.Second.ToString().PadLeft(2, '0')}:";
            if (string.IsNullOrWhiteSpace(KeepLog))
            {
                KeepLog = curDate.Replace(':', '-').Replace('/', '.') + ".log";
            }
            string str = $"[{Enum.GetName(typeof(Levels), level)}][{curDate}] {info}";
            if (level >= ConsoleLogAt)
            {
                ConsoleColor oldFG = Console.ForegroundColor;
                ConsoleColor oldBG = Console.BackgroundColor;
                switch (level)
                {
                    case Levels.Info:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case Levels.Warn:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case Levels.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case Levels.Fatal:
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.BackgroundColor = ConsoleColor.Red;
                        break;
                }
                Console.WriteLine(str);
                Console.ForegroundColor = oldFG;
                Console.BackgroundColor = oldBG;
            }
            if (level >= FileLogAt)
            {
                string log = "";
                if (File.Exists(LogFile))
                {
                    log =File.ReadAllText(LogFile);
                }
                log += $"{str}\n";
                File.WriteAllText(LogFile, log);
                if (KeepOld)
                {
                    File.WriteAllText(KeepLog, log);
                }
            }
            if (level >= StopAt)
            {
                throw new Exception(str);
            }
        }
    }
}
