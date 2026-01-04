using JetBrains.Annotations;

namespace ReactDesktop.Rpc;

/// <summary>
/// Must be used on a method with the signature (IRpcPublisher) => void.
/// The name of the method will be used as the RPC method name.
/// </summary>
[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method)]
public class RpcPushAttribute : Attribute
{
}
