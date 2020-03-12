using FluentAssertions;
using HL7Lib.Base;
using HL7Lib.Segments;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace HL7LibTests
{
    public class SegmentTests
    {
        [Fact]
        public void GetById_Test()
        {
            string msg = "PID|||98765^^^IHE^PI||Doe^John^^^^^L||19660101000000-0600|M";
            Message message = new Message(msg);
            var segment = message.Segments.First(s => s.Name == nameof(PID));
            var component = segment.GetByID("PID-3.1");
            component.IDParts.FieldIndex.Should().Be(3);
            component.IDParts.ComponentIndex.Should().Be(0);
            component.Value.Should().Be("98765");
        }

        [Fact]
        public void Construct_Segment_Manually()
        {
            Message message = new Message();
            Segment segment = new Segment(Segments.PID);
            message.Segments = new List<Segment>();
            message.Segments.Add(segment);
        }

        [Fact]
        public void GetFieldValue()
        {
            Segment segment = new Segment(Segments.PID);
            segment.GetByID("PID-3.1").Value = "98765";
            segment.GetByID("PID-3.4").Value = "IHE";
            segment.GetByID("PID-3.5").Value = "PI";
            var value = segment.GetFieldValue("Patient Identifier List");
            value.Should().Be("98765^^^IHE^PI");

        }
    }
}