using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NAudio.Midi;
using System.Linq;
using System.Windows.Forms;
using FMM;

namespace FMM
{
    internal class Program
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

        [STAThread]
        private static void Main(string[] args)
        {
            Console.WriteLine("Funkin' MIDI Mapper [FMM]");
            Console.WriteLine("your all in one charting tool... version " + Globals.VersionNumber);

            while (true)
            {
                Console.WriteLine("Select mode:");
                Console.WriteLine("1. Forward Function");
                Console.WriteLine("2. Reverse Function");
                Console.WriteLine("3. Event Create");
                Console.WriteLine("4. Combine Charts");
                Console.WriteLine("5. Midi Form");
                Console.Write("Enter your choice (1, 2, 3, 4, or 5): ");
                string modeInput = Console.ReadLine();

                switch (modeInput)
                {
                    case "1":
                        ForwardFunction(args);
                        break;
                    case "2":
                        ReverseFunction();
                        break;
                    case "3":
                        EventCreate();
                        break;
                    case "4":
                        CombineCharts();
                        break;
                    case "5":
                        MidiForm();
                        break;
                    default:
                        Console.WriteLine("Invalid input. Please enter 1, 2, 3, 4, or 5.");
                        continue;
                }

                Console.WriteLine("Press any key to close...");
                Console.ReadKey();
                break;
            }
        }

        private static void ReverseFunction()
        {
            Console.Write("Enter the chart file path (notes.json): ");
            string notesFilePath = Console.ReadLine();

            if (File.Exists(notesFilePath))
            {
                string tempFilePath = Path.Combine(Path.GetDirectoryName(notesFilePath), "temp_notes.json");
                File.Copy(notesFilePath, tempFilePath, true);

                ChartPostfix.FixChart(tempFilePath);

                JObject chartJson = JObject.Parse(File.ReadAllText(tempFilePath));
                JArray notesSections = (JArray)chartJson["song"]["notes"];

                List<JObject> midiNotes = new List<JObject>();

                foreach (JObject section in notesSections)
                {
                    section["mustHitSection"] = true;

                    JArray sectionNotes = (JArray)section["sectionNotes"];
                    foreach (JArray noteArray in sectionNotes)
                    {
                        double time = noteArray[0].Value<double>() / 1000.0;
                        int pitch = noteArray[1].Value<int>() % 12; // Mod 12 to keep pitch within MIDI range
                        int velocity = 127; // Default velocity
                        double duration = noteArray[2].Value<double>() / 1000.0; // Convert duration to seconds

                        JObject midiNote = new JObject
                        {
                            ["time"] = time,
                            ["pitch"] = pitch,
                            ["velocity"] = velocity,
                            ["duration"] = duration
                        };

                        midiNotes.Add(midiNote);
                    }
                }

                JObject midiJson = new JObject
                {
                    ["bpm"] = chartJson["song"]["bpm"],
                    ["notes"] = JArray.FromObject(midiNotes)
                };

                string midiJsonFilePath = Path.Combine(Path.GetDirectoryName(notesFilePath), "midi_convert.json");
                File.WriteAllText(midiJsonFilePath, midiJson.ToString());

                Console.WriteLine("Reverse function completed. MIDI conversion file saved as midi_convert.json.");
                Console.WriteLine("Press any key to close...");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("The specified file does not exist.");
            }
        }

