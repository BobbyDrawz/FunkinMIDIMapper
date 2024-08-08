using System;
using System.Collections.Generic;
using System.IO;
using NAudio.Midi;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FMM
{
    internal static class NoteCameraCombiner
    {
        public static void CombineFiles(string notesFilePath, string cameraFilePath, string outputFilePath)
        {
            JObject notesJson = JObject.Parse(File.ReadAllText(notesFilePath));
            List<int> cameraEvents = new List<int>(Array.ConvertAll(File.ReadAllLines(cameraFilePath), int.Parse));

            JArray notesSections = (JArray)notesJson["song"]["notes"];
            for (int i = 0; i < notesSections.Count; i++)
            {
                if (i < cameraEvents.Count)
                {
                    notesSections[i]["mustHitSection"] = cameraEvents[i] == 1;
                }
            }

            File.WriteAllText(outputFilePath, notesJson.ToString(Formatting.Indented));
            ChartPostfix.FixChart(outputFilePath);
        }

        public static void AddSectionIdentifiers(string notesFilePath, Dictionary<string, List<int>> sectionIdentifiers)
        {
            JObject notesJson = JObject.Parse(File.ReadAllText(notesFilePath));
            JArray notesSections = (JArray)notesJson["song"]["notes"];
            foreach (var sectionIdentifier in sectionIdentifiers)
            {
                for (int i = 0; i < notesSections.Count; i++)
                {
                    if (i < sectionIdentifier.Value.Count)
                    {
                        notesSections[i][sectionIdentifier.Key] = sectionIdentifier.Value[i] == 1;
                    }
                }
            }

            File.WriteAllText(notesFilePath, notesJson.ToString(Formatting.Indented));
        }

        public static void CreateSectionProperties(MidiFile midiFile, string outputFilePath)
        {
            int totalBars = midiFile.Events.Max(track => track.Count);
            List<int> sectionProperties = new List<int>(new int[totalBars]);
            int previousValue = 0;

            foreach (var track in midiFile.Events)
            {
                foreach (var midiEvent in track)
                {
                    if (midiEvent is NoteOnEvent noteEvent && (noteEvent.NoteNumber == (int)MIDINotes.SP_FALSE || noteEvent.NoteNumber == (int)MIDINotes.SP_TRUE))
                    {
                        int barIndex = (int)(noteEvent.AbsoluteTime / midiFile.DeltaTicksPerQuarterNote);
                        if (barIndex < totalBars)
                        {
                            sectionProperties[barIndex] = (noteEvent.NoteNumber == (int)MIDINotes.SP_TRUE) ? 1 : 0;
                            previousValue = sectionProperties[barIndex];
                        }
                    }
                }
            }

            for (int i = 0; i < totalBars; i++)
            {
                if (sectionProperties[i] == 0 && previousValue == 1)
                {
                    sectionProperties[i] = previousValue;
                }
            }

            File.WriteAllLines(outputFilePath, sectionProperties.Select(x => x.ToString()));
        }

        public static void CombineCharts(string initialDirectory, string songName, float bpm, int needsVoices, string player1, string player2, float speed, List<JObject> charts)
        {
            JObject combinedChart = new JObject
            {
                { "song", songName },
                { "bpm", bpm },
                { "needsVoices", needsVoices > 0 },
                { "player1", player1 },
                { "player2", player2 },
                { "speed", speed }
            };

            List<JObject> combinedSections = new List<JObject>();
            int maxSections = charts.Max(chart => ((JArray)chart["song"]["notes"]).Count);

            for (int sectionIndex = 0; sectionIndex < maxSections; sectionIndex++)
            {
                JObject combinedSection = new JObject
                {
                    { "lengthInSteps", 16 },
                    { "mustHitSection", true },
                    { "sectionNotes", new JArray() }
                };

                foreach (var chart in charts)
                {
                    JArray chartSections = (JArray)chart["song"]["notes"];
                    if (sectionIndex < chartSections.Count)
                    {
                        JArray sectionNotes = (JArray)chartSections[sectionIndex]["sectionNotes"];
                        foreach (JArray note in sectionNotes)
                        {
                            ((JArray)combinedSection["sectionNotes"]).Add(note);
                        }
                    }
                }

                combinedSections.Add(combinedSection);
            }

            combinedChart.Add("notes", JArray.FromObject(combinedSections));
            string outputDirectory = Path.Combine(initialDirectory, "cc-" + songName);
            Directory.CreateDirectory(outputDirectory);

            string combinedChartPath = Path.Combine(outputDirectory, "chart.json");
            File.WriteAllText(combinedChartPath, combinedChart.ToString(Formatting.Indented));
        }
    }
}
