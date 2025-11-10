using System.Text;

namespace ClassGenerator
{
    public static class TypeGenerator
    {
        public static string GenerateClasses(JsonNode root)
        {
            var sb = new StringBuilder();
            GenerateClass(root, sb, new HashSet<string>());
            return sb.ToString();
        }

        private static void GenerateClass(JsonNode node, StringBuilder sb, HashSet<string> emitted)
        {
            if (node.Children.Count == 0) return;

            string className = node.IsDictionary ? node.Name + "Item" : node.Name;
            if (!emitted.Add(className)) return;

            sb.AppendLine($"public class {className}");
            sb.AppendLine("{");

            foreach (var child in node.Children)
            {
                string type = child.FinalType;
                string name = child.Name;
                sb.AppendLine($"    [JsonProperty(\"{name}\")]");
                sb.AppendLine($"    public {type}? {name} {{ get; set; }}");
            }

            sb.AppendLine("}\n");

            foreach (var child in node.Children)
            {
                GenerateClass(child, sb, emitted);
            }
        }
    }
}
