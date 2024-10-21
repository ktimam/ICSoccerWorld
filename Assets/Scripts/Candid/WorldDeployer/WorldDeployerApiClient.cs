using EdjCase.ICP.Agent.Agents;
using EdjCase.ICP.Candid.Models;
using EdjCase.ICP.Candid;
using System.Threading.Tasks;
using EdjCase.ICP.Agent.Responses;
using System.Collections.Generic;
using Candid.WorldDeployer;
using EdjCase.ICP.Candid.Mapping;
using System;

namespace Candid.WorldDeployer
{
	public class WorldDeployerApiClient
	{
		public IAgent Agent { get; }

		public Principal CanisterId { get; }

		public CandidConverter? Converter { get; }

		public WorldDeployerApiClient(IAgent agent, Principal canisterId, CandidConverter? converter = default)
		{
			this.Agent = agent;
			this.CanisterId = canisterId;
			this.Converter = converter;
		}

		public async Task AddAdmin(string arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			await this.Agent.CallAndWaitAsync(this.CanisterId, "addAdmin", arg);
		}

		public async Task<string> CreateWorldCanister(string arg0, string arg1)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter), CandidTypedValue.FromObject(arg1, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "createWorldCanister", arg);
			return reply.ToObjects<string>(this.Converter);
		}

		public async Task<UnboundedUInt> CycleBalance()
		{
			CandidArg arg = CandidArg.FromCandid();
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "cycleBalance", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<UnboundedUInt>(this.Converter);
		}

		public async Task<List<string>> GetAllAdmins()
		{
			CandidArg arg = CandidArg.FromCandid();
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getAllAdmins", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<List<string>>(this.Converter);
		}

		public async Task<Dictionary<string, string>> GetAllWorlds()
		{
			CandidArg arg = CandidArg.FromCandid();
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getAllWorlds", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<Dictionary<string, string>>(this.Converter);
		}

		public async Task<OptionalValue<string>> GetOwner(string arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getOwner", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<OptionalValue<string>>(this.Converter);
		}

		public async Task<UnboundedUInt> GetTotalWorlds()
		{
			CandidArg arg = CandidArg.FromCandid();
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getTotalWorlds", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<UnboundedUInt>(this.Converter);
		}

		public async Task<UnboundedUInt> GetUserTotalWorlds(string arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getUserTotalWorlds", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<UnboundedUInt>(this.Converter);
		}

		public async Task<List<Models.World>> GetUserWorlds(string arg0, UnboundedUInt arg1)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter), CandidTypedValue.FromObject(arg1, this.Converter));
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getUserWorlds", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<List<Models.World>>(this.Converter);
		}

		public async Task<string> GetWorldCover(string arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getWorldCover", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<string>(this.Converter);
		}

		public async Task<OptionalValue<Models.World>> GetWorldDetails(string arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getWorldDetails", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<OptionalValue<Models.World>>(this.Converter);
		}

		public async Task<string> GetWorldWasmVersion()
		{
			CandidArg arg = CandidArg.FromCandid();
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getWorldWasmVersion", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<string>(this.Converter);
		}

		public async Task<List<Models.World>> GetWorlds(UnboundedUInt arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getWorlds", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<List<Models.World>>(this.Converter);
		}

		public async Task<Models.HttpResponse> HttpRequest(Models.HttpRequest arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "http_request", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<Models.HttpResponse>(this.Converter);
		}

		public async Task RemoveAdmin(string arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			await this.Agent.CallAndWaitAsync(this.CanisterId, "removeAdmin", arg);
		}

		public async Task<Models.Result> UpdateWorldCover(string arg0, string arg1)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter), CandidTypedValue.FromObject(arg1, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "updateWorldCover", arg);
			return reply.ToObjects<Models.Result>(this.Converter);
		}

		public async Task<Models.Result> UpdateWorldName(string arg0, string arg1)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter), CandidTypedValue.FromObject(arg1, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "updateWorldName", arg);
			return reply.ToObjects<Models.Result>(this.Converter);
		}

		public async Task<UnboundedInt> UpdateWorldWasmModule(WorldDeployerApiClient.UpdateWorldWasmModuleArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "updateWorldWasmModule", arg);
			return reply.ToObjects<UnboundedInt>(this.Converter);
		}

		public async Task UpgradeWorlds(UnboundedInt arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			await this.Agent.CallAndWaitAsync(this.CanisterId, "upgrade_worlds", arg);
		}

		public async Task<WorldDeployerApiClient.ValidateUpgradeWorldsReturnArg0> ValidateUpgradeWorlds(UnboundedInt arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "validate_upgrade_worlds", arg);
			return reply.ToObjects<WorldDeployerApiClient.ValidateUpgradeWorldsReturnArg0>(this.Converter);
		}

		public class UpdateWorldWasmModuleArg0
		{
			[CandidName("version")]
			public string Version { get; set; }

			[CandidName("wasm")]
			public List<byte> Wasm { get; set; }

			public UpdateWorldWasmModuleArg0(string version, List<byte> wasm)
			{
				this.Version = version;
				this.Wasm = wasm;
			}

			public UpdateWorldWasmModuleArg0()
			{
			}
		}

		[Variant]
		public class ValidateUpgradeWorldsReturnArg0
		{
			[VariantTagProperty]
			public WorldDeployerApiClient.ValidateUpgradeWorldsReturnArg0Tag Tag { get; set; }

			[VariantValueProperty]
			public object? Value { get; set; }

			public ValidateUpgradeWorldsReturnArg0(WorldDeployerApiClient.ValidateUpgradeWorldsReturnArg0Tag tag, object? value)
			{
				this.Tag = tag;
				this.Value = value;
			}

			protected ValidateUpgradeWorldsReturnArg0()
			{
			}

			public static WorldDeployerApiClient.ValidateUpgradeWorldsReturnArg0 Err(string info)
			{
				return new WorldDeployerApiClient.ValidateUpgradeWorldsReturnArg0(WorldDeployerApiClient.ValidateUpgradeWorldsReturnArg0Tag.Err, info);
			}

			public static WorldDeployerApiClient.ValidateUpgradeWorldsReturnArg0 Ok(string info)
			{
				return new WorldDeployerApiClient.ValidateUpgradeWorldsReturnArg0(WorldDeployerApiClient.ValidateUpgradeWorldsReturnArg0Tag.Ok, info);
			}

			public string AsErr()
			{
				this.ValidateTag(WorldDeployerApiClient.ValidateUpgradeWorldsReturnArg0Tag.Err);
				return (string)this.Value!;
			}

			public string AsOk()
			{
				this.ValidateTag(WorldDeployerApiClient.ValidateUpgradeWorldsReturnArg0Tag.Ok);
				return (string)this.Value!;
			}

			private void ValidateTag(WorldDeployerApiClient.ValidateUpgradeWorldsReturnArg0Tag tag)
			{
				if (!this.Tag.Equals(tag))
				{
					throw new InvalidOperationException($"Cannot cast '{this.Tag}' to type '{tag}'");
				}
			}
		}

		public enum ValidateUpgradeWorldsReturnArg0Tag
		{
			Err,
			Ok
		}
	}
}