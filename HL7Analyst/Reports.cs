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

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Message = HL7Lib.Base.Message;

#endregion

namespace HL7_Analyst
{
    /// <summary>
    /// Reports Class: Used to perform operations on Report Files.
    /// </summary>
    internal class Reports
    {
        /// <summary>
        /// The Columns assigned to this report
        /// </summary>
        public List<ReportColumn> Columns { get; set; }

        /// <summary>
        /// The Items for this report
        /// </summary>
        public List<List<string>> Items { get; set; }

        /// <summary>
        /// LoadReport Method: Loads the specified report with the values in a list of messages
        /// </summary>
        /// <param name="reportName">The report to load</param>
        /// <param name="messages">The list of Messages to use in the report</param>
        public void LoadReport(string reportName, List<string> messages)
        {
            Columns = new List<ReportColumn>();
            Items = new List<List<string>>();

            if (Directory.Exists(Path.Combine(Application.StartupPath, "Reports")))
            {
                if (File.Exists(Path.Combine(Path.Combine(Application.StartupPath, "Reports"), reportName + ".xml")))
                {
                    var xtr =
                        new XmlTextReader(Path.Combine(Path.Combine(Application.StartupPath, "Reports"),
                            reportName + ".xml"));
                    xtr.Read();
                    var xDoc = new XmlDocument();
                    xDoc.Load(xtr);

                    var nodes = xDoc.SelectNodes("Report/Column");

                    foreach (XmlNode node in nodes)
                    {
                        var rc = new ReportColumn();
                        rc.Name = node.InnerText.Replace("-", "").Replace(".", "");
                        rc.Header = node.InnerText;
                        if (!Columns.Contains(rc))
                            Columns.Add(rc);
                    }

                    xtr.Close();

                    foreach (var m in messages)
                    {
                        var itemList = new List<string>();
                        var msg = new Message(m);
                        foreach (var s in msg.Segments)
                        {
                            foreach (var f in s.Fields)
                            {
                                foreach (var c in f.Components)
                                {
                                    if (!string.IsNullOrEmpty(GetColumn(c.ID)))
                                    {
                                        itemList.Add(c.Value);
                                    }
                                }
                            }
                        }
                        Items.Add(itemList);
                    }
                }
            }
        }

        /// <summary>
        /// GetColumn Method: Pulls the specified column from the list of columns.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private string GetColumn(string id)
        {
            var returnStr = "";

            if (Columns.Count > 0)
            {
                var rc = Columns.Find(delegate(ReportColumn col) { return col.Header == id; });
                if (rc != null)
                    returnStr = rc.Header;
            }
            return returnStr;
        }

        /// <summary>
        /// SaveReport Method: Saves a report file with the specified name and columns
        /// </summary>
        /// <param name="reportItems">The report items to use</param>
        /// <param name="reportName">The report name to use</param>
        public void SaveReport(List<string> reportItems, string reportName)
        {
            if (Directory.Exists(Path.Combine(Application.StartupPath, "Reports")))
            {
                var xtw =
                    new XmlTextWriter(
                        Path.Combine(Path.Combine(Application.StartupPath, "Reports"),
                            Helper.RemoveUnsupportedChars(reportName) + ".xml"), Encoding.UTF8);
                xtw.WriteStartDocument();
                xtw.WriteStartElement("Report");
                foreach (var item in reportItems)
                {
                    xtw.WriteStartElement("Column");
                    xtw.WriteString(item);
                    xtw.WriteEndElement();
                }
                xtw.WriteEndElement();
                xtw.Close();
            }
            else
            {
                Directory.CreateDirectory(Path.Combine(Application.StartupPath, "Reports"));
                SaveReport(reportItems, reportName);
            }
        }

        /// <summary>
        /// Delete Report Method: Deletes the specified report file
        /// </summary>
        /// <param name="reportName"></param>
        public static void DeleteReport(string reportName)
        {
            File.Delete(Path.Combine(Path.Combine(Application.StartupPath, "Reports"), reportName + ".xml"));
        }
    }
}