using System;
using System.Collections.Generic;
using System.Text;

namespace Akade.IndexedSet.InternalSourceGenerator;

public class IntendedStringBuilder
{
    private readonly StringBuilder _builder = new();
    private bool _indentationWrittenForCurrentLine = false;
    private int _indentationLevel = 0;

    public override string ToString()
    {
        return _builder.ToString();
    }

    public IntendedStringBuilder AppendLine(string value = "")
    {
        AppendIndentation();
        _ = _builder.AppendLine(value);
        _indentationWrittenForCurrentLine = false;

        return this;
    }

    public IntendedStringBuilder Append(string value)
    {
        AppendIndentation();
        _ = _builder.Append(value);

        return this;
    }
    
    public IntendedStringBuilder AppendJoin(string delimeter, IEnumerable<string> values)
    {
        bool first = true;

        foreach (string value in values)
        {
            if (!first)
            {
                _ = _builder.Append(delimeter);
            }
            first = false;

            _ = _builder.Append(value);
        }

        return this;
    }

    private void AppendIndentation()
    {
        if (!_indentationWrittenForCurrentLine)
        {
            _ = _builder.Append(' ', _indentationLevel * 4);
            _indentationWrittenForCurrentLine = true;
        }
    }

    private void Indent()
    {
        _indentationLevel++;
    }

    private void Unindent()
    {
        _indentationLevel--;
    }

    public IDisposable StartCodeBlock()
    {
        return new CodeBlock(this);
    }

    private class CodeBlock : IDisposable
    {
        private readonly IntendedStringBuilder _intendedStringBuilder;

        public CodeBlock(IntendedStringBuilder intendedStringBuilder)
        {
            _intendedStringBuilder = intendedStringBuilder;
            _ = _intendedStringBuilder.AppendLine("{");
            _intendedStringBuilder.Indent();
        }

        public void Dispose()
        {
            _intendedStringBuilder.Unindent();
            _ = _intendedStringBuilder.AppendLine("}");
        }
    }
}
