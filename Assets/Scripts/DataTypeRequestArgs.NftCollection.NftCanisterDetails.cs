// Ignore Spelling: metadata eid gid wid
namespace Boom
{
    using Boom.Patterns.Broadcasts;
    using Boom.Utility;
    using Candid.World.Models;
    using EdjCase.ICP.Agent.Agents;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Scripting;


    public enum DataSourceType
    {
        Caller, Target, World
    }

    public static class EntityFieldEdit
    {
        public abstract class Base { }

        public class DeleteField : Base
        {
            public DeleteField()
            {
            }
        }


        public class SetText : Base
        {
            public string Value { get; set; }

            public SetText(string value)
            {
                Value = value;
            }
        }

        public class AddToList : Base
        {
            public string Value { get; set; }

            public AddToList(string value)
            {
                Value = value;
            }
        }
        public class RemoveFromList : Base
        {
            public string Value { get; set; }

            public RemoveFromList(string value)
            {
                Value = value;
            }
        }

        public class Numeric : Base
        {
            public enum NumericType { Set, Increment, Decrement, RenewTimestamp }
            public NumericType NumericType_ { get; set; }
            public double Value { get; set; }
            public LinkedList<(string formula, NumericType numericType)> FormulasToApply;
            public bool HasFormulas;


            public Numeric(double value, NumericType numericType)
            {
                NumericType_ = numericType;
                Value = value;
            }
            public Numeric(string formula, NumericType numericType)
            {
                AddFormulaToApply(formula, numericType);
            }

            public void AddFormulaToApply(string formula, NumericType numericType)
            {
                FormulasToApply ??= new();

                FormulasToApply.AddLast((formula, numericType));

                HasFormulas = true;
            }

            public void EditNumericValue(double value, NumericType numericType)
            {
                if (numericType == NumericType.Set)
                {
                    NumericType_ = numericType;
                    Value = value;
                }
                else if (numericType == NumericType.Increment)
                {
                    if (NumericType_ == NumericType.Set)
                    {
                        NumericType_ = numericType;
                        Value = value;
                    }
                    else if (NumericType_ == NumericType.Increment)
                    {
                        Value += value;
                    }
                    else
                    {
                        Value -= value;

                        if (Value < 0)
                        {
                            Value *= -1;
                            NumericType_ = numericType;
                        }
                    }
                }
                else if (NumericType_ == NumericType.Decrement)
                {
                    if (NumericType_ == NumericType.Set)
                    {
                        NumericType_ = numericType;
                        Value = value;
                    }
                    else if (NumericType_ == NumericType.Decrement)
                    {
                        Value += value;
                    }
                    else
                    {
                        Value -= value;

                        if (Value < 0)
                        {
                            Value *= -1;
                            NumericType_ = numericType;
                        }
                    }
                }
                else
                {
                    Value += value;
                    NumericType_ = numericType;
                }
            }
        }
    }
    [Preserve]
    [Serializable]
    public class EntityOutcome
    {
        [Preserve] public string wid;
        [Preserve] public string eid;
        [Preserve] public Dictionary<string, EntityFieldEdit.Base> fields;
        [Preserve] public bool dispose;
        public EntityOutcome(string wid, string eid, Dictionary<string, EntityFieldEdit.Base> fields)
        {
            this.wid = string.IsNullOrEmpty(wid) ? BoomManager.Instance.WORLD_CANISTER_ID : wid;
            this.eid = eid;
            this.fields = fields ?? new();
            dispose = fields == null;
        }
        public string GetKey()
        {
            return $"{wid}{eid}";
        }
    }

    public class NftCollectionToFetch
    {
        public string collectionId;

        public NftCollectionToFetch(string collectionId)
        {
            this.collectionId = collectionId;
        }
    }

    public static class EntityConstrainTypes
    {
        public abstract class Base
        {
            protected Base(string wid, string eid, string constrainType)
            {
                //Debug.Log($"### Constraint > wid: {wid} gid: {gid} eid: {eid} {constrainType} {fieldName}");
                Wid = string.IsNullOrEmpty(wid) ? BoomManager.Instance.WORLD_CANISTER_ID : wid;
                Eid = eid;
                ConstrainType = constrainType;
            }
            public string Wid { get; private set; }
            public string Eid { get; private set; }

            public string ConstrainType { get; private set; }
            public abstract bool Check(Dictionary<string, DataTypes.Entity> entities);
            public string GetKey()
            {
                return $"{Wid}{Eid}";
            }
            public abstract object GetValue();
        }

        public class EqualToText : Base
        {
            protected string fieldName;

            public string expectedValue;

            public EqualToText(string wid, string eid, string constrainType, string fieldName, string expectedValue) : base(wid, eid, constrainType)
            {
                this.fieldName = fieldName;

                this.expectedValue = expectedValue;
            }

            public override bool Check(Dictionary<string, DataTypes.Entity> entities)
            {
                if (entities.TryGetValue(GetKey(), out var entity) == false)
                {
                    //Debug.LogError($"Failed when finding entity id: {Eid} in:\n{JsonConvert.SerializeObject(entities)}");

                    return false;
                }

                if (entity.fields.TryGetValue(fieldName, out var field) == false)
                {
                    //Debug.LogError($"Failed when finding field name: {fieldName} of entity id: {Eid}");

                    return false;
                }

                return field.ToString() == expectedValue;
            }

            public override object GetValue()
            {
                return expectedValue;
            }
        }
        public class ContainsText : Base
        {
            protected string fieldName;

            public string expectedValue;

            public ContainsText(string wid, string eid, string constrainType, string fieldName, string expectedValue) : base(wid, eid, constrainType)
            {
                this.fieldName = fieldName;

                this.expectedValue = expectedValue;
            }

