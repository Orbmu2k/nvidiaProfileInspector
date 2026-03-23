using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace nvidiaProfileInspector.UI.Controls
{
    public class ChoiceCard : Button
    {
        static ChoiceCard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ChoiceCard), new FrameworkPropertyMetadata(typeof(ChoiceCard)));
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(ChoiceCard),
            new PropertyMetadata(string.Empty));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            nameof(Description),
            typeof(string),
            typeof(ChoiceCard),
            new PropertyMetadata(string.Empty));

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public static readonly DependencyProperty IconPathProperty = DependencyProperty.Register(
            nameof(IconPath),
            typeof(Geometry),
            typeof(ChoiceCard),
            new PropertyMetadata(null));

        public Geometry IconPath
        {
            get => (Geometry)GetValue(IconPathProperty);
            set => SetValue(IconPathProperty, value);
        }

        public static readonly DependencyProperty IconFillProperty = DependencyProperty.Register(
            nameof(IconFill),
            typeof(Brush),
            typeof(ChoiceCard),
            new PropertyMetadata(null));

        public Brush IconFill
        {
            get => (Brush)GetValue(IconFillProperty);
            set => SetValue(IconFillProperty, value);
        }
    }
}
