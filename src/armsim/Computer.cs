// Computer.cs
// Holds instances of the Memory class for
// RAM and registers.
// Contains methods for run and step.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace armsim
{
    public class Computer
    {
        const int NUM_REGISTERS = 17 * 4;   // number of registers; r0-r15 and CPSR
        const int PC_REG = 15 * 4;          // start of program counter word
        const int SP_REG = 13 * 4;          // start of stack pointer word
        private int memSize;                // size of memory
        private string fileName;            // name of ELF file
        public Memory ram;                  // instance of Memory class to simulate the virtual RAM
        public Memory registers;            // instance of Memory class to simulate the registers
        public CPU processor;               // instance of CPU class
        public Loader load;                        // instance of the Loader class
        private static int retCode;         // code that returns to View
        public uint textStart;       // start address of the .text section
        public uint textSize;        // size in bytes of the .text section
        public static bool finished;    // indicates whether the cpu is finished executing

        public event EventHandler RunCompleted; // delegate for handling the OnRunCompleted event

        /// <summary>
        /// Constructor for the Computer class
        /// </summary>
        /// <param name="_memSize">size in bytes of virtual memory</param>
        /// <param name="_fileName">name of ELF file to load</param>
        public Computer(int _memSize, string _fileName)
        {
            memSize = _memSize;
            fileName = _fileName;
            LoadFile();
            finished = false;
        }

        /// <summary>
        /// Runs through the program beginning at the address
        /// specified in the program counter and performs the
        /// fetch, decode, execute cycle until fetch return 0
        /// </summary>
        public void run()
        {
            while (!finished)
            {
                foreach (MainWindow.breakRow row in MainWindow.breakList)
                {
                    if (string.Format("{0:X8}:", registers.ReadWord(PC_REG)) == row.breakAddr)
                    {
                        processor.running = false;
                        OnRunCompleted(EventArgs.Empty);
                        return;
                    }
                }

                uint instrWord = processor.fetch();
                registers.WriteWord(PC_REG, registers.ReadWord(PC_REG) + 4);     // update PC
                
                Instruction instr = processor.decode(instrWord);
                processor.execute(instr);
            }

            processor.running = false;
            OnRunCompleted(EventArgs.Empty);
        }

        /// <summary>
        /// Performs one fetch, decode, execute cycle
        /// at the address specified in the program counter
        /// </summary>
        public void step()
        {
            if (!finished)
            {
                uint instruction = processor.fetch();
                registers.WriteWord(PC_REG, registers.ReadWord(PC_REG) + 4);     // update PC
                Instruction instr = processor.decode(instruction);
                processor.execute(instr);

                processor.running = false;
                OnRunCompleted(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Creates:
        ///     an instance of Memory for virtual RAM,
        ///     an instance of Memory for registers,
        ///     an instance of CPU for processing the information,
        ///     an instance of Loader for loading the ELF file into virtual memory
        /// </summary>
        public void LoadFile()
        {
            ram = new Memory(memSize);
            registers = new Memory(NUM_REGISTERS);
            processor = new CPU(ram, registers);
            load = new Loader(memSize, fileName, ram, this);
            registers.WriteWord(PC_REG, GetPC());
            registers.WriteWord(SP_REG, 0x7000);
            retCode = load.GetRetCode();
        }

        /// <summary>
        /// Getter method to retrieve the initial program counter
        /// value from the load instance variable
        /// </summary>
        public uint GetPC()
        {
            return load.GetElfEntry();
        }

        /// <summary>
        /// Getter method to retrieve the stack pointer
        /// </summary>
        public uint GetSP()
        {
            return registers.ReadWord(SP_REG);
        }

        /// <summary>
        /// Getter method to retrieve the retCode instance variable
        /// </summary>
        public int GetRetCode()
        {
            return retCode;
        }

        /// <summary>
        /// Getter method to retrieve the memSize instance variable
        /// </summary>
        public int GetMemSize()
        {
            return memSize;
        }

        /// <summary>
        /// Makes CalculateChecksum() in the Memory class accessible to MainWindow.xaml.cs
        /// </summary>
        /// <returns>Checksum as an int</returns>
        public int GetChecksum()
        {
            return ram.CalculateChecksum();
        }

        /// <summary>
        /// Event handler method
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnRunCompleted(EventArgs e)
        {
            RunCompleted?.Invoke(this, e);
        }
    }
}
