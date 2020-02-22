using System;
using System.IO;
using System.Linq;
using System.CommandLine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.CommandLine.Rendering;
using System.CommandLine.Invocation;
using System.CommandLine.Rendering.Views;

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

        private static InvocationContext _invocationContext;
        private static ConsoleRenderer _consoleRenderer;

        #region Helpers

        /// <summary>
        /// Read PULGA_HOME value from <see cref="PulgaHome"/> and return available profile(s)
        /// </summary>
        /// <returns>Selected profile</returns>
        private static string GetProfile(string profileName = null)
        {
            if (GetProfiles().FirstOrDefault() is null)
            {
                WriteLine("Profile not found, create one first!");
                Environment.Exit(1);
            }

            if (!string.IsNullOrEmpty(profileName))
            {
                var selectedProfile = GetProfiles().FirstOrDefault(p => p == profileName);
                
                if (!string.IsNullOrEmpty(selectedProfile)) return profileName;
                
                WriteLine($"Profile \"{profileName}\" don't exist!");
                Environment.Exit(1);
            }

            return string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PULGA_PROFILE"))
                ? GetProfiles().First()
                : Environment.GetEnvironmentVariable("PULGA_PROFILE");
        }

        private static IEnumerable<string> GetProfiles() => Directory.GetFiles(PulgaHome)
            .Select(Path.GetFileName)
            .Where(name => name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .Select(name => name.Replace(".json", string.Empty, StringComparison.OrdinalIgnoreCase));

        #endregion

        #region Profile Command

        private static Command ProfileCommand()
        {
            var cmd = new Command("profile", "Select default, create, delete and list user profiles for Bradesco Cartões");

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

            cmd.AddOption(new Option<string>(new[] { "--profile", "-p" })
            {
                Description = $"Select a default profile for this session under current PULGA_HOME ({PulgaHome})",
                Argument = new Argument<string>
                {
                    Arity = ArgumentArity.ExactlyOne,
                    Name = "profile",
                    Description = "Name of the profile to use"
                }
            });

            cmd.Handler = CommandHandler.Create<bool, string, string, string>((list, create, delete, profile) =>
            {
                CurrentProfile = string.IsNullOrEmpty(profile) ? GetProfile() : GetProfile(profile);

                if (list)
                {
                    WriteLine($"Active profiles under PULGA_HOME ({PulgaHome}):");
                    WriteLine();

                    var profilesTable = new TableView<string>
                    {
                        Items = GetProfiles().ToArray()
                    };

                    // ReSharper disable once VariableHidesOuterVariable
                    profilesTable.AddColumn(profile => profile, "Profile Name");
                
                    var screen = new ScreenView(_consoleRenderer, _invocationContext.Console) { Child = profilesTable };
                    screen.Render();
                }

                if (!string.IsNullOrEmpty(delete))
                {
                    if (delete == CurrentProfile)
                    {
                        WriteLine($"You cannot delete the current profile ({delete})!");
                        Environment.Exit(1);
                    }

                    GetProfile(delete);

                    var profilePath = Path.Combine(PulgaHome, $"{delete}.json");

                    if (!File.Exists(profilePath))
                    {
                        WriteLine($"Unable to find profile \"{delete}\"!");
                        Environment.Exit(1);
                    }

                    File.Delete(profilePath);
                    WriteLine($"Profile {delete} removed");
                }
            });

            return cmd;
        }

        #endregion


        /// <summary>
        /// Bradesco Cartões as a command line interface
        /// </summary>
        /// <param name="args">Command line arguments for the application internals.</param>
        /// <param name="invocationContext"></param>
        /// <returns>If the provided parameters match with the requisites</returns>
        // ReSharper disable once UnusedMember.Local
        private static async Task<int> Main(string[] args, InvocationContext invocationContext)
        {
            _invocationContext = invocationContext;
            _consoleRenderer = new ConsoleRenderer(
                invocationContext.Console,
                mode: invocationContext.BindingContext.OutputMode(),
                resetAfterRender: true);
            
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

            return await rootCommand.InvokeAsync(args).ConfigureAwait(true);
        }
    }
}
