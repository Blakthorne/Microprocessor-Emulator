using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace armsim
{
    public class Instruction
    {
        protected static uint[] conds = {
            0x0, // Equal
            0x1, // Not equal
            0x2, // Carry set/unsigned higher or same
            0x3, // Carry clear/unsigned lower
            0x4, // Minus/negative
            0x5, // Plus/positive or zero
            0x6, // Overflow
            0x7, // No overflow
            0x8, // Unigned higher
            0x9, // Unsigned lower or same
            0xA, // Signed greater than or equal
            0xB, // Signed less than
            0xC, // Signed greater than
            0xD, // Signed less than or equal
            0xE  // Always (unconditional)
        };

        protected static string[] condNames = {
            "eq",
            "ne",
            "cs/hs",
            "cs/lo",
            "mi",
            "pl",
            "vs",
            "vc",
            "hi",
            "ls",
            "ge",
            "lt",
            "gt",
            "le",
            ""
        };

        public static uint[] shifts = {
            0b00,   // LSL
            0b01,   // LSR
            0b10,   // ASR
            0b11    // ROR
        };

        public static string[] shiftNames = {
            "lsl",
            "lsr",
            "asr",
            "ror"
        };

        public enum TypeInstr
        {
            DataProcessing,
            LoadStore,
            LoadStoreMultiple,
            Multiply,
            Swi,
            Branch,
            BX
        }

        public enum TypeDP
        {
            Imm,
            ShiftByImm,
            ShiftByReg
        }

        public enum TypeLS
        {
            Imm,
            Register
        }

        public uint instr;          // holds the instruction for this class and all derived classes
        public string text;         // textual ARM representation

        public TypeInstr typeInstr;
        public TypeDP typeDP;
        public TypeLS typeLS;

        /* -------------------------------------------------------------------------------------------------------------*/
        // ALL INSTRUCTIONS

        // bits pertaining to all instructions
        public string instrString;  // contains the disassembled instruction string
        public uint conditionFlags; // bits 28-31; code for the condition flags
        public uint type;           // bits 25-27; determine types of operation - dataprocessing, load/store, or branch
        public uint bit4;           // bit 4; if data processing, set to 1, and bit7 set to 1 - then multiply
        public uint bit7;           // bit 7; if data processing, set to 1, and bit4 set to 1 - then multiply
        public bool executed;       // if the conditional allowed the instruction to execute

        /* -------------------------------------------------------------------------------------------------------------*/
        // DATA PROCESSING

        // bits pertaining to all data processing instructions
        public uint opcode_DP; // bits 21-24; determines the specific instruction
        public uint s_DP;      // bit 20; determines whether or not to write condition codes
        public uint rn_DP;     // bits 16-19; base register
        public uint rd_DP;     // bits 12-15; destination register

        // bits pertaining to immediate data processing instructions
        public uint rotate_DP;      // bits 8-11; immediate value is rotated by (2 * rot)
        public uint imm_DP;         // bits 0-7; the immediate value

        // bits pertaining to all shifted data processing instructions
        public uint shiftCode_DP;   // bits 5-6; code determines the shift operation
        public uint rm_DP;          // bits 0-3; the register in operand2 whose value can be shifted

        // bits pertaining to immediate shifted data processing instructions
        public uint shiftAmount_DP;        // bits 7-11; imm value to shift by

        // bits pertaining to register shifted data processing instructions
        public uint rs_DP;              // bits 8-11; the register in operand2 whose value is the shift amount


        /* -------------------------------------------------------------------------------------------------------------*/
        // LOAD/STORE

        // bits pertaining to all load/store commands
        public uint p_LS;  // bit 24; 0=post-indexed, 1=pre-indexed
        public uint u_LS;  // bit 23; 1=positive offset, 0=negative offset
        public uint b_LS;  // bit 22; 1=unsigned byte, 0=word
        public uint w_LS;  // bit 21; when p=1, 0=no writeback, 1=writeback
        public uint l_LS;  // bit 20; 1=load, 0=store
        public uint rn_LS; // bits 16-19; the base register
        public uint rd_LS; // bits 12-15; the destination register

        // bits pertaining to immediate load/store commands
        public uint imm_LS;         // bits 0-11; the immediate value offset

        // bits pertaining to shifted load/store commands
        public uint shiftAmount_LS;    // bits 7-11; the immediate value if shift is by immediate
        public uint shiftCode_LS;      // bits 5-6; code that determines the shift operation
        public uint rm_LS;             // bits 0-3; the register whose value is being shifted


        /* -------------------------------------------------------------------------------------------------------------*/
        // LOAD/STORE MULTIPLE

        public List<int> regsList_LSM;  // bits 0-15; whatever number bits are 1, that register is loaded/stored
        public uint p_LSM;  // bit 24;
        public uint u_LSM;  // bit 23; 1=positive offset, 0=negative offset
        public uint s_LSM;  // bit 22; 1=unsigned byte, 0=word
        public uint w_LSM;  // bit 21; when p=1, 0=no writeback, 1=writeback
        public uint l_LSM;  // bit 20; 1=load, 0=store
        public uint rn_LSM; // bits 16-19; the base register


        /* -------------------------------------------------------------------------------------------------------------*/
        // MULTIPLY

        public uint s_M;           // bit 20; determines whether or not to write condition codes
        public uint rd_M;          // bits 16-19; destination register
        public uint rs_M;          // bits 8-11; value to be multiplied with the value in rm
        public uint rm_M;          // bits 0-3; first value to be multiplied

        /* -------------------------------------------------------------------------------------------------------------*/
        // SWI

        public uint offset_swi;     // bits 0-23; the offset in a swi instruction

        /* -------------------------------------------------------------------------------------------------------------*/
        // Branch

        public uint l_b;            // bit 24; whether or not to update link register (r14)
        public int offset_b;        // bits 0-23; the offset in a branch instruction
        public uint pc_b;           // value of pc for branch
        public int target_b;       // value of target address for branch

        /* -------------------------------------------------------------------------------------------------------------*/
        // BX

        public uint rm_bx;          // bits 0-3; rm register
        public uint encoding_bx;    // bits 4-27; the encoding specific to bx
        public uint bit0_bx;        // bit 0; for determining thumb mode


        /// <summary>
        /// Creates a narrower instance of Instruction by
        /// using the type instance variable
        /// </summary>
        public Instruction(uint _instr)
        {
            instr = _instr;
            conditionFlags = FindConditionFlags();
            type = FindType(instr);
            bit4 = FindBit4(instr);
            bit7 = FindBit7(instr);
        }

        /// <summary>
        /// Extracts the cond nybble and
        /// stores in the instance variable conditionFlags
        /// </summary>
        private uint FindConditionFlags()
        {
            return Memory.ShiftToEnd(instr, 28, 31);
        }

        /// <summary>
        /// Extracts the 3 bit type from the encoding and
        /// stores in the instance variable type
        /// </summary>
        public static uint FindType(uint instr)
        {
            return Memory.ShiftToEnd(instr, 25, 27);
        }

        /// <summary>
        /// Extracts the 7th bit of the encoding and
        /// stores in the instance variable bit7
        /// </summary>
        public static uint FindBit7(uint instr)
        {
            return Memory.ShiftToEnd(instr, 7, 7);
        }

        /// <summary>
        /// Extracts the 4th bit of the encoding and
        /// stores in the instance variable bit4
        /// </summary>
        public static uint FindBit4(uint instr)
        {
            return Memory.ShiftToEnd(instr, 4, 4);
        }

        /// <summary>
        /// Extracts bits 4 - 27 that are
        /// specific to the bx instruction
        /// </summary>
        public static uint FindBxEncoding(uint instr)
        {
            return Memory.ShiftToEnd(instr, 4, 27);
        }

        /// <summary>
        /// Finds the correct conditional string
        /// ending for the textual representation
        /// </summary>
        public string FindCondString()
        {
            string name = "";
            for (int i = 0; i < conds.Length; ++i)
            {
                if (conditionFlags == i)
                {
                    name = condNames[i];
                }
            }

            return name;
        }

        /// <summary>
        /// Finds the correct shift string
        /// for the textual representation
        /// </summary>
        public string FindShiftString()
        {
            string name = "";

            if ((type == 0b000) ||
                (type == 0b001))
            {
                for (int i = 0; i < shifts.Length; ++i)
                {
                    if (shiftCode_DP == i)
                    {
                        name = shiftNames[i];
                    }
                }
            }
            else
            {
                for (int i = 0; i < shifts.Length; ++i)
                {
                    if (shiftCode_LS == i)
                    {
                        name = shiftNames[i];
                    }
                }
            }

            return name;
        }
    }
}
