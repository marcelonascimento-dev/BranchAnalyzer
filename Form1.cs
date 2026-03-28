using System.Diagnostics;

namespace BranchAnalyzer;

public partial class Form1 : Form
{
    private readonly GitService _git = new();
    private readonly AppSettings _settings = AppSettings.Load();

    // -- Controles --------------------------------------------------------
    private ComboBox txtBranchA = null!;
    private ComboBox txtBranchB = null!;
    private Button btnSetRepo = null!;
    private Button btnFetch = null!;
    private Button btnSwap = null!;
    private Button btnAnalyze = null!;
    private Label lblRepo = null!;
    private Label lblStatus = null!;
    private Label lblCurrentBranch = null!;
    private Label lblA = null!;
    private Label lblB = null!;
    private Panel pnlTop = null!;
    private Panel pnlStatus = null!;
    private TabControl tabs = null!;

    // Tab: Status Merge
    private TabPage tabMergeStatus = null!;
    private Label lblMergeResult = null!;
    private Label lblMergePending = null!;
    private Label lblMergeAhead = null!;
    private Label lblMergeBase = null!;
    private Panel pnlMergeIcon = null!;
    private DataGridView dgvMergeCommits = null!;

    // NEW: Commit Search
    private TextBox txtCommitSearch = null!;
    private Label lblCommitSearchCount = null!;
    private List<CommitInfo> _allMergeCommits = new();

    // Tab: Meu Branch
    private TabPage tabMyBranch = null!;
    private RichTextBox _rtbMyBranch = null!;
    private DataGridView _dgvMyCommits = null!;
    private DataGridView _dgvLocalChanges = null!;

    // Tab: Batch (Verificacao em Lote)
    private TabPage tabBatch = null!;
    private ComboBox txtBatchReceptor = null!;
    private CheckedListBox clbBatchBranches = null!;
    private DataGridView dgvBatchResults = null!;
    private Button btnBatchAnalyze = null!;
    private Button btnBatchSelectAll = null!;
    private Button btnBatchDeselectAll = null!;
    private Button btnBatchInvertSel = null!;
    private Button btnBatchExport = null!;
    private Button btnBatchExportCsv = null!;
    private Button btnBatchExportJson = null!;
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

    // NEW: Batch Cancel + ETA
    private CancellationTokenSource? _batchCts;
    private Button btnBatchCancel = null!;
    private Label lblBatchEta = null!;

    // NEW: Batch Dashboard
    private Panel pnlBatchDashboard = null!;

    // NEW: Batch Commit Search
    private TextBox _txtBatchCommitSearch = null!;

    // NEW: Tab Branch Health
    private TabPage tabBranchHealth = null!;
    private DataGridView dgvBranchHealth = null!;
    private Label lblHealthSummary = null!;

    // Search Branch (declarados em Form1.SearchBranch.cs)
    private TabPage tabSearchBranch = null!;
    private TextBox txtSearchTerm = null!;
    private Button btnSearch = null!;
    private DataGridView dgvSearchResults = null!;
    private Label lblSearchStatus = null!;
    private Label lblSearchCount = null!;
    private CancellationTokenSource? _searchCts;

    // Branch lists (shared across all partials)
    private List<string> _allBranches = new();
    private List<string> _localBranches = new();
    private List<string> _prioritizedBranches = new();
    private List<string> _batchBranches = new();

    // Fetch animation state
    private System.Windows.Forms.Timer? _fetchAnimTimer;
    private int _fetchAnimDots = 0;
    private bool _isFetching = false;

    public Form1()
    {
        InitializeComponent();
        SetupUI();
        RestoreWindowState();
        FormClosing += (_, _) => SaveSettings();
        Shown += (_, _) =>
        {
            BeginInvoke(() =>
            {
                try { splitBatch.SplitterDistance = 350; } catch { }
            });
        };
    }

    // =====================================================================
    //  UI SETUP
    // =====================================================================

