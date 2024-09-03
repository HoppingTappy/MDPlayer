﻿#if X64
using MDPlayerx64;
#else
using MDPlayer.Properties;
#endif

namespace MDPlayer.form
{
    public partial class frmYM2203 : frmBase
    {
        public bool isClosed = false;
        public int x = -1;
        public int y = -1;
        private int frameSizeW = 0;
        private int frameSizeH = 0;
        private int chipID = 0;
        private int zoom = 1;

        private MDChipParams.YM2203 newParam = null;
        private MDChipParams.YM2203 oldParam = null;
        private FrameBuffer frameBuffer = new FrameBuffer();

        public frmYM2203(frmMain frm, int chipID, int zoom, MDChipParams.YM2203 newParam, MDChipParams.YM2203 oldParam) : base(frm)
        {
            this.chipID = chipID;
            this.zoom = zoom;
            InitializeComponent();

            this.newParam = newParam;
            this.oldParam = oldParam;
            frameBuffer.Add(pbScreen, ResMng.ImgDic["planeYM2203"], null, zoom);
            bool YM2203Type = (chipID == 0)
                ? parent.setting.YM2203Type[0].UseReal[0]
                : parent.setting.YM2203Type[1].UseReal[0];
            int YM2203SoundLocation = (chipID == 0)
                ? parent.setting.YM2203Type[0].realChipInfo[0].SoundLocation
                : parent.setting.YM2203Type[1].realChipInfo[0].SoundLocation;
            int tp = !YM2203Type ? 0 : (YM2203SoundLocation < 0 ? 2 : 1);
            DrawBuff.screenInitYM2203(frameBuffer, tp);
            update();
        }

        public void update()
        {
            frameBuffer.Refresh(null);
        }

        protected override bool ShowWithoutActivation
        {
            get
            {
                return true;
            }
        }

