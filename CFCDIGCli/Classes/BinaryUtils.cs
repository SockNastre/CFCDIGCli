using System.IO;

namespace CFCDIGCli.Classes
{
    public static class BinaryUtils
    {
        /// <summary>
        /// Writes padding in BinaryWriter by chosen multiples.
        /// </summary>
        /// <param name="writer">BinaryWriter to write padding in.</param>
        /// <param name="padMultiple">Multiple to pad by.</param>
        /// <param name="writeMultiple">Multiple to write padding by.</param>
        public static void WritePadding(this BinaryWriter writer, uint padMultiple, uint writeMultiple = 1)
        {
            while (writer.BaseStream.Position % padMultiple != 0)
            {
                writer.Write(new byte[writeMultiple]);
            }
        }

        /// <summary>
        /// Gets size after it is padded by chosen multiples.
        /// </summary>
        /// <param name="size">Initial size to pad.</param>
        /// <param name="padMultiple">Multiple to pad by.</param>
        /// <param name="addMultiple">Multiple to add padding by.</param>
        /// <returns>Size after being padded.</returns>
        public static uint GetPaddedSize(uint size, uint padMultiple, uint addMultiple = 1)
        {
            while (size % padMultiple != 0)
            {
                size += addMultiple;
            }

            return size;
        }
    }
}
