using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NESSharp.Core;

public static class Json {
	private static readonly JsonSerializerOptions _options = new JsonSerializerOptions(){PropertyNamingPolicy = JsonNamingPolicy.CamelCase};

	static Json() {
		_options.Converters.Add(new U8Converter());
	}
	public static T? LoadString<T>(string json) => JsonSerializer.Deserialize<T>(json, _options);
	public static bool TryLoadFile<T>(string path, out T? configObj) where T : class {
		configObj = default;

		if (!File.Exists(path)) return false;

		//try {
			var contents = File.ReadAllText(path);
			configObj = JsonSerializer.Deserialize<T>(contents, _options);
			return true;
		//} catch (Exception e) {
		//	Console.WriteLine(e.StackTrace);
		//}

		//return false;
	}
}

public class U8Converter : JsonConverter<U8> {
	public override U8 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.GetInt32();
		public override void Write(Utf8JsonWriter writer, U8 val, JsonSerializerOptions options) => writer.WriteNumberValue(val);
	}

public static class Config {
	public static bool TryLoad<T>(string path, out T? configObj) where T : class => Json.TryLoadFile($"./config/{path}", out configObj);
}
