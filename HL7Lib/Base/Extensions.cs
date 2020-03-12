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
using System.Linq;
using System.Text;

#endregion

namespace HL7Lib.Base
{
    /// <summary>
    /// Extension Methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Pulls a specified segment from a list of segments by name.
        /// </summary>
        /// <param name="s">The list of segments to search.</param>
        /// <param name="SegmentName">The segment name to pull.</param>
        /// <returns>Returns the specified segment if it exists, else returns null.</returns>
        public static List<Segment> Get(this List<Segment> s, string SegmentName)
        {
            var returnSegment = s.FindAll(delegate (Segment seg) { return seg.Name.ToUpper() == SegmentName.ToUpper(); });
            return returnSegment;
        }

        /// <summary>
        /// Pulls a specified Field from a list of Fields by Name
        /// </summary>
        /// <param name="fList">The list of fields to search</param>
        /// <param name="FieldName">The field name to pull</param>
        /// <returns>Returns the specified field if it exists, else returns null.</returns>
        public static Field Get(this List<Field> fList, string FieldName)
        {
            var field = fList.Find(delegate (Field f) { return f.Name == FieldName; });
            return field;
        }

        /// <summary>
        /// Pulls a specified Component from a list of Components by Name
        /// </summary>
        /// <param name="cList">The list of components to search</param>
        /// <param name="componentName">The component name to pull</param>
        /// <returns>Returns the specified component if it exists, else returns null</returns>
        public static Component Get(this List<Component> cList, string componentName)
        {
            var component = cList.Find(c => c.Name == componentName);
            return component;
        }

        /// <summary>
        /// Pulls a specified Component from a list of Components byID
        /// </summary>
        /// <param name="cList">The list of components to search</param>
        /// <param name="componentId">The component ID to pull</param>
        /// <returns>Returns the specified component if it exists, else returns null</returns>
        public static Component GetByID(this List<Component> cList, string componentId)
        {
            var component = cList.Find(c => c.ID == componentId);
            return component;
        }

        /// <summary>
        /// Gets the specified component from a segment by the component ID
        /// </summary>
        /// <param name="s">The segment</param>
        /// <param name="componentId">The component ID</param>
        /// <returns>Returns the specified component if it exists, else returns null</returns>
        public static Component GetByID(this Segment s, string componentId)
        {
            var item =
                (from field in s.Fields
                 where field.Components.GetByID(componentId) != null
                 select field.Components.GetByID(componentId)).First();
            return item;
        }

        /// <summary>
        /// Takes a standard Component ID and converts it to a ComponentID object
        /// </summary>
        /// <param name="comId">The Component ID string to convert</param>
        /// <returns></returns>
        public static ComponentId ConvertID(this string comId)
        {
            var cid = new ComponentId();
            var parts = comId.Split('-');
            var idParts = parts.GetValue(1).ToString().Split('.');
            cid.SegmentName = parts.GetValue(0).ToString();
            cid.FieldIndex = Convert.ToInt32(idParts.GetValue(0).ToString());
            cid.ComponentIndex = Convert.ToInt32(idParts.GetValue(1).ToString()) - 1;
            return cid;
        }

