namespace BranchAnalyzer.Api.Models;

public record SetRepoRequest(string Path);
public record CloneRepoRequest(string Url, string? CachePath = null);
public record ValidateUrlRequest(string Url);
public record BatchAnalyzeRequest(string Receptor, List<string> Branches);
public record ExportCsvRequest(List<Dictionary<string, object>> Data, List<string> Columns, string Separator = ";");
public record ExportJsonRequest(object Data, object? Metadata = null);
public record ExportTxtRequest(List<Dictionary<string, object>> Data, string Receptor);
