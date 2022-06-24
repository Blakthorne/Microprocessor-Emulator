using System;
using System.Collections.Generic;
using System.Text;

namespace armsim.Execute
{
    class ExecSwi : Execute
    {
        public event EventHandler PutChar; // delegate for handling the OnPutChar event

        public static void ExecuteSwi()
        {
            switch(instr.offset_swi)
            {
                case 0x0:
                    break;
                case 0x11:
                    Computer.finished = true;
                    break;
                case 0x06a:
                    break;
                default:    // all other swi numbers are treated as no-ops
                    break;
            }
        }

        public void Put()
        {
            OnPutChar(EventArgs.Empty);
        }

        /// <summary>
        /// Event handler method
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnPutChar(EventArgs e)
        {
            PutChar?.Invoke(this, e);
        }
    }
}
