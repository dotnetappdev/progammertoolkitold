
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;



namespace ProgrammersToolKit.Views
{
    public partial class JsonQueryView : UserControl
    {
        private TextBox? _jsonInputBox;
        private TextBox? _sqlQueryBox;
        private TextBox? _queryResultBox;
        private Button? _runQueryBtn;
        private TreeView? _jsonTreeView;

        public JsonQueryView()
        {
            AvaloniaXamlLoader.Load(this);
            _jsonInputBox = this.FindControl<TextBox>("JsonInputBox");
            _sqlQueryBox = this.FindControl<TextBox>("SqlQueryBox");
            _queryResultBox = this.FindControl<TextBox>("QueryResultBox");
            _runQueryBtn = this.FindControl<Button>("RunQueryBtn");
            _jsonTreeView = this.FindControl<TreeView>("JsonTreeView");
            if (_runQueryBtn != null)
                _runQueryBtn.Click += (s, e) => RunQuery();
        }

        private void RunQuery()
        {
            if (_jsonInputBox == null || _sqlQueryBox == null || _queryResultBox == null) return;
            try
            {
                var json = _jsonInputBox.Text ?? "{}";
                var sql = _sqlQueryBox.Text ?? string.Empty;
                // Use JsonDocument to parse and flatten JSON to a DataTable
                var dt = JsonToDataTable(json);
                // Use DataTable.Compute for simple SELECTs (not full SQL, but demo)
                if (!string.IsNullOrWhiteSpace(sql) && dt != null)
                {
                    // Only support SELECT col FROM $ WHERE ...
                    var result = SimpleSqlSelect(dt, sql);
                    _queryResultBox.Text = result;
                }
                else
                {
                    _queryResultBox.Text = "No query or invalid JSON.";
                }
                // Visualize JSON as tree
                if (_jsonTreeView != null)
                {
                    var rootNode = BuildJsonTree(JsonDocument.Parse(json).RootElement, null);
                    _jsonTreeView.ItemsSource = new List<JsonTreeNode> { rootNode };
                }
            }
            catch (Exception ex)
            {
                _queryResultBox.Text = $"Error: {ex.Message}";
            }
        }

        private DataTable? JsonToDataTable(string json)
        {
            try
            {
                var doc = JsonDocument.Parse(json);
                var dt = new DataTable();
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in doc.RootElement.EnumerateObject())
                        dt.Columns.Add(prop.Name);
                    var row = dt.NewRow();
                    foreach (var prop in doc.RootElement.EnumerateObject())
                        row[prop.Name] = prop.Value.ToString();
                    dt.Rows.Add(row);
                }
                else if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    var arr = doc.RootElement.EnumerateArray().ToList();
                    if (arr.Count > 0)
                    {
                        foreach (var prop in arr[0].EnumerateObject())
                            dt.Columns.Add(prop.Name);
                        foreach (var item in arr)
                        {
                            var row = dt.NewRow();
                            foreach (var prop in item.EnumerateObject())
                                row[prop.Name] = prop.Value.ToString();
                            dt.Rows.Add(row);
                        }
                    }
                }
                return dt;
            }
            catch { return null; }
        }

        private string SimpleSqlSelect(DataTable dt, string sql)
        {
            // Only support: SELECT col1,col2 FROM $
            try
            {
                var lower = sql.ToLower();
                if (!lower.StartsWith("select") || !lower.Contains("from"))
                    return "Only simple SELECT ... FROM $ supported.";
                var selectPart = sql.Substring(6, lower.IndexOf("from") - 6).Trim();
                var cols = selectPart.Split(',').Select(c => c.Trim()).ToList();
                var sb = new StringBuilder();
                foreach (DataRow row in dt.Rows)
                {
                    foreach (var col in cols)
                    {
                        if (dt.Columns.Contains(col))
                            sb.Append(row[col] + "\t");
                    }
                    sb.AppendLine();
                }
                return sb.ToString();
            }
            catch (Exception ex) { return $"Query error: {ex.Message}"; }
        }

        private JsonTreeNode BuildJsonTree(JsonElement elem, string? name)
        {
            var node = new JsonTreeNode { Name = name };
            switch (elem.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var prop in elem.EnumerateObject())
                        node.Children.Add(BuildJsonTree(prop.Value, prop.Name));
                    break;
                case JsonValueKind.Array:
                    int idx = 0;
                    foreach (var item in elem.EnumerateArray())
                        node.Children.Add(BuildJsonTree(item, $"[{idx++}]"));
                    break;
                default:
                    node.Value = elem.ToString();
                    break;
            }
            return node;
        }

        public class JsonTreeNode
        {
            public string? Name { get; set; }
            public string? Value { get; set; }
            public List<JsonTreeNode> Children { get; set; } = new();
            public string Display => Name != null ? (Value == null ? Name : $"{Name}: {Value}") : (Value ?? "");
        }
    }
}
