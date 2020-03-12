using FluentAssertions;
using HL7Lib.Base;
using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace HL7LibTests
{
    public class MessageTests
    {
        [Fact]
        public void Parse_HL7_Message()
        {
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string hl7MessageFileName = Path.Combine(location, "HL7Message-CommunicateInfusionOrder-01.txt");
            Message message = new Message(FileUtilities.ReadAllLinesWithoutComments(hl7MessageFileName));
            message.Segments.Should().HaveCount(6);
        }

        [Fact]
        public void Create_Message_By_Template()
        {
            Message message = new Message(@"MSH|^~\&|" + Environment.NewLine + "PID|");
            var mshSegment = message.Segments[0];
            mshSegment.GetByID("MSH-3.1").Value = "IOPVENDOR";
            var v = mshSegment.GetFieldValue("Sending Application");
            v.Should().Be("IOPVENDOR");
        }
    }
}