namespace Boom
{
    using Cysharp.Threading.Tasks;
    using Boom.Values;
    using System.Collections.Generic;
    using Candid.World.Models;
    using EdjCase.ICP.Candid.Models;
    using System.Linq;
    using Boom.Utility;
    using Candid.IcrcLedger;
    using Newtonsoft.Json;
    using Boom.Patterns.Broadcasts;
    using Candid.Extv2Boom;
    using static Boom.ProcessedActionResponse;


    //TRANSFER ERROR TYPES
    public static class TransferErrType
    {
        public abstract class Base
        {
            public string content;
            public string Content { get { return $"{GetType()}: {content}"; } }

            protected Base(string content)
            {
                this.content = content;
            }
        }
        public class LogIn : Base
        {
            public LogIn(string msg) : base(msg)
            {

            }
        }
        public class InsufficientBalance : Base
        {
            public InsufficientBalance(string msg) : base(msg)
            {

            }
        }
        public class Transfer : Base
        {
            public Transfer(string msg) : base(msg)
            {

            }
        }
        public class Other : Base
        {
            public Other(string msg) : base(msg)
            {

            }
        }
    }
    //ACTION ERROR TYPES
    public static class ActionErrType
    {
        public abstract class Base
        {
            public string content;
            public string Content { get { return $"{GetType()}: {content}"; } }

            protected Base(string content)
            {
                this.content = content;
            }
        }

        public class LogIn : Base
        {
            public LogIn(string msg) : base(msg)
            {

            }
        }

        //ACTION CONSTRAINTS
        public class ActionExecutionFailure : Base
        {
            public ActionExecutionFailure(string msg) : base(msg)
            {
            }
        }
        public class ActionsPerInterval : Base
        {
            public ActionsPerInterval(string msg) : base(msg)
            {
            }
        }
        public class EntityConstrain : Base
        {
            public EntityConstrain(string msg) : base(msg)
            {
            }
        }
        public class WrongActionType : Base
        {
            public WrongActionType(string content) : base(content)
            {
            }
        }

        //BALANCE
        public class InsufficientBalance : Base
        {
            public InsufficientBalance(string msg) : base(msg)
            {

            }
        }

        //TRANSFER
        public class Transfer : Base
        {
            public Transfer(string msg) : base(msg)
            {

            }
        }

        //OTHER
        public class Other : Base
        {
            public Other(string msg) : base(msg)
            {
            }
        }
    }

    //PROCESS ACTION RESULT
    public class ProcessedActionResponse
    {
        public class Outcomes
        {
            public string uid;
            public Dictionary<string, EntityOutcome> entityOutcomes = new();
            public List<MintNft> nfts;
            public List<TransferIcrc> tokens;

