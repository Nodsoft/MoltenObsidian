﻿using System.Net.Http.Json;
using JetBrains.Annotations;
using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.Http;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring MoltenObsidian vaults in the dependency injection container.
/// </summary>
[PublicAPI]
[System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
public static class VaultDependencyInjectionExtensions
{
	/// <summary>
	/// Adds a HTTP-based MoltenObsidian vault to the service collection.
	/// </summary>
	/// <param name="serviceCollection">The service collection to add the vault to.</param>
	/// <param name="httpClientProvider">A function that returns an <see cref="HttpClient"/> to use for the vault.</param>
	/// <returns>The service collection.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the <see cref="RemoteVaultManifest"/> could not be retrieved from the root vault address.</exception>
	public static IServiceCollection AddMoltenObsidianHttpVault(this IServiceCollection serviceCollection, Func<IServiceProvider, HttpClient> httpClientProvider)
		=> serviceCollection.AddSingleton<IVault>(services =>
		{
			return services.GetRequiredService<Task<IVault>>().Result;
		}).AddMoltenObsidianHttpAsyncVault(httpClientProvider);

	/// <summary>
	/// Adds a HTTP-based MoltenObsidian vault to the service collection.
	/// </summary>
	/// <remarks>
	/// This overload provides its own <see cref="HttpClient"/> instance, which is configured to use the specified base address. <br />
	/// In most cases, you should use <see cref="AddMoltenObsidianHttpVault(IServiceCollection,Func{IServiceProvider,HttpClient})"/> instead.
	/// </remarks>
	/// <param name="serviceCollection">The service collection to add the vault to.</param>
	/// <param name="vaultRootUri">The root URI of the vault.</param>
	/// <returns>The service collection.</returns>
	public static IServiceCollection AddMoltenObsidianHttpVault(this IServiceCollection serviceCollection, string vaultRootUri)
		=> serviceCollection.AddMoltenObsidianHttpVault(_ => new() { BaseAddress = new(vaultRootUri) });
}

/// <summary>
/// Provides extension methods for configuring asynchronous MoltenObsidian vaults in the dependency injection container.
/// </summary>
[PublicAPI]
public static class AsyncVaultDependencyInjectionExtensions
{
	/// <summary>
	/// Adds a HTTP-based MoltenObsidian vault asynchronously to the service collection.
	/// </summary>
	/// <param name="serviceCollection">The service collection to add the vault to.</param>
	/// <param name="httpClientProvider">A function that returns an <see cref="HttpClient"/> to use for the vault.</param>
	/// <returns>The service collection.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the <see cref="RemoteVaultManifest"/> could not be retrieved from the root vault address.</exception>
	public static IServiceCollection AddMoltenObsidianHttpAsyncVault(this IServiceCollection serviceCollection, Func<IServiceProvider, HttpClient> httpClientProvider)
		=> serviceCollection.AddSingleton<Task<IVault>>(async services =>
		{
			HttpClient httpClient = httpClientProvider(services);

			// Get the vault manifest from the server
			RemoteVaultManifest manifest = await httpClient.GetFromJsonAsync<RemoteVaultManifest>("moltenobsidian.manifest.json")
				?? throw new InvalidOperationException("Failed to retrieve the vault manifest from the server.");

			return HttpRemoteVault.FromManifest(manifest, httpClient);
		});

	/// <summary>
	/// Adds a HTTP-based MoltenObsidian vault asynchronously to the service collection.
	/// </summary>
	/// <remarks>
	/// This overload provides its own <see cref="HttpClient"/> instance, which is configured to use the specified base address. <br />
	/// In most cases, you should use <see cref="AddMoltenObsidianHttpAsyncVault(IServiceCollection,Func{IServiceProvider,HttpClient})"/> instead.
	/// </remarks>
	/// <param name="serviceCollection">The service collection to add the vault to.</param>
	/// <param name="vaultRootUri">The root URI of the vault.</param>
	/// <returns>The service collection.</returns>
	public static IServiceCollection AddMoltenObsidianHttpAsyncVault(this IServiceCollection serviceCollection, string vaultRootUri)
		=> serviceCollection.AddMoltenObsidianHttpAsyncVault(_ => new() { BaseAddress = new(vaultRootUri) });
}