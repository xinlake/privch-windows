﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XTransmit.Model.Server;

/**
 * shadowsocks-libev-win-x86_64, 2019-09-22
 * shadowsocks-libev 3.3.1
 * 
 * Updated: 2019-09-22
 */
namespace XTransmit.Utility
{
    public static class SSManager
    {
        public static readonly Dictionary<ServerProfile, Process> SSProcessMap = new Dictionary<ServerProfile, Process>();
        private static string SSExePath => $@"{App.PathShadowsocks}\{ss_local_exe_name}";
        private static readonly Random RandGen = new Random();

        private const string cygev_4_dll_name = "cygev-4.dll";
        private const string cygev_4_dll_md5 = "1d4ab5325fe69fd662ab1b9af8c03145";

        private const string cyggcc_s_seh_1_dll_name = "cyggcc_s-seh-1.dll";
        private const string cyggcc_s_seh_1_dll_md5 = "b84b92d9ddb884a0af4b2e11e4224f45";

        private const string cygmbedcrypto_3_dll_name = "cygmbedcrypto-3.dll";
        private const string cygmbedcrypto_3_dll_md5 = "c9d2f4a389f86d30fcbd94de90849546";

        private const string cygpcre_1_dll_name = "cygpcre-1.dll";
        private const string cygpcre_1_dll_md5 = "ef24e5503e42d0f7b7ef7c8dfe298769";

        private const string cygsodium_23_dll_name = "cygsodium-23.dll";
        private const string cygsodium_23_dll_md5 = "1ecdffa32acffaf104e45c6c4de5a430";

        private const string cygwin1_dll_name = "cygwin1.dll";
        private const string cygwin1_dll_md5 = "42c5eb56ae8be10f34b53bef76caa24e";

        private const string ss_local_exe_name = "ss-local-x.exe"; // name ss-local-x.exe is for process control
        private const string ss_local_exe_process = "ss-local-x";
        private const string ss_local_exe_md5 = "cd5a05ed703aaa8b17a463ca23a4d828";

        public static void KillRunning()
        {
            // this list contain only this app's "ss" process
            Process[] list = Process.GetProcessesByName(ss_local_exe_process);
            if (list != null && list.Length > 0)
            {
                // kill app's ss-local-x process
                try
                {
                    Process running = list.First(process => process.MainModule.FileName == SSExePath);
                    if (running != null)
                    {
                        running.CloseMainWindow();
                        running.Kill();
                        running.WaitForExit();
                    }
                }
                catch (Exception) { }

                foreach (Process process in list)
                {
                    process.Dispose();
                }
            }
        }

        public static bool Prepare()
        {
            // Creates all directories and subdirectories
            try { System.IO.Directory.CreateDirectory(App.PathShadowsocks); }
            catch { return false; }

            // Check binary files
            object[,] checks =
            {
                { $@"{App.PathShadowsocks}\{cygev_4_dll_name}", cygev_4_dll_md5, Properties.Resources.cygev_4_dll_gz },
                { $@"{App.PathShadowsocks}\{cyggcc_s_seh_1_dll_name}", cyggcc_s_seh_1_dll_md5 , Properties.Resources.cyggcc_s_seh_1_dll_gz },
                { $@"{App.PathShadowsocks}\{cygmbedcrypto_3_dll_name}", cygmbedcrypto_3_dll_md5, Properties.Resources.cygmbedcrypto_3_dll_gz },
                { $@"{App.PathShadowsocks}\{cygpcre_1_dll_name}", cygpcre_1_dll_md5 , Properties.Resources.cygpcre_1_dll_gz },
                { $@"{App.PathShadowsocks}\{cygsodium_23_dll_name}", cygsodium_23_dll_md5 , Properties.Resources.cygsodium_23_dll_gz },
                { $@"{App.PathShadowsocks}\{cygwin1_dll_name}", cygwin1_dll_md5 , Properties.Resources.cygwin1_dll_gz },
                { SSExePath, ss_local_exe_md5, Properties.Resources.ss_local_exe_gz },
            };

            int length = checks.GetLength(0);
            for (int i = 0; i < length; i++)
            {
                if (!FileUtil.CheckMD5((string)checks[i, 0], (string)checks[i, 1]))
                {
                    if (!FileUtil.UncompressGZ((string)checks[i, 0], (byte[])checks[i, 2]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool Start(ServerProfile server, int listen)
        {
            if (SSProcessMap.ContainsKey(server))
            {
                return true;
            }

            string arguments = $"-s {server.HostIP} -p {server.HostPort} -l {listen} -k {server.Password} -m {server.Encrypt} -t {server.Timeout}";

            try
            {
                Process process = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = SSExePath,
                        Arguments = arguments,
                        WorkingDirectory = App.PathShadowsocks,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        LoadUserProfile = false,
                    });

                server.ListenPort = listen;
                SSProcessMap.Add(server, process);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static void Stop(ServerProfile server)
        {
            if (SSProcessMap.ContainsKey(server))
            {
                Process process = SSProcessMap[server];

                try
                {
                    process.CloseMainWindow();
                    process.Kill();
                    process.WaitForExit();
                }
                catch (Exception) { }
                finally
                {
                    /**The dispose method calls Close
                    * https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process.close
                    */
                    process.Dispose();
                }

                server.ListenPort = -1;
                SSProcessMap.Remove(server);
            }
        }

        // Server Pool 
        public static ServerProfile GerRendom()
        {
            if (SSProcessMap.Count > 0)
            {
                int index = RandGen.Next(0, SSProcessMap.Count - 1);
                return SSProcessMap.Keys.ElementAt(index);
            }
            else
            {
                return null;
            }
        }
    }
}
