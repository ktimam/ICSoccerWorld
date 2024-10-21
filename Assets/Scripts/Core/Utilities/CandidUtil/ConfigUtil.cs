namespace Boom
{
    using Boom.Utility;
    using Boom.Values;
    using Candid;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public static class ConfigUtil
    {
        //TRY GET CONFGIS
        public static bool TryGetConfig(string worldId, string configId, out MainDataTypes.AllConfigs.Config outValue)
        {
            outValue = default;

            var result = UserUtil.GetMainData<MainDataTypes.AllConfigs>();


            if (result.IsErr)
            {
                result.AsErr().Error();
                return false;
            }

            var allConfigs = result.AsOk();

            if (allConfigs.configs.TryGetValue(worldId, out var worldConfigs) == false)
            {
                $"Could not find configs from world of id: \"{worldId}\"".Warning();
                return false;
            }

            if (worldConfigs.TryGetValue(configId, out outValue) == false)
            {
                $"Could not find config of id: \"{configId}\" in world of id: \"{worldId}\"".Warning();

                return false;
            }

            return result.IsOk;
        }
        public static bool TryGetConfig(string worldId, Predicate<MainDataTypes.AllConfigs.Config> predicate, out MainDataTypes.AllConfigs.Config outValue)
        {
            outValue = default;

            var result = UserUtil.GetMainData<MainDataTypes.AllConfigs>();


            if (result.IsErr)
            {
                Debug.LogError(result.AsErr());
                return false;
            }

            var allConfigs = result.AsOk();

            if (allConfigs.configs.TryGetValue(worldId, out var worldConfigs) == false)
            {
                Debug.LogError($"Could not find configs from world of id: {worldId}");
                return false;
            }


            foreach (var item in worldConfigs)
            {
                if (predicate(item.Value))
                {
                    outValue = item.Value;
                    return true;
                }
            }

            return false;
        }

        public static bool QueryConfigs(string worldId, Predicate<MainDataTypes.AllConfigs.Config> predicate, out LinkedList<MainDataTypes.AllConfigs.Config> outValue)
        {
            outValue = default;

            var configsResult = UserUtil.GetMainData<MainDataTypes.AllConfigs>();


            if (configsResult.IsErr)
            {
                Debug.LogError(configsResult.AsErr());
                return false;
            }

            var allConfigs = configsResult.AsOk();

            if (allConfigs.configs.TryGetValue(worldId, out var worldConfigs) == false)
            {
                Debug.LogError($"Could not find configs from world of id: {worldId}");
                return false;
            }

            outValue = new();

            foreach (var item in worldConfigs)
            {
                if (predicate(item.Value)) outValue.AddLast(item.Value);
            }

            return true;
        }
        public static bool QueryConfigsByTag(string worldId, string tag, out LinkedList<MainDataTypes.AllConfigs.Config> outValue)
        {
            return QueryConfigs(worldId, e =>
            {
                if (!e.fields.TryGetValue("tag", out var value)) return false;

                return value.Contains(tag);
            }, out outValue);
        }

        //

        public static bool TryGetConfig(this EntityOutcome entity, string worldId, out MainDataTypes.AllConfigs.Config entityConfig)
        {
            return TryGetConfig(worldId, entity.eid, out entityConfig);
        }
        public static bool TryGetConfig(this DataTypes.Entity entity, string worldId, out MainDataTypes.AllConfigs.Config entityConfig)
        {
            return TryGetConfig(worldId, entity.eid, out entityConfig);
        }
        public static bool TryGetConfig(this EntityConstrainTypes.Base entity, string worldId, out MainDataTypes.AllConfigs.Config entityConfig)
        {
            return TryGetConfig(worldId, entity.Eid, out entityConfig);
        }

        public static bool TryGetConfigFieldAs<T>(string worldId, string configId, string fieldName, out T outValue, T defaultValue = default)
        {
            outValue = defaultValue;

            if (!TryGetConfig(worldId, configId, out var config))
            {
                return false;
            }

            if (!config.fields.TryGetValue(fieldName, out var value))
            {
                $"Failure to find in config of id: \"{configId}\" a field of name \"{fieldName}\"".Warning();

                return false;
            }

            if (value.TryParseValue<T>(out outValue) == false)
            {
                $"Failure to parse config field of id: \"{configId}\" to \"{typeof(T).Name}\"".Warning();

                return false;
            }

            return true;
        }
        public static bool TryGetConfigFieldAs<T>(this DataTypes.Entity entity, string fieldName, out T outValue, T defaultValue = default)
        {
            return TryGetConfigFieldAs(entity.wid, entity.eid, fieldName, out outValue, defaultValue);
        }
        public static bool TryGetConfigFieldAs<T>(this EntityOutcome newEntityValues, string worldId, string fieldName, out T outValue, T defaultValue = default)
        {
            return TryGetConfigFieldAs(worldId, newEntityValues.eid, fieldName, out outValue, defaultValue);
        }
        public static bool TryGetConfigFieldAs<T>(this MainDataTypes.AllConfigs.Config config, string fieldName, out T outValue, T defaultValue = default)
        {
            outValue = defaultValue;
            if (!config.fields.TryGetValue(fieldName, out var value)) return false;

            if (value.TryParseValue<T>(out outValue))
            {
                return true;
            }
            return false;
        }

        //ACTIONS
        public static bool TryGetAction(string actionId, out MainDataTypes.AllAction.Action outValue, string worldId = "")
        {
            outValue = default;

            if (string.IsNullOrEmpty(worldId)) worldId = BoomManager.Instance.WORLD_CANISTER_ID;

            var result = UserUtil.GetMainData<MainDataTypes.AllAction>();


            if (result.IsErr)
            {
                result.AsErr().Error();
                return false;
            }

            var allConfigs = result.AsOk();

            if (allConfigs.actions.TryGetValue(worldId, out var worldActions) == false)
            {
                $"Could not find configs from world of id: {worldId}".Warning();
                return false;
            }

            if (worldActions.TryGetValue(actionId, out outValue) == false)
            {
                $"Could not find config of id: {actionId} in world of id: {worldId}".Warning();
                return false;
            }

            return true;
        }


        public static bool TryGetSubAction(this string actionId, out SubAction outValue, DataSourceType dataSourceType, string worldId = "")
        {
            outValue = default;

            if (!ConfigUtil.TryGetAction(actionId, out var action, worldId))
            {
                Debug.LogError("Could not find action of id: " + actionId);

                return false;
            }

            try
            {
                outValue = dataSourceType switch
                {
                    DataSourceType.Caller => action.callerAction,
                    DataSourceType.Target => action.targetAction,
                    DataSourceType.World => action.worldAction,
                    _ => throw new Exception("Unmatch")
                };
            }
            catch (Exception e)
            {
                e.Message.Error();
                return false;
            }

            return true;
        }

        public static bool TryGetActionOutcomeUpdateEntity(this string actionId, out List<Outcome> outValue, DataSourceType dataSourceType, string worldId = "")
        {
            outValue = default;

            if (!ConfigUtil.TryGetSubAction(actionId, out var subAction, dataSourceType, worldId))
            {
                Debug.LogError("Could not find action of id: " + actionId);

                return false;
            }

            outValue = subAction.Outcomes;

            return true;
        }

        public static bool TryGetFirstActionEntityOutcomeFieldValue<T>(string actionId, string entityId, string fieldId, out T outValue, DataSourceType dataSourceType = DataSourceType.Caller) where T : EntityFieldEdit.Base
        {
            outValue = null;

            if (TryGetSubAction(actionId, out var subAction, dataSourceType) == false)
            {
                "Could not find subaction".Error();

                return false;
            }

            var gacha = subAction.Outcomes;

            if (gacha == null)
            {
                "There is no outcomes".Error();
                return false;
            }

            foreach (var roll in gacha)
            {
                foreach (var item in roll.PossibleOutcomes)
                {
                    if (item.possibleOutcomeType == Candid.World.Models.ActionOutcomeOption.OptionInfoTag.UpdateEntity)
                    {
                        var asEntity = item as PossibleOutcomeTypes.UpdateEntity;

                        if (asEntity.Eid == entityId)
                        {
                            foreach (var field in asEntity.Fields)
                            {
                                if (field.Key == fieldId)
                                {
                                    if (field.Value is T t)
                                    {
                                        outValue = t;
                                        return true;
                                    }
                                    else
                                    {
                                        $"EntityOutcome casted to the wrong type: {typeof(T).Name}, current type is: {field.Value.GetType().Name}".Error();
                                    }
                                }
                            }
                        }

                    }
                }
            }

            return false;
        }

        public static bool TryGetActionPart<T>(this string actionId, Func<MainDataTypes.AllAction.Action, T> func, out T outValue, string worldId = "")
        {
            outValue = default;

            if (!ConfigUtil.TryGetAction(actionId, out var action, worldId))
            {
                Debug.LogError("Could not find action of id: " + actionId);

                return false;
            }

            try
            {
                outValue = func(action);
            }
            catch
            {
                return false;
            }

            return true;
        }

        //TOKENS
        public static UResult<LinkedList<MainDataTypes.AllTokenConfigs.TokenConfig>, string> GetAllTokenConfigs()
        {
            var result = UserUtil.GetMainData<MainDataTypes.AllTokenConfigs>();

            if (result.IsErr)
            {
                return new(result.AsErr());
            }

            var allConfigs = result.AsOk();

            LinkedList<MainDataTypes.AllTokenConfigs.TokenConfig> configs = new();

            foreach (var tokenConfig in allConfigs.configs)
            {
                configs.AddLast(tokenConfig.Value);
            }

            return new(configs);
        }
        public static bool TryGetTokenConfig(string canisterId, out MainDataTypes.AllTokenConfigs.TokenConfig outValue)
        {
            outValue = default;

            var result = UserUtil.GetMainData<MainDataTypes.AllTokenConfigs>();

            if (result.IsErr)
            {
                Debug.LogError(result.AsErr());
                return false;
            }

            var allConfigs = result.AsOk();

            if (allConfigs.configs.TryGetValue(canisterId, out outValue) == false)
            {
                Debug.LogError($"Could not find token configs for canister id: {canisterId}");
                return false;
            }

            return true;
        }
        public static bool TryGetTokenConfig(this DataTypes.Token token, out MainDataTypes.AllTokenConfigs.TokenConfig outValue)
        {
            return TryGetTokenConfig(token.canisterId, out outValue);
        }
        //NFTS
        public static UResult<LinkedList<MainDataTypes.AllNftCollectionConfig.NftConfig>, string> GetAllNftConfigs()
        {
            var result = UserUtil.GetMainData<MainDataTypes.AllNftCollectionConfig>();

            if (result.IsErr)
            {
                return new(result.AsErr());
            }

            var allConfigs = result.AsOk();

            LinkedList<MainDataTypes.AllNftCollectionConfig.NftConfig> configs = new();

            foreach (var tokenConfig in allConfigs.configs)
            {
                configs.AddLast(tokenConfig.Value);
            }

            return new(configs);
        }

        public static bool TryGetNftCollectionConfig(string canisterId, out MainDataTypes.AllNftCollectionConfig.NftConfig outValue)
        {
            outValue = default;

            var result = UserUtil.GetMainData<MainDataTypes.AllNftCollectionConfig>();

            if (result.IsErr)
            {
                Debug.LogError(result.AsErr());
                return false;
            }

            var allConfigs = result.AsOk();

            if (allConfigs.configs.TryGetValue(canisterId, out outValue) == false)
            {
                Debug.LogError($"Could not find token configs for canister id: {canisterId}");
                return false;
            }

            return true;
        }

        public static bool TryGetNftCollectionConfig(this DataTypes.NftCollection collection, out MainDataTypes.AllNftCollectionConfig.NftConfig outValue)
        {
            return TryGetNftCollectionConfig(collection.canisterId, out outValue);
        }

    }
}