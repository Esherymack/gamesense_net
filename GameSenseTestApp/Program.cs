using gamesense_net;
using System;
using System.Threading.Tasks;
using WindowsMediaController;

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
                try
                {
                    var session = mediaMan.GetFocusedSession();
                    if (session == null)
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
                        var res = await session.ControlSession.TryGetMediaPropertiesAsync();

                        dataJson = $$"""
    "value": {{c.GetNextValueFromCycler()}},
    "frame": {
        "song-name": "{{res.Title}}",
        "album-name": "{{res.Artist}}"
    }
""";
                    }


                    c.RunEvent("SONGTEXT", dataJson);
                    await Task.Delay(1500);
                }
                catch(Exception ex)
                {
                    // Don't do anything if an exception is thrown
                }
                
            }
        }
    }
}

