/*
 Author:    Sebastian Brenner
 Version:   0.6?
 Issues:    Can not handle derived types.
 License:   Noncommercial
 Nice to have features (tba):
    *Log Levels
    *Compression
    *Shared object reuse by reference
 */

using Newtonsoft.Json;
using NLog;
using System;


namespace FileSerializationDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            // Usage: see ReserializationTest.cs in FileSerializationDemoTests
        }

    }
}
