// MemoryTest.cs
// Contains unit testing for the Memory class

using System;
using System.Security.Policy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using armsim;

namespace armsimTest
{
    // Tests the methods in the Execute and derived classes
    [TestClass]
    public class ExecuteTest
    {
        const int NUM_REGISTERS = 17 * 4;   // number of registers; r0-r15 and CPSR
        const int REGS_MULTIPLIER = 4;

        [TestMethod]
        public void MovImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);

            // It is previously known that cpu.decode works correctly,
            // so it is valid to use here
            Instruction test = cpu.decode(0xE3A02030); // mov r2, #48
            cpu.execute(test);

            Assert.AreEqual((uint) 48, testRegs.ReadWord(2 * REGS_MULTIPLIER));
        }

        [TestMethod]
        public void MovImmRotate_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);

            // It is previously known that cpu.decode works correctly,
            // so it is valid to use here
            Instruction test = cpu.decode(0xE3A0220A); // mov r2, #0xA000 0000
            cpu.execute(test);

            Assert.AreEqual((uint)0xA0000000, testRegs.ReadWord(2 * REGS_MULTIPLIER));
        }

        [TestMethod]
        public void MovLslShiftImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 1);    // r2 contains the value 1

            // It is previously known that cpu.decode works correctly,
            // so it is valid to use here
            Instruction test = cpu.decode(0xE1A01182); // mov r1, r2, lsl #3
            cpu.execute(test);

            Assert.AreEqual((uint)8, testRegs.ReadWord(1 * REGS_MULTIPLIER));
        }

        [TestMethod]
        public void MovLsrShiftImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 8);    // r2 contains the value 8

            // It is previously known that cpu.decode works correctly,
            // so it is valid to use here
            Instruction test = cpu.decode(0xE1A011A2); // mov r1, r2, lsr #3
            cpu.execute(test);

            Assert.AreEqual((uint)1, testRegs.ReadWord(1 * REGS_MULTIPLIER));
        }

        [TestMethod]
        public void MovAsrShiftImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 0x80000000);    // r2 contains the value 0x8000 0000

            // It is previously known that cpu.decode works correctly,
            // so it is valid to use here
            Instruction test = cpu.decode(0xE1A011C2); // mov r1, r2, asr #3
            cpu.execute(test);

            Assert.AreEqual((uint)0xF0000000, testRegs.ReadWord(1 * REGS_MULTIPLIER));
        }

        [TestMethod]
        public void MovRorShiftImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 1);    // r2 contains the value 1

            // It is previously known that cpu.decode works correctly,
            // so it is valid to use here
            Instruction test = cpu.decode(0xE1A010E2); // mov r1, r2, ror #1
            cpu.execute(test);

            Assert.AreEqual((uint)0x80000000, testRegs.ReadWord(1 * REGS_MULTIPLIER));
        }

        [TestMethod]
        public void MovLslShiftReg_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 1);    // r2 contains the value 1
            cpu.registers.WriteWord(4 * REGS_MULTIPLIER, 3);    // r4 contains the value 3

            // It is previously known that cpu.decode works correctly,
            // so it is valid to use here
            Instruction test = cpu.decode(0xE1A01412); // mov r1, r2, lsl r4
            cpu.execute(test);

            Assert.AreEqual((uint)8, testRegs.ReadWord(1 * REGS_MULTIPLIER));
        }

        [TestMethod]
        public void MovLsrShiftReg_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 8);    // r2 contains the value 8
            cpu.registers.WriteWord(4 * REGS_MULTIPLIER, 3);    // r4 contains the value 3

            // It is previously known that cpu.decode works correctly,
            // so it is valid to use here
            Instruction test = cpu.decode(0xE1A01432); // mov r1, r2, lsr r4
            cpu.execute(test);

            Assert.AreEqual((uint)1, testRegs.ReadWord(1 * REGS_MULTIPLIER));
        }

        [TestMethod]
        public void MovAsrShiftReg_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 0x80000000);   // r2 contains the value 0x8000 0000
            cpu.registers.WriteWord(4 * REGS_MULTIPLIER, 3);            // r4 contains the value 3

            // It is previously known that cpu.decode works correctly,
            // so it is valid to use here
            Instruction test = cpu.decode(0xE1A01452); // mov r1, r2, asr r4
            cpu.execute(test);

            Assert.AreEqual((uint)0xF0000000, testRegs.ReadWord(1 * REGS_MULTIPLIER));
        }

        [TestMethod]
        public void MovRorShiftReg_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 1);    // r2 contains the value 1
            cpu.registers.WriteWord(4 * REGS_MULTIPLIER, 1);    // r4 contains the value 1

            // It is previously known that cpu.decode works correctly,
            // so it is valid to use here
            Instruction test = cpu.decode(0xE1A01472); // mov r1, r2, ror r4
            cpu.execute(test);

            Assert.AreEqual((uint)0x80000000, testRegs.ReadWord(1 * REGS_MULTIPLIER));
        }

        [TestMethod]
        public void MvnImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);

            // It is previously known that cpu.decode works correctly,
            // so it is valid to use here
            Instruction test = cpu.decode(0xE3E020FF); // mvn r2, #FF
            cpu.execute(test);

            Assert.AreEqual((uint)0xFFFFFF00, testRegs.ReadWord(2 * REGS_MULTIPLIER));
        }

        [TestMethod]
        public void AddImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 3);    // r2 contains the value 3

            // It is previously known that cpu.decode works correctly,
            // so it is valid to use here
            Instruction test = cpu.decode(0xE2821003); // add r1, r2, #3
            cpu.execute(test);

            Assert.AreEqual((uint)6, testRegs.ReadWord(1 * REGS_MULTIPLIER));
        }

        [TestMethod]
        public void SubImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 4);    // r2 contains the value 4

            // It is previously known that cpu.decode works correctly,
            // so it is valid to use here
            Instruction test = cpu.decode(0xE2421003); // sub r1, r2, #3
            cpu.execute(test);

            Assert.AreEqual((uint)1, testRegs.ReadWord(1 * REGS_MULTIPLIER));
        }

        [TestMethod]
        public void RsbImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 3);    // r2 contains the value 3

            // It is previously known that cpu.decode works correctly,
            // so it is valid to use here
            Instruction test = cpu.decode(0xE2621004); // rsb r1, r2, #4
            cpu.execute(test);

            Assert.AreEqual((uint)1, testRegs.ReadWord(1 * REGS_MULTIPLIER));
        }

        [TestMethod]
        public void MulImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 4);    // r2 contains the value 4
            cpu.registers.WriteWord(3 * REGS_MULTIPLIER, 5);    // r3 contains the value 5

            // It is previously known that cpu.decode works correctly,
            // so it is valid to use here
            Instruction test = cpu.decode(0xE0010392);          // mul r1, r2, r3
            cpu.execute(test);

            Assert.AreEqual((uint)20, testRegs.ReadWord(1 * REGS_MULTIPLIER));
        }

        [TestMethod]
        public void AndImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 0b1111);   // r2 contains the value 0b1111

            // It is previously known that cpu.decode works correctly,
            // so it is valid to use here
            Instruction test = cpu.decode(0xE202100A);              // and r1, r2, 0b1010
            cpu.execute(test);

            Assert.AreEqual((uint)10, testRegs.ReadWord(1 * REGS_MULTIPLIER));
        }

        [TestMethod]
        public void OrrImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 0b0101);   // r2 contains the value 0b0101

            // It is previously known that cpu.decode works correctly,
            // so it is valid to use here
            Instruction test = cpu.decode(0xE382100A);              // orr r1, r2, 0b1010
            cpu.execute(test);

            Assert.AreEqual((uint)15, testRegs.ReadWord(1 * REGS_MULTIPLIER));
        }

        [TestMethod]
        public void EorImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 0b1110);   // r2 contains the value 0b1110

            // It is previously known that cpu.decode works correctly,
            // so it is valid to use here
            Instruction test = cpu.decode(0xE2221009);              // eor r1, r2, 0b1001
            cpu.execute(test);

            Assert.AreEqual((uint)7, testRegs.ReadWord(1 * REGS_MULTIPLIER));
        }

        [TestMethod]
        public void BicImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 0xFFFFFFFF);   // r2 contains the value 0xFFFFFFFF

            // It is previously known that cpu.decode works correctly,
            // so it is valid to use here
            Instruction test = cpu.decode(0xE3C21001);                  // bic r1, r2, #1
            cpu.execute(test);

            Assert.AreEqual((uint)0xFFFFFFFE, testRegs.ReadWord(1 * REGS_MULTIPLIER));
        }

        [TestMethod]
        public void LdrImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 0x100);    // r2 contains the value 0x100
            cpu.ram.WriteWord(0x104, 0xFF);                         // address 0x100 in memory contains the value 0xFF

            // It is previously known that cpu.decode works correctly,
            // so it is valid to use here
            Instruction test = cpu.decode(0xE5921004);              // ldr r1, [r2, #4]
            cpu.execute(test);

            Assert.AreEqual((uint)0xFF, testRegs.ReadWord(1 * REGS_MULTIPLIER));
        }

        [TestMethod]
        public void StrImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(1 * REGS_MULTIPLIER, 0xFF);     // r1 contains the value 0xFF
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 0x100);    // r2 contains the value 0x100

            // It is previously known that cpu.decode works correctly,
            // so it is valid to use here
            Instruction test = cpu.decode(0xE5821004);              // str r1, [r2, #4]
            cpu.execute(test);

            Assert.AreEqual((uint)0xFF, cpu.ram.ReadWord(0x104));
        }

        [TestMethod]
        public void StrRegOff_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(1 * REGS_MULTIPLIER, 0xFF);     // r1 contains the value 0xFF
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 0x100);    // r2 contains the value 0x100
            cpu.registers.WriteWord(3 * REGS_MULTIPLIER, 4);        // r3 contains the value 0x4

            // It is previously known that cpu.decode works correctly,
            // so it is valid to use here
            Instruction test = cpu.decode(0xE7821003);              // str r1, [r2, r3]
            cpu.execute(test);

            Assert.AreEqual((uint)0xFF, cpu.ram.ReadWord(0x104));
        }

        [TestMethod]
        public void StrScaledRegOff_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(1 * REGS_MULTIPLIER, 0xFF);     // r1 contains the value 0xFF
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 0x100);    // r2 contains the value 0x100
            cpu.registers.WriteWord(3 * REGS_MULTIPLIER, 1);        // r3 contains the value 0x1

            // It is previously known that cpu.decode works correctly,
            // so it is valid to use here
            Instruction test = cpu.decode(0xE7821103);              // str r1, [r2, r3, lsl #2]
            cpu.execute(test);

            Assert.AreEqual((uint)0xFF, cpu.ram.ReadWord(0x104));
        }

        [TestMethod]
        public void StrImmWB_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(1 * REGS_MULTIPLIER, 0xFF);     // r1 contains the value 0xFF
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 0x100);    // r2 contains the value 0x100

            // It is previously known that cpu.decode works correctly,
            // so it is valid to use here
            Instruction test = cpu.decode(0xE5a21004);              // str r1, [r2, #4]!
            cpu.execute(test);

            Assert.AreEqual((uint)0xFF, cpu.ram.ReadWord(0x104));
            Assert.AreEqual((uint)0x104, cpu.registers.ReadWord(2 * REGS_MULTIPLIER));
        }

        [TestMethod]
        public void StrImmNegOff_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(1 * REGS_MULTIPLIER, 0xFF);     // r1 contains the value 0xFF
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 0x108);    // r2 contains the value 0x108

            // It is previously known that cpu.decode works correctly,
            // so it is valid to use here
            Instruction test = cpu.decode(0xE5021004);              // str r1, [r2, #-4]
            cpu.execute(test);

            Assert.AreEqual((uint)0xFF, cpu.ram.ReadWord(0x104));
        }
    }
}
