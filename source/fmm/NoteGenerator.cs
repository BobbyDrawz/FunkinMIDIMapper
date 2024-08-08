using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using NAudio.Midi;

namespace FMM
{
    internal class NoteGenerator
    {
        private static void ResetGlobals()
        {
            Globals.ppqn = 96;
            Globals.name = "";
            Globals.bpm = 0f;
            Globals.needsVoices = 0;
            Globals.player1 = "";
            Globals.player2 = "";
        }

        private static JObject DefaultSection()
        {
            JObject section = new JObject
            {
                { "lengthInSteps", 16 },
                { "mustHitSection", true },
                { "sectionNotes", new JArray() }
            };
            return section;
        }

        private static float MIDITimeToMillis(float bpm)
        {
            return 60000f / bpm / Globals.ppqn;
        }

        public static JObject MidiToJSON(List<MidiEvent> midiEvents, float bpm, string songName, int needsVoices, string player1, string player2, float speed)
        {
            Globals.name = songName;
            Globals.needsVoices = needsVoices;
            Globals.player1 = player1;
            Globals.player2 = player2;

            JObject songJson = new JObject
            {
                { "song", Globals.name },
                { "bpm", bpm },
                { "needsVoices", Globals.needsVoices > 0 },
                { "player1", Globals.player1 },
                { "player2", Globals.player2 },
                { "speed", speed }
            };

            List<JObject> sections = new List<JObject>();
            bool mustHitSection = true;

            JObject currentSection = DefaultSection();
            List<JArray> sectionNotes = new List<JArray>();
            long lastBarStart = 0;

            foreach (var midiEvent in midiEvents)
            {
                if (midiEvent.AbsoluteTime >= lastBarStart + Globals.ppqn * 4)
                {
                    currentSection["sectionNotes"] = JArray.FromObject(sectionNotes);
                    sections.Add(currentSection);

                    currentSection = DefaultSection();
                    sectionNotes = new List<JArray>();
                    lastBarStart += Globals.ppqn * 4;
                }

                if (midiEvent is NoteOnEvent noteOn)
                {
                    if (noteOn.OffEvent != null)
                    {
                        uint noteDuration = (uint)(noteOn.OffEvent.AbsoluteTime - noteOn.AbsoluteTime);
                        float noteTime = noteOn.AbsoluteTime * MIDITimeToMillis(bpm);
                        float sustainLength = noteDuration * MIDITimeToMillis(bpm);

                        int noteType = MapMidiNoteToFNF(noteOn.NoteNumber, mustHitSection);
                        if (noteType >= 0)
                        {
                            // Logic to ensure notes are not sustained if they are less than two steps and have a velocity over 50%
                            if (noteDuration < (Globals.ppqn * 4 / 16) * 2 && noteOn.Velocity > 63)
                            {
                                sustainLength = 0;
                            }
                            JArray noteArray = new JArray { noteTime, noteType, sustainLength };
                            sectionNotes.Add(noteArray);
                        }
                    }
                }
            }

            currentSection["sectionNotes"] = JArray.FromObject(sectionNotes);
            sections.Add(currentSection);
            songJson.Add("notes", JArray.FromObject(sections));

            JObject finalJson = new JObject
            {
                { "song", songJson },
                { "generatedBy", "FMM v2" }
            };

            return finalJson;
        }

        public static JObject CreateBaseChartJson(float bpm, string songName, int needsVoices, string player1, string player2, float speed)
        {
            JObject songJson = new JObject
            {
                { "song", songName },
                { "bpm", bpm },
                { "needsVoices", needsVoices > 0 },
                { "player1", player1 },
                { "player2", player2 },
                { "speed", speed },
                { "notes", new JArray() }
            };

            JObject finalJson = new JObject
            {
                { "song", songJson },
                { "generatedBy", "FMM v2" }
            };

            return finalJson;
        }

        private static int MapMidiNoteToFNF(int noteNumber, bool mustHitSection)
        {
            switch (noteNumber)
            {
                case 48: return mustHitSection ? 0 : 4; // BF Left
                case 49: return mustHitSection ? 1 : 5; // BF Down
                case 50: return mustHitSection ? 2 : 6; // BF Up
                case 51: return mustHitSection ? 3 : 7; // BF Right
                case 60: return !mustHitSection ? 0 : 4; // EN Left
                case 61: return !mustHitSection ? 1 : 5; // EN Down
                case 62: return !mustHitSection ? 2 : 6; // EN Up
                case 63: return !mustHitSection ? 3 : 7; // EN Right
                default: return -1; // Ignore other notes
            }
        }

        public static List<MidiEvent> ReadMidiFile(string midiFilePath)
        {
            var midiFile = new MidiFile(midiFilePath, false);
            List<MidiEvent> midiEvents = new List<MidiEvent>();

            foreach (var track in midiFile.Events)
            {
                foreach (var midiEvent in track)
                {
                    midiEvents.Add(midiEvent);
                }
            }

            return midiEvents;
        }

        public static void JSONToMidi(JObject chartJson, float bpm)
        {
            List<MidiEvent> midiEvents = new List<MidiEvent>();

            foreach (var section in chartJson["song"]["notes"])
            {
                foreach (var note in section["sectionNotes"])
                {
                    float noteTime = (float)note[0];
                    int noteType = (int)note[1];
                    float sustainLength = (float)note[2];

                    long absoluteTime = (long)(noteTime * 1000);
                    int noteNumber = MapFNFNoteToMidi(noteType);

                    var noteOn = new NoteOnEvent(absoluteTime, 1, noteNumber, 127, (int)sustainLength);
                    var noteOff = new NoteEvent(absoluteTime + (long)sustainLength, 1, MidiCommandCode.NoteOff, noteNumber, 0);

                    midiEvents.Add(noteOn);
                    midiEvents.Add(noteOff);
                }
            }

            var midiEventCollection = new MidiEventCollection(1, Globals.ppqn);
            midiEventCollection.AddTrack(midiEvents);
            string outputPath = "rf-" + chartJson["song"]["song"] + ".mid";
            MidiFile.Export(outputPath, midiEventCollection);

            Console.WriteLine("MIDI file created: " + outputPath);
        }

        private static int MapFNFNoteToMidi(int noteType)
        {
            switch (noteType)
            {
                case 0: return 48; // BF Left
                case 1: return 49; // BF Down
                case 2: return 50; // BF Up
                case 3: return 51; // BF Right
                case 4: return 60; // EN Left
                case 5: return 61; // EN Down
                case 6: return 62; // EN Up
                case 7: return 63; // EN Right
                default: return -1; // Ignore other notes
            }
        }
    }
}
