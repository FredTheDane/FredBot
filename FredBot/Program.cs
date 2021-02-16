using System;

namespace FredBot
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Bootstrap and start the bot
            new Bot().MainAsync().GetAwaiter().GetResult();
        }
    }
}
