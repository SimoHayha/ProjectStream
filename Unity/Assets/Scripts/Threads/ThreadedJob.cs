﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Threads
{
    public class ThreadedJob
    {
        private bool _IsDone = false;
        private object _Handle = new Object();
        private System.Threading.Thread _Thread = null;

        public bool IsDone
        {
            get
            {
                bool tmp;
                lock (_Handle)
                {
                    tmp = _IsDone;
                }
                return tmp;
            }
            set
            {
                lock (_Handle)
                {
                    _IsDone = value;
                }
            }
        }

        public virtual void Start()
        {
            _Thread = new System.Threading.Thread(Run);
            _Thread.Start();
        }

        public virtual void Abort()
        {
            _Thread.Abort();
        }

        protected virtual void ThreadFunction() { }

        protected virtual void OnFinished() { }

        public virtual bool Update()
        {
            if (IsDone)
            {
                OnFinished();
                return true;
            }
            return false;
        }

        private void Run()
        {
            ThreadFunction();
            IsDone = true;
        }
    }
}
