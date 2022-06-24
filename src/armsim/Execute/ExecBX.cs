using System;
using System.Collections.Generic;
using System.Text;

namespace armsim.Execute
{
    class ExecBX : Execute
    {
        public static void ExecuteBX()
        {
            if (instr.bit0_bx == 1)
            {
                cpu.registers.SetFlag(16 * 4, 5, true);
            }
            else
            {
                cpu.registers.SetFlag(16 * 4, 5, false);
            }

            cpu.registers.WriteWord(15 * REGS_MULTIPLIER, (cpu.registers.ReadWord(instr.rm_bx * REGS_MULTIPLIER) & 0xFFFFFFFE) + 4);
        }
    }
}
