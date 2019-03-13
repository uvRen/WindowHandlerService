using System;

namespace WindowServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var windowListener = new Server();
            windowListener.Start();

            Console.ReadLine();

            windowListener.Stop();
        }
    }
}
