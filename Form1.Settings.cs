namespace BranchAnalyzer;

public partial class Form1 : Form
{
    private void RestoreWindowState()
    {
        if (_settings.WindowWidth > 0 && _settings.WindowHeight > 0)
        {
            var bounds = new Rectangle(
                _settings.WindowX, _settings.WindowY,
                _settings.WindowWidth, _settings.WindowHeight);

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
        // 1) Restaurar ultimo repositorio salvo nas settings
        if (!string.IsNullOrEmpty(_settings.LastRepoPath)
            && Directory.Exists(Path.Combine(_settings.LastRepoPath, ".git")))
        {
            SetRepo(_settings.LastRepoPath, autoFetch: true);
            if (!string.IsNullOrEmpty(_settings.LastRepoUrl))
                lblRepo.Text = $"{_settings.LastRepoUrl}  ->  {_settings.LastRepoPath}";
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
                lblRepo.Text = $"{_settings.LastRepoUrl}  ->  {cachedPath}";
                return;
            }
        }

        // 3) Subir a arvore de diretorios a partir do exe
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

        if (tabs.SelectedTab == tabMyBranch)
            LoadMyBranchInfo();

        if (autoFetch && !_isFetching)
        {
            StartFetchAnimation("Atualizando branches remotos");
            Task.Run(() =>
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                _git.FetchOrigin();
                sw.Stop();
                Invoke(() => StopFetchAnimation(sw.Elapsed.TotalSeconds));
            });
        }
    }

    private void BtnSetRepo_Click(object? sender, EventArgs e)
    {
        var cachePath = string.IsNullOrWhiteSpace(_settings.CloneCachePath) ? null : _settings.CloneCachePath;
        using var dlg = new RepoSelectDialog(_settings.RecentRepoPaths, cachePath);
        if (dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(dlg.SelectedRepoPath))
        {
            _settings.LastRepoUrl = dlg.SelectedRepoUrl;
            SetRepo(dlg.SelectedRepoPath);
        }
    }
}
