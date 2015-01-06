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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

namespace IntraClip
{
    public partial class MainForm : Form
    {
        private ushort hotKeyId;

        private bool canShow = false;

        private decimal audioCount = 0;

        private decimal imageCount = 0;

        private decimal imageTotal = 0;

        private PreferencesForm myPreferencesForm = null;

        public MainForm()
        {
            InitializeComponent();

            Properties.Settings.Default.Number = 20;

            try
            {
                Stream myFile = File.Create("LogFile.txt");
                TextWriterTraceListener myTextListener = new TextWriterTraceListener(myFile);
                Trace.Listeners.Add(myTextListener);
                Trace.AutoFlush = true;
            }
            catch { }

            try
            {
                ClipboardPlus.AddClipboardFormatListener(this.Handle);

                string atomName = Thread.CurrentThread.ManagedThreadId.ToString("X8") + this.GetType().FullName;
                hotKeyId = HotKeyPlus.GlobalAddAtom(atomName);
                HotKeyPlus.RegisterHotKey(this.Handle, hotKeyId, HotKeyPlus.MOD_CONTROL | HotKeyPlus.MOD_SHIFT, (uint)Keys.C);
            }
            catch (Exception myException)
            {
                Trace.WriteLine(Utils.FormatLog(myException.ToString()));
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case (ClipboardPlus.WM_CLIPBOARDUPDATE):
                    Dispatch();
                    break;
                case (HotKeyPlus.WM_HOTKEY):
                    if ((ushort)m.WParam == hotKeyId)
                        InvokeStrip();
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        protected override void SetVisibleCore(bool value)
        {
            if (canShow)
                base.SetVisibleCore(value);
            else
                base.SetVisibleCore(false);
        }

        private void Dispatch()
        {
            try
            {
                if (Clipboard.ContainsText())
                    AddTextItem();
                else if (Clipboard.ContainsImage())
                    AddImageItem();
                else if (Clipboard.ContainsFileDropList())
                    AddFileDropListItem();
                else if (Clipboard.ContainsAudio())
                    AddAudioItem();
                else
                    UncheckLastItem();
            }
            catch (Exception myException)
            {
                Trace.WriteLine(Utils.FormatLog(myException.ToString()));
            }
        }

        private void InvokeStrip()
        {
            mainNotifyIcon.ContextMenuStrip = leftContextMenuStrip;
            MethodInfo myMethodInfo = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
            myMethodInfo.Invoke(mainNotifyIcon, null);
            mainNotifyIcon.ContextMenuStrip = rightContextMenuStrip;
        }

        private void PrepareStrip()
        {
            if (!leftContextMenuStrip.Items[2].Enabled)
                leftContextMenuStrip.Items[2].Dispose();
            else
            {
                while (leftContextMenuStrip.Items.Count >= 2 + Properties.Settings.Default.Number)
                {
                    if (((ItemTag)leftContextMenuStrip.Items[2].Tag).Type == DataFormats.Bitmap)
                        imageTotal--;
                    leftContextMenuStrip.Items[2].Dispose();
                }
                if (imageTotal == 0)
                    leftContextMenuStrip.ShowImageMargin = false;
                UncheckLastItem();
            }
        }

        private void UncheckLastItem()
        {
            ((ToolStripMenuItem)leftContextMenuStrip.Items[leftContextMenuStrip.Items.Count - 1]).Checked = false;
        }

        private void AddAudioItem()
        {
            PrepareStrip();

            audioCount++;
            decimal audioIndex = audioCount;

            int i = 2;
            bool br = false;
            while ((i < leftContextMenuStrip.Items.Count) && (!br))
            {
                if (((ItemTag)leftContextMenuStrip.Items[i].Tag).Type.Equals(DataFormats.WaveAudio))
                    if (Utils.AreEqual(Clipboard.GetAudioStream(), (Stream)((ItemTag)leftContextMenuStrip.Items[i].Tag).Data))
                    {
                        audioCount--;
                        audioIndex = decimal.Parse(leftContextMenuStrip.Items[i].Text.Substring(7, leftContextMenuStrip.Items[i].Text.Length - 7 - 1));

                        leftContextMenuStrip.Items[i].Dispose();
                        br = true;
                    }
                i++;
            }

            ResourceManager resources = new ResourceManager(typeof(MainForm));
            ToolStripMenuItem menuItem = new ToolStripMenuItem(resources.GetString("audio") + " " + audioIndex.ToString());
            resources.ReleaseAllResources();
            menuItem.Checked = true;
            menuItem.Font = new Font(menuItem.Font, FontStyle.Italic);
            menuItem.Tag = new ItemTag(DataFormats.WaveAudio, Clipboard.GetAudioStream());
            menuItem.Click += new EventHandler(toolStripMenuItem_Click);
            leftContextMenuStrip.Items.Add(menuItem);
        }

        private void AddFileDropListItem()
        {
            PrepareStrip();

            int i = 2;
            bool br = false;
            while ((i < leftContextMenuStrip.Items.Count) && (!br))
            {
                if (((ItemTag)leftContextMenuStrip.Items[i].Tag).Type.Equals(DataFormats.FileDrop))
                    if (Utils.AreEqual(Clipboard.GetFileDropList(), (StringCollection)((ItemTag)leftContextMenuStrip.Items[i].Tag).Data))
                    {
                        leftContextMenuStrip.Items[i].Dispose();
                        br = true;
                    }
                i++;
            }

            ToolStripMenuItem menuItem = new ToolStripMenuItem(Utils.FormatItem(Clipboard.GetFileDropList()));
            menuItem.Checked = true;
            menuItem.Font = new Font(menuItem.Font, FontStyle.Italic);
            menuItem.Tag = new ItemTag(DataFormats.FileDrop, Clipboard.GetFileDropList());
            string itemToolTipText = "";
            for (int j = 0; j < Clipboard.GetFileDropList().Count; j++)
                itemToolTipText += Clipboard.GetFileDropList()[j] + Environment.NewLine;
            menuItem.ToolTipText = itemToolTipText;
            menuItem.Click += new EventHandler(toolStripMenuItem_Click);
            leftContextMenuStrip.Items.Add(menuItem);
        }

        private void AddImageItem()
        {
            PrepareStrip();

            imageCount++;
            imageTotal++;
            decimal imageIndex = imageCount;

            int i = 2;
            bool br = false;
            while ((i < leftContextMenuStrip.Items.Count) && (!br))
            {
                if (((ItemTag)leftContextMenuStrip.Items[i].Tag).Type.Equals(DataFormats.Bitmap))
                    if (Utils.AreEqual(Clipboard.GetImage(), (Image)((ItemTag)leftContextMenuStrip.Items[i].Tag).Data))
                    {
                        imageCount--;
                        imageTotal--;
                        imageIndex = decimal.Parse(leftContextMenuStrip.Items[i].Text.Substring(7, leftContextMenuStrip.Items[i].Text.Length - 7 - 1));

                        leftContextMenuStrip.Items[i].Dispose();
                        br = true;
                    }
                i++;
            }

            ResourceManager resources = new ResourceManager(typeof(MainForm));
            ToolStripMenuItem menuItem = new ToolStripMenuItem(resources.GetString("image") + " " + imageIndex.ToString() + " (" + Clipboard.GetImage().Width + " x " + Clipboard.GetImage().Height + ")");
            resources.ReleaseAllResources();
            menuItem.Checked = true;
            menuItem.Font = new Font(menuItem.Font, FontStyle.Italic);
            menuItem.Tag = new ItemTag(DataFormats.Bitmap, Clipboard.GetImage());
            menuItem.Image = Clipboard.GetImage();
            leftContextMenuStrip.ShowImageMargin = true;
            menuItem.Click += new EventHandler(toolStripMenuItem_Click);
            leftContextMenuStrip.Items.Add(menuItem);
        }

        private void AddTextItem()
        {
            PrepareStrip();

            int i = 2;
            bool br = false;
            while ((i < leftContextMenuStrip.Items.Count) && (!br))
            {
                if (((ItemTag)leftContextMenuStrip.Items[i].Tag).Type.Equals(DataFormats.Text))
                    if (Clipboard.GetText().Equals(((ItemTag)leftContextMenuStrip.Items[i].Tag).Data))
                    {
                        leftContextMenuStrip.Items[i].Dispose();
                        br = true;
                    }
                i++;
            }

            ToolStripMenuItem menuItem = new ToolStripMenuItem(Utils.FormatItem(Clipboard.GetText()));
            menuItem.Checked = true;
            menuItem.Tag = new ItemTag(DataFormats.Text, Clipboard.GetText());
            //menuItem.ToolTipText = Clipboard.GetText();
            menuItem.Click += new EventHandler(toolStripMenuItem_Click);
            leftContextMenuStrip.Items.Add(menuItem);
        }

        private void mainNotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                InvokeStrip();
        }

        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (myPreferencesForm == null)
            {
                myPreferencesForm = new PreferencesForm();
                myPreferencesForm.FormClosed += PreferencesForm_FormClosed;
                myPreferencesForm.Show();
            }
            else
                myPreferencesForm.Focus();
        }

