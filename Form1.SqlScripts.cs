using System.Text;

namespace BranchAnalyzer;

public partial class Form1 : Form
{
    // Tab: SQL Scripts
    private TabPage tabSqlScripts = null!;
    private ComboBox cmbSqlBaseBranch = null!;
    private DataGridView dgvSqlResults = null!;
    private Button btnSqlAnalyze = null!;
    private Button btnSqlGenerate = null!;
    private Button btnSqlExportCsv = null!;
    private Button btnSqlCancel = null!;
    private ProgressBar pgSql = null!;
    private Label lblSqlEta = null!;
    private Label lblSqlSummary = null!;
    private Panel pnlSqlDashboard = null!;
    private CancellationTokenSource? _sqlCts;
    private List<BranchSqlResult> _sqlResults = new();

    private void SetupSqlScriptsTab()
    {
        tabSqlScripts = CreateTab("SQL Scripts");

        // -- Painel superior: Base branch + Analisar + Gerar Script + Cancel --
        var pnlSqlTop = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = Color.FromArgb(30, 30, 42), Padding = new Padding(8) };

        // Linha 1: Info
        var lblSqlInfo = new Label
        {
            Text = "Analisa branches ativos nos ultimos 120 dias e identifica scripts .sql adicionados/modificados comparando com a branch base (Develop).",
            ForeColor = Color.FromArgb(160, 160, 180),
            Font = new Font("Segoe UI", 8.5f),
            Location = new Point(8, 4),
            AutoSize = false,
            Size = new Size(900, 18)
        };
        pnlSqlTop.Controls.Add(lblSqlInfo);

        // Linha 2: Branch base + botoes
        var lblBaseBranch = new Label
        {
            Text = "Branch Base:",
            ForeColor = Color.FromArgb(100, 220, 100),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Location = new Point(8, 28),
            AutoSize = true
        };
        pnlSqlTop.Controls.Add(lblBaseBranch);

        cmbSqlBaseBranch = CreateBranchComboBox(new Point(110, 26));
        cmbSqlBaseBranch.Width = 250;
        cmbSqlBaseBranch.Text = "Develop";
        SetupBranchAutocomplete(cmbSqlBaseBranch);
        pnlSqlTop.Controls.Add(cmbSqlBaseBranch);

        btnSqlAnalyze = new Button
        {
            Text = "ANALISAR SQL",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(40, 100, 200),
            ForeColor = Color.White,
            Size = new Size(140, 28),
            Location = new Point(370, 26),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };
        btnSqlAnalyze.FlatAppearance.BorderColor = Color.FromArgb(60, 120, 220);
        btnSqlAnalyze.Click += BtnSqlAnalyze_Click;
        pnlSqlTop.Controls.Add(btnSqlAnalyze);

        btnSqlGenerate = new Button
        {
            Text = "GERAR SCRIPT COMPLETO",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(40, 130, 80),
            ForeColor = Color.White,
            Size = new Size(200, 28),
            Location = new Point(520, 26),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Enabled = false
        };
        btnSqlGenerate.FlatAppearance.BorderColor = Color.FromArgb(60, 160, 100);
        btnSqlGenerate.Click += BtnSqlGenerate_Click;
        pnlSqlTop.Controls.Add(btnSqlGenerate);