            public override bool Check(Dictionary<string, DataTypes.Entity> entities)
            {
                if (entities.TryGetValue(GetKey(), out var entity) == false)
                {
                    //Debug.LogError($"Failed when finding entity id: {Eid} in:\n{JsonConvert.SerializeObject(entities)}");

                    return false;
                }

                if (entity.fields.TryGetValue(fieldName, out var field) == false)
                {
                    //Debug.LogError($"Failed when finding field name: {fieldName} of entity id: {Eid}");

                    return false;
                }

                return field.Contains(expectedValue);
            }

            public override object GetValue()
            {
                return expectedValue;
            }
        }
        public class EqualToNumber : Base
        {
            protected string fieldName;

            public double expectedValue;

            public EqualToNumber(string wid, string eid, string constrainType, string fieldName, double expectedValue) : base(wid, eid, constrainType)
            {
                this.fieldName = fieldName;

                this.expectedValue = expectedValue;
            }

            public override bool Check(Dictionary<string, DataTypes.Entity> entities)
            {
                if (entities.TryGetValue(GetKey(), out var entity) == false)
                {
                    //Debug.LogError($"Failed when finding entity id: {Eid} in:\n{JsonConvert.SerializeObject(entities)}");

                    return false;
                }

                if (entity.fields.TryGetValue(fieldName, out var field) == false)
                {
                    //Debug.LogError($"Failed when finding field name: {fieldName} of entity id: {Eid}");

                    return false;
                }

                if (field.TryParseValue<double>(out var value) == false)
                {
                    Debug.LogError("Failed when parsing field value");
                    return false;
                }

                return value == expectedValue;
            }

            public override object GetValue()
            {
                return expectedValue;
            }
        }

        public class GreaterThanNumber : Base
        {
            protected string fieldName;

            public double expectedValue;

            public GreaterThanNumber(string wid, string eid, string constrainType, string fieldName, double expectedValue) : base(wid, eid, constrainType)
            {
                this.fieldName = fieldName;

                this.expectedValue = expectedValue;
            }

            public override bool Check(Dictionary<string, DataTypes.Entity> entities)
            {
                if (entities.TryGetValue(GetKey(), out var entity) == false)
                {
                    //Debug.LogError($"Failed when finding entity id: {Eid} in:\n{JsonConvert.SerializeObject(entities)}");

                    return false;
                }

                if (entity.fields.TryGetValue(fieldName, out var field) == false)
                {
                    //Debug.LogError($"Failed when finding field name: {fieldName} of entity id: {Eid}");

                    return false;
                }

                if (field.TryParseValue<double>(out var value) == false)
                {
                    Debug.LogError("Failed when parsing field value");
                    return false;
                }

                return value > expectedValue;
            }

            public override object GetValue()
            {
                return expectedValue;
            }
        }
        public class LessThanNumber : Base
        {
            protected string fieldName;

            public double expectedValue;
            public LessThanNumber(string wid, string eid, string constrainType, string fieldName, double expectedValue) : base(wid, eid, constrainType)
            {
                this.fieldName = fieldName;

                this.expectedValue = expectedValue;
            }
            public override bool Check(Dictionary<string, DataTypes.Entity> entities)
            {
                if (entities.TryGetValue(GetKey(), out var entity) == false)
                {
                    return true;
                }

                if (entity.fields.TryGetValue(fieldName, out var field) == false)
                {
                    return true;
                }

                if (field.TryParseValue<double>(out var value) == false)
                {
                    Debug.LogError("Failed when parsing field value");
                    return false;
                }

                return value < expectedValue;
            }

            public override object GetValue()
            {
                return expectedValue;
            }
        }

        public class GreaterThanEqualToNumber : Base
        {
            protected string fieldName;

            public double expectedValue;

            public GreaterThanEqualToNumber(string wid, string eid, string constrainType, string fieldName, double expectedValue) : base(wid, eid, constrainType)
            {
                this.fieldName = fieldName;

                this.expectedValue = expectedValue;
            }

            public override bool Check(Dictionary<string, DataTypes.Entity> entities)
            {
                if (entities.TryGetValue(GetKey(), out var entity) == false)
                {
                    //Debug.LogError($"Failed when finding entity id: {Eid} in:\n{JsonConvert.SerializeObject(entities)}");

                    return false;
                }

                if (entity.fields.TryGetValue(fieldName, out var field) == false)
                {
                    //Debug.LogError($"Failed when finding field name: {fieldName} of entity id: {Eid}");

                    return false;
                }

                if (field.TryParseValue<double>(out var value) == false)
                {
                    Debug.LogError("Failed when parsing field value");
                    return false;
                }

                return value >= expectedValue;
            }

            public override object GetValue()
            {
                return expectedValue;
            }
        }
        public class LessThanEqualToNumber : Base
        {
            protected string fieldName;

            public double expectedValue;
            public LessThanEqualToNumber(string wid, string eid, string constrainType, string fieldName, double expectedValue) : base(wid, eid, constrainType)
            {
                this.fieldName = fieldName;

                this.expectedValue = expectedValue;
            }
            public override bool Check(Dictionary<string, DataTypes.Entity> entities)
            {
                if (entities.TryGetValue(GetKey(), out var entity) == false)
                {
                    //Debug.LogError($"Failed when finding entity id: {Eid} in:\n{JsonConvert.SerializeObject(entities)}");

                    return true;
                }

                if (entity.fields.TryGetValue(fieldName, out var field) == false)
                {
                    //Debug.LogError($"Failed when finding field name: {fieldName} of entity id: {Eid}");

                    return true;
                }

                if (field.TryParseValue<double>(out var value) == false)
                {
                    Debug.LogError("Failed when parsing field value");
                    return false;
                }

                return value <= expectedValue;
            }

            public override object GetValue()
            {
                return expectedValue;
            }
        }

        public class GreaterThanNowTimestamp : Base
        {
            protected string fieldName;

