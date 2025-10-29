using System;

namespace ArabicCompiler
{
    // استثناء خاص بالمحلل اللغوي
    public class LexerException : Exception
    {
        public int Line { get; }
        public int Column { get; }

        public LexerException(string message, int line, int column)
            : base($"Lexical Error at Line {line}, Column {column}: {message}")
        {
            Line = line;
            Column = column;
        }
    }
}
