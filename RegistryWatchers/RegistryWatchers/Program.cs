﻿using System;
using Microsoft.Win32;
using RegistryWatchers.Win32;

namespace RegistryWatchers
{
    class Program
    {
        static void Main(string[] args)
        {
            // Registry64を使用
            RegistryKeyWatcher watcher = new RegistryKeyWatcher(RegistryHive.LocalMachine, RegistryView.Registry64, @"SOFTWARE\\Test\\Watch");

            watcher.RegistryKeyChanged += (sender, eventArgs) => { Console.WriteLine("Changed"); };

            watcher.Start();

            // Registry64をBaseKeyとして開いておく
            RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            RegistryKey regKey = baseKey.OpenSubKey(@"SOFTWARE\\Test\\Watch");
            if (regKey != null)
            {
                int value = (int) regKey.GetValue("Trial");
            }
        }
    }
}
