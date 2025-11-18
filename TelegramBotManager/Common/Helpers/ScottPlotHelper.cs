using ScottPlot;

namespace TelegramBotManager.Common.Helpers;

public static class ScottPlotHelper
{
    public static async Task<string> GeneratePieChart(List<Tuple<string, double>> tupleOfCategoryAndValue)
    {
        ScottPlot.Plot myPlot = new();
        var pieSlices =
            tupleOfCategoryAndValue.GroupBy(x => x.Item1)
                                   .Select(pie =>
                                        new PieSlice()
                                        {
                                            Label = pie.Key,
                                            Value = pie.Sum(x => x.Item2),
                                            FillColor = Color.RandomHue()
                                        })
                                   .ToList();

        var pie = myPlot.Add.Pie(pieSlices);
        pie.ExplodeFraction = 0.1;
        pie.SliceLabelDistance = 0.1;

        pieSlices.ForEach(x => x.LabelFontColor = x.FillColor.Darken(.5));

        double total = pie.Slices.Select(x => x.Value).Sum();
        for (int i = 0; i < pie.Slices.Count; i++)
        {
            string currentLabel = pie.Slices[i].Label.ToString();

            pie.Slices[i].LabelFontSize = 20;
            pie.Slices[i].LegendText = $"{currentLabel} " +
                $"({pie.Slices[i].Value / total:p1})";
            pie.Slices[i].Label = string.Empty;
        }

        myPlot.ShowLegend();

        myPlot.Axes.Frameless();
        myPlot.HideGrid();

        var imagePath = Path.Combine(FileHelper.GetFolderPath, $"{DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss")}.png");
        myPlot.SavePng(imagePath, 400, 300);

        return imagePath;
    }
}
