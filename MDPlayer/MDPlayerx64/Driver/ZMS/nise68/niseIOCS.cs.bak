using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDPlayer.Driver.ZMS.nise68
{
    public class niseIOCS
    {
        private Memory68 mem;
        private Register68 reg;
        private Action[] cmdTbl;

        public niseIOCS(Memory68 mem, Register68 reg)
        {
            this.mem = mem;
            this.reg = reg;
            cmdTbl = new Action[]
            {
                //00
                null ,null,null,null, null,null,null,null,  null,null,null,null, null,null,null,null,
                //10
                null ,null,null,null, null,null,null,null,  null,null,null,null, null,null,null,null,
                //20
                null ,null,null,null, null,null,null,null,  null,null,null,null, null,null,null,null,
                //30
                null ,null,null,null, null,null,null,null,  null,null,null,null, null,null,null,null,
                //40
                null ,null,null,null, null,null,null,null,  null,null,null,null, null,null,null,null,
                //50
                null ,null,null,null, null,null,null,null,  null,null,null,null, null,null,null,null,
                //60
                _ADPCMOUT ,null,null,null, null,null,null,null,  null,null,null,null, null,null,null,null,
                //70
                null ,null,null,null, null,null,null,null,  null,null,null,null, null,null,null,null,
                //80
                _B_INTVCS ,_B_SUPER,null,null, null,null,null,null,  null,null,null,null, null,null,null,null,
                //90
                null ,null,null,null, null,null,null,null,  null,null,null,null, null,null,null,null,
                //a0
                null ,null,null,null, null,null,null,null,  null,null,null,null, null,null,null,null,
                //b0
                null ,null,null,null, null,null,null,null,  null,null,null,null, null,null,null,null,
                //c0
                null ,null,null,null, null,null,null,null,  null,null,null,null, null,null,null,null,
                //d0
                null ,null,null,null, null,null,null,null,  null,null,null,null, null,null,null,null,
                //e0
                null ,null,null,null, null,null,null,null,  null,null,null,null, null,null,null,null,
                //f0
                null ,null,null,null, null,null,null,null,  null,null,null,null, null,null,null,null,
            };
        }

        public void Call()
        {
            byte n = (byte)reg.D[0];
            if (cmdTbl[n] != null)
            {
                cmdTbl[n]();
            }
            else
            {
                throw new NotImplementedException(string.Format("IOCS 未実装!! [{0:x04}]", n));
            }
        }

        private void _ADPCMOUT()
        {
            Log.WriteLine(LogLevel.Trace, "IOCS _ADPCMOUT");

            reg.SR = mem.PeekW(reg.SSP);
            reg.SSP += 2;
            reg.PC = mem.PeekL(reg.SSP);
            reg.SSP += 4;

            ushort frq_pan = reg.GetDw(1);
            uint length = reg.GetDl(2);
            uint ptr = reg.GetAl(1);
        }

        private void _B_INTVCS()
        {
            Log.WriteLine(LogLevel.Trace, "IOCS _B_INTVCS");

            reg.SR = mem.PeekW(reg.SSP);
            reg.SSP += 2;
            reg.PC = mem.PeekL(reg.SSP);
            reg.SSP += 4;

            ushort vctnum = reg.GetDw(1);
            uint ptr = reg.GetAl(1);
            reg.SetDl(0, mem.PeekL((uint)(vctnum * 4)));
            mem.PokeL((uint)(vctnum * 4), ptr);
        }

        private void _B_SUPER()
        {
            Log.WriteLine(LogLevel.Trace, "IOCS _B_SUPER");

            reg.SR = mem.PeekW(reg.SSP);
            reg.SSP += 2;
            reg.PC = mem.PeekL(reg.SSP);
            reg.SSP += 4;

            if (reg.A[1] == 0)
            {
                reg.SR |= 0x2000;//super
                //reg.D[0] = -;
            }
            else
            {
                reg.SR &= 0xdfff;//user
                reg.D[0] = 0;
            }

        }

    }
}
