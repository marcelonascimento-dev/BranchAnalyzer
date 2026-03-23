namespace BranchAnalyzer;

public partial class Form1 : Form
{
    private void SetupMyBranchTab()
    {
        tabMyBranch = CreateTab("Meu Branch");

        // Barra superior com botao Atualizar
        var pnlMyBranchTop = new Panel
        {
            Dock = DockStyle.Top,
            Height = 36,
            BackColor = Color.FromArgb(30, 30, 42),
            Padding = new Padding(8, 5, 8, 5)
        };
        var btnRefreshMyBranch = new Button
        {
            Text = "\u21BB  Atualizar",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(40, 100, 200),
            ForeColor = Color.White,
            Size = new Size(110, 26),
            Location = new Point(8, 5),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        };
        btnRefreshMyBranch.FlatAppearance.BorderColor = Color.FromArgb(60, 120, 220);
        btnRefreshMyBranch.Click += (_, _) => LoadMyBranchInfo();
        pnlMyBranchTop.Controls.Add(btnRefreshMyBranch);

        // Resumo do branch (RichTextBox compacto na esquerda)
        _rtbMyBranch = CreateRichTextBox();

        // Grid: commits recentes
        _dgvMyCommits = CreateDataGrid();
        _dgvMyCommits.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Hash", HeaderText = "Hash", Width = 85, DataPropertyName = "Hash" },
            new DataGridViewTextBoxColumn { Name = "Author", HeaderText = "Autor", Width = 170, DataPropertyName = "Author" },
            new DataGridViewTextBoxColumn { Name = "RelativeDate", HeaderText = "Quando", Width = 110, DataPropertyName = "RelativeDate" },
            new DataGridViewTextBoxColumn { Name = "Message", HeaderText = "Mensagem", Width = 500, DataPropertyName = "Message" }
        );

