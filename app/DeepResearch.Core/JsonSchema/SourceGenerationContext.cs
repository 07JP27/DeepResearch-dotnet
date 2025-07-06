using System.Text.Json.Serialization;
using DeepResearch.Core.JsonSchema;
namespace DeepResearch.Core;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(GenerateQueryResponse))]
[JsonSerializable(typeof(ReflectionOnSummaryResponse))]
internal partial class SourceGenerationContext : JsonSerializerContext;