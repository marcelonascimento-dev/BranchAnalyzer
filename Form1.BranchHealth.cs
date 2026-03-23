namespace BranchAnalyzer;

public partial class Form1 : Form
{
    private void SetupBranchHealthTab()
    {
        tabBranchHealth = CreateTab("Saude dos Branches");

        // -- Dashboard cards panel --
        var pnlHealthDashboard = new Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = Color.FromArgb(28, 28, 38),
            Padding = new Padding(8, 8, 8, 8)
        };

        lblHealthSummary = new Label
        {
            Text = "Carregue os branches para ver a saude do repositorio.",
            ForeColor = Color.FromArgb(160, 160, 180),
            Font = new Font("Segoe UI", 9.5f),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        pnlHealthDashboard.Controls.Add(lblHealthSummary);

        // -- Toolbar --
        var pnlHealthToolbar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 36,
            BackColor = Color.FromArgb(30, 30, 42),
            Padding = new Padding(8, 5, 8, 5)
        };
        var btnRefreshHealth = new Button
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
        btnRefreshHealth.FlatAppearance.BorderColor = Color.FromArgb(60, 120, 220);
        btnRefreshHealth.Click += (_, _) => LoadBranchHealth();
        pnlHealthToolbar.Controls.Add(btnRefreshHealth);

        // -- DataGridView --
        dgvBranchHealth = CreateDataGrid();
        dgvBranchHealth.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Branch", HeaderText = "Branch", Width = 350, DataPropertyName = "Branch" },
            new DataGridViewTextBoxColumn { Name = "Autor", HeaderText = "Autor", Width = 180, DataPropertyName = "Autor" },
            new DataGridViewTextBoxColumn { Name = "Data", HeaderText = "Data", Width = 130, DataPropertyName = "Data" },
            new DataGridViewTextBoxColumn { Name = "DiasInativo", HeaderText = "Dias Inativo", Width = 110, DataPropertyName = "DiasInativo" },
            new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 120, DataPropertyName = "Status" }
        );

        // Order: Fill first, then Top elements
        tabBranchHealth.Controls.Add(dgvBranchHealth);       // Fill
        tabBranchHealth.Controls.Add(pnlHealthDashboard);     // Top (dashboard)
        tabBranchHealth.Controls.Add(pnlHealthToolbar);       // Top (toolbar)
    }

    private void LoadBranchHealth()
    {
        if (string.IsNullOrEmpty(_git.RepoPath)) return;

        SetStatus("Carregando saude dos branches...");
        UseWaitCursor = true; Application.DoEvents();

        Task.Run(() =>
        {
            try
            {
                var metadata = _git.GetBranchesMetadata();

                var healthItems = metadata.Select(bm =>
                {
                    var daysInactive = (int)(DateTime.Now - bm.Date).TotalDays;
                    var status = daysInactive < 60 ? "ATIVO"
                               : daysInactive <= 180 ? "INATIVO"
                               : "OBSOLETO";

                    return new BranchHealthItem
                    {
                        Branch = bm.ShortName,
                        Autor = bm.Author,
                        Data = bm.DateShort,
                        DiasInativo = daysInactive,
                        Status = status,
                        DateValue = bm.Date
                    };
                })
                .OrderByDescending(h => h.DiasInativo)
                .ToList();

                var totalCount = healthItems.Count;
                var activeCount = healthItems.Count(h => h.Status == "ATIVO");
                var inactiveCount = healthItems.Count(h => h.Status == "INATIVO");
                var obsoleteCount = healthItems.Count(h => h.Status == "OBSOLETO");

                Invoke(() =>
                {
                    dgvBranchHealth.DataSource = null;
                    dgvBranchHealth.DataSource = healthItems;

                    // Color-code rows
                    foreach (DataGridViewRow row in dgvBranchHealth.Rows)
                    {
                        if (row.DataBoundItem is BranchHealthItem item)
                        {
                            row.DefaultCellStyle.ForeColor = item.Status switch
                            {
                                "ATIVO" => Color.FromArgb(80, 220, 80),
                                "INATIVO" => Color.FromArgb(255, 200, 80),
                                "OBSOLETO" => Color.FromArgb(255, 100, 80),
                                _ => Color.White
                            };
                        }
                    }

                    // Update dashboard cards
                    UpdateHealthDashboard(totalCount, activeCount, inactiveCount, obsoleteCount);

                    SetStatus($"Saude: {totalCount} branches | {activeCount} ativos | {inactiveCount} inativos | {obsoleteCount} obsoletos");
                    RestoreDefaultCursor();
                });
            }
            catch (Exception ex)
            {
                Invoke(() => { SetStatus($"Erro: {ex.Message}"); RestoreDefaultCursor(); });
            }
        });
    }

    private void UpdateHealthDashboard(int total, int active, int inactive, int obsolete)
    {
        // Find the dashboard panel (first Top panel in tabBranchHealth)
        var pnlDashboard = tabBranchHealth.Controls.OfType<Panel>()
            .FirstOrDefault(p => p.Dock == DockStyle.Top && p.Height == 70);

        if (pnlDashboard == null) return;

        pnlDashboard.Controls.Clear();

        var cards = new[]
        {
            ("Total", total.ToString(), Color.FromArgb(120, 180, 255)),
            ("Ativos (<60d)", active.ToString(), Color.FromArgb(80, 220, 80)),
            ("Inativos (60-180d)", inactive.ToString(), Color.FromArgb(255, 200, 80)),
            ("Obsoletos (>180d)", obsolete.ToString(), Color.FromArgb(255, 100, 80))
        };

        int cardWidth = 170;
        int cardHeight = 52;
        int gap = 12;
        int x = 8;

        foreach (var (title, value, color) in cards)
        {
            var card = new Panel
            {
                Location = new Point(x, 8),
                Size = new Size(cardWidth, cardHeight),
                BackColor = Color.FromArgb(35, 35, 50)
            };
            card.Paint += (s, e) =>
            {
                using var pen = new Pen(color, 2);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
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
                Location = new Point(8, 32),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            pnlDashboard.Controls.Add(card);
            x += cardWidth + gap;
        }
    }
}

// Model for Branch Health grid
public class BranchHealthItem
{
    public string Branch { get; set; } = "";
    public string Autor { get; set; } = "";
    public string Data { get; set; } = "";
    public int DiasInativo { get; set; }
    public string Status { get; set; } = "";
    public DateTime DateValue { get; set; }
}
