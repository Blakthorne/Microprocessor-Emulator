// MemoryTest.cs
// Contains unit testing for the Memory class

using System;
using System.Security.Policy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using armsim;

namespace armsimTest
{
    // Tests the methods in the Memory class
    [TestClass]
    public class MemoryTest
    {
        [TestMethod]
        public void ReadByte_Pass()
        {
            Memory testMemory = new Memory(32768);
            testMemory.memArray[0] = 0xCC;   // 1100 1100
            Assert.AreEqual(0xCC, testMemory.ReadByte(0));
        }

        [TestMethod]
        public void ReadHalfWord_Pass()
        {
            Memory testMemory = new Memory(32768);
            testMemory.memArray[0] = 0xC3;   // 1100 0011
            testMemory.memArray[1] = 0x3C;   // 0011 1100
            Assert.AreEqual((ushort)0x3CC3, testMemory.ReadHalfWord(0));
        }

        [TestMethod]
        public void ReadHalfWord_Fail()
        {
            Memory testMemory = new Memory(32768);
            testMemory.memArray[0] = 0xC3;   // 1100 0011
            testMemory.memArray[1] = 0x3C;   // 0011 1100
            try
            {
                ushort result = testMemory.ReadHalfWord(1);
            }
            catch (Exception e)
            {
                Assert.IsNotNull(e);
            }
        }

        [TestMethod]
        public void ReadWord_Pass()
        {
            Memory testMemory = new Memory(32768);
            testMemory.memArray[0] = 0xC3;   // 1100 0011
            testMemory.memArray[1] = 0x3C;   // 0011 1100
            testMemory.memArray[2] = 0xAA;   // 1010 1010
            testMemory.memArray[3] = 0x55;   // 0101 0101
            Assert.AreEqual((uint)0x55AA3CC3, testMemory.ReadWord(0));
        }

        [TestMethod]
        public void ReadWord_Fail()
        {
            Memory testMemory = new Memory(32768);
            testMemory.memArray[0] = 0xC3;   // 1100 0011
            testMemory.memArray[1] = 0x3C;   // 0011 1100
            testMemory.memArray[2] = 0xAA;   // 1010 1010
            testMemory.memArray[3] = 0x55;   // 0101 0101
            try
            {
                uint result = testMemory.ReadWord(3);
            }
            catch (Exception e)
            {
                Assert.IsNotNull(e);
            }
        }

        [TestMethod]
        public void WriteByte_Pass()
        {
            Memory testMemory = new Memory(32768);
            testMemory.WriteByte(0, 0xAA);    // 1010 1010
            Assert.AreEqual(0xAA, testMemory.memArray[0]);
        }

        [TestMethod]
        public void WriteHalfWord_Pass()
        {
            Memory testMemory = new Memory(32768);
            testMemory.WriteHalfWord(0, 0xC355);    // 1100 0011 0101 0101
            Assert.AreEqual(0x55, testMemory.memArray[0]);
            Assert.AreEqual(0xC3, testMemory.memArray[1]);
        }

        [TestMethod]
        public void WriteHalfWord_Fail()
        {
            Memory testMemory = new Memory(32768);
            try
            {
                testMemory.WriteHalfWord(5, 0xC355);    // 1100 0011 0101 0101
            }
            catch (Exception e)
            {
                Assert.IsNotNull(e);
            }
        }

        [TestMethod]
        public void WriteWord_Pass()
        {
            Memory testMemory = new Memory(32768);
            testMemory.WriteWord(0, 0x55AA3CC3);
            Assert.AreEqual(0xC3, testMemory.memArray[0]);
            Assert.AreEqual(0x3C, testMemory.memArray[1]);
            Assert.AreEqual(0xAA, testMemory.memArray[2]);
            Assert.AreEqual(0x55, testMemory.memArray[3]);
        }

        [TestMethod]
        public void WriteWord_Fail()
        {
            Memory testMemory = new Memory(32768);
            try
            {
                testMemory.WriteWord(3, 0x55AA3CC3);
            }
            catch (Exception e)
            {
                Assert.IsNotNull(e);
            }
        }

        [TestMethod]
        public void CalculateChecksum_Pass()
        {
            Memory testMemory = new Memory(32768);
            testMemory.memArray[0] = 0x01;
            testMemory.memArray[1] = 0x82;
            testMemory.memArray[2] = 0x03;
            testMemory.memArray[3] = 0x84;
            Assert.AreEqual(536854790, testMemory.CalculateChecksum());
        }

        [TestMethod]
        public void TestFlag_Pass()
        {
            Memory testMemory = new Memory(32768);
            testMemory.memArray[0] = 0x08;     // 0000 1000
            Assert.IsTrue(testMemory.TestFlag(0, 3));
        }

        [TestMethod]
        public void SetFlag_FalseFlag()
        {
            Memory testMemory = new Memory(32786);
            testMemory.memArray[0] = 0x88;     // 1000 1000
            testMemory.SetFlag(0, 3, false);
            Assert.AreEqual(0x80, testMemory.memArray[0]);
        }

        [TestMethod]
        public void SetFlag_TrueFlag()
        {
            Memory testMemory = new Memory(32786);
            testMemory.memArray[0] = 0x80;     // 1000 0000
            testMemory.SetFlag(0, 3, true);
            Assert.AreEqual(0x88, testMemory.memArray[0]);
        }

        [TestMethod]
        public void ExtractBits_Pass()
        {
            Memory testMemory = new Memory(32786);
            testMemory.memArray[0] = 0xA5;     // 1010 0101
            Assert.AreEqual((uint)4, Memory.ExtractBits(0xA5, 1, 3));
        }

        [TestMethod]
        public void ExtractBits_Fail()
        {
            Memory testMemory = new Memory(32768);
            testMemory.memArray[0] = 0xA5;     // 1010 0101
            try
            {
                uint result = Memory.ExtractBits(0xA5, 3, 1);
            }
            catch (Exception e)
            {
                Assert.IsNotNull(e);
            }
        }

        [TestMethod]
        public void ShiftToEnd_Pass()
        {
            Memory testMemory = new Memory(32786);
            testMemory.memArray[0] = 0xA5;     // 1010 0101
            Assert.AreEqual((uint)9, Memory.ShiftToEnd(0xA5, 2, 5));
        }
    }
}
