using System;
using System.Collections.Generic;
using System.Text;

namespace armsim.Execute
{
    class Execute
    {
        protected static Instruction instr;
        protected static CPU cpu;
        public const int REGS_MULTIPLIER = 4;
        public static bool N;
        public static bool Z;
        public static bool C;
        public static bool V;

        public static void ExecuteInstruction(Instruction _instr, CPU _cpu)
        {
            instr = _instr;
            cpu = _cpu;

            N = cpu.registers.TestFlag(16 * 4, 31);
            Z = cpu.registers.TestFlag(16 * 4, 30);
            C = cpu.registers.TestFlag(16 * 4, 29);
            V = cpu.registers.TestFlag(16 * 4, 28);

            if (FindIfContinue())
            {
                instr.executed = true;
                switch (instr.typeInstr)
                {
                    case Instruction.TypeInstr.DataProcessing:
                        ExecDataProcessing.ExecuteDataProcessing();
                        break;
                    case Instruction.TypeInstr.LoadStore:
                        ExecLoadStore.ExecuteLoadStore();
                        break;
                    case Instruction.TypeInstr.LoadStoreMultiple:
                        ExecLoadStoreMultiple.ExecuteLoadStoreMultiple();
                        break;
                    case Instruction.TypeInstr.Multiply:
                        ExecMultiply.ExecuteMultiply();
                        break;
                    case Instruction.TypeInstr.Branch:
                        ExecBranch.ExecuteBranch();
                        break;
                    case Instruction.TypeInstr.BX:
                        ExecBX.ExecuteBX();
                        break;
                    default:
                        break;
                }
            }
        }

        public static bool FindIfContinue()
        {
            switch (instr.conditionFlags)
            {
                case 0b0000:  // Equal
                    return Z;
                case 0b0001:  // Not equal
                    return !Z;
                case 0b0010:  // Carry set/unsigned higher or same
                    return C;
                case 0b0011:  // Carry clear/unsigned lower
                    return !C;
                case 0b0100:  // Minus/negative
                    return N;
                case 0b0101:  // Plus/positive or zero
                    return !N;
                case 0b0110:  // Overflow
                    return V;
                case 0b0111:  // No overflow
                    return !V;
                case 0b1000:  // Unsigned higher
                    return C && !Z;
                case 0b1001:  // Unsigned lower or same
                    return !C || Z;
                case 0b1010:  // Signed greater than or equal
                    return N == V;
                case 0b1011:  // Signed less than
                    return N != V;
                case 0b1100:  // Signed greater than
                    return !Z && (N == V);
                case 0b1101:  // Signed less than or equal
                    return Z || (N != V);
                case 0b1110:  // always
                    return true;
                default:
                    instr.executed = false;
                    return false;
            }
        }
    }
}
