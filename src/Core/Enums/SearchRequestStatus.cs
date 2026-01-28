namespace Core.Enums;

public enum SearchRequestStatus
{
    Pending = 0, // zero attempts
    InProgress = 1,
    Completed = 2,
    Failed = 3, // end time for searching
    Cancelled = 4 // stopped by user
}