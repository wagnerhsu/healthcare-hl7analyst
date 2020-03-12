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

#endregion

namespace HL7_Analyst
{
    /// <summary>
    /// Reports Form: Displays selected report values in a data grid.
    /// </summary>
    public partial class frmReports : Form
    {
        private readonly List<string> _messages = new List<string>();
        private readonly string _reportName = "";

        /// <summary>
        /// Initialization Method: Sets the messages and report name at runtime
        /// </summary>
        /// <param name="Msgs">Messages to pull records from</param>
        /// <param name="RN">Report name to use.</param>
        public frmReports(List<string> Msgs, string RN)
        {
            InitializeComponent();
            _messages = Msgs;
            _reportName = RN;
        }

        private delegate void AddColumnsDelegate(ReportColumn columnName);

        private delegate void AddRowsDelegate(List<object> objs);

        private delegate void UpdateFormTextDelegate(string s);

        private delegate void UpdateFormCursorDelegate(Cursor c);

        #region Cross Thread Invoke Methods

        /// <summary>
        /// Adds columns to the data grid
        /// </summary>
        /// <param name="columnName">The column values to add</param>
        private void AddColumns(ReportColumn columnName)
        {
            try
            {
                if (dgvReportItems.IsHandleCreated)
                {
                    if (dgvReportItems.InvokeRequired)
                        dgvReportItems.Invoke(new AddColumnsDelegate(AddColumns), columnName);
                    else
                        dgvReportItems.Columns.Add(columnName.Name, columnName.Header);
                }
            }

            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        /// <summary>
        /// Adds rows to the data grid
        /// </summary>
        /// <param name="objs">The grid row object array to add</param>
        private void AddRows(List<object> objs)
        {
            try
            {
                if (dgvReportItems.IsHandleCreated)
                {
                    if (dgvReportItems.InvokeRequired)
                        dgvReportItems.Invoke(new AddRowsDelegate(AddRows), objs);
                    else
                        dgvReportItems.Rows.Add(objs.ToArray());
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        /// <summary>
        /// Updates the forms title text
        /// </summary>
        /// <param name="s">The text to update to</param>
        private void UpdateFormText(string s)
        {
            try
            {
                if (IsHandleCreated)
                {
                    if (InvokeRequired)
                        Invoke(new UpdateFormTextDelegate(UpdateFormText), s);
                    else
                        Text = s;
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        /// <summary>
        /// Updates the forms cursor to the specified cursor
        /// </summary>
        /// <param name="c">Cursor to update to</param>
        private void UpdateFormCursor(Cursor c)
        {
            try
            {
                if (IsHandleCreated)
                {
                    if (InvokeRequired)
                        Invoke(new UpdateFormCursorDelegate(UpdateFormCursor), c);
                    else
                        Cursor = c;
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Form load event: Sets up background worker.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmReports_Load(object sender, EventArgs e)
        {
            try
            {
                Text = "Report - " + _reportName;
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
        /// Background Worker Do Work Event: Adds the reports rows to the data grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                UpdateFormCursor(Cursors.WaitCursor);
                var r = new Reports();
                r.LoadReport(_reportName, _messages);
                foreach (var rc in r.Columns)
                    AddColumns(rc);
                for (var i = 0; i < r.Items.Count; i++)
                {
                    var objs = new List<object>();
                    for (var x = 0; x < r.Items[i].Count; x++)
                    {
                        objs.Add(FormatItem(r.Items[i][x]));
                    }
                    AddRows(objs);
                }
                UpdateFormText(string.Format("Reports - {0} - {1} Records Displayed", _reportName, r.Items.Count));
                UpdateFormCursor(Cursors.Default);
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Opens a save file dialog and passes the selected file name to the SaveReport method.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                SaveReport();
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Data Grid View Report Items Key Down Event: Sets up shortcut key support for saving the report.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvReportItems_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Control)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.S:
                            e.Handled = true;
                            e.SuppressKeyPress = true;
                            SaveReport();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// FormatItem Method: Formats the value string for display
        /// </summary>
        /// <param name="item">Item Text</param>
        /// <returns>Returns the formatted object</returns>
        private object FormatItem(string item)
        {
            try
            {
                object returnObj;
                var d = item.FromHl7Date();
                if (d != null)
                    returnObj = d.Value.ToString("MM/dd/yyyy HH:mm:ss");
                else
                    returnObj = item;

                return returnObj;
            }
            catch (Exception)
            {
                return item;
            }
        }

        /// <summary>
        /// Saves the report to disk
        /// </summary>        
        private void SaveReport()
        {
            try
            {
                var dr = sfdSaveReport.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    var sw = new StreamWriter(sfdSaveReport.FileName);
                    foreach (DataGridViewColumn column in dgvReportItems.Columns)
                    {
                        sw.Write("{0},", column.HeaderText);
                    }
                    sw.Write("\r\n");
                    foreach (DataGridViewRow row in dgvReportItems.Rows)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            sw.Write("\"{0}\",", cell.Value);
                        }
                        sw.Write("\r\n");
                    }
                    sw.Close();
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