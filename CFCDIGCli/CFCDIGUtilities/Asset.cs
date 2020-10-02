using System.IO;

namespace CFCDIGCli.CFCDIGUtilities
{
    /// <summary>
    /// Provides fields, properties, constructor, and method to aid in reading a .raw archive.
    /// </summary>
    public class RawArchive
    {
        private uint offset;
        public uint Offset
        {
            get { return offset; }
            set { offset = value * 0x800; }
        }

        public uint PackedSize { get; set; }
        public ushort SectionCount { get; set; }
        public bool IsCompressed { get; set; }
        public uint UnpackedSize { get; set; }

        public RawArchive(BinaryReader reader)
        {
            this.Offset = reader.ReadUInt32();
            this.PackedSize = reader.ReadUInt32();
            this.SectionCount = reader.ReadUInt16();
            this.IsCompressed = reader.ReadInt16() == 1;
            this.UnpackedSize = reader.ReadUInt32();
        }

        /// <summary>
        /// Gets file (aka section) count of .raw archive.
        /// </summary>
        /// <param name="path">Path of .raw archive.</param>
        /// <returns>Amount of sections, returns 0xFFFF if invalid.</returns>
        public static ushort GetFileCount(string path)
        {
            using (var reader = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                if (reader.ReadInt32() != 0) // Is compressed, or is unknown file type
                {
                    return 0xFFFF;
                }

                reader.BaseStream.Position = 8;
                return (ushort)(reader.ReadUInt32() / 16);
            }
        }
    }
}
