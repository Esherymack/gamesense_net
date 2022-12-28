using gamesense_net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Windows.Media;
using Windows.Media.Control;
using WindowsMediaController;
using static WindowsMediaController.MediaManager;

namespace GameSenseTestApp
{
    internal class Program
    {
        private static Client c;
        private static MediaManager mediaMan;

        public static async Task Main()
        {
            c = new Client("OLED_DISP", "OLED Display", "Esherymack");

            mediaMan = new MediaManager();

            await mediaMan.StartAsync();
            
            string handlerJson = $$"""
    "datas":[
    {
        "lines":[
        {    
            "has-text": true,
            "context-frame-key": "song-name"
        },
        {
            "has-text": true,
            "context-frame-key": "album-name"
        }]
    }],
    "device-type": "screened",
    "mode": "screen",
    "zone": "one"
""";

            c.RunSetup("SONGTEXT", handlerJson);
            string dataJson = string.Empty;

            while (true)
            {
                var session = mediaMan.GetFocusedSession();
                var res = await session.ControlSession.TryGetMediaPropertiesAsync();

                if(res.Title == string.Empty || res.Title == null)
                {
                    dataJson = $$"""
    "value": {{c.GetNextValueFromCycler()}},
    "frame": {
        "song-name": "Nothing Playing",
        "album-name": " "
    }
""";
                }
                else
                {
                    dataJson = $$"""
    "value": {{c.GetNextValueFromCycler()}},
    "frame": {
        "song-name": "{{res.Title}}",
        "album-name": "{{res.Artist}}"
    }
""";
                }
               
                c.RunEvent("SONGTEXT", dataJson);
            }
        }
    }
}

