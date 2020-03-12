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

#endregion

namespace HL7Lib.Base
{
    /// <summary>
    /// Field Class: Constructs an HL7 Field Object
    /// </summary>
    public class Field
    {
        /// <summary>
        /// Empty Constructor
        /// </summary>
        public Field()
        {
        }

        /// <summary>
        /// Constructs a Field with the specified Name
        /// </summary>
        /// <param name="name">The Name of the Field to Construct</param>
        public Field(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Constructs a Field with the specified Name and Components List
        /// </summary>
        /// <param name="name">The Name of the Field to Construct</param>
        /// <param name="components">The list of Components for the Field to Construct</param>
        public Field(string name, List<Component> components)
        {
            Name = name;
            Components = components;
        }

        /// <summary>
        /// The Name of the Field
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The List of Components Associated with this Field
        /// </summary>
        public List<Component> Components { get; set; }

        /// <summary>
        /// Sets a static field for all Segment Name Fields
        /// </summary>
        /// <returns>Returns the segment name field</returns>
        public static Field SegName()
        {
            var f = new Field("Segment Name");
            var c = new List<Component>();
            c.Add(new Component(""));
            f.Components = c;
            return f;
        }
    }
}