            public GreaterThanNowTimestamp(string wid, string eid, string constrainType, string fieldName) : base(wid, eid, constrainType)
            {
                this.fieldName = fieldName;
            }
            public override bool Check(Dictionary<string, DataTypes.Entity> entities)
            {
                if (entities.TryGetValue(GetKey(), out var entity) == false)
                {
                    //Debug.LogError($"Failed when finding entity id: {Eid} in:\n{JsonConvert.SerializeObject(entities)}");

                    return false;
                }

                if (entity.fields.TryGetValue(fieldName, out var field) == false)
                {
                    //Debug.LogError($"Failed when finding field name: {fieldName} of entity id: {Eid}");

                    return false;
                }

                if (field.TryParseValue<ulong>(out var value) == false)
                {
                    Debug.LogError("Failed when parsing field value");
                    return false;
                }

                return value.NanoToMilliseconds() > MainUtil.Now();
            }

            public override object GetValue()
            {
                return null;
            }
        }
        public class LesserThanNowTimestamp : Base
        {
            protected string fieldName;

            public LesserThanNowTimestamp(string wid, string eid, string constrainType, string fieldName) : base(wid, eid, constrainType)
            {
                this.fieldName = fieldName;
            }
            public override bool Check(Dictionary<string, DataTypes.Entity> entities)
            {

                if (entities.TryGetValue(GetKey(), out var entity) == false)
                {
                    //Debug.LogError($"Failed when finding entity id: {Eid} in:\n{JsonConvert.SerializeObject(entities)}");

                    return true;
                }

                if (entity.fields.TryGetValue(fieldName, out var field) == false)
                {
                    //Debug.LogError($"Failed when finding field name: {fieldName} of entity id: {Eid}");

                    return true;
                }

                if (field.TryParseValue<ulong>(out var value) == false)
                {
                    Debug.LogError("Failed when parsing field value");
                    return false;
                }

                return value.NanoToMilliseconds() < MainUtil.Now();
            }

            public override object GetValue()
            {
                return null;
            }
        }

        public class ExistField : Base
        {
            protected string fieldName;

            public bool value;
            public ExistField(string wid, string eid, string constrainType, string fieldName, bool value) : base(wid, eid, constrainType)
            {
                this.fieldName = fieldName;
                this.value = value;
            }

            public override bool Check(Dictionary<string, DataTypes.Entity> entities)
            {
                if (entities.TryGetValue(GetKey(), out var entity) == false) return false;

                if (entity.fields.TryGetValue(fieldName, out var field) == false) return false;

                return true;
            }

            public override object GetValue()
            {
                return value;
            }
        }
        public class Exist : Base
        {
            public bool value;

            public Exist(string wid, string eid, string constrainType, bool value) : base(wid, eid, constrainType)
            {
                this.value = value;
            }

            public override bool Check(Dictionary<string, DataTypes.Entity> entities)
            {
                if (entities.ContainsKey(Eid)) return true;
                return false;
            }

            public override object GetValue()
            {
                return value;
            }
        }
    }

    public class TimeConstraint
    {
        public class ActionTimeInterval
        {
            public ActionTimeInterval(ulong actionsPerInterval, ulong intervalDuration)
            {
                ActionsPerInterval = actionsPerInterval;
                IntervalDuration = intervalDuration;
            }

            public ulong ActionsPerInterval { get; set; }
            public ulong IntervalDuration { get; set; }
        }
        public TimeConstraint(Candid.World.Models.ActionConstraint.TimeConstraintValue timeConstraint)
        {
            if (timeConstraint.ActionTimeInterval.HasValue)
            {
                var _actionTimeInterval = timeConstraint.ActionTimeInterval.ValueOrDefault;
                _actionTimeInterval.ActionsPerInterval.TryToUInt64(out ulong actionsPerInterval);
                _actionTimeInterval.IntervalDuration.TryToUInt64(out ulong intervalDuration);
                actionTimeInterval = new(actionsPerInterval, intervalDuration);
            }

            if (timeConstraint.ActionExpirationTimestamp.HasValue)
            {
                timeConstraint.ActionExpirationTimestamp.ValueOrDefault.TryToUInt64(out ulong _actionExpirationTimestamp);
                actionExpirationTimestamp = actionExpirationTimestamp;
            }
            else actionExpirationTimestamp = null;
        }

        public ulong? actionExpirationTimestamp { get; set; }
        public ActionTimeInterval actionTimeInterval { get; set; }

    }

    public static class PossibleOutcomeTypes
    {
        public abstract class Base
        {
            public double weight;
            public ActionOutcomeOption.OptionInfoTag possibleOutcomeType;

            protected Base(double weight, ActionOutcomeOption.OptionInfoTag possibleOutcomeType)
            {
                this.weight = weight;
                this.possibleOutcomeType = possibleOutcomeType;
            }
        }

        public class TransferIcrc : Base
        {
            public TransferIcrc(double weight, ActionOutcomeOption.OptionInfoTag possibleOutcomeType, Candid.World.Models.TransferIcrc reference) : base(weight, possibleOutcomeType)
            {
                Canister = reference.Canister;
                Quantity = reference.Quantity;
            }

            public string Canister { get; set; }

            public double Quantity { get; set; }
        }

        public class MintNft : Base
        {
            public MintNft(double weight, ActionOutcomeOption.OptionInfoTag possibleOutcomeType, Candid.World.Models.MintNft reference) : base(weight, possibleOutcomeType)
            {

                this.AssetId = reference.AssetId;
                this.Canister = reference.Canister;
                this.Metadata = reference.Metadata;
            }

            public string AssetId { get; set; }

            public string Canister { get; set; }

            public string Metadata { get; set; }
        }

        public class UpdateEntity : Base
        {
            [Preserve] public string Wid;
            [Preserve] public string Eid;
            [Preserve] public Dictionary<string, EntityFieldEdit.Base> Fields;
            [Preserve] public bool Dispose;

