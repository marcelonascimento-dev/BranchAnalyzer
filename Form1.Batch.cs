namespace BranchAnalyzer;

public partial class Form1 : Form
{
    private void SetupBatchTab()
    {
        tabBatch = CreateTab("Lote (Multi-Branch)");

        // -- Painel superior: Receptor + Analisar + Cancel + ETA ----------
        var pnlBatchTop = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(30, 30, 42), Padding = new Padding(8, 8, 8, 8) };

        var tblBatchTop = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        tblBatchTop.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));  // Label
        tblBatchTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));   // ComboBox
        tblBatchTop.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));  // Botao Analisar
        tblBatchTop.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));  // Botao Cancel

        var lblBatchReceptor = new Label
        {
            Text = "Receptor (A):",
            ForeColor = Color.FromArgb(100, 220, 100),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };
        tblBatchTop.Controls.Add(lblBatchReceptor, 0, 0);

        txtBatchReceptor = CreateBranchComboBox(Point.Empty);
        txtBatchReceptor.Dock = DockStyle.Fill;
        txtBatchReceptor.Margin = new Padding(0, 3, 8, 3);
        SetupBranchAutocomplete(txtBatchReceptor);
        tblBatchTop.Controls.Add(txtBatchReceptor, 1, 0);

        btnBatchAnalyze = new Button
        {
            Text = "ANALISAR SELECIONADOS",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(40, 100, 200),
            ForeColor = Color.White,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 2, 4, 2),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };
        btnBatchAnalyze.FlatAppearance.BorderColor = Color.FromArgb(60, 120, 220);
        btnBatchAnalyze.Click += BtnBatchAnalyze_Click;
        tblBatchTop.Controls.Add(btnBatchAnalyze, 2, 0);

        btnBatchCancel = new Button
        {
            Text = "CANCELAR",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(180, 50, 50),
            ForeColor = Color.White,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 2, 0, 2),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Visible = false
        };
        btnBatchCancel.FlatAppearance.BorderColor = Color.FromArgb(220, 60, 60);
        btnBatchCancel.Click += (_, _) =>
        {
            _batchCts?.Cancel();
            btnBatchCancel.Enabled = false;
            btnBatchCancel.Text = "Cancelando...";
        };
        tblBatchTop.Controls.Add(btnBatchCancel, 3, 0);

        pnlBatchTop.Controls.Add(tblBatchTop);

        // Export buttons row
        btnBatchExport = new Button
        {
            Text = "Exportar TXT",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(50, 50, 70),
            ForeColor = Color.White,
            Size = new Size(110, 28),
            Location = new Point(630, 12),
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnBatchExport.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 100);
        btnBatchExport.Click += BtnBatchExport_Click;
        pnlBatchTop.Controls.Add(btnBatchExport);

        btnBatchExportCsv = new Button
        {
            Text = "CSV",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(40, 90, 60),
            ForeColor = Color.White,
            Size = new Size(55, 28),
            Location = new Point(745, 12),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 8.5f),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnBatchExportCsv.FlatAppearance.BorderColor = Color.FromArgb(60, 120, 80);
        btnBatchExportCsv.Click += (_, _) => ExportGrid(dgvBatchResults, "csv");
        pnlBatchTop.Controls.Add(btnBatchExportCsv);

        btnBatchExportJson = new Button
        {
            Text = "JSON",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(40, 70, 120),
            ForeColor = Color.White,
            Size = new Size(60, 28),
            Location = new Point(805, 12),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 8.5f),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnBatchExportJson.FlatAppearance.BorderColor = Color.FromArgb(60, 90, 150);
        btnBatchExportJson.Click += (_, _) => ExportGrid(dgvBatchResults, "json");
        pnlBatchTop.Controls.Add(btnBatchExportJson);

        pgBatch = new ProgressBar
        {
            Location = new Point(875, 16),
            Size = new Size(180, 20),
            Visible = false,
            Style = ProgressBarStyle.Continuous,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        pnlBatchTop.Controls.Add(pgBatch);

        lblBatchEta = new Label
        {
            Text = "",
            ForeColor = Color.FromArgb(180, 180, 200),
            Font = new Font("Segoe UI", 8f),
            AutoSize = true,
            Location = new Point(875, 0),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Visible = false
        };
        pnlBatchTop.Controls.Add(lblBatchEta);

        tabBatch.Controls.Add(pnlBatchTop);
        pnlBatchTop.BringToFront();

        // -- NEW: Batch Dashboard Cards -----------------------------------
        pnlBatchDashboard = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = Color.FromArgb(28, 28, 38),
            Padding = new Padding(8, 6, 8, 6),
            Visible = false
        };
        // Cards will be created dynamically in UpdateBatchDashboard

        // -- Painel esquerdo: filtros + lista de branches -----------------
        var pnlBatchLeft = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(28, 28, 38),
            Padding = new Padding(8)
        };

        // -- Secao de Filtros Avancados --
        var pnlFilters = new Panel
        {
            Dock = DockStyle.Top,
            Height = 220,
            BackColor = Color.FromArgb(32, 32, 45),
            Padding = new Padding(8, 6, 8, 4)
        };

        var lblFiltersTitle = new Label
        {
            Text = "FILTROS",
            ForeColor = Color.FromArgb(100, 180, 255),
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 22
        };

        var tblFilters = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6,
            AutoSize = false,
            Padding = Padding.Empty,
            Margin = Padding.Empty
        };
        tblFilters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58));
        tblFilters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tblFilters.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        tblFilters.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        tblFilters.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        tblFilters.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        tblFilters.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        tblFilters.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

        // Row 0: Label Nome
        var lblFilterName = new Label
        {
            Text = "Nome:",
            ForeColor = Color.FromArgb(180, 180, 190),
            Font = new Font("Segoe UI", 8.5f),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft
        };
        tblFilters.Controls.Add(lblFilterName, 0, 0);
        tblFilters.SetColumnSpan(lblFilterName, 2);

        // Row 1: TextBox Nome
        txtBatchFilter = new TextBox
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(40, 40, 55),
            ForeColor = Color.White,
            Font = new Font("Consolas", 9.5f),
            PlaceholderText = "Buscar por nome...",
            Margin = new Padding(0, 2, 0, 2)
        };
        txtBatchFilter.TextChanged += (_, _) => ApplyBatchFilters();
        tblFilters.Controls.Add(txtBatchFilter, 0, 1);
        tblFilters.SetColumnSpan(txtBatchFilter, 2);

        // Row 2: Label Autor
        var lblFilterAuthor = new Label
        {
            Text = "Autor:",
            ForeColor = Color.FromArgb(180, 180, 190),
            Font = new Font("Segoe UI", 8.5f),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft
        };
        tblFilters.Controls.Add(lblFilterAuthor, 0, 2);
        tblFilters.SetColumnSpan(lblFilterAuthor, 2);

        // Row 3: ComboBox Autor
        cmbBatchFilterAuthor = new ComboBox
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(40, 40, 55),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Consolas", 9f),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(0, 2, 0, 2)
        };
        cmbBatchFilterAuthor.Items.Add("(Todos os autores)");
        cmbBatchFilterAuthor.SelectedIndex = 0;
        cmbBatchFilterAuthor.SelectedIndexChanged += (_, _) => ApplyBatchFilters();
        tblFilters.Controls.Add(cmbBatchFilterAuthor, 0, 3);
        tblFilters.SetColumnSpan(cmbBatchFilterAuthor, 2);

        // Row 4: Tipo + Periodo
        var tblTipoPeriodo = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Margin = new Padding(0, 2, 0, 2)
        };
        tblTipoPeriodo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38));
        tblTipoPeriodo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        tblTipoPeriodo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 55));
        tblTipoPeriodo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));

        var lblFilterPrefix = new Label
        {
            Text = "Tipo:",
            ForeColor = Color.FromArgb(180, 180, 190),
            Font = new Font("Segoe UI", 8.5f),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        tblTipoPeriodo.Controls.Add(lblFilterPrefix, 0, 0);

        cmbBatchFilterPrefix = new ComboBox
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(40, 40, 55),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Consolas", 9f),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbBatchFilterPrefix.Items.Add("(Todos)");
        cmbBatchFilterPrefix.SelectedIndex = 0;
        cmbBatchFilterPrefix.SelectedIndexChanged += (_, _) => ApplyBatchFilters();
        tblTipoPeriodo.Controls.Add(cmbBatchFilterPrefix, 1, 0);

        var lblFilterDays = new Label
        {
            Text = "Periodo:",
            ForeColor = Color.FromArgb(180, 180, 190),
            Font = new Font("Segoe UI", 8.5f),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight
        };
        tblTipoPeriodo.Controls.Add(lblFilterDays, 2, 0);

        cmbBatchFilterDays = new ComboBox
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(40, 40, 55),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Consolas", 9f),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbBatchFilterDays.Items.AddRange(new object[] { "Todos", "7d", "15d", "30d", "60d", "90d" });
        cmbBatchFilterDays.SelectedIndex = 0;
        cmbBatchFilterDays.SelectedIndexChanged += (_, _) => ApplyBatchFilters();
        tblTipoPeriodo.Controls.Add(cmbBatchFilterDays, 3, 0);

        tblFilters.Controls.Add(tblTipoPeriodo, 0, 4);
        tblFilters.SetColumnSpan(tblTipoPeriodo, 2);

        // Row 5: Botoes aplicar/limpar
        var pnlFilterButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(0, 4, 0, 0),
            WrapContents = false,
            AutoSize = false
        };

        btnBatchApplyFilters = new Button
        {
            Text = "Aplicar Filtros",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(40, 100, 180),
            ForeColor = Color.White,
            Size = new Size(120, 26),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            Margin = new Padding(0, 0, 6, 0)
        };
        btnBatchApplyFilters.FlatAppearance.BorderColor = Color.FromArgb(60, 120, 200);
        btnBatchApplyFilters.Click += (_, _) => ApplyBatchFilters();
        pnlFilterButtons.Controls.Add(btnBatchApplyFilters);

        btnBatchClearFilters = new Button
        {
            Text = "Limpar",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(70, 50, 50),
            ForeColor = Color.White,
            Size = new Size(75, 26),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 8.5f),
            Margin = new Padding(0, 0, 0, 0)
        };
        btnBatchClearFilters.FlatAppearance.BorderColor = Color.FromArgb(120, 60, 60);
        btnBatchClearFilters.Click += BtnBatchClearFilters_Click;
        pnlFilterButtons.Controls.Add(btnBatchClearFilters);

        tblFilters.Controls.Add(pnlFilterButtons, 0, 5);
        tblFilters.SetColumnSpan(pnlFilterButtons, 2);

        var lblFilterResult = new Label
        {
            Name = "lblFilterResult",
            Text = "",
            ForeColor = Color.FromArgb(160, 160, 180),
            Font = new Font("Segoe UI", 8f, FontStyle.Italic),
            Dock = DockStyle.Bottom,
            Height = 18,
            TextAlign = ContentAlignment.MiddleLeft
        };

        pnlFilters.Controls.Add(tblFilters);
        pnlFilters.Controls.Add(lblFilterResult);
        pnlFilters.Controls.Add(lblFiltersTitle);
        lblFiltersTitle.BringToFront();

        // -- Titulo da lista --
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

        btnBatchInvertSel = new Button
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

        pnlBatchLeft.Controls.Add(clbBatchBranches);
        pnlBatchLeft.Controls.Add(lblBatchCount);
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
        dgvBatchResults.CellDoubleClick += DgvBatchResults_CellDoubleClick;

        // SplitContainer
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

        foreach (Control ctrl in pnlBatchLeft.Controls.Cast<Control>().ToArray())
        {
            pnlBatchLeft.Controls.Remove(ctrl);
            splitBatch.Panel1.Controls.Add(ctrl);
        }

        // Right panel: dashboard on top, grid fills rest
        splitBatch.Panel2.Controls.Add(dgvBatchResults);
        splitBatch.Panel2.Controls.Add(pnlBatchDashboard);

        tabBatch.Controls.Add(splitBatch);
        tabBatch.Controls.Add(pnlBatchTop);

        // Mover aba Lote para segunda posicao (indice 1)
        tabs.TabPages.Remove(tabBatch);
        tabs.TabPages.Insert(1, tabBatch);
    }

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
            if (!bm.FullName.StartsWith("origin/"))
                metadataFiltered.Add($"origin/{bm.FullName}");
        }

        clbBatchBranches.Items.Clear();
        int count = 0;
        foreach (var b in _batchBranches)
        {
            if (!string.IsNullOrEmpty(nameFilter) && !b.ToLowerInvariant().Contains(nameFilter))
                continue;
            if ((authorFilter != null || prefixFilter != null || cutoffDate.HasValue)
                && !metadataFiltered.Contains(b))
                continue;

            clbBatchBranches.Items.Add(b);
            count++;
        }

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

    private void LoadBatchBranches()
    {
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
            clbBatchBranches.Items.Add(b);

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

    private void BtnBatchClearFilters_Click(object? sender, EventArgs e)
    {
        txtBatchFilter.Text = "";
        cmbBatchFilterAuthor.SelectedIndex = 0;
        cmbBatchFilterPrefix.SelectedIndex = 0;
        cmbBatchFilterDays.SelectedIndex = 0;
        ApplyBatchFilters();
    }

    // =====================================================================
    //  BATCH ANALYZE - Parallel with SemaphoreSlim + CancellationToken
    // =====================================================================

    private async void BtnBatchAnalyze_Click(object? sender, EventArgs e)
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

        // Setup UI for running state
        _batchCts = new CancellationTokenSource();
        var ct = _batchCts.Token;

        SetStatus($"Analisando {selected.Count} branches em lote (paralelo)...");
        UseWaitCursor = true; Application.DoEvents();
        btnBatchAnalyze.Visible = false;
        btnBatchCancel.Visible = true;
        btnBatchCancel.Enabled = true;
        btnBatchCancel.Text = "CANCELAR";
        pgBatch.Visible = true;
        pgBatch.Minimum = 0;
        pgBatch.Maximum = selected.Count;
        pgBatch.Value = 0;
        lblBatchEta.Visible = true;
        lblBatchEta.Text = "ETA: calculando...";

        var results = new BatchMergeResult[selected.Count];
        int processed = 0;
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var semaphore = new SemaphoreSlim(4);
            var tasks = selected.Select((branchName, index) => Task.Run(async () =>
            {
                await semaphore.WaitAsync(ct);
                try
                {
                    ct.ThrowIfCancellationRequested();

                    var resolved = _git.ResolveBranch(branchName);
                    if (resolved == null)
                    {
                        results[index] = new BatchMergeResult
                        {
                            BranchFeature = branchName,
                            Status = "NAO ENCONTRADO"
                        };
                    }
                    else
                    {
                        try
                        {
                            var mergeStatus = _git.CheckMergeStatus(receptor, resolved);
                            var conflicts = _git.DetectPotentialConflicts(receptor, resolved);
                            var files = _git.GetChangedFiles(receptor, resolved);
                            var branchInfo = _git.GetBranchInfo(receptor, resolved);

                            results[index] = new BatchMergeResult
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
                            };
                        }
                        catch
                        {
                            results[index] = new BatchMergeResult
                            {
                                BranchFeature = branchName,
                                Status = "ERRO"
                            };
                        }
                    }

                    var current = Interlocked.Increment(ref processed);
                    Invoke(() =>
                    {
                        pgBatch.Value = current;
                        SetStatus($"Analisando {current}/{selected.Count}: {branchName}");

                        // ETA calculation
                        var elapsed = sw.Elapsed.TotalSeconds;
                        if (current > 0 && current < selected.Count)
                        {
                            var avgPerItem = elapsed / current;
                            var remaining = (selected.Count - current) * avgPerItem;
                            var eta = TimeSpan.FromSeconds(remaining);
                            lblBatchEta.Text = $"ETA: {eta:mm\\:ss}";
                        }
                        else if (current >= selected.Count)
                        {
                            lblBatchEta.Text = $"Concluido em {sw.Elapsed.TotalSeconds:F1}s";
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
            // Fill remaining nulls with cancelled status
            for (int i = 0; i < results.Length; i++)
            {
                results[i] ??= new BatchMergeResult
                {
                    BranchFeature = selected[i],
                    Status = "CANCELADO"
                };
            }
        }

        sw.Stop();

        // Update UI with results
        var resultList = results.Where(r => r != null).ToList();
        dgvBatchResults.DataSource = null;
        dgvBatchResults.DataSource = resultList;

        // Colorir linhas pelo status
        foreach (DataGridViewRow row in dgvBatchResults.Rows)
        {
            if (row.DataBoundItem is BatchMergeResult r)
            {
                if (r.Status == "MERGED")
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(80, 220, 80);
                else if (r.Status == "PENDENTE")
                    row.DefaultCellStyle.ForeColor = r.ConflitosArquivos > 0
                        ? Color.FromArgb(255, 100, 80)
                        : Color.FromArgb(255, 200, 80);
                else if (r.Status == "CANCELADO")
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(160, 160, 180);
                else
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(180, 80, 80);
            }
        }

        UpdateBatchDashboard(resultList);

        var merged = resultList.Count(r => r.IsMerged);
        var pending = resultList.Count(r => r.Status == "PENDENTE");
        var withConflicts = resultList.Count(r => r.ConflitosArquivos > 0);
        var cancelled = resultList.Count(r => r.Status == "CANCELADO");

        var statusMsg = $"Lote concluido em {sw.Elapsed.TotalSeconds:F1}s: {resultList.Count} branches | {merged} merged | {pending} pendentes | {withConflicts} com conflitos";
        if (cancelled > 0) statusMsg += $" | {cancelled} cancelados";
        SetStatus(statusMsg);

        pgBatch.Visible = false;
        lblBatchEta.Visible = false;
        btnBatchAnalyze.Visible = true;
        btnBatchCancel.Visible = false;
        RestoreDefaultCursor();
        _batchCts?.Dispose();
        _batchCts = null;
    }

    // =====================================================================
    //  BATCH DASHBOARD CARDS
    // =====================================================================

    private void UpdateBatchDashboard(List<BatchMergeResult> results)
    {
        pnlBatchDashboard.Controls.Clear();
        pnlBatchDashboard.Visible = true;

        var total = results.Count;
        var merged = results.Count(r => r.IsMerged);
        var pending = results.Count(r => r.Status == "PENDENTE");
        var conflicts = results.Count(r => r.ConflitosArquivos > 0);

        var cards = new[]
        {
            ("Total", total.ToString(), Color.FromArgb(120, 180, 255)),
            ("Merged", merged.ToString(), Color.FromArgb(80, 220, 80)),
            ("Pendentes", pending.ToString(), Color.FromArgb(255, 200, 80)),
            ("Com Conflitos", conflicts.ToString(), Color.FromArgb(255, 100, 80))
        };

        int cardWidth = 150;
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
                Location = new Point(8, 30),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            pnlBatchDashboard.Controls.Add(card);
            x += cardWidth + gap;
        }
    }

    // =====================================================================
    //  DRILL-DOWN DIALOG
    // =====================================================================

    private void DgvBatchResults_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        if (dgvBatchResults.Rows[e.RowIndex].DataBoundItem is not BatchMergeResult result) return;

        var receptor = _git.ResolveBranch(txtBatchReceptor.Text);
        if (receptor == null) return;

        var resolved = _git.ResolveBranch(result.BranchFeature);
        if (resolved == null) return;

        ShowDrillDownDialog(result, receptor, resolved);
    }

    private void ShowDrillDownDialog(BatchMergeResult result, string receptor, string resolved)
    {
        var dlg = new Form
        {
            Text = $"Detalhes: {result.BranchFeature}",
            Size = new Size(900, 650),
            MinimumSize = new Size(700, 500),
            StartPosition = FormStartPosition.CenterParent,
            BackColor = Color.FromArgb(24, 24, 32),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.5f),
            Icon = Icon
        };

        var tabsDrill = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9.5f),
            Padding = new Point(10, 5)
        };

        // -- Tab: Resumo --
        var tabResumo = new TabPage("Resumo") { BackColor = Color.FromArgb(24, 24, 32), Padding = new Padding(10) };
        var rtbResumo = CreateRichTextBox();
        AppendRtb(rtbResumo, "\n  RESUMO DA ANALISE\n\n", Color.FromArgb(120, 180, 255), bold: true);
        AppendRtb(rtbResumo, $"  Branch Feature: ", Color.FromArgb(140, 140, 160));
        AppendRtb(rtbResumo, $"{result.BranchFeature}\n", Color.FromArgb(255, 180, 80));
        AppendRtb(rtbResumo, $"  Branch Receptor: ", Color.FromArgb(140, 140, 160));
        AppendRtb(rtbResumo, $"{receptor}\n\n", Color.FromArgb(100, 220, 100));
        AppendRtb(rtbResumo, $"  Status: ", Color.FromArgb(140, 140, 160));
        var statusColor = result.IsMerged ? Color.FromArgb(80, 220, 80) : Color.FromArgb(255, 200, 80);
        AppendRtb(rtbResumo, $"{result.Status}\n", statusColor);
        AppendRtb(rtbResumo, $"  Commits Pendentes: ", Color.FromArgb(140, 140, 160));
        AppendRtb(rtbResumo, $"{result.CommitsPendentes}\n", Color.FromArgb(220, 220, 230));
        AppendRtb(rtbResumo, $"  Conflitos Potenciais: ", Color.FromArgb(140, 140, 160));
        AppendRtb(rtbResumo, $"{result.ConflitosArquivos}\n", result.ConflitosArquivos > 0 ? Color.FromArgb(255, 100, 80) : Color.FromArgb(80, 220, 80));
        AppendRtb(rtbResumo, $"  Arquivos Alterados: ", Color.FromArgb(140, 140, 160));
        AppendRtb(rtbResumo, $"{result.ArquivosAlterados}\n", Color.FromArgb(220, 220, 230));
        AppendRtb(rtbResumo, $"  Ultimo Autor: ", Color.FromArgb(140, 140, 160));
        AppendRtb(rtbResumo, $"{result.UltimoAutor}\n", Color.FromArgb(220, 220, 230));
        AppendRtb(rtbResumo, $"  Ultimo Commit: ", Color.FromArgb(140, 140, 160));
        AppendRtb(rtbResumo, $"{result.UltimoCommit}\n", Color.FromArgb(220, 220, 230));
        tabResumo.Controls.Add(rtbResumo);
        tabsDrill.TabPages.Add(tabResumo);

        // -- Tab: Commits --
        var tabCommits = new TabPage("Commits") { BackColor = Color.FromArgb(24, 24, 32), Padding = new Padding(4) };
        var dgvDrillCommits = CreateDataGrid();
        dgvDrillCommits.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Hash", HeaderText = "Hash", Width = 100, DataPropertyName = "Hash" },
            new DataGridViewTextBoxColumn { Name = "Author", HeaderText = "Autor", Width = 200, DataPropertyName = "Author" },
            new DataGridViewTextBoxColumn { Name = "RelativeDate", HeaderText = "Quando", Width = 130, DataPropertyName = "RelativeDate" },
            new DataGridViewTextBoxColumn { Name = "Message", HeaderText = "Mensagem", Width = 500, DataPropertyName = "Message" }
        );
        tabCommits.Controls.Add(dgvDrillCommits);
        tabsDrill.TabPages.Add(tabCommits);

        // -- Tab: Arquivos --
        var tabArquivos = new TabPage("Arquivos") { BackColor = Color.FromArgb(24, 24, 32), Padding = new Padding(4) };
        var dgvDrillFiles = CreateDataGrid();
        dgvDrillFiles.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 100, DataPropertyName = "Status" },
            new DataGridViewTextBoxColumn { Name = "FilePath", HeaderText = "Arquivo", Width = 700, DataPropertyName = "FilePath" }
        );
        tabArquivos.Controls.Add(dgvDrillFiles);
        tabsDrill.TabPages.Add(tabArquivos);

        // -- Tab: Conflitos --
        var tabConflitos = new TabPage("Conflitos") { BackColor = Color.FromArgb(24, 24, 32), Padding = new Padding(4) };
        var dgvDrillConflicts = CreateDataGrid();
        dgvDrillConflicts.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "FilePath", HeaderText = "Arquivo com Conflito Potencial", Width = 800, DataPropertyName = "FilePath" }
        );
        tabConflitos.Controls.Add(dgvDrillConflicts);
        tabsDrill.TabPages.Add(tabConflitos);

        dlg.Controls.Add(tabsDrill);

        // Load data async
        Task.Run(() =>
        {
            try
            {
                var commits = _git.GetPendingCommits(receptor, resolved);
                var files = _git.GetChangedFiles(receptor, resolved);
                var conflicts = _git.DetectPotentialConflicts(receptor, resolved);

                dlg.Invoke(() =>
                {
                    dgvDrillCommits.DataSource = commits;
                    dgvDrillFiles.DataSource = files;
                    dgvDrillConflicts.DataSource = conflicts.Select(f => new { FilePath = f }).ToList();
                });
            }
            catch (Exception ex)
            {
                try { dlg.Invoke(() => MessageBox.Show($"Erro ao carregar detalhes: {ex.Message}", "Erro")); }
                catch { /* dialog may be closed */ }
            }
        });

        dlg.ShowDialog(this);
    }

    // =====================================================================
    //  BATCH EXPORT TXT
    // =====================================================================

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
