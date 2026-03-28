namespace BranchAnalyzer;

public partial class Form1 : Form
{
    private void SetupMergeTab()
    {
        tabMergeStatus = CreateTab("Status Merge");

        // Painel superior com resumo
        var pnlMergeSummary = new Panel
        {
            Dock = DockStyle.Top,
            Height = 160,
            BackColor = Color.FromArgb(24, 24, 32),
            Padding = new Padding(20, 15, 20, 10)
        };

        pnlMergeIcon = new Panel
        {
            Size = new Size(80, 80),
            Location = new Point(20, 20),
            BackColor = Color.FromArgb(50, 50, 70)
        };
        pnlMergeIcon.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var isMerged = pnlMergeIcon.Tag is true;
            e.Graphics.FillEllipse(
                new SolidBrush(isMerged ? Color.FromArgb(50, 180, 80) : Color.FromArgb(220, 80, 60)),
                5, 5, 70, 70);
            var icon = isMerged ? "OK" : "?";
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString(icon, new Font("Segoe UI", 22, FontStyle.Bold), Brushes.White, new RectangleF(0, 0, 80, 80), sf);
        };
        pnlMergeSummary.Controls.Add(pnlMergeIcon);

        lblMergeResult = CreateInfoLabel("Selecione os branches e clique em Analisar.", 120, 25, 14, true);
        pnlMergeSummary.Controls.Add(lblMergeResult);

        lblMergePending = CreateInfoLabel("", 120, 60);
        pnlMergeSummary.Controls.Add(lblMergePending);

        lblMergeAhead = CreateInfoLabel("", 120, 85);
        pnlMergeSummary.Controls.Add(lblMergeAhead);

        lblMergeBase = CreateInfoLabel("", 120, 110);
        pnlMergeSummary.Controls.Add(lblMergeBase);

