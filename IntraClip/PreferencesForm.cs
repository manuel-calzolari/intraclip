/*
 * IntraClip - A simple clipboard manager for Windows
 * Copyright (C) 2011  Manuel Calzolari
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, version 3 of the License.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace IntraClip
{
    public partial class PreferencesForm : Form
    {
        public PreferencesForm()
        {
            InitializeComponent();
        }

        private void PreferencesForm_Load(object sender, EventArgs e)
        {
            this.numberNumericUpDown.Value = Properties.Settings.Default.Number;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            try
            {
                Properties.Settings.Default.Number = this.numberNumericUpDown.Value;
                Properties.Settings.Default.Save();
                this.Close();
            }
            catch (Exception myException)
            {
                Trace.WriteLine(Utils.FormatLog(myException.ToString()));
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
