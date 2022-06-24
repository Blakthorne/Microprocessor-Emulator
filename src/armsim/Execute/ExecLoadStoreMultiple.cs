using System;
using System.Collections.Generic;
using System.Text;

namespace armsim.Execute
{
    class ExecLoadStoreMultiple : Execute
    {
        public static void ExecuteLoadStoreMultiple()
        {
            if (instr.l_LSM == 1)
            {
                LDM ldm = new LDM();
            }
            else
            {
                STM stm = new STM();
            }
        }

        public static uint FindEA()
        {
            return 0;
        }
    }

    class LDM : ExecLoadStoreMultiple
    {
        public LDM()
        {
            uint ea = 0;

            // full descending (increment after)
            if ((instr.l_LSM == 1) &&
                (instr.p_LSM == 0) &&
                (instr.u_LSM == 1))
            {
                ea = cpu.registers.ReadWord(instr.rn_LSM * REGS_MULTIPLIER);

                foreach (int reg in instr.regsList_LSM)
                {
                    cpu.registers.WriteWord((uint)reg * REGS_MULTIPLIER, cpu.ram.ReadWord(ea));
                    ea += 4;
                }
            }


            if (instr.w_LSM == 1)
            {
                cpu.registers.WriteWord(instr.rn_LSM * REGS_MULTIPLIER, cpu.registers.ReadWord(instr.rn_LSM * REGS_MULTIPLIER) + ((uint)instr.regsList_LSM.Count * 4));
            }
        }

    }

    class STM : ExecLoadStoreMultiple
    {
        public STM()
        {
            uint ea = 0;
            // full descending (decrement before)
            if ((instr.l_LSM == 0) &&
                (instr.p_LSM == 1) &&
                (instr.u_LSM == 0))
            {
                ea = cpu.registers.ReadWord(instr.rn_LSM * REGS_MULTIPLIER) - (uint)(instr.regsList_LSM.Count * 4);

                foreach (int reg in instr.regsList_LSM)
                {
                    cpu.ram.WriteWord(ea, cpu.registers.ReadWord((uint)reg * REGS_MULTIPLIER));
                    ea += 4;
                }
            }


            if (instr.w_LSM == 1)
            {
                cpu.registers.WriteWord(instr.rn_LSM * REGS_MULTIPLIER, cpu.registers.ReadWord(instr.rn_LSM * REGS_MULTIPLIER) - ((uint)instr.regsList_LSM.Count * 4));
            }
        }

    }
}
