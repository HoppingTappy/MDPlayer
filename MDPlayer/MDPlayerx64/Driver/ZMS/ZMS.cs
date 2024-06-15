﻿using MDPlayer.Driver.MNDRV;
using MDPlayer.Driver.ZMS.nise68;
using MDSound;
using static MDPlayer.Driver.MXDRV.MXDRV;

namespace MDPlayer.Driver.ZMS
{
    public class ZMS(EnmFileFormat format) : baseDriver
    {
        private readonly EnmFileFormat format = format;
        private nise68.nise68 nise68;
        public mpcmX68k mpcm;
        public MDSound.ym2151_x68sound opmPCM;
        private int checkCounter = 0;
        private List<string> envZPDs = new List<string>();
        public int version = 0;
        private FMTimer timerOPM;
        public Pcm8St[] pcm8St = new Pcm8St[8] { new(), new(), new(), new(), new(), new(), new(), new() };
        public MPCMSt[] mpcmSt = new MPCMSt[16] { new(), new(), new(), new(), new(), new(), new(), new(), new(), new(), new(), new(), new(), new(), new(), new() };

        public class MPCMSt
        {
            public bool Keyon = false;
            public bool Keyoff = false;
            public byte type = 0;
            public byte orig = 0;
            public int adrs_ptr = 0;
            public uint size = 0;
            public uint start = 0;
            public uint end = 0;
            public uint count = 0;
            public int frq = 0;
            public int pitch = 0;
            public int volume = 0;
            public int pan = 0;
        }

        public string PlayingFileName { get; internal set; }
        public byte[] CompiledData { get; set; }

        public override GD3 getGD3Info(byte[] buf, uint vgmGd3)
        {
            if(format== EnmFileFormat.ZMS)
            {
                return GetGD3InfoZMS(buf);
            }
            if (format == EnmFileFormat.ZMD)
            {
                return GetGD3InfoZMD(buf);
            }
            return new GD3();
        }

        private GD3 GetGD3InfoZMS(byte[] buf)
        {
            string text = System.Text.Encoding.GetEncoding("shift_jis").GetString(buf);
            string[] texts = text.Split("\r\n");
            string cmt = "";
            string comment = ".COMMENT";
            foreach(string s in texts)
            {
                if (s.ToUpper().Trim().IndexOf(comment) < 0) continue;
                cmt = s.Trim().Substring(s.ToUpper().Trim().IndexOf(comment) + comment.Length).Trim();
                break;
            }
            GD3 gd3=new GD3();
            if(!string.IsNullOrEmpty(cmt))
            {
                gd3.TrackName = cmt;
                gd3.TrackNameJ = cmt;
            }
            return gd3;
        }

        private GD3 GetGD3InfoZMD(byte[] buf)
        {
            GD3 gd3 = new GD3();

            if (buf.Length < 8)
            {
                throw new Exception("Unknown zmd file");
            }
            else
            {
                int chkID1 = buf[0] * 0x100_0000 + buf[1] * 0x1_0000 + buf[2] * 0x100 + buf[3] * 0x1;
                int chkID2 = buf[4] * 0x100_0000 + buf[5] * 0x1_0000 + buf[6] * 0x100 + buf[7] * 0x1;
                if (chkID1 == 0x1a5a_6d75 && chkID2 == 0x5369_4330) version = 3;
                if (chkID1 == 0x105a_6d75 && chkID2 != 0x5369_4330) version = 2;

                if (version == 0)
                {
                    throw new Exception("Version check error");
                }
            }

            string cmt = "";
            try
            {
                if (version == 3)
                {
                    int ptr = buf[9 * 4 + 0] * 0x100_0000 + buf[9 * 4 + 1] * 0x1_0000
                        + buf[9 * 4 + 2] * 0x100 + buf[9 * 4 + 3] * 0x1 + 40;
                    int ePtr = ptr;
                    while (buf[ePtr] != 0x00)
                    {
                        if (buf[ePtr] == 0x0d && buf[ePtr + 1] == 0x0a) break;
                        ePtr++;
                    }

                    cmt = System.Text.Encoding.GetEncoding("shift_jis").GetString(buf, ptr, ePtr - ptr);
                }
            }
            catch
            {
                ;//何もしない
            }

            if (!string.IsNullOrEmpty(cmt))
            {
                gd3.TrackName = cmt;
                gd3.TrackNameJ = cmt;
            }
            return gd3;
        }

