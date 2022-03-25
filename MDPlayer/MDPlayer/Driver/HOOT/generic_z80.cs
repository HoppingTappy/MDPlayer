using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO.Compression;

namespace MDPlayer.Driver.HOOT
{
    public class generic_z80 : baseDriver
    {
        public override GD3 getGD3Info(byte[] buf, uint vgmGd3)
        {
            GD3 ret = new GD3();
            return ret;
        }

        public override bool init(byte[] vgmBuf, ChipRegister chipRegister, EnmModel model, EnmChip[] useChip, uint latency, uint waitTime)
        {
            this.chipRegister = chipRegister;
            LoopCounter = 0;
            vgmCurLoop = 0;
            this.model = model;
            vgmFrameCounter = -latency - waitTime;
            bankWriteCount = 0;
            bankAddr = 0;
            isWhileInt = false;
            ym2612adr[0] = 0;
            ym2612adr[1] = 0;

            var pathLength = Common.getLE32(vgmBuf, 0);
            var pathBin = new byte[pathLength];
            Array.Copy(vgmBuf, 4, pathBin, 0, pathLength);
            var fileName = System.Text.Encoding.Unicode.GetString(pathBin);
            var dirPath = Path.GetDirectoryName(fileName) + Path.DirectorySeparatorChar;
            var xmlBinLength = vgmBuf.Length - 4 - pathLength;
            var xmlBin = new byte[xmlBinLength];
            Array.Copy(vgmBuf, 4 + pathLength, xmlBin, 0, xmlBinLength);
            string xml = Encoding.GetEncoding("Shift_JIS").GetString(xmlBin);
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            foreach (XmlElement option in xmlDoc.SelectSingleNode("hoot/options"))
            {
                var name = option.Attributes["name"].Value;
                var value = option.Attributes["value"].Value;
                options[name] = (uint)Common.StrToInt(value);
            }

            var romName = xmlDoc.SelectSingleNode("hoot/romlist/rom").InnerText;
            var zipName = xmlDoc.SelectSingleNode("hoot/romlist").Attributes["archive"].Value;
            zipName = dirPath + zipName;
            
            if (Path.GetExtension(zipName).ToLower() != ".zip")
            {
                zipName += ".zip";
            }
            
            var zipPath = zipName;

            using (var archive = ZipFile.OpenRead(zipPath))
            {
                MemoryStream ms = new MemoryStream();
                var fileEntry = archive.GetEntry(romName);
                rom = new byte[fileEntry.Length];
                fileEntry.Open().CopyTo(ms);
                rom = ms.ToArray();
            }

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
                        oneFrameMain();
                    }
                    else
                    {
                        vgmFrameCounter++;
                    }
                }
                //Stopped = !IsPlaying();
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);

            }

        }

        private void oneFrameMain()
        {
            try
            {
                Counter++;
                vgmFrameCounter++;

                if (vgmFrameCounter % (Common.VGMProcSampleRate / 60) == 0)
                {
                    interrupt();
                }
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);

            }
        }

        private void interrupt()
        {
            if (z80.Registers.IFF1 == 1)
            {
                isWhileInt = true;
                z80.ExecuteCall(0x0038);
            }
            z80.Continue();
        }

        private Z80Processor z80;
        private static byte[] rom = null;
        private static int bankWriteCount = 0;
        private static int bankAddr = 0;
        private static bool isWhileInt = false;
        private static bool isWhileInit = false;
        private static int returnAddr = -1;
        public byte reqNumber = 0x00;

        private static byte[] ym2612adr = new byte[2];
        internal static uint masterClock = 53693175;
        internal static uint baseclockYM2612 = (uint)Math.Round(masterClock / 7.0f);
        internal static uint baseclockSN76489 = (uint)Math.Round(masterClock / 15.0f);
        internal static uint samplingRateYM2612 = (uint)Math.Round(masterClock / 7.0f / 24 / 6);
        internal static decimal clockZ80 = masterClock / 15 /1000000;

        private static Dictionary<string, dynamic> options = new Dictionary<string, dynamic>(){
            {"topaddr", 0x0},
            {"subreqaddr", 0x0},
            {"reqaddr", 0x0},
            {"data1", 0x2000},
            {"data2", 0x2000},
            {"data3", 0x2000},
            {"data4", 0x2000},
            {"data5", 0x2000},
            {"data6", 0x2000},
            {"data7", 0x2000},
            {"data8", 0x2000},
            {"size1", 0x0},
            {"size2", 0x0},
            {"size3", 0x0},
            {"size4", 0x0},
            {"size5", 0x0},
            {"size6", 0x0},
            {"size7", 0x0},
            {"size8", 0x0},
            {"trigadr", 0xffffffff},
            {"trigvalue", 0xffffffff},
            {"trigsubadr", 0xffffffff},
            {"trigsubval", 0xffffffff},
            {"timeradr", 0xffffffff},
            {"timervalue", 0xffffffff},
            {"reqz80reset", 0xffffffff},
            {"stop", 0x0},
        };

        private void Run(byte[] vgmBuf)
        {
            z80 = new Z80Processor();
            z80.ClockSynchronizer = null;
            z80.AutoStopOnRetWithStackEmpty = true;
            //z80.BeforeInstructionFetch += Z80OnBeforeInstructionFetch;
            z80.AfterInstructionExecution += Z80OnAfterInstructionFetch;
            z80.MemoryAccess += Z80MemoryAccess;
            z80.ClockFrequencyInMHz = clockZ80;

            //Stopwatch sw = new Stopwatch();
            //sw.Start();


            z80.Reset();
            z80.Memory.SetContents(0x0000, rom, (int)options["topaddr"], (int)options["size1"]);
            z80.Registers.PC = 0x0000;
            isWhileInit = true;
            z80.Continue();
        }

        public string PlayingFileName { get; internal set; }

        private void Z80MemoryAccess(object sender, MemoryAccessEventArgs args)
        {
            switch (args.EventType)
            {
                case MemoryAccessEventType.AfterMemoryWrite:
                    switch (args.Address)
                    {
                        case 0x7f11:
                            chipRegister.setSN76489Register(0, args.Value, model);
                            break;
                        case 0x4000:
                        case 0x4002:
                            ym2612adr[args.Address>>1&1] = args.Value;
                            break;
                        case 0x4001:
                        case 0x4003:
                            chipRegister.setYM2612Register(0, args.Address >> 1 & 1, ym2612adr[args.Address >> 1 & 1], args.Value, model, -1);
                            break;
                        case 0x6000:
                            bankAddr |= (args.Value&1) << (bankWriteCount + 15);
                            bankWriteCount++;
                            if (bankWriteCount == 9)
                            {
                                z80.Memory.SetContents(0x8000, rom, bankAddr, 0x8000);
                                bankWriteCount = 0;
                                bankAddr = 0;
                            }
                            break;
                    }
                    break;
                case MemoryAccessEventType.AfterMemoryRead:
                    switch (args.Address)
                    {
                        case 0x4000:
                        case 0x4001:
                        case 0x4002:
                        case 0x4003:
                            args.Value = 0x00;
                            break;
                    }
                    break;
            }
        }
        //private void Z80OnBeforeInstructionFetch(object sender, BeforeInstructionFetchEventArgs args)
        private void Z80OnAfterInstructionFetch(object sender, AfterInstructionExecutionEventArgs args)
        {
            var z80 = (IZ80Processor)sender;
            if (z80.Registers.IFF1 == 1 && isWhileInt == false && isWhileInit == true)
            {
                isWhileInit = false;
                z80.Memory[(int)(options["reqaddr"] & 0xffff)] = (byte)(reqNumber & 0xff);
                z80.Memory[(int)(options["trigadr"] & 0xffff)] = (byte)(options["trigvalue"] & 0xff);
                returnAddr = z80.Registers.PC;
                args.ExecutionStopper.Stop();


            } else if (z80.Registers.PC == returnAddr){
                args.ExecutionStopper.Stop();
            }
        }
    }
}
