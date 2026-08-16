using System;
using System.IO;

namespace CkMp.Data.Compression
{
    /// <summary>
    /// EA "RefPack" codec (LZ77 variant), the compression used by C&amp;C Generals for .map files.
    ///
    /// The bitstream layout is taken from the reference decoder in
    /// GeneralsGameCode/Core/Libraries/Source/Compression (CompressionManager.cpp / RefPack).
    ///
    /// This class handles the bare RefPack stream only (starting with the 0x10FB signature).
    /// The surrounding "EAR\0" container used by the game is handled by <see cref="MapCompression"/>.
    /// </summary>
    public static class Refpack
    {
        private const int MinMatch = 3;
        private const int MaxMatch = 1028;

        // Largest back reference the long command can express: 17 bit field, stored as offset - 1.
        private const int MaxOffset = 1 << 17;

        // Largest back reference the short and medium commands can express.
        private const int ShortMaxOffset = 1 << 10;
        private const int MediumMaxOffset = 1 << 14;

        // Literal-only command copies a multiple of four bytes, at most 112 at a time.
        private const int MaxLiteralBlock = 112;

        #region decoding

        /// <summary>
        /// True if the data at <paramref name="index"/> starts with a RefPack signature.
        /// </summary>
        public static bool IsRefpackStream(byte[] src, int index, int count)
        {
            if (src == null || count < 2 || index + 1 >= src.Length)
                return false;

            // The low byte is always 0xFB, the high byte carries the flags.
            return src[index + 1] == 0xFB && (src[index] & 0x3F) == 0x10;
        }

        /// <summary>
        /// Reads the uncompressed size from the RefPack header without decoding the stream.
        /// </summary>
        public static int GetDecodedSize(byte[] src, int index)
        {
            int headerSize;
            return ReadHeader(src, index, out headerSize);
        }

        /// <summary>
        /// Decodes a RefPack stream into a new array.
        /// </summary>
        public static byte[] Decode(byte[] src, int index, int count)
        {
            int consumed;
            return Decode(src, index, count, out consumed);
        }

        /// <summary>
        /// Decodes a RefPack stream into a new array and reports how many source bytes were used.
        /// </summary>
        public static byte[] Decode(byte[] src, int index, int count, out int bytesRead)
        {
            if (src == null)
                throw new ArgumentNullException("src");
            if (index < 0 || count < 0 || index + count > src.Length)
                throw new ArgumentOutOfRangeException("count");

            int headerSize;
            int decodedSize = ReadHeader(src, index, out headerSize);

            byte[] dst = new byte[decodedSize];
            int getp = index + headerSize;
            int end = index + count;
            int putp = 0;

            while (true)
            {
                if (getp >= end)
                    throw new InvalidDataException("Truncated RefPack stream (no end marker).");

                byte first = src[getp++];
                int run;
                int reference;

                if ((first & 0x80) == 0)
                {
                    // Short command: 2 bytes, offset <= 1024, length 3..10.
                    byte second = src[getp++];

                    run = first & 3;
                    while (run-- > 0)
                        dst[putp++] = src[getp++];

                    reference = putp - 1 - (((first & 0x60) << 3) + second);
                    run = ((first & 0x1C) >> 2) + MinMatch;
                }
                else if ((first & 0x40) == 0)
                {
                    // Medium command: 3 bytes, offset <= 16384, length 4..67.
                    byte second = src[getp++];
                    byte third = src[getp++];

                    run = second >> 6;
                    while (run-- > 0)
                        dst[putp++] = src[getp++];

                    reference = putp - 1 - (((second & 0x3F) << 8) + third);
                    run = (first & 0x3F) + 4;
                }
                else if ((first & 0x20) == 0)
                {
                    // Long command: 4 bytes, offset <= 131072, length 5..1028.
                    byte second = src[getp++];
                    byte third = src[getp++];
                    byte forth = src[getp++];

                    run = first & 3;
                    while (run-- > 0)
                        dst[putp++] = src[getp++];

                    reference = putp - 1 - ((((first & 0x10) >> 4) << 16) + (second << 8) + third);
                    run = (((first & 0x0C) >> 2) << 8) + forth + 5;
                }
                else
                {
                    // Literal command: copies (first & 0x1F) * 4 + 4 bytes verbatim.
                    run = ((first & 0x1F) << 2) + 4;

                    if (run <= MaxLiteralBlock)
                    {
                        while (run-- > 0)
                            dst[putp++] = src[getp++];
                        continue;
                    }

                    // End marker, optionally followed by up to 3 trailing literals.
                    run = first & 3;
                    while (run-- > 0)
                        dst[putp++] = src[getp++];

                    break;
                }

                if (reference < 0)
                    throw new InvalidDataException("Invalid RefPack back reference.");

                // Copied byte by byte on purpose: source and destination ranges may overlap.
                while (run-- > 0)
                    dst[putp++] = dst[reference++];
            }

            if (putp != decodedSize)
                throw new InvalidDataException(
                    String.Format("RefPack size mismatch: header says {0} bytes, decoded {1}.", decodedSize, putp));

            bytesRead = getp - index;
            return dst;
        }

