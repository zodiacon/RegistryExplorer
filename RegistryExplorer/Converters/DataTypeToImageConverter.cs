using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Microsoft.Win32;

namespace RegistryExplorer.Converters {
	class DataTypeToImageConverter : IValueConverter {
		Dictionary<RegistryValueKind, string> _images = new Dictionary<RegistryValueKind, string> {
			{ RegistryValueKind.Binary, "/images/DataTypes/reg_binary.png" },
			{ RegistryValueKind.String, "/images/DataTypes/reg_string.png" },
			{ RegistryValueKind.MultiString, "/images/DataTypes/reg_string.png" },
			{ RegistryValueKind.DWord, "/images/DataTypes/reg_binary.png" },
			{ RegistryValueKind.QWord, "/images/DataTypes/reg_binary.png" },
			{ RegistryValueKind.ExpandString, "/images/DataTypes/reg_string.png" },
			{ RegistryValueKind.None, "/images/DataTypes/empty.png" }
		};

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			var type = (RegistryValueKind)value;
			string image;
			if(_images.TryGetValue(type, out image))
				return image;
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
