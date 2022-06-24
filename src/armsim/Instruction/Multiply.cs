using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace armsim
{
    class Multiply : Instruction
    {
        /// <summary>
        /// Represents the information found in multiply instruction
        /// </summary>
        public Multiply(uint instr) : base(instr)
        {
            s_M = FindS();
            rd_M = FindRd();
            rs_M = FindRs();
            rm_M = FindRm();

            Disassemble();
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
        /// Extracts the Rd nibble and
        /// stores in the instance variable rd
        /// </summary>
        private uint FindRd()
        {
            return Memory.ShiftToEnd(instr, 16, 19);
        }

        /// <summary>
        /// Extracts the Rd nibble and
        /// stores in the instance variable rd
        /// </summary>
        private uint FindRs()
        {
            return Memory.ShiftToEnd(instr, 8, 11);
        }

        /// <summary>
        /// Extracts the Rd nibble and
        /// stores in the instance variable rd
        /// </summary>
        private uint FindRm()
        {
            return Memory.ShiftToEnd(instr, 0, 3);
        }

        /// <summary>
        /// Produce the textual representation
        /// </summary>
        public void Disassemble()
        {
            text = "mul" + FindCondString() + " r" + rd_M + ", r" + rm_M + ", r" + rs_M;
        }
    }
}
