using System;
using System.Collections.Generic;
using System.Text;

namespace armsim
{
    /// <summary>
    /// Represents the information found in swi instruction
    /// </summary>
    class SWI : Instruction
    {
        public SWI(uint instr) : base(instr)
        {
            offset_swi = FindOffset();

            Disassemble();
        }

        /// <summary>
        /// Extracts the first 24 bits of the encoding
        /// </summary>
        public uint FindOffset()
        {
            return Memory.ShiftToEnd(instr, 0, 23);
        }

        /// <summary>
        /// Produce the textual representation
        /// </summary>
        public void Disassemble()
        {
            text = "swi #" + offset_swi;
        }
    }
}
