using System;
using System.Reflection;
using System.Reflection.PortableExecutable;
using ProjectTrumps.Core;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var session = new SessionController();
            session.RunCoreSession();
        }
    }
}