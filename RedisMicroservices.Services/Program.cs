using System;

namespace RedisMicroservices.Services
{
    class Program
    {
        public static void Main(string[] args)
        {
            ServicesEngine.Boot();

            Console.ReadLine();
        }
    }
}