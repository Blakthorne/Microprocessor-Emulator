using System;
using System.Collections.Generic;
using System.Text;

namespace armsim.Execute
{
    class ExecMultiply : Execute
    {
        public static void ExecuteMultiply()
        {
            MUL mul = new MUL();
        }
    }

    class MUL : ExecDataProcessing
    {
        public MUL()
        {
            uint first = cpu.registers.ReadWord(instr.rs_M * REGS_MULTIPLIER);
            uint second = cpu.registers.ReadWord(instr.rm_M * REGS_MULTIPLIER);
            cpu.registers.WriteWord(instr.rd_M * REGS_MULTIPLIER, first * second);
        }
    }
}
