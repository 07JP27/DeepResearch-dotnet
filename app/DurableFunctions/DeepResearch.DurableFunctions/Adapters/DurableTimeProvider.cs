using Microsoft.DurableTask;

namespace DeepResearch.DurableFunctions.Adapters;
public class DurableTimeProvider(TaskOrchestrationContext context) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => context.CurrentUtcDateTime;
    public override long GetTimestamp() => context.CurrentUtcDateTime.Ticks;
    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period) => 
        throw new NotSupportedException("CreateTimer is not supported in DurableTimeProvider. Use CreateTimer instead.");
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
