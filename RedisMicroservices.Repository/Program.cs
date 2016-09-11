using System;

namespace RedisMicroservices.Repository
{
    class Program
    {
        public static void Main(string[] args)
        {
            RepositoryEngine.Boot();

            Console.ReadLine();
        }
    }
}