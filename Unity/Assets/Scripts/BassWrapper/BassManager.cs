using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Un4seen.Bass;
using Assets.Scripts.NetworkWrapper;
using Assets.Scripts.Threads;
using System.IO;
using System.Collections;

namespace Assets.Scripts.BassWrapper
{
    public class BassManager : MonoBehaviour
    {
        private ThreadedJob _GetMusics;
        private ThreadedJob _MyJob;
        private int _CurrentStream = 0;
        private bool _IsEnabled = false;
        private double _CurrentStreamTime = 0.0d;
        private double _CurrentTime = 0.0d;
        private List<string> _Musics;
        private DirectoryInfo _CurrentDirectory;
        private Vector2 _ScrollPosition;
        private bool _ShowNavigator;
        private GroovesharkGUI _GroovesharkGUI;

        public List<string> Musics
        {
            get { return _Musics; }
            set { _Musics = value; }
        }

        public bool UseGrooveshark;
        private Grooveshark _Grooveshark;

        private void OnDestroy()
        {
            if (_CurrentStream != 0)
                Bass.BASS_StreamFree(_CurrentStream);
            Bass.BASS_Free();
        }

        private void Awake()
        {
            BassNet.Registration("delporte.valentin@gmail.com", "2X9231427152222");
        }

        private IEnumerator Start()
        {
            _Musics = new List<string>();
            _ShowNavigator = true;
            _Grooveshark = GameObject.FindGameObjectWithTag("GroovesharkAPI").GetComponent<Grooveshark>();
            _GroovesharkGUI = gameObject.AddComponent<GroovesharkGUI>();
            _GroovesharkGUI.enabled = UseGrooveshark;
            //_GroovesharkGUI = new GroovesharkGUI(_Grooveshark);

            if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
                _IsEnabled = false;
            else
            {
                _IsEnabled = true;

                _CurrentDirectory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
            }

            //yield return StartCoroutine(_Grooveshark.PingService());
            yield return StartCoroutine(_Grooveshark.StartSession());
            yield return StartCoroutine(_Grooveshark.GetCountry());
            //yield return StartCoroutine(_Grooveshark.GetStreamKeyStreamServer(new Action<string>(StartGroovesharkStream)));
        }

        public void StartGroovesharkStream(string url)
        {
            if (_CurrentStream != 0)
            {
                Bass.BASS_ChannelStop(_CurrentStream);
                _CurrentStream = 0;
            }

            int stream = Bass.BASS_StreamCreateURL(url, 0, BASSFlag.BASS_SAMPLE_FLOAT, null, IntPtr.Zero);
            if (stream != 0)
            {
                UnityEngine.Debug.Log("Start streaming");
                Bass.BASS_ChannelPlay(stream, false);
                _CurrentStream = stream;
                long len = Bass.BASS_ChannelGetLength(_CurrentStream, BASSMode.BASS_POS_BYTES);
                _CurrentStreamTime = Bass.BASS_ChannelBytes2Seconds(_CurrentStream, len);
                _CurrentTime = 0.0d;
            }
            else
            {
                UnityEngine.Debug.Log("Cannot start music " + Bass.BASS_ErrorGetCode());
            }
        }

        public void StartMusic(string title)
        {
            UnityEngine.Debug.Log("Start " + title);

            if (!_IsEnabled)
                return;

            UnityEngine.Debug.Log("Engine is started");

            if (_CurrentStream != 0)
            {
                Bass.BASS_ChannelStop(_CurrentStream);
                _CurrentStream = 0;
            }

            int stream = Bass.BASS_StreamCreateFile(title, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT);
            if (stream != 0)
            {
                UnityEngine.Debug.Log("Start streaming");
                Bass.BASS_ChannelPlay(stream, false);
                _CurrentStream = stream;
                long len = Bass.BASS_ChannelGetLength(_CurrentStream, BASSMode.BASS_POS_BYTES);
                _CurrentStreamTime = Bass.BASS_ChannelBytes2Seconds(_CurrentStream, len);
                _CurrentTime = 0.0d;
            }
            else
            {
                UnityEngine.Debug.Log("Cannot start music " + Bass.BASS_ErrorGetCode());
                Bass.BASS_ChannelStop(_CurrentStream);
                _CurrentStream = 0;
            }
        }

