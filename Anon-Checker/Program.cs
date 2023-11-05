using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Anon_Checker
{
    internal class Program
    {

        private class Writer
        {
            internal static void WriteFromDict(Dictionary<string, bool> dict)
            {
                List<string> validItems = new List<string>();
                List<string> invalidItems = new List<string>();
                
                foreach (var kvp in dict)
                {
                    if (kvp.Value)
                        validItems.Add(kvp.Key);
                    else
                        invalidItems.Add(kvp.Key);
                }

                using (StreamWriter writer = new StreamWriter("valid.anon"))
                {
                    foreach (var item in validItems)
                    {
                        writer.WriteLine(item);
                    }
                }
                using (StreamWriter writer = new StreamWriter("invalid.anon"))
                {
                    foreach (var item in invalidItems)
                    {
                        writer.WriteLine(item);
                    }
                }

            }
        }

        private class Checker
        {
            private const string baseUrl = "https://discord.com/api/v9/users/@me";
            private const bool checkName = true;

            internal static async Task Single(string token)
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", token);

                HttpResponseMessage response = await client.GetAsync(baseUrl);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.Write(" [+] Valid Token.");
                    if (checkName)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();

                        try
                        {
                            string jsonData = JObject.Parse(responseBody)["username"].ToString();

                            Console.WriteLine(" | Name: " + jsonData);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error deserializing JSON: " + ex.Message);
                        }
                    }
                    else { Console.WriteLine(); }

                }
                else if (response.StatusCode == (HttpStatusCode)429)
                {
                    Console.WriteLine(" [/] Ratelimit.");
                }
                else
                {
                    Console.WriteLine(" [-] Invalid Token.");
                }
                Console.WriteLine(Environment.NewLine + " PRESS ANY KEY TO RETURN TO MAIN MENU");
                Console.ReadKey();
                await StartUp.MainMenu();
            }


            internal static async Task Mass(string filepath)
            {
                string[] tokens = File.ReadAllLines(filepath);

                Dictionary<string, bool> tokenValidation = new Dictionary<string, bool>();
                long valid = 0;
                long invalid = 0;
                long ratelimited = 0;
                foreach (string token in tokens)
                {
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Authorization", token);

                    HttpResponseMessage response = await client.GetAsync(baseUrl);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        valid++;
                        tokenValidation[token] = true;
                    }
                    else if (response.StatusCode == (HttpStatusCode)429)
                    {
                        ratelimited++;
                    }
                    else
                    {
                        invalid++;
                        tokenValidation[token] = false;
                    }

                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write($"\r {valid} Valid ");
                    Console.ForegroundColor= ConsoleColor.DarkGray;
                    Console.Write("Token(s)");
                    Console.ResetColor();
                    Console.Write(" | ");
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write($"{invalid} Invalid ");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("Token(s)");
                    Console.ResetColor();
                    Console.Write(" | ");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write($"{ratelimited} Ratelimited ");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("(not checked) Token(s)");
                    Console.ResetColor();

                    client.Dispose();
                }
                Writer.WriteFromDict(tokenValidation);
                Console.WriteLine(Environment.NewLine + " PRESS ANY KEY TO RETURN TO MAIN MENU");
                Console.ReadKey();
                await StartUp.MainMenu();
            }

        }

        private class StartUp
        {
            internal static string _logo
            {
                get
                {
                    return @"  _______  _        _______  _          _______           _______  _______  _        _______  _______ 
 (  ___  )( (    /|(  ___  )( (    /|  (  ____ \|\     /|(  ____ \(  ____ \| \    /\(  ____ \(  ____ )
 | (   ) ||  \  ( || (   ) ||  \  ( |  | (    \/| )   ( || (    \/| (    \/|  \  / /| (    \/| (    )|
 | (___) ||   \ | || |   | ||   \ | |  | |      | (___) || (__    | |      |  (_/ / | (__    | (____)|
 |  ___  || (\ \) || |   | || (\ \) |  | |      |  ___  ||  __)   | |      |   _ (  |  __)   |     __)
 | (   ) || | \   || |   | || | \   |  | |      | (   ) || (      | |      |  ( \ \ | (      | (\ (   
 | )   ( || )  \  || (___) || )  \  |  | (____/\| )   ( || (____/\| (____/\|  /  \ \| (____/\| ) \ \__
 |/     \||/    )_)(_______)|/    )_)  (_______/|/     \|(_______/(_______/|_/    \/(_______/|/   \__/
";
                }
            }

            internal static void Logo()
            {
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine(_logo);
                Console.ResetColor();
            }

            private static void Cursor()
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write($" {Environment.UserName}");
                Console.ResetColor();
                Console.Write("@");
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.Write("AC");
                Console.ResetColor();
                Console.Write(" ~> ");
            }

            internal static async Task MainMenu()
            {
                Console.Clear();
                Logo();
                Console.WriteLine(@" Options:

 mass   - check a file
 single - check a single token
");
                Cursor();
                string cmd = Console.ReadLine();
                if (cmd.StartsWith("mass"))
                {
                    Console.Clear();
                    Logo();
                    Console.WriteLine(" Enter Filename (have to be in the same dir like the program)");
                    Cursor();
                    string path = Console.ReadLine();
                    await Checker.Mass(path);
                }
                else if (cmd.StartsWith("single"))
                {
                    Console.Clear();
                    Logo();
                    Console.WriteLine(" Enter Token");
                    Cursor();
                    string token = Console.ReadLine();
                    await Checker.Single(token);
                    Console.ReadKey();
                }
                else
                {
                    Console.Clear();
                    Logo();
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(" THIS IS NOT A VALID COMMAND!");
                    Console.ReadKey();
                    await MainMenu();
                }
            }
        }

        static async Task Main(string[] args)
        {
            Console.Title = "Token Checker by »Anon«";
            Console.SetWindowSize(103, Console.WindowHeight);
            await StartUp.MainMenu();
        }
    }
}
