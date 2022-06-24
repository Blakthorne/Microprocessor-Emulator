using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace armsim
{
    /// <summary>
    /// Interaction logic for InputDialog.xaml
    /// </summary>
    public partial class InputDialog : Window
    {
        public InputDialog()
        {
            InitializeComponent();

            for (int i = MainWindow.sim.processor.consolePtr; i < MainWindow.sim.processor.console.Length; ++i)
            {
                inputText.Text += MainWindow.sim.processor.console[i];
                MainWindow.sim.processor.consolePtr++;
            }
        }

        /// <summary>
        /// Event handler for when the OK button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            RetrieveInput();
            Close();
        }

        private void inputText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                RetrieveInput();
                Close();
            }
        }

        private void RetrieveInput()
        {
            int inputLen = inputText.Text.Length - MainWindow.sim.processor.consolePtr;
            string input = inputText.Text.Substring(MainWindow.sim.processor.consolePtr, inputLen);
            int maxLen = (int)MainWindow.sim.processor.registers.ReadWord(2 * 4);
            uint dest = MainWindow.sim.processor.registers.ReadWord(1 * 4);

            if (maxLen <= inputLen)
            {
                input.Remove(maxLen - 1);
                input += '\0';
            }
            else if (maxLen == (inputLen + 1))
            {
                input += '\0';
            }
            else
            {
                input += '\r' + '\0';
            }

            for (uint i = 0; i < input.Length; ++i)
            {
                MainWindow.sim.processor.ram.WriteByte(dest + i, (byte)input[(int)i]);
                MainWindow.sim.processor.console += input[(int)i];
                MainWindow.sim.processor.consolePtr++;
            }
        }
    }
}
