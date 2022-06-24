// Loader.cs
// Contains methods to implement loading
// of ELF executables into virtual memory.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO.Packaging;
using log4net;
using log4net.Config;

namespace armsim
{
    // Simulates loading contents of ELF into virtual memory
    public class Loader
    {
        // To log, set the loggingEnabled variable to true
        static bool loggingEnabled = true;

        private static int retCode = 0;     // the code to return with default of 0
        private static Memory memory;       // an instance of the Memory class
        private static Computer sim;        // the Computer instance that calls this class
        private static int memSize;         // size in bytes of virtual memory
        private static string fileName;     // name of ELF file to load
        private ELF elfHeader;              // struct to hold the information in the ELF header

        private static readonly ILog log = LogManager.GetLogger(typeof(Loader));

        /// <summary>
        /// Constructor for the Loader class
        /// </summary>
        /// <param name="_memSize">size in bytes of virtual memory</param>
        /// <param name="_fileName">name of ELF file to load</param>
        /// <param name="_memory">Reference to ram variable in the Computer class</param>
        public Loader(int _memSize, string _fileName, Memory _memory, Computer _sim)
        {
            memSize = _memSize;
            fileName = _fileName;
            memory = _memory;
            sim = _sim;
            Continue();
        }

        /// <summary>
        /// Struct to hold data read from ELF header
        /// From Dr. Schaub
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ELF
        {
            public byte EI_MAG0, EI_MAG1, EI_MAG2, EI_MAG3, EI_CLASS, EI_DATA, EI_VERSION;
            byte unused1, unused2, unused3, unused4, unused5, unused6, unused7, unused8, unused9;
            public ushort e_type;
            public ushort e_machine;
            public uint e_version;
            public uint e_entry;
            public uint e_phoff;
            public uint e_shoff;
            public uint e_flags;
            public ushort e_ehsize;
            public ushort e_phentsize;
            public ushort e_phnum;
            public ushort e_shentsize;
            public ushort e_shnum;
            public ushort e_shstrndx;
        }

        /// <summary>
        /// Struct to hold data read from a progMemory header
        /// </summary>
        public struct Program
        {
            public uint p_type;
            public uint p_offset;
            public uint p_vaddr;
            public uint p_paddr;
            public uint p_filesz;
            public uint p_memsz;
            public uint p_flags;
            public uint p_align;
        }

        /// <summary>
        /// Struct to hold data read from a section header
        /// </summary>
        public struct Section
        {
            public uint s_name;
            public uint s_type;
            public uint s_addr;
            public uint s_off;
            public uint s_size;
            public uint s_es;
            public uint s_flg;
            public uint s_lk;
            public uint s_inf;
            public uint s_al;
        }

        /// <summary>
        /// Getter method to retrieve program entry address from
        /// the ELF Header - the initial PC value
        /// </summary>
        /// <returns></returns>
        public uint GetElfEntry()
        {
            return elfHeader.e_entry;
        }

        /// <summary>
        /// Getter method to retrieve the retCode instance variable
        /// </summary>
        /// <returns></returns>
        public int GetRetCode()
        {
            return retCode;
        }

        /// <summary>
        /// Loads contents of ELF executable into virtual memory
        /// </summary>
        /// <param name="stream">The FileStream object of the executable created in Continue()</param>
        /// <param name="elfHeader">ELF struct instance</param>
        public void LoadFile(FileStream stream, ELF elfHeader)
        {
            // Read program header;
            // From Dr. Schaub
            stream.Seek(elfHeader.e_phoff, SeekOrigin.Begin);

            Program programHeader;  // struct to hold individual program header info
            byte[] headerData;
            byte[] data;
            int numBytesRead = 0;   // for record of offset in elfHeader

            XmlConfigurator.Configure();
            if (loggingEnabled == true)
            {
                log.Info("Simulator.LoadFile: Number of segments: " + elfHeader.e_phnum);
                log.Info("Simulator.LoadFile: Program header offset: " + elfHeader.e_phoff);
                log.Info("Simulator.LoadFile: Size of program headers: " + elfHeader.e_phentsize);
            }

            // Iterate over program headers
            for (int i = 0; i < elfHeader.e_phnum; ++i)
            {
                headerData = new byte[elfHeader.e_phentsize];   // create array of size specified in elfHeader
                numBytesRead += stream.Read(headerData, 0, (int)elfHeader.e_phentsize);
                programHeader = ByteArrayToStructure<Program>(headerData);

                if (loggingEnabled == true)
                {
                    log.Info("Simulator.LoadFile: Segment " + (i + 1) +
                                " - Address = " + (elfHeader.e_phoff + numBytesRead) +
                                ", Offset = " + programHeader.p_offset +
                                ", Size = " + programHeader.p_filesz);
                }

                stream.Seek(programHeader.p_offset, SeekOrigin.Begin);  // Go to program content in ELF file
                data = new byte[programHeader.p_filesz];
                stream.Read(data, 0, (int)programHeader.p_filesz);      // Load content into byte array

                // Write contents to virtual memory
                for (int j = 0; j < data.Length; ++j)
                {
                    if ((programHeader.p_vaddr + j) > memSize)
                    {
                        retCode = 7;
                        if (loggingEnabled == true)
                        {
                            log.Info("Simulator.LoadFile: Memory needs exceed what was provided");
                        }
                        return;
                    }
                    memory.WriteByte((uint)(programHeader.p_vaddr + j), data[j]);
                }

                // Set stream for next iteration at the next program header
                stream.Seek(elfHeader.e_phoff + numBytesRead, SeekOrigin.Begin);
            }

            // Seek to the second section header - .text
            stream.Seek(elfHeader.e_shoff + elfHeader.e_shentsize, SeekOrigin.Begin);

            Section sectionHeader;  // struct to hold individual section header info
            byte[] sectionHeaderData;
            numBytesRead = 0;   // for record of offset in elfHeader

            sectionHeaderData = new byte[elfHeader.e_shentsize];   // create array of size specified in elfHeader
            numBytesRead += stream.Read(sectionHeaderData, 0, (int)elfHeader.e_shentsize);
            sectionHeader = ByteArrayToStructure<Section>(sectionHeaderData);

            sim.textStart = sectionHeader.s_off;
        }

