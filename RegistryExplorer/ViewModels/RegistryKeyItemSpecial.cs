using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegistryExplorer.ViewModels {
	class RegistryKeyItemSpecial : RegistryKeyItemBase {
		public string Icon { get; set; }

		public IEnumerable<object> Values {
			get { return null; }
		}

		public RegistryKeyItemSpecial(RegistryKeyItemBase parent)
			: base(parent) {
		}
	}
}
