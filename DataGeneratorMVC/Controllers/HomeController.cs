using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using DataGeneratorMVC.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

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
    public IActionResult Index(GeneratorDataViewModel model, string cmd)
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
                }
            }
        }

        return View(model);
    }

    private void GenerateNew(GeneratorDataViewModel model)
    {
        var tmp = DataGenerator.Generate(model.Settings);
        var labelsSb = new StringBuilder();
        labelsSb.Append("[");
        var dataSb = new StringBuilder();
        dataSb.Append("[");
        
        // var allDataSets = new StringBuilder();
        //
        // string datasetTemplate = "{{label: '{0}', data: {1}, backgroundColor: [\'rgba({2}, {3}, {4}, 0.2)\'], borderColor: [\'rgba(255, 99, 132, 1)\'], fill: false, tension: 0.4, spangaps: true}}{5} ";
        //
        // double[,] years = new double[model.Settings.Years, 366];
        //
        // foreach (var turnover in tmp)
        // {
        //     years[turnover.Key.Year - model.Settings.StartYear, turnover.Key.DayOfYear - 1] = turnover.Value;
        // }
        //
        // for (int i = 0; i < model.Settings.Years; i++)
        // {
        //     var dataSb = new StringBuilder();
        //     dataSb.Append("[");
        //     for (int j = 0; j < 366; j++)
        //     {
        //         if(years[i, j] != 0)
        //             dataSb.Append($"{years[i, j].ToString("0.00").Replace(',', '.')}, ");
        //         else
        //             dataSb.Append($"{years[i, j-1].ToString("0.00").Replace(',', '.')}, ");
        //     }
        //     dataSb.Append("]");
        //     allDataSets.Append(string.Format(datasetTemplate, model.Settings.StartYear + i, 
        //         dataSb.ToString(), 255, 99 + i, 132 + i, i-1 == model.Settings.Years ? "" : ","));
        //     dataSb.Clear();
        // }
        //
        // for (int i = 0; i < 366; i++)
        // {
        //     labelsSb.Append($"{i}, ");
        // }

        int counter = 0;
        
        foreach (var value in tmp)
        {
            counter++;
            labelsSb.Append($"{counter}, ");
            //labels.Append($"{value.Key}, ");
            dataSb.Append($"{value.Value.ToString("0.00").Replace(',', '.')}, ");
        }

        labelsSb.Append("]");
        dataSb.Append("]");
        model.Sql = DataGenerator.Save(tmp);
        model.DataSet = dataSb.ToString();
        model.LabelSet = labelsSb.ToString();
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