        private void Update()
        {
            if (_MyJob != null)
            {
                if (_MyJob.Update())
                {
                    UnityEngine.Debug.Log("Job done");
                    _MyJob = null;
                }
            }

            if (_GetMusics != null)
            {
                if (_GetMusics.Update())
                {
                    UnityEngine.Debug.Log("Job done");
                    _GetMusics = null;
                }
            }

            RenderSettings.haloStrength = GetIntensity();

            if (Input.GetKeyDown(KeyCode.Space))
                _ShowNavigator = !_ShowNavigator;

            if (_CurrentStreamTime != 0)
            {
                _CurrentTime += Time.deltaTime;
                if (_CurrentTime >= _CurrentStreamTime)
                    StartMusic(null);
            }

        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                Bass.BASS_Pause();
            else
                Bass.BASS_Start();
        }

        private void OnGUI()
        {
            if (UseGrooveshark)
            {
                //_GroovesharkGUI.OnGUI();
            }
            else
            {
                if (_CurrentStream != 0)
                {
                    GUI.Label(new Rect(500.0f, 5.0f, 100.0f, 30.0f), new GUIContent(((int)(_CurrentTime / 60)).ToString("00") + ":" + ((int)(_CurrentTime % 60)).ToString("00") + " - " + ((int)(_CurrentStreamTime / 60)).ToString("00") + ":" + ((int)(_CurrentStreamTime % 60)).ToString("00")));
                    float energy = GetPeakFrequency();
                    GUI.Label(new Rect(500.0f, 30.0f, 100.0f, 30.0f), new GUIContent(energy.ToString("Energy : 0.0000")));
                }

                if (_ShowNavigator)
                {
                    _ScrollPosition = GUILayout.BeginScrollView(_ScrollPosition, GUILayout.Width(300.0f), GUILayout.Height(500.0f));
                    if (_CurrentDirectory != null)
                    {
                        GUILayout.Button(new GUIContent(_CurrentDirectory.Name));

                        if (_CurrentDirectory.Parent != null)
                        {
                            if (GUILayout.Button(".."))
                                _CurrentDirectory = _CurrentDirectory.Parent;
                        }

                        try
                        {
                            DirectoryInfo[] subDirs = _CurrentDirectory.GetDirectories();
                            foreach (var dirInfo in subDirs)
                            {
                                if (GUILayout.Button(new GUIContent(dirInfo.Name)))
                                    _CurrentDirectory = dirInfo;
                            }
                        }
                        catch (UnauthorizedAccessException) { }

                        PrintFiles("*.ogg");
                        PrintFiles("*.mp3");
                        PrintFiles("*.wav");
                    }
                    GUILayout.EndScrollView();
                }
            }
        }

        private void PrintFiles(string extension)
        {
            FileInfo[] files = null;
            try
            {
                files = _CurrentDirectory.GetFiles(extension);
            }
            catch (UnauthorizedAccessException) { }

            if (files != null)
            {
                foreach (var fi in files)
                {
                    if (GUILayout.Button(new GUIContent(fi.Name)))
                        StartMusic(fi.FullName);
                }
            }
        }

        public float[] GetPeaks(int number)
        {
            int b0 = 0;
            float[] fft = GetCurrentChannelData();
            if (fft == null)
                return null;
            float[] peaks = new float[number];

            for (int x = 0; x < number; ++x)
            {
                peaks[x] = 0.0f;
                int b1 = (int)Mathf.Pow(2, x * 10.0f / (number - 1));
                if (b1 > 1023)
                    b1 = 1023;
                if (b1 <= b0)
                    b1 = b0 + 1;
                for (; b0 < b1; b0++)
                    if (peaks[x] < fft[1 + b0])
                        peaks[x] = fft[1 + b0];
            }

            return peaks;
        }

        public float[] GetCurrentChannelData()
        {
            if (_CurrentStream != 0)
            {
                float[] buffer = new float[1024];
                Bass.BASS_ChannelGetData(_CurrentStream, buffer, (int)BASSData.BASS_DATA_FFT2048);
                return buffer;
            }
            else
                return null;
        }

        public float GetIntensity()
        {
            Un4seen.Bass.Misc.Visuals v = new Un4seen.Bass.Misc.Visuals();
            float energy = 0.0f;

            v.DetectPeakFrequency(_CurrentStream, out energy);

            return energy;
        }

        public void AddMusic(string path)
        {
            lock (_Musics)
                _Musics.Add(path);
        }

        public float GetPeakFrequency()
        {
            Un4seen.Bass.Misc.Visuals v = new Un4seen.Bass.Misc.Visuals();
            float energy = 0.0f;

            v.DetectPeakFrequency(_CurrentStream, out energy);

            return energy;
        }

        //public float GetLowPitchIntensity()
        //{
        //    float[] peaks = GetPeaks(512);
        //    float avg = 0.0f;

        //    for (int i = 0; i < peaks.Length; ++i)
        //        avg += peaks[i];
        //    avg /= peaks.Length;

        //    return avg;
        //}

        //public float GetHighPitchIntensity()
        //{

        //}
    }
}
