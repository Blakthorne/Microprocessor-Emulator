using System;
using System.Collections.Generic;
using System.Text;

namespace armsim
{
    class Branch : Instruction
    {
        /// <summary>
        /// Represents the information found in data processing instruction
        /// </summary>
        public Branch(uint instr) : base(instr)
        {
            l_b = FindL();
            offset_b = FindOffset();
        }

        /// <summary>
        /// Extracts the L bit and
        /// stores in the instance variable l
        /// </summary>
        private uint FindL()
        {
            return Memory.ShiftToEnd(instr, 24, 24);
        }

        /// <summary>
        /// Extracts the first 24 bits of the encoding
        /// </summary>
        public int FindOffset()
        {
            int offset_b = checked((int)Memory.ShiftToEnd(instr, 0, 23));
            int isNeg = offset_b & 0x800000;
            if (isNeg != 0)
            {
                offset_b |= 0x3F000000;
            }
            int shifted = offset_b << 2;
            return shifted;
        }

        /// <summary>
        /// Computes the final target address for the branch
        /// </summary>
        public int FindTarget()
        {
            return offset_b + (int)pc_b;
        }

        /// <summary>
        /// Produce the textual representation
        /// </summary>
        public void Disassemble()
        {
            target_b = FindTarget();

            text = "b";

            if (l_b == 1)
            {
                text += "l";
            }

            text += FindCondString() + " #" + string.Format("0x{0:X}", target_b);
        }
    }
}
