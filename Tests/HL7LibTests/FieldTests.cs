using FluentAssertions;
using HL7Lib.Base;
using HL7Lib.Segments;
using System;
using System.Collections.Generic;
using Xunit;

namespace HL7LibTests
{
    public class FieldTests
    {
        [Fact]
        public void Get_PID_Field_Name()
        {
            PID pid = new PID();
            int index = 3;
            var name = pid.Fields.GetFieldName(index);
            name.Should().Be("Patient Identifier List");
        }

        [Fact]
        public void Get_Field_Name_With_Exception()
        {
            Action action = () =>
            {
                List<Field> fList = new List<Field>();
                var name = fList.GetFieldName(3);
            };
            action.Should().Throw<Exception>();
        }
    }
}