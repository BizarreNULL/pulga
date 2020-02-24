using System;
using System.IO;
using System.CommandLine;
using System.Threading.Tasks;
using System.CommandLine.Rendering;
using System.CommandLine.Invocation;

using static System.Console;

using Pulga.Commands;

namespace Pulga
{
    internal static class App
    {
        /// <summary>
        /// Value for the current Pulga home working directory
        /// </summary>
        public static readonly string PulgaHome = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PULGA_HOME"))
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".pulga")
            : Environment.GetEnvironmentVariable("PULGA_HOME");

        private static InvocationContext _invocationContext;
        private static ConsoleRenderer _consoleRenderer;

        #region Profile Command

        private static Command ProfileCommand()
        {
            var cmd = new Command("profile", "Select default, create, delete and list user profiles for Bradesco Cartões");

            cmd.AddOption(new Option<bool>(new[] { "--list", "-l" })
            {
                Description = $"List available profiles under current PULGA_HOME ({PulgaHome})"
            });

            cmd.AddOption(new Option<string>(new[] { "--create", "-c" })
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

            cmd.AddOption(new Option<bool>(new[] { "--current-profile"})
            {
                Description = "Profile used by default"
            });

            cmd.Handler = CommandHandler.Create<bool, string, string, string, bool>((list, create, delete, profile, currentProfile) =>
            {
                _ = new ProfileOperations(profile);

                if (list)
                    ProfileOperations.List(_consoleRenderer, _invocationContext);

                if (currentProfile)
                    WriteLine($"Default profile is {ProfileOperations.CurrentProfile}");

                if (!string.IsNullOrEmpty(delete))
                    ProfileOperations.Delete(delete);
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
