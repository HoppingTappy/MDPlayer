﻿using MDPlayer;
using MDPlayer.Driver.FMP.Nise98;
using MDPlayer.Driver.MNDRV;
using MDPlayer.Driver.ZMS.nise68;
using MDSound;
using NiseC86ctl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MDSound.fm._ssg_callbacks;

namespace MDPlayer.Driver.ZMS
{
    public class ZMS : baseDriver
    {
        private EnmFileFormat format;
        private nise68.nise68 nise68;
        int rc;

        public ZMS(EnmFileFormat format)
        {
            this.format = format;
        }

        public string PlayingFileName { get; internal set; }

        public override GD3 getGD3Info(byte[] buf, uint vgmGd3)
        {
            return new GD3();
        }

        public override bool init(byte[] vgmBuf, ChipRegister chipRegister, EnmModel model, EnmChip[] useChip, uint latency, uint waitTime)
        {
            GD3 = getGD3Info(vgmBuf, 0);
            this.chipRegister = chipRegister;
            LoopCounter = 0;
            vgmCurLoop = 0;
            this.model = model;
            vgmFrameCounter = -latency - waitTime;

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
            if (model == EnmModel.RealModel) { return; }
            try
            {
                vgmSpeedCounter += (double)Common.VGMProcSampleRate / setting.outputDevice.SampleRate * vgmSpeed;
                while (vgmSpeedCounter >= 1.0)
                {
                    vgmSpeedCounter -= 1.0;

                    if (vgmFrameCounter > -1)
                    {
                        Counter++;

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
                    vgmFrameCounter++;

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
            if (model == EnmModel.RealModel) { return; }
            string fn = PlayingFileName;
            string withoutExtFn;
            string? dn = Path.GetDirectoryName(fn);
            if (!string.IsNullOrEmpty(dn)) withoutExtFn = Path.Combine(dn, Path.GetFileNameWithoutExtension(fn));
            else withoutExtFn = Path.GetFileNameWithoutExtension(fn);
            string fnZMD = withoutExtFn + ".ZMD";
            string fnZMS = withoutExtFn + ".ZMS";
            string ext = Path.GetExtension(fn).ToUpper();

            string crntDir = Environment.CurrentDirectory;
            string zmsc3 = Path.Combine(crntDir, "ZMSC3.X");
            string zmc = Path.Combine(crntDir, "ZMC.X");

            if (!File.Exists(zmsc3))
            {
                log.Write(LogLevel.Information, "File not found : {0}", zmsc3);
                throw new FileNotFoundException(zmsc3);
            }
            if (!File.Exists(zmc))
            {
                log.Write(LogLevel.Information, "File not found : {0}", zmc);
                throw new FileNotFoundException(zmc);
            }

            MDPlayer.Driver.ZMS.nise68.Log.SetMsgWrite(MsgWrite);
            nise68 = new nise68.nise68();
            nise68.SetMPCM(MPCMCallBack);
            nise68.SetOPM(OPMCallBack);
            nise68.SetMIDI(MIDICallBack, (int)setting.outputDevice.SampleRate);
            nise68.SetSCC_A(SCCCallBack, (int)setting.outputDevice.SampleRate);
            nise68.Init();

            //コンパイル
            if (format== EnmFileFormat.ZMS)
            {
                nise68.hmn.fb.Add(fnZMS, vgmBuf);
                if ((rc = nise68.LoadRun(zmc, Path.GetFileName(fnZMS), Path.GetDirectoryName(fnZMS), 0x00012000
                , true, true, true
                )) != 0) throw new Exception("Compile Error");
            }

            //zmsc3常駐

            nise68.hmn.memMng = new memMng(0x0004_0000);

            if ((rc = nise68.LoadRun(zmsc3, "-w", Path.GetDirectoryName(fnZMD), 0x00012000
            , true, true, true
            )) != 0) throw new Exception("zmsc3 regident Error");

            //演奏
            //Log.WriteLine(LogLevel.Information, "");
            //Log.SetLogLevel(LogLevel.Information);

            //if ((rc = nise68.LoadRun("C:\\ZP3.R", "-PC:\\SAMPLE1\\SAMPLE.ZMS", "C:\\", 0x00042000
            //    , true, true, true
            //    )) != 0) Environment.Exit(rc);
            int trp = 3 + 32;
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
                byte[] zmd;
                if (nise68.hmn.fb.ContainsKey(fnZMD))
                {
                    zmd = nise68.hmn.fb[fnZMD];
                }
                else
                {
                    zmd = File.ReadAllBytes(fnZMD);
                    nise68.hmn.fb.Add(fnZMD, zmd);
                }
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

            //FlashMIDIoutBuf();

            //MakeRealTimer();


        }

        private void MsgWrite(string arg1, object[] arg2)
        {
            log.Write(LogLevel.Information, arg1, arg2);
        }

        int MPCMCallBack(int n)
        {
            //switch (n & 0xfff0)
            //{
            //    case 0x0000:
            //        //Log.WriteLine(LogLevel.Trace2, "MPCM #M_KEY_ON(${0:X04})", n);
            //        mpcm.KeyOn(0, n & 0xf);
            //        break;
            //    case 0x0100:
            //        //Log.WriteLine(LogLevel.Trace2, "MPCM #M_KEY_OFF(${0:X04})", n);
            //        mpcm.KeyOff(0, n & 0xf);
            //        break;
            //    case 0x0200:
            //        //Log.WriteLine(LogLevel.Trace2, "MPCM #M_SET_PCM(${0:X04})", n);
            //        MDSound.mpcmX68k.SETPCM ptr = new mpcmX68k.SETPCM();
            //        ptr.adrs_buf = nise68.mem.mem;
            //        ptr.type = nise68.mem.PeekB(0x00 + nise68.reg.GetAl(1));
            //        ptr.orig = nise68.mem.PeekB(0x01 + nise68.reg.GetAl(1));
            //        ptr.adrs_ptr = (int)nise68.mem.PeekL(0x04 + nise68.reg.GetAl(1));
            //        ptr.size = nise68.mem.PeekL(0x08 + nise68.reg.GetAl(1));
            //        ptr.start = nise68.mem.PeekL(0x0c + nise68.reg.GetAl(1));
            //        ptr.end = nise68.mem.PeekL(0x10 + nise68.reg.GetAl(1));
            //        ptr.count = nise68.mem.PeekL(0x14 + nise68.reg.GetAl(1));
            //        //nise68.DumpMemory((uint)ptr.adrs_ptr, (uint)(ptr.adrs_ptr + ptr.size));
            //        mpcm.SetPcm(0, n & 0xf, ptr);
            //        break;
            //    case 0x0300:
            //        //Log.WriteLine(LogLevel.Trace2, "MPCM #M_SET_FRQ(${0:X04}) D1${1:X08}", n, nise68.reg.GetDl(1));
            //        mpcm.SetPitch(0, n & 0xf, (int)nise68.reg.GetDl(1));
            //        break;
            //    case 0x0400:
            //        //Log.WriteLine(LogLevel.Trace2, "MPCM #M_SET_PITCH(${0:X04}) D1${1:X04}", n, nise68.reg.GetDl(1));
            //        mpcm.SetPitch(0, n & 0xf, (int)nise68.reg.GetDl(1));
            //        break;
            //    case 0x0500:
            //        //Log.WriteLine(LogLevel.Trace2, "MPCM #M_SET_VOL(${0:X04}) = ${1:X02}", n, nise68.reg.GetDb(1));
            //        mpcm.SetVol(0, n & 0xf, (int)nise68.reg.GetDb(1));
            //        break;
            //    case 0x0600:
            //        //Log.WriteLine(LogLevel.Trace2, "MPCM #M_SET_PAN(${0:X04}) = ${1:X02}", n, nise68.reg.GetDb(1));
            //        mpcm.SetPan(0, n & 0xf, (int)nise68.reg.GetDb(1));
            //        break;
            //    case 0x8000://
            //        switch (n & 0x000f)
            //        {
            //            case 0x0:
            //                //Log.WriteLine(LogLevel.Trace2, "MPCM #M_LOCK(${0:X04})", n);
            //                break;
            //            case 0x2://
            //                     //Log.WriteLine(LogLevel.Trace2, "MPCM #M_INIT(${0:X04})", n);
            //                mpcm.Reset(0);
            //                break;
            //            case 0x5://
            //                     //Log.WriteLine(LogLevel.Trace2, "MPCM #M_SET_VOLTBL(${0:X04})", n);
            //                int[] vtbl = new int[128];
            //                for (int i = 0; i < 128; i++)
            //                {
            //                    vtbl[i] = (int)(nise68.mem.PeekW((uint)(nise68.reg.GetAl(1) + (i * 2))));
            //                }
            //                mpcm.SetVolTableZms(0, (int)nise68.reg.GetDl(1), vtbl);
            //                break;
            //        }
            //        break;
            //    default:
            //        throw new NotImplementedException();
            //}

            return 0;
        }

        int OPMCallBack(int adr, int dat)
        {
            chipRegister.setYM2151Register(0, 0, adr, dat, model, YM2151Hosei[0], 0);
            return 0;
        }

        int MIDICallBack(int n, byte dat)
        {
            //if (midiOutsBuff == null || midiOutsFrame == null) return 0;
            //if (n >= midiOutsBuff.Count) return 0;

            //midiOutsBuff[n].Add(dat);
            //midiOutsFrame[n].Add(virtualFrameCounter);
            return 0;
        }

        int SCCCallBack(int n, byte dat)
        {
            //if (midiOutsBuff == null || midiOutsFrame == null) return 0;
            //if (2 + n >= midiOutsBuff.Count) return 0;

            //midiOutsBuff[2 + n].Add(dat);
            //midiOutsFrame[2 + n].Add(virtualFrameCounter);
            return 0;
        }

    }
}
