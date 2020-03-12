/***************************************************************
* Copyright (C) 2011 Jeremy Reagan, All Rights Reserved.
* I may be reached via email at: jeremy.reagan@live.com
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of the GNU General Public License
* as published by the Free Software Foundation; under version 2
* of the License.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
****************************************************************/

#region

using FTPLib;
using HL7Analyst.Properties;
using HL7Lib.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Component = HL7Lib.Base.Component;
using Message = HL7Lib.Base.Message;
using WxUtilities.Extensions;

#endregion

namespace HL7_Analyst
{
    /// <summary>
    /// Main application form, displays HL7 messages in an understandable format and allows for analysis work to be performed on displayed messages.
    /// </summary>
    public partial class frmMain : Form
    {
        private readonly List<string> _ftpFiles = new List<string>();
        private readonly SynchronizationContext _synchronizationContext;
        private List<string> _allMessages = new List<string>();
        private int _currentMessage;
        private List<string> _extensions = new List<string>();
        private DatabaseOptions _loadedDbOptions = new DatabaseOptions();
        private FTPOptions _loadedFtpOptions = new FTPOptions();
        private TCPIPOptions _loadedTcpipOptions = new TCPIPOptions();
        private List<string> _messages = new List<string>();
        private bool _runTcpipServerLoop = true;

        /// <summary>
        /// Initialization Method
        /// </summary>
        public frmMain()
        {
            InitializeComponent();
            _synchronizationContext = SynchronizationContext.Current;
        }

        private delegate void SegmentDisplayClearItemsDelegate();

        private delegate void SegmentChangerClearItemsDelegate();

        private delegate void MessageTotalSetTextDelegate(string s);

        private delegate void MessageBoxSetTextDelegate(string s);

        private delegate void CurrentMessageSetTextDelegate(string s);

        private delegate void FormSetTextDelegate(string s);

        private delegate void SegmentChangerAddItemDelegate(object item);

        private delegate void SegmentDisplayAddItemDelegate(ListViewItem item);

        private delegate void FormSetCursorDelegate(Cursor c);

        private delegate void TCPIPTransferDisplayAddRowDelegate(List<object> items);

        private delegate void MessageBoxSetFontFormatDelegate();

        #region Cross Thread Invoke Methods

