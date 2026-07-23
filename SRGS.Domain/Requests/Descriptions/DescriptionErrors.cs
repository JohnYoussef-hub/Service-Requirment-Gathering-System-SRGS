using SRGS.Domain.Common.Results;

namespace SRGS.Domain.Requests.Notes;

public static class DescriptionErrors
{
    public static Error RequestIdRequired =>
        Error.Validation("Description.RequestId.Required", "Request id is required.");

    public static Error AuthorRequired =>
        Error.Validation("Description.AuthorId.Required", "Author id is required.");

    public static Error DescriptionTextRequired =>
        Error.Validation("Description.DescriptionText.Required", "Description text is required.");
}