            public LinkedList<EntityFieldEdit.SetText> QuerySetTextFields()
            {
                LinkedList<EntityFieldEdit.SetText> tempList = new();

                foreach (var field in Fields)
                {
                    if (field.Value is EntityFieldEdit.SetText field_)
                    {
                        tempList.AddLast(field_);
                    }
                }

                return tempList;
            }

            public LinkedList<EntityFieldEdit.Numeric> QueryNumericFields(EntityFieldEdit.Numeric.NumericType numericType)
            {
                LinkedList<EntityFieldEdit.Numeric> tempList = new();

                foreach (var field in Fields)
                {
                    if(field.Value is EntityFieldEdit.Numeric field_)
                    {
                        if (field_.NumericType_ == numericType) tempList.AddLast(field_);
                    }
                }

                return tempList;
            }

            public LinkedList<string> QueryFieldsToDelete()
            {
                LinkedList<string> tempList = new();

                foreach (var field in Fields)
                {
                    if (field.Value is EntityFieldEdit.DeleteField)
                    {
                        tempList.AddLast(field.Key);
                    }
                }

                return tempList;
            }

            public bool IsMarkedAsDispose()
            {
                return Dispose;
            }

            public UpdateEntity(double weight, ActionOutcomeOption.OptionInfoTag possibleOutcomeType, Candid.World.Models.UpdateEntity reference) : base(weight, possibleOutcomeType)
            {
                this.Wid = string.IsNullOrEmpty(reference.Wid.ValueOrDefault) ? BoomManager.Instance.WORLD_CANISTER_ID : reference.Wid.ValueOrDefault;
                this.Eid = reference.Eid;

                this.Fields = new();

                foreach(var update in reference.Updates)
                {
                    switch (update.Tag)
                    {
                        case UpdateEntityTypeTag.SetText:

                            var unwrap1 = update.AsSetText();

                            Fields[unwrap1.FieldName] = new EntityFieldEdit.SetText(unwrap1.FieldValue);

                            break;
                        case UpdateEntityTypeTag.AddToList:

                            var unwrap2 = update.AsAddToList();

                            Fields[unwrap2.FieldName] = new EntityFieldEdit.AddToList(unwrap2.Value);

                            break;
                        case UpdateEntityTypeTag.RemoveFromList:

                            var unwrap3 = update.AsRemoveFromList();

                            Fields[unwrap3.FieldName] = new EntityFieldEdit.AddToList(unwrap3.Value);

                            break;
                        case UpdateEntityTypeTag.SetNumber:

                            var unwrap4 = update.AsSetNumber();

                            if(unwrap4.FieldValue.Tag == SetNumber.FieldValueInfoTag.Number)
                            {
                                Fields[unwrap4.FieldName] = new EntityFieldEdit.Numeric(
                                unwrap4.FieldValue.AsNumber(),
                                EntityFieldEdit.Numeric.NumericType.Set);
                            }
                            else
                            {
                                Fields[unwrap4.FieldName] = new EntityFieldEdit.Numeric(
                                unwrap4.FieldValue.AsFormula(),
                                EntityFieldEdit.Numeric.NumericType.Set);
                            }

                            break;
                        case UpdateEntityTypeTag.IncrementNumber:

                            var unwrap5 = update.AsIncrementNumber();

                            if (unwrap5.FieldValue.Tag == IncrementNumber.FieldValueInfoTag.Number)
                            {
                                var value5 = unwrap5.FieldValue.AsNumber();

                                if (Fields.TryAdd(unwrap5.FieldName, new EntityFieldEdit.Numeric(value5, EntityFieldEdit.Numeric.NumericType.Increment)) == false)
                                {
                                    (Fields[unwrap5.FieldName] as EntityFieldEdit.Numeric).EditNumericValue(value5,
                                        EntityFieldEdit.Numeric.NumericType.Increment);
                                }
                            }
                            else
                            {
                                var value5 = unwrap5.FieldValue.AsFormula();

                                if (Fields.TryAdd(unwrap5.FieldName, new EntityFieldEdit.Numeric(value5, EntityFieldEdit.Numeric.NumericType.Increment)) == false)
                                {
                                    (Fields[unwrap5.FieldName] as EntityFieldEdit.Numeric).AddFormulaToApply(value5,
                                        EntityFieldEdit.Numeric.NumericType.Increment);
                                }
                            }

                            break;
                        case UpdateEntityTypeTag.DecrementNumber:

                            var unwrap6 = update.AsDecrementNumber();

                            if (unwrap6.FieldValue.Tag == DecrementNumber.FieldValueInfoTag.Number)
                            {
                                var value6 = unwrap6.FieldValue.AsNumber();

                                if (Fields.TryAdd(unwrap6.FieldName, new EntityFieldEdit.Numeric(value6, EntityFieldEdit.Numeric.NumericType.Decrement)) == false)
                                {
                                    (Fields[unwrap6.FieldName] as EntityFieldEdit.Numeric).EditNumericValue(value6,
                                        EntityFieldEdit.Numeric.NumericType.Decrement);
                                }
                            }
                            else
                            {
                                var value6 = unwrap6.FieldValue.AsFormula();

                                if (Fields.TryAdd(unwrap6.FieldName, new EntityFieldEdit.Numeric(value6, EntityFieldEdit.Numeric.NumericType.Decrement)) == false)
                                {
                                    (Fields[unwrap6.FieldName] as EntityFieldEdit.Numeric).AddFormulaToApply(value6,
                                        EntityFieldEdit.Numeric.NumericType.Decrement);
                                }
                            }
                            break;
                        case UpdateEntityTypeTag.RenewTimestamp:

                            var unwrap7 = update.AsRenewTimestamp();

                            if (unwrap7.FieldValue.Tag == RenewTimestamp.FieldValueInfoTag.Number)
                            {
                                var value7 = unwrap7.FieldValue.AsNumber();

                                if (Fields.TryAdd(unwrap7.FieldName, new EntityFieldEdit.Numeric(value7, EntityFieldEdit.Numeric.NumericType.RenewTimestamp)) == false)
                                {
                                    (Fields[unwrap7.FieldName] as EntityFieldEdit.Numeric).EditNumericValue(value7,
                                        EntityFieldEdit.Numeric.NumericType.RenewTimestamp);
                                }
                            }
                            else
                            {
                                var value7 = unwrap7.FieldValue.AsFormula();

                                if (Fields.TryAdd(unwrap7.FieldName, new EntityFieldEdit.Numeric(value7, EntityFieldEdit.Numeric.NumericType.RenewTimestamp)) == false)
                                {
                                    (Fields[unwrap7.FieldName] as EntityFieldEdit.Numeric).AddFormulaToApply(value7,
                                        EntityFieldEdit.Numeric.NumericType.RenewTimestamp);
                                }
                            }

                            break;
                        case UpdateEntityTypeTag.DeleteField:
                            var unwrap8 = update.AsDeleteField();
                            Fields[unwrap8.FieldName] = new EntityFieldEdit.DeleteField();
                            break;
                        case UpdateEntityTypeTag.DeleteEntity:
                            Dispose = true;
                            break;

                    }
                }

                Dispose = Fields == null;
            }
            public string GetKey()
            {
                return $"{Wid}{Eid}";
            }
        }
    }


