using IXICore;
using IXICore.Meta;
using IXICore.Network;
using IXICore.Utils;
using System;
using System.Linq;
using System.Threading;

namespace IxianSeeder.Meta
{
    public class StatsConsoleScreen
    {
        private DateTime startTime;

        private Thread thread = null;
        private bool running = false;

        private int consoleWidth = 55;
        private uint drawCycle = 0; // Keep a count of screen draw cycles as a basic method of preventing visual artifacts

        public StatsConsoleScreen()
        {
            Console.Clear();

            Console.CursorVisible = ConsoleHelpers.verboseConsoleOutput;

            // Start thread
            running = true;
            thread = new Thread(new ThreadStart(threadLoop));
            thread.Name = "Stats_Console_Thread";
            thread.Start();

            startTime = DateTime.UtcNow;
        }

        // Shutdown console thread
        public void stop()
        {
            running = false;
        }

        private void threadLoop()
        {
            while (running)
            {
                if (ConsoleHelpers.verboseConsoleOutput == false)
                {
                    // Clear the screen every 10 seconds to prevent any persisting visual artifacts
                    if (drawCycle > 5)
                    {
                        clearScreen();
                        drawCycle = 0;
                    }
                    else
                    {
                        drawScreen();
                        drawCycle++;
                    }
                }

                Thread.Sleep(2000);
            }
        }

        public void clearScreen()
        {
            Console.Clear();
            drawScreen();
        }

        public void drawInfoBox(int connectionsOut)
        {
            string server_version = checkForUpdate();
            bool update_avail = false;
            if (server_version != null)
            {
                if (server_version.CompareTo(Config.version) > 0)
                {
                    update_avail = true;
                }
            }

            if (update_avail)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                writeLine(" An update (" + server_version + ") of Ixian Seeder is available");
                writeLine(" Please visit https://www.ixian.io");
                Console.ResetColor();
            }
            else
            {
                if (!NetworkServer.isConnectable() && connectionsOut == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    writeLine(" Your node isn't connectable from the internet.");
                    writeLine(" Please set-up port forwarding for port " + IxianHandler.publicPort + ".");
                    writeLine(" Make sure you can connect to: " + IxianHandler.getFullPublicAddress());
                    Console.ResetColor();
                }
                else
                {
                    writeLine(" Thank you for running an Ixian Seeder node.");
                    writeLine(" For help please visit https://www.ixian.io");
                }
            }
        }

        public void drawScreen()
        {
            Console.SetCursorPosition(0, 0);

            int connectionsOut = NetworkClientManager.getConnectedClients(true).Count();
            int connectionsIn = NetworkServer.getConnectedClients().Count();

            writeLine("           ██╗██╗  ██╗██╗ █████╗ ███╗   ██╗           ");
            writeLine("           ██║╚██╗██╔╝██║██╔══██╗████╗  ██║           ");
            writeLine("           ██║ ╚███╔╝ ██║███████║██╔██╗ ██║           ");
            writeLine("           ██║ ██╔██╗ ██║██╔══██║██║╚██╗██║           ");
            writeLine("           ██║██╔╝ ██╗██║██║  ██║██║ ╚████║           ");
            writeLine("           ╚═╝╚═╝  ╚═╝╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝           ");
            writeLine(" {0}", (Config.version + " BETA ").PadLeft(53));
            writeLine(" {0}", ("http://localhost:" + Config.apiPort + "/"));
            writeLine("──────────────────────────────────────────────────────");
            drawInfoBox(connectionsOut);
            writeLine("──────────────────────────────────────────────────────");


            // Node status
            Console.Write(" Status:               ");

            string serviceStatus = "active";


            if (connectionsIn + connectionsOut < 1)
                serviceStatus = "connecting   ";

            if (Clock.getTimestamp() - BlockHeaderStorage.lastBlockHeaderTime > 1800) // if no block for over 1800 seconds
            {
                Console.ForegroundColor = ConsoleColor.Red;
                serviceStatus = "No block received for over 30 minutes";
            }

            writeLine(serviceStatus);
            Console.ResetColor();

            writeLine("");

            string connectionsInStr = "-";  // Default to no inbound connections accepted
            if (NetworkServer.isRunning())
            {
                // If the server is running, show the number of inbound connections
                connectionsInStr = String.Format("{0}", connectionsIn);
            }
            writeLine(" Connections (I/O):    {0}", connectionsInStr + "/" + connectionsOut);
            writeLine(" Presences:            {0}", PresenceList.getTotalPresences());

            writeLine("");
            
            writeLine("──────────────────────────────────────────────────────");

            TimeSpan elapsed = DateTime.UtcNow - startTime;

            writeLine(" Running for {0} days {1}h {2}m {3}s", elapsed.Days, elapsed.Hours, elapsed.Minutes, elapsed.Seconds);
            writeLine("");
            writeLine(" Press V to toggle stats. Esc key to exit.");

        }

        private void writeLine(string str, params object[] arguments)
        {
            Console.WriteLine(string.Format(str, arguments).PadRight(consoleWidth));
        }

        private string checkForUpdate()
        {
            if (!UpdateVerify.ready && !UpdateVerify.error) return null;
            if (UpdateVerify.ready)
            {
                if (UpdateVerify.error) return null;
                return UpdateVerify.serverVersion;
            }
            return null;
        }
    }
}
