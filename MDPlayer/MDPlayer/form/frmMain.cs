﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NAudio.Midi;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using MDSound;
using MDPlayer.Properties;
using MDPlayer.Driver.ZGM.ZgmChip;

namespace MDPlayer.form
{
    public partial class frmMain : Form
    {

        private PictureBox pbRf5c164Screen;
        private DoubleBuffer screen;
        private int pWidth = 0;
        private int pHeight = 0;

        private frmInfo frmInfo = null;
        private frmPlayList frmPlayList = null;
        private frmVSTeffectList frmVSTeffectList = null;

        private frmMegaCD[] frmMCD = new frmMegaCD[2] { null, null };
        private frmC140[] frmC140 = new frmC140[2] { null, null };
        private frmYMZ280B[] frmYMZ280B = new frmYMZ280B[2] { null, null };
        private frmC352[] frmC352 = new frmC352[2] { null, null };
        private frmMultiPCM[] frmMultiPCM = new frmMultiPCM[2] { null, null };
        private frmQSound[] frmQSound = new frmQSound[2] { null, null };
        private frmYM2608[] frmYM2608 = new frmYM2608[2] { null, null };
        private frmYM2151[] frmYM2151 = new frmYM2151[2] { null, null };
        private frmYM2203[] frmYM2203 = new frmYM2203[2] { null, null };
        private frmYM2610[] frmYM2610 = new frmYM2610[2] { null, null };
        private frmYM2612[] frmYM2612 = new frmYM2612[2] { null, null };
        private frmYM3526[] frmYM3526 = new frmYM3526[2] { null, null };
        private frmY8950[] frmY8950 = new frmY8950[2] { null, null };
        private frmYM3812[] frmYM3812 = new frmYM3812[2] { null, null };
        private frmOKIM6258[] frmOKIM6258 = new frmOKIM6258[2] { null, null };
        private frmOKIM6295[] frmOKIM6295 = new frmOKIM6295[2] { null, null };
        private frmSN76489[] frmSN76489 = new frmSN76489[2] { null, null };
        private frmSegaPCM[] frmSegaPCM = new frmSegaPCM[2] { null, null };
        private frmAY8910[] frmAY8910 = new frmAY8910[2] { null, null };
        private frmHuC6280[] frmHuC6280 = new frmHuC6280[2] { null, null };
        private frmK051649[] frmK051649 = new frmK051649[2] { null, null };
        private frmYM2413[] frmYM2413 = new frmYM2413[2] { null, null };
        private frmYMF262[] frmYMF262 = new frmYMF262[2] { null, null };
        private frmYMF278B[] frmYMF278B = new frmYMF278B[2] { null, null };
        private frmMIDI[] frmMIDI = new frmMIDI[2] { null, null };
        private frmYM2612MIDI frmYM2612MIDI = null;
        private frmMixer2 frmMixer2 = null;
        private frmNESDMC[] frmNESDMC = new frmNESDMC[2] { null, null };
        private frmFDS[] frmFDS = new frmFDS[2] { null, null };
        private frmMMC5[] frmMMC5 = new frmMMC5[2] { null, null };
        private frmVRC7[] frmVRC7 = new frmVRC7[2] { null, null };
        private frmRegTest frmRegTest;

        private List<Form[]> lstForm = new List<Form[]>();

        public MDChipParams oldParam = new MDChipParams();
        private MDChipParams newParam = new MDChipParams();

        private int[] oldButton = new int[18];
        private int[] newButton = new int[18];
        private int[] oldButtonMode = new int[18];
        private int[] newButtonMode = new int[18];

        private bool isRunning = false;
        private bool stopped = false;

        private bool IsInitialOpenFolder = true;

        private byte[] srcBuf;

        public Setting setting = Setting.Load();
        public TonePallet tonePallet = TonePallet.Load(null);

        private int frameSizeW = 0;
        private int frameSizeH = 0;


        private MidiIn midiin = null;
        private bool forcedExit = false;
        private YM2612MIDI YM2612MIDI = null;
        private bool flgReinit = false;
        public bool reqAllScreenInit = true;


        public frmMain()
        {
            log.ForcedWrite("起動処理開始");
            log.ForcedWrite("frmMain(コンストラクタ):STEP 00");

            InitializeComponent();
            DrawBuff.Init();

            lstForm.Add(frmMCD);
            lstForm.Add(frmC140);
            lstForm.Add(frmC352);
            lstForm.Add(frmY8950);
            lstForm.Add(frmYM2608);
            lstForm.Add(frmYM2151);
            lstForm.Add(frmYM2203);
            lstForm.Add(frmYM2413);
            lstForm.Add(frmYM2610);
            lstForm.Add(frmYM2612);
            lstForm.Add(frmYM3526);
            lstForm.Add(frmYM3812);
            lstForm.Add(frmYMF262);
            lstForm.Add(frmYMF278B);
            lstForm.Add(frmOKIM6258);
            lstForm.Add(frmOKIM6295);
            lstForm.Add(frmSN76489);
            lstForm.Add(frmSegaPCM);
            lstForm.Add(frmAY8910);
            lstForm.Add(frmHuC6280);
            lstForm.Add(frmK051649);
            lstForm.Add(frmMIDI);
            lstForm.Add(frmNESDMC);
            lstForm.Add(frmFDS);
            lstForm.Add(frmMMC5);
            lstForm.Add(frmVRC7);

            log.ForcedWrite("frmMain(コンストラクタ):STEP 01");

            //引数が指定されている場合のみプロセスチェックを行い、自分と同じアプリケーションが実行中ならばそちらに引数を渡し終了する
            if (Environment.GetCommandLineArgs().Length > 1)
            {
                Process prc = GetPreviousProcess();
                if (prc != null)
                {
                    SendString(prc.MainWindowHandle, Environment.GetCommandLineArgs()[1]);
                    forcedExit = true;
                    try
                    {
                        this.Close();
                    }
                    catch { }
                    return;
                }
            }

            log.ForcedWrite("frmMain(コンストラクタ):STEP 02");

            pbScreen.AllowDrop = true;

            log.ForcedWrite("frmMain(コンストラクタ):STEP 03");
            if (setting == null)
            {
                log.ForcedWrite("frmMain(コンストラクタ):setting is null");
            }

            log.ForcedWrite("起動時のAudio初期化処理開始");

            Audio.frmMain = this;
            Audio.Init(setting);

            YM2612MIDI = new YM2612MIDI(this, Audio.mdsMIDI, newParam);

            log.ForcedWrite("起動時のAudio初期化処理完了");

            StartMIDIInMonitoring();

            log.ForcedWrite("frmMain(コンストラクタ):STEP 04");

            log.debug = setting.Debug_DispFrameCounter;

        }


        private void frmMain_Load(object sender, EventArgs e)
        {
            Microsoft.Win32.SystemEvents.SessionEnding += SystemEvents_SessionEnding;

            log.ForcedWrite("frmMain_Load:STEP 05");

            if (setting.location.PMain != System.Drawing.Point.Empty)
                this.Location = setting.location.PMain;

            // DoubleBufferオブジェクトの作成

            pbRf5c164Screen = new PictureBox();
            pbRf5c164Screen.Width = 320;
            pbRf5c164Screen.Height = 72;

            log.ForcedWrite("frmMain_Load:STEP 06");

            screen = new DoubleBuffer(pbScreen, Properties.Resources.planeControl, 1);
            screen.setting = setting;
            //oldParam = new MDChipParams();
            //newParam = new MDChipParams();
            reqAllScreenInit = true;

            log.ForcedWrite("frmMain_Load:STEP 07");

            pWidth = pbScreen.Width;
            pHeight = pbScreen.Height;

            frmPlayList = new frmPlayList(this);
            frmPlayList.Show();
            frmPlayList.Visible = false;
            frmPlayList.Opacity = 1.0;
            //frmPlayList.Location = new System.Drawing.Point(this.Location.X + 328, this.Location.Y + 264);
            frmPlayList.Refresh();

            frmVSTeffectList = new frmVSTeffectList(this, setting);
            frmVSTeffectList.Show();
            frmVSTeffectList.Visible = false;
            frmVSTeffectList.Opacity = 1.0;
            //frmVSTeffectList.Location = new System.Drawing.Point(this.Location.X + 328, this.Location.Y + 264);
            frmVSTeffectList.Refresh();

            if (setting.location.OPlayList) dispPlayList();
            if (setting.location.OInfo) openInfo();
            if (setting.location.OMixer) openMixer();
            if (setting.location.OpenYm2612MIDI) openMIDIKeyboard();

            for (int chipID = 0; chipID < 2; chipID++)
            {
                if (setting.location.OpenAY8910[chipID]) OpenFormAY8910(chipID);
                if (setting.location.OpenC140[chipID]) OpenFormC140(chipID);
                if (setting.location.OpenYMZ280B[chipID]) OpenFormYMZ280B(chipID);
                if (setting.location.OpenC352[chipID]) OpenFormC352(chipID);
                if (setting.location.OpenMultiPCM[chipID]) OpenFormMultiPCM(chipID);
                if (setting.location.OpenQSound[chipID]) OpenFormQSound(chipID);
                if (setting.location.OpenHuC6280[chipID]) OpenFormHuC6280(chipID);
                if (setting.location.OpenK051649[chipID]) OpenFormK051649(chipID);
                if (setting.location.OpenMIDI[chipID]) OpenFormMIDI(chipID);
                if (setting.location.OpenNESDMC[chipID]) OpenFormNESDMC(chipID);
                if (setting.location.OpenFDS[chipID]) OpenFormFDS(chipID);
                if (setting.location.OpenMMC5[chipID]) OpenFormMMC5(chipID);
                if (setting.location.OpenOKIM6258[chipID]) OpenFormOKIM6258(chipID);
                if (setting.location.OpenOKIM6295[chipID]) OpenFormOKIM6295(chipID);
                if (setting.location.OpenRf5c164[chipID]) OpenFormMegaCD(chipID);
                if (setting.location.OpenSN76489[chipID]) OpenFormSN76489(chipID);
                if (setting.location.OpenSegaPCM[chipID]) OpenFormSegaPCM(chipID);
                if (setting.location.OpenYm2151[chipID]) OpenFormYM2151(chipID);
                if (setting.location.OpenYm2203[chipID]) OpenFormYM2203(chipID);
                if (setting.location.OpenYm2413[chipID]) OpenFormYM2413(chipID);
                if (setting.location.OpenYm2608[chipID]) OpenFormYM2608(chipID);
                if (setting.location.OpenYm2610[chipID]) OpenFormYM2610(chipID);
                if (setting.location.OpenYm2612[chipID]) OpenFormYM2612(chipID);
                if (setting.location.OpenYm3526[chipID]) OpenFormYM3526(chipID);
                if (setting.location.OpenY8950[chipID]) OpenFormY8950(chipID);
                if (setting.location.OpenYm3812[chipID]) OpenFormYM3812(chipID);
                if (setting.location.OpenYmf262[chipID]) OpenFormYMF262(chipID);
                if (setting.location.OpenYmf278b[chipID]) OpenFormYMF278B(chipID);
                if (setting.location.OpenVrc7[chipID]) OpenFormVRC7(chipID);
                if (setting.location.OpenRegTest[chipID]) OpenFormRegTest(chipID);
            }

            log.ForcedWrite("frmMain_Load:STEP 08");

            frameSizeW = this.Width - this.ClientSize.Width;
            frameSizeH = this.Height - this.ClientSize.Height;

            changeZoom();

        }

        private void SystemEvents_SessionEnding(object sender, Microsoft.Win32.SessionEndingEventArgs e)
        {
            this.Close();
        }

        private void changeZoom()
        {
            this.MaximumSize = new System.Drawing.Size(frameSizeW + Properties.Resources.planeControl.Width * setting.other.Zoom, frameSizeH + Properties.Resources.planeControl.Height * setting.other.Zoom);
            this.MinimumSize = new System.Drawing.Size(frameSizeW + Properties.Resources.planeControl.Width * setting.other.Zoom, frameSizeH + Properties.Resources.planeControl.Height * setting.other.Zoom);
            this.Size = new System.Drawing.Size(frameSizeW + Properties.Resources.planeControl.Width * setting.other.Zoom, frameSizeH + Properties.Resources.planeControl.Height * setting.other.Zoom);
            frmMain_Resize(null, null);

            if (frmMCD[0] != null && !frmMCD[0].isClosed)
            {
                tsmiPRF5C164_Click(null, null);
                tsmiPRF5C164_Click(null, null);
            }

            if (frmC140[0] != null && !frmC140[0].isClosed)
            {
                tsmiPC140_Click(null, null);
                tsmiPC140_Click(null, null);
            }

            if (frmYMZ280B[0] != null && !frmYMZ280B[0].isClosed)
            {
                tsmiYMZ280B_Click(null, null);
                tsmiYMZ280B_Click(null, null);
            }

            if (frmC352[0] != null && !frmC352[0].isClosed)
            {
                tsmiPC352_Click(null, null);
                tsmiPC352_Click(null, null);
            }

            if (frmQSound[0] != null && !frmQSound[0].isClosed)
            {
                tsmiPQSound_Click(null, null);
                tsmiPQSound_Click(null, null);
            }

            if (frmYM2608[0] != null && !frmYM2608[0].isClosed)
            {
                tsmiPOPNA_Click(null, null);
                tsmiPOPNA_Click(null, null);
            }

            if (frmYM2151[0] != null && !frmYM2151[0].isClosed)
            {
                tsmiPOPM_Click(null, null);
                tsmiPOPM_Click(null, null);
            }

            if (frmYM2203[0] != null && !frmYM2203[0].isClosed)
            {
                tsmiPOPN_Click(null, null);
                tsmiPOPN_Click(null, null);
            }

            if (frmYM2413[0] != null && !frmYM2413[0].isClosed)
            {
                tsmiPOPLL_Click(null, null);
                tsmiPOPLL_Click(null, null);
            }

            if (frmYM2610[0] != null && !frmYM2610[0].isClosed)
            {
                tsmiPOPNB_Click(null, null);
                tsmiPOPNB_Click(null, null);
            }

            if (frmYM2612[0] != null && !frmYM2612[0].isClosed)
            {
                tsmiPOPN2_Click(null, null);
                tsmiPOPN2_Click(null, null);
            }

            if (frmYM3526[0] != null && !frmYM3526[0].isClosed)
            {
                tsmiPOPL_Click(null, null);
                tsmiPOPL_Click(null, null);
            }

            if (frmY8950[0] != null && !frmY8950[0].isClosed)
            {
                tsmiPY8950_Click(null, null);
                tsmiPY8950_Click(null, null);
            }

            if (frmYM3812[0] != null && !frmYM3812[0].isClosed)
            {
                tsmiPOPL2_Click(null, null);
                tsmiPOPL2_Click(null, null);
            }

            if (frmYMF262[0] != null && !frmYMF262[0].isClosed)
            {
                tsmiPOPL3_Click(null, null);
                tsmiPOPL3_Click(null, null);
            }

            if (frmYMF278B[0] != null && !frmYMF278B[0].isClosed)
            {
                tsmiPOPL4_Click(null, null);
                tsmiPOPL4_Click(null, null);
            }

            if (frmOKIM6258[0] != null && !frmOKIM6258[0].isClosed)
            {
                tsmiPOKIM6258_Click(null, null);
                tsmiPOKIM6258_Click(null, null);
            }

            if (frmOKIM6295[0] != null && !frmOKIM6295[0].isClosed)
            {
                tsmiPOKIM6295_Click(null, null);
                tsmiPOKIM6295_Click(null, null);
            }

            if (frmSN76489[0] != null && !frmSN76489[0].isClosed)
            {
                tsmiPDCSG_Click(null, null);
                tsmiPDCSG_Click(null, null);
            }

            if (frmSegaPCM[0] != null && !frmSegaPCM[0].isClosed)
            {
                tsmiPSegaPCM_Click(null, null);
                tsmiPSegaPCM_Click(null, null);
            }

            if (frmAY8910[0] != null && !frmAY8910[0].isClosed)
            {
                tsmiPAY8910_Click(null, null);
                tsmiPAY8910_Click(null, null);
            }

            if (frmHuC6280[0] != null && !frmHuC6280[0].isClosed)
            {
                tsmiPHuC6280_Click(null, null);
                tsmiPHuC6280_Click(null, null);
            }

            if (frmK051649[0] != null && !frmK051649[0].isClosed)
            {
                tsmiPK051649_Click(null, null);
                tsmiPK051649_Click(null, null);
            }



            if (frmMCD[1] != null && !frmMCD[1].isClosed)
            {
                tsmiSRF5C164_Click(null, null);
                tsmiSRF5C164_Click(null, null);
            }

            if (frmC140[1] != null && !frmC140[1].isClosed)
            {
                tsmiSC140_Click(null, null);
                tsmiSC140_Click(null, null);
            }

            if (frmYMZ280B[1] != null && !frmYMZ280B[1].isClosed)
            {
                tsmiSYMZ280B_Click(null, null);
                tsmiSYMZ280B_Click(null, null);
            }

            if (frmC352[1] != null && !frmC352[1].isClosed)
            {
                tsmiSC352_Click(null, null);
                tsmiSC352_Click(null, null);
            }

            if (frmYM2608[1] != null && !frmYM2608[1].isClosed)
            {
                tsmiSOPNA_Click(null, null);
                tsmiSOPNA_Click(null, null);
            }

            if (frmYM2151[1] != null && !frmYM2151[1].isClosed)
            {
                tsmiSOPM_Click(null, null);
                tsmiSOPM_Click(null, null);
            }

            if (frmYM2203[1] != null && !frmYM2203[1].isClosed)
            {
                tsmiSOPN_Click(null, null);
                tsmiSOPN_Click(null, null);
            }

            if (frmYM3526[1] != null && !frmYM3526[1].isClosed)
            {
                tsmiSOPL_Click(null, null);
                tsmiSOPL_Click(null, null);
            }

            if (frmY8950[1] != null && !frmY8950[1].isClosed)
            {
                tsmiSY8950_Click(null, null);
                tsmiSY8950_Click(null, null);
            }

            if (frmYM3812[1] != null && !frmYM3812[1].isClosed)
            {
                tsmiSOPL2_Click(null, null);
                tsmiSOPL2_Click(null, null);
            }

            if (frmYM2413[1] != null && !frmYM2413[1].isClosed)
            {
                tsmiSOPLL_Click(null, null);
                tsmiSOPLL_Click(null, null);
            }

            if (frmYM2610[1] != null && !frmYM2610[1].isClosed)
            {
                tsmiSOPNB_Click(null, null);
                tsmiSOPNB_Click(null, null);
            }

            if (frmYM2612[1] != null && !frmYM2612[1].isClosed)
            {
                tsmiSOPN2_Click(null, null);
                tsmiSOPN2_Click(null, null);
            }

            if (frmYMF262[1] != null && !frmYMF262[1].isClosed)
            {
                tsmiSOPL3_Click(null, null);
                tsmiSOPL3_Click(null, null);
            }

            if (frmYMF278B[1] != null && !frmYMF278B[1].isClosed)
            {
                tsmiSOPL4_Click(null, null);
                tsmiSOPL4_Click(null, null);
            }

            if (frmOKIM6258[1] != null && !frmOKIM6258[1].isClosed)
            {
                tsmiSOKIM6258_Click(null, null);
                tsmiSOKIM6258_Click(null, null);
            }

            if (frmOKIM6295[1] != null && !frmOKIM6295[1].isClosed)
            {
                tsmiSOKIM6295_Click(null, null);
                tsmiSOKIM6295_Click(null, null);
            }

            if (frmSN76489[1] != null && !frmSN76489[1].isClosed)
            {
                tsmiSDCSG_Click(null, null);
                tsmiSDCSG_Click(null, null);
            }

            if (frmSegaPCM[1] != null && !frmSegaPCM[1].isClosed)
            {
                tsmiSSegaPCM_Click(null, null);
                tsmiSSegaPCM_Click(null, null);
            }

            if (frmAY8910[1] != null && !frmAY8910[1].isClosed)
            {
                tsmiSAY8910_Click(null, null);
                tsmiSAY8910_Click(null, null);
            }

            if (frmHuC6280[1] != null && !frmHuC6280[1].isClosed)
            {
                tsmiSHuC6280_Click(null, null);
                tsmiSHuC6280_Click(null, null);
            }

            if (frmK051649[1] != null && !frmK051649[1].isClosed)
            {
                tsmiSK051649_Click(null, null);
                tsmiSK051649_Click(null, null);
            }

            if (frmYM2612MIDI != null && !frmYM2612MIDI.isClosed)
            {
                openMIDIKeyboard();
                openMIDIKeyboard();
            }

            if (frmMIDI[0] != null && !frmMIDI[0].isClosed)
            {
                OpenFormMIDI(0);
                OpenFormMIDI(0);
            }

            if (frmMIDI[1] != null && !frmMIDI[1].isClosed)
            {
                OpenFormMIDI(1);
                OpenFormMIDI(1);
            }

            if (frmVRC7[0] != null && !frmVRC7[0].isClosed)
            {
                OpenFormVRC7(0);
                OpenFormVRC7(0);
            }

            if (frmVRC7[1] != null && !frmVRC7[1].isClosed)
            {
                OpenFormVRC7(1);
                OpenFormVRC7(1);
            }

            if (frmNESDMC[0] != null && !frmNESDMC[0].isClosed)
            {
                OpenFormNESDMC(0);
                OpenFormNESDMC(0);
            }

            if (frmNESDMC[1] != null && !frmNESDMC[1].isClosed)
            {
                OpenFormNESDMC(1);
                OpenFormNESDMC(1);
            }

            if (frmMixer2 != null && !frmMixer2.isClosed)
            {
                openMixer();
                openMixer();
            }

        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            log.ForcedWrite("frmMain_Shown:STEP 09");

            System.Threading.Thread trd = new System.Threading.Thread(screenMainLoop);
            trd.Priority = System.Threading.ThreadPriority.BelowNormal;
            trd.Start();
            string[] args = Environment.GetCommandLineArgs();

            Application.DoEvents();
            Activate();

            if (args.Length < 2)
            {
                return;
            }

            log.ForcedWrite("frmMain_Shown:STEP 10");

            try
            {

                frmPlayList.Stop();

                PlayList pl = frmPlayList.getPlayList();
                if (pl.lstMusic.Count < 1 || pl.lstMusic[pl.lstMusic.Count - 1].fileName != args[1])
                {
                    pl.AddFile(args[1]);
                    //frmPlayList.AddList(args[1]);
                }

                if (!loadAndPlay(0, 0, args[1], ""))
                {
                    frmPlayList.Stop();
                    Audio.Stop();
                    return;
                }

                frmPlayList.setStart(-1);

                oldParam = new MDChipParams();
                frmPlayList.Play();

            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
                MessageBox.Show("ファイルの読み込みに失敗しました。");
            }

            log.ForcedWrite("frmMain_Shown:STEP 11");
            log.ForcedWrite("起動処理完了");

        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            // リサイズ時は再確保

            if (screen != null) screen.Dispose();

            screen = new DoubleBuffer(pbScreen, Properties.Resources.planeControl, setting.other.Zoom);
            screen.setting = setting;
            reqAllScreenInit = true;
            //screen.screenInitAll();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (forcedExit) return;

            log.ForcedWrite("終了処理開始");
            log.ForcedWrite("frmMain_FormClosing:STEP 00");

            frmPlayList.Stop();
            frmPlayList.Save();

            tonePallet.Save(null);

            log.ForcedWrite("frmMain_FormClosing:STEP 01");

            StopMIDIInMonitoring();
            Audio.Close();
            Audio.RealChipClose();

            log.ForcedWrite("frmMain_FormClosing:STEP 02");

            isRunning = false;
            while (!stopped)
            {
                System.Threading.Thread.Sleep(1);
                Application.DoEvents();
            }

            log.ForcedWrite("frmMain_FormClosing:STEP 03");

            YM2612MIDI.Close();

            // 解放
            screen.Dispose();

            setting.location.OInfo = false;
            setting.location.OPlayList = false;
            setting.location.OMixer = false;
            setting.location.OpenYm2612MIDI = false;
            for (int chipID = 0; chipID < 2; chipID++)
            {
                setting.location.OpenAY8910[chipID] = false;
                setting.location.OpenC140[chipID] = false;
                setting.location.OpenYMZ280B[chipID] = false;
                setting.location.OpenC352[chipID] = false;
                setting.location.OpenQSound[chipID] = false;
                setting.location.OpenHuC6280[chipID] = false;
                setting.location.OpenK051649[chipID] = false;
                setting.location.OpenMIDI[chipID] = false;
                setting.location.OpenNESDMC[chipID] = false;
                setting.location.OpenFDS[chipID] = false;
                setting.location.OpenMMC5[chipID] = false;
                setting.location.OpenVrc7[chipID] = false;
                setting.location.OpenOKIM6258[chipID] = false;
                setting.location.OpenOKIM6295[chipID] = false;
                setting.location.OpenRf5c164[chipID] = false;
                setting.location.OpenSegaPCM[chipID] = false;
                setting.location.OpenSN76489[chipID] = false;
                setting.location.OpenYm2151[chipID] = false;
                setting.location.OpenYm2203[chipID] = false;
                setting.location.OpenYm2413[chipID] = false;
                setting.location.OpenYm2608[chipID] = false;
                setting.location.OpenYm2610[chipID] = false;
                setting.location.OpenYm2612[chipID] = false;
                setting.location.OpenYm3526[chipID] = false;
                setting.location.OpenY8950[chipID] = false;
                setting.location.OpenYm3812[chipID] = false;
                setting.location.OpenYmf262[chipID] = false;
                setting.location.OpenYmf278b[chipID] = false;
                setting.location.OpenRegTest[chipID] = false;
            }

            log.ForcedWrite("frmMain_FormClosing:STEP 04");

            if (WindowState == FormWindowState.Normal)
            {
                setting.location.PMain = Location;
            }
            else
            {
                setting.location.PMain = RestoreBounds.Location;
            }
            if (frmPlayList != null && !frmPlayList.isClosed)
            {
                frmPlayList.Close();
                setting.location.OPlayList = true;
            }
            if (frmInfo != null && !frmInfo.isClosed)
            {
                frmInfo.Close();
                setting.location.OInfo = true;
            }
            if (frmMixer2 != null && !frmMixer2.isClosed)
            {
                frmMixer2.Close();
                setting.location.OMixer = true;
            }
            if (frmYM2612MIDI != null && !frmYM2612MIDI.isClosed)
            {
                frmYM2612MIDI.Close();
                setting.location.OpenYm2612MIDI = true;
            }
            if (frmVSTeffectList != null && !frmVSTeffectList.isClosed)
            {
                frmVSTeffectList.Close();
                setting.location.OpenVSTeffectList = true;
            }

            for (int chipID = 0; chipID < 2; chipID++)
            {
                if (frmAY8910[chipID] != null && !frmAY8910[chipID].isClosed)
                {
                    frmAY8910[chipID].Close();
                    setting.location.OpenAY8910[chipID] = true;
                }
                if (frmC140[chipID] != null && !frmC140[chipID].isClosed)
                {
                    frmC140[chipID].Close();
                    setting.location.OpenC140[chipID] = true;
                }
                if (frmYMZ280B[chipID] != null && !frmYMZ280B[chipID].isClosed)
                {
                    frmYMZ280B[chipID].Close();
                    setting.location.OpenYMZ280B[chipID] = true;
                }
                if (frmC352[chipID] != null && !frmC352[chipID].isClosed)
                {
                    frmC352[chipID].Close();
                    setting.location.OpenC352[chipID] = true;
                }
                if (frmQSound[chipID] != null && !frmQSound[chipID].isClosed)
                {
                    frmQSound[chipID].Close();
                    setting.location.OpenQSound[chipID] = true;
                }
                if (frmFDS[chipID] != null && !frmFDS[chipID].isClosed)
                {
                    frmFDS[chipID].Close();
                    setting.location.OpenFDS[chipID] = true;
                }
                if (frmHuC6280[chipID] != null && !frmHuC6280[chipID].isClosed)
                {
                    frmHuC6280[chipID].Close();
                    setting.location.OpenHuC6280[chipID] = true;
                }
                if (frmK051649[chipID] != null && !frmK051649[chipID].isClosed)
                {
                    frmK051649[chipID].Close();
                    setting.location.OpenK051649[chipID] = true;
                }
                if (frmK051649[chipID] != null && !frmK051649[chipID].isClosed)
                {
                    frmK051649[chipID].Close();
                    setting.location.OpenK051649[chipID] = true;
                }
                if (frmMCD[chipID] != null && !frmMCD[chipID].isClosed)
                {
                    frmMCD[chipID].Close();
                    setting.location.OpenRf5c164[chipID] = true;
                }
                if (frmMIDI[chipID] != null && !frmMIDI[chipID].isClosed)
                {
                    frmMIDI[chipID].Close();
                    setting.location.OpenMIDI[chipID] = true;
                }
                if (frmMMC5[chipID] != null && !frmMMC5[chipID].isClosed)
                {
                    frmMMC5[chipID].Close();
                    setting.location.OpenMMC5[chipID] = true;
                }
                if (frmVRC7[chipID] != null && !frmVRC7[chipID].isClosed)
                {
                    frmVRC7[chipID].Close();
                    setting.location.OpenVrc7[chipID] = true;
                }
                if (frmNESDMC[chipID] != null && !frmNESDMC[chipID].isClosed)
                {
                    frmNESDMC[chipID].Close();
                    setting.location.OpenNESDMC[chipID] = true;
                }
                if (frmOKIM6258[chipID] != null && !frmOKIM6258[chipID].isClosed)
                {
                    frmOKIM6258[chipID].Close();
                    setting.location.OpenOKIM6258[chipID] = true;
                }
                if (frmOKIM6295[chipID] != null && !frmOKIM6295[chipID].isClosed)
                {
                    frmOKIM6295[chipID].Close();
                    setting.location.OpenOKIM6295[chipID] = true;
                }
                if (frmSegaPCM[chipID] != null && !frmSegaPCM[chipID].isClosed)
                {
                    frmSegaPCM[chipID].Close();
                    setting.location.OpenSegaPCM[chipID] = true;
                }
                if (frmSN76489[chipID] != null && !frmSN76489[chipID].isClosed)
                {
                    frmSN76489[chipID].Close();
                    setting.location.OpenSN76489[chipID] = true;
                }
                if (frmYM2151[chipID] != null && !frmYM2151[chipID].isClosed)
                {
                    frmYM2151[chipID].Close();
                    setting.location.OpenYm2151[chipID] = true;
                }
                if (frmYM2203[chipID] != null && !frmYM2203[chipID].isClosed)
                {
                    frmYM2203[chipID].Close();
                    setting.location.OpenYm2203[chipID] = true;
                }
                if (frmYM2413[chipID] != null && !frmYM2413[chipID].isClosed)
                {
                    frmYM2413[chipID].Close();
                    setting.location.OpenYm2413[chipID] = true;
                }
                if (frmYM2608[chipID] != null && !frmYM2608[chipID].isClosed)
                {
                    frmYM2608[chipID].Close();
                    setting.location.OpenYm2608[chipID] = true;
                }
                if (frmYM2610[chipID] != null && !frmYM2610[chipID].isClosed)
                {
                    frmYM2610[chipID].Close();
                    setting.location.OpenYm2610[chipID] = true;
                }
                if (frmYM2612[chipID] != null && !frmYM2612[chipID].isClosed)
                {
                    frmYM2612[chipID].Close();
                    setting.location.OpenYm2612[chipID] = true;
                }
                if (frmYM3526[chipID] != null && !frmYM3526[chipID].isClosed)
                {
                    frmYM3526[chipID].Close();
                    setting.location.OpenYm3526[chipID] = true;
                }
                if (frmY8950[chipID] != null && !frmY8950[chipID].isClosed)
                {
                    frmY8950[chipID].Close();
                    setting.location.OpenY8950[chipID] = true;
                }
                if (frmYM3812[chipID] != null && !frmYM3812[chipID].isClosed)
                {
                    frmYM3812[chipID].Close();
                    setting.location.OpenYm3812[chipID] = true;
                }
                if (frmYMF262[chipID] != null && !frmYMF262[chipID].isClosed)
                {
                    frmYMF262[chipID].Close();
                    setting.location.OpenYmf262[chipID] = true;
                }
                if (frmYMF278B[chipID] != null && !frmYMF278B[chipID].isClosed)
                {
                    frmYMF278B[chipID].Close();
                    setting.location.OpenYmf278b[chipID] = true;
                }

                if (frmRegTest != null && !frmRegTest.isClosed)
                {
                    frmRegTest.Close();
                    setting.location.OpenRegTest[chipID] = true;
                }
            }

            log.ForcedWrite("frmMain_FormClosing:STEP 05");

            setting.Save();

            log.ForcedWrite("frmMain_FormClosing:STEP 06");
            log.ForcedWrite("終了処理完了");

        }