    public class Outcome
    {
        public List<PossibleOutcomeTypes.Base> PossibleOutcomes;

        public Outcome(Candid.World.Models.ActionOutcome outcome)
        {
            this.PossibleOutcomes = new();

            foreach (var possibleOutcome in outcome.PossibleOutcomes)
            {
                switch (possibleOutcome.Option.Tag)
                {
                    case ActionOutcomeOption.OptionInfoTag.TransferIcrc:
                        PossibleOutcomes.Add(new PossibleOutcomeTypes.TransferIcrc(possibleOutcome.Weight, possibleOutcome.Option.Tag, possibleOutcome.Option.AsTransferIcrc()));
                        continue;
                    case ActionOutcomeOption.OptionInfoTag.MintNft:
                        PossibleOutcomes.Add(new PossibleOutcomeTypes.MintNft(possibleOutcome.Weight, possibleOutcome.Option.Tag, possibleOutcome.Option.AsMintNft()));
                        continue;
                    case ActionOutcomeOption.OptionInfoTag.UpdateEntity:
                        PossibleOutcomes.Add(new PossibleOutcomeTypes.UpdateEntity(possibleOutcome.Weight, possibleOutcome.Option.Tag, possibleOutcome.Option.AsUpdateEntity()));
                        continue;

                }
            }
        }
    }

    public class SubAction
    {
 
        public bool HasConstraint { get; set; }

        public TimeConstraint TimeConstraint { get; set; }
        public List<EntityConstrainTypes.Base> EntityConstraints { get; set; }
        public List<IcrcTx> IcrcConstraint { get; set; }
        public List<NftTx> NftConstraint { get; set; }

        public List<Outcome> Outcomes { get; set; }

