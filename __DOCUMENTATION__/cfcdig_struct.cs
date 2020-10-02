struct FileRecord{
	uint offset; // Divided by 0x800
	uint packedSize;
	ushort sectionCount;
	bool isCompressed;
	byte padding;
	uint orgSize;
}