    private void SetupUI()
    {
        Text = "Branch Analyzer";
        Size = new Size(1280, 850);
        MinimumSize = new Size(1100, 700);
        MaximumSize = new Size(1400, 950);
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(24, 24, 32);
        Font = new Font("Segoe UI", 9.5f);
        Icon = CreateAppIcon();

        // -- Painel superior ----------------------------------------------
        pnlTop = new Panel
        {
            Dock = DockStyle.Top,
            Height = 110,
            BackColor = Color.FromArgb(30, 30, 42),
            Padding = new Padding(12, 6, 12, 6)
        };
        pnlTop.Resize += (_, _) => LayoutTopPanel();
        Controls.Add(pnlTop);

        // -- Linha 1: Titulo + info repo --
        var lblTitle = new Label
        {
            Text = "BRANCH ANALYZER",
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Color.FromArgb(120, 180, 255),
            AutoSize = true,
            Location = new Point(4, 2)
        };
        pnlTop.Controls.Add(lblTitle);

        var lblRepoLabel = new Label
        {
            Text = "Repo:",
            ForeColor = Color.FromArgb(140, 140, 160),
            AutoSize = true,
            Font = new Font("Segoe UI", 8f),
            Location = new Point(210, 2)
        };
        pnlTop.Controls.Add(lblRepoLabel);

        lblRepo = new Label
        {
            Text = "(nenhum)",
            ForeColor = Color.FromArgb(255, 200, 80),
            AutoSize = true,
            Font = new Font("Consolas", 8.5f),
            Location = new Point(248, 2),
            MaximumSize = new Size(600, 0)
        };
        pnlTop.Controls.Add(lblRepo);

        var lblCurrentLabel = new Label
        {
            Text = "Branch:",
            ForeColor = Color.FromArgb(140, 140, 160),
            AutoSize = true,
            Font = new Font("Segoe UI", 8f),
            Location = new Point(210, 18)
        };
        pnlTop.Controls.Add(lblCurrentLabel);

        lblCurrentBranch = new Label
        {
            Text = "",
            ForeColor = Color.FromArgb(80, 220, 120),
            AutoSize = true,
            Font = new Font("Consolas", 8.5f, FontStyle.Bold),
            Location = new Point(258, 18)
        };
        pnlTop.Controls.Add(lblCurrentBranch);

        // Botoes no canto superior direito
        btnSetRepo = new Button
        {
            Text = "Selecionar Repo",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(50, 50, 70),
            ForeColor = Color.White,
            Size = new Size(120, 26),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 8f),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnSetRepo.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 100);
        btnSetRepo.Click += BtnSetRepo_Click;
        pnlTop.Controls.Add(btnSetRepo);

