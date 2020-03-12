using FluentAssertions;
using HL7Lib.Base;
using Xunit;

namespace HL7LibTests
{
    public class ComponentTests
    {
        [Fact]
        public void Create_Component()
        {
            Component com = new Component();
            com.ID = "PID-0.1";
            com.Value = "PID";
            com.IDParts.FieldIndex.Should().Be(0);
            com.IDParts.ComponentIndex.Should().Be(0);
        }
    }
}