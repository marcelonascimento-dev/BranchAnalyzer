using System.Diagnostics;

namespace BranchAnalyzer;

public partial class Form1 : Form
{
    private readonly GitService _git = new();
    private readonly AppSettings _settings = AppSettings.Load();

    // ── Controles ──────────────────────────────────────────────────
    private TextBox txtBranchA = null!;
    private ListBox lstBranchA = null!;
    private TextBox txtBranchB = null!;
    private ListBox lstBranchB = null!;
    private Button btnSetRepo = null!;
    private Button btnFetch = null!;
    private Button btnSwap = null!;
    private Label lblRepo = null!;
    private Label lblStatus = null!;
    private Label lblCurrentBranch = null!;
    private Panel pnlStatus = null!;
    private TabControl tabs = null!;

    // Tabs
    private TabPage tabMergeStatus = null!;

    // Status merge
    private Label lblMergeResult = null!;
    private Label lblMergePending = null!;
    private Label lblMergeAhead = null!;
    private Label lblMergeBase = null!;
    private Panel pnlMergeIcon = null!;
    private DataGridView dgvMergeCommits = null!;

    // Meu Branch
    private TabPage tabMyBranch = null!;
    private RichTextBox _rtbMyBranch = null!;
    private DataGridView _dgvMyCommits = null!;
    private DataGridView _dgvLocalChanges = null!;

    // Batch (Verificacao em Lote)
    private TabPage tabBatch = null!;
    private TextBox txtBatchReceptor = null!;
    private ListBox lstBatchReceptor = null!;
    private CheckedListBox clbBatchBranches = null!;
    private DataGridView dgvBatchResults = null!;
    private Button btnBatchAnalyze = null!;
    private Button btnBatchSelectAll = null!;
    private Button btnBatchDeselectAll = null!;
    private Button btnBatchExport = null!;
    private TextBox txtBatchFilter = null!;
    private Label lblBatchCount = null!;
    private ProgressBar pgBatch = null!;
    // Filtros avancados do Lote
    private ComboBox cmbBatchFilterAuthor = null!;
    private ComboBox cmbBatchFilterPrefix = null!;
    private ComboBox cmbBatchFilterDays = null!;
    private Button btnBatchApplyFilters = null!;
    private Button btnBatchClearFilters = null!;
    private SplitContainer splitBatch = null!;
    private List<BranchMetadata> _allBranchesMetadata = new();

    public Form1()
    {
        InitializeComponent();
        SetupUI();
        RestoreWindowState();
        FormClosing += (_, _) => SaveSettings();
    }

    // ══════════════════════════════════════════════════════════════════
    //  UI SETUP
    // ══════════════════════════════════════════════════════════════════

    private void SetupUI()
    {
        Text = "Branch Analyzer";
        Size = new Size(1280, 850);
        MinimumSize = new Size(1000, 700);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(24, 24, 32);
        Font = new Font("Segoe UI", 9.5f);
        Icon = CreateAppIcon();

        // ── Painel superior ────────────────────────────────────────
        var pnlTop = new Panel
        {
            Dock = DockStyle.Top,
            Height = 140,
            BackColor = Color.FromArgb(30, 30, 42),
            Padding = new Padding(16, 10, 16, 10)
        };
        Controls.Add(pnlTop);

        // Titulo
        var lblTitle = new Label
        {
            Text = "BRANCH ANALYZER",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(120, 180, 255),
            AutoSize = true,
            Location = new Point(16, 8)
        };
        pnlTop.Controls.Add(lblTitle);

        var lblSubtitle = new Label
        {
            Text = "Ferramenta de analise de branches  |  Somente leitura",
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = Color.FromArgb(140, 140, 160),
            AutoSize = true,
            Location = new Point(18, 38)
        };
        pnlTop.Controls.Add(lblSubtitle);

        // Repositorio
        var lblRepoLabel = new Label
        {
            Text = "Repositorio:",
            ForeColor = Color.FromArgb(180, 180, 200),
            AutoSize = true,
            Location = new Point(16, 62)
        };
        pnlTop.Controls.Add(lblRepoLabel);

        lblRepo = new Label
        {
            Text = "(nenhum selecionado)",
            ForeColor = Color.FromArgb(255, 200, 80),
            AutoSize = true,
            Font = new Font("Consolas", 9),
            Location = new Point(110, 62)
        };
        pnlTop.Controls.Add(lblRepo);

        // Branch atual
        var lblCurrentLabel = new Label
        {
            Text = "Branch atual:",
            ForeColor = Color.FromArgb(180, 180, 200),
            AutoSize = true,
            Location = new Point(16, 82)
        };
        pnlTop.Controls.Add(lblCurrentLabel);

        lblCurrentBranch = new Label
        {
            Text = "",
            ForeColor = Color.FromArgb(80, 220, 120),
            AutoSize = true,
            Font = new Font("Consolas", 9.5f, FontStyle.Bold),
            Location = new Point(110, 82)
        };
        pnlTop.Controls.Add(lblCurrentBranch);

        btnSetRepo = new Button
        {
            Text = "Selecionar Repo",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(50, 50, 70),
            ForeColor = Color.White,
            Size = new Size(130, 28),
            Location = new Point(16, 95),
            Cursor = Cursors.Hand
        };
        btnSetRepo.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 100);
        btnSetRepo.Click += BtnSetRepo_Click;
        pnlTop.Controls.Add(btnSetRepo);

