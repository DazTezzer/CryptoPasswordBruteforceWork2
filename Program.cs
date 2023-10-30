using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace CryptoPasswordBruteforce
{
    public class LetterCombinations
    {
        private static object consoleLock = new object();
        public static void GenerateCombinations(char startLetter, char endLetter, int length, int numThreads, string[] hashes)
        {
            var tasks = new List<Task>();
            int count = 0;
            int treadnumcount = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(26) / Convert.ToDouble(numThreads)));
            char firstLetter = startLetter;
            for (int i = 0; i < numThreads; i++)
            {
                char insidebuffer = firstLetter;
                if (count != 3){
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        char letterbuffer = insidebuffer;
                        DateTime startTime = DateTime.Now;
                        for (int j = 0 ; j < treadnumcount ; j++)
                        {
                            letterbuffer = (char)(insidebuffer + j);
                            if (letterbuffer > endLetter)
                            {
                                return;
                            }
                            count = count + GenerateCombinationsRecursive(letterbuffer.ToString(), startLetter, endLetter, length, hashes, startTime);
                        }
                    }));
                }
                else
                {
                    return;
                }
                firstLetter = (char)(firstLetter + treadnumcount);
            }
            Task.WaitAll(tasks.ToArray());
        }

        private static int GenerateCombinationsRecursive(string current, char startLetter, char endLetter, int length, string[] hashes, DateTime startTime)
        {
            if (current.Length == length)
            {
                string passwordHash = ComputeSha256Hash(current);
                DateTime endTime = DateTime.Now;
                int index = Array.IndexOf(hashes, passwordHash);
                if (index >= 0)
                {
                    Monitor.Enter(consoleLock);
                    try
                    {
                        Console.WriteLine($"Поток {Thread.CurrentThread.ManagedThreadId} запущен в {startTime}");
                        Console.WriteLine("Данный набор: {0} соответствует хэшу - {1}", current, hashes[index]);
                        Console.WriteLine($"Поток {Thread.CurrentThread.ManagedThreadId} завершен в {endTime}. Время выполнения: {endTime - startTime}");
                    }
                    finally
                    {
                        Monitor.Exit(consoleLock);
                    }
                    return 1;
                }
                return 0;
            }
     
            for (char letter = startLetter; letter <= endLetter; letter++)
            {
                GenerateCombinationsRecursive(current + letter, startLetter, endLetter, length, hashes, startTime);
            }
            return 0;
        }
        static string ComputeSha256Hash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Введите три Хэш-значения SHA-256 через пробел: ");
            string[] hashes = Console.ReadLine().Split(' ');
            Console.WriteLine("Первый хэш: {0} \nВторой хэш: {1} \nТретий хэш: {2} \n", hashes[0], hashes[1], hashes[2]);
            Console.WriteLine("Введите количество потоков: ");
            int numThreads = Convert.ToInt32(Console.ReadLine());
            DateTime startTime = DateTime.Now;
            Console.WriteLine($"Программа запущена в {startTime}");
            LetterCombinations.GenerateCombinations('a','z', 5, numThreads, hashes);
            DateTime endTime = DateTime.Now;
            Console.WriteLine($"Программа отработала , завершена в {endTime}. Время выполнения: {endTime - startTime}");
            Console.ReadLine();
        }
    }
}
