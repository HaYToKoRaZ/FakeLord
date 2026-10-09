using System;
using System.Threading;

namespace HaYTooL
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.Title = "HaYTooL";
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("==================================================");
                Console.WriteLine("                    HaYTooL");
                Console.WriteLine("==================================================");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Durum: Aktif (%0 CPU)");
                Console.ResetColor();
                Console.WriteLine("==================================================");
            }
            catch { }

            while (true)
            {
                Thread.Sleep(3600000);
            }
        }
    }
}
