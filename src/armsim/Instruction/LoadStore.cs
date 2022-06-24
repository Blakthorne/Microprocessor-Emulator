using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace armsim
{
    class LoadStore : Instruction
    {
        /// <summary>
        /// Represents the information found in load/store instruction
        /// </summary>
        public LoadStore(uint instr) : base(instr)
        {
            p_LS = FindP();
            u_LS = FindU();
            b_LS = FindB();
            w_LS = FindW();
            l_LS = FindL();
            rn_LS = FindRn();
            rd_LS = FindRd();

            if (type == 0b010)       // load/store with immediate offset
            {
                typeLS = TypeLS.Imm;
                imm_LS = FindImm();
            }
            else if (type == 0b011)  // load/store with register/shift involved
            {
                typeLS = TypeLS.Register;
                shiftAmount_LS = FindShiftAmount();
                shiftCode_LS = FindShiftCode();
                rm_LS = FindRm();
            }
            else { }

            Disassemble();
        }

        /// <summary>
        /// Extracts the P bit and
        /// stores in the instance variable p
        /// </summary>
        private uint FindP()
        {
            return Memory.ShiftToEnd(instr, 24, 24);
        }

        /// <summary>
        /// Extracts the U bit and
        /// stores in the instance variable u
        /// </summary>
        private uint FindU()
        {
            return Memory.ShiftToEnd(instr, 23, 23);
        }

        /// <summary>
        /// Extracts the B bit and
        /// stores in the instance variable b
        /// </summary>
        private uint FindB()
        {
            return Memory.ShiftToEnd(instr, 22, 22);
        }

        /// <summary>
        /// Extracts the W bit and
        /// stores in the instance variable w
        /// </summary>
        private uint FindW()
        {
            return Memory.ShiftToEnd(instr, 21, 21);
        }

        /// <summary>
        /// Extracts the L bit and
        /// stores in the instance variable l
        /// </summary>
        private uint FindL()
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
        /// Extracts the 12 bit imm value and
        /// stores in the instance variable imm
        /// </summary>
        private uint FindImm()
        {
            return Memory.ShiftToEnd(instr, 0, 11);
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
        /// Extracts the 2 bit shift code and
        /// stores in the instance variable shiftCode
        /// </summary>
        private uint FindShiftCode()
        {
            return Memory.ShiftToEnd(instr, 5, 6);
        }

        /// <summary>
        /// Extracts the 5 bit shift amount and
        /// stores in the instance variable shiftAmount
        /// </summary>
        private uint FindShiftAmount()
        {
            return Memory.ShiftToEnd(instr, 7, 11);
        }

        /// <summary>
        /// Produce the textual representation
        /// </summary>
        public void Disassemble()
        {
            string typeString = "";
            string lengthString = "";
            string signString = "";

            if (l_LS == 1)
            {
                typeString = "ldr";
            }
            else
            {
                typeString = "str";
            }

            if (b_LS == 1)
            {
                lengthString = "b";
            }

            if (u_LS == 0)
            {
                signString = "-";
            }

            if (type == 0b010)       // load/store with immediate offset
            {
                text = typeString + lengthString + FindCondString() + " r" + rd_LS + ", [r" + rn_LS + ", #" + signString + imm_LS + "]";
            }
            else if (type == 0b011)  // load/store with register/shift involved
            {
                if ((shiftAmount_LS == 0) &&    // register offset
                    (shiftCode_LS == 0))
                {
                    text = typeString + lengthString + FindCondString() + " r" + rd_LS + ", [r" + rn_LS + ", r" + rm_LS + "]";
                }
                else    // scaled register offset
                {
                    text = typeString + lengthString + FindCondString() + " r" + rd_LS + ", [r" + rn_LS + ", r" + rm_LS + ", " + FindShiftString() + " #" + shiftAmount_LS + "]";
                }
            }
            else { }

            if (w_LS == 1)
            {
                text += "!";
            }
        }
    }
}
