// MainWindow.xaml.cs
// Begins the program, parses command line arguments,
// and outputs information to the GUI application

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommandLine;
using Microsoft.Win32;

namespace armsim
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static int memSize;      // size in bytes of virtual memory
        private static string fileName; // name of ELF file to load
        private static bool isExec;     // true if --exec was provided
        private static bool loadedFile; // true if the file was loaded with the GUI
        public static Computer sim;     // single instance of the Computer class
        private static Memory ram;      // instance of Memory class to simulate the virtual RAM
        private static Memory regs;     // instance of Memory class to simulate the registers
        public int[] breakArray;        // array of current breakpoints
        public bool traceOn;            // is trace turned on or not
        public static int traceCount;   // counter for the trace steps
        public static FileStream stream;
        public static StreamWriter writer;
        public static List<Instruction> decodedInstructions;

        // Rows for the GUI ListView panes
        List<Register> regsList = new List<Register>();
        List<Flag> flagsList = new List<Flag>();
        List<MemRow> memDisplayList = new List<MemRow>();
        List<StackRow> stackList = new List<StackRow>();
        List<DisRow> disList = new List<DisRow>();
        public static List<breakRow> breakList = new List<breakRow>();

        // Routed commands for key shortcuts
        public static RoutedCommand LoadShortcut = new RoutedCommand();
        public static RoutedCommand RunShortcut = new RoutedCommand();
        public static RoutedCommand StepShortcut = new RoutedCommand();
        public static RoutedCommand StopShortcut = new RoutedCommand();
        public static RoutedCommand ResetShortcut = new RoutedCommand();
        public static RoutedCommand BrkPntShortcut = new RoutedCommand();
        public static RoutedCommand TraceShortcut = new RoutedCommand();

        /// <summary>
        /// Initializes the GUI and parses command line arguments.
        /// Provides logic for binding key shortcuts
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // found at https://stackoverflow.com/questions/9343381/wpf-command-line-arguments-a-smart-way
            string[] args = Environment.GetCommandLineArgs();

            // Use the Parser class from the CommandLine namespace
            // and use the Default singleton.
            // If parsing is successful   - go to Continue,
            // If parsing is unsuccessful - to to HandleError
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(Run)
                .WithNotParsed(HandleError);

            // Key binding logic found at https://stackoverflow.com/questions/1361350/keyboard-shortcuts-in-wpf
            LoadShortcut.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control));
            RunShortcut.InputGestures.Add(new KeyGesture(Key.F5));
            StepShortcut.InputGestures.Add(new KeyGesture(Key.F10));
            StopShortcut.InputGestures.Add(new KeyGesture(Key.Q, ModifierKeys.Control));
            ResetShortcut.InputGestures.Add(new KeyGesture(Key.R, ModifierKeys.Control));
            BrkPntShortcut.InputGestures.Add(new KeyGesture(Key.B, ModifierKeys.Control));
            TraceShortcut.InputGestures.Add(new KeyGesture(Key.T, ModifierKeys.Control));

            // The methods to execute when each key shortcut is made
            CommandBindings.Add(new CommandBinding(LoadShortcut, btnLoad_OnClicked));
            CommandBindings.Add(new CommandBinding(RunShortcut, btnRun_OnClicked));
            CommandBindings.Add(new CommandBinding(StepShortcut, btnStep_OnClicked));
            CommandBindings.Add(new CommandBinding(StopShortcut, btnStop_OnClicked));
            CommandBindings.Add(new CommandBinding(ResetShortcut, btnReset_OnClicked));
            CommandBindings.Add(new CommandBinding(BrkPntShortcut, btnAddBreak_OnClicked));
            CommandBindings.Add(new CommandBinding(TraceShortcut, shortcutTrace));
        }

        /// <summary>
        /// Display a mesage to GUI if command line parsing failure
        /// </summary>
        /// <param name="errors">the errors provided by the command line parser</param>
        private void HandleError(IEnumerable<Error> errors)
        {
            statusError.Text = "Error: Illegal command line syntax. usage: armsim [ --mem memory-size ] [ --exec ] [ elf-file ]";
        }

        /// <summary>
        /// Run the program normally if command line arguments
        /// properly parsed.
        /// </summary>
        /// <param name="options">the command line parser object</param>
        private void Run(Options options)
        {
            memSize = options.MemSize;
            isExec = options.Exec;

            // If a filename provided, load it
            if (options.FileName != null)
            {
                fileName = options.FileName;
                LoadGUI();
            }
            else  // otherwise, disable all buttons except for load and do nothing
            {
                btnRun.IsEnabled = false;
                btnStop.IsEnabled = false;
                btnStep.IsEnabled = false;
                btnReset.IsEnabled = false;
                btnAddBreak.IsEnabled = false;
                memEntry.IsReadOnly = true;
                statusFile.Text = "File: None\n";
                statusChecksum.Text = "Checksum: N/A";
            }
        }

        /// <summary>
        /// Starts the process of simulation.
        /// Called from Run() and LoadFile().
        /// </summary>
        private void LoadGUI()
        {
            int returnCode = InitComputer();
            InitGUI(returnCode);
        }

        /// <summary>
        /// Called from LoadGUI and initializes the simulator by:
        ///     Initializing the instance of the Computer class
        ///     Initializing the instances for RAM and registers of the Memory class
        ///     Enabling the appropriate buttons
        /// </summary>
        /// <returns>return code of initializing the Computer class,
        ///          specifically from the loader setup</returns>
        private int InitComputer()
        {
            sim = new Computer(memSize, fileName);
            int returnCode = sim.GetRetCode();
            ram = sim.ram;
            regs = sim.registers;
            sim.RunCompleted += sim_RunCompleted;
            sim.processor.ExecuteCompleted += sim_ExecuteCompleted;
            sim.processor.PutChar += sim_PutChar;
            sim.processor.ReadLine += sim_ReadLine;
            btnRun.IsEnabled = true;
            btnStop.IsEnabled = false;
            btnStep.IsEnabled = true;
            btnReset.IsEnabled = true;
            btnAddBreak.IsEnabled = true;
            memEntry.IsReadOnly = false;
            traceCount = 1;
            return returnCode;
        }

        /// <summary>
        /// Called from LoadGUI after InitComputer.
        /// Gets the checksum and initializes all the
        /// panes of the GUI that display data.
        /// Displays an error if one occured.
        /// </summary>
        /// <param name="returnCode">the retCode returned from InitComputer</param>
        private void InitGUI(int returnCode)
        {
            // Determine which action to take depending on returnCode
            if (returnCode == 0)
            {
                statusFile.Text = "File: " + fileName + "\n";
                statusChecksum.Text = "Checksum: " + sim.GetChecksum().ToString();
                InitMemory();
                InitRegs();
                InitFlags();
                InitStack();
                InitDisassembly();
                consoleView.Clear();

                // If the --exec option was provided on the command line,
                // start running the program automatically.
                if (isExec && !loadedFile)
                {
                    btnRun.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            }
            else
            {
                statusFile.Text = "File: None" + "\n";
                statusChecksum.Text = "Checksum: N/A" + "\n";
                switch (returnCode)
                {
                    case 1:
                        statusError.Text = "Error: The file specified is not an executable file.";
                        break;
                    case 2:
                        statusError.Text = "Error: Simulated RAM size must be between 0 and 1,000,000 bytes.";
                        break;
                    case 3:
                        statusError.Text = "Error: The file specified does not exist.";
                        break;
                    case 4:
                        statusError.Text = "Error: The file specified was not in the ELF format.";
                        break;
                    case 5:
                        statusError.Text = "Error: The file was not able to be accessed because another process was using it.";
                        break;
                    case 6:
                        statusError.Text = "Error: The file was not a 32-bit format.";
                        break;
                    case 7:
                        statusError.Text = "Error: There was not enough memory provided.";
                        break;
                    default:
                        statusError.Text = "Error: An unexpected error occurred.";
                        break;
                }
            }
        }

        /// <summary>
        /// Only called when Load button is clicked in GUI.
        /// Calls LoadFileDialog() to get filename of new file to load.
        /// </summary>
        private void LoadFile()
        {
            string tempName = LoadFileDialog();
            int lastSlash = tempName.LastIndexOf("\\");
            fileName = tempName.Substring(lastSlash + 1);

            // Don't go through the process of initializing a new
            // simulation if the user didn't pick a file from the file picker
            if (fileName == "")
            {
                return;
            }

            // Clear the breaklist
            breakList.Clear();
            if ((bool)traceToggle.IsChecked)
            {
                traceToggle.IsChecked = false;
                traceToggle.IsChecked = true;
            }
            loadedFile = true;
            LoadGUI();
        }

        /// <summary>
        /// Opens a native file picker dialog
        /// </summary>
        /// <returns>the filename of the file the user chose</returns>
        private string LoadFileDialog()
        {
            string fileName;

            // Logic for opening file dialog found at https://www.wpf-tutorial.com/dialogs/the-openfiledialog/
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
            openFileDialog.ShowDialog();
            fileName = openFileDialog.FileName;
            return fileName;
        }

        /// <summary>
        /// Creates the memory dump in the Memory ListView in the GUI
        /// </summary>
        private void InitMemory()
        {
            memDisplayList.Clear();

            int numRows = memSize / 16;
            uint memByte = 0;
            string rowString;
            string asciiString;
            string tempByte;
            byte tempByteNum;

            for (int i = 0; i < numRows; ++i)
            {
                rowString = "";
                asciiString = "";
                for (int j = 0; j < 16; ++j)
                {
                    tempByte = string.Format("{0:X}", ram.ReadByte(memByte));

                    tempByteNum = ram.ReadByte(memByte);

                    if ((tempByteNum >= 33) && (tempByteNum <= 122))
                    {
                        asciiString += Convert.ToChar(tempByteNum);
                    }
                    else
                    {
                        asciiString += ".";
                    }

                    if (tempByte.Length == 1)
                    {
                        tempByte = "0" + tempByte;
                    }

                    if ((j + 1) % 2 == 0)
                    {
                        tempByte += " ";
                    }

                    rowString += tempByte;
                    memByte += 1;
                }

                memDisplayList.Add(new MemRow()
                {
                    MemoryAddr = string.Format("{0:X8}:", i * 16),
                    MemoryRow = rowString,
                    MemoryRep = asciiString
                });

                memory.ItemsSource = memDisplayList;
                memory.Items.Refresh();
            }
        }

        /// <summary>
        /// Creates the register display in the Registers ListView in the GUI
        /// </summary>
        private void InitRegs()
        {
            regsList.Clear();

            for (int i = 0; i < 15; ++i)
            {
                if (i < 10)
                {
                    regsList.Add(new Register()
                    {
                        RegName = "r" + i + "  =",
                        RegValue = sim.registers.ReadWord((uint)(i * 4)).ToString("X8")
                    });
                }
                else
                {
                    regsList.Add(new Register()
                    {
                        RegName = "r" + i + " =",
                        RegValue = sim.registers.ReadWord((uint)(i * 4)).ToString("X8")
                    });
                }
            }

            regsList.Add(new Register()
            {
                RegName = "r15 =",
                RegValue = sim.GetPC().ToString("X8")
            });

            registersView.ItemsSource = regsList;
            registersView.Items.Refresh();
        }

        /// <summary>
        /// Creates the flag display in the Flags ListView in the GUI
        /// </summary>
        private void InitFlags()
        {
            flagsList.Clear();

            uint cpsrReg = 16 * 4;

            flagsList.Add(new Flag() { FlagName = "N =", FlagValue = sim.registers.TestFlag(cpsrReg, 31) ? 1 : 0 });
            flagsList.Add(new Flag() { FlagName = "Z =", FlagValue = sim.registers.TestFlag(cpsrReg, 30) ? 1 : 0 });
            flagsList.Add(new Flag() { FlagName = "C =", FlagValue = sim.registers.TestFlag(cpsrReg, 29) ? 1 : 0 });
            flagsList.Add(new Flag() { FlagName = "V =", FlagValue = sim.registers.TestFlag(cpsrReg, 28) ? 1 : 0 });
            flagsList.Add(new Flag() { FlagName = "I =", FlagValue = sim.registers.TestFlag(cpsrReg, 7) ? 1 : 0 });

            flagsView.ItemsSource = flagsList;
            flagsView.Items.Refresh();
        }

        /// <summary>
        /// Creates the stack display in the Stack ListView in the GUI
        /// </summary>
        private void InitStack()
        {
            stackList.Clear();

            for (uint i = 0, counter = 0; i < 10; ++i, counter += 4)
            {
                stackList.Add(new StackRow()
                {
                    StackAddr = string.Format("{0:X8}:", sim.GetSP() - counter),
                    StackValue = sim.ram.ReadWord(sim.GetSP() - counter).ToString("X8")
                });
            }

            stackView.ItemsSource = stackList;
            stackView.Items.Refresh();
        }

        /// <summary>
        /// Creates the disassembly display in the Disassembly ListView in the GUI
        /// </summary>
        private void InitDisassembly()
        {
            disList.Clear();

            decodedInstructions = new List<Instruction>();
            Instruction tempInstr;
            uint counter = sim.textStart;
            uint origPC = sim.load.GetElfEntry();
            uint tempPC = sim.textStart;

            while (true)
            {
                tempPC += 4;
                sim.registers.WriteWord(15 * 4, tempPC);
                tempInstr = sim.processor.decode(sim.ram.ReadWord(counter));

                if (tempInstr.instr == 0)
                {
                    break;
                }

                decodedInstructions.Add(tempInstr);

                disList.Add(new DisRow()
                {
                    DisAddr = string.Format("{0:X8}:", counter),
                    DisInstr = sim.ram.ReadWord(counter).ToString("X8"),
                    DisRep = tempInstr.text
                });

                counter += 4;
            }

            sim.registers.WriteWord(15 * 4, origPC);

            disassemblyView.ItemsSource = disList;
            disassemblyView.Items.Refresh();
        }

        /// <summary>
        /// Updates the GUI from the background thread.
        /// Called at the end of execution.
        /// </summary>
        private void UpdateGUI()
        {
            // found at https://stackoverflow.com/questions/9732709/the-calling-thread-cannot-access-this-object-because-a-different-thread-owns-it
            Dispatcher.Invoke((Action)(() =>
            {
                btnRun.IsEnabled = true;
                btnStop.IsEnabled = false;
                UpdateRegs();
                UpdateStack();
                UpdateFlags();
                UpdateDisassembly();
            }));
        }

        /// <summary>
        /// Updates the Register ListView in the GUI
        /// </summary>
        private void UpdateRegs()
        {
            for (int i = 0; i < 16; ++i)
            {
                regsList[i].RegValue = regs.ReadWord((uint)(i * 4)).ToString("X8");
            }

            registersView.Items.Refresh();
        }

        /// <summary>
        /// Updates the Stack ListView in the GUI
        /// </summary>
        private void UpdateStack()
        {
            for (uint i = 0, counter = 0; i < stackList.Count; ++i, counter += 4)
            {
                stackList[(int)i].StackAddr = string.Format("{0:X8}:", 0x7000 - counter);
                stackList[(int)i].StackValue = sim.ram.ReadWord(0x7000 - counter).ToString("X8");
            }

            stackView.Items.Refresh();
        }

        /// <summary>
        /// Updates the Flags ListView in the GUI
        /// </summary>
        private void UpdateFlags()
        {
            uint cpsrReg = 16 * 4;
            int flagBit = 31;

            for (int i = 0; i < flagsList.Count; ++i, --flagBit)
            {
                flagsList[i].FlagValue = sim.registers.TestFlag(cpsrReg, flagBit) ? 1 : 0;
            }

            flagsView.Items.Refresh();
        }

        /// <summary>
        /// Updates the Disassembly ListView in the GUI
        /// </summary>
        private void UpdateDisassembly()
        {
            disassemblyView.SelectedIndex = 0;

            for (int i = 0; i < disList.Count; ++i)
            {
                if (disList[i].DisAddr == string.Format("{0:X8}:", sim.registers.ReadWord(15 * 4)))
                {
                    disassemblyView.SelectedIndex = i;
                    break;
                }
            }

            disassemblyView.SelectedItem = disassemblyView.Items.GetItemAt(disassemblyView.SelectedIndex);
            disassemblyView.ScrollIntoView(disassemblyView.SelectedItem);
        }

        /// <summary>
        /// Event handler for when the Run button is clicked.
        /// Will update the GUI when the new thread completes execution
        /// and calls sim_RunCompleted().
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRun_OnClicked(object sender, RoutedEventArgs e)
        {
            btnRun.IsEnabled = false;
            btnStop.IsEnabled = true;
            sim.processor.running = true;
            Thread runThread = new Thread(sim.run);
            runThread.Start();
        }

        /// <summary>
        /// Event handler for when the Stop button is clicked.
        /// Updates the GUI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStop_OnClicked(object sender, RoutedEventArgs e)
        {
            sim.processor.running = false;
            UpdateGUI();
        }

        /// <summary>
        /// Event handler for when the Step button is clicked.
        /// Will update the GUI when the new threads completes execution
        /// and calls sim_RunCompleted().
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStep_OnClicked(object sender, RoutedEventArgs e)
        {
            sim.processor.running = true;
            Thread stepThread = new Thread(sim.step);
            stepThread.Start();
        }

        /// <summary>
        /// Event handler for when the Break button is clicked.
        /// Opens a dialog box for breakpoint interaction.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddBreak_OnClicked(object sender, RoutedEventArgs e)
        {
            BreakpointDialog bpoint = new BreakpointDialog();
            bpoint.ShowDialog();
        }

        /// <summary>
        /// Event handler for when the Reset button is clicked.
        /// Recreates the simulation, but only updates the GUI instead
        /// of re-initializing it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReset_OnClicked(object sender, RoutedEventArgs e)
        {
            InitComputer();
            UpdateGUI();
            consoleView.Clear();
            if ((bool)traceToggle.IsChecked)
            {
                traceToggle.IsChecked = false;
                traceToggle.IsChecked = true;
            }
        }

        /// <summary>
        /// Event handler for when the Load button is clicked.
        /// Opens the native file picker.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLoad_OnClicked(object sender, RoutedEventArgs e)
        {
            LoadFile();
        }

        /// <summary>
        /// Event handler for when the user enters text into the memEntry TextBox.
        /// Scrolls the Row of memory specified into view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void memEntry_KeyPressed(object sender, KeyEventArgs e)
        {
            int entry = Int32.MaxValue;
            
            // Scroll the Row of memory specified into view when the user presses enter
            if (e.Key == Key.Return)
            {
                // Try converting input to Int32
                try
                {
                    entry = Convert.ToInt32(memEntry.Text, 16);
                }
                catch (Exception) { }

                // Ensure the entry is within bounds
                if ((entry < 0) || (entry > memSize))
                {
                    memEntryBoundsError.Visibility = Visibility.Visible;
                    return;
                }
                else
                {
                    memEntryBoundsError.Visibility = Visibility.Hidden;
                }

                // row-focus logic found at https://stackoverflow.com/questions/211971/scroll-wpf-listview-to-specific-line
                memory.SelectedItem = memory.Items.GetItemAt(entry / 16);
                memory.ScrollIntoView(memory.SelectedItem);
                ListViewItem item = memory.ItemContainerGenerator.ContainerFromItem(memory.SelectedItem) as ListViewItem;
                item.Focus();
            }
        }

        private void shortcutTrace(object sender, RoutedEventArgs e)
        {
            if ((bool)traceToggle.IsChecked)
            {
                traceToggle.IsChecked = false;
            }
            else
            {
                traceToggle.IsChecked = true;
            }
        }

        private void traceToggle_Checked(object sender, RoutedEventArgs e)
        {
            traceOn = true;
            traceCount = 1;
            stream = new FileStream("trace.log", FileMode.Create);
            writer = new StreamWriter(stream);
            writer.AutoFlush = true;
        }

        private void traceToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            traceOn = false;
            writer.Close();
            stream.Close();
        }

        /// <summary>
        /// Row object for Register ListView in GUI
        /// </summary>
        public class Register
        {
            public string RegName { get; set; }
            public string RegValue { get; set; }
        }

        /// <summary>
        /// Row object for Flag ListView in GUI
        /// </summary>
        public class Flag
        {
            public string FlagName { get; set; }
            public int FlagValue { get; set; }
        }

        /// <summary>
        /// Row object for Stack ListView in GUI
        /// </summary>
        public class StackRow
        {
            public string StackAddr { get; set; }
            public string StackValue { get; set; }
        }

        /// <summary>
        /// Row object for Memory ListView in GUI
        /// </summary>
        public class MemRow
        {
            public string MemoryAddr { get; set; }
            public string MemoryRow { get; set; }
            public string MemoryRep { get; set; }
        }

        /// <summary>
        /// Row object for Disassembly ListView in GUI
        /// </summary>
        public class DisRow
        {
            public string DisAddr { get; set; }
            public string DisInstr { get; set; }
            public string DisRep { get; set; }
        }

        /// <summary>
        /// Row object for Breakpoints ListView in GUI
        /// </summary>
        public class breakRow
        {
            public string breakAddr { get; set; }
            public string breakInstr { get; set; }
            public string breakRep { get; set; }
        }

        /// <summary>
        /// Event handler for when a process has been completed by the simulator
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sim_RunCompleted(object sender, EventArgs e)
        {
            UpdateGUI();
            
            // If the --exec option was provided,
            // shut down the application when finished running.
            if (isExec && !loadedFile)
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    Close();
                }));
            }
        }

        /// <summary>
        /// Event handler for when an execute has been completed by the simulator
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sim_ExecuteCompleted(object sender, EventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                if ((bool)traceToggle.IsChecked)
                {
                    writer.WriteLine(sim.processor.traceLine);
                    traceCount++;
                }
            }));
        }

        /// <summary>
        /// Event handler for outputting text to the console
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sim_PutChar(object sender, EventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                consoleView.Text += Convert.ToChar(sim.registers.ReadWord(0));
                sim.processor.console += Convert.ToChar(sim.registers.ReadWord(0));
            }));
        }

        /// <summary>
        /// Event handler for when input is requested
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sim_ReadLine(object sender, EventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                InputDialog input = new InputDialog();
                input.ShowDialog();
            }));
        }
    }
}
