using System.IO;
using System.Threading.Tasks;
using WEB.Models;

namespace WEB.Reports.PDF
{
    public class TestReport : PDFReports
    {
        public TestReport(ApplicationDbContext db, Settings settings)
            : base(db, settings)
        {
        }

        public override async Task<byte[]> GenerateAsync()
        {
            #region load data
            
            #endregion

            byte[] bytes;
            using (ms = new MemoryStream())
            {
                CreateDoc(hasCoverPage: true);

                #region cover page
                var coverTable = CreateTable(new float[] { 25f, 75f }, spacingBefore: 0);

                coverTable.AddCell(CreateCell("Something here").SetStyle(CellStyle.Label));
                coverTable.AddCell(CreateCell("value").SetStyle(CellStyle.Text));

                AddCoverPage("Report Name", coverTable);
                #endregion;

                bytes = Close();
            }

            return bytes;
        }

        //private MemoryStream GetBarGraphImage(List<Municipality> municipalities, List<Fact> facts, Indicator indicator, Guid highlightMunicipalityId)
        //{
        //    var axisFont = new System.Drawing.Font(new FontFamily("Arial"), 20);
        //    var titleFont = new System.Drawing.Font(new FontFamily("Arial"), 24, FontStyle.Bold);
        //    var dataLabelFont = new System.Drawing.Font(new FontFamily("Arial"), 20);
        //    //var graphType = component.GetSettingAsString("graphType", String.Empty);
        //    //var dataLabel = component.GetSettingAsString("dataLabel", String.Empty);

        //    using (var chart = new Chart())
        //    {
        //        chart.Width = 1800;
        //        chart.Height = 900;
        //        //if (graphType == "StackedColumn100" || graphType == "StackedColumn")
        //        //    result.Rows.Reverse();

        //        var chartarea = new ChartArea();
        //        chart.ChartAreas.Add(chartarea);

        //        // title
        //        //var graphTitle = component.GetSettingAsString("graphTitle", String.Empty);
        //        //if (!String.IsNullOrWhiteSpace(graphTitle))
        //        //{
        //        //    chart.Titles.Add(graphTitle);
        //        //    chart.Titles[0].Docking = Docking.Top;
        //        //    chart.Titles[0].Font = new System.Drawing.Font("Arial", 20);
        //        //}

        //        // x-axis
        //        chartarea.AxisX.LabelStyle.Angle = -90;
        //        chartarea.AxisX.MajorGrid.Enabled = false;
        //        //chartarea.AxisX.IsLabelAutoFit = true;
        //        //chartarea.AxisX.LabelAutoFitMinFontSize = (int)axisFont.Size;
        //        chartarea.AxisX.IsLabelAutoFit = false;
        //        chartarea.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.None;
        //        chartarea.AxisX.LabelStyle.Font = axisFont;
        //        chartarea.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
        //        chartarea.AxisX.Interval = 1;
        //        //var xAxisTitle = component.GetSettingAsString("xAxisTitle", String.Empty);
        //        //if (!String.IsNullOrWhiteSpace(xAxisTitle))
        //        //{
        //        //    chartarea.AxisX.Title = xAxisTitle;
        //        //    chartarea.AxisX.TitleFont = titleFont;
        //        //}

        //        // y-axis
        //        chartarea.AxisY.Minimum = 0;
        //        chartarea.AxisY.MajorGrid.LineColor = System.Drawing.Color.Gray;
        //        chartarea.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
        //        chartarea.AxisY.MinorGrid.Enabled = false;
        //        chartarea.AxisY.MinorTickMark.Enabled = false;
        //        chartarea.AxisY.LabelStyle.Format = indicator.Format;
        //        //if (graphType == "StackedColumn100") chartarea.AxisY.LabelStyle.Format = Utilities.GetFormat("Percent", 0);
        //        chartarea.AxisY.LabelAutoFitStyle = LabelAutoFitStyles.None;
        //        chartarea.AxisY.LabelStyle.Font = axisFont;
        //        //var yAxisTitle = component.GetSettingAsString("yAxisTitle", String.Empty);
        //        //if (!String.IsNullOrWhiteSpace(xAxisTitle))
        //        //{
        //        //    chartarea.AxisY.Title = yAxisTitle;
        //        //    chartarea.AxisY.TitleFont = titleFont;
        //        //}

