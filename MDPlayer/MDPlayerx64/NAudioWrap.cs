﻿using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.Asio;

namespace MDPlayer
{
    public class NAudioWrap
    {

        public delegate int naudioCallBack(short[] buffer, int offset, int sampleCount);
        public event EventHandler<StoppedEventArgs> PlaybackStopped;

        private WaveOutEvent waveOut;
        private WasapiOut wasapiOut;
        private DirectSoundOut dsOut;
        private AsioOut asioOut;
        private myAsioOut myAsioOut;
        private NullOut nullOut;
        private SineWaveProvider16 waveProvider;

        private static naudioCallBack callBack = null;
        private Setting setting = null;
        private SynchronizationContext syncContext = SynchronizationContext.Current;

        private bool myAsioSw = true;

        public NAudioWrap(int sampleRate, naudioCallBack nCallBack)
        {
            Init(sampleRate, nCallBack);
        }

        public void Init(int sampleRate, naudioCallBack nCallBack)
        {

            Stop();

            waveProvider = new SineWaveProvider16();
            waveProvider.SetWaveFormat(sampleRate, 2);

            callBack = nCallBack;

        }

        public void Start(Setting setting)
        {
            this.setting = setting;
            if (waveOut != null) waveOut.Dispose();
            waveOut = null;
            if (wasapiOut != null) wasapiOut.Dispose();
            wasapiOut = null;
            if (dsOut != null) dsOut.Dispose();
            dsOut = null;
            if (asioOut != null) asioOut.Dispose();
            asioOut = null;
            if (myAsioOut != null) myAsioOut.Dispose();
            myAsioOut = null;
            if (nullOut != null) ((IDisposable)nullOut).Dispose();
            nullOut = null;

            try
            {
                switch (setting.outputDevice.DeviceType)
                {
                    case 0:
                        waveOut = new WaveOutEvent();
                        waveOut.DeviceNumber = 0;
                        waveOut.DesiredLatency = setting.outputDevice.Latency;
                        for (int i = 0; i < WaveOut.DeviceCount; i++)
                        {
                            if (setting.outputDevice.WaveOutDeviceName == WaveOut.GetCapabilities(i).ProductName)
                            {
                                waveOut.DeviceNumber = i;
                                break;
                            }
                        }
                        waveOut.PlaybackStopped += DeviceOut_PlaybackStopped;
                        waveOut.Init(waveProvider);
                        waveOut.Play();
                        break;
                    case 1:
                        System.Guid g = System.Guid.Empty;
                        foreach (DirectSoundDeviceInfo d in DirectSoundOut.Devices)
                        {
                            if (setting.outputDevice.DirectSoundDeviceName == d.Description)
                            {
                                g = d.Guid;
                                break;
                            }
                        }
                        if (g == System.Guid.Empty)
                        {
                            dsOut = new DirectSoundOut(setting.outputDevice.Latency);
                        }
                        else
                        {
                            dsOut = new DirectSoundOut(g, setting.outputDevice.Latency);
                        }
                        dsOut.PlaybackStopped += DeviceOut_PlaybackStopped;
                        dsOut.Init(waveProvider);
                        dsOut.Play();
                        break;
                    case 2:
                        MMDevice dev = null;
                        var enumerator = new MMDeviceEnumerator();
                        var endPoints = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                        foreach (var endPoint in endPoints)
                        {
                            if (setting.outputDevice.WasapiDeviceName == string.Format("{0} ({1})", endPoint.FriendlyName, endPoint.DeviceFriendlyName))
                            {
                                dev = endPoint;
                                break;
                            }
                        }
                        if (dev == null)
                        {
                            wasapiOut = new WasapiOut(setting.outputDevice.WasapiShareMode ? AudioClientShareMode.Shared : AudioClientShareMode.Exclusive, setting.outputDevice.Latency);
                        }
                        else
                        {
                            wasapiOut = new WasapiOut(dev, setting.outputDevice.WasapiShareMode ? AudioClientShareMode.Shared : AudioClientShareMode.Exclusive, false, setting.outputDevice.Latency);
                        }
                        wasapiOut.PlaybackStopped += DeviceOut_PlaybackStopped;
                        wasapiOut.Init(waveProvider);
                        wasapiOut.Play();
                        break;
                    case 3:
                        if (AsioOut.isSupported())
                        {
                            int i = 0;
                            foreach (string s in AsioOut.GetDriverNames())
                            {
                                if (setting.outputDevice.AsioDeviceName == s)
                                {
                                    break;
                                }
                                i++;
                            }
                            if (myAsioSw)
                            {
                                myAsioOut = new myAsioOut(i);
                                myAsioOut.PlaybackStopped += DeviceOut_PlaybackStopped;
                                myAsioOut.Init(waveProvider);
                                myAsioOut.Play();
                            }
                            else
                            {
                                asioOut = new AsioOut(i);
                                asioOut.PlaybackStopped += DeviceOut_PlaybackStopped;
                                asioOut.Init(waveProvider);
                                asioOut.Play();
                            }
                        }
                        break;

                    case 5:
                        nullOut = new NullOut(true);
                        nullOut.PlaybackStopped += DeviceOut_PlaybackStopped;
                        nullOut.Init(waveProvider);
                        nullOut.Play();
                        break;
                }
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
                waveOut = new WaveOutEvent();
                waveOut.PlaybackStopped += DeviceOut_PlaybackStopped;
                waveOut.Init(waveProvider);
                waveOut.DeviceNumber = 0;
                waveOut.Play();
            }

        }

