namespace BranchAnalyzer;

/// <summary>
/// Diálogo para selecionar repositório: pasta local ou URL remota.
/// </summary>
public class RepoSelectDialog : Form
{
    private TabControl tabs = null!;
    private TextBox txtUrl = null!;
    private TextBox txtLocalPath = null!;
    private Button btnBrowse = null!;
    private Button btnOk = null!;
    private Button btnCancel = null!;
    private Label lblProgress = null!;
    private ProgressBar progressBar = null!;
    private ListBox lstRecent = null!;

    /// <summary>Caminho local do repositório selecionado (resultado)</summary>
    public string SelectedRepoPath { get; private set; } = "";

    /// <summary>URL original se foi clonado via URL</summary>
    public string? SelectedRepoUrl { get; private set; }

    private readonly List<string> _recentPaths;
    private readonly string? _customCachePath;

    public RepoSelectDialog(List<string> recentPaths, string? customCachePath = null)
    {
        _recentPaths = recentPaths ?? new();
        _customCachePath = customCachePath;
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        Text = "Selecionar Repositório";
        Size = new Size(560, 420);
        MinimumSize = new Size(500, 380);
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        BackColor = Color.FromArgb(24, 24, 32);
        ForeColor = Color.FromArgb(220, 220, 230);
        Font = new Font("Segoe UI", 9.5f);

        tabs = new TabControl
        {
            Dock = DockStyle.Top,
            Height = 280,
            Font = new Font("Segoe UI", 9.5f)
        };

        // ── Tab: Pasta Local ──────────────────────────────────────
        var tabLocal = new TabPage("📁 Pasta Local")
        {
            BackColor = Color.FromArgb(28, 28, 38),
            Padding = new Padding(12)
        };

        var lblLocal = new Label
        {
            Text = "Caminho do repositório Git:",
            ForeColor = Color.FromArgb(180, 180, 200),
            Location = new Point(12, 15),
            AutoSize = true
        };
        tabLocal.Controls.Add(lblLocal);

        txtLocalPath = new TextBox
        {
            Location = new Point(12, 38),
            Size = new Size(420, 26),
            BackColor = Color.FromArgb(35, 35, 50),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Consolas", 9.5f)
        };
        tabLocal.Controls.Add(txtLocalPath);

        btnBrowse = new Button
        {
            Text = "...",
            Location = new Point(438, 37),
            Size = new Size(40, 26),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(50, 50, 70),
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };
        btnBrowse.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 100);
        btnBrowse.Click += BtnBrowse_Click;
        tabLocal.Controls.Add(btnBrowse);

