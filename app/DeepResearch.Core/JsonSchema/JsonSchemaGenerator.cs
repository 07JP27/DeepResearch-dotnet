using System.ComponentModel;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.Unicode;
using NJsonSchema;

namespace DeepResearch.Core;


/// <summary>
/// JsonSchema を生成する。
/// クラス定義に Description 属性を指定することで JsonSchema にも description を追加する。
/// </summary>
internal static class JsonSchemaGenerator
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
    };

    internal static string GenerateSchema(JsonTypeInfo type)
    {
        var schema = NJsonSchema.JsonSchema.FromType(type.Type);
        return schema.ToJson();
    }
    
    internal static BinaryData GenerateSchemaAsBinaryData(JsonTypeInfo type) =>
        BinaryData.FromString(GenerateSchema(type));
}
