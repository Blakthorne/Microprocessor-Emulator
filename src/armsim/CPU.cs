// CPU.cs
// Perform the fetch-decode-execute cycle
// using the RAM and registers instances
// of the Memory class.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Media.TextFormatting;

namespace armsim
{
    public class CPU
    {
        public Memory ram;              // instance variable referencing the virtual RAM in the Computer class
        public Memory registers;        // instance variable referencing the registers in the Computer class
        const int PC_REG = 15 * 4;      // start of program counter word
        public bool running = false;    // flag to detect if the CPU is currently running
        public string traceLine;        // line that gets output to trace.log
        public const int REGS_MULTIPLIER = 4;
        public bool isFinished;
        public int consolePtr; // points to the most recent position in the console
        public string console;  // keeps up to date with GUI

        public event EventHandler ExecuteCompleted; // delegate for handling the OnRunCompleted event
        public event EventHandler PutChar; // delegate for handling the OnPutChar event
        public event EventHandler ReadLine; // delegate for handling the ReadLine event

        /// <summary>
        /// Constructor for the CPU class
        /// </summary>
        /// <param name="_ram">Reference to ram variable in the Computer class</param>
        /// <param name="_registers">Reference to the registers variable in the Computer class</param>
        public CPU(Memory _ram, Memory _registers)
        {
            ram = _ram;
            registers = _registers;
            consolePtr = 0;
        }

        /// <summary>
        /// Fetches the word at the address
        /// specified by the program counter
        /// </summary>
        /// <returns>A uint resembling the word fetched from memory</returns>
        public uint fetch()
        {
            if (!running) { return 0; }
            uint instrAddr = registers.ReadWord(PC_REG);    // program counter
            traceLine = string.Format("{0:000000} ", MainWindow.traceCount);
            traceLine += string.Format("{0:X8} ", instrAddr);
            uint test = ram.ReadWord(instrAddr);            // instruction at program counter value
            return test;
        }

        /// <summary>
        /// Decodes the word that was fetched from memory
        /// </summary>
        /// <param name="instr">The word that needs to be decoded</param>
        /// <returns>An instance of the Instruction class containing information
        ///          for that specific instruction</returns>
        //public Instruction decode(uint instr)
        public Instruction decode(uint instr)
        {
            uint type = Instruction.FindType(instr);
            uint bit4 = Instruction.FindBit4(instr);
            uint bit7 = Instruction.FindBit7(instr);
            uint isBx = Instruction.FindBxEncoding(instr);

            if (isBx == 0x12FFF1)
            {
                BX bx = new BX(instr);
                bx.typeInstr = Instruction.TypeInstr.BX;
                return bx;
            }
            else if ((type == 0b001) ||
                ((type == 0b000) &&
                (!((bit4 == 0b1) &&
                  (bit7 == 0b1)))))          // type = 000/001; data processing instruction
            {
                DataProcessing dp = new DataProcessing(instr);
                dp.typeInstr = Instruction.TypeInstr.DataProcessing;
                return dp;
            }
            else if ((type == 0b000) &&   // type = 000; multiply instruction
                     (bit4 == 0b1) &&
                     (bit7 == 0b1))
            {
                Multiply mul = new Multiply(instr);
                mul.typeInstr = Instruction.TypeInstr.Multiply;
                return mul;
            }
            else if ((type == 0b010) ||
                     (type == 0b011))     // type = 010/011; load/store instruction
            {
                LoadStore ls = new LoadStore(instr);
                ls.typeInstr = Instruction.TypeInstr.LoadStore;
                return ls;
            }
            else if (type == 0b100)       // type = 100; load/store multiple instruction
            {
                LoadStoreMultiple lsm = new LoadStoreMultiple(instr);
                lsm.typeInstr = Instruction.TypeInstr.LoadStoreMultiple;
                return lsm;
            }
            else if (type == 0b111)
            {
                SWI swi = new SWI(instr);
                swi.typeInstr = Instruction.TypeInstr.Swi;
                return swi;
            }
            else if (type == 0b101)
            {
                Branch branch = new Branch(instr);
                branch.typeInstr = Instruction.TypeInstr.Branch;
                branch.pc_b = registers.ReadWord(PC_REG) + 4;
                branch.Disassemble();
                return branch;
            }
            else { return new Instruction(instr); }
        }

        /// <summary>
        /// Executes the instruction provided
        /// by the Instruction instance
        /// </summary>
        /// <param name="instr"></param>
        public void execute(Instruction instr)
        {
            uint cpsrReg = 16 * 4;

            registers.WriteWord(PC_REG, registers.ReadWord(PC_REG) + 4);
            Execute.Execute.ExecuteInstruction(instr, this);
            if (instr.typeInstr != Instruction.TypeInstr.Branch ||
               ((instr.typeInstr == Instruction.TypeInstr.Branch) &&
                !instr.executed))
            {
                registers.WriteWord(PC_REG, registers.ReadWord(PC_REG) - 4);
            }

            if (instr.typeInstr == Instruction.TypeInstr.Swi)
            {
                switch (instr.offset_swi)
                {
                    case 0x0:
                        OnPutChar(EventArgs.Empty);
                        break;
                    case 0x11:
                        Computer.finished = true;
                        break;
                    case 0x06a:
                        OnReadLine(EventArgs.Empty);
                        break;
                    default:    // all other swi numbers are treated as no-ops
                        break;
                }
            }

            traceLine += string.Format("{0:X8} ", ram.CalculateChecksum());

            if (registers.TestFlag(cpsrReg, 31))
            {
                traceLine += "1";
            }
            else { traceLine += "0"; }
            if (registers.TestFlag(cpsrReg, 30))
            {
                traceLine += "1";
            }
            else { traceLine += "0"; }
            if (registers.TestFlag(cpsrReg, 29))
            {
                traceLine += "1";
            }
            else { traceLine += "0"; }
            if (registers.TestFlag(cpsrReg, 28))
            {
                traceLine += "1 ";
            }
            else { traceLine += "0 "; }

            traceLine += "SYS ";

            for (int i = 0; i < 15; ++i)
            {
                traceLine += i + "=" + string.Format("{0:X8} ", registers.ReadWord((uint)(i * REGS_MULTIPLIER)));
            }

            OnExecuteCompleted(EventArgs.Empty);
            traceLine = "";
        }
        

        /// <summary>
        /// Event handler method
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnExecuteCompleted(EventArgs e)
        {
            ExecuteCompleted?.Invoke(this, e);
        }

        /// <summary>
        /// Event handler method
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnPutChar(EventArgs e)
        {
            PutChar?.Invoke(this, e);
        }

        /// <summary>
        /// Event handler method
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnReadLine(EventArgs e)
        {
            ReadLine?.Invoke(this, e);
        }
    }
}
