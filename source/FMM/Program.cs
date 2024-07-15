using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NAudio.Midi;

namespace FMM
{
    internal static class NoteCameraCombiner
    {
        public static void CombineFiles(string notesFilePath, string cameraFilePath, string outputFilePath)
        {
            // Read notes.json
            JObject notesJson = JObject.Parse(File.ReadAllText(notesFilePath));

            // Read camera.txt
            List<int> cameraEvents = new List<int>(Array.ConvertAll(File.ReadAllLines(cameraFilePath), int.Parse));

            // Update mustHitSection based on camera.txt
            JArray notesSections = (JArray)notesJson["song"]["notes"];
            for (int i = 0; i < notesSections.Count; i++)
            {
                if (i < cameraEvents.Count)
                {
                    notesSections[i]["mustHitSection"] = cameraEvents[i] == 1;
                }
            }

            // Save updated notes.json to chart.json
            File.WriteAllText(outputFilePath, notesJson.ToString(Formatting.Indented));

            // Apply ChartPostfix logic
            ChartPostfix.FixChart(outputFilePath);
        }
    }

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
            List<string> messages = new List<string>
            {
                "no shit bro we funkin",
                "init release took me 4 days and it was worth it",
                "basically sniff but with midi import",
                "big thanks to mth for the base source code"
            };

            Random random = new Random();
            string selectedMessage = messages[random.Next(messages.Count)];

            Console.WriteLine("Funkin' MIDI Mapper [FMM] v1.0\n");
            Console.WriteLine("Created by BobbyDrawz_ [" + selectedMessage + "]\n");

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

                        string outputDirectory = Path.Combine(initialDirectory, songName);
                        Directory.CreateDirectory(outputDirectory);

                        string notesFilePath = Path.Combine(outputDirectory, "notes.json");
                        File.WriteAllText(notesFilePath, songJson.ToString(Formatting.Indented));
                        Console.WriteLine("---Generated base note chart.---");

                        FunkinCam.GenerateCameraFile(midiEvents, bpm, Path.Combine(outputDirectory, "camera.txt"));
                        string cameraFilePath = Path.Combine(outputDirectory, "camera.txt");
                        Console.WriteLine("---Generated camera text file.---");

                        // Combine files to create chart.json
                        NoteCameraCombiner.CombineFiles(notesFilePath, cameraFilePath, Path.Combine(outputDirectory, "chart.json"));
                        string chartFilePath = Path.Combine(outputDirectory, "chart.json");
                        Console.WriteLine("---Combined files to produce chart.json.---");

                        // Ask if the user wants to delete remnants
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
    }
}
