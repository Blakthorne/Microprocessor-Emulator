using System;
using System.Collections.Generic;
using System.Text;

namespace armsim.Execute
{
    class ExecLoadStore : Execute
    {
        public static void ExecuteLoadStore()
        {
            if (instr.l_LS == 1)
            {
                LDR ldr = new LDR();
            }
            else
            {
                STR str = new STR();
            }
        }

        public static uint FindEA()
        {
            uint ea;
            if (instr.u_LS == 1)
            {
                ea = FindPosEA();
            }
            else
            {
                ea = FindNegEA();
            }
            return ea;
        }

        public static uint FindPosEA()
        {
            uint ea = 0;
            uint baseReg = cpu.registers.ReadWord(instr.rn_LS * REGS_MULTIPLIER);

            if (instr.type == 0b010)
            {
                ea = baseReg + instr.imm_LS;
            }
            else
            {
                if ((instr.shiftAmount_LS == 0) &&    // register offset
                    (instr.shiftCode_LS == 0))
                {
                    ea = baseReg + cpu.registers.ReadWord(instr.rm_LS * REGS_MULTIPLIER);
                }
                else    // scaled register offset
                {
                    switch (instr.shiftCode_LS)
                    {
                        case 0b00:    // logical shift left
                            ea = baseReg + (cpu.registers.ReadWord(instr.rm_LS * REGS_MULTIPLIER) << (int)instr.shiftAmount_LS);
                            break;
                        case 0b01:    // logical shift right
                            ea = baseReg + (cpu.registers.ReadWord(instr.rm_LS * REGS_MULTIPLIER) >> (int)instr.shiftAmount_LS);
                            break;
                        case 0b10:    // arithmetic shift right
                            ea = baseReg + ((uint)(unchecked((int)cpu.registers.ReadWord(instr.rm_LS * REGS_MULTIPLIER)) >> (int)instr.shiftAmount_LS));
                            break;
                        case 0b11:    // rotate right
                            ea = baseReg + ((instr.imm_LS >> (int)instr.shiftAmount_LS) | (cpu.registers.ReadWord(instr.rm_LS * REGS_MULTIPLIER) << (32 - (int)instr.shiftAmount_LS)));
                            break;
                        default:
                            return 0;
                    }
                }
            }
            return ea;
        }

        public static uint FindNegEA()
        {
            uint ea = 0;
            uint baseReg = cpu.registers.ReadWord(instr.rn_LS * REGS_MULTIPLIER);

            if (instr.type == 0b010)
            {
                ea = baseReg - instr.imm_LS;
            }
            else
            {
                if ((instr.shiftAmount_LS == 0) &&    // register offset
                    (instr.shiftCode_LS == 0))
                {
                    ea = baseReg - cpu.registers.ReadWord(instr.rm_LS * REGS_MULTIPLIER);
                }
                else    // scaled register offset
                {
                    switch (instr.shiftCode_LS)
                    {
                        case 0b00:    // logical shift left
                            ea = baseReg - (cpu.registers.ReadWord(instr.rm_LS * REGS_MULTIPLIER) << (int)instr.shiftAmount_LS);
                            break;
                        case 0b01:    // logical shift right
                            ea = baseReg - (cpu.registers.ReadWord(instr.rm_LS * REGS_MULTIPLIER) >> (int)instr.shiftAmount_LS);
                            break;
                        case 0b10:    // arithmetic shift right
                            ea = baseReg - ((uint)(unchecked((int)cpu.registers.ReadWord(instr.rm_LS * REGS_MULTIPLIER)) >> (int)instr.shiftAmount_LS));
                            break;
                        case 0b11:    // rotate right
                            ea = baseReg - ((instr.imm_LS >> ((int)instr.shiftAmount_LS) * 2) | (cpu.registers.ReadWord(instr.rm_LS * REGS_MULTIPLIER) << ((32 - (int)instr.shiftAmount_LS) * 2)));
                            break;
                        default:
                            return 0;
                    }
                }
            }
            return ea;
        }
    }

    class LDR : ExecLoadStore
    {
        public LDR()
        {
            uint ea = FindEA();
            
            if (instr.b_LS == 1)
            {
                cpu.registers.WriteWord(instr.rd_LS * REGS_MULTIPLIER, cpu.ram.ReadByte(ea));
            }
            else
            {
                cpu.registers.WriteWord(instr.rd_LS * REGS_MULTIPLIER, cpu.ram.ReadWord(ea));
            }

            if (instr.w_LS == 1)
            {
                cpu.registers.WriteWord(instr.rn_LS * REGS_MULTIPLIER, ea);
            }
        }
    }

    class STR : ExecLoadStore
    {
        public STR()
        {
            uint ea = FindEA();

            if (instr.b_LS == 1)
            {
                cpu.ram.WriteByte(ea, cpu.registers.ReadByte(instr.rd_LS * REGS_MULTIPLIER));
            }
            else
            {
                cpu.ram.WriteWord(ea, cpu.registers.ReadWord(instr.rd_LS * REGS_MULTIPLIER));
            }

            if (instr.w_LS == 1)
            {
                cpu.registers.WriteWord(instr.rn_LS * REGS_MULTIPLIER, ea);
            }
        }
    }
}
