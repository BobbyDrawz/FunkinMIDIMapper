using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAudio.Midi;

namespace FMM
{
    public static class FunkinCam
    {
        public static void GenerateCameraFile(List<MidiEvent> midiEvents, float bpm, string cameraFilePath)
        {
            List<int> cameraEvents = new List<int>();
            bool mustHitSection = true;
            long lastBarStart = 0;

            foreach (var midiEvent in midiEvents)
            {
                if (midiEvent.AbsoluteTime >= lastBarStart + Globals.ppqn * 4)
                {
                    cameraEvents.Add(mustHitSection ? 1 : 0);
                    lastBarStart += Globals.ppqn * 4;
                }

                if (midiEvent is NoteOnEvent noteOn)
                {
                    if (noteOn.NoteNumber == 53)
                    {
                        mustHitSection = true;
                    }
                    if (noteOn.NoteNumber == 54)
                    {
                        mustHitSection = false;
                    }
                }
            }

            // Duplicate last camera event if there are any remaining sections without events
            if (cameraEvents.Count < (lastBarStart / (Globals.ppqn * 4)))
            {
                int lastEvent = cameraEvents.Last();
                while (cameraEvents.Count < (lastBarStart / (Globals.ppqn * 4)))
                {
                    cameraEvents.Add(lastEvent);
                }
            }

            // Write camera events to camera.txt
            File.WriteAllLines(cameraFilePath, cameraEvents.Select(e => e.ToString()).ToArray());
        }
    }
}
