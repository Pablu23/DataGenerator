using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using DataGeneratorMVC.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using JsonConverter = System.Text.Json.Serialization.JsonConverter;

namespace DataGeneratorMVC.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var model = new GeneratorDataViewModel();
        model.Settings = new GeneratorSettings(2019, 3, 3000);
        GenerateNew(model);
        return View(model);
    }

    [HttpPost]
    public IActionResult Index(GeneratorDataViewModel model, string cmd, IFormFile? file)
    {
        if (ModelState.IsValid)
        {
            if (!string.IsNullOrWhiteSpace(cmd))
            {
                switch (cmd)
                {
                    case "generate":
                        GenerateNew(model);
                        break;
                    case "save":
                        return File(Encoding.UTF8.GetBytes(model.Sql), "text/plain", "sql_insert.sql");
                    case "upload":
                        Upload(model, file);
                        break;
                }
            }
        }

        return View(model);
    }
    
    private void Upload(GeneratorDataViewModel model, IFormFile up)
    {

        var rgx = new Regex(@"(?<=\()(.*)(?=\))"); // Match only values of SQL Statement
        var dict = new Dictionary<DateTime, double>();

        using (var reader = new StreamReader(up.OpenReadStream()))
        {
            while (reader.Peek() >= 0)
            {
                string line = reader.ReadLine();
                line = rgx.Match(line).Value;
                string[] inputs = line.Split(",");
                dict.Add(DateTime.Parse(inputs[1].Replace("\'", "")), Double.Parse(inputs[2]));
            }
        }
        
        //var model = new GeneratorDataViewModel();
        GenerateNew(model, dict);
    }
    
    private void GenerateNew(GeneratorDataViewModel model, Dictionary<DateTime, double>? input = null)
    {
        Dictionary<DateTime, double> tmp;
        if (input != null)
            tmp = input;
        else
            tmp = DataGenerator.Generate(model.Settings);
        
        
        var labels = new List<double>();
        
        for (int i = 0; i < tmp.Count; i++)
        {
            labels.Add(i);
        }

        var datasets = new List<Dataset>();
        datasets.Add(new Dataset("Umsatz", tmp.Values, "rgba(255, 99, 132, 0.2)", "rgba(255, 99, 132, 1)", false, 0.4f, true));
        
        var trendline = new Trendline( new List<double>(tmp.Values), labels);

        var average = new Collection<double>();
        var minimum = new Collection<double>();
        var maximum = new Collection<double>();
        var summed = new Collection<double>();
        var trend = new Collection<double>();
        var trendStandardDeviationUp = new Collection<double>();
        var trendStandardDeviationDown = new Collection<double>();

        
        double stdDeviation = StandardDeviation(tmp.Values);
        double avg = tmp.Values.Average();
        double min = tmp.Values.Min();
        double max = tmp.Values.Max();
        
        for (int i = 0; i < labels.Count; i++)
        {
            double trendValue = trendline.GetYValue(i);
            
            average.Add(avg);
            minimum.Add(min);
            maximum.Add(max);
            
            trendStandardDeviationUp.Add(trendValue + stdDeviation);
            trendStandardDeviationDown.Add(trendValue - stdDeviation);
            trend.Add(trendValue);
        }
        
        datasets.Add(new Dataset("Trendlinie", trend){BorderColor = "rgba(11,127,171)"});
        datasets.Add(new Dataset("Standard Deviation Up", trendStandardDeviationUp){BorderColor = "rgba(0,181,204)"});
        datasets.Add(new Dataset("Standard Deviation Down", trendStandardDeviationDown){BorderColor = "rgba(0,181,204)"});
        datasets.Add(new Dataset("Average", average){BorderColor = "rgba(8,14,44)"});
        datasets.Add(new Dataset("Min", minimum){BorderColor = "rgba(8,14,44)"});
        datasets.Add(new Dataset("Max", maximum){BorderColor = "rgba(8,14,44)"});


        model.Sql = DataGenerator.Save(tmp);
        model.DataSet = JsonConvert.SerializeObject(datasets, new JsonSerializerSettings(){NullValueHandling = NullValueHandling.Ignore});
        model.LabelSet = JsonConvert.SerializeObject(labels);
    }

    private static double StandardDeviation(IEnumerable<double> sequence)
    {
        double result = 0;

        if (sequence.Any())
        {
            double average = sequence.Average();
            double sum = sequence.Sum(d => Math.Pow(d - average, 2));
            result = Math.Sqrt((sum) / (sequence.Count() - 1));
        }
        return result;
    }
    
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
}