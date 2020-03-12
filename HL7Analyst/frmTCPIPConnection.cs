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
using System.Net;
using System.Windows.Forms;

#endregion

namespace HL7_Analyst
{
    /// <summary>
    /// TCPIP Connection Form: Used to create a new TCP/IP Connection File
    /// </summary>
    public partial class frmTCPIPConnection : Form
    {
        /// <summary>
        /// The Connection Name entered by the user
        /// </summary>
        public string OptionsName = "";

        /// <summary>
        /// The TCPIPOptions filled in by the user
        /// </summary>
        public TCPIPOptions TcpipOps = new TCPIPOptions();

        /// <summary>
        /// Initialization Method
        /// </summary>
        public frmTCPIPConnection()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Save Button Click Event: Saves the TCP/IP Options to File
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(txtName.Text))
                {
                    var ip = ParseHostAddress(txtHostAddress.Text);
                    var port = ParsePort(txtPort.Text);

                    if (ip != null)
                    {
                        if (port != 0)
                        {
                            OptionsName = txtName.Text;
                            TcpipOps.HostAddress = ip;
                            TcpipOps.Port = port;
                            TcpipOps.LLPHeader = LLP.GetLlpString(txtHeader.Text);
                            TcpipOps.LLPTrailer = LLP.GetLlpString(txtTrailer.Text);
                            TcpipOps.WaitForAck = cbWaitForAck.Checked;
                            TcpipOps.SendAck = cbSendAck.Checked;
                            DialogResult = DialogResult.OK;
                            Close();
                        }
                        else
                        {
                            MessageBox.Show("Please Add Port.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please Add Host Address.");
                    }
                }
                else
                {
                    MessageBox.Show("Please Enter a Unique Name for this Connection.");
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Cancels the Dialog Box
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
        /// Parses a Host Address from a string
        /// </summary>
        /// <param name="s">The string to parse</param>
        /// <returns>The IPAddress</returns>
        private IPAddress ParseHostAddress(string s)
        {
            try
            {
                IPAddress ip;
                if (IPAddress.TryParse(s, out ip))
                    return ip;
                return null;
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
                return null;
            }
        }

        /// <summary>
        /// Parses a port
        /// </summary>
        /// <param name="s">The string to parse</param>
        /// <returns>The port int</returns>
        private int ParsePort(string s)
        {
            try
            {
                var p = 0;
                if (int.TryParse(s, out p))
                    return p;
                return 0;
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
                return 0;
            }
        }

        /// <summary>
        /// Loads the LLP form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnHeader_Click(object sender, EventArgs e)
        {
            try
            {
                var fl = new frmLLP();
                var dr = fl.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    foreach (var l in fl.LLPList)
                        txtHeader.Text += l.Hex;
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }

        /// <summary>
        /// Loads the LLP Form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTrailer_Click(object sender, EventArgs e)
        {
            try
            {
                var fl = new frmLLP();
                var dr = fl.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    foreach (var l in fl.LLPList)
                        txtTrailer.Text += l.Hex;
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex).ShowDialog();
            }
        }
    }
}