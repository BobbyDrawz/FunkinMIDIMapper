# Funkin' MIDI Mapper (FMM)

Funkin' MIDI Mapper (FMM) is a tool to convert MIDI files into chart files for Friday Night Funkin' (FNF) which can be found here:
- [FNF Itch.io Page](https://ninja-muffin24.itch.io/funkin)

It is a heavily modified fork of the SiIva Note Importer For FNF (SNIFF) released by MtH which can be found here:
- [SNIFF Repository](https://github.com/PrincessMtH/SNIFF)

This fork is designed to support MIDI usage rather than an FLP.

## Features
- Convert MIDI files into FNF chart files.
- Generate camera events for the charts.
- Customize playable and opponent characters.
- Supports combining notes and camera events into a single chart file.

## Installation
To build and run FMM from the source code, you need to have the following installed on your system:
- [Microsoft Visual Studio](https://visualstudio.microsoft.com/)

Clone or download the repository and open the `FMM.csproj` file in Visual Studio. Then build the project.

For release builds, you simply need to run the `FMM.exe` file provided in the release.

**KEEP IN MIND THAT THIS IS DESIGNED FOR WINDOWS OS**

## Usage
To use FMM, run the executable and follow the prompts to select a MIDI file, set the BPM, and configure other settings.

Make sure you use the SNIFF FPC Preset to map your notes [BPM Change and Alternate Animation Section triggers won't do anything at the moment] and camera events. It is included with the release builds.

## Parameters
- **Enter BPM**: Sets the BPM/Tempo of the chart to the value listed. Make sure it lines up with your song.
- **Song name**: Self-explanatory.
- **Needs voice file**: Determines whether you need vocal files to accompany the instrumental. [y for yes, n for no, default answer is y]
- **Enter the playable character**: Decides what character you play as in the chart. [e.g. bf, bf-pixel, pico-playable, etc.]
- **Enter the opponent character**: Decides what character you play against in the chart. [e.g. dad, senpai, mom-car, tankman, etc.]
- **Scroll speed**: You already know.

## Contributions
Fan contributions are welcome! Please submit pull requests or issues as needed.

## Tips
- This tool generates FNF charts designed for legacy builds [more specifically the Week 6 update]. This means if you want to convert this into a chart format for any other engine [psych engine JSONs, FNFC files, etc.], then you certainly can!
- Notes are sustained if they are 2 steps or longer [a step is 1/16 of a bar], but if your note is shorter, you can lower the velocity to below 50% if you want to sustain them anyway.
- This was all based off of the SNIFF source code, meaning this program is written in C#.
- CAM events can only be caught up by the program if they are placed at the very start of a bar.

## License
This project is licensed under the MIT License - see the [LICENSE](https://github.com/BobbyDrawz/FunkinMIDIMapper/blob/main/LICENSE.md) file for details.

Now go out there and have fun charting!