        btnSqlExportCsv = new Button
        {
            Text = "CSV",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(40, 90, 60),
            ForeColor = Color.White,
            Size = new Size(55, 28),
            Location = new Point(730, 26),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 8.5f)
        };
        btnSqlExportCsv.FlatAppearance.BorderColor = Color.FromArgb(60, 120, 80);
        btnSqlExportCsv.Click += (_, _) => ExportGrid(dgvSqlResults, "csv");
        pnlSqlTop.Controls.Add(btnSqlExportCsv);

        btnSqlCancel = new Button
        {
            Text = "CANCELAR",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(180, 50, 50),
            ForeColor = Color.White,
            Size = new Size(100, 28),
            Location = new Point(795, 26),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Visible = false
        };
        btnSqlCancel.FlatAppearance.BorderColor = Color.FromArgb(220, 60, 60);
        btnSqlCancel.Click += (_, _) =>
        {
            _sqlCts?.Cancel();
            btnSqlCancel.Enabled = false;
            btnSqlCancel.Text = "Cancelando...";
        };
        pnlSqlTop.Controls.Add(btnSqlCancel);

        pgSql = new ProgressBar
        {
            Location = new Point(8, 62),
            Size = new Size(600, 18),
            Visible = false,
            Style = ProgressBarStyle.Continuous
        };
        pnlSqlTop.Controls.Add(pgSql);

        lblSqlEta = new Label
        {
            Text = "",
            ForeColor = Color.FromArgb(180, 180, 200),
            Font = new Font("Segoe UI", 8f),
            Location = new Point(620, 62),
            AutoSize = true,
            Visible = false
        };
        pnlSqlTop.Controls.Add(lblSqlEta);

        tabSqlScripts.Controls.Add(pnlSqlTop);
        pnlSqlTop.BringToFront();

        // -- Dashboard Cards --
        pnlSqlDashboard = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = Color.FromArgb(28, 28, 38),
            Padding = new Padding(8, 6, 8, 6),
            Visible = false
        };
        tabSqlScripts.Controls.Add(pnlSqlDashboard);

        // -- Grid de resultados --
        dgvSqlResults = CreateDataGrid();
        dgvSqlResults.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Branch", HeaderText = "Branch", Width = 250, DataPropertyName = "Branch" },
            new DataGridViewTextBoxColumn { Name = "Autor", HeaderText = "Autor", Width = 160, DataPropertyName = "Autor" },
            new DataGridViewTextBoxColumn { Name = "UltimaData", HeaderText = "Ultima Data", Width = 100, DataPropertyName = "UltimaData" },
            new DataGridViewTextBoxColumn { Name = "ScriptsAdicionados", HeaderText = "Adicionados", Width = 100, DataPropertyName = "ScriptsAdicionados" },
            new DataGridViewTextBoxColumn { Name = "ScriptsModificados", HeaderText = "Modificados", Width = 100, DataPropertyName = "ScriptsModificados" },
            new DataGridViewTextBoxColumn { Name = "TotalScripts", HeaderText = "Total Scripts", Width = 100, DataPropertyName = "TotalScripts" },
            new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 130, DataPropertyName = "Status" }
        );
        dgvSqlResults.CellDoubleClick += DgvSqlResults_CellDoubleClick;

        // -- Summary label --
        lblSqlSummary = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 28,
            BackColor = Color.FromArgb(30, 30, 42),
            ForeColor = Color.FromArgb(160, 160, 180),
            Font = new Font("Segoe UI", 9f),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(8, 0, 0, 0),
            Text = "Execute a analise para verificar scripts SQL nos branches ativos."
        };

        tabSqlScripts.Controls.Add(dgvSqlResults);
        tabSqlScripts.Controls.Add(lblSqlSummary);
        dgvSqlResults.BringToFront();
    }

    // =====================================================================
    //  SQL ANALYZE - Scan branches for .sql files
    // =====================================================================

    private async void BtnSqlAnalyze_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(cmbSqlBaseBranch.Text))
        {
            MessageBox.Show("Selecione o branch base (ex: Develop).", "Atencao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var baseBranch = _git.ResolveBranch(cmbSqlBaseBranch.Text);
        if (baseBranch == null)
        {
            MessageBox.Show($"Branch '{cmbSqlBaseBranch.Text}' nao encontrado.\nExecute um Fetch primeiro.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _sqlCts = new CancellationTokenSource();
        var ct = _sqlCts.Token;

        // Filtrar branches ativos nos ultimos 120 dias
        var cutoffDate = DateTime.Now.AddDays(-120);
        var activeBranches = _allBranchesMetadata
            .Where(b => b.Date >= cutoffDate)
            .Where(b => !b.ShortName.Equals(cmbSqlBaseBranch.Text, StringComparison.OrdinalIgnoreCase))
            .Where(b => !b.ShortName.Equals("master", StringComparison.OrdinalIgnoreCase))
            .Where(b => !b.ShortName.Equals("main", StringComparison.OrdinalIgnoreCase))
            .Where(b => !b.ShortName.Equals("HEAD", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (activeBranches.Count == 0)
        {
            MessageBox.Show("Nenhum branch ativo encontrado nos ultimos 120 dias.", "Atencao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Setup UI
        SetStatus($"Analisando {activeBranches.Count} branches para scripts SQL...");
        SetBusy(true);
        btnSqlAnalyze.Visible = false;
        btnSqlCancel.Visible = true;
        btnSqlCancel.Enabled = true;
        btnSqlCancel.Text = "CANCELAR";
        btnSqlGenerate.Enabled = false;
        pgSql.Visible = true;
        pgSql.Minimum = 0;
        pgSql.Maximum = activeBranches.Count;
        pgSql.Value = 0;
        lblSqlEta.Visible = true;
        lblSqlEta.Text = "ETA: calculando...";

        // Obter lista de scripts SQL na branch base
        var baseSqlFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await Task.Run(() =>
        {
            var files = _git.GetAllSqlFiles(baseBranch);
            foreach (var f in files) baseSqlFiles.Add(f);
        }, ct);

        var results = new BranchSqlResult[activeBranches.Count];
        int processed = 0;
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var semaphore = new SemaphoreSlim(4);
            var tasks = activeBranches.Select((branch, index) => Task.Run(async () =>
            {
                await semaphore.WaitAsync(ct);
                try
                {
                    ct.ThrowIfCancellationRequested();

                    var resolved = _git.ResolveBranch(branch.ShortName);
                    if (resolved == null)
                    {
                        results[index] = new BranchSqlResult
                        {
                            Branch = branch.ShortName,
                            Autor = branch.Author,
                            Status = "NAO ENCONTRADO"
                        };
                        return;
                    }

                    try
                    {
                        var sqlFiles = _git.GetSqlFilesChanged(baseBranch, resolved);
                        if (sqlFiles.Count == 0)
                        {
                            results[index] = new BranchSqlResult
                            {
                                Branch = branch.ShortName,
                                Autor = branch.Author,
                                UltimaData = branch.DateShort,
                                Status = "SEM SCRIPTS"
                            };
                            return;
                        }

                        var added = sqlFiles.Count(f => f.Status == "Adicionado");
                        var modified = sqlFiles.Count(f => f.Status == "Modificado");

                        // Verificar quais scripts da feature existem na base
                        var allInBase = sqlFiles.All(f => baseSqlFiles.Contains(f.FilePath));
                        var someInBase = sqlFiles.Any(f => baseSqlFiles.Contains(f.FilePath));
                        string status;
                        if (allInBase)
                            status = "NA BASE";
                        else if (someInBase)
                            status = "PARCIAL";
                        else
                            status = "FALTANDO";

                        var lastDate = sqlFiles
                            .Where(f => !string.IsNullOrEmpty(f.Date))
                            .OrderByDescending(f => f.Date)
                            .FirstOrDefault()?.Date ?? branch.DateShort;

                        results[index] = new BranchSqlResult
                        {
                            Branch = branch.ShortName,
                            Autor = sqlFiles.FirstOrDefault()?.Author ?? branch.Author,
                            UltimaData = lastDate,
                            ScriptsAdicionados = added,
                            ScriptsModificados = modified,
                            TotalScripts = sqlFiles.Count,
                            Status = status,
                            Scripts = sqlFiles
                        };
                    }
                    catch
                    {
                        results[index] = new BranchSqlResult
                        {
                            Branch = branch.ShortName,
                            Autor = branch.Author,
                            Status = "ERRO"
                        };
                    }

                    var current = Interlocked.Increment(ref processed);
                    Invoke(() =>
                    {
                        pgSql.Value = current;
                        SetStatus($"Analisando SQL {current}/{activeBranches.Count}: {branch.ShortName}");

                        var elapsed = sw.Elapsed.TotalSeconds;
                        if (current > 0 && current < activeBranches.Count)
                        {
                            var avgPerItem = elapsed / current;
                            var remaining = (activeBranches.Count - current) * avgPerItem;
                            var eta = TimeSpan.FromSeconds(remaining);
                            lblSqlEta.Text = $"ETA: {eta:mm\\:ss}";
                        }
                        else if (current >= activeBranches.Count)
                        {
                            lblSqlEta.Text = $"Concluido em {sw.Elapsed.TotalSeconds:F1}s";
                        }
                    });
                }
                finally
                {
                    semaphore.Release();
                }
            }, ct)).ToArray();

            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            for (int i = 0; i < results.Length; i++)
            {
                results[i] ??= new BranchSqlResult
                {
                    Branch = activeBranches[i].ShortName,
                    Status = "CANCELADO"
                };
            }
        }

        sw.Stop();

        // Filtrar apenas branches que tem scripts SQL (excluir SEM SCRIPTS)
        _sqlResults = results
            .Where(r => r != null && r.Status != "SEM SCRIPTS")
            .OrderByDescending(r => r.TotalScripts)
            .ToList();

        dgvSqlResults.DataSource = null;
        dgvSqlResults.DataSource = _sqlResults;

        // Colorir linhas
        foreach (DataGridViewRow row in dgvSqlResults.Rows)
        {
            if (row.DataBoundItem is BranchSqlResult r)
            {
                row.DefaultCellStyle.ForeColor = r.Status switch
                {
                    "NA BASE" => Color.FromArgb(80, 220, 80),
                    "PARCIAL" => Color.FromArgb(255, 200, 80),
                    "FALTANDO" => Color.FromArgb(255, 100, 80),
                    "ERRO" => Color.FromArgb(180, 80, 80),
                    "CANCELADO" => Color.FromArgb(160, 160, 180),
                    _ => Color.FromArgb(220, 220, 230)
                };
            }
        }

        UpdateSqlDashboard(_sqlResults);

        var totalWithScripts = _sqlResults.Count;
        var missing = _sqlResults.Count(r => r.Status == "FALTANDO");
        var partial = _sqlResults.Count(r => r.Status == "PARCIAL");
        var inBase = _sqlResults.Count(r => r.Status == "NA BASE");
        var totalScripts = _sqlResults.Sum(r => r.TotalScripts);

        lblSqlSummary.Text = $"Analisados: {activeBranches.Count} branches | {totalWithScripts} com scripts SQL | {totalScripts} scripts total | {missing} faltando | {partial} parcial | {inBase} na base";
        SetStatus($"Analise SQL concluida em {sw.Elapsed.TotalSeconds:F1}s: {totalWithScripts} branches com scripts SQL");

        pgSql.Visible = false;
        lblSqlEta.Visible = false;
        btnSqlAnalyze.Visible = true;
        btnSqlCancel.Visible = false;
        btnSqlGenerate.Enabled = _sqlResults.Count > 0;
        RestoreDefaultCursor();
        _sqlCts?.Dispose();
        _sqlCts = null;
    }

    // =====================================================================
    //  SQL DASHBOARD
    // =====================================================================

    private void UpdateSqlDashboard(List<BranchSqlResult> results)
    {
        pnlSqlDashboard.Controls.Clear();
        pnlSqlDashboard.Visible = true;

        var total = results.Count;
        var missing = results.Count(r => r.Status == "FALTANDO");
        var partial = results.Count(r => r.Status == "PARCIAL");
        var inBase = results.Count(r => r.Status == "NA BASE");
        var totalScripts = results.Sum(r => r.TotalScripts);

        var cards = new[]
        {
            ("Branches c/ SQL", total.ToString(), Color.FromArgb(120, 180, 255)),
            ("Total Scripts", totalScripts.ToString(), Color.FromArgb(200, 200, 255)),
            ("Na Base", inBase.ToString(), Color.FromArgb(80, 220, 80)),
            ("Parcial", partial.ToString(), Color.FromArgb(255, 200, 80)),
            ("Faltando", missing.ToString(), Color.FromArgb(255, 100, 80))
        };

        int cardWidth = 140;
        int cardHeight = 48;
        int gap = 10;
        int x = 8;

        foreach (var (title, value, color) in cards)
        {
            var card = new Panel
            {
                Location = new Point(x, 6),
                Size = new Size(cardWidth, cardHeight),
                BackColor = Color.FromArgb(35, 35, 50)
            };
            card.Paint += (s, pe) =>
            {
                using var pen = new Pen(color, 2);
                pe.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            var lblValue = new Label
            {
                Text = value,
                ForeColor = color,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(8, 2),
                AutoSize = true
            };
            card.Controls.Add(lblValue);

            var lblTitle = new Label
            {
                Text = title,
                ForeColor = Color.FromArgb(160, 160, 180),
                Font = new Font("Segoe UI", 8f),
                Location = new Point(8, 30),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            pnlSqlDashboard.Controls.Add(card);
            x += cardWidth + gap;
        }
    }

    // =====================================================================
    //  GENERATE COMPLETE SQL SCRIPT
    // =====================================================================

    private void BtnSqlGenerate_Click(object? sender, EventArgs e)
    {
        var baseBranchName = cmbSqlBaseBranch.Text;
        var baseBranch = _git.ResolveBranch(baseBranchName);
        if (baseBranch == null)
        {
            MessageBox.Show("Branch base nao encontrado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Coletar todos os scripts de todos os branches (incluindo NA BASE para ter a versao completa)
        var allScripts = _sqlResults
            .Where(r => r.Scripts.Count > 0)
            .SelectMany(r => r.Scripts)
            .Where(s => s.Status == "Adicionado" || s.Status == "Modificado")
            .ToList();

        if (allScripts.Count == 0)
        {
            MessageBox.Show("Nenhum script SQL encontrado nos branches analisados.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Agrupar por caminho do arquivo (mesmo arquivo modificado em branches diferentes)
        var scriptsByFile = allScripts
            .GroupBy(s => s.FilePath, StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g.Key)
            .ToList();

        // Selecionar pasta destino
        using var dlg = new FolderBrowserDialog
        {
            Description = "Selecione a pasta para salvar os scripts SQL consolidados",
            ShowNewFolderButton = true,
            UseDescriptionForTitle = true
        };

        if (dlg.ShowDialog() != DialogResult.OK) return;

        var outputDir = Path.Combine(dlg.SelectedPath, $"sql_consolidado_{DateTime.Now:yyyyMMdd_HHmm}");
        Directory.CreateDirectory(outputDir);

        SetStatus("Gerando scripts SQL consolidados...");
        SetBusy(true);

        Task.Run(() =>
        {
            try
            {
                int filesGenerated = 0;
                var errors = new List<string>();
                var reportSb = new StringBuilder();

                reportSb.AppendLine("================================================================");
                reportSb.AppendLine("  RELATORIO DE SCRIPTS SQL CONSOLIDADOS");
                reportSb.AppendLine($"  Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}");
                reportSb.AppendLine($"  Branch base: {baseBranchName}");
                reportSb.AppendLine($"  Repositorio: {_git.RepoPath}");
                reportSb.AppendLine($"  Arquivos SQL unicos: {scriptsByFile.Count}");
                reportSb.AppendLine("================================================================");
                reportSb.AppendLine();

                foreach (var group in scriptsByFile)
                {
                    var filePath = group.Key;
                    var fileName = Path.GetFileName(filePath);
                    var branches = group.OrderByDescending(s => s.Date).ToList();

                    // Recriar a estrutura de pastas do repositorio
                    var relativePath = filePath.Replace('/', Path.DirectorySeparatorChar);
                    var fullOutputPath = Path.Combine(outputDir, relativePath);
                    var fileDir = Path.GetDirectoryName(fullOutputPath);
                    if (!string.IsNullOrEmpty(fileDir))
                        Directory.CreateDirectory(fileDir);

                    var sb = new StringBuilder();
                    sb.AppendLine("-- ================================================================");
                    sb.AppendLine($"-- SCRIPT CONSOLIDADO: {fileName}");
                    sb.AppendLine($"-- Caminho original: {filePath}");
                    sb.AppendLine($"-- Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}");
                    sb.AppendLine($"-- Branch base: {baseBranchName}");
                    sb.AppendLine($"-- Branches que alteraram este arquivo: {branches.Count}");
                    foreach (var b in branches)
                        sb.AppendLine($"--   - {b.Branch} ({b.Author}, {b.Date}) [{b.Status}]");
                    sb.AppendLine("-- ================================================================");
                    sb.AppendLine();

                    // Primeiro: pegar o conteudo da branch base (se existir)
                    bool hasBaseContent = false;
                    try
                    {
                        if (_git.FileExistsInBranch(baseBranch, filePath))
                        {
                            var baseContent = _git.GetFileContent(baseBranch, filePath);
                            if (!string.IsNullOrWhiteSpace(baseContent))
                            {
                                sb.AppendLine($"/* ============================================================");
                                sb.AppendLine($"   ORIGEM: {baseBranchName} (branch base)");
                                sb.AppendLine($"   ============================================================ */");
                                sb.AppendLine();
                                sb.AppendLine(baseContent);
                                sb.AppendLine();
                                hasBaseContent = true;
                            }
                        }
                    }
                    catch { /* ignorar erro ao ler base */ }

                    // Depois: para cada branch que modificou, adicionar as diferencas
                    // Pegar a versao mais recente de cada branch
                    var branchesSeen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var script in branches)
                    {
                        if (!branchesSeen.Add(script.Branch)) continue;

                        try
                        {
                            var resolved = _git.ResolveBranch(script.Branch);
                            if (resolved == null)
                            {
                                sb.AppendLine($"-- ERRO: Branch {script.Branch} nao encontrado");
                                errors.Add($"{filePath} ({script.Branch}): branch nao encontrado");
                                continue;
                            }

                            var content = _git.GetFileContent(resolved, filePath);
                            if (string.IsNullOrWhiteSpace(content))
                            {
                                errors.Add($"{filePath} ({script.Branch}): conteudo vazio");
                                continue;
                            }

                            // Se ja tem conteudo da base, adicionar apenas o que vem do branch
                            if (hasBaseContent)
                            {
                                sb.AppendLine("GO");
                                sb.AppendLine();
                                sb.AppendLine($"/* ============================================================");
                                sb.AppendLine($"   ORIGEM: {script.Branch}");
                                sb.AppendLine($"   Autor: {script.Author}");
                                sb.AppendLine($"   Data: {script.Date}");
                                sb.AppendLine($"   Commit: {script.CommitMessage}");
                                sb.AppendLine($"   ============================================================ */");
                                sb.AppendLine();

                                // Pegar apenas o diff (linhas adicionadas) entre base e branch
                                var diffContent = _git.GetFileContent(resolved, filePath);
                                var baseContent2 = _git.GetFileContent(baseBranch, filePath);
                                var addedLines = GetAddedLines(baseContent2, diffContent);
                                if (!string.IsNullOrWhiteSpace(addedLines))
                                {
                                    sb.AppendLine(addedLines);
                                }
                                else
                                {
                                    sb.AppendLine("-- (sem linhas novas em relacao a base)");
                                }
                            }
                            else
                            {
                                // Nao tem na base, pegar conteudo completo do branch
                                sb.AppendLine($"/* ============================================================");
                                sb.AppendLine($"   ORIGEM: {script.Branch}");
                                sb.AppendLine($"   Autor: {script.Author}");
                                sb.AppendLine($"   Data: {script.Date}");
                                sb.AppendLine($"   Commit: {script.CommitMessage}");
                                sb.AppendLine($"   ============================================================ */");
                                sb.AppendLine();
                                sb.AppendLine(content);
                                sb.AppendLine();
                                // Marcar que ja temos conteudo para nao duplicar
                                hasBaseContent = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            sb.AppendLine($"-- ERRO ao ler de {script.Branch}: {ex.Message}");
                            errors.Add($"{filePath} ({script.Branch}): {ex.Message}");
                        }
                    }

                    File.WriteAllText(fullOutputPath, sb.ToString(), Encoding.UTF8);
                    filesGenerated++;

                    reportSb.AppendLine($"  [{(hasBaseContent ? "OK" : "NOVO")}] {filePath}");
                    foreach (var b in branches.DistinctBy(x => x.Branch))
                        reportSb.AppendLine($"        -> {b.Branch} ({b.Author}, {b.Date})");
                }

                // Salvar relatorio
                reportSb.AppendLine();
                reportSb.AppendLine("================================================================");
                reportSb.AppendLine($"  Total de arquivos gerados: {filesGenerated}");
                if (errors.Count > 0)
                {
                    reportSb.AppendLine($"  Erros: {errors.Count}");
                    foreach (var err in errors)
                        reportSb.AppendLine($"    - {err}");
                }
                reportSb.AppendLine("================================================================");

                File.WriteAllText(Path.Combine(outputDir, "_RELATORIO.txt"), reportSb.ToString(), Encoding.UTF8);

                Invoke(() =>
                {
                    RestoreDefaultCursor();
                    var msg = $"Scripts consolidados salvos em:\n{outputDir}\n\n{filesGenerated} arquivo(s) gerado(s).";
                    if (errors.Count > 0)
                        msg += $"\n\n{errors.Count} erro(s) - verifique o _RELATORIO.txt.";
                    SetStatus($"Scripts SQL gerados em: {outputDir}");
                    MessageBox.Show(msg, "Scripts Gerados", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Abrir a pasta no explorer
                    try { System.Diagnostics.Process.Start("explorer.exe", outputDir); } catch { }
                });
            }
            catch (Exception ex)
            {
                Invoke(() =>
                {
                    RestoreDefaultCursor();
                    SetStatus($"Erro ao gerar scripts: {ex.Message}");
                    MessageBox.Show($"Erro ao gerar scripts:\n{ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
        });
    }

    /// <summary>
    /// Compara duas versoes de um arquivo e retorna apenas as linhas adicionadas na versao nova.
    /// </summary>
    private static string GetAddedLines(string baseContent, string newContent)
    {
        var baseLines = new HashSet<string>(
            (baseContent ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.TrimEnd('\r').Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
        );

        var newLines = (newContent ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var added = new StringBuilder();

        foreach (var line in newLines)
        {
            var trimmed = line.TrimEnd('\r').Trim();
            if (!string.IsNullOrWhiteSpace(trimmed) && !baseLines.Contains(trimmed))
            {
                added.AppendLine(line.TrimEnd('\r'));
            }
        }

        return added.ToString().TrimEnd();
    }

    // =====================================================================
    //  SQL DRILL-DOWN (Double-click on row)
    // =====================================================================

    private void DgvSqlResults_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        if (dgvSqlResults.Rows[e.RowIndex].DataBoundItem is not BranchSqlResult result) return;
        if (result.Scripts.Count == 0) return;

        var dlg = new Form
        {
            Text = $"Scripts SQL: {result.Branch}",
            Size = new Size(950, 500),
            MinimumSize = new Size(700, 400),
            StartPosition = FormStartPosition.CenterParent,
            BackColor = Color.FromArgb(24, 24, 32),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.5f),
            Icon = Icon
        };

        var dgvScripts = CreateDataGrid();
        dgvScripts.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "FileName", HeaderText = "Arquivo", Width = 220, DataPropertyName = "FileName" },
            new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 100, DataPropertyName = "Status" },
            new DataGridViewTextBoxColumn { Name = "Author", HeaderText = "Autor", Width = 150, DataPropertyName = "Author" },
            new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Data", Width = 100, DataPropertyName = "Date" },
            new DataGridViewTextBoxColumn { Name = "CommitMessage", HeaderText = "Commit", Width = 300, DataPropertyName = "CommitMessage" },
            new DataGridViewTextBoxColumn { Name = "FilePath", HeaderText = "Caminho", Width = 400, DataPropertyName = "FilePath" }
        );
        dgvScripts.DataSource = result.Scripts;

        // Colorir por status
        dgvScripts.DataBindingComplete += (_, _) =>
        {
            foreach (DataGridViewRow row in dgvScripts.Rows)
            {
                if (row.DataBoundItem is SqlScriptInfo s)
                {
                    row.DefaultCellStyle.ForeColor = s.Status == "Adicionado"
                        ? Color.FromArgb(80, 220, 120)
                        : Color.FromArgb(255, 200, 80);
                }
            }
        };

        // Header
        var pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40,
            BackColor = Color.FromArgb(30, 30, 42),
            Padding = new Padding(8)
        };
        var lblHeader = new Label
        {
            Text = $"Branch: {result.Branch}  |  Autor: {result.Autor}  |  {result.TotalScripts} script(s)  |  Status: {result.Status}",
            ForeColor = Color.FromArgb(120, 180, 255),
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        pnlHeader.Controls.Add(lblHeader);

        dlg.Controls.Add(dgvScripts);
        dlg.Controls.Add(pnlHeader);
        dgvScripts.BringToFront();

        dlg.ShowDialog(this);
    }
}
