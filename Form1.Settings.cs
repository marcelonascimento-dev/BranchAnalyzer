using System.Diagnostics;

namespace BranchAnalyzer;

public partial class Form1
{
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

    private void TryAutoDetectRepo()
    {
        // 1) Restaurar último repositório salvo nas settings
        if (!string.IsNullOrEmpty(_settings.LastRepoPath)
            && Directory.Exists(Path.Combine(_settings.LastRepoPath, ".git")))
        {
            SetRepo(_settings.LastRepoPath, autoFetch: true);
            // Mostrar URL se foi clonado via URL
            if (!string.IsNullOrEmpty(_settings.LastRepoUrl))
                lblRepo.Text = $"{_settings.LastRepoUrl}  →  {_settings.LastRepoPath}";
            return;
        }

        // 2) Se tinha uma URL salva, tentar o caminho do cache
        if (!string.IsNullOrEmpty(_settings.LastRepoUrl))
        {
            var cacheDir = string.IsNullOrWhiteSpace(_settings.CloneCachePath) ? null : _settings.CloneCachePath;
            var cachedPath = GitService.GetCachedRepoPath(_settings.LastRepoUrl, cacheDir);
            if (Directory.Exists(Path.Combine(cachedPath, ".git")))
            {
                SetRepo(cachedPath, autoFetch: true);
                lblRepo.Text = $"{_settings.LastRepoUrl}  →  {cachedPath}";
                return;
            }
        }

        // 3) Subir a árvore de diretórios a partir do exe
        var currentDir = AppDomain.CurrentDomain.BaseDirectory;
        var dir = new DirectoryInfo(currentDir);
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
            {
                SetRepo(dir.FullName, autoFetch: true);
                return;
            }
            dir = dir.Parent;
        }
    }

    private void SetRepo(string path, bool autoFetch = false)
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

        if (autoFetch && !_isFetching)
        {
            StartFetchAnimation("Atualizando branches remotos");
            Task.Run(() =>
            {
                var sw = Stopwatch.StartNew();
                _git.FetchOrigin();
                sw.Stop();
                Invoke(() => StopFetchAnimation(sw.Elapsed.TotalSeconds));
            });
        }
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
}
