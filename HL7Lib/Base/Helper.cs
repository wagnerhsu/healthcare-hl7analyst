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

using HL7Lib.Properties;
using System;
using System.Collections.Generic;

#endregion

namespace HL7Lib.Base
{
    /// <summary>
    /// Helper Class: Used to setup helper objects
    /// </summary>
    public class Helper
    {
        /// <summary>
        /// Sets a random first name for the PID segment, used to de-identify the HL7 message
        /// </summary>
        /// <param name="sex">The sex in the PID segment</param>
        /// <returns>Returns a randomly selected name from a list of names based on the sex of the patient</returns>
        public static string RandomFirstName(string sex)
        {
            var rand = new Random();
            if (!string.IsNullOrEmpty(sex))
            {
                if (sex.ToUpper() == "MALE" || sex.ToUpper() == "M")
                {
                    var names = Resources.Boy_Names.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    return names.GetValue(rand.Next(names.Length)).ToString();
                }
                if (sex.ToUpper() == "FEMALE" || sex.ToUpper() == "F")
                {
                    var names = Resources.Girl_Names.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    return names.GetValue(rand.Next(names.Length)).ToString();
                }
                else
                {
                    var names = Resources.Boy_Names.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    return names.GetValue(rand.Next(names.Length)).ToString();
                }
            }
            var _names = Resources.Boy_Names.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            return _names.GetValue(rand.Next(_names.Length)).ToString();
        }

        /// <summary>
        /// Sets a random last name for the PID segment, used to de-identify the HL7 message.
        /// </summary>
        /// <returns>Returns a randomly selected name from a list of names</returns>
        public static string RandomLastName()
        {
            var names = Resources.Last_Names.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var rand = new Random();
            return names.GetValue(rand.Next(names.Length)).ToString();
        }

        /// <summary>
        /// Sets a random street address, used to de-identify the HL7 message.
        /// </summary>
        /// <returns>Returns a randomly selected street address</returns>
        public static string RandomAddress()
        {
            var houseRand = new Random();
            var addressRand = new Random();
            var streetRand = new Random();
            var house = houseRand.Next(99999);
            var addresses = Resources.Address_Names.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            string[] streets = { "Street", "Drive", "Avenue", "Lane", "Way" };
            var streetAddress = string.Format("{0} {1} {2}", house.ToString().Trim(),
                addresses.GetValue(addressRand.Next(addresses.Length)).ToString().Trim(),
                streets.GetValue(streetRand.Next(streets.Length)).ToString().Trim());
            return streetAddress;
        }

        /// <summary>
        /// Sets a random MRN for the patient, used to de-identify the HL7 message
        /// </summary>
        /// <returns>Returns a randomly selected MRN</returns>
        public static string RandomMRN()
        {
            var rand = new Random();
            return rand.Next(99999999).ToString();
        }

        /// <summary>
        /// Creates an ACK from the MSH segment of the associated message
        /// </summary>
        /// <param name="inboundMessage">The inbound message</param>
        /// <returns>The ack</returns>
        public static Message CreateAck(Message inboundMessage)
        {
            if (inboundMessage.Segments.Get("MSH").Count == 1)
            {
                var msh = inboundMessage.Segments.Get("MSH")[0];
                var args = new List<object>();
                args.Add(msh.GetByID("MSH-3.1").Value); //String.Format Arg 0
                args.Add(msh.GetByID("MSH-4.1").Value); //String.Format Arg 1
                args.Add(msh.GetByID("MSH-5.1").Value); //String.Format Arg 2
                args.Add(msh.GetByID("MSH-6.1").Value); //String.Format Arg 3
                args.Add(msh.GetByID("MSH-7.1").Value); //String.Format Arg 4
                args.Add(msh.GetByID("MSH-9.2").Value); //String.Format Arg 5
                args.Add(msh.GetByID("MSH-10.1").Value); //String.Format Arg 6
                args.Add(msh.GetByID("MSH-11.1").Value); //String.Format Arg 7
                args.Add(msh.GetByID("MSH-12.1").Value); //String.Format Arg 8
                var msg = string.Format("MSH|^~\\&|{0}|{1}|{2}|{3}|{4}||ACK^{5}|{6}|{7}|{8}\r\nMSA|AA|{6}",
                    args.ToArray());
                var m = new Message(msg);
                return m;
            }
            throw new Exception("No MSH Segment Present in Inbound Message.");
        }

        /// <summary>
        /// Validates the ack against the message
        /// </summary>
        /// <param name="outboundMessage">The message to use</param>
        /// <param name="ackMessage">The ack to use</param>
        /// <returns></returns>
        public static bool ValidateAck(Message outboundMessage, Message ackMessage)
        {
            var returnValue = false;
            if (outboundMessage.Segments.Get("MSH").Count == 1 && ackMessage.Segments.Get("MSA").Count == 1)
            {
                var msh = outboundMessage.Segments.Get("MSH")[0];
                var msa = ackMessage.Segments.Get("MSA")[0];

                if (msh.GetByID("MSH-10.1").Value == msa.GetByID("MSA-2.1").Value &&
                    msa.GetByID("MSA-1.1").Value == "AA")
                    returnValue = true;
            }
            return returnValue;
        }
    }
}