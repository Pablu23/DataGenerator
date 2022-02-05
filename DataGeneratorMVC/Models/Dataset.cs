using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace DataGeneratorMVC.Models;

public class Dataset
{
    [JsonProperty("label")]
    public string Label { get; set; }
    [JsonProperty("data")]
    public ICollection<double> Data { get; set; }
    [JsonProperty("backgroundColor")]
    public string? BackgroundColor { get; set; }
    [JsonProperty("borderColor")]
    public string? BorderColor { get; set; }
    [JsonProperty("fill")]
    public bool? Fill { get; set; }
    [JsonProperty("tension")]
    public float? Tension { get; set; }
    [JsonProperty("spangaps")]
    public bool? Spangaps { get; set; }

    public Dataset(string label, ICollection<double> data)
    {
        Label = label;
        Data = data;
    }
    public Dataset(string label, ICollection<double> data, string backgroundColor, string borderColor, bool fill, float tension, bool spangaps)
    {
        Label = label;
        Data = data;
        BackgroundColor = backgroundColor;
        BorderColor = borderColor;
        Fill = fill;
        Tension = tension;
        Spangaps = spangaps;
    }
}