        /// <summary>
        /// Set up stream to get contents of ELF file
        /// </summary>
        public void ReadFile()
        {
            try
            {
                // Open ELF file and automatically close when finsihed
                using (FileStream stream = new FileStream(fileName, FileMode.Open))
                {
                    if (loggingEnabled == true)
                    {
                        log.Info("Simulator.ReadFile: Reading " + fileName + "...");
                    }

                    elfHeader = new ELF();  // Declare new ELF struct to hold ELF header data
                    byte[] data = new byte[Marshal.SizeOf(elfHeader)];  // Declare new byte array with size equal to ELF header struct
                    stream.Read(data, 0, data.Length);  // Read ELF header into data byte array                
                    elfHeader = ByteArrayToStructure<ELF>(data);    // Convert byte array to struct

                    // ELF signature found at https://en.wikipedia.org/wiki/List_of_file_signatures
                    if (!(elfHeader.EI_MAG0 == 0x7F &&
                          elfHeader.EI_MAG1 == 0x45 &&
                          elfHeader.EI_MAG2 == 0x4C &&
                          elfHeader.EI_MAG3 == 0x46))
                    {
                        retCode = 4;
                        if (loggingEnabled == true)
                        {
                            log.Info("Simulator.ReadFile: File is not in the ELF format");
                        }
                        return;
                    }

                    if (elfHeader.EI_CLASS != 1)
                    {
                        retCode = 6;
                        if (loggingEnabled == true)
                        {
                            log.Info("Simulator.ReadFile: File is not in a 32-bit format");
                        }
                        return;
                    }

                    LoadFile(stream, elfHeader);
                }
            }
            catch (IOException)
            {
                retCode = 5;
                return;
            }

        }

        /// <summary>
        /// Verify options before reading the file
        /// </summary>
        /// <returns>The retCode:
        ///     0: Load success
        ///     1: Not a .exe file
        ///     2: Invalid memory size specified
        ///     3: File does not exist
        ///     4: File not in ELF format
        ///     5: Cannot access file
        ///     6: File is not 32-bit format
        ///     7: Not enough space in memory
        /// </returns>
        public void Continue()
        {
            // Verify that the filename has the .exe extension
            if (fileName.Length < 5)    // need this for if Load is clicked on GUI and nothing selected on close of app
            {
                retCode = 1;
                if (loggingEnabled == true)
                {
                    log.Info("Simulator.Continue: File does not have the .exe extension");
                }
            }
            else if (!fileName.Substring(fileName.Length - 4).Equals(".exe"))
            {
                retCode = 1;
                if (loggingEnabled == true)
                {
                    log.Info("Simulator.Continue: File does not have the .exe extension");
                }
                return;
            }

            // Verify that the memory size is within acceptable boundaries:
            // Between 0 and 1,000,000
            if ((memSize > 1000000) || (memSize < 0))
            {
                retCode = 2;
                if (loggingEnabled == true)
                {
                    log.Info("Simulator.Continue: Memory is not within bounds");
                }
                return;
            }

            // Verify that the file specified actually exists
            if (!File.Exists(fileName))
            {
                retCode = 3;
                if (loggingEnabled == true)
                {
                    log.Info("Simulator.Continue: File does not exist");
                }
                return;
            }
            else   // Otherwise, read the file
            {
                ReadFile();
                return;
            }
        }

        /// <summary>
        /// Convert a byte array to a <Program> struct
        /// From Dr. Schaub
        /// </summary>
        /// <typeparam name="T">the <Program> struct</typeparam>
        /// <param name="bytes">data to convert</param>
        static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(),
                typeof(T));
            handle.Free();
            return stuff;
        }
    }
}
