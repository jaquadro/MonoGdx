/**
 * Copyright 2013 See AUTHORS file.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Text;

namespace MonoGdx.Utils
{
    public abstract class CharSequence : IEquatable<string>, IEquatable<StringBuilder>, IEquatable<char[]>
    {
        public abstract char this[int index] { get; }
        public abstract int Length { get; }

        public override bool Equals (object obj)
        {
            CharSequence other = obj as CharSequence;
            if (other == null)
                return false;

            int length = Length;
            if (length != other.Length)
                return false;
            for (int i = 0; i < length; i++) {
                if (this[i] != other[i])
                    return false;
            }

            return true;
        }

        public bool Equals (string other)
        {
            int length = Length;
            if (length != other.Length)
                return false;
            for (int i = 0; i < length; i++) {
                if (this[i] != other[i])
                    return false;
            }

            return true;
        }

        public bool Equals (StringBuilder other)
        {
            int length = Length;
            if (length != other.Length)
                return false;
            for (int i = 0; i < length; i++) {
                if (this[i] != other[i])
                    return false;
            }

            return true;
        }

        public bool Equals (char[] other)
        {
            int length = Length;
            if (length != other.Length)
                return false;
            for (int i = 0; i < length; i++) {
                if (this[i] != other[i])
                    return false;
            }

            return true;
        }
    }

    public class StringSequence : CharSequence
    {
        public StringSequence (string value)
        {
            Value = value;
        }

        public string Value { get; set; }

        public override int Length
        {
            get { return Value.Length; }
        }

        public override char this[int index]
        {
            get { return Value[index]; }
        }

        public override string ToString ()
        {
            return Value;
        }
    }

    public class StringBuilderSequence : CharSequence
    {
        public StringBuilderSequence (StringBuilder value)
        {
            Value = value;
        }

        public StringBuilder Value { get; set; }

        public override int Length
        {
            get { return Value.Length; }
        }

        public override char this[int index]
        {
            get { return Value[index]; }
        }

        public override string ToString ()
        {
            return Value.ToString();
        }
    }

    public class CharArraySequence : CharSequence
    {
        public CharArraySequence (char[] value)
        {
            Value = value;
        }

        public char[] Value { get; set; }

        public override int Length
        {
            get { return Value.Length; }
        }

        public override char this[int index]
        {
            get { return Value[index]; }
        }

        public override string ToString ()
        {
            return Value.ToString();
        }
    }
}