        private void frmYM2203_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                parent.setting.location.PosYm2203[chipID] = Location;
            }
            else
            {
                parent.setting.location.PosYm2203[chipID] = RestoreBounds.Location;
            }
            isClosed = true;
        }

        private void frmYM2203_Load(object sender, EventArgs e)
        {
            this.Location = new Point(x, y);

            frameSizeW = this.Width - this.ClientSize.Width;
            frameSizeH = this.Height - this.ClientSize.Height;

            changeZoom();
        }

        public void changeZoom()
        {
            this.MaximumSize = new System.Drawing.Size(frameSizeW + ResMng.ImgDic["planeYM2203"].Width * zoom, frameSizeH + ResMng.ImgDic["planeYM2203"].Height * zoom);
            this.MinimumSize = new System.Drawing.Size(frameSizeW + ResMng.ImgDic["planeYM2203"].Width * zoom, frameSizeH + ResMng.ImgDic["planeYM2203"].Height * zoom);
            this.Size = new System.Drawing.Size(frameSizeW + ResMng.ImgDic["planeYM2203"].Width * zoom, frameSizeH + ResMng.ImgDic["planeYM2203"].Height * zoom);
            frmYM2203_Resize(null, null);

        }

        private void frmYM2203_Resize(object sender, EventArgs e)
        {

        }

        private static byte[] md = new byte[]
        {
            0x08<<4,
            0x08<<4,
            0x08<<4,
            0x08<<4,
            0x0c<<4,
            0x0e<<4,
            0x0e<<4,
            0x0f<<4
        };

        private static float[] fmDivTbl = new float[] { 6, 3, 2 };
        private static float[] ssgDivTbl = new float[] { 4, 2, 1 };

        public void screenChangeParams()
        {
            bool isFmEx;
            int[] ym2203Register = Audio.GetYM2203Register(chipID);
            int[] fmKeyYM2203 = Audio.GetYM2203KeyOn(chipID);
            int[] ym2203Vol = Audio.GetYM2203Volume(chipID);
            int[] ym2203Ch3SlotVol = Audio.GetYM2203Ch3SlotVolume(chipID);

            isFmEx = (ym2203Register[0x27] & 0x40) > 0;
            newParam.channels[2].ex = isFmEx;

            int defaultMasterClock = 7987200 / 2;
            double ssgMul = 1.0;
            int masterClock = defaultMasterClock;
            if (Audio.ClockYM2203 != 0)
            {
                ssgMul = Audio.ClockYM2203 / (float)defaultMasterClock;
                masterClock = Audio.ClockYM2203;
            }

            int divInd = ym2203Register[0x2d];
            if (divInd < 0 || divInd > 2) divInd = 0;
            float fmDiv = fmDivTbl[divInd];
            float ssgDiv = ssgDivTbl[divInd];
            ssgMul = ssgMul / ssgDiv * 4;

            for (int ch = 0; ch < 3; ch++)
            {
                int c = ch;
                for (int i = 0; i < 4; i++)
                {
                    int ops = (i == 0) ? 0 : ((i == 1) ? 8 : ((i == 2) ? 4 : 12));
                    newParam.channels[ch].inst[i * 11 + 0] = ym2203Register[0x50 + ops + c] & 0x1f; //AR
                    newParam.channels[ch].inst[i * 11 + 1] = ym2203Register[0x60 + ops + c] & 0x1f; //DR
                    newParam.channels[ch].inst[i * 11 + 2] = ym2203Register[0x70 + ops + c] & 0x1f; //SR
                    newParam.channels[ch].inst[i * 11 + 3] = ym2203Register[0x80 + ops + c] & 0x0f; //RR
                    newParam.channels[ch].inst[i * 11 + 4] = (ym2203Register[0x80 + ops + c] & 0xf0) >> 4;//SL
                    newParam.channels[ch].inst[i * 11 + 5] = ym2203Register[0x40 + ops + c] & 0x7f;//TL
                    newParam.channels[ch].inst[i * 11 + 6] = (ym2203Register[0x50 + ops + c] & 0xc0) >> 6;//KS
                    newParam.channels[ch].inst[i * 11 + 7] = ym2203Register[0x30 + ops + c] & 0x0f;//ML
                    newParam.channels[ch].inst[i * 11 + 8] = (ym2203Register[0x30 + ops + c] & 0x70) >> 4;//DT
                    newParam.channels[ch].inst[i * 11 + 9] = (ym2203Register[0x60 + ops + c] & 0x80) >> 7;//AM
                    newParam.channels[ch].inst[i * 11 + 10] = ym2203Register[0x90 + ops + c] & 0x0f;//SG
                }
                newParam.channels[ch].inst[44] = ym2203Register[0xb0 + c] & 0x07;//AL
                newParam.channels[ch].inst[45] = (ym2203Register[0xb0 + c] & 0x38) >> 3;//FB
                newParam.channels[ch].inst[46] = (ym2203Register[0xb4 + c] & 0x38) >> 4;//AMS
                newParam.channels[ch].inst[47] = ym2203Register[0xb4 + c] & 0x07;//FMS

                newParam.channels[ch].pan = 3;
                newParam.channels[ch].slot = (byte)(fmKeyYM2203[ch] >> 4);

                int freq = 0;
                int octav = 0;
                int n = -1;
                if (ch != 2 || !isFmEx)
                {
                    octav = (ym2203Register[0xa4 + c] & 0x38) >> 3;
                    freq = ym2203Register[0xa0 + c] + (ym2203Register[0xa4 + c] & 0x07) * 0x100;
                    newParam.channels[ch].freq = (freq & 0x7ff) | ((octav & 7) << 11);
                    float ff = freq / ((2 << 20) / (masterClock / (12 * fmDiv))) * (2 << (octav + 2));
                    ff /= 1038f;

                    newParam.channels[ch].slot = (byte)0;
                    if ((fmKeyYM2203[ch] & 1) != 0)
                    {
                        n = Math.Min(Math.Max(Common.searchYM2608Adpcm(ff) - 1, 0), 95);
                        newParam.channels[ch].slot = (byte)(fmKeyYM2203[ch] >> 4);
                    }

                    byte con = (byte)(fmKeyYM2203[ch]);
                    int v = 127;
                    int m = md[ym2203Register[0xb0 + c] & 7];
                    //OP1
                    v = (((con & 0x10) != 0) && ((m & 0x10) != 0) && v > (ym2203Register[0x40 + c] & 0x7f)) ? (ym2203Register[0x40 + c] & 0x7f) : v;
                    //OP3
                    v = (((con & 0x20) != 0) && ((m & 0x20) != 0) && v > (ym2203Register[0x44 + c] & 0x7f)) ? (ym2203Register[0x44 + c] & 0x7f) : v;
                    //OP2
                    v = (((con & 0x40) != 0) && ((m & 0x40) != 0) && v > (ym2203Register[0x48 + c] & 0x7f)) ? (ym2203Register[0x48 + c] & 0x7f) : v;
                    //OP4
                    v = (((con & 0x80) != 0) && ((m & 0x80) != 0) && v > (ym2203Register[0x4c + c] & 0x7f)) ? (ym2203Register[0x4c + c] & 0x7f) : v;
                    newParam.channels[ch].volumeL = Math.Min(Math.Max((int)((127 - v) / 127.0 * ym2203Vol[ch] / 80.0), 0), 19);
                }
                else
                {
                    int m = md[ym2203Register[0xb0 + 2] & 7];
                    if (parent.setting.other.ExAll) m = 0xf0;
                    freq = ym2203Register[0xa9] + (ym2203Register[0xad] & 0x07) * 0x100;
                    octav = (ym2203Register[0xad] & 0x38) >> 3;
                    newParam.channels[2].freq = (freq & 0x7ff) | ((octav & 7) << 11);
                    float ff = freq / ((2 << 20) / (masterClock / (12 * fmDiv))) * (2 << (octav + 2));
                    ff /= 1038f;

                    if ((fmKeyYM2203[2] & 0x10) != 0 && ((m & 0x10) != 0))
                        n = Math.Min(Math.Max(Common.searchYM2608Adpcm(ff) - 1, 0), 95);

                    int v = ((m & 0x10) != 0) ? ym2203Register[0x40 + c] : 127;
                    newParam.channels[2].volumeL = Math.Min(Math.Max((int)((127 - v) / 127.0 * ym2203Ch3SlotVol[0] / 80.0), 0), 19);
                }
                newParam.channels[ch].note = n;


            }

            for (int ch = 3; ch < 6; ch++) //FM EX
            {
                int[] exReg = new int[3] { 2, 0, -6 };
                int c = exReg[ch - 3];

                newParam.channels[ch].pan = 0;

                if (isFmEx)
                {
                    int m = md[ym2203Register[0xb0 + 2] & 7];
                    if (parent.setting.other.ExAll) m = 0xf0;
                    int op = ch - 2;
                    op = op == 1 ? 2 : (op == 2 ? 1 : op);

                    int freq = ym2203Register[0xa8 + c] + (ym2203Register[0xac + c] & 0x07) * 0x100;
                    int octav = (ym2203Register[0xac + c] & 0x38) >> 3;
                    newParam.channels[ch].freq = (freq & 0x7ff) | ((octav & 7) << 11);
                    int n = -1;
                    if ((fmKeyYM2203[2] & (0x20 << (ch - 3))) != 0 && ((m & (0x10 << op)) != 0))
                    {
                        float ff = freq / ((2 << 20) / (masterClock / (12 * fmDiv))) * (2 << (octav + 2));
                        ff /= 1038f;
                        n = Math.Min(Math.Max(Common.searchYM2608Adpcm(ff) - 1, 0), 95);
                    }
                    newParam.channels[ch].note = n;

                    int v = ((m & (0x10 << op)) != 0) ? ym2203Register[0x42 + op * 4] : 127;
                    newParam.channels[ch].volumeL = Math.Min(Math.Max((int)((127 - v) / 127.0 * ym2203Ch3SlotVol[ch - 2] / 80.0), 0), 19);
                }
                else
                {
                    newParam.channels[ch].note = -1;
                    newParam.channels[ch].volumeL = 0;
                }
            }

            for (int ch = 0; ch < 3; ch++) //SSG
            {
                MDChipParams.Channel channel = newParam.channels[ch + 6];

                bool t = (ym2203Register[0x07] & (0x1 << ch)) == 0;
                bool n = (ym2203Register[0x07] & (0x8 << ch)) == 0;
                channel.tn = (t ? 1 : 0) + (n ? 2 : 0);
                channel.volumeL = ym2203Register[0x08 + ch] & 0xf;
                channel.volume = (int)(((t || n) ? 1 : 0) * (ym2203Register[0x08 + ch] & 0xf));

                int ft = ym2203Register[0x00 + ch * 2];
                int ct = ym2203Register[0x01 + ch * 2];
                int tp = (ct << 8) | ft;
                channel.freq = tp;

                if (!t && !n && channel.volume > 0)
                {
                    channel.volume--;
                }

                if (channel.volume == 0)
                {
                    channel.note = -1;
                }
                else
                {
                    if (tp == 0) tp = 1;
                    float ftone = (float)(7987200.0 / (64.0 * tp) * ssgMul);// 7987200 = MasterClock(↓のメソッドが7987200を基準としたテーブルの為)
                    channel.note = Common.searchSSGNote(ftone);
                }
                channel.ex = (ym2203Register[0x08 + ch] & 0xf0) != 0;

            }

            newParam.nfrq = ym2203Register[0x06] & 0x1f;
            newParam.efrq = ym2203Register[0x0c] * 0x100 + ym2203Register[0x0b];
            newParam.etype = (ym2203Register[0x0d] & 0xf);

        }


        public void screenDrawParams()
        {
            bool YM2203Type = (chipID == 0)
                ? parent.setting.YM2203Type[0].UseReal[0]
                : parent.setting.YM2203Type[1].UseReal[0];
            int YM2203SoundLocation = (chipID == 0)
                ? parent.setting.YM2203Type[0].realChipInfo[0].SoundLocation
                : parent.setting.YM2203Type[1].realChipInfo[0].SoundLocation;
            int tp = !YM2203Type ? 0 : (YM2203SoundLocation < 0 ? 2 : 1);

            for (int c = 0; c < 6; c++)
            {

                MDChipParams.Channel oyc = oldParam.channels[c];
                MDChipParams.Channel nyc = newParam.channels[c];

                if (c == 2)
                {
                    DrawBuff.Volume(frameBuffer, 272, 8 + c * 8, 0, ref oyc.volumeL, nyc.volumeL, tp);
                    DrawBuff.KeyBoardOPNA(frameBuffer, 32, 8 + c * 8, ref oyc.note, nyc.note, tp);
                    DrawBuff.Inst(frameBuffer, 1, 12, c, oyc.inst, nyc.inst);
                    DrawBuff.Ch3YM2203(frameBuffer, c, ref oyc.mask, nyc.mask, ref oyc.ex, nyc.ex, tp);
                    DrawBuff.Slot(frameBuffer, 0 + 4 * 64, 8 + c * 8, ref oyc.slot, nyc.slot);
                    DrawBuff.font4Hex16Bit(frameBuffer, 0 + 4 * 78, 8 + c * 8, 0, ref oyc.freq, nyc.freq);
                }
                else if (c < 3)
                {
                    DrawBuff.Volume(frameBuffer, 272, 8 + c * 8, 0, ref oyc.volumeL, nyc.volumeL, tp);
                    DrawBuff.KeyBoardOPNA(frameBuffer, 32, 8 + c * 8, ref oyc.note, nyc.note, tp);
                    DrawBuff.Inst(frameBuffer, 1, 12, c, oyc.inst, nyc.inst);
                    DrawBuff.ChYM2203(frameBuffer, c, ref oyc.mask, nyc.mask, tp);
                    DrawBuff.Slot(frameBuffer, 0 + 4 * 64, 8 + c * 8, ref oyc.slot, nyc.slot);
                    DrawBuff.font4Hex16Bit(frameBuffer, 0 + 4 * 78, 8 + c * 8, 0, ref oyc.freq, nyc.freq);
                }
                else
                {
                    DrawBuff.Volume(frameBuffer, 272, 8 + (c + 3) * 8, 0, ref oyc.volumeL, nyc.volumeL, tp);
                    DrawBuff.KeyBoardOPNA(frameBuffer, 32, 8 + (c+3) * 8, ref oyc.note, nyc.note, tp);
                    DrawBuff.ChYM2203(frameBuffer, c, ref oyc.mask, nyc.mask, tp);
                    DrawBuff.Slot(frameBuffer, 0 + 4 * 64, 8 + (c + 3) * 8, ref oyc.slot, nyc.slot);
                    DrawBuff.font4Hex16Bit(frameBuffer, 0 + 4 * 78, 8 + (c + 3) * 8, 0, ref oyc.freq, nyc.freq);
                }


            }

            for (int c = 0; c < 3; c++)
            {
                MDChipParams.Channel oyc = oldParam.channels[c + 6];
                MDChipParams.Channel nyc = newParam.channels[c + 6];

                DrawBuff.VolumeShort(frameBuffer, 280 + 0, 8 + (c + 3) * 8, 0, ref oyc.volume, nyc.volume, tp);
                DrawBuff.KeyBoardOPNA(frameBuffer, 32, 8 + (c + 3) * 8, ref oyc.note, nyc.note, tp);
                DrawBuff.Tn(frameBuffer, 6, 2, c + 3, ref oyc.tn, nyc.tn, ref oyc.tntp, tp * 2);

                DrawBuff.ChYM2203(frameBuffer, c + 6, ref oyc.mask, nyc.mask, tp);
                DrawBuff.drawNESSw(frameBuffer, 268 + 0, 8 + c * 8, ref oyc.ex, nyc.ex);
                DrawBuff.font4Hex16Bit(frameBuffer, 0 + 4 * 78, 8 + (c + 3) * 8, 0, ref oyc.freq, nyc.freq);
                DrawBuff.font4HexByte(frameBuffer, 272 + 0, 8 + (c + 3) * 8, 0, ref oyc.volumeL, nyc.volumeL);

            }

            DrawBuff.Nfrq(frameBuffer, 5, 32, ref oldParam.nfrq, newParam.nfrq);
            DrawBuff.Efrq(frameBuffer, 18, 32, ref oldParam.efrq, newParam.efrq);
            DrawBuff.Etype(frameBuffer, 33, 32, ref oldParam.etype, newParam.etype);

        }

        public void screenInit()
        {
            bool YM2203Type = (chipID == 0)
                ? parent.setting.YM2203Type[0].UseReal[0]
                : parent.setting.YM2203Type[1].UseReal[0];
            int YM2203SoundLocation = (chipID == 0)
                ? parent.setting.YM2203Type[0].realChipInfo[0].SoundLocation
                : parent.setting.YM2203Type[1].realChipInfo[0].SoundLocation;
            int tp = !YM2203Type ? 0 : (YM2203SoundLocation < 0 ? 2 : 1);

            for (int ch = 0; ch < 3; ch++)
            {
                for (int i = 0; i < 4; i++)
                {
                    newParam.channels[ch].inst[i * 11 + 0] = 0;
                    newParam.channels[ch].inst[i * 11 + 1] = 0;
                    newParam.channels[ch].inst[i * 11 + 2] = 0;
                    newParam.channels[ch].inst[i * 11 + 3] = 0;
                    newParam.channels[ch].inst[i * 11 + 4] = 0;
                    newParam.channels[ch].inst[i * 11 + 5] = 0;
                    newParam.channels[ch].inst[i * 11 + 6] = 0;
                    newParam.channels[ch].inst[i * 11 + 7] = 0;
                    newParam.channels[ch].inst[i * 11 + 8] = 0;
                    newParam.channels[ch].inst[i * 11 + 9] = 0;
                    newParam.channels[ch].inst[i * 11 + 10] = 0;
                }
                newParam.channels[ch].inst[44] = 0;
                newParam.channels[ch].inst[45] = 0;
                newParam.channels[ch].inst[46] = 0;
                newParam.channels[ch].inst[47] = 0;
                newParam.channels[ch].pan = 3;
                newParam.channels[ch].volumeL = 0;
                newParam.channels[ch].note = -1;
            }

            for (int ch = 3; ch < 6; ch++) //FM EX
            {
                newParam.channels[ch].pan = 0;
                newParam.channels[ch].note = -1;
                newParam.channels[ch].volumeL = 0;
                newParam.channels[ch].note = -1;
            }

            for (int ch = 0; ch < 3; ch++) //SSG
            {
                MDChipParams.Channel channel = newParam.channels[ch + 6];
                channel.tn = 0;
                channel.volume = 0;
                channel.note = -1;
            }

            newParam.nfrq = 0;
            newParam.efrq = 0;
            newParam.etype = 0;

            DrawBuff.screenInitYM2203(frameBuffer, tp);
        }



        private void pbScreen_MouseClick(object sender, MouseEventArgs e)
        {
            int px = e.Location.X / zoom;
            int py = e.Location.Y / zoom;

            //上部のラベル行の場合は何もしない
            if (py < 1 * 8)
            {
                //但しchをクリックした場合はマスク反転
                if (px < 8)
                {
                    for (int ch = 0; ch < 9; ch++)
                    {
                        if (newParam.channels[ch].mask == true)
                            parent.ResetChannelMask(EnmChip.YM2203, chipID, ch);
                        else
                            parent.SetChannelMask(EnmChip.YM2203, chipID, ch);
                    }
                }
                return;
            }

            //鍵盤
            if (py < 10 * 8)
            {
                int ch = (py / 8) - 1;
                if (ch < 0) return;

                if (e.Button == MouseButtons.Left)
                {
                    //FM Ch3専用マスク判定処理
                    if (ch == 2 || ch == 6 || ch == 7 || ch == 8)
                    {
                        if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                        {
                            //マスク
                            if (newParam.channels[ch].mask == true)
                                parent.ResetChannelMask(EnmChip.YM2203, chipID, ch);
                            else
                                parent.SetChannelMask(EnmChip.YM2203, chipID, ch);
                            return;
                        }

                        //マスク
                        if (newParam.channels[ch].mask == true)
                        {
                            parent.ResetChannelMask(EnmChip.YM2203, chipID, 2);
                            parent.ResetChannelMask(EnmChip.YM2203, chipID, 6);
                            parent.ResetChannelMask(EnmChip.YM2203, chipID, 7);
                            parent.ResetChannelMask(EnmChip.YM2203, chipID, 8);
                        }
                        else
                        {
                            parent.SetChannelMask(EnmChip.YM2203, chipID, 2);
                            parent.SetChannelMask(EnmChip.YM2203, chipID, 6);
                            parent.SetChannelMask(EnmChip.YM2203, chipID, 7);
                            parent.SetChannelMask(EnmChip.YM2203, chipID, 8);
                        }
                        return;
                    }

                    //マスク
                    if (newParam.channels[ch].mask == true)
                        parent.ResetChannelMask(EnmChip.YM2203, chipID, ch);
                    else
                        parent.SetChannelMask(EnmChip.YM2203, chipID, ch);
                    return;
                }

                //マスク解除
                for (ch = 0; ch < 9; ch++) parent.ResetChannelMask(EnmChip.YM2203, chipID, ch);
                return;
            }

            //音色で右クリックした場合は何もしない
            if (e.Button == MouseButtons.Right) return;

            // 音色表示欄の判定
            int instCh = Math.Min(px / (13 * 8), 2);

            if (instCh < 3)
            {
                //クリップボードに音色をコピーする
                parent.GetInstCh(EnmChip.YM2203, instCh, chipID, newParam.channels[instCh].note);
            }
        }
    }
}
