using System;
using System.Diagnostics;
using System.Net;

namespace Obvs.Configuration
{
    public static class RequesterId
    {
        public static string Create()
        {
            Process process = Process.GetCurrentProcess();
            string userName = Environment.GetEnvironmentVariable("USERNAME") ?? "";
            string hostName = System.Net.Dns.GetHostName();
            return string.Format("{0}-{1}-{2}-{3}", process.ProcessName, hostName, userName, process.Id);
        }
    }
}