        /// <summary>
        /// Gets a list of components from a single message by ID
        /// </summary>
        /// <param name="m">The message to search</param>
        /// <param name="id">The ID of the component to pull</param>
        /// <returns>Returns the list of components</returns>
        public static List<Component> GetByID(this Message m, string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                ComponentId cid = id.ConvertID();
                var returnValue = new List<Component>();
                var items = from com in m.Segments.Get(cid.SegmentName)
                            where com.GetByID(id) != null
                            select com.GetByID(id);
                foreach (Component c in items)
                    returnValue.Add(c);
                return returnValue;
            }
            return null;
        }

        /// <summary>
        /// Pulls a list of components from a list of messages based on the ID passed in
        /// </summary>
        /// <param name="msgs">The messages to pull from</param>
        /// <param name="id">The component ID to pull</param>
        /// <returns>A list of components matching the ID passed in</returns>
        public static List<Component> GetByID(this List<Message> msgs, string id)
        {
            var items = from com in msgs where com.GetByID(id) != null select com.GetByID(id);
            var coms = new List<Component>();
            foreach (List<Component> cs in items)
                foreach (var c in cs)
                    coms.Add(c);
            return coms;
        }

        /// <summary>
        /// Gets a single component from a single message by ID and with the specified value
        /// </summary>
        /// <param name="m">The message to search</param>
        /// <param name="id">The ID of the component to pull</param>
        /// <param name="valueString">The value of the component to search for</param>
        /// <returns>The component returned</returns>
        public static Component GetByID(this Message m, string id, string valueString)
        {
            var returnValue = new Component();
            if (!string.IsNullOrEmpty(id))
            {
                ComponentId cid = id.ConvertID();
                var segments = m.Segments.Get(cid.SegmentName);
                foreach (var s in segments)
                {
                    Component c = s.GetByID(id);
                    if (valueString.ToUpper() == "NULL")
                    {
                        if (c != null && string.IsNullOrEmpty(c.Value))
                            returnValue = c;
                    }
                    else if (valueString.ToUpper() == "!NULL")
                    {
                        if (c != null && !string.IsNullOrEmpty(c.Value))
                            returnValue = c;
                    }
                    else
                    {
                        if (c != null && c.Value != null)
                            if (c.Value.ToUpper() == valueString.ToUpper())
                                returnValue = c;
                    }
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Removes patient identifying information from message and replaces it with made up patient data.
        /// </summary>
        /// <param name="m">The message to de-identify.</param>
        /// <returns>Returns the original message without identifying information.</returns>
        public static Message DeIdentify(this Message m)
        {
            var msg = m;

            if (msg.Segments.Get("PID") != null)
            {
                var mrn = Helper.RandomMRN();
                var sex = msg.Segments.Get("PID")[0].Fields[8].Components[0].Value;

                msg.Segments.Get("PID")[0].Fields[2].Components[0].Value = mrn;
                msg.Segments.Get("PID")[0].Fields[3].Components[0].Value = mrn;
                msg.Segments.Get("PID")[0].Fields[4].Components[0].Value = mrn;
                msg.Segments.Get("PID")[0].Fields[5].Components[0].Value = Helper.RandomLastName();
                msg.Segments.Get("PID")[0].Fields[5].Components[1].Value = Helper.RandomFirstName(sex);
                msg.Segments.Get("PID")[0].Fields[6].Components[0].Value = Helper.RandomLastName();
                msg.Segments.Get("PID")[0].Fields[6].Components[1].Value = Helper.RandomFirstName("FEMALE");
                msg.Segments.Get("PID")[0].Fields[9].Components[0].Value = "";
                msg.Segments.Get("PID")[0].Fields[9].Components[1].Value = "";
                msg.Segments.Get("PID")[0].Fields[11].Components[0].Value = Helper.RandomAddress();
                msg.Segments.Get("PID")[0].Fields[13].Components[0].Value = "";
                msg.Segments.Get("PID")[0].Fields[13].Components[11].Value = "";
                msg.Segments.Get("PID")[0].Fields[14].Components[0].Value = "";
                msg.Segments.Get("PID")[0].Fields[14].Components[11].Value = "";
                msg.Segments.Get("PID")[0].Fields[18].Components[0].Value = mrn;
                msg.Segments.Get("PID")[0].Fields[19].Components[0].Value = "999999999";
            }
            return msg;
        }

        /// <summary>
        /// Converts a regular date into an HL7 date string.
        /// </summary>
        /// <param name="d">The date to convert.</param>
        /// <returns>Returns the HL7 date string.</returns>
        public static string ToHl7Date(this DateTime d)
        {
            return d.ToString("yyyyMMddHHmmss");
        }

        /// <summary>
        /// Converts an HL7 date string into a .Net date.
        /// </summary>
        /// <param name="hl7Date">The HL7 date string to convert.</param>
        /// <returns>Returns the date after conversion.</returns>
        public static DateTime? FromHl7Date(this string hl7Date)
        {
            try
            {
                var y = 0;
                var M = 0;
                var d = 0;
                var H = 0;
                var m = 0;
                var s = 0;

                switch (hl7Date.Trim().Length)
                {
                    case 8:
                        y = Convert.ToInt32(hl7Date.Trim().Substring(0, 4));
                        M = Convert.ToInt32(hl7Date.Trim().Substring(4, 2));
                        d = Convert.ToInt32(hl7Date.Trim().Substring(6, 2));
                        break;

                    case 12:
                        y = Convert.ToInt32(hl7Date.Trim().Substring(0, 4));
                        M = Convert.ToInt32(hl7Date.Trim().Substring(4, 2));
                        d = Convert.ToInt32(hl7Date.Trim().Substring(6, 2));
                        H = Convert.ToInt32(hl7Date.Trim().Substring(8, 2));
                        m = Convert.ToInt32(hl7Date.Trim().Substring(10, 2));
                        break;

                    case 14:
                    case 15:
                    case 16:
                    case 17:
                    case 18:
                        y = Convert.ToInt32(hl7Date.Trim().Substring(0, 4));
                        M = Convert.ToInt32(hl7Date.Trim().Substring(4, 2));
                        d = Convert.ToInt32(hl7Date.Trim().Substring(6, 2));
                        H = Convert.ToInt32(hl7Date.Trim().Substring(8, 2));
                        m = Convert.ToInt32(hl7Date.Trim().Substring(10, 2));
                        s = Convert.ToInt32(hl7Date.Trim().Substring(12, 2));
                        break;
                }
                return new DateTime(y, M, d, H, m, s);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the index of the specified field within the field list
        /// </summary>
        /// <param name="fList">The field list to pull</param>
        /// <param name="f">The field to get the index for</param>
        /// <returns>Returns the index of the specified field</returns>
        public static int GetIndex(this List<Field> fList, Field f)
        {
            var i = fList.FindIndex(field => field.Name == f.Name);
            return i;
        }

        /// <summary>
        /// Gets the index of the specified component within the component list
        /// </summary>
        /// <param name="cList">The component list to pull</param>
        /// <param name="c">The component to get the index for</param>
        /// <returns>Returns the index of the specified component</returns>
        public static int GetIndex(this List<Component> cList, Component c)
        {
            var i = cList.FindIndex(component => component.Name == c.Name);
            return i;
        }

        /// <summary>
        /// Unescapes escape sequences in the HL7 segment
        /// </summary>
        /// <param name="msg">The string to unescape</param>
        /// <param name="escapeCharacter">The escape character being used</param>
        /// <returns>The unescaped string</returns>
        public static string UnEscape(this string msg, string escapeCharacter)
        {
            var returnStr = msg;
            returnStr = returnStr.Replace(string.Format("{0}T{0}", escapeCharacter), "&");
            returnStr = returnStr.Replace(string.Format("{0}S{0}", escapeCharacter), "^");
            returnStr = returnStr.Replace(string.Format("{0}F{0}", escapeCharacter), "|");
            returnStr = returnStr.Replace(string.Format("{0}R{0}", escapeCharacter), "~");
            returnStr = returnStr.Replace(string.Format("{0}E{0}", escapeCharacter), "\\");
            return returnStr;
        }

        /// <summary>
        /// Gets the field value.
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        public static string GetFieldValue(this Segment segment, string fieldName)
        {
            StringBuilder sb = new StringBuilder();
            var field = segment.Fields.FirstOrDefault(x => x.Name == fieldName);
            if (field != null)
            {
                foreach (var com in field.Components)
                {
                    sb.Append(com.Value);
                    sb.Append("^");
                }
            }
            return sb.ToString().TrimEnd('^');
        }

        /// <summary>
        /// Gets the name of the field.
        /// </summary>
        /// <param name="fList">The f list.</param>
        /// <param name="fieldIndex">Index of the field.</param>
        /// <returns></returns>
        public static string GetFieldName(this List<Field> fList, int fieldIndex)
        {
            if (fieldIndex < 0 || fieldIndex >= fList.Count)
                throw new ArgumentException("fieldIndex error");
            return fList[fieldIndex].Name;
        }
    }
}