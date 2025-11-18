using ScottPlot;

namespace TelegramBotManager.Common.Helpers;

public static class ScottPlotHelper
{
    public static async Task<string> GeneratePieChart(List<Tuple<string, double>> tupleOfCategoryAndValue)
    {
        ScottPlot.Plot myPlot = new();

        double total = tupleOfCategoryAndValue.Sum(x => x.Item2);

        var pieSlices =
            tupleOfCategoryAndValue.GroupBy(x => x.Item1)
                                   .Select(cureentCategory =>
                                        new PieSlice()
                                        {
                                            Label = string.Empty,
                                            Value = cureentCategory.Sum(x => x.Item2),
                                            FillColor = Color.RandomHue(),
                                            LabelFontSize = 20,
                                            LegendText = $"{cureentCategory.Key} " + $"({cureentCategory.Sum(x => x.Item2) / total:p1})",
                                        })
                                   .ToList();
        pieSlices =
            pieSlices.OrderByDescending(p => p.Value)
                     .ToList();

        pieSlices.ForEach(x => x.FillColor = RandomHue(pieSlices));

        pieSlices.ForEach(x => x.LabelFontColor = x.FillColor.Darken(.5));

        var pie = myPlot.Add.Pie(pieSlices);
        pie.ExplodeFraction = 0.1;
        pie.SliceLabelDistance = 0.1;

        myPlot.Axes.Frameless();
        myPlot.HideGrid();


        string fontPath =
            Path.Combine(
                FileHelper.AppContextPath,
                "Common",
                "Assets",
                "Fonts",
                "arialnarrowbold.ttf");

        Fonts.AddFontFile("Arial Narrow", fontPath, bold: false, italic: false);

        myPlot.Legend.FontName = "Arial Narrow";
        myPlot.Legend.FontSize = 16;
        myPlot.ShowLegend(Edge.Bottom);

        var imagePath = Path.Combine(FileHelper.TempPath, $"{DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss")}.png");
        myPlot.SavePng(imagePath, 400, 300);

        return imagePath;
    }

    private static Color RandomHue(List<PieSlice> pieSlices)
    {
        Random random = new Random();
        var newColor =
            new Color(random.Next(256), random.Next(256), random.Next(256));

        int attempts = 0;

        while (pieSlices.Any(y => y.FillColor == newColor) && attempts < 5)
        {
            attempts++;

            newColor =
                Color.RandomHue();
        }

        return newColor;
    }
}
