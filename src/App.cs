using System;
using System.IO;
using System.Linq;
using System.CommandLine;
using System.Threading.Tasks;
using System.CommandLine.Invocation;

using static System.Console;

namespace Pulga
{
    internal static class App
    {
        private static void Start(string profileName, bool listEnvironment, bool listProfile, bool verbose, bool configure)
        {
            var pulgaHome = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PULGA_HOME"))
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".pulga")
                : Environment.GetEnvironmentVariable("PULGA_HOME");

            if (!Directory.Exists(pulgaHome))
                Directory.CreateDirectory(pulgaHome);

            var profiles = Directory.GetFiles(pulgaHome)
                .Where(name => name.EndsWith(".json"))
                .ToList();

            if (string.IsNullOrEmpty(profiles.FirstOrDefault()))
            {
                WriteLine("Any profile found, create one first before run anything!");
                WriteLine(" - Add a new profile with --configure");
                Environment.Exit(111);
            }

            var pulgaProfile = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PULGA_PROFILE"))
                ? string.Empty
                : Environment.GetEnvironmentVariable("PULGA_PROFILE");

            if (profiles.Count > 1 && string.IsNullOrEmpty(pulgaProfile))
            {
                WriteLine("More than 1 profile found!");
                WriteLine(" - Parametrize the defaults one with -p");
                Environment.Exit(111);
            }
            
            if (string.IsNullOrEmpty(pulgaProfile))
                pulgaProfile = profiles.First();

            if (verbose)
                WriteLine($"Using profile {pulgaProfile}");
        }

        /// <summary>
        /// Bradesco Cartões as a command line interface
        /// </summary>
        /// <param name="args">Command line arguments for the application internals.</param>
        /// <returns>If the provided parameters match with the requisites</returns>
        private static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                new Option<string>(
                    new []{"--profile-name", "-p"},
                    getDefaultValue: () => string.Empty,
                    description: "Select default profile to use. Overwrite PULGA_PROFILE value"),

                new Option<bool>(
                    new []{"--list-profiles", "-l"},
                    "List available profiles on the application"),

                new Option<bool>(
                    new []{"--environment", "-e"},
                    "List specific application environment variables"),

                new Option<bool>(
                    new []{"--verbose", "-v"},
                    "Enable verbose output"),

                new Option<bool>(
                    new []{"--configure", "-c"},
                    "Start default Pulga configuration"),

                new Command("list-cards", "List managed cards")
            };

            rootCommand.Description = "Bradesco Cartões as a command line interface";
            rootCommand.Handler =
                CommandHandler.Create<string, bool, bool, bool, bool>(Start);

            return await rootCommand.InvokeAsync(args);
        }
    }
}
