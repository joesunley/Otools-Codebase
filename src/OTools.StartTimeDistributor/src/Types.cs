using OTools.Events;

namespace OTools.StartTimeDistributor;

public record struct StartTimeLimits(DateTime First, DateTime Last, TimeSpan Delta);
	
// Seeding from 0-1 with 1 being highest
internal record struct SeededEntry(Entry Entry, float Seeding);

internal record struct ResultEntry(Entry Entry, Result Result);