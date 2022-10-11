using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace FullscreenEditor {
    public static class Cmd {

        public static string Run(string command, params object[] formatArgs) {
            return Run(command, false, formatArgs);
        }

        public static string Run(string command, bool asAdmin, params object[] formatArgs) {
            command = string.Format(command, formatArgs);

            var stdout = string.Empty;
            var stderr = string.Empty;
            var exitCode = Run(command, asAdmin, out stdout, out stderr);

            if (exitCode == 0)
                return stdout.Trim();

            throw new Exception(string.Format("Command {0} exited with code {1}", command, exitCode));
        }

        public static int Run(string command, bool asAdmin, out string stdout, out string stderr) {
            var proc = new Process();
            var stdoutBuilder = new StringBuilder();
            var stderrBuilder = new StringBuilder();

            proc.EnableRaisingEvents = true;

            if (Application.platform == RuntimePlatform.WindowsEditor)
                proc.StartInfo = new ProcessStartInfo() {
                    FileName = "cmd.exe",
                    Arguments = "/C \"" + command + "\"",
                    UseShellExecute = asAdmin,
                    RedirectStandardError = !asAdmin,
                    RedirectStandardOutput = !asAdmin,
                    Verb = asAdmin? "runas": "",
                    CreateNoWindow = !asAdmin,
                    WorkingDirectory = Environment.CurrentDirectory
                };
            else
                proc.StartInfo = new ProcessStartInfo() {
                    FileName = "/bin/bash",
                    Arguments = "-c \"" + command + "\"",
                    UseShellExecute = asAdmin,
                    RedirectStandardError = !asAdmin,
                    RedirectStandardOutput = !asAdmin,
                    CreateNoWindow = !asAdmin,
                    WorkingDirectory = Environment.CurrentDirectory
                };

            if (!asAdmin) {
                proc.OutputDataReceived += (sender, args) => {
                    if (!string.IsNullOrEmpty(args.Data))
                        stdoutBuilder.AppendLine(args.Data);
                };

                proc.ErrorDataReceived += (sender, args) => {
                    if (!string.IsNullOrEmpty(args.Data))
                        stderrBuilder.AppendLine(args.Data);
                };
            }

            //proc.Exited += (sender, args) => {
            //    Debug.LogWarningFormat("Command {0} exited with code {1}", command, proc.ExitCode);
            //};

            if (proc.Start()) {
                if (!asAdmin) {
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                }
                proc.WaitForExit();

                stdout = stdoutBuilder.ToString();
                stderr = stderrBuilder.ToString();

                Logger.Debug("{3}: {0}\nstdout: {1}\nstderr: {2}", command, stdout, stderr, proc.ExitCode);

                return proc.ExitCode;
            }

            throw new Exception(string.Format("Failed to start process for {0}", command));
        }

    }
}
