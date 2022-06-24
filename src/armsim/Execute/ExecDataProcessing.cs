using System;
using System.Collections.Generic;
using System.Text;

namespace armsim.Execute
{
    class ExecDataProcessing : Execute
    {
        public static void ExecuteDataProcessing()
        {
            switch (instr.opcode_DP)
            {
                case 0b1101:    // MOV
                    MOV mov = new MOV();
                    break;
                case 0b1111:    // MVN
                    MVN mvn = new MVN();
                    break;
                case 0b0100:    // ADD
                    ADD add = new ADD();
                    break;
                case 0b0010:    // SUB
                    SUB sub = new SUB();
                    break;
                case 0b0011:    // RSB
                    RSB rsb = new RSB();
                    break;
                case 0b0000:    // AND
                    AND and = new AND();
                    break;
                case 0b1100:    // ORR
                    ORR orr = new ORR();
                    break;
                case 0b0001:    // EOR
                    EOR eor = new EOR();
                    break;
                case 0b1110:    // BIC
                    BIC bic = new BIC();
                    break;
                case 0b1010:    // CMP
                    CMP cmp = new CMP();
                    break;
                default:
                    break;
            }
        }

        public static uint FindOperand2()
        {
            if (instr.typeDP == Instruction.TypeDP.Imm)
            {
                // rotate right
                return (instr.imm_DP >> ((int)instr.rotate_DP) * 2) | (instr.imm_DP << ((32 - (int)instr.rotate_DP) * 2));
            }
            else if (instr.typeDP == Instruction.TypeDP.ShiftByImm)
            {
                switch (instr.shiftCode_DP)
                {
                    case 0b00:    // logical shift left
                        return cpu.registers.ReadWord(instr.rm_DP * REGS_MULTIPLIER) << (int)instr.shiftAmount_DP;
                    case 0b01:    // logical shift right
                        return cpu.registers.ReadWord(instr.rm_DP * REGS_MULTIPLIER) >> (int)instr.shiftAmount_DP;
                    case 0b10:    // arithmetic shift right
                        return (uint)(unchecked((int)cpu.registers.ReadWord(instr.rm_DP * REGS_MULTIPLIER)) >> (int)instr.shiftAmount_DP);
                    case 0b11:    // rotate right
                        return (cpu.registers.ReadWord(instr.rm_DP * REGS_MULTIPLIER) >> (int)instr.shiftAmount_DP) | (cpu.registers.ReadWord(instr.rm_DP * REGS_MULTIPLIER) << (32 - (int)instr.shiftAmount_DP));
                    default:
                        return 0;
                }
            }
            else if (instr.typeDP == Instruction.TypeDP.ShiftByReg)
            {
                switch (instr.shiftCode_DP)
                {
                    case 0b00:    // logical shift left
                        return cpu.registers.ReadWord(instr.rm_DP * REGS_MULTIPLIER) << (int)cpu.registers.ReadWord(instr.rs_DP * REGS_MULTIPLIER);
                    case 0b01:    // logical shift right
                        return cpu.registers.ReadWord(instr.rm_DP * REGS_MULTIPLIER) >> (int)cpu.registers.ReadWord(instr.rs_DP * REGS_MULTIPLIER);
                    case 0b10:    // arithmetic shift right
                        return (uint)(unchecked((int)cpu.registers.ReadWord(instr.rm_DP * REGS_MULTIPLIER)) >> (int)cpu.registers.ReadWord(instr.rs_DP * REGS_MULTIPLIER));
                    case 0b11:    // rotate right
                        return (cpu.registers.ReadWord(instr.rm_DP * REGS_MULTIPLIER) >> (int)cpu.registers.ReadWord(instr.rs_DP * REGS_MULTIPLIER)) | (cpu.registers.ReadWord(instr.rm_DP * REGS_MULTIPLIER) << (32 - (int)cpu.registers.ReadWord(instr.rs_DP * REGS_MULTIPLIER)));
                    default:
                        return 0;
                }
            }
            else { return cpu.registers.ReadWord(instr.rm_DP * REGS_MULTIPLIER); }
        }
    }