        private void PreferencesForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            while (leftContextMenuStrip.Items.Count > 2 + Properties.Settings.Default.Number)
                leftContextMenuStrip.Items[2].Dispose();
            myPreferencesForm = null;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            canShow = true;
            this.Visible = true;
            this.Focus();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                while (leftContextMenuStrip.Items.Count > 2)
                    leftContextMenuStrip.Items[2].Dispose();
                leftContextMenuStrip.ShowImageMargin = false;
                ToolStripMenuItem menuItem = new ToolStripMenuItem(emptyHistoryToolStripMenuItem.Text);
                leftContextMenuStrip.Items.Add(menuItem);
                leftContextMenuStrip.Items[2].Enabled = false;
                audioCount = 0;
                imageCount = 0;
                imageTotal = 0;
                ClipboardPlus.Clear();
            }
            catch (Exception myException)
            {
                Trace.WriteLine(Utils.FormatLog(myException.ToString()));
            }
        }

        private void toolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
                if (((ItemTag)menuItem.Tag).Type == DataFormats.WaveAudio)
                    Clipboard.SetAudio((Stream)((ItemTag)menuItem.Tag).Data);
                else if (((ItemTag)menuItem.Tag).Type == DataFormats.FileDrop)
                    Clipboard.SetFileDropList((StringCollection)((ItemTag)menuItem.Tag).Data);
                else if (((ItemTag)menuItem.Tag).Type == DataFormats.Bitmap)
                    Clipboard.SetImage((Image)((ItemTag)menuItem.Tag).Data);
                else if (((ItemTag)menuItem.Tag).Type == DataFormats.Text)
                    Clipboard.SetText((string)((ItemTag)menuItem.Tag).Data);
            }
            catch (Exception myException)
            {
                Trace.WriteLine(Utils.FormatLog(myException.ToString()));
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            versionLabel.Text += " " + Application.ProductVersion;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                canShow = false;
                this.Visible = false;
                e.Cancel = true;
            }
        }

        private void siteLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/manuel-calzolari/intraclip");
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
