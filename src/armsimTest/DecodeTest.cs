// DecodeTest.cs
// Contains unit testing for the Memory class

using System;
using System.Security.Policy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using armsim;

namespace armsimTest
{
    // Tests the methods in the Instruction and derived classes
    [TestClass]
    public class DecodeTest
    {
        const int NUM_REGISTERS = 17 * 4;   // number of registers; r0-r15 and CPSR
        const int REGS_MULTIPLIER = 4;

        [TestMethod]
        public void MovImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);

            Instruction test = cpu.decode(0xE3A02030);  // mov r2, #48

            Assert.AreEqual((uint)0b1110, test.conditionFlags);
            Assert.AreEqual((uint)0b001, test.type);
            Assert.AreEqual((uint)0b1101, test.opcode_DP);
            Assert.AreEqual((uint)0b0, test.s_DP);
            Assert.AreEqual((uint)0b0000, test.rn_DP);
            Assert.AreEqual((uint)0b0010, test.rd_DP);
            Assert.AreEqual((uint)0b0000, test.rotate_DP);
            Assert.AreEqual((uint)0b110000, test.imm_DP);
            Assert.AreEqual("mov r2, #48", test.text);
        }

        [TestMethod]
        public void MovImmRotate_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);

            Instruction test = cpu.decode(0xE3A0220A);  // mov r2, #A000 0000

            Assert.AreEqual((uint)0b1110, test.conditionFlags);
            Assert.AreEqual((uint)0b001, test.type);
            Assert.AreEqual((uint)0b1101, test.opcode_DP);
            Assert.AreEqual((uint)0b0, test.s_DP);
            Assert.AreEqual((uint)0b0000, test.rn_DP);
            Assert.AreEqual((uint)0b0010, test.rd_DP);
            Assert.AreEqual((uint)0b0010, test.rotate_DP);
            Assert.AreEqual((uint)0b1010, test.imm_DP);
            Assert.AreEqual("mov r2, #2684354560", test.text);
        }

        [TestMethod]
        public void MovLslShiftImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 1);    // r2 contains the value 1

            Instruction test = cpu.decode(0xE1A01182); // mov r1, r2, lsl #3

            Assert.AreEqual((uint)0b1110, test.conditionFlags);
            Assert.AreEqual((uint)0b000, test.type);
            Assert.AreEqual((uint)0b0, test.bit4);
            Assert.AreEqual((uint)0b1101, test.opcode_DP);
            Assert.AreEqual((uint)0b0, test.s_DP);
            Assert.AreEqual((uint)0b0000, test.rn_DP);
            Assert.AreEqual((uint)0b0001, test.rd_DP);
            Assert.AreEqual((uint)0b00, test.shiftCode_DP);
            Assert.AreEqual((uint)0b0010, test.rm_DP);
            Assert.AreEqual((uint)0b00011, test.shiftAmount_DP);
            Assert.AreEqual("mov r1, r2, lsl #3", test.text);
        }

        [TestMethod]
        public void MovLsrShiftImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 8);    // r2 contains the value 8

            Instruction test = cpu.decode(0xE1A011A2); // mov r1, r2, lsr #3

            Assert.AreEqual((uint)0b1110, test.conditionFlags);
            Assert.AreEqual((uint)0b000, test.type);
            Assert.AreEqual((uint)0b0, test.bit4);
            Assert.AreEqual((uint)0b1101, test.opcode_DP);
            Assert.AreEqual((uint)0b0, test.s_DP);
            Assert.AreEqual((uint)0b0000, test.rn_DP);
            Assert.AreEqual((uint)0b0001, test.rd_DP);
            Assert.AreEqual((uint)0b01, test.shiftCode_DP);
            Assert.AreEqual((uint)0b0010, test.rm_DP);
            Assert.AreEqual((uint)0b00011, test.shiftAmount_DP);
            Assert.AreEqual("mov r1, r2, lsr #3", test.text);
        }

        [TestMethod]
        public void MovAsrShiftImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 0x80000000);    // r2 contains the value 0x8000 0000

            Instruction test = cpu.decode(0xE1A011C2); // mov r1, r2, asr #3

            Assert.AreEqual((uint)0b1110, test.conditionFlags);
            Assert.AreEqual((uint)0b000, test.type);
            Assert.AreEqual((uint)0b0, test.bit4);
            Assert.AreEqual((uint)0b1101, test.opcode_DP);
            Assert.AreEqual((uint)0b0, test.s_DP);
            Assert.AreEqual((uint)0b0000, test.rn_DP);
            Assert.AreEqual((uint)0b0001, test.rd_DP);
            Assert.AreEqual((uint)0b10, test.shiftCode_DP);
            Assert.AreEqual((uint)0b0010, test.rm_DP);
            Assert.AreEqual((uint)0b00011, test.shiftAmount_DP);
            Assert.AreEqual("mov r1, r2, asr #3", test.text);
        }

        [TestMethod]
        public void MovLslShiftReg_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 1);    // r2 contains the value 1
            cpu.registers.WriteWord(4 * REGS_MULTIPLIER, 3);    // r4 contains the value 3

            Instruction test = cpu.decode(0xE1A01412); // mov r1, r2, lsl r4

            Assert.AreEqual((uint)0b1110, test.conditionFlags);
            Assert.AreEqual((uint)0b000, test.type);
            Assert.AreEqual((uint)0b1, test.bit4);
            Assert.AreEqual((uint)0b1101, test.opcode_DP);
            Assert.AreEqual((uint)0b0, test.s_DP);
            Assert.AreEqual((uint)0b0000, test.rn_DP);
            Assert.AreEqual((uint)0b0001, test.rd_DP);
            Assert.AreEqual((uint)0b0100, test.rs_DP);
            Assert.AreEqual((uint)0b00, test.shiftCode_DP);
            Assert.AreEqual((uint)0b0010, test.rm_DP);
            Assert.AreEqual("mov r1, r2, lsl r4", test.text);
        }

        [TestMethod]
        public void MovLsrShiftReg_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 8);    // r2 contains the value 8
            cpu.registers.WriteWord(4 * REGS_MULTIPLIER, 3);    // r4 contains the value 3

            Instruction test = cpu.decode(0xE1A01432); // mov r1, r2, lsr r4

            Assert.AreEqual((uint)0b1110, test.conditionFlags);
            Assert.AreEqual((uint)0b000, test.type);
            Assert.AreEqual((uint)0b1, test.bit4);
            Assert.AreEqual((uint)0b1101, test.opcode_DP);
            Assert.AreEqual((uint)0b0, test.s_DP);
            Assert.AreEqual((uint)0b0000, test.rn_DP);
            Assert.AreEqual((uint)0b0001, test.rd_DP);
            Assert.AreEqual((uint)0b0100, test.rs_DP);
            Assert.AreEqual((uint)0b01, test.shiftCode_DP);
            Assert.AreEqual((uint)0b0010, test.rm_DP);
            Assert.AreEqual("mov r1, r2, lsr r4", test.text);
        }

        [TestMethod]
        public void MovAsrShiftReg_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 0x80000000);   // r2 contains the value 0x8000 0000
            cpu.registers.WriteWord(4 * REGS_MULTIPLIER, 3);            // r4 contains the value 3

            Instruction test = cpu.decode(0xE1A01452); // mov r1, r2, asr r4

            Assert.AreEqual((uint)0b1110, test.conditionFlags);
            Assert.AreEqual((uint)0b000, test.type);
            Assert.AreEqual((uint)0b1, test.bit4);
            Assert.AreEqual((uint)0b1101, test.opcode_DP);
            Assert.AreEqual((uint)0b0, test.s_DP);
            Assert.AreEqual((uint)0b0000, test.rn_DP);
            Assert.AreEqual((uint)0b0001, test.rd_DP);
            Assert.AreEqual((uint)0b0100, test.rs_DP);
            Assert.AreEqual((uint)0b10, test.shiftCode_DP);
            Assert.AreEqual((uint)0b0010, test.rm_DP);
            Assert.AreEqual("mov r1, r2, asr r4", test.text);
        }

        [TestMethod]
        public void MvnImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);

            Instruction test = cpu.decode(0xE3E02006);  // mvn r2, #6

            Assert.AreEqual((uint)0b1110, test.conditionFlags);
            Assert.AreEqual((uint)0b001, test.type);
            Assert.AreEqual((uint)0b1111, test.opcode_DP);
            Assert.AreEqual((uint)0b0, test.s_DP);
            Assert.AreEqual((uint)0b0000, test.rn_DP);
            Assert.AreEqual((uint)0b0010, test.rd_DP);
            Assert.AreEqual((uint)0b0000, test.rotate_DP);
            Assert.AreEqual((uint)0b0110, test.imm_DP);
            Assert.AreEqual("mvn r2, #6", test.text);
        }

        [TestMethod]
        public void AddImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);

            Instruction test = cpu.decode(0xE2821003);          // add r1, r2, #3
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 3);    // r2 contains the value 3

            Assert.AreEqual((uint)0b1110, test.conditionFlags);
            Assert.AreEqual((uint)0b001, test.type);
            Assert.AreEqual((uint)0b0100, test.opcode_DP);
            Assert.AreEqual((uint)0b0, test.s_DP);
            Assert.AreEqual((uint)0b0010, test.rn_DP);
            Assert.AreEqual((uint)0b0001, test.rd_DP);
            Assert.AreEqual((uint)0b0000, test.rotate_DP);
            Assert.AreEqual((uint)0b0011, test.imm_DP);
            Assert.AreEqual("add r1, r2, #3", test.text);
        }

        [TestMethod]
        public void SubImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);

            Instruction test = cpu.decode(0xE2421003);          // sub r1, r2, #3
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 4);    // r2 contains the value 4

            Assert.AreEqual((uint)0b1110, test.conditionFlags);
            Assert.AreEqual((uint)0b001, test.type);
            Assert.AreEqual((uint)0b0010, test.opcode_DP);
            Assert.AreEqual((uint)0b0, test.s_DP);
            Assert.AreEqual((uint)0b0010, test.rn_DP);
            Assert.AreEqual((uint)0b0001, test.rd_DP);
            Assert.AreEqual((uint)0b0000, test.rotate_DP);
            Assert.AreEqual((uint)0b0011, test.imm_DP);
            Assert.AreEqual("sub r1, r2, #3", test.text);
        }

        [TestMethod]
        public void RsbImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);

            Instruction test = cpu.decode(0xE2621004);          // rsb r1, r2, #4
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 3);    // r2 contains the value 3

            Assert.AreEqual((uint)0b1110, test.conditionFlags);
            Assert.AreEqual((uint)0b001, test.type);
            Assert.AreEqual((uint)0b0011, test.opcode_DP);
            Assert.AreEqual((uint)0b0, test.s_DP);
            Assert.AreEqual((uint)0b0010, test.rn_DP);
            Assert.AreEqual((uint)0b0001, test.rd_DP);
            Assert.AreEqual((uint)0b0000, test.rotate_DP);
            Assert.AreEqual((uint)0b0100, test.imm_DP);
            Assert.AreEqual("rsb r1, r2, #4", test.text);
        }

        [TestMethod]
        public void MulImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);

            Instruction test = cpu.decode(0xE0010392);          // mul r1, r2, r3
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 4);    // r2 contains the value 4
            cpu.registers.WriteWord(3 * REGS_MULTIPLIER, 5);    // r3 contains the value 5

            Assert.AreEqual((uint)0b1110, test.conditionFlags);
            Assert.AreEqual((uint)0b000, test.type);
            Assert.AreEqual((uint)0b0, test.s_M);
            Assert.AreEqual((uint)0b0001, test.rd_M);
            Assert.AreEqual((uint)0b0010, test.rm_M);
            Assert.AreEqual((uint)0b1, test.bit7);
            Assert.AreEqual((uint)0b1, test.bit4);
            Assert.AreEqual((uint)0b0011, test.rs_M);
            Assert.AreEqual("mul r1, r2, r3", test.text);
        }

        [TestMethod]
        public void AndImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);

            Instruction test = cpu.decode(0xE202100A);              // and r1, r2, 0b1010
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 0b1111);   // r2 contains the value 0b1111

            Assert.AreEqual((uint)0b1110, test.conditionFlags);
            Assert.AreEqual((uint)0b001, test.type);
            Assert.AreEqual((uint)0b0000, test.opcode_DP);
            Assert.AreEqual((uint)0b0, test.s_DP);
            Assert.AreEqual((uint)0b0010, test.rn_DP);
            Assert.AreEqual((uint)0b0001, test.rd_DP);
            Assert.AreEqual((uint)0b0000, test.rotate_DP);
            Assert.AreEqual((uint)0b1010, test.imm_DP);
            Assert.AreEqual("and r1, r2, #10", test.text);
        }

        [TestMethod]
        public void OrrImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);

            Instruction test = cpu.decode(0xE382100A);              // orr r1, r2, 0b1010
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 0b0101);   // r2 contains the value 0b0101

            Assert.AreEqual((uint)0b1110, test.conditionFlags);
            Assert.AreEqual((uint)0b001, test.type);
            Assert.AreEqual((uint)0b1100, test.opcode_DP);
            Assert.AreEqual((uint)0b0, test.s_DP);
            Assert.AreEqual((uint)0b0010, test.rn_DP);
            Assert.AreEqual((uint)0b0001, test.rd_DP);
            Assert.AreEqual((uint)0b0000, test.rotate_DP);
            Assert.AreEqual((uint)0b1010, test.imm_DP);
            Assert.AreEqual("orr r1, r2, #10", test.text);
        }

        [TestMethod]
        public void EorImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);

            Instruction test = cpu.decode(0xE2221009);              // eor r1, r2, 0b1001
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 0b0110);   // r2 contains the value 0b0110

            Assert.AreEqual((uint)0b1110, test.conditionFlags);
            Assert.AreEqual((uint)0b001, test.type);
            Assert.AreEqual((uint)0b0001, test.opcode_DP);
            Assert.AreEqual((uint)0b0, test.s_DP);
            Assert.AreEqual((uint)0b0010, test.rn_DP);
            Assert.AreEqual((uint)0b0001, test.rd_DP);
            Assert.AreEqual((uint)0b0000, test.rotate_DP);
            Assert.AreEqual((uint)0b1001, test.imm_DP);
            Assert.AreEqual("eor r1, r2, #9", test.text);
        }

        [TestMethod]
        public void BicImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);

            Instruction test = cpu.decode(0xE3C21001);                  // bic r1, r2, #1
            cpu.registers.WriteWord(2 * REGS_MULTIPLIER, 0xFFFFFFFF);   // r2 contains the value 0xFFFFFFFF

            Assert.AreEqual((uint)0b1110, test.conditionFlags);
            Assert.AreEqual((uint)0b001, test.type);
            Assert.AreEqual((uint)0b1110, test.opcode_DP);
            Assert.AreEqual((uint)0b0, test.s_DP);
            Assert.AreEqual((uint)0b0010, test.rn_DP);
            Assert.AreEqual((uint)0b0001, test.rd_DP);
            Assert.AreEqual((uint)0b0000, test.rotate_DP);
            Assert.AreEqual((uint)0b0001, test.imm_DP);
            Assert.AreEqual("bic r1, r2, #1", test.text);
        }

        [TestMethod]
        public void StrImm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);

            Instruction test = cpu.decode(0xE5821004);         // str r1, [r2, #4]
            cpu.registers.WriteWord(1 * REGS_MULTIPLIER, 8);   // r1 contains the value 8

            Assert.AreEqual((uint)0b1110, test.conditionFlags);
            Assert.AreEqual((uint)0b010, test.type);
            Assert.AreEqual((uint)0b1, test.p_LS);
            Assert.AreEqual((uint)0b1, test.u_LS);
            Assert.AreEqual((uint)0b0, test.b_LS);
            Assert.AreEqual((uint)0b0, test.w_LS);
            Assert.AreEqual((uint)0b0, test.l_LS);
            Assert.AreEqual((uint)2, test.rn_LS);
            Assert.AreEqual((uint)1, test.rd_LS);
            Assert.AreEqual((uint)4, test.imm_LS);
            Assert.AreEqual("str r1, [r2, #4]", test.text);
        }

        [TestMethod]
        public void StrReg_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);

            Instruction test = cpu.decode(0xE7821003);         // str r1, [r2, r3]
            cpu.registers.WriteWord(1 * REGS_MULTIPLIER, 8);   // r1 contains the value 8
            cpu.registers.WriteWord(3 * REGS_MULTIPLIER, 4);   // r3 contains the value 4

            Assert.AreEqual((uint)0b1110, test.conditionFlags);
            Assert.AreEqual((uint)0b011, test.type);
            Assert.AreEqual((uint)0b1, test.p_LS);
            Assert.AreEqual((uint)0b1, test.u_LS);
            Assert.AreEqual((uint)0b0, test.b_LS);
            Assert.AreEqual((uint)0b0, test.w_LS);
            Assert.AreEqual((uint)0b0, test.l_LS);
            Assert.AreEqual((uint)2, test.rn_LS);
            Assert.AreEqual((uint)1, test.rd_LS);
            Assert.AreEqual((uint)3, test.rm_LS);
            Assert.AreEqual("str r1, [r2, r3]", test.text);
        }

        [TestMethod]
        public void StrRegWb_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);

            Instruction test = cpu.decode(0xE7A21003);         // str r1, [r2, r3]!
            cpu.registers.WriteWord(1 * REGS_MULTIPLIER, 8);   // r1 contains the value 8
            cpu.registers.WriteWord(3 * REGS_MULTIPLIER, 4);   // r3 contains the value 4

            Assert.AreEqual((uint)0b1110, test.conditionFlags);
            Assert.AreEqual((uint)0b011, test.type);
            Assert.AreEqual((uint)0b1, test.p_LS);
            Assert.AreEqual((uint)0b1, test.u_LS);
            Assert.AreEqual((uint)0b0, test.b_LS);
            Assert.AreEqual((uint)0b1, test.w_LS);
            Assert.AreEqual((uint)0b0, test.l_LS);
            Assert.AreEqual((uint)2, test.rn_LS);
            Assert.AreEqual((uint)1, test.rd_LS);
            Assert.AreEqual((uint)3, test.rm_LS);
            Assert.AreEqual("str r1, [r2, r3]!", test.text);
        }

        [TestMethod]
        public void StrRegByte_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);

            Instruction test = cpu.decode(0xE7C21003);         // strb r1, [r2, r3]
            cpu.registers.WriteWord(1 * REGS_MULTIPLIER, 8);   // r1 contains the value 8
            cpu.registers.WriteWord(3 * REGS_MULTIPLIER, 4);   // r3 contains the value 4

            Assert.AreEqual((uint)0b1110, test.conditionFlags);
            Assert.AreEqual((uint)0b011, test.type);
            Assert.AreEqual((uint)0b1, test.p_LS);
            Assert.AreEqual((uint)0b1, test.u_LS);
            Assert.AreEqual((uint)0b1, test.b_LS);
            Assert.AreEqual((uint)0b0, test.w_LS);
            Assert.AreEqual((uint)0b0, test.l_LS);
            Assert.AreEqual((uint)2, test.rn_LS);
            Assert.AreEqual((uint)1, test.rd_LS);
            Assert.AreEqual((uint)3, test.rm_LS);
            Assert.AreEqual("strb r1, [r2, r3]", test.text);
        }

        [TestMethod]
        public void LdrReg_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);

            Instruction test = cpu.decode(0xE7921003);         // ldr r1, [r2, r3]
            cpu.registers.WriteWord(1 * REGS_MULTIPLIER, 8);   // r1 contains the value 8
            cpu.registers.WriteWord(3 * REGS_MULTIPLIER, 4);   // r3 contains the value 4

            Assert.AreEqual((uint)0b1110, test.conditionFlags);
            Assert.AreEqual((uint)0b011, test.type);
            Assert.AreEqual((uint)0b1, test.p_LS);
            Assert.AreEqual((uint)0b1, test.u_LS);
            Assert.AreEqual((uint)0b0, test.b_LS);
            Assert.AreEqual((uint)0b0, test.w_LS);
            Assert.AreEqual((uint)0b1, test.l_LS);
            Assert.AreEqual((uint)2, test.rn_LS);
            Assert.AreEqual((uint)1, test.rd_LS);
            Assert.AreEqual((uint)3, test.rm_LS);
            Assert.AreEqual("ldr r1, [r2, r3]", test.text);
        }

        [TestMethod]
        public void LdrRegScaled_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);

            Instruction test = cpu.decode(0xE7921183);         // ldr r1, [r2, r3, lsl #3]
            cpu.registers.WriteWord(1 * REGS_MULTIPLIER, 8);   // r1 contains the value 8
            cpu.registers.WriteWord(3 * REGS_MULTIPLIER, 1);   // r3 contains the value 1

            Assert.AreEqual((uint)0b1110, test.conditionFlags);
            Assert.AreEqual((uint)0b011, test.type);
            Assert.AreEqual((uint)0b1, test.p_LS);
            Assert.AreEqual((uint)0b1, test.u_LS);
            Assert.AreEqual((uint)0b0, test.b_LS);
            Assert.AreEqual((uint)0b0, test.w_LS);
            Assert.AreEqual((uint)0b1, test.l_LS);
            Assert.AreEqual((uint)2, test.rn_LS);
            Assert.AreEqual((uint)1, test.rd_LS);
            Assert.AreEqual((uint)3, test.shiftAmount_LS);
            Assert.AreEqual((uint)0b00, test.shiftCode_LS);
            Assert.AreEqual((uint)3, test.rm_LS);
            Assert.AreEqual("ldr r1, [r2, r3, lsl #3]", test.text);
        }

        [TestMethod]
        public void Ldm_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);

            Instruction test = cpu.decode(0xE891110C);  // ldm r1, {r2, r3, r8, r12}

            Assert.AreEqual((uint)0b1110, test.conditionFlags);
            Assert.AreEqual((uint)0b100, test.type);
            Assert.AreEqual((uint)0b0, test.p_LSM);
            Assert.AreEqual((uint)0b1, test.u_LSM);
            Assert.AreEqual((uint)0b0, test.s_LSM);
            Assert.AreEqual((uint)0b0, test.w_LSM);
            Assert.AreEqual((uint)0b1, test.l_LSM);
            Assert.AreEqual((uint)1, test.rn_LSM);
            Assert.AreEqual("ldm r1, {r2, r3, r8, r12}", test.text);
        }

        [TestMethod]
        public void Swi_Pass()
        {
            Memory testMemory = new Memory(32768);
            Memory testRegs = new Memory(NUM_REGISTERS);
            CPU cpu = new CPU(testMemory, testRegs);

            Instruction test = cpu.decode(0xEF000001);  // swi #1

            Assert.AreEqual((uint)0b1110, test.conditionFlags);
            Assert.AreEqual((uint)0b111, test.type);
            Assert.AreEqual("swi #1", test.text);
        }
    }
}
