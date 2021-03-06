﻿using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace RegistryWatchers.Win32
{
    internal static class NativeMethods
    {
        internal static class Registry
        {
            internal enum RegWow64Options
            {
                None = 0,
                KEY_WOW64_64KEY = 0x0100,
                KEY_WOW64_32KEY = 0x0200
            }

            internal enum RegistryRights
            {
                ReadKey = 0x20019,
                WriteKey = 0x20006
            }

            internal static UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(0x80000002u);
            internal static UIntPtr HKEY_CURRENT_USER = new UIntPtr(0x80000001u);
            internal static UIntPtr HKEY_USERS = new UIntPtr(0x80000003u);

            [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
            internal static extern int RegOpenKeyEx(
                UIntPtr hKey,
                string subKey,
                int ulOptions,
                int samDesired,
                out IntPtr hkResult);

            [DllImport("Advapi32.dll")]
            internal static extern int RegNotifyChangeKeyValue(
               IntPtr hKey,
               bool watchSubtree,
               REG_NOTIFY_CHANGE notifyFilter,
               IntPtr hEvent,
               bool asynchronous
                );

            [Flags]
            internal enum REG_NOTIFY_CHANGE : uint
            {
                /// <summary>
                /// Notify the caller if a subkey is added or deleted
                /// </summary>
                NAME = 0x1,
                /// <summary>
                /// Notify the caller of changes to the attributes of the key,
                /// such as the security descriptor information
                /// </summary>
                ATTRIBUTES = 0x2,
                /// <summary>
                /// Notify the caller of changes to a value of the key. This can
                /// include adding or deleting a value, or changing an existing value
                /// </summary>
                LAST_SET = 0x4,
                /// <summary>
                /// Notify the caller of changes to the security descriptor of the key
                /// </summary>
                SECURITY = 0x8
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern SafeWaitHandle CreateWaitableTimer(IntPtr lpTimerAttributes, bool bManualReset, string lpTimerName);

        internal const UInt32 INFINITE = 0xFFFFFFFF;
        internal const UInt32 WAIT_ABANDONED = 0x00000080;
        internal const UInt32 WAIT_OBJECT_0 = 0x00000000;
        internal const UInt32 WAIT_TIMEOUT = 0x00000102;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

        [DllImport("kernel32.dll")]
        internal static extern uint WaitForMultipleObjects(uint nCount, IntPtr[] lpHandles, bool bWaitAll, uint dwMilliseconds);

        [DllImport("KERNEL32.DLL", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool CloseHandle(IntPtr hHandle);

        [DllImport("kernel32.dll")]
        internal static extern bool SetEvent(IntPtr hEvent);
    }
}