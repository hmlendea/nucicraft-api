namespace NuciCraft.API.DataAccess.DataObjects
{
    public sealed class CoordinatesDataObject
    {
        public string World { get; set; }

        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public float Pitch { get; set; } = 0.0f;

        public float Yaw { get; set; } = 179.9f;
    }
}
