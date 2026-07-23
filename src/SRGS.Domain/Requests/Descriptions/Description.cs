using SRGS.Domain.Common;
using SRGS.Domain.Common.Results;

namespace SRGS.Domain.Requests.Notes;

// Maps to the DESCRIPTION table (a per-request clarification/discussion thread).
public sealed class Description : Entity<int>
{
    public int RequestId { get; private set; }
    public int AuthorId { get; private set; }
    public string DescriptionText { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Description()
#pragma warning restore CS8618
    { }

    private Description(int requestId, int authorId, string descriptionText)
    {
        RequestId = requestId;
        AuthorId = authorId;
        DescriptionText = descriptionText;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public static Result<Description> Create(int requestId, int authorId, string descriptionText)
    {
        if (requestId <= 0)
        {
            return DescriptionErrors.RequestIdRequired;
        }

        if (authorId <= 0)
        {
            return DescriptionErrors.AuthorRequired;
        }

        if (string.IsNullOrWhiteSpace(descriptionText))
        {
            return DescriptionErrors.DescriptionTextRequired;
        }

        return new Description(requestId, authorId, descriptionText.Trim());
    }
}
