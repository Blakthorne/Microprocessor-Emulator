using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace armsim
{
    class LoadStoreMultiple : Instruction
    {
        /// <summary>
        /// Represents the information found in load/store multiple instruction
        /// </summary>
        public LoadStoreMultiple(uint instr) : base(instr)
        {
            regsList_LSM = FindRegsList();
            p_LSM = FindP();
            u_LSM = FindU();
            s_LSM = FindS();
            w_LSM = FindW();
            l_LSM = FindL();
            rn_LSM = FindRn();

            Disassemble();
        }

        private List<int> FindRegsList()
        {
            List<int> regs_List = new List<int>();

            for (uint i = 0; i <= 15; ++i)
            {
                if (Memory.ShiftToEnd(instr, i, i) == 1)
                {
                    regs_List.Add((int) i);
                }
            }

            return regs_List;
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
        private uint FindS()
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
        /// Produce the textual representation
        /// </summary>
        public void Disassemble()
        {
            string typeString = "";
            string wString = "";

            if (l_LSM == 1)
            {
                typeString = "ldm";
            }
            else
            {
                typeString = "stm";
            }

            if (w_LSM == 1)
            {
                wString = "!";
            }

            text = typeString + FindCondString() + " r" + rn_LSM + wString + ", {";

            for (int i = 0; i < regsList_LSM.Count; ++i)
            {
                if (i < regsList_LSM.Count - 1)
                {
                    text += "r" + regsList_LSM[i] + ", ";
                }
                else
                {
                    text += "r" + regsList_LSM[i];
                }
            }

            text += "}";
        }
    }
}
