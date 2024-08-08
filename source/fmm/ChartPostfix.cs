using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FMM
{
    internal static class ChartPostfix
    {
        public static void FixChart(string chartFilePath)
        {
            JObject chartJson = JObject.Parse(File.ReadAllText(chartFilePath));
            JArray notesSections = (JArray)chartJson["song"]["notes"];

            foreach (JObject section in notesSections)
            {
                bool mustHitSection = section["mustHitSection"].Value<bool>();
                if (!mustHitSection)
                {
                    JArray sectionNotes = (JArray)section["sectionNotes"];

                    for (int i = 0; i < sectionNotes.Count; i++)
                    {
                        JArray noteArray = (JArray)sectionNotes[i];
                        int noteType = noteArray[1].Value<int>();

                        noteType = noteType switch
                        {
                            0 => 4,
                            1 => 5,
                            2 => 6,
                            3 => 7,
                            4 => 0,
                            5 => 1,
                            6 => 2,
                            7 => 3,
                            _ => noteType
                        };

                        noteArray[1] = noteType;
                    }
                }
            }

            File.WriteAllText(chartFilePath, chartJson.ToString(Formatting.Indented));
        }
    }
}
