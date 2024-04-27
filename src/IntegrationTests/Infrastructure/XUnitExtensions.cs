// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace IntegrationTests.Infrastructure;

internal static partial class DistributedApplicationTestFactory
{
    ///// <summary>
    ///// Creates an <see cref="IDistributedApplicationTestingBuilder"/> for the specified app host assembly and outputs logs to the provided <see cref="ITestOutputHelper"/>.
    ///// </summary>
    //public static async Task<IDistributedApplicationTestingBuilder> CreateAsync(string appHostAssemblyPath, ITestOutputHelper testOutputHelper)
    //{
    //    var builder = await CreateAsync(appHostAssemblyPath, new XUnitTextWriter(testOutputHelper));
    //    builder.Services.AddSingleton<ILoggerProvider, XUnitLoggerProvider>();
    //    builder.Services.AddSingleton(testOutputHelper);
    //    return builder;
    //}

    /// <summary>
    /// Writes <see cref="ILogger"/> messages and resource logs to the provided <see cref="ITestOutputHelper"/>.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="testOutputHelper">The output.</param>
    /// <returns>The builder.</returns>
    public static IDistributedApplicationTestingBuilder WriteOutputTo(this IDistributedApplicationTestingBuilder builder, ITestOutputHelper testOutputHelper)
    {
        // Enable the core ILogger and resource output capturing
        builder.WriteOutputTo(new XUnitTextWriter(testOutputHelper));

        // Enable ILogger going to xUnit output
        builder.Services.AddSingleton(testOutputHelper);
        builder.Services.AddSingleton<ILoggerProvider, XUnitLoggerProvider>();

        return builder;
    }
}
