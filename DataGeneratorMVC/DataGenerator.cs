using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace DataGeneratorMVC;

//[Bind("StartYear,Years,MinDiff,MaxDiff,HasSin,Smoothing,RSmoothing,MinSmoothing,MaxSmoothing,StartTurnover,Linear,HasPeak,PeakLength,PeakStrength,RPeakInYear,PeakInYear")]
public class GeneratorSettings : IValidatableObject
{
    [Range(1500,3000)]
    public int StartYear { get; set; }
    [Range(1,2000)]
    public int Years { get; set; }
    [Range(int.MinValue, 0)]
    public int MinDiff { get; set; }
    [Range(0, int.MaxValue)]
    public int MaxDiff { get; set; }
    [Range(1,10)]
    public int Smoothing { get; set; }
    public bool RSmoothing { get; set; }
    [Range(1,10)]
    public int MinSmoothing { get; set; }
    [Range(1,10)]
    public int MaxSmoothing { get; set; }
    [DataType(DataType.Currency)]
    public double StartTurnover { get; set; }
    public double Linear { get; set; }
    public bool HasSin { get; set; }
    public double SinStrength { get; set; }
    public double SinLength { get; set; }
    public bool SinNegative { get; set; }
    public bool HasPeak { get; set; }
    public double PeakLength { get; set; }
    public double PeakStrength { get; set; }
    public bool RPeakInYear { get; set; }
    public int PeakInYear { get; set; }
    
    public GeneratorSettings()
    {
    }
    
    public GeneratorSettings(int startYear, int years, int startTurnover = 1000, int minDiff = -100, int maxDiff = 100,
        int smoothing = 3, bool rSmoothing = false, int minSmoothing = 1,
        int maxSmoothing = 10)
    {
        StartYear = Clamp(startYear, 1999, 3000);
        Years = Clamp(years, 1, 100);
        MinDiff = minDiff;
        MaxDiff = maxDiff;
        Smoothing = smoothing;
        RSmoothing = rSmoothing;
        MinSmoothing = Clamp(minSmoothing, 1, 100);
        MaxSmoothing = Clamp(maxSmoothing, 1, 100);
        StartTurnover = startTurnover;
        SinLength = 182.5;
        SinStrength = 10;
        SinNegative = true;
    }

    private static int Clamp(int value, int min, int max)
    {
        return (value < min) ? min : (value > max) ? max : value;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (HasPeak && (PeakLength < 1 || PeakStrength < 1 || RPeakInYear == false && 1 > PeakInYear && PeakInYear < 360))
            yield return new ValidationResult("If Peak is enabled, all Peak settings must be set", new []{nameof(HasPeak), nameof(PeakLength), nameof(PeakStrength), nameof(RPeakInYear), nameof(PeakInYear)});
        
        if (HasSin && (SinLength < 1 || SinStrength < 1))
            yield return new ValidationResult("If Sin is enabled, all Sin settings must be set", new []{nameof(HasSin), nameof(SinLength), nameof(SinStrength)});
    }
}

public static class DataGenerator
{
    private static Random _r = new Random();
    public static Dictionary<DateTime, double> Generate(GeneratorSettings settings)
    {
        double currentCurve;
        var turnoverPerDay = new Dictionary<DateTime, double>();

        Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
        
        int minDiff = settings.MinDiff;
        int maxDiff = settings.MaxDiff;
        int smoothing = settings.Smoothing;

        int counter = 0;
        double linear = settings.Linear;
        
        int peakInYear = settings.RPeakInYear ? _r.Next(10, 350) : settings.PeakInYear;
        double peakLength = settings.PeakLength;
        double peakStrength = settings.PeakStrength;
        int peakCounter = 0;

        for (int year = 0; year < settings.Years; year++)
        {
            // Turn the added years into a real year
            int curYear = year + settings.StartYear;
            peakCounter = 0;
            
            // For twelve Months
            for (int month = 1; month < 13; month++)
            {
                // For the Days in the Month of the Year
                for (int day = 1; day <= DateTime.DaysInMonth(curYear, month); day++)
                {
                    counter++;
                    // Reset the current Curve to not let it get beyond a specific point
                    currentCurve = 0;

                    if (settings.HasPeak)
                    {
                        if (peakInYear <= DateTime.Parse($"{day}/{month}/{curYear}").DayOfYear &&
                            peakInYear + peakLength >= DateTime.Parse($"{day}/{month}/{curYear}").DayOfYear)
                        {
                            peakCounter++;
                            currentCurve += peakStrength * Math.Sin(peakCounter * Math.PI / ((float) peakLength / 2));
                        }
                    }

                    if (settings.RSmoothing)
                    {
                        smoothing = _r.Next(settings.MinSmoothing, settings.MaxSmoothing + 1);
                        maxDiff = settings.MaxDiff / smoothing;
                        minDiff = settings.MinDiff / smoothing;
                    }

                    // Smoothing works because of the bell curve
                    for (int k = 0; k < smoothing; k++)
                        currentCurve += _r.Next(minDiff, maxDiff + 1);

                    if(settings.HasSin)
                        currentCurve += settings.SinNegative switch
                        {
                            true => -(settings.SinStrength * Math.Sin(counter * Math.PI / settings.SinLength)),
                            false => (settings.SinStrength * Math.Sin(counter * Math.PI / settings.SinLength)) //182.5
                        };

                    currentCurve += linear;

                    // If it is the first Day of the Simulation start
                    // to start from the startTurnover and not the day before
                    if (month == 1 && year == 0 && day == 1)
                        // Add the first Day to the dictionary with the starting Turnover + a little random Curve
                        turnoverPerDay.Add(DateTime.Parse($"{day}/{month}/{curYear}"),
                            settings.StartTurnover + currentCurve);
                    else
                    {
                        // Add a new Day to the dictionary with the random Curve to the last Days turnover
                        var newDate = DateTime.Parse($"{day}/{month}/{curYear}");
                        turnoverPerDay.Add(newDate, turnoverPerDay[newDate.AddDays(-1)] + currentCurve);
                    }
                }
            }
        }

        return turnoverPerDay;
    }

    public static string Save(Dictionary<DateTime, double> turnoverPerDay)
    {
        var insertSb = new StringBuilder();
        
        foreach (var x in turnoverPerDay)
            insertSb.Append($"insert into umsatz values(0,'{x.Key.ToString("yyyy-MM-dd")}',{x.Value.ToString().Replace(',', '.')});\n");

        return insertSb.ToString();
    }
}