        // Botão Fetch Origin (estilo Visual Studio)
        btnFetch = new Button
        {
            Text = "↓ Fetch Origin",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(50, 50, 70),
            ForeColor = Color.White,
            Size = new Size(120, 28),
            Location = new Point(155, 95),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9f)
        };
        btnFetch.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 100);
        btnFetch.Click += BtnFetch_Click;
        pnlTop.Controls.Add(btnFetch);

        // Botão dropdown Fetch Completo (com prune)
        var btnFetchFull = new Button
        {
            Text = "▼",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(50, 50, 70),
            ForeColor = Color.FromArgb(160, 160, 180),
            Size = new Size(24, 28),
            Location = new Point(275, 95),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 7f)
        };
        btnFetchFull.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 100);
        var fetchMenu = new ContextMenuStrip();
        fetchMenu.BackColor = Color.FromArgb(30, 30, 45);
        fetchMenu.ForeColor = Color.White;
        fetchMenu.Renderer = new DarkMenuRenderer();
        fetchMenu.Items.Add("↓  Fetch Origin (rapido)", null, (_, _) => BtnFetch_Click(null, EventArgs.Empty));
        fetchMenu.Items.Add("↓  Fetch + Prune (completo)", null, (_, _) => BtnFetchFull_Click());
        btnFetchFull.Click += (_, _) => fetchMenu.Show(btnFetchFull, new Point(0, btnFetchFull.Height));
        pnlTop.Controls.Add(btnFetchFull);

        // Branch A
        var lblA = new Label
        {
            Text = "Branch Receptor (A):",
            ForeColor = Color.FromArgb(100, 220, 100),
            AutoSize = true,
            Location = new Point(340, 62)
        };
        pnlTop.Controls.Add(lblA);

        txtBranchA = new TextBox
        {
            Location = new Point(340, 82),
            Size = new Size(280, 28),
            BackColor = Color.FromArgb(40, 40, 55),
            ForeColor = Color.White,
            Font = new Font("Consolas", 9.5f),
            BorderStyle = BorderStyle.FixedSingle
        };
        lstBranchA = new ListBox
        {
            Visible = false,
            BackColor = Color.FromArgb(30, 30, 50),
            ForeColor = Color.White,
            Font = new Font("Consolas", 9f),
            BorderStyle = BorderStyle.FixedSingle,
            IntegralHeight = false
        };
        SetupBranchAutocomplete(txtBranchA, lstBranchA, new Point(340, 110));
        pnlTop.Controls.Add(txtBranchA);

        // Swap
        btnSwap = new Button
        {
            Text = "<>",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(50, 50, 70),
            ForeColor = Color.FromArgb(200, 200, 255),
            Size = new Size(35, 28),
            Location = new Point(628, 82),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        btnSwap.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 100);
        btnSwap.Click += (_, _) =>
        {
            (txtBranchA.Text, txtBranchB.Text) = (txtBranchB.Text, txtBranchA.Text);
        };
        pnlTop.Controls.Add(btnSwap);

        // Branch B
        var lblB = new Label
        {
            Text = "Branch Feature (B):",
            ForeColor = Color.FromArgb(255, 180, 80),
            AutoSize = true,
            Location = new Point(672, 62)
        };
        pnlTop.Controls.Add(lblB);

        txtBranchB = new TextBox
        {
            Location = new Point(672, 82),
            Size = new Size(280, 28),
            BackColor = Color.FromArgb(40, 40, 55),
            ForeColor = Color.White,
            Font = new Font("Consolas", 9.5f),
            BorderStyle = BorderStyle.FixedSingle
        };
        lstBranchB = new ListBox
        {
            Visible = false,
            BackColor = Color.FromArgb(30, 30, 50),
            ForeColor = Color.White,
            Font = new Font("Consolas", 9f),
            BorderStyle = BorderStyle.FixedSingle,
            IntegralHeight = false
        };
        SetupBranchAutocomplete(txtBranchB, lstBranchB, new Point(672, 110));
        pnlTop.Controls.Add(txtBranchB);

        // Botao analisar
        var btnAnalyze = new Button
        {
            Text = "ANALISAR",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(40, 100, 200),
            ForeColor = Color.White,
            Size = new Size(120, 28),
            Location = new Point(962, 82),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };
        btnAnalyze.FlatAppearance.BorderColor = Color.FromArgb(60, 120, 220);
        btnAnalyze.Click += BtnAnalyze_Click;
        pnlTop.Controls.Add(btnAnalyze);

        // ── Status bar inferior ────────────────────────────────────
        pnlStatus = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 30,
            BackColor = Color.FromArgb(30, 30, 42)
        };
        lblStatus = new Label
        {
            Text = "Pronto. Selecione um reposit\u00f3rio para come\u00e7ar.",
            ForeColor = Color.FromArgb(140, 140, 160),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0)
        };
        pnlStatus.Controls.Add(lblStatus);
        Controls.Add(pnlStatus);

        // ── TabControl ─────────────────────────────────────────────
        tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9.5f),
            Padding = new Point(12, 6)
        };
        Controls.Add(tabs);

        SetupTabs();
        tabs.BringToFront();

        // Adicionar ListBoxes de autocomplete ao Form (para sobrepor outros controles)
        Controls.Add(lstBranchA);
        Controls.Add(lstBranchB);
        Controls.Add(lstBatchReceptor);
        lstBranchA.BringToFront();
        lstBranchB.BringToFront();
        lstBatchReceptor.BringToFront();

        // Auto-detectar repo atual
        TryAutoDetectRepo();

        // Restaurar branches salvos
        if (!string.IsNullOrEmpty(_settings.LastBranchA))
            txtBranchA.Text = _settings.LastBranchA;
        if (!string.IsNullOrEmpty(_settings.LastBranchB))
            txtBranchB.Text = _settings.LastBranchB;
        if (!string.IsNullOrEmpty(_settings.LastBatchReceptor))
            txtBatchReceptor.Text = _settings.LastBatchReceptor;
    }

    private void SetupBranchAutocomplete(TextBox txt, ListBox lst, Point relativePos)
    {
        lst.Size = new Size(txt.Width, 250);
        lst.Visible = false;
        lst.DrawMode = DrawMode.OwnerDrawFixed;
        lst.ItemHeight = 20;

        bool _suppressFilter = false;

        // Desenhar items com separador visual entre locais e remotos
        lst.DrawItem += (_, e) =>
        {
            if (e.Index < 0) return;
            e.DrawBackground();
            var item = lst.Items[e.Index]?.ToString() ?? "";
            var isLocal = _localBranches.Contains(item, StringComparer.OrdinalIgnoreCase);
            var isSelected = (e.State & DrawItemState.Selected) != 0;

            // Fundo
            var bgColor = isSelected ? Color.FromArgb(55, 55, 90) : Color.FromArgb(30, 30, 50);
            using var bgBrush = new SolidBrush(bgColor);
            e.Graphics.FillRectangle(bgBrush, e.Bounds);

            // Separador visual: header "LOCAIS" / "REMOTOS"
            if (item == "── LOCAIS (recentes) ──" || item == "── REMOTOS ──")
            {
                using var headerBrush = new SolidBrush(Color.FromArgb(100, 100, 140));
                using var headerFont = new Font("Segoe UI", 7.5f, FontStyle.Bold);
                e.Graphics.DrawString(item, headerFont, headerBrush, e.Bounds.X + 4, e.Bounds.Y + 3);
                // Linha separadora
                using var pen = new Pen(Color.FromArgb(60, 60, 80));
                e.Graphics.DrawLine(pen, e.Bounds.X, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
                return;
            }

            // Indicador de local
            if (isLocal)
            {
                using var dotBrush = new SolidBrush(Color.FromArgb(80, 200, 120));
                e.Graphics.FillEllipse(dotBrush, e.Bounds.X + 4, e.Bounds.Y + 6, 7, 7);
            }

            var textColor = isLocal ? Color.FromArgb(140, 230, 170) : Color.FromArgb(200, 200, 220);
            using var textBrush = new SolidBrush(textColor);
            using var font = new Font("Consolas", 9f);
            e.Graphics.DrawString(item, font, textBrush, e.Bounds.X + (isLocal ? 16 : 16), e.Bounds.Y + 2);
        };

        void ShowDropdown(string? filter = null)
        {
            List<string> items;

            if (string.IsNullOrEmpty(filter))
            {
                // Sem filtro: mostrar locais primeiro, depois remotos
                items = new List<string>();
                if (_localBranches.Count > 0)
                {
                    items.Add("── LOCAIS (recentes) ──");
                    items.AddRange(_localBranches.Take(15));
                }
                items.Add("── REMOTOS ──");
                var localSet = new HashSet<string>(_localBranches, StringComparer.OrdinalIgnoreCase);
                var remotos = _allBranches.Where(b => !localSet.Contains(b)).Take(30).ToList();
                items.AddRange(remotos);
            }
            else
            {
                // Com filtro: busca contains, locais primeiro
                var localSet = new HashSet<string>(_localBranches, StringComparer.OrdinalIgnoreCase);
                var matchLocal = _localBranches
                    .Where(b => b.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                var matchRemote = _allBranches
                    .Where(b => !localSet.Contains(b) && b.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    .Take(25)
                    .ToList();

                items = new List<string>();
                // Match exato primeiro
                var exact = _allBranches.Where(b => b.Equals(filter, StringComparison.OrdinalIgnoreCase)).ToList();
                if (exact.Count > 0 && !matchLocal.Contains(exact[0], StringComparer.OrdinalIgnoreCase))
                {
                    matchLocal.InsertRange(0, exact);
                }

                if (matchLocal.Count > 0)
                {
                    items.Add("── LOCAIS (recentes) ──");
                    items.AddRange(matchLocal.Distinct(StringComparer.OrdinalIgnoreCase));
                }
                if (matchRemote.Count > 0)
                {
                    items.Add("── REMOTOS ──");
                    items.AddRange(matchRemote);
                }
            }

            if (items.Count == 0 || (items.Count <= 2 && items.All(i => i.StartsWith("──"))))
            {
                lst.Visible = false;
                return;
            }

            lst.Items.Clear();
            foreach (var item in items)
                lst.Items.Add(item);

            var screenPos = txt.PointToScreen(new Point(0, txt.Height));
            var formPos = PointToClient(screenPos);
            lst.Location = formPos;
            var visibleItems = Math.Min(items.Count, 15);
            lst.Size = new Size(Math.Max(txt.Width, 320), visibleItems * lst.ItemHeight + 4);
            lst.Visible = true;
            lst.BringToFront();
        }

        // Ao clicar no TextBox: mostrar lista inicial (como combo)
        txt.GotFocus += (_, _) =>
        {
            if (!_suppressFilter && _allBranches.Count > 0)
                ShowDropdown(string.IsNullOrEmpty(txt.Text) ? null : txt.Text.Trim());
        };

        txt.Click += (_, _) =>
        {
            if (!lst.Visible && !_suppressFilter && _allBranches.Count > 0)
                ShowDropdown(string.IsNullOrEmpty(txt.Text) ? null : txt.Text.Trim());
        };

        txt.TextChanged += (_, _) =>
        {
            if (_suppressFilter) return;
            var filter = txt.Text.Trim();
            ShowDropdown(string.IsNullOrEmpty(filter) ? null : filter);
        };

        void SelectItem()
        {
            if (lst.SelectedIndex >= 0)
            {
                var selected = lst.SelectedItem?.ToString() ?? "";
                if (selected.StartsWith("──")) return; // Ignorar headers
                _suppressFilter = true;
                txt.Text = selected;
                txt.SelectionStart = txt.Text.Length;
                _suppressFilter = false;
                lst.Visible = false;
            }
        }

        txt.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Down)
            {
                if (!lst.Visible)
                {
                    ShowDropdown(string.IsNullOrEmpty(txt.Text) ? null : txt.Text.Trim());
                    return;
                }
                e.Handled = true;
                var next = lst.SelectedIndex + 1;
                // Pular headers
                while (next < lst.Items.Count && lst.Items[next]?.ToString()?.StartsWith("──") == true)
                    next++;
                if (next < lst.Items.Count)
                    lst.SelectedIndex = next;
                else if (lst.Items.Count > 0)
                {
                    lst.SelectedIndex = 0;
                    if (lst.Items[0]?.ToString()?.StartsWith("──") == true && lst.Items.Count > 1)
                        lst.SelectedIndex = 1;
                }
            }
            else if (e.KeyCode == Keys.Up && lst.Visible)
            {
                e.Handled = true;
                var prev = lst.SelectedIndex - 1;
                while (prev >= 0 && lst.Items[prev]?.ToString()?.StartsWith("──") == true)
                    prev--;
                if (prev >= 0)
                    lst.SelectedIndex = prev;
            }
            else if ((e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab) && lst.Visible)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                SelectItem();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                lst.Visible = false;
            }
        };

        lst.Click += (_, _) => { SelectItem(); txt.Focus(); };
        lst.DoubleClick += (_, _) => { SelectItem(); txt.Focus(); };

        txt.LostFocus += async (_, _) =>
        {
            await Task.Delay(200);
            if (!lst.Focused)
                lst.Visible = false;
        };
    }

    private void SetupTabs()
    {
        // ── Tab: Status de Merge ────────────────────────────────────
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
            Font = new Font("Segoe UI", 8f)
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
            Font = new Font("Segoe UI", 8f)
        };
        btnExportJson.FlatAppearance.BorderColor = Color.FromArgb(60, 90, 150);
        btnExportJson.Click += (_, _) => ExportGrid(dgvMergeCommits, "json");
        pnlMergeCommitsBar.Controls.Add(btnExportJson);

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
        tabMergeStatus.Controls.Add(pnlMergeCommitsBar);    // Top (barra com export)
        tabMergeStatus.Controls.Add(pnlMergeSummary);        // Top (topo)

        // ── Tab: Meu Branch (informacoes do branch atual) ──────────
        tabMyBranch = CreateTab("Meu Branch");
        var pnlMyBranchTop = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40,
            BackColor = Color.FromArgb(30, 30, 42),
            Padding = new Padding(8)
        };
        var btnRefreshMyBranch = new Button
        {
            Text = "Atualizar",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(40, 100, 200),
            ForeColor = Color.White,
            Size = new Size(110, 26),
            Location = new Point(10, 7),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        };
        btnRefreshMyBranch.FlatAppearance.BorderColor = Color.FromArgb(60, 120, 220);
        btnRefreshMyBranch.Click += (_, _) => LoadMyBranchInfo();
        pnlMyBranchTop.Controls.Add(btnRefreshMyBranch);

        _rtbMyBranch = CreateRichTextBox();

        // Grid com commits recentes do branch atual
        _dgvMyCommits = CreateDataGrid();
        _dgvMyCommits.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Hash", HeaderText = "Hash", Width = 90, DataPropertyName = "Hash" },
            new DataGridViewTextBoxColumn { Name = "Author", HeaderText = "Autor", Width = 180, DataPropertyName = "Author" },
            new DataGridViewTextBoxColumn { Name = "RelativeDate", HeaderText = "Quando", Width = 120, DataPropertyName = "RelativeDate" },
            new DataGridViewTextBoxColumn { Name = "Message", HeaderText = "Mensagem", Width = 550, DataPropertyName = "Message" }
        );

        // Grid com arquivos modificados localmente
        _dgvLocalChanges = CreateDataGrid();
        _dgvLocalChanges.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 150, DataPropertyName = "Status" },
            new DataGridViewTextBoxColumn { Name = "FilePath", HeaderText = "Arquivo", Width = 700, DataPropertyName = "FilePath" }
        );

        // SplitContainer: info+local changes em cima, commits embaixo
        var splitMyBranch = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 250,
            SplitterWidth = 4,
            BackColor = Color.FromArgb(50, 50, 70)
        };

        // Painel superior: info resumo + alteracoes locais
        var splitTop = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 400,
            SplitterWidth = 4,
            BackColor = Color.FromArgb(50, 50, 70)
        };
        splitTop.Panel1.Controls.Add(_rtbMyBranch);
        splitTop.Panel1.BackColor = Color.FromArgb(24, 24, 32);

        var lblLocalChanges = new Label
        {
            Text = "Alteracoes locais (nao commitadas):",
            ForeColor = Color.FromArgb(255, 180, 60),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 25,
            Padding = new Padding(5, 4, 0, 0),
            BackColor = Color.FromArgb(28, 28, 38)
        };
        splitTop.Panel2.Controls.Add(_dgvLocalChanges);
        splitTop.Panel2.Controls.Add(lblLocalChanges);
        splitTop.Panel2.BackColor = Color.FromArgb(24, 24, 32);

        splitMyBranch.Panel1.Controls.Add(splitTop);
        splitMyBranch.Panel1.BackColor = Color.FromArgb(24, 24, 32);

        // Painel inferior: commits recentes
        var lblRecentCommits = new Label
        {
            Text = "Ultimos commits no branch atual:",
            ForeColor = Color.FromArgb(120, 180, 255),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 25,
            Padding = new Padding(5, 4, 0, 0),
            BackColor = Color.FromArgb(28, 28, 38)
        };
        splitMyBranch.Panel2.Controls.Add(_dgvMyCommits);
        splitMyBranch.Panel2.Controls.Add(lblRecentCommits);
        splitMyBranch.Panel2.BackColor = Color.FromArgb(24, 24, 32);

        tabMyBranch.Controls.Add(splitMyBranch);
        tabMyBranch.Controls.Add(pnlMyBranchTop);
        pnlMyBranchTop.BringToFront();

        // ── Tab: Verificacao em Lote ───────────────────────────────
        tabBatch = CreateTab("Lote (Multi-Branch)");
        var pnlBatchTop = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(30, 30, 42), Padding = new Padding(8) };

        var lblBatchReceptor = new Label
        {
            Text = "Receptor (A):",
            ForeColor = Color.FromArgb(100, 220, 100),
            AutoSize = true,
            Location = new Point(10, 16),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };
        pnlBatchTop.Controls.Add(lblBatchReceptor);

        txtBatchReceptor = new TextBox
        {
            Location = new Point(120, 12),
            Size = new Size(280, 28),
            BackColor = Color.FromArgb(40, 40, 55),
            ForeColor = Color.White,
            Font = new Font("Consolas", 9.5f),
            BorderStyle = BorderStyle.FixedSingle
        };
        lstBatchReceptor = new ListBox
        {
            Visible = false,
            BackColor = Color.FromArgb(30, 30, 50),
            ForeColor = Color.White,
            Font = new Font("Consolas", 9f),
            BorderStyle = BorderStyle.FixedSingle,
            IntegralHeight = false
        };
        SetupBranchAutocomplete(txtBatchReceptor, lstBatchReceptor, new Point(120, 40));
        pnlBatchTop.Controls.Add(txtBatchReceptor);

        btnBatchAnalyze = new Button
        {
            Text = "ANALISAR SELECIONADOS",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(40, 100, 200),
            ForeColor = Color.White,
            Size = new Size(200, 28),
            Location = new Point(420, 12),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };
        btnBatchAnalyze.FlatAppearance.BorderColor = Color.FromArgb(60, 120, 220);
        btnBatchAnalyze.Click += BtnBatchAnalyze_Click;
        pnlBatchTop.Controls.Add(btnBatchAnalyze);

        btnBatchExport = new Button
        {
            Text = "Exportar TXT",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(50, 50, 70),
            ForeColor = Color.White,
            Size = new Size(110, 28),
            Location = new Point(630, 12),
            Cursor = Cursors.Hand
        };
        btnBatchExport.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 100);
        btnBatchExport.Click += BtnBatchExport_Click;
        pnlBatchTop.Controls.Add(btnBatchExport);

        var btnBatchExportCsv = new Button
        {
            Text = "CSV",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(40, 90, 60),
            ForeColor = Color.White,
            Size = new Size(55, 28),
            Location = new Point(745, 12),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 8.5f)
        };
        btnBatchExportCsv.FlatAppearance.BorderColor = Color.FromArgb(60, 120, 80);
        btnBatchExportCsv.Click += (_, _) => ExportGrid(dgvBatchResults, "csv");
        pnlBatchTop.Controls.Add(btnBatchExportCsv);

        var btnBatchExportJson = new Button
        {
            Text = "JSON",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(40, 70, 120),
            ForeColor = Color.White,
            Size = new Size(60, 28),
            Location = new Point(805, 12),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 8.5f)
        };
        btnBatchExportJson.FlatAppearance.BorderColor = Color.FromArgb(60, 90, 150);
        btnBatchExportJson.Click += (_, _) => ExportGrid(dgvBatchResults, "json");
        pnlBatchTop.Controls.Add(btnBatchExportJson);

        pgBatch = new ProgressBar
        {
            Location = new Point(875, 16),
            Size = new Size(180, 20),
            Visible = false,
            Style = ProgressBarStyle.Continuous
        };
        pnlBatchTop.Controls.Add(pgBatch);

        tabBatch.Controls.Add(pnlBatchTop);
        pnlBatchTop.BringToFront();

        // Painel esquerdo: filtros + lista de branches
        var pnlBatchLeft = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(28, 28, 38),
            Padding = new Padding(8)
        };

        // ── Secao de Filtros Avancados ──
        var pnlFilters = new Panel
        {
            Dock = DockStyle.Top,
            Height = 230,
            BackColor = Color.FromArgb(32, 32, 45),
            Padding = new Padding(6)
        };

        var lblFiltersTitle = new Label
        {
            Text = "FILTROS",
            ForeColor = Color.FromArgb(100, 180, 255),
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            Location = new Point(6, 4),
            AutoSize = true
        };
        pnlFilters.Controls.Add(lblFiltersTitle);

        // Filtro por nome
        var lblFilterName = new Label
        {
            Text = "Nome:",
            ForeColor = Color.FromArgb(180, 180, 190),
            Font = new Font("Segoe UI", 8.5f),
            Location = new Point(6, 28),
            AutoSize = true
        };
        pnlFilters.Controls.Add(lblFilterName);

        txtBatchFilter = new TextBox
        {
            Location = new Point(6, 46),
            Size = new Size(306, 26),
            BackColor = Color.FromArgb(40, 40, 55),
            ForeColor = Color.White,
            Font = new Font("Consolas", 9.5f),
            PlaceholderText = "Buscar por nome..."
        };
        txtBatchFilter.TextChanged += (_, _) => ApplyBatchFilters();
        pnlFilters.Controls.Add(txtBatchFilter);

        // Filtro por autor
        var lblFilterAuthor = new Label
        {
            Text = "Autor:",
            ForeColor = Color.FromArgb(180, 180, 190),
            Font = new Font("Segoe UI", 8.5f),
            Location = new Point(6, 76),
            AutoSize = true
        };
        pnlFilters.Controls.Add(lblFilterAuthor);

        cmbBatchFilterAuthor = new ComboBox
        {
            Location = new Point(6, 94),
            Size = new Size(306, 26),
            BackColor = Color.FromArgb(40, 40, 55),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Consolas", 9f),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbBatchFilterAuthor.Items.Add("(Todos os autores)");
        cmbBatchFilterAuthor.SelectedIndex = 0;
        cmbBatchFilterAuthor.SelectedIndexChanged += (_, _) => ApplyBatchFilters();
        pnlFilters.Controls.Add(cmbBatchFilterAuthor);

        // Filtro por tipo de branch (prefixo)
        var lblFilterPrefix = new Label
        {
            Text = "Tipo:",
            ForeColor = Color.FromArgb(180, 180, 190),
            Font = new Font("Segoe UI", 8.5f),
            Location = new Point(6, 122),
            AutoSize = true
        };
        pnlFilters.Controls.Add(lblFilterPrefix);

        cmbBatchFilterPrefix = new ComboBox
        {
            Location = new Point(56, 120),
            Size = new Size(120, 26),
            BackColor = Color.FromArgb(40, 40, 55),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Consolas", 9f),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbBatchFilterPrefix.Items.Add("(Todos)");
        cmbBatchFilterPrefix.SelectedIndex = 0;
        cmbBatchFilterPrefix.SelectedIndexChanged += (_, _) => ApplyBatchFilters();
        pnlFilters.Controls.Add(cmbBatchFilterPrefix);

        // Filtro por periodo
        var lblFilterDays = new Label
        {
            Text = "Periodo:",
            ForeColor = Color.FromArgb(180, 180, 190),
            Font = new Font("Segoe UI", 8.5f),
            Location = new Point(184, 122),
            AutoSize = true
        };
        pnlFilters.Controls.Add(lblFilterDays);

        cmbBatchFilterDays = new ComboBox
        {
            Location = new Point(244, 120),
            Size = new Size(68, 26),
            BackColor = Color.FromArgb(40, 40, 55),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Consolas", 9f),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbBatchFilterDays.Items.AddRange(new object[] { "Todos", "7d", "15d", "30d", "60d", "90d" });
        cmbBatchFilterDays.SelectedIndex = 0;
        cmbBatchFilterDays.SelectedIndexChanged += (_, _) => ApplyBatchFilters();
        pnlFilters.Controls.Add(cmbBatchFilterDays);

        // Botoes aplicar/limpar filtros
        btnBatchApplyFilters = new Button
        {
            Text = "Aplicar Filtros",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(40, 100, 180),
            ForeColor = Color.White,
            Size = new Size(148, 28),
            Location = new Point(6, 152),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
        };
        btnBatchApplyFilters.FlatAppearance.BorderColor = Color.FromArgb(60, 120, 200);
        btnBatchApplyFilters.Click += (_, _) => ApplyBatchFilters();
        pnlFilters.Controls.Add(btnBatchApplyFilters);

        btnBatchClearFilters = new Button
        {
            Text = "Limpar",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(70, 50, 50),
            ForeColor = Color.White,
            Size = new Size(80, 28),
            Location = new Point(160, 152),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 8.5f)
        };
        btnBatchClearFilters.FlatAppearance.BorderColor = Color.FromArgb(120, 60, 60);
        btnBatchClearFilters.Click += BtnBatchClearFilters_Click;
        pnlFilters.Controls.Add(btnBatchClearFilters);

        // Label com resultado do filtro
        var lblFilterResult = new Label
        {
            Name = "lblFilterResult",
            Text = "",
            ForeColor = Color.FromArgb(160, 160, 180),
            Font = new Font("Segoe UI", 8f, FontStyle.Italic),
            Location = new Point(6, 185),
            AutoSize = true
        };
        pnlFilters.Controls.Add(lblFilterResult);

        // Adicionar ao pnlBatchLeft na ordem correta para WinForms docking:
        // Fill primeiro, depois Bottom, depois Top (ultimo adicionado com Top fica mais acima)

        // ── Titulo da lista ──
        var lblBatchListTitle = new Label
        {
            Text = "Branches de feature (B):",
            ForeColor = Color.FromArgb(255, 180, 80),
            Dock = DockStyle.Top,
            Height = 22,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        };

        // Botoes selecionar/desmarcar
        var pnlBatchButtons = new Panel { Dock = DockStyle.Top, Height = 30 };
        btnBatchSelectAll = new Button
        {
            Text = "Selec. Todos",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(50, 50, 70),
            ForeColor = Color.White,
            Size = new Size(100, 24),
            Location = new Point(0, 2),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 8f)
        };
        btnBatchSelectAll.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 100);
        btnBatchSelectAll.Click += (_, _) => { for (int i = 0; i < clbBatchBranches.Items.Count; i++) clbBatchBranches.SetItemChecked(i, true); UpdateBatchCount(); };
        pnlBatchButtons.Controls.Add(btnBatchSelectAll);

        btnBatchDeselectAll = new Button
        {
            Text = "Desmarcar",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(50, 50, 70),
            ForeColor = Color.White,
            Size = new Size(90, 24),
            Location = new Point(105, 2),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 8f)
        };
        btnBatchDeselectAll.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 100);
        btnBatchDeselectAll.Click += (_, _) => { for (int i = 0; i < clbBatchBranches.Items.Count; i++) clbBatchBranches.SetItemChecked(i, false); UpdateBatchCount(); };
        pnlBatchButtons.Controls.Add(btnBatchDeselectAll);

        var btnBatchInvertSel = new Button
        {
            Text = "Inverter",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(50, 50, 70),
            ForeColor = Color.White,
            Size = new Size(80, 24),
            Location = new Point(200, 2),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 8f)
        };
        btnBatchInvertSel.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 100);
        btnBatchInvertSel.Click += (_, _) => { for (int i = 0; i < clbBatchBranches.Items.Count; i++) clbBatchBranches.SetItemChecked(i, !clbBatchBranches.GetItemChecked(i)); UpdateBatchCount(); };
        pnlBatchButtons.Controls.Add(btnBatchInvertSel);

        // CheckedListBox
        clbBatchBranches = new CheckedListBox
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(28, 28, 38),
            ForeColor = Color.FromArgb(220, 220, 230),
            Font = new Font("Consolas", 9f),
            BorderStyle = BorderStyle.None,
            CheckOnClick = true
        };
        clbBatchBranches.ItemCheck += (_, _) => BeginInvoke(new Action(UpdateBatchCount));

        lblBatchCount = new Label
        {
            Text = "0 selecionado(s) | 0 listado(s)",
            ForeColor = Color.FromArgb(120, 180, 255),
            Dock = DockStyle.Bottom,
            Height = 25,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };

        // Ordem de adicao ao pnlBatchLeft (WinForms docking):
        // 1) Fill primeiro (checkedlistbox preenche o meio)
        pnlBatchLeft.Controls.Add(clbBatchBranches);
        // 2) Bottom (contador)
        pnlBatchLeft.Controls.Add(lblBatchCount);
        // 3) Top na ordem inversa (ultimo adicionado = mais acima)
        pnlBatchLeft.Controls.Add(pnlBatchButtons);
        pnlBatchLeft.Controls.Add(lblBatchListTitle);
        pnlBatchLeft.Controls.Add(pnlFilters);

        // Grid de resultados (direita)
        dgvBatchResults = CreateDataGrid();
        dgvBatchResults.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "BranchFeature", HeaderText = "Branch Feature (B)", Width = 280, DataPropertyName = "BranchFeature" },
            new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 130, DataPropertyName = "Status" },
            new DataGridViewTextBoxColumn { Name = "CommitsPendentes", HeaderText = "Commits Pend.", Width = 110, DataPropertyName = "CommitsPendentes" },
            new DataGridViewTextBoxColumn { Name = "ConflitosArquivos", HeaderText = "Conflitos Pot.", Width = 110, DataPropertyName = "ConflitosArquivos" },
            new DataGridViewTextBoxColumn { Name = "ArquivosAlterados", HeaderText = "Arq. Alterados", Width = 110, DataPropertyName = "ArquivosAlterados" },
            new DataGridViewTextBoxColumn { Name = "UltimoAutor", HeaderText = "Ultimo Autor", Width = 160, DataPropertyName = "UltimoAutor" },
            new DataGridViewTextBoxColumn { Name = "UltimoCommit", HeaderText = "Ultimo Commit", Width = 250, DataPropertyName = "UltimoCommit" }
        );

        // SplitContainer para dividir esquerda (lista) e direita (grid) corretamente
        splitBatch = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 350,
            SplitterWidth = 4,
            BackColor = Color.FromArgb(50, 50, 70),
            FixedPanel = FixedPanel.Panel1,
            Panel1MinSize = 320
        };
        splitBatch.Panel1.BackColor = Color.FromArgb(28, 28, 38);
        splitBatch.Panel2.BackColor = Color.FromArgb(24, 24, 32);

        // Mover controles do pnlBatchLeft para Panel1 do split
        foreach (Control ctrl in pnlBatchLeft.Controls.Cast<Control>().ToArray())
        {
            pnlBatchLeft.Controls.Remove(ctrl);
            splitBatch.Panel1.Controls.Add(ctrl);
        }

        // Grid no Panel2
        splitBatch.Panel2.Controls.Add(dgvBatchResults);

        // Ordem IMPORTA no WinForms: adicionar Fill primeiro, depois Top
        // O ultimo adicionado com Dock=Top reserva espaco antes do Fill
        tabBatch.Controls.Add(splitBatch);   // Fill - adicionado primeiro
        tabBatch.Controls.Add(pnlBatchTop);  // Top  - adicionado depois, reserva espaco acima

        // Mover aba Lote para segunda posicao (indice 1)
        tabs.TabPages.Remove(tabBatch);
        tabs.TabPages.Insert(1, tabBatch);

        // Forçar SplitterDistance quando a tab Lote é selecionada
        // (WinForms reseta SplitterDistance em tabs ocultas)
        tabs.SelectedIndexChanged += (_, _) =>
        {
            if (tabs.SelectedTab == tabBatch && splitBatch != null)
            {
                BeginInvoke(() =>
                {
                    try { splitBatch.SplitterDistance = 350; } catch { }
                });
            }
            else if (tabs.SelectedTab == tabMyBranch)
            {
                LoadMyBranchInfo();
            }
        };
    }

    // ══════════════════════════════════════════════════════════════════
    //  HELPER CONTROLES
    // ══════════════════════════════════════════════════════════════════

    private TabPage CreateTab(string text)
    {
        var tp = new TabPage(text) { BackColor = Color.FromArgb(24, 24, 32), Padding = new Padding(4) };
        tabs.TabPages.Add(tp);
        return tp;
    }

    private DataGridView CreateDataGrid()
    {
        var dgv = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.FromArgb(24, 24, 32),
            ForeColor = Color.White,
            GridColor = Color.FromArgb(50, 50, 70),
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            RowHeadersVisible = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(24, 24, 32),
                ForeColor = Color.FromArgb(220, 220, 230),
                SelectionBackColor = Color.FromArgb(50, 80, 140),
                SelectionForeColor = Color.White,
                Font = new Font("Consolas", 9.5f),
                Padding = new Padding(4, 2, 4, 2)
            },
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(35, 35, 50),
                ForeColor = Color.FromArgb(120, 180, 255),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Padding = new Padding(4)
            },
            EnableHeadersVisualStyles = false,
            AutoGenerateColumns = false
        };
        dgv.ColumnHeadersHeight = 35;
        dgv.RowTemplate.Height = 28;
        return dgv;
    }

    private RichTextBox CreateRichTextBox()
    {
        return new RichTextBox
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(24, 24, 32),
            ForeColor = Color.FromArgb(220, 220, 230),
            Font = new Font("Consolas", 10),
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            WordWrap = false
        };
    }

    private Label CreateInfoLabel(string text, int x, int y, float fontSize = 11, bool bold = false)
    {
        return new Label
        {
            Text = text,
            ForeColor = Color.FromArgb(220, 220, 230),
            Font = new Font("Segoe UI", fontSize, bold ? FontStyle.Bold : FontStyle.Regular),
            AutoSize = true,
            Location = new Point(x, y)
        };
    }

    // ══════════════════════════════════════════════════════════════════
    //  SETTINGS
    // ══════════════════════════════════════════════════════════════════

    private void RestoreWindowState()
    {
        if (_settings.WindowWidth > 0 && _settings.WindowHeight > 0)
        {
            var bounds = new Rectangle(
                _settings.WindowX, _settings.WindowY,
                _settings.WindowWidth, _settings.WindowHeight);

            // Garantir que a janela está visível em algum monitor
            if (Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(bounds)))
            {
                StartPosition = FormStartPosition.Manual;
                Location = new Point(_settings.WindowX, _settings.WindowY);
                Size = new Size(_settings.WindowWidth, _settings.WindowHeight);
            }
        }

        if (_settings.WindowMaximized)
            WindowState = FormWindowState.Maximized;

        if (_settings.LastSelectedTab >= 0 && _settings.LastSelectedTab < tabs.TabCount)
            tabs.SelectedIndex = _settings.LastSelectedTab;
    }

    private void SaveSettings()
    {
        if (WindowState == FormWindowState.Maximized)
        {
            _settings.WindowMaximized = true;
            // Manter as coordenadas do estado normal (antes de maximizar)
            _settings.WindowWidth = RestoreBounds.Width;
            _settings.WindowHeight = RestoreBounds.Height;
            _settings.WindowX = RestoreBounds.X;
            _settings.WindowY = RestoreBounds.Y;
        }
        else
        {
            _settings.WindowMaximized = false;
            _settings.WindowWidth = Width;
            _settings.WindowHeight = Height;
            _settings.WindowX = Left;
            _settings.WindowY = Top;
        }

        _settings.LastSelectedTab = tabs.SelectedIndex;

        // Salvar branches usados por último
        if (!string.IsNullOrEmpty(txtBranchA.Text))
            _settings.LastBranchA = txtBranchA.Text;
        if (!string.IsNullOrEmpty(txtBranchB.Text))
            _settings.LastBranchB = txtBranchB.Text;
        if (!string.IsNullOrEmpty(txtBatchReceptor.Text))
            _settings.LastBatchReceptor = txtBatchReceptor.Text;

        _settings.Save();
    }

    // ══════════════════════════════════════════════════════════════════
    //  EVENTOS
    // ══════════════════════════════════════════════════════════════════

    private void TryAutoDetectRepo()
    {
        // 1) Restaurar último repositório salvo nas settings
        if (!string.IsNullOrEmpty(_settings.LastRepoPath)
            && Directory.Exists(Path.Combine(_settings.LastRepoPath, ".git")))
        {
            SetRepo(_settings.LastRepoPath);
            return;
        }

        // 2) Subir a árvore de diretórios a partir do exe
        var currentDir = AppDomain.CurrentDomain.BaseDirectory;
        var dir = new DirectoryInfo(currentDir);
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
            {
                SetRepo(dir.FullName);
                return;
            }
            dir = dir.Parent;
        }
    }

    private void SetRepo(string path)
    {
        _git.RepoPath = path;
        lblRepo.Text = path;
        UpdateCurrentBranch();
        SetStatus("Carregando branches...");
        LoadBranches();
        SetStatus($"Repositorio configurado: {path}");

        _settings.AddRecentRepo(path);
        _settings.Save();

        // Carregar info do branch atual se a aba Meu Branch estiver visivel
        if (tabs.SelectedTab == tabMyBranch)
            LoadMyBranchInfo();
    }

    private void UpdateCurrentBranch()
    {
        try
        {
            var branch = _git.GetCurrentBranch();
            lblCurrentBranch.Text = branch;
        }
        catch
        {
            lblCurrentBranch.Text = "(desconhecido)";
        }
    }

    private List<string> _allBranches = new();
    private List<string> _localBranches = new();
    private List<string> _prioritizedBranches = new();
    private List<string> _batchBranches = new();

    private void LoadBranches()
    {
        _allBranches = _git.GetAllBranches();
        _localBranches = _git.GetLocalBranches();
        _prioritizedBranches = _git.GetBranchesPrioritized();
        _allBranchesMetadata = _git.GetBranchesMetadata();

        // Lista unica para o lote: prefere origin/, remove duplicatas
        var batchSet = new HashSet<string>();
        _batchBranches = new List<string>();
        foreach (var b in _allBranches)
        {
            var shortName = b.StartsWith("origin/") ? b["origin/".Length..] : b;
            if (batchSet.Add(shortName))
                _batchBranches.Add(shortName);
        }
        _batchBranches.Sort();

        clbBatchBranches.Items.Clear();
        foreach (var b in _batchBranches)
        {
            clbBatchBranches.Items.Add(b);
        }

        // Popular filtros de autor e prefixo
        var currentAuthor = cmbBatchFilterAuthor.SelectedItem?.ToString();
        var currentPrefix = cmbBatchFilterPrefix.SelectedItem?.ToString();

        cmbBatchFilterAuthor.Items.Clear();
        cmbBatchFilterAuthor.Items.Add("(Todos os autores)");
        var authors = _allBranchesMetadata.Select(b => b.Author).Where(a => !string.IsNullOrEmpty(a)).Distinct().OrderBy(a => a);
        foreach (var a in authors)
            cmbBatchFilterAuthor.Items.Add(a);
        cmbBatchFilterAuthor.SelectedIndex = currentAuthor != null && cmbBatchFilterAuthor.Items.Contains(currentAuthor)
            ? cmbBatchFilterAuthor.Items.IndexOf(currentAuthor) : 0;

        cmbBatchFilterPrefix.Items.Clear();
        cmbBatchFilterPrefix.Items.Add("(Todos)");
        var prefixes = _allBranchesMetadata.Select(b => b.Prefix).Where(p => !string.IsNullOrEmpty(p)).Distinct().OrderBy(p => p);
        foreach (var p in prefixes)
            cmbBatchFilterPrefix.Items.Add(p);
        cmbBatchFilterPrefix.SelectedIndex = currentPrefix != null && cmbBatchFilterPrefix.Items.Contains(currentPrefix)
            ? cmbBatchFilterPrefix.Items.IndexOf(currentPrefix) : 0;

        UpdateBatchCount();
    }

    private void SetStatus(string text)
    {
        lblStatus.Text = text;
        lblStatus.Refresh();
    }

    private void RestoreDefaultCursor()
    {
        UseWaitCursor = false;
        Cursor = Cursors.Default;
        // Forçar reset em todos os DataGridViews (bug do WinForms)
        foreach (var dgv in new[] { dgvMergeCommits, _dgvMyCommits, _dgvLocalChanges, dgvBatchResults })
        {
            if (dgv != null)
            {
                dgv.Cursor = Cursors.Default;
            }
        }
        Application.DoEvents();
    }

    private void BtnSetRepo_Click(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description = "Selecione a pasta do reposit\u00f3rio Git",
            UseDescriptionForTitle = true
        };
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            if (Directory.Exists(Path.Combine(dlg.SelectedPath, ".git")))
                SetRepo(dlg.SelectedPath);
            else
                MessageBox.Show("A pasta selecionada n\u00e3o \u00e9 um reposit\u00f3rio Git.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private System.Windows.Forms.Timer? _fetchAnimTimer;
    private int _fetchAnimDots = 0;
    private bool _isFetching = false;

    private void StartFetchAnimation(string label)
    {
        _isFetching = true;
        btnFetch.Enabled = false;
        btnFetch.BackColor = Color.FromArgb(60, 60, 30);
        UseWaitCursor = true; Application.DoEvents();

        _fetchAnimDots = 0;
        _fetchAnimTimer = new System.Windows.Forms.Timer { Interval = 350 };
        _fetchAnimTimer.Tick += (_, _) =>
        {
            _fetchAnimDots = (_fetchAnimDots + 1) % 4;
            var dots = new string('.', _fetchAnimDots + 1);
            btnFetch.Text = $"↓ {label}{dots}";
            SetStatus($"{label}{dots}");
        };
        _fetchAnimTimer.Start();
    }

    private void StopFetchAnimation(double seconds)
    {
        _fetchAnimTimer?.Stop();
        _fetchAnimTimer?.Dispose();
        _fetchAnimTimer = null;
        _isFetching = false;

        LoadBranches();
        UpdateCurrentBranch();
        SetStatus($"Fetch concluido em {seconds:F1}s  |  {_allBranches.Count} branches carregados");
        RestoreDefaultCursor();

        btnFetch.Enabled = true;
        btnFetch.Text = "↓ Fetch Origin";
        btnFetch.BackColor = Color.FromArgb(50, 50, 70);
    }

    private void BtnFetch_Click(object? sender, EventArgs e)
    {
        if (_isFetching) return;
        StartFetchAnimation("Fetch Origin");

        Task.Run(() =>
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            _git.FetchOrigin();
            sw.Stop();
            Invoke(() => StopFetchAnimation(sw.Elapsed.TotalSeconds));
        });
    }

    private void BtnFetchFull_Click()
    {
        if (_isFetching) return;
        StartFetchAnimation("Fetch + Prune");

        Task.Run(() =>
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            _git.FetchPrune();
            sw.Stop();
            Invoke(() => StopFetchAnimation(sw.Elapsed.TotalSeconds));
        });
    }

    private (string? a, string? b) ResolveBranches()
    {
        if (string.IsNullOrWhiteSpace(txtBranchA.Text) || string.IsNullOrWhiteSpace(txtBranchB.Text))
        {
            MessageBox.Show("Selecione os branches A e B.", "Aten\u00e7\u00e3o", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return (null, null);
        }
        var a = _git.ResolveBranch(txtBranchA.Text);
        var b = _git.ResolveBranch(txtBranchB.Text);
        if (a == null) { MessageBox.Show($"Branch '{txtBranchA.Text}' n\u00e3o encontrado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); return (null, null); }
        if (b == null) { MessageBox.Show($"Branch '{txtBranchB.Text}' n\u00e3o encontrado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); return (null, null); }
        return (a, b);
    }

    private void BtnAnalyze_Click(object? sender, EventArgs e)
    {
        var (a, b) = ResolveBranches();
        if (a == null || b == null) return;

        SetStatus("Analisando branches...");
        UseWaitCursor = true; Application.DoEvents();

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
            dgvMergeCommits.DataSource = null;
            dgvMergeCommits.DataSource = commits;
        }
    }

    private void AppendRtb(RichTextBox rtb, string text, Color color, bool bold = false)
    {
        rtb.SelectionStart = rtb.TextLength;
        rtb.SelectionLength = 0;
        rtb.SelectionColor = color;
        if (bold) rtb.SelectionFont = new Font(rtb.Font, FontStyle.Bold);
        rtb.AppendText(text);
        if (bold) rtb.SelectionFont = rtb.Font;
    }

    // ══════════════════════════════════════════════════════════════════
    //  MEU BRANCH
    // ══════════════════════════════════════════════════════════════════

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
                    // Atualizar RichTextBox com info resumida
                    _rtbMyBranch.Clear();

                    AppendRtb(_rtbMyBranch, "\n  BRANCH ATUAL\n", Color.FromArgb(120, 180, 255), bold: true);
                    AppendRtb(_rtbMyBranch, $"  Nome: ", Color.FromArgb(140, 140, 160));
                    AppendRtb(_rtbMyBranch, $"{branch}\n\n", Color.FromArgb(80, 220, 120));

                    // Status vs remote
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

                    // Alteracoes locais
                    AppendRtb(_rtbMyBranch, $"\n  ALTERACOES LOCAIS\n", Color.FromArgb(120, 180, 255), bold: true);
                    if (localChanges.Count == 0)
                        AppendRtb(_rtbMyBranch, "  Nenhuma alteracao local pendente\n", Color.FromArgb(80, 220, 80));
                    else
                        AppendRtb(_rtbMyBranch, $"  {localChanges.Count} arquivo(s) modificado(s)\n", Color.FromArgb(255, 180, 60));

                    // Stashes
                    AppendRtb(_rtbMyBranch, $"\n  STASHES\n", Color.FromArgb(120, 180, 255), bold: true);
                    if (stashes.Count == 0)
                        AppendRtb(_rtbMyBranch, "  Nenhum stash salvo\n", Color.FromArgb(140, 140, 160));
                    else
                    {
                        AppendRtb(_rtbMyBranch, $"  {stashes.Count} stash(es):\n", Color.FromArgb(255, 200, 80));
                        foreach (var s in stashes.Take(5))
                            AppendRtb(_rtbMyBranch, $"    {s}\n", Color.FromArgb(180, 180, 200));
                    }

                    // Branches locais
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

                    // Grids
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

    private static Icon CreateAppIcon()
    {
        var bmp = new Bitmap(32, 32);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);

        // Fundo circular azul escuro
        using var bgBrush = new SolidBrush(Color.FromArgb(30, 80, 180));
        g.FillEllipse(bgBrush, 1, 1, 30, 30);

        // Desenhar simbolo de branch (bifurcação)
        using var pen = new Pen(Color.White, 2.2f) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round };

        // Linha principal vertical
        g.DrawLine(pen, 10, 8, 10, 24);
        // Linha diagonal (branch)
        g.DrawLine(pen, 10, 14, 22, 8);

        // Bolinhas nos nós
        using var nodeBrush = new SolidBrush(Color.FromArgb(80, 220, 120));
        g.FillEllipse(nodeBrush, 7, 5, 6, 6);    // topo principal
        g.FillEllipse(nodeBrush, 7, 21, 6, 6);    // base principal
        using var nodeBrush2 = new SolidBrush(Color.FromArgb(255, 180, 60));
        g.FillEllipse(nodeBrush2, 19, 5, 6, 6);   // branch

        var handle = bmp.GetHicon();
        return Icon.FromHandle(handle);
    }

    private static string EscapeJson(string s) =>
        s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");


    // ══════════════════════════════════════════════════════════════════
    //  VERIFICACAO EM LOTE
    // ══════════════════════════════════════════════════════════════════

    private void UpdateBatchCount()
    {
        var selected = clbBatchBranches.CheckedItems.Count;
        var total = clbBatchBranches.Items.Count;
        lblBatchCount.Text = $"{selected} selecionado(s) | {total} listado(s)";
    }

    private void ApplyBatchFilters()
    {
        var nameFilter = txtBatchFilter.Text.Trim().ToLowerInvariant();
        var authorFilter = cmbBatchFilterAuthor.SelectedIndex > 0 ? cmbBatchFilterAuthor.SelectedItem?.ToString() : null;
        var prefixFilter = cmbBatchFilterPrefix.SelectedIndex > 0 ? cmbBatchFilterPrefix.SelectedItem?.ToString() : null;
        var daysFilter = cmbBatchFilterDays.SelectedItem?.ToString();

        int? maxDays = daysFilter switch
        {
            "7d" => 7,
            "15d" => 15,
            "30d" => 30,
            "60d" => 60,
            "90d" => 90,
            _ => null
        };
        var cutoffDate = maxDays.HasValue ? DateTime.Now.AddDays(-maxDays.Value) : (DateTime?)null;

        // Construir set de branches que passam no filtro de metadados
        var metadataFiltered = new HashSet<string>();
        foreach (var bm in _allBranchesMetadata)
        {
            if (authorFilter != null && !bm.Author.Equals(authorFilter, StringComparison.OrdinalIgnoreCase))
                continue;
            if (prefixFilter != null && !bm.Prefix.Equals(prefixFilter, StringComparison.OrdinalIgnoreCase))
                continue;
            if (cutoffDate.HasValue && bm.Date < cutoffDate.Value)
                continue;
            metadataFiltered.Add(bm.ShortName);
            metadataFiltered.Add(bm.FullName);
            // Adicionar variantes com origin/
            if (!bm.FullName.StartsWith("origin/"))
                metadataFiltered.Add($"origin/{bm.FullName}");
        }

        clbBatchBranches.Items.Clear();
        int count = 0;
        foreach (var b in _batchBranches)
        {
            // Filtro por nome (texto)
            if (!string.IsNullOrEmpty(nameFilter) && !b.ToLowerInvariant().Contains(nameFilter))
                continue;

            // Filtro por metadados (autor, prefixo, periodo)
            if ((authorFilter != null || prefixFilter != null || cutoffDate.HasValue)
                && !metadataFiltered.Contains(b))
                continue;

            clbBatchBranches.Items.Add(b);
            count++;
        }

        // Atualizar label de resultado do filtro
        var filterPanel = tabBatch.Controls.OfType<SplitContainer>().FirstOrDefault()?.Panel1;
        var lblResult = filterPanel?.Controls.OfType<Panel>()
            .SelectMany(p => p.Controls.OfType<Label>())
            .FirstOrDefault(l => l.Name == "lblFilterResult");
        if (lblResult != null)
        {
            var filters = new List<string>();
            if (!string.IsNullOrEmpty(nameFilter)) filters.Add($"nome: '{nameFilter}'");
            if (authorFilter != null) filters.Add($"autor: {authorFilter}");
            if (prefixFilter != null) filters.Add($"tipo: {prefixFilter}");
            if (daysFilter != null && daysFilter != "Todos") filters.Add($"periodo: {daysFilter}");

            lblResult.Text = filters.Count > 0
                ? $"Filtros: {string.Join(", ", filters)} ({count} resultados)"
                : "";
        }

        UpdateBatchCount();
    }

    private void BtnBatchClearFilters_Click(object? sender, EventArgs e)
    {
        txtBatchFilter.Text = "";
        cmbBatchFilterAuthor.SelectedIndex = 0;
        cmbBatchFilterPrefix.SelectedIndex = 0;
        cmbBatchFilterDays.SelectedIndex = 0;
        ApplyBatchFilters();
    }

    private void BtnBatchAnalyze_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtBatchReceptor.Text))
        {
            MessageBox.Show("Selecione o branch Receptor (A).", "Atencao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var receptor = _git.ResolveBranch(txtBatchReceptor.Text);
        if (receptor == null)
        {
            MessageBox.Show($"Branch '{txtBatchReceptor.Text}' nao encontrado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var selected = new List<string>();
        foreach (var item in clbBatchBranches.CheckedItems)
            selected.Add(item.ToString()!);

        if (selected.Count == 0)
        {
            MessageBox.Show("Selecione pelo menos um branch de feature na lista.", "Atencao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SetStatus($"Analisando {selected.Count} branches em lote...");
        UseWaitCursor = true; Application.DoEvents();
        btnBatchAnalyze.Enabled = false;

        Invoke(() =>
        {
            pgBatch.Visible = true;
            pgBatch.Minimum = 0;
            pgBatch.Maximum = selected.Count;
            pgBatch.Value = 0;
        });

        Task.Run(() =>
        {
            var results = new List<BatchMergeResult>();
            int processed = 0;

            foreach (var branchName in selected)
            {
                var resolved = _git.ResolveBranch(branchName);
                if (resolved == null)
                {
                    results.Add(new BatchMergeResult
                    {
                        BranchFeature = branchName,
                        Status = "NAO ENCONTRADO"
                    });
                }
                else
                {
                    try
                    {
                        var mergeStatus = _git.CheckMergeStatus(receptor, resolved);
                        var conflicts = _git.DetectPotentialConflicts(receptor, resolved);
                        var files = _git.GetChangedFiles(receptor, resolved);
                        var branchInfo = _git.GetBranchInfo(receptor, resolved);

                        results.Add(new BatchMergeResult
                        {
                            BranchFeature = branchName,
                            Status = mergeStatus.IsMerged ? "MERGED" : "PENDENTE",
                            CommitsPendentes = mergeStatus.PendingCommits,
                            ConflitosArquivos = conflicts.Count,
                            ArquivosAlterados = files.Count,
                            UltimoAutor = branchInfo.LastCommitAuthor,
                            UltimoCommit = branchInfo.LastCommitMessage.Length > 60
                                ? branchInfo.LastCommitMessage[..60] + "..."
                                : branchInfo.LastCommitMessage,
                            IsMerged = mergeStatus.IsMerged
                        });
                    }
                    catch
                    {
                        results.Add(new BatchMergeResult
                        {
                            BranchFeature = branchName,
                            Status = "ERRO"
                        });
                    }
                }

                processed++;
                Invoke(() =>
                {
                    pgBatch.Value = processed;
                    SetStatus($"Analisando {processed}/{selected.Count}: {branchName}");
                });
            }

            Invoke(() =>
            {
                dgvBatchResults.DataSource = null;
                dgvBatchResults.DataSource = results;

                // Colorir linhas pelo status
                foreach (DataGridViewRow row in dgvBatchResults.Rows)
                {
                    if (row.DataBoundItem is BatchMergeResult r)
                    {
                        if (r.Status == "MERGED")
                        {
                            row.DefaultCellStyle.ForeColor = Color.FromArgb(80, 220, 80);
                        }
                        else if (r.Status == "PENDENTE")
                        {
                            row.DefaultCellStyle.ForeColor = r.ConflitosArquivos > 0
                                ? Color.FromArgb(255, 100, 80)
                                : Color.FromArgb(255, 200, 80);
                        }
                        else
                        {
                            row.DefaultCellStyle.ForeColor = Color.FromArgb(180, 80, 80);
                        }
                    }
                }

                var merged = results.Count(r => r.IsMerged);
                var pending = results.Count(r => r.Status == "PENDENTE");
                var withConflicts = results.Count(r => r.ConflitosArquivos > 0);

                SetStatus($"Lote concluido: {results.Count} branches | {merged} merged | {pending} pendentes | {withConflicts} com conflitos potenciais");
                pgBatch.Visible = false;
                btnBatchAnalyze.Enabled = true;
                RestoreDefaultCursor();
            });
        });
    }

    private void BtnBatchExport_Click(object? sender, EventArgs e)
    {
        if (dgvBatchResults.Rows.Count == 0)
        {
            MessageBox.Show("Execute a analise em lote primeiro.", "Atencao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var dlg = new SaveFileDialog
        {
            Filter = "Arquivo de Texto|*.txt",
            Title = "Exportar Resultado em Lote",
            FileName = $"batch_report_{DateTime.Now:yyyyMMdd_HHmm}.txt"
        };

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            var sb = new System.Text.StringBuilder();
            var line = new string('=', 120);
            sb.AppendLine(line);
            sb.AppendLine($"  RELATORIO EM LOTE - VERIFICACAO DE MERGE");
            sb.AppendLine($"  Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"  Repositorio: {_git.RepoPath}");
            sb.AppendLine($"  Branch Receptor (A): {txtBatchReceptor.Text}");
            sb.AppendLine(line);
            sb.AppendLine();
            sb.AppendLine($"  {"BRANCH FEATURE",-45} {"STATUS",-12} {"COMMITS",-10} {"CONFLITOS",-12} {"ARQUIVOS",-10} {"AUTOR",-22} {"ULTIMO COMMIT"}");
            sb.AppendLine($"  {new string('-', 45)} {new string('-', 12)} {new string('-', 10)} {new string('-', 12)} {new string('-', 10)} {new string('-', 22)} {new string('-', 40)}");

            foreach (DataGridViewRow row in dgvBatchResults.Rows)
            {
                if (row.DataBoundItem is BatchMergeResult r)
                {
                    sb.AppendLine($"  {r.BranchFeature,-45} {r.Status,-12} {r.CommitsPendentes,-10} {r.ConflitosArquivos,-12} {r.ArquivosAlterados,-10} {r.UltimoAutor,-22} {r.UltimoCommit}");
                }
            }

            var results = dgvBatchResults.Rows.Cast<DataGridViewRow>()
                .Select(r => r.DataBoundItem as BatchMergeResult).Where(r => r != null).ToList();

            sb.AppendLine();
            sb.AppendLine($"  RESUMO: {results.Count} branches | {results.Count(r => r!.IsMerged)} merged | {results.Count(r => r!.Status == "PENDENTE")} pendentes | {results.Count(r => r!.ConflitosArquivos > 0)} com conflitos");
            sb.AppendLine(line);

            File.WriteAllText(dlg.FileName, sb.ToString());
            SetStatus($"Relatorio em lote exportado: {dlg.FileName}");
            MessageBox.Show($"Relatorio salvo em:\n{dlg.FileName}", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}

// Renderer para ContextMenuStrip com tema escuro
public class DarkMenuRenderer : ToolStripProfessionalRenderer
{
    public DarkMenuRenderer() : base(new DarkMenuColors()) { }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        e.TextColor = Color.White;
        base.OnRenderItemText(e);
    }

    protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
    {
        e.ArrowColor = Color.White;
        base.OnRenderArrow(e);
    }
}

public class DarkMenuColors : ProfessionalColorTable
{
    public override Color MenuItemSelected => Color.FromArgb(55, 55, 80);
    public override Color MenuItemSelectedGradientBegin => Color.FromArgb(55, 55, 80);
    public override Color MenuItemSelectedGradientEnd => Color.FromArgb(55, 55, 80);
    public override Color MenuItemBorder => Color.FromArgb(70, 70, 100);
    public override Color MenuBorder => Color.FromArgb(60, 60, 80);
    public override Color ToolStripDropDownBackground => Color.FromArgb(30, 30, 45);
    public override Color ImageMarginGradientBegin => Color.FromArgb(30, 30, 45);
    public override Color ImageMarginGradientMiddle => Color.FromArgb(30, 30, 45);
    public override Color ImageMarginGradientEnd => Color.FromArgb(30, 30, 45);
    public override Color SeparatorDark => Color.FromArgb(50, 50, 70);
    public override Color SeparatorLight => Color.FromArgb(50, 50, 70);
}