        btnFetch = new Button
        {
            Text = "\u2193 Fetch",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(50, 50, 70),
            ForeColor = Color.White,
            Size = new Size(80, 26),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 8f),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnFetch.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 100);
        btnFetch.Click += BtnFetch_Click;
        pnlTop.Controls.Add(btnFetch);

        var btnFetchFull = new Button
        {
            Text = "\u25BC",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(50, 50, 70),
            ForeColor = Color.FromArgb(160, 160, 180),
            Size = new Size(22, 26),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 7f),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnFetchFull.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 100);
        var fetchMenu = new ContextMenuStrip();
        fetchMenu.BackColor = Color.FromArgb(30, 30, 45);
        fetchMenu.ForeColor = Color.White;
        fetchMenu.Renderer = new DarkMenuRenderer();
        fetchMenu.Items.Add("\u2193  Fetch Origin (rapido)", null, (_, _) => BtnFetch_Click(null, EventArgs.Empty));
        fetchMenu.Items.Add("\u2193  Fetch + Prune (completo)", null, (_, _) => BtnFetchFull_Click());
        btnFetchFull.Click += (_, _) => fetchMenu.Show(btnFetchFull, new Point(0, btnFetchFull.Height));
        pnlTop.Controls.Add(btnFetchFull);

        // -- Linha 2: Branch selectors (A) <> (B) [ANALISAR] --
        lblA = new Label
        {
            Text = "Receptor (A):",
            ForeColor = Color.FromArgb(100, 220, 100),
            AutoSize = true,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            Location = new Point(4, 40)
        };
        pnlTop.Controls.Add(lblA);

        txtBranchA = CreateBranchComboBox(new Point(4, 58));
        SetupBranchAutocomplete(txtBranchA);
        pnlTop.Controls.Add(txtBranchA);

        btnSwap = new Button
        {
            Text = "\u21C4",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(50, 50, 70),
            ForeColor = Color.FromArgb(200, 200, 255),
            Size = new Size(36, 26),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 11, FontStyle.Bold)
        };
        btnSwap.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 100);
        btnSwap.Click += (_, _) =>
        {
            (txtBranchA.Text, txtBranchB.Text) = (txtBranchB.Text, txtBranchA.Text);
        };
        pnlTop.Controls.Add(btnSwap);

        lblB = new Label
        {
            Text = "Feature (B):",
            ForeColor = Color.FromArgb(255, 180, 80),
            AutoSize = true,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
        };
        pnlTop.Controls.Add(lblB);

        txtBranchB = CreateBranchComboBox(Point.Empty);
        SetupBranchAutocomplete(txtBranchB);
        pnlTop.Controls.Add(txtBranchB);

        btnAnalyze = new Button
        {
            Text = "ANALISAR",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(40, 100, 200),
            ForeColor = Color.White,
            Size = new Size(110, 26),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnAnalyze.FlatAppearance.BorderColor = Color.FromArgb(60, 120, 220);
        btnAnalyze.Click += BtnAnalyze_Click;
        pnlTop.Controls.Add(btnAnalyze);

        // Posicionar inicialmente
        LayoutTopPanel();

        // -- Status bar inferior ------------------------------------------
        pnlStatus = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 30,
            BackColor = Color.FromArgb(30, 30, 42)
        };
        lblStatus = new Label
        {
            Text = "Pronto. Selecione um repositorio para comecar.",
            ForeColor = Color.FromArgb(140, 140, 160),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0)
        };
        pnlStatus.Controls.Add(lblStatus);
        Controls.Add(pnlStatus);

        // -- TabControl ---------------------------------------------------
        tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9.5f),
            Padding = new Point(12, 6)
        };
        Controls.Add(tabs);

        SetupTabs();
        tabs.BringToFront();

        // Auto-detectar repo atual
        TryAutoDetectRepo();
    }

    private void SetupTabs()
    {
        SetupMergeTab();
        SetupBatchTab();
        SetupMyBranchTab();
        SetupBranchHealthTab();
        SetupSearchBranchTab();

        // Mover aba Pesquisar Branch para terceira posicao (indice 2)
        tabs.TabPages.Remove(tabSearchBranch);
        tabs.TabPages.Insert(2, tabSearchBranch);

        // Tab change handler
        tabs.SelectedIndexChanged += Tab_SelectedIndexChanged;
    }

    private void LayoutTopPanel()
    {
        if (txtBranchA == null || txtBranchB == null || btnSwap == null || btnAnalyze == null)
            return;

        var w = pnlTop.ClientSize.Width;
        const int pad = 12;
        const int swapW = 36;
        const int analyzeW = 110;
        const int gap = 6;
        const int yLabel = 42;
        const int yCombo = 58;

        // -- Botoes superiores (direita, linha 1) --
        var rx = w - pad;
        var btnFetchFullCtrl = pnlTop.Controls.OfType<Button>().FirstOrDefault(b => b.Text == "\u25BC");
        if (btnFetchFullCtrl != null)
        {
            btnFetchFullCtrl.Location = new Point(rx - btnFetchFullCtrl.Width, 4);
            rx = btnFetchFullCtrl.Left - 2;
        }
        btnFetch.Location = new Point(rx - btnFetch.Width, 4);
        rx = btnFetch.Left - 4;
        btnSetRepo.Location = new Point(rx - btnSetRepo.Width, 4);

        // -- Linha 2: [ComboA] [<>] [ComboB] [ANALISAR] --
        var leftX = pad;
        var rightEnd = w - pad;
        var comboArea = rightEnd - leftX - swapW - analyzeW - gap * 3;
        var comboW = Math.Max(150, comboArea / 2);

        lblA.Location = new Point(leftX, yLabel);
        txtBranchA.Location = new Point(leftX, yCombo);
        txtBranchA.Width = comboW;

        var swapX = leftX + comboW + gap;
        btnSwap.Location = new Point(swapX, yCombo);

        var bX = swapX + swapW + gap;
        lblB.Location = new Point(bX, yLabel);
        txtBranchB.Location = new Point(bX, yCombo);
        txtBranchB.Width = comboW;

        var analyzeX = bX + comboW + gap;
        btnAnalyze.Location = new Point(analyzeX, yCombo);
        btnAnalyze.Width = Math.Max(analyzeW, rightEnd - analyzeX);
    }

    private static ComboBox CreateBranchComboBox(Point location)
    {
        var cmb = new ComboBox
        {
            Location = location,
            Size = new Size(280, 28),
            BackColor = Color.FromArgb(40, 40, 55),
            ForeColor = Color.White,
            Font = new Font("Consolas", 9.5f),
            FlatStyle = FlatStyle.Flat,
            DropDownStyle = ComboBoxStyle.DropDown,
            MaxDropDownItems = 20,
            DropDownWidth = 320,
            DropDownHeight = 300
        };
        return cmb;
    }

    private void SetupBranchAutocomplete(ComboBox cmb)
    {
        cmb.DrawMode = DrawMode.OwnerDrawFixed;
        cmb.ItemHeight = 20;

        bool suppressFilter = false;

        cmb.DrawItem += (_, e) =>
        {
            if (e.Index < 0) return;
            e.DrawBackground();
            var item = cmb.Items[e.Index]?.ToString() ?? "";
            var isLocal = _localBranches.Contains(item, StringComparer.OrdinalIgnoreCase);
            var isSelected = (e.State & DrawItemState.Selected) != 0;

            var bgColor = isSelected ? Color.FromArgb(55, 55, 90) : Color.FromArgb(30, 30, 50);
            using var bgBrush = new SolidBrush(bgColor);
            e.Graphics.FillRectangle(bgBrush, e.Bounds);

            if (item == "\u2500\u2500 LOCAIS (recentes) \u2500\u2500" || item == "\u2500\u2500 REMOTOS \u2500\u2500")
            {
                using var headerBrush = new SolidBrush(Color.FromArgb(100, 100, 140));
                using var headerFont = new Font("Segoe UI", 7.5f, FontStyle.Bold);
                e.Graphics.DrawString(item, headerFont, headerBrush, e.Bounds.X + 4, e.Bounds.Y + 3);
                using var pen = new Pen(Color.FromArgb(60, 60, 80));
                e.Graphics.DrawLine(pen, e.Bounds.X, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
                return;
            }

            if (isLocal)
            {
                using var dotBrush = new SolidBrush(Color.FromArgb(80, 200, 120));
                e.Graphics.FillEllipse(dotBrush, e.Bounds.X + 4, e.Bounds.Y + 6, 7, 7);
            }

            var textColor = isLocal ? Color.FromArgb(140, 230, 170) : Color.FromArgb(200, 200, 220);
            using var textBrush = new SolidBrush(textColor);
            using var font = new Font("Consolas", 9f);
            e.Graphics.DrawString(item, font, textBrush, e.Bounds.X + 16, e.Bounds.Y + 2);
        };

        List<string> BuildItems(string? filter)
        {
            var items = new List<string>();
            var localSet = new HashSet<string>(_localBranches, StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrEmpty(filter))
            {
                if (_localBranches.Count > 0)
                {
                    items.Add("\u2500\u2500 LOCAIS (recentes) \u2500\u2500");
                    items.AddRange(_localBranches.Take(15));
                }
                items.Add("\u2500\u2500 REMOTOS \u2500\u2500");
                items.AddRange(_allBranches.Where(b => !localSet.Contains(b)).Take(30));
            }
            else
            {
                var matchLocal = _localBranches
                    .Where(b => b.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                var matchRemote = _allBranches
                    .Where(b => !localSet.Contains(b) && b.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    .Take(25)
                    .ToList();

                var exact = _allBranches.Where(b => b.Equals(filter, StringComparison.OrdinalIgnoreCase)).ToList();
                if (exact.Count > 0 && !matchLocal.Contains(exact[0], StringComparer.OrdinalIgnoreCase))
                    matchLocal.InsertRange(0, exact);

                if (matchLocal.Count > 0)
                {
                    items.Add("\u2500\u2500 LOCAIS (recentes) \u2500\u2500");
                    items.AddRange(matchLocal.Distinct(StringComparer.OrdinalIgnoreCase));
                }
                if (matchRemote.Count > 0)
                {
                    items.Add("\u2500\u2500 REMOTOS \u2500\u2500");
                    items.AddRange(matchRemote);
                }
            }

            return items;
        }

        void PopulateDropdown(string? filter)
        {
            var items = BuildItems(filter);
            if (items.Count == 0 || items.All(i => i.StartsWith("\u2500\u2500")))
                return;

            suppressFilter = true;
            var currentText = cmb.Text;
            var selStart = cmb.SelectionStart;
            cmb.Items.Clear();
            foreach (var item in items)
                cmb.Items.Add(item);
            cmb.Text = currentText;
            cmb.SelectionStart = selStart;
            cmb.SelectionLength = 0;
            suppressFilter = false;

            cmb.DroppedDown = true;
        }

        cmb.TextChanged += (_, _) =>
        {
            if (suppressFilter) return;
            var filter = cmb.Text.Trim();
            PopulateDropdown(string.IsNullOrEmpty(filter) ? null : filter);
        };

        cmb.SelectedIndexChanged += (_, _) =>
        {
            if (suppressFilter) return;
            var selected = cmb.SelectedItem?.ToString() ?? "";
            if (selected.StartsWith("\u2500\u2500"))
            {
                suppressFilter = true;
                var idx = cmb.SelectedIndex + 1;
                while (idx < cmb.Items.Count && cmb.Items[idx]?.ToString()?.StartsWith("\u2500\u2500") == true)
                    idx++;
                if (idx < cmb.Items.Count)
                    cmb.SelectedIndex = idx;
                suppressFilter = false;
            }
        };

        cmb.DropDown += (_, _) =>
        {
            if (_allBranches.Count > 0)
            {
                var filter = cmb.Text.Trim();
                var items = BuildItems(string.IsNullOrEmpty(filter) ? null : filter);
                suppressFilter = true;
                var currentText = cmb.Text;
                cmb.Items.Clear();
                foreach (var item in items)
                    cmb.Items.Add(item);
                cmb.Text = currentText;
                suppressFilter = false;
            }
        };

        cmb.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                cmb.DroppedDown = false;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                cmb.DroppedDown = false;
            }
        };
    }

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

    private void SetStatus(string text)
    {
        lblStatus.Text = text;
        lblStatus.Refresh();
    }

    private void SetBusy(bool busy)
    {
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
    }

    private void RestoreDefaultCursor()
    {
        Cursor = Cursors.Default;
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

    private (string? a, string? b) ResolveBranches()
    {
        if (string.IsNullOrWhiteSpace(txtBranchA.Text) || string.IsNullOrWhiteSpace(txtBranchB.Text))
        {
            MessageBox.Show("Selecione os branches A e B.", "Atencao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return (null, null);
        }
        var a = _git.ResolveBranch(txtBranchA.Text);
        var b = _git.ResolveBranch(txtBranchB.Text);
        if (a == null) { MessageBox.Show($"Branch '{txtBranchA.Text}' nao encontrado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); return (null, null); }
        if (b == null) { MessageBox.Show($"Branch '{txtBranchB.Text}' nao encontrado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); return (null, null); }
        return (a, b);
    }

    private static string EscapeJson(string s) =>
        s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");

    private static Icon CreateAppIcon()
    {
        var bmp = new Bitmap(32, 32);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);

        using var bgBrush = new SolidBrush(Color.FromArgb(30, 80, 180));
        g.FillEllipse(bgBrush, 1, 1, 30, 30);

        using var pen = new Pen(Color.White, 2.2f) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round };
        g.DrawLine(pen, 10, 8, 10, 24);
        g.DrawLine(pen, 10, 14, 22, 8);

        using var nodeBrush = new SolidBrush(Color.FromArgb(80, 220, 120));
        g.FillEllipse(nodeBrush, 7, 5, 6, 6);
        g.FillEllipse(nodeBrush, 7, 21, 6, 6);
        using var nodeBrush2 = new SolidBrush(Color.FromArgb(255, 180, 60));
        g.FillEllipse(nodeBrush2, 19, 5, 6, 6);

        var handle = bmp.GetHicon();
        return Icon.FromHandle(handle);
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
