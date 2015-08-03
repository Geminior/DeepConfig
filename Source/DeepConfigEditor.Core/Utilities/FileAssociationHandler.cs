namespace DeepConfigEditor.Utilities
{
    using System;
    using System.Linq;
    using Microsoft.Win32;

    internal sealed class FileAssociationHandler
    {
        private const string ClassesSubKey = "SOFTWARE\\Classes";

        private string _appRegName;
        private bool _machineWide;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileAssociationHandler"/> class.
        /// </summary>
        /// <param name="appRegName">The unique registry class name of the application to make associations for. A key with this name will be added to the appropriate section under Software/Classes in the registry to control the association.</param>
        /// <param name="machineWide">if set to <c>true</c> the associations created will be machine wide and will require administrator privileges on most versions of Windows.</param>
        internal FileAssociationHandler(string appRegName, bool machineWide)
        {
            Ensure.ArgumentNotNullOrEmpty(appRegName, "appRegName");

            _appRegName = appRegName.Trim().TrimStart('.').Replace(" ", ".").Replace("/", ".").Replace("\\", ".");
            _machineWide = machineWide;
        }

        private RegistryKey RootKey
        {
            get { return _machineWide ? Registry.LocalMachine : Registry.CurrentUser; }
        }

        internal bool IsAssociated(string extension, string shellCommandName, string shellCommand)
        {
            Ensure.ArgumentNotNullOrEmpty(extension, "extension");
            Ensure.ArgumentNotNullOrEmpty(shellCommandName, "shellCommandName");
            Ensure.ArgumentNotNullOrEmpty(shellCommand, "shellCommand");

            extension = EnsureValidExtension(extension);

            //First check if an association exists
            string currentHandlerKeyName = null;
            using (var extKey = this.RootKey.OpenSubKey(MakeKey(ClassesSubKey, extension)))
            {
                if (extKey == null)
                {
                    return false;
                }

                currentHandlerKeyName = extKey.GetValue(null) as string;

                //If no handler is set there is no more to do
                if (string.IsNullOrEmpty(currentHandlerKeyName))
                {
                    return false;
                }
            }

            //Now check that the association matches the app at hand
            using (var shellKey = this.RootKey.OpenSubKey(MakeKey(ClassesSubKey, currentHandlerKeyName, "shell")))
            {
                if (shellKey == null)
                {
                    return false;
                }

                var curCmd = shellKey.GetValue(null) as string;
                if (!curCmd.Equals(shellCommandName, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                using (var cmdKey = shellKey.OpenSubKey(MakeKey(shellCommandName, "Command")))
                {
                    if (cmdKey == null)
                    {
                        return false;
                    }

                    curCmd = cmdKey.GetValue(null) as string;

                    return curCmd.Equals(shellCommand, StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        internal void Associate(string extension, string shellCommandName, string shellCommandCaption, string shellCommand)
        {
            Ensure.ArgumentNotNullOrEmpty(extension, "extension");
            Ensure.ArgumentNotNullOrEmpty(shellCommandName, "shellCommandName");
            Ensure.ArgumentNotNullOrEmpty(shellCommandCaption, "shellCommandCaption");
            Ensure.ArgumentNotNullOrEmpty(shellCommand, "shellCommand");

            extension = EnsureValidExtension(extension);

            //First check if an association exists
            string currentHandlerKeyName = null;
            using (var extKey = this.RootKey.CreateSubKey(MakeKey(ClassesSubKey, extension)))
            {
                currentHandlerKeyName = extKey.GetValue(null) as string;

                //If no handler is set, we set it to the app at hand
                if (string.IsNullOrEmpty(currentHandlerKeyName))
                {
                    extKey.SetValue(null, _appRegName, RegistryValueKind.String);
                    currentHandlerKeyName = _appRegName;
                }
            }

            string rollBackValue = null;
            bool piggyBackingSucceded = false;

            //If another program has associated itself, the first thing to try is to append the shell command to that program's shell options in order to preserve any other shell options of that program.
            if (currentHandlerKeyName != _appRegName)
            {
                rollBackValue = currentHandlerKeyName;

                using (var shellKey = this.RootKey.OpenSubKey(MakeKey(ClassesSubKey, currentHandlerKeyName, "shell"), true))
                {
                    if (shellKey != null)
                    {
                        //If the other program already defines a command with the same name, we need to abort this approach
                        var curCmds = shellKey.GetSubKeyNames();
                        if (!curCmds.Contains(shellCommandName, StringComparer.OrdinalIgnoreCase))
                        {
                            using (var cmdNameKey = shellKey.CreateSubKey(shellCommandName))
                            {
                                cmdNameKey.SetValue(null, shellCommandCaption, RegistryValueKind.String);

                                using (var cmdKey = cmdNameKey.CreateSubKey("Command"))
                                {
                                    cmdKey.SetValue(null, shellCommand, RegistryValueKind.String);
                                }
                            }

                            rollBackValue = shellKey.GetValue(null) as string;
                            shellKey.SetValue(null, shellCommandName, RegistryValueKind.String);

                            piggyBackingSucceded = true;
                        }
                    }
                }
            }

            //Now create the entry for the program at hand. Contents will depend on whether 'piggybacking' was done or not
            using (var appKey = this.RootKey.CreateSubKey(MakeKey(ClassesSubKey, _appRegName)))
            {
                if (!string.IsNullOrEmpty(rollBackValue))
                {
                    appKey.SetValue("HandlerRollback", rollBackValue, RegistryValueKind.String);
                }

                //No other program was registered to handle this extention or it could not be updated to support the new command
                if (!piggyBackingSucceded)
                {
                    using (var shellKey = appKey.CreateSubKey("shell"))
                    {
                        using (var cmdNameKey = shellKey.CreateSubKey(shellCommandName))
                        {
                            cmdNameKey.SetValue(null, shellCommandCaption, RegistryValueKind.String);

                            using (var cmdKey = cmdNameKey.CreateSubKey("Command"))
                            {
                                cmdKey.SetValue(null, shellCommand, RegistryValueKind.String);
                            }
                        }

                        shellKey.SetValue(null, shellCommandName, RegistryValueKind.String);
                    }

                    //In the case where another program is associated but could not be updated with the new command, we change the association on the extension
                    if (currentHandlerKeyName != _appRegName)
                    {
                        using (var extKey = this.RootKey.OpenSubKey(MakeKey(ClassesSubKey, extension), true))
                        {
                            extKey.SetValue(null, _appRegName, RegistryValueKind.String);
                        }
                    }
                }
            }
        }

        internal void Disassociate(string extension, string shellCommandName)
        {
            Ensure.ArgumentNotNullOrEmpty(extension, "extension");
            Ensure.ArgumentNotNullOrEmpty(shellCommandName, "shellCommandName");

            extension = EnsureValidExtension(extension);

            string rollBackValue = null;

            //Get the rollback value if it exists
            using (var appKey = this.RootKey.OpenSubKey(MakeKey(ClassesSubKey, _appRegName)))
            {
                if (appKey != null)
                {
                    rollBackValue = appKey.GetValue("HandlerRollback") as string;
                }
            }

            //Remove the app entry
            using (var clsKey = this.RootKey.OpenSubKey(ClassesSubKey, true))
            {
                clsKey.DeleteSubKeyTree(_appRegName, false);
            }

            //Update the extension entry
            string currentHandlerKeyName = null;
            using (var extKey = this.RootKey.OpenSubKey(MakeKey(ClassesSubKey, extension), true))
            {
                if (extKey == null)
                {
                    return;
                }

                currentHandlerKeyName = extKey.GetValue(null) as string;

                if (string.IsNullOrEmpty(currentHandlerKeyName))
                {
                    return;
                }

                //If the current handler is the app at hand, then roll back to the previous handler if applicable
                if (currentHandlerKeyName.Equals(_appRegName, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(rollBackValue))
                    {
                        extKey.DeleteValue(null);
                    }
                    else
                    {
                        extKey.SetValue(null, rollBackValue, RegistryValueKind.String);
                    }

                    return;
                }
            }

            //Update the entry of the app on which the app at hand has piggy backed
            using (var shellKey = this.RootKey.OpenSubKey(MakeKey(ClassesSubKey, currentHandlerKeyName, "shell"), true))
            {
                if (shellKey == null)
                {
                    return;
                }

                var curCmd = shellKey.GetValue(null) as string;

                if (curCmd.Equals(shellCommandName, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(rollBackValue))
                    {
                        shellKey.DeleteValue(null);
                    }
                    else
                    {
                        shellKey.SetValue(null, rollBackValue, RegistryValueKind.String);
                    }
                }

                shellKey.DeleteSubKeyTree(shellCommandName, false);
            }
        }

        private static string EnsureValidExtension(string extension)
        {
            extension = extension.Trim();
            if (!extension.StartsWith("."))
            {
                extension = "." + extension;
            }

            return extension;
        }

        private static string MakeKey(params string[] pathElements)
        {
            return string.Join("\\", pathElements);
        }
    }
}
