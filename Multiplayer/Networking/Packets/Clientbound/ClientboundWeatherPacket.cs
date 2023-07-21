using System;

namespace Multiplayer.Networking.Packets.Clientbound;

[Serializable]
public class ClientboundWeatherPacket
{
    public double OADate { get; set; }
    public float WeatherOffset { get; set; }
    public float Wetness { get; set; }
    public float StartingWeatherTransitionStart { get; set; }
    public float StartingWeatherTransitionEnd { get; set; }
    public float StartingWeatherX { get; set; }
    public float StartingWeatherY { get; set; }
    public float StartingWeatherRain { get; set; }
    public float StartingWeatherThunder { get; set; }
    public float StartingWeatherWetness { get; set; }
}
