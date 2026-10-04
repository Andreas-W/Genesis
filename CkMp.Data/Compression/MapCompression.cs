using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace CkMp.Data.Compression
{
    /// <summary>
    /// Compression formats the C&amp;C Generals engine recognises for .map files.
    /// See CompressionManager.cpp in the Generals source.
    /// </summary>
    public enum MapCompressionType
    {
        None = 0,
        RefPack,
        NoxLzh,
        ZLib1,
        ZLib2,
        ZLib3,
        ZLib4,
        ZLib5,
        ZLib6,
        ZLib7,
        ZLib8,
        ZLib9,
        BTree,
        Huffman
    }

    /// <summary>
    /// Handles the 8 byte container the game wraps around compressed map data:
    /// a four character format tag followed by the uncompressed size as a little endian Int32.
    /// </summary>
    public static class MapCompression
    {
        public const int HeaderSize = 8;

        private static readonly byte[] RefPackTag = Tag("EAR\0");
        private static readonly byte[] NoxTag = Tag("NOX\0");
        private static readonly byte[] BTreeTag = Tag("EAB\0");
        private static readonly byte[] HuffmanTag = Tag("EAH\0");

        private static byte[] Tag(String value)
        {
            return Encoding.ASCII.GetBytes(value);
        }

        #region detection

        /// <summary>
        /// Identifies the format from the first <see cref="HeaderSize"/> bytes of a file.
        /// </summary>
        public static MapCompressionType GetCompressionType(byte[] header, int index = 0)
        {
            if (header == null || index + HeaderSize > header.Length)
                return MapCompressionType.None;

            if (Matches(header, index, RefPackTag))
                return MapCompressionType.RefPack;
            if (Matches(header, index, NoxTag))
                return MapCompressionType.NoxLzh;
            if (Matches(header, index, BTreeTag))
                return MapCompressionType.BTree;
            if (Matches(header, index, HuffmanTag))
                return MapCompressionType.Huffman;

            // "ZL1".."ZL9"
            if (header[index] == (byte)'Z' && header[index + 1] == (byte)'L' &&
                header[index + 3] == 0 &&
                header[index + 2] >= (byte)'1' && header[index + 2] <= (byte)'9')
            {
                return MapCompressionType.ZLib1 + (header[index + 2] - '1');
            }

            return MapCompressionType.None;
        }

        private static bool Matches(byte[] data, int index, byte[] tag)
        {
            for (int i = 0; i < tag.Length; i++)
            {
                if (data[index + i] != tag[i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Reads the compression type from the current position and restores it afterwards.
        /// Returns <see cref="MapCompressionType.None"/> for streams that cannot seek.
        /// </summary>
        public static MapCompressionType PeekCompressionType(Stream stream)
        {
            if (stream == null || !stream.CanSeek)
                return MapCompressionType.None;

            long position = stream.Position;
            try
            {
                return GetCompressionType(stream);
            }
            finally
            {
                stream.Position = position;
            }
        }

        /// <summary>
        /// True if the stream starts with a known compression header. The stream position is restored.
        /// </summary>
        public static bool IsCompressed(Stream stream)
        {
            return PeekCompressionType(stream) != MapCompressionType.None;
        }

        public static MapCompressionType GetCompressionType(String fileName)
        {
            using (var stream = File.OpenRead(fileName))
                return GetCompressionType(stream);
        }

        public static MapCompressionType GetCompressionType(Stream stream)
        {
            byte[] header = new byte[HeaderSize];
            if (ReadFully(stream, header, HeaderSize) < HeaderSize)
                return MapCompressionType.None;

            return GetCompressionType(header);
        }

        #endregion

        #region decompression

        /// <summary>
        /// Returns a stream with the plain map data. If <paramref name="stream"/> is not compressed
        /// it is returned unchanged (rewound to where it started), otherwise a new in-memory stream
        /// holding the decompressed data is returned and the original is left fully consumed.
        /// </summary>
        public static Stream Decompress(Stream stream)
        {
            MapCompressionType type;
            return Decompress(stream, out type);
        }

        /// <summary>
        /// Same as <see cref="Decompress(Stream)"/>, but also reports the container that was found.
        /// </summary>
        public static Stream Decompress(Stream stream, out MapCompressionType type)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            long start = stream.CanSeek ? stream.Position : 0;

            byte[] header = new byte[HeaderSize];
            int headerRead = ReadFully(stream, header, HeaderSize);
            type = headerRead < HeaderSize ? MapCompressionType.None : GetCompressionType(header);

            if (type == MapCompressionType.None)
            {
                if (stream.CanSeek)
                {
                    stream.Position = start;
                    return stream;
                }

                // Not seekable: rebuild the original content in memory.
                var copy = new MemoryStream();
                copy.Write(header, 0, headerRead);
                stream.CopyTo(copy);
                copy.Position = 0;
                return copy;
            }

            int uncompressedSize = BitConverter.ToInt32(header, 4);
            byte[] body = ReadToEnd(stream);

            return new MemoryStream(Decompress(type, body, uncompressedSize), false);
        }

        /// <summary>
        /// Decompresses the payload that follows the 8 byte container header.
        /// </summary>
        public static byte[] Decompress(MapCompressionType type, byte[] body, int uncompressedSize)
        {
            switch (type)
            {
                case MapCompressionType.None:
                    return body;

                case MapCompressionType.RefPack:
                    return Refpack.Decode(body, 0, body.Length);

                case MapCompressionType.ZLib1:
                case MapCompressionType.ZLib2:
                case MapCompressionType.ZLib3:
                case MapCompressionType.ZLib4:
                case MapCompressionType.ZLib5:
                case MapCompressionType.ZLib6:
                case MapCompressionType.ZLib7:
                case MapCompressionType.ZLib8:
                case MapCompressionType.ZLib9:
                    return InflateZLib(body, uncompressedSize);

                default:
                    throw new NotSupportedException(
                        String.Format("Map compression format {0} is not supported.", type));
            }
        }

        /// <summary>
        /// Decompresses a whole map file (container header included) into plain map data.
        /// Uncompressed input is returned unchanged.
        /// </summary>
        public static byte[] Decompress(byte[] file)
        {
            var type = GetCompressionType(file);
            if (type == MapCompressionType.None)
                return file;

            int uncompressedSize = BitConverter.ToInt32(file, 4);

            byte[] body = new byte[file.Length - HeaderSize];
            Buffer.BlockCopy(file, HeaderSize, body, 0, body.Length);

            return Decompress(type, body, uncompressedSize);
        }

        private static byte[] InflateZLib(byte[] body, int uncompressedSize)
        {
            // DeflateStream cannot read the 2 byte zlib wrapper, so skip it.
            int skip = (body.Length >= 2 && (body[0] & 0x0F) == 8 && ((body[0] << 8) | body[1]) % 31 == 0) ? 2 : 0;

            using (var input = new MemoryStream(body, skip, body.Length - skip, false))
            using (var deflate = new DeflateStream(input, CompressionMode.Decompress))
            using (var output = new MemoryStream(uncompressedSize > 0 ? uncompressedSize : 0))
            {
                deflate.CopyTo(output);
                return output.ToArray();
            }
        }

        #endregion

        #region compression

        /// <summary>
        /// Wraps <paramref name="data"/> in the requested compression container.
        /// Returns the input unchanged for <see cref="MapCompressionType.None"/>.
        /// </summary>
        public static byte[] Compress(byte[] data, MapCompressionType type)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            if (type == MapCompressionType.None)
                return data;

            if (type != MapCompressionType.RefPack)
                throw new NotSupportedException(
                    String.Format("Writing map compression format {0} is not supported, only RefPack.", type));

            byte[] compressed = Refpack.Encode(data);

            byte[] result = new byte[HeaderSize + compressed.Length];
            Buffer.BlockCopy(RefPackTag, 0, result, 0, RefPackTag.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(data.Length), 0, result, 4, 4);
            Buffer.BlockCopy(compressed, 0, result, HeaderSize, compressed.Length);

            return result;
        }

        #endregion

        #region helpers

        private static int ReadFully(Stream stream, byte[] buffer, int count)
        {
            int total = 0;
            while (total < count)
            {
                int read = stream.Read(buffer, total, count - total);
                if (read <= 0)
                    break;
                total += read;
            }
            return total;
        }

        private static byte[] ReadToEnd(Stream stream)
        {
            using (var buffer = new MemoryStream())
            {
                stream.CopyTo(buffer);
                return buffer.ToArray();
            }
        }

        #endregion
    }
}
