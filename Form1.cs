using System.Diagnostics;

namespace BranchAnalyzer;

public partial class Form1 : Form
{
    private readonly GitService _git = new();
    private readonly AppSettings _settings = AppSettings.Load();

    // ── Controles ──────────────────────────────────────────────────
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
    private ComboBox txtBatchReceptor = null!;
    private CheckedListBox clbBatchBranches = null!;
    private DataGridView dgvBatchResults = null!;
    private Button btnBatchAnalyze = null!;
    private Button btnBatchSelectAll = null!;
    private Button btnBatchDeselectAll = null!;
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

    // Batch parallel analysis
    private CancellationTokenSource? _batchCts;
    private Button btnBatchCancel = null!;
    private Label lblBatchEta = null!;
    private readonly List<BatchMergeResult> _batchResults = new();

    // Branch lists
    private List<string> _allBranches = new();
    private List<string> _localBranches = new();
    private List<string> _prioritizedBranches = new();
    private List<string> _batchBranches = new();

    // Fetch animation
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
            // Forçar SplitterDistance após o form estar visível
            // WinForms reseta splitters em tabs ocultas durante a criação
            BeginInvoke(() =>
            {
                try { splitBatch.SplitterDistance = 350; } catch (Exception ex) { Logger.Warn($"SplitterDistance reset failed: {ex.Message}"); }
            });
        };
    }

    // ══════════════════════════════════════════════════════════════════
    //  UI SETUP
    // ══════════════════════════════════════════════════════════════════

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

        // ── Painel superior ────────────────────────────────────────
        pnlTop = new Panel
        {
            Dock = DockStyle.Top,
            Height = 110,
            BackColor = Color.FromArgb(30, 30, 42),
            Padding = new Padding(12, 6, 12, 6)
        };
        pnlTop.Resize += (_, _) => LayoutTopPanel();
        Controls.Add(pnlTop);

        // ── Linha 1: Titulo + info repo ──
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

        // ── Linha 2: Branch selectors (A) <> (B) [ANALISAR] ──
        // Posicionados com LayoutTopPanel() para serem responsivos
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

        // Auto-detectar repo atual
        TryAutoDetectRepo();

        // Branches começam vazios ao abrir o app
    }

    private void SetupTabs()
    {
        SetupMergeStatusTab();
        SetupMyBranchTab();
        SetupBatchTab();

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
                    try { splitBatch.SplitterDistance = 350; } catch (Exception ex) { Logger.Warn($"SplitterDistance reset failed: {ex.Message}"); }
                });
            }
            else if (tabs.SelectedTab == tabMyBranch)
            {
                LoadMyBranchInfo();
            }
        };
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

    private static string EscapeJson(string s) =>
        s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
}
