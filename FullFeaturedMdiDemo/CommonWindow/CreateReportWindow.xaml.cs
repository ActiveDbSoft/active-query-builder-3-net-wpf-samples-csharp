//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2024 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//


using System.Windows;
using FullFeaturedMdiDemo.Common;

namespace FullFeaturedMdiDemo.CommonWindow
{
    public partial class CreateReportWindow
    {
        public static readonly DependencyProperty SelectedReportTypeProperty = DependencyProperty.Register(
            "SelectedReportType", typeof(ReportType?), typeof(CreateReportWindow), new PropertyMetadata(null));

        public ReportType? SelectedReportType
        {
            get { return (ReportType?) GetValue(SelectedReportTypeProperty); }
            set { SetValue(SelectedReportTypeProperty, value); }
        }
        public CreateReportWindow()
        {
            InitializeComponent();
        }

        private void ReportButton_OnChecked(object sender, RoutedEventArgs e)
        {
            if (RadioButtonActiveReports.IsChecked == true)
            {
               SelectedReportType = ReportType.ActiveReports14;
                return;
            }

            if (RadioButtonStimulsoft.IsChecked == true)
            {
                SelectedReportType = ReportType.Stimulsoft;
                return;
            }

            if (RadioButtonFastReport.IsChecked == true)
            {
                SelectedReportType = ReportType.FastReport;
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
