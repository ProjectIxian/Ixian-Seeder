﻿using Fclp;
using IXICore;
using IXICore.Meta;
using IXICore.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IxianSeeder.Meta
{
    public class Config
    {
        // Providing pre-defined values
        // Can be read from a file later, or read from the command line
        public static int serverPort = 10234;

        private static int defaultServerPort = 10234;
        private static int defaultTestnetServerPort = 11234;

        public static NetworkType networkType = NetworkType.main;

        public static int apiPort = 8001;
        public static int testnetApiPort = 8101;

        public static Dictionary<string, string> apiUsers = new Dictionary<string, string>();

        public static List<string> apiAllowedIps = new List<string>();
        public static List<string> apiBinds = new List<string>();

        public static string configFilename = "ixian.cfg";
        public static string walletFile = "ixian.wal";

        public static int maxLogSize = 50;
        public static int maxLogCount = 10;

        public static int logVerbosity = (int)LogSeverity.info + (int)LogSeverity.warn + (int)LogSeverity.error;

        public static bool onlyShowAddresses = false;

        public static string dangerCommandlinePasswordCleartextUnsafe = "";

        public static int maxOutgoingConnections = 6;

        public static int maxIncomingClientNodes = 2000;

        public static bool enableActivity = false;

        // Store the device id in a cache for reuse in later instances
        public static string externalIp = "";

        // Read-only values
        public static readonly string version = "xseedc-0.9.3a"; // Seeder Node version

        public static readonly string checkVersionUrl = "https://www.ixian.io/seeder-update.txt";
        public static readonly int checkVersionSeconds = 6 * 60 * 60; // 6 hours

        // internal
        public static bool changePass = false;

        private Config()
        {

        }

        private static string outputHelp()
        {
            Program.noStart = true;

            Console.WriteLine("Starts a new instance of Ixian Seeder Node");
            Console.WriteLine("");
            Console.WriteLine(" IxianSeeder.exe [-h] [-v] [-t] [-x] [-c] [-p 10234] [-a 8081] [-i ip] [-w ixian.wal] [-n seed1.ixian.io:10234]");
            Console.WriteLine(" [--config ixian.cfg] [--maxLogSize 50] [--maxLogCount 10] [--maxOutgoingConnections]");
            Console.WriteLine(" [--maxIncomingClientNodes] [--walletPassword]");
            Console.WriteLine("");
            Console.WriteLine("    -h\t\t\t Displays this help");
            Console.WriteLine("    -v\t\t\t Displays version");
            Console.WriteLine("    -t\t\t\t Starts node in testnet mode");
            Console.WriteLine("    -x\t\t\t Change password of an existing wallet");
            Console.WriteLine("    -c\t\t\t Removes cache, peers.dat and ixian.log files before starting");
            Console.WriteLine("    -p\t\t\t Port to listen on");
            Console.WriteLine("    -a\t\t\t HTTP/API port to listen on");
            Console.WriteLine("    -i\t\t\t External IP Address to use");
            Console.WriteLine("    -w\t\t\t Specify location of the ixian.wal file");
            Console.WriteLine("    -n\t\t\t Specify which seed node to use");
            Console.WriteLine("    --config\t\t Specify config filename (default ixian.cfg)");
            Console.WriteLine("    --maxLogSize\t Specify maximum log file size in MB");
            Console.WriteLine("    --maxLogCount\t Specify maximum number of log files");
            Console.WriteLine("    --logVerbosity\t Sets log verbosity (0 = none, trace = 1, info = 2, warn = 4, error = 8)");
            Console.WriteLine("    --maxOutgoingConnections\t Max outgoing connections.");
            Console.WriteLine("    --maxIncomingClientNodes\t Max incoming client connections.");
            Console.WriteLine("    --walletPassword\t Specify the password for the wallet.");
            Console.WriteLine("    --enableActivity\t Enables activity/incoming tx processing.");
            Console.WriteLine("");
            Console.WriteLine("----------- Config File Options -----------");
            Console.WriteLine(" Config file options should use parameterName = parameterValue semantics.");
            Console.WriteLine(" Each option should be specified in its own line. Example:");
            Console.WriteLine("    seederPort = 10234");
            Console.WriteLine("    apiPort = 8081");
            Console.WriteLine("");
            Console.WriteLine(" Available options:");
            Console.WriteLine("    seederPort\t\t Port to listen on (same as -p CLI)");
            Console.WriteLine("    testnetSeederPort\t Port to listen on in testnet mode (same as -p CLI)");

            Console.WriteLine("    apiPort\t\t HTTP/API port to listen on (same as -a CLI)");
            Console.WriteLine("    apiAllowIp\t\t Allow API connections from specified source or sources (can be used multiple times)");
            Console.WriteLine("    apiBind\t\t Bind to given address to listen for API connections (can be used multiple times)");
            Console.WriteLine("    testnetApiPort\t HTTP/API port to listen on in testnet mode (same as -a CLI)");
            Console.WriteLine("    addApiUser\t\t Adds user:password that can access the API (can be used multiple times)");

            Console.WriteLine("    externalIp\t\t External IP Address to use (same as -i CLI)");
            Console.WriteLine("    addPeer\t\t Specify which seed node to use (same as -n CLI) (can be used multiple times)");
            Console.WriteLine("    addTestnetPeer\t Specify which seed node to use in testnet mode (same as -n CLI) (can be used multiple times)");
            Console.WriteLine("    maxLogSize\t\t Specify maximum log file size in MB (same as --maxLogSize CLI)");
            Console.WriteLine("    maxLogCount\t\t Specify maximum number of log files (same as --maxLogCount CLI)");
            Console.WriteLine("    logVerbosity\t Sets log verbosity (same as --logVerbosity CLI)");
            Console.WriteLine("    walletNotify\t Execute command when a wallet transaction changes");

            return "";
        }

        private static string outputVersion()
        {
            Program.noStart = true;

            // Do nothing since version is the first thing displayed

            return "";
        }


        private static void readConfigFile(string filename)
        {
            if (!File.Exists(filename))
            {
                return;
            }
            Logging.info("Reading config file: " + filename);
            List<string> lines = File.ReadAllLines(filename).ToList();
            foreach (string line in lines)
            {
                string[] option = line.Split('=');
                if (option.Length < 2)
                {
                    continue;
                }
                string key = option[0].Trim(new char[] { ' ', '\t', '\r', '\n' });
                string value = option[1].Trim(new char[] { ' ', '\t', '\r', '\n' });

                if (key.StartsWith(";"))
                {
                    continue;
                }
                Logging.info("Processing config parameter '" + key + "' = '" + value + "'");
                switch (key)
                {
                    case "seederPort":
                        Config.defaultServerPort = int.Parse(value);
                        break;
                    case "testnetSeederPort":
                        Config.defaultTestnetServerPort = int.Parse(value);
                        break;
                    case "apiPort":
                        apiPort = int.Parse(value);
                        break;
                    case "testnetApiPort":
                        testnetApiPort = int.Parse(value);
                        break;
                    case "apiAllowIp":
                        apiAllowedIps.Add(value);
                        break;
                    case "apiBind":
                        apiBinds.Add(value);
                        break;
                    case "addApiUser":
                        string[] credential = value.Split(':');
                        if (credential.Length == 2)
                        {
                            apiUsers.Add(credential[0], credential[1]);
                        }
                        break;
                    case "externalIp":
                        externalIp = value;
                        break;
                    case "addPeer":
                        CoreNetworkUtils.seedNodes.Add(new string[2] { value, null });
                        break;
                    case "addTestnetPeer":
                        CoreNetworkUtils.seedTestNetNodes.Add(new string[2] { value, null });
                        break;
                    case "maxLogSize":
                        maxLogSize = int.Parse(value);
                        break;
                    case "maxLogCount":
                        maxLogCount = int.Parse(value);
                        break;
                    case "walletNotify":
                        CoreConfig.walletNotifyCommand = value;
                        break;
                    case "logVerbosity":
                        logVerbosity = int.Parse(value);
                        break;
                    default:
                        // unknown key
                        Logging.warn("Unknown config parameter was specified '" + key + "'");
                        break;
                }
            }
        }
        public static void readFromCommandLine(string[] args)
        {
            // first pass
            var cmd_parser = new FluentCommandLineParser();

            // help
            cmd_parser.SetupHelp("h", "help").Callback(text => outputHelp());

            // config file
            cmd_parser.Setup<string>("config").Callback(value => configFilename = value).Required();

            cmd_parser.Parse(args);

            if (Program.noStart)
            {
                return;
            }

            readConfigFile(configFilename);



            // second pass
            cmd_parser = new FluentCommandLineParser();

            // testnet
            cmd_parser.Setup<bool>('t', "testnet").Callback(value => networkType = NetworkType.test).Required();

            cmd_parser.Parse(args);

            if (networkType == NetworkType.test)
            {
                serverPort = defaultTestnetServerPort;
                apiPort = testnetApiPort;
            }
            else
            {
                serverPort = defaultServerPort;
            }


            string seedNode = "";

            // third pass
            cmd_parser = new FluentCommandLineParser();

            bool start_clean = false; // Flag to determine if node should delete cache+logs

            // version
            cmd_parser.Setup<bool>('v', "version").Callback(text => outputVersion());

            // Check for password change
            cmd_parser.Setup<bool>('x', "changepass").Callback(value => changePass = value).Required();

            // Check for clean parameter
            cmd_parser.Setup<bool>('c', "clean").Callback(value => start_clean = value).Required();


            cmd_parser.Setup<int>('p', "port").Callback(value => Config.serverPort = value).Required();

            cmd_parser.Setup<int>('a', "apiport").Callback(value => apiPort = value).Required();

            cmd_parser.Setup<string>('i', "ip").Callback(value => externalIp = value).Required();

            cmd_parser.Setup<string>('w', "wallet").Callback(value => walletFile = value).Required();

            cmd_parser.Setup<string>('n', "node").Callback(value => seedNode = value).Required();

            cmd_parser.Setup<int>("maxLogSize").Callback(value => maxLogSize = value).Required();

            cmd_parser.Setup<int>("maxLogCount").Callback(value => maxLogCount = value).Required();

            cmd_parser.Setup<bool>("onlyShowAddresses").Callback(value => onlyShowAddresses = true).Required();

            cmd_parser.Setup<int>("maxOutgoingConnections").Callback(value => maxOutgoingConnections = value);

            cmd_parser.Setup<int>("maxIncomingClientNodes").Callback(value => maxIncomingClientNodes = value);

            cmd_parser.Setup<string>("walletPassword").Callback(value => dangerCommandlinePasswordCleartextUnsafe = value).SetDefault("");

            cmd_parser.Setup<bool>("enableActivity").Callback(value => enableActivity = true).Required();

            cmd_parser.Setup<int>("logVerbosity").Callback(value => logVerbosity = value).Required();

            cmd_parser.Parse(args);


            // Validate parameters

            if (start_clean)
            {
                Node.cleanCacheAndLogs();
            }

            if (seedNode != "")
            {
                if (networkType == NetworkType.test)
                {
                    CoreNetworkUtils.seedTestNetNodes = new List<string[]>
                        {
                            new string[2] { seedNode, null }
                        };
                }
                else
                {
                    CoreNetworkUtils.seedNodes = new List<string[]>
                        {
                            new string[2] { seedNode, null }
                        };
                }
            }
        }

    }

}