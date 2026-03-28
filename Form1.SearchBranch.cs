namespace BranchAnalyzer;

public partial class Form1
{
    private void SetupSearchBranchTab()
    {
        tabSearchBranch = CreateTab("Pesquisar Branch");

        // ── Top bar (tudo junto: titulo, busca, status) ──────
        var pnlSearchTop = new Panel
        {
            Dock = DockStyle.Top,
            Height = 110,
            BackColor = Color.FromArgb(30, 30, 42),
            Padding = new Padding(8)
        };

        var lblSearchTitle = new Label
        {
            Text = "PESQUISAR BRANCH POR OS / COMMIT",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(120, 180, 255),
            AutoSize = true,
            Location = new Point(10, 6)
        };
        pnlSearchTop.Controls.Add(lblSearchTitle);

        var lblSearchHint = new Label
        {
            Text = "Digite o numero da OS, descricao ou qualquer termo presente na mensagem do commit:",
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = Color.FromArgb(140, 140, 160),
            AutoSize = true,
            Location = new Point(10, 28)
        };
        pnlSearchTop.Controls.Add(lblSearchHint);

        txtSearchTerm = new TextBox
        {
            Location = new Point(10, 50),
            Size = new Size(500, 28),
            BackColor = Color.FromArgb(40, 40, 55),
            ForeColor = Color.White,
            Font = new Font("Consolas", 10f),
            BorderStyle = BorderStyle.FixedSingle
        };
        txtSearchTerm.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ExecuteBranchSearch();
            }
        };
        pnlSearchTop.Controls.Add(txtSearchTerm);

        btnSearch = new Button
        {
            Text = "PESQUISAR",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(40, 100, 200),
            ForeColor = Color.White,
            Size = new Size(120, 28),
            Location = new Point(520, 50),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };
        btnSearch.FlatAppearance.BorderColor = Color.FromArgb(60, 120, 220);
        btnSearch.Click += (_, _) => ExecuteBranchSearch();
        pnlSearchTop.Controls.Add(btnSearch);

        lblSearchCount = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(160, 160, 180),
            AutoSize = true,
            Location = new Point(650, 54)
        };
        pnlSearchTop.Controls.Add(lblSearchCount);

        lblSearchStatus = new Label
        {
            Text = "Digite um termo e clique em Pesquisar.",
            ForeColor = Color.FromArgb(160, 160, 180),
            Font = new Font("Segoe UI", 8.5f),
            AutoSize = true,
            Location = new Point(10, 84)
        };
        pnlSearchTop.Controls.Add(lblSearchStatus);

        // ── Grid de resultados ───────────────────────────────
        dgvSearchResults = CreateDataGrid();
        dgvSearchResults.AutoGenerateColumns = false;
        dgvSearchResults.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Branch", HeaderText = "Branch", Width = 300 },
            new DataGridViewTextBoxColumn { Name = "Hash", HeaderText = "Hash", Width = 80 },
            new DataGridViewTextBoxColumn { Name = "Autor", HeaderText = "Autor", Width = 200 },
            new DataGridViewTextBoxColumn { Name = "Data", HeaderText = "Data", Width = 100 },
            new DataGridViewTextBoxColumn { Name = "Mensagem", HeaderText = "Mensagem Commit", Width = 450 }
        );

        // Double-click para copiar o nome do branch
        dgvSearchResults.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex < 0) return;
            var branchName = dgvSearchResults.Rows[e.RowIndex].Cells["Branch"].Value?.ToString();
            if (!string.IsNullOrEmpty(branchName))
            {
                Clipboard.SetText(branchName);
                lblSearchStatus.Text = $"Branch \"{branchName}\" copiado para a area de transferencia!";
                lblSearchStatus.ForeColor = Color.FromArgb(80, 220, 120);
            }
        };

        // Montar a tab: grid fill + top bar acima
        tabSearchBranch.Controls.Add(dgvSearchResults);
        tabSearchBranch.Controls.Add(pnlSearchTop);
    }

    private async void ExecuteBranchSearch()
    {
        var term = txtSearchTerm.Text.Trim();
        if (string.IsNullOrEmpty(term))
        {
            lblSearchStatus.Text = "Digite um termo para pesquisar.";
            lblSearchStatus.ForeColor = Color.FromArgb(255, 180, 80);
            return;
        }

        if (string.IsNullOrEmpty(_git.RepoPath))
        {
            lblSearchStatus.Text = "Nenhum repositorio selecionado.";
            lblSearchStatus.ForeColor = Color.FromArgb(255, 80, 80);
            return;
        }

        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var ct = _searchCts.Token;

        btnSearch.Enabled = false;
        btnSearch.Text = "Pesquisando...";
        lblSearchStatus.Text = $"Pesquisando \"{term}\" em todos os branches...";
        lblSearchStatus.ForeColor = Color.FromArgb(255, 200, 80);
        lblSearchCount.Text = "";
        dgvSearchResults.Rows.Clear();

        try
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var results = await _git.SearchBranchByCommitMessageAsync(term, ct);
            sw.Stop();

            if (ct.IsCancellationRequested) return;

            dgvSearchResults.Rows.Clear();

            if (results.Count == 0)
            {
                lblSearchStatus.Text = $"Nenhum resultado encontrado para \"{term}\".";
                lblSearchStatus.ForeColor = Color.FromArgb(255, 180, 80);
                lblSearchCount.Text = "";
                return;
            }

            foreach (var r in results)
            {
                var rowIdx = dgvSearchResults.Rows.Add(r.Branch, r.Hash, r.Author, r.Date, r.Message);

                if (r.Branch == "(não encontrado)" || r.Branch == "develop" || r.Branch == "master" || r.Branch == "main")
                    dgvSearchResults.Rows[rowIdx].DefaultCellStyle.ForeColor = Color.FromArgb(140, 140, 160);
                else
                    dgvSearchResults.Rows[rowIdx].Cells["Branch"].Style.ForeColor = Color.FromArgb(80, 220, 120);
            }

            lblSearchStatus.Text = $"Pesquisa concluida em {sw.ElapsedMilliseconds}ms. Duplo-clique para copiar o nome do branch.";
            lblSearchStatus.ForeColor = Color.FromArgb(160, 220, 255);
            lblSearchCount.Text = $"{results.Count} resultado(s)";
            lblSearchCount.ForeColor = Color.FromArgb(80, 220, 120);
        }
        catch (OperationCanceledException)
        {
            lblSearchStatus.Text = "Pesquisa cancelada.";
            lblSearchStatus.ForeColor = Color.FromArgb(140, 140, 160);
        }
        catch (Exception ex)
        {
            Logger.Error($"SearchBranch error: {ex.Message}");
            lblSearchStatus.Text = $"Erro: {ex.Message}";
            lblSearchStatus.ForeColor = Color.FromArgb(255, 80, 80);
        }
        finally
        {
            btnSearch.Enabled = true;
            btnSearch.Text = "PESQUISAR";
        }
    }
}