            public Outcomes(string uid, string callerPrincipalId, string optionalTargetPrincipalId, List<ActionOutcomeOption> outcomes, IEnumerable<DataTypes.Entity> worldEntities, IEnumerable<DataTypes.Entity> callerEntities, IEnumerable<DataTypes.Entity> targetEntities, IEnumerable<MainDataTypes.AllConfigs.Config> configs, IEnumerable<Field> args)
            {
                this.uid = uid;
                this.entityOutcomes = new();
                this.nfts = new();
                this.tokens = new();

                outcomes.Iterate(e =>
                {
                    switch (e.Option.Tag)
                    {
                        case ActionOutcomeOption.OptionInfoTag.UpdateEntity:

                            string fieldName;
                            Dictionary<string, EntityFieldEdit.Base> allFieldsToEdit;

                            var updateEntity =  e.Option.AsUpdateEntity();

                            string wid = string.IsNullOrEmpty(updateEntity.Wid.ValueOrDefault) == false? updateEntity.Wid.ValueOrDefault : BoomManager.Instance.WORLD_CANISTER_ID;
                            string eid = updateEntity.Eid;

                            if (eid.Contains("$caller"))
                            {
                                eid = callerPrincipalId;
                            }
                            else if (eid.Contains("$target"))
                            {
                                eid = optionalTargetPrincipalId;
                            }
                            else if (eid.Contains("$args"))
                            {
                                var splitedUid = eid.Split('.');

                                if (splitedUid.Length != 2)
                                {
                                    "Issue assigning an argument as an entity id".Error();
                                    return;
                                }

                                var argFieldName = splitedUid[1].TrimEnd('}');

                                foreach (var arg in args)
                                {
                                    if (arg.FieldName == argFieldName)
                                    {
                                        eid = EntityUtil.ReplaceVariables(arg.FieldValue, worldEntities, callerEntities, targetEntities, configs, args);
                                        break;
                                    }
                                }
                            }


                            var entityKey = $"{wid}{eid}";

                            if (updateEntity.Updates.Count > 0)
                            {
                                foreach (var update in updateEntity.Updates)
                                {
                                    switch (update.Tag)
                                    {
                                        case UpdateEntityTypeTag.DeleteField:

                                            var val00 = update.AsDeleteField();
                                            fieldName = val00.FieldName;

                                            if (fieldName == "$caller") fieldName = callerPrincipalId;
                                            else if (fieldName == "$target") fieldName = optionalTargetPrincipalId;

                                            if (!entityOutcomes.TryGetValue(entityKey, out var entityToEdit00))
                                            {
                                                entityToEdit00 = new(wid, eid, new());
                                                entityOutcomes.Add(entityKey, entityToEdit00);
                                            }

                                            allFieldsToEdit = entityToEdit00.fields;


                                            if (allFieldsToEdit != null)
                                            {
                                                if (!allFieldsToEdit.TryAdd(fieldName, new EntityFieldEdit.DeleteField()))
                                                {
                                                    allFieldsToEdit[fieldName] = new EntityFieldEdit.DeleteField();
                                                }
                                            }

                                            break;
                                        case UpdateEntityTypeTag.SetText:

                                            var val1 = update.AsSetText();
                                            fieldName = val1.FieldName;
                                            var fieldValue1 = val1.FieldValue;

                                            if (fieldName == "$caller") fieldName = callerPrincipalId;
                                            else if (fieldName == "$target") fieldName = optionalTargetPrincipalId;

                                            if (!entityOutcomes.TryGetValue(entityKey, out var entityToEdit1))
                                            {
                                                entityToEdit1 = new(wid, eid, new());
                                                entityOutcomes.Add(entityKey, entityToEdit1);
                                            }

                                            allFieldsToEdit = entityToEdit1.fields;


                                            if (allFieldsToEdit != null)
                                            {
                                                fieldValue1 = EntityUtil.ReplaceVariables(fieldValue1, worldEntities, callerEntities, targetEntities, configs, args);

                                                if (!allFieldsToEdit.TryAdd(fieldName, new EntityFieldEdit.SetText((string)fieldValue1)))
                                                {
                                                    allFieldsToEdit[fieldName] = new EntityFieldEdit.SetText((string)fieldValue1);
                                                }
                                            }

                                            break;
                                        case UpdateEntityTypeTag.AddToList:

                                            var val10 = update.AsAddToList();
                                            fieldName = val10.FieldName;
                                            var fieldValue10 = val10.Value;

                                            if (fieldName == "$caller") fieldName = callerPrincipalId;
                                            else if (fieldName == "$target") fieldName = optionalTargetPrincipalId;

                                            if (fieldValue10 == "$caller") fieldValue10 = callerPrincipalId;
                                            else if (fieldValue10 == "$target") fieldValue10 = optionalTargetPrincipalId;

                                            if (!entityOutcomes.TryGetValue(entityKey, out var entityToEdit10))
                                            {
                                                entityToEdit10 = new(wid, eid, new());
                                                entityOutcomes.Add(entityKey, entityToEdit10);
                                            }

                                            allFieldsToEdit = entityToEdit10.fields;


                                            if (allFieldsToEdit != null)
                                            {
                                                fieldValue10 = EntityUtil.ReplaceVariables(fieldValue10, worldEntities, callerEntities, targetEntities, configs, args);

                                                if (!allFieldsToEdit.TryAdd(fieldName, new EntityFieldEdit.AddToList((string)fieldValue10)))
                                                {
                                                    allFieldsToEdit[fieldName] = new EntityFieldEdit.AddToList((string)fieldValue10);
                                                }
                                            }

                                            break;
                                        case UpdateEntityTypeTag.RemoveFromList:

                                            var val11 = update.AsRemoveFromList();
                                            fieldName = val11.FieldName;
                                            var fieldValue11 = val11.Value;

                                            if (fieldName == "$caller") fieldName = callerPrincipalId;
                                            else if (fieldName == "$target") fieldName = optionalTargetPrincipalId;

                                            if (fieldValue11 == "$caller") fieldValue11 = callerPrincipalId;
                                            else if (fieldValue11 == "$target") fieldValue11 = optionalTargetPrincipalId;

                                            if (!entityOutcomes.TryGetValue(entityKey, out var entityToEdit11))
                                            {
                                                entityToEdit11 = new(wid, eid, new());
                                                entityOutcomes.Add(entityKey, entityToEdit11);
                                            }

                                            if (entityToEdit11 != null)
                                            {
                                                allFieldsToEdit = entityToEdit11.fields;


                                                if (allFieldsToEdit != null)
                                                {
                                                    fieldValue11 = EntityUtil.ReplaceVariables(fieldValue11, worldEntities, callerEntities, targetEntities, configs, args);

                                                    if (!allFieldsToEdit.TryAdd(fieldName, new EntityFieldEdit.RemoveFromList((string)fieldValue11)))
                                                    {
                                                        UnityEngine.Debug.Log($"Remove E");

                                                        allFieldsToEdit[fieldName] = new EntityFieldEdit.RemoveFromList((string)fieldValue11);
                                                    }
                                                }
                                            }

                                            break;
                                        case UpdateEntityTypeTag.SetNumber:

                                            var val2 = update.AsSetNumber();

                                            fieldName = val2.FieldName;
                                            var fieldValue2 = val2.FieldValue;

                                            if (fieldName == "$caller") fieldName = callerPrincipalId;
                                            else if (fieldName == "$target") fieldName = optionalTargetPrincipalId;

                                            if (!entityOutcomes.TryGetValue(entityKey, out var entityToEdit2))
                                            {
                                                entityToEdit2 = new(wid, eid, new());
                                                entityOutcomes.Add(entityKey, entityToEdit2);
                                            }

                                            allFieldsToEdit = entityToEdit2.fields;

                                            if (allFieldsToEdit != null)
                                            {
                                                if (fieldValue2.Tag == Candid.World.Models.SetNumber.FieldValueInfoTag.Number)
                                                {
                                                    var v = new EntityFieldEdit.Numeric(fieldValue2.AsNumber(), EntityFieldEdit.Numeric.NumericType.Set);
                                                    if (!allFieldsToEdit.TryAdd(fieldName, v))
                                                    {
                                                        var currentValue = allFieldsToEdit[fieldName];
                                                        if (currentValue is EntityFieldEdit.Numeric currentNumericValue)
                                                        {
                                                            currentNumericValue.EditNumericValue(v.Value, EntityFieldEdit.Numeric.NumericType.Set);
                                                        }
                                                        else
                                                        {
                                                            $"Something went wrong setting up outcomes".Error(typeof(ActionUtil).Name);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    var formula = fieldValue2.AsFormula();
                                                    //TODO: Parse formula
                                                    var formulaResult = EntityUtil.EvaluateFormula(formula, worldEntities, callerEntities, targetEntities, configs, args);
                                                    var v = new EntityFieldEdit.Numeric(formulaResult, EntityFieldEdit.Numeric.NumericType.Set);
                                                    if (!allFieldsToEdit.TryAdd(fieldName, v))
                                                    {
                                                        var currentValue = allFieldsToEdit[fieldName];
                                                        if (currentValue is EntityFieldEdit.Numeric currentNumericValue)
                                                        {
                                                            currentNumericValue.EditNumericValue(v.Value, EntityFieldEdit.Numeric.NumericType.Set);
                                                        }
                                                        else
                                                        {
                                                            $"Something went wrong setting up outcomes".Error(typeof(ActionUtil).Name);
                                                        }
                                                    }
                                                }
                                            }

                                            break;
                                        case UpdateEntityTypeTag.IncrementNumber:

                                            var val3 = update.AsIncrementNumber();

                                            fieldName = val3.FieldName;
                                            var fieldValue3 = val3.FieldValue;

                                            if (fieldName == "$caller") fieldName = callerPrincipalId;
                                            else if (fieldName == "$target") fieldName = optionalTargetPrincipalId;

                                            if (!entityOutcomes.TryGetValue(entityKey, out var entityToEdit3))
                                            {
                                                entityToEdit3 = new(wid, eid, new());
                                                entityOutcomes.Add(entityKey, entityToEdit3);
                                            }

                                            allFieldsToEdit = entityToEdit3.fields;

                                            if (allFieldsToEdit != null)
                                            {
                                                if (fieldValue3.Tag == Candid.World.Models.IncrementNumber.FieldValueInfoTag.Number)
                                                {
                                                    var v = new EntityFieldEdit.Numeric(fieldValue3.AsNumber(), EntityFieldEdit.Numeric.NumericType.Increment);
                                                    if (!allFieldsToEdit.TryAdd(fieldName, v))
                                                    {
                                                        var currentValue = allFieldsToEdit[fieldName];
                                                        if (currentValue is EntityFieldEdit.Numeric currentNumericValue)
                                                        {
                                                            currentNumericValue.EditNumericValue(v.Value, EntityFieldEdit.Numeric.NumericType.Increment);
                                                        }
                                                        else
                                                        {
                                                            $"Something went wrong setting up increment outcome".Error(typeof(ActionUtil).Name);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    var formula = fieldValue3.AsFormula();

                                                    var formulaResult = EntityUtil.EvaluateFormula(formula, worldEntities, callerEntities, targetEntities, configs, args);
                                                    var v = new EntityFieldEdit.Numeric(formulaResult, EntityFieldEdit.Numeric.NumericType.Increment);
                                                    if (!allFieldsToEdit.TryAdd(fieldName, v))
                                                    {
                                                        var currentValue = allFieldsToEdit[fieldName];
                                                        if (currentValue is EntityFieldEdit.Numeric currentNumericValue)
                                                        {
                                                            currentNumericValue.EditNumericValue(v.Value, EntityFieldEdit.Numeric.NumericType.Increment);
                                                        }
                                                        else
                                                        {
                                                            $"Something went wrong setting up increment outcome".Error(typeof(ActionUtil).Name);
                                                        }
                                                    }
                                                }
                                            }

                                            break;
                                        case UpdateEntityTypeTag.DecrementNumber:

                                            var val4 = update.AsDecrementNumber();

                                            fieldName = val4.FieldName;
                                            var fieldValue4 = val4.FieldValue;

                                            if (fieldName == "$caller") fieldName = callerPrincipalId;
                                            else if (fieldName == "$target") fieldName = optionalTargetPrincipalId;

                                            if (!entityOutcomes.TryGetValue(entityKey, out var entityToEdit4))
                                            {
                                                entityToEdit4 = new(wid, eid, new());
                                                entityOutcomes.Add(entityKey, entityToEdit4);
                                            }

                                            allFieldsToEdit = entityToEdit4.fields;

                                            if (allFieldsToEdit != null)
                                            {
                                                if (fieldValue4.Tag == Candid.World.Models.DecrementNumber.FieldValueInfoTag.Number)
                                                {
                                                    var v = new EntityFieldEdit.Numeric(fieldValue4.AsNumber(), EntityFieldEdit.Numeric.NumericType.Decrement);
                                                    if (!allFieldsToEdit.TryAdd(fieldName, v))
                                                    {
                                                        var currentValue = allFieldsToEdit[fieldName];
                                                        if (currentValue is EntityFieldEdit.Numeric currentNumericValue)
                                                        {
                                                            currentNumericValue.EditNumericValue(v.Value, EntityFieldEdit.Numeric.NumericType.Increment);
                                                        }
                                                        else
                                                        {
                                                            $"Something went wrong setting up decrement outcome".Error(typeof(ActionUtil).Name);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    var formula = fieldValue4.AsFormula();

                                                    var formulaResult = EntityUtil.EvaluateFormula(formula, worldEntities, callerEntities, targetEntities, configs, args);
                                                    var v = new EntityFieldEdit.Numeric(formulaResult, EntityFieldEdit.Numeric.NumericType.Decrement);
                                                    if (!allFieldsToEdit.TryAdd(fieldName, v))
                                                    {
                                                        var currentValue = allFieldsToEdit[fieldName];
                                                        if (currentValue is EntityFieldEdit.Numeric currentNumericValue)
                                                        {
                                                            currentNumericValue.EditNumericValue(v.Value, EntityFieldEdit.Numeric.NumericType.Increment);
                                                        }
                                                        else
                                                        {
                                                            $"Something went wrong setting up decrement outcome".Error(typeof(ActionUtil).Name);
                                                        }
                                                    }
                                                }
                                            }

                                            break;
                                        case UpdateEntityTypeTag.RenewTimestamp:

                                            var val5 = update.AsRenewTimestamp();

                                            fieldName = val5.FieldName;
                                            var fieldValue5 = val5.FieldValue;

                                            if (fieldName == "$caller") fieldName = callerPrincipalId;
                                            else if (fieldName == "$target") fieldName = optionalTargetPrincipalId;

                                            if (!entityOutcomes.TryGetValue(entityKey, out var entityToEdit5))
                                            {
                                                entityToEdit5 = new(wid, eid, new());
                                                entityOutcomes.Add(entityKey, entityToEdit5);
                                            }

                                            allFieldsToEdit = entityToEdit5.fields;

                                            if (allFieldsToEdit != null)
                                            {
                                                if (fieldValue5.Tag == Candid.World.Models.RenewTimestamp.FieldValueInfoTag.Number)
                                                {
                                                    var v = new EntityFieldEdit.Numeric(fieldValue5.AsNumber(), EntityFieldEdit.Numeric.NumericType.RenewTimestamp);

                                                    if (!allFieldsToEdit.TryAdd(fieldName, v))
                                                    {
                                                        (allFieldsToEdit[fieldName] as EntityFieldEdit.Numeric).EditNumericValue(fieldValue5.AsNumber(), EntityFieldEdit.Numeric.NumericType.RenewTimestamp);
                                                    }
                                                }
                                                else
                                                {
                                                    var formula = fieldValue5.AsFormula();
                                                    //TODO: Parse formula
                                                    var formulaResult = EntityUtil.EvaluateFormula(formula, worldEntities, callerEntities, targetEntities, configs, args);
                                                    var v = new EntityFieldEdit.Numeric(formulaResult, EntityFieldEdit.Numeric.NumericType.RenewTimestamp);

                                                    if (!allFieldsToEdit.TryAdd(fieldName, v))
                                                    {
                                                        (allFieldsToEdit[fieldName] as EntityFieldEdit.Numeric).EditNumericValue(formulaResult, EntityFieldEdit.Numeric.NumericType.RenewTimestamp);
                                                    }
                                                }
                                            }

                                            break;
                                        case UpdateEntityTypeTag.DeleteEntity:

                                            var val6 = update.AsDeleteEntity();

                                            if (entityOutcomes.TryGetValue(entityKey, out var entityToEdit6))
                                            {
                                                entityToEdit6.fields.Clear();
                                            }

                                            EntityOutcome entityToDelete = new(wid, eid, null);
                                            entityOutcomes[entityKey] = entityToDelete;

                                            goto entityLoop;
                                    }
                                }
                            }
                            else
                            {
                                EntityOutcome newEntity = new(wid, eid, new());
                                entityOutcomes.Add(entityKey, newEntity);
                            }


                            break;
                        case ActionOutcomeOption.OptionInfoTag.TransferIcrc:

                            tokens.Add(e.Option.AsTransferIcrc());
                            break;
                        case ActionOutcomeOption.OptionInfoTag.MintNft:

                            nfts.Add(e.Option.AsMintNft());
                            break;
                    }

                    entityLoop: { }
                });
            }
        }

        public Outcomes callerOutcomes;
        public Outcomes targetOutcomes;
        public Outcomes worldOutcomes;

        public bool hasTarget;

        public ProcessedActionResponse(ActionReturn actionReturn, IEnumerable<DataTypes.Entity> worldEntities, IEnumerable<DataTypes.Entity> callerEntities, IEnumerable<DataTypes.Entity> targetEntities, IEnumerable<MainDataTypes.AllConfigs.Config> configs, IEnumerable<Field> args)
        {
            var callerOutcomeResult = actionReturn.CallerOutcomes;
            var targetOutcomeResult = actionReturn.TargetOutcomes;
            var worldOutcomeResult = actionReturn.WorldOutcomes;

            hasTarget = !string.IsNullOrEmpty(actionReturn.TargetPrincipalId);

            callerOutcomes = new(actionReturn.CallerPrincipalId, actionReturn.CallerPrincipalId, actionReturn.TargetPrincipalId, callerOutcomeResult, worldEntities, callerEntities, targetEntities, configs, args);

            targetOutcomes = new(actionReturn.TargetPrincipalId, actionReturn.CallerPrincipalId, actionReturn.TargetPrincipalId, targetOutcomeResult, worldEntities, callerEntities, targetEntities, configs, args);

            worldOutcomes = new(actionReturn.WorldPrincipalId, actionReturn.CallerPrincipalId, actionReturn.TargetPrincipalId, worldOutcomeResult, worldEntities, callerEntities, targetEntities, configs, args);
        }
    }//UTILS

    public static class ActionUtil
    {
        public static async UniTask<UResult<bool, string>> CanClaimGuildQuest(string questActionId)
        {
            if (UserUtil.IsLoggedIn(out var loginData) == false)
            {
                $"User is not yet logged in!".Warning(typeof(ActionUtil).Name);

                return new("User is not yet logged in!");
            }


            var result = await BoomManager.Instance.GuildApiClient.GetActionStatusComposite(new Candid.World.WorldApiClient.GetActionStatusCompositeArg0(questActionId, loginData.principal));


            if (result.Tag == Result8Tag.Err) return new(result.AsErr());

            return new(result.AsOk().IsValid);
        }
        public static bool ActionsInProcess(params string[] actionIds)
        {
            foreach (var actionDependency in actionIds)
            {
                if (BroadcastState.TryRead(out ActionExecutionState state, actionDependency))
                {
                    if (state.inProcess)
                    {

                        return true;
                    }
                }
            }

            return false;
        }

        //Validate Entity Constraint
        public static bool TryGetTriesDetails(string actionId, out (ulong maxTries, ulong triesLeft) details)
        {
            details = (0, 0);

            var allActionConfigResult = UserUtil.GetMainData<MainDataTypes.AllAction>();

            if (allActionConfigResult.IsErr)
            {
                $"{allActionConfigResult.AsErr()}".Error();
                return false;
            }
            var allActionConfigAsOk = allActionConfigResult.AsOk();

            if (allActionConfigAsOk.actions.TryGetValue(BoomManager.Instance.WORLD_CANISTER_ID, out var worldActions) == false)
            {
                $"Could not find world's actions of world id: {BoomManager.Instance.WORLD_CANISTER_ID}".Error();
                return false;
            }

            if (worldActions.TryGetValue(actionId, out var action) == false)
            {
                $"Could not find action of id: {actionId}".Error();
                return false;
            }

            var subAction = action.callerAction;

            if (subAction == null)
            {
                details = (1, 1);
                return true;
            }

            if (subAction.TimeConstraint == null)
            {
                details = (1, 1);
                return true;
            }

            if (subAction.TimeConstraint.actionTimeInterval == null)
            {
                details = (1, 1);
                return true;
            }

            var actionsPerInterval = subAction.TimeConstraint.actionTimeInterval.ActionsPerInterval;

            var actionStateResult = UserUtil.GetElementOfTypeSelf<DataTypes.ActionState>(actionId);

            if (actionStateResult.IsErr)
            {
                details = (actionsPerInterval, actionsPerInterval);

                return true;
            }

            var actionStateAsOk = actionStateResult.AsOk();

            var intervalStartTs = actionStateAsOk.intervalStartTs;
            var actionCount = actionStateAsOk.actionCount;

            if (actionsPerInterval == 0)
            {
                return false;
            }

            var intervalDuration = subAction.TimeConstraint.actionTimeInterval.IntervalDuration;

            var timeConstrainToCompareWith = intervalStartTs.NanoToMilliseconds() + intervalDuration.NanoToMilliseconds();

            if (timeConstrainToCompareWith < MainUtil.Now())
            {
                details = (actionsPerInterval, actionsPerInterval);

                return true;
            }
            else if (actionCount < actionsPerInterval)
            {
                details = (actionsPerInterval, actionsPerInterval - actionCount);
                return true;
            }
            else
            {
                details = (actionsPerInterval, 0);
                return true;
            }
        }

        private static UResult<Null, string> ValidateConstraint(string actionId, bool tryIncrementActionExecutionCount, string optionalTargetPrincipalId = "")
        {

            if(UserUtil.IsLoggedIn(out var loginData) == false)
            {
                $"User is not yet logged in!".Warning(typeof(ActionUtil).Name);

                return new("User is not yet logged in!");
            }

            var userPrincipaId = loginData.principal;


            LinkedList<KeyValue<string, SubAction>> subActions = new();


            if (!ConfigUtil.TryGetAction(actionId, out var action, BoomManager.Instance.WORLD_CANISTER_ID))
            {
                $"Could not find action of id: {actionId}".Error();

                return new($"Could not find action of id: {actionId}");
            }

            if (action.callerAction != null) subActions.AddLast(new KeyValue<string, SubAction>(userPrincipaId, action.callerAction));

            if (action.worldAction != null) subActions.AddLast(new KeyValue<string, SubAction>(BoomManager.Instance.WORLD_CANISTER_ID, action.worldAction));

            if (action.targetAction != null)
            {
                if (string.IsNullOrEmpty(optionalTargetPrincipalId))
                {
                    $"Action of ID: {actionId}, has target subaction, you are not specifying the optionalTargetPrincipalId".Error(typeof(ActionUtil).Name);
                    return new("Action of ID: {actionId}, has target subaction, you are not specifying the optionalTargetPrincipalId");
                }

                subActions.AddLast(new KeyValue<string, SubAction>(optionalTargetPrincipalId, action.targetAction));
            }

            var runner = subActions.First;

            while(runner != null)
            {
                var principalId = runner.Value.key;
                var subAction = runner.Value.value;
                //
                var actionExpirationTimestampConstraint = subAction.TimeConstraint == null? null : subAction.TimeConstraint.actionExpirationTimestamp;
                var actionTimeIntervalConstraint = subAction.TimeConstraint == null ? null : subAction.TimeConstraint.actionTimeInterval;
                var entityConstraints = subAction.EntityConstraints;
                var icrcConstraint = subAction.IcrcConstraint;
                var nftConstraint = subAction.NftConstraint;

                //ACION EXPIRATION CONSTRAINT
                if(actionExpirationTimestampConstraint != null)
                {
                    if (actionExpirationTimestampConstraint.HasValue)
                    {
                        if (actionExpirationTimestampConstraint.GetValueOrDefault() <= (ulong)MainUtil.Now()) return new("Action is expired");
                    }
                }

                //ACTION TIME INTERVAL COSNTRAINT
                if (actionTimeIntervalConstraint != null)
                {
                    var actionStateResult = UserUtil.GetElementOfType<DataTypes.ActionState>(principalId, actionId);

                    if (actionStateResult.IsOk)
                    {
                        var actionStateAsOk = actionStateResult.AsOk();

                        var intervalStartTs = actionStateAsOk.intervalStartTs;
                        var actionCount = actionStateAsOk.actionCount;


                        var actionsPerInterval = subAction.TimeConstraint.actionTimeInterval.ActionsPerInterval;

                        if (actionsPerInterval == 0)
                        {
                            return new("Action per interval is equal to 0");
                        }

                        var intervalDuration = subAction.TimeConstraint.actionTimeInterval.IntervalDuration;

                        var timeConstrainToCompareWith = intervalStartTs.NanoToMilliseconds() + intervalDuration.NanoToMilliseconds();

                        if (((timeConstrainToCompareWith < MainUtil.Now()) || (actionCount < actionsPerInterval)) == false)
                        {
                            return new("You dont have tries left");
                        }
                        else
                        {
                            if (tryIncrementActionExecutionCount)
                            {
                                if (timeConstrainToCompareWith < MainUtil.Now())
                                {
                                    actionStateAsOk.intervalStartTs = MainUtil.Now().MilliToNano();
                                    actionStateAsOk.actionCount = 1;

                                    BroadcastState.ForceInvoke<Data<DataTypes.ActionState>>(e=>e,principalId == userPrincipaId ? "self" : principalId);
                                }
                                else if (actionCount < actionsPerInterval)
                                {
                                    ++actionStateAsOk.actionCount;

                                    BroadcastState.ForceInvoke<Data<DataTypes.ActionState>>(e => e, principalId == userPrincipaId ? "self" : principalId);
                                }
                            }
                        }
                    }
                }


                //ENTITY CONSTRAINTS
                if (entityConstraints.Count > 0)
                {
                    var dataResult = UserUtil.GetData<DataTypes.Entity>(principalId);

                    if (dataResult.IsErr)
                    {
                        $"{dataResult.AsErr()}".Error(typeof(ActionUtil).Name);
                        return new(dataResult.AsErr());
                    }

                    var data = dataResult.AsOk();

                    foreach (var constraint in entityConstraints)
                    {
                        if (constraint.Check(data.elements) == false) return new("Failure to validate entity constraint");
                    }
                }

                //TOKEN CONSTRAINTS
                if (icrcConstraint.Count > 0)
                {
                    foreach (var constraint in icrcConstraint)
                    {
                        var tokenDetailsResult = TokenUtil.GetTokenDetails(principalId, constraint.Canister);

                        if (tokenDetailsResult.IsErr)
                        {
                            $"{tokenDetailsResult.AsErr()}".Error(typeof(ActionUtil).Name);
                            return new($"{tokenDetailsResult.AsErr()}");
                        }

                        var tokenDetails = tokenDetailsResult.AsOk();

                        if (constraint.Amount > tokenDetails.token.baseUnitAmount.ConvertToDecimal(tokenDetails.configs.decimals)) return new("Insuficient funds");
                    }
                }

                //NFT CONSTRAINTS
                //TODO: NFT CONSTRAINT MUST ALSO CHECK FOR NFT METADATA
                if (nftConstraint.Count > 0)
                {
                    foreach (var constraint in nftConstraint)
                    {
                        if (NftUtil.HasAnyNft(principalId, constraint.Canister) == false) return new($"You don't have nft from collection of id: {constraint.Canister}");
                    }
                }

                //
                runner = runner.Next;
            }

            return new(new Null());
        }

        public static bool ValidateConstraint(string actionId, string optionalTargetPrincipalId = "")
        {
            return ValidateConstraint(actionId, false, optionalTargetPrincipalId).IsOk;
        }

        private async static UniTask<UResult<bool, ActionErrType.Base>> HandlePosibleActionTransfers(string actionId)
        {
            bool hasTxConstraints = false;
            if (ConfigUtil.TryGetActionPart(actionId, e => e.callerAction.IcrcConstraint, out var txConstraints))
            {
                hasTxConstraints = txConstraints.Count > 0;
                //We loop through all the icrc constraint and make all the required icrc transfers.
                foreach (var txConstraint in txConstraints)
                {
                    var blockIndexResult = await ActionUtil.Transfer.TransferIcrc(txConstraint);

                    //We handle any transfer error
                    if (blockIndexResult.IsErr)
                    {
                        var error = blockIndexResult.AsErr();

                        if (error is TransferErrType.InsufficientBalance balanceError)
                        {
                            return new(new ActionErrType.InsufficientBalance(balanceError.content));
                        }
                        else if (error is TransferErrType.Transfer transferError)
                        {
                            return new(new ActionErrType.Transfer(transferError.content));
                        }
                        if (error is TransferErrType.LogIn loginError)
                        {
                            return new(new ActionErrType.Transfer(loginError.content));
                        }
                        else
                        {
                            return new(new ActionErrType.Other($"Other issue!\n\nUnkown..."));
                        }
                    }
                }
            }

            //SUCCESS
            return new(hasTxConstraints);
        }


        //MAIN FUNCTION TO PROCESS AN ACTION

        public async static UniTask<UResult<ProcessedActionResponse, ActionErrType.Base>> ProcessAction(string actionId, List<Field> args = default)
        {
            $"Try Process Action, ActionId: {actionId}".Log(nameof(ActionUtil));

            args ??= new();

            if (ActionsInProcess(actionId)) return new(new ActionErrType.Other("Action in progress"));

            //TODO: try pass target principal ID
            var validateActionResult = ValidateConstraint(actionId, true, "");

            if (validateActionResult.IsErr) return new(new ActionErrType.Other(validateActionResult.AsErr()));

            BroadcastState.Invoke(new ActionExecutionState(actionId, true), false, actionId);

            //If the action has ICRC constraints, we do the required transfers
            var hasTxConstraintsResult = await HandlePosibleActionTransfers(actionId);

            if (hasTxConstraintsResult.IsErr)
            {
                return new(hasTxConstraintsResult.AsErr());
            }

            var hasTxConstraints = hasTxConstraintsResult.AsOk();

            //
            var actionConfigResult = UserUtil.GetMainData<MainDataTypes.AllAction>();

            if (actionConfigResult.IsErr)
            {
                return new(new ActionErrType.Other(actionConfigResult.AsErr()));
            }

            var actionConfigAsOk = actionConfigResult.AsOk();

            if(actionConfigAsOk.actions.TryGetValue(BoomManager.Instance.WORLD_CANISTER_ID, out var allActionsConfigs) == false)
            {
                return new(new ActionErrType.Other($"Failure to find actions from world of id: {BoomManager.Instance.WORLD_CANISTER_ID}"));
            }

            if(allActionsConfigs.TryGetValue(actionId, out var actionConfig) == false)
            {
                return new(new ActionErrType.Other($"Failure to find action config of actionId: {actionId}"));
            }


            //Execute Action
            Result4 actionResponse = await BoomManager.Instance.WorldApiClient.ProcessAction(new ActionArg(actionId, args));


            if (actionResponse.Tag == Result4Tag.Err)
            {
                //Reset action state to prev state
                //TODO: MUST HANDLE REDO FOR WORLD AND TARGET AS WELL
                UserUtil.RequestDataSelf<DataTypeRequestArgs.ActionState>();

                CoroutineManagerUtil.DelayAction(() =>
                {
                    BroadcastState.Invoke(new ActionExecutionState(actionId, false), false, actionId);

                }, 5, BoomManager.Instance.transform);

                return new(new ActionErrType.Other(actionResponse.AsErr()));
            }

            var actionResponseAsOk = actionResponse.AsOk();

            if (hasTxConstraints) await UniTask.Delay(10 * 1000);


            var formulaDep = EntityUtil.GetFormulaDependencies(actionResponseAsOk);

            $"Action Response: {JsonConvert.SerializeObject(actionResponseAsOk)}".Log(typeof(EntityUtil).Name);

            ProcessedActionResponse processedActionResponse = new(actionResponseAsOk, formulaDep.worldEntities, formulaDep.callerEntities, formulaDep.targetEntities, formulaDep.configs, args);

            if (BoomManager.Instance.BoomDaoGameType == BoomManager.GameType.SinglePlayer)
            {
                List<string> outcomeUids = new();

                //CALLER OUTCOMES
                if (processedActionResponse.callerOutcomes != null)
                {
                    if (processedActionResponse.callerOutcomes.entityOutcomes.Count > 0)
                    {
                        EntityUtil.ApplyEntityEdits(processedActionResponse.callerOutcomes);
                        outcomeUids.Add(processedActionResponse.callerOutcomes.uid);
                    }

                    //NFTS
                    if (processedActionResponse.callerOutcomes.nfts.Count > 0)
                    {
                        NftUtil.TryAddMintedNft(processedActionResponse.callerOutcomes.uid, processedActionResponse.callerOutcomes.nfts.ToArray());

                        CoroutineManagerUtil.DelayAction(() =>
                        {
                            UserUtil.RequestData(new DataTypeRequestArgs.NftCollection(processedActionResponse.callerOutcomes.nfts.Map(e => e.Canister).ToArray(), processedActionResponse.callerOutcomes.uid));
                        }, 20f, BoomManager.Instance.transform);
                    }

                    //TOKENS
                    if (processedActionResponse.callerOutcomes.tokens.Count > 0)
                    {
                        TokenUtil.IncrementTokenByDecimal(processedActionResponse.callerOutcomes.uid, processedActionResponse.callerOutcomes.tokens.Map<TransferIcrc, (string canister, double decimalAmount)>(e => (e.Canister, e.Quantity)).ToArray());
                    }
                }
                //TARGET OUTCOMES
                if (processedActionResponse.targetOutcomes != null)
                {
                    if (processedActionResponse.targetOutcomes.entityOutcomes.Count > 0)
                    {
                        EntityUtil.ApplyEntityEdits(processedActionResponse.targetOutcomes);
                        outcomeUids.Add(processedActionResponse.targetOutcomes.uid);
                    }

                    //NFTS
                    if (processedActionResponse.targetOutcomes.nfts.Count > 0)
                    {
                        NftUtil.TryAddMintedNft(processedActionResponse.targetOutcomes.uid, processedActionResponse.targetOutcomes.nfts.ToArray());

                        CoroutineManagerUtil.DelayAction(() =>
                        {
                            UserUtil.RequestData(new DataTypeRequestArgs.NftCollection(processedActionResponse.targetOutcomes.nfts.Map(e => e.Canister).ToArray(), processedActionResponse.targetOutcomes.uid));
                        }, 20f, BoomManager.Instance.transform);
                    }

                    //TOKENS
                    if (processedActionResponse.targetOutcomes.tokens.Count > 0)
                    {
                        TokenUtil.IncrementTokenByDecimal(processedActionResponse.targetOutcomes.uid, processedActionResponse.targetOutcomes.tokens.Map<TransferIcrc, (string canister, double decimalAmount)>(e => (e.Canister, e.Quantity)).ToArray());
                    }
                }
                //WORLD OUTCOMES
                if (processedActionResponse.worldOutcomes != null)
                {
                    if (processedActionResponse.worldOutcomes.entityOutcomes.Count > 0)
                    {
                        EntityUtil.ApplyEntityEdits(processedActionResponse.worldOutcomes);
                        outcomeUids.Add(processedActionResponse.worldOutcomes.uid);
                    }

                    //NFTS
                    if (processedActionResponse.worldOutcomes.nfts.Count > 0)
                    {
                        NftUtil.TryAddMintedNft(processedActionResponse.worldOutcomes.uid, processedActionResponse.worldOutcomes.nfts.ToArray());

                        CoroutineManagerUtil.DelayAction(() =>
                        {
                            UserUtil.RequestData(new DataTypeRequestArgs.NftCollection(processedActionResponse.worldOutcomes.nfts.Map(e => e.Canister).ToArray(), processedActionResponse.worldOutcomes.uid));
                        }, 20f, BoomManager.Instance.transform);
                    }

                    //TOKENS
                    if (processedActionResponse.worldOutcomes.tokens.Count > 0)
                    {
                        TokenUtil.IncrementTokenByDecimal(processedActionResponse.worldOutcomes.uid, processedActionResponse.worldOutcomes.tokens.Map<TransferIcrc, (string canister, double decimalAmount)>(e => (e.Canister, e.Quantity)).ToArray());
                    }
                }

                //if (outcomeUids.Count > 0)
                //{
                //    CoroutineManagerUtil.DelayAction(() =>
                //    {
                //        UserUtil.RequestData(new DataTypeRequestArgs.Entity(outcomeUids.ToArray()));
                //    }, 20f, BoomManager.Instance.transform);
                //}
            }

            $"Action Processed Success, ActionId: {actionId}, outcome: {JsonConvert.SerializeObject(processedActionResponse)}".Log(nameof(ActionUtil));

            BroadcastState.Invoke(new ActionExecutionState(actionId, false), false, actionId);

            return new(processedActionResponse);
        }

        public static class Guilds
        {
            public async static UniTask<UResult<ProcessedActionResponse,string>> UserWonMatch()
            {
                var result = await ProcessAction("match_won");

                if (result.IsOk) return new(result.AsOk());
                return new(result.AsErr().Content);
            }
            public async static UniTask<UResult<ProcessedActionResponse, string>> UserLostMatch()
            {
                var result = await ProcessAction("match_lost");

                if (result.IsOk) return new(result.AsOk());
                return new(result.AsErr().Content);
            }
        }

        public static class Transfer
        {
            //ICRC
            public async static UniTask<UResult<ulong, TransferErrType.Base>> TransferIcrc(IcrcTx tx)
            {
                return await TransferIcrc(tx.Amount, tx.ToPrincipal, tx.Canister);
            }

            public async static UniTask<UResult<ulong, TransferErrType.Base>> TransferIcrc(double amount, string toPrincipalId, string icrcCanisterId = "")
            {
                if (string.IsNullOrEmpty(toPrincipalId))
                {
                    return new(new TransferErrType.Other("You must specify an address to sent the tokens to!"));
                }

                if (string.IsNullOrEmpty(icrcCanisterId)) icrcCanisterId = Env.CanisterIds.ICP_LEDGER;

                //CHECK LOGIN
                var principalResult = UserUtil.GetPrincipal();

                if (principalResult.Tag == UResultTag.Err)
                {
                    return new(new TransferErrType.LogIn($"You cannot execute this function, you might not be logged in or maybe you are as anon.\n More details:\n{principalResult.AsErr()}"));

                }
                var principal = principalResult.AsOk().Value;

                //CHECK USER BALANCE
                var tokenDetailsResult = TokenUtil.GetTokenDetails(principal, icrcCanisterId);

                if (tokenDetailsResult.Tag == UResultTag.Err)
                {
                    return new(new TransferErrType.Other(tokenDetailsResult.AsErr()));
                }

                var (token, metadata) = tokenDetailsResult.AsOk();

                var requiredBaseUnitAmount = TokenUtil.ConvertToBaseUnit(amount, metadata.decimals);

                if (token.baseUnitAmount < requiredBaseUnitAmount + metadata.fee)
                {
                    return new(new TransferErrType.InsufficientBalance($"Not enough \"${icrcCanisterId}\" currency. Current balance: {token.baseUnitAmount.ConvertToDecimal(metadata.decimals).NotScientificNotation()}, required balance: {amount}"));
                }

                //UPDATE LOCAL STATE
                TokenUtil.DecrementTokenByBaseUnit(principal, (icrcCanisterId, requiredBaseUnitAmount + metadata.fee));

                //SETUP INTERFACE
                var tokenInterface = new IcrcLedgerApiClient(UserUtil.GetAgent().AsOk(), Principal.FromText(icrcCanisterId));

                //SETUP ARGS
                var arg = new Candid.IcrcLedger.Models.TransferArg(
                    new(),
                    new(Principal.FromText(toPrincipalId), new()),
                    (UnboundedUInt)requiredBaseUnitAmount,
                    new((UnboundedUInt)metadata.fee),
                    new(),
                    new()
                    );

                //TRANSFER
                $"Transfer to principal: {toPrincipalId},\n amount {amount},\n baseUnitAmount: {requiredBaseUnitAmount},\n decimals: {metadata.decimals},\n fee: {metadata.fee}".Log(nameof(ActionUtil));
                var result = await tokenInterface.Icrc1Transfer(arg);

                //CHECK SUCCESS
                if (result.Tag == Candid.IcrcLedger.Models.TransferResultTag.Ok)
                {
                    var blockIndex = (ulong)result.AsOk();
                    $"BlockIndex Transfer: {blockIndex}".Log(nameof(ActionUtil));
                    return new(blockIndex);
                }
                else
                {   //Due to failure restore to previews value
                    TokenUtil.IncrementTokenByBaseUnit(principal, (icrcCanisterId, requiredBaseUnitAmount + metadata.fee));

                    return new(new TransferErrType.Transfer($"{result.AsErr().Tag}: {result.AsErr().Value}"));
                }
            }

            public async static UniTask<UResult<ulong, TransferErrType.Base>> TransferNft(string canister, string tokenIdentifier, string to)
            {
                //CHECK LOGIN

                var isLoggedIn = UserUtil.IsLoggedIn();

                if (isLoggedIn == false)
                {
                    return new(new TransferErrType.LogIn("You must log in"));
                }

                var uid = UserUtil.GetPrincipal().AsOk().Value;
                var fromPrincipal = Principal.FromText(uid);

                //UPDATE LOCAL STATE

                var removalResult = NftUtil.TryRemoveNftByIdentifier(uid, canister, tokenIdentifier);

                if (removalResult.IsErr)
                {
                    return new(new TransferErrType.InsufficientBalance(removalResult.AsErr()));
                }
                var removedNft = removalResult.AsOk();

                //SETUP INTERFACE
                Extv2BoomApiClient tokenInterface = new(UserUtil.GetAgent().AsOk(), Principal.FromText(canister));

                //SETUP ARGS
                var arg = new Candid.Extv2Boom.Models.TransferRequest(
                    amount: (UnboundedUInt)1,
                    from: new(Candid.Extv2Boom.Models.UserTag.Principal, fromPrincipal),
                    memo: new(),
                    notify: false,
                    subaccount: new(),
                    to: new(Candid.Extv2Boom.Models.UserTag.Principal, Principal.FromText(to)),
                    token: tokenIdentifier
                );

                //TRANSFER
                $"Transfer to principal: {to},\n nft of id: {tokenIdentifier}".Log(nameof(ActionUtil));
                var result = await tokenInterface.ExtTransfer(arg);

                //CHECK SUCCESS
                if (result.Tag == Candid.Extv2Boom.Models.TransferResponseTag.Ok)
                {
                    var blockIndex = (ulong)result.AsOk();
                    $"BlockIndex Transfer: {blockIndex}".Log(nameof(ActionUtil));
                    return new(blockIndex);
                }
                else
                {
                    NftUtil.TryAddMintedNft(uid, removedNft);
                    return new(new TransferErrType.Transfer($"{result.AsErr().Tag}: {result.AsErr().Value}"));
                }
            }
        }
    }
}