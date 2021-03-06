﻿using System;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace RegistryWatchers.Win32
{
    internal class RegistryKeyWatcher : IDisposable
    {
        internal event EventHandler<RegistryKeyChangedEventArgs> RegistryKeyChanged;

        private RegistryHive Hive;
        private RegistryView View;
        private string KeyPath;

        private IntPtr _cancelEvent;

        internal RegistryKeyWatcher(RegistryHive hive, RegistryView view, string keyPath)
        {
            this.Hive = hive;
            this.View = view;
            this.KeyPath = keyPath;
        }

        internal void Cancel()
        {
            if (_cancelEvent != IntPtr.Zero)
            {
                NativeMethods.SetEvent(_cancelEvent);
                IsRunning = false;
            }
        }

        internal bool IsRunning { get; private set; } = false;
        internal void Start()
        {
            if (!IsRunning)
            {
                _cancelEvent = NativeMethods.CreateEvent(IntPtr.Zero, false, false, Guid.NewGuid().ToString());

                Task.Run(() =>
                {
                    Console.WriteLine($"Registry Key Watcher task is started");

                    IsRunning = true;

                    IntPtr regEvent = NativeMethods.CreateEvent(IntPtr.Zero, false, false, Guid.NewGuid().ToString());
                    IntPtr reg = IntPtr.Zero;

                    IntPtr[] waitEvents = new IntPtr[] { regEvent, _cancelEvent };

                    UIntPtr hive = UIntPtr.Zero;
                    if (Hive == RegistryHive.LocalMachine)
                    {
                        hive = NativeMethods.Registry.HKEY_LOCAL_MACHINE;
                    }
                    else if (Hive == RegistryHive.CurrentUser)
                    {
                        hive = NativeMethods.Registry.HKEY_CURRENT_USER;
                    }
                    else if (Hive == RegistryHive.Users)
                    {
                        hive = NativeMethods.Registry.HKEY_USERS;
                    }

                    if (hive == UIntPtr.Zero)
                    {
                        Console.WriteLine($"Failed to get hive pointer: Hive={Hive.ToString()}");
                        return;
                    }

                    if (0 != NativeMethods.Registry.RegOpenKeyEx(
                        hive,
                        KeyPath,
                        0,
                        (int)NativeMethods.Registry.RegistryRights.ReadKey | (View == RegistryView.Registry64 ? (int)NativeMethods.Registry.RegWow64Options.KEY_WOW64_64KEY : (int)NativeMethods.Registry.RegWow64Options.KEY_WOW64_32KEY),
                        out reg))
                    {
                        Console.WriteLine($"Failed to open registry: Hive={Hive.ToString()}, KeyPath={KeyPath}");
                        return;
                    }

                    while (true)
                    {
                        if (0 != NativeMethods.Registry.RegNotifyChangeKeyValue(reg, false, NativeMethods.Registry.REG_NOTIFY_CHANGE.LAST_SET, regEvent, true))
                        {
                            Console.WriteLine($"Failed to register RegNotify: Hive={Hive.ToString()}, KeyPath={KeyPath}");
                            break;
                        }

                        if (NativeMethods.WAIT_OBJECT_0 != NativeMethods.WaitForMultipleObjects((uint)waitEvents.Length, waitEvents, false, NativeMethods.INFINITE))
                        {
                            Console.WriteLine($"Registry Monitor Exit.");
                            break;
                        }

                        RegistryKeyChanged?.Invoke(this, new RegistryKeyChangedEventArgs(Hive.ToString(), KeyPath));
                    }

                    if (reg != IntPtr.Zero)
                    {
                        NativeMethods.CloseHandle(reg);
                        reg = IntPtr.Zero;
                    }
                    if (regEvent != IntPtr.Zero)
                    {
                        NativeMethods.CloseHandle(regEvent);
                        regEvent = IntPtr.Zero;
                    }
                    if (_cancelEvent != IntPtr.Zero)
                    {
                        NativeMethods.CloseHandle(_cancelEvent);
                        _cancelEvent = IntPtr.Zero;
                    }

                    Console.WriteLine($"Registry Key Watcher task is finished");
                });
            }
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (_cancelEvent != IntPtr.Zero)
                {
                    NativeMethods.CloseHandle(_cancelEvent);
                    _cancelEvent = IntPtr.Zero;
                }

                disposedValue = true;
            }
        }

         ~RegistryKeyWatcher()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
