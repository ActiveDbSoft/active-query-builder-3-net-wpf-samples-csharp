//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2025 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Data;
using System.Drawing;
using System.Windows;
using ActiveQueryBuilder.View.WPF.Annotations;
using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.Document.Section;
using GrapeCity.ActiveReports.SectionReportModel;
using Color = System.Drawing.Color;

namespace GrapeCityExtension
{
    /// <summary>
    /// Interaction logic for ActiveReportsWindow.xaml
    /// </summary>
    public partial class ActiveReportsWindow
    {
        private DataTable DataTable { get; set; }
        public ActiveReportsWindow([NotNull] DataTable dataTable)
        {

            DataTable = dataTable;
            InitializeComponent();
        }

        private SectionReport _sectionReport;

        private void ActiveReportsWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _sectionReport = new SectionReport { DataSource = DataTable };

            _sectionReport.Document.Printer.PrinterName = "";
            _sectionReport.PageSettings.Margins.Left = 0.1f;
            _sectionReport.PageSettings.Margins.Right = 0.1f;
            _sectionReport.PrintWidth = 15F;

            _sectionReport.Sections.Add(SectionType.ReportHeader, "ReportHeader");
            _sectionReport.Sections.Add(SectionType.Detail, "Detail");
            _sectionReport.Sections.Add(SectionType.ReportFooter, "ReportFooter");

            var section1 = _sectionReport.Sections[1];
            section1.CanGrow = true;
            section1.CanShrink = true;

            float locationX = 0;

            foreach (DataColumn dataSetColumn in DataTable.Columns)
            {
                var labelHeader = new Label
                {
                    Text = dataSetColumn.ColumnName,
                    Alignment = GrapeCity.ActiveReports.Document.Section.TextAlignment.Left,
                    Location = new PointF(locationX, 0.0F),
                    ShrinkToFit = false,
                    MinCondenseRate = 100,
                    BackColor = Color.Gainsboro,
                    Width = 2
                };
                _sectionReport.Sections[0].Controls.Add(labelHeader);

                var ctl = new TextBox
                {
                    Name = dataSetColumn.ColumnName,
                    Text = dataSetColumn.ColumnName,
                    DataField = dataSetColumn.ColumnName,
                    Location = new PointF(locationX, 0.05F),
                    ShrinkToFit = false,
                    WrapMode = GrapeCity.ActiveReports.Document.Section.WrapMode.NoWrap,
                    CanShrink = false,
                };

                ctl.Border.BottomStyle = BorderLineStyle.Dash;

                _sectionReport.Sections[1].Controls.Add(ctl);

                locationX += ctl.Width;
            }

            ReportViewer.LoadDocument(_sectionReport);
        }
    }
}
