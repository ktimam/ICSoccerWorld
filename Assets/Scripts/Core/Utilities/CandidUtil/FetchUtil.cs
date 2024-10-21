namespace Boom
{
    using Boom;
    using Boom.Utility;
    using Boom.Values;
    using Candid;
    using Candid.Extv2Boom;
    using Candid.Extv2Standard;
    using Candid.IcrcLedger;
    using Candid.IcrcLedger.Models;
    using Candid.World;
    using Cysharp.Threading.Tasks;
    using EdjCase.ICP.Agent.Agents;
    using EdjCase.ICP.Candid.Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using static Env;

    public static class FetchUtil
    {
        public async static UniTask<UResult<Dictionary<string, Return>, string>> ProcessWorldCall<Return>(System.Func<WorldApiClient, string, UniTask<Return>> task, params string[] worldIds)
        {
            var agentResult = UserUtil.GetAgent();
            if (agentResult.IsErr) throw new(agentResult.AsErr());

            Dictionary<string, Return> responses = new();

            foreach (var wid in worldIds)
            {
                WorldApiClient worldApiClient = new(agentResult.AsOk(), Principal.FromText(wid));

                var response = await task(worldApiClient, wid);

                responses.TryAdd(wid, response);

            }

            return new(responses);
        }

        //ENTITY
        public async static UniTask<UResult<Dictionary<string, IEnumerable<DataTypes.Entity>>, string>> GetAllEntities(string worldId, params string[] uids)
        {
            var agentResult = UserUtil.GetAgent();
            if (agentResult.IsErr) throw new(agentResult.AsErr());

            WorldApiClient worldApiClient = new(agentResult.AsOk(), Principal.FromText(worldId));

            Dictionary<string, IEnumerable<DataTypes.Entity>> responses = new();
            List<UniTask> asyncFunctions = new();

            foreach (var principal in uids)
            {
                asyncFunctions.Add(FetchUserEntities(worldId, worldApiClient, responses, principal));
            }

            await UniTask.WhenAll(asyncFunctions);


            return new(responses);
        }

        private static async UniTask FetchUserEntities(string worldId, WorldApiClient worldApiClient, Dictionary<string, IEnumerable<DataTypes.Entity>> responses, string uid)
        {
            //responses.Add(uid, new List<DataTypes.Entity>());

            var response = await worldApiClient.GetAllUserEntities(new WorldApiClient.GetAllUserEntitiesArg0(new(), uid));

            if (response.Tag == Candid.World.Models.Result5Tag.Ok)
            {
                List<Candid.World.Models.StableEntity> userGameEntities = response.AsOk();

                responses.Add(uid, userGameEntities.Map(e => new DataTypes.Entity(worldId, e)));
            }
            else
            {
                $"DATA of type {nameof(DataTypes.Entity)} failed to load. Message: {response.AsErr()}".Warning(nameof(BoomManager));
            }
        }

        //ACTION STATE
        public async static UniTask<UResult<Dictionary<string, IEnumerable<DataTypes.ActionState>>, string>> GetAllActionState(string worldId, params string[] uids)
        {
            var agentResult = UserUtil.GetAgent();
            if (agentResult.IsErr) throw new(agentResult.AsErr());

            WorldApiClient worldApiClient = new(agentResult.AsOk(), Principal.FromText(worldId));

            Dictionary<string, IEnumerable<DataTypes.ActionState>> responses = new();
            List<UniTask> asyncFunctions = new();

            foreach (var principal in uids)
            {
                //Debug.Log("Load actionState from user of id: " + principal);

                asyncFunctions.Add(FetchUserActionStates(worldApiClient, responses, principal));
            }

            await UniTask.WhenAll(asyncFunctions);


            return new(responses);
        }

        private static async UniTask FetchUserActionStates(WorldApiClient worldApiClient, Dictionary<string, IEnumerable<DataTypes.ActionState>> responses, string uid)
        {
            var response = await worldApiClient.GetAllUserActionStates(new WorldApiClient.GetAllUserActionStatesArg0(uid));


            if (response.Tag == Candid.World.Models.Result7Tag.Ok)
            {
                List<Candid.World.Models.ActionState> userGameEntities = response.AsOk();

                responses.Add(uid, userGameEntities.Map(e => new DataTypes.ActionState(e)));
            }
            else
            {
                $"DATA of type{nameof(DataTypes.Token)} failed to load. Message: {response.AsErr()}".Warning(nameof(BoomManager));
            }
        }

        //TOKEN
        public static async UniTask<UResult<Dictionary<string, Dictionary<string, ulong>>, string>> GetAllTokens(string[] canisterIds, params string[] uids)
        {
            var agentResult = UserUtil.GetAgent();
            if (agentResult.IsErr) throw new(agentResult.AsErr());
            var agent = agentResult.AsOk();

            Dictionary<string, Dictionary<string, ulong>> responses = new(); //uid -> tokenCanisterId -> amount
            Dictionary<string, string> userAddresses = new(); //uid -> address

            List<UniTask> asyncFunctions = new();

            foreach (var uid in uids)
            {
                asyncFunctions.Add(GetUserAddress(uid, userAddresses));
            }

            await UniTask.WhenAll(asyncFunctions);

            asyncFunctions = new();

            foreach (var uid in uids)
            {
                var tokens = new Dictionary<string, ulong>();
                responses.Add(uid, tokens);

                foreach (var canisterId in canisterIds)
                {
                    asyncFunctions.Add(GetToken(uid, canisterId, userAddresses, tokens, agent));
                }
            }

            await UniTask.WhenAll(asyncFunctions);


            return new(responses);
        }

        private static async UniTask GetToken(string uid, string canisterId, Dictionary<string, string> userAddresses, Dictionary<string, ulong> tokens, IAgent agent)
        {
            var tokenInterface = new IcrcLedgerApiClient(agent, Principal.FromText(canisterId));

            if (canisterId == Env.CanisterIds.ICP_LEDGER)
            {
                var baseUnitAmount = await tokenInterface.Icrc1BalanceOf(new Account(Principal.FromText(uid), new()));

                baseUnitAmount.TryToUInt64(out var outVal);
                tokens.Add(canisterId, outVal);
            }
            else
            {
                var baseUnitAmount = await tokenInterface.Icrc1BalanceOf(new Candid.IcrcLedger.Models.Account(Principal.FromText(uid), new Candid.IcrcLedger.Models.Account.SubaccountInfo(new List<byte>())));
                baseUnitAmount.TryToUInt64(out ulong _baseUnitAmount);
                tokens.Add(canisterId, _baseUnitAmount);
            }
        }

        private static async UniTask GetUserAddress(string uid, Dictionary<string, string> userAddresses)
        {
            var address = await BoomManager.Instance.WorldHub.GetAccountIdentifier(uid);
            userAddresses.Add(uid, address);
        }

        //NFTs


        public static async UniTask<UResult<Dictionary<string, LinkedList<DataTypes.NftCollection>>, string>> GetAllNfts(string[] canisterIds, params string[] uids)
        {
            var agentResult = UserUtil.GetAgent();
            if (agentResult.IsErr) throw new(agentResult.AsErr());
            var agent = agentResult.AsOk();

            Dictionary<string, LinkedList<DataTypes.NftCollection>> responses = new(); //uid -> nftCanisterId -> Collection
            Dictionary<string, string> userAddresses = new(); //uid -> address

            List<UniTask> asyncFunctions = new();

            foreach (var uid in uids)
            {
                asyncFunctions.Add(GetUserAddress(uid, userAddresses));
            }

            await UniTask.WhenAll(asyncFunctions);

            asyncFunctions = new();

            foreach (var uid in uids)
            {
                var collections = new LinkedList<DataTypes.NftCollection>();
                responses.Add(uid, collections);

                foreach (var canisterId in canisterIds)
                {
                    if (ConfigUtil.TryGetNftCollectionConfig(canisterId, out var collectionConfig) == false)
                    {
                        return new($"Issue finding nft collection config of id: {canisterId}");
                    }

                    if (collectionConfig.isBoomDaoStandard) asyncFunctions.Add(FetchBoomDaoNfts(userAddresses[uid], agent, canisterId, collections));
                    else asyncFunctions.Add(FetchNfts(userAddresses[uid], agent, canisterId, collections));
                }
            }

            await UniTask.WhenAll(asyncFunctions);


            return new(responses);
        }


        #region ORIGINAL NFTs
        private static async UniTask<DataTypes.NftCollection> GetNfts(string userAddress, IAgent agent, string collectionId)
        {
            await UniTask.SwitchToMainThread();

            var api = new Extv2StandardApiClient(agent, Principal.FromText(collectionId));
            var registry = await api.GetRegistry(); // In the returned ValueTuple, the UInt32 is the token index, and the String is the address that owns it
            var collection = new DataTypes.NftCollection(collectionId);
            foreach (var value in registry)
            {
                if (string.Equals(value.F1, userAddress)) // Checks that the address that owns the NFT is same as your address
                {
                    var tokenIdentifier = await BoomManager.Instance.WorldHub.GetTokenIdentifier(collection.canisterId, value.F0);

                    collection.tokens.Add(
                        new(
                            collection.canisterId,
                            value.F0,
                            tokenIdentifier,
                            $"https://{collection.canisterId}.raw.icp0.io/?&tokenid={tokenIdentifier}&type=thumbnail",
                            ""));
                }
            }

            return collection;
        }

        private static async UniTask FetchNfts(string userAddress, IAgent agent, string collectionId, LinkedList<DataTypes.NftCollection> collections)
        {
            await UniTask.SwitchToMainThread();

            try
            {

                var collection = await GetNfts(userAddress, agent, collectionId) ?? throw new($"collection value of {nameof(FetchDataReq<DataTypeRequestArgs.NftCollection>)} is Null");
                collections.AddLast(collection);
#if UNITY_EDITOR
                //$"Standard Collections\nCount: {collection.tokens.Count}\nElements:  {collection.tokens.Reduce(k => $" - {k.index}", "\n")}".Log(nameof(CandidApiManager));
#endif
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
        #endregion

        #region BoomDao NFTs

        private static async UniTask<string> GetNftTokenIdentifier(DataTypes.NftCollection collection, uint index)
        {
            return await BoomManager.Instance.WorldHub.GetTokenIdentifier(collection.canisterId, index);
        }

        private static async UniTask<OptionalValue<Candid.Extv2Boom.Models.Metadata>> GetNftMetadata(Extv2BoomApiClient api, uint index)
        {
            return await api.GetTokenMetadata(index);
        }

        private static async UniTask GetPagedRegistry(string userAddress, Extv2BoomApiClient api, DataTypes.NftCollection collection, uint index)
        {
            await UniTask.SwitchToMainThread();

            try
            {
                var accountIdentifier = userAddress;

                var pagedRegistry = await api.GetPagedRegistry(index); // We used paged registries for Boom NFTs
                List<uint> indexes = new();
                List<UniTask<string>> asyncTokenIdFunctions = new();
                List<UniTask<OptionalValue<Candid.Extv2Boom.Models.Metadata>>> asyncMetadataFunctions = new();
                foreach (var value in pagedRegistry)
                {
                    if (string.Equals(value.F1,
                        accountIdentifier)) // Checks that the address that owns the NFT is same as your address
                    {
                        //Debug.Log($"-- Try fetch Token Metadata of index: {value.F0}");

                        indexes.Add(value.F0);
                        asyncMetadataFunctions.Add(GetNftMetadata(api, value.F0));
                        asyncTokenIdFunctions.Add(GetNftTokenIdentifier(collection, value.F0));
                    }
                }

                var metadataResults = await UniTask.WhenAll(asyncMetadataFunctions);
                var tokenIdResults = await UniTask.WhenAll(asyncTokenIdFunctions);

                if (metadataResults.Length != indexes.Count)
                {
                    Debug.LogError("Metadata array is not same length as NFT index array. Index length=" + indexes.Count + "  Metadata length=" + metadataResults.Length);
                    return;
                }

                if (tokenIdResults.Length != indexes.Count)
                {
                    Debug.LogError("Token id array is not same length as NFT index array. Index length=" + indexes.Count + "  Token id length=" + tokenIdResults.Length);
                    return;
                }

                for (int i = 0; i < indexes.Count; i++)
                {
                    string metadata = metadataResults[i].ValueOrDefault.AsNonfungible().Metadata.ValueOrDefault
                        .AsJson();
                    //Debug.Log($"-- Nft metadata fetched of index: {indexes[i]}");


                    collection.tokens.Add(
                        new(
                            collection.canisterId,
                            indexes[i],
                            tokenIdResults[i],
                            $"https://{collection.canisterId}.raw.icp0.io/?&tokenid={tokenIdResults[i]}&type=thumbnail",
                            metadata));
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        private static async UniTask<DataTypes.NftCollection> GetBoomDaoNfts(string userAddress, IAgent agent, string collectionId)
        {
            await UniTask.SwitchToMainThread();

            var api = new Extv2BoomApiClient(agent, Principal.FromText(collectionId));

            var result = await api.Supply(""); // Query the NFT supply so we can calculate the amount of pages in the NFT registry

            UnboundedUInt supply = (UnboundedUInt)result.Value;
            int pages = (int)supply / 10000;

            var collection = new DataTypes.NftCollection(collectionId);

            List<UniTask> asyncFunctions = new();
            for (uint i = 0; i <= pages; i++)
            {
                asyncFunctions.Add(GetPagedRegistry(userAddress, api, collection, i));
            }

            await UniTask.WhenAll(asyncFunctions);

            return collection;
        }

        private static async UniTask FetchBoomDaoNfts(string userAddress, IAgent agent, string collectionId, LinkedList<DataTypes.NftCollection> collections)
        {
            await UniTask.SwitchToMainThread();

            try
            {
                if (string.IsNullOrEmpty(collectionId))
                {
                    throw new($"collectionId value of {nameof(FetchDataReq<DataTypeRequestArgs.NftCollection>)} cannot be an empty string, specify some CollectionId");
                }

                var collection = await GetBoomDaoNfts(userAddress, agent, collectionId) ?? throw new($"collection value of {nameof(FetchDataReq<DataTypeRequestArgs.NftCollection>)} is Null");
                collections.AddLast(collection);

#if UNITY_EDITOR
                //$"BoomDao Collections\nCount: {collection.tokens.Count}\nElements: {collection.tokens.Reduce(k => $" - {k.index}", "\n")}".Log(nameof(CandidApiManager));
#endif
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
        #endregion


        //

        //ENTITY
        public async static UniTask<UResult<Dictionary<string, LinkedList<DataTypes.StakedNftCollections>>, string>> GetAllStakedNFTs(string worldId, params string[] uids)
        {
            var agentResult = UserUtil.GetAgent();
            if (agentResult.IsErr) throw new(agentResult.AsErr());
            var agent = agentResult.AsOk();

            WorldApiClient worldApiClient = new(agent, Principal.FromText(worldId));


            Dictionary<string, LinkedList<DataTypes.StakedNftCollections>> responses = new(); //uid -> nftCanisterId -> Collection
            Dictionary<string, string> userAddresses = new(); //uid -> address

            List<UniTask> asyncFunctions = new();

            foreach (var uid in uids)
            {
                asyncFunctions.Add(GetUserAddress(uid, userAddresses));

            }

            await UniTask.WhenAll(asyncFunctions);

            asyncFunctions = new();



            foreach (var uid in uids)
            {
                var collections = new LinkedList<DataTypes.StakedNftCollections>();
                responses.Add(uid, collections);

                asyncFunctions.Add(GetUserStakedNFTs(agent, worldApiClient, uid, collections));
            }

            await UniTask.WhenAll(asyncFunctions);

            return new(responses);
        }

        private static async UniTask GetUserStakedNFTs(IAgent agent, WorldApiClient worldApiClient, string uid, LinkedList<DataTypes.StakedNftCollections> stakedCollections)
        {
            //responses.Add(uid, new List<DataTypes.Entity>());

            var response = await worldApiClient.GetUserExtStakesInfo(uid);

            Dictionary<string, DataTypes.StakedNftCollections> collections = new();

            List<UniTask> asyncFunctions = new();

            foreach (var stake in response)
            {
                var key = stake.Key;
                var keySplit = key.Split('|');
                var nftCollectionID = keySplit[0];
                var nftIndexText = keySplit[1];

                nftIndexText.TryParseValue<uint>(out var nftIndex);

                if(collections.TryGetValue(nftCollectionID, out var collection) == false)
                {
                    collection = new(nftCollectionID);
                    collections.Add(nftCollectionID, collection);
                }

                asyncFunctions.Add(GetStakedNFTIdentifier(agent, nftCollectionID, nftIndex, stake.Value, collection));
            }

            await UniTask.WhenAll(asyncFunctions);

            collections.Iterate(e => stakedCollections.AddFirst(e.Value));
        }
        public static async UniTask GetStakedNFTIdentifier(IAgent agent, string collectionCanisterID, uint index, Candid.World.Models.EXTStake stake,DataTypes.StakedNftCollections collection)
        {
            var nftIdentifier = await BoomManager.Instance.WorldHub.GetTokenIdentifier(collectionCanisterID, index);
            //var api = new Extv2StandardApiClient(agent, Principal.FromText(collectionCanisterID));
      
            var nft = new DataTypes.StakedNftCollections.Nft(collectionCanisterID, index, nftIdentifier, $"https://{collection.canisterId}.raw.icp0.io/?&tokenid={nftIdentifier}&type=thumbnail", "");

            collection.tokens.Add(nft);

        }
    }
}