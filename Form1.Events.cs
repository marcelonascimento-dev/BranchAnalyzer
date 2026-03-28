namespace BranchAnalyzer;

public partial class Form1 : Form
{
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

        LoadBatchBranches();
    }

    private void StartFetchAnimation(string label)
    {
        _isFetching = true;
        btnFetch.Enabled = false;
        btnFetch.BackColor = Color.FromArgb(60, 60, 30);
        SetBusy(true);

        _fetchAnimDots = 0;
        _fetchAnimTimer = new System.Windows.Forms.Timer { Interval = 350 };
        _fetchAnimTimer.Tick += (_, _) =>
        {
            _fetchAnimDots = (_fetchAnimDots + 1) % 4;
            var dots = new string('.', _fetchAnimDots + 1);
            btnFetch.Text = $"\u2193 {label}{dots}";
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
        btnFetch.Text = "\u2193 Fetch Origin";
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

    private void Tab_SelectedIndexChanged(object? sender, EventArgs e)
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
        else if (tabs.SelectedTab == tabBranchHealth)
        {
            // Auto-load on first visit if we have a repo
            if (dgvBranchHealth.Rows.Count == 0 && !string.IsNullOrEmpty(_git.RepoPath))
                LoadBranchHealth();
        }
    }
}
