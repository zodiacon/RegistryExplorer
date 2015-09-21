using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace RegistryExplorer.Extensions {
	[MarkupExtensionReturnType(typeof(Image))]
	class ImageExtension : MarkupExtension {
		public string Uri { get; set; }

		public double Width { get; set; }
		public double Height { get; set; }

		public ImageExtension(string uri = null) {
			Uri = uri;
			Width = Height = double.NaN;
		}

		public override object ProvideValue(IServiceProvider serviceProvider) {
			return new Image { 
				Source = new BitmapImage(new Uri(Uri, UriKind.RelativeOrAbsolute)),
				Width = Width,
				Height = Height
			};
		}
	}
}
