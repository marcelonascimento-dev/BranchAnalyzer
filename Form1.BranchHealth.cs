namespace BranchAnalyzer;

public partial class Form1
{
    private void SetupBranchHealthTab()
    {
        tabBranchHealth = CreateTab("Branch Health");

        // Top bar
        var pnlHealthTop = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = Color.FromArgb(30, 30, 42),
            Padding = new Padding(8)
        };

        var btnHealthRefresh = new Button
        {
            Text = "Atualizar",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(40, 100, 200),
            ForeColor = Color.White,
            Size = new Size(120, 28),
            Location = new Point(10, 10),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };
        btnHealthRefresh.FlatAppearance.BorderColor = Color.FromArgb(60, 120, 220);
        btnHealthRefresh.Click += (_, _) => LoadBranchHealth();
        pnlHealthTop.Controls.Add(btnHealthRefresh);

        var btnHealthExportCsv = new Button
        {
            Text = "CSV",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(40, 90, 60),
            ForeColor = Color.White,
            Size = new Size(55, 28),
            Location = new Point(140, 10),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 8.5f)
        };
        btnHealthExportCsv.FlatAppearance.BorderColor = Color.FromArgb(60, 120, 80);
        btnHealthExportCsv.Click += (_, _) => ExportGrid(dgvBranchHealth, "csv");
        pnlHealthTop.Controls.Add(btnHealthExportCsv);

        lblHealthSummary = new Label
        {
            Text = "Clique em Atualizar para analisar a saude dos branches.",
            ForeColor = Color.FromArgb(160, 160, 180),
            Font = new Font("Segoe UI", 9f),
            AutoSize = true,
            Location = new Point(210, 16)
        };
        pnlHealthTop.Controls.Add(lblHealthSummary);

        // Grid
        dgvBranchHealth = CreateDataGrid();
        dgvBranchHealth.AutoGenerateColumns = false;
        dgvBranchHealth.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Branch", HeaderText = "Branch", Width = 300 },
            new DataGridViewTextBoxColumn { Name = "UltimoCommit", HeaderText = "Ultimo Commit", Width = 110 },
            new DataGridViewTextBoxColumn { Name = "DiasInativo", HeaderText = "Dias Inativo", Width = 90 },
            new DataGridViewTextBoxColumn { Name = "Autor", HeaderText = "Ultimo Autor", Width = 180 },
            new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 120 },
            new DataGridViewTextBoxColumn { Name = "Mensagem", HeaderText = "Ultimo Commit Msg", Width = 350 }
        );

        tabBranchHealth.Controls.Add(dgvBranchHealth);
        tabBranchHealth.Controls.Add(pnlHealthTop);
        pnlHealthTop.BringToFront();
    }

    private async void LoadBranchHealth()
    {
        if (string.IsNullOrEmpty(_git.RepoPath)) return;

        lblHealthSummary.Text = "Carregando dados dos branches...";
        lblHealthSummary.ForeColor = Color.FromArgb(255, 200, 80);
        dgvBranchHealth.Rows.Clear();

        try
        {
            var metadata = await Task.Run(() => _git.GetBranchesMetadata());
            var now = DateTime.Now;

            dgvBranchHealth.Rows.Clear();

            int obsolete = 0, stale = 0, active = 0;

            foreach (var bm in metadata.OrderByDescending(m => (now - m.Date).TotalDays))
            {
                var daysInactive = (int)(now - bm.Date).TotalDays;

                string status;
                Color rowColor;
                if (daysInactive > 180)
                {
                    status = "OBSOLETO";
                    rowColor = Color.FromArgb(255, 80, 80);
                    obsolete++;
                }
                else if (daysInactive > 60)
                {
                    status = "INATIVO";
                    rowColor = Color.FromArgb(255, 180, 60);
                    stale++;
                }
                else
                {
                    status = "ATIVO";
                    rowColor = Color.FromArgb(80, 220, 80);
                    active++;
                }

                var rowIdx = dgvBranchHealth.Rows.Add(
                    bm.ShortName,
                    bm.DateShort,
                    daysInactive,
                    bm.Author,
                    status,
                    ""  // message filled later if needed
                );

                dgvBranchHealth.Rows[rowIdx].DefaultCellStyle.ForeColor = rowColor;
            }

            lblHealthSummary.Text = $"{metadata.Count} branches  |  {active} ativos  |  {stale} inativos (>60d)  |  {obsolete} obsoletos (>180d)";
            lblHealthSummary.ForeColor = Color.FromArgb(160, 220, 255);
        }
        catch (Exception ex)
        {
            Logger.Error($"LoadBranchHealth error: {ex.Message}");
            lblHealthSummary.Text = $"Erro: {ex.Message}";
            lblHealthSummary.ForeColor = Color.FromArgb(255, 80, 80);
        }
    }
}
