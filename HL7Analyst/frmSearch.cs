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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using HL7Lib.Base;
using Message = HL7Lib.Base.Message;

#endregion

namespace HL7_Analyst
{
    /// <summary>
    /// Search Form: Searches files in specified locations for messages containing the search terms.
    /// </summary>
    public partial class frmSearch : Form
    {
        private readonly BackgroundWorker bgw = new BackgroundWorker();
        private List<string> _extensions = new List<string>();

        /// <summary>
        /// The Messages being returned after the search
        /// </summary>
        public List<string> Messages = new List<string>();

        private List<string> _previousSearches = new List<string>();

        /// <summary>
        /// Initialization Method
        /// </summary>        
        public frmSearch(string st)
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(st))
                txtSearchTerms.Text = st;
        }

        private delegate void UpdateCurrentFileDelegate(string item);

        private delegate void UpdateMatchCountDelegate(string item);

        private delegate void CloseFormDelegate();

        #region Cross Thread Invoke Methods

        /// <summary>
        /// Update the current file label
        /// </summary>
        /// <param name="v">The file to update to</param>
        private void UpdateCurrentFile(string v)
        {
            try
            {
                if (lblCurrentFile.IsHandleCreated)
                {
                    if (lblCurrentFile.InvokeRequired)
                    {
                        lblCurrentFile.Invoke(new UpdateCurrentFileDelegate(UpdateCurrentFile), v);
                    }
                    else
                    {
                        var fi = new FileInfo(v);
                        lblCurrentFile.Text = fi.Name;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        /// <summary>
        /// Updates the match count label
        /// </summary>
        /// <param name="v">The count to update to</param>
        private void UpdateMatchCount(string v)
        {
            try
            {
                if (lblMatchCount.IsHandleCreated)
                {
                    if (lblMatchCount.InvokeRequired)
                        lblMatchCount.Invoke(new UpdateMatchCountDelegate(UpdateMatchCount), v);
                    else
                        lblMatchCount.Text = v;
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        /// <summary>
        /// Closes the form
        /// </summary>
        private void CloseForm()
        {
            try
            {
                if (IsHandleCreated)
                {
                    if (InvokeRequired)
                        Invoke(new CloseFormDelegate(CloseForm));
                    else
                        Close();
                }
            }
            catch (ObjectDisposedException)
            {
                CloseForm();
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Form Load Event: Loads settings and sets up Background Worker.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmSearch_Load(object sender, EventArgs e)
        {
            try
            {
                var s = new Settings();
                s.GetSettings();
                txtSearchPath.Text = s.SearchPath;
                txtSearchPath.Text = HL7Analyst.Properties.Settings.Default.SearchPath;

                _extensions = s.Extensions;

                _previousSearches = SearchTerm.PullPreviousQueries();
                txtSearchTerms.AutoCompleteCustomSource.AddRange(_previousSearches.ToArray());

                bgw.WorkerSupportsCancellation = true;
                bgw.WorkerReportsProgress = true;
                bgw.DoWork += bgw_DoWork;
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Background Worker Do Work Event: Performs search
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var root = txtSearchPath.Text;
                var matchCount = 0;
                foreach (var ext in _extensions)
                {
                    if (!e.Cancel)
                    {
                        foreach (var file in Directory.GetFiles(root, "*." + ext, SearchOption.TopDirectoryOnly))
                        {
                            UpdateCurrentFile(file);
                            if (!e.Cancel)
                            {
                                var m = SearchFile(file);
                                if (m.Count > 0)
                                {
                                    foreach (var msg in m)
                                    {
                                        if (!e.Cancel)
                                        {
                                            matchCount++;
                                            UpdateMatchCount(matchCount.ToString());
                                            Messages.Add(msg);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        foreach (string d in clbSubFolders.CheckedItems)
                        {
                            if (!e.Cancel)
                            {
                                foreach (
                                    var f in
                                        Directory.GetFiles(Path.Combine(root, d), "*." + ext,
                                            SearchOption.TopDirectoryOnly))
                                {
                                    UpdateCurrentFile(f);
                                    if (!e.Cancel)
                                    {
                                        var m = SearchFile(f);
                                        if (m.Count > 0)
                                        {
                                            foreach (var msg in m)
                                            {
                                                if (!e.Cancel)
                                                {
                                                    matchCount++;
                                                    UpdateMatchCount(matchCount.ToString());
                                                    Messages.Add(msg);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                DialogResult = DialogResult.OK;
                CloseForm();
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        /// <summary>
        /// Populates the sub-folders selection box if the entered path exists.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSearchPath_TextChanged(object sender, EventArgs e)
        {
            try
            {
                clbSubFolders.Items.Clear();
                if (Directory.Exists(txtSearchPath.Text))
                {
                    var di = new DirectoryInfo(txtSearchPath.Text);
                    foreach (var d in di.GetDirectories())
                    {
                        clbSubFolders.Items.Add(d.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Cancels the background workers operations and closes the form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                if (bgw.IsBusy) bgw.CancelAsync();
                DialogResult = DialogResult.Cancel;
                Close();
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Starts the search using the background worker thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, EventArgs e)
        {
            cbSearchAll.Enabled = false;
            clbSubFolders.Enabled = false;
            txtSearchPath.Enabled = false;
            txtSearchTerms.Enabled = false;
            btnComponents.Enabled = false;
            btnSearchPath.Enabled = false;
            StartSearch();
        }

        /// <summary>
        /// Opens the Build Search Query form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnComponents_Click(object sender, EventArgs e)
        {
            try
            {
                var fbs = new frmBuildSearch(txtSearchTerms.Text);
                var dr = fbs.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    txtSearchTerms.Text = fbs.returnSearch.ToString().Trim();
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        private void btnSearchPath_Click(object sender, EventArgs e)
        {
            try
            {
                var dr = fbSearchPath.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    txtSearchPath.Text = fbSearchPath.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        private void txtSearchTerms_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    StartSearch();
                }
            }
        }

        private void frmSearch_FormClosed(object sender, FormClosedEventArgs e)
        {
            SearchTerm.SavePreviousQueries(_previousSearches);
            HL7Analyst.Properties.Settings.Default.SearchPath = txtSearchPath.Text;
            HL7Analyst.Properties.Settings.Default.Save();
        }

        private void cbSearchAll_CheckedChanged(object sender, EventArgs e)
        {
            if (cbSearchAll.Checked)
            {
                for (var i = 0; i < clbSubFolders.Items.Count; i++)
                    clbSubFolders.SetItemChecked(i, true);
            }
            else
            {
                for (var i = 0; i < clbSubFolders.Items.Count; i++)
                    clbSubFolders.SetItemChecked(i, false);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Searches the specified file
        /// </summary>
        /// <param name="f">The file to search</param>
        /// <returns>A list of messages that match search query</returns>
        private List<string> SearchFile(string f)
        {
            try
            {
                var msgList = new List<string>();
                var fi = new FileInfo(f);
                var sr = new StreamReader(fi.FullName);
                var contents = sr.ReadToEnd();
                sr.Close();

                var msgs = contents.Split(new[] {"MSH|"}, StringSplitOptions.RemoveEmptyEntries);

                foreach (var msg in msgs)
                {
                    var m = "MSH|" + msg;
                    var message = new Message(m);
                    if (SearchMessage(message))
                    {
                        msgList.Add(message.DisplayString);
                    }
                }
                return msgList;
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
                return new List<string>();
            }
        }

        /// <summary>
        /// Searches the message using the search query.
        /// </summary>
        /// <param name="m">The message to search</param>
        /// <returns>Returns true if the message matches the search query</returns>
        private bool SearchMessage(Message m)
        {
            try
            {
                var returnValue = false;

                foreach (var item in txtSearchTerms.Text.Split('|'))
                {
                    var allMatched = false;
                    var searchTerms =
                        SearchTerm.GetSearchTerms(item.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries));
                    foreach (var st in searchTerms)
                    {
                        var c = m.GetByID(st.ID, st.Value.ToUpper());
                        if (!string.IsNullOrEmpty(c.ID))
                        {
                            allMatched = true;
                        }
                        else
                        {
                            allMatched = false;
                            break;
                        }
                    }
                    if (allMatched)
                    {
                        returnValue = true;
                        break;
                    }
                }
                return returnValue;
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
                return false;
            }
        }

        private void StartSearch()
        {
            try
            {
                if (!string.IsNullOrEmpty(txtSearchPath.Text) && !string.IsNullOrEmpty(txtSearchTerms.Text))
                {
                    if (!_previousSearches.Contains(txtSearchTerms.Text))
                    {
                        _previousSearches.Add(txtSearchTerms.Text);
                    }
                    Cursor = Cursors.WaitCursor;
                    btnSearch.Enabled = false;
                    bgw.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        #endregion
    }
}