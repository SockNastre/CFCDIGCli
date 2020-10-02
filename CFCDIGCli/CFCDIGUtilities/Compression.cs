using System.Collections.Generic;
using System.Linq;

namespace CFCDIGCli.CFCDIGUtilities
{
	public static class Compression
	{
		/// <summary>
		/// Original C++ code: <see href="https://github.com/Raw-man/Racjin-de-compression/blob/master/Racjin%20(de)compression/Decode.cpp">Racjin (de)compression</see>.
		/// Decompresses file compressed with Racjin's LZSS-based compression.
		/// </summary>
		/// <param name="buffer">Data to be decompressed.</param>
		/// <param name="decompressedSize">Size of data before compression.</param>
		/// <returns>Decompresseed data</returns>
		public static byte[] Decompress(byte[] buffer, uint decompressedSize)
		{
			uint lastDec = 0; // Last decoded byte of the previus decoding iteration
			uint destIndex = 0; // Position (destination) of a decoded byte in the output buffer
			byte bitShift = 0; // Shift right by bitShift

			var frequencies = new byte[256];
			var seqIndices = new uint[8192]; // References to previously decoded sequences
			var decompressedBuffer = new byte[decompressedSize];

			for (uint i = 0; i < buffer.Count(); i++)
			{
				uint nextToken = buffer[i + 1]; // Next pair of bytes to decode from the input buffer
				nextToken <<= 8;
				nextToken |= buffer[i];
				nextToken >>= bitShift; // Unfold 9-bit token

				/* 
				   The result can be interpreted as follows:
				   iiiiiiif|ooooolll - f=0
				   iiiiiiif|bbbbbbbb - f=1
				   i - ignore 
				   f - flag  (is literal or offset/length pair)
				   l - length (add 1 to get the real length)
				   o - occurrences/frequency
				   b - byte literal
				*/

				bitShift++;
				if (bitShift == 8)
				{
					bitShift = 0;
					i++;
				}

				uint seqIndex = destIndex; // Start of a byte sequence

				if ((nextToken & 0x100) != 0) // Bit Flag: is nextToken a literal or a reference?
				{
					decompressedBuffer[destIndex] = (byte)(nextToken & 0xFF); // Store the literal
					destIndex++;
				}
				else
				{

					uint key1 = ((nextToken >> 3) & 0x1F) + lastDec * 32; //0x1F + 0xFF*32 = 8191
					uint srcIndex = seqIndices[key1]; //get a reference to a previously decoded sequence

					for (byte length = 0; length < (nextToken & 0x07) + 1; length++, destIndex++, srcIndex++)
					{
						decompressedBuffer[destIndex] = decompressedBuffer[srcIndex]; // Copy a previously decoded byte sequence (up to 8)
					}
				}

				if (destIndex >= decompressedBuffer.Count())
				{
					break;
				}

				uint key2 = frequencies[lastDec] + lastDec * 32; // 0x1F + 0xFF * 32 = 8191
				seqIndices[key2] = seqIndex;
				frequencies[lastDec] = (byte)((frequencies[lastDec] + 1) & 0x1F); // Increases by 1 (up to 31)
				lastDec = decompressedBuffer[destIndex - 1];
			}

			return decompressedBuffer;
		}

		/// <summary>
		/// Original C++ code: <see href="https://github.com/Raw-man/Racjin-de-compression/blob/master/Racjin%20(de)compression/Encode.cpp">Racjin (de)compression</see>.
		/// Compresses file with Racjin's LZSS-based compression.
		/// 
		/// <para><b>NOTE: FUNCTION CURRENTLY BROKEN FOR C#.</b></para>
		/// </summary>
		/// <param name="buffer">Data to be compressed.</param>
		/// <returns>Compressed data</returns>
		public static byte[] Compress(byte[] buffer)
		{
			/*
			 * As stated in the summary this code is currently broken most likely through an error
			 * from manual conversion from C++ to C#. This will be looked into but is not a
			 * priority for repacking CFC.DIG .
			 */

			uint lastEnc = 0; // Last encoded byte
			byte bitShift = 0; // Shift by bitShift (used to fold tokens)

			var frequencies = new ushort[256];
			var seqIndices = new uint[8192];

			var tokens = new List<ushort>
			{
				Capacity = buffer.Count() / 2
			};

			var compressedBuffer = new List<byte>
			{
				Capacity = buffer.Count()
			};

			for (uint i = 0; i < buffer.Count(); i++)
			{
				byte bestFreq = 0;
				byte bestMatch = 0;

				/* 
				
				To get the compression for Bomberman Kart DX do:
				
				if (frequencies[lastEncByte] >= 256)
				{
					frequencies[lastEncByte] = 0x00;
				}

				*/

				var positionsToCheck = (byte)(frequencies[lastEnc] < 32 ? (frequencies[lastEnc] & 0x1F) : 32);
				uint seqIndex = i;

				for (byte freq = 0; freq < positionsToCheck; freq++)
				{
					uint key1 = freq + lastEnc * 32; // 0x1F + 0xFF*32 = 8191
					uint srcIndex = seqIndices[key1];
					byte matched = 0;
					var maxLength = (uint)(i + 8 < buffer.Count() ? 8 : buffer.Count() - i);

					for (byte offset = 0; offset < maxLength; offset++)
					{
						if (buffer[srcIndex + offset] == buffer[i + offset])
						{
							matched++;
						}
						else
						{
							break;
						}
					}

					if (matched > bestMatch)
					{
						bestFreq = freq;
						bestMatch = matched;
					}
				}

				ushort token = 0x00;
				if (bestMatch > 0) // Found a better match?
				{
					token |= (ushort)(bestFreq << 3); // f|ooooolll - f=0 (flag), o - occurrence/frequency, l -length
					token |= (ushort)(bestMatch - 1); // Encode a reference
					i += bestMatch;
				}
				else // Encode byte literal
				{
					token = (ushort)(0x100 | buffer[i]); // f|bbbbbbbb - f=1
					i++;
				}

				token <<= bitShift; // Prepare for folding
				tokens.Add(token);

				bitShift++;
				if (bitShift == 8)
				{
					bitShift = 0;
				}

				uint key2 = (uint)((frequencies[lastEnc] & 0x1F) + lastEnc * 32); // 0x1F + 0xFF*32 = 8191
				seqIndices[key2] = seqIndex;
				frequencies[lastEnc] = (ushort)(frequencies[lastEnc] + 1);
				lastEnc = buffer[i - 1];
			}

			// Fold tokens (8 tokens, 16 bytes -> 8 tokens, 9 bytes)
			for (uint i = 0; i < tokens.Count; i += 8)
			{
				var groupSize = (ulong)(i + 8 < tokens.Count ? 8 : tokens.Count - i);

				for (uint s = 0; s <= groupSize; s += 2)
				{
					var first = (ushort)(s > 0 ? tokens[(int)(s + i - 1)] : 0x00);
					var middle = (ushort)(s < groupSize ? tokens[(int)(s + i)] : 0x00);
					var last = (ushort)(s < groupSize - 1 ? tokens[(int)(s + i + 1)] : 0x00);

					var result = (ushort)(middle | (first >> 8) | (last << 8));
					compressedBuffer.Add((byte)(result & 0xFF));

					if (s < groupSize)
					{
						compressedBuffer.Add((byte)(result >> 8));
					}
				}
			}

			return compressedBuffer.ToArray();
		}
	}
}
