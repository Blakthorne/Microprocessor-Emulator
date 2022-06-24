using System;
using System.Collections.Generic;
using System.Text;

namespace armsim
{
    class BX : Instruction
    {
        /// <summary>
        /// Represents the information found in bx instruction
        /// </summary>
        public BX(uint instr) : base(instr)
        {
            rm_bx = FindRm();
            bit0_bx = FindBit0();

            Disassemble();
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
        /// Extracts the bit 0
        /// </summary>
        private uint FindBit0()
        {
            return Memory.ShiftToEnd(instr, 0, 0);
        }

        /// <summary>
        /// Produce the textual representation
        /// </summary>
        public void Disassemble()
        {
            text = "bx" + FindCondString() + " r" + rm_bx;
        }
    }
}
