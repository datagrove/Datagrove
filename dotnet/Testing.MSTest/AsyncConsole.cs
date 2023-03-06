using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

public class AsyncLocalConsoleWriter : StringWriter
    {
        private AsyncLocalConsoleWriter() { }

        public static AsyncLocalConsoleWriter Instance = new AsyncLocalConsoleWriter();
        public static List<StringBuilder> AdditionalOutputs { get; } = new List<StringBuilder>();
        public static AsyncLocal<StringBuilder> State { get; } = new AsyncLocal<StringBuilder>();

        public static StringBuilder AllOutput = new StringBuilder();

        public override Encoding Encoding => throw new NotImplementedException();

        public override void WriteLine(string value)
        {
            AllOutput.AppendLine(value);
            if (State?.Value == null)
            {
                var sb = new StringBuilder();
                AdditionalOutputs.Add(sb);
                State.Value = sb;
            }

            State.Value.AppendLine(value);
        }
    }

