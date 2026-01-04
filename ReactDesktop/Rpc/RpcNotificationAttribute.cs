using JetBrains.Annotations;

namespace ReactDesktop.Rpc;

/// <summary>
/// Must be used on a method which returns either void or non-generic Task.
/// Its arguments can be either (CancellationToken) or (TParams, CancellationToken).
/// </summary>
[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method)]
public sealed class RpcNotificationAttribute : Attribute
{
}
