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

namespace HL7_Analyst
{
    /// <summary>
    /// GraphItems Class: Used to store information about each graph item used in the graphs.
    /// </summary>
    public class GraphItems
    {
        /// <summary>
        /// Empty constructor
        /// </summary>
        public GraphItems()
        {
        }

        /// <summary>
        /// GraphItem constructor
        /// </summary>
        /// <param name="name">The name to assign to the GraphItem</param>
        /// <param name="count">The value to assign to the GraphItem</param>
        public GraphItems(string name, int count)
        {
            Name = name;
            Count = count;
        }

        /// <summary>
        /// The Name of the item
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The Count of items with the specified name
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Pulls the specified GraphItem from a list
        /// </summary>
        /// <param name="items">The list to search</param>
        /// <param name="name">The name of the GraphItem to find</param>
        /// <returns>The GraphItem that was found.</returns>
        public static GraphItems GetGraphItem(List<GraphItems> items, string name)
        {
            var gi = items.Find(g => g.Name == name);
            return gi;
        }
    }
}