        // Painel com label + botoes de export
        var pnlMergeCommitsBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 32,
            BackColor = Color.FromArgb(28, 28, 38)
        };
        var lblMergeCommitsTitle = new Label
        {
            Text = "Commits pendentes de merge:",
            ForeColor = Color.FromArgb(180, 180, 200),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(10, 6)
        };
        pnlMergeCommitsBar.Controls.Add(lblMergeCommitsTitle);

        var btnExportCsv = new Button
        {
            Text = "Exportar CSV",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(40, 90, 60),
            ForeColor = Color.White,
            Size = new Size(100, 24),
            Location = new Point(300, 4),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 8f),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnExportCsv.FlatAppearance.BorderColor = Color.FromArgb(60, 120, 80);
        btnExportCsv.Click += (_, _) => ExportGrid(dgvMergeCommits, "csv");
        pnlMergeCommitsBar.Controls.Add(btnExportCsv);

        var btnExportJson = new Button
        {
            Text = "Exportar JSON",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(40, 70, 120),
            ForeColor = Color.White,
            Size = new Size(105, 24),
            Location = new Point(405, 4),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 8f),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnExportJson.FlatAppearance.BorderColor = Color.FromArgb(60, 90, 150);
        btnExportJson.Click += (_, _) => ExportGrid(dgvMergeCommits, "json");
        pnlMergeCommitsBar.Controls.Add(btnExportJson);

        // -- NEW: Commit Search Bar --
        var pnlCommitSearch = new Panel
        {
            Dock = DockStyle.Top,
            Height = 32,
            BackColor = Color.FromArgb(28, 28, 38),
            Padding = new Padding(8, 3, 8, 3)
        };

        var lblSearchIcon = new Label
        {
            Text = "Buscar:",
            ForeColor = Color.FromArgb(140, 140, 160),
            Font = new Font("Segoe UI", 8.5f),
            AutoSize = true,
            Location = new Point(10, 7)
        };
        pnlCommitSearch.Controls.Add(lblSearchIcon);

        txtCommitSearch = new TextBox
        {
            Location = new Point(60, 4),
            Size = new Size(350, 24),
            BackColor = Color.FromArgb(40, 40, 55),
            ForeColor = Color.White,
            Font = new Font("Consolas", 9.5f),
            PlaceholderText = "Filtrar por mensagem, autor ou hash..."
        };
        txtCommitSearch.TextChanged += (_, _) => FilterMergeCommits();
        pnlCommitSearch.Controls.Add(txtCommitSearch);

        lblCommitSearchCount = new Label
        {
            Text = "",
            ForeColor = Color.FromArgb(120, 180, 255),
            Font = new Font("Segoe UI", 8.5f),
            AutoSize = true,
            Location = new Point(420, 7)
        };
        pnlCommitSearch.Controls.Add(lblCommitSearchCount);

        // Grid com commits pendentes
        dgvMergeCommits = CreateDataGrid();
        dgvMergeCommits.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Hash", HeaderText = "Hash", Width = 100, DataPropertyName = "Hash" },
            new DataGridViewTextBoxColumn { Name = "Author", HeaderText = "Autor", Width = 200, DataPropertyName = "Author" },
            new DataGridViewTextBoxColumn { Name = "RelativeDate", HeaderText = "Quando", Width = 140, DataPropertyName = "RelativeDate" },
            new DataGridViewTextBoxColumn { Name = "Message", HeaderText = "Mensagem", Width = 600, DataPropertyName = "Message" }
        );

        // Ordem: Fill primeiro, depois Top
        tabMergeStatus.Controls.Add(dgvMergeCommits);       // Fill
        tabMergeStatus.Controls.Add(pnlCommitSearch);        // Top (search bar)
        tabMergeStatus.Controls.Add(pnlMergeCommitsBar);     // Top (barra com export)
        tabMergeStatus.Controls.Add(pnlMergeSummary);         // Top (topo)
    }

    private void FilterMergeCommits()
    {
        var filter = txtCommitSearch.Text.Trim();
        if (string.IsNullOrEmpty(filter))
        {
            dgvMergeCommits.DataSource = null;
            dgvMergeCommits.DataSource = _allMergeCommits;
            lblCommitSearchCount.Text = _allMergeCommits.Count > 0
                ? $"{_allMergeCommits.Count} commit(s)"
                : "";
            return;
        }

        var filtered = _allMergeCommits.Where(c =>
            c.Message.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
            c.Author.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
            c.Hash.Contains(filter, StringComparison.OrdinalIgnoreCase)
        ).ToList();

        dgvMergeCommits.DataSource = null;
        dgvMergeCommits.DataSource = filtered;
        lblCommitSearchCount.Text = $"{filtered.Count} de {_allMergeCommits.Count} commit(s)";
    }

    private void BtnAnalyze_Click(object? sender, EventArgs e)
    {
        var (a, b) = ResolveBranches();
        if (a == null || b == null) return;

        SetStatus("Analisando branches...");
        SetBusy(true);

        Task.Run(() =>
        {
            try
            {
                var status = _git.CheckMergeStatus(a, b);
                var commits = _git.GetPendingCommits(a, b);
                Invoke(() => UpdateMergeStatus(status, a, b, commits));

                Invoke(() => { SetStatus($"Analise concluida. Branch A: {a} | Branch B: {b}"); RestoreDefaultCursor(); });
            }
            catch (Exception ex)
            {
                Invoke(() => { SetStatus($"Erro: {ex.Message}"); RestoreDefaultCursor(); });
            }
        });
    }

    private void UpdateMergeStatus(MergeStatus status, string branchA, string branchB, List<CommitInfo>? commits = null)
    {
        pnlMergeIcon.Tag = status.IsMerged;
        pnlMergeIcon.Invalidate();

        lblMergeResult.Text = status.IsMerged
            ? $"MERGE JA REALIZADO - Todos os commits de [{branchB}] estao em [{branchA}]"
            : $"MERGE PENDENTE - Existem commits em [{branchB}] que nao estao em [{branchA}]";
        lblMergeResult.ForeColor = status.IsMerged ? Color.FromArgb(80, 220, 80) : Color.FromArgb(255, 180, 60);

        lblMergePending.Text = $"Commits pendentes: {status.PendingCommits}";
        lblMergePending.ForeColor = status.PendingCommits > 0 ? Color.FromArgb(255, 180, 60) : Color.FromArgb(80, 220, 80);

        lblMergeAhead.Text = $"{branchA} esta a frente de {branchB} em: {status.AheadCommits} commit(s)";

        lblMergeBase.Text = $"Merge base: {(status.MergeBase.Length > 12 ? status.MergeBase[..12] : status.MergeBase)}";
        lblMergeBase.ForeColor = Color.FromArgb(140, 140, 160);

        if (commits != null)
        {
            _allMergeCommits = commits;
            txtCommitSearch.Text = "";
            dgvMergeCommits.DataSource = null;
            dgvMergeCommits.DataSource = commits;
            lblCommitSearchCount.Text = commits.Count > 0 ? $"{commits.Count} commit(s)" : "";
        }
    }

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

            var headers = new List<string>();
            foreach (DataGridViewColumn col in dgv.Columns)
                if (col.Visible) headers.Add(col.HeaderText);
            sb.AppendLine(string.Join(sep, headers));

            foreach (DataGridViewRow row in dgv.Rows)
            {
                var values = new List<string>();
                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    if (!col.Visible) continue;
                    var val = row.Cells[col.Index].Value?.ToString() ?? "";
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
