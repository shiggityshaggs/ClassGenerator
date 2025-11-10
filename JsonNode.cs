namespace ClassGenerator;

public class JsonNode
{
    public string? Name { get; set; }
    public string? InferredType { get; set; }
    public string? OverrideType { get; set; }
    public bool IsDictionary { get; set; }
    public List<JsonNode> Children { get; set; } = new();

    public string FinalType => OverrideType ?? InferredType ?? "FinalTypeFail";

    public override string ToString() => Name ?? "NoName";
}
