using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NESSharp.Core {
	public static class Config {
		public static T Load<T>(string json) => JsonConvert.DeserializeObject<T>(json, ConfigConverter.Settings);
		public static bool TryLoad<T>(string path, out T configObj) {
			path = "./config/" + path;
			configObj = default;
			if (!File.Exists(path)) return false;
			configObj = Load<T>(File.ReadAllText(path));
			return true;
		}
	}
	public static class ConfigConverter {
		public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings {
			MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
			Converters = {
				new U8Converter()
			},
		};
	}
	public class U8Converter : JsonConverter {
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			var token = JToken.ReadFrom(reader);
			long target = token.Value<long>();

			return (U8)Convert.ToInt32(target);
		}
		public override bool CanConvert(Type objectType) => typeof(U8) == objectType;
		public override bool CanWrite => false;
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
	}
}