        //        var s = new Series();
        //        s.Name = indicator.Name;

        //        foreach (var municipality in municipalities)
        //        {
        //            var fact = facts.FirstOrDefault(o => o.MunicipalityId == municipality.MunicipalityId && o.IndicatorId == indicator.IndicatorId);
        //            //var total = result.ResultPoints.Where(p => p.ColumnKey == col.Key && p.Value.HasValue).Sum(p => Convert.ToDecimal(p.Value));
        //            //var resultPoint = result.ResultPoints.Single(p => p.ColumnKey == col.Key && p.RowKey == fact.Key);
        //            var dataPoint = new DataPoint();

        //            if (fact == null || fact.Value == null) dataPoint.IsEmpty = true;
        //            else dataPoint.YValues = new[] { (double)fact.Value };

        //            dataPoint.AxisLabel = municipality.ShortName;
        //            if (municipality.MunicipalityId == highlightMunicipalityId)
        //                dataPoint.Color = System.Drawing.Color.ForestGreen;

        //            dataPoint.LabelFormat = indicator.Format;
        //            //if (dataLabel == "value") dataPoint.IsValueShownAsLabel = true;
        //            //else if (dataLabel == "series") dataPoint.Label = fact.Label;

        //            dataPoint.Font = dataLabelFont;

        //            //if (dataLabel == "value" && graphType == "StackedColumn100") dataPoint.LabelFormat = Utilities.GetFormat("Percent", 0);
        //            //if (graphType.StartsWith("Stacked") && total != 0 && resultPoint.Value / total * 100 < 1) dataLabel = "";

        //            s.Points.Add(dataPoint);

        //        }
        //        chart.Series.Add(s);
        //        //if (graphType == "StackedColumn100") s.ChartType = SeriesChartType.StackedColumn100;
        //        //else if (graphType == "StackedColumn") s.ChartType = SeriesChartType.StackedColumn;

        //        // legend
        //        //var legendSetting = component.GetSettingAsString("legend", "Bottom");
        //        //if (legendSetting != "None")
        //        //{
        //        //    var legend = new Legend();
        //        //    switch (legendSetting)
        //        //    {
        //        //        case "Left":
        //        //            legend.Docking = Docking.Left;
        //        //            break;
        //        //        case "Top":
        //        //            legend.Docking = Docking.Top;
        //        //            break;
        //        //        case "Right":
        //        //            legend.Docking = Docking.Right;
        //        //            break;
        //        //        default:
        //        //            legend.Docking = Docking.Bottom;
        //        //            break;
        //        //    }

        //        //    if (graphType == "StackedColumn100" || graphType == "StackedColumn")
        //        //        legend.LegendItemOrder = LegendItemOrder.SameAsSeriesOrder;
        //        //    else
        //        //        legend.LegendItemOrder = LegendItemOrder.ReversedSeriesOrder;
        //        //    legend.IsEquallySpacedItems = true;
        //        //    legend.Alignment = StringAlignment.Center;
        //        //    legend.TitleAlignment = StringAlignment.Near;
        //        //    legend.Font = new System.Drawing.Font("Arial", 18);
        //        //    chart.Legends.Add(legend);
        //        //}

        //        // return
        //        var imageStream = new MemoryStream();
        //        chart.SaveImage(imageStream);
        //        return imageStream;
        //    }
        //}

        public override string GetReportName()
        {
            return "Municipality Comparison Report.pdf";
        }

        public override string GetContentType()
        {
            return PDFContentType;
        }
    }
}