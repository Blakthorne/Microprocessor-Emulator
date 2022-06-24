using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace armsim
{
    class DataProcessing : Instruction
    {
        public static uint[] opcodes = {
            0x0, // AND
            0x1, // EOR
            0x2, // SUB
            0x3, // RSB
            0x4, // ADD
            0x5, // ADC
            0x6, // SBC
            0x7, // RSC
            0x8, // TST
            0x9, // TEQ
            0xA, // CMP
            0xB, // CMN
            0xC, // ORR
            0xD, // MOV
            0xE, // BIC
            0xF  // MVN
        };

        public static string[] opcodeNames = {
            "and",
            "eor",
            "sub",
            "rsb",
            "add",
            "adc",
            "sbc",
            "rsc",
            "tst",
            "teq",
            "cmp",
            "cmn",
            "orr",
            "mov",
            "bic",
            "mvn"
        };

        /// <summary>
        /// Represents the information found in data processing instruction
        /// </summary>
        public DataProcessing(uint instr) : base(instr)
        {
            opcode_DP = FindOpcode();
            s_DP = FindS();
            rn_DP = FindRn();
            rd_DP = FindRd();

            if (type == 0b000)      // requires a shift either by register or immediate
            {
                shiftCode_DP = FindShiftCode();
                rm_DP = FindRm();

                if (bit4 == 0b0) // requires a shift by immediate
                {
                    typeDP = TypeDP.ShiftByImm;
                    shiftAmount_DP = FindShiftAmount();
                }
                else if (bit4 == 0b1)    // requires a shift by register
                {
                    typeDP = TypeDP.ShiftByReg;
                    rs_DP = FindRs();
                }
                else { }
            }
            else if (type == 0b001) // does not require a shift
            {
                typeDP = TypeDP.Imm;
                rotate_DP = FindRotate();
                imm_DP = FindImm();
            }
            else { }

            Disassemble();
        }

        /// <summary>
        /// Extracts the opcode nibble and
        /// stores in the instance variable opcode
        /// </summary>
        private uint FindOpcode()
        {
            return Memory.ShiftToEnd(instr, 21, 24);
        }

        /// <summary>
        /// Extracts the S bit and
        /// stores in the instance variable s
        /// </summary>
        private uint FindS()
        {
            return Memory.ShiftToEnd(instr, 20, 20);
        }

        /// <summary>
        /// Extracts the Rn nibble and
        /// stores in the instance variable rn
        /// </summary>
        private uint FindRn()
        {
            return Memory.ShiftToEnd(instr, 16, 19);
        }

        /// <summary>
        /// Extracts the Rd nibble and
        /// stores in the instance variable rd
        /// </summary>
        private uint FindRd()
        {
            return Memory.ShiftToEnd(instr, 12, 15);
        }

        /// <summary>
        /// Extracts the rotate nibble and
        /// stores in the instance variable rotate
        /// </summary>
        private uint FindRotate()
        {
            return Memory.ShiftToEnd(instr, 8, 11);
        }

        /// <summary>
        /// Extracts the rotate nibble and
        /// stores in the instance variable rotate
        /// </summary>
        private uint FindImm()
        {
            return Memory.ShiftToEnd(instr, 0, 7);
        }

        /// <summary>
        /// Extracts the 5 bit shift number and
        /// stores in the instance variable shiftAmount
        /// </summary>
        private uint FindShiftAmount()
        {
            return Memory.ShiftToEnd(instr, 7, 11);
        }

        /// <summary>
        /// Extracts the Rm nibble and
        /// stores in the instance variable rm
        /// </summary>
        private uint FindRm()
        {
            return Memory.ShiftToEnd(instr, 0, 3);
        }

        /// <summary>
        /// Extracts the 2 bit Sh code and
        /// stores in the instance variable shiftCode
        /// </summary>
        private uint FindShiftCode()
        {
            return Memory.ShiftToEnd(instr, 5, 6);
        }

        /// <summary>
        /// Extracts the Rs nibble and
        /// stores in the instance variable rs
        /// </summary>
        private uint FindRs()
        {
            return Memory.ShiftToEnd(instr, 8, 11);
        }

        /// <summary>
        /// Finds the correct opcode string
        /// for the textual representation
        /// </summary>
        public string FindOpcodeString()
        {
            string name = "";
            for (int i = 0; i < opcodes.Length; ++i)
            {
                if (opcode_DP == i)
                {
                    name = opcodeNames[i];
                }
            }

            return name;
        }

        /// <summary>
        /// Finds the correct immediate string after rotation
        /// for the textual representation
        /// </summary>
        public string FindRotatedImm()
        {
            return Convert.ToString((imm_DP >> ((int)rotate_DP) * 2) | (imm_DP << ((32 - (int)rotate_DP) * 2)));
        }

        /// <summary>
        /// Produce the textual representation
        /// </summary>
        public void Disassemble()
        {
            if (type == 0b000)      // requires a shift either by register or immediate
            {
                if (bit4 == 0b0) // requires a shift by immediate
                {
                    if (opcode_DP == 0b1010)    // CMP
                    {
                        text = FindOpcodeString() + FindCondString() + " r" + rn_DP + ", r" + rm_DP + ", " + FindShiftString() + " #" + shiftAmount_DP;
                    }
                    else
                    {
                        text = FindOpcodeString() + FindCondString() + " r" + rd_DP + ", r" + rm_DP + ", " + FindShiftString() + " #" + shiftAmount_DP;
                    }
                }
                else if (bit4 == 0b1)    // requires a shift by register
                {
                    if (opcode_DP == 0b1010)    // CMP
                    {
                        text = FindOpcodeString() + FindCondString() + " r" + rn_DP + ", r" + rm_DP + ", " + FindShiftString() + " r" + rs_DP;
                    }
                    else
                    {
                        text = FindOpcodeString() + FindCondString() + " r" + rd_DP + ", r" + rm_DP + ", " + FindShiftString() + " r" + rs_DP;
                    }
                }
                else { }
            }
            else if (type == 0b001) // does not require a shift
            {
                if ((opcode_DP == 0b1101) ||    // MOV or MVN that only have one register
                    (opcode_DP == 0b1111))
                {
                    text = FindOpcodeString() + FindCondString() + " r" + rd_DP + ", #" + FindRotatedImm();
                }
                else if (opcode_DP == 0b1010)
                {
                    text = FindOpcodeString() + FindCondString() + " r" + rn_DP + ", #" + FindRotatedImm();
                }
                else
                {
                    text = FindOpcodeString() + FindCondString() + " r" + rd_DP + ", r" + rn_DP + ", #"+ FindRotatedImm();
                }
            }
            else { }
        }
    }
}
