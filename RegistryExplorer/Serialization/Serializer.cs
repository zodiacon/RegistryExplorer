using RegistryExplorer.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegistryExplorer.Serialization {
	static class Serializer {
		public static void SaveKeys(Stream stm, ICollection<RegistryKeyItemBase> keys) {
			var writer = new BinaryWriter(stm);
			writer.Write(keys.Count);
			foreach(RegistryKeyItem key in keys) {
				writer.Write(key.Root.Name);
				writer.Write(key.Path);
			}
		}

		public static ICollection<RegistryKeyItemBase> LoadKeys(Stream stm) {
			var reader = new BinaryReader(stm);
			int count = reader.ReadInt32();
			var keys = new List<RegistryKeyItemBase>(count);

			for(int i = 0; i < count; i++) {
				var key = new RegistryKeyItem(reader.ReadString(), reader.ReadString());
				keys.Add(key);
			}
			return keys;
		}
	}
}
