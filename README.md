<p align="center">
  <img src="https://raw.githubusercontent.com/OCircles/LiveSplit.MemoryReader/master/screenshot.png" alt="Memory Reader"/>
</p>

## LiveSplit.MemoryReader
Reads and displays a memory value from your game

First attempt at a LiveSplit component so it's very sloppy, but it seems to work fine with surprisingly little memory/performance impact!


#### Features
	
	+ Multi-level pointers
	+ Save pointer lists for individual games
	+ Display signed/unsigned byte, short, int, long and float

#### Todo

	1. Labelling pointers
	2. Add suffix after value, either static or also read from memory (so you can show say "3/7")
	3. Add option to hide component when game isn't running
	4. Fix the dumb binary > type > string conversion

## Packages / Requirements

- If you plan on building this solution yourself, you may need to add references to the following dlls, all of which are distributed with LiveSplit: [http://livesplit.org/](http://livesplit.org/ "Livesplit Home")

	+ LiveSplit.Core.dll
	+ UpdateManger.dll
	+ WinformsColor.dll
