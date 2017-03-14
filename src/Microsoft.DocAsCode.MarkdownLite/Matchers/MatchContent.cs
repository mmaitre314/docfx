﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DocAsCode.MarkdownLite.Matchers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public struct MatchContent
    {
        public readonly string Text;
        public readonly int StartIndex;
        public readonly MatchDirection Direction;
        private readonly Dictionary<string, KeyValuePair<int, int>> _group;

        public MatchContent(string text, int startIndex, MatchDirection direction = MatchDirection.Forward)
            : this(text, startIndex, direction, new Dictionary<string, KeyValuePair<int, int>>()) { }

        private MatchContent(string text, int startIndex, MatchDirection direction, Dictionary<string, KeyValuePair<int, int>> group)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            if (startIndex < 0 || startIndex > text.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), "Out of range.");
            }
            Text = text;
            StartIndex = startIndex;
            Direction = direction;
            _group = group;
        }

        public char GetCurrentChar() => this[0];

        public char this[int offset] => Text[GetCharIndex(offset)];

        public bool BeginOfString() => Direction == MatchDirection.Forward ? StartIndex == 0 : StartIndex == Text.Length;

        public bool EndOfString() => Direction == MatchDirection.Forward ? StartIndex == Text.Length : StartIndex == 0;

        public int Length => Direction == MatchDirection.Forward ? Text.Length - StartIndex : StartIndex;

        public MatchContent Offset(int offset) => new MatchContent(Text, GetIndex(offset), Direction, _group);

        public MatchContent Reverse() => new MatchContent(Text, StartIndex, Direction ^ MatchDirection.Backward, _group);

        public int CountUntil(char ch)
        {
            if (EndOfString())
            {
                return 0;
            }
            int index;
            if (Direction == MatchDirection.Forward)
            {
                index = Text.IndexOf(ch, StartIndex);
                if (index == -1)
                {
                    return Length;
                }
                return index - StartIndex;
            }
            else
            {
                index = Text.LastIndexOf(ch, StartIndex - 1);
                if (index == -1)
                {
                    return Length;
                }
                return StartIndex - 1 - index;
            }
        }

        public int CountUntilAny(char[] ch)
        {
            if (ch == null)
            {
                throw new ArgumentNullException(nameof(ch));
            }
            if (EndOfString())
            {
                return 0;
            }
            int index;
            if (Direction == MatchDirection.Forward)
            {
                index = Text.IndexOfAny(ch, StartIndex);
                if (index == -1)
                {
                    return Length;
                }
                return index - StartIndex;
            }
            else
            {
                index = Text.LastIndexOfAny(ch, StartIndex - 1);
                if (index == -1)
                {
                    return Length;
                }
                return StartIndex - 1 - index;
            }
        }

        public int CountWhile(char ch)
        {
            if (EndOfString())
            {
                return 0;
            }
            if (Direction == MatchDirection.Forward)
            {
                for (int i = StartIndex; i < Text.Length; i++)
                {
                    if (Text[i] != ch)
                    {
                        return i - StartIndex;
                    }
                }
                return Length;
            }
            else
            {
                for (int i = StartIndex - 1; i >= 0; i--)
                {
                    if (Text[i] != ch)
                    {
                        return StartIndex - 1 - i;
                    }
                }
                return Length;
            }
        }

        public int CountWhileAny(char[] ch)
        {
            if (EndOfString())
            {
                return 0;
            }
            if (Direction == MatchDirection.Forward)
            {
                for (int i = StartIndex; i < Text.Length; i++)
                {
                    if (Array.IndexOf(ch, Text[i]) == -1)
                    {
                        return i - StartIndex;
                    }
                }
                return Length;
            }
            else
            {
                for (int i = StartIndex - 1; i >= 0; i--)
                {
                    if (Array.IndexOf(ch, Text[i]) == -1)
                    {
                        return StartIndex - 1 - i;
                    }
                }
                return Length;
            }
        }

        public void AddGroup(string name, int startIndex, int count)
        {
            if (Direction == MatchDirection.Forward)
            {
                _group[name] = new KeyValuePair<int, int>(startIndex, count);
            }
            else
            {
                _group[name] = new KeyValuePair<int, int>(startIndex - count, count);
            }
        }

        public MatchGroup? GetGroup(string name)
        {
            KeyValuePair<int, int> pair;
            if (!_group.TryGetValue(name, out pair))
            {
                return null;
            }
            return new MatchGroup(name, Text, pair.Key, pair.Value);
        }

        public IEnumerable<MatchGroup> EnumerateGroups()
        {
            foreach (var pair in _group)
            {
                yield return new MatchGroup(pair.Key, Text, pair.Value.Key, pair.Value.Value);
            }
        }

        private int GetIndex(int offset)
        {
            int result = GetIndexNoThrow(offset);
            if (result < 0 || result > Text.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            return result;
        }

        private int GetCharIndex(int offset)
        {
            int result = GetIndexNoThrow(offset);
            if (Direction == MatchDirection.Backward)
            {
                result--;
            }
            if (result < 0 || result >= Text.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            return result;
        }

        private int GetIndexNoThrow(int offset) =>
            Direction == MatchDirection.Forward ? StartIndex + offset : StartIndex - offset;
    }
}