        /// <summary>
        /// Clears the segment display
        /// </summary>
        private void SegmentDisplayClearItems()
        {
            try
            {
                if (lvSegmentDisplay.IsHandleCreated)
                    if (lvSegmentDisplay.InvokeRequired)
                        lvSegmentDisplay.Invoke(new SegmentDisplayClearItemsDelegate(SegmentDisplayClearItems));
                    else
                        lvSegmentDisplay.Items.Clear();
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        /// <summary>
        /// Clears the segment changer
        /// </summary>
        private void SegmentChangerClearItems()
        {
            try
            {
                if (tsSegmentToolbar.IsHandleCreated)
                    if (tsSegmentToolbar.InvokeRequired)
                        tsSegmentToolbar.Invoke(new SegmentChangerClearItemsDelegate(SegmentChangerClearItems));
                    else
                        cbSegmentChanger.Items.Clear();
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        /// <summary>
        /// Sets the Message Total text
        /// </summary>
        /// <param name="s">The string to use</param>
        private void MessageTotalSetText(string s)
        {
            try
            {
                if (tsSegmentToolbar.IsHandleCreated)
                    if (tsSegmentToolbar.InvokeRequired)
                        tsSegmentToolbar.Invoke(new MessageTotalSetTextDelegate(MessageTotalSetText), s);
                    else
                        txtMessageTotal.Text = s;
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        /// <summary>
        /// Sets the message box text
        /// </summary>
        /// <param name="s">The string to use</param>
        private void MessageBoxSetText(string s)
        {
            try
            {
                if (rtbMessageBox.IsHandleCreated)
                    if (rtbMessageBox.InvokeRequired)
                        rtbMessageBox.Invoke(new MessageBoxSetTextDelegate(MessageBoxSetText), s);
                    else
                        rtbMessageBox.Text = s;
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        /// <summary>
        /// Sets the Current Message text
        /// </summary>
        /// <param name="s">The string to use</param>
        private void CurrentMessageSetText(string s)
        {
            try
            {
                if (tsSegmentToolbar.IsHandleCreated)
                    if (tsSegmentToolbar.InvokeRequired)
                        tsSegmentToolbar.Invoke(new CurrentMessageSetTextDelegate(CurrentMessageSetText), s);
                    else
                        txtCurrentMessage.Text = s;
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        /// <summary>
        /// Sets the forms text
        /// </summary>
        /// <param name="s">The string to use</param>
        private void FormSetText(string s)
        {
            try
            {
                if (IsHandleCreated)
                    if (InvokeRequired)
                        Invoke(new FormSetTextDelegate(FormSetText), s);
                    else
                        Text = s;
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        /// <summary>
        /// Adds items to the Segment Changer
        /// </summary>
        /// <param name="item">The item to add</param>
        private void SegmentChangerAddItem(object item)
        {
            try
            {
                if (tsSegmentToolbar.IsHandleCreated)
                    if (tsSegmentToolbar.InvokeRequired)
                        tsSegmentToolbar.Invoke(new SegmentChangerAddItemDelegate(SegmentChangerAddItem), item);
                    else
                        cbSegmentChanger.Items.Add(item);
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        /// <summary>
        /// Adds items to the Segment Display
        /// </summary>
        /// <param name="item">The item to add</param>
        private void SegmentDisplayAddItem(ListViewItem item)
        {
            try
            {
                if (lvSegmentDisplay.IsHandleCreated)
                    if (lvSegmentDisplay.InvokeRequired)
                        lvSegmentDisplay.Invoke(new SegmentDisplayAddItemDelegate(SegmentDisplayAddItem), item);
                    else
                        lvSegmentDisplay.Items.Add(item);
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        /// <summary>
        /// Sets the forms cursor
        /// </summary>
        /// <param name="c">The cursor to use</param>
        private void FormSetCursor(Cursor c)
        {
            try
            {
                if (IsHandleCreated)
                    if (InvokeRequired)
                        Invoke(new FormSetCursorDelegate(FormSetCursor), c);
                    else
                        Cursor = c;
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        /// <summary>
        /// Adds a row to the TCPIP Transfer Display
        /// </summary>
        /// <param name="items">The item to add</param>
        private void TCPIPTransferDisplayAddRow(List<object> items)
        {
            try
            {
                if (dgvTCPIPTransferDisplay.IsHandleCreated)
                    if (dgvTCPIPTransferDisplay.InvokeRequired)
                        dgvTCPIPTransferDisplay.Invoke(
                            new TCPIPTransferDisplayAddRowDelegate(TCPIPTransferDisplayAddRow), items);
                    else
                        dgvTCPIPTransferDisplay.Rows.Add(items.ToArray());
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        /// <summary>
        /// Sets the font format across threads.
        /// </summary>
        private void MessageBoxSetFontFormat()
        {
            try
            {
                if (rtbMessageBox.IsHandleCreated)
                    if (rtbMessageBox.InvokeRequired)
                        rtbMessageBox.Invoke(new MessageBoxSetFontFormatDelegate(MessageBoxSetFontFormat));
                    else
                        SetRtbTextFormatOptions();
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Form Load Event: Loads application settings and the currently stored reports.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMain_Load(object sender, EventArgs e)
        {
            try
            {
                var settings = new Settings();
                settings.GetSettings();
                if (settings.CheckForUpdates)
                {
                    if (UpdateChecker.UpdateCheck())
                    {
                        var fua = new frmUpdateAvailable();
                        fua.ShowDialog();
                    }
                    UpdateChecker.SaveLastRunDate();
                }
                tsmHideEmpty.Checked = settings.HideEmptyFields;
                _extensions = settings.Extensions;
                var sb = new StringBuilder();
                for (var i = 0; i < settings.Extensions.Count; i++)
                    if (i != settings.Extensions.Count - 1)
                        sb.Append(settings.Extensions[i].ToUpper() + "|" + "*." + settings.Extensions[i] + "|");
                    else
                        sb.Append(settings.Extensions[i].ToUpper() + "|" + "*." + settings.Extensions[i]);
                sb.Append("|All|*.*");
                ofdOpenFiles.Filter = sb.ToString();
                if (Directory.Exists(Path.Combine(Application.StartupPath, "Reports")))
                    foreach (
                        var f in
                        Directory.GetFiles(Path.Combine(Application.StartupPath, "Reports"), "*.xml",
                            SearchOption.TopDirectoryOnly))
                    {
                        var fi = new FileInfo(f);
                        cbReportSelector.Items.Add(fi.Name.Replace(".xml", ""));
                    }
                foreach (var ftpConnectionFile in FTPOptions.GetFTPConnections())
                    cbFTPConnections.Items.Add(ftpConnectionFile);
                foreach (var tcpipConnectionFile in TCPIPOptions.GetTCPIPConnections())
                    cbTCPIPConnections.Items.Add(tcpipConnectionFile);
                foreach (var databaseConnectionFile in DatabaseOptions.GetDatabaseConnections())
                    cbDatabaseConnections.Items.Add(databaseConnectionFile);
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Open File(s) Tool Strip Menu Item Click Event: Displays an Open File Dialog and Opens The Selected File(s) after dialog closes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var dr = ofdOpenFiles.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    var loadFilesBgw = new BackgroundWorker();
                    loadFilesBgw.DoWork += LoadFilesBGW_DoWork;
                    loadFilesBgw.RunWorkerAsync(ofdOpenFiles.FileNames);
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Loads all selected files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadFilesBGW_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                FormSetCursor(Cursors.WaitCursor);
                var files = (string[])e.Argument;
                foreach (var f in files)
                    SetMessage(f);
                _currentMessage = 0;
                SetMessageDisplay(_currentMessage);
                FormSetCursor(Cursors.Default);
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Open Folder Tool Stip Menu Item Click Event: Displays an Folder Selector Dialog and opens the files in selected folder and sub-folders after dialog closes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var dr = fbOpenFolder.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    var loadFoldersBgw = new BackgroundWorker();
                    loadFoldersBgw.DoWork += LoadFoldersBGW_DoWork;
                    loadFoldersBgw.RunWorkerAsync(fbOpenFolder.SelectedPath);
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Loads all files in each folder in selected folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadFoldersBGW_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                FormSetCursor(Cursors.WaitCursor);
                foreach (var ext in _extensions)
                    foreach (var f in Directory.GetFiles((string)e.Argument, "*." + ext, SearchOption.AllDirectories))
                        SetMessage(f);
                _currentMessage = 0;
                SetMessageDisplay(_currentMessage);
                FormSetCursor(Cursors.Default);
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Search For File(s) Tool Stip Menu Item Click Event: Calls the frmSearch dialog box to search for files, opens the returned files after dialog closes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void searchForFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SearchForFiles("");
        }

        /// <summary>
        /// Close Tool Stip Menu Item Click Event: Closes the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Remove Message Click Event (Used by Tool Strip Menu and Context Menu)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void removeMessage_Click(object sender, EventArgs e)
        {
            RemoveMessage();
        }

        /// <summary>
        /// Clear Session Click Event (Used by Tool Strip Menu and Context Menu)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearSession_Click(object sender, EventArgs e)
        {
            ClearSessionDisplay();
        }

        /// <summary>
        /// Filter Records Click Event (Used by Tool Strip Menu and Context Menu)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void filterRecords_Click(object sender, EventArgs e)
        {
            FilterRecords();
        }

        /// <summary>
        /// Clear Filter Click Event (Used by Tool Strip Menu and Context Menu)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearFilter_Click(object sender, EventArgs e)
        {
            ClearFilters();
        }

        /// <summary>
        /// First Record Click Event (Used by Tool Bar Button, Tool Strip Menu, and Context Menu)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void first_Click(object sender, EventArgs e)
        {
            DisplayFirstRecord();
        }

        /// <summary>
        /// Previous Record Click Event (Used by Tool Bar Button, Tool Strip Menu, and Context Menu)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void previous_Click(object sender, EventArgs e)
        {
            DisplayPreviousRecord();
        }

        /// <summary>
        /// Next Record Click Event  (Used by Tool Bar Button, Tool Strip Menu, and Context Menu)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void next_Click(object sender, EventArgs e)
        {
            DisplayNextRecord();
        }

        /// <summary>
        /// Last Record Click Event  (Used by Tool Bar Button, Tool Strip Menu, and Context Menu)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void last_Click(object sender, EventArgs e)
        {
            DisplayLastRecord();
        }

        /// <summary>
        /// Toggle Segment Display Click Event  (Used by Tool Bar Button, Tool Strip Menu, and Context Menu)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toggleSegmentDisplay_Click(object sender, EventArgs e)
        {
            SetSegmentDisplay();
        }

        /// <summary>
        /// Create Report Click Event  (Used by Tool Bar Button, Tool Strip Menu, and Context Menu): Builds list of ListViewItem(s) and then calls CreateReport Method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void createReport_Click(object sender, EventArgs e)
        {
            try
            {
                if (lvSegmentDisplay.SelectedIndices.Count > 0)
                {
                    var lvis = new List<ListViewItem>();
                    foreach (ListViewItem lvi in lvSegmentDisplay.SelectedItems)
                        lvis.Add(lvi);
                    CreateReport(lvis);
                }
                else
                {
                    MessageBox.Show("You must select which fields to create a report from to create a report.");
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Delete Report Click Event: Calls the DeleteReport Method of the Reports class and then removes the item from the Report Selector box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (cbReportSelector.SelectedIndex > -1)
                {
                    Reports.DeleteReport(cbReportSelector.SelectedItem.ToString());
                    cbReportSelector.Items.Remove(cbReportSelector.SelectedItem);
                    cbReportSelector.Text = "";
                }
                else
                {
                    MessageBox.Show("You must select a report to delete a report.");
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// View Filled Components Click Event: Calls the frmSegments form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void viewUsedComponentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_messages.Count > 0)
            {
                Cursor = Cursors.WaitCursor;
                var fs = new frmSegments(_messages);
                fs.Show();
                Cursor = Cursors.Default;
            }
            else
            {
                MessageBox.Show("You must open a message file to display the filled fields.");
            }
        }

        /// <summary>
        /// View Message Statistics Click Event (Used by Tool Strip Menu and Context Menu)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void viewMessageTypeStatistics_Click(object sender, EventArgs e)
        {
            ViewStatistics();
        }

        /// <summary>
        /// View Hourly Traffic Statistics Click Event (Used by Tool Strip Menu and Context Menu)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void viewHourlyTrafficStatistics_Click(object sender, EventArgs e)
        {
            ViewHourlyTrafficStatistics();
        }

        /// <summary>
        /// View Daily Traffic Statistics Click Event (Used by Tool Strip Menu and Context Menu)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void viewDailyTrafficStatistics_Click(object sender, EventArgs e)
        {
            ViewDailyTrafficStatistics();
        }

        /// <summary>
        /// Hide Empty Components Check State Changed Event: Removes empty components from the Segment Display List View for the current message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmHideEmpty_CheckStateChanged(object sender, EventArgs e)
        {
            try
            {
                if (_messages.Count > 0)
                {
                    Cursor = Cursors.WaitCursor;
                    lvSegmentDisplay.Items.Clear();
                    var m = new Message(_messages[_currentMessage]);
                    foreach (var s in m.Segments)
                        SetListViewDisplay(s);
                    Cursor = Cursors.Default;
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Show Options Menu Click Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fo = new frmOptions();
            fo.ShowDialog();
        }

        /// <summary>
        /// Show About Box Click Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fa = new frmAbout();
            fa.Show();
        }

        /// <summary>
        /// Segment Selector Selected Index Changed Event: Clears the Segment Display List View of Segments and Displays the Selected Segment.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbSegmentChanger_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cbSegmentChanger.SelectedIndex > -1)
                {
                    Cursor = Cursors.WaitCursor;
                    lvSegmentDisplay.Items.Clear();
                    var m = new Message(_messages[_currentMessage]);
                    if (cbSegmentChanger.SelectedItem.ToString().ToUpper() != "ALL SEGMENTS")
                        foreach (var s in m.Segments.Get(cbSegmentChanger.SelectedItem.ToString()))
                            SetListViewDisplay(s);
                    else
                        foreach (var s in m.Segments)
                            SetListViewDisplay(s);
                    Cursor = Cursors.Default;
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Current Message Text Box KeyDown Event: Sets the current message to the entered number
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtCurrentMessage_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    var outInt = 0;
                    if (int.TryParse(txtCurrentMessage.Text, out outInt))
                    {
                        _currentMessage = outInt - 1;
                        if (_currentMessage > -1 && _currentMessage < _messages.Count)
                            SetMessageDisplay(_currentMessage);
                    }
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Sets support for copying Segment Display Values to the clipboard.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvSegmentDisplay_KeyDown(object sender, KeyEventArgs e)
        {
            if (lvSegmentDisplay.SelectedItems.Count > 0)
                if (e.KeyCode == Keys.C && e.Control)
                {
                    var sb = new StringBuilder();
                    foreach (ListViewItem lvi in lvSegmentDisplay.SelectedItems)
                        if (lvi.SubItems.Count > 2)
                        {
                            sb.Append(lvi.SubItems[2].Text);
                            sb.Append("\r\n");
                        }
                    Clipboard.SetText(sb.ToString());
                }
        }

        /// <summary>
        /// Segment Display Double Click Event: Calls EditFieldValues Method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvSegmentDisplay_DoubleClick(object sender, EventArgs e)
        {
            EditFieldValues();
        }

        /// <summary>
        /// Edit Selected Field Menu Item Click Event: Calls EditFieldValues Method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editSelectedFieldValues_Click(object sender, EventArgs e)
        {
            EditFieldValues();
        }

        /// <summary>
        /// Save Current Message Menu Item Click Event: Opens a Save File Dialog and then saves the file specified.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveCurrentMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var dr = sfdSaveFile.ShowDialog();

                if (dr == DialogResult.OK)
                    SaveMessage(rtbMessageBox.Text, sfdSaveFile.FileName);
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Save All Messages Menu Item Click Event: Opens a folder selection dialog and then saves all messages to that folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveAllMessagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var dr = fbOpenFolder.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    Cursor = Cursors.WaitCursor;
                    for (var i = 0; i < _messages.Count; i++)
                    {
                        var f = Path.Combine(fbOpenFolder.SelectedPath,
                            string.Format("HL7 Analyst {0}{1}.hl7", DateTime.Now.ToString("MMddyyyyHHmmss"), i));
                        SaveMessage(_messages[i], f);
                    }
                    Cursor = Cursors.Default;
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// DeIdentifiy Message Menu Click Event: Calls the DeIdentifyMessages Method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deIdentifyMessages_Click(object sender, EventArgs e)
        {
            DeIdentifyMessages();
        }

        /// <summary>
        /// Takes the selected FTP Connection Options and downloads the folders and files from the FTP site.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFTPConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (cbFTPConnections.SelectedIndex > -1)
                {
                    tvFTPDisplay.Nodes.Clear();
                    Cursor = Cursors.WaitCursor;
                    _loadedFtpOptions = FTPOptions.Load(cbFTPConnections.SelectedItem.ToString());
                    var root = new TreeNode(_loadedFtpOptions.FTPAddress);
                    var dl = FTPOperations.ListDirs(_loadedFtpOptions, _loadedFtpOptions.FTPAddress);
                    var fl = FTPOperations.ListFiles(_loadedFtpOptions, _loadedFtpOptions.FTPAddress, _extensions);

                    foreach (var d in dl)
                        root.Nodes.Add(d);
                    foreach (var f in fl)
                        root.Nodes.Add(f);
                    tvFTPDisplay.Nodes.Add(root);
                    Cursor = Cursors.Default;
                    btnFTPUpload.Enabled = true;
                    btnFTPDownload.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Downloads the selected folders files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvFTPDisplay_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (cbFTPConnections.SelectedIndex > -1)
                    if (tvFTPDisplay.SelectedNode != null)
                        if (tvFTPDisplay.SelectedNode.Text.IndexOf(".") == -1)
                        {
                            if (tvFTPDisplay.SelectedNode.Nodes.Count == 0)
                            {
                                Cursor = Cursors.WaitCursor;
                                var root = tvFTPDisplay.SelectedNode;

                                var subDir = root.FullPath.Replace("\\", "/");
                                var dl = FTPOperations.ListDirs(_loadedFtpOptions, subDir);
                                var fl = FTPOperations.ListFiles(_loadedFtpOptions, subDir, _extensions);

                                foreach (var d in dl)
                                    tvFTPDisplay.SelectedNode.Nodes.Add(d);
                                foreach (var f in fl)
                                    tvFTPDisplay.SelectedNode.Nodes.Add(f);
                                tvFTPDisplay.SelectedNode.Expand();
                                Cursor = Cursors.Default;
                            }
                        }
                        else
                        {
                            tvFTPDisplay.SelectedNode.Checked = !tvFTPDisplay.SelectedNode.Checked;
                        }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Creates and runs a background worker to download the selected files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFTPDownload_Click(object sender, EventArgs e)
        {
            try
            {
                if (cbFTPConnections.SelectedIndex > -1 && _ftpFiles.Count > 0)
                {
                    var ftpLoadFilesBgw = new BackgroundWorker();
                    ftpLoadFilesBgw.DoWork += FTPLoadFilesBGW_DoWork;
                    ftpLoadFilesBgw.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Downloads selected files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FTPLoadFilesBGW_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                FormSetCursor(Cursors.WaitCursor);
                foreach (var ftpFile in _ftpFiles)
                {
                    var contents = FTPOperations.Get(_loadedFtpOptions, ftpFile);
                    SetDownloadedMessage(contents);
                }
                if (_messages.Count == 1)
                {
                    _currentMessage = 0;
                    SetMessageDisplay(_currentMessage);
                }
                else
                {
                    MessageTotalSetText(string.Format("{0:0,0}", _messages.Count));
                }
                FormSetCursor(Cursors.Default);
                MessageBox.Show("Download Complete");
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Uploads all currently open messages to the FTP server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFTPUpload_Click(object sender, EventArgs e)
        {
            try
            {
                if (cbFTPConnections.SelectedIndex > -1 && _messages.Count > 0)
                {
                    Cursor = Cursors.WaitCursor;
                    for (var i = 0; i < _messages.Count; i++)
                    {
                        var fName = "";
                        if (tvFTPDisplay.SelectedNode != null)
                            fName = FTPOperations.Send(_loadedFtpOptions, _messages[i],
                                tvFTPDisplay.SelectedNode.FullPath.Replace("\\", "/"), i);
                        else
                            fName = FTPOperations.Send(_loadedFtpOptions, _messages[i], _loadedFtpOptions.FTPAddress, i);
                        tvFTPDisplay.SelectedNode.Nodes.Add(fName);
                    }
                    Cursor = Cursors.Default;
                    MessageBox.Show("Upload Complete");
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Loads the FTPConnection form to add a new FTP connection file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddFTPConnection_Click(object sender, EventArgs e)
        {
            try
            {
                var ffc = new frmFTPConnection();
                var dr = ffc.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    var ftpOps = ffc.ftpOps;
                    ftpOps.Save(ftpOps, Helper.RemoveUnsupportedChars(ffc.ConnectionName));
                    cbFTPConnections.Items.Add(Helper.RemoveUnsupportedChars(ffc.ConnectionName));
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Deletes the selected FTP connection file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteFTPConnection_Click(object sender, EventArgs e)
        {
            try
            {
                if (cbFTPConnections.SelectedIndex > -1)
                {
                    FTPOptions.Delete(cbFTPConnections.SelectedItem.ToString());
                    cbFTPConnections.Items.Remove(cbFTPConnections.SelectedItem);
                    cbFTPConnections.Text = "";
                }
            }
            catch (IOException)
            {
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Opens the FTP/TCPIP Transfer panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void transferToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                scSidePanel.Panel2Collapsed = !scSidePanel.Panel2Collapsed;
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// If the item being checked is a folder all files are checked if not it just checks the item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvFTPDisplay_AfterCheck(object sender, TreeViewEventArgs e)
        {
            try
            {
                if (e.Node.Text.IndexOf(".") == -1 && e.Node.Checked)
                {
                    for (var i = 0; i < e.Node.Nodes.Count; i++)
                        e.Node.Nodes[i].Checked = true;
                }
                else if (e.Node.Text.IndexOf(".") == -1 && !e.Node.Checked)
                {
                    for (var i = 0; i < e.Node.Nodes.Count; i++)
                        e.Node.Nodes[i].Checked = false;
                }
                else
                {
                    if (e.Node.Checked)
                        _ftpFiles.Add(e.Node.FullPath.Replace("\\", "/"));
                    else
                        _ftpFiles.Remove(e.Node.FullPath.Replace("\\", "/"));
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Loads the selected Connection File
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbTCPIPConnections_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cbTCPIPConnections.SelectedIndex > -1)
                {
                    _loadedTcpipOptions = TCPIPOptions.Load(cbTCPIPConnections.SelectedItem.ToString());
                    btnServer.Enabled = true;
                    btnClient.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Creates a Background Worker and runs the TCPListener in it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnServer_Click(object sender, EventArgs e)
        {
            try
            {
                var tcpListenerBgw = new BackgroundWorker();
                tcpListenerBgw.DoWork += tcpListenerBGW_DoWork;
                tcpListenerBgw.WorkerSupportsCancellation = true;
                if (btnServer.Text == ConstHelper.StartServer)
                {
                    _runTcpipServerLoop = true;
                    tcpListenerBgw.RunWorkerAsync();
                    btnServer.Text = ConstHelper.StopServer;
                    btnClient.Enabled = false;
                }
                else
                {
                    _runTcpipServerLoop = false;
                    tcpListenerBgw.CancelAsync();
                    btnServer.Text = ConstHelper.StartServer;
                    btnClient.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Downloads all messages sent to this TCP/IP Listener
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tcpListenerBGW_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var listener = new TcpListener(_loadedTcpipOptions.HostAddress, _loadedTcpipOptions.Port);
                listener.Start();
                while (_runTcpipServerLoop)
                    if (!e.Cancel)
                    {
                        if (listener.Pending())
                        {
                            var client = listener.AcceptTcpClient();
                            var stream = client.GetStream();
                            var messageBuffer = new byte[4096];
                            var sb = new StringBuilder();
                            int bytesRead;

                            while ((bytesRead = stream.Read(messageBuffer, 0, messageBuffer.Length)) != 0)
                                if (!e.Cancel)
                                {
                                    sb.AppendFormat("{0}", Encoding.ASCII.GetString(messageBuffer));
                                    if (sb.ToString().Contains(_loadedTcpipOptions.LLPHeader) &&
                                        sb.ToString().Contains(_loadedTcpipOptions.LLPTrailer))
                                    {
                                        var msgStrs = sb.ToString()
                                            .Split(new[] { _loadedTcpipOptions.LLPHeader },
                                                StringSplitOptions.RemoveEmptyEntries);
                                        foreach (var msg in msgStrs)
                                        {
                                            if (!e.Cancel)
                                            {
                                                if (msg.Contains(_loadedTcpipOptions.LLPTrailer))
                                                {
                                                    var dgvItems = new List<object>();
                                                    var m = new Message(msg.Replace(_loadedTcpipOptions.LLPHeader, ""));
                                                    dgvItems.Add(m.Segments.Get("MSH")[0].GetByID("MSH-10.1").Value);
                                                    if (_loadedTcpipOptions.SendAck)
                                                    {
                                                        var ack = HL7Lib.Base.Helper.CreateAck(m);
                                                        var ackBuffer =
                                                            Encoding.ASCII.GetBytes(string.Format("{0}{1}{2}",
                                                                _loadedTcpipOptions.LLPHeader, ack.DisplayString,
                                                                _loadedTcpipOptions.LLPTrailer));
                                                        stream.Write(ackBuffer, 0, ackBuffer.Length);
                                                        dgvItems.Add(true);
                                                    }
                                                    else
                                                    {
                                                        dgvItems.Add(false);
                                                    }
                                                    TCPIPTransferDisplayAddRow(dgvItems);
                                                    SetDownloadedMessage(m.DisplayString);
                                                    if (_messages.Count == 1)
                                                    {
                                                        _currentMessage = _messages.Count - 1;
                                                        SetMessageDisplay(_currentMessage);
                                                    }
                                                    else
                                                    {
                                                        MessageTotalSetText(string.Format("{0:0,0}", _messages.Count));
                                                    }
                                                }
                                                else
                                                {
                                                    sb = new StringBuilder();
                                                    sb.Append(msg);
                                                }
                                            }
                                            else
                                            {
                                                listener.Stop();
                                                listener.Server.Close();
                                                stream.Close();
                                                client.Close();
                                                break;
                                            }
                                            sb = sb.Replace(msg, "");
                                        }
                                    }
                                }
                                else
                                {
                                    listener.Stop();
                                    listener.Server.Close();
                                    stream.Close();
                                    client.Close();
                                    break;
                                }
                        }
                    }
                    else
                    {
                        listener.Stop();
                        listener.Server.Close();
                        break;
                    }
                listener.Stop();
                listener.Server.Close();
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Creates a Background Worker and runs it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClient_Click(object sender, EventArgs e)
        {
            try
            {
                var tcpClientBgw = new BackgroundWorker();
                tcpClientBgw.DoWork += tcpClientBGW_DoWork;
                tcpClientBgw.WorkerSupportsCancellation = true;
                if (btnClient.Text == ConstHelper.StartClient)
                {
                    tcpClientBgw.RunWorkerAsync();
                    btnClient.Text = ConstHelper.StopClient;
                    btnServer.Enabled = false;
                }
                else
                {
                    tcpClientBgw.CancelAsync();
                    btnClient.Text = ConstHelper.StartClient;
                    btnServer.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Sends all currently open messages to the connected server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tcpClientBGW_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var client = new TcpClient();
                var server = new IPEndPoint(_loadedTcpipOptions.HostAddress, _loadedTcpipOptions.Port);
                client.Connect(server);
                var stream = client.GetStream();
                foreach (var msg in _messages)
                    if (!e.Cancel)
                    {
                        var dgvItems = new List<object>();
                        var msgLLP =
                            Encoding.ASCII.GetBytes(string.Format("{0}{1}{2}", _loadedTcpipOptions.LLPHeader, msg,
                                _loadedTcpipOptions.LLPTrailer));
                        stream.Write(msgLLP, 0, msgLLP.Length);

                        var outboundMsg = new Message(msg);
                        dgvItems.Add(outboundMsg.Segments.Get("MSH")[0].GetByID("MSH-10.1").Value);
                        if (_loadedTcpipOptions.WaitForAck)
                        {
                            var ackLLP = new byte[4096];
                            stream.Read(ackLLP, 0, ackLLP.Length);
                            var ackStr = Encoding.ASCII.GetString(ackLLP);
                            var ackMsg =
                                new Message(
                                    ackStr.Replace(_loadedTcpipOptions.LLPHeader, "")
                                        .Replace(_loadedTcpipOptions.LLPTrailer, ""));
                            if (!HL7Lib.Base.Helper.ValidateAck(outboundMsg, ackMsg))
                                dgvItems.Add(false);
                            else
                                dgvItems.Add(true);
                        }
                        else
                        {
                            dgvItems.Add(false);
                        }
                        TCPIPTransferDisplayAddRow(dgvItems);
                    }
                    else
                    {
                        break;
                    }
                stream.Close();
                client.Close();
                MessageBox.Show("Upload Complete");
                _synchronizationContext.Post(_ =>
                {
                    btnClient.Text = ConstHelper.StartClient;
                    btnServer.Enabled = true;
                }, null);
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Loads the TCPIP Connection Form and creates a new TCP/IP Connection File
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddTCPIPConnection_Click(object sender, EventArgs e)
        {
            try
            {
                var ftc = new frmTCPIPConnection();
                var dr = ftc.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    TCPIPOptions.Save(Helper.RemoveUnsupportedChars(ftc.OptionsName), ftc.TcpipOps);
                    cbTCPIPConnections.Items.Add(Helper.RemoveUnsupportedChars(ftc.OptionsName));
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Deletes the selected TCP/IP Connection File
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteTCPIPConnection_Click(object sender, EventArgs e)
        {
            try
            {
                if (cbTCPIPConnections.SelectedIndex > -1)
                {
                    TCPIPOptions.Delete(cbTCPIPConnections.SelectedItem.ToString());
                    cbTCPIPConnections.Items.Remove(cbTCPIPConnections.SelectedItem.ToString());
                    cbTCPIPConnections.Text = "";
                }
            }
            catch (IOException)
            {
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Loads the selected report.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLoadReport_Click(object sender, EventArgs e)
        {
            if (cbReportSelector.SelectedIndex > -1 && _messages.Count > 0)
            {
                Cursor = Cursors.WaitCursor;
                var fr = new frmReports(_messages, cbReportSelector.SelectedItem.ToString());
                fr.Show();
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Message Box Rich Text Key Down Event: Allows for Copy and Paste Support.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rtbMessageBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Control)
                    switch (e.KeyCode)
                    {
                        case Keys.C:
                            e.Handled = true;
                            e.SuppressKeyPress = true;
                            Clipboard.SetText(rtbMessageBox.Text);
                            break;

                        case Keys.X:
                            e.Handled = true;
                            e.SuppressKeyPress = true;
                            Clipboard.SetText(rtbMessageBox.Text);
                            break;

                        case Keys.V:
                            e.Handled = true;
                            e.SuppressKeyPress = true;
                            SetDownloadedMessage(Clipboard.GetText());
                            _currentMessage = 0;
                            SetMessageDisplay(_currentMessage);
                            break;
                    }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Takes all currently displayed messages and determines their unique values and the amount of occurances for each of those values and displays them in a form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void displayUniqueValuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (lvSegmentDisplay.SelectedIndices.Count == 1 && _messages.Count > 0)
                {
                    var fuv = new frmUniqueValues(lvSegmentDisplay.SelectedItems[0].Text, _messages);
                    fuv.Show();
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Opens the Database Connection form and adds a new item to the cbDatabaseConnections combo box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddDatabaseConnection_Click(object sender, EventArgs e)
        {
            try
            {
                var fdc = new frmDatabaseConnection();
                var dr = fdc.ShowDialog();

                if (dr == DialogResult.OK)
                    cbDatabaseConnections.Items.Add(Helper.RemoveUnsupportedChars(fdc.Controls["txtName"].Text));
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Deletes the selected database connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteDatabaseConnection_Click(object sender, EventArgs e)
        {
            try
            {
                if (cbDatabaseConnections.SelectedIndex > -1)
                {
                    DatabaseOptions.Delete(cbDatabaseConnections.SelectedItem.ToString());
                    cbDatabaseConnections.Items.Remove(cbDatabaseConnections.SelectedItem);
                    cbDatabaseConnections.Text = "";
                }
            }
            catch (IOException)
            {
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Loads the selected database connection and enables the Execute Button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbDatabaseConnections_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cbDatabaseConnections.SelectedIndex > -1)
                {
                    txtDatabaseQuery.Text = "";
                    txtDatabaseWhereClause.Text = "";
                    btnExecute.Enabled = true;
                    _loadedDbOptions = DatabaseOptions.Load(cbDatabaseConnections.SelectedItem.ToString());
                    var queryParts = _loadedDbOptions.SQLQuery.Split(new[] { "Where" },
                        StringSplitOptions.RemoveEmptyEntries);

                    txtDatabaseQuery.Text = queryParts.GetValue(0).ToString();
                    if (queryParts.Length == 2)
                        txtDatabaseWhereClause.Text = queryParts.GetValue(1).ToString();
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Sets up the background worker and executes it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExecute_Click(object sender, EventArgs e)
        {
            try
            {
                var bgw = new BackgroundWorker();
                bgw.DoWork += bgw_DoWork;
                bgw.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Executes the selected query and downloads the returned HL7 messages from the database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            var con = new SqlConnection(_loadedDbOptions.SQLConnectionString);
            try
            {
                FormSetCursor(Cursors.WaitCursor);
                if (con.State == ConnectionState.Closed) con.Open();
                var query = txtDatabaseQuery.Text +
                            (txtDatabaseWhereClause.Text.Length > 0 ? "Where " + txtDatabaseWhereClause.Text : "");
                var command = new SqlCommand(query, con);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    SetDownloadedMessage((string)reader[_loadedDbOptions.SQLColumn]);
                    if (_messages.Count == 1)
                    {
                        _currentMessage = _messages.Count - 1;
                        SetMessageDisplay(_currentMessage);
                    }
                    else
                    {
                        MessageTotalSetText(string.Format("{0:0,0}", _messages.Count));
                    }
                }
                if (con.State == ConnectionState.Open) con.Close();
                FormSetCursor(Cursors.Default);
                MessageBox.Show("Download Complete");
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show(sqlEx.Message);
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
            finally
            {
                if (con.State == ConnectionState.Open) con.Close();
            }
        }

        /// <summary>
        /// Opens the default browser to the online documentation on CodePlex.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void helpDocumentationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://hl7analyst.codeplex.com/documentation");
        }

        /// <summary>
        /// Opens the default browser to the online Issue Tracker on CodePlex.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void reportBugSuggestFeatureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://hl7analyst.codeplex.com/workitem/list/basic");
        }

        /// <summary>
        /// Pulls any selected field values and sets them to a search string
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void searchUsingSelectedFieldsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var sb = new StringBuilder();
                foreach (ListViewItem lvi in lvSegmentDisplay.SelectedItems)
                    sb.AppendFormat("[{0}]{1} ", lvi.Text, lvi.SubItems[2].Text);
                SearchForFiles(sb.ToString());
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Calls the Hex form to display the current message in it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void viewMessageInHexViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_messages.Count > 0)
            {
                var fh = new frmHex(_messages[_currentMessage]);
                fh.Show();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Reads the selected files contents and sets all HL7 messages to the Messages and AllMessages Lists.
        /// </summary>
        /// <param name="file">The File to Read</param>
        private void SetMessage(string file)
        {
            try
            {
                var contents = file.ReadAllLinesWithoutComments();
                //var fi = new FileInfo(file);
                //var sr = new StreamReader(fi.FullName);
                //var contents = sr.ReadToEnd();
                if (contents.ToUpper().Contains("MSH"))
                {
                    var msgs = contents.Split(new[] { "MSH|" }, StringSplitOptions.RemoveEmptyEntries);
                    //sr.Close();

                    foreach (var msg in msgs)
                    {
                        var m = "MSH|" + msg;
                        _messages.Add(m);
                        _allMessages.Add(m);
                    }
                }
            }
            catch (OutOfMemoryException oome)
            {
                Log.LogException(oome).ShowDialog();
            }
            catch (FileNotFoundException fnfe)
            {
                Log.LogException(fnfe).ShowDialog();
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Reads the selected downloaded file contents and sets all HL7 messages to the Messages and AllMessages Lists.
        /// </summary>
        /// <param name="contents">The contents to Read</param>
        private void SetDownloadedMessage(string contents)
        {
            try
            {
                if (contents.ToUpper().Contains("MSH"))
                {
                    var msgs = contents.Split(new[] { "MSH|" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var msg in msgs)
                    {
                        var m = "MSH|" + msg;
                        _messages.Add(m);
                        _allMessages.Add(m);
                    }
                }
            }
            catch (OutOfMemoryException oome)
            {
                Log.LogException(oome).ShowDialog();
            }
            catch (FileNotFoundException fnfe)
            {
                Log.LogException(fnfe).ShowDialog();
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Sets the selected message display items to their respective controls on the form.
        /// </summary>
        /// <param name="MessageIndex">The message to display</param>
        private void SetMessageDisplay(int MessageIndex)
        {
            try
            {
                if (_messages.Count > 0)
                {
                    SegmentDisplayClearItems();
                    SegmentChangerClearItems();

                    var m = new Message(_messages[MessageIndex]);
                    MessageTotalSetText(string.Format("{0:0,0}", _messages.Count));
                    MessageBoxSetText(m.DisplayString);
                    CurrentMessageSetText(string.Format("{0}", MessageIndex + 1));

                    SegmentChangerAddItem("All Segments");
                    foreach (var segName in m.SegmentNames)
                        SegmentChangerAddItem(segName);

                    foreach (var s in m.Segments)
                        SetListViewDisplay(s);
                    if (m.GetByID("MSH-9.2") != null && m.GetByID("MSH-9.2").Count > 0 &&
                        m.GetByID("MSH-9.2")[0].Value != null)
                    {
                        var c = m.GetByID("MSH-9.2")[0];
                        var mt = new MessageType(c.Value);
                        FormSetText(string.Format("HL7 Analyst - {0}", mt.Description));
                    }
                    MessageBoxSetFontFormat();
                }
            }
            catch (ArgumentOutOfRangeException aore)
            {
                Log.LogException(aore).ShowDialog();
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Sets the Segment Display List View to the selected segment
        /// </summary>
        /// <param name="s">The segment to display</param>
        private void SetListViewDisplay(Segment s)
        {
            try
            {
                var fields = s.Fields;
                fields.Sort(
                    delegate (Field f1, Field f2)
                    {
                        return f1.Components[0].IDParts.FieldIndex.CompareTo(f2.Components[0].IDParts.FieldIndex);
                    });
                foreach (var f in fields)
                {
                    var Components = f.Components;
                    if (Components == null) throw new ArgumentNullException(nameof(Components));
                    Components.Sort(
                        delegate (Component c1, Component c2)
                        {
                            return c1.IDParts.ComponentIndex.CompareTo(c2.IDParts.ComponentIndex);
                        });
                    foreach (var c in Components)
                    {
                        if (tsmHideEmpty.Checked && string.IsNullOrEmpty(c.Value))
                            continue;

                        var lvi = new ListViewItem(c.ID);
                        if (!string.IsNullOrEmpty(c.Name))
                            lvi.SubItems.Add(f.Name + "-|-" + c.Name);
                        else
                            lvi.SubItems.Add(f.Name);
                        lvi.SubItems.Add(c.Value);
                        SegmentDisplayAddItem(lvi);
                    }
                }
                var emptyItem = new ListViewItem("");
                emptyItem.BackColor = Color.CornflowerBlue;
                SegmentDisplayAddItem(emptyItem);
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Toggles the segment display
        /// </summary>
        private void SetSegmentDisplay()
        {
            try
            {
                if (scSplitter.Panel1Collapsed)
                {
                    scSplitter.Panel1Collapsed = false;
                    btnMaximizeMinimize.Image = Resources.MaximizeDisplay;
                }
                else
                {
                    scSplitter.Panel1Collapsed = true;
                    btnMaximizeMinimize.Image = Resources.MinimizeDisplay;
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Clears all messages and form controls
        /// </summary>
        private void ClearSessionDisplay()
        {
            try
            {
                _messages.Clear();
                _allMessages.Clear();
                rtbMessageBox.Text = "";
                lvSegmentDisplay.Items.Clear();
                cbSegmentChanger.Items.Clear();
                tvFTPDisplay.Nodes.Clear();
                dgvTCPIPTransferDisplay.Rows.Clear();
                txtCurrentMessage.Text = "0";
                txtMessageTotal.Text = "0";
                Text = "HL7 Analyst";
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Displays the first message
        /// </summary>
        private void DisplayFirstRecord()
        {
            try
            {
                if (_messages.Count <= 0) return;
                _currentMessage = 0;
                SetMessageDisplay(_currentMessage);
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Displays the previous message
        /// </summary>
        private void DisplayPreviousRecord()
        {
            try
            {
                if (_currentMessage != 0 && _messages.Count > 0)
                {
                    _currentMessage--;
                    SetMessageDisplay(_currentMessage);
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Displays the next record
        /// </summary>
        private void DisplayNextRecord()
        {
            try
            {
                if (_currentMessage != _messages.Count - 1 && _messages.Count > 0)
                {
                    _currentMessage++;
                    SetMessageDisplay(_currentMessage);
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Displays the last record
        /// </summary>
        private void DisplayLastRecord()
        {
            try
            {
                if (_messages.Count <= 0) return;
                _currentMessage = _messages.Count - 1;
                SetMessageDisplay(_currentMessage);
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Filters the records based on selected list view item(s).
        /// </summary>
        private void FilterRecords()
        {
            try
            {
                if (lvSegmentDisplay.SelectedIndices.Count > 0)
                {
                    var lvis = lvSegmentDisplay.SelectedItems.Cast<ListViewItem>().ToList();

                    var ff = new frmFilter(lvis);
                    var dr = ff.ShowDialog();

                    if (dr == DialogResult.OK)
                    {
                        Cursor = Cursors.WaitCursor;
                        var displayMessages = new List<string>();

                        foreach (var m in _messages)
                        {
                            var allFiltersMatch = false;
                            var msg = new Message(m);
                            var dgv = (DataGridView)ff.Controls["dgvFilterOptions"];
                            foreach (DataGridViewRow dgvr in dgv.Rows)
                            {
                                var c = msg.GetByID(dgvr.Cells["chID"].FormattedValue.ToString(),
                                    dgvr.Cells["chValue"].FormattedValue.ToString());

                                if (!string.IsNullOrEmpty(c.ID))
                                {
                                    allFiltersMatch = true;
                                }
                                else
                                {
                                    allFiltersMatch = false;
                                    break;
                                }
                            }
                            if (allFiltersMatch)
                                displayMessages.Add(m);
                        }
                        if (displayMessages.Count > 0)
                        {
                            _messages = displayMessages;
                            _currentMessage = 0;
                            SetMessageDisplay(_currentMessage);
                        }
                        else
                        {
                            _messages = new List<string>();
                            _currentMessage = 0;
                            SegmentDisplayClearItems();
                            SegmentChangerClearItems();
                            rtbMessageBox.Text = "";
                            txtCurrentMessage.Text = "0";
                            txtMessageTotal.Text = "0";
                            MessageBox.Show("No messages found using entered filter");
                        }
                        Cursor = Cursors.Default;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Clears any filters that are active
        /// </summary>
        private void ClearFilters()
        {
            try
            {
                _messages = new List<string>();
                foreach (var s in _allMessages)
                    _messages.Add(s);
                _currentMessage = 0;
                SetMessageDisplay(_currentMessage);
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Creates a new report based on selected list view items
        /// </summary>
        /// <param name="lvis">The list of list view items to create report from.</param>
        private void CreateReport(List<ListViewItem> lvis)
        {
            try
            {
                var fnr = new frmNewReport();
                var dr = fnr.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    var reportName = fnr.Controls["txtReportName"].Text;
                    if (!string.IsNullOrEmpty(reportName))
                    {
                        var reportItems = new List<string>();
                        foreach (var lvi in lvis)
                            reportItems.Add(lvi.Text);
                        var r = new Reports();
                        r.SaveReport(reportItems, Helper.RemoveUnsupportedChars(reportName));
                        cbReportSelector.Items.Add(Helper.RemoveUnsupportedChars(reportName));
                        cbReportSelector.SelectedItem = Helper.RemoveUnsupportedChars(reportName);
                    }
                    else
                    {
                        MessageBox.Show("You must enter a report name to save it.");
                    }
                }
            }
            catch (DirectoryNotFoundException dnfe)
            {
                Log.LogException(dnfe).ShowDialog();
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Removes the selected message from the Messages and AllMessages list and re-sets the display
        /// </summary>
        private void RemoveMessage()
        {
            try
            {
                if (_messages.Count > 0)
                {
                    if (_currentMessage >= _messages.Count)
                        _currentMessage = _messages.Count - 1;
                    _messages.RemoveAt(_currentMessage);
                    _allMessages.RemoveAt(_currentMessage);
                    if (_messages.Count == 0)
                    {
                        rtbMessageBox.Text = "";
                        lvSegmentDisplay.Items.Clear();
                        cbSegmentChanger.Items.Clear();
                        txtCurrentMessage.Text = "0";
                        txtMessageTotal.Text = "0";
                    }
                    else
                    {
                        if (_messages.Count > _currentMessage)
                            SetMessageDisplay(_currentMessage);
                        else
                            SetMessageDisplay(_currentMessage - 1);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Opens the frmMessageStats form with the selected list view item.
        /// </summary>
        private void ViewStatistics()
        {
            try
            {
                if (lvSegmentDisplay.SelectedItems.Count == 1)
                {
                    var gTitle = string.Format("{0} Statistics", lvSegmentDisplay.SelectedItems[0].SubItems[1].Text);
                    var fms = new frmMessageStats(_messages, gTitle, lvSegmentDisplay.SelectedItems[0].Text, "STAT");
                    fms.Show();
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Opens the frmMessageStats with the MSH-7.1 Hourly option.
        /// </summary>
        private void ViewHourlyTrafficStatistics()
        {
            try
            {
                if (_messages.Count <= 0) return;
                var gTitle = "Hourly Message Traffic Statistics";
                var fms = new frmMessageStats(_messages, gTitle, "MSH-7.1", "HOURLY");
                fms.Show();
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Opens the frmMessageStats with the MSH-7.1 Daily option.
        /// </summary>
        private void ViewDailyTrafficStatistics()
        {
            try
            {
                if (_messages.Count <= 0) return;
                var gTitle = "Daily Message Traffic Statistics";
                var fms = new frmMessageStats(_messages, gTitle, "MSH-7.1", "DAILY");
                fms.Show();
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// De-Identifies all Messages
        /// </summary>
        private void DeIdentifyMessages()
        {
            try
            {
                if (_messages.Count <= 0) return;
                Cursor = Cursors.WaitCursor;
                var Msgs = new List<string>();
                foreach (var msg in _messages)
                {
                    var m = new Message(msg);
                    var segments = m.Segments.Get("PID");
                    if (segments.Count == 1)
                    {
                        var s = segments[0];

                        var last = s.GetByID("PID-5.1");
                        var first = s.GetByID("PID-5.2");
                        var sex = s.GetByID("PID-8.1");
                        var address = s.GetByID("PID-11.1");
                        var mrn = s.GetByID("PID-18.1");
                        var ssn = s.GetByID("PID-19.1");

                        var items = new List<EditItem>();
                        if (!string.IsNullOrEmpty(last.Value))
                            items.Add(new EditItem(last.ID, last.Value, HL7Lib.Base.Helper.RandomLastName()));
                        if (!string.IsNullOrEmpty(first.Value))
                            items.Add(new EditItem(first.ID, first.Value,
                                HL7Lib.Base.Helper.RandomFirstName(sex.Value)));
                        if (!string.IsNullOrEmpty(address.Value))
                            items.Add(new EditItem(address.ID, address.Value, HL7Lib.Base.Helper.RandomAddress()));
                        if (!string.IsNullOrEmpty(mrn.Value))
                            items.Add(new EditItem(mrn.ID, mrn.Value, HL7Lib.Base.Helper.RandomMRN()));
                        if (!string.IsNullOrEmpty(ssn.Value))
                            items.Add(new EditItem(ssn.ID, ssn.Value, "999-99-9999"));

                        Msgs.Add(EditValues(msg, items));
                    }
                }
                _messages = Msgs;
                SetMessageDisplay(_currentMessage);
                Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Opens the Edit Field Form and then edits the message(s)
        /// </summary>
        private void EditFieldValues()
        {
            try
            {
                if (lvSegmentDisplay.SelectedIndices.Count > 0)
                {
                    var lvis = new List<ListViewItem>();
                    foreach (ListViewItem lvi in lvSegmentDisplay.SelectedItems)
                        lvis.Add(lvi);

                    var fef = new frmEditField(lvis);
                    var dr = fef.ShowDialog();

                    if (dr == DialogResult.OK)
                    {
                        Cursor = Cursors.WaitCursor;
                        var editAllMessages = fef.EditAllMessages;
                        var editItems = fef.Items;

                        if (!editAllMessages)
                        {
                            var msg = _messages[_currentMessage];
                            _messages.RemoveAt(_currentMessage);
                            _messages.Insert(_currentMessage, EditValues(msg, editItems));
                        }
                        else
                        {
                            var editMessages = new List<string>();
                            foreach (var msg in _messages)
                                editMessages.Add(EditValues(msg, editItems));
                            _messages = editMessages;
                        }
                        _allMessages = _messages;
                        SetMessageDisplay(_currentMessage);
                        Cursor = Cursors.Default;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Edits the message string
        /// </summary>
        /// <param name="msg">The message to edit</param>
        /// <param name="items">The items to edit in the message</param>
        /// <returns>The message string after editing</returns>
        private string EditValues(string msg, List<EditItem> items)
        {
            try
            {
                var finalList = new List<EditItem>();
                var returnMsg = msg;
                var m = new Message(msg);
                foreach (var item in items)
                {
                    var com = m.GetByID(item.ComponentID);
                    foreach (var c in com)
                        finalList.Add(new EditItem(c.ID, c.Value, item.NewValue));
                }
                foreach (var i in finalList)
                    if (!string.IsNullOrEmpty(i.OldValue))
                        returnMsg = returnMsg.Replace(i.OldValue, i.NewValue);
                return returnMsg;
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
                return "";
            }
        }

        /// <summary>
        /// Saves the specified message to disk.
        /// </summary>
        /// <param name="msg">The message to save</param>
        /// <param name="f">The file path to save to</param>
        private void SaveMessage(string msg, string f)
        {
            try
            {
                var sw = new StreamWriter(f);
                sw.Write(msg.Replace("\n", "\r") + "\r\n");
                sw.Close();
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Creates a message from the current text in the RTF Box and calls the formatting methods to format the text in the RTF Box
        /// </summary>
        private void SetRtbTextFormatOptions()
        {
            var msg = new Message(rtbMessageBox.Text);
            SetRtbFontFormat(rtbMessageBox, msg);
        }

        /// <summary>
        /// Loops over each character in the text of the RTF Box and sets it's formatting based on what character it is.
        /// </summary>
        /// <param name="rtb">The RichTextBox to use</param>
        /// <param name="msg">The Message object to use</param>
        private void SetRtbFontFormat(RichTextBox rtb, Message msg)
        {
            try
            {
                for (var i = 0; i < rtb.Text.Length; i++)
                {
                    var c = rtb.Text[i].ToString();

                    if (c == msg.FieldSeperator)
                        SetRtbSelection(rtb, i, 1, Color.CornflowerBlue);
                    else if (c == msg.ComponentSeperator)
                        SetRtbSelection(rtb, i, 1, Color.Coral);
                    else if (c == msg.FieldRepeatSeperator)
                        SetRtbSelection(rtb, i, 1, Color.Turquoise);
                    else if (c == msg.SubComponentSeperator)
                        SetRtbSelection(rtb, i, 1, Color.Goldenrod);
                    else if (c == msg.EscapeCharacter)
                        SetRtbSelection(rtb, i, 1, Color.Fuchsia);
                }
                rtb.SelectionStart = 0;
                rtb.SelectionLength = 0;
            }
            catch (IndexOutOfRangeException)
            {
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }

        /// <summary>
        /// Sets the selection start and length and selection color of the RTF Box
        /// </summary>
        /// <param name="rtb">The RichTextBox to use</param>
        /// <param name="i">The selection start</param>
        /// <param name="len">The selection length</param>
        /// <param name="c">The color to set selection to</param>
        private void SetRtbSelection(RichTextBox rtb, int i, int len, Color c)
        {
            try
            {
                rtb.SelectionStart = i;
                rtb.SelectionLength = len;
                rtb.SelectionColor = c;
            }
            catch (IndexOutOfRangeException)
            {
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }

        /// <summary>
        /// Calls the search form and sets the returned messages to the message display
        /// </summary>
        /// <param name="searchTerms">The search query to use</param>
        private void SearchForFiles(string searchTerms)
        {
            try
            {
                var fs = new frmSearch(searchTerms);
                var dr = fs.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    if (fs.Messages.Count > 0)
                    {
                        foreach (var s in fs.Messages)
                        {
                            _messages.Add(s);
                            _allMessages.Add(s);
                        }
                        _currentMessage = 0;
                        SetMessageDisplay(_currentMessage);
                    }
                    else
                    {
                        MessageBox.Show("No messages returned by search");
                    }
                }
                else
                {
                    if (fs.Messages.Count > 0)
                    {
                        foreach (var s in fs.Messages)
                        {
                            _messages.Add(s);
                            _allMessages.Add(s);
                        }
                        _currentMessage = 0;
                        SetMessageDisplay(_currentMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        #endregion

        private void topMostToolStripMenuItem_Click(object sender, EventArgs e)
        {
            topMostToolStripMenuItem.Checked = !topMostToolStripMenuItem.Checked;
            TopMost = topMostToolStripMenuItem.Checked;
        }
    }
}