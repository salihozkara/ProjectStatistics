using Newtonsoft.Json;
namespace Shared;

public static class ObjectExtensions
{
    public static void ToJsonFile(this object obj,string? path = null,[System.Runtime.CompilerServices.CallerArgumentExpression("obj")] string callerExp = "")
    {
        path ??= Path.Combine("./Jsons", callerExp + ".json");

        path = Path.GetFullPath(path);
        
        if(!Directory.Exists(Path.GetDirectoryName(path)))
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);


        File.WriteAllText(path, JsonConvert.SerializeObject(obj, new JsonSerializerSettings { 
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore, 
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize ,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            MaxDepth = 100,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            DateParseHandling = DateParseHandling.DateTimeOffset,
            FloatFormatHandling = FloatFormatHandling.String,
            FloatParseHandling = FloatParseHandling.Double,
            StringEscapeHandling = StringEscapeHandling.Default,
            Culture = System.Globalization.CultureInfo.InvariantCulture,
            CheckAdditionalContent = true,
            Converters = new JsonConverter[] { new Newtonsoft.Json.Converters.StringEnumConverter()}
        }));
    }
}