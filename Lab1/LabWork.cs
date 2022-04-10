namespace Lab1;

public class LabWork
{
    public IReadOnlyList<int> RejectionRates { get; init; }

    public double Gamma { get; init; }

    public int HoursForFaultless { get; init; }

    public int HoursForIntensity { get; init; }

    public int IntervalsCount { get; init; } = 10;

    private double Tcp => Convert.ToDouble(RejectionRates.Sum()) / RejectionRates.Count();

    private double IntervalLength => Convert.ToDouble(RejectionRates.Max()) / IntervalsCount;

    public void Calculate()
    {
        Console.WriteLine($"Tcp: {Tcp}");
        var (t, statDensities) = GetT();
        Console.WriteLine($"Ty when y = {Gamma}, {t}");
        Console.WriteLine($"Without failures {HoursForFaultless} hours, {ProbFaultless(statDensities)}");
        Console.WriteLine($"Hours of intensity {HoursForIntensity}, {FailIntensity(statDensities)}");
    }

    private (double t, List<double> statDensities) GetT()
    {
        var populatedIntervals = new List<List<int>>();
        var statDensities = new List<double>();
        var probabilities = new List<double>();

        double intervalStart = 0;
        var len = IntervalLength;
        foreach (var intervalIdx in Enumerable.Range(0, IntervalsCount))
        {
            var interval = new List<int>();
            var intervalEnd = intervalStart + len;
            // Console.WriteLine($"{intervalIdx}-інтервал від {intervalStart} до {intervalEnd}");
            interval.AddRange(RejectionRates.Where(r => r >= intervalStart && r <= intervalEnd));
            intervalStart = intervalEnd;
            populatedIntervals.Add(interval);
            statDensities.Add(Convert.ToDouble(interval.Count) / (RejectionRates.Count * IntervalLength));
        }

        double areaSum = 1;

        foreach (var idx in Enumerable.Range(0, IntervalsCount))
        {
            probabilities.Add(areaSum);
            areaSum -= statDensities[idx] * IntervalLength;
        }

        var probLess = probabilities.Where(p => p < Gamma).Max();
        var probMore = probabilities.Where(p => p > Gamma).Min();

        return (
            probabilities.IndexOf(probMore) + IntervalLength * (probMore - Gamma) / (probMore - probLess),
            statDensities);
    }

    private double ProbFaultless(List<double> statDensities)
    {
        double sum = 1;
        var fullIntervals = Convert.ToInt32(HoursForFaultless / IntervalLength);

        for (var i = 0; i < fullIntervals; i++)
        {
            sum -= statDensities[i] * IntervalLength;
        }

        sum -= statDensities[fullIntervals] * (HoursForFaultless % IntervalLength);

        return sum;
    }

    private double FailIntensity(List<double> statDensities) =>
        statDensities[Convert.ToInt32(Math.Floor(HoursForIntensity / IntervalLength))] / ProbFaultless(statDensities);
}