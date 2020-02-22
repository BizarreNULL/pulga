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
        /// <summary>
        /// Value for the current Pulga home working directory
        /// </summary>
        private static readonly string PulgaHome = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PULGA_HOME"))
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".pulga")
            : Environment.GetEnvironmentVariable("PULGA_HOME");

        /// <summary>
        /// Current profile for this session
        /// </summary>
        public static string CurrentProfile { get; set; }

        #region Helpers

        /// <summary>
        /// Read PULGA_HOME value from <see cref="PulgaHome"/> and return available profile(s)
        /// </summary>
        /// <returns>Selected profile</returns>
        private static string GetProfile(string profileName = null)
        {
            if (string.IsNullOrEmpty(PulgaHome))
                throw new ArgumentNullException(nameof(PulgaHome));

            var profiles = Directory.GetFiles(PulgaHome)
                .Where(name => name.EndsWith(".json"))
                .ToList();

            if (profiles.FirstOrDefault() is null)
            {
                WriteLine("Any profile found, create one first before run anything!");
                WriteLine(" - Add a new profile with --configure");
                Environment.Exit(111);
            }

            if (!string.IsNullOrEmpty(profileName) && profiles.FirstOrDefault(p => p == profileName) != null)
                return profileName;

            if (profiles.Count > 1)
            {
                WriteLine("More than 1 profile found!");
                WriteLine(" - Parametrize the defaults one with -p");
                Environment.Exit(111);
            }

            return string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PULGA_PROFILE"))
                ? profiles.First()
                : Environment.GetEnvironmentVariable("PULGA_PROFILE");
        }

        #endregion

        #region Profile Command

        private static Command ProfileCommand()
        {
            var cmd = new Command("profile", "Select default, create, delete, edit and list user profiles for Bradesco Cartões");

            cmd.AddOption(new Option<bool>(new[] { "--list", "-l" })
            {
                Description = $"List available profiles under current PULGA_HOME ({PulgaHome})"
            });

            cmd.AddOption(new Option<string>(new[] {"--create", "-c"})
            {
                Description = $"Create new profiles under current PULGA_HOME ({PulgaHome})",
                Argument = new Argument<string>
                {
                    Arity = ArgumentArity.ExactlyOne,
                    Name = "name",
                    Description = "Name to new profile"
                }
            });

            cmd.AddOption(new Option<string>(new[] { "--delete", "-d" })
            {
                Description = $"Delete a profiles under current PULGA_HOME ({PulgaHome})",
                Argument = new Argument<string>
                {
                    Arity = ArgumentArity.ExactlyOne,
                    Name = "delete",
                    Description = "Name of the profile to delete"
                }
            });

            cmd.AddOption(new Option<string>(new[] { "--edit", "-e" })
            {
                Description = $"Edit a profiles under current PULGA_HOME ({PulgaHome})",
                Argument = new Argument<string>
                {
                    Arity = ArgumentArity.ExactlyOne,
                    Name = "edit",
                    Description = "Name of the profile to edit"
                }
            });

            cmd.AddOption(new Option<string>(new[] { "--default", "-d" })
            {
                Description = $"Select a default profile for this session under current PULGA_HOME ({PulgaHome})",
                Argument = new Argument<string>
                {
                    Arity = ArgumentArity.ExactlyOne,
                    Name = "default",
                    Description = "Name of the profile to use"
                }
            });

            cmd.Handler = CommandHandler.Create<bool, string, string, string, string>((list, create, delete, edit, @default) =>
            {
                CurrentProfile = string.IsNullOrEmpty(@default) ? GetProfile() : GetProfile(@default);
            });

            return cmd;
        }

        #endregion


        /// <summary>
        /// Bradesco Cartões as a command line interface
        /// </summary>
        /// <param name="args">Command line arguments for the application internals.</param>
        /// <returns>If the provided parameters match with the requisites</returns>
        private static async Task<int> Main(string[] args)
        {
            var pulgaHome = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PULGA_HOME"))
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".pulga")
                : Environment.GetEnvironmentVariable("PULGA_HOME");

            if (!Directory.Exists(pulgaHome))
                Directory.CreateDirectory(pulgaHome);

            var rootCommand = new RootCommand
            {
                ProfileCommand()
            };

            rootCommand.Description = "Bradesco Cartões as a command line interface";

            return await rootCommand.InvokeAsync(args);
        }
    }
}
