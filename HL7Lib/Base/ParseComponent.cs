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

namespace HL7Lib.Base
{
    /// <summary>
    /// ParseComponent Class: Creates a Component value object
    /// </summary>
    public class ParseComponent
    {
        /// <summary>
        /// ParseComponent Constructor: Constructs a ParseComponent object
        /// </summary>
        /// <param name="index">The Index of this instance of the ParseComponent</param>
        /// <param name="value">The Value of this instance of the ParseComponent</param>
        public ParseComponent(int index, string value)
        {
            ComponentIndex = index;
            ComponentValue = value;
        }

        /// <summary>
        /// The ComponentIndex of this component value object
        /// </summary>
        public int ComponentIndex { get; set; }

        /// <summary>
        /// The ComponentValue of this component value object
        /// </summary>
        public string ComponentValue { get; set; }
    }
}