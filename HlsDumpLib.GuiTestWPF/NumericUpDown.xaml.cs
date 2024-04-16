using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HlsDumpLib.GuiTestWPF
{
    public partial class NumericUpDown : UserControl
    {
        public const int DEFAULT_MAXIMAL_VALUE = 10;
        public const int DEFAULT_MINIMAL_VALUE = 0;
        public const int DEFAULT_VALUE = 5;
        public const int DEFAULT_STEP_SIZE = 1;

        public int MaxValue
        {
            get => (int)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(
            name: "MaxValue",
            propertyType: typeof(int),
            ownerType: typeof(NumericUpDown),
            typeMetadata: new FrameworkPropertyMetadata(
                defaultValue: DEFAULT_MAXIMAL_VALUE,
                flags: FrameworkPropertyMetadataOptions.AffectsMeasure,
                propertyChangedCallback: new PropertyChangedCallback((s, e) => (s as NumericUpDown)?.OnValueChanged()),
                coerceValueCallback: new CoerceValueCallback((d, v) =>
                {
                    int val = (int)v;
                    NumericUpDown upDown = d as NumericUpDown;
                    if (val < upDown.MinValue) { return DEFAULT_MAXIMAL_VALUE; }
                    return val;
                })),
            validateValueCallback: new ValidateValueCallback(obj => true));

        public int MinValue
        {
            get => (int)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register(
            name: "MinValue",
            propertyType: typeof(int),
            ownerType: typeof(NumericUpDown),
            typeMetadata: new FrameworkPropertyMetadata(
                defaultValue: DEFAULT_MINIMAL_VALUE,
                flags: FrameworkPropertyMetadataOptions.AffectsMeasure,
                propertyChangedCallback: new PropertyChangedCallback((s, e) => (s as NumericUpDown)?.OnValueChanged()),
                coerceValueCallback: new CoerceValueCallback((d, v) =>
                {
                    int val = (int)v;
                    NumericUpDown upDown = d as NumericUpDown;
                    if (val >= upDown.MaxValue) { return DEFAULT_MINIMAL_VALUE; }
                    return val;
                })),
            validateValueCallback: new ValidateValueCallback(obj => true));

        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            name: "Value",
            propertyType: typeof(int),
            ownerType: typeof(NumericUpDown),
            typeMetadata: new FrameworkPropertyMetadata(
                defaultValue: DEFAULT_VALUE,
                flags: FrameworkPropertyMetadataOptions.AffectsMeasure,
                propertyChangedCallback: new PropertyChangedCallback((s, e) => (s as NumericUpDown)?.OnValueChanged()),
                coerceValueCallback: new CoerceValueCallback((d, v) =>
                {
                    int val = (int)v;
                    NumericUpDown upDown = d as NumericUpDown;
                    if (val < upDown.MinValue) { return upDown.MinValue; }
                    if (val > upDown.MaxValue) { return upDown.MaxValue; }
                    return val;
                })),
            validateValueCallback: new ValidateValueCallback(obj => true));

        public int StepSize
        {
            get => (int)GetValue(StepSizeProperty);
            set => SetValue(StepSizeProperty, value);
        }

        public static readonly DependencyProperty StepSizeProperty =
            DependencyProperty.Register("StepSize", typeof(int), typeof(NumericUpDown),
                new PropertyMetadata(DEFAULT_STEP_SIZE));

        public NumericUpDown()
        {
            InitializeComponent();
        }

        #region Event handlers
        private void btnIncrease_Click(object sender, RoutedEventArgs e)
        {
            ShiftValue(StepSize);
        }

        private void btnDecrease_Click(object sender, RoutedEventArgs e)
        {
            ShiftValue(-StepSize);
        }

        private void textBoxNumber_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            switch (key)
            {
                case Key.Up:
                    ShiftValue(StepSize);
                    return;

                case Key.Down:
                    ShiftValue(-StepSize);
                    return;
            }

            if ((!IsNumber(key) && key != Key.Subtract && key != Key.OemMinus &&
                key != Key.Back && key != Key.Space &&
                key != Key.Home && key != Key.End && key != Key.Delete &&
                key != Key.Left && key != Key.Right) ||
                ((key == Key.Subtract || key == Key.OemMinus) &&
                (textBlockNumber.CaretIndex != 0 || textBlockNumber.Text.Contains("-"))))
            {
                e.Handled = true;
                return;
            }
        }

        private void textBlockNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(textBlockNumber.Text, out int n))
            {
                Value = ClampValue(n);
            }
            else
            {
                textBlockNumber.Text = Value.ToString();
            }
        }

        private void OnValueChanged()
        {
            textBlockNumber.Text = Value.ToString();
        }
        #endregion

        private void ShiftValue(int shift)
        {
            Value = ClampValue(Value + shift);
        }

        private int ClampValue(int value)
        {
            if (value < MinValue) { return MinValue; }
            if (value > MaxValue) { return MaxValue; }
            return value;
        }

        private bool IsNumber(Key key)
        {
            return (key >= Key.D0 && key <= Key.D9) || (key >= Key.NumPad0 && key <= Key.NumPad9);
        }
    }
}