        private static int ReadHeader(byte[] src, int index, out int headerSize)
        {
            if (src == null)
                throw new ArgumentNullException("src");
            if (index + 2 > src.Length)
                throw new InvalidDataException("Truncated RefPack header.");

            int flags = (src[index] << 8) | src[index + 1];
            int p = index + 2;

            int decodedSize;
            if ((flags & 0x8000) != 0)
            {
                // Large-file variant: sizes are stored as 4 bytes.
                if ((flags & 0x0100) != 0)
                    p += 6; // skip the optional compressed size field

                decodedSize = (src[p] << 24) | (src[p + 1] << 16) | (src[p + 2] << 8) | src[p + 3];
                p += 4;
            }
            else
            {
                if ((flags & 0x0100) != 0)
                    p += 5; // skip the optional compressed size field

                decodedSize = (src[p] << 16) | (src[p + 1] << 8) | src[p + 2];
                p += 3;
            }

            if (decodedSize < 0)
                throw new InvalidDataException("Invalid RefPack decoded size.");

            headerSize = p - index;
            return decodedSize;
        }

        #endregion

        #region encoding

        /// <summary>
        /// Compresses <paramref name="src"/> into a RefPack stream.
        /// </summary>
        /// <param name="effort">
        /// How many candidates per hash bucket the match finder inspects. Higher is smaller but slower.
        /// </param>
        public static byte[] Encode(byte[] src, int effort = 64)
        {
            if (src == null)
                throw new ArgumentNullException("src");

            return new Encoder(src, effort).Run();
        }

        private sealed class Encoder
        {
            private const int HashBits = 16;
            private const int HashSize = 1 << HashBits;

            private readonly byte[] src;
            private readonly int length;
            private readonly int maxChain;

            private readonly int[] head;
            private readonly int[] chain;

            private byte[] dst;
            private int dstLength;

            public Encoder(byte[] source, int effort)
            {
                src = source;
                length = source.Length;
                maxChain = effort < 1 ? 1 : effort;

                head = new int[HashSize];
                for (int i = 0; i < HashSize; i++)
                    head[i] = -1;

                chain = new int[length < 1 ? 1 : length];

                // Worst case for incompressible input is one command byte per 112 literals.
                dst = new byte[length + (length / MaxLiteralBlock) + 32];
            }

            public byte[] Run()
            {
                WriteHeader();

                int pos = 0;
                int literalStart = 0;

                bool havePending = false;
                int pendingLength = 0;
                int pendingOffset = 0;

                while (pos < length)
                {
                    int matchLength;
                    int matchOffset;

                    if (havePending)
                    {
                        matchLength = pendingLength;
                        matchOffset = pendingOffset;
                        havePending = false;
                    }
                    else
                    {
                        FindMatch(pos, out matchLength, out matchOffset);
                    }

                    if (matchLength < MinMatch)
                    {
                        Insert(pos);
                        pos++;
                        continue;
                    }

                    if (pos + 1 < length && matchLength < MaxMatch)
                    {
                        // Lazy matching: a longer match one byte later beats emitting this one now.
                        Insert(pos);

                        int nextLength;
                        int nextOffset;
                        FindMatch(pos + 1, out nextLength, out nextOffset);

                        if (nextLength > matchLength)
                        {
                            pendingLength = nextLength;
                            pendingOffset = nextOffset;
                            havePending = true;
                            pos++;
                            continue;
                        }

                        EmitMatch(literalStart, pos, matchLength, matchOffset);
                        InsertRange(pos + 1, pos + matchLength);
                    }
                    else
                    {
                        EmitMatch(literalStart, pos, matchLength, matchOffset);
                        InsertRange(pos, pos + matchLength);
                    }

                    pos += matchLength;
                    literalStart = pos;
                }

                EmitEnd(literalStart, length);

                byte[] result = new byte[dstLength];
                Buffer.BlockCopy(dst, 0, result, 0, dstLength);
                return result;
            }

            #region match finder

            private int Hash(int pos)
            {
                uint v = (uint)(src[pos] | (src[pos + 1] << 8) | (src[pos + 2] << 16));
                return (int)((v * 2654435761u) >> (32 - HashBits));
            }

            private void Insert(int pos)
            {
                if (pos + MinMatch > length)
                    return;

                int h = Hash(pos);
                chain[pos] = head[h];
                head[h] = pos;
            }