        private void pbScreen_MouseMove(object sender, MouseEventArgs e)
        {
            int px = e.Location.X / setting.other.Zoom;
            int py = e.Location.Y / setting.other.Zoom;

            if (py < 24)
            {
                for (int n = 0; n < newButton.Length; n++)
                {
                    newButton[n] = 0;
                }
                return;
            }

            for (int n = 0; n < newButton.Length; n++)
            {
                //if (px >= 320 - (16 - n) * 16 && px < 320 - (15 - n) * 16) newButton[n] = 1;
                if (px >= n * 16 + 32 && px < n * 16 + 48) newButton[n] = 1;
                else newButton[n] = 0;
            }

        }

        private void pbScreen_MouseLeave(object sender, EventArgs e)
        {
            for (int i = 0; i < newButton.Length; i++)
                newButton[i] = 0;
        }

        private void pbScreen_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button== MouseButtons.Right)
            {
                cmsMenu.Show();
                System.Drawing.Point p = Control.MousePosition;
                cmsMenu.Top = p.Y;
                cmsMenu.Left = p.X;
                return;
            }

            int px = e.Location.X / setting.other.Zoom;
            int py = e.Location.Y / setting.other.Zoom;

            if (py < 16)
            {
                if (px < 8 * 2) return;
                if (px < 8 * 5 + 4)
                {
                    if (py < 8) tsmiPAY8910_Click(null, null);
                    else tsmiSAY8910_Click(null, null);
                    return;
                }
                if (px < 8 * 7)
                {
                    if (py < 8) tsmiPOPLL_Click(null, null);
                    else tsmiSOPLL_Click(null, null);
                    return;
                }
                if (px < 8 * 9)
                {
                    if (py < 8) tsmiPOPN_Click(null, null);
                    else tsmiSOPN_Click(null, null);
                    return;
                }
                if (px < 8 * 11)
                {
                    if (py < 8) tsmiPOPN2_Click(null, null);
                    else tsmiSOPN2_Click(null, null);
                    return;
                }
                if (px < 8 * 13 + 4)
                {
                    if (py < 8) tsmiPOPNA_Click(null, null);
                    else tsmiSOPNA_Click(null, null);
                    return;
                }
                if (px < 8 * 16)
                {
                    if (py < 8) tsmiPOPNB_Click(null, null);
                    else tsmiSOPNB_Click(null, null);
                    return;
                }
                if (px < 8 * 18 + 4)
                {
                    if (py < 8) tsmiPOPM_Click(null, null);
                    else tsmiSOPM_Click(null, null);
                    return;
                }
                if (px < 8 * 20 + 4)
                {
                    if (py < 8) tsmiPDCSG_Click(null, null);
                    else tsmiSDCSG_Click(null, null);
                    return;
                }
                if (px < 8 * 23)
                {
                    if (py < 8) tsmiPRF5C164_Click(null, null);
                    else tsmiSRF5C164_Click(null, null);
                    return;
                }
                if (px < 8 * 25 + 4)
                {
                    return;
                }
                if (px < 8 * 27 + 4)
                {
                    if (py < 8) tsmiPOKIM6258_Click(null, null);
                    else tsmiSOKIM6258_Click(null, null);
                    return;
                }
                if (px < 8 * 30)
                {
                    if (py < 8) tsmiPOKIM6295_Click(null, null);
                    else tsmiSOKIM6295_Click(null, null);
                    return;
                }
                if (px < 8 * 32 + 4)
                {
                    if (py < 8) tsmiPC140_Click(null, null);
                    else tsmiSC140_Click(null, null);
                    return;
                }
                if (px < 8 * 35)
                {
                    if (py < 8) tsmiPSegaPCM_Click(null, null);
                    else tsmiSSegaPCM_Click(null, null);
                    return;
                }
                if (px < 8 * 37 + 4)
                {
                    if (py < 8) tsmiPHuC6280_Click(null, null);
                    else tsmiSHuC6280_Click(null, null);
                    return;
                }
                return;
            }

            if (py < 24) return;

            // ボタンの判定

            if (px >= 0 * 16 + 32 && px < 1 * 16 + 32)
            {
                tsmiOption_Click(null, null);
                return;
            }

            if (px >= 1 * 16 + 32 && px < 2 * 16 + 32)
            {
                tsmiStop_Click(null, null);
                return;
            }

            if (px >= 2 * 16 + 32 && px < 3 * 16 + 32)
            {
                tsmiPause_Click(null,null);
                return;
            }

            if (px >= 3 * 16 + 32 && px < 4 * 16 + 32)
            {
                tsmiFadeOut_Click(null,null);
                return;
            }

            if (px >= 4 * 16 + 32 && px < 5 * 16 + 32)
            {
                prev();
                oldParam = new MDChipParams();
                return;
            }

            if (px >= 5 * 16 + 32 && px < 6 * 16 + 32)
            {
                tsmiSlow_Click(null, null);
                return;
            }

            if (px >= 6 * 16 + 32 && px < 7 * 16 + 32)
            {
                tsmiPlay_Click(null, null);
                return;
            }

            if (px >= 7 * 16 + 32 && px < 8 * 16 + 32)
            {
                tsmiFf_Click(null, null);
                return;
            }

            if (px >= 8 * 16 + 32 && px < 9 * 16 + 32)
            {
                tsmiNext_Click(null, null);
                return;
            }

            if (px >= 9 * 16 + 32 && px < 10 * 16 + 32)
            {
                tsmiPlayMode_Click(null, null);
                return;
            }

            if (px >= 10 * 16 + 32 && px < 11 * 16 + 32)
            {
                tsmiOpenFile_Click(null, null);
                return;
            }

            if (px >= 11 * 16 + 32 && px < 12 * 16 + 32)
            {
                tsmiPlayList_Click(null, null);
                return;
            }

            if (px >= 12 * 16 + 32 && px < 13 * 16 + 32)
            {
                tsmiOpenInfo_Click(null, null);
                return;
            }

            if (px >= 13 * 16 + 32 && px < 14 * 16 + 32)
            {
                tsmiOpenMixer_Click(null, null);
                return;
            }

            if (px >= 14 * 16 + 32 && px < 15 * 16 + 32)
            {
                tsmiKBrd_Click(null,null);
                return;
            }

            if (px >= 15 * 16 + 32 && px < 16 * 16 + 32)
            {
                tsmiVST_Click(null,null);
                return;
            }

            if (px >= 16 * 16 + 32 && px < 17 * 16 + 32)
            {
                tsmiMIDIkbd_Click(null,null);
                return;
            }

