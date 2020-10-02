using System.IO;

namespace CFCDIGCli.Classes
{
    public static class BinaryUtils
    {
        public static void WritePadding(this BinaryWriter writer, uint padMultiple, uint writeMultiple = 1)
        {
            while (writer.BaseStream.Position % padMultiple != 0)
            {
                writer.Write(new byte[writeMultiple]);
            }
        }

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
