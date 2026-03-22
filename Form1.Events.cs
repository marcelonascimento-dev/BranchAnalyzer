using System.Diagnostics;

namespace BranchAnalyzer;

public partial class Form1
{
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

    private void StartFetchAnimation(string label)
    {
        _isFetching = true;
        btnFetch.Enabled = false;
        btnFetch.BackColor = Color.FromArgb(60, 60, 30);
        UseWaitCursor = true; Application.DoEvents();

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

    private async void BtnFetch_Click(object? sender, EventArgs e)
    {
        if (_isFetching) return;
        StartFetchAnimation("Fetch Origin");

        var sw = Stopwatch.StartNew();
        try
        {
            await _git.FetchOriginAsync();
        }
        catch (Exception ex)
        {
            Logger.Error("FetchOriginAsync failed", ex);
        }
        sw.Stop();
        StopFetchAnimation(sw.Elapsed.TotalSeconds);
    }

    private async void BtnFetchFull_Click()
    {
        if (_isFetching) return;
        StartFetchAnimation("Fetch + Prune");

        var sw = Stopwatch.StartNew();
        try
        {
            await _git.FetchPruneAsync();
        }
        catch (Exception ex)
        {
            Logger.Error("FetchPruneAsync failed", ex);
        }
        sw.Stop();
        StopFetchAnimation(sw.Elapsed.TotalSeconds);
    }
}
