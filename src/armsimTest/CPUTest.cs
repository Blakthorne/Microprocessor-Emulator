// CPUTest.cs
// Contains unit testing for the CPU class

using System;
using System.Security.Policy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using armsim;

namespace armsimTest
{
    // Tests the methods in the Memory class
    [TestClass]
    public class CPUTest
    {
        [TestMethod]
        public void ReadByte_Pass()
        {
            Memory testMemory = new Memory(32768);
            testMemory.memArray[0] = 0xCC;   // 1100 1100
            Assert.AreEqual(0xCC, testMemory.ReadByte(0));
        }
    }
}
