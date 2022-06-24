using System;
using System.Collections.Generic;
using System.Text;

namespace armsim.Execute
{
    class ExecBranch : Execute
    {
        public static void ExecuteBranch()
        {
            if (instr.l_b == 0)
            {
                B b = new B();
            }
            else
            {
                BL bl = new BL();
            }
        }

        //public static uint FindTargetAddr()
        //{
        //    int isNeg = instr.offset_b & 800000;
        //    if (isNeg != 0)
        //    {
        //        instr.offset_b |= 0x3f000000;
        //    }
        //    int left_shifted = instr.offset_b << 2;
        //    int pc_read = (int)cpu.registers.ReadWord(15 * 4);
        //    return (uint)(left_shifted + pc_read);
        //}
    }

    class B : ExecBranch
    {
        public B()
        {
            cpu.registers.WriteWord(15 * 4, (uint)instr.target_b);
        }
    }

    class BL : ExecBranch
    {
        public BL()
        {
            cpu.registers.WriteWord(14 * 4, cpu.registers.ReadWord(15 * 4) - 4);
            cpu.registers.WriteWord(15 * 4, (uint)instr.target_b);
        }
    }
}
