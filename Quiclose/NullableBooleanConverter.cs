using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Data;

namespace Quiclose {

    /// <summary>
    /// Used for converting between booleans and misc objects in XAML.
    /// </summary>
    [ValueConversion(typeof(bool?), typeof(object))]
    internal class NullableBooleanConverter : IValueConverter {

        // Todo? These should be dependency props?
        public object? TrueValue { get; set; } = true;
        public object? FalseValue { get; set; } = false;
        public object? NullValue { get; set; } = null;

        public object? Convert (object? value, Type targetType, object? parameter, CultureInfo culture) => (value as bool?) switch {
            true => TrueValue,
            false => FalseValue,
            null => NullValue
        };

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