        public override bool init(byte[] vgmBuf, ChipRegister chipRegister, EnmModel model, EnmChip[] useChip, uint latency, uint waitTime)
        {
            GD3 = getGD3Info(vgmBuf, 0);
            this.chipRegister = chipRegister;
            LoopCounter = 0;
            vgmCurLoop = 0;
            this.model = model;
            vgmFrameCounter = -latency - waitTime;
            SetZPDSearchPath();

            try
            {
                Run(vgmBuf);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public override bool init(byte[] vgmBuf, int fileType, ChipRegister chipRegister, EnmModel model, EnmChip[] useChip, uint latency, uint waitTime)
        {
            throw new NotImplementedException();
        }

        public override void oneFrameProc()
        {
            try
            {
                vgmSpeedCounter += (double)Common.VGMProcSampleRate / setting.outputDevice.SampleRate * vgmSpeed;
                while (vgmSpeedCounter >= 1.0)
                {
                    vgmSpeedCounter -= 1.0;

                    if (vgmFrameCounter > -1)
                    {
                        Counter++;

                        if (version == 2)
                        {
                            timerOPM.timer();
                            while ((timerOPM.ReadStatus() & 3) != 0)
                            {
                                nise68.TrapOPM();// true, true, true);
                            }
                        }
                        else
                        {
                            //virtualFrameCounter++;
                            while (nise68.IntTimer())
                            {
#if DEBUG
                                nise68.Trap(0x8e, true, true, true);
                                //nise68.Trap(0x8e);
#else
        nise68.Trap(0x8e);
#endif
                            }
                        }
                    }
                    vgmFrameCounter++;

                }

                checkCounter--;
                if (checkCounter < 0)
                {
                    checkCounter = 100;
                    if (version == 2)
                    {
                        //演奏中か確認
                        nise68.reg.SetDl(1, 0x09);//m_stat
                        nise68.reg.SetDl(2, 0);//チェックモード(0:全チャンネル検査)
                        nise68.Trap(3 + 32);
                        uint d0 = nise68.reg.GetDl(0);
                        if (d0 == 0)
                            Stopped = true;

                        //ループ回数チェック
                        nise68.reg.SetDl(1, 0x4d);//get_loop_time
                        nise68.Trap(3 + 32);
                        d0 = nise68.reg.GetDl(0);
                        vgmCurLoop = d0 - 1;
                    }
                    else
                    {
                        //演奏中か確認
                        nise68.reg.SetDl(0, 0x0b);//ZM_PLAY_STATUS
                        nise68.reg.SetDl(1, 0);//チェックモード(0:全チャンネル検査)
                        nise68.reg.SetAl(1, 0);//検査結果格納バッファアドレス(0にすると検査結果を簡略して返す)
                        nise68.Trap(3 + 32);
                        uint d0 = nise68.reg.GetDl(0);
                        if (d0 == 0)
                            Stopped = true;

                        //ループ回数チェック
                        nise68.reg.SetDl(0, 0x59);//ZM_LOOP_CONTROL
                        nise68.reg.SetDl(1, 0xffff_ffff);//コントロールモード(-1 = ループ回数取得)
                        nise68.Trap(3 + 32);
                        d0 = nise68.reg.GetDl(0);
                        vgmCurLoop = d0 - 1;
                    }
                }
                //vgmCurLoop = mm.ReadUInt16(reg.a6 + dw.LOOP_COUNTER);
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
        }


        private void Run(byte[] vgmBuf)
        {
            //if (model == EnmModel.RealModel) { return; }
            string fn = PlayingFileName;
            string withoutExtFn;
            string? dn = Path.GetDirectoryName(fn);
            if (!string.IsNullOrEmpty(dn)) withoutExtFn = Path.Combine(dn, Path.GetFileNameWithoutExtension(fn));
            else withoutExtFn = Path.GetFileNameWithoutExtension(fn);
            string fnZMD = withoutExtFn + ".ZMD";
            string fnZMS = withoutExtFn + ".ZMS";

            MDPlayer.Driver.ZMS.nise68.Log.SetMsgWrite(MsgWrite);
            nise68 = new nise68.nise68();
            nise68.SetMPCM(version==2 ? PCM8CallBack : MPCMCallBack);
            nise68.SetOPM(OPMCallBack);
            nise68.SetMIDI(MIDICallBack, (int)Common.VGMProcSampleRate);
            nise68.SetSCC_A(SCCCallBack, (int)Common.VGMProcSampleRate);
            nise68.Init(envZPDs, version == 2);

            nise68.hmn.fb.Add(fnZMD, vgmBuf);
            //if (format == EnmFileFormat.ZMD) nise68.hmn.fb.Add(fnZMD, vgmBuf);
            //else
            //{
            //    //コンパイル
            //    if (model != EnmModel.RealModel || CompiledData == null) Compile(vgmBuf);
            //    else
            //    {
            //        //realはvirtualのコンパイル結果をもらう
            //        nise68.hmn.fb.Add(fnZMS, vgmBuf);
            //        nise68.hmn.fb.Add(fnZMD, CompiledData);
            //    }
            //}

            Play();
        }

        private void Play()
        {
            string fn = PlayingFileName;
            string withoutExtFn;
            string? dn = Path.GetDirectoryName(fn);
            if (!string.IsNullOrEmpty(dn)) withoutExtFn = Path.Combine(dn, Path.GetFileNameWithoutExtension(fn));
            else withoutExtFn = Path.GetFileNameWithoutExtension(fn);
            string fnZMD = withoutExtFn + ".ZMD";
            string crntDir = Path.GetDirectoryName(Application.ExecutablePath);
            string zmsc3 = Path.Combine(crntDir, "ZMSC3.X");
            if (!File.Exists(zmsc3))
            {
                log.Write(LogLevel.Information, "File not found : {0}", zmsc3);
                throw new FileNotFoundException(zmsc3);
            }
            string zmusic = Path.Combine(crntDir, "ZMUSIC.X");//ver2
            if (!File.Exists(zmusic))
            {
                log.Write(LogLevel.Information, "File not found : {0}", zmusic);
                throw new FileNotFoundException(zmusic);
            }

            int trp = 3 + 32;

            if (version == 2)
            {
                timerOPM = new FMTimer(true, null, 4000000);//, Common.VGMProcSampleRate);
                if (nise68.LoadRun(zmusic, "-P9212 -T2048", Path.GetDirectoryName(fnZMD), 0x00012000
                , true, true, true
                ) != 0) throw new Exception("zmusic regident Error");

                opmPCM?.x68sound[0].MountMemory(nise68.mem.mem);

                //演奏
                byte[] zmd = null;
                if (File.Exists(fnZMD))
                {
                    zmd = File.ReadAllBytes(fnZMD);
                    if (!nise68.hmn.fb.ContainsKey(fnZMD))
                    {
                        nise68.hmn.fb.Add(fnZMD, zmd);
                    }
                }
                else
                {
                    if (nise68.hmn.fb.ContainsKey(fnZMD))
                    {
                        zmd = nise68.hmn.fb[fnZMD];
                    }
                }
                if (zmd == null)
                {
                    throw new Exception(string.Format("Zmd[{0}] file not found. ", fnZMD));
                }
                uint fileSize = (uint)zmd.Length;
                uint filePtr = (uint)nise68.hmn.memMng.Malloc(fileSize);
                for (int i = 0; i < zmd.Length; i++)
                {
                    nise68.mem.PokeB((uint)(filePtr + i), zmd[i]);
                }

                nise68.reg.SetDl(1, 0x11);//play_cnv_data
                nise68.reg.SetDl(2, (uint)(zmd.Length - 7));
                nise68.reg.SetAl(1, filePtr + 7);
                nise68.Trap(trp);//, true, true, true);

                return;
            }

            //zmsc3常駐
            nise68.hmn.memMng = new memMng(0x0004_0000);

            if (nise68.LoadRun(zmsc3, "-w", Path.GetDirectoryName(fnZMD), 0x00012000
            , true, true, true
            ) != 0) throw new Exception("zmsc3 regident Error");

            //演奏
            //Log.WriteLine(LogLevel.Information, "");
            //Log.SetLogLevel(LogLevel.Information);

            //if ((rc = nise68.LoadRun("C:\\ZP3.R", "-PC:\\SAMPLE1\\SAMPLE.ZMS", "C:\\", 0x00042000
            //    , true, true, true
            //    )) != 0) Environment.Exit(rc);
            {
                //エラーストックバッファ解放
                nise68.reg.SetDl(0, 0x73);//ZM_FREE_MEM2
                nise68.reg.SetDl(3, 0x82645252);// 'ＥRR'
                nise68.Trap(trp);
            }
            {
                //ZMD解放
                nise68.reg.SetDl(0, 0x73);//ZM_FREE_MEM2
                nise68.reg.SetDl(3, 0x5a826c44);// 'ZＭD'
                nise68.Trap(trp);
            }
            {
                //ZM_COMPILER DETECT
                nise68.reg.SetDl(0, 0x6b);//ZM_HOOK_FNC_SERVICE
                nise68.reg.SetDl(1, 2);// ZM_COMPILER
                nise68.reg.SetAl(1, 0xffff_ffff);//detect_mode
                nise68.Trap(trp);
            }
            {
                //バージョンチェック(呼ぶだけ)
                nise68.reg.SetDl(0, 0x7e);//ZM_ZMUSIC_MODE
                nise68.reg.SetDl(1, 0xffff_ffff);//
                nise68.Trap(trp);
            }
            {
                //演奏
                byte[] zmd = nise68.hmn.fb[fnZMD];
                uint fileSize = (uint)zmd.Length;
                uint filePtr = (uint)nise68.hmn.memMng.Malloc(fileSize);
                for (int i = 0; i < zmd.Length; i++)
                {
                    nise68.mem.PokeB((uint)(filePtr + i), zmd[i]);
                }
                nise68.reg.SetDl(0, 0x10);//ZM_PLAY_ZMD
                nise68.reg.SetDl(2, fileSize);
                nise68.reg.SetAl(1, filePtr + 8);
                nise68.Trap(trp);
            }
            {
                //エラーの確認
                nise68.reg.SetDl(0, 0x6e);//ZM_STORE_ERROR
                nise68.reg.SetDl(1, 0xffff_ffff);
                nise68.reg.SetDl(2, 0x0000_0000);
                nise68.Trap(trp);
                //D0.lにエラーの個数が返る
            }

        }

        public bool Compile(byte[] vgmBuf)
        {
            string fn = PlayingFileName;
            string withoutExtFn;
            string? dn = Path.GetDirectoryName(fn);
            if (!string.IsNullOrEmpty(dn)) withoutExtFn = Path.Combine(dn, Path.GetFileNameWithoutExtension(fn));
            else withoutExtFn = Path.GetFileNameWithoutExtension(fn);
            string fnZMD = withoutExtFn + ".ZMD";
            string fnZMS = withoutExtFn + ".ZMS";
            string crntDir = Path.GetDirectoryName(Application.ExecutablePath);
            string zmc = Path.Combine(crntDir, "ZMC.X");
            if (!File.Exists(zmc))
            {
                log.Write(LogLevel.Information, "File not found : {0}", zmc);
                return false;//throw new FileNotFoundException(zmc);
            }

            MDPlayer.Driver.ZMS.nise68.Log.SetMsgWrite(MsgWrite);
            nise68 = new nise68.nise68();
            nise68.SetMPCM(MPCMCallBack);
            nise68.SetOPM(OPMCallBack);
            nise68.SetMIDI(MIDICallBack, (int)Common.VGMProcSampleRate);
            nise68.SetSCC_A(SCCCallBack, (int)Common.VGMProcSampleRate);
            nise68.Init(null, false);

            //コンパイル
            nise68.hmn.fb.Add(fnZMS, vgmBuf);
            if (nise68.LoadRun(zmc, Path.GetFileName(fnZMS), Path.GetDirectoryName(fnZMS), 0x00012000
            , true, true, true
            ) != 0)
            {
                log.Write(LogLevel.Information, "v3 Compile Error", zmc);
                return false;
            }
            CompiledData = nise68.hmn.fb[fnZMD];

            return true;
        }

        public bool Compilev2(byte[] vgmBuf)
        {
            string fn = PlayingFileName;
            string withoutExtFn;
            string? dn = Path.GetDirectoryName(fn);
            if (!string.IsNullOrEmpty(dn)) withoutExtFn = Path.Combine(dn, Path.GetFileNameWithoutExtension(fn));
            else withoutExtFn = Path.GetFileNameWithoutExtension(fn);
            string fnZMD = withoutExtFn + ".ZMD";
            string fnZMS = withoutExtFn + ".ZMS";
            string crntDir = Path.GetDirectoryName(Application.ExecutablePath);
            string zmusic = Path.Combine(crntDir, "ZMUSIC.X");
            if (!File.Exists(zmusic))
            {
                log.Write(LogLevel.Information, "File not found : {0}", zmusic);
                return false;//throw new FileNotFoundException(zmc);
            }

            MDPlayer.Driver.ZMS.nise68.Log.SetMsgWrite(MsgWrite);
            nise68 = new nise68.nise68();
            nise68.SetMPCM(PCM8CallBack);
            nise68.SetOPM(OPMCallBack);
            nise68.SetMIDI(MIDICallBack, (int)Common.VGMProcSampleRate);
            nise68.SetSCC_A(SCCCallBack, (int)Common.VGMProcSampleRate);
            nise68.Init(null, false);

            //コンパイル
            nise68.hmn.fb.Add(fnZMS, vgmBuf);
            if (nise68.LoadRun(zmusic, "-C "+Path.GetFileName(fnZMS), Path.GetDirectoryName(fnZMS), 0x00012000
            , true, true, true
            ) != 0)
            {
                log.Write(LogLevel.Information, "v2 Compile Error", zmusic);
                return false;
            }
            CompiledData = nise68.hmn.fb[fnZMD];

            return true;
        }

        private void MsgWrite(string arg1, object[] arg2)
        {
            log.Write(LogLevel.Information, arg1, arg2);
        }

        private int MPCMCallBack(int n)
        {
            switch (n & 0xfff0)
            {
                case 0x0000:
                    //Log.WriteLine(LogLevel.Trace2, "MPCM #M_KEY_ON(${0:X04})", n);
                    mpcm?.KeyOn(0, n & 0xf);
                    mpcmSt[n&0xf].Keyon = true;
                    break;
                case 0x0100:
                    //Log.WriteLine(LogLevel.Trace2, "MPCM #M_KEY_OFF(${0:X04})", n);
                    mpcm?.KeyOff(0, n & 0xf);
                    mpcmSt[n & 0xf].Keyoff = true;
                    break;
                case 0x0200:
                    //Log.WriteLine(LogLevel.Trace2, "MPCM #M_SET_PCM(${0:X04})", n);
                    MDSound.mpcmX68k.SETPCM ptr = new mpcmX68k.SETPCM();
                    ptr.adrs_buf = nise68.mem.mem;
                    mpcmSt[n & 0xf].type = ptr.type = nise68.mem.PeekB(0x00 + nise68.reg.GetAl(1));
                    mpcmSt[n & 0xf].orig = ptr.orig = nise68.mem.PeekB(0x01 + nise68.reg.GetAl(1));
                    mpcmSt[n & 0xf].adrs_ptr = ptr.adrs_ptr = (int)nise68.mem.PeekL(0x04 + nise68.reg.GetAl(1));
                    mpcmSt[n & 0xf].size = ptr.size = nise68.mem.PeekL(0x08 + nise68.reg.GetAl(1));
                    mpcmSt[n & 0xf].start = ptr.start = nise68.mem.PeekL(0x0c + nise68.reg.GetAl(1));
                    mpcmSt[n & 0xf].end = ptr.end = nise68.mem.PeekL(0x10 + nise68.reg.GetAl(1));
                    mpcmSt[n & 0xf].count = ptr.count = nise68.mem.PeekL(0x14 + nise68.reg.GetAl(1));
                    //nise68.DumpMemory((uint)ptr.adrs_ptr, (uint)(ptr.adrs_ptr + ptr.size));
                    mpcm?.SetPcm(0, n & 0xf, ptr);
                    break;
                case 0x0300:
                    //Log.WriteLine(LogLevel.Trace2, "MPCM #M_SET_FRQ(${0:X04}) D1${1:X08}", n, nise68.reg.GetDl(1));
                    mpcm?.SetFreq(0, n & 0xf, (int)nise68.reg.GetDl(1));
                    mpcmSt[n & 0xf].frq = (int)nise68.reg.GetDl(1);
                    break;
                case 0x0400:
                    //Log.WriteLine(LogLevel.Trace2, "MPCM #M_SET_PITCH(${0:X04}) D1${1:X04}", n, nise68.reg.GetDl(1));
                    mpcm?.SetPitch(0, n & 0xf, (int)nise68.reg.GetDl(1));
                    mpcmSt[n & 0xf].pitch = (int)nise68.reg.GetDl(1);
                    break;
                case 0x0500:
                    //Log.WriteLine(LogLevel.Trace2, "MPCM #M_SET_VOL(${0:X04}) = ${1:X02}", n, nise68.reg.GetDb(1));
                    mpcm?.SetVol(0, n & 0xf, (int)nise68.reg.GetDb(1));
                    mpcmSt[n & 0xf].volume = (int)nise68.reg.GetDb(1);
                    break;
                case 0x0600:
                    //Log.WriteLine(LogLevel.Trace2, "MPCM #M_SET_PAN(${0:X04}) = ${1:X02}", n, nise68.reg.GetDb(1));
                    mpcm?.SetPan(0, n & 0xf, (int)nise68.reg.GetDb(1));
                    mpcmSt[n & 0xf].pan = (int)nise68.reg.GetDb(1);
                    break;
                case 0x8000://
                    switch (n & 0x000f)
                    {
                        case 0x0:
                            //Log.WriteLine(LogLevel.Trace2, "MPCM #M_LOCK(${0:X04})", n);
                            break;
                        case 0x2://
                                 //Log.WriteLine(LogLevel.Trace2, "MPCM #M_INIT(${0:X04})", n);
                            mpcm?.Reset(0);
                            break;
                        case 0x5://
                                 //Log.WriteLine(LogLevel.Trace2, "MPCM #M_SET_VOLTBL(${0:X04})", n);
                            int[] vtbl = new int[128];
                            for (int i = 0; i < 128; i++)
                            {
                                vtbl[i] = (int)(nise68.mem.PeekW((uint)(nise68.reg.GetAl(1) + (i * 2))));
                            }
                            mpcm?.SetVolTableZms(0, (int)nise68.reg.GetDl(1), vtbl);
                            break;
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            return 0;
        }

        private int PCM8CallBack(int n)
        {
            int ch;
            switch (n & 0xfff0)
            {
                case 0x0000:
                    //x68Sound.Pcm8_Out((int)D0 & 0xff, A1, (int)D1, (int)D2);
                    opmPCM?.x68sound[0].X68Sound_Pcm8_Out((int)n & 0xff, null, nise68.reg.GetAl(1), (int)nise68.reg.GetDl(1), (int)nise68.reg.GetDl(2));//指定チャンネル発音開始
                    ch = (int)((n & 0xff) % 8);
                    pcm8St[ch].tablePtr = nise68.reg.GetAl(1);
                    pcm8St[ch].mode = nise68.reg.GetDl(1);
                    pcm8St[ch].length = nise68.reg.GetDl(2);
                    pcm8St[ch].Keyon = true;
                    break;
                case 0x0100:
                    switch (n & 0xffff)
                    {
                        case 0x0100:
                            ch = (int)((n & 0xff) % 8);
                            pcm8St[ch].tablePtr = 0;
                            pcm8St[ch].mode = 0;
                            pcm8St[ch].length = 0;
                            pcm8St[ch].Keyon = false;
                            opmPCM?.x68sound[0].X68Sound_Pcm8_Out((int)n & 0xff, null, 0, 0, 0);//指定チャンネル発音停止
                            break;
                        case 0x0101:
                            opmPCM?.x68sound[0].X68Sound_Pcm8_Abort();//全チャンネル発音停止
                            break;
                    }
                    break;
                case 0x01F0:
                    switch (n & 0xffff)
                    {
                        case 0x01FC:
                            nise68.reg.SetDl(0, 1);
                            break;
                    }
                    break;
                default:
                    break;
            }
            return 0;
        }

        private int OPMCallBack(int adr, int dat)
        {
            chipRegister.setYM2151Register(0, 0, adr, dat, model, YM2151Hosei[0], 0);
            timerOPM?.WriteReg((byte)adr, (byte)dat);
            return 0;
        }

        private int MIDICallBack(int n, byte dat)
        {
            //if (midiOutsBuff == null || midiOutsFrame == null) return 0;
            //if (n >= midiOutsBuff.Count) return 0;

            //midiOutsBuff[n].Add(dat);
            //midiOutsFrame[n].Add(virtualFrameCounter);
            chipRegister.sendMIDIout(model,n,new byte[] { dat });
            return 0;
        }

        private int SCCCallBack(int n, byte dat)
        {
            //if (midiOutsBuff == null || midiOutsFrame == null) return 0;
            //if (2 + n >= midiOutsBuff.Count) return 0;

            //midiOutsBuff[2 + n].Add(dat);
            //midiOutsFrame[2 + n].Add(virtualFrameCounter);
            chipRegister.sendMIDIout(model, 2+n, new byte[] { dat });
            return 0;
        }


        private void SetZPDSearchPath()
        {
            try
            {
                //環境変数"ZPD"を取得する
                string envZPD = "";
                try
                {
                    envZPD = Environment.GetEnvironmentVariable("zmusic_ZPD", System.EnvironmentVariableTarget.User);
                }
                catch
                {
                }
                if (!string.IsNullOrEmpty(envZPD))
                {
                    envZPDs = envZPD.Split(";").ToList();
                }
            }
            catch
            {
                envZPDs = new List<string>();
            }

        }

    }
}
