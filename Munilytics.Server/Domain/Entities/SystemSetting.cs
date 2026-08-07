namespace Munilytics.Server.Domain.Entities
{
    public class SystemSetting
    {
        public string Id { get; set; } = "Global";
        public DateTimeOffset LastSync { get; set; } = DateTimeOffset.UnixEpoch;
        public bool InProgress { get; set; }
        public DateTimeOffset? InProgressUpdatedAt { get; set; }
    }
}
