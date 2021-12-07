using POSSystems.Helpers;
using System;

namespace POSSystems.Tools
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting POSSystems tools...");

            Console.WriteLine(string.Format("Super Admin Pass: {0}", SecurityHelper.HashPassword("admin123")));
        }
    }
}