        // Grid: arquivos modificados localmente
        _dgvLocalChanges = CreateDataGrid();
        _dgvLocalChanges.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 100, DataPropertyName = "Status" },
            new DataGridViewTextBoxColumn { Name = "FilePath", HeaderText = "Arquivo", Width = 500, DataPropertyName = "FilePath" }
        );

        // -- Layout: 3 secoes verticais --
        var splitMyBranch = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 320,
            SplitterWidth = 3,
            BackColor = Color.FromArgb(40, 40, 55),
            Panel1MinSize = 150,
            Panel2MinSize = 150
        };

        var splitInfoAndChanges = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 180,
            SplitterWidth = 3,
            BackColor = Color.FromArgb(40, 40, 55),
            Panel1MinSize = 100,
            Panel2MinSize = 80
        };

        // Secao: Resumo do branch
        var pnlInfoSection = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(24, 24, 32)
        };
        var lblInfoTitle = new Label
        {
            Text = "\u2139  Informacoes do Branch",
            ForeColor = Color.FromArgb(120, 180, 255),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 26,
            Padding = new Padding(8, 5, 0, 0),
            BackColor = Color.FromArgb(28, 28, 42)
        };
        pnlInfoSection.Controls.Add(_rtbMyBranch);
        pnlInfoSection.Controls.Add(lblInfoTitle);
        splitInfoAndChanges.Panel1.Controls.Add(pnlInfoSection);
        splitInfoAndChanges.Panel1.BackColor = Color.FromArgb(24, 24, 32);

        // Secao: Alteracoes locais
        var pnlLocalSection = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(24, 24, 32)
        };
        var lblLocalChanges = new Label
        {
            Text = "\u270E  Alteracoes locais (nao commitadas)",
            ForeColor = Color.FromArgb(255, 180, 60),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 26,
            Padding = new Padding(8, 5, 0, 0),
            BackColor = Color.FromArgb(28, 28, 42)
        };
        pnlLocalSection.Controls.Add(_dgvLocalChanges);
        pnlLocalSection.Controls.Add(lblLocalChanges);
        splitInfoAndChanges.Panel2.Controls.Add(pnlLocalSection);
        splitInfoAndChanges.Panel2.BackColor = Color.FromArgb(24, 24, 32);

        splitMyBranch.Panel1.Controls.Add(splitInfoAndChanges);
        splitMyBranch.Panel1.BackColor = Color.FromArgb(24, 24, 32);

        // Secao: Commits recentes
        var pnlCommitsSection = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(24, 24, 32)
        };
        var lblRecentCommits = new Label
        {
            Text = "\u23F0  Ultimos commits no branch atual",
            ForeColor = Color.FromArgb(120, 180, 255),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 26,
            Padding = new Padding(8, 5, 0, 0),
            BackColor = Color.FromArgb(28, 28, 42)
        };
        pnlCommitsSection.Controls.Add(_dgvMyCommits);
        pnlCommitsSection.Controls.Add(lblRecentCommits);
        splitMyBranch.Panel2.Controls.Add(pnlCommitsSection);
        splitMyBranch.Panel2.BackColor = Color.FromArgb(24, 24, 32);

        tabMyBranch.Controls.Add(splitMyBranch);
        tabMyBranch.Controls.Add(pnlMyBranchTop);
        pnlMyBranchTop.BringToFront();
    }

    private void LoadMyBranchInfo()
    {
        if (string.IsNullOrEmpty(_git.RepoPath)) return;

        SetStatus("Carregando informacoes do branch atual...");
        UseWaitCursor = true; Application.DoEvents();

        Task.Run(() =>
        {
            try
            {
                var branch = _git.GetCurrentBranch();
                var (ahead, behind) = _git.GetAheadBehind();
                var recentCommits = _git.GetRecentCommits(30);
                var localChanges = _git.GetLocalChanges();
                var stashes = _git.GetStashes();
                var localBranches = _git.GetLocalBranchesInfo();

                Invoke(() =>
                {
                    _rtbMyBranch.Clear();

                    AppendRtb(_rtbMyBranch, "\n  BRANCH ATUAL\n", Color.FromArgb(120, 180, 255), bold: true);
                    AppendRtb(_rtbMyBranch, $"  Nome: ", Color.FromArgb(140, 140, 160));
                    AppendRtb(_rtbMyBranch, $"{branch}\n\n", Color.FromArgb(80, 220, 120));

                    AppendRtb(_rtbMyBranch, "  STATUS vs REMOTE\n", Color.FromArgb(120, 180, 255), bold: true);
                    if (ahead > 0)
                    {
                        AppendRtb(_rtbMyBranch, $"  Ahead: ", Color.FromArgb(140, 140, 160));
                        AppendRtb(_rtbMyBranch, $"{ahead} commit(s) a frente do remote\n", Color.FromArgb(255, 180, 60));
                    }
                    if (behind > 0)
                    {
                        AppendRtb(_rtbMyBranch, $"  Behind: ", Color.FromArgb(140, 140, 160));
                        AppendRtb(_rtbMyBranch, $"{behind} commit(s) atras do remote\n", Color.FromArgb(255, 100, 80));
                    }
                    if (ahead == 0 && behind == 0)
                    {
                        AppendRtb(_rtbMyBranch, $"  Sincronizado com o remote\n", Color.FromArgb(80, 220, 80));
                    }

                    AppendRtb(_rtbMyBranch, $"\n  ALTERACOES LOCAIS\n", Color.FromArgb(120, 180, 255), bold: true);
                    if (localChanges.Count == 0)
                        AppendRtb(_rtbMyBranch, "  Nenhuma alteracao local pendente\n", Color.FromArgb(80, 220, 80));
                    else
                        AppendRtb(_rtbMyBranch, $"  {localChanges.Count} arquivo(s) modificado(s)\n", Color.FromArgb(255, 180, 60));

                    AppendRtb(_rtbMyBranch, $"\n  STASHES\n", Color.FromArgb(120, 180, 255), bold: true);
                    if (stashes.Count == 0)
                        AppendRtb(_rtbMyBranch, "  Nenhum stash salvo\n", Color.FromArgb(140, 140, 160));
                    else
                    {
                        AppendRtb(_rtbMyBranch, $"  {stashes.Count} stash(es):\n", Color.FromArgb(255, 200, 80));
                        foreach (var s in stashes.Take(5))
                            AppendRtb(_rtbMyBranch, $"    {s}\n", Color.FromArgb(180, 180, 200));
                    }

                    AppendRtb(_rtbMyBranch, $"\n  BRANCHES LOCAIS ({localBranches.Count})\n", Color.FromArgb(120, 180, 255), bold: true);
                    foreach (var lb in localBranches.Take(10))
                    {
                        var isCurrent = lb.Name == branch;
                        var prefix = isCurrent ? " > " : "   ";
                        var nameColor = isCurrent ? Color.FromArgb(80, 220, 120) : Color.FromArgb(200, 200, 220);
                        AppendRtb(_rtbMyBranch, prefix, Color.FromArgb(140, 140, 160));
                        AppendRtb(_rtbMyBranch, $"{lb.Name,-40} ", nameColor);
                        AppendRtb(_rtbMyBranch, $"{lb.Date}  ", Color.FromArgb(100, 100, 130));
                        AppendRtb(_rtbMyBranch, $"{lb.LastCommit}\n", Color.FromArgb(160, 160, 180));
                    }
                    if (localBranches.Count > 10)
                        AppendRtb(_rtbMyBranch, $"   ... e mais {localBranches.Count - 10} branches\n", Color.FromArgb(100, 100, 130));

                    _dgvMyCommits.DataSource = null;
                    _dgvMyCommits.DataSource = recentCommits;

                    _dgvLocalChanges.DataSource = null;
                    _dgvLocalChanges.DataSource = localChanges;
                    foreach (DataGridViewRow row in _dgvLocalChanges.Rows)
                    {
                        if (row.DataBoundItem is FileChange fc)
                        {
                            row.DefaultCellStyle.ForeColor = fc.Status switch
                            {
                                "Modificado" => Color.FromArgb(255, 200, 80),
                                "Adicionado" => Color.FromArgb(80, 220, 80),
                                "Removido" => Color.FromArgb(220, 80, 80),
                                "Nao rastreado" => Color.FromArgb(140, 140, 160),
                                _ => Color.White
                            };
                        }
                    }

                    lblCurrentBranch.Text = branch;
                    SetStatus($"Branch atual: {branch} | {localChanges.Count} alteracoes locais | {ahead} ahead | {behind} behind");
                    RestoreDefaultCursor();
                });
            }
            catch (Exception ex)
            {
                Invoke(() => { SetStatus($"Erro: {ex.Message}"); RestoreDefaultCursor(); });
            }
        });
    }
}
