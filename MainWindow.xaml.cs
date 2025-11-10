using Newtonsoft.Json.Linq;
using System.IO;
using System.Windows;

namespace ClassGenerator
{
    public partial class MainWindow : Window
    {
        const string fullName = @"test.json";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadJson_Click(object sender, RoutedEventArgs e)
        {
            LoadJson(fullName);
            return;
        }

        private JsonNode? selectedNode;
        private void JsonTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            selectedNode = e.NewValue as JsonNode;
            if (selectedNode == null) return;

            NodePath.Text = selectedNode.Name;
            InferredTypeBox.Text = selectedNode.InferredType;
            OverrideTypeBox.Text = selectedNode.OverrideType ?? "";
            IsDictionaryBox.IsChecked = selectedNode.IsDictionary;

            ClassPreviewBox.Text = TypeGenerator.GenerateClasses(selectedNode);
        }

        private void ApplyOverride_Click(object sender, RoutedEventArgs e)
        {
            if (selectedNode == null) return;

            selectedNode.OverrideType = string.IsNullOrWhiteSpace(OverrideTypeBox.Text) ? null : OverrideTypeBox.Text;
            selectedNode.IsDictionary = IsDictionaryBox.IsChecked == true;

            ClassPreviewBox.Text = TypeGenerator.GenerateClasses(selectedNode);
        }

        private void LoadJson(string path)
        {
            string json = File.ReadAllText(path);
            var token = JToken.Parse(json);
            var rootNode = BuildTree("Root", token);
            JsonTree.ItemsSource = new List<JsonNode> { rootNode };
        }

        private JsonNode BuildTree(string name, JToken token)
        {
            var node = new JsonNode { Name = name };

            if (token.Type == JTokenType.Object)
            {
                var obj = (JObject)token;
                var children = obj.Properties().ToList();

                bool isDictionary = children.All(p => p.Value.Type == JTokenType.Object);
                if (isDictionary || node.IsDictionary)
                {
                    var allProps = new HashSet<string>();
                    foreach (var child in children)
                    {
                        var childObj = (JObject)child.Value;
                        foreach (var prop in childObj.Properties())
                        {
                            allProps.Add(prop.Name);
                        }
                    }

                    node.IsDictionary = true;
                    node.InferredType = $"Dictionary<string, {name}Item>";
                    node.Children = allProps.Select(propName => new JsonNode
                    {
                        Name = propName,
                        InferredType = "string"
                    }).ToList();

                    return node;
                }

                node.InferredType = name;
                node.Children = children.Select(p => BuildTree(p.Name, p.Value)).ToList();
            }
            else if (token.Type == JTokenType.Array)
            {
                var firstItem = token.First;
                var child = BuildTree(name + "Item", firstItem);
                node.Children.Add(child);
                node.InferredType = $"List<{child.InferredType}>";
            }
            else
            {
                node.InferredType = InferSimpleType(token);
            }

            return node;
        }

        private static string InferSimpleType(JToken token)
        {
            return token.Type switch
            {
                JTokenType.Integer => "int",
                JTokenType.Float => "double",
                JTokenType.Boolean => "bool",
                JTokenType.String => "string",
                _ => "string"
            };
        }
    }
}