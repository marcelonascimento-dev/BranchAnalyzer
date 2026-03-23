namespace BranchAnalyzer.Api.Models;

public class MergeStatus
{
    public bool IsMerged { get; set; }
    public int PendingCommits { get; set; }
    public int AheadCommits { get; set; }
    public string MergeBase { get; set; } = "";
}

public class CommitInfo
{
    public string Hash { get; set; } = "";
    public string Author { get; set; } = "";
    public string RelativeDate { get; set; } = "";
    public DateTime Date { get; set; }
    public string Message { get; set; } = "";
}

public class FileChange
{
    public string Status { get; set; } = "";
    public char StatusCode { get; set; }
    public string FilePath { get; set; } = "";
}

public class ContributorInfo
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public int CommitCount { get; set; }
}

public class DiffStats
{
    public string Summary { get; set; } = "";
    public Dictionary<string, int> FilesByExtension { get; set; } = new();
}

public class BranchInfo
{
    public string DivergenceDate { get; set; } = "";
    public string FirstCommitDate { get; set; } = "";
    public string LastCommitDate { get; set; } = "";
    public string LastCommitAuthor { get; set; } = "";
    public string LastCommitMessage { get; set; } = "";
}

public class LargeCommit
{
    public string Hash { get; set; } = "";
    public int LinesChanged { get; set; }
    public string Author { get; set; } = "";
    public string Message { get; set; } = "";
}

public class RemoteBranch
{
    public string Name { get; set; } = "";
    public string Date { get; set; } = "";
    public string Author { get; set; } = "";
    public string LastCommit { get; set; } = "";
}

public class BranchMetadata
{
    public string FullName { get; set; } = "";
    public string ShortName { get; set; } = "";
    public string DateShort { get; set; } = "";
    public DateTime Date { get; set; }
    public string Author { get; set; } = "";
    public string Prefix { get; set; } = "";  // bugfix, improvement, feature, etc.
}

public class BatchMergeResult
{
    public string BranchFeature { get; set; } = "";
    public string Status { get; set; } = "";
    public int CommitsPendentes { get; set; }
    public int ConflitosArquivos { get; set; }
    public int ArquivosAlterados { get; set; }
    public string UltimoAutor { get; set; } = "";
    public string UltimoCommit { get; set; } = "";
    public bool IsMerged { get; set; }
}
