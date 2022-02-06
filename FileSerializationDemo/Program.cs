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

using FileSerializationDemo.ObjectFileSystemSerializer;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;

namespace FileSerializationDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger logger = LogManager.GetCurrentClassLogger();
        }
    }
}
