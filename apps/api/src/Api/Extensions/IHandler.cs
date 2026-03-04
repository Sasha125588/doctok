using System.Diagnostics.CodeAnalysis;

namespace Api.Extensions;

/// <summary>
/// Marker interface for feature handlers. Used for automatic DI registration.
/// </summary>
[SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Used as marker for assembly scanning")]
public interface IHandler
{
}
