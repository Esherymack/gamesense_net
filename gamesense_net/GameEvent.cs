using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;


namespace gamesense_net
{
    internal static class Endpoints
    {
        internal enum EndpointsEnum
        {
            [Endpoint("/game_metadata")]
            REGISTER_GAME,
            [Endpoint("/remove_game")]
            REMOVE_GAME,
            [Endpoint("/register_game_event")]
            REGISTER_EVENT,
            [Endpoint("/bind_game_event")]
            BIND_EVENT,
            [Endpoint("/remove_game_event")]
            REMOVE_EVENT,
            [Endpoint("/game_event")]
            SEND_EVENT,
            [Endpoint("/game_heartbeat")]
            HEARTBEAT
        }

        public static TAttribute GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
        {
            var enumType = value.GetType();
            var name = Enum.GetName(enumType, value);
            return enumType.GetField(name).GetCustomAttributes(false).OfType<TAttribute>().SingleOrDefault();
        }
    }

    public class EndpointAttribute : Attribute
    {
        public string Name { get; private set; }
        public EndpointAttribute(string name)
        {
            Name = name;
        }
    }

    internal class GameEvent
    {
        internal string Game { get; set; }
        internal string Game_Display_Name { get; set; }
        internal string Developer { get; set; }
        internal int Deinitialize_Timer_Length_Ms { get; set; }
        internal string JsonAddressString { get; private set; }
        internal int ValueCycler { get; set; }
        internal int Timeout { get; set; }

        public GameEvent(string? game, string? game_display_name, string? developer, int? deinit_timer_length_ms)
        {

            Game = game != null ? game : string.Empty;
            Game_Display_Name = game_display_name != null ? game_display_name : string.Empty;
            Developer = developer != null ? developer : string.Empty;
            Deinitialize_Timer_Length_Ms = (int)(deinit_timer_length_ms != null ? deinit_timer_length_ms : 0);
            FindSSE3Port();
            Timeout = 1;
        }

        public void FindSSE3Port()
        {
            // Open the coreProps.json file to parse what port SteelSeries Engine 3 is listening on 
            // (this is randomized to some unused port, so SteelSeries generates this file)
            string? jsonAddrString = string.Empty;
            string corePropsFileName = string.Empty;
            JObject jObject = new JObject();

            // We also need to determine which OS this is running on
            // I know I know this is a C# class library, but .NET 7 can run on MacOS so you never know.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                corePropsFileName = @"C:\ProgramData\SteelSeries\SteelSeries Engine 3\coreProps.json";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                corePropsFileName = @"/Library/Application Support/SteelSeries Engine 3/coreProps.json";
            }

            try
            {
                jObject = JObject.Parse(File.ReadAllText(corePropsFileName));
            }
            catch (FileNotFoundException fe)
            {
                Console.WriteLine(fe.StackTrace);
                Console.WriteLine("error: coreProps.json not found!");
            }
            catch (IOException ie)
            {
                Console.WriteLine(ie.StackTrace);
                Console.WriteLine("Unhandled exception.");
            }

            // Save the address to our SteelSeries Engine 3 for events
            try
            {
                if (jObject.HasValues)
                {
                    jsonAddrString = jObject?.SelectToken("address")?.ToString();
                    JsonAddressString = jsonAddrString != null ? jsonAddrString : string.Empty;
                    Console.WriteLine(JsonAddressString);
                }
            }
            catch (JsonException je)
            {
                Console.WriteLine(je.StackTrace);
                Console.WriteLine("Exception getting JSON address string from JObject.");
            }

        }

        public void GetNewValueCycle()
        {
            Random r = new Random();
            ValueCycler = r.Next(0, 100);
        }

        public void Post(string json, Endpoints.EndpointsEnum endpoint)
        {
            using (var client = new HttpClient())
            {
                MediaTypeWithQualityHeaderValue? contentType = new MediaTypeWithQualityHeaderValue("application/json");
                string baseAddress = $"http://{JsonAddressString}{endpoint.GetAttribute<EndpointAttribute>().Name}";

                client.BaseAddress = new Uri(baseAddress);
                client.DefaultRequestHeaders.Accept.Add(contentType);
                StringContent contentData = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage? response = client.Send(new HttpRequestMessage(HttpMethod.Post, baseAddress) { Content = contentData });

                if (response.IsSuccessStatusCode)
                {
                    //Console.WriteLine("Success!");
                }
            }
        }

        // Register the game with the SteelSeries engine.
        public void RegisterGame(bool reset = false)
        {
            if (reset)
            {
                UnregisterGame();
            }

            // JSON string for game metadata
            string json = $$"""
             {
             "game": "{{Game}}",
             "game_display_name": "{{Game_Display_Name}}",
             "developer": "{{Developer}}"
             }
             """;

            Post(json, Endpoints.EndpointsEnum.REGISTER_GAME);
        }

        // Unregister the game with the SteelSeries engine.
        public void UnregisterGame()
        {
            try
            {
                string json = $$"""
                {
                "game": "{{Game}}"
                }
                """;

                Post(json, Endpoints.EndpointsEnum.REMOVE_GAME);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("Some exception occurred when unregistering SteelSeries game object.");
            }
        }

        // Bind an event to this game object
        public void BindEvent(string eventName, string handler_json)
        {
            string json = $$"""
            {
                "game": "{{Game}}",
                "event": "{{eventName}}",
                "handlers": [
                {
                    {{handler_json}}
                }]
            }
            """;

            Post(json, Endpoints.EndpointsEnum.BIND_EVENT);
        }

        // Remove the given event from the game object
        public void RemoveEvent(string eventName)
        {
            string json = $$"""
            {
                "game": "{{Game}}",
                "event": "{{eventName}}"
            }
            """;

            Post(json, Endpoints.EndpointsEnum.REMOVE_EVENT);
        }

        // Send an event to the engine
        public void SendEvent(string eventName, string data)
        {
            string json = $$"""
            {
                "game": "{{Game}}",
                "event": "{{eventName}}",
                "data": {
                    {{data}}
                 }
            }
            """;

            Post(json, Endpoints.EndpointsEnum.SEND_EVENT);
        }

        // Set heartbeat
        public void SendHeartbeat()
        {
            string json = $$"""
            {
                "game": "{{Game}}"
            }
            """;
            Post(json, Endpoints.EndpointsEnum.HEARTBEAT);
        }

    }
}
