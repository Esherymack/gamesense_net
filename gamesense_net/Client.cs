namespace gamesense_net
{
    public class Client
    {
        internal GameEvent GameEvent { get; set; }
        public Client(string game, string game_display_name, string developer, int? deinit_timer_length = null)
        {
            GameEvent = new GameEvent(game, game_display_name, developer, deinit_timer_length);
        }

        public void RunSetup(string eventName, string handlerJson)
        {
            GameEvent.RegisterGame();
            GameEvent.BindEvent(eventName, handlerJson);
        }

        public void RunEvent(string eventName, string dataJson)
        {
            GameEvent.SendEvent(eventName, dataJson);
        }

        public int GetNextValueFromCycler()
        {
            GameEvent.GetNewValueCycle();
            return GameEvent.ValueCycler;
        }
    }
}