        public SubAction(Candid.World.Models.SubAction subAction)
        {
            HasConstraint = subAction.ActionConstraint.HasValue;

            if(subAction.ActionResult != null)
            {
                Outcomes = new();

                foreach (var outcome in subAction.ActionResult.Outcomes)
                {
                    Outcomes.Add(new Outcome(outcome));
                }
            }

            if (HasConstraint)
            {
                var constraints = subAction.ActionConstraint.GetValueOrDefault();
                //SETUP TIME CONSTRAINT
                if (constraints.TimeConstraint.HasValue) TimeConstraint = new(constraints.TimeConstraint.ValueOrDefault);

                //SETUP ENTITY CONSTRAINT
                EntityConstraints = new();
                foreach (var item in constraints.EntityConstraint)
                {
                    if (item.EntityConstraintType.Tag == EntityConstraintTypeTag.EqualToText)
                    {
                        var expectedValue = item.EntityConstraintType.AsEqualToText();
                        this.EntityConstraints.Add(new EntityConstrainTypes.EqualToText(item.Wid.ValueOrDefault, item.Eid, $"{nameof(EntityConstrainTypes.EqualToText)}", expectedValue.FieldName, expectedValue.Value));
                    }
                    else if (item.EntityConstraintType.Tag == EntityConstraintTypeTag.ContainsText)
                    {
                        var expectedValue = item.EntityConstraintType.AsContainsText();
                        this.EntityConstraints.Add(new EntityConstrainTypes.ContainsText(item.Wid.ValueOrDefault, item.Eid, $"{nameof(EntityConstrainTypes.ContainsText)}", expectedValue.FieldName, expectedValue.Value));
                    }
                    else if (item.EntityConstraintType.Tag == EntityConstraintTypeTag.EqualToNumber)
                    {
                        var expectedValue = item.EntityConstraintType.AsEqualToNumber();
                        this.EntityConstraints.Add(new EntityConstrainTypes.EqualToNumber(item.Wid.ValueOrDefault, item.Eid, $"{nameof(EntityConstrainTypes.EqualToNumber)}", expectedValue.FieldName, expectedValue.Value));
                    }
                    else if (item.EntityConstraintType.Tag == EntityConstraintTypeTag.GreaterThanNumber)
                    {
                        var expectedValue = item.EntityConstraintType.AsGreaterThanNumber();
                        this.EntityConstraints.Add(new EntityConstrainTypes.GreaterThanNumber(item.Wid.ValueOrDefault, item.Eid, $"{nameof(EntityConstrainTypes.GreaterThanNumber)}", expectedValue.FieldName, expectedValue.Value));
                    }
                    else if (item.EntityConstraintType.Tag == EntityConstraintTypeTag.LessThanNumber)
                    {
                        var expectedValue = item.EntityConstraintType.AsLessThanNumber();
                        this.EntityConstraints.Add(new EntityConstrainTypes.LessThanNumber(item.Wid.ValueOrDefault, item.Eid, $"{nameof(EntityConstrainTypes.LessThanNumber)}", expectedValue.FieldName, expectedValue.Value));
                    }
                    else if (item.EntityConstraintType.Tag == EntityConstraintTypeTag.GreaterThanEqualToNumber)
                    {
                        var expectedValue = item.EntityConstraintType.AsGreaterThanEqualToNumber();
                        this.EntityConstraints.Add(new EntityConstrainTypes.GreaterThanEqualToNumber(item.Wid.ValueOrDefault, item.Eid, $"{nameof(EntityConstrainTypes.GreaterThanEqualToNumber)}", expectedValue.FieldName, expectedValue.Value));
                    }
                    else if (item.EntityConstraintType.Tag == EntityConstraintTypeTag.LessThanEqualToNumber)
                    {
                        var expectedValue = item.EntityConstraintType.AsLessThanEqualToNumber();
                        this.EntityConstraints.Add(new EntityConstrainTypes.LessThanEqualToNumber(item.Wid.ValueOrDefault, item.Eid, $"{nameof(EntityConstrainTypes.LessThanEqualToNumber)}", expectedValue.FieldName, expectedValue.Value));
                    }
                    else if (item.EntityConstraintType.Tag == EntityConstraintTypeTag.GreaterThanNowTimestamp)
                    {
                        var expectedValue = item.EntityConstraintType.AsGreaterThanNowTimestamp();

                        this.EntityConstraints.Add(new EntityConstrainTypes.GreaterThanNowTimestamp(item.Wid.ValueOrDefault, item.Eid, $"{nameof(EntityConstrainTypes.GreaterThanNowTimestamp)}", expectedValue.FieldName));
                    }
                    else if (item.EntityConstraintType.Tag == EntityConstraintTypeTag.LessThanNowTimestamp)
                    {
                        var expectedValue = item.EntityConstraintType.AsLessThanNowTimestamp();

                        this.EntityConstraints.Add(new EntityConstrainTypes.LesserThanNowTimestamp(item.Wid.ValueOrDefault, item.Eid, $"{nameof(EntityConstrainTypes.LesserThanNowTimestamp)}", expectedValue.FieldName));
                    }
                    else if (item.EntityConstraintType.Tag == EntityConstraintTypeTag.ExistField)
                    {
                        var expectedValue = item.EntityConstraintType.AsExistField();
                        this.EntityConstraints.Add(new EntityConstrainTypes.ExistField(item.Wid.ValueOrDefault, item.Eid, $"{nameof(EntityConstrainTypes.ExistField)}", expectedValue.FieldName, expectedValue.Value));
                    }
                }

                IcrcConstraint = constraints.IcrcConstraint;
                NftConstraint = new();

                constraints.NftConstraint.Iterate(e =>
                {
                    if (e.NftConstraintType.Tag == NftTx.NftConstraintTypeInfoTag.Transfer)
                    {
                        var transferConstraint = e.NftConstraintType.AsTransfer();

                        if (string.IsNullOrEmpty(transferConstraint.ToPrincipal)) NftConstraint.Add(new(e.Canister, e.Metadata, new NftTx.NftConstraintTypeInfo(NftTx.NftConstraintTypeInfoTag.Transfer, new NftTransfer("0000000000000000000000000000000000000000000000000000001"))));
                        else NftConstraint.Add(e);
                    }
                    else
                    {
                        NftConstraint.Add(e);
                    }
                });
            }
            else
            {
                EntityConstraints = new();
                IcrcConstraint = new();
                NftConstraint = new();
            }
        }
    }

    //

    //


    public static class DataTypeRequestArgs
    {
        public class Base
        {
            public string[] uids;

            protected Base(params string[] usersUid)
            {
                this.uids = usersUid;
            }
        }

        internal class Entity : Base
        {
            public Entity() : base("self") { }
            public Entity(params string[] usersUid) : base(usersUid)
            {
            }
        }

        internal class ActionState : Base
        {
            public ActionState() : base("self") { }
            public ActionState(params string[] usersUid) : base(usersUid)
            {
            }
        }

        internal class Token : Base
        {
            public readonly string[] canisterIds;
            public Token() : base("self") { }
            public Token(string[] canisterIds, params string[] usersUid) : base(usersUid)
            {
                this.canisterIds = canisterIds;
            }
        }

        internal class NftCollection : Base
        {
            public readonly string[] canisterIds;
            public NftCollection() : base("self") { }
            public NftCollection(string[] canisterIds, params string[] usersUid) : base(usersUid)
            {
                this.canisterIds = canisterIds;
            }
        }

        //
        internal class StakedNftCollections : Base
        {
            public StakedNftCollections() : base("self") { }
            public StakedNftCollections(params string[] usersUid) : base(usersUid)
            {
            }
        }
    }

    public static class DataTypes
    {
        public abstract class Base : Boom.IDisposable
        {
            public bool isScheduleForDisposal;

            public abstract string GetKey();

            public void ScheduleDisposal()
            {
                isScheduleForDisposal = true;
            }

            public bool CanDispose()
            {
                return isScheduleForDisposal;
            }
        }

        [Preserve]
        [Serializable]
        public class Entity : Base
        {
            [Preserve] public string wid;
            [Preserve] public string eid;
            [Preserve] public Dictionary<string, string> fields;

            public Entity(string wid, string eid, Dictionary<string, string> fields)
            {
                this.wid = string.IsNullOrEmpty(wid) ? BoomManager.Instance.WORLD_CANISTER_ID : wid;
                this.eid = eid;
                this.fields = fields;
            }
            public Entity(string wid, Candid.World.Models.StableEntity entity)
            {
                this.wid = wid;
                this.eid = entity.Eid;
                this.fields = new();

                foreach (var field in entity.Fields)
                {
                    fields.Add(field.FieldName, field.FieldValue);
                }
            }

