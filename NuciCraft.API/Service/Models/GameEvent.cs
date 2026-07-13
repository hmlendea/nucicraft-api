namespace NuciCraft.API.Service.Models
{
    public class GameEvent
    {
        public string GameEventType { get; set; }

        public string Player { get; set; }

        public string World { get; set; }

        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }
    }
}
