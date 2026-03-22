namespace BranchAnalyzer;

public partial class Form1
{
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

            if (item == "── LOCAIS (recentes) ──" || item == "── REMOTOS ──")
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
                    items.Add("── LOCAIS (recentes) ──");
                    items.AddRange(_localBranches.Take(15));
                }
                items.Add("── REMOTOS ──");
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
                    items.Add("── LOCAIS (recentes) ──");
                    items.AddRange(matchLocal.Distinct(StringComparer.OrdinalIgnoreCase));
                }
                if (matchRemote.Count > 0)
                {
                    items.Add("── REMOTOS ──");
                    items.AddRange(matchRemote);
                }
            }

            return items;
        }

        void PopulateDropdown(string? filter)
        {
            var items = BuildItems(filter);
            if (items.Count == 0 || items.All(i => i.StartsWith("──")))
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
            if (selected.StartsWith("──"))
            {
                // Pular headers — selecionar o próximo item válido
                suppressFilter = true;
                var idx = cmb.SelectedIndex + 1;
                while (idx < cmb.Items.Count && cmb.Items[idx]?.ToString()?.StartsWith("──") == true)
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

    // ══════════════════════════════════════════════════════════════════
    //  LAYOUT RESPONSIVO
    // ══════════════════════════════════════════════════════════════════

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

        // ── Botoes superiores (direita, linha 1) ──
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

        // ── Linha 2: [ComboA] [<>] [ComboB] [ANALISAR] ──
        // Labels ficam acima dos combos
        var leftX = pad;
        var rightEnd = w - pad;
        var comboArea = rightEnd - leftX - swapW - analyzeW - gap * 3;
        var comboW = Math.Max(150, comboArea / 2);

        // Receptor (A)
        lblA.Location = new Point(leftX, yLabel);
        txtBranchA.Location = new Point(leftX, yCombo);
        txtBranchA.Width = comboW;

        // Swap
        var swapX = leftX + comboW + gap;
        btnSwap.Location = new Point(swapX, yCombo);

        // Feature (B)
        var bX = swapX + swapW + gap;
        lblB.Location = new Point(bX, yLabel);
        txtBranchB.Location = new Point(bX, yCombo);
        txtBranchB.Width = comboW;

        // Analisar
        var analyzeX = bX + comboW + gap;
        btnAnalyze.Location = new Point(analyzeX, yCombo);
        btnAnalyze.Width = Math.Max(analyzeW, rightEnd - analyzeX);
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
