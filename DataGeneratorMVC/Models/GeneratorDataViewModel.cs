using Microsoft.AspNetCore.Mvc;

namespace DataGeneratorMVC.Models;

public class GeneratorDataViewModel
{
    public GeneratorSettings Settings { get; set; }
    public string Sql { get; set; }
    public string DataSet { get; set; }
    public string LabelSet { get; set; }
}