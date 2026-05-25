namespace Itecnous.AI.OpenAI.Models.Responses;

public class ResponseCitation
{
	public string? SourceType { get; set; }

	public string? FileId { get; set; }

	public string? DocumentId { get; set; }

	public string? Title { get; set; }

	public string? Snippet { get; set; }

	public string? Url { get; set; }

	public double? Score { get; set; }

	public int? StartIndex { get; set; }

	public int? EndIndex { get; set; }
}
