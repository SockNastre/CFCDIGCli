## CFCDIGCli
Requires .NET Framework 4.8

### Features
  - Can pack/unpack CFC.DIG archives.
  - Drag & drop CFC.DIG archive to unpack, or, folder to pack into executable. Making it easier to work with.
  - Allows for modification of files inside of CFC.DIG for various PS2/PSP games.
  - Tool can read from a "filelist" allowing custom paths to be correctly packed in order.
  - Provides working compression and decompression algorithm which can be easily implemented in other tools/systems.

### F.A.Q

Q: _Why does opening the tool with no arguments give me an error?_  
A: This is because when the tool is opened with no arguments it searches for a "data" folder and (optionally) a "filelist.txt" to auto-pack; this is a feature for ease-of-access. If you are looking to see the help page then use the "-h" command.

Q: _How do I use a "filelist"? What does that even mean?_  
A: Read DOCUMENTATION for details about that.

Q: _How can I get in direct contact with anyone related to this tool?_  
A: You can join the [Naruto Classic's Discord](https://discord.gg/jhKmg97), where we usually discuss all of this stuff.

### Credits
  - Biggest thanks to [Raw-man](https://github.com/Raw-man) (!Mr.JellyBean) for figuring out the compression/decompression algorithms, and for creating [Racjin (de)compression](https://github.com/Raw-man/Racjin-de-compression) which was the basis for my conversion of his C++ compression/decompression code into C#. He also helped confirm things about CFC.DIG and did some digging around into other game's CFC.DIG or similar archives that were made by Racjin.
  - Greg Beech for his [Natural String Comparer](https://stackoverflow.com/a/248613/10216412).
