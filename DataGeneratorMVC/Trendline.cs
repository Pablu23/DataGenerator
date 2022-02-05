namespace DataGeneratorMVC;

public class Trendline
{
    public Trendline(IList<double> yAxisValues, IList<double> xAxisValues)
        : this(yAxisValues.Select((t, i) => new Tuple<double, double>(xAxisValues[i], t)))
    { }
    public Trendline(IEnumerable<Tuple<double, double>> data)
    {
        var cachedData = data.ToList();

        int n = cachedData.Count;
        double sumX = cachedData.Sum(x => x.Item1);
        double sumX2 = cachedData.Sum(x => x.Item1 * x.Item1);
        double sumY = cachedData.Sum(x => x.Item2);
        double sumXY = cachedData.Sum(x => x.Item1 * x.Item2);

        //b = (sum(x*y) - sum(x)sum(y)/n)
        //      / (sum(x^2) - sum(x)^2/n)
        Slope = (sumXY - ((sumX * sumY) / n))
                / (sumX2 - (sumX * sumX / n));

        //a = sum(y)/n - b(sum(x)/n)
        Intercept = (sumY / n) - (Slope * (sumX / n));

        Start = GetYValue(cachedData.Min(a => a.Item1));
        End = GetYValue(cachedData.Max(a => a.Item1));
    }

    public double Slope { get; private set; }
    public double Intercept { get; private set; }
    public double Start { get; private set; }
    public double End { get; private set; }

    public double GetYValue(double xValue)
    {
        return Intercept + Slope * xValue;
    }
}