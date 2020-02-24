using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.CommandLine.Rendering;
using System.CommandLine.Invocation;
using System.CommandLine.Rendering.Views;

using static Pulga.App;
using static System.Console;

namespace Pulga.Commands
{
    /// <summary>
    /// Implements profile command operations
    /// </summary>
    public class ProfileOperations
    {
        /// <summary>
        /// Current Pulga profile
        /// </summary>
        public static string CurrentProfile { get; private set; }

        /// <summary>
        /// Default constructor for <see cref="ProfileOperations"/>.
        /// </summary>
        /// <param name="givenProfile">Profile provided by command line option</param>
        public ProfileOperations(string givenProfile)
        {
            CurrentProfile = string.IsNullOrEmpty(givenProfile) ? GetProfile() : GetProfile(givenProfile);
        }

        /// <summary>
        /// List Pulga profiles under the given <see cref="PulgaHome"/>
        /// </summary>
        /// <param name="consoleRenderer">Renderer from static class</param>
        /// <param name="invocationContext">Invocation from entry point</param>
        public static void List(ConsoleRenderer consoleRenderer, InvocationContext invocationContext)
        {
            if (invocationContext is null)
                throw new ArgumentNullException(nameof(consoleRenderer));

            WriteLine($"Active profiles under PULGA_HOME ({PulgaHome}):\n");

            var profilesTable = new TableView<string>
            {
                Items = GetProfiles()
                    .Select(p =>
                    {
                        if (p == CurrentProfile)
                            p += " (current profile)";

                        return p;
                    })
                    .ToArray()
            };

            // ReSharper disable once VariableHidesOuterVariable
            profilesTable.AddColumn(profile => profile, "Profile Name");

            var screen = new ScreenView(consoleRenderer ?? throw new ArgumentNullException(nameof(consoleRenderer)),
                invocationContext.Console) {Child = profilesTable};
            screen.Render();
            screen.Dispose();
        }

        /// <summary>
        /// Delete given profile.
        /// </summary>
        /// <param name="profileName"></param>
        public static void Delete(string profileName)
        {
            if (profileName == CurrentProfile)
            {
                WriteLine($"You cannot delete the current profile ({profileName})!");
                Environment.Exit(1);
            }

            GetProfile(profileName);

            var profilePath = Path.Combine(PulgaHome, $"{profileName}.json");

            if (!File.Exists(profilePath))
            {
                WriteLine($"Unable to find profile \"{profileName}\"!");
                Environment.Exit(1);
            }

            File.Delete(profilePath);
            WriteLine($"Profile {profileName} removed");
        }

        #region Local methods

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

        /// <summary>
        /// Enumerate application profiles.
        /// </summary>
        /// <returns>Enumerable entities</returns>
        private static IEnumerable<string> GetProfiles() => Directory.GetFiles(PulgaHome)
            .Select(Path.GetFileName)
            .Where(name => name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .Select(name => name.Replace(".json", string.Empty, StringComparison.OrdinalIgnoreCase));

        #endregion
    }
}