    class AND : ExecDataProcessing
    {
        public AND()
        {
            uint first = cpu.registers.ReadWord(instr.rn_DP * REGS_MULTIPLIER);
            uint second = FindOperand2();
            cpu.registers.WriteWord(instr.rd_DP * REGS_MULTIPLIER, first & second);
        }
    }

    class ADD : ExecDataProcessing
    {
        public ADD()
        {
            uint first = cpu.registers.ReadWord(instr.rn_DP * REGS_MULTIPLIER);
            uint second = FindOperand2();
            cpu.registers.WriteWord(instr.rd_DP * REGS_MULTIPLIER, first + second);
        }
    }

    class BIC : ExecDataProcessing
    {
        public BIC()
        {
            uint first = cpu.registers.ReadWord(instr.rn_DP * REGS_MULTIPLIER);
            uint second = FindOperand2();
            cpu.registers.WriteWord(instr.rd_DP * REGS_MULTIPLIER, first & ~second);
        }
    }

    class CMP : ExecDataProcessing
    {
        const uint CPSR = 16 * 4;

        public CMP()
        {
            uint eval = cpu.registers.ReadWord(instr.rn_DP * REGS_MULTIPLIER) - FindOperand2();
            uint eval_bit31 = eval >> 31;

            // set N flag
            if (eval_bit31 == 1)
            {
                cpu.registers.SetFlag(CPSR, 31, true);
            }
            else
            {
                cpu.registers.SetFlag(CPSR, 31, false);
            }

            // set Z flag
            if (eval == 0)
            {
                cpu.registers.SetFlag(CPSR, 30, true);
            }
            else
            {
                cpu.registers.SetFlag(CPSR, 30, false);
            }

            // set C flag
            if (FindOperand2() <= cpu.registers.ReadWord(instr.rn_DP * REGS_MULTIPLIER))
            {
                cpu.registers.SetFlag(CPSR, 29, true);
            }
            else
            {
                cpu.registers.SetFlag(CPSR, 29, false);
            }

            // set V flag
            if ((((cpu.registers.ReadWord(instr.rn_DP * REGS_MULTIPLIER) >> 31) ^ (FindOperand2() >> 31)) == 1) &&
                 (((cpu.registers.ReadWord(instr.rn_DP * REGS_MULTIPLIER) >> 31) ^ eval_bit31) == 1))
            {
                cpu.registers.SetFlag(CPSR, 28, true);
            }
            else
            {
                cpu.registers.SetFlag(CPSR, 28, false);
            }
        }
    }

    class EOR : ExecDataProcessing
    {
        public EOR()
        {
            uint first = cpu.registers.ReadWord(instr.rn_DP * REGS_MULTIPLIER);
            uint second = FindOperand2();
            cpu.registers.WriteWord(instr.rd_DP * REGS_MULTIPLIER, first ^ second);
        }
    }

    class MOV : ExecDataProcessing
    {
        public MOV()
        {
            cpu.registers.WriteWord(instr.rd_DP * REGS_MULTIPLIER, FindOperand2());
        }
    }

    class MVN : ExecDataProcessing
    {
        public MVN()
        {
            cpu.registers.WriteWord(instr.rd_DP * REGS_MULTIPLIER, ~FindOperand2());
        }
    }

    class ORR : ExecDataProcessing
    {
        public ORR()
        {
            uint first = cpu.registers.ReadWord(instr.rn_DP * REGS_MULTIPLIER);
            uint second = FindOperand2();
            cpu.registers.WriteWord(instr.rd_DP * REGS_MULTIPLIER, first | second);
        }
    }

    class RSB : ExecDataProcessing
    {
        public RSB()
        {
            uint first = cpu.registers.ReadWord(instr.rn_DP * REGS_MULTIPLIER);
            uint second = FindOperand2();
            cpu.registers.WriteWord(instr.rd_DP * REGS_MULTIPLIER, second - first);
        }
    }

    class SUB : ExecDataProcessing
    {
        public SUB()
        {
            uint second = cpu.registers.ReadWord(instr.rn_DP * REGS_MULTIPLIER);
            uint first = FindOperand2();
            cpu.registers.WriteWord(instr.rd_DP * REGS_MULTIPLIER, second - first);
        }
    }
}
