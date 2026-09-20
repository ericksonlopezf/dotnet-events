// Copyright © Erickson Lopez. MIT License.

namespace EricksonLopez.Events.Bus.Configuration;

/// <summary>
/// Specifies the service provider scoping policy when resolving event handlers.
/// </summary>
public enum HandlerScopePolicy
{
    /// <summary>
    /// Automatically determines the scoping policy: creates isolated child scopes for parallel execution to ensure
    /// thread safety of scoped dependencies, and reuses ambient scope for sequential execution.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// Creates an isolated <see cref="Microsoft.Extensions.DependencyInjection.IServiceScope"/> for each handler invocation,
    /// disposing scoped services deterministically upon completion.
    /// </summary>
    CreatePerHandler = 1,

    /// <summary>
    /// Reuses the ambient <see cref="System.IServiceProvider"/> scope provided by the caller without creating new scopes.
    /// </summary>
    /// <remarks>
    /// <b>WARNING:</b> When used with parallel execution mode, concurrent handlers will share the ambient scope,
    /// which can cause concurrency conflicts on non-thread-safe scoped dependencies (such as Entity Framework Core DbContext).
    /// For parallel execution, <see cref="Auto"/> or <see cref="CreatePerHandler"/> is strongly recommended.
    /// </remarks>
    ReuseAmbientScope = 2
}