            private void InsertRange(int from, int to)
            {
                for (int i = from; i < to; i++)
                    Insert(i);
            }

            private static bool IsEncodable(int matchLength, int offset)
            {
                if (matchLength >= 5)
                    return offset <= MaxOffset;
                if (matchLength == 4)
                    return offset <= MediumMaxOffset;
                return offset <= ShortMaxOffset;
            }

            private void FindMatch(int pos, out int bestLength, out int bestOffset)
            {
                bestLength = 0;
                bestOffset = 0;

                int maxLength = length - pos;
                if (maxLength > MaxMatch)
                    maxLength = MaxMatch;
                if (maxLength < MinMatch)
                    return;

                int limit = pos - MaxOffset;
                if (limit < 0)
                    limit = 0;

                int candidate = head[Hash(pos)];
                int tries = maxChain;

                while (candidate >= limit && tries-- > 0)
                {
                    // Cheap rejection: a longer match must at least match at the current best length.
                    if (bestLength == 0 || bestLength >= maxLength ||
                        src[candidate + bestLength] == src[pos + bestLength])
                    {
                        int len = 0;
                        while (len < maxLength && src[candidate + len] == src[pos + len])
                            len++;

                        int offset = pos - candidate;
                        if (len > bestLength && len >= MinMatch && IsEncodable(len, offset))
                        {
                            bestLength = len;
                            bestOffset = offset;

                            if (len >= maxLength)
                                break;
                        }
                    }

                    candidate = chain[candidate];
                }
            }

            #endregion

            #region bitstream output

            private void Ensure(int extra)
            {
                if (dstLength + extra <= dst.Length)
                    return;

                int capacity = dst.Length * 2;
                while (capacity < dstLength + extra)
                    capacity *= 2;

                byte[] grown = new byte[capacity];
                Buffer.BlockCopy(dst, 0, grown, 0, dstLength);
                dst = grown;
            }

            private void Put(byte value)
            {
                Ensure(1);
                dst[dstLength++] = value;
            }

            private void PutLiterals(int from, int count)
            {
                if (count <= 0)
                    return;

                Ensure(count);
                Buffer.BlockCopy(src, from, dst, dstLength, count);
                dstLength += count;
            }

            private void WriteHeader()
            {
                if (length <= 0xFFFFFF)
                {
                    Put(0x10);
                    Put(0xFB);
                    Put((byte)(length >> 16));
                    Put((byte)(length >> 8));
                    Put((byte)length);
                }
                else
                {
                    // Large-file variant, size stored as 4 bytes.
                    Put(0x90);
                    Put(0xFB);
                    Put((byte)(length >> 24));
                    Put((byte)(length >> 16));
                    Put((byte)(length >> 8));
                    Put((byte)length);
                }
            }

            /// <summary>
            /// Emits whole literal blocks until at most 3 literals are left, which a copy command
            /// or the end marker can carry inline. Returns the start of the remaining literals.
            /// </summary>
            private int FlushLiteralBlocks(int from, int count)
            {
                while (count >= 4)
                {
                    int block = count & ~3;
                    if (block > MaxLiteralBlock)
                        block = MaxLiteralBlock;

                    Put((byte)(0xE0 | ((block - 4) >> 2)));
                    PutLiterals(from, block);

                    from += block;
                    count -= block;
                }

                return from;
            }

            private void EmitMatch(int literalStart, int pos, int matchLength, int matchOffset)
            {
                int literalFrom = FlushLiteralBlocks(literalStart, pos - literalStart);
                int literalCount = pos - literalFrom;

                int offset = matchOffset - 1;

                if (matchLength <= 10 && offset < ShortMaxOffset)
                {
                    Put((byte)(((offset >> 8) << 5) | ((matchLength - MinMatch) << 2) | literalCount));
                    Put((byte)offset);
                }
                else if (matchLength <= 67 && offset < MediumMaxOffset)
                {
                    Put((byte)(0x80 | (matchLength - 4)));
                    Put((byte)((literalCount << 6) | (offset >> 8)));
                    Put((byte)offset);
                }
                else
                {
                    Put((byte)(0xC0 | ((offset >> 16) << 4) | (((matchLength - 5) >> 8) << 2) | literalCount));
                    Put((byte)(offset >> 8));
                    Put((byte)offset);
                    Put((byte)(matchLength - 5));
                }

                PutLiterals(literalFrom, literalCount);
            }

            private void EmitEnd(int literalStart, int end)
            {
                int literalFrom = FlushLiteralBlocks(literalStart, end - literalStart);
                int literalCount = end - literalFrom;

                Put((byte)(0xFC | literalCount));
                PutLiterals(literalFrom, literalCount);
            }

            #endregion
        }

        #endregion
    }
}
