using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Quiclose {
    internal class ChainConverter : List<IValueConverter>, IValueConverter {

        public object Convert (object value, Type targetType, object parameter, CultureInfo culture) =>
            this.Aggregate(value, (current, converter) => converter.Convert(value, targetType, parameter, culture));

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