            public override string GetKey()
            {
                return $"{wid}{eid}";
            }
        }

        [Preserve]
        [Serializable]
        public class Badge : Base
        {
            [Preserve] public string wid;
            [Preserve] public string eid;
            [Preserve] public Dictionary<string, string> fields;

            public Badge(string wid, string eid, Dictionary<string, string> fields)
            {
                this.wid = string.IsNullOrEmpty(wid) ? BoomManager.Instance.WORLD_CANISTER_ID : wid;
                this.eid = eid;
                this.fields = fields;
            }
            public Badge(string wid, Candid.World.Models.StableEntity entity)
            {
                this.wid = wid;
                this.eid = entity.Eid;
                this.fields = new();

                foreach (var field in entity.Fields)
                {
                    fields.Add(field.FieldName, field.FieldValue);
                }
            }

            public override string GetKey()
            {
                return $"{eid}";
            }
        }

        [Preserve]
        [Serializable]
        public class ActionState : Base
        {
            public string actionId;
            public ulong actionCount;
            public ulong intervalStartTs;

            public ActionState(string actionId, ulong actionCount, ulong intervalStartTs)
            {
                this.actionId = actionId;
                this.actionCount = actionCount;
                this.intervalStartTs = intervalStartTs;
            }
            public ActionState(Candid.World.Models.ActionState action)
            {
                this.actionId = action.ActionId;
                action.ActionCount.TryToUInt64(out actionCount);
                action.IntervalStartTs.TryToUInt64(out intervalStartTs);
            }

            public override string GetKey()
            {
                return actionId;
            }
        }

        [Preserve]
        [Serializable]
        public class Token : Base
        {
            public string canisterId;
            public ulong baseUnitAmount;
            public Token(string canisterId, ulong baseUnitAmount)
            {
                this.canisterId = canisterId;
                this.baseUnitAmount = baseUnitAmount;
            }

            public override string GetKey()
            {
                return canisterId;
            }
        }

        [Preserve]
        [Serializable]
        public class NftCollection : Base
        {
            [Preserve]
            [Serializable]
            public class Nft
            {
                [Preserve] public string canister;
                [Preserve] public uint index;
                [Preserve] public string tokenIdentifier;
                public string url;
                [Preserve] public string metadata;

                public Nft(string canister, uint index, string tokenIdentifier, string url, string metadata)
                {
                    this.canister = canister;
                    this.index = index;
                    this.tokenIdentifier = tokenIdentifier;
                    this.url = url;
                    this.metadata = metadata;
                }
            }

            [Preserve] public string canisterId;


            [Preserve] public List<Nft> tokens = new();

            public NftCollection(string canister)
            {
                this.canisterId = canister;
            }

            public override string GetKey()
            {
                return canisterId;
            }
        }

        //

        [Preserve]
        [Serializable]
        public class StakedNftCollections : Base
        {
            [Preserve]
            [Serializable]
            public class Nft
            {
                [Preserve] public string canister;
                [Preserve] public uint index;
                [Preserve] public string tokenIdentifier;
                public string url;
                [Preserve] public string metadata;

                public Nft(string canister, uint index, string tokenIdentifier, string url, string metadata)
                {
                    this.canister = canister;
                    this.index = index;
                    this.tokenIdentifier = tokenIdentifier;
                    this.url = url;
                    this.metadata = metadata;
                }
            }

            [Preserve] public string canisterId;


            [Preserve] public List<Nft> tokens = new();

            public StakedNftCollections(string canister)
            {
                this.canisterId = canister;
            }

            public override string GetKey()
            {
                return canisterId;
            }
        }
    }

    public class MainDataTypes
    {
        public abstract class Base : IBroadcastState
        {
            protected Base()
            {
            }

            public int MaxSavedStatesCount()
            {
                return 0;
            }
        }


        [Preserve]
        [Serializable]
        public class AllConfigs : Base
        {
            public class Config
            {
                public string cid;
                [Preserve] public Dictionary<string, string> fields;

                public Config(string cid, Dictionary<string, string> fields)
                {
                    this.cid = cid;
                    this.fields = fields;
                }

                public Config(Candid.World.Models.StableConfig entity)
                {
                    this.cid = entity.Cid;
                    this.fields = new();

                    foreach (var field in entity.Fields)
                    {
                        fields.Add(field.FieldName, field.FieldValue);
                    }
                }
            }

            public Dictionary<string, Dictionary<string, Config>> configs; //worldId -> cid -> config

            public AllConfigs()
            {
                this.configs = new();
            }
            public AllConfigs(Dictionary<string, Dictionary<string, Config>> configs)
            {
                this.configs = configs;
            }
        }

        [Preserve]
        [Serializable]
        public class AllAction : Base
        {
            public class Action
            {
                public Action(Candid.World.Models.Action arg)
                {
                    //Setup constraints
                    callerAction = arg.CallerAction.HasValue ? new(arg.CallerAction.ValueOrDefault) : null;
                    targetAction = arg.TargetAction.HasValue ? new(arg.TargetAction.ValueOrDefault) : null;
                    worldAction = arg.WorldAction.HasValue ? new(arg.WorldAction.ValueOrDefault) : null;

                    aid = arg.Aid;
                }

                public SubAction callerAction;
                public SubAction targetAction;
                public SubAction worldAction;

                public string aid;
            }

            public Dictionary<string, Dictionary<string, Action>> actions; //worldId -> aid -> action

            public AllAction()
            {
                this.actions = new();
            }

            public AllAction(Dictionary<string, Dictionary<string, Action>> actions)
            {
                this.actions = actions;
            }
        }


