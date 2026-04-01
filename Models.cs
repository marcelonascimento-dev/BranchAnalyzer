namespace BranchAnalyzer;

public class MergeStatus
{
    public bool IsMerged { get; set; }
    public int PendingCommits { get; set; }
    public int AheadCommits { get; set; }
    public string MergeBase { get; set; } = "";
    public int CherryEquivalents { get; set; }   // Commits ja aplicados via PR separado (mesmo patch)
    public int RealPending { get; set; }          // Commits realmente pendentes (PendingCommits - CherryEquivalents)
}

public class CherryCommitInfo
{
    public string Hash { get; set; } = "";
    public bool IsEquivalent { get; set; }  // true = ja aplicado via outro PR
    public string Author { get; set; } = "";
    public string Date { get; set; } = "";
    public string Message { get; set; } = "";
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

public class BranchSearchResult
{
    public string Branch { get; set; } = "";
    public string Hash { get; set; } = "";
    public string Author { get; set; } = "";
    public string Date { get; set; } = "";
    public string Message { get; set; } = "";
}

public class BatchMergeResult
{
    public string BranchFeature { get; set; } = "";
    public string Status { get; set; } = "";
    public int CommitsPendentes { get; set; }
    public int JaAplicados { get; set; }         // Cherry equivalents (PR separado)
    public int ReaisPendentes { get; set; }       // Realmente pendentes
    public int ConflitosArquivos { get; set; }
    public int ArquivosAlterados { get; set; }
    public string UltimoAutor { get; set; } = "";
    public string UltimoCommit { get; set; } = "";
    public bool IsMerged { get; set; }
}

public class SqlScriptInfo
{
    public string FilePath { get; set; } = "";
    public string FileName { get; set; } = "";
    public string Branch { get; set; } = "";
    public string Author { get; set; } = "";
    public string Date { get; set; } = "";
    public string CommitMessage { get; set; } = "";
    public string Status { get; set; } = "";  // Adicionado, Modificado
}

public class BranchSqlResult
{
    public string Branch { get; set; } = "";
    public string Autor { get; set; } = "";
    public string UltimaData { get; set; } = "";
    public int ScriptsAdicionados { get; set; }
    public int ScriptsModificados { get; set; }
    public int TotalScripts { get; set; }
    public string Status { get; set; } = "";  // NA DEVELOP, FALTANDO, PARCIAL
    public List<SqlScriptInfo> Scripts { get; set; } = new();
}