            if (px >= 17 * 16 + 32 && px < 18 * 16 + 32)
            {
                tsmiChangeZoom_Click(null, null);
                return;
            }
        }



        private void tsmiPOPN_Click(object sender, EventArgs e)
        {
            OpenFormYM2203(0);
        }

        private void tsmiPOPN2_Click(object sender, EventArgs e)
        {
            OpenFormYM2612(0);
        }

        private void tsmiPOPNA_Click(object sender, EventArgs e)
        {
            OpenFormYM2608(0);
        }

        private void tsmiPOPNB_Click(object sender, EventArgs e)
        {
            OpenFormYM2610(0);
        }

        private void tsmiPOPM_Click(object sender, EventArgs e)
        {
            OpenFormYM2151(0);
        }

        private void tsmiPDCSG_Click(object sender, EventArgs e)
        {
            OpenFormSN76489(0);
        }

        private void tsmiPRF5C164_Click(object sender, EventArgs e)
        {
            OpenFormMegaCD(0);
        }

        private void tsmiPPWM_Click(object sender, EventArgs e)
        {

        }

        private void tsmiPOKIM6258_Click(object sender, EventArgs e)
        {
            OpenFormOKIM6258(0);
        }

        private void tsmiPOKIM6295_Click(object sender, EventArgs e)
        {
            OpenFormOKIM6295(0);
        }

        private void tsmiPC140_Click(object sender, EventArgs e)
        {
            OpenFormC140(0);
        }

        private void tsmiPC352_Click(object sender, EventArgs e)
        {
            OpenFormC352(0);
        }

        private void tsmiPMultiPCM_Click(object sender, EventArgs e)
        {
            OpenFormMultiPCM(0);
        }

        private void tsmiPQSound_Click(object sender, EventArgs e)
        {
            OpenFormQSound(0);
        }

        private void tsmiPSegaPCM_Click(object sender, EventArgs e)
        {
            OpenFormSegaPCM(0);
        }

        private void tsmiPAY8910_Click(object sender, EventArgs e)
        {
            OpenFormAY8910(0);
        }

        private void tsmiPOPLL_Click(object sender, EventArgs e)
        {
            OpenFormYM2413(0);
        }

        private void tsmiPOPL_Click(object sender, EventArgs e)
        {
            OpenFormYM3526(0);
        }

        private void tsmiPY8950_Click(object sender, EventArgs e)
        {
            OpenFormY8950(0);
        }

        private void tsmiPOPL2_Click(object sender, EventArgs e)
        {
            OpenFormYM3812(0);
        }

        private void tsmiPOPL3_Click(object sender, EventArgs e)
        {
            OpenFormYMF262(0);
        }

        private void tsmiPOPL4_Click(object sender, EventArgs e)
        {
            OpenFormYMF278B(0);
        }

        private void tsmiPHuC6280_Click(object sender, EventArgs e)
        {
            OpenFormHuC6280(0);
        }

        private void tsmiPK051649_Click(object sender, EventArgs e)
        {
            OpenFormK051649(0);
        }

        private void tsmiPMMC5_Click(object sender, EventArgs e)
        {
            OpenFormMMC5(0);
        }

        private void tsmiSMMC5_Click(object sender, EventArgs e)
        {
            OpenFormMMC5(1);
        }

        private void tsmiSOPN_Click(object sender, EventArgs e)
        {
            OpenFormYM2203(1);
        }

        private void tsmiSOPN2_Click(object sender, EventArgs e)
        {
            OpenFormYM2612(1);
        }

        private void tsmiSOPNA_Click(object sender, EventArgs e)
        {
            OpenFormYM2608(1);
        }

        private void tsmiSOPNB_Click(object sender, EventArgs e)
        {
            OpenFormYM2610(1);
        }

        private void tsmiSOPM_Click(object sender, EventArgs e)
        {
            OpenFormYM2151(1);
        }

        private void tsmiSDCSG_Click(object sender, EventArgs e)
        {
            OpenFormSN76489(1);
        }

        private void tsmiSRF5C164_Click(object sender, EventArgs e)
        {
            OpenFormMegaCD(1);
        }

        private void tsmiSPWM_Click(object sender, EventArgs e)
        {

        }

        private void tsmiSOKIM6258_Click(object sender, EventArgs e)
        {
            OpenFormOKIM6258(1);
        }

        private void tsmiSOKIM6295_Click(object sender, EventArgs e)
        {
            OpenFormOKIM6295(1);
        }

        private void tsmiSC140_Click(object sender, EventArgs e)
        {
            OpenFormC140(1);
        }

        private void tsmiYMZ280B_Click(object sender, EventArgs e)
        {
            OpenFormYMZ280B(0);
        }

        private void tsmiSYMZ280B_Click(object sender, EventArgs e)
        {
            OpenFormYMZ280B(1);
        }

        private void tsmiSC352_Click(object sender, EventArgs e)
        {
            OpenFormC352(1);
        }

        private void tsmiSMultiPCM_Click(object sender, EventArgs e)
        {
            OpenFormMultiPCM(1);
        }

        private void tsmiSQSound_Click(object sender, EventArgs e)
        {
            OpenFormQSound(1);
        }

        private void tsmiSSegaPCM_Click(object sender, EventArgs e)
        {
            OpenFormSegaPCM(1);
        }

        private void tsmiSAY8910_Click(object sender, EventArgs e)
        {
            OpenFormAY8910(1);
        }

        private void tsmiSOPLL_Click(object sender, EventArgs e)
        {
            OpenFormYM2413(1);
        }

        private void tsmiSOPL_Click(object sender, EventArgs e)
        {
            OpenFormYM3526(1);
        }

        private void tsmiSY8950_Click(object sender, EventArgs e)
        {
            OpenFormY8950(1);
        }

        private void tsmiSOPL2_Click(object sender, EventArgs e)
        {
            OpenFormYM3812(1);
        }

        private void tsmiSOPL3_Click(object sender, EventArgs e)
        {
            OpenFormYMF262(1);
        }

        private void tsmiSOPL4_Click(object sender, EventArgs e)
        {
            OpenFormYMF278B(1);
        }

        private void tsmiSHuC6280_Click(object sender, EventArgs e)
        {
            OpenFormHuC6280(1);
        }

        private void tsmiSK051649_Click(object sender, EventArgs e)
        {
            OpenFormK051649(1);
        }

        private void tsmiPMIDI_Click(object sender, EventArgs e)
        {
            OpenFormMIDI(0);
        }

        private void tsmiSMIDI_Click(object sender, EventArgs e)
        {
            OpenFormMIDI(1);
        }

        private void tsmiPNESDMC_Click(object sender, EventArgs e)
        {
            OpenFormNESDMC(0);
        }

        private void tsmiSNESDMC_Click(object sender, EventArgs e)
        {
            OpenFormNESDMC(1);
        }

        private void tsmiPFDS_Click(object sender, EventArgs e)
        {
            OpenFormFDS(0);
        }

        private void tsmiSFDS_Click(object sender, EventArgs e)
        {
            OpenFormFDS(1);
        }

        private void tsmiPVRC7_Click(object sender, EventArgs e)
        {
            OpenFormVRC7(0);
        }

        private void tsmiSVRC7_Click(object sender, EventArgs e)
        {
            OpenFormVRC7(1);
        }


        private void OpenFormMegaCD(int chipID, bool force = false)
        {
            if (frmMCD[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormMegaCD(chipID);
                    return;
                }
                else
                    return;
            }

            frmMCD[chipID] = new frmMegaCD(this, chipID, setting.other.Zoom, newParam.rf5c164[chipID], oldParam.rf5c164[chipID]);
            if (setting.location.PosRf5c164[chipID] == System.Drawing.Point.Empty)
            {
                frmMCD[chipID].x = this.Location.X;
                frmMCD[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmMCD[chipID].x = setting.location.PosRf5c164[chipID].X;
                frmMCD[chipID].y = setting.location.PosRf5c164[chipID].Y;
            }

            frmMCD[chipID].Show();
            frmMCD[chipID].update();
            frmMCD[chipID].Text = string.Format("RF5C164 ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.rf5c164[chipID] = new MDChipParams.RF5C164();

            CheckAndSetForm(frmMCD[chipID]);
        }

        private void CloseFormMegaCD(int chipID)
        {
            if (frmMCD[chipID] == null) return;

            try
            {
                frmMCD[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);

            }
            try
            {
                frmMCD[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmMCD[chipID] = null;
        }

        private void OpenFormYM2608(int chipID, bool force = false)
        {
            if (frmYM2608[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormYM2608(chipID);
                    return;
                }
                else
                    return;
            }

            frmYM2608[chipID] = new frmYM2608(this, chipID, setting.other.Zoom, newParam.ym2608[chipID], oldParam.ym2608[chipID]);

            if (setting.location.PosYm2608[chipID] == System.Drawing.Point.Empty)
            {
                frmYM2608[chipID].x = this.Location.X;
                frmYM2608[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmYM2608[chipID].x = setting.location.PosYm2608[chipID].X;
                frmYM2608[chipID].y = setting.location.PosYm2608[chipID].Y;
            }

            frmYM2608[chipID].Show();
            frmYM2608[chipID].update();
            frmYM2608[chipID].Text = string.Format("YM2608 ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.ym2608[chipID] = new MDChipParams.YM2608();

            CheckAndSetForm(frmYM2608[chipID]);
        }

        private void CloseFormYM2608(int chipID)
        {
            if (frmYM2608[chipID] == null) return;

            try
            {
                frmYM2608[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmYM2608[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmYM2608[chipID] = null;
            return;
        }

        private void OpenFormYM2151(int chipID, bool force = false)
        {
            if (frmYM2151[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormYM2151(chipID);
                    return;
                }
                else return;
            }

            frmYM2151[chipID] = new frmYM2151(this, chipID, setting.other.Zoom, newParam.ym2151[chipID], oldParam.ym2151[chipID]);

            if (setting.location.PosYm2151[chipID] == System.Drawing.Point.Empty)
            {
                frmYM2151[chipID].x = this.Location.X;
                frmYM2151[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmYM2151[chipID].x = setting.location.PosYm2151[chipID].X;
                frmYM2151[chipID].y = setting.location.PosYm2151[chipID].Y;
            }

            frmYM2151[chipID].Show();
            frmYM2151[chipID].update();
            frmYM2151[chipID].Text = string.Format("YM2151 ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.ym2151[chipID] = new MDChipParams.YM2151();

            CheckAndSetForm(frmYM2151[chipID]);
        }

        private void CloseFormYM2151(int chipID)
        {
            if (frmYM2151[chipID] == null) return;

            try
            {
                frmYM2151[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmYM2151[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmYM2151[chipID] = null;
        }

        private void OpenFormC140(int chipID, bool force = false)
        {
            if (frmC140[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormC140(chipID);
                    return;
                }
                else return;
            }

            frmC140[chipID] = new frmC140(this, chipID, setting.other.Zoom, newParam.c140[chipID], oldParam.c140[chipID]);

            if (setting.location.PosC140[chipID] == System.Drawing.Point.Empty)
            {
                frmC140[chipID].x = this.Location.X;
                frmC140[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmC140[chipID].x = setting.location.PosC140[chipID].X;
                frmC140[chipID].y = setting.location.PosC140[chipID].Y;
            }

            frmC140[chipID].Show();
            frmC140[chipID].update();
            frmC140[chipID].Text = string.Format("C140 ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.c140[chipID] = new MDChipParams.C140();

            CheckAndSetForm(frmC140[chipID]);
        }

        private void CloseFormC140(int chipID)
        {
            if (frmC140[chipID] == null) return;

            try
            {
                frmC140[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmC140[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmC140[chipID] = null;
        }

        private void OpenFormYMZ280B(int chipID, bool force = false)
        {
            if (frmYMZ280B[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormYMZ280B(chipID);
                    return;
                }
                else return;
            }

            frmYMZ280B[chipID] = new frmYMZ280B(this, chipID, setting.other.Zoom, newParam.ymz280b[chipID], oldParam.ymz280b[chipID]);

            if (setting.location.PosYMZ280B[chipID] == System.Drawing.Point.Empty)
            {
                frmYMZ280B[chipID].x = this.Location.X;
                frmYMZ280B[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmYMZ280B[chipID].x = setting.location.PosYMZ280B[chipID].X;
                frmYMZ280B[chipID].y = setting.location.PosYMZ280B[chipID].Y;
            }

            frmYMZ280B[chipID].Show();
            frmYMZ280B[chipID].update();
            frmYMZ280B[chipID].Text = string.Format("YMZ280B ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.ymz280b[chipID] = new MDChipParams.YMZ280B();

            CheckAndSetForm(frmYMZ280B[chipID]);
        }

        private void CloseFormYMZ280B(int chipID)
        {
            if (frmYMZ280B[chipID] == null) return;

            try
            {
                frmYMZ280B[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmYMZ280B[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmYMZ280B[chipID] = null;
        }

        private void OpenFormC352(int chipID, bool force = false)
        {
            if (frmC352[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormC352(chipID);
                    return;
                }
                else return;
            }

            frmC352[chipID] = new frmC352(this, chipID, setting.other.Zoom, newParam.c352[chipID], oldParam.c352[chipID]);

            if (setting.location.PosC352[chipID] == System.Drawing.Point.Empty)
            {
                frmC352[chipID].x = this.Location.X;
                frmC352[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmC352[chipID].x = setting.location.PosC352[chipID].X;
                frmC352[chipID].y = setting.location.PosC352[chipID].Y;
            }

            frmC352[chipID].Show();
            frmC352[chipID].update();
            frmC352[chipID].Text = string.Format("C352 ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.c352[chipID] = new MDChipParams.C352();

            CheckAndSetForm(frmC352[chipID]);
        }

        private void CloseFormC352(int chipID)
        {
            if (frmC352[chipID] == null) return;

            try
            {
                frmC352[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmC352[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmC352[chipID] = null;
        }

        private void OpenFormMultiPCM(int chipID, bool force = false)
        {
            if (frmMultiPCM[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormMultiPCM(chipID);
                    return;
                }
                else return;
            }

            frmMultiPCM[chipID] = new frmMultiPCM(this, chipID, setting.other.Zoom, newParam.multiPCM[chipID], oldParam.multiPCM[chipID]);

            if (setting.location.PosMultiPCM[chipID] == System.Drawing.Point.Empty)
            {
                frmMultiPCM[chipID].x = this.Location.X;
                frmMultiPCM[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmMultiPCM[chipID].x = setting.location.PosMultiPCM[chipID].X;
                frmMultiPCM[chipID].y = setting.location.PosMultiPCM[chipID].Y;
            }

            frmMultiPCM[chipID].Show();
            frmMultiPCM[chipID].update();
            frmMultiPCM[chipID].Text = string.Format("MultiPCM ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.multiPCM[chipID] = new MDChipParams.MultiPCM();

            CheckAndSetForm(frmMultiPCM[chipID]);
        }

        private void CloseFormMultiPCM(int chipID)
        {
            if (frmMultiPCM[chipID] == null) return;

            try
            {
                frmMultiPCM[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmMultiPCM[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmMultiPCM[chipID] = null;
        }

        private void OpenFormQSound(int chipID, bool force = false)
        {
            if (frmQSound[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormQSound(chipID);
                    return;
                }
                else return;
            }

            frmQSound[chipID] = new frmQSound(this, chipID, setting.other.Zoom, newParam.qSound[chipID], oldParam.qSound[chipID]);

            if (setting.location.PosQSound[chipID] == System.Drawing.Point.Empty)
            {
                frmQSound[chipID].x = this.Location.X;
                frmQSound[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmQSound[chipID].x = setting.location.PosQSound[chipID].X;
                frmQSound[chipID].y = setting.location.PosQSound[chipID].Y;
            }

            frmQSound[chipID].Show();
            frmQSound[chipID].update();
            frmQSound[chipID].Text = string.Format("QSound ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.qSound[chipID] = new MDChipParams.QSound();

            CheckAndSetForm(frmQSound[chipID]);
        }

        private void CloseFormQSound(int chipID)
        {
            if (frmQSound[chipID] == null) return;

            try
            {
                frmQSound[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmQSound[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmQSound[chipID] = null;
        }

        private void OpenFormYM2203(int chipID, bool force = false)
        {
            if (frmYM2203[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormYM2203(chipID);
                    return;
                }
                else return;
            }

            frmYM2203[chipID] = new frmYM2203(this, chipID, setting.other.Zoom, newParam.ym2203[chipID], oldParam.ym2203[chipID]);

            if (setting.location.PosYm2203[chipID] == System.Drawing.Point.Empty)
            {
                frmYM2203[chipID].x = this.Location.X;
                frmYM2203[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmYM2203[chipID].x = setting.location.PosYm2203[chipID].X;
                frmYM2203[chipID].y = setting.location.PosYm2203[chipID].Y;
            }

            frmYM2203[chipID].Show();
            frmYM2203[chipID].update();
            frmYM2203[chipID].Text = string.Format("YM2203 ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.ym2203[chipID] = new MDChipParams.YM2203();

            CheckAndSetForm(frmYM2203[chipID]);

        }

        private void CloseFormYM2203(int chipID)
        {
            if (frmYM2203[chipID] == null) return;

            try
            {
                frmYM2203[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmYM2203[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmYM2203[chipID] = null;
        }

        private void OpenFormYM2610(int chipID, bool force = false)
        {
            if (frmYM2610[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormYM2610(chipID);
                    return;
                }
                else return;
            }

            frmYM2610[chipID] = new frmYM2610(this, chipID, setting.other.Zoom, newParam.ym2610[chipID], oldParam.ym2610[chipID]);

            if (setting.location.PosYm2610[chipID] == System.Drawing.Point.Empty)
            {
                frmYM2610[chipID].x = this.Location.X;
                frmYM2610[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmYM2610[chipID].x = setting.location.PosYm2610[chipID].X;
                frmYM2610[chipID].y = setting.location.PosYm2610[chipID].Y;
            }

            frmYM2610[chipID].Show();
            frmYM2610[chipID].update();
            frmYM2610[chipID].Text = string.Format("YM2610 ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.ym2610[chipID] = new MDChipParams.YM2610();

            CheckAndSetForm(frmYM2610[chipID]);
        }

        private void CloseFormYM2610(int chipID)
        {
            if (frmYM2610[chipID] == null) return;

            try
            {
                frmYM2610[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmYM2610[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmYM2610[chipID] = null;
        }

        private void OpenFormYM2612(int chipID, bool force = false)
        {
            if (frmYM2612[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormYM2612(chipID);
                    return;
                }
                else return;
            }

            frmYM2612[chipID] = new frmYM2612(this, chipID, setting.other.Zoom, newParam.ym2612[chipID], oldParam.ym2612[chipID]);

            if (setting.location.PosYm2612[chipID] == System.Drawing.Point.Empty)
            {
                frmYM2612[chipID].x = this.Location.X;
                frmYM2612[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmYM2612[chipID].x = setting.location.PosYm2612[chipID].X;
                frmYM2612[chipID].y = setting.location.PosYm2612[chipID].Y;
            }

            frmYM2612[chipID].Show();
            frmYM2612[chipID].update();
            frmYM2612[chipID].Text = string.Format("YM2612 ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.ym2612[chipID] = new MDChipParams.YM2612();

            CheckAndSetForm(frmYM2612[chipID]);
        }

        private void CloseFormYM2612(int chipID)
        {
            if (frmYM2612[chipID] == null) return;
            try
            {
                frmYM2612[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmYM2612[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmYM2612[chipID] = null;
        }

        private void OpenFormOKIM6258(int chipID, bool force = false)
        {
            if (frmOKIM6258[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormOKIM6258(chipID);
                    return;
                }
                else return;
            }

            frmOKIM6258[chipID] = new frmOKIM6258(this, chipID, setting.other.Zoom, newParam.okim6258[chipID]);

            if (setting.location.PosOKIM6258[chipID] == System.Drawing.Point.Empty)
            {
                frmOKIM6258[chipID].x = this.Location.X;
                frmOKIM6258[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmOKIM6258[chipID].x = setting.location.PosOKIM6258[chipID].X;
                frmOKIM6258[chipID].y = setting.location.PosOKIM6258[chipID].Y;
            }

            frmOKIM6258[chipID].Show();
            frmOKIM6258[chipID].update();
            frmOKIM6258[chipID].Text = string.Format("OKIM6258 ({0})", chipID == 0 ? "Primary" : "Secondary");

            CheckAndSetForm(frmOKIM6258[chipID]);
        }

        private void CloseFormOKIM6258(int chipID)
        {
            if (frmOKIM6258[chipID] == null) return;

            try
            {
                frmOKIM6258[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmOKIM6258[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmOKIM6258[chipID] = null;
        }

        private void OpenFormOKIM6295(int chipID, bool force = false)
        {
            if (frmOKIM6295[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormOKIM6295(chipID);
                    return;
                }
                else return;
            }

            frmOKIM6295[chipID] = new frmOKIM6295(this, chipID, setting.other.Zoom, newParam.okim6295[chipID], oldParam.okim6295[chipID]);

            if (setting.location.PosOKIM6295[chipID] == System.Drawing.Point.Empty)
            {
                frmOKIM6295[chipID].x = this.Location.X;
                frmOKIM6295[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmOKIM6295[chipID].x = setting.location.PosOKIM6295[chipID].X;
                frmOKIM6295[chipID].y = setting.location.PosOKIM6295[chipID].Y;
            }

            frmOKIM6295[chipID].Show();
            frmOKIM6295[chipID].update();
            frmOKIM6295[chipID].Text = string.Format("OKIM6295 ({0})", chipID == 0 ? "Primary" : "Secondary");

            CheckAndSetForm(frmOKIM6295[chipID]);
        }

        private void CloseFormOKIM6295(int chipID)
        {
            if (frmOKIM6295[chipID] == null) return;

            try
            {
                frmOKIM6295[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmOKIM6295[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmOKIM6295[chipID] = null;
        }

        private void OpenFormSN76489(int chipID, bool force = false)
        {
            if (frmSN76489[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormSN76489(chipID);
                    return;
                }
                else return;
            }

            frmSN76489[chipID] = new frmSN76489(this, chipID, setting.other.Zoom, newParam.sn76489[chipID], oldParam.sn76489[chipID]);

            if (setting.location.PosSN76489[chipID] == System.Drawing.Point.Empty)
            {
                frmSN76489[chipID].x = this.Location.X;
                frmSN76489[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmSN76489[chipID].x = setting.location.PosSN76489[chipID].X;
                frmSN76489[chipID].y = setting.location.PosSN76489[chipID].Y;
            }

            frmSN76489[chipID].Show();
            frmSN76489[chipID].update();
            frmSN76489[chipID].Text = string.Format("SN76489 ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.sn76489[chipID] = new MDChipParams.SN76489();

            CheckAndSetForm(frmSN76489[chipID]);
        }

        private void CloseFormSN76489(int chipID)
        {
            if (frmSN76489[chipID] == null) return;

            try
            {
                frmSN76489[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmSN76489[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmSN76489[chipID] = null;
        }

        private void OpenFormSegaPCM(int chipID, bool force = false)
        {
            if (frmSegaPCM[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormSegaPCM(chipID);
                    return;
                }
                else return;
            }

            frmSegaPCM[chipID] = new frmSegaPCM(this, chipID, setting.other.Zoom, newParam.segaPcm[chipID], oldParam.segaPcm[chipID]);

            if (setting.location.PosSegaPCM[chipID] == System.Drawing.Point.Empty)
            {
                frmSegaPCM[chipID].x = this.Location.X;
                frmSegaPCM[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmSegaPCM[chipID].x = setting.location.PosSegaPCM[chipID].X;
                frmSegaPCM[chipID].y = setting.location.PosSegaPCM[chipID].Y;
            }

            frmSegaPCM[chipID].Show();
            frmSegaPCM[chipID].update();
            frmSegaPCM[chipID].Text = string.Format("SegaPCM ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.segaPcm[chipID] = new MDChipParams.SegaPcm();

            CheckAndSetForm(frmSegaPCM[chipID]);
        }

        private void CloseFormSegaPCM(int chipID)
        {
            if (frmSegaPCM[chipID] == null) return;

            try
            {
                frmSegaPCM[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmSegaPCM[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmSegaPCM[chipID] = null;
        }


        private void OpenFormAY8910(int chipID, bool force = false)
        {
            if (frmAY8910[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormAY8910(chipID);
                    return;
                }
                else return;
            }

            frmAY8910[chipID] = new frmAY8910(this, chipID, setting.other.Zoom, newParam.ay8910[chipID], oldParam.ay8910[chipID]);

            if (setting.location.PosAY8910[chipID] == System.Drawing.Point.Empty)
            {
                frmAY8910[chipID].x = this.Location.X;
                frmAY8910[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmAY8910[chipID].x = setting.location.PosAY8910[chipID].X;
                frmAY8910[chipID].y = setting.location.PosAY8910[chipID].Y;
            }

            frmAY8910[chipID].Show();
            frmAY8910[chipID].update();
            frmAY8910[chipID].Text = string.Format("AY8910 ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.ay8910[chipID] = new MDChipParams.AY8910();

            CheckAndSetForm(frmAY8910[chipID]);
        }

        private void CloseFormAY8910(int chipID)
        {
            if (frmAY8910[chipID] == null) return;

            try
            {
                frmAY8910[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmAY8910[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmAY8910[chipID] = null;
        }

        private void OpenFormHuC6280(int chipID, bool force = false)
        {
            if (frmHuC6280[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormHuC6280(chipID);
                    return;
                }
                else return;
            }

            frmHuC6280[chipID] = new frmHuC6280(this, chipID, setting.other.Zoom, newParam.huc6280[chipID], oldParam.huc6280[chipID]);

            if (setting.location.PosHuC6280[chipID] == System.Drawing.Point.Empty)
            {
                frmHuC6280[chipID].x = this.Location.X;
                frmHuC6280[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmHuC6280[chipID].x = setting.location.PosHuC6280[chipID].X;
                frmHuC6280[chipID].y = setting.location.PosHuC6280[chipID].Y;
            }

            frmHuC6280[chipID].Show();
            frmHuC6280[chipID].update();
            frmHuC6280[chipID].Text = string.Format("HuC6280 ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.huc6280[chipID] = new MDChipParams.HuC6280();

            CheckAndSetForm(frmHuC6280[chipID]);
        }

        private void CloseFormHuC6280(int chipID)
        {
            if (frmHuC6280[chipID] == null) return;

            try
            {
                frmHuC6280[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmHuC6280[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmHuC6280[chipID] = null;
        }

        private void OpenFormK051649(int chipID, bool force = false)
        {
            if (frmK051649[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormK051649(chipID);
                    return;
                }
                else return;
            }

            frmK051649[chipID] = new frmK051649(this, chipID, setting.other.Zoom, newParam.k051649[chipID]);

            if (setting.location.PosK051649[chipID] == System.Drawing.Point.Empty)
            {
                frmK051649[chipID].x = this.Location.X;
                frmK051649[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmK051649[chipID].x = setting.location.PosK051649[chipID].X;
                frmK051649[chipID].y = setting.location.PosK051649[chipID].Y;
            }

            frmK051649[chipID].Show();
            frmK051649[chipID].update();
            frmK051649[chipID].Text = string.Format("K051649 ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.k051649[chipID] = new MDChipParams.K051649();

            CheckAndSetForm(frmK051649[chipID]);
        }

        private void CloseFormK051649(int chipID)
        {
            if (frmK051649[chipID] == null) return;

            try
            {
                frmK051649[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmK051649[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmK051649[chipID] = null;
        }

        private void OpenFormYM2413(int chipID, bool force = false)
        {
            if (frmYM2413[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormYM2413(chipID);
                    return;
                }
                else return;
            }

            frmYM2413[chipID] = new frmYM2413(this, chipID, setting.other.Zoom, newParam.ym2413[chipID], oldParam.ym2413[chipID]);

            if (setting.location.PosYm2413[chipID] == System.Drawing.Point.Empty)
            {
                frmYM2413[chipID].x = this.Location.X;
                frmYM2413[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmYM2413[chipID].x = setting.location.PosYm2413[chipID].X;
                frmYM2413[chipID].y = setting.location.PosYm2413[chipID].Y;
            }

            frmYM2413[chipID].Show();
            frmYM2413[chipID].update();
            frmYM2413[chipID].Text = string.Format("YM2413 ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.ym2413[chipID] = new MDChipParams.YM2413();

            CheckAndSetForm(frmYM2413[chipID]);
        }

        private void CloseFormYM2413(int chipID)
        {
            if (frmYM2413[chipID] == null) return;

            try
            {
                frmYM2413[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmYM2413[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmYM2413[chipID] = null;
        }

        private void OpenFormYM3526(int chipID, bool force = false)
        {
            if (frmYM3526[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormYM3526(chipID);
                    return;
                }
                else return;
            }

            frmYM3526[chipID] = new frmYM3526(this, chipID, setting.other.Zoom, newParam.ym3526[chipID], oldParam.ym3526[chipID]);

            if (setting.location.PosYm3526[chipID] == System.Drawing.Point.Empty)
            {
                frmYM3526[chipID].x = this.Location.X;
                frmYM3526[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmYM3526[chipID].x = setting.location.PosYm3526[chipID].X;
                frmYM3526[chipID].y = setting.location.PosYm3526[chipID].Y;
            }

            frmYM3526[chipID].Show();
            frmYM3526[chipID].update();
            frmYM3526[chipID].Text = string.Format("YM3526 ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.ym3526[chipID] = new MDChipParams.YM3526();

            CheckAndSetForm(frmYM3526[chipID]);
        }

        private void CloseFormYM3526(int chipID)
        {
            if (frmYM3526[chipID] == null) return;

            try
            {
                frmYM3526[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmYM3526[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmYM3526[chipID] = null;
        }

        private void OpenFormY8950(int chipID, bool force = false)
        {
            if (frmY8950[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormY8950(chipID);
                    return;
                }
                else return;
            }

            frmY8950[chipID] = new frmY8950(this, chipID, setting.other.Zoom, newParam.y8950[chipID]);

            if (setting.location.PosY8950[chipID] == System.Drawing.Point.Empty)
            {
                frmY8950[chipID].x = this.Location.X;
                frmY8950[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmY8950[chipID].x = setting.location.PosY8950[chipID].X;
                frmY8950[chipID].y = setting.location.PosY8950[chipID].Y;
            }

            frmY8950[chipID].Show();
            frmY8950[chipID].update();
            frmY8950[chipID].Text = string.Format("Y8950 ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.y8950[chipID] = new MDChipParams.Y8950();

            CheckAndSetForm(frmY8950[chipID]);
        }

        private void CloseFormY8950(int chipID)
        {
            if (frmY8950[chipID] == null) return;

            try
            {
                frmY8950[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmY8950[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmY8950[chipID] = null;
        }

        private void OpenFormYM3812(int chipID, bool force = false)
        {
            if (frmYM3812[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormYM3812(chipID);
                    return;
                }
                else return;
            }

            frmYM3812[chipID] = new frmYM3812(this, chipID, setting.other.Zoom, newParam.ym3812[chipID], oldParam.ym3812[chipID]);

            if (setting.location.PosYm3812[chipID] == System.Drawing.Point.Empty)
            {
                frmYM3812[chipID].x = this.Location.X;
                frmYM3812[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmYM3812[chipID].x = setting.location.PosYm3812[chipID].X;
                frmYM3812[chipID].y = setting.location.PosYm3812[chipID].Y;
            }

            frmYM3812[chipID].Show();
            frmYM3812[chipID].update();
            frmYM3812[chipID].Text = string.Format("YM3812 ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.ym3812[chipID] = new MDChipParams.YM3812();

            CheckAndSetForm(frmYM3812[chipID]);
        }

        private void CloseFormYM3812(int chipID)
        {
            if (frmYM3812[chipID] == null) return;

            try
            {
                frmYM3812[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmYM3812[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmYM3812[chipID] = null;
        }

        private void OpenFormYMF262(int chipID, bool force = false)
        {
            if (frmYMF262[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormYMF262(chipID);
                    return;
                }
                else return;
            }

            frmYMF262[chipID] = new frmYMF262(this, chipID, setting.other.Zoom, newParam.ymf262[chipID], oldParam.ymf262[chipID]);

            if (setting.location.PosYmf262[chipID] == System.Drawing.Point.Empty)
            {
                frmYMF262[chipID].x = this.Location.X;
                frmYMF262[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmYMF262[chipID].x = setting.location.PosYmf262[chipID].X;
                frmYMF262[chipID].y = setting.location.PosYmf262[chipID].Y;
            }

            frmYMF262[chipID].Show();
            frmYMF262[chipID].update();
            frmYMF262[chipID].Text = string.Format("YMF262 ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.ymf262[chipID] = new MDChipParams.YMF262();

            CheckAndSetForm(frmYMF262[chipID]);
        }

        private void CloseFormYMF262(int chipID)
        {
            if (frmYMF262[chipID] == null) return;

            try
            {
                frmYMF262[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmYMF262[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmYMF262[chipID] = null;
        }

        private void OpenFormYMF278B(int chipID, bool force = false)
        {
            if (frmYMF278B[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormYMF278B(chipID);
                    return;
                }
                else return;
            }

            frmYMF278B[chipID] = new frmYMF278B(this, chipID, setting.other.Zoom, newParam.ymf278b[chipID]);

            if (setting.location.PosYmf278b[chipID] == System.Drawing.Point.Empty)
            {
                frmYMF278B[chipID].x = this.Location.X;
                frmYMF278B[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmYMF278B[chipID].x = setting.location.PosYmf278b[chipID].X;
                frmYMF278B[chipID].y = setting.location.PosYmf278b[chipID].Y;
            }

            frmYMF278B[chipID].Show();
            frmYMF278B[chipID].update();
            frmYMF278B[chipID].Text = string.Format("YMF278B ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.ymf278b[chipID] = new MDChipParams.YMF278B();

            CheckAndSetForm(frmYMF278B[chipID]);
        }

        private void CloseFormYMF278B(int chipID)
        {
            if (frmYMF278B[chipID] == null) return;

            try
            {
                frmYMF278B[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmYMF278B[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmYMF278B[chipID] = null;
        }

        private void OpenFormMIDI(int chipID, bool force = false)
        {
            if (frmMIDI[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormMIDI(chipID);
                    return;
                }
                else return;
            }

            frmMIDI[chipID] = new frmMIDI(this, chipID, setting.other.Zoom, newParam.midi[chipID]);

            if (setting.location.PosMIDI[chipID] == System.Drawing.Point.Empty)
            {
                frmMIDI[chipID].x = this.Location.X;
                frmMIDI[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmMIDI[chipID].x = setting.location.PosMIDI[chipID].X;
                frmMIDI[chipID].y = setting.location.PosMIDI[chipID].Y;
            }

            frmMIDI[chipID].Show();
            frmMIDI[chipID].update();
            frmMIDI[chipID].Text = string.Format("MIDI ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.midi[chipID] = new MIDIParam();

            CheckAndSetForm(frmMIDI[chipID]);
        }

        private void CloseFormMIDI(int chipID)
        {
            if (frmMIDI[chipID] == null) return;

            try
            {
                frmMIDI[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmMIDI[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmMIDI[chipID] = null;
        }

        private void OpenFormNESDMC(int chipID, bool force = false)
        {
            if (frmNESDMC[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormNESDMC(chipID);
                    return;
                }
                else return;
            }

            frmNESDMC[chipID] = new frmNESDMC(this, chipID, setting.other.Zoom, newParam.nesdmc[chipID]);

            if (setting.location.PosNESDMC[chipID] == System.Drawing.Point.Empty)
            {
                frmNESDMC[chipID].x = this.Location.X;
                frmNESDMC[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmNESDMC[chipID].x = setting.location.PosNESDMC[chipID].X;
                frmNESDMC[chipID].y = setting.location.PosNESDMC[chipID].Y;
            }

            frmNESDMC[chipID].Show();
            frmNESDMC[chipID].update();
            frmNESDMC[chipID].Text = string.Format("NES&DMC ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.nesdmc[chipID] = new MDChipParams.NESDMC();

            CheckAndSetForm(frmNESDMC[chipID]);
        }

        private void CloseFormNESDMC(int chipID)
        {
            if (frmNESDMC[chipID] == null) return;

            try
            {
                frmNESDMC[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmNESDMC[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmNESDMC[chipID] = null;
        }

        private void OpenFormFDS(int chipID, bool force = false)
        {
            if (frmFDS[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormFDS(chipID);
                    return;
                }
                else return;
            }

            frmFDS[chipID] = new frmFDS(this, chipID, setting.other.Zoom, newParam.fds[chipID]);

            if (setting.location.PosFDS[chipID] == System.Drawing.Point.Empty)
            {
                frmFDS[chipID].x = this.Location.X;
                frmFDS[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmFDS[chipID].x = setting.location.PosFDS[chipID].X;
                frmFDS[chipID].y = setting.location.PosFDS[chipID].Y;
            }

            frmFDS[chipID].Show();
            frmFDS[chipID].update();
            frmFDS[chipID].Text = string.Format("FDS ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.fds[chipID] = new MDChipParams.FDS();

            CheckAndSetForm(frmFDS[chipID]);
        }

        private void CloseFormFDS(int chipID)
        {
            if (frmFDS[chipID] == null) return;

            try
            {
                frmFDS[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmFDS[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmFDS[chipID] = null;
        }

        private void OpenFormVRC7(int chipID, bool force = false)
        {
            if (frmVRC7[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormVRC7(chipID);
                    return;
                }
                else return;
            }

            frmVRC7[chipID] = new frmVRC7(this, chipID, setting.other.Zoom, newParam.vrc7[chipID]);

            if (setting.location.PosVrc7[chipID] == System.Drawing.Point.Empty)
            {
                frmVRC7[chipID].x = this.Location.X;
                frmVRC7[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmVRC7[chipID].x = setting.location.PosVrc7[chipID].X;
                frmVRC7[chipID].y = setting.location.PosVrc7[chipID].Y;
            }

            frmVRC7[chipID].Show();
            frmVRC7[chipID].update();
            frmVRC7[chipID].Text = string.Format("VRC7 ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.vrc7[chipID] = new MDChipParams.VRC7();

            CheckAndSetForm(frmVRC7[chipID]);
        }

        private void CloseFormVRC7(int chipID)
        {
            if (frmVRC7[chipID] == null) return;

            try
            {
                frmVRC7[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmVRC7[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmVRC7[chipID] = null;
        }

        private void OpenFormMMC5(int chipID, bool force = false)
        {
            if (frmMMC5[chipID] != null)// && frmInfo.isClosed)
            {
                if (!force)
                {
                    CloseFormMMC5(chipID);
                    return;
                }
                else return;
            }

            frmMMC5[chipID] = new frmMMC5(this, chipID, setting.other.Zoom, newParam.mmc5[chipID]);

            if (setting.location.PosMMC5[chipID] == System.Drawing.Point.Empty)
            {
                frmMMC5[chipID].x = this.Location.X;
                frmMMC5[chipID].y = this.Location.Y + 264;
            }
            else
            {
                frmMMC5[chipID].x = setting.location.PosMMC5[chipID].X;
                frmMMC5[chipID].y = setting.location.PosMMC5[chipID].Y;
            }

            frmMMC5[chipID].Show();
            frmMMC5[chipID].update();
            frmMMC5[chipID].Text = string.Format("MMC5 ({0})", chipID == 0 ? "Primary" : "Secondary");
            oldParam.mmc5[chipID] = new MDChipParams.MMC5();

            CheckAndSetForm(frmMMC5[chipID]);
        }

        private void CloseFormMMC5(int chipID)
        {
            if (frmMMC5[chipID] == null) return;

            try
            {
                frmMMC5[chipID].Close();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            try
            {
                frmMMC5[chipID].Dispose();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
            frmMMC5[chipID] = null;
        }

        private void OpenFormRegTest(int chipID, EnmChip selectedChip = EnmChip.Unuse, bool force = false) {
            if(frmRegTest != null) {
                frmRegTest.changeChip(selectedChip);
                return;
            }
            /*if (frmRegTest != null) {
                if (!force) {
                    CloseFormRegTest(chipID);
                    return;
                } else return;
            }*/

            frmRegTest = new frmRegTest(this, chipID, selectedChip, setting.other.Zoom);

            if (setting.location.PosRegTest[chipID] == System.Drawing.Point.Empty)
            {
                frmRegTest.x = this.Location.X;
                frmRegTest.y = this.Location.Y + 264;
            }
            else
            {
                frmRegTest.x = setting.location.PosRegTest[chipID].X;
                frmRegTest.y = setting.location.PosRegTest[chipID].Y;
            }

            frmRegTest.Show();
            frmRegTest.update();
            frmRegTest.Text = string.Format("RegTest ({0})", chipID == 0 ? "Primary" : "Secondary");

            CheckAndSetForm(frmRegTest);
        }

        private void CloseFormRegTest(int chipID) {
            if (frmRegTest == null) return;

            try {
                frmRegTest.Close();
            } catch (Exception ex) {
                log.ForcedWrite(ex);
            }
            try {
                frmRegTest.Dispose();
            } catch (Exception ex) {
                log.ForcedWrite(ex);
            }
            frmRegTest = null;
        }



        private void openInfo()
        {
            if (frmInfo != null && !frmInfo.isClosed)
            {
                try
                {
                    frmInfo.Close();
                    frmInfo.Dispose();
                }
                catch { }
                finally
                {
                    frmInfo = null;
                }
                return;
            }

            if (frmInfo != null)
            {
                try
                {
                    frmInfo.Close();
                    frmInfo.Dispose();
                }
                catch { }
                finally
                {
                    frmInfo = null;
                }
            }

            frmInfo = new frmInfo(this);
            if (setting.location.PInfo == System.Drawing.Point.Empty)
            {
                frmInfo.x = this.Location.X + 328;
                frmInfo.y = this.Location.Y;
            }
            else
            {
                frmInfo.x = setting.location.PInfo.X;
                frmInfo.y = setting.location.PInfo.Y;
            }

            Screen s = Screen.FromControl(frmInfo);
            Rectangle rc = new Rectangle(frmInfo.Location, frmInfo.Size);
            if (s.WorkingArea.Contains(rc))
            {
                frmInfo.Location = rc.Location;
                frmInfo.Size = rc.Size;
            }
            else
            {
                frmInfo.Location = new System.Drawing.Point(100, 100);
            }

            frmInfo.setting = setting;
            frmInfo.Show();
            frmInfo.update();
        }

        private void openMIDIKeyboard()
        {
            if (frmYM2612MIDI != null && !frmYM2612MIDI.isClosed)
            {
                try
                {
                    frmYM2612MIDI.Close();
                    frmYM2612MIDI.Dispose();
                }
                catch { }
                finally
                {
                    frmYM2612MIDI = null;
                }
                return;
            }

            if (frmYM2612MIDI != null)
            {
                try
                {
                    frmYM2612MIDI.Close();
                    frmYM2612MIDI.Dispose();
                }
                catch { }
                finally
                {
                    frmYM2612MIDI = null;
                }
            }

            frmYM2612MIDI = new frmYM2612MIDI(this, setting.other.Zoom, newParam.ym2612Midi);
            if (setting.location.PosYm2612MIDI == System.Drawing.Point.Empty)
            {
                frmYM2612MIDI.x = this.Location.X + 328;
                frmYM2612MIDI.y = this.Location.Y;
            }
            else
            {
                frmYM2612MIDI.x = setting.location.PosYm2612MIDI.X;
                frmYM2612MIDI.y = setting.location.PosYm2612MIDI.Y;
            }

            Screen s = Screen.FromControl(frmYM2612MIDI);
            Rectangle rc = new Rectangle(frmYM2612MIDI.Location, frmYM2612MIDI.Size);
            if (s.WorkingArea.Contains(rc))
            {
                frmYM2612MIDI.Location = rc.Location;
                frmYM2612MIDI.Size = rc.Size;
            }
            else
            {
                frmYM2612MIDI.Location = new System.Drawing.Point(100, 100);
            }

            //frmYM2612MIDI.setting = setting;
            frmYM2612MIDI.Show();
            frmYM2612MIDI.update();
            oldParam.ym2612Midi = new MDChipParams.YM2612MIDI();
        }

        private void openSetting()
        {
            frmSetting frm = new frmSetting(setting);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                flgReinit = true;
                reinit(frm.setting);

            }
        }

        private void reinit(Setting setting)
        {
            if (!flgReinit) return;

            StopMIDIInMonitoring();
            frmPlayList.Stop();
            Audio.Stop();
            Audio.Close();

            this.setting = setting;
            this.setting.Save();

            screen.setting = this.setting;
            frmPlayList.setting = this.setting;
            //oldParam = new MDChipParams();
            //newParam = new MDChipParams();
            reqAllScreenInit = true;
            //screen.screenInitAll();

            log.ForcedWrite("設定が変更されたため、再度Audio初期化処理開始");

            Audio.Init(this.setting);

            log.ForcedWrite("Audio初期化処理完了");
            log.debug = this.setting.Debug_DispFrameCounter;

            frmVSTeffectList.dispPluginList();
            StartMIDIInMonitoring();

            IsInitialOpenFolder = true;
            flgReinit = false;

            for (int i = 0; i < 5; i++)
            {
                System.Threading.Thread.Sleep(100);
                Application.DoEvents();
            }

        }

        private void openMixer()
        {
            if (frmMixer2 != null && !frmMixer2.isClosed)
            {
                try
                {
                    frmMixer2.Close();
                    frmMixer2.Dispose();
                }
                catch
                {
                }
                finally
                {
                    frmMixer2 = null;
                }
                return;
            }

            if (frmMixer2 != null)
            {
                try
                {
                    frmMixer2.Close();
                    frmMixer2.Dispose();
                }
                catch
                {
                }
                finally
                {
                    frmMixer2 = null;
                }
            }

            frmMixer2 = new frmMixer2(this, setting.other.Zoom, newParam.mixer);
            if (setting.location.PosMixer == System.Drawing.Point.Empty)
            {
                frmMixer2.x = this.Location.X + 328;
                frmMixer2.y = this.Location.Y;
            }
            else
            {
                frmMixer2.x = setting.location.PosMixer.X;
                frmMixer2.y = setting.location.PosMixer.Y;
            }

            Screen s = Screen.FromControl(frmMixer2);
            Rectangle rc = new Rectangle(frmMixer2.Location, frmMixer2.Size);
            if (s.WorkingArea.Contains(rc))
            {
                frmMixer2.Location = rc.Location;
                frmMixer2.Size = rc.Size;
            }
            else
            {
                frmMixer2.Location = new System.Drawing.Point(100, 100);
            }

            //frmMixer.setting = setting;
            //screen.AddMixer(frmMixer2.pbScreen, Properties.Resources.planeMixer);
            frmMixer2.Show();
            frmMixer2.update();
            //screen.screenInitMixer();
            oldParam.mixer = new MDChipParams.Mixer();
        }



        private void pbScreen_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;

        }

        private void pbScreen_DragDrop(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string filename = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];

                try
                {

                    frmPlayList.Stop();

                    frmPlayList.getPlayList().AddFile(filename);
                    //frmPlayList.AddList(filename);

                    if (filename.ToLower().LastIndexOf(".zip") == -1)
                    {
                        loadAndPlay(0, 0, filename);
                        frmPlayList.setStart(-1);
                        oldParam = new MDChipParams();

                        frmPlayList.Play();
                    }
                }
                catch (Exception ex)
                {
                    log.ForcedWrite(ex);
                    MessageBox.Show("ファイルの読み込みに失敗しました。");
                }
            }
        }

        protected override bool ShowWithoutActivation
        {
            get
            {
                return true;
            }
        }


        private void allScreenInit()
        {
            //oldParam = new MDChipParams();
            DrawBuff.drawTimer(screen.mainScreen, 0, ref oldParam.Cminutes, ref oldParam.Csecond, ref oldParam.Cmillisecond, newParam.Cminutes, newParam.Csecond, newParam.Cmillisecond);
            DrawBuff.drawTimer(screen.mainScreen, 1, ref oldParam.TCminutes, ref oldParam.TCsecond, ref oldParam.TCmillisecond, newParam.TCminutes, newParam.TCsecond, newParam.TCmillisecond);
            DrawBuff.drawTimer(screen.mainScreen, 2, ref oldParam.LCminutes, ref oldParam.LCsecond, ref oldParam.LCmillisecond, newParam.LCminutes, newParam.LCsecond, newParam.LCmillisecond);
            screenInit();

            for (int i = 0; i < 2; i++)
            {
                //if (frmAY8910[i] != null) frmAY8910[i].screenInit();
                //if (frmC140[i] != null) frmC140[i].screenInit();
                //if (frmC352[i] != null) frmC352[i].screenInit();
                if (frmFDS[i] != null) frmFDS[i].screenInit();
                //if (frmHuC6280[i] != null) frmHuC6280[i].screenInit();
                if (frmK051649[i] != null) frmK051649[i].screenInit();
                if (frmMCD[i] != null) frmMCD[i].screenInit();
                if (frmMIDI[i] != null) frmMIDI[i].screenInit();
                if (frmMMC5[i] != null) frmMMC5[i].screenInit();
                if (frmNESDMC[i] != null) frmNESDMC[i].screenInit();
                if (frmOKIM6258[i] != null) frmOKIM6258[i].screenInit();
                if (frmOKIM6295[i] != null) frmOKIM6295[i].screenInit();
                //if (frmSegaPCM[i] != null) frmSegaPCM[i].screenInit();
                //if (frmSN76489[i] != null) frmSN76489[i].screenInit();
                //if (frmYM2151[i] != null) frmYM2151[i].screenInit();
                //if (frmYM2203[i] != null) frmYM2203[i].screenInit();
                //if (frmYM2413[i] != null) frmYM2413[i].screenInit();
                //if (frmYM2608[i] != null) frmYM2608[i].screenInit();
                //if (frmYM2610[i] != null) frmYM2610[i].screenInit();
                //if (frmYM2612[i] != null) frmYM2612[i].screenInit();
                if (frmYM3526[i] != null) frmYM3526[i].screenInit();
                if (frmY8950[i] != null) frmY8950[i].screenInit();
                if (frmYM3812[i] != null) frmYM3812[i].screenInit();
                if (frmYMF262[i] != null) frmYMF262[i].screenInit();
                if (frmYMF278B[i] != null) frmYMF278B[i].screenInit();
                if (frmVRC7[i] != null) frmVRC7[i].screenInit();

            }

            if (frmMixer2 != null) frmMixer2.screenInit();
            if (frmInfo != null) frmInfo.screenInit();
            //if (frmYM2612MIDI != null) frmYM2612MIDI.screenInit();

            if (frmRegTest != null) frmRegTest.screenInit();

            reqAllScreenInit = false;
        }

        private void screenMainLoop()
        {
            double nextFrame = (double)System.Environment.TickCount;
            isRunning = true;
            stopped = false;

            while (isRunning)
            {

                if (reqAllScreenInit)
                {
                    allScreenInit();
                }

                float period = 1000f / (float)setting.other.ScreenFrameRate;
                double tickCount = (double)System.Environment.TickCount;

                if (tickCount < nextFrame)
                {
                    if (nextFrame - tickCount > 1)
                    {
                        System.Threading.Thread.Sleep((int)(nextFrame - tickCount));
                    }
                    continue;
                }

                screenChangeParams();

                for (int chipID = 0; chipID < 2; chipID++)
                {
                    if (frmMCD[chipID] != null && !frmMCD[chipID].isClosed) frmMCD[chipID].screenChangeParams();
                    else frmMCD[chipID] = null;

                    if (frmC140[chipID] != null && !frmC140[chipID].isClosed) frmC140[chipID].screenChangeParams();
                    else frmC140[chipID] = null;

                    if (frmYMZ280B[chipID] != null && !frmYMZ280B[chipID].isClosed) frmYMZ280B[chipID].screenChangeParams();
                    else frmYMZ280B[chipID] = null;

                    if (frmC352[chipID] != null && !frmC352[chipID].isClosed) frmC352[chipID].screenChangeParams();
                    else frmC352[chipID] = null;

                    if (frmMultiPCM[chipID] != null && !frmMultiPCM[chipID].isClosed) frmMultiPCM[chipID].screenChangeParams();
                    else frmMultiPCM[chipID] = null;

                    if (frmQSound[chipID] != null && !frmQSound[chipID].isClosed) frmQSound[chipID].screenChangeParams();
                    else frmQSound[chipID] = null;

                    if (frmYM2608[chipID] != null && !frmYM2608[chipID].isClosed) frmYM2608[chipID].screenChangeParams();
                    else frmYM2608[chipID] = null;

                    if (frmYM2151[chipID] != null && !frmYM2151[chipID].isClosed) frmYM2151[chipID].screenChangeParams();
                    else frmYM2151[chipID] = null;

                    if (frmYM2203[chipID] != null && !frmYM2203[chipID].isClosed) frmYM2203[chipID].screenChangeParams();
                    else frmYM2203[chipID] = null;

                    if (frmYM2413[chipID] != null && !frmYM2413[chipID].isClosed) frmYM2413[chipID].screenChangeParams();
                    else frmYM2413[chipID] = null;

                    if (frmYM2610[chipID] != null && !frmYM2610[chipID].isClosed) frmYM2610[chipID].screenChangeParams();
                    else frmYM2610[chipID] = null;

                    if (frmYM2612[chipID] != null && !frmYM2612[chipID].isClosed) frmYM2612[chipID].screenChangeParams();
                    else frmYM2612[chipID] = null;

                    if (frmYM3526[chipID] != null && !frmYM3526[chipID].isClosed) frmYM3526[chipID].screenChangeParams();
                    else frmYM3526[chipID] = null;

                    if (frmY8950[chipID] != null && !frmY8950[chipID].isClosed) frmY8950[chipID].screenChangeParams();
                    else frmY8950[chipID] = null;

                    if (frmYM3812[chipID] != null && !frmYM3812[chipID].isClosed) frmYM3812[chipID].screenChangeParams();
                    else frmYM3812[chipID] = null;

                    if (frmYMF262[chipID] != null && !frmYMF262[chipID].isClosed) frmYMF262[chipID].screenChangeParams();
                    else frmYMF262[chipID] = null;
             
                    if (frmYMF278B[chipID] != null && !frmYMF278B[chipID].isClosed) frmYMF278B[chipID].screenChangeParams();
                    else frmYMF278B[chipID] = null;

                    if (frmOKIM6258[chipID] != null && !frmOKIM6258[chipID].isClosed) frmOKIM6258[chipID].screenChangeParams();
                    else frmOKIM6258[chipID] = null;

                    if (frmOKIM6295[chipID] != null && !frmOKIM6295[chipID].isClosed) frmOKIM6295[chipID].screenChangeParams();
                    else frmOKIM6295[chipID] = null;

                    if (frmSN76489[chipID] != null && !frmSN76489[chipID].isClosed) frmSN76489[chipID].screenChangeParams();
                    else frmSN76489[chipID] = null;

                    if (frmSegaPCM[chipID] != null && !frmSegaPCM[chipID].isClosed) frmSegaPCM[chipID].screenChangeParams();
                    else frmSegaPCM[chipID] = null;

                    if (frmAY8910[chipID] != null && !frmAY8910[chipID].isClosed)
                    {
                        frmAY8910[chipID].screenChangeParams();
                    }
                    else frmAY8910[chipID] = null;

                    if (frmHuC6280[chipID] != null && !frmHuC6280[chipID].isClosed) frmHuC6280[chipID].screenChangeParams();
                    else frmHuC6280[chipID] = null;

                    if (frmK051649[chipID] != null && !frmK051649[chipID].isClosed) frmK051649[chipID].screenChangeParams();
                    else frmK051649[chipID] = null;

                    if (frmMIDI[chipID] != null && !frmMIDI[chipID].isClosed) frmMIDI[chipID].screenChangeParams();
                    else frmMIDI[chipID] = null;

                    if (frmNESDMC[chipID] != null && !frmNESDMC[chipID].isClosed) frmNESDMC[chipID].screenChangeParams();
                    else frmNESDMC[chipID] = null;

                    if (frmFDS[chipID] != null && !frmFDS[chipID].isClosed) frmFDS[chipID].screenChangeParams();
                    else frmFDS[chipID] = null;

                    if (frmMMC5[chipID] != null && !frmMMC5[chipID].isClosed) frmMMC5[chipID].screenChangeParams();
                    else frmMMC5[chipID] = null;

                    if (frmVRC7[chipID] != null && !frmVRC7[chipID].isClosed) frmVRC7[chipID].screenChangeParams();
                    else frmVRC7[chipID] = null;

                }
                if (frmYM2612MIDI != null && !frmYM2612MIDI.isClosed) frmYM2612MIDI.screenChangeParams();
                else frmYM2612MIDI = null;
                if (frmMixer2 != null && !frmMixer2.isClosed) frmMixer2.screenChangeParams();
                else frmMixer2 = null;

                if (frmRegTest != null && !frmRegTest.isClosed) frmRegTest.screenChangeParams();
                else frmRegTest = null;

                if ((double)System.Environment.TickCount >= nextFrame + period)
                {
                    nextFrame += period;
                    continue;
                }

                screenDrawParams();

                for (int chipID = 0; chipID < 2; chipID++)
                {
                    if (frmMCD[chipID] != null && !frmMCD[chipID].isClosed) { frmMCD[chipID].screenDrawParams(); frmMCD[chipID].update(); }
                    else frmMCD[chipID] = null;

                    if (frmC140[chipID] != null && !frmC140[chipID].isClosed) { frmC140[chipID].screenDrawParams(); frmC140[chipID].update(); }
                    else frmC140[chipID] = null;

                    if (frmYMZ280B[chipID] != null && !frmYMZ280B[chipID].isClosed) { frmYMZ280B[chipID].screenDrawParams(); frmYMZ280B[chipID].update(); }
                    else frmYMZ280B[chipID] = null;

                    if (frmC352[chipID] != null && !frmC352[chipID].isClosed) { frmC352[chipID].screenDrawParams(); frmC352[chipID].update(); }
                    else frmC352[chipID] = null;

                    if (frmMultiPCM[chipID] != null && !frmMultiPCM[chipID].isClosed) { frmMultiPCM[chipID].screenDrawParams(); frmMultiPCM[chipID].update(); }
                    else frmMultiPCM[chipID] = null;

                    if (frmQSound[chipID] != null && !frmQSound[chipID].isClosed) { frmQSound[chipID].screenDrawParams(); frmQSound[chipID].update(); }
                    else frmQSound[chipID] = null;

                    if (frmYM2608[chipID] != null && !frmYM2608[chipID].isClosed) { frmYM2608[chipID].screenDrawParams(); frmYM2608[chipID].update(); }
                    else frmYM2608[chipID] = null;

                    if (frmYM2151[chipID] != null && !frmYM2151[chipID].isClosed) { frmYM2151[chipID].screenDrawParams(); frmYM2151[chipID].update(); }
                    else frmYM2151[chipID] = null;

                    if (frmYM2203[chipID] != null && !frmYM2203[chipID].isClosed) { frmYM2203[chipID].screenDrawParams(); frmYM2203[chipID].update(); }
                    else frmYM2203[chipID] = null;

                    if (frmYM2413[chipID] != null && !frmYM2413[chipID].isClosed) { frmYM2413[chipID].screenDrawParams(); frmYM2413[chipID].update(); }
                    else frmYM2413[chipID] = null;

                    if (frmYM2610[chipID] != null && !frmYM2610[chipID].isClosed) { frmYM2610[chipID].screenDrawParams(); frmYM2610[chipID].update(); }
                    else frmYM2610[chipID] = null;

                    if (frmYM2612[chipID] != null && !frmYM2612[chipID].isClosed) { frmYM2612[chipID].screenDrawParams(); frmYM2612[chipID].update(); }
                    else frmYM2612[chipID] = null;

                    if (frmYM3526[chipID] != null && !frmYM3526[chipID].isClosed) { frmYM3526[chipID].screenDrawParams(); frmYM3526[chipID].update(); }
                    else frmYM3526[chipID] = null;

                    if (frmY8950[chipID] != null && !frmY8950[chipID].isClosed) { frmY8950[chipID].screenDrawParams(); frmY8950[chipID].update(); }
                    else frmY8950[chipID] = null;

                    if (frmYM3812[chipID] != null && !frmYM3812[chipID].isClosed) { frmYM3812[chipID].screenDrawParams(); frmYM3812[chipID].update(); }
                    else frmYM3812[chipID] = null;

                    if (frmYMF262[chipID] != null && !frmYMF262[chipID].isClosed) { frmYMF262[chipID].screenDrawParams(); frmYMF262[chipID].update(); }
                    else frmYMF262[chipID] = null;

                    if (frmYMF278B[chipID] != null && !frmYMF278B[chipID].isClosed) { frmYMF278B[chipID].screenDrawParams(); frmYMF278B[chipID].update(); }
                    else frmYMF278B[chipID] = null;

                    if (frmOKIM6258[chipID] != null && !frmOKIM6258[chipID].isClosed) { frmOKIM6258[chipID].screenDrawParams(); frmOKIM6258[chipID].update(); }
                    else frmOKIM6258[chipID] = null;

                    if (frmOKIM6295[chipID] != null && !frmOKIM6295[chipID].isClosed) { frmOKIM6295[chipID].screenDrawParams(); frmOKIM6295[chipID].update(); }
                    else frmOKIM6295[chipID] = null;

                    if (frmSN76489[chipID] != null && !frmSN76489[chipID].isClosed) { frmSN76489[chipID].screenDrawParams(); frmSN76489[chipID].update(); }
                    else frmSN76489[chipID] = null;

                    if (frmSegaPCM[chipID] != null && !frmSegaPCM[chipID].isClosed) { frmSegaPCM[chipID].screenDrawParams(); frmSegaPCM[chipID].update(); }
                    else frmSegaPCM[chipID] = null;

                    if (frmAY8910[chipID] != null && !frmAY8910[chipID].isClosed) { frmAY8910[chipID].screenDrawParams(); frmAY8910[chipID].update(); }
                    else frmAY8910[chipID] = null;

                    if (frmHuC6280[chipID] != null && !frmHuC6280[chipID].isClosed) { frmHuC6280[chipID].screenDrawParams(); frmHuC6280[chipID].update(); }
                    else frmHuC6280[chipID] = null;

                    if (frmK051649[chipID] != null && !frmK051649[chipID].isClosed) { frmK051649[chipID].screenDrawParams(); frmK051649[chipID].update(); }
                    else frmK051649[chipID] = null;

                    if (frmMIDI[chipID] != null && !frmMIDI[chipID].isClosed) { frmMIDI[chipID].screenDrawParams(); frmMIDI[chipID].update(); }
                    else frmMIDI[chipID] = null;

                    if (frmNESDMC[chipID] != null && !frmNESDMC[chipID].isClosed) { frmNESDMC[chipID].screenDrawParams(); frmNESDMC[chipID].update(); }
                    else frmNESDMC[chipID] = null;

                    if (frmVRC7[chipID] != null && !frmVRC7[chipID].isClosed) { frmVRC7[chipID].screenDrawParams(); frmVRC7[chipID].update(); }
                    else frmVRC7[chipID] = null;

                    if (frmFDS[chipID] != null && !frmFDS[chipID].isClosed) { frmFDS[chipID].screenDrawParams(); frmFDS[chipID].update(); }
                    else frmFDS[chipID] = null;

                    if (frmMMC5[chipID] != null && !frmMMC5[chipID].isClosed) { frmMMC5[chipID].screenDrawParams(); frmMMC5[chipID].update(); }
                    else frmMMC5[chipID] = null;

                }
                if (frmYM2612MIDI != null && !frmYM2612MIDI.isClosed) { frmYM2612MIDI.screenDrawParams(); frmYM2612MIDI.update(); }
                else frmYM2612MIDI = null;
                if (frmMixer2 != null && !frmMixer2.isClosed) { frmMixer2.screenDrawParams(); frmMixer2.update(); }
                else frmMixer2 = null;

                if (frmRegTest != null && !frmRegTest.isClosed) { frmRegTest.screenDrawParams(); frmRegTest.update(); } else frmRegTest = null;


                nextFrame += period;

                if (frmPlayList.isPlaying())
                {
                    if ((setting.other.UseLoopTimes && Audio.GetVgmCurLoopCounter() > setting.other.LoopTimes - 1)
                        || Audio.GetVGMStopped())
                    {
                        fadeout();
                    }
                    if (Audio.Stopped && frmPlayList.isPlaying())
                    {
                        nextPlayMode();
                    }
                }

                if (Audio.fatalError)
                {
                    log.ForcedWrite("AudioでFatalErrorが発生。再度Audio初期化処理開始");

                    frmPlayList.Stop();
                    try { Audio.Stop(); }
                    catch (Exception ex)
                    {
                        log.ForcedWrite(ex);
                    }

                    try { Audio.Close(); }
                    catch (Exception ex)
                    {
                        log.ForcedWrite(ex);
                    }

                    Audio.fatalError = false;
                    Audio.Init(setting);

                    log.ForcedWrite("Audio初期化処理完了");
                }
            }

            stopped = true;

        }

        private void screenChangeParams()
        {

            long w = Audio.GetCounter();
            double sec = (double)w / (double)Common.SampleRate;
            newParam.Cminutes = (int)(sec / 60);
            sec -= newParam.Cminutes * 60;
            newParam.Csecond = (int)sec;
            sec -= newParam.Csecond;
            newParam.Cmillisecond = (int)(sec * 100.0);

            w = Audio.GetTotalCounter();
            sec = (double)w / (double)Common.SampleRate;
            newParam.TCminutes = (int)(sec / 60);
            sec -= newParam.TCminutes * 60;
            newParam.TCsecond = (int)sec;
            sec -= newParam.TCsecond;
            newParam.TCmillisecond = (int)(sec * 100.0);

            w = Audio.GetLoopCounter();
            sec = (double)w / (double)Common.SampleRate;
            newParam.LCminutes = (int)(sec / 60);
            sec -= newParam.LCminutes * 60;
            newParam.LCsecond = (int)sec;
            sec -= newParam.LCsecond;
            newParam.LCmillisecond = (int)(sec * 100.0);

        }

        private void screenDrawParams()
        {

            // 描画

            DrawBuff.drawButtons(screen.mainScreen, oldButton, newButton, oldButtonMode, newButtonMode);

            DrawBuff.drawTimer(screen.mainScreen, 0, ref oldParam.Cminutes, ref oldParam.Csecond, ref oldParam.Cmillisecond, newParam.Cminutes, newParam.Csecond, newParam.Cmillisecond);
            DrawBuff.drawTimer(screen.mainScreen, 1, ref oldParam.TCminutes, ref oldParam.TCsecond, ref oldParam.TCmillisecond, newParam.TCminutes, newParam.TCsecond, newParam.TCmillisecond);
            DrawBuff.drawTimer(screen.mainScreen, 2, ref oldParam.LCminutes, ref oldParam.LCsecond, ref oldParam.LCmillisecond, newParam.LCminutes, newParam.LCsecond, newParam.LCmillisecond);

            byte[] chips = Audio.GetChipStatus();
            DrawBuff.drawChipName(screen.mainScreen, 14 * 4, 0 * 8, 0, ref oldParam.chipLED.PriOPN, chips[0]);
            DrawBuff.drawChipName(screen.mainScreen, 18 * 4, 0 * 8, 1, ref oldParam.chipLED.PriOPN2, chips[1]);
            DrawBuff.drawChipName(screen.mainScreen, 23 * 4, 0 * 8, 2, ref oldParam.chipLED.PriOPNA, chips[2]);
            DrawBuff.drawChipName(screen.mainScreen, 28 * 4, 0 * 8, 3, ref oldParam.chipLED.PriOPNB, chips[3]);
            DrawBuff.drawChipName(screen.mainScreen, 33 * 4, 0 * 8, 4, ref oldParam.chipLED.PriOPM, chips[4]);
            DrawBuff.drawChipName(screen.mainScreen, 37 * 4, 0 * 8, 5, ref oldParam.chipLED.PriDCSG, chips[5]);
            DrawBuff.drawChipName(screen.mainScreen, 42 * 4, 0 * 8, 6, ref oldParam.chipLED.PriRF5C, chips[6]);
            DrawBuff.drawChipName(screen.mainScreen, 47 * 4, 0 * 8, 7, ref oldParam.chipLED.PriPWM, chips[7]);
            DrawBuff.drawChipName(screen.mainScreen, 51 * 4, 0 * 8, 8, ref oldParam.chipLED.PriOKI5, chips[8]);
            DrawBuff.drawChipName(screen.mainScreen, 56 * 4, 0 * 8, 9, ref oldParam.chipLED.PriOKI9, chips[9]);
            DrawBuff.drawChipName(screen.mainScreen, 61 * 4, 0 * 8, 10, ref oldParam.chipLED.PriC140, chips[10]);
            DrawBuff.drawChipName(screen.mainScreen, 66 * 4, 0 * 8, 11, ref oldParam.chipLED.PriSPCM, chips[11]);
            DrawBuff.drawChipName(screen.mainScreen, 4 * 4, 0 * 8, 12, ref oldParam.chipLED.PriAY10, chips[12]);
            DrawBuff.drawChipName(screen.mainScreen, 9 * 4, 0 * 8, 13, ref oldParam.chipLED.PriOPLL, chips[13]);
            DrawBuff.drawChipName(screen.mainScreen, 71 * 4, 0 * 8, 14, ref oldParam.chipLED.PriHuC8, chips[14]);

            DrawBuff.drawChipName(screen.mainScreen, 14 * 4, 1 * 8, 0, ref oldParam.chipLED.SecOPN, chips[128 + 0]);
            DrawBuff.drawChipName(screen.mainScreen, 18 * 4, 1 * 8, 1, ref oldParam.chipLED.SecOPN2, chips[128 + 1]);
            DrawBuff.drawChipName(screen.mainScreen, 23 * 4, 1 * 8, 2, ref oldParam.chipLED.SecOPNA, chips[128 + 2]);
            DrawBuff.drawChipName(screen.mainScreen, 28 * 4, 1 * 8, 3, ref oldParam.chipLED.SecOPNB, chips[128 + 3]);
            DrawBuff.drawChipName(screen.mainScreen, 33 * 4, 1 * 8, 4, ref oldParam.chipLED.SecOPM, chips[128 + 4]);
            DrawBuff.drawChipName(screen.mainScreen, 37 * 4, 1 * 8, 5, ref oldParam.chipLED.SecDCSG, chips[128 + 5]);
            DrawBuff.drawChipName(screen.mainScreen, 42 * 4, 1 * 8, 6, ref oldParam.chipLED.SecRF5C, chips[128 + 6]);
            DrawBuff.drawChipName(screen.mainScreen, 47 * 4, 1 * 8, 7, ref oldParam.chipLED.SecPWM, chips[128 + 7]);
            DrawBuff.drawChipName(screen.mainScreen, 51 * 4, 1 * 8, 8, ref oldParam.chipLED.SecOKI5, chips[128 + 8]);
            DrawBuff.drawChipName(screen.mainScreen, 56 * 4, 1 * 8, 9, ref oldParam.chipLED.SecOKI9, chips[128 + 9]);
            DrawBuff.drawChipName(screen.mainScreen, 61 * 4, 1 * 8, 10, ref oldParam.chipLED.SecC140, chips[128 + 10]);
            DrawBuff.drawChipName(screen.mainScreen, 66 * 4, 1 * 8, 11, ref oldParam.chipLED.SecSPCM, chips[128 + 11]);
            DrawBuff.drawChipName(screen.mainScreen, 4 * 4, 1 * 8, 12, ref oldParam.chipLED.SecAY10, chips[128 + 12]);
            DrawBuff.drawChipName(screen.mainScreen, 9 * 4, 1 * 8, 13, ref oldParam.chipLED.SecOPLL, chips[128 + 13]);
            DrawBuff.drawChipName(screen.mainScreen, 71 * 4, 0 * 8, 14, ref oldParam.chipLED.SecHuC8, chips[128 + 14]);

            DrawBuff.drawFont4(screen.mainScreen, 0, 24, 1, Audio.GetIsDataBlock(EnmModel.VirtualModel) ? "VD" : "  ");
            DrawBuff.drawFont4(screen.mainScreen, 12, 24, 1, Audio.GetIsPcmRAMWrite(EnmModel.VirtualModel) ? "VP" : "  ");
            DrawBuff.drawFont4(screen.mainScreen, 0, 32, 1, Audio.GetIsDataBlock(EnmModel.RealModel) ? "RD" : "  ");
            DrawBuff.drawFont4(screen.mainScreen, 12, 32, 1, Audio.GetIsPcmRAMWrite(EnmModel.RealModel) ? "RP" : "  ");

            if (setting.Debug_DispFrameCounter)
            {
                long v = Audio.getVirtualFrameCounter();
                if (v != -1) DrawBuff.drawFont8(screen.mainScreen, 0, 0, 0, string.Format("EMU        : {0:D12} ", v));
                long r = Audio.getRealFrameCounter();
                if (r != -1) DrawBuff.drawFont8(screen.mainScreen, 0, 8, 0, string.Format("REAL CHIP  : {0:D12} ", r));
                long d = r - v;
                if (r != -1 && v != -1) DrawBuff.drawFont8(screen.mainScreen, 0, 16, 0, string.Format("R.CHIP-EMU : {0:D12} ", d));
                DrawBuff.drawFont8(screen.mainScreen, 0, 24, 0, string.Format("PROC TIME  : {0:D12} ", Audio.ProcTimePer1Frame));
            }

            screen.Refresh();

            Audio.updateVol();


        }

        private void screenInit()
        {

            oldParam.chipLED.PriOPN = 255;
            oldParam.chipLED.PriOPN2 = 255;
            oldParam.chipLED.PriOPNA = 255;
            oldParam.chipLED.PriOPNB = 255;
            oldParam.chipLED.PriOPM = 255;
            oldParam.chipLED.PriDCSG = 255;
            oldParam.chipLED.PriRF5C = 255;
            oldParam.chipLED.PriPWM = 255;
            oldParam.chipLED.PriOKI5 = 255;
            oldParam.chipLED.PriOKI9 = 255;
            oldParam.chipLED.PriC140 = 255;
            oldParam.chipLED.PriSPCM = 255;
            oldParam.chipLED.PriAY10 = 255;
            oldParam.chipLED.PriOPLL = 255;
            oldParam.chipLED.PriHuC8 = 255;
            oldParam.chipLED.SecOPN = 255;
            oldParam.chipLED.SecOPN2 = 255;
            oldParam.chipLED.SecOPNA = 255;
            oldParam.chipLED.SecOPNB = 255;
            oldParam.chipLED.SecOPM = 255;
            oldParam.chipLED.SecDCSG = 255;
            oldParam.chipLED.SecRF5C = 255;
            oldParam.chipLED.SecPWM = 255;
            oldParam.chipLED.SecOKI5 = 255;
            oldParam.chipLED.SecOKI9 = 255;
            oldParam.chipLED.SecC140 = 255;
            oldParam.chipLED.SecSPCM = 255;
            oldParam.chipLED.SecAY10 = 255;
            oldParam.chipLED.SecOPLL = 255;
            oldParam.chipLED.SecHuC8 = 255;

            byte[] chips = Audio.GetChipStatus();
            DrawBuff.drawChipName(screen.mainScreen, 14 * 4, 0 * 8, 0, ref oldParam.chipLED.PriOPN, chips[0]);
            DrawBuff.drawChipName(screen.mainScreen, 18 * 4, 0 * 8, 1, ref oldParam.chipLED.PriOPN2, chips[1]);
            DrawBuff.drawChipName(screen.mainScreen, 23 * 4, 0 * 8, 2, ref oldParam.chipLED.PriOPNA, chips[2]);
            DrawBuff.drawChipName(screen.mainScreen, 28 * 4, 0 * 8, 3, ref oldParam.chipLED.PriOPNB, chips[3]);
            DrawBuff.drawChipName(screen.mainScreen, 33 * 4, 0 * 8, 4, ref oldParam.chipLED.PriOPM, chips[4]);
            DrawBuff.drawChipName(screen.mainScreen, 37 * 4, 0 * 8, 5, ref oldParam.chipLED.PriDCSG, chips[5]);
            DrawBuff.drawChipName(screen.mainScreen, 42 * 4, 0 * 8, 6, ref oldParam.chipLED.PriRF5C, chips[6]);
            DrawBuff.drawChipName(screen.mainScreen, 47 * 4, 0 * 8, 7, ref oldParam.chipLED.PriPWM, chips[7]);
            DrawBuff.drawChipName(screen.mainScreen, 51 * 4, 0 * 8, 8, ref oldParam.chipLED.PriOKI5, chips[8]);
            DrawBuff.drawChipName(screen.mainScreen, 56 * 4, 0 * 8, 9, ref oldParam.chipLED.PriOKI9, chips[9]);
            DrawBuff.drawChipName(screen.mainScreen, 61 * 4, 0 * 8, 10, ref oldParam.chipLED.PriC140, chips[10]);
            DrawBuff.drawChipName(screen.mainScreen, 66 * 4, 0 * 8, 11, ref oldParam.chipLED.PriSPCM, chips[11]);
            DrawBuff.drawChipName(screen.mainScreen, 4 * 4, 0 * 8, 12, ref oldParam.chipLED.PriAY10, chips[12]);
            DrawBuff.drawChipName(screen.mainScreen, 9 * 4, 0 * 8, 13, ref oldParam.chipLED.PriOPLL, chips[13]);
            DrawBuff.drawChipName(screen.mainScreen, 71 * 4, 0 * 8, 14, ref oldParam.chipLED.PriHuC8, chips[14]);

            DrawBuff.drawChipName(screen.mainScreen, 14 * 4, 1 * 8, 0, ref oldParam.chipLED.SecOPN, chips[128 + 0]);
            DrawBuff.drawChipName(screen.mainScreen, 18 * 4, 1 * 8, 1, ref oldParam.chipLED.SecOPN2, chips[128 + 1]);
            DrawBuff.drawChipName(screen.mainScreen, 23 * 4, 1 * 8, 2, ref oldParam.chipLED.SecOPNA, chips[128 + 2]);
            DrawBuff.drawChipName(screen.mainScreen, 28 * 4, 1 * 8, 3, ref oldParam.chipLED.SecOPNB, chips[128 + 3]);
            DrawBuff.drawChipName(screen.mainScreen, 33 * 4, 1 * 8, 4, ref oldParam.chipLED.SecOPM, chips[128 + 4]);
            DrawBuff.drawChipName(screen.mainScreen, 37 * 4, 1 * 8, 5, ref oldParam.chipLED.SecDCSG, chips[128 + 5]);
            DrawBuff.drawChipName(screen.mainScreen, 42 * 4, 1 * 8, 6, ref oldParam.chipLED.SecRF5C, chips[128 + 6]);
            DrawBuff.drawChipName(screen.mainScreen, 47 * 4, 1 * 8, 7, ref oldParam.chipLED.SecPWM, chips[128 + 7]);
            DrawBuff.drawChipName(screen.mainScreen, 51 * 4, 1 * 8, 8, ref oldParam.chipLED.SecOKI5, chips[128 + 8]);
            DrawBuff.drawChipName(screen.mainScreen, 56 * 4, 1 * 8, 9, ref oldParam.chipLED.SecOKI9, chips[128 + 9]);
            DrawBuff.drawChipName(screen.mainScreen, 61 * 4, 1 * 8, 10, ref oldParam.chipLED.SecC140, chips[128 + 10]);
            DrawBuff.drawChipName(screen.mainScreen, 66 * 4, 1 * 8, 11, ref oldParam.chipLED.SecSPCM, chips[128 + 11]);
            DrawBuff.drawChipName(screen.mainScreen, 4 * 4, 1 * 8, 12, ref oldParam.chipLED.SecAY10, chips[128 + 12]);
            DrawBuff.drawChipName(screen.mainScreen, 9 * 4, 1 * 8, 13, ref oldParam.chipLED.SecOPLL, chips[128 + 13]);
            DrawBuff.drawChipName(screen.mainScreen, 71 * 4, 0 * 8, 14, ref oldParam.chipLED.SecHuC8, chips[128 + 14]);

            DrawBuff.drawFont4(screen.mainScreen, 0, 24, 1, Audio.GetIsDataBlock(EnmModel.VirtualModel) ? "VD" : "  ");
            DrawBuff.drawFont4(screen.mainScreen, 12, 24, 1, Audio.GetIsPcmRAMWrite(EnmModel.VirtualModel) ? "VP" : "  ");
            DrawBuff.drawFont4(screen.mainScreen, 0, 32, 1, Audio.GetIsDataBlock(EnmModel.RealModel) ? "RD" : "  ");
            DrawBuff.drawFont4(screen.mainScreen, 12, 32, 1, Audio.GetIsPcmRAMWrite(EnmModel.RealModel) ? "RP" : "  ");

        }



        public void stop()
        {
            if (Audio.isPaused)
            {
                Audio.Pause();
            }

            if(Audio.trdStopped && Audio.Stopped)
            {
                Audio.ResetTimeCounter();
            }

            frmPlayList.Stop();
            Audio.Stop();
            screenInit();
        }

        public void pause()
        {
            Audio.Pause();
        }

        public void fadeout()
        {
            if (Audio.isPaused)
            {
                Audio.Pause();
            }

            Audio.Fadeout();
        }

        public void prev()
        {
            if (Audio.isPaused)
            {
                Audio.Pause();
            }

            frmPlayList.prevPlay();
        }

        public void play()
        {

            if (Audio.isPaused)
            {
                Audio.Pause();
            }

            string[] fn = null;
            Tuple<int, int, string, string> playFn = null;

            frmPlayList.Stop();

            //if (srcBuf == null && frmPlayList.getMusicCount() < 1)
            if (frmPlayList.getMusicCount() < 1)
            {
                fn = fileOpen(false);
                if (fn == null) return;
                frmPlayList.getPlayList().AddFile(fn[0]);
                //frmPlayList.AddList(fn[0]);
                playFn = frmPlayList.setStart(-1); //last
            }
            else
            {
                fn = new string[1] { "" };
                playFn = frmPlayList.setStart(-2);//first 
            }

            reqAllScreenInit = true;

            if (loadAndPlay(playFn.Item1, playFn.Item2, playFn.Item3, playFn.Item4))
            {
                frmPlayList.Play();
            }

        }

        private void playdata()
        {
            try
            {

                if (srcBuf == null)
                {
                    Audio.errMsg = "cancel";
                    return;
                }

                if (Audio.isPaused)
                {
                    Audio.Pause();
                }
                //stop();

                //for (int chipID = 0; chipID < 2; chipID++)
                //{
                //    for (int ch = 0; ch < 3; ch++) ForceChannelMask(EnmChip.AY8910, chipID, ch, newParam.ay8910[chipID].channels[ch].mask);
                //    for (int ch = 0; ch < 8; ch++) ForceChannelMask(EnmChip.YM2151, chipID, ch, newParam.ym2151[chipID].channels[ch].mask);
                //    for (int ch = 0; ch < 9; ch++) ForceChannelMask(EnmChip.YM2203, chipID, ch, newParam.ym2203[chipID].channels[ch].mask);
                //    for (int ch = 0; ch < 14; ch++) ForceChannelMask(EnmChip.YM2413, chipID, ch, newParam.ym2413[chipID].channels[ch].mask);
                //    for (int ch = 0; ch < 14; ch++) ForceChannelMask(EnmChip.YM2608, chipID, ch, newParam.ym2608[chipID].channels[ch].mask);
                //    for (int ch = 0; ch < 14; ch++) ForceChannelMask(EnmChip.YM2610, chipID, ch, newParam.ym2610[chipID].channels[ch].mask);
                //    for (int ch = 0; ch < 6; ch++) ForceChannelMask(EnmChip.YM2612, chipID, ch, newParam.ym2612[chipID].channels[ch].mask);
                //    for (int ch = 0; ch < 4; ch++) ForceChannelMask(EnmChip.SN76489, chipID, ch, newParam.sn76489[chipID].channels[ch].mask);
                //    for (int ch = 0; ch < 8; ch++) ForceChannelMask(EnmChip.RF5C164, chipID, ch, newParam.rf5c164[chipID].channels[ch].mask);
                //    for (int ch = 0; ch < 24; ch++) ForceChannelMask(EnmChip.C140, chipID, ch, newParam.c140[chipID].channels[ch].mask);
                //    for (int ch = 0; ch < 32; ch++) ForceChannelMask(EnmChip.C352, chipID, ch, newParam.c352[chipID].channels[ch].mask);
                //    for (int ch = 0; ch < 16; ch++) ForceChannelMask(EnmChip.SEGAPCM, chipID, ch, newParam.segaPcm[chipID].channels[ch].mask);
                //    for (int ch = 0; ch < 6; ch++) ForceChannelMask(EnmChip.HuC6280, chipID, ch, newParam.huc6280[chipID].channels[ch].mask); 
                //    for (int ch = 0; ch < 4; ch++) ForceChannelMask(EnmChip.OKIM6295, chipID, ch, newParam.okim6295[chipID].channels[ch].mask);
                //    for (int ch = 0; ch < 2; ch++) ResetChannelMask(EnmChip.NES, chipID, ch);
                //    for (int ch = 0; ch < 3; ch++) ResetChannelMask(EnmChip.DMC, chipID, ch);
                //    for (int ch = 0; ch < 3; ch++) ResetChannelMask(EnmChip.MMC5, chipID, ch);
                //    ResetChannelMask(EnmChip.FDS, chipID, 0);
                //}

                //oldParam = new MDChipParams();
                //newParam = new MDChipParams();
                reqAllScreenInit = true;

                if (setting.other.WavSwitch)
                {
                    if (!System.IO.Directory.Exists(setting.other.WavPath))
                    {
                        DialogResult res = MessageBox.Show(
                            "wavファイル出力先に設定されたパスが存在しません。作成し演奏を続けますか。"
                            , "パス作成確認"
                            , MessageBoxButtons.YesNo
                            , MessageBoxIcon.Information);
                        if (res == DialogResult.No)
                        {
                            Audio.errMsg = "cancel";
                            return;
                        }
                        try
                        {
                            Directory.CreateDirectory(setting.other.WavPath);
                        }
                        catch
                        {
                            MessageBox.Show(
                               "パスの作成に失敗しました。演奏を停止します。"
                               , "作成失敗"
                               , MessageBoxButtons.OK
                               , MessageBoxIcon.Error);
                            Audio.errMsg = "cancel";
                            return;
                        }
                    }
                }

                if (!Audio.Play(setting))
                {
                    try
                    {
                        frmPlayList.Stop();
                        Audio.Stop();
                    }
                    catch (Exception ex)
                    {
                        log.ForcedWrite(ex);
                    }
                    if (Audio.errMsg == "") throw new Exception();
                    else
                    {
                        MessageBox.Show(Audio.errMsg, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                for (int chipID = 0; chipID < 2; chipID++)
                {
                    for (int ch = 0; ch < 3; ch++) ForceChannelMask(EnmChip.AY8910, chipID, ch, newParam.ay8910[chipID].channels[ch].mask);
                    for (int ch = 0; ch < 8; ch++) ForceChannelMask(EnmChip.YM2151, chipID, ch, newParam.ym2151[chipID].channels[ch].mask);
                    for (int ch = 0; ch < 9; ch++) ForceChannelMask(EnmChip.YM2203, chipID, ch, newParam.ym2203[chipID].channels[ch].mask);
                    for (int ch = 0; ch < 14; ch++) ForceChannelMask(EnmChip.YM2413, chipID, ch, newParam.ym2413[chipID].channels[ch].mask);
                    for (int ch = 0; ch < 14; ch++) ForceChannelMask(EnmChip.YM2608, chipID, ch, newParam.ym2608[chipID].channels[ch].mask);
                    for (int ch = 0; ch < 14; ch++) ForceChannelMask(EnmChip.YM2610, chipID, ch, newParam.ym2610[chipID].channels[ch].mask);
                    for (int ch = 0; ch < 6; ch++) ForceChannelMask(EnmChip.YM2612, chipID, ch, newParam.ym2612[chipID].channels[ch].mask);
                    for (int ch = 0; ch < 4; ch++) ForceChannelMask(EnmChip.SN76489, chipID, ch, newParam.sn76489[chipID].channels[ch].mask);
                    for (int ch = 0; ch < 8; ch++) ForceChannelMask(EnmChip.RF5C164, chipID, ch, newParam.rf5c164[chipID].channels[ch].mask);
                    for (int ch = 0; ch < 24; ch++) ForceChannelMask(EnmChip.C140, chipID, ch, newParam.c140[chipID].channels[ch].mask);
                    for (int ch = 0; ch < 32; ch++) ForceChannelMask(EnmChip.C352, chipID, ch, newParam.c352[chipID].channels[ch].mask);
                    for (int ch = 0; ch < 16; ch++) ForceChannelMask(EnmChip.SEGAPCM, chipID, ch, newParam.segaPcm[chipID].channels[ch].mask);
                    for (int ch = 0; ch < 6; ch++) ForceChannelMask(EnmChip.HuC6280, chipID, ch, newParam.huc6280[chipID].channels[ch].mask);
                    for (int ch = 0; ch < 4; ch++) ForceChannelMask(EnmChip.OKIM6295, chipID, ch, newParam.okim6295[chipID].channels[ch].mask);
                    for (int ch = 0; ch < 2; ch++) ResetChannelMask(EnmChip.NES, chipID, ch);
                    for (int ch = 0; ch < 3; ch++) ResetChannelMask(EnmChip.DMC, chipID, ch);
                    for (int ch = 0; ch < 3; ch++) ResetChannelMask(EnmChip.MMC5, chipID, ch);
                    ResetChannelMask(EnmChip.FDS, chipID, 0);
                }

                if (frmInfo != null)
                {
                    frmInfo.update();
                }

                if (setting.other.AutoOpen)
                {

                    if (Audio.chipLED.PriOPM != 0) OpenFormYM2151(0, true); else CloseFormYM2151(0);
                    if (Audio.chipLED.SecOPM != 0) OpenFormYM2151(1, true); else CloseFormYM2151(1);

                    if (Audio.chipLED.PriOPN != 0) OpenFormYM2203(0, true); else CloseFormYM2203(0);
                    if (Audio.chipLED.SecOPN != 0) OpenFormYM2203(1, true); else CloseFormYM2203(1);

                    if (Audio.chipLED.PriOPLL != 0) OpenFormYM2413(0, true); else CloseFormYM2413(0);
                    if (Audio.chipLED.SecOPLL != 0) OpenFormYM2413(1, true); else CloseFormYM2413(1);

                    if (Audio.chipLED.PriOPNA != 0) OpenFormYM2608(0, true); else CloseFormYM2608(0);
                    if (Audio.chipLED.SecOPNA != 0) OpenFormYM2608(1, true); else CloseFormYM2608(1);

                    if (Audio.chipLED.PriOPNB != 0) OpenFormYM2610(0, true); else CloseFormYM2610(0);
                    if (Audio.chipLED.SecOPNB != 0) OpenFormYM2610(1, true); else CloseFormYM2610(1);

                    if (Audio.chipLED.PriOPN2 != 0) OpenFormYM2612(0, true); else CloseFormYM2612(0);
                    if (Audio.chipLED.SecOPN2 != 0) OpenFormYM2612(1, true); else CloseFormYM2612(1);

                    if (Audio.chipLED.PriDCSG != 0) OpenFormSN76489(0, true); else CloseFormSN76489(0);
                    if (Audio.chipLED.SecDCSG != 0) OpenFormSN76489(1, true); else CloseFormSN76489(1);

                    if (Audio.chipLED.PriRF5C != 0) OpenFormMegaCD(0, true); else CloseFormMegaCD(0);
                    if (Audio.chipLED.SecRF5C != 0) OpenFormMegaCD(1, true); else CloseFormMegaCD(1);

                    if (Audio.chipLED.PriOKI5 != 0) OpenFormOKIM6258(0, true); else CloseFormOKIM6258(0);
                    if (Audio.chipLED.SecOKI5 != 0) OpenFormOKIM6258(1, true); else CloseFormOKIM6258(1);

                    if (Audio.chipLED.PriOKI9 != 0) OpenFormOKIM6295(0, true); else CloseFormOKIM6295(0);
                    if (Audio.chipLED.SecOKI9 != 0) OpenFormOKIM6295(1, true); else CloseFormOKIM6295(1);

                    if (Audio.chipLED.PriC140 != 0) OpenFormC140(0, true); else CloseFormC140(0);
                    if (Audio.chipLED.SecC140 != 0) OpenFormC140(1, true); else CloseFormC140(1);

                    if (Audio.chipLED.PriYMZ != 0) OpenFormYMZ280B(0, true); else CloseFormYMZ280B(0);
                    if (Audio.chipLED.SecYMZ != 0) OpenFormYMZ280B(1, true); else CloseFormYMZ280B(1);

                    if (Audio.chipLED.PriC352 != 0) OpenFormC352(0, true); else CloseFormC352(0);
                    if (Audio.chipLED.SecC352 != 0) OpenFormC352(1, true); else CloseFormC352(1);

                    if (Audio.chipLED.PriMPCM != 0) OpenFormMultiPCM(0, true); else CloseFormMultiPCM(0);
                    if (Audio.chipLED.SecMPCM != 0) OpenFormMultiPCM(1, true); else CloseFormMultiPCM(1);

                    if (Audio.chipLED.PriQsnd != 0) OpenFormQSound(0, true); else CloseFormQSound(0);
                    //if (Audio.chipLED.SecQsnd != 0) OpenFormQSound(1, true); else CloseFormQSound(1);

                    if (Audio.chipLED.PriSPCM != 0) OpenFormSegaPCM(0, true); else CloseFormSegaPCM(0);
                    if (Audio.chipLED.SecSPCM != 0) OpenFormSegaPCM(1, true); else CloseFormSegaPCM(1);

                    if (Audio.chipLED.PriAY10 != 0) OpenFormAY8910(0, true); else CloseFormAY8910(0);
                    if (Audio.chipLED.SecAY10 != 0) OpenFormAY8910(1, true); else CloseFormAY8910(1);

                    if (Audio.chipLED.PriHuC != 0) OpenFormHuC6280(0, true); else CloseFormHuC6280(0);
                    if (Audio.chipLED.SecHuC != 0) OpenFormHuC6280(1, true); else CloseFormHuC6280(1);

                    if (Audio.chipLED.PriK051649 != 0) OpenFormK051649(0, true); else CloseFormK051649(0);
                    if (Audio.chipLED.SecK051649 != 0) OpenFormK051649(1, true); else CloseFormK051649(1);

                    if (Audio.chipLED.PriMID != 0) OpenFormMIDI(0, true); else CloseFormMIDI(0);
                    if (Audio.chipLED.SecMID != 0) OpenFormMIDI(1, true); else CloseFormMIDI(1);

                    if (Audio.chipLED.PriNES != 0 || Audio.chipLED.PriDMC != 0) OpenFormNESDMC(0, true); else CloseFormNESDMC(0);
                    if (Audio.chipLED.SecNES != 0 || Audio.chipLED.SecDMC != 0) OpenFormNESDMC(1, true); else CloseFormNESDMC(1);

                    if (Audio.chipLED.PriFDS != 0) OpenFormFDS(0, true); else CloseFormFDS(0);
                    if (Audio.chipLED.SecFDS != 0) OpenFormFDS(1, true); else CloseFormFDS(1);

                    if (Audio.chipLED.PriVRC7 != 0) OpenFormVRC7(0, true); else CloseFormVRC7(0);
                    if (Audio.chipLED.SecVRC7 != 0) OpenFormVRC7(1, true); else CloseFormVRC7(1);

                    if (Audio.chipLED.PriMMC5 != 0) OpenFormMMC5(0, true); else CloseFormMMC5(0);
                    if (Audio.chipLED.SecMMC5 != 0) OpenFormMMC5(1, true); else CloseFormMMC5(1);

                    if (Audio.chipLED.PriOPL != 0) OpenFormYM3526(0, true); else CloseFormYM3526(0);
                    if (Audio.chipLED.SecOPL != 0) OpenFormYM3526(1, true); else CloseFormYM3526(1);

                    if (Audio.chipLED.PriY8950 != 0) OpenFormY8950(0, true); else CloseFormY8950(0);
                    if (Audio.chipLED.SecY8950 != 0) OpenFormY8950(1, true); else CloseFormY8950(1);

                    if (Audio.chipLED.PriOPL2 != 0) OpenFormYM3812(0, true); else CloseFormYM3812(0);
                    if (Audio.chipLED.SecOPL2 != 0) OpenFormYM3812(1, true); else CloseFormYM3812(1);

                    if (Audio.chipLED.PriOPL3 != 0) OpenFormYMF262(0, true); else CloseFormYMF262(0);
                    if (Audio.chipLED.SecOPL3 != 0) OpenFormYMF262(1, true); else CloseFormYMF262(1);

                    if (Audio.chipLED.PriOPL4 != 0) OpenFormYMF278B(0, true); else CloseFormYMF278B(0);
                    if (Audio.chipLED.SecOPL4 != 0) OpenFormYMF278B(1, true); else CloseFormYMF278B(1);

                }
            }
            catch(Exception e)
            {
                Audio.errMsg = e.Message;
            }
        }

        public void ff()
        {
            if (Audio.isPaused)
            {
                Audio.Pause();
            }

            Audio.FF();
        }

        public void next()
        {
            if (Audio.isPaused)
            {
                Audio.Pause();
            }
            Audio.Stop();
            screenInit();

            frmPlayList.nextPlay();
        }

        private void nextPlayMode()
        {
            frmPlayList.nextPlayMode(newButtonMode[9]);
        }

        public void slow()
        {
            if (Audio.isPaused)
            {
                Audio.StepPlay(4000);
                Audio.Pause();
                return;
            }

            if (Audio.isStopped)
            {
                play();
            }

            Audio.Slow();
        }

        private void playMode()
        {
            newButtonMode[9]++;
            if (newButtonMode[9] > 3) newButtonMode[9] = 0;

        }

        private string[] fileOpen(bool flg)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = Properties.Resources.cntSupportFile.Replace("\r\n", "");
            ofd.Title = "ファイルを選択してください";
            ofd.FilterIndex = setting.other.FilterIndex;

            if (setting.other.DefaultDataPath != "" && Directory.Exists(setting.other.DefaultDataPath) && IsInitialOpenFolder)
            {
                ofd.InitialDirectory = setting.other.DefaultDataPath;
            }
            else
            {
                ofd.RestoreDirectory = true;
            }
            ofd.CheckPathExists = true;
            ofd.Multiselect = flg;

            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return null;
            }

            IsInitialOpenFolder = false;
            setting.other.FilterIndex = ofd.FilterIndex;

            return ofd.FileNames;

        }

        private void dispPlayList()
        {
            frmPlayList.setting = setting;
            //if (setting.location.PPlayList != System.Drawing.Point.Empty)
            //{
            //    frmPlayList.Location = setting.location.PPlayList;

            //}
            //if (setting.location.PPlayListWH != System.Drawing.Point.Empty)
            //{
            //    frmPlayList.Width = setting.location.PPlayListWH.X;
            //    frmPlayList.Height = setting.location.PPlayListWH.Y;
            //}
            frmPlayList.Visible = !frmPlayList.Visible;
            if(frmPlayList.Visible) CheckAndSetForm(frmPlayList);
            frmPlayList.TopMost = true;
            frmPlayList.TopMost = false;
        }

        private void dispVSTList()
        {
            frmVSTeffectList.Visible = !frmVSTeffectList.Visible;
            if (frmVSTeffectList.Visible) CheckAndSetForm(frmVSTeffectList);
            frmVSTeffectList.TopMost = true;
            frmVSTeffectList.TopMost = false;
        }

        private void showContextMenu()
        {
            cmsOpenOtherPanel.Show();
            System.Drawing.Point p = Control.MousePosition;
            cmsOpenOtherPanel.Top = p.Y;
            cmsOpenOtherPanel.Left = p.X;
        }



        public const int FCC_VGM = 0x206D6756;	// "Vgm "

        public byte[] getAllBytes(string filename, out EnmFileFormat format)
        {
            format = EnmFileFormat.unknown;

            //先ずは丸ごと読み込む
            byte[] buf = System.IO.File.ReadAllBytes(filename);

            string ext = Path.GetExtension(filename).ToLower();

            //.NRDファイルの場合は拡張子判定
            if (ext==".nrd")
            {
                format = EnmFileFormat.NRT;
                return buf;
            }

            if (ext == ".mdr")
            {
                format = EnmFileFormat.MDR;
                return buf;
            }

            if (ext == ".mdx")
            {
                format = EnmFileFormat.MDX;
                return buf;
            }

            if (ext == ".mnd")
            {
                format = EnmFileFormat.MND;
                return buf;
            }

            if (ext == ".mub")
            {
                format = EnmFileFormat.MUB;
                return buf;
            }

            if (ext == ".muc")
            {
                format = EnmFileFormat.MUC;
                return buf;
            }

            if (ext == ".xgm")
            {
                format = EnmFileFormat.XGM;
                return buf;
            }

            if (ext == ".zgm")
            {
                format = EnmFileFormat.ZGM;
                return buf;
            }

            if (ext == ".s98")
            {
                format = EnmFileFormat.S98;
                return buf;
            }

            if (ext == ".nsf")
            {
                format = EnmFileFormat.NSF;
                return buf;
            }

            if (ext == ".hes")
            {
                format = EnmFileFormat.HES;
                return buf;
            }

            if (ext == ".sid")
            {
                format = EnmFileFormat.SID;
                return buf;
            }

            if (ext == ".mid")
            {
                format = EnmFileFormat.MID;
                return buf;
            }

            if (ext == ".rcp")
            {
                format = EnmFileFormat.RCP;
                return buf;
            }


            //.VGMの場合はヘッダの確認とGzipで解凍後のファイルのヘッダの確認
            uint vgm = (UInt32)buf[0] + (UInt32)buf[1] * 0x100 + (UInt32)buf[2] * 0x10000 + (UInt32)buf[3] * 0x1000000;
            if (vgm == FCC_VGM)
            {
                format = EnmFileFormat.VGM;
                return buf;
            }

            int num;
            buf = new byte[1024]; // 1Kbytesずつ処理する

            FileStream inStream // 入力ストリーム
              = new FileStream(filename, FileMode.Open, FileAccess.Read);

            GZipStream decompStream // 解凍ストリーム
              = new GZipStream(
                inStream, // 入力元となるストリームを指定
                CompressionMode.Decompress); // 解凍（圧縮解除）を指定

            MemoryStream outStream // 出力ストリーム
              = new MemoryStream();

            using (inStream)
            using (outStream)
            using (decompStream)
            {
                while ((num = decompStream.Read(buf, 0, buf.Length)) > 0)
                {
                    outStream.Write(buf, 0, num);
                }
            }

            format = EnmFileFormat.VGM;
            return outStream.ToArray();
        }

        public void getInstCh(EnmChip chip, int ch, int chipID)
        {
            if (chip == EnmChip.YM2413)
            {
                getInstChForMGSC(chip, ch, chipID);
                return;
            }
            if (chip == EnmChip.VRC7)
            {
                getInstChForMGSC(chip, ch, chipID);
                return;
            }

            YM2612MIDI.SetVoiceFromChipRegister(chip, chipID, ch);

            if (!setting.other.UseGetInst) return;

            switch (setting.other.InstFormat)
            {
                case EnmInstFormat.FMP7:
                    getInstChForFMP7(chip, ch, chipID);
                    break;
                case EnmInstFormat.MDX:
                    getInstChForMDX(chip, ch, chipID);
                    break;
                case EnmInstFormat.MML2VGM:
                    getInstChForMML2VGM(chip, ch, chipID);
                    break;
                case EnmInstFormat.MUCOM88:
                    getInstChForMucom88(chip, ch, chipID);
                    break;
                case EnmInstFormat.MUSICLALF:
                    getInstChForMUSICLALF(chip, ch, chipID);
                    break;
                case EnmInstFormat.MUSICLALF2:
                    getInstChForMUSICLALF2(chip, ch, chipID);
                    break;
                case EnmInstFormat.TFI:
                    getInstChForTFI(chip, ch, chipID);
                    break;
                case EnmInstFormat.NRTDRV:
                    getInstChForNRTDRV(chip, ch, chipID);
                    break;
                case EnmInstFormat.HUSIC:
                    getInstChForHuSIC(chip, ch, chipID);
                    break;
                case EnmInstFormat.VOPM:
                    getInstChForVOPM(chip, ch, chipID);
                    break;
                case EnmInstFormat.PMD:
                    getInstChForPMD(chip, ch, chipID);
                    break;
            }
        }

        private void getInstChForFMP7(EnmChip chip, int ch, int chipID)
        {

            string n = "";

            if (chip == EnmChip.YM2612 || chip == EnmChip.YM2608 || chip == EnmChip.YM2203 || chip == EnmChip.YM2610)
            {
                int p = (ch > 2) ? 1 : 0;
                int c = (ch > 2) ? ch - 3 : ch;
                int[][] fmRegister = (chip == EnmChip.YM2612) ? Audio.GetFMRegister(chipID) : (chip == EnmChip.YM2608 ? Audio.GetYM2608Register(chipID) : (chip == EnmChip.YM2203 ? new int[][] { Audio.GetYM2203Register(chipID), null } : Audio.GetYM2610Register(chipID)));

                n = "'@ FA xx\r\n   AR  DR  SR  RR  SL  TL  KS  ML  DT  AM\r\n";

                for (int i = 0; i < 4; i++)
                {
                    int ops = (i == 0) ? 0 : ((i == 1) ? 8 : ((i == 2) ? 4 : 12));
                    n += string.Format("'@ {0:D3},{1:D3},{2:D3},{3:D3},{4:D3},{5:D3},{6:D3},{7:D3},{8:D3},{9:D3}\r\n"
                        , fmRegister[p][0x50 + ops + c] & 0x1f //AR
                        , fmRegister[p][0x60 + ops + c] & 0x1f //DR
                        , fmRegister[p][0x70 + ops + c] & 0x1f //SR
                        , fmRegister[p][0x80 + ops + c] & 0x0f //RR
                        , (fmRegister[p][0x80 + ops + c] & 0xf0) >> 4//SL
                        , fmRegister[p][0x40 + ops + c] & 0x7f//TL
                        , (fmRegister[p][0x50 + ops + c] & 0xc0) >> 6//KS
                        , fmRegister[p][0x30 + ops + c] & 0x0f//ML
                        , (fmRegister[p][0x30 + ops + c] & 0x70) >> 4//DT
                        , (fmRegister[p][0x60 + ops + c] & 0x80) >> 7//AM
                    );
                }
                n += "   ALG FB\r\n";
                n += string.Format("'@ {0:D3},{1:D3}\r\n"
                    , fmRegister[p][0xb0 + c] & 0x07//AL
                    , (fmRegister[p][0xb0 + c] & 0x38) >> 3//FB
                );
            }
            else if (chip == EnmChip.YM2151)
            {
                int[] ym2151Register = Audio.GetYM2151Register(chipID);
                n = "'@ FC xx\r\n   AR  DR  SR  RR  SL  TL  KS  ML  DT1 DT2 AM\r\n";

                for (int i = 0; i < 4; i++)
                {
                    int ops = (i == 0) ? 0 : ((i == 1) ? 16 : ((i == 2) ? 8 : 24));
                    n += string.Format("'@ {0:D3},{1:D3},{2:D3},{3:D3},{4:D3},{5:D3},{6:D3},{7:D3},{8:D3},{9:D3},{10:D3}\r\n"
                        , ym2151Register[0x80 + ops + ch] & 0x1f //AR
                        , ym2151Register[0xa0 + ops + ch] & 0x1f //DR
                        , ym2151Register[0xc0 + ops + ch] & 0x1f //SR
                        , ym2151Register[0xe0 + ops + ch] & 0x0f //RR
                        , (ym2151Register[0xe0 + ops + ch] & 0xf0) >> 4 //SL
                        , ym2151Register[0x60 + ops + ch] & 0x7f //TL
                        , (ym2151Register[0x80 + ops + ch] & 0xc0) >> 6 //KS
                        , ym2151Register[0x40 + ops + ch] & 0x0f //ML
                        , (ym2151Register[0x40 + ops + ch] & 0x70) >> 4 //DT
                        , (ym2151Register[0xc0 + ops + ch] & 0xc0) >> 6 //DT2
                        , (ym2151Register[0xa0 + ops + ch] & 0x80) >> 7 //AM
                    );
                }
                n += "   ALG FB\r\n";
                n += string.Format("'@ {0:D3},{1:D3}\r\n"
                    , ym2151Register[0x20 + ch] & 0x07 //AL
                    , (ym2151Register[0x20 + ch] & 0x38) >> 3//FB
                );
            }

            if (!string.IsNullOrEmpty(n)) Clipboard.SetText(n);
        }

        private void getInstChForMDX(EnmChip chip, int ch, int chipID)
        {

            string n = "";

            if (chip == EnmChip.YM2612 || chip == EnmChip.YM2608 || chip == EnmChip.YM2203 || chip == EnmChip.YM2610)
            {
                int p = (ch > 2) ? 1 : 0;
                int c = (ch > 2) ? ch - 3 : ch;
                int[][] fmRegister = (chip == EnmChip.YM2612) ? Audio.GetFMRegister(chipID) : (chip == EnmChip.YM2608 ? Audio.GetYM2608Register(chipID) : (chip == EnmChip.YM2203 ? new int[][] { Audio.GetYM2203Register(chipID), null } : Audio.GetYM2610Register(chipID)));

                n = "'@xx = {\r\n/* AR  DR  SR  RR  SL  TL  KS  ML  DT1 DT2 AME\r\n";

                for (int i = 0; i < 4; i++)
                {
                    int ops = (i == 0) ? 0 : ((i == 1) ? 8 : ((i == 2) ? 4 : 12));
                    n += string.Format("   {0:D3},{1:D3},{2:D3},{3:D3},{4:D3},{5:D3},{6:D3},{7:D3},{8:D3},{9:D3},{10:D3}\r\n"
                        , fmRegister[p][0x50 + ops + c] & 0x1f //AR
                        , fmRegister[p][0x60 + ops + c] & 0x1f //DR
                        , fmRegister[p][0x70 + ops + c] & 0x1f //SR
                        , fmRegister[p][0x80 + ops + c] & 0x0f //RR
                        , (fmRegister[p][0x80 + ops + c] & 0xf0) >> 4//SL
                        , fmRegister[p][0x40 + ops + c] & 0x7f//TL
                        , (fmRegister[p][0x50 + ops + c] & 0xc0) >> 6//KS
                        , fmRegister[p][0x30 + ops + c] & 0x0f//ML
                        , (fmRegister[p][0x30 + ops + c] & 0x70) >> 4//DT
                        , 0
                        , (fmRegister[p][0x60 + ops + c] & 0x80) >> 7//AM
                    );
                }
                n += "/* ALG FB  OP\r\n";
                n += string.Format("   {0:D3},{1:D3},15\r\n}}\r\n"
                    , fmRegister[p][0xb0 + c] & 0x07//AL
                    , (fmRegister[p][0xb0 + c] & 0x38) >> 3//FB
                );
            }
            else if (chip == EnmChip.YM2151)
            {
                int[] ym2151Register = Audio.GetYM2151Register(chipID);

                n = "'@xx = {\r\n/* AR  DR  SR  RR  SL  TL  KS  ML  DT1 DT2 AME\r\n";

                for (int i = 0; i < 4; i++)
                {
                    int ops = (i == 0) ? 0 : ((i == 1) ? 16 : ((i == 2) ? 8 : 24));
                    n += string.Format("   {0:D3},{1:D3},{2:D3},{3:D3},{4:D3},{5:D3},{6:D3},{7:D3},{8:D3},{9:D3},{10:D3}\r\n"
                        , ym2151Register[0x80 + ops + ch] & 0x1f //AR
                        , ym2151Register[0xa0 + ops + ch] & 0x1f //DR
                        , ym2151Register[0xc0 + ops + ch] & 0x1f //SR
                        , ym2151Register[0xe0 + ops + ch] & 0x0f //RR
                        , (ym2151Register[0xe0 + ops + ch] & 0xf0) >> 4 //SL
                        , ym2151Register[0x60 + ops + ch] & 0x7f //TL
                        , (ym2151Register[0x80 + ops + ch] & 0xc0) >> 6 //KS
                        , ym2151Register[0x40 + ops + ch] & 0x0f //ML
                        , (ym2151Register[0x40 + ops + ch] & 0x70) >> 4 //DT
                        , (ym2151Register[0xc0 + ops + ch] & 0xc0) >> 6 //DT2
                        , (ym2151Register[0xa0 + ops + ch] & 0x80) >> 7 //AM
                    );
                }
                n += "/* ALG FB  OP\r\n";
                n += string.Format("   {0:D3},{1:D3},15\r\n}}\r\n"
                    , ym2151Register[0x20 + ch] & 0x07 //AL
                    , (ym2151Register[0x20 + ch] & 0x38) >> 3//FB
                );
            }

            if (!string.IsNullOrEmpty(n)) Clipboard.SetText(n);
        }

        private void getInstChForMML2VGM(EnmChip chip, int ch, int chipID)
        {

            string n = "";

            if (chip == EnmChip.YM2612 || chip == EnmChip.YM2608 || chip == EnmChip.YM2203 || chip == EnmChip.YM2610)
            {
                int p = (ch > 2) ? 1 : 0;
                int c = (ch > 2) ? ch - 3 : ch;
                int[][] fmRegister = (chip == EnmChip.YM2612) ? Audio.GetFMRegister(chipID) : (chip == EnmChip.YM2608 ? Audio.GetYM2608Register(chipID) : (chip == EnmChip.YM2203 ? new int[][] { Audio.GetYM2203Register(chipID), null } : Audio.GetYM2610Register(chipID)));

                n = "'@ N xx\r\n   AR  DR  SR  RR  SL  TL  KS  ML  DT  AM  SSG-EG\r\n";

                for (int i = 0; i < 4; i++)
                {
                    int ops = (i == 0) ? 0 : ((i == 1) ? 8 : ((i == 2) ? 4 : 12));
                    n += string.Format("'@ {0:D3},{1:D3},{2:D3},{3:D3},{4:D3},{5:D3},{6:D3},{7:D3},{8:D3},{9:D3},{10:D3}\r\n"
                        , fmRegister[p][0x50 + ops + c] & 0x1f //AR
                        , fmRegister[p][0x60 + ops + c] & 0x1f //DR
                        , fmRegister[p][0x70 + ops + c] & 0x1f //SR
                        , fmRegister[p][0x80 + ops + c] & 0x0f //RR
                        , (fmRegister[p][0x80 + ops + c] & 0xf0) >> 4//SL
                        , fmRegister[p][0x40 + ops + c] & 0x7f//TL
                        , (fmRegister[p][0x50 + ops + c] & 0xc0) >> 6//KS
                        , fmRegister[p][0x30 + ops + c] & 0x0f//ML
                        , (fmRegister[p][0x30 + ops + c] & 0x70) >> 4//DT
                        , (fmRegister[p][0x60 + ops + c] & 0x80) >> 7//AM
                        , fmRegister[p][0x90 + ops + c] & 0x0f//SG
                    );
                }
                n += "   ALG FB\r\n";
                n += string.Format("'@ {0:D3},{1:D3}\r\n"
                    , fmRegister[p][0xb0 + c] & 0x07//AL
                    , (fmRegister[p][0xb0 + c] & 0x38) >> 3//FB
                );
            }
            else if (chip == EnmChip.YM2151)
            {
                int[] ym2151Register = Audio.GetYM2151Register(chipID);
                n = "'@ M xx\r\n   AR  DR  SR  RR  SL  TL  KS  ML  DT1 DT2 AME\r\n";

                for (int i = 0; i < 4; i++)
                {
                    int ops = (i == 0) ? 0 : ((i == 1) ? 16 : ((i == 2) ? 8 : 24));
                    n += string.Format("'@ {0:D3},{1:D3},{2:D3},{3:D3},{4:D3},{5:D3},{6:D3},{7:D3},{8:D3},{9:D3},{10:D3}\r\n"
                        , ym2151Register[0x80 + ops + ch] & 0x1f //AR
                        , ym2151Register[0xa0 + ops + ch] & 0x1f //DR
                        , ym2151Register[0xc0 + ops + ch] & 0x1f //SR
                        , ym2151Register[0xe0 + ops + ch] & 0x0f //RR
                        , (ym2151Register[0xe0 + ops + ch] & 0xf0) >> 4 //SL
                        , ym2151Register[0x60 + ops + ch] & 0x7f //TL
                        , (ym2151Register[0x80 + ops + ch] & 0xc0) >> 6 //KS
                        , ym2151Register[0x40 + ops + ch] & 0x0f //ML
                        , (ym2151Register[0x40 + ops + ch] & 0x70) >> 4 //DT1
                        , (ym2151Register[0xc0 + ops + ch] & 0xc0) >> 6 //DT2
                        , (ym2151Register[0xa0 + ops + ch] & 0x80) >> 7 //AM
                    );
                }
                n += "   ALG FB\r\n";
                n += string.Format("'@ {0:D3},{1:D3}\r\n"
                    , ym2151Register[0x20 + ch] & 0x07 //AL
                    , (ym2151Register[0x20 + ch] & 0x38) >> 3//FB
                );
            }
            else if (chip == EnmChip.HuC6280)
            {
                MDSound.Ootake_PSG.huc6280_state huc6280Register = Audio.GetHuC6280Register(chipID);
                if (huc6280Register == null) return;
                MDSound.Ootake_PSG.PSG psg = huc6280Register.Psg[ch];
                if (psg == null) return;
                if (psg.wave == null) return;
                if (psg.wave.Length != 32) return;

                n = "'@ H xx,\r\n   +0 +1 +2 +3 +4 +5 +6 +7\r\n";

                for (int i = 0; i < 32; i += 8)
                {
                    n += string.Format("'@ {0:D2},{1:D2},{2:D2},{3:D2},{4:D2},{5:D2},{6:D2},{7:D2}\r\n"
                        , (17 - psg.wave[i + 0])
                        , (17 - psg.wave[i + 1])
                        , (17 - psg.wave[i + 2])
                        , (17 - psg.wave[i + 3])
                        , (17 - psg.wave[i + 4])
                        , (17 - psg.wave[i + 5])
                        , (17 - psg.wave[i + 6])
                        , (17 - psg.wave[i + 7])
                        );
                }
            }

            if (!string.IsNullOrEmpty(n)) Clipboard.SetText(n);
        }

        private void getInstChForMUSICLALF(EnmChip chip, int ch, int chipID)
        {

            string n = "";

            if (chip == EnmChip.YM2612 || chip == EnmChip.YM2608 || chip == EnmChip.YM2203 || chip == EnmChip.YM2610)
            {
                int p = (ch > 2) ? 1 : 0;
                int c = (ch > 2) ? ch - 3 : ch;
                int[][] fmRegister = (chip == EnmChip.YM2612) ? Audio.GetFMRegister(chipID) : (chip == EnmChip.YM2608 ? Audio.GetYM2608Register(chipID) : (chip == EnmChip.YM2203 ? new int[][] { Audio.GetYM2203Register(chipID), null } : Audio.GetYM2610Register(chipID)));

                n = string.Format("  @xx:{{\r\n  {0:D3} {1:D3}\r\n"
                    , (fmRegister[p][0xb0 + c] & 0x38) >> 3//FB
                    , fmRegister[p][0xb0 + c] & 0x07//AL
                    );

                for (int i = 0; i < 4; i++)
                {
                    int ops = (i == 0) ? 0 : ((i == 1) ? 8 : ((i == 2) ? 4 : 12));
                    n += string.Format("  {0:D3} {1:D3} {2:D3} {3:D3} {4:D3} {5:D3} {6:D3} {7:D3} {8:D3}\r\n"
                        , fmRegister[p][0x50 + ops + c] & 0x1f //AR
                        , fmRegister[p][0x60 + ops + c] & 0x1f //DR
                        , fmRegister[p][0x70 + ops + c] & 0x1f //SR
                        , fmRegister[p][0x80 + ops + c] & 0x0f //RR
                        , (fmRegister[p][0x80 + ops + c] & 0xf0) >> 4//SL
                        , fmRegister[p][0x40 + ops + c] & 0x7f//TL
                        , (fmRegister[p][0x50 + ops + c] & 0xc0) >> 6//KS
                        , fmRegister[p][0x30 + ops + c] & 0x0f//ML
                        , (fmRegister[p][0x30 + ops + c] & 0x70) >> 4//DT
                    );
                }
                n += "  }\r\n";
            }
            else if (chip == EnmChip.YM2151)
            {
                int[] ym2151Register = Audio.GetYM2151Register(chipID);

                n = string.Format("@xx:{{\r\n  {0:D3} {1:D3}\r\n"
                    , ym2151Register[0x20 + ch] & 0x07 //AL
                    , (ym2151Register[0x20 + ch] & 0x38) >> 3//FB
                    );

                for (int i = 0; i < 4; i++)
                {
                    int ops = (i == 0) ? 0 : ((i == 1) ? 16 : ((i == 2) ? 8 : 24));
                    n += string.Format("  {0:D3} {1:D3} {2:D3} {3:D3} {4:D3} {5:D3} {6:D3} {7:D3} {8:D3}\r\n"
                        , ym2151Register[0x80 + ops + ch] & 0x1f //AR
                        , ym2151Register[0xa0 + ops + ch] & 0x1f //DR
                        , ym2151Register[0xc0 + ops + ch] & 0x1f //SR
                        , ym2151Register[0xe0 + ops + ch] & 0x0f //RR
                        , (ym2151Register[0xe0 + ops + ch] & 0xf0) >> 4 //SL
                        , ym2151Register[0x60 + ops + ch] & 0x7f //TL
                        , (ym2151Register[0x80 + ops + ch] & 0xc0) >> 6 //KS
                        , ym2151Register[0x40 + ops + ch] & 0x0f //ML
                        , (ym2151Register[0x40 + ops + ch] & 0x70) >> 4 //DT
                    );
                }
                n += "  }\r\n";
            }

            if (!string.IsNullOrEmpty(n)) Clipboard.SetText(n);
        }

        private void getInstChForMucom88(EnmChip chip, int ch, int chipID)
        {

            string n = "";

            if (chip == EnmChip.YM2612 || chip == EnmChip.YM2608 || chip == EnmChip.YM2203 || chip == EnmChip.YM2610)
            {
                int p = (ch > 2) ? 1 : 0;
                int c = (ch > 2) ? ch - 3 : ch;
                int[][] fmRegister = (chip == EnmChip.YM2612) ? Audio.GetFMRegister(chipID) : (chip == EnmChip.YM2608 ? Audio.GetYM2608Register(chipID) : (chip == EnmChip.YM2203 ? new int[][] { Audio.GetYM2203Register(chipID), null } : Audio.GetYM2610Register(chipID)));

                n = string.Format("  @xx:{{\r\n  {0:D3}, {1:D3}\r\n"
                    , (fmRegister[p][0xb0 + c] & 0x38) >> 3//FB
                    , fmRegister[p][0xb0 + c] & 0x07//AL
                    );

                for (int i = 0; i < 4; i++)
                {
                    int ops = (i == 0) ? 0 : ((i == 1) ? 8 : ((i == 2) ? 4 : 12));
                    n += string.Format("  {0:D3}, {1:D3}, {2:D3}, {3:D3}, {4:D3}, {5:D3}, {6:D3}, {7:D3}, {8:D3}" + (i != 3 ? "\r\n" : "")
                        , fmRegister[p][0x50 + ops + c] & 0x1f //AR
                        , fmRegister[p][0x60 + ops + c] & 0x1f //DR
                        , fmRegister[p][0x70 + ops + c] & 0x1f //SR
                        , fmRegister[p][0x80 + ops + c] & 0x0f //RR
                        , (fmRegister[p][0x80 + ops + c] & 0xf0) >> 4//SL
                        , fmRegister[p][0x40 + ops + c] & 0x7f//TL
                        , (fmRegister[p][0x50 + ops + c] & 0xc0) >> 6//KS
                        , fmRegister[p][0x30 + ops + c] & 0x0f//ML
                        , (fmRegister[p][0x30 + ops + c] & 0x70) >> 4//DT
                    );
                }
                n += ",\"MDP\"  }\r\n";
            }
            else if (chip == EnmChip.YM2151)
            {
                int[] ym2151Register = Audio.GetYM2151Register(chipID);

                n = string.Format("@xx:{{\r\n  {0:D3}, {1:D3}\r\n"
                    , ym2151Register[0x20 + ch] & 0x07 //AL
                    , (ym2151Register[0x20 + ch] & 0x38) >> 3//FB
                    );

                for (int i = 0; i < 4; i++)
                {
                    int ops = (i == 0) ? 0 : ((i == 1) ? 16 : ((i == 2) ? 8 : 24));
                    n += string.Format("  {0:D3}, {1:D3}, {2:D3}, {3:D3}, {4:D3}, {5:D3}, {6:D3}, {7:D3}, {8:D3}" + (i != 3 ? "\r\n" : "")
                        , ym2151Register[0x80 + ops + ch] & 0x1f //AR
                        , ym2151Register[0xa0 + ops + ch] & 0x1f //DR
                        , ym2151Register[0xc0 + ops + ch] & 0x1f //SR
                        , ym2151Register[0xe0 + ops + ch] & 0x0f //RR
                        , (ym2151Register[0xe0 + ops + ch] & 0xf0) >> 4 //SL
                        , ym2151Register[0x60 + ops + ch] & 0x7f //TL
                        , (ym2151Register[0x80 + ops + ch] & 0xc0) >> 6 //KS
                        , ym2151Register[0x40 + ops + ch] & 0x0f //ML
                        , (ym2151Register[0x40 + ops + ch] & 0x70) >> 4 //DT
                    );
                }
                n += ",\"MDP\"  }\r\n";
            }

            if (!string.IsNullOrEmpty(n)) Clipboard.SetText(n);
        }

        private void getInstChForMUSICLALF2(EnmChip chip, int ch, int chipID)
        {

            string n = "";

            if (chip == EnmChip.YM2612 || chip == EnmChip.YM2608 || chip == EnmChip.YM2203 || chip == EnmChip.YM2610)
            {
                int p = (ch > 2) ? 1 : 0;
                int c = (ch > 2) ? ch - 3 : ch;
                int[][] fmRegister = (chip == EnmChip.YM2612) ? Audio.GetFMRegister(chipID) : (chip == EnmChip.YM2608 ? Audio.GetYM2608Register(chipID) : (chip == EnmChip.YM2203 ? new int[][] { Audio.GetYM2203Register(chipID), null } : Audio.GetYM2610Register(chipID)));

                n = "@%xxx\r\n";

                for (int i = 0; i < 6; i++)
                {
                    n += string.Format("${0:X3},${1:X3},${2:X3},${3:X3}\r\n"
                        , fmRegister[p][0x30 + 0 + c + i * 0x10] & 0xff
                        , fmRegister[p][0x30 + 8 + c + i * 0x10] & 0xff
                        , fmRegister[p][0x30 + 16 + c + i * 0x10] & 0xff
                        , fmRegister[p][0x30 + 24 + c + i * 0x10] & 0xff
                    );
                }
                n += string.Format("${0:X3}\r\n"
                    , fmRegister[p][0xb0 + c] //FB/AL
                    );
            }
            else if (chip == EnmChip.YM2151)
            {
                int[] ym2151Register = Audio.GetYM2151Register(chipID);

                n = "@%xxx\r\n";

                n += string.Format("${0:X3},${1:X3},${2:X3},${3:X3}\r\n"
                    , (ym2151Register[0x40 + 0 + ch] & 0x7f) //DT/ML
                    , (ym2151Register[0x40 + 8 + ch] & 0x7f) //DT/ML
                    , (ym2151Register[0x40 + 16 + ch] & 0x7f)//DT/ML
                    , (ym2151Register[0x40 + 24 + ch] & 0x7f)//DT/ML
                );
                n += string.Format("${0:X3},${1:X3},${2:X3},${3:X3}\r\n"
                    , (ym2151Register[0x60 + 0 + ch] & 0x7f) //TL
                    , (ym2151Register[0x60 + 8 + ch] & 0x7f) //TL
                    , (ym2151Register[0x60 + 16 + ch] & 0x7f)//TL
                    , (ym2151Register[0x60 + 24 + ch] & 0x7f)//TL
                );
                n += string.Format("${0:X3},${1:X3},${2:X3},${3:X3}\r\n"
                    , (ym2151Register[0x80 + 0 + ch] & 0xdf) //KS/AR
                    , (ym2151Register[0x80 + 8 + ch] & 0xdf) //KS/AR
                    , (ym2151Register[0x80 + 16 + ch] & 0xdf)//KS/AR
                    , (ym2151Register[0x80 + 24 + ch] & 0xdf)//KS/AR
                );
                n += string.Format("${0:X3},${1:X3},${2:X3},${3:X3}\r\n"
                    , (ym2151Register[0xa0 + 0 + ch] & 0x9f) //AM/DR
                    , (ym2151Register[0xa0 + 8 + ch] & 0x9f) //AM/DR
                    , (ym2151Register[0xa0 + 16 + ch] & 0x9f)//AM/DR
                    , (ym2151Register[0xa0 + 24 + ch] & 0x9f)//AM/DR
                );
                n += string.Format("${0:X3},${1:X3},${2:X3},${3:X3}\r\n"
                    , (ym2151Register[0xc0 + 0 + ch] & 0x1f) //SR
                    , (ym2151Register[0xc0 + 8 + ch] & 0x1f) //SR
                    , (ym2151Register[0xc0 + 16 + ch] & 0x1f)//SR
                    , (ym2151Register[0xc0 + 24 + ch] & 0x1f)//SR
                );
                n += string.Format("${0:X3},${1:X3},${2:X3},${3:X3}\r\n"
                    , (ym2151Register[0xe0 + 0 + ch] & 0xff) //SL/RR
                    , (ym2151Register[0xe0 + 8 + ch] & 0xff) //SL/RR
                    , (ym2151Register[0xe0 + 16 + ch] & 0xff)//SL/RR
                    , (ym2151Register[0xe0 + 24 + ch] & 0xff)//SL/RR
                );

                n += string.Format("${0:X3}\r\n"
                    , ym2151Register[0x20 + ch] //FB/AL
                    );
            }

            if (!string.IsNullOrEmpty(n)) Clipboard.SetText(n);
        }

        private void getInstChForNRTDRV(EnmChip chip, int ch, int chipID)
        {

            string n = "";

            if (chip == EnmChip.YM2612 || chip == EnmChip.YM2608 || chip == EnmChip.YM2203 || chip == EnmChip.YM2610)
            {
                int p = (ch > 2) ? 1 : 0;
                int c = (ch > 2) ? ch - 3 : ch;
                int[][] fmRegister = (chip == EnmChip.YM2612) ? Audio.GetFMRegister(chipID) : (chip == EnmChip.YM2608 ? Audio.GetYM2608Register(chipID) : (chip == EnmChip.YM2203 ? new int[][] { Audio.GetYM2203Register(chipID), null } : Audio.GetYM2610Register(chipID)));

                n = "@ xxxx {\r\n";
                n += string.Format("000,{0:D3},{1:D3},015\r\n"
                    , fmRegister[p][0xb0 + c] & 0x07//AL
                    , (fmRegister[p][0xb0 + c] & 0x38) >> 3//FB
                );

                for (int i = 0; i < 4; i++)
                {
                    int ops = (i == 0) ? 0 : ((i == 1) ? 8 : ((i == 2) ? 4 : 12));
                    n += string.Format(" {0:D3},{1:D3},{2:D3},{3:D3},{4:D3},{5:D3},{6:D3},{7:D3},{8:D3},{9:D3},{10:D3}\r\n"
                        , fmRegister[p][0x50 + ops + c] & 0x1f //AR
                        , fmRegister[p][0x60 + ops + c] & 0x1f //DR
                        , fmRegister[p][0x70 + ops + c] & 0x1f //SR
                        , fmRegister[p][0x80 + ops + c] & 0x0f //RR
                        , (fmRegister[p][0x80 + ops + c] & 0xf0) >> 4//SL
                        , fmRegister[p][0x40 + ops + c] & 0x7f//TL
                        , (fmRegister[p][0x50 + ops + c] & 0xc0) >> 6//KS
                        , fmRegister[p][0x30 + ops + c] & 0x0f//ML
                        , (fmRegister[p][0x30 + ops + c] & 0x70) >> 4//DT
                        , 0
                        , (fmRegister[p][0x60 + ops + c] & 0x80) >> 7//AM
                    );
                }
                n += "}\r\n";
            }
            else if (chip == EnmChip.YM2151)
            {
                int[] ym2151Register = Audio.GetYM2151Register(chipID);

                n = "@ xxxx {\r\n";
                n += string.Format("000,{0:D3},{1:D3},015\r\n"
                    , ym2151Register[0x20 + ch] & 0x07 //AL
                    , (ym2151Register[0x20 + ch] & 0x38) >> 3//FB
                );

                for (int i = 0; i < 4; i++)
                {
                    int ops = (i == 0) ? 0 : ((i == 1) ? 16 : ((i == 2) ? 8 : 24));
                    n += string.Format(" {0:D3},{1:D3},{2:D3},{3:D3},{4:D3},{5:D3},{6:D3},{7:D3},{8:D3},{9:D3},{10:D3}\r\n"
                        , ym2151Register[0x80 + ops + ch] & 0x1f //AR
                        , ym2151Register[0xa0 + ops + ch] & 0x1f //DR
                        , ym2151Register[0xc0 + ops + ch] & 0x1f //SR
                        , ym2151Register[0xe0 + ops + ch] & 0x0f //RR
                        , (ym2151Register[0xe0 + ops + ch] & 0xf0) >> 4 //SL
                        , ym2151Register[0x60 + ops + ch] & 0x7f //TL
                        , (ym2151Register[0x80 + ops + ch] & 0xc0) >> 6 //KS
                        , ym2151Register[0x40 + ops + ch] & 0x0f //ML
                        , (ym2151Register[0x40 + ops + ch] & 0x70) >> 4 //DT
                        , (ym2151Register[0xc0 + ops + ch] & 0xc0) >> 6 //DT2
                        , (ym2151Register[0xa0 + ops + ch] & 0x80) >> 7 //AM
                    );
                }
                n += "}\r\n";
            }

            if (!string.IsNullOrEmpty(n)) Clipboard.SetText(n);
        }

        private void getInstChForHuSIC(EnmChip chip, int ch, int chipID)
        {

            string n = "";

            if (chip == EnmChip.HuC6280)
            {
                MDSound.Ootake_PSG.huc6280_state huc6280Register = Audio.GetHuC6280Register(chipID);
                if (huc6280Register == null) return;
                MDSound.Ootake_PSG.PSG psg = huc6280Register.Psg[ch];
                if (psg == null) return;
                if (psg.wave == null) return;
                if (psg.wave.Length != 32) return;

                n = "@WTx={\r\n";

                for (int i = 0; i < 32; i += 8)
                {
                    n += string.Format("${0:x2},${1:x2},${2:x2},${3:x2},${4:x2},${5:x2},${6:x2},${7:x2},\r\n"
                        , (17 - psg.wave[i + 0])
                        , (17 - psg.wave[i + 1])
                        , (17 - psg.wave[i + 2])
                        , (17 - psg.wave[i + 3])
                        , (17 - psg.wave[i + 4])
                        , (17 - psg.wave[i + 5])
                        , (17 - psg.wave[i + 6])
                        , (17 - psg.wave[i + 7])
                        );
                }

                n = n.Substring(0, n.Length - 3) + "\r\n}\r\n";
            }

            if (!string.IsNullOrEmpty(n)) Clipboard.SetText(n);
        }

        private void getInstChForMGSC(EnmChip chip, int ch, int chipID)
        {

            string n = "";
            int[] Register=null;

            if (chip == EnmChip.YM2413)
            {
                Register = Audio.GetYM2413Register(chipID);
            }
            else if (chip == EnmChip.VRC7)
            {
                byte[] r = Audio.GetVRC7Register(chipID);
                if (r == null) return;
                Register = new int[r.Length];
                for (int i = 0; i < r.Length; i++)
                {
                    Register[i] = r[i];
                }
            }

            if (Register == null) return;
            n = "@vXX = { \r\n";
            n += "   ;       TL FB\r\n";
            n += string.Format("           {0:d2},{1:d2},\r\n"
                , Register[0x02] & 0x3f
                , Register[0x03] & 0x7
                );
            n += "   ;       AR DR SL RR KL MT AM VB EG KR DT\r\n";

            n += string.Format("           {0:d2},{1:d2},{2:d2},{3:d2},{4:d2},{5:d2},{6:d2},{7:d2},{8:d2},{9:d2},{10:d2},\r\n"
                , (Register[0x04] & 0xf0) >> 4
                , (Register[0x04] & 0x0f)
                , (Register[0x06] & 0xf0) >> 4
                , (Register[0x06] & 0x0f)
                , (Register[0x02] & 0xc0) >> 6
                , (Register[0x00] & 0x0f)
                , (Register[0x00] & 0x80) >> 7
                , (Register[0x00] & 0x40) >> 6
                , (Register[0x00] & 0x20) >> 5
                , (Register[0x00] & 0x10) >> 4
                , (Register[0x03] & 0x08) >> 3
                );

            n += string.Format("           {0:d2},{1:d2},{2:d2},{3:d2},{4:d2},{5:d2},{6:d2},{7:d2},{8:d2},{9:d2},{10:d2} }}\r\n"
                , (Register[0x05] & 0xf0) >> 4
                , (Register[0x05] & 0x0f)
                , (Register[0x07] & 0xf0) >> 4
                , (Register[0x07] & 0x0f)
                , (Register[0x03] & 0xc0) >> 6
                , (Register[0x01] & 0x0f)
                , (Register[0x01] & 0x80) >> 7
                , (Register[0x01] & 0x40) >> 6
                , (Register[0x01] & 0x20) >> 5
                , (Register[0x01] & 0x10) >> 4
                , (Register[0x03] & 0x10) >> 4
                );



            if (!string.IsNullOrEmpty(n)) Clipboard.SetText(n);
        }

        private void getInstChForTFI(EnmChip chip, int ch, int chipID)
        {

            byte[] n = new byte[42];

            if (chip == EnmChip.YM2612 || chip == EnmChip.YM2608 || chip == EnmChip.YM2203 || chip == EnmChip.YM2610)
            {
                int p = (ch > 2) ? 1 : 0;
                int c = (ch > 2) ? ch - 3 : ch;
                int[][] fmRegister = (chip == EnmChip.YM2612) ? Audio.GetFMRegister(chipID) : (chip == EnmChip.YM2608 ? Audio.GetYM2608Register(chipID) : (chip == EnmChip.YM2203 ? new int[][] { Audio.GetYM2203Register(chipID), null } : Audio.GetYM2610Register(chipID)));

                n[0] = (byte)(fmRegister[p][0xb0 + c] & 0x07);//AL
                n[1] = (byte)((fmRegister[p][0xb0 + c] & 0x38) >> 3);//FB


                for (int i = 0; i < 4; i++)
                {
                    //int ops = (i == 0) ? 0 : ((i == 1) ? 4 : ((i == 2) ? 8 : 12));
                    int ops = i * 4;

                    n[i * 10 + 2] = (byte)(fmRegister[p][0x30 + ops + c] & 0x0f);//ML
                    int dt = (fmRegister[p][0x30 + ops + c] & 0x70) >> 4;//DT
                    // 0>3  1>4  2>5  3>6  4>3  5>2  6>1  7>0
                    dt = (dt < 4) ? (dt + 3) : (7 - dt);
                    n[i * 10 + 3] = (byte)dt;
                    n[i * 10 + 4] = (byte)(fmRegister[p][0x40 + ops + c] & 0x7f);//TL
                    n[i * 10 + 5] = (byte)((fmRegister[p][0x50 + ops + c] & 0xc0) >> 6);//KS
                    n[i * 10 + 6] = (byte)(fmRegister[p][0x50 + ops + c] & 0x1f); //AR
                    n[i * 10 + 7] = (byte)(fmRegister[p][0x60 + ops + c] & 0x1f); //DR
                    n[i * 10 + 8] = (byte)(fmRegister[p][0x70 + ops + c] & 0x1f); //SR
                    n[i * 10 + 9] = (byte)(fmRegister[p][0x80 + ops + c] & 0x0f); //RR
                    n[i * 10 + 10] = (byte)((fmRegister[p][0x80 + ops + c] & 0xf0) >> 4);//SL
                    n[i * 10 + 11] = (byte)(fmRegister[p][0x90 + ops + c] & 0x0f);//SSG
                }

            }
            else if (chip == EnmChip.YM2151)
            {
                int[] ym2151Register = Audio.GetYM2151Register(chipID);

                n[0] = (byte)(ym2151Register[0x20 + ch] & 0x07);//AL
                n[1] = (byte)((ym2151Register[0x20 + ch] & 0x38) >> 3);//FB

                for (int i = 0; i < 4; i++)
                {
                    //int ops = (i == 0) ? 0 : ((i == 1) ? 8 : ((i == 2) ? 16 : 24));
                    int ops = i * 8;

                    n[i * 10 + 2] = (byte)(ym2151Register[0x40 + ops + ch] & 0x0f);//ML
                    int dt = ((ym2151Register[0x40 + ops + ch] & 0x70) >> 4);//DT
                    // 0>3  1>4  2>5  3>6  4>3  5>2  6>1  7>0
                    dt = (dt < 4) ? (dt + 3) : (7 - dt);
                    n[i * 10 + 3] = (byte)dt;
                    n[i * 10 + 4] = (byte)(ym2151Register[0x60 + ops + ch] & 0x7f);//TL
                    n[i * 10 + 5] = (byte)((ym2151Register[0x80 + ops + ch] & 0xc0) >> 6);//KS
                    n[i * 10 + 6] = (byte)(ym2151Register[0x80 + ops + ch] & 0x1f); //AR
                    n[i * 10 + 7] = (byte)(ym2151Register[0xa0 + ops + ch] & 0x1f); //DR
                    n[i * 10 + 8] = (byte)(ym2151Register[0xc0 + ops + ch] & 0x1f); //SR
                    n[i * 10 + 9] = (byte)(ym2151Register[0xe0 + ops + ch] & 0x0f); //RR
                    n[i * 10 + 10] = (byte)((ym2151Register[0xe0 + ops + ch] & 0xf0) >> 4);//SL
                    n[i * 10 + 11] = 0;
                }

            }

            SaveFileDialog sfd = new SaveFileDialog();

            sfd.FileName = "音色ファイル.tfi";
            sfd.Filter = "TFIファイル(*.tfi)|*.tfi|すべてのファイル(*.*)|*.*";
            sfd.FilterIndex = 1;
            sfd.Title = "名前を付けて保存";
            sfd.RestoreDirectory = true;

            if (sfd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using (System.IO.FileStream fs = new System.IO.FileStream(
                sfd.FileName,
                System.IO.FileMode.Create,
                System.IO.FileAccess.Write))
            {

                fs.Write(n, 0, n.Length);

            }
        }

        private void getInstChForVOPM(EnmChip chip, int ch, int chipID)
        {

            string n = "";

            if (chip == EnmChip.YM2612 || chip == EnmChip.YM2608 || chip == EnmChip.YM2203 || chip == EnmChip.YM2610)
            {
                int p = (ch > 2) ? 1 : 0;
                int c = (ch > 2) ? ch - 3 : ch;
                int[][] fmRegister = (chip == EnmChip.YM2612) ? Audio.GetFMRegister(chipID) : (chip == EnmChip.YM2608 ? Audio.GetYM2608Register(chipID) : (chip == EnmChip.YM2203 ? new int[][] { Audio.GetYM2203Register(chipID), null } : Audio.GetYM2610Register(chipID)));

                n = "@: n MDPlayer\r\n";
                n += "LFO:  0   0   0   0   0\r\n";
                n += string.Format("CH: 64  {0,2}  {1,2}   0   0 120   0\r\n"
                    , (fmRegister[p][0xb0 + c] & 0x38) >> 3//FB
                    , fmRegister[p][0xb0 + c] & 0x07//AL
                );

                for (int i = 0; i < 4; i++)
                {
                    int ops = (i == 0) ? 0 : ((i == 1) ? 8 : ((i == 2) ? 4 : 12));
                    n += string.Format(
                        "{0}{1}"
                        , string.Format(
                            "{0}:{1,3} {2,3} {3,3} {4,3} {5,3} "
                            , "M1C1M2C2".Substring(i * 2, 2)
                            , fmRegister[p][0x50 + ops + c] & 0x1f //AR
                            , fmRegister[p][0x60 + ops + c] & 0x1f //DR
                            , fmRegister[p][0x70 + ops + c] & 0x1f //SR
                            , fmRegister[p][0x80 + ops + c] & 0x0f //RR
                            , (fmRegister[p][0x80 + ops + c] & 0xf0) >> 4//SL
                        )
                        , string.Format(
                            "{0,3} {1,3} {2,3} {3,3}   0 {4,3}\r\n"
                            , fmRegister[p][0x40 + ops + c] & 0x7f//TL
                            , (fmRegister[p][0x50 + ops + c] & 0xc0) >> 6//KS
                            , fmRegister[p][0x30 + ops + c] & 0x0f//ML
                            , (fmRegister[p][0x30 + ops + c] & 0x70) >> 4//DT
                            , (fmRegister[p][0x60 + ops + c] & 0x80) >> 7//AM
                        )
                    );
                }
            }
            else if (chip == EnmChip.YM2151)
            {
                int[] ym2151Register = Audio.GetYM2151Register(chipID);

                n = "@: n MDPlayer\r\n";
                n += "LFO:  0   0   0   0   0\r\n";
                n += string.Format("CH: 64  {0,2}  {1,2}   0   0 120   0\r\n"
                    , (ym2151Register[0x20 + ch] & 0x38) >> 3//FB
                    , ym2151Register[0x20 + ch] & 0x07 //AL
                );

                for (int i = 0; i < 4; i++)
                {
                    int ops = (i == 0) ? 0 : ((i == 1) ? 16 : ((i == 2) ? 8 : 24));
                    n += string.Format(
                        "{0}{1}"
                        , string.Format(
                            "{0}:{1,3} {2,3} {3,3} {4,3} {5,3} "
                            , "M1C1M2C2".Substring(i * 2, 2)
                            , ym2151Register[0x80 + ops + ch] & 0x1f //AR
                            , ym2151Register[0xa0 + ops + ch] & 0x1f //DR
                            , ym2151Register[0xc0 + ops + ch] & 0x1f //SR
                            , ym2151Register[0xe0 + ops + ch] & 0x0f //RR
                            , (ym2151Register[0xe0 + ops + ch] & 0xf0) >> 4 //SL
                        )
                        , string.Format(
                            "{0,3} {1,3} {2,3} {3,3}   0 {4,3}\r\n"
                            , ym2151Register[0x60 + ops + ch] & 0x7f //TL
                            , (ym2151Register[0x80 + ops + ch] & 0xc0) >> 6 //KS
                            , ym2151Register[0x40 + ops + ch] & 0x0f //ML
                            , (ym2151Register[0x40 + ops + ch] & 0x70) >> 4 //DT
                            , (ym2151Register[0xc0 + ops + ch] & 0xc0) >> 6 //DT2
                            , (ym2151Register[0xa0 + ops + ch] & 0x80) >> 7 //AM
                        )
                    );
                }
            }

            if (!string.IsNullOrEmpty(n)) Clipboard.SetText(n);
        }

        private void getInstChForPMD(EnmChip chip, int ch, int chipID)
        {

            string n = "";

            if (chip == EnmChip.YM2612 || chip == EnmChip.YM2608 || chip == EnmChip.YM2203 || chip == EnmChip.YM2610)
            {
                int p = (ch > 2) ? 1 : 0;
                int c = (ch > 2) ? ch - 3 : ch;
                int[][] fmRegister = (chip == EnmChip.YM2612) ? Audio.GetFMRegister(chipID) : (chip == EnmChip.YM2608 ? Audio.GetYM2608Register(chipID) : (chip == EnmChip.YM2203 ? new int[][] { Audio.GetYM2203Register(chipID), null } : Audio.GetYM2610Register(chipID)));

                n = "; nm alg fbl\r\n";
                n += string.Format("@xxx {0:D3} {1:D3}                            =      MDPlayer\r\n"
                    , fmRegister[p][0xb0 + c] & 0x07//AL
                    , (fmRegister[p][0xb0 + c] & 0x38) >> 3//FB
                );
                n += "; ar  dr  sr  rr  sl  tl  ks  ml  dt ams   seg\r\n";

                for (int i = 0; i < 4; i++)
                {
                    int ops = (i == 0) ? 0 : ((i == 1) ? 8 : ((i == 2) ? 4 : 12));
                    n += string.Format(" {0:D3} {1:D3} {2:D3} {3:D3} {4:D3} {5:D3} {6:D3} {7:D3} {8:D3} {9:D3} ; {10:D3}\r\n"
                        , fmRegister[p][0x50 + ops + c] & 0x1f //AR
                        , fmRegister[p][0x60 + ops + c] & 0x1f //DR
                        , fmRegister[p][0x70 + ops + c] & 0x1f //SR
                        , fmRegister[p][0x80 + ops + c] & 0x0f //RR
                        , (fmRegister[p][0x80 + ops + c] & 0xf0) >> 4//SL
                        , fmRegister[p][0x40 + ops + c] & 0x7f//TL
                        , (fmRegister[p][0x50 + ops + c] & 0xc0) >> 6//KS
                        , fmRegister[p][0x30 + ops + c] & 0x0f//ML
                        , (fmRegister[p][0x30 + ops + c] & 0x70) >> 4//DT
                        , (fmRegister[p][0x60 + ops + c] & 0x80) >> 7//AM
                        , fmRegister[p][0x90 + ops + c] & 0x0f//SG
                    );
                }
            }
            else if (chip == EnmChip.YM2151)
            {
                int[] ym2151Register = Audio.GetYM2151Register(chipID);
                n = "; nm alg fbl\r\n";
                n += string.Format("@xxx {0:D3} {1:D3}                            =      MDPlayer\r\n"
                    , ym2151Register[0x20 + ch] & 0x07 //AL
                    , (ym2151Register[0x20 + ch] & 0x38) >> 3//FB
                );
                n += "; ar  dr  sr  rr  sl  tl  ks  ml  dt ams   seg\r\n";

                for (int i = 0; i < 4; i++)
                {
                    int ops = (i == 0) ? 0 : ((i == 1) ? 16 : ((i == 2) ? 8 : 24));
                    n += string.Format(" {0:D3} {1:D3} {2:D3} {3:D3} {4:D3} {5:D3} {6:D3} {7:D3} {8:D3} {9:D3} ; {10:D3}\r\n"
                        , ym2151Register[0x80 + ops + ch] & 0x1f //AR
                        , ym2151Register[0xa0 + ops + ch] & 0x1f //DR
                        , ym2151Register[0xc0 + ops + ch] & 0x1f //SR
                        , ym2151Register[0xe0 + ops + ch] & 0x0f //RR
                        , (ym2151Register[0xe0 + ops + ch] & 0xf0) >> 4 //SL
                        , ym2151Register[0x60 + ops + ch] & 0x7f //TL
                        , (ym2151Register[0x80 + ops + ch] & 0xc0) >> 6 //KS
                        , ym2151Register[0x40 + ops + ch] & 0x0f //ML
                        , (ym2151Register[0x40 + ops + ch] & 0x70) >> 4 //DT
                        //, (ym2151Register[0xc0 + ops + ch] & 0xc0) >> 6 //DT2
                        , (ym2151Register[0xa0 + ops + ch] & 0x80) >> 7 //AM
                        , 0
                    );
                }
            }

            if (!string.IsNullOrEmpty(n)) Clipboard.SetText(n);
        }




        //SendMessageで送る構造体（Unicode文字列送信に最適化したパターン）
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpData;
        }

        //SendMessage（データ転送）
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref COPYDATASTRUCT lParam);
        public const int WM_COPYDATA = 0x004A;
        public const int WM_PASTE = 0x0302;

        //SendMessageを使ってプロセス間通信で文字列を渡す
        void SendString(IntPtr targetWindowHandle, string str)
        {
            COPYDATASTRUCT cds = new COPYDATASTRUCT();
            cds.dwData = IntPtr.Zero;
            cds.lpData = str;
            cds.cbData = str.Length * sizeof(char);
            //受信側ではlpDataの文字列を(cbData/2)の長さでstring.Substring()する

            IntPtr myWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
            SendMessage(targetWindowHandle, WM_COPYDATA, myWindowHandle, ref cds);
        }

        public void windowsMessage(ref Message m)
        {
            if (m.Msg == WM_COPYDATA)
            {
                string sParam = ReceiveString(m);
                try
                {

                    frmPlayList.Stop();

                    PlayList pl = frmPlayList.getPlayList();
                    if (pl.lstMusic.Count < 1 || pl.lstMusic[pl.lstMusic.Count - 1].fileName != sParam)
                    {
                        frmPlayList.getPlayList().AddFile(sParam);
                        //frmPlayList.AddList(sParam);
                    }

                    if (!loadAndPlay(0, 0, sParam))
                    {
                        frmPlayList.Stop();
                        Audio.Stop();
                        return;
                    }

                    frmPlayList.setStart(-1);
                    oldParam = new MDChipParams();

                    frmPlayList.Play();

                }
                catch (Exception ex)
                {
                    log.ForcedWrite(ex);
                    //メッセージによる読み込み失敗の場合は何も表示しない
                    //                    MessageBox.Show("ファイルの読み込みに失敗しました。");
                }
            }

        }

        //メッセージ処理
        protected override void WndProc(ref Message m)
        {
            windowsMessage(ref m);
            base.WndProc(ref m);
        }

        //SendString()で送信された文字列を取り出す
        string ReceiveString(Message m)
        {
            string str = null;
            try
            {
                COPYDATASTRUCT cds = (COPYDATASTRUCT)m.GetLParam(typeof(COPYDATASTRUCT));
                str = cds.lpData;
                str = str.Substring(0, cds.cbData / 2);
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
                str = null;
            }
            return str;
        }

        public static Process GetPreviousProcess()
        {
            Process curProcess = Process.GetCurrentProcess();
            Process[] allProcesses = Process.GetProcessesByName(curProcess.ProcessName);

            foreach (Process checkProcess in allProcesses)
            {
                // 自分自身のプロセスIDは無視する
                if (checkProcess.Id != curProcess.Id)
                {
                    // プロセスのフルパス名を比較して同じアプリケーションか検証
                    if (String.Compare(
                        checkProcess.MainModule.FileName,
                        curProcess.MainModule.FileName, true) == 0)
                    {
                        // 同じフルパス名のプロセスを取得
                        return checkProcess;
                    }
                }
            }

            // 同じアプリケーションのプロセスが見つからない！
            return null;
        }



        public bool loadAndPlay(int m, int songNo, string fn, string zfn = null)
        {
            try
            {
                if (Audio.flgReinit) flgReinit = true;
                if (setting.other.InitAlways) flgReinit = true;
                reinit(setting);

                if (Audio.isPaused)
                {
                    Audio.Pause();
                }

                string playingFileName = fn;
                string playingArcFileName = "";
                EnmFileFormat format = EnmFileFormat.unknown;
                List<Tuple<string, byte[]>> extFile = null;

                if (zfn == null || zfn == "")
                {
                    srcBuf = getAllBytes(fn, out format);
                    extFile = getExtendFile(fn, srcBuf, format);
                }
                else
                {

                    playingArcFileName = zfn;

                    if (Path.GetExtension(zfn).ToUpper() == ".ZIP")
                    {
                        using (ZipArchive archive = ZipFile.OpenRead(zfn))
                        {
                            ZipArchiveEntry entry = archive.GetEntry(fn);
                            string arcFn = "";

                            format = Common.CheckExt(fn);
                            if (format != EnmFileFormat.unknown)
                            {
                                srcBuf = getBytesFromZipFile(entry, out arcFn);
                                if (arcFn != "") playingFileName = arcFn;
                                extFile = getExtendFile(fn, srcBuf, format, archive);
                            }
                        }
                    }
                    else
                    {
                        format = Common.CheckExt(fn);
                        if (format != EnmFileFormat.unknown)
                        {
                            UnlhaWrap.UnlhaCmd cmd = new UnlhaWrap.UnlhaCmd();
                            srcBuf = cmd.GetFileByte(zfn, fn);
                            playingFileName = fn;
                            extFile = getExtendFile(fn, srcBuf, format, new Tuple<string, string>(zfn, fn));
                        }
                    }
                }

                //再生前に音量のバランスを設定する
                LoadPresetMixerBalance(playingFileName, playingArcFileName, format);

                Audio.SetVGMBuffer(format, srcBuf, playingFileName, playingArcFileName, m, songNo, extFile);
                newParam.ym2612[0].fileFormat = format;
                newParam.ym2612[1].fileFormat = format;

                if (srcBuf != null)
                {
                    this.Invoke((Action)playdata);
                    if (Audio.errMsg != "") return false;
                }

            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
                srcBuf = null;
                MessageBox.Show(string.Format("ファイルの読み込みに失敗しました。\r\nメッセージ={0}", ex.Message), "MDPlayer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private List<Tuple<string, byte[]>> getExtendFile(string fn, byte[] srcBuf, EnmFileFormat format, object archive = null)
        {
            List<Tuple<string, byte[]>> ret = new List<Tuple<string, byte[]>>();
            byte[] buf;
            switch (format)
            {
                case EnmFileFormat.RCP:
                    string CM6, GSD, GSD2;
                    RCP.getControlFileName(srcBuf, out CM6, out GSD, out GSD2);
                    if (!string.IsNullOrEmpty(CM6))
                    {
                        buf = getExtendFileAllBytes(fn, CM6, archive);
                        if (buf != null) ret.Add(new Tuple<string, byte[]>(".CM6", buf));
                    }
                    if (!string.IsNullOrEmpty(GSD))
                    {
                        buf = getExtendFileAllBytes(fn, GSD, archive);
                        if (buf != null) ret.Add(new Tuple<string, byte[]>(".GSD", buf));
                    }
                    if (!string.IsNullOrEmpty(GSD2))
                    {
                        buf = getExtendFileAllBytes(fn, GSD2, archive);
                        if (buf != null) ret.Add(new Tuple<string, byte[]>(".GSD", buf));
                    }
                    break;
                case EnmFileFormat.MDR:
                    buf = getExtendFileAllBytes(fn, System.IO.Path.GetFileNameWithoutExtension(fn) + ".PCM", archive);
                    if (buf != null) ret.Add(new Tuple<string, byte[]>(".PCM", buf));
                    break;
                case EnmFileFormat.MDX:
                    string PDX;
                    Driver.MXDRV.MXDRV.getPDXFileName(srcBuf, out PDX);
                    if (!string.IsNullOrEmpty(PDX))
                    {
                        buf = getExtendFileAllBytes(fn, PDX, archive);
                        if (buf == null)
                        {
                            buf = getExtendFileAllBytes(fn, PDX + ".PDX", archive);
                        }
                        if (buf != null) ret.Add(new Tuple<string, byte[]>(".PDX", buf));
                    }
                    break;
                case EnmFileFormat.MND:
                    uint hs=(uint)((srcBuf[0x06] << 8) + srcBuf[0x07]);
                    uint pcmptr = (uint)((srcBuf[0x14] << 24) + (srcBuf[0x15] << 16) + (srcBuf[0x16] << 8) + srcBuf[0x17]);
                    if (hs < 0x18) pcmptr = 0;
                    if (pcmptr != 0)
                    {
                        int pcmnum = (srcBuf[pcmptr] << 8) + srcBuf[pcmptr + 1];
                        pcmptr += 2;
                        for (int i = 0; i < pcmnum; i++)
                        {
                            string mndPcmFn = Common.getNRDString(srcBuf, ref pcmptr);
                            buf = getExtendFileAllBytes(fn, mndPcmFn, archive);
                            if (buf != null) ret.Add(new Tuple<string, byte[]>(".PND", buf));
                        }
                    }
                    break;
                default:
                    return null;
            }

            return ret;
        }

        private byte[] getExtendFileAllBytes(string srcFn, string extFn, object archive)
        {
            try
            {
                if (archive == null)
                {
                    string trgFn = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(srcFn), extFn).Trim();
                    if (!System.IO.File.Exists(trgFn)) return null;
                    return System.IO.File.ReadAllBytes(trgFn);
                }
                else
                {
                    string trgFn = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(srcFn), extFn);
                    trgFn = trgFn.Replace("\\", "/").Trim();
                    if (archive is ZipArchive)
                    {
                        ZipArchiveEntry entry = ((ZipArchive)archive).GetEntry(trgFn);
                        if (entry == null) return null;
                        string arcFn = "";
                        return getBytesFromZipFile(entry, out arcFn);
                    }
                    else
                    {
                        UnlhaWrap.UnlhaCmd cmd = new UnlhaWrap.UnlhaCmd();
                        return cmd.GetFileByte(((Tuple<string, string>)archive).Item1, trgFn);
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public byte[] getBytesFromZipFile(ZipArchiveEntry entry, out string arcFn)
        {
            byte[] buf = null;
            arcFn = entry.FullName;
            using (BinaryReader reader = new BinaryReader(entry.Open()))
            {
                buf = reader.ReadBytes((int)entry.Length);
            }

            if (Common.CheckExt(entry.FullName) == EnmFileFormat.VGM)
            {
                try
                {
                    uint vgm = (UInt32)buf[0] + (UInt32)buf[1] * 0x100 + (UInt32)buf[2] * 0x10000 + (UInt32)buf[3] * 0x1000000;
                    if (vgm != FCC_VGM)
                    {
                        int num;
                        buf = new byte[1024]; // 1Kbytesずつ処理する

                        Stream inStream // 入力ストリーム
                          = entry.Open();

                        GZipStream decompStream // 解凍ストリーム
                          = new GZipStream(
                            inStream, // 入力元となるストリームを指定
                            CompressionMode.Decompress); // 解凍（圧縮解除）を指定

                        MemoryStream outStream // 出力ストリーム
                          = new MemoryStream();

                        using (inStream)
                        using (outStream)
                        using (decompStream)
                        {
                            while ((num = decompStream.Read(buf, 0, buf.Length)) > 0)
                            {
                                outStream.Write(buf, 0, num);
                            }
                        }

                        buf = outStream.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    log.ForcedWrite(ex);
                    buf = null;
                }
            }

            return buf;
        }

        public void SetChannelMask(EnmChip chip, int chipID, int ch)
        {
            switch (chip)
            {
                case EnmChip.YM2203:
                    if (ch >= 0 && ch < 9)
                    {
                        if (!newParam.ym2203[chipID].channels[ch].mask)
                            Audio.setYM2203Mask(chipID, ch);
                        else
                            Audio.resetYM2203Mask(chipID, ch);

                        newParam.ym2203[chipID].channels[ch].mask = !newParam.ym2203[chipID].channels[ch].mask;

                        //FM(2ch) FMex
                        if ((ch == 2) || (ch >= 6 && ch < 9))
                        {
                            newParam.ym2203[chipID].channels[2].mask = newParam.ym2203[chipID].channels[ch].mask;
                            newParam.ym2203[chipID].channels[6].mask = newParam.ym2203[chipID].channels[ch].mask;
                            newParam.ym2203[chipID].channels[7].mask = newParam.ym2203[chipID].channels[ch].mask;
                            newParam.ym2203[chipID].channels[8].mask = newParam.ym2203[chipID].channels[ch].mask;
                        }
                    }
                    break;
                case EnmChip.YM2413:
                    if (ch >= 0 && ch < 14)
                    {
                        if (!newParam.ym2413[chipID].channels[ch].mask)
                            Audio.setYM2413Mask(chipID, ch);
                        else
                            Audio.resetYM2413Mask(chipID, ch);

                        newParam.ym2413[chipID].channels[ch].mask = !newParam.ym2413[chipID].channels[ch].mask;
                    }
                    break;
                case EnmChip.YM3526:
                    if (ch >= 0 && ch < 14)
                    {
                        if (!newParam.ym3526[chipID].channels[ch].mask)
                            Audio.setYM3526Mask(chipID, ch);
                        else
                            Audio.resetYM3526Mask(chipID, ch);

                        newParam.ym3526[chipID].channels[ch].mask = !newParam.ym3526[chipID].channels[ch].mask;

                    }
                    break;
                case EnmChip.Y8950:
                    if (ch >= 0 && ch < 15)
                    {
                        if (!newParam.y8950[chipID].channels[ch].mask)
                            Audio.setY8950Mask(chipID, ch);
                        else
                            Audio.resetY8950Mask(chipID, ch);

                        newParam.y8950[chipID].channels[ch].mask = !newParam.y8950[chipID].channels[ch].mask;

                    }
                    break;
                case EnmChip.YM3812:
                    if (ch >= 0 && ch < 14)
                    {
                        if (!newParam.ym3812[chipID].channels[ch].mask)
                            Audio.setYM3812Mask(chipID, ch);
                        else
                            Audio.resetYM3812Mask(chipID, ch);

                        newParam.ym3812[chipID].channels[ch].mask = !newParam.ym3812[chipID].channels[ch].mask;

                    }
                    break;
                case EnmChip.YMF262:
                    if (ch >= 0 && ch < 24)
                    {
                        if (!newParam.ymf262[chipID].channels[ch].mask)
                            Audio.setYMF262Mask(chipID, ch);
                        else
                            Audio.resetYMF262Mask(chipID, ch);

                        newParam.ymf262[chipID].channels[ch].mask = !newParam.ymf262[chipID].channels[ch].mask;

                    }
                    break;
                case EnmChip.YMF278B:
                    if (ch >= 0 && ch < 47)
                    {
                        if (!newParam.ymf278b[chipID].channels[ch].mask)
                            Audio.setYMF278BMask(chipID, ch);
                        else
                            Audio.resetYMF278BMask(chipID, ch);

                        newParam.ymf278b[chipID].channels[ch].mask = !newParam.ymf278b[chipID].channels[ch].mask;

                    }
                    break;
                case EnmChip.YM2608:
                    if (ch >= 0 && ch < 14)
                    {
                        if (!newParam.ym2608[chipID].channels[ch].mask)
                            Audio.setYM2608Mask(chipID, ch);
                        else
                            Audio.resetYM2608Mask(chipID, ch);

                        newParam.ym2608[chipID].channels[ch].mask = !newParam.ym2608[chipID].channels[ch].mask;

                        //FM(2ch) FMex
                        if ((ch == 2) || (ch >= 9 && ch < 12))
                        {
                            newParam.ym2608[chipID].channels[2].mask = newParam.ym2608[chipID].channels[ch].mask;
                            newParam.ym2608[chipID].channels[9].mask = newParam.ym2608[chipID].channels[ch].mask;
                            newParam.ym2608[chipID].channels[10].mask = newParam.ym2608[chipID].channels[ch].mask;
                            newParam.ym2608[chipID].channels[11].mask = newParam.ym2608[chipID].channels[ch].mask;
                        }
                    }
                    break;
                case EnmChip.YM2610:
                    if (ch >= 0 && ch < 14)
                    {
                        int c = ch;
                        if (ch == 12) c = 13;
                        if (ch == 13) c = 12;

                        if (!newParam.ym2610[chipID].channels[c].mask)
                            Audio.setYM2610Mask(chipID, ch);
                        else
                            Audio.resetYM2610Mask(chipID, ch);
                        newParam.ym2610[chipID].channels[c].mask = !newParam.ym2610[chipID].channels[c].mask;

                        //FM(2ch) FMex
                        if ((ch == 2) || (ch >= 9 && ch < 12))
                        {
                            newParam.ym2610[chipID].channels[2].mask = newParam.ym2610[chipID].channels[ch].mask;
                            newParam.ym2610[chipID].channels[9].mask = newParam.ym2610[chipID].channels[ch].mask;
                            newParam.ym2610[chipID].channels[10].mask = newParam.ym2610[chipID].channels[ch].mask;
                            newParam.ym2610[chipID].channels[11].mask = newParam.ym2610[chipID].channels[ch].mask;
                        }
                    }
                    break;
                case EnmChip.YM2612:
                    if (ch >= 0 && ch < 9)
                    {
                        if (!newParam.ym2612[chipID].channels[ch].mask)
                            Audio.setYM2612Mask(chipID, ch);
                        else
                            Audio.resetYM2612Mask(chipID, ch);

                        newParam.ym2612[chipID].channels[ch].mask = !newParam.ym2612[chipID].channels[ch].mask;

                        //FM(2ch) FMex
                        if ((ch == 2) || (ch >= 6 && ch < 9))
                        {
                            newParam.ym2612[chipID].channels[2].mask = newParam.ym2612[chipID].channels[ch].mask;
                            newParam.ym2612[chipID].channels[6].mask = newParam.ym2612[chipID].channels[ch].mask;
                            newParam.ym2612[chipID].channels[7].mask = newParam.ym2612[chipID].channels[ch].mask;
                            newParam.ym2612[chipID].channels[8].mask = newParam.ym2612[chipID].channels[ch].mask;
                        }
                    }
                    break;
                case EnmChip.SN76489:
                    if (!newParam.sn76489[chipID].channels[ch].mask)
                    {
                        Audio.setSN76489Mask(chipID, ch);
                    }
                    else
                    {
                        Audio.resetSN76489Mask(chipID, ch);
                    }
                    newParam.sn76489[chipID].channels[ch].mask = !newParam.sn76489[chipID].channels[ch].mask;
                    break;
                case EnmChip.RF5C164:
                    if (!newParam.rf5c164[chipID].channels[ch].mask)
                    {
                        Audio.setRF5C164Mask(chipID, ch);
                    }
                    else
                    {
                        Audio.resetRF5C164Mask(chipID, ch);
                    }
                    newParam.rf5c164[chipID].channels[ch].mask = !newParam.rf5c164[chipID].channels[ch].mask;
                    break;
                case EnmChip.YM2151:
                    if (!newParam.ym2151[chipID].channels[ch].mask)
                    {
                        Audio.setYM2151Mask(chipID, ch);
                    }
                    else
                    {
                        Audio.resetYM2151Mask(chipID, ch);
                    }
                    newParam.ym2151[chipID].channels[ch].mask = !newParam.ym2151[chipID].channels[ch].mask;
                    break;
                case EnmChip.C140:
                    if (!newParam.c140[chipID].channels[ch].mask)
                    {
                        Audio.setC140Mask(chipID, ch);
                    }
                    else
                    {
                        Audio.resetC140Mask(chipID, ch);
                    }
                    newParam.c140[chipID].channels[ch].mask = !newParam.c140[chipID].channels[ch].mask;
                    break;
                case EnmChip.C352:
                    if (!newParam.c352[chipID].channels[ch].mask)
                    {
                        Audio.setC352Mask(chipID, ch);
                    }
                    else
                    {
                        Audio.resetC352Mask(chipID, ch);
                    }
                    newParam.c352[chipID].channels[ch].mask = !newParam.c352[chipID].channels[ch].mask;
                    break;
                case EnmChip.SEGAPCM:
                    if (!newParam.segaPcm[chipID].channels[ch].mask)
                    {
                        Audio.setSegaPCMMask(chipID, ch);
                    }
                    else
                    {
                        Audio.resetSegaPCMMask(chipID, ch);
                    }
                    newParam.segaPcm[chipID].channels[ch].mask = !newParam.segaPcm[chipID].channels[ch].mask;
                    break;
                case EnmChip.AY8910:
                    if (!newParam.ay8910[chipID].channels[ch].mask)
                    {
                        Audio.setAY8910Mask(chipID, ch);
                    }
                    else
                    {
                        Audio.resetAY8910Mask(chipID, ch);
                    }
                    newParam.ay8910[chipID].channels[ch].mask = !newParam.ay8910[chipID].channels[ch].mask;
                    break;
                case EnmChip.HuC6280:
                    if (!newParam.huc6280[chipID].channels[ch].mask)
                    {
                        Audio.setHuC6280Mask(chipID, ch);
                    }
                    else
                    {
                        Audio.resetHuC6280Mask(chipID, ch);
                    }
                    newParam.huc6280[chipID].channels[ch].mask = !newParam.huc6280[chipID].channels[ch].mask;
                    break;
                case EnmChip.OKIM6258:
                    if (!newParam.okim6258[chipID].mask)
                    {
                        Audio.setOKIM6258Mask(chipID);
                    }
                    else
                    {
                        Audio.resetOKIM6258Mask(chipID);
                    }
                    newParam.okim6258[chipID].mask = !newParam.okim6258[chipID].mask;
                    break;
                case EnmChip.OKIM6295:
                    if (!newParam.okim6295[chipID].channels[ch].mask)
                    {
                        Audio.setOKIM6295Mask(chipID, ch);
                    }
                    else
                    {
                        Audio.resetOKIM6295Mask(chipID,ch);
                    }
                    newParam.okim6295[chipID].channels[ch].mask = !newParam.okim6295[chipID].channels[ch].mask;
                    break;
                case EnmChip.NES:
                    if (!newParam.nesdmc[chipID].sqrChannels[ch].mask)
                    {
                        Audio.setNESMask(chipID, ch);
                    }
                    else
                    {
                        Audio.resetNESMask(chipID, ch);
                    }
                    newParam.nesdmc[chipID].sqrChannels[ch].mask = !newParam.nesdmc[chipID].sqrChannels[ch].mask;
                    break;
                case EnmChip.DMC:
                    switch (ch)
                    {
                        case 0:
                            if (!newParam.nesdmc[chipID].triChannel.mask) Audio.setDMCMask(chipID, ch);
                            else Audio.resetDMCMask(chipID, ch);
                            newParam.nesdmc[chipID].triChannel.mask = !newParam.nesdmc[chipID].triChannel.mask;
                            break;
                        case 1:
                            if (!newParam.nesdmc[chipID].noiseChannel.mask) Audio.setDMCMask(chipID, ch);
                            else Audio.resetDMCMask(chipID, ch);
                            newParam.nesdmc[chipID].noiseChannel.mask = !newParam.nesdmc[chipID].noiseChannel.mask;
                            break;
                        case 2:
                            if (!newParam.nesdmc[chipID].dmcChannel.mask) Audio.setDMCMask(chipID, ch);
                            else Audio.resetDMCMask(chipID, ch);
                            newParam.nesdmc[chipID].dmcChannel.mask = !newParam.nesdmc[chipID].dmcChannel.mask;
                            break;
                    }
                    break;
                case EnmChip.FDS:
                    if (!newParam.fds[chipID].channel.mask) Audio.setFDSMask(chipID);
                    else Audio.resetFDSMask(chipID);
                    newParam.fds[chipID].channel.mask = !newParam.fds[chipID].channel.mask;
                    break;
                case EnmChip.MMC5:
                    switch (ch)
                    {
                        case 0:
                            if (!newParam.mmc5[chipID].sqrChannels[0].mask) Audio.setMMC5Mask(chipID, ch);
                            else Audio.resetMMC5Mask(chipID, ch);
                            newParam.mmc5[chipID].sqrChannels[0].mask = !newParam.mmc5[chipID].sqrChannels[0].mask;
                            break;
                        case 1:
                            if (!newParam.mmc5[chipID].sqrChannels[1].mask) Audio.setMMC5Mask(chipID, ch);
                            else Audio.resetMMC5Mask(chipID, ch);
                            newParam.mmc5[chipID].sqrChannels[1].mask = !newParam.mmc5[chipID].sqrChannels[1].mask;
                            break;
                        case 2:
                            if (!newParam.mmc5[chipID].pcmChannel.mask) Audio.setMMC5Mask(chipID, ch);
                            else Audio.resetMMC5Mask(chipID, ch);
                            newParam.mmc5[chipID].pcmChannel.mask = !newParam.mmc5[chipID].pcmChannel.mask;
                            break;
                    }
                    break;
                case EnmChip.VRC7:
                    if (ch >= 0 && ch < 6)
                    {
                        if (!newParam.vrc7[chipID].channels[ch].mask)
                            Audio.setVRC7Mask(chipID, ch);
                        else
                            Audio.resetVRC7Mask(chipID, ch);

                        newParam.vrc7[chipID].channels[ch].mask = !newParam.vrc7[chipID].channels[ch].mask;
                    }
                    break;
                case EnmChip.K051649:
                    if (ch >= 0 && ch < 5)
                    {
                        if (!newParam.k051649[chipID].channels[ch].mask)
                            Audio.setK051649Mask(chipID, ch);
                        else
                            Audio.resetK051649Mask(chipID, ch);

                        newParam.k051649[chipID].channels[ch].mask = !newParam.k051649[chipID].channels[ch].mask;
                    }
                    break;
            }
        }

        public void ResetChannelMask(EnmChip chip, int chipID, int ch)
        {
            switch (chip)
            {
                case EnmChip.SN76489:
                    newParam.sn76489[chipID].channels[ch].mask = false;
                    Audio.resetSN76489Mask(chipID, ch);
                    break;
                case EnmChip.RF5C164:
                    newParam.rf5c164[chipID].channels[ch].mask = false;
                    Audio.resetRF5C164Mask(chipID, ch);
                    break;
                case EnmChip.YM2151:
                    newParam.ym2151[chipID].channels[ch].mask = false;
                    Audio.resetYM2151Mask(chipID, ch);
                    break;
                case EnmChip.YM2203:
                    newParam.ym2203[chipID].channels[ch].mask = false;
                    Audio.resetYM2203Mask(chipID, ch);
                    break;
                case EnmChip.YM2413:
                    newParam.ym2413[chipID].channels[ch].mask = false;
                    Audio.resetYM2413Mask(chipID, ch);
                    break;
                case EnmChip.VRC7:
                    newParam.vrc7[chipID].channels[ch].mask = false;
                    Audio.resetVRC7Mask(chipID, ch);
                    break;
                case EnmChip.YM2608:
                    newParam.ym2608[chipID].channels[ch].mask = false;
                    Audio.resetYM2608Mask(chipID, ch);
                    break;
                case EnmChip.YM2610:
                    if (ch < 12)
                        newParam.ym2610[chipID].channels[ch].mask = false;
                    else if (ch == 12)
                        newParam.ym2610[chipID].channels[13].mask = false;
                    else if (ch == 13)
                        newParam.ym2610[chipID].channels[12].mask = false;

                    Audio.resetYM2610Mask(chipID, ch);
                    break;
                case EnmChip.YM2612:
                    newParam.ym2612[chipID].channels[ch].mask = false;
                    if (ch == 2)
                    {
                        newParam.ym2612[chipID].channels[6].mask = false;
                        newParam.ym2612[chipID].channels[7].mask = false;
                        newParam.ym2612[chipID].channels[8].mask = false;
                    }
                    if (ch < 6) Audio.resetYM2612Mask(chipID, ch);
                    break;
                case EnmChip.YM3526:
                    newParam.ym3526[chipID].channels[ch].mask = false;
                    Audio.resetYM3526Mask(chipID, ch);
                    break;
                case EnmChip.Y8950:
                    newParam.y8950[chipID].channels[ch].mask = false;
                    Audio.resetY8950Mask(chipID, ch);
                    break;
                case EnmChip.YM3812:
                    newParam.ym3812[chipID].channels[ch].mask = false;
                    Audio.resetYM3812Mask(chipID, ch);
                    break;
                case EnmChip.YMF262:
                    newParam.ymf262[chipID].channels[ch].mask = false;
                    Audio.resetYMF262Mask(chipID, ch);
                    break;
                case EnmChip.YMF278B:
                    newParam.ymf278b[chipID].channels[ch].mask = false;
                    Audio.resetYMF278BMask(chipID, ch);
                    break;
                case EnmChip.C140:
                    newParam.c140[chipID].channels[ch].mask = false;
                    if (ch < 24) Audio.resetC140Mask(chipID, ch);
                    break;
                case EnmChip.C352:
                    newParam.c352[chipID].channels[ch].mask = false;
                    if (ch < 32) Audio.resetC352Mask(chipID, ch);
                    break;
                case EnmChip.SEGAPCM:
                    newParam.segaPcm[chipID].channels[ch].mask = false;
                    if (ch < 16) Audio.resetSegaPCMMask(chipID, ch);
                    break;
                case EnmChip.AY8910:
                    newParam.ay8910[chipID].channels[ch].mask = false;
                    Audio.resetAY8910Mask(chipID, ch);
                    break;
                case EnmChip.HuC6280:
                    newParam.huc6280[chipID].channels[ch].mask = false;
                    Audio.resetHuC6280Mask(chipID, ch);
                    break;
                case EnmChip.K051649:
                    newParam.k051649[chipID].channels[ch].mask = false;
                    Audio.resetK051649Mask(chipID, ch);
                    break;
                case EnmChip.OKIM6258:
                    newParam.okim6258[chipID].mask = false;
                    Audio.resetOKIM6258Mask(chipID);
                    break;
                case EnmChip.OKIM6295:
                    newParam.okim6295[chipID].channels[ch].mask = false;
                    Audio.resetOKIM6295Mask(chipID,ch);
                    break;
                case EnmChip.NES:
                    switch (ch)
                    {
                        case 0:
                        case 1:
                            newParam.nesdmc[chipID].sqrChannels[ch].mask = false;
                            Audio.resetNESMask(chipID, ch);
                            break;
                        case 2:
                            newParam.nesdmc[chipID].triChannel.mask = false;
                            Audio.resetDMCMask(chipID, 0);
                            break;
                        case 3:
                            newParam.nesdmc[chipID].noiseChannel.mask = false;
                            Audio.resetDMCMask(chipID, 1);
                            break;
                        case 4:
                            newParam.nesdmc[chipID].dmcChannel.mask = false;
                            Audio.resetDMCMask(chipID, 2);
                            break;
                    }
                    break;
                case EnmChip.DMC:
                    switch (ch)
                    {
                        case 0:
                            newParam.nesdmc[chipID].triChannel.mask = false;
                            Audio.resetDMCMask(chipID, 0);
                            break;
                        case 1:
                            newParam.nesdmc[chipID].noiseChannel.mask = false;
                            Audio.resetDMCMask(chipID, 1);
                            break;
                        case 2:
                            newParam.nesdmc[chipID].dmcChannel.mask = false;
                            Audio.resetDMCMask(chipID, 2);
                            break;
                    }
                    break;
                case EnmChip.FDS:
                    newParam.fds[chipID].channel.mask = false;
                    Audio.resetFDSMask(chipID);
                    break;
                case EnmChip.MMC5:
                    switch (ch)
                    {
                        case 0:
                            newParam.mmc5[chipID].sqrChannels[0].mask = false;
                            break;
                        case 1:
                            newParam.mmc5[chipID].sqrChannels[1].mask = false;
                            break;
                        case 2:
                            newParam.mmc5[chipID].pcmChannel.mask = false;
                            break;
                    }
                    Audio.resetMMC5Mask(chipID, ch);
                    break;

            }
        }

        public void ForceChannelMask(EnmChip chip, int chipID, int ch, bool mask)
        {
            switch (chip)
            {
                case EnmChip.AY8910:
                    if (mask)
                        Audio.setAY8910Mask(chipID, ch);
                    else
                        Audio.resetAY8910Mask(chipID, ch);
                    newParam.ay8910[chipID].channels[ch].mask = mask;
                    oldParam.ay8910[chipID].channels[ch].mask = !mask;
                    break;
                case EnmChip.C140:
                    if (mask)
                        Audio.setC140Mask(chipID, ch);
                    else
                        Audio.resetC140Mask(chipID, ch);
                    newParam.c140[chipID].channels[ch].mask = mask;
                    oldParam.c140[chipID].channels[ch].mask = !mask;
                    break;
                case EnmChip.C352:
                    if (mask)
                        Audio.setC352Mask(chipID, ch);
                    else
                        Audio.resetC352Mask(chipID, ch);
                    newParam.c352[chipID].channels[ch].mask = mask;
                    oldParam.c352[chipID].channels[ch].mask = !mask;
                    break;
                case EnmChip.HuC6280:
                    if (mask)
                        Audio.setHuC6280Mask(chipID, ch);
                    else
                        Audio.resetHuC6280Mask(chipID, ch);
                    newParam.huc6280[chipID].channels[ch].mask = mask;
                    oldParam.huc6280[chipID].channels[ch].mask = !mask;
                    break;
                case EnmChip.RF5C164:
                    if (mask)
                        Audio.setRF5C164Mask(chipID, ch);
                    else
                        Audio.resetRF5C164Mask(chipID, ch);
                    newParam.rf5c164[chipID].channels[ch].mask = mask;
                    oldParam.rf5c164[chipID].channels[ch].mask = !mask;
                    break;
                case EnmChip.SEGAPCM:
                    if (mask)
                        Audio.setSegaPCMMask(chipID, ch);
                    else
                        Audio.resetSegaPCMMask(chipID, ch);
                    newParam.segaPcm[chipID].channels[ch].mask = mask;
                    oldParam.segaPcm[chipID].channels[ch].mask = !mask;
                    break;
                case EnmChip.YM2151:
                    if (mask)
                        Audio.setYM2151Mask(chipID, ch);
                    else
                        Audio.resetYM2151Mask(chipID, ch);
                    newParam.ym2151[chipID].channels[ch].mask = mask;
                    oldParam.ym2151[chipID].channels[ch].mask = !mask;
                    break;
                case EnmChip.YM2203:
                    if (ch >= 0 && ch < 9)
                    {
                        if (mask)
                            Audio.setYM2203Mask(chipID, ch);
                        else
                            Audio.resetYM2203Mask(chipID, ch);

                        newParam.ym2203[chipID].channels[ch].mask = mask;
                        oldParam.ym2203[chipID].channels[ch].mask = !mask;

                        //FM(2ch) FMex
                        if ((ch == 2) || (ch >= 6 && ch < 9))
                        {
                            newParam.ym2203[chipID].channels[2].mask = mask;
                            newParam.ym2203[chipID].channels[6].mask = mask;
                            newParam.ym2203[chipID].channels[7].mask = mask;
                            newParam.ym2203[chipID].channels[8].mask = mask;
                            oldParam.ym2203[chipID].channels[2].mask = !mask;
                            oldParam.ym2203[chipID].channels[6].mask = !mask;
                            oldParam.ym2203[chipID].channels[7].mask = !mask;
                            oldParam.ym2203[chipID].channels[8].mask = !mask;
                        }
                    }
                    break;
                case EnmChip.YM2413:
                    if (ch >= 0 && ch < 14)
                    {
                        if (mask)
                            Audio.setYM2413Mask(chipID, ch);
                        else
                            Audio.resetYM2413Mask(chipID, ch);

                        newParam.ym2413[chipID].channels[ch].mask = mask;
                        oldParam.ym2413[chipID].channels[ch].mask = !mask;
                    }
                    break;
                case EnmChip.YM2608:
                    if (ch >= 0 && ch < 14)
                    {
                        //if (mask)
                        //    Audio.setYM2608Mask(chipID, ch);
                        //else
                        //    Audio.resetYM2608Mask(chipID, ch);

                        newParam.ym2608[chipID].channels[ch].mask = mask;
                        oldParam.ym2608[chipID].channels[ch].mask = !mask;

                        //FM(2ch) FMex
                        if ((ch == 2) || (ch >= 9 && ch < 12))
                        {
                            newParam.ym2608[chipID].channels[2].mask = mask;
                            newParam.ym2608[chipID].channels[9].mask = mask;
                            newParam.ym2608[chipID].channels[10].mask = mask;
                            newParam.ym2608[chipID].channels[11].mask = mask;
                            oldParam.ym2608[chipID].channels[2].mask = !mask;
                            oldParam.ym2608[chipID].channels[9].mask = !mask;
                            oldParam.ym2608[chipID].channels[10].mask = !mask;
                            oldParam.ym2608[chipID].channels[11].mask = !mask;
                        }
                    }
                    break;
                case EnmChip.YM2610:
                    if (ch >= 0 && ch < 14)
                    {
                        int c = ch;
                        if (ch == 12) c = 13;
                        if (ch == 13) c = 12;

                        if (mask)
                            Audio.setYM2610Mask(chipID, ch);
                        else
                            Audio.resetYM2610Mask(chipID, ch);
                        newParam.ym2610[chipID].channels[c].mask = mask;
                        oldParam.ym2610[chipID].channels[c].mask = !mask;

                        //FM(2ch) FMex
                        if ((ch == 2) || (ch >= 9 && ch < 12))
                        {
                            newParam.ym2610[chipID].channels[ 2].mask = mask;
                            newParam.ym2610[chipID].channels[ 9].mask = mask;
                            newParam.ym2610[chipID].channels[10].mask = mask;
                            newParam.ym2610[chipID].channels[11].mask = mask;
                            oldParam.ym2610[chipID].channels[ 2].mask = !mask;
                            oldParam.ym2610[chipID].channels[ 9].mask = !mask;
                            oldParam.ym2610[chipID].channels[10].mask = !mask;
                            oldParam.ym2610[chipID].channels[11].mask = !mask;
                        }
                    }
                    break;
                case EnmChip.YM2612:
                    if (ch >= 0 && ch < 9)
                    {
                        if (mask)
                            Audio.setYM2612Mask(chipID, ch);
                        else
                            Audio.resetYM2612Mask(chipID, ch);

                        newParam.ym2612[chipID].channels[ch].mask = mask;
                        oldParam.ym2612[chipID].channels[ch].mask = !mask;

                        //FM(2ch) FMex
                        if ((ch == 2) || (ch >= 6 && ch < 9))
                        {
                            newParam.ym2612[chipID].channels[2].mask = mask;
                            newParam.ym2612[chipID].channels[6].mask = mask;
                            newParam.ym2612[chipID].channels[7].mask = mask;
                            newParam.ym2612[chipID].channels[8].mask = mask;
                            oldParam.ym2612[chipID].channels[2].mask = !mask;
                            oldParam.ym2612[chipID].channels[6].mask = !mask;
                            oldParam.ym2612[chipID].channels[7].mask = !mask;
                            oldParam.ym2612[chipID].channels[8].mask = !mask;
                        }
                    }
                    break;
                case EnmChip.YM3526:
                    if (ch >= 0 && ch < 14)
                    {
                        if (mask)
                            Audio.setYM3526Mask(chipID, ch);
                        else
                            Audio.resetYM3526Mask(chipID, ch);

                        newParam.ym3526[chipID].channels[ch].mask = mask;
                        oldParam.ym3526[chipID].channels[ch].mask = !mask;
                    }
                    break;
                case EnmChip.Y8950:
                    if (ch >= 0 && ch < 15)
                    {
                        if (mask)
                            Audio.setY8950Mask(chipID, ch);
                        else
                            Audio.resetY8950Mask(chipID, ch);

                        newParam.y8950[chipID].channels[ch].mask = mask;
                        oldParam.y8950[chipID].channels[ch].mask = !mask;
                    }
                    break;
                case EnmChip.YM3812:
                    if (ch >= 0 && ch < 14)
                    {
                        if (mask)
                            Audio.setYM3812Mask(chipID, ch);
                        else
                            Audio.resetYM3812Mask(chipID, ch);

                        newParam.ym3812[chipID].channels[ch].mask = mask;
                        oldParam.ym3812[chipID].channels[ch].mask = !mask;
                    }
                    break;
                case EnmChip.YMF262:
                    if (ch >= 0 && ch < 24)
                    {
                        if (mask)
                            Audio.setYMF262Mask(chipID, ch);
                        else
                            Audio.resetYMF262Mask(chipID, ch);

                        newParam.ymf262[chipID].channels[ch].mask = mask;
                        oldParam.ymf262[chipID].channels[ch].mask = !mask;
                    }
                    break;
                case EnmChip.YMF278B:
                    if (ch >= 0 && ch < 47)
                    {
                        if (mask)
                            Audio.setYMF278BMask(chipID, ch);
                        else
                            Audio.resetYMF278BMask(chipID, ch);

                        newParam.ymf278b[chipID].channels[ch].mask = mask;
                        oldParam.ymf278b[chipID].channels[ch].mask = !mask;
                    }
                    break;
                case EnmChip.SN76489:
                    if (mask)
                        Audio.setSN76489Mask(chipID, ch);
                    else
                        Audio.resetSN76489Mask(chipID, ch);
                    newParam.sn76489[chipID].channels[ch].mask = mask;
                    oldParam.sn76489[chipID].channels[ch].mask = !mask;
                    break;
                case EnmChip.OKIM6295:
                    if (mask)
                        Audio.setOKIM6295Mask(chipID, ch);
                    else
                        Audio.resetOKIM6295Mask(chipID, ch);
                    newParam.okim6295[chipID].channels[ch].mask = mask;
                    oldParam.okim6295[chipID].channels[ch].mask = !mask;
                    break;
            }
        }


        private void StartMIDIInMonitoring()
        {

            if (setting.midiKbd.MidiInDeviceName == "")
            {
                return;
            }

            if (midiin != null)
            {
                try
                {
                    midiin.Stop();
                    midiin.Dispose();
                    midiin.MessageReceived -= midiIn_MessageReceived;
                    midiin.ErrorReceived -= midiIn_ErrorReceived;
                    midiin = null;
                }
                catch
                {
                    midiin = null;
                }
            }

            if (midiin == null)
            {
                for (int i = 0; i < MidiIn.NumberOfDevices; i++)
                {
                    if (setting.midiKbd.MidiInDeviceName == MidiIn.DeviceInfo(i).ProductName)
                    {
                        try
                        {
                            midiin = new MidiIn(i);
                            midiin.MessageReceived += midiIn_MessageReceived;
                            midiin.ErrorReceived += midiIn_ErrorReceived;
                            midiin.Start();
                        }
                        catch
                        {
                            midiin = null;
                        }
                    }
                }
            }

        }

        void midiIn_ErrorReceived(object sender, MidiInMessageEventArgs e)
        {
            Console.WriteLine(String.Format("Error Time {0} Message 0x{1:X8} Event {2}",
                e.Timestamp, e.RawMessage, e.MidiEvent));
        }

        private void StopMIDIInMonitoring()
        {
            if (midiin != null)
            {
                try
                {
                    midiin.Stop();
                    midiin.Dispose();
                    midiin.MessageReceived -= midiIn_MessageReceived;
                    midiin.ErrorReceived -= midiIn_ErrorReceived;
                    midiin = null;
                }
                catch
                {
                    midiin = null;
                }
            }
        }

        void midiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
        {
            if (!setting.midiKbd.UseMIDIKeyboard) return;

            YM2612MIDI.midiIn_MessageReceived(e);
        }

        public void ym2612Midi_ClearNoteLog()
        {
            YM2612MIDI.ClearNoteLog();
        }

        public void ym2612Midi_ClearNoteLog(int ch)
        {
            YM2612MIDI.ClearNoteLog(ch);
        }

        public void ym2612Midi_Log2MML(int ch)
        {
            YM2612MIDI.Log2MML(ch);
        }

        public void ym2612Midi_Log2MML66(int ch)
        {
            YM2612MIDI.Log2MML66(ch);
        }

        public void ym2612Midi_AllNoteOff()
        {
            YM2612MIDI.AllNoteOff();
        }

        public void ym2612Midi_SetMode(int m)
        {
            YM2612MIDI.SetMode(m);
        }

        public void ym2612Midi_SelectChannel(int ch)
        {
            YM2612MIDI.SelectChannel(ch);
        }

        public void ym2612Midi_SetTonesToSetting()
        {
            YM2612MIDI.SetTonesToSettng();
        }

        public void ym2612Midi_SetTonesFromSetting()
        {
            YM2612MIDI.SetTonesFromSettng();
        }

        public void ym2612Midi_SaveTonePallet(string fn, int tp)
        {
            YM2612MIDI.SaveTonePallet(fn, tp, tonePallet);
        }

        public void ym2612Midi_LoadTonePallet(string fn, int tp)
        {
            YM2612MIDI.LoadTonePallet(fn, tp, tonePallet);
        }

        public void ym2612Midi_CopyToneToClipboard()
        {
            if (setting.midiKbd.IsMONO)
            {
                YM2612MIDI.CopyToneToClipboard(new int[] { setting.midiKbd.UseMONOChannel });
            }
            else
            {
                List<int> uc = new List<int>();
                for (int i = 0; i < setting.midiKbd.UseChannel.Length; i++)
                {
                    if (setting.midiKbd.UseChannel[i]) uc.Add(i);
                }
                YM2612MIDI.CopyToneToClipboard(uc.ToArray());
            }
        }

        public void ym2612Midi_PasteToneFromClipboard()
        {
            if (setting.midiKbd.IsMONO)
            {
                YM2612MIDI.PasteToneFromClipboard(new int[] { setting.midiKbd.UseMONOChannel });
            }
            else
            {
                List<int> uc = new List<int>();
                for (int i = 0; i < setting.midiKbd.UseChannel.Length; i++)
                {
                    if (setting.midiKbd.UseChannel[i]) uc.Add(i);
                }
                YM2612MIDI.PasteToneFromClipboard(uc.ToArray());
            }
        }

        public void ym2612Midi_CopyToneToClipboard(int ch)
        {
            YM2612MIDI.CopyToneToClipboard(new int[] { ch });
        }

        public void ym2612Midi_PasteToneFromClipboard(int ch)
        {
            YM2612MIDI.PasteToneFromClipboard(new int[] { ch });
        }

        public void ym2612Midi_SetSelectInstParam(int ch, int n)
        {
            YM2612MIDI.newParam.ym2612Midi.selectCh = ch;
            YM2612MIDI.newParam.ym2612Midi.selectParam = n;
        }

        public void ym2612Midi_AddSelectInstParam(int n)
        {
            int p = YM2612MIDI.newParam.ym2612Midi.selectParam;
            p += n;
            if (p > 47) p = 0;
            YM2612MIDI.newParam.ym2612Midi.selectParam = p;
        }

        public void ym2612Midi_ChangeSelectedParamValue(int n)
        {
            YM2612MIDI.ChangeSelectedParamValue(n);
        }

        private void LoadPresetMixerBalance(string playingFileName,string playingArcFileName, EnmFileFormat format)
        {
            if (!setting.autoBalance.UseThis) return;

            try
            {
                Setting.Balance balance = null;
                string fullPath = Common.settingFilePath;
                fullPath = Path.Combine(fullPath, "MixerBalance");
                if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);
                string fn = "";
                string defMbc = "";

                //曲ごとのプリセットを読み込むモード
                if (setting.autoBalance.LoadSongBalance)
                {
                    if (setting.autoBalance.SamePositionAsSongData)
                    {
                        fullPath = Path.GetDirectoryName(playingFileName);
                        if (!string.IsNullOrEmpty(playingArcFileName))
                        {
                            fullPath = Path.GetDirectoryName(playingArcFileName);
                        }
                    }
                    fn = Path.GetFileName(playingFileName);
                    if (!string.IsNullOrEmpty(playingArcFileName))
                    {
                        fn = Path.GetFileName(playingArcFileName);
                    }
                    fn += ".mbc";
                    if (!File.Exists(Path.Combine(fullPath, fn)))
                    {
                        fn = "";
                        fullPath = Common.settingFilePath;
                        fullPath = Path.Combine(fullPath, "MixerBalance");
                    }
                    else
                    {
                        fullPath = Path.Combine(fullPath, fn);
                    }
                }

                //ドライバごとのプリセットを読み込むモード
                if (setting.autoBalance.LoadDriverBalance && fn == "")
                {
                    switch (format)
                    {
                        case EnmFileFormat.VGM:
                            fn = "DriverBalance_VGM.mbc";
                            defMbc = Properties.Resources.DefaultVolumeBalance_VGM;
                            break;
                        case EnmFileFormat.XGM:
                            fn = "DriverBalance_XGM.mbc";
                            defMbc = Properties.Resources.DefaultVolumeBalance_XGM;
                            break;
                        case EnmFileFormat.ZGM:
                            fn = "DriverBalance_ZGM.mbc";
                            defMbc = Properties.Resources.DefaultVolumeBalance_ZGM;
                            break;
                        case EnmFileFormat.HES:
                            fn = "DriverBalance_HES.mbc";
                            defMbc = Properties.Resources.DefaultVolumeBalance_HES;
                            break;
                        case EnmFileFormat.NSF:
                            fn = "DriverBalance_NSF.mbc";
                            defMbc = Properties.Resources.DefaultVolumeBalance_NSF;
                            break;
                        case EnmFileFormat.NRT:
                            fn = "DriverBalance_NRT.mbc";
                            defMbc = Properties.Resources.DefaultVolumeBalance_NRT;
                            break;
                        case EnmFileFormat.MDR:
                            fn = "DriverBalance_MDR.mbc";
                            defMbc = Properties.Resources.DefaultVolumeBalance_MDR;
                            break;
                        case EnmFileFormat.MDX:
                            fn = "DriverBalance_MDX.mbc";
                            defMbc = Properties.Resources.DefaultVolumeBalance_MDX;
                            break;
                        case EnmFileFormat.MND:
                            fn = "DriverBalance_MND.mbc";
                            defMbc = Properties.Resources.DefaultVolumeBalance_MND;
                            break;
                        case EnmFileFormat.S98:
                            fn = "DriverBalance_S98.mbc";
                            defMbc = Properties.Resources.DefaultVolumeBalance_S98;
                            break;
                        case EnmFileFormat.SID:
                            fn = "DriverBalance_SID.mbc";
                            defMbc = Properties.Resources.DefaultVolumeBalance_SID;
                            break;
                        case EnmFileFormat.MUC:
                            fn = "DriverBalance_MUC.mbc";
                            defMbc = Properties.Resources.DefaultVolumeBalance_MUC;
                            break;
                        case EnmFileFormat.MUB:
                            fn = "DriverBalance_MUB.mbc";
                            defMbc = Properties.Resources.DefaultVolumeBalance_MUB;
                            break;
                    }

                    fullPath = Path.Combine(fullPath, fn);

                }

                if (fn == "") return;


                //存在確認。無い場合は作成。
                if (!File.Exists(fullPath) && defMbc != "") File.WriteAllText(fullPath, defMbc);
                //データフォルダに存在するファイルを読み込む
                balance = Setting.Balance.Load(fullPath);

                if (balance == null) return;

                //ミキサーバランス変更処理
                setting.balance = balance;
                if (frmMixer2 != null) frmMixer2.update();
                Application.DoEvents();

            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
        }

        private void ManualSavePresetMixerBalance(bool isDriverBalance, string playingFileName, string playingArcFileName, EnmFileFormat format, Setting.Balance balance)
        {
            if (!setting.autoBalance.UseThis) return;

            try
            {
                string fullPath = Common.settingFilePath;
                fullPath = Path.Combine(fullPath, "MixerBalance");
                if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);
                string fn = "";

                if (isDriverBalance)
                {
                    switch (format)
                    {
                        case EnmFileFormat.VGM:
                            fn = "DriverBalance_VGM.mbc";
                            break;
                        case EnmFileFormat.XGM:
                            fn = "DriverBalance_XGM.mbc";
                            break;
                        case EnmFileFormat.HES:
                            fn = "DriverBalance_HES.mbc";
                            break;
                        case EnmFileFormat.NSF:
                            fn = "DriverBalance_NSF.mbc";
                            break;
                        case EnmFileFormat.NRT:
                            fn = "DriverBalance_NRT.mbc";
                            break;
                        case EnmFileFormat.MDR:
                            fn = "DriverBalance_MDR.mbc";
                            break;
                        case EnmFileFormat.MDX:
                            fn = "DriverBalance_MDX.mbc";
                            break;
                        case EnmFileFormat.MND:
                            fn = "DriverBalance_MND.mbc";
                            break;
                        case EnmFileFormat.S98:
                            fn = "DriverBalance_S98.mbc";
                            break;
                        case EnmFileFormat.SID:
                            fn = "DriverBalance_SID.mbc";
                            break;
                        case EnmFileFormat.MUC:
                            fn = "DriverBalance_MUC.mbc";
                            break;
                        case EnmFileFormat.MUB:
                            fn = "DriverBalance_MUB.mbc";
                            break;
                    }

                    fullPath = Path.Combine(fullPath, fn);

                }
                else
                {

                }

                balance.Save(fullPath);
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }
        }

        public string SaveDriverBalance(Setting.Balance balance)
        {
            PlayList.music music = frmPlayList.getPlayingSongInfo();
            if(music==null)
            {
                throw new Exception("演奏情報が取得できませんでした。\r\n演奏中又は演奏完了直後に再度お試しください。");
            }

            EnmFileFormat fmt = music.format;
            ManualSavePresetMixerBalance(true, "", "", fmt, balance);

            return fmt.ToString();
        }

        public PlayList.music GetPlayingMusicInfo()
        {
            PlayList.music music = frmPlayList.getPlayingSongInfo();
            return music;
        }

        public static Action<HongliangSoft.Utilities.Gui.KeyboardHookedEventArgs> keyHookMeth=null;

        private void keyboardHook1_KeyboardHooked(object sender, HongliangSoft.Utilities.Gui.KeyboardHookedEventArgs e)
        {
            if (e.UpDown != HongliangSoft.Utilities.Gui.KeyboardUpDown.Up) return;

            if (keyHookMeth != null)
            {
                keyHookMeth(e);
                return;
            }

            string k = e.KeyCode.ToString();
            bool Shift = (Control.ModifierKeys & Keys.Shift) != 0;
            bool Ctrl = (Control.ModifierKeys & Keys.Control) != 0;
            bool Alt = (Control.ModifierKeys & Keys.Alt) != 0;
            Setting.KeyBoardHook.HookKeyInfo info;

            info = setting.keyBoardHook.Stop;
            if (info.Key == k && info.Shift == Shift && info.Ctrl == Ctrl && info.Alt == Alt)
            {
                stop();
                return;
            }

            info = setting.keyBoardHook.Pause;
            if (info.Key == k && info.Shift == Shift && info.Ctrl == Ctrl && info.Alt == Alt)
            {
                pause();
                return;
            }

            info = setting.keyBoardHook.Fadeout;
            if (info.Key == k && info.Shift == Shift && info.Ctrl == Ctrl && info.Alt == Alt)
            {
                fadeout();
                return;
            }

            info = setting.keyBoardHook.Prev;
            if (info.Key == k && info.Shift == Shift && info.Ctrl == Ctrl && info.Alt == Alt)
            {
                prev();
                return;
            }

            info = setting.keyBoardHook.Slow;
            if (info.Key == k && info.Shift == Shift && info.Ctrl == Ctrl && info.Alt == Alt)
            {
                slow();
                return;
            }

            info = setting.keyBoardHook.Play;
            if (info.Key == k && info.Shift == Shift && info.Ctrl == Ctrl && info.Alt == Alt)
            {
                play();
                return;
            }

            info = setting.keyBoardHook.Next;
            if (info.Key == k && info.Shift == Shift && info.Ctrl == Ctrl && info.Alt == Alt)
            {
                next();
                return;
            }

            info = setting.keyBoardHook.Fast;
            if (info.Key == k && info.Shift == Shift && info.Ctrl == Ctrl && info.Alt == Alt)
            {
                ff();
                return;
            }


        }


        private void CheckAndSetForm(Form frm)
        {
            Screen s = Screen.FromControl(frm);
            Rectangle rc = new Rectangle(frm.Location, frm.Size);
            if (s.WorkingArea.Contains(rc))
            {
                frm.Location = rc.Location;
                frm.Size = rc.Size;
                return;
            }
            else
            {
                frm.Location = new System.Drawing.Point(100, 100);
                return;
            }

        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Microsoft.Win32.SystemEvents.SessionEnding -= SystemEvents_SessionEnding;
        }

        private void tsmiOpenFile_Click(object sender, EventArgs e)
        {
            string[] fn = fileOpen(true);

            if (fn != null)
            {
                if (Audio.isPaused)
                {
                    Audio.Pause();
                }

                if (fn.Length == 1)
                {
                    frmPlayList.Stop();

                    //frmPlayList.AddList(fn[0]);
                    frmPlayList.getPlayList().AddFile(fn[0]);

                    if (Common.CheckExt(fn[0]) != EnmFileFormat.M3U && Common.CheckExt(fn[0]) != EnmFileFormat.ZIP)
                    {
                        loadAndPlay(0, 0, fn[0], "");
                        frmPlayList.setStart(-1);
                    }
                    oldParam = new MDChipParams();

                    frmPlayList.Play();
                }
                else
                {
                    frmPlayList.Stop();

                    try
                    {
                        foreach (string f in fn)
                        {
                            frmPlayList.getPlayList().AddFile(f);
                            //frmPlayList.AddList(f);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ForcedWrite(ex);
                    }
                }
            }


        }

        private void tsmiExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tsmiPlay_Click(object sender, EventArgs e)
        {
            play();
            oldParam = new MDChipParams();
        }

        private void tsmiStop_Click(object sender, EventArgs e)
        {
            frmPlayList.Stop();
            stop();
        }

        private void tsmiPause_Click(object sender, EventArgs e)
        {
            pause();
        }

        private void tsmiFadeOut_Click(object sender, EventArgs e)
        {
            fadeout();
            frmPlayList.Stop();
        }

        private void tsmiSlow_Click(object sender, EventArgs e)
        {
            slow();
        }

        private void tsmiFf_Click(object sender, EventArgs e)
        {
            ff();
        }

        private void tsmiNext_Click(object sender, EventArgs e)
        {
            next();
            oldParam = new MDChipParams();
        }

        private void tsmiPlayMode_Click(object sender, EventArgs e)
        {
            playMode();
        }

        private void tsmiOption_Click(object sender, EventArgs e)
        {
            openSetting();
        }

        private void tsmiPlayList_Click(object sender, EventArgs e)
        {
            dispPlayList();
        }

        private void tsmiOpenInfo_Click(object sender, EventArgs e)
        {
            openInfo();
        }

        private void tsmiOpenMixer_Click(object sender, EventArgs e)
        {
            openMixer();
        }

        private void tsmiChangeZoom_Click(object sender, EventArgs e)
        {
            if (sender == tsmiChangeZoomX1) setting.other.Zoom = 1;
            else if (sender == tsmiChangeZoomX2) setting.other.Zoom = 2;
            else if (sender == tsmiChangeZoomX3) setting.other.Zoom = 3;
            else if (sender == tsmiChangeZoomX4) setting.other.Zoom = 4;
            else
                setting.other.Zoom = (setting.other.Zoom == 4) ? 1 : (setting.other.Zoom + 1);

            changeZoom();
        }

        private void tsmiVST_Click(object sender, EventArgs e)
        {
            dispVSTList();
        }

        private void tsmiMIDIkbd_Click(object sender, EventArgs e)
        {
            openMIDIKeyboard();
        }

        private void tsmiKBrd_Click(object sender, EventArgs e)
        {
            showContextMenu();
        }

        private void RegisterDumpMenuItem_Click(object sender, EventArgs e) {
            if (sender == yM2612ToolStripMenuItem) OpenFormRegTest(0, EnmChip.YM2612);
            else if (sender == ym2151ToolStripMenuItem) OpenFormRegTest(0, EnmChip.YM2151);
            else if (sender == ym2203ToolStripMenuItem) OpenFormRegTest(0, EnmChip.YM2203);
            else if (sender == ym2413ToolStripMenuItem) OpenFormRegTest(0, EnmChip.YM2413);
            else if (sender == ym2608ToolStripMenuItem) OpenFormRegTest(0, EnmChip.YM2608);
            else if (sender == yMF278BToolStripMenuItem) OpenFormRegTest(0, EnmChip.YMF278B);
            else if (sender == yMF262ToolStripMenuItem) OpenFormRegTest(0, EnmChip.YMF262);
            else if (sender == yM2610ToolStripMenuItem) OpenFormRegTest(0, EnmChip.YM2610);
            else if (sender == qSoundToolStripMenuItem) OpenFormRegTest(0, EnmChip.QSound);
            else if (sender == segaPCMToolStripMenuItem) OpenFormRegTest(0, EnmChip.SEGAPCM);
            else if (sender == yMZ280BToolStripMenuItem) OpenFormRegTest(0, EnmChip.YMZ280B);
            else if (sender == sN76489ToolStripMenuItem) OpenFormRegTest(0, EnmChip.SN76489);
            else if (sender == aY8910ToolStripMenuItem) OpenFormRegTest(0, EnmChip.AY8910);
            else if (sender == c140ToolStripMenuItem) OpenFormRegTest(0, EnmChip.C140);
            else if (sender == c352ToolStripMenuItem) OpenFormRegTest(0, EnmChip.C352);
            else if (sender == yM3812ToolStripMenuItem) OpenFormRegTest(0, EnmChip.YM3812);
            else OpenFormRegTest(0);
        }
    }
}