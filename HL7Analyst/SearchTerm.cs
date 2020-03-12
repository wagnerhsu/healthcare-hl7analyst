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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;

#endregion

namespace HL7_Analyst
{
    /// <summary>
    /// SearchTerm Class: Used to parse a string for search terms
    /// </summary>
    public class SearchTerm
    {
        /// <summary>
        /// Empty Constructor
        /// </summary>
        public SearchTerm()
        {
        }

        /// <summary>
        /// SearchTerm Constructor
        /// </summary>
        /// <param name="term"></param>
        public SearchTerm(string term)
        {
            var st = GetSearchTerm(term);
            if (st != null)
            {
                ID = st.ID;
                Value = st.Value;
            }
            else
            {
                ID = "";
                Value = "";
            }
        }

        /// <summary>
        /// The ID of the search term
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// The Value of the search term
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets the search term from the specified search string
        /// </summary>
        /// <param name="term">The term to get</param>
        /// <returns>Returns the SearchTerm</returns>
        public static SearchTerm GetSearchTerm(string term)
        {
            var st = new SearchTerm();
            if (term.Contains("]"))
            {
                var idReg = new Regex("(?<=\\[)[A-Za-z0-9]+-[0-9]+.[0-9]+(?=\\])");
                var idMatch = idReg.Match(term);
                st.ID = idMatch.Value;
                st.Value = idReg.Replace(term, "");
                st.Value = st.Value.Replace("[", "");
                st.Value = st.Value.Replace("]", "");
            }
            return st;
        }

        /// <summary>
        /// Builds a list of SearchTerms from a string array
        /// </summary>
        /// <param name="terms">The string array</param>
        /// <returns>A list of SearchTerms</returns>
        public static List<SearchTerm> GetSearchTerms(string[] terms)
        {
            var returnValue = new List<SearchTerm>();
            foreach (var term in terms)
            {
                var st = GetSearchTerm(term);
                returnValue.Add(st);
            }
            return returnValue;
        }

        /// <summary>
        /// Gets the search terms from a string of search terms
        /// </summary>
        /// <param name="terms">The string of terms to search</param>
        /// <returns>The double list of search terms and search term groups</returns>
        public static List<List<SearchTerm>> GetSearchTerms(string terms)
        {
            var returnValue = new List<List<SearchTerm>>();
            if (terms.Length > 0)
            {
                foreach (var term in terms.Split('|'))
                {
                    var searchGroup = new List<SearchTerm>();
                    foreach (var t in term.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var st = GetSearchTerm(t);
                        searchGroup.Add(st);
                    }
                    returnValue.Add(searchGroup);
                }
            }
            else
            {
                returnValue = new List<List<SearchTerm>>();
            }
            return returnValue;
        }

        /// <summary>
        /// Takes a list of search terms and builds a string representation of them
        /// </summary>
        /// <param name="terms">The list of search terms</param>
        /// <returns>The string of search terms</returns>
        public static string BuildSearchQueryString(List<SearchTerm> terms)
        {
            var sb = new StringBuilder();

            foreach (var term in terms)
            {
                sb.AppendFormat(" [{0}]{1}", term.ID, term.Value);
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Takes a list of search terms and builds a string representation of them
        /// </summary>
        /// <param name="terms">The list of search terms</param>
        /// <returns>The string of search terms</returns>
        public static string BuildSearchQueryString(List<List<SearchTerm>> terms)
        {
            var sb = new StringBuilder();
            foreach (var list in terms)
            {
                foreach (var term in list)
                {
                    sb.AppendFormat(" [{0}]{1}", term.ID, term.Value);
                }
                if (terms.Count > 1 && terms.LastIndexOf(list) != terms.Count - 1)
                    sb.AppendFormat("|");
            }
            return sb.ToString().Trim();
        }

        /// <summary>
        /// Pulls the previous searches from disk for autocomplete in the search terms box
        /// </summary>
        /// <returns>Returns the list of previously ran searches</returns>
        public static List<string> PullPreviousQueries()
        {
            var rootPath = Path.Combine(Application.StartupPath, "Previous Queries.xml");
            if (File.Exists(rootPath))
            {
                var items = new List<string>();
                var xDoc = XDocument.Load(rootPath);
                var list = from x in xDoc.Descendants("Query") select new {item = x.Value};
                foreach (var l in list)
                    items.Add(l.item.Trim());
                return items;
            }
            return new List<string>();
        }

        /// <summary>
        /// Saves the list of previous ran searches
        /// </summary>
        /// <param name="list">The list of previously ran search queries</param>
        public static void SavePreviousQueries(List<string> list)
        {
            var rootPath = Path.Combine(Application.StartupPath, "Previous Queries.xml");
            var rootElement = new XElement("Searches");
            foreach (var l in list)
            {
                var element = new XElement("Query");
                var cdata = new XCData(l);
                element.Add(cdata);
                rootElement.Add(element);
            }
            rootElement.Save(rootPath);
        }
    }
}