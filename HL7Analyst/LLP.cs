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
using System.Text;
using System.Text.RegularExpressions;

#endregion

namespace HL7_Analyst
{
    /// <summary>
    /// LLP Class: Defines an LLP object
    /// </summary>
    public class LLP
    {
        /// <summary>
        /// Empty Constructor
        /// </summary>
        public LLP()
        {
        }

        /// <summary>
        /// Constructs an LLP object with the specified values
        /// </summary>
        /// <param name="charValue">The Char Value to use in the LLP Wrapper</param>
        /// <param name="hex">The Hex Value of the Char Code</param>
        /// <param name="desc">The Hex Code Description</param>
        public LLP(char charValue, string hex, string desc)
        {
            CharValue = charValue;
            Hex = hex;
            Description = desc;
        }

        /// <summary>
        /// The Char Value to use in the LLP Wrapper
        /// </summary>
        public char CharValue { get; set; }

        /// <summary>
        /// The Hex Value of the Char Code
        /// </summary>
        public string Hex { get; set; }

        /// <summary>
        /// The Hex Code Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Pulls an LLP Object from the list of LLP objects
        /// </summary>
        /// <param name="hex">The Hex Value of the Char Code</param>
        /// <returns>Returns the LLP object</returns>
        public static LLP LoadLLP(string hex)
        {
            var llp = LoadLlpList().Find(delegate(LLP l) { return l.Hex == hex; });
            return llp;
        }

        /// <summary>
        /// Pulls the LLP Char Value based on the Hex code passed in
        /// </summary>
        /// <param name="s">The hex code to search for</param>
        /// <returns>The LLP Char Value</returns>
        public static string GetLlpString(string s)
        {
            var sb = new StringBuilder();
            var reg = new Regex("\\[0x[A-Za-z0-9]+\\]");
            var matches = reg.Matches(s);
            foreach (Match match in matches)
            {
                var l = LoadLLP(match.Value);
                sb.Append(l.CharValue);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Loads the list of LLP accepted values
        /// </summary>
        /// <returns>The List of LLP values</returns>
        public static List<LLP> LoadLlpList()
        {
            var llpList = new List<LLP>();
            llpList.Add(new LLP((char) 0, "[0x0]", "Null char"));
            llpList.Add(new LLP((char) 1, "[0x1]", "Start of Heading"));
            llpList.Add(new LLP((char) 2, "[0x2]", "Start of Text"));
            llpList.Add(new LLP((char) 3, "[0x3]", "End of Text"));
            llpList.Add(new LLP((char) 4, "[0x4]", "End of Transmission"));
            llpList.Add(new LLP((char) 5, "[0x5]", "Inquiry"));
            llpList.Add(new LLP((char) 6, "[0x6]", "Acknowledgment"));
            llpList.Add(new LLP((char) 7, "[0x7]", "Bell"));
            llpList.Add(new LLP((char) 8, "[0x8]", "Back Space"));
            llpList.Add(new LLP((char) 9, "[0x9]", "Horizontal Tab"));
            llpList.Add(new LLP((char) 10, "[0x0A]", "Line Feed"));
            llpList.Add(new LLP((char) 11, "[0x0B]", "Vertical Tab"));
            llpList.Add(new LLP((char) 12, "[0x0C]", "Form Feed"));
            llpList.Add(new LLP((char) 13, "[0x0D]", "Carriage Return"));
            llpList.Add(new LLP((char) 14, "[0x0E]", "Shift Out / X-On"));
            llpList.Add(new LLP((char) 15, "[0x0F]", "Shift In / X-Off"));
            llpList.Add(new LLP((char) 16, "[0x10]", "Data Line Escape"));
            llpList.Add(new LLP((char) 17, "[0x11]", "Device Control 1 (oft. XON)"));
            llpList.Add(new LLP((char) 18, "[0x12]", "Device Control 2"));
            llpList.Add(new LLP((char) 19, "[0x13]", "Device Control 3 (oft. XOFF)"));
            llpList.Add(new LLP((char) 20, "[0x14]", "Device Control 4"));
            llpList.Add(new LLP((char) 21, "[0x15]", "Negative Acknowledgment"));
            llpList.Add(new LLP((char) 22, "[0x16]", "Synchronous Idle"));
            llpList.Add(new LLP((char) 23, "[0x17]", "End of Transmit Block"));
            llpList.Add(new LLP((char) 24, "[0x18]", "Cancel"));
            llpList.Add(new LLP((char) 25, "[0x19]", "End of Medium"));
            llpList.Add(new LLP((char) 26, "[0x1A]", "Substitute"));
            llpList.Add(new LLP((char) 27, "[0x1B]", "Escape"));
            llpList.Add(new LLP((char) 28, "[0x1C]", "File Separator"));
            llpList.Add(new LLP((char) 29, "[0x1D]", "Group Separator"));
            llpList.Add(new LLP((char) 30, "[0x1E]", "Record Separator"));
            llpList.Add(new LLP((char) 31, "[0x1F]", "Unit Separator"));
            return llpList;
        }
    }
}