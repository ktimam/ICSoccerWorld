using EdjCase.ICP.Agent.Agents;
using EdjCase.ICP.Candid.Models;
using EdjCase.ICP.Candid;
using System.Threading.Tasks;
using Candid.World;
using System.Collections.Generic;
using EdjCase.ICP.Agent.Responses;
using EdjCase.ICP.Candid.Mapping;
using WorldId = System.String;
using UserId = System.String;
using EntityId = System.String;
using ActionId = System.String;

namespace Candid.World
{
	public class WorldApiClient
	{
		public IAgent Agent { get; }

		public Principal CanisterId { get; }

		public CandidConverter? Converter { get; }

		public WorldApiClient(IAgent agent, Principal canisterId, CandidConverter? converter = default)
		{
			this.Agent = agent;
			this.CanisterId = canisterId;
			this.Converter = converter;
		}

		public async Task AddAdmin(WorldApiClient.AddAdminArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			await this.Agent.CallAndWaitAsync(this.CanisterId, "addAdmin", arg);
		}

		public async Task AddTrustedOrigins(WorldApiClient.AddTrustedOriginsArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			await this.Agent.CallAndWaitAsync(this.CanisterId, "addTrustedOrigins", arg);
		}

		public async Task<Models.Result2> CreateAction(Models.Action arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "createAction", arg);
			return reply.ToObjects<Models.Result2>(this.Converter);
		}

