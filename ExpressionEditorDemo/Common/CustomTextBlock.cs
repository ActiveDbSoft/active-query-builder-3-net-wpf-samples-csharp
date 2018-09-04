//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2018 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ActiveQueryBuilder.View.WPF.Annotations;

namespace ExpressionEditorDemo.Common
{
    internal enum StyleSelection { Rectangle, Underline }

    internal class CustomTextBlock : Control, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Typeface Typeface { get { return new Typeface(FontFamily, FontStyle, FontWeight, FontStretch); } }

        private Rect FinalRectText
        {
            get
            {
                var formated = new FormattedText(string.IsNullOrEmpty(Text) ? "" : Text, CultureInfo.CurrentCulture,
                    FlowDirection,
                    Typeface, FontSize,
                    Foreground);
                return new Rect(0, 0, formated.WidthIncludingTrailingWhitespace, formated.Height);
            }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string),
            typeof(CustomTextBlock), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty SelectionAreasProperty = DependencyProperty.Register(
            "SelectionAreas", typeof(IList<SelectionArea>), typeof(CustomTextBlock), new FrameworkPropertyMetadata(default(IList<SelectionArea>), FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StyleSelectionProperty = DependencyProperty.Register("StyleSelection",
           typeof(StyleSelection), typeof(CustomTextBlock),
           new FrameworkPropertyMetadata(StyleSelection.Rectangle, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StringToUnderscoreProperty = DependencyProperty.Register(
            "StringToUnderscore", typeof(string), typeof(CustomTextBlock), new PropertyMetadata(default(string), StringToUnderscore_Changed));

        private static void StringToUnderscore_Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var current = dependencyObject as CustomTextBlock;

            if (current == null) return;

            current.RecalculationSelectionAreas();
        }

        public string StringToUnderscore
        {
            get { return (string)GetValue(StringToUnderscoreProperty); }
            set { SetValue(StringToUnderscoreProperty, value); }
        }

        public IList<SelectionArea> SelectionAreas
        {
            get { return (IList<SelectionArea>)GetValue(SelectionAreasProperty); }
            set { SetValue(SelectionAreasProperty, value); }
        }

        public CustomTextBlock()
        {
            SelectionAreas = new List<SelectionArea>();
        }

        public void RecalculationSelectionAreas()
        {
            SelectionAreas.Clear();
            if (string.IsNullOrEmpty(StringToUnderscore))
            {
                InvalidateVisual();
                return;
            }

            var startIndex = 0;

            while (startIndex + StringToUnderscore.Length <= Text.Length)
            {
                var index = Text.IndexOf(StringToUnderscore, startIndex, StringComparison.OrdinalIgnoreCase);

                if (index == -1)
                    break;

                SelectionAreas.Add(new SelectionArea(index, StringToUnderscore.Length));
                startIndex = index + StringToUnderscore.Length;
            }

            InvalidateVisual();
        }

        public StyleSelection StyleSelection
        {
            set
            {
                SetValue(StyleSelectionProperty, value);
                OnPropertyChanged("StyleSelection");
                InvalidateVisual();
            }
            private get { return (StyleSelection)GetValue(StyleSelectionProperty); }
        }

        public string Text
        {
            set
            {
                SetValue(TextProperty, value);
                OnPropertyChanged("Text");
                InvalidateVisual();
            }
            get { return (string)GetValue(TextProperty); }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var textformat = new FormattedText(
                string.IsNullOrEmpty(Text) ? "" : Text,
                CultureInfo.CurrentCulture,
                FlowDirection,
                Typeface, FontSize,
                Foreground);

            if (SelectionAreas == null || SelectionAreas.Count == 0)
            {
                drawingContext.DrawText(textformat, new Point(0, 0));
                return;
            }

            var chunks = new Dictionary<FormattedText, bool>();
            var startPosition = 0;
            foreach (var selectionArea in SelectionAreas)
            {
                var selection = Text.Substring(selectionArea.Start, selectionArea.Length);
                var textBefore = Text.Substring(startPosition, Math.Abs(selectionArea.Start - startPosition));
                startPosition = selectionArea.Start + selectionArea.Length;

                var formatTextBefore = new FormattedText(textBefore, CultureInfo.CurrentCulture, FlowDirection, Typeface, FontSize, Foreground);
                var formatTextSelect = new FormattedText(selection, CultureInfo.CurrentCulture, FlowDirection, Typeface, FontSize, StyleSelection == StyleSelection.Rectangle ? Brushes.White : Foreground);

                chunks.Add(formatTextBefore, false);
                chunks.Add(formatTextSelect, true);
            }
            var textAfter = Text.Substring(startPosition);
            var formatTextAfter = new FormattedText(textAfter, CultureInfo.CurrentCulture, FlowDirection, Typeface, FontSize, Foreground);
            chunks.Add(formatTextAfter, false);

            var startPoint = new Point(Padding.Left, Padding.Top);

            foreach (var chunk in chunks)
            {

                if (!chunk.Value)
                {
                    drawingContext.DrawText(chunk.Key, startPoint);
                }
                else
                {
                    switch (StyleSelection)
                    {
                        case StyleSelection.Rectangle:
                            var visualRect = new DrawingVisual();
                            visualRect.SetValue(UseLayoutRoundingProperty, true);
                            visualRect.SetValue(SnapsToDevicePixelsProperty, true);
                            visualRect.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

                            using (var context = visualRect.RenderOpen())
                            {
                                context.DrawRectangle(Brushes.Black, null,
                                    new Rect(startPoint,
                                        new Size(chunk.Key.WidthIncludingTrailingWhitespace,
                                            chunk.Key.Height)));
                            }

                            drawingContext.DrawRectangle(new VisualBrush(visualRect), null, visualRect.ContentBounds);
                            drawingContext.DrawText(chunk.Key, startPoint);
                            break;
                        case StyleSelection.Underline:
                            var startPointLine = new Point(startPoint.X,
                                textformat.Height - .5);

                            var endPointLine = new Point(
                                startPoint.X +
                                chunk.Key.WidthIncludingTrailingWhitespace, textformat.Height - .5);

                            var visual = new DrawingVisual();
                            visual.SetValue(UseLayoutRoundingProperty, true);
                            visual.SetValue(SnapsToDevicePixelsProperty, true);
                            visual.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Unspecified);
                            using (var context = visual.RenderOpen())
                            {
                                context.DrawLine(new Pen(Brushes.Black, 1), startPointLine, endPointLine);
                            }
                            drawingContext.DrawRectangle(new VisualBrush(visual), null, visual.ContentBounds);
                            drawingContext.DrawText(chunk.Key, startPoint);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                }
                startPoint.X += chunk.Key.WidthIncludingTrailingWhitespace;
            }

            drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(new Point(0, 0), new Size(textformat.WidthIncludingTrailingWhitespace, textformat.Height)));
            base.OnRender(drawingContext);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            return new Size(FinalRectText.Width, FinalRectText.Height);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal class SelectionArea
    {
        public int Start { set; get; }
        public int Length { set; get; }

        public SelectionArea(int start, int length)
        {
            Start = start;
            Length = length;
        }
    }
}
