﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDPlayer.Driver.MUCOM88.ver1_0
{
    public class ssgdat
    {

        //ORG 5E00H

        public byte[] dat = new byte[] {
            8,0			// ﾉｰﾏﾙ
            ,255,255,255,255,0,255 // E
            ,1,0,0,0,0,0,0,0,

            8,0			//ｺﾅﾐ(1)
            ,255,255,255,200,0,10
            ,1,0,0,0,0,0,0,0,

            8,0			//ｺﾅﾐ(2)
            ,255,255,255,200,1,10
            ,1,0,0,0,0,0,0,0,

            8,0			//ｺﾅﾐ+LFO(1)
            ,255,255,255,190,0,10
            ,0,16,1,25,0,4,0,0,

            8,0			//ｺﾅﾐ+LFO(2)
            ,255,255,255,190,1,10
            ,0,16,1,25,0,4,0,0,

            8,0			//ｺﾅﾐ(3)
            ,255,255,255,170,0,10
            ,1,0,0,0,0,0,0,0,

            //5
            8,0			//ｾｶﾞ ﾀｲﾌﾟ
            ,40,70,14,190,0,15
            ,0,16,1,24,0,5,0,0,

            8,0			//ｽﾄﾘﾝｸﾞ ﾀｲﾌﾟ
            ,120,030,255,255,0,10
            ,0,16,1,25,0,4,0,0,

            8,0			//ﾋﾟｱﾉ･ﾊｰﾌﾟ ﾀｲﾌﾟ
            ,255,255,255,225,8,15
            ,1,0,0,0,0,0,0,0,

            1,0			//ｸﾛｰｽﾞ ﾊｲﾊｯﾄ
            ,255,255,255,1,255,255
            ,1,0,0,0,0,0,0,0,

            1,0			//ｵｰﾌﾟﾝ ﾊｲﾊｯﾄ
            ,255,255,255,200,8,255
            ,1,0,0,0,0,0,0,0,

            //10
            8,0			//ｼﾝｾﾀﾑ･ｼﾝｾｷｯｸ
            ,255,255,255,220,20,8
            ,0,1,1,0x2C,1,0x0FF,0,0,

            8,0			//UFO
            ,255,255,255,255,0,10
            ,0,1,1,0x70,0x0FE,4,0,0,

            8,0			//FALLING
            ,255,255,255,255,0,10
            ,0,1,1,0x50,00,255,0,0,

            8,0			//ﾎｲｯｽﾙ
            ,120,80,255,255,0,255
            ,0,1,1,06,0x0FF,1,0,0,

            8,0			//BOM!
            ,255,255,255,220,0,255
            ,0,1,1,0xB8,0x0B,255,0,0

        };

        public void SetSSGDAT(Mem mem)
        {
            ushort adr = 0x5e00;
            foreach(byte d in dat)
            {
                mem.LD_8(adr++, d);
            }
        }

    }
}