		public async Task<Models.Result2> CreateConfig(Models.StableConfig arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "createConfig", arg);
			return reply.ToObjects<Models.Result2>(this.Converter);
		}

		public async Task<Models.Result2> CreateEntity(Models.EntitySchema arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "createEntity", arg);
			return reply.ToObjects<Models.Result2>(this.Converter);
		}

		public async Task<Models.Result2> CreateEntityForAllUsers(WorldApiClient.CreateEntityForAllUsersArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "createEntityForAllUsers", arg);
			return reply.ToObjects<Models.Result2>(this.Converter);
		}

		public async Task<Models.Result2> CreateTestQuestActions(WorldApiClient.CreateTestQuestActionsArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "createTestQuestActions", arg);
			return reply.ToObjects<Models.Result2>(this.Converter);
		}

		public async Task<Models.Result2> CreateTestQuestConfigs(List<WorldApiClient.CreateTestQuestConfigsArg0Item> arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "createTestQuestConfigs", arg);
			return reply.ToObjects<Models.Result2>(this.Converter);
		}

		public async Task<UnboundedUInt> CycleBalance()
		{
			CandidArg arg = CandidArg.FromCandid();
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "cycleBalance", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<UnboundedUInt>(this.Converter);
		}

		public async Task<Models.Result2> DeleteAction(WorldApiClient.DeleteActionArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "deleteAction", arg);
			return reply.ToObjects<Models.Result2>(this.Converter);
		}

		public async Task DeleteActionHistoryForUser(WorldApiClient.DeleteActionHistoryForUserArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			await this.Agent.CallAndWaitAsync(this.CanisterId, "deleteActionHistoryForUser", arg);
		}

		public async Task DeleteActionLockState(Models.ActionLockStateArgs arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			await this.Agent.CallAndWaitAsync(this.CanisterId, "deleteActionLockState", arg);
		}

		public async Task<Models.Result3> DeleteActionStateForAllUsers(WorldApiClient.DeleteActionStateForAllUsersArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "deleteActionStateForAllUsers", arg);
			return reply.ToObjects<Models.Result3>(this.Converter);
		}

		public async Task<Models.Result10> DeleteActionStateForUser(WorldApiClient.DeleteActionStateForUserArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "deleteActionStateForUser", arg);
			return reply.ToObjects<Models.Result10>(this.Converter);
		}

		public async Task DeleteAllActionLockStates()
		{
			CandidArg arg = CandidArg.FromCandid();
			await this.Agent.CallAndWaitAsync(this.CanisterId, "deleteAllActionLockStates", arg);
		}

		public async Task<Models.Result3> DeleteAllActions()
		{
			CandidArg arg = CandidArg.FromCandid();
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "deleteAllActions", arg);
			return reply.ToObjects<Models.Result3>(this.Converter);
		}

		public async Task<Models.Result3> DeleteAllConfigs()
		{
			CandidArg arg = CandidArg.FromCandid();
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "deleteAllConfigs", arg);
			return reply.ToObjects<Models.Result3>(this.Converter);
		}

		public async Task DeleteCache()
		{
			CandidArg arg = CandidArg.FromCandid();
			await this.Agent.CallAsync(this.CanisterId, "deleteCache", arg);
		}

		public async Task<Models.Result2> DeleteConfig(WorldApiClient.DeleteConfigArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "deleteConfig", arg);
			return reply.ToObjects<Models.Result2>(this.Converter);
		}

		public async Task<Models.Result2> DeleteEntity(WorldApiClient.DeleteEntityArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "deleteEntity", arg);
			return reply.ToObjects<Models.Result2>(this.Converter);
		}

		public async Task<Models.Result10> DeleteTestQuestActionStateForUser(WorldApiClient.DeleteTestQuestActionStateForUserArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "deleteTestQuestActionStateForUser", arg);
			return reply.ToObjects<Models.Result10>(this.Converter);
		}

		public async Task DeleteUser(WorldApiClient.DeleteUserArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			await this.Agent.CallAndWaitAsync(this.CanisterId, "deleteUser", arg);
		}

		public async Task<Models.Result2> DisburseBOOMStake()
		{
			CandidArg arg = CandidArg.FromCandid();
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "disburseBOOMStake", arg);
			return reply.ToObjects<Models.Result2>(this.Converter);
		}

		public async Task<Models.Result2> DisburseExtNft(string arg0, uint arg1)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter), CandidTypedValue.FromObject(arg1, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "disburseExtNft", arg);
			return reply.ToObjects<Models.Result2>(this.Converter);
		}

		public async Task<Models.Result2> DissolveBoomStake()
		{
			CandidArg arg = CandidArg.FromCandid();
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "dissolveBoomStake", arg);
			return reply.ToObjects<Models.Result2>(this.Converter);
		}

		public async Task<Models.Result2> DissolveExtNft(string arg0, uint arg1)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter), CandidTypedValue.FromObject(arg1, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "dissolveExtNft", arg);
			return reply.ToObjects<Models.Result2>(this.Converter);
		}

		public async Task<Models.Action> EditAction(WorldApiClient.EditActionArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "editAction", arg);
			return reply.ToObjects<Models.Action>(this.Converter);
		}

		public async Task<Models.StableConfig> EditConfig(WorldApiClient.EditConfigArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "editConfig", arg);
			return reply.ToObjects<Models.StableConfig>(this.Converter);
		}

		public async Task<Models.EntitySchema> EditEntity(WorldApiClient.EditEntityArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "editEntity", arg);
			return reply.ToObjects<Models.EntitySchema>(this.Converter);
		}

		public async Task<List<Models.Action>> ExportActions()
		{
			CandidArg arg = CandidArg.FromCandid();
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "exportActions", arg);
			return reply.ToObjects<List<Models.Action>>(this.Converter);
		}

		public async Task<List<Models.StableConfig>> ExportConfigs()
		{
			CandidArg arg = CandidArg.FromCandid();
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "exportConfigs", arg);
			return reply.ToObjects<List<Models.StableConfig>>(this.Converter);
		}

		public async Task<Models.Result9> GetActionHistory(WorldApiClient.GetActionHistoryArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "getActionHistory", arg);
			return reply.ToObjects<Models.Result9>(this.Converter);
		}

		public async Task<Models.Result9> GetActionHistoryComposite(WorldApiClient.GetActionHistoryCompositeArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getActionHistoryComposite", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<Models.Result9>(this.Converter);
		}

		public async Task<bool> GetActionLockState(Models.ActionLockStateArgs arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getActionLockState", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<bool>(this.Converter);
		}

		public async Task<Models.Result8> GetActionStatusComposite(WorldApiClient.GetActionStatusCompositeArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getActionStatusComposite", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<Models.Result8>(this.Converter);
		}

		public async Task<List<Models.Action>> GetAllActions()
		{
			CandidArg arg = CandidArg.FromCandid();
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getAllActions", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<List<Models.Action>>(this.Converter);
		}

		public async Task<List<Models.StableConfig>> GetAllConfigs()
		{
			CandidArg arg = CandidArg.FromCandid();
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getAllConfigs", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<List<Models.StableConfig>>(this.Converter);
		}

		public async Task<Models.Result7> GetAllUserActionStates(WorldApiClient.GetAllUserActionStatesArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "getAllUserActionStates", arg);
			return reply.ToObjects<Models.Result7>(this.Converter);
		}

		public async Task<Models.Result7> GetAllUserActionStatesComposite(WorldApiClient.GetAllUserActionStatesCompositeArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getAllUserActionStatesComposite", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<Models.Result7>(this.Converter);
		}

		public async Task<Models.Result5> GetAllUserEntities(WorldApiClient.GetAllUserEntitiesArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "getAllUserEntities", arg);
			return reply.ToObjects<Models.Result5>(this.Converter);
		}

		public async Task<Models.Result5> GetAllUserEntitiesComposite(WorldApiClient.GetAllUserEntitiesCompositeArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getAllUserEntitiesComposite", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<Models.Result5>(this.Converter);
		}

		public async Task<UnboundedUInt> GetCurrentDauCount()
		{
			CandidArg arg = CandidArg.FromCandid();
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getCurrentDauCount", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<UnboundedUInt>(this.Converter);
		}

		public async Task<Dictionary<string, Dictionary<string, Models.EntityPermission>>> GetEntityPermissionsOfWorld()
		{
			CandidArg arg = CandidArg.FromCandid();
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "getEntityPermissionsOfWorld", arg);
			return reply.ToObjects<Dictionary<string, Dictionary<string, Models.EntityPermission>>>(this.Converter);
		}

		public async Task<WorldApiClient.GetGlobalPermissionsOfWorldReturnArg0> GetGlobalPermissionsOfWorld()
		{
			CandidArg arg = CandidArg.FromCandid();
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "getGlobalPermissionsOfWorld", arg);
			return reply.ToObjects<WorldApiClient.GetGlobalPermissionsOfWorldReturnArg0>(this.Converter);
		}

		public async Task<string> GetOwner()
		{
			CandidArg arg = CandidArg.FromCandid();
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getOwner", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<string>(this.Converter);
		}

		public async Task<UnboundedUInt> GetProcessActionCount()
		{
			CandidArg arg = CandidArg.FromCandid();
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getProcessActionCount", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<UnboundedUInt>(this.Converter);
		}

		public async Task<uint> GetTokenIndex(string arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getTokenIndex", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<uint>(this.Converter);
		}

		public async Task<Models.Result6> GetUserBoomStakeInfo(string arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getUserBoomStakeInfo", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<Models.Result6>(this.Converter);
		}

		public async Task<Models.Result2> GetUserBoomStakeTier(string arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getUserBoomStakeTier", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<Models.Result2>(this.Converter);
		}

		public async Task<Models.Result5> GetUserEntitiesFromWorldNodeComposite(WorldApiClient.GetUserEntitiesFromWorldNodeCompositeArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getUserEntitiesFromWorldNodeComposite", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<Models.Result5>(this.Converter);
		}

		public async Task<Models.Result5> GetUserEntitiesFromWorldNodeFilteredSortingComposite(WorldApiClient.GetUserEntitiesFromWorldNodeFilteredSortingCompositeArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getUserEntitiesFromWorldNodeFilteredSortingComposite", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<Models.Result5>(this.Converter);
		}

		public async Task<Dictionary<string, string>> GetUserExtStakes(string arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getUserExtStakes", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<Dictionary<string, string>>(this.Converter);
		}

		public async Task<Dictionary<string, Models.EXTStake>> GetUserExtStakesInfo(string arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getUserExtStakesInfo", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<Dictionary<string, Models.EXTStake>>(this.Converter);
		}

		public async Task<List<string>> GetUserSpecificExtStakes(WorldApiClient.GetUserSpecificExtStakesArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "getUserSpecificExtStakes", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<List<string>>(this.Converter);
		}

		public async Task<List<string>> GetTrustedOrigins()
		{
			CandidArg arg = CandidArg.FromCandid();
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "get_trusted_origins", arg);
			return reply.ToObjects<List<string>>(this.Converter);
		}

		public async Task GrantEntityPermission(Models.EntityPermission arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			await this.Agent.CallAndWaitAsync(this.CanisterId, "grantEntityPermission", arg);
		}

		public async Task GrantGlobalPermission(Models.GlobalPermission arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			await this.Agent.CallAndWaitAsync(this.CanisterId, "grantGlobalPermission", arg);
		}

		public async Task<Models.Result2> ImportAllActionsOfWorld(WorldApiClient.ImportAllActionsOfWorldArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "importAllActionsOfWorld", arg);
			return reply.ToObjects<Models.Result2>(this.Converter);
		}

		public async Task<Models.Result2> ImportAllConfigsOfWorld(WorldApiClient.ImportAllConfigsOfWorldArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "importAllConfigsOfWorld", arg);
			return reply.ToObjects<Models.Result2>(this.Converter);
		}

		public async Task<Models.Result2> ImportAllPermissionsOfWorld(WorldApiClient.ImportAllPermissionsOfWorldArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "importAllPermissionsOfWorld", arg);
			return reply.ToObjects<Models.Result2>(this.Converter);
		}

		public async Task<Models.Result2> ImportAllUsersDataOfWorld(WorldApiClient.ImportAllUsersDataOfWorldArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "importAllUsersDataOfWorld", arg);
			return reply.ToObjects<Models.Result2>(this.Converter);
		}

		public async Task LogsClear()
		{
			CandidArg arg = CandidArg.FromCandid();
			await this.Agent.CallAndWaitAsync(this.CanisterId, "logsClear", arg);
		}

		public async Task<List<string>> LogsGet()
		{
			CandidArg arg = CandidArg.FromCandid();
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "logsGet", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<List<string>>(this.Converter);
		}

		public async Task<UnboundedUInt> LogsGetCount()
		{
			CandidArg arg = CandidArg.FromCandid();
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "logsGetCount", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<UnboundedUInt>(this.Converter);
		}

		public async Task<Models.Result4> ProcessAction(Models.ActionArg arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "processAction", arg);
			return reply.ToObjects<Models.Result4>(this.Converter);
		}

		public async Task<Models.Result4> ProcessActionAwait(Models.ActionArg arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "processActionAwait", arg);
			return reply.ToObjects<Models.Result4>(this.Converter);
		}

		public async Task ProcessActionForAllUsers(Models.ActionArg arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			await this.Agent.CallAndWaitAsync(this.CanisterId, "processActionForAllUsers", arg);
		}

		public async Task RemoveAdmin(WorldApiClient.RemoveAdminArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			await this.Agent.CallAndWaitAsync(this.CanisterId, "removeAdmin", arg);
		}

		public async Task RemoveAllUserNodeRef()
		{
			CandidArg arg = CandidArg.FromCandid();
			await this.Agent.CallAndWaitAsync(this.CanisterId, "removeAllUserNodeRef", arg);
		}

		public async Task RemoveEntityPermission(Models.EntityPermission arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			await this.Agent.CallAndWaitAsync(this.CanisterId, "removeEntityPermission", arg);
		}

		public async Task RemoveGlobalPermission(Models.GlobalPermission arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			await this.Agent.CallAndWaitAsync(this.CanisterId, "removeGlobalPermission", arg);
		}

		public async Task RemoveTrustedOrigins(WorldApiClient.RemoveTrustedOriginsArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			await this.Agent.CallAndWaitAsync(this.CanisterId, "removeTrustedOrigins", arg);
		}

		public async Task<Models.Result3> ResetActionsAndConfigsToHardcodedTemplate()
		{
			CandidArg arg = CandidArg.FromCandid();
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "resetActionsAndConfigsToHardcodedTemplate", arg);
			return reply.ToObjects<Models.Result3>(this.Converter);
		}

		public async Task SetDevWorldCanisterId(string arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			await this.Agent.CallAndWaitAsync(this.CanisterId, "setDevWorldCanisterId", arg);
		}

		public async Task<Models.Result2> StakeBoomTokens(UnboundedUInt arg0, string arg1, string arg2, UnboundedUInt arg3, Models.ICRCStakeKind arg4)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter), CandidTypedValue.FromObject(arg1, this.Converter), CandidTypedValue.FromObject(arg2, this.Converter), CandidTypedValue.FromObject(arg3, this.Converter), CandidTypedValue.FromObject(arg4, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "stakeBoomTokens", arg);
			return reply.ToObjects<Models.Result2>(this.Converter);
		}

		public async Task<Models.Result2> StakeExtNft(uint arg0, string arg1, string arg2, string arg3)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter), CandidTypedValue.FromObject(arg1, this.Converter), CandidTypedValue.FromObject(arg2, this.Converter), CandidTypedValue.FromObject(arg3, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "stakeExtNft", arg);
			return reply.ToObjects<Models.Result2>(this.Converter);
		}

		public async Task<UnboundedUInt> StoreDauCount()
		{
			CandidArg arg = CandidArg.FromCandid();
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "storeDauCount", arg);
			return reply.ToObjects<UnboundedUInt>(this.Converter);
		}

		public async Task<WorldApiClient.ValidateConstraintsReturnArg0> ValidateConstraints(string arg0, ActionId arg1, OptionalValue<Models.ActionConstraint> arg2)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter), CandidTypedValue.FromObject(arg1, this.Converter), CandidTypedValue.FromObject(arg2, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "validateConstraints", arg);
			return reply.ToObjects<WorldApiClient.ValidateConstraintsReturnArg0>(this.Converter);
		}

		public async Task<bool> ValidateEntityConstraints(string arg0, List<Models.StableEntity> arg1, List<Models.EntityConstraint> arg2)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter), CandidTypedValue.FromObject(arg1, this.Converter), CandidTypedValue.FromObject(arg2, this.Converter));
			QueryResponse response = await this.Agent.QueryAsync(this.CanisterId, "validateEntityConstraints", arg);
			CandidArg reply = response.ThrowOrGetReply();
			return reply.ToObjects<bool>(this.Converter);
		}

		public async Task<Models.Result1> WithdrawIcpFromWorld(WorldApiClient.WithdrawIcpFromWorldArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "withdrawIcpFromWorld", arg);
			return reply.ToObjects<Models.Result1>(this.Converter);
		}

		public async Task<Models.Result> WithdrawIcrcFromWorld(WorldApiClient.WithdrawIcrcFromWorldArg0 arg0)
		{
			CandidArg arg = CandidArg.FromCandid(CandidTypedValue.FromObject(arg0, this.Converter));
			CandidArg reply = await this.Agent.CallAndWaitAsync(this.CanisterId, "withdrawIcrcFromWorld", arg);
			return reply.ToObjects<Models.Result>(this.Converter);
		}

		public class AddAdminArg0
		{
			[CandidName("principal")]
			public string Principal { get; set; }

			public AddAdminArg0(string principal)
			{
				this.Principal = principal;
			}

			public AddAdminArg0()
			{
			}
		}

		public class AddTrustedOriginsArg0
		{
			[CandidName("originUrl")]
			public string OriginUrl { get; set; }

			public AddTrustedOriginsArg0(string originUrl)
			{
				this.OriginUrl = originUrl;
			}

			public AddTrustedOriginsArg0()
			{
			}
		}

		public class CreateEntityForAllUsersArg0
		{
			[CandidName("eid")]
			public EntityId Eid { get; set; }

			[CandidName("fields")]
			public List<Models.Field> Fields { get; set; }

			public CreateEntityForAllUsersArg0(EntityId eid, List<Models.Field> fields)
			{
				this.Eid = eid;
				this.Fields = fields;
			}

			public CreateEntityForAllUsersArg0()
			{
			}
		}

		public class CreateTestQuestActionsArg0
		{
			[CandidName("actionId_1")]
			public string Actionid1 { get; set; }

			[CandidName("actionId_2")]
			public string Actionid2 { get; set; }

			[CandidName("game_world_canister_id")]
			public string GameWorldCanisterId { get; set; }

			public CreateTestQuestActionsArg0(string actionid1, string actionid2, string gameWorldCanisterId)
			{
				this.Actionid1 = actionid1;
				this.Actionid2 = actionid2;
				this.GameWorldCanisterId = gameWorldCanisterId;
			}

			public CreateTestQuestActionsArg0()
			{
			}
		}

		public class CreateTestQuestConfigsArg0Item
		{
			[CandidName("cid")]
			public string Cid { get; set; }

			[CandidName("description")]
			public string Description { get; set; }

			[CandidName("image_url")]
			public string ImageUrl { get; set; }

			[CandidName("name")]
			public string Name { get; set; }

			[CandidName("quest_url")]
			public string QuestUrl { get; set; }

			public CreateTestQuestConfigsArg0Item(string cid, string description, string imageUrl, string name, string questUrl)
			{
				this.Cid = cid;
				this.Description = description;
				this.ImageUrl = imageUrl;
				this.Name = name;
				this.QuestUrl = questUrl;
			}

			public CreateTestQuestConfigsArg0Item()
			{
			}
		}

		public class DeleteActionArg0
		{
			[CandidName("aid")]
			public string Aid { get; set; }

			public DeleteActionArg0(string aid)
			{
				this.Aid = aid;
			}

			public DeleteActionArg0()
			{
			}
		}

		public class DeleteActionHistoryForUserArg0
		{
			[CandidName("uid")]
			public UserId Uid { get; set; }

			public DeleteActionHistoryForUserArg0(UserId uid)
			{
				this.Uid = uid;
			}

			public DeleteActionHistoryForUserArg0()
			{
			}
		}

		public class DeleteActionStateForAllUsersArg0
		{
			[CandidName("aid")]
			public string Aid { get; set; }

			public DeleteActionStateForAllUsersArg0(string aid)
			{
				this.Aid = aid;
			}

			public DeleteActionStateForAllUsersArg0()
			{
			}
		}

		public class DeleteActionStateForUserArg0
		{
			[CandidName("aid")]
			public string Aid { get; set; }

			[CandidName("uid")]
			public string Uid { get; set; }

			public DeleteActionStateForUserArg0(string aid, string uid)
			{
				this.Aid = aid;
				this.Uid = uid;
			}

			public DeleteActionStateForUserArg0()
			{
			}
		}

		public class DeleteConfigArg0
		{
			[CandidName("cid")]
			public string Cid { get; set; }

			public DeleteConfigArg0(string cid)
			{
				this.Cid = cid;
			}

			public DeleteConfigArg0()
			{
			}
		}

		public class DeleteEntityArg0
		{
			[CandidName("eid")]
			public string Eid { get; set; }

			[CandidName("uid")]
			public string Uid { get; set; }

			public DeleteEntityArg0(string eid, string uid)
			{
				this.Eid = eid;
				this.Uid = uid;
			}

			public DeleteEntityArg0()
			{
			}
		}

		public class DeleteTestQuestActionStateForUserArg0
		{
			[CandidName("aid")]
			public string Aid { get; set; }

			public DeleteTestQuestActionStateForUserArg0(string aid)
			{
				this.Aid = aid;
			}

			public DeleteTestQuestActionStateForUserArg0()
			{
			}
		}

		public class DeleteUserArg0
		{
			[CandidName("uid")]
			public UserId Uid { get; set; }

			public DeleteUserArg0(UserId uid)
			{
				this.Uid = uid;
			}

			public DeleteUserArg0()
			{
			}
		}

		public class EditActionArg0
		{
			[CandidName("aid")]
			public string Aid { get; set; }

			public EditActionArg0(string aid)
			{
				this.Aid = aid;
			}

			public EditActionArg0()
			{
			}
		}

		public class EditConfigArg0
		{
			[CandidName("cid")]
			public string Cid { get; set; }

			public EditConfigArg0(string cid)
			{
				this.Cid = cid;
			}

			public EditConfigArg0()
			{
			}
		}

		public class EditEntityArg0
		{
			[CandidName("entityId")]
			public string EntityId { get; set; }

			[CandidName("userId")]
			public string UserId { get; set; }

			public EditEntityArg0(string entityId, string userId)
			{
				this.EntityId = entityId;
				this.UserId = userId;
			}

			public EditEntityArg0()
			{
			}
		}

		public class GetActionHistoryArg0
		{
			[CandidName("uid")]
			public UserId Uid { get; set; }

			public GetActionHistoryArg0(UserId uid)
			{
				this.Uid = uid;
			}

			public GetActionHistoryArg0()
			{
			}
		}

		public class GetActionHistoryCompositeArg0
		{
			[CandidName("uid")]
			public UserId Uid { get; set; }

			public GetActionHistoryCompositeArg0(UserId uid)
			{
				this.Uid = uid;
			}

			public GetActionHistoryCompositeArg0()
			{
			}
		}

		public class GetActionStatusCompositeArg0
		{
			[CandidName("aid")]
			public ActionId Aid { get; set; }

			[CandidName("uid")]
			public string Uid { get; set; }

			public GetActionStatusCompositeArg0(ActionId aid, string uid)
			{
				this.Aid = aid;
				this.Uid = uid;
			}

			public GetActionStatusCompositeArg0()
			{
			}
		}

		public class GetAllUserActionStatesArg0
		{
			[CandidName("uid")]
			public string Uid { get; set; }

			public GetAllUserActionStatesArg0(string uid)
			{
				this.Uid = uid;
			}

			public GetAllUserActionStatesArg0()
			{
			}
		}

		public class GetAllUserActionStatesCompositeArg0
		{
			[CandidName("uid")]
			public string Uid { get; set; }

			public GetAllUserActionStatesCompositeArg0(string uid)
			{
				this.Uid = uid;
			}

			public GetAllUserActionStatesCompositeArg0()
			{
			}
		}

		public class GetAllUserEntitiesArg0
		{
			[CandidName("page")]
			public OptionalValue<UnboundedUInt> Page { get; set; }

			[CandidName("uid")]
			public string Uid { get; set; }

			public GetAllUserEntitiesArg0(OptionalValue<UnboundedUInt> page, string uid)
			{
				this.Page = page;
				this.Uid = uid;
			}

			public GetAllUserEntitiesArg0()
			{
			}
		}

		public class GetAllUserEntitiesCompositeArg0
		{
			[CandidName("page")]
			public OptionalValue<UnboundedUInt> Page { get; set; }

			[CandidName("uid")]
			public string Uid { get; set; }

			public GetAllUserEntitiesCompositeArg0(OptionalValue<UnboundedUInt> page, string uid)
			{
				this.Page = page;
				this.Uid = uid;
			}

			public GetAllUserEntitiesCompositeArg0()
			{
			}
		}

		public class GetGlobalPermissionsOfWorldReturnArg0 : List<WorldId>
		{
			public GetGlobalPermissionsOfWorldReturnArg0()
			{
			}
		}

		public class GetUserEntitiesFromWorldNodeCompositeArg0
		{
			[CandidName("page")]
			public OptionalValue<UnboundedUInt> Page { get; set; }

			[CandidName("uid")]
			public string Uid { get; set; }

			public GetUserEntitiesFromWorldNodeCompositeArg0(OptionalValue<UnboundedUInt> page, string uid)
			{
				this.Page = page;
				this.Uid = uid;
			}

			public GetUserEntitiesFromWorldNodeCompositeArg0()
			{
			}
		}

		public class GetUserEntitiesFromWorldNodeFilteredSortingCompositeArg0
		{
			[CandidName("fieldName")]
			public string FieldName { get; set; }

			[CandidName("order")]
			public WorldApiClient.GetUserEntitiesFromWorldNodeFilteredSortingCompositeArg0.OrderInfo Order { get; set; }

			[CandidName("page")]
			public OptionalValue<UnboundedUInt> Page { get; set; }

			[CandidName("uid")]
			public string Uid { get; set; }

			public GetUserEntitiesFromWorldNodeFilteredSortingCompositeArg0(string fieldName, WorldApiClient.GetUserEntitiesFromWorldNodeFilteredSortingCompositeArg0.OrderInfo order, OptionalValue<UnboundedUInt> page, string uid)
			{
				this.FieldName = fieldName;
				this.Order = order;
				this.Page = page;
				this.Uid = uid;
			}

			public GetUserEntitiesFromWorldNodeFilteredSortingCompositeArg0()
			{
			}

			public enum OrderInfo
			{
				Ascending,
				Descending
			}
		}

		public class GetUserSpecificExtStakesArg0
		{
			[CandidName("collectionCanisterId")]
			public string CollectionCanisterId { get; set; }

			[CandidName("uid")]
			public string Uid { get; set; }

			public GetUserSpecificExtStakesArg0(string collectionCanisterId, string uid)
			{
				this.CollectionCanisterId = collectionCanisterId;
				this.Uid = uid;
			}

			public GetUserSpecificExtStakesArg0()
			{
			}
		}

		public class ImportAllActionsOfWorldArg0
		{
			[CandidName("ofWorldId")]
			public string OfWorldId { get; set; }

			public ImportAllActionsOfWorldArg0(string ofWorldId)
			{
				this.OfWorldId = ofWorldId;
			}

			public ImportAllActionsOfWorldArg0()
			{
			}
		}

		public class ImportAllConfigsOfWorldArg0
		{
			[CandidName("ofWorldId")]
			public string OfWorldId { get; set; }

			public ImportAllConfigsOfWorldArg0(string ofWorldId)
			{
				this.OfWorldId = ofWorldId;
			}

			public ImportAllConfigsOfWorldArg0()
			{
			}
		}

		public class ImportAllPermissionsOfWorldArg0
		{
			[CandidName("ofWorldId")]
			public string OfWorldId { get; set; }

			public ImportAllPermissionsOfWorldArg0(string ofWorldId)
			{
				this.OfWorldId = ofWorldId;
			}

			public ImportAllPermissionsOfWorldArg0()
			{
			}
		}

		public class ImportAllUsersDataOfWorldArg0
		{
			[CandidName("ofWorldId")]
			public string OfWorldId { get; set; }

			public ImportAllUsersDataOfWorldArg0(string ofWorldId)
			{
				this.OfWorldId = ofWorldId;
			}

			public ImportAllUsersDataOfWorldArg0()
			{
			}
		}

		public class RemoveAdminArg0
		{
			[CandidName("principal")]
			public string Principal { get; set; }

			public RemoveAdminArg0(string principal)
			{
				this.Principal = principal;
			}

			public RemoveAdminArg0()
			{
			}
		}

		public class RemoveTrustedOriginsArg0
		{
			[CandidName("originUrl")]
			public string OriginUrl { get; set; }

			public RemoveTrustedOriginsArg0(string originUrl)
			{
				this.OriginUrl = originUrl;
			}

			public RemoveTrustedOriginsArg0()
			{
			}
		}

		public class ValidateConstraintsReturnArg0
		{
			[CandidName("aid")]
			public string Aid { get; set; }

			[CandidName("status")]
			public bool Status { get; set; }

			public ValidateConstraintsReturnArg0(string aid, bool status)
			{
				this.Aid = aid;
				this.Status = status;
			}

			public ValidateConstraintsReturnArg0()
			{
			}
		}

		public class WithdrawIcpFromWorldArg0
		{
			[CandidName("toPrincipal")]
			public string ToPrincipal { get; set; }

			public WithdrawIcpFromWorldArg0(string toPrincipal)
			{
				this.ToPrincipal = toPrincipal;
			}

			public WithdrawIcpFromWorldArg0()
			{
			}
		}

		public class WithdrawIcrcFromWorldArg0
		{
			[CandidName("toPrincipal")]
			public string ToPrincipal { get; set; }

			[CandidName("tokenCanisterId")]
			public string TokenCanisterId { get; set; }

			public WithdrawIcrcFromWorldArg0(string toPrincipal, string tokenCanisterId)
			{
				this.ToPrincipal = toPrincipal;
				this.TokenCanisterId = tokenCanisterId;
			}

			public WithdrawIcrcFromWorldArg0()
			{
			}
		}
	}
}