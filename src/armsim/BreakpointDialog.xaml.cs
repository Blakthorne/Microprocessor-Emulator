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
    /// Interaction logic for BreakpointDialog.xaml
    /// </summary>
    public partial class BreakpointDialog : Window
    {
        /// <summary>
        /// Initializes the Breakpoint dialog box and sets
        /// the ListView of the breakpoints to the list
        /// in the MainWindow class.
        /// </summary>
        public BreakpointDialog()
        {
            InitializeComponent();

            breakView.ItemsSource = MainWindow.breakList;
        }

        /// <summary>
        /// Event handler for when the Add button is clicked.
        /// Performs error checking and adds it to the list of
        /// breakpoints in the MainWindow class if all checks pass.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            uint entry = UInt32.MaxValue;

            // Try converting input to Int32
            try
            {
                entry = Convert.ToUInt32(addTextBox.Text, 16);
            }
            catch (Exception) { }

            // Perform error checking
            if ((entry < 0) || (entry > MainWindow.memSize))
            {
                brkEntryMulError.Visibility = Visibility.Hidden;
                brkEntryBoundsError.Visibility = Visibility.Visible;
                return;
            }
            else if (entry % 4 != 0)
            {
                brkEntryBoundsError.Visibility = Visibility.Hidden;
                brkEntryMulError.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                brkEntryBoundsError.Visibility = Visibility.Hidden;
                brkEntryMulError.Visibility = Visibility.Hidden;
            }

            // Make a new row with the user specified data
            MainWindow.breakRow newRow = new MainWindow.breakRow()
            {
                breakAddr = string.Format("{0:X8}:", entry),
                breakInstr = MainWindow.sim.ram.ReadWord(entry).ToString("X8"),
                breakRep = "mov r2 r3"
            };
            
            // Insert new row into ListView if not a duplicate
            foreach (MainWindow.breakRow row in MainWindow.breakList)
            {
                if (newRow.breakAddr == row.breakAddr)
                {
                    return;
                }
            }

            addTextBox.Clear();
            addTextBox.Focus();
            MainWindow.breakList.Add(newRow);

            breakView.Items.Refresh();
        }

        /// <summary>
        /// Event handler for when the Remove button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            // Remove the selected row at the specified index
            if ((breakView.SelectedIndex >= 0) && (breakView.SelectedIndex <= MainWindow.breakList.Count))
            {
                MainWindow.breakList.RemoveAt(breakView.SelectedIndex);
                breakView.Items.Refresh();
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// Event handler for when the OK button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
