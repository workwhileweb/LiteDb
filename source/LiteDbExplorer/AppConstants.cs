using System;

namespace LiteDbExplorer
{
    public class AppConstants
    {
        public class SettingsPaths
        {
            public const string Environment = "_Environment";
        }

        public class Github
        {
            public const string RepositoryOwner = @"julianpaulozzi";
            public const string RepositoryName = @"LiteDbExplorer";
        }

        public class CmdlineCommands
        {
            public const string Open = "open";
            public const string New = "new";
            public const string Focus = "focus";
        }

        public class Versions
        {
            public static Version CurrentVersion => System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
        }
    }
}