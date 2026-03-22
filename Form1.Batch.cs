namespace BranchAnalyzer;

public partial class Form1
{
    private void SetupBatchTab()
    {
        // ── Tab: Verificacao em Lote ───────────────────────────────
        tabBatch = CreateTab("Lote (Multi-Branch)");
        var pnlBatchTop = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(30, 30, 42), Padding = new Padding(8, 8, 8, 8) };

        var tblBatchTop = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        tblBatchTop.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));  // Label
        tblBatchTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));   // ComboBox
        tblBatchTop.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));  // Botão

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
            Margin = new Padding(0, 2, 0, 2),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };
        btnBatchAnalyze.FlatAppearance.BorderColor = Color.FromArgb(60, 120, 220);
        btnBatchAnalyze.Click += BtnBatchAnalyze_Click;
        tblBatchTop.Controls.Add(btnBatchAnalyze, 2, 0);

        // Cancel button (hidden by default, shown during analysis)
        btnBatchCancel = new Button
        {
            Text = "CANCELAR",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(180, 50, 50),
            ForeColor = Color.White,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 2, 0, 2),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Visible = false
        };
        btnBatchCancel.FlatAppearance.BorderColor = Color.FromArgb(220, 60, 60);
        btnBatchCancel.Click += BtnBatchCancel_Click;
        tblBatchTop.Controls.Add(btnBatchCancel, 2, 0);

        // ETA label (hidden by default)
        lblBatchEta = new Label
        {
            Text = "",
            ForeColor = Color.FromArgb(160, 160, 180),
            Font = new Font("Segoe UI", 8f, FontStyle.Italic),
            AutoSize = true,
            Visible = false,
            Location = new Point(10, 36)
        };
        pnlBatchTop.Controls.Add(lblBatchEta);

        pnlBatchTop.Controls.Add(tblBatchTop);

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

        // TableLayoutPanel para organizar filtros de forma responsiva
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
        tblFilters.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // Nome label + field
        tblFilters.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // Nome field
        tblFilters.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // Autor label + field
        tblFilters.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // Autor field
        tblFilters.RowStyles.Add(new RowStyle(SizeType.Absolute, 32)); // Tipo + Periodo
        tblFilters.RowStyles.Add(new RowStyle(SizeType.Absolute, 36)); // Botoes

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

        // Row 4: Tipo + Periodo (sub-table)
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
            Text = "Período:",
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

        // Label com resultado do filtro (no rodapé do painel)
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

        // Montar painel de filtros (ordem WinForms: Fill por ultimo)
        pnlFilters.Controls.Add(tblFilters);
        pnlFilters.Controls.Add(lblFilterResult);
        pnlFilters.Controls.Add(lblFiltersTitle);
        lblFiltersTitle.BringToFront();

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

        dgvBatchResults.CellDoubleClick += DgvBatchResults_CellDoubleClick;
        dgvBatchResults.AutoGenerateColumns = false;

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

        // Dashboard panel (hidden until analysis completes)
        pnlBatchDashboard = new Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = Color.FromArgb(30, 30, 42),
            Visible = false,
            Padding = new Padding(10, 6, 10, 6)
        };
        pnlBatchDashboard.Paint += (s, e) =>
        {
            // Draw bottom border
            using var pen = new Pen(Color.FromArgb(50, 50, 70), 1);
            e.Graphics.DrawLine(pen, 0, pnlBatchDashboard.Height - 1, pnlBatchDashboard.Width, pnlBatchDashboard.Height - 1);
        };

        // Grid no Panel2
        splitBatch.Panel2.Controls.Add(dgvBatchResults);
        splitBatch.Panel2.Controls.Add(pnlBatchDashboard);

        // Ordem IMPORTA no WinForms: adicionar Fill primeiro, depois Top
        // O ultimo adicionado com Dock=Top reserva espaco antes do Fill
        tabBatch.Controls.Add(splitBatch);   // Fill - adicionado primeiro
        tabBatch.Controls.Add(pnlBatchTop);  // Top  - adicionado depois, reserva espaco acima
    }

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

        // Setup UI for analysis
        _batchCts = new CancellationTokenSource();
        var ct = _batchCts.Token;
        btnBatchAnalyze.Visible = false;
        btnBatchCancel.Visible = true;
        lblBatchEta.Visible = true;
        lblBatchEta.Text = "Estimando...";
        pgBatch.Visible = true;
        pgBatch.Minimum = 0;
        pgBatch.Maximum = selected.Count;
        pgBatch.Value = 0;

        _batchResults.Clear();
        dgvBatchResults.DataSource = null;
        dgvBatchResults.Rows.Clear();

        SetStatus($"Analisando {selected.Count} branches em paralelo...");

        var sw = System.Diagnostics.Stopwatch.StartNew();
        int processed = 0;
        const int maxConcurrency = 4;
        var semaphore = new SemaphoreSlim(maxConcurrency);
        var lockObj = new object();

        try
        {
            var tasks = selected.Select(async branchName =>
            {
                await semaphore.WaitAsync(ct).ConfigureAwait(false);
                try
                {
                    ct.ThrowIfCancellationRequested();
                    var result = await AnalyzeSingleBranchAsync(receptor, branchName, ct).ConfigureAwait(false);

                    var count = Interlocked.Increment(ref processed);

                    Invoke(() =>
                    {
                        lock (lockObj)
                            _batchResults.Add(result);

                        pgBatch.Value = count;
                        ColorBatchRow(result);

                        // ETA calculation
                        var elapsed = sw.Elapsed;
                        var avgPerBranch = elapsed / count;
                        var remaining = avgPerBranch * (selected.Count - count);
                        lblBatchEta.Text = remaining.TotalSeconds < 2
                            ? "Quase pronto..."
                            : $"~{remaining:mm\\:ss} restante";

                        SetStatus($"Analisando {count}/{selected.Count}: {branchName}");
                    });
                }
                finally
                {
                    semaphore.Release();
                }
            }).ToList();

            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            SetStatus($"Analise cancelada. {processed}/{selected.Count} branches processados.");
        }
        catch (Exception ex)
        {
            Logger.Error($"Batch analysis error: {ex.Message}");
            SetStatus($"Erro na analise em lote: {ex.Message}");
        }
        finally
        {
            sw.Stop();

            // Refresh grid with final sorted results
            lock (lockObj)
            {
                dgvBatchResults.DataSource = null;
                dgvBatchResults.DataSource = _batchResults.OrderBy(r => r.BranchFeature).ToList();
            }
            ColorAllBatchRows();

            var merged = _batchResults.Count(r => r.IsMerged);
            var pending = _batchResults.Count(r => r.Status == "PENDENTE");
            var withConflicts = _batchResults.Count(r => r.ConflitosArquivos > 0);

            if (!ct.IsCancellationRequested)
            {
                SetStatus($"Lote concluido em {sw.Elapsed:mm\\:ss}: {_batchResults.Count} branches | {merged} merged | {pending} pendentes | {withConflicts} com conflitos");
                UpdateBatchDashboard(sw.Elapsed, merged, pending, withConflicts);
            }

            pgBatch.Visible = false;
            btnBatchCancel.Visible = false;
            lblBatchEta.Visible = false;
            btnBatchAnalyze.Visible = true;
            _batchCts?.Dispose();
            _batchCts = null;
        }
    }

    private async Task<BatchMergeResult> AnalyzeSingleBranchAsync(string receptor, string branchName, CancellationToken ct)
    {
        var resolved = _git.ResolveBranch(branchName);
        if (resolved == null)
        {
            return new BatchMergeResult
            {
                BranchFeature = branchName,
                Status = "NAO ENCONTRADO"
            };
        }

        try
        {
            var mergeStatusTask = _git.CheckMergeStatusAsync(receptor, resolved, ct);
            var conflictsTask = _git.DetectPotentialConflictsAsync(receptor, resolved, ct);
            var filesTask = _git.GetChangedFilesAsync(receptor, resolved, ct);
            var branchInfoTask = _git.GetBranchInfoAsync(receptor, resolved, ct);

            await Task.WhenAll(mergeStatusTask, conflictsTask, filesTask, branchInfoTask).ConfigureAwait(false);

            var mergeStatus = mergeStatusTask.Result;
            var conflicts = conflictsTask.Result;
            var files = filesTask.Result;
            var branchInfo = branchInfoTask.Result;

            return new BatchMergeResult
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
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            Logger.Error($"Error analyzing branch {branchName}: {ex.Message}");
            return new BatchMergeResult
            {
                BranchFeature = branchName,
                Status = "ERRO"
            };
        }
    }

    private void BtnBatchCancel_Click(object? sender, EventArgs e)
    {
        _batchCts?.Cancel();
        btnBatchCancel.Enabled = false;
        btnBatchCancel.Text = "Cancelando...";
        SetStatus("Cancelando analise...");
    }

    private void ColorBatchRow(BatchMergeResult r)
    {
        // Add row directly to the grid
        var rowIndex = dgvBatchResults.Rows.Add(
            r.BranchFeature, r.Status, r.CommitsPendentes,
            r.ConflitosArquivos, r.ArquivosAlterados,
            r.UltimoAutor, r.UltimoCommit);

        var row = dgvBatchResults.Rows[rowIndex];
        row.Tag = r;
        ApplyRowColor(row, r);
    }

    private void ColorAllBatchRows()
    {
        foreach (DataGridViewRow row in dgvBatchResults.Rows)
        {
            if (row.DataBoundItem is BatchMergeResult r)
                ApplyRowColor(row, r);
        }
    }

    private static void ApplyRowColor(DataGridViewRow row, BatchMergeResult r)
    {
        row.DefaultCellStyle.ForeColor = r.Status switch
        {
            "MERGED" => Color.FromArgb(80, 220, 80),
            "PENDENTE" => r.ConflitosArquivos > 0
                ? Color.FromArgb(255, 100, 80)
                : Color.FromArgb(255, 200, 80),
            _ => Color.FromArgb(180, 80, 80)
        };
    }

    // ══════════════════════════════════════════════════════════════════
    //  DASHBOARD RESUMO DO LOTE
    // ══════════════════════════════════════════════════════════════════

    private void UpdateBatchDashboard(TimeSpan elapsed, int merged, int pending, int withConflicts)
    {
        pnlBatchDashboard.Controls.Clear();
        pnlBatchDashboard.Visible = true;

        var total = _batchResults.Count;
        var errors = _batchResults.Count(r => r.Status == "ERRO" || r.Status == "NAO ENCONTRADO");

        // Stats cards
        AddDashboardCard(pnlBatchDashboard, 0, "Total", total.ToString(), Color.FromArgb(120, 180, 255));
        AddDashboardCard(pnlBatchDashboard, 1, "Merged", merged.ToString(), Color.FromArgb(80, 220, 80));
        AddDashboardCard(pnlBatchDashboard, 2, "Pendentes", pending.ToString(), Color.FromArgb(255, 200, 80));
        AddDashboardCard(pnlBatchDashboard, 3, "Conflitos", withConflicts.ToString(), Color.FromArgb(255, 100, 80));
        if (errors > 0)
            AddDashboardCard(pnlBatchDashboard, 4, "Erros", errors.ToString(), Color.FromArgb(180, 80, 80));

        // Top 5 branches com mais commits pendentes
        var top5 = _batchResults
            .Where(r => r.Status == "PENDENTE")
            .OrderByDescending(r => r.CommitsPendentes)
            .Take(5)
            .ToList();

        if (top5.Count > 0)
        {
            var lblTop5 = new Label
            {
                Text = "Top pendentes: " + string.Join("  |  ", top5.Select(r => $"{Truncate(r.BranchFeature, 25)} ({r.CommitsPendentes}c)")),
                ForeColor = Color.FromArgb(160, 160, 180),
                Font = new Font("Segoe UI", 8f),
                AutoSize = true,
                Location = new Point(10, 48)
            };
            pnlBatchDashboard.Controls.Add(lblTop5);
        }

        // Tempo de execução
        var lblTime = new Label
        {
            Text = $"Tempo: {elapsed:mm\\:ss}",
            ForeColor = Color.FromArgb(140, 140, 160),
            Font = new Font("Segoe UI", 8f),
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(pnlBatchDashboard.Width - 100, 6)
        };
        pnlBatchDashboard.Controls.Add(lblTime);
    }

    private static void AddDashboardCard(Panel parent, int index, string label, string value, Color color)
    {
        int x = 10 + index * 115;
        var lblValue = new Label
        {
            Text = value,
            ForeColor = color,
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            Location = new Point(x, 2),
            AutoSize = true
        };
        parent.Controls.Add(lblValue);

        var lblLabel = new Label
        {
            Text = label,
            ForeColor = Color.FromArgb(140, 140, 160),
            Font = new Font("Segoe UI", 8f),
            Location = new Point(x, 30),
            AutoSize = true
        };
        parent.Controls.Add(lblLabel);
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "...";

    // ══════════════════════════════════════════════════════════════════
    //  DRILL-DOWN: Duplo-clique em resultado do lote
    // ══════════════════════════════════════════════════════════════════

    private void DgvBatchResults_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;

        var row = dgvBatchResults.Rows[e.RowIndex];
        var result = row.DataBoundItem as BatchMergeResult ?? row.Tag as BatchMergeResult;
        if (result == null || result.Status == "NAO ENCONTRADO" || result.Status == "ERRO") return;

        var receptor = txtBatchReceptor.Text;

        // Open drill-down dialog
        using var dlg = new Form
        {
            Text = $"Detalhes: {result.BranchFeature} → {receptor}",
            Size = new Size(900, 600),
            StartPosition = FormStartPosition.CenterParent,
            BackColor = Color.FromArgb(24, 24, 32),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.5f),
            MaximizeBox = false,
            MinimizeBox = false,
            Icon = Icon
        };

        var tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9.5f),
            Padding = new Point(8, 4)
        };

        // Tab 1: Resumo
        var tabSummary = new TabPage("Resumo") { BackColor = Color.FromArgb(28, 28, 38) };
        var rtbSummary = new RichTextBox
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(28, 28, 38),
            ForeColor = Color.FromArgb(220, 220, 230),
            Font = new Font("Consolas", 10f),
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            WordWrap = true
        };

        var summaryText = $@"BRANCH FEATURE:    {result.BranchFeature}