        private void DeviceOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            var handler = PlaybackStopped;
            if (handler != null)
            {
                if (this.syncContext == null)
                {
                    handler(this, e);
                }
                else
                {
                    syncContext.Post(state => handler(this, e), null);
                }
            }
        }

        /// <summary>
        /// コールバックの中から呼び出さないこと(ハングします)
        /// </summary>
        public void Stop()
        {
            if (waveOut != null)
            {
                try
                {
                    //waveOut.Pause();
                    waveOut.Stop();
                    while (waveOut.PlaybackState != PlaybackState.Stopped) { System.Threading.Thread.Sleep(1); }
                    waveOut.Dispose();
                }
                catch { }
                waveOut = null;
            }

            if (wasapiOut != null)
            {
                try
                {
                    //wasapiOut.Pause();
                    wasapiOut.Stop();
                    while (wasapiOut.PlaybackState != PlaybackState.Stopped) { System.Threading.Thread.Sleep(1); }
                    wasapiOut.Dispose();
                }
                catch { }
                wasapiOut = null;
            }

            if (dsOut != null)
            {
                try
                {
                    //dsOut.Pause();
                    dsOut.Stop();
                    while (dsOut.PlaybackState != PlaybackState.Stopped) { System.Threading.Thread.Sleep(1); }
                    dsOut.Dispose();
                }
                catch { }
                dsOut = null;
            }

            if (asioOut != null)
            {
                try
                {
                    //asioOut.Pause();
                    asioOut.Stop();
                    while (asioOut.PlaybackState != PlaybackState.Stopped) { System.Threading.Thread.Sleep(1); }
                    asioOut.Dispose();
                }
                catch { }
                asioOut = null;
            }

            if (myAsioOut != null)
            {
                try
                {
                    //asioOut.Pause();
                    myAsioOut.Stop();
                    while (myAsioOut.PlaybackState != PlaybackState.Stopped) { System.Threading.Thread.Sleep(1); }
                    myAsioOut.Dispose();
                }
                catch { }
                myAsioOut = null;
            }

            if (nullOut != null)
            {
                try
                {
                    nullOut.Stop();
                    while (nullOut.PlaybackState != PlaybackState.Stopped) { Thread.Sleep(1); }
                    ((IDisposable)nullOut).Dispose();
                }
                catch { }
                nullOut = null;
            }

            //一休み
            //for (int i = 0; i < 10; i++)
            //{
            //    System.Threading.Thread.Sleep(1);
            //    System.Windows.Forms.Application.DoEvents();
            //}
        }

        public class SineWaveProvider16 : WaveProvider16
        {
            public SineWaveProvider16()
            {
            }

            public override int Read(short[] buffer, int offset, int count)
            {
                return callBack(buffer, offset, count);
            }

        }

        public NAudio.Wave.PlaybackState? GetPlaybackState()
        {
            bool notNull = false;

            if (waveOut != null)
            {
                if (waveOut.PlaybackState != PlaybackState.Stopped) return waveOut.PlaybackState;
            }
            if (dsOut != null)
            {
                if (dsOut.PlaybackState != PlaybackState.Stopped) return dsOut.PlaybackState;
            }
            if (wasapiOut != null)
            {
                if (wasapiOut.PlaybackState != PlaybackState.Stopped) return wasapiOut.PlaybackState;
            }
            if (asioOut != null)
            {
                if (asioOut.PlaybackState != PlaybackState.Stopped) return asioOut.PlaybackState;
            }
            if (myAsioOut != null)
            {
                if (myAsioOut.PlaybackState != PlaybackState.Stopped) return myAsioOut.PlaybackState;
            }
            if (nullOut != null)
            {
                if (nullOut.PlaybackState != PlaybackState.Stopped) return nullOut.PlaybackState;
            }

            return notNull ? (PlaybackState?)PlaybackState.Stopped : null;
        }

        public int getAsioLatency()
        {
            if (myAsioOut != null) return myAsioOut.PlaybackLatency;
            if (asioOut != null) return asioOut.PlaybackLatency;
            return 0;
        }





        private long dmySamplePos;
        private Asio64Bit dmyTimeStamp = new();
        private long vSamplePos;
        private long vLatestSampleCount;
        private long rSamplePos;
        private int latency;

        private void GetNowSamplePosition(out long samplePos)
        {
            myAsioOut.basicDriver.GetSamplePosition(out dmySamplePos, ref dmyTimeStamp);
            samplePos = (dmySamplePos / 0x1_0000_0000);
            ////timeStamp = ((long)dmyTimeStamp.hi << 32) | (long)dmyTimeStamp.lo;
            //timeStamp = (long)dmyTimeStamp.hi;
        }

        public void UpdateSamplePosition(int sampleCount)
        {
            if (myAsioOut == null) return;
            GetNowSamplePosition(out vSamplePos);
            vSamplePos = (dmySamplePos / 0x1_0000_0000);
            vLatestSampleCount = sampleCount / 2;
        }

        public bool CheckLate()
        {
            if (myAsioOut == null) return true;
            if (rSamplePos >= vSamplePos - latency)//rが遅れている場合は問答無用で処理
                return true;
            return false;
        }

        public bool CheckLate2()
        {
            if (myAsioOut == null) return false;
            if (rSamplePos >= vSamplePos + vLatestSampleCount - latency)
            {
                return true;
            }

            rSamplePos++;
            return false;
        }

        public void resetMyAsioOut()
        {
            long old = vSamplePos;
            while (old == vSamplePos) { Thread.Sleep(0); }
            rSamplePos = vSamplePos;
        }

    }
}
