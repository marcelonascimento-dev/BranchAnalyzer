namespace BranchAnalyzer;

public partial class Form1
{
    private void ExportGrid(DataGridView dgv, string format)
    {
        if (dgv.Rows.Count == 0)
        {
            MessageBox.Show("Nao ha dados para exportar.", "Atencao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var branchA = txtBranchA.Text;
        var branchB = txtBranchB.Text;
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");

        if (format == "csv")
        {
            using var dlg = new SaveFileDialog
            {
                Filter = "CSV (separado por ;)|*.csv|CSV (separado por ,)|*.csv",
                Title = "Exportar para CSV",
                FileName = $"commits_{branchB.Replace("/", "_")}_{timestamp}.csv"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            var sep = dlg.FilterIndex == 1 ? ";" : ",";
            var sb = new System.Text.StringBuilder();

            // Header
            var headers = new List<string>();
            foreach (DataGridViewColumn col in dgv.Columns)
                if (col.Visible) headers.Add(col.HeaderText);
            sb.AppendLine(string.Join(sep, headers));

            // Rows
            foreach (DataGridViewRow row in dgv.Rows)
            {
                var values = new List<string>();
                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    if (!col.Visible) continue;
                    var val = row.Cells[col.Index].Value?.ToString() ?? "";
                    // Escapar aspas e campos com separador
                    if (val.Contains(sep) || val.Contains('"') || val.Contains('\n'))
                        val = $"\"{val.Replace("\"", "\"\"")}\"";
                    values.Add(val);
                }
                sb.AppendLine(string.Join(sep, values));
            }

            File.WriteAllText(dlg.FileName, sb.ToString(), System.Text.Encoding.UTF8);
            SetStatus($"CSV exportado: {dlg.FileName}");
            MessageBox.Show($"Exportado com sucesso!\n{dlg.FileName}\n\n{dgv.Rows.Count} registro(s)", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else if (format == "json")
        {
            using var dlg = new SaveFileDialog
            {
                Filter = "JSON|*.json",
                Title = "Exportar para JSON",
                FileName = $"commits_{branchB.Replace("/", "_")}_{timestamp}.json"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            var columns = new List<DataGridViewColumn>();
            foreach (DataGridViewColumn col in dgv.Columns)
                if (col.Visible) columns.Add(col);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine($"  \"branchReceptor\": \"{EscapeJson(branchA)}\",");
            sb.AppendLine($"  \"branchFeature\": \"{EscapeJson(branchB)}\",");
            sb.AppendLine($"  \"exportDate\": \"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\",");
            sb.AppendLine($"  \"totalRegistros\": {dgv.Rows.Count},");
            sb.AppendLine("  \"dados\": [");

            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                var row = dgv.Rows[i];
                sb.Append("    {");
                var fields = new List<string>();
                foreach (var col in columns)
                {
                    var val = row.Cells[col.Index].Value?.ToString() ?? "";
                    fields.Add($"\"{col.HeaderText}\": \"{EscapeJson(val)}\"");
                }
                sb.Append(string.Join(", ", fields));
                sb.Append(i < dgv.Rows.Count - 1 ? "}," : "}");
                sb.AppendLine();
            }

            sb.AppendLine("  ]");
            sb.AppendLine("}");

            File.WriteAllText(dlg.FileName, sb.ToString(), System.Text.Encoding.UTF8);
            SetStatus($"JSON exportado: {dlg.FileName}");
            MessageBox.Show($"Exportado com sucesso!\n{dlg.FileName}\n\n{dgv.Rows.Count} registro(s)", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