        [Preserve]
        [Serializable]
        public class AllTokenConfigs : Base
        {
            public class TokenConfig
            {
                public string canisterId;
                public string name;
                public string symbol;
                public byte decimals;
                public ulong fee;
                public string description;
                public string urlLogo;

                public TokenConfig() { }
                public TokenConfig(string canisterId, string name, string symbol, byte decimals, ulong fee, string description = "", string urlLogo = "")
                {
                    this.canisterId = canisterId;
                    this.name = name;
                    this.symbol = symbol;
                    this.decimals = decimals;
                    this.fee = fee;
                    this.description = description;
                    this.urlLogo = urlLogo;
                }
            }

            public Dictionary<string, TokenConfig> configs; //canisterId -> config

            public AllTokenConfigs() { configs = new(); }

            public AllTokenConfigs(Dictionary<string, TokenConfig> configs)
            {
                this.configs = configs;
            }
        }


        public class AllNftCollectionConfig : Base
        {
            [Preserve]
            [Serializable]
            public class NftConfig
            {

                [Preserve] public string canisterId;
                public bool isBoomDaoStandard;
                public string name;
                public string description;
                public string urlLogo;

                public NftConfig() { }
                public NftConfig(string canister, bool isBoomDaoStandard, string name, string description, string urlLogo)
                {
                    this.canisterId = canister;
                    this.isBoomDaoStandard = isBoomDaoStandard;

                    this.name = name;
                    this.description = description;
                    this.urlLogo = urlLogo;
                }
            }

            public Dictionary<string, NftConfig> configs; //canisterId -> config

            public AllNftCollectionConfig() { configs = new(); }

            public AllNftCollectionConfig(Dictionary<string, NftConfig> configs)
            {
                this.configs = configs;
            }
        }

        [Preserve]
        [Serializable]
        public class LoginData : Base
        {
            public enum State
            {
                None,
                LoginRequested,
                FetchingUserData,
                LoggedIn,
                Logedout,
            }
            public IAgent agent;
            public string principal;
            public string accountIdentifier;
            public State state;
            public long updateTs;
            public bool isEmbeddedAgent;
            public string tier;

            public LoginData()
            {
                this.state = State.None;
            }
            public LoginData(LoginData loginData, bool isEmbeddedAgent)
            {
                this.agent = loginData.agent;
                this.principal = loginData.principal;
                this.accountIdentifier = loginData.accountIdentifier;
                this.state = loginData.state;
                updateTs = MainUtil.Now();

                this.isEmbeddedAgent = isEmbeddedAgent;
            }
            public LoginData(LoginData loginData, State state, bool isEmbeddedAgent)
            {
                this.agent = loginData.agent;
                this.principal = loginData.principal;
                this.accountIdentifier = loginData.accountIdentifier;
                this.state = state;
                updateTs = MainUtil.Now();

                this.isEmbeddedAgent = isEmbeddedAgent;
                this.tier = loginData.tier;
            }
            public LoginData(IAgent agent, string principal, string accountIdentifier, State state, bool isEmbeddedAgent, string tier)
            {
                this.agent = agent;
                this.principal = principal;
                this.accountIdentifier = accountIdentifier;
                this.state = state;
                updateTs = MainUtil.Now();
                this.isEmbeddedAgent = isEmbeddedAgent;
                this.tier = tier;
            }
        }

        [Preserve]
        [Serializable]
        public class AllRoomData : Base
        {
            public class RoomData
            {
                public string roomId;
                public int userCount;
                public string[] users;

                public RoomData(string roomId, string[] users)
                {
                    this.roomId = roomId;
                    this.userCount = users.Length;
                    this.users = users;
                }
            }
            public bool inRoom;
            public string currentRoomId;
            public RoomData currentRoom;
            public Dictionary<string, RoomData> rooms = new();
            public long updateTs;

            public AllRoomData() { }
            public AllRoomData(IEnumerable<DataTypes.Entity> roomEntities)
            {
                updateTs = MainUtil.Now();

                //
                if (UserUtil.IsLoggedIn(out var loginData) == false)
                {
                    return;
                }

                foreach (var roomEntity in roomEntities)
                {
                    roomEntity.TryGetFieldAsDouble("userCount", out var userCount);

                    if (userCount > 0)
                    {
                        if (roomEntity.TryGetFieldAsText("users", out var users))
                        {
                            var usersInRoom = users.Split(',');
                            var room = new RoomData(roomEntity.eid, usersInRoom);
                            this.rooms.TryAdd(roomEntity.eid, room);

                            if (users.Contains(loginData.principal))
                            {
                                currentRoomId = roomEntity.eid;
                                currentRoom = room;
                                inRoom = true;
                                break;
                            }
                        }
                    }
                }
            }

            public string[] GetAllUsersInCurrentRoom()
            {
                if (inRoom)
                {
                    if (rooms.TryGetValue(currentRoomId, out var room)) return room.users;
                    else Debug.LogWarning("Could not find users of room id: " + currentRoomId);
                }
                return null;
            }
        }

        [Preserve]
        [Serializable]
        public class AllListings : Base
        {
            [Preserve]
            [Serializable]
            public class Listing : Base
            {
                public Listing(string tokenIdentifier, Candid.Extv2Boom.Extv2BoomApiClient.ListingsReturnArg0.ListingsReturnArg0Element arg)
                {
                    this.tokenIdentifier = tokenIdentifier;
                    index = arg.F0;
                    details = arg.F1;
                    metadataLegacy = arg.F2;
                }

                public string tokenIdentifier;
                public uint index;
                public Candid.Extv2Boom.Models.Listing details;
                public Candid.Extv2Boom.Models.MetadataLegacy metadataLegacy;
            }

            public Dictionary<uint, Listing> listings;

            public AllListings()
            {
            }
            public AllListings(Dictionary<uint, Listing> listings)
            {
                this.listings = listings;
            }
        }
    }
}






