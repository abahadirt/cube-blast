using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Blast.Bot.Metrics
{
    /// <summary>
    /// Writes CSV rows with invariant formatting and standard escaping.
    /// </summary>
    public sealed class CsvLineWriter : IDisposable
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        private readonly TextWriter _writer;
        private readonly StringBuilder _row = new StringBuilder(256);
        private bool _rowEmpty = true;

        public CsvLineWriter(TextWriter writer)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        private void Separate()
        {
            if (!_rowEmpty) _row.Append(',');
            _rowEmpty = false;
        }

        public CsvLineWriter Field(int value) { Separate(); _row.Append(value.ToString(Inv)); return this; }
        public CsvLineWriter Field(long value) { Separate(); _row.Append(value.ToString(Inv)); return this; }
        public CsvLineWriter Field(float value) { Separate(); _row.Append(value.ToString(Inv)); return this; }
        public CsvLineWriter Field(bool value) { Separate(); _row.Append(value ? "true" : "false"); return this; }
        public CsvLineWriter Field(string value) { Separate(); AppendEscaped(value); return this; }

        /// <summary>Writes an empty field.</summary>
        public CsvLineWriter Empty() { Separate(); return this; }

        public void EndRow()
        {
            _row.Append('\n');
            _writer.Write(_row.ToString());
            _row.Clear();
            _rowEmpty = true;
        }

        /// <summary>Writes a line without escaping.</summary>
        public void RawLine(string line)
        {
            _writer.Write(line);
            _writer.Write('\n');
        }

        public void Flush() => _writer.Flush();

        public void Dispose()
        {
            _writer.Flush();
            _writer.Dispose();
        }

        private void AppendEscaped(string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            bool needsQuote = value.IndexOf(',') >= 0 || value.IndexOf('"') >= 0
                              || value.IndexOf('\n') >= 0 || value.IndexOf('\r') >= 0;
            if (!needsQuote) { _row.Append(value); return; }

            _row.Append('"');
            foreach (char c in value)
            {
                if (c == '"') _row.Append('"'); // RFC-4180: double the quote
                _row.Append(c);
            }
            _row.Append('"');
        }
    }
}