using HL7Lib.Base;
using System;

namespace ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string text = @"MSH|^~\&|||||||ORU^R01|1202|P|2.3.1|"
                + Environment.NewLine + @"OBX||CE|2026^||1101^~1114^II~256^~1^||||||F";
            Message message = new Message(text);
            foreach (var segment in message.Segments)
            {
                Console.WriteLine(segment.ToString());
            }
        }
    }
}