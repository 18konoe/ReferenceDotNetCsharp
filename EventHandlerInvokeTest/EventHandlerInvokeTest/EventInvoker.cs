﻿using System;

namespace EventHandlerInvokeTest
{
    public class EventInvoker
    {
        private bool _TrialValueSync = false;
        private bool _TrialValueAsync = false;

        public bool TrialValueSync
        {
            get
            {
                return _TrialValueSync;
            }
            set
            {
                Console.WriteLine("Set sync enter");
                if (_TrialValueSync != value)
                {
                    _TrialValueSync = value;
                    TrialValueChanged?.Invoke(this, _TrialValueSync);
                }
                Console.WriteLine("Set sync exit");
            }
        }

        public bool TrialValueAsync
        {
            get
            {
                return _TrialValueAsync;
            }
            set
            {
                Console.WriteLine("Set async enter");
                if (_TrialValueAsync != value)
                {
                    _TrialValueAsync = value;
                    TrialValueChanged?.BeginInvoke(this, _TrialValueAsync, null, null);
                }
                Console.WriteLine("Set async exit");
            }
        }

        public event EventHandler<bool> TrialValueChanged;
    }
}