using Microsoft.Dynamics.Nav.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace SynchronizerTests.Utilities
{
    internal class StringContentManager
    {
        public string Original { get; }
        public string Current { get; private set; }

        private List<TextSpan> _removedSpans = new List<TextSpan>();
        private List<TextSpan> _addedSpans = new List<TextSpan>();

        public StringContentManager(string content)
        {
            Original = content;
            Current = content;
        }

        public bool HasBeenRemoved(TextSpan span)
        {
            return _removedSpans.Any(x => x.Start <= span.Start && x.End >= span.End);
        }

        public void RemoveSpan(TextSpan span)
        {
            if(HasBeenRemoved(span))
            {
                return;
            }
            var start = ComputePosition(span.Start);
            var end = ComputePosition(span.End);

            Current = Current.Substring(0, start) + Current.Substring(end);
            _removedSpans.Add(span);
        }

        public void InsertText(int pos, string text)
        {
            var newPos = ComputePosition(pos);
            Current = Current.Insert(newPos, text);
            _addedSpans.Add(new TextSpan(pos, text.Length));
        }

        private int ComputePosition(int pos)
        {
            var newPos = pos;
            foreach (var removedSpan in _removedSpans)
            {
                if (removedSpan.Start < pos)
                {
                    newPos -= removedSpan.Length;
                }
            }
            foreach (var addedSpan in _addedSpans)
            {
                if (addedSpan.Start < pos)
                {
                    newPos += addedSpan.Length;
                }
            }
            return newPos;
        }
    }
}
