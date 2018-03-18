using System;
using System.Collections.Generic;

namespace RpgMakerEncoder.Ruby
{
    public class RubyArray : RubyToken, IList<RubyToken>
    {
        public override RubyTokenType Type { get; } = RubyTokenType.Array;
        public List<RubyToken> Array { get; set; } = new List<RubyToken>();

        /// <inheritdoc />
        public override RubyToken this[object key]
        {
            get
            {
                if (key is int indexKey)
                {
                    return this[indexKey];
                }
                throw new ArgumentException($"Argument of type {key.GetType().Name} is not a valid index key.");
            }
        }

        /// <inheritdoc />
        public override IEnumerator<RubyToken> GetEnumerator()
        {
            return Array.GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(RubyToken item)
        {
            Array.Add(item);
        }

        /// <inheritdoc />
        public void Clear()
        {
            Array.Clear();
        }

        /// <inheritdoc />
        public bool Contains(RubyToken item)
        {
            return Array.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(RubyToken[] array, int arrayIndex)
        {
            array.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(RubyToken item)
        {
            return Array.Remove(item);
        }

        /// <inheritdoc />
        public int Count => Array.Count;
        /// <inheritdoc />
        public bool IsReadOnly => ((IList<RubyToken>)Array).IsReadOnly;

        /// <inheritdoc />
        public int IndexOf(RubyToken item)
        {
            return Array.IndexOf(item);
        }

        /// <inheritdoc />
        public void Insert(int index, RubyToken item)
        {
            Array.Insert(index, item);
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            Array.RemoveAt(index);
        }

        /// <inheritdoc />
        public RubyToken this[int index]
        {
            get => Array[index];
            set => Array[index] = value;
        }
    }
}