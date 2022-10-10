using System.Text.Json.Serialization;

namespace HuntStats.Models;

public class Dataset
{
    [JsonPropertyName("label")]
    public string Label { get; set; }

    [JsonPropertyName("data")]
    public List<int> Data { get; set; }
    
    [JsonPropertyName("backgroundColor")]
    public List<string> BackgroundColor { get; set; }
    
    [JsonPropertyName("hoverOffset")]
    public int HoverOffset { get; set; }

    [JsonPropertyName("fill")]
    public bool Fill { get; set; }

    [JsonPropertyName("borderColor")]
    public string BorderColor { get; set; }

    [JsonPropertyName("tension")]
    public double Tension { get; set; }
    
    [JsonPropertyName("pointRadius")]
    public double PointRadius { get; set; }
}

