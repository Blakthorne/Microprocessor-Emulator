// Options.cs
// Contains the implementation for the 
// command line parser from the CommandLine API

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;  // documentation can be found at https://github.com/commandlineparser/commandline
using CommandLine.Text;

namespace armsim
{
    public class Options
    {
        // Create a MemSize property using the CommandLine parser
        // to be used by the Parser class
        [Option("mem", Required = false, Default = (int)32768,
                       HelpText = "The number of bytes in the simulated RAM. Default: 32768")]
        public int MemSize { get; set; }

        // Create an Exec property using the CommandLine parser
        // to be used by the Parser class
        [Option("exec", Required = false,
                       HelpText = "Immediately execute program and shut down upon completion.")]
        public bool Exec { get; set; }

        // The parser reads the .exe file as a Value, but will
        // not allow the first Value be at index 1.
        // This Value property is to take up space in the indexing
        // and serves no other purpose.
        [Value(0)]
        public string ExecutablePath { get; set; }

        // Create a FileName property using the CommandLine parser
        // to be used by the Parser class
        [Value(1)]
        public string FileName { get; set; }

        // Sets up a usage statement to be displayed on parsing failure
        [Usage(ApplicationAlias = "armsim")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>() {
                new Example("Load an ELF executable into memory", new Options { FileName = "elf-file.exe" })
                };
            }
        }
    }
}
