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
    /// Component Class: Constructs an HL7 Component Object
    /// </summary>
    public class Component
    {
        /// <summary>
        /// Empty Constructor
        /// </summary>
        public Component()
        {
        }

        /// <summary>
        /// Constructs a Component with the specified Name
        /// </summary>
        /// <param name="name">The Name of the Component to Construct</param>
        public Component(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Constructs a Component with the specified Name and ID
        /// </summary>
        /// <param name="name">The Name of the Component to Construct</param>
        /// <param name="id">The ID of the Component to Construct</param>
        public Component(string name, string id)
        {
            Name = name;
            ID = id;
        }

        /// <summary>
        /// The Name of the Component
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The ID of the Component
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// The Value of the Component
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The ComponentID Parts for the ID: Read Only
        /// </summary>
        public ComponentId IDParts
        {
            get { return ID.ConvertID(); }
        }
    }
}