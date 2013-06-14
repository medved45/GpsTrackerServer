using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GpsTrackerServer.Data;

namespace GpsTrackerServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Server.TrackerServer();
            server.StartServer();
            Console.ReadKey();
            server.StopServer();
           
        }
    }
}