BRANCH RECEPTOR:   {receptor}
STATUS:            {result.Status}

COMMITS PENDENTES: {result.CommitsPendentes}
CONFLITOS POT.:    {result.ConflitosArquivos}
ARQUIVOS ALTERADOS:{result.ArquivosAlterados}

ULTIMO AUTOR:      {result.UltimoAutor}
ULTIMO COMMIT:     {result.UltimoCommit}
";
        rtbSummary.Text = summaryText;
        tabSummary.Controls.Add(rtbSummary);
        tabs.TabPages.Add(tabSummary);

        // Tab 2: Commits pendentes
        var tabCommits = new TabPage("Commits Pendentes") { BackColor = Color.FromArgb(28, 28, 38) };
        var dgvCommits = CreateDataGrid();
        dgvCommits.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Hash", HeaderText = "Hash", Width = 80 },
            new DataGridViewTextBoxColumn { Name = "Autor", HeaderText = "Autor", Width = 180 },
            new DataGridViewTextBoxColumn { Name = "Data", HeaderText = "Quando", Width = 120 },
            new DataGridViewTextBoxColumn { Name = "Mensagem", HeaderText = "Mensagem", Width = 450 }
        );
        tabCommits.Controls.Add(dgvCommits);
        tabs.TabPages.Add(tabCommits);

        // Tab 3: Arquivos alterados
        var tabFiles = new TabPage("Arquivos Alterados") { BackColor = Color.FromArgb(28, 28, 38) };
        var dgvFiles = CreateDataGrid();
        dgvFiles.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 100 },
            new DataGridViewTextBoxColumn { Name = "Arquivo", HeaderText = "Arquivo", Width = 700 }
        );
        tabFiles.Controls.Add(dgvFiles);
        tabs.TabPages.Add(tabFiles);

        // Tab 4: Conflitos
        var tabConflicts = new TabPage("Conflitos Potenciais") { BackColor = Color.FromArgb(28, 28, 38) };
        var lstConflicts = new ListBox
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(28, 28, 38),
            ForeColor = Color.FromArgb(255, 100, 80),
            Font = new Font("Consolas", 10f),
            BorderStyle = BorderStyle.None
        };
        tabConflicts.Controls.Add(lstConflicts);
        tabs.TabPages.Add(tabConflicts);

        dlg.Controls.Add(tabs);

        // Loading label
        var lblLoading = new Label
        {
            Text = "Carregando detalhes...",
            ForeColor = Color.FromArgb(120, 180, 255),
            Dock = DockStyle.Bottom,
            Height = 30,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 9f, FontStyle.Italic)
        };
        dlg.Controls.Add(lblLoading);

        // Load data async after dialog is shown
        dlg.Shown += async (_, _) =>
        {
            try
            {
                var resolvedReceptor = _git.ResolveBranch(receptor)!;
                var resolvedFeature = _git.ResolveBranch(result.BranchFeature)!;

                var commitsTask = _git.GetPendingCommitsAsync(resolvedReceptor, resolvedFeature);
                var filesTask = _git.GetChangedFilesAsync(resolvedReceptor, resolvedFeature);
                var conflictsTask = _git.DetectPotentialConflictsAsync(resolvedReceptor, resolvedFeature);

                await Task.WhenAll(commitsTask, filesTask, conflictsTask);

                var commits = commitsTask.Result;
                var files = filesTask.Result;
                var conflicts = conflictsTask.Result;

                foreach (var c in commits)
                    dgvCommits.Rows.Add(c.Hash[..Math.Min(8, c.Hash.Length)], c.Author, c.RelativeDate, c.Message);

                foreach (var f in files)
                {
                    var rowIdx = dgvFiles.Rows.Add(f.Status, f.FilePath);
                    dgvFiles.Rows[rowIdx].DefaultCellStyle.ForeColor = f.StatusCode switch
                    {
                        'A' => Color.FromArgb(80, 220, 80),
                        'D' => Color.FromArgb(255, 80, 80),
                        _ => Color.FromArgb(255, 200, 80)
                    };
                }

                if (conflicts.Count == 0)
                    lstConflicts.Items.Add("Nenhum conflito potencial detectado.");
                else
                    foreach (var c in conflicts)
                        lstConflicts.Items.Add(c);

                tabCommits.Text = $"Commits Pendentes ({commits.Count})";
                tabFiles.Text = $"Arquivos Alterados ({files.Count})";
                tabConflicts.Text = $"Conflitos Potenciais ({conflicts.Count})";

                lblLoading.Visible = false;
            }
            catch (Exception ex)
            {
                lblLoading.Text = $"Erro ao carregar: {ex.Message}";
                lblLoading.ForeColor = Color.FromArgb(255, 80, 80);
            }
        };

        dlg.ShowDialog(this);
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
