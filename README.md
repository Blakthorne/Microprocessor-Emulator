# Created by David Polar

## A more detailed description can be found [here](report.pdf), in the *Technical Report*.

# Overview
The program can be opened via the command line with options and gracefully handles errors in the GUI. IF a file is not provided via the command line, the user can choose a file with the native file picker to load. Upon loading the file, it displays a memory dump, disassembly (with fake instructions), registers, flags, stack, and a terminal (not functional). The simulator can run or step through the program, stop while being run, reset execution, and add breakpoints. All of these commands are available thorugh the following keyboard shortcuts:
 - Load file: `Ctrl-O`
 - Run: `F5`
 - Single-step: `F10`
 - Stop execution: `Ctrl-Q`
 - Reset: `Ctrl-R`
 - Open breakpoint dialog: `Ctrl-B`

The simulator will run the program on a background thread so as to not lock up the GUI while being run. When a breakpoint is hit, the simulator will stop execution and update the GUI.

# Prerequisites
 - Windows 10 operating system
 - .NET Core Desktop Runtime
 - MSTest.TestAdapter
 - MSTest.TestFramework
 - CommandLineParser distributed by gsscoder,nemec,ericnewton76,moh-hassan
 - log4net distributed by The Apache Software Foundation

# Build and Test
## IDE Instructions
Verify that it will be building in Release mode. To add command line arguments, go to *Project > armsim Properties*. Click on the Debug pane and, under Start options, enter the command line arguments. To run the unit tests, go to *Test > Run All Tests*. This will open the Test Explorer and display the test results upon completion.  
## Command Line Instructions
Open the command prompt by either  
1. Searching for Developer Command Prompt for VS 2019 in the Windows Search Bar
2. Going to the applications and finding it in the Visual Studio 2019 folder

Navigate to the project solution file. In the project hierarchy, it is located in *src > armsim*. Enter `msbuild armsim.sln` to build the project. This will create the executable file in *src > armsim > armsim > bin > Debug*. To run the program, navigate to the Debug folder and enter `armsim.exe` with the appropriate command line arguments.  

# Configuration
Logging is, by default, turned on and being directed to the file *armsim.log*. To turn off logging, navigate to the file Loader.cs in *src > armsim*. Find the *loggingEnabled* variable at the top of the *Loader* class, and change its value to *false*. The *armsim.log* file will still be created at build time, but nothing will be written to it.

# User Guide
To run the program navigate into the *install* folder containing the *armsim.exe* file. The usage for the command line is `armsim [ --mem memory-size ] [ elf-file ]`, where `memory-size` must be between 0 and 1,000,000, and `elf-file` must be an ELF file. The application will start and display the GUI. If the file was provided in the command line, it displays the content of the file - if not, the only button enabled will be `Load`. To load a file, either click `Load` or use the key shortcut. The rest of the application should be functional.

# Instruction Implementation

## Data Processing

### Instructions
 - MOV
 - MVN
 - ADD
 - SUB
 - RSB
 - MUL
 - AND
 - ORR
 - EOR
 - BIC

### Addressing Modes
 - Barrel shifter with: lsl, lsr, asr, and ror
 - Operand2 with: immediate, register with immediate shift, and register with register shift

## Load/Store

### Instructions
 - LDR
 - STR
 - LDM
 - STM

### Addressing Modes
 - LDR/STR: word and unsigned byte, preindexed, with and without writeback
 - LDM/STM: FD variant, with and without writeback

## SWI

- 0x00
- 0x11
- 0x6a

## CMP

## B, BL, and BX