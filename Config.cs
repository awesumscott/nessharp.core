//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NESSharp.Core {
	public static class Json {
		//public string Path {get; private set;}
		private static readonly JsonSerializerOptions _options = new JsonSerializerOptions(){PropertyNamingPolicy = JsonNamingPolicy.CamelCase};

		static Json() {
			_options.Converters.Add(new U8Converter());
		}
		public static T LoadString<T>(string json) => JsonSerializer.Deserialize<T>(json, _options);
		public static bool TryLoadFile<T>(string path, out T? configObj) where T : class {
			configObj = default;

			if (!File.Exists(path)) return false;

			//try {
				var contents = File.ReadAllText(path);
				configObj = JsonSerializer.Deserialize<T>(contents, _options); // JsonConvert.DeserializeObject<T>(contents, ConfigConverter.Settings);
				return true;
			//} catch (Exception e) {
			//	Console.WriteLine(e.StackTrace);
			//}

			return false;
		}
	}
	public class U8Converter : JsonConverter<U8> {
		public override U8 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			(U8)reader.GetInt32();

        public override void Write(Utf8JsonWriter writer, U8 val, JsonSerializerOptions options) =>
			writer.WriteNumberValue(val);
    }




	public static class Config {
		public static bool TryLoad<T>(string path, out T? configObj) where T : class {
			return Json.TryLoadFile($"./config/{path}", out configObj);
		}
	}
	//public static class Config {
	//	public static T Load<T>(string json) => JsonConvert.DeserializeObject<T>(json, ConfigConverter.Settings);
	//	public static bool TryLoad<T>(string path, out T? configObj) where T : class {
	//		path = "./config/" + path;
	//		configObj = default;
	//		if (!File.Exists(path)) return false;
	//		configObj = Load<T>(File.ReadAllText(path));
	//		return true;
	//	}
	//}
	//public static class ConfigConverter {
	//	public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings {
	//		MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
	//		Converters = {
	//			new U8Converter()
	//		},
	//	};
	//}
	//public class U8Converter : JsonConverter {
	//	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
	//		var token = JToken.ReadFrom(reader);
	//		long target = token.Value<long>();

	//		return (U8)Convert.ToInt32(target);
	//	}
	//	public override bool CanConvert(Type objectType) => typeof(U8) == objectType;
	//	public override bool CanWrite => false;
	//	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
	//}
}
