﻿/***************************************************************
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
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

#endregion

namespace HL7_Analyst
{
    /// <summary>
    /// Database Connection Form: Allows the user to build a query to download HL7 messages
    /// </summary>
    public partial class frmDatabaseConnection : Form
    {
        private string SQLColumn = "";
        private string SQLConnectionString = "";

        /// <summary>
        /// Initialization Method
        /// </summary>
        public frmDatabaseConnection()
        {
            InitializeComponent();
        }

        /// <summary>
        /// When the form loads it loads the frmDatabaseLogin form so the user can build a database connection string
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmDatabaseConnection_Load(object sender, EventArgs e)
        {
            try
            {
                var fdl = new frmDatabaseLogin();
                var dr = fdl.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    SQLConnectionString = fdl.SQLConnectionString.ToString();
                    AddTables();
                }
                else
                {
                    DialogResult = DialogResult.Cancel;
                    Close();
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Calls the AddColumns for the selected table
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cbTables.SelectedIndex > -1)
                    AddColumns(cbTables.SelectedItem.ToString());
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Sets the select statement for the selected column
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvColumns_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (cbTables.SelectedIndex > -1)
                {
                    string[] supportedTypes = {"VARCHAR", "NVARCHAR", "TEXT", "NTEXT", "IMAGE"};
                    SQLColumn = dgvColumns["cColumn", e.RowIndex].Value.ToString();
                    var data_type = dgvColumns["cDataType", e.RowIndex].Value.ToString();
                    var from = cbTables.SelectedItem.ToString();
                    if (supportedTypes.Contains(data_type.ToUpper()))
                    {
                        if (data_type.ToUpper() == "IMAGE")
                            txtSelect.Text =
                                string.Format(
                                    "Select Cast(Cast({0} As varbinary(max)) As varchar(max)) As {0}\r\nFrom {1}",
                                    SQLColumn, from);
                        else
                            txtSelect.Text = string.Format("Select {0}\r\nFrom {1}", SQLColumn, from);
                        txtWhere.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Executes the query to test it out
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTest_Click(object sender, EventArgs e)
        {
            var con = new SqlConnection(SQLConnectionString);
            try
            {
                if (con.State == ConnectionState.Closed) con.Open();
                var command = new SqlCommand();
                if (string.IsNullOrEmpty(txtWhere.Text))
                    command.CommandText = string.Format("{0}", txtSelect.Text);
                else
                    command.CommandText = string.Format("{0} Where {1}", txtSelect.Text, txtWhere.Text);
                command.Connection = con;
                command.ExecuteNonQuery();
                if (con.State == ConnectionState.Open) con.Close();
                MessageBox.Show("Query Succesful");
            }
            catch (SqlException sqlEX)
            {
                MessageBox.Show(sqlEX.Message);
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
        /// Sets up the database options object and saves it to disk.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(txtName.Text))
                {
                    var dbOptions = new DatabaseOptions();
                    dbOptions.SQLConnectionString = SQLConnectionString;
                    dbOptions.SQLColumn = SQLColumn;
                    if (!string.IsNullOrEmpty(txtWhere.Text))
                        dbOptions.SQLQuery = string.Format("{0} Where {1}", txtSelect.Text, txtWhere.Text);
                    else
                        dbOptions.SQLQuery = string.Format("{0}", txtSelect.Text);
                    DatabaseOptions.Save(txtName.Text, dbOptions);
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Closes the form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Adds the tables from the specified database to the combo box
        /// </summary>
        private void AddTables()
        {
            var con = new SqlConnection(SQLConnectionString);
            try
            {
                if (con.State == ConnectionState.Closed) con.Open();
                var command = new SqlCommand("Select TABLE_NAME From INFORMATION_SCHEMA.TABLES", con);
                var reader = command.ExecuteReader();
                while (reader.Read())
                    cbTables.Items.Add(reader["TABLE_NAME"]);
                if (con.State == ConnectionState.Open) con.Close();
            }
            catch (SqlException sqlEX)
            {
                MessageBox.Show(sqlEX.Message);
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
        /// Loads the columns from the specified table into the datagrid
        /// </summary>
        /// <param name="TBLName">The table to load</param>
        private void AddColumns(string TBLName)
        {
            dgvColumns.Rows.Clear();
            var con = new SqlConnection(SQLConnectionString);
            try
            {
                if (con.State == ConnectionState.Closed) con.Open();
                var command =
                    new SqlCommand(
                        string.Format(
                            "Select COLUMN_NAME, DATA_TYPE From INFORMATION_SCHEMA.COLUMNS Where TABLE_NAME = '{0}'",
                            TBLName), con);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var items = new List<object>();
                    items.Add(reader["COLUMN_NAME"]);
                    items.Add(reader["DATA_TYPE"]);
                    dgvColumns.Rows.Add(items.ToArray());
                }
                if (con.State == ConnectionState.Open) con.Close();
            }
            catch (SqlException sqlEX)
            {
                MessageBox.Show(sqlEX.Message);
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
    }
}