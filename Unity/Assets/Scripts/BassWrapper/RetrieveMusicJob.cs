using Assets.Scripts.Threads;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Assets.Scripts.BassWrapper
{
    public class RetrieveMusicJob : ThreadedJob
    {
        private BassManager _Bass;

        public RetrieveMusicJob(BassManager bass)
        {
            _Bass = bass;
        }

        protected override void ThreadFunction()
        {
            DirectoryInfo root = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
            UnityEngine.Debug.Log(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
            if (root.Exists)
                WalkDirectoryTree(root);
        }

        private void WalkDirectoryTree(DirectoryInfo root)
        {
            FileInfo[] files = null;
            DirectoryInfo[] subDirs = null;

            try
            {
                files = root.GetFiles("*.ogg");
            }
            catch (UnauthorizedAccessException e)
            {
                UnityEngine.Debug.Log(e.ToString());
            }

            if (files != null)
            {
                foreach (var fi in files)
                    _Bass.AddMusic(fi.FullName);

                subDirs = root.GetDirectories();

                foreach (var dirInfo in subDirs)
                    WalkDirectoryTree(dirInfo);
            }
        }

        protected override void OnFinished()
        {
            foreach (var music in _Bass.Musics)
                UnityEngine.Debug.Log(music);
        }
    }
}