        private static void ForwardFunction(string[] args)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Directory.GetCurrentDirectory(),
                Filter = "MIDI file (*.mid)|*.mid|All files (*.*)|*.*",
                Multiselect = true
            };

            if (args.Length == 0)
            {
                Console.WriteLine("Select your .mid file...");
            }

            if (args.Length != 0 || openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (args.Length == 0)
                {
                    args = openFileDialog.FileNames;
                }

                string initialDirectory = Directory.GetCurrentDirectory();
                string[] inputFiles = args;

                foreach (string inputFile in inputFiles)
                {
                    try
                    {
                        var midiFile = new MidiFile(inputFile, false);
                        List<MidiEvent> midiEvents = midiFile.Events.SelectMany(track => track).ToList();

                        Console.Write("Enter BPM: ");
                        float bpm = float.Parse(Console.ReadLine());

                        Console.Write("Song name: ");
                        string songName = Console.ReadLine();

                        Console.Write("Needs voice file? (y/N, default y): ");
                        int needsVoices = (Console.ReadLine().ToLower().Trim() != "n") ? 1 : -1;

                        Console.Write("Enter the playable character (e.g., 'bf'): ");
                        string player1 = Console.ReadLine();

                        Console.Write("Enter the opponent character (e.g., 'dad'): ");
                        string player2 = Console.ReadLine();

                        Console.Write("Scroll speed: ");
                        float speed = float.Parse(Console.ReadLine());

                        JObject songJson = NoteGenerator.MidiToJSON(midiEvents, bpm, songName, needsVoices, player1, player2, speed);

                        string outputDirectory = Path.Combine(initialDirectory, "ff-" + songName);
                        Directory.CreateDirectory(outputDirectory);

                        string notesFilePath = Path.Combine(outputDirectory, "notes.json");
                        File.WriteAllText(notesFilePath, songJson.ToString(Formatting.Indented));
                        Console.WriteLine("---Generated base note chart.---");

                        FunkinCam.GenerateCameraFile(midiEvents, bpm, Path.Combine(outputDirectory, "camera.txt"));
                        string cameraFilePath = Path.Combine(outputDirectory, "camera.txt");
                        Console.WriteLine("---Generated camera text file.---");

                        NoteCameraCombiner.CombineFiles(notesFilePath, cameraFilePath, Path.Combine(outputDirectory, "chart.json"));
                        string chartFilePath = Path.Combine(outputDirectory, "chart.json");
                        Console.WriteLine("---Combined files to produce chart.json.---");

                        while (true)
                        {
                            Console.Write("Skip section identifier handling? (y/n): ");
                            string input = Console.ReadLine().ToLower().Trim();
                            if (input == "y")
                            {
                                break;
                            }
                            else if (input == "n")
                            {
                                while (true)
                                {
                                    Console.Write("Add bpm section identifier? (y/n): ");
                                    input = Console.ReadLine().ToLower().Trim();
                                    if (input == "y")
                                    {
                                        List<int> bpmSection = Enumerable.Repeat(1, songJson["song"]["notes"].Count()).ToList();
                                        NoteCameraCombiner.AddSectionIdentifiers(notesFilePath, new Dictionary<string, List<int>> { { "bpm", bpmSection } });
                                        Console.WriteLine("---Added bpm section identifier.---");
                                        break;
                                    }
                                    else if (input == "n")
                                    {
                                        Console.WriteLine("Enter the names of the section identifier text files (e.g., gfSection.txt, altAnim.txt) separated by commas:");
                                        string[] identifierFiles = Console.ReadLine().Split(',');

                                        Dictionary<string, List<int>> additionalSectionIdentifiers = new Dictionary<string, List<int>>();
                                        foreach (string identifierFile in identifierFiles)
                                        {
                                            string identifierName = Path.GetFileNameWithoutExtension(identifierFile.Trim());
                                            if (identifierName == "changeBPM")
                                            {
                                                Console.WriteLine("changeBPM identifier found. Skipping as per user choice.");
                                                continue;
                                            }

                                            List<int> identifierValues = new List<int>(Array.ConvertAll(File.ReadAllLines(Path.Combine(initialDirectory, identifierFile.Trim())), int.Parse));
                                            additionalSectionIdentifiers.Add(identifierName, identifierValues);
                                        }

                                        NoteCameraCombiner.AddSectionIdentifiers(notesFilePath, additionalSectionIdentifiers);
                                        Console.WriteLine("---Added additional section identifiers.---");
                                        break;
                                    }
                                    else
                                    {
                                        Console.WriteLine("Invalid input. Please enter y or n.");
                                    }
                                }
                                break;
                            }
                            else
                            {
                                Console.WriteLine("Invalid input. Please enter y or n.");
                            }
                        }

                        while (true)
                        {
                            Console.Write("Delete chart remnants? (1 for Yes, 0 for No): ");
                            string input = Console.ReadLine();
                            if (input == "1")
                            {
                                File.Delete(notesFilePath);
                                File.Delete(cameraFilePath);
                                Console.WriteLine("Deleted notes.json and camera.txt.");
                                break;
                            }
                            else if (input == "0")
                            {
                                break;
                            }
                            else
                            {
                                Console.WriteLine("Invalid input. Please enter 1 or 0.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }
                }

                ResetGlobals();
                Console.WriteLine("Press any key to close...");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Dialog closed");
            }
        }

        private static void EventCreate()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Directory.GetCurrentDirectory(),
                Filter = "MIDI file (*.mid)|*.mid|All files (*.*)|*.*",
                Multiselect = false
            };

            Console.WriteLine("Select your .mid file...");
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string inputFile = openFileDialog.FileName;
                string outputFile = Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileNameWithoutExtension(inputFile) + "_events.txt");

                var midiFile = new MidiFile(inputFile, false);
                List<MidiEvent> midiEvents = midiFile.Events.SelectMany(track => track).ToList();

                using (StreamWriter writer = new StreamWriter(outputFile))
                {
                    foreach (MidiEvent midiEvent in midiEvents)
                    {
                        writer.WriteLine(midiEvent.ToString());
                    }
                }

                Console.WriteLine("Event create function completed. Events saved in " + outputFile);
            }
            else
            {
                Console.WriteLine("Dialog closed");
            }
        }

        private static void CombineCharts()
        {
            try
            {
                Console.WriteLine("Enter the directory containing the charts (initial directory): ");
                string initialDirectory = Console.ReadLine();

                Console.WriteLine("Enter the song name: ");
                string songName = Console.ReadLine();

                Console.WriteLine("Enter the BPM: ");
                if (!float.TryParse(Console.ReadLine(), out float bpm))
                {
                    Console.WriteLine("Invalid BPM. Please enter a numeric value.");
                    return;
                }

                Console.WriteLine("Does the chart need voices? (Enter 1 for Yes, 0 for No): ");
                if (!int.TryParse(Console.ReadLine(), out int needsVoices))
                {
                    Console.WriteLine("Invalid input for needsVoices. Please enter 1 or 0.");
                    return;
                }

                Console.WriteLine("Enter the player 1 name: ");
                string player1 = Console.ReadLine();

                Console.WriteLine("Enter the player 2 name: ");
                string player2 = Console.ReadLine();

                Console.WriteLine("Enter the game speed: ");
                if (!float.TryParse(Console.ReadLine(), out float speed))
                {
                    Console.WriteLine("Invalid speed. Please enter a numeric value.");
                    return;
                }

                Console.WriteLine("Enter the first chart file path (relative to the initial directory, JSON format): ");
                string chartFilePath1 = Path.Combine(initialDirectory, Console.ReadLine());

                Console.WriteLine("Enter the note type for the first chart (or 'default' for none): ");
                string noteType1 = Console.ReadLine();

                Console.WriteLine("Enter the second chart file path (relative to the initial directory, JSON format): ");
                string chartFilePath2 = Path.Combine(initialDirectory, Console.ReadLine());

                Console.WriteLine("Enter the note type for the second chart (or 'default' for none): ");
                string noteType2 = Console.ReadLine();

                if (File.Exists(chartFilePath1) && File.Exists(chartFilePath2))
                {
                    JObject chartJson1 = JObject.Parse(File.ReadAllText(chartFilePath1));
                    JObject chartJson2 = JObject.Parse(File.ReadAllText(chartFilePath2));

                    // Apply the note type as a fourth parameter in each note of the first chart
                    if (noteType1.ToLower() != "default")
                    {
                        AddNoteTypeToSectionNotes(chartJson1, noteType1);
                    }

                    // Apply the note type as a fourth parameter in each note of the second chart
                    if (noteType2.ToLower() != "default")
                    {
                        AddNoteTypeToSectionNotes(chartJson2, noteType2);
                    }

                    List<JObject> charts = new List<JObject> { chartJson1, chartJson2 };

                    NoteCameraCombiner.CombineCharts(initialDirectory, songName, bpm, needsVoices, player1, player2, speed, charts);

                    Console.WriteLine("Charts combined successfully.");
                }
                else
                {
                    Console.WriteLine("One or both of the specified files do not exist.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred during the combine charts operation: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void AddNoteTypeToSectionNotes(JObject chartJson, string noteType)
        {
            JArray sections = (JArray)chartJson["song"]["notes"];
            foreach (JObject section in sections)
            {
                JArray sectionNotes = (JArray)section["sectionNotes"];
                for (int i = 0; i < sectionNotes.Count; i++)
                {
                    JArray note = (JArray)sectionNotes[i];
                    if (note.Count == 3)
                    {
                        // Add the noteType as the 4th parameter
                        note.Add(noteType);
                    }
                }
            }
        }

        private static void MidiForm()
        {
            Console.WriteLine("Enter the path to the reverseFunction JSON file:");
            string jsonFilePath = Console.ReadLine();

            if (!File.Exists(jsonFilePath))
            {
                Console.WriteLine("File does not exist.");
                return;
            }

            // Read JSON file
            string jsonContent = File.ReadAllText(jsonFilePath);
            JObject jsonObject = JObject.Parse(jsonContent);

            // Extract BPM
            float bpm = jsonObject["bpm"].Value<float>();

            // Extract notes
            JArray notesArray = (JArray)jsonObject["notes"];
            List<MidiEvent> midiEvents = new List<MidiEvent>();

            int ppqn = 96; // Pulses per quarter note

            foreach (JObject noteObject in notesArray)
            {
                double timeInSeconds = noteObject["time"].Value<double>();
                int pitch = noteObject["pitch"].Value<int>();
                int velocity = noteObject["velocity"].Value<int>();
                double durationInSeconds = noteObject["duration"].Value<double>();

                double ticksPerSecond = (ppqn * bpm) / 60.0;
                long absoluteTime = (long)(timeInSeconds * ticksPerSecond);
                int noteLength = (int)(durationInSeconds * ticksPerSecond);

                // Note on event
                midiEvents.Add(new NoteOnEvent(absoluteTime, 1, pitch, velocity, noteLength));
                // Note off event
                midiEvents.Add(new NoteEvent(absoluteTime + noteLength, 1, MidiCommandCode.NoteOff, pitch, 0));
            }

            // Create MIDI file
            string midiFilePath = Path.Combine(Path.GetDirectoryName(jsonFilePath), "output.mid");
            MidiEventCollection midiEventCollection = new MidiEventCollection(1, ppqn);
            midiEventCollection.AddTrack(midiEvents);

            MidiFile.Export(midiFilePath, midiEventCollection);
            Console.WriteLine("MIDI file created: " + midiFilePath);
        }
    }
}
