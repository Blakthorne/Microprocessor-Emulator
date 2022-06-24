// Memory.cs
// Acts as the virtual memory with
// methods to read, write, and manipulate

using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Peers;

namespace armsim
{
    // Virtual memory for microprocessor
    public class Memory
    {
        public byte[] memArray;    // holds the contents of virtual memory

        /// <summary>
        /// Constructor for the Memory class
        /// </summary>
        /// <param name="memArraySize">Size of virutal memory</param>
        public Memory(int memArraySize)
        {
            memArray = new byte[memArraySize];
        }

        /// <summary>
        /// Reads a word (32 bits) from virtual memory
        /// </summary>
        /// <param name="addr">The address in virtual memory to begin reading</param>
        /// <returns>An unsigned int comprised of the 4 bytes read</returns>
        public uint ReadWord(uint addr)
        {
            // If address given is not divisible by 4, throw an exception
            if ((addr % 4) != 0)
            {
                throw new Exception("ReadWord: Attmpted to read a word at an address not divisible by 4.");
            }
            else
            {
                uint tempMem = 0;

                // OR all 4 bytes together, shifting the bits left by 8 each time
                for (int i = 0; i < 4; ++i)
                {
                    tempMem |= (uint)((memArray[addr + i]) << (8 * i));
                }

                return tempMem;
            }
        }

        /// <summary>
        /// Reads half of a word (16 bits) from virtual memory
        /// </summary>
        /// <param name="addr">The address in virtual memory to begin reading</param>
        /// <returns>An unsigned short comprised of the 2 bytes read</returns>
        public ushort ReadHalfWord(uint addr)
        {
            // If address given is not divisible by 2, throw an exception
            if ((addr % 2) != 0)
            {
                throw new Exception("ReadHalfWord: Attmpted to read a half-word at an address not divisible by 2.");
            }
            else
            {
                ushort tempMem = 0;

                // OR both bytes together, shifting the bits left by 8 each time
                for (int i = 0; i < 2; ++i)
                {
                    tempMem |= (ushort)((memArray[addr + i]) << (8 * i));
                }

                return tempMem;
            }
        }

        /// <summary>
        /// Reads one byte (8 bits) from virtual memory
        /// </summary>
        /// <param name="addr">The address in virtual memory to begin reading</param>
        /// <returns>The byte at the address specified</returns>
        public byte ReadByte(uint addr)
        {
            return memArray[addr];
        }

        /// <summary>
        /// Writes a word (32 bits) to virtual memory
        /// </summary>
        /// <param name="addr">The address in virtual memory to begin writing</param>
        /// <param name="data">An unsigned int to write to virtual memory</param>
        public void WriteWord(uint addr, uint data)
        {
            // If address given is not divisible by 4, throw an exception
            if ((addr % 4) != 0)
            {
                throw new Exception("WriteWord: Attmpted to write a word to an address not divisible by 4.");
            }
            else
            {
                // Takes one byte at a time from the uint and writes to virtual memory
                for (int i = 0; i < 4; ++i)
                {
                    memArray[addr + i] = (byte)(data & 0xFF);
                    data = (uint)(data >> 8);
                }
            }
        }

        /// <summary>
        /// Writes a word (16 bits) to virtual memory
        /// </summary>
        /// <param name="addr">The address in memory to begin writing</param>
        /// <param name="data">An unsigned short to write to virtual memory</param>
        public void WriteHalfWord(uint addr, ushort data)
        {
            // If address given is not divisible by 2, throw an exception
            if ((addr % 2) != 0)
            {
                throw new Exception("WriteHalfWord: Attmpted to write a half-word to an address not divisible by 2.");
            }
            else
            {
                // Takes one byte at a time from the ushort and writes to virtual memory
                for (int i = 0; i < 2; ++i)
                {
                    memArray[addr + i] = (byte)(data & 0xFF);
                    data = (ushort)(data >> 8);
                }
            }
        }

        /// <summary>
        /// Writes a byte (8 bits) to virtual memory
        /// </summary>
        /// <param name="addr">the address in memory to begin writing</param>
        /// <param name="data">A byte to write to virtual memory</param>
        public void WriteByte(uint addr, byte data)
        {
            memArray[addr] = data;
        }

        /// <summary>
        /// Calculates the checksum of based on the virtual memory for testing purposes
        /// </summary>
        /// <returns>An int that is the checksum</returns>
        public int CalculateChecksum()
        {
            int checksum = 0;
            for (int i = 0; i < memArray.Length; ++i)
            {
                checksum += memArray[i] ^ i;   // XORs each byte with its address and add them together
            }
            return checksum;
        }

        /// <summary>
        /// Reads the word at addr and returns information about a specific bit in the word
        /// </summary>
        /// <param name="addr">The address in virtual memory to begin reading</param>
        /// <param name="bit">The bit in the word to test, beginning at the least significant bit</param>
        /// <returns>If bit is 1, return true.
        ///          If bit is 0, return false.</returns>
        public bool TestFlag(uint addr, int bit)
        {
            uint data = ReadWord(addr);
            uint result = data & (uint)(0x00000001 << bit);

            return result != 0;
        }

        /// <summary>
        /// Sets a specific bit in the word
        /// </summary>
        /// <param name="addr">The address in virtual memory to begin reading</param>
        /// <param name="bit">The bit in the word to set, beginning at the least significant bit</param>
        /// <param name="flag">If flag is true, set bit to 1.
        ///                    If flag is false, set bit to 0.</param>
        public void SetFlag(uint addr, int bit, bool flag)
        {
            uint data = ReadWord(addr);
            uint result = 0;

            if (flag)
            {
                result = data | (uint)(0x00000001 << bit);
            }
            else
            {
                uint tempData = ~data;
                tempData |= (uint)(0x00000001 << bit);
                result = ~tempData;
            }

            WriteWord(addr, result);
        }

        /// <summary>
        /// Zeroes out all the bits in word except for those in the range startBit...endBit
        /// </summary>
        /// <param name="word">The initial word that will be manipulated</param>
        /// <param name="startBit">The lowest bit in the range</param>
        /// <param name="endBit">The highest bit in the range</param>
        /// <returns>An unsigned int representing only the bits in the range specified</returns>
        public static uint ExtractBits(uint word, uint startBit, uint endBit)
        {
            if (startBit > endBit)
            {
                throw new Exception("ExtractBits: Gave a startBit greater than endBit.");
            }
            else
            {
                uint mask = 0;
                for (int i = (int)startBit; i <= (int)endBit; ++i)
                {
                    mask |= (uint)(0x00000001 << i);
                }
                return word & mask;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="word"></param>
        /// <param name="startBit"></param>
        /// <param name="endBit"></param>
        /// <returns></returns>
        public static uint ShiftToEnd(uint word, uint startBit, uint endBit)
        {
            return ExtractBits(word, startBit, endBit) >> (int)startBit;
        }
    }
}
