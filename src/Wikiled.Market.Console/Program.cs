using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NLog;
using Wikiled.Console.Arguments;
using Wikiled.Market.Console.Commands;

namespace Wikiled.Market.Console
{
    internal class Program
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            log.Info("Starting {0} version utility...", Assembly.GetExecutingAssembly().GetName().Version);
            if (args.Length == 0)
            {
                log.Warn("Please specify arguments");
                return;
            }

            List<Command> commandsList = new List<Command>();

            commandsList.Add(new GeneratePredictionCommand());
            var commands = commandsList.ToDictionary(item => item.Name, item => item, StringComparer.OrdinalIgnoreCase);

            if (args.Length == 0 ||
                !commands.TryGetValue(args[0], out var command))
            {
                log.Error("Please specify argumets");
                return;
            }

            try
            {
                command.ParseArguments(args.Skip(1));
                command.Execute();
            }
            catch (Exception ex)
            {
                log.Error(ex);
                System.Console.ReadLine();
            }
        }
    }
}