        // Lista de recentes
        if (_recentPaths.Count > 0)
        {
            var lblRecent = new Label
            {
                Text = "Repositórios recentes:",
                ForeColor = Color.FromArgb(180, 180, 200),
                Location = new Point(12, 75),
                AutoSize = true
            };
            tabLocal.Controls.Add(lblRecent);

            lstRecent = new ListBox
            {
                Location = new Point(12, 98),
                Size = new Size(466, 140),
                BackColor = Color.FromArgb(35, 35, 50),
                ForeColor = Color.FromArgb(120, 180, 255),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 9f)
            };
            foreach (var p in _recentPaths)
                lstRecent.Items.Add(p);
            lstRecent.SelectedIndexChanged += (_, _) =>
            {
                if (lstRecent.SelectedItem is string path)
                    txtLocalPath.Text = path;
            };
            lstRecent.DoubleClick += (_, _) =>
            {
                if (lstRecent.SelectedItem is string path)
                {
                    txtLocalPath.Text = path;
                    ConfirmLocal();
                }
            };
            tabLocal.Controls.Add(lstRecent);
        }

        tabs.TabPages.Add(tabLocal);

        // ── Tab: URL Remota ───────────────────────────────────────
        var tabUrl = new TabPage("🌐 URL Remota")
        {
            BackColor = Color.FromArgb(28, 28, 38),
            Padding = new Padding(12)
        };

        var lblUrl = new Label
        {
            Text = "URL do repositório Git (HTTPS ou SSH):",
            ForeColor = Color.FromArgb(180, 180, 200),
            Location = new Point(12, 15),
            AutoSize = true
        };
        tabUrl.Controls.Add(lblUrl);

        txtUrl = new TextBox
        {
            Location = new Point(12, 38),
            Size = new Size(466, 26),
            BackColor = Color.FromArgb(35, 35, 50),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Consolas", 9.5f)
        };
        txtUrl.PlaceholderText = "https://github.com/user/repo.git";
        tabUrl.Controls.Add(txtUrl);

        var lblUrlHint = new Label
        {
            Text = "Ex: https://github.com/user/repo.git\n" +
                   "      git@github.com:user/repo.git\n" +
                   "      https://dev.azure.com/org/project/_git/repo",
            ForeColor = Color.FromArgb(120, 120, 140),
            Location = new Point(12, 72),
            Size = new Size(466, 55),
            Font = new Font("Consolas", 8.5f)
        };
        tabUrl.Controls.Add(lblUrlHint);

        var lblCacheInfo = new Label
        {
            Text = "O repositório será clonado para uma pasta cache local.\n" +
                   "Clones subsequentes serão apenas atualizados (fetch).",
            ForeColor = Color.FromArgb(100, 180, 100),
            Location = new Point(12, 135),
            Size = new Size(466, 35),
            Font = new Font("Segoe UI", 8.5f)
        };
        tabUrl.Controls.Add(lblCacheInfo);

        progressBar = new ProgressBar
        {
            Location = new Point(12, 180),
            Size = new Size(466, 18),
            Style = ProgressBarStyle.Marquee,
            MarqueeAnimationSpeed = 30,
            Visible = false
        };
        tabUrl.Controls.Add(progressBar);

        lblProgress = new Label
        {
            Text = "",
            ForeColor = Color.FromArgb(120, 180, 255),
            Location = new Point(12, 205),
            Size = new Size(466, 40),
            Font = new Font("Consolas", 8f)
        };
        tabUrl.Controls.Add(lblProgress);

        tabs.TabPages.Add(tabUrl);
        Controls.Add(tabs);

        // ── Botões OK / Cancelar ──────────────────────────────────
        var pnlButtons = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            BackColor = Color.FromArgb(24, 24, 32),
            Padding = new Padding(12, 8, 12, 8)
        };

        btnCancel = new Button
        {
            Text = "Cancelar",
            DialogResult = DialogResult.Cancel,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(50, 50, 70),
            ForeColor = Color.White,
            Size = new Size(100, 32),
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };
        btnCancel.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 100);
        btnCancel.Location = new Point(pnlButtons.Width - btnCancel.Width - 12, 8);
        pnlButtons.Controls.Add(btnCancel);

        btnOk = new Button
        {
            Text = "Confirmar",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(40, 100, 200),
            ForeColor = Color.White,
            Size = new Size(120, 32),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };
        btnOk.FlatAppearance.BorderColor = Color.FromArgb(60, 120, 220);
        btnOk.Location = new Point(btnCancel.Left - btnOk.Width - 8, 8);
        btnOk.Click += BtnOk_Click;
        pnlButtons.Controls.Add(btnOk);

        Controls.Add(pnlButtons);

        CancelButton = btnCancel;
        AcceptButton = btnOk;
    }

    private void BtnBrowse_Click(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description = "Selecione a pasta do repositório Git",
            UseDescriptionForTitle = true
        };
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            txtLocalPath.Text = dlg.SelectedPath;
        }
    }

    private void BtnOk_Click(object? sender, EventArgs e)
    {
        if (tabs.SelectedIndex == 0)
        {
            // Pasta local
            ConfirmLocal();
        }
        else
        {
            // URL remota
            ConfirmUrl();
        }
    }

    private void ConfirmLocal()
    {
        var path = txtLocalPath.Text.Trim();
        if (string.IsNullOrEmpty(path))
        {
            MessageBox.Show("Informe o caminho do repositório.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!Directory.Exists(Path.Combine(path, ".git")))
        {
            MessageBox.Show("A pasta selecionada não é um repositório Git.",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SelectedRepoPath = path;
        SelectedRepoUrl = null;
        DialogResult = DialogResult.OK;
        Close();
    }

    private async void ConfirmUrl()
    {
        var url = txtUrl.Text.Trim();
        if (string.IsNullOrEmpty(url))
        {
            MessageBox.Show("Informe a URL do repositório.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Desabilitar controles durante o clone
        btnOk.Enabled = false;
        btnCancel.Enabled = false;
        txtUrl.Enabled = false;
        tabs.Enabled = false;
        progressBar.Visible = true;
        lblProgress.Text = "Verificando URL...";
        lblProgress.ForeColor = Color.FromArgb(120, 180, 255);

        try
        {
            string clonedPath = "";
            string error = "";

            await Task.Run(() =>
            {
                (clonedPath, error) = GitService.CloneRepository(url, progress =>
                {
                    try
                    {
                        Invoke(() => lblProgress.Text = progress);
                    }
                    catch (Exception ex) { Logger.Warn($"Clone progress UI update failed: {ex.Message}"); }
                }, _customCachePath);
            });

            if (!string.IsNullOrEmpty(error) && string.IsNullOrEmpty(clonedPath))
            {
                lblProgress.Text = error;
                lblProgress.ForeColor = Color.FromArgb(255, 100, 100);
                return;
            }

            SelectedRepoPath = clonedPath;
            SelectedRepoUrl = url;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            lblProgress.Text = $"Erro: {ex.Message}";
            lblProgress.ForeColor = Color.FromArgb(255, 100, 100);
        }
        finally
        {
            btnOk.Enabled = true;
            btnCancel.Enabled = true;
            txtUrl.Enabled = true;
            tabs.Enabled = true;
            progressBar.Visible = false;
        }
    }
}
