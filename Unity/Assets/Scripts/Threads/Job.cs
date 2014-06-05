using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Threads
{
    public sealed class Job : ThreadedJob
    {
        public Vector3[] InData;
        public Vector3[] OutData;

        protected override void ThreadFunction()
        {
            // Do your threaded task. DON'T use the Unity API here as you are not in the main thread anymore ...
            for (int i = 0; i < 100000000; ++i)
            {
                InData[i % InData.Length] += InData[(i + 1) % InData.Length];
            }
        }

        protected override void OnFinished()
        {
            // This is executed by the Unity main thrad when the job is finished
            for (int i = 0; i < InData.Length; ++i)
            {
                UnityEngine.Debug.Log("Results(" + i + "): " + InData[i]);
            }
        }
    }
}
