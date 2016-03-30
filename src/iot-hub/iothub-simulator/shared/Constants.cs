using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared
{
    public class Constants
    {
        public const string DefaultHubName = "fl-iothub";
        public const string ConnectionString = "HostName={0};SharedAccessKeyName={1};SharedAccessKey={2}";

        public static string GetServiceString()
        {
            return String.Format(ConnectionString, GetHubName(), "service", "q4ElMfFhLF2rylihv0TiH0PsloriB1jcQyNRV1PKlZw=");
        }

        public static string GetRegistryWrite()
        {
            return String.Format(ConnectionString, GetHubName(), "registryReadWrite", "rQr78Ge21PT0R9P0PeOojp7gwhdDz1zLb23InT44t2Q="); 
        }

        public static string GetRegistryRead()
        {
            return String.Format(ConnectionString, GetHubName(), "registryRead", "IF7tU/cz6wxtx2m+wJNb+Co+ai4Jzs7a2WoMoOy0RIs=");
        }

        public static string GetHubName()
        {
            return DefaultHubName + ".azure-devices.net";
        }
    }
}
