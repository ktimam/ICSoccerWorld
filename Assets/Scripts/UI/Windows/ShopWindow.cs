using Candid.World.Models;
using Boom.Patterns.Broadcasts;
using Boom.UI;
using Boom.Utility;
using Boom.Values;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Boom;
using System.Linq;
using Candid;

public class ShopWindow : Window
{
    //[SerializeField] GameObject loadingStateGo;

    [SerializeField] string listOfValidActionsConfig = "shop_window_actions";

    [SerializeField] Transform content;
    [SerializeField] Button closeButton;
    [SerializeField, ShowOnly] bool buying;
    public override bool RequireUnlockCursor()
    {
        return false;
    }

    public override void Setup(object data)
    {
        UserUtil.AddListenerDataChangeSelf<DataTypes.Entity>(UpdateWindow);
        UserUtil.AddListenerDataChangeSelf<DataTypes.ActionState>(UpdateWindow);
        UserUtil.AddListenerMainDataChange<MainDataTypes.AllConfigs>(UpdateWindow, new() { invokeOnRegistration = true });
    }

    private void OnDestroy()
    {
        UserUtil.RemoveListenerDataChangeSelf<DataTypes.Entity>(UpdateWindow);
        UserUtil.RemoveListenerDataChangeSelf<DataTypes.ActionState>(UpdateWindow);
        UserUtil.RemoveListenerMainDataChange<MainDataTypes.AllConfigs>(UpdateWindow);
    }
    private void UpdateWindow(Data<DataTypes.Entity> state)
    {
        var principalResult =  UserUtil.GetPrincipal();

        if (principalResult.IsErr)
        {
            principalResult.AsErr().Value.Error();

            return;
        }

        if (UserUtil.IsDataValid<DataTypes.Entity>(principalResult.AsOk().Value))
        {
            var entityConfigData = UserUtil.GetMainData<MainDataTypes.AllConfigs>();
            UpdateWindow(entityConfigData.AsOk());
        }
    }
    private void UpdateWindow(Data<DataTypes.ActionState> state)
    {
        var principalResult = UserUtil.GetPrincipal();

        if (principalResult.IsErr)
        {
            principalResult.AsErr().Value.Error();

            return;
        }

        if (UserUtil.IsDataValid<DataTypes.Entity>(principalResult))
        {
            var entityConfigData = UserUtil.GetMainData<MainDataTypes.AllConfigs>();
            UpdateWindow(entityConfigData.AsOk());
        }
    }

    private void UpdateWindow(MainDataTypes.AllConfigs state)
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        if (!ConfigUtil.TryGetConfig(BoomManager.Instance.WORLD_CANISTER_ID, listOfValidActionsConfig, out var config))
        {
            ("Could not find config of id: " + listOfValidActionsConfig).Error();
            return;
        }

        var configFields = config.fields;

        configFields.Iterate(e =>
        {
            string actionId = e.Key;
            string actionType = e.Value;

            if (!ConfigUtil.TryGetConfig(BoomManager.Instance.WORLD_CANISTER_ID, actionId, out var actionConfig))
            {
                ("Could not find config of id: " + listOfValidActionsConfig).Error();
                return;
            }

            actionConfig.fields.TryGetValue("name", out string name);
            actionConfig.fields.TryGetValue("description", out string description);
            actionConfig.fields.TryGetValue("imageUrl", out string imageUrl);

            if (!ConfigUtil.TryGetAction(actionId, out var action, BoomManager.Instance.WORLD_CANISTER_ID))
            {
                ("Could not find action of id: " + actionId).Error();

                return;
            }

            var callerSubAction = action.callerAction;


            if (actionType == "trade")
            {
                if(!ConfigUtil.TryGetActionPart<List<EntityConstrainTypes.Base>>(actionId, e => e.callerAction.EntityConstraints, out var entityConstraints))
                {
                    ("Could not find entities constraints of action of id: " + actionId).Error();

                    return;
                }

                var constrain = entityConstraints.Reduce(e =>
                {
                    if (e.ConstrainType != $"{nameof(EntityConstrainTypes.GreaterThanEqualToNumber)}")
                    {
                        return $"ConstrainType Error! " + e.ConstrainType;
                    }
                    double amount = (double)e.GetValue();
                    string name = e.GetKey();

                    if (ConfigUtil.TryGetConfigFieldAs<string>(BoomManager.Instance.WORLD_CANISTER_ID, e.Eid, "name", out var configName)) name = configName;

                    return $"{name} x {amount}\n\n";
                }, "\n");

                ActionWidget aw = WindowManager.Instance.AddWidgets<ActionWidget>(new ActionWidget.WindowData()
                {
                    id = actionId,
                    textButtonContent = $"Trade Item",
                    content = $"{name}\n\nItemA x 1 -> ItemC x 1",
                    action = (actionId, customData) => { Trade(actionId, "constraints").Forget(); },
                    imageContentType = new ImageContentType.Url(imageUrl),
                    infoWindowData = new InfoPopupWindow.WindowData(name, description)
                }, content);
            }
            else if (actionType == "verifyICP")
            {
                if (!ConfigUtil.TryGetActionPart<List<IcrcTx>>(actionId, e => e.callerAction.IcrcConstraint, out var txConstraints))
                {
                    ("Could not find icrc constraint of action of id: " + actionId).Error();

                    return;
                }

                if (txConstraints.Count == 0)
                {
                    ("empty icrc constraint of action of id: " + actionId).Error();

                    return;
                }

                var txConstraint = txConstraints[0];

                ActionWidget aw = WindowManager.Instance.AddWidgets<ActionWidget>(new ActionWidget.WindowData()
                {
                    id = actionId,
                    textButtonContent = $"{(actionId == "spend_icp_to_mint_test_nft" ? "Mint Nft" : "Buy Item")}",
                    content = $"{name}\n\nprice: {txConstraint.Amount} ICP",
                    action = (actionId, customData) => { BuyWithIcp(actionId, txConstraint).Forget(); },
                    imageContentType = new ImageContentType.Url(imageUrl),
                    infoWindowData = new InfoPopupWindow.WindowData(name, description)
                }, content);
            }
            else if (actionType == "verifyICRC")
            {
                if (!ConfigUtil.TryGetActionPart<List<IcrcTx>>(actionId, e => e.callerAction.IcrcConstraint, out var txConstraints))
                {
                    ("Could not find icrc constraint of action of id: " + actionId).Error();

                    return;
                }

                if(txConstraints.Count == 0)
                {
                    ("empty icrc constraint of action of id: " + actionId).Error();

                    return;
                }

                var txConstraint = txConstraints[0];

                ConfigUtil.TryGetTokenConfig(txConstraint.Canister, out var tokenConfig);

                var tokenName = tokenConfig != null ? tokenConfig.name : "ICRC";

                ActionWidget aw = WindowManager.Instance.AddWidgets<ActionWidget>(new ActionWidget.WindowData()
                {
                    id = actionId,
                    textButtonContent = $"Buy Item",
                    content = $"{name}\n\nprice: {txConstraint.Amount} {tokenName}", //{config.Amt.ToString("0." + new string('#', 339))}
                    action = (actionId, customData) => { BuyWithIcrc(actionId, txConstraint).Forget(); }
                    ,
                    imageContentType = new ImageContentType.Url(imageUrl),
                    infoWindowData = new InfoPopupWindow.WindowData(name, description)
                }, content);
            }
            else if (actionType == "verifyNftBurn")
            {
                if (!ConfigUtil.TryGetActionPart(actionId, e => e.callerAction.Outcomes, out var outcomes))
                {
                    ("Could not find action result of action of id: " + actionId).Error();

                    return;
                }
                if (!ConfigUtil.TryGetActionPart<List<NftTx>>(actionId, e => e.callerAction.NftConstraint, out var txConstraints))
                {
                    ("Could not find nft constraint of action of id: " + actionId).Error();

                    return;
                }

                if (txConstraints.Count == 0)
                {
                    ("empty nft constraint of action of id: " + actionId).Error();

                    return;
                }

                var txConstraint = txConstraints[0];
                var possibleOutcomes = outcomes[0].PossibleOutcomes;

                string possibleOutcoemsContent = possibleOutcomes.Filter(e =>
                {
                    PossibleOutcomeTypes.UpdateEntity update = e switch
                    {
                        PossibleOutcomeTypes.UpdateEntity r => r,
                        _ => null
                    };

                    if (update == null) return false;

                    var numericUpdates = update.QueryNumericFields(EntityFieldEdit.Numeric.NumericType.Increment);

                    return numericUpdates.Count > 0;
                }).Reduce(k =>
                {
                    PossibleOutcomeTypes.UpdateEntity update = k switch
                    {
                        PossibleOutcomeTypes.UpdateEntity r => r,
                        _ => null
                    };
                    var numericUpdates = update.QueryNumericFields(EntityFieldEdit.Numeric.NumericType.Increment);

                    var asIncrementNumber = numericUpdates.First();

                    ConfigUtil.TryGetConfigFieldAs<string>(BoomManager.Instance.WORLD_CANISTER_ID, update.Eid, "name", out var configName, update.Eid);

                    return $"{configName} x {(asIncrementNumber.HasFormulas == false? asIncrementNumber.Value : "some formula")}";
                });


                ActionWidget aw = WindowManager.Instance.AddWidgets<ActionWidget>(new ActionWidget.WindowData()
                {
                    id = actionId,
                    textButtonContent = $"Burn Nft",
                    content = $"Nft Burn Posible Rewards:\n\n{possibleOutcoemsContent}",
                    action = (actionId, customData) => { BurnNftHandler(actionId, txConstraint); }
                  ,
                    imageContentType = new ImageContentType.Url(imageUrl),
                    infoWindowData = new InfoPopupWindow.WindowData(name, description)
                }, content);
            }
            else if (actionType == "verifyNftHold")
            {
                if (!ConfigUtil.TryGetActionPart(actionId, e => e.callerAction.Outcomes, out var outcomes))
                {
                    ("Could not find action result of action of id: " + actionId).Error();

                    return;
                }
                if (!ConfigUtil.TryGetActionPart<List<NftTx>>(actionId, e => e.callerAction.NftConstraint, out var txConstraints))
                {
                    ("Could not find nft constraint of action of id: " + actionId).Error();

                    return;
                }

                if (txConstraints.Count == 0)
                {
                    ("empty nft constraint of action of id: " + actionId).Error();

                    return;
                }

                var txConstraint = txConstraints[0];
                var possibleOutcomes = outcomes[0].PossibleOutcomes;

                string possibleOutcoemsContent = possibleOutcomes.Filter(e =>
                {
                    PossibleOutcomeTypes.UpdateEntity update = e switch
                    {
                        PossibleOutcomeTypes.UpdateEntity r => r,
                        _ => null
                    };

                    if (update == null) return false;

                    var numericUpdates = update.QueryNumericFields(EntityFieldEdit.Numeric.NumericType.Increment);

                    return numericUpdates.Count > 0;
                }).Reduce(k =>
                {
                    PossibleOutcomeTypes.UpdateEntity update = k switch
                    {
                        PossibleOutcomeTypes.UpdateEntity r => r,
                        _ => null
                    };
                    var numericUpdates = update.QueryNumericFields(EntityFieldEdit.Numeric.NumericType.Increment);

                    var asIncrementNumber = numericUpdates.First();

                    ConfigUtil.TryGetConfigFieldAs<string>(BoomManager.Instance.WORLD_CANISTER_ID, update.Eid, "name", out var configName, update.Eid);

                    return $"{configName} x {(asIncrementNumber.HasFormulas == false ? asIncrementNumber.Value : "some formula")}";
                });


                ActionWidget aw = WindowManager.Instance.AddWidgets<ActionWidget>(new ActionWidget.WindowData()
                {
                    id = actionId,
                    textButtonContent = $"Check Nft Holdings",
                    content = $"Nft Hold Posible Rewards:\n\n{possibleOutcoemsContent}",
                    action = (actionId, customData) => { CheckHoldNftHandler(actionId, txConstraint); }
                  ,
                    imageContentType = new ImageContentType.Url(imageUrl),
                    infoWindowData = new InfoPopupWindow.WindowData(name, description)
                }, content);
            }
            else
            {
                ("action type not handled: " + actionType).Error();

            }
        });
    }

    private void BurnNftHandler(string actionid, NftTx tx)
    {
        BurnNft(actionid, tx).Forget();
    }
    private void CheckHoldNftHandler(string actionid, NftTx tx)
    {
        CheckHoldNft(actionid, tx).Forget();
    }

    private async UniTaskVoid BurnNft(string actionid, NftTx tx)
    {
        //await UniTask.SwitchToMainThread();

        BroadcastState.Invoke(new WaitingForResponse(true));

        var principalResult = UserUtil.GetPrincipal();

        if (principalResult.Tag == UResultTag.Err)
        {
            principalResult.AsErr().Value.Error();
            return;
        }
        var principal = principalResult.AsOk().Value;

        if (tx.NftConstraintType.Tag == NftTx.NftConstraintTypeInfoTag.Hold)
        {
            WindowManager.Instance.OpenWindow<InfoPopupWindow>(new InfoPopupWindow.WindowData(">Some other issue!", "You cannot burn a nft for a hold constraint"), 3);
            BroadcastState.Invoke(new WaitingForResponse(false));
            return;
        }
        var transferConstraint = tx.NftConstraintType.AsTransfer();

        var nextNftIndexResult = NftUtil.TryGetNextNft(principal, tx.Canister);

        if (nextNftIndexResult.IsErr)
        {
            WindowManager.Instance.OpenWindow<InfoPopupWindow>(new InfoPopupWindow.WindowData(">>Some other issue!", nextNftIndexResult.AsErr()), 3);
            BroadcastState.Invoke(new WaitingForResponse(false));

            return;
        }

        var nextNft = nextNftIndexResult.AsOk();

        var transferResult = await ActionUtil.Transfer.TransferNft(tx.Canister, nextNft.tokenIdentifier, transferConstraint.ToPrincipal);

        if (transferResult.IsErr)
        {
            WindowManager.Instance.OpenWindow<InfoPopupWindow>(new InfoPopupWindow.WindowData(">>>Some other issue!", transferResult.AsErr().content), 3);
            BroadcastState.Invoke(new WaitingForResponse(false));

            return;
        }

        var result = await ActionUtil.ProcessAction(actionid);

        if (result.Tag == UResultTag.Err)
        {
            result.AsErr().content.Error();

            switch (result.AsErr())
            {
                case ActionErrType.InsufficientBalance content:
                    Window infoPopup = null;
                    infoPopup = WindowManager.Instance.OpenWindow<InfoPopupWindow>(
                    new InfoPopupWindow.WindowData(
                        $"You don't a nft to burn",
                        $"{content.content}",
                        new(
                            new(
                                $"Mint a Nft",
                                () =>
                                {
                                    if (infoPopup != null) infoPopup.Close();
                                    Close();
                                    WindowManager.Instance.OpenWindow<MintTestTokensWindow>(null, 1);
                                }
                               )
                            )),
                        3);
                    break;
                default:
                    WindowManager.Instance.OpenWindow<InfoPopupWindow>(new InfoPopupWindow.WindowData("Some other issue!", result.AsErr().Content), 3);
                    break;
            }

            BroadcastState.Invoke(new WaitingForResponse(false));
            return;
        }

        var resultAsOk = result.AsOk();

        DisplayActionResponse(resultAsOk);

        BroadcastState.Invoke(new WaitingForResponse(false));
    }
    private async UniTaskVoid CheckHoldNft(string actionid, NftTx tx)
    {
        //await UniTask.SwitchToMainThread();

        var principalResult = UserUtil.GetPrincipal();

        if (principalResult.Tag == UResultTag.Err)
        {
            principalResult.AsErr().Value.Error();
            return;
        }
        var principal = principalResult.AsOk().Value;

        BroadcastState.Invoke(new WaitingForResponse(true));


        var nextNftIndexResult = NftUtil.TryGetNextNftIndex(principal, tx.Canister);

        if (nextNftIndexResult.IsErr)
        {
            WindowManager.Instance.OpenWindow<InfoPopupWindow>(new InfoPopupWindow.WindowData("Some other issue!", nextNftIndexResult.AsErr()), 3);
            BroadcastState.Invoke(new WaitingForResponse(false));

            return;
        }

        var result = await ActionUtil.ProcessAction(actionid);

        if (result.Tag == UResultTag.Err)
        {
            result.AsErr().content.Error();

            switch (result.AsErr())
            {
                case ActionErrType.InsufficientBalance content:
                    Window infoPopup = null;
                    infoPopup = WindowManager.Instance.OpenWindow<InfoPopupWindow>(
                    new InfoPopupWindow.WindowData(
                        $"You don't a nft to burn",
                        $"{content.content}",
                        new(
                            new(
                                $"Mint a Nft",
                                () =>
                                {
                                    if (infoPopup != null) infoPopup.Close();
                                    Close();
                                    WindowManager.Instance.OpenWindow<MintTestTokensWindow>(null, 1);
                                }
                               )
                            )),
                        3);
                    break;
                default:
                    WindowManager.Instance.OpenWindow<InfoPopupWindow>(new InfoPopupWindow.WindowData("Some other issue!", result.AsErr().Content), 3);
                    break;
            }

            BroadcastState.Invoke(new WaitingForResponse(false));
            return;
        }

        var resultAsOk = result.AsOk();

        DisplayActionResponse(resultAsOk);

        BroadcastState.Invoke(new WaitingForResponse(false));
    }


    private async UniTaskVoid Trade(string actionId, string constrain)
    {
        BroadcastState.Invoke(new WaitingForResponse(true));

        var actionResult = await ActionUtil.ProcessAction(actionId);

        if (actionResult.Tag == UResultTag.Err)
        {
            var actionError = actionResult.AsErr();

            switch (actionError)
            {
                case ActionErrType.EntityConstrain content:
                    WindowManager.Instance.OpenWindow<InfoPopupWindow>(new InfoPopupWindow.WindowData("Oops you dont meet requirements", $"Requirements:\n{constrain}"), 3);
                    break;
                case ActionErrType.ActionsPerInterval content:
                    WindowManager.Instance.OpenWindow<InfoPopupWindow>(new InfoPopupWindow.WindowData("Time constrain!", content.Content), 3);
                    break;
                default:
                    WindowManager.Instance.OpenWindow<InfoPopupWindow>(new InfoPopupWindow.WindowData("Other issue!", actionResult.AsErr().content), 3);
                    break;
            }

            BroadcastState.Invoke(new WaitingForResponse(false));
            return;
        }

        var resultAsOk = actionResult.AsOk();

        DisplayActionResponse(resultAsOk);

        BroadcastState.Invoke(new WaitingForResponse(false));
    }
    private async UniTaskVoid BuyWithIcp(string actionId, IcrcTx icpConstraint)
    {
        BroadcastState.Invoke(new WaitingForResponse(true));

        var actionResult = await ActionUtil.ProcessAction(actionId);

        //CHECK FOR ERR
        if (actionResult.Tag == UResultTag.Err)
        {
            var actionError = actionResult.AsErr();

            switch (actionError)
            {
                case ActionErrType.InsufficientBalance content:
                    WindowManager.Instance.OpenWindow<InfoPopupWindow>(new InfoPopupWindow.WindowData("You don't have enough ICP", $"Requirements:\n{$"{icpConstraint.Amount} ICP\n\nYou need to deposit some ICP"}"), 3);
                    break;
                case ActionErrType.ActionsPerInterval content:
                    WindowManager.Instance.OpenWindow<InfoPopupWindow>(new InfoPopupWindow.WindowData("Time constrain!", content.Content), 3);
                    break;
                default:
                    WindowManager.Instance.OpenWindow<InfoPopupWindow>(new InfoPopupWindow.WindowData("Other issue!", actionResult.AsErr().content), 3);
                    break;
            }

            BroadcastState.Invoke(new WaitingForResponse(false));
            return;
        }

        var resultAsOk = actionResult.AsOk();

        DisplayActionResponse(resultAsOk);

        BroadcastState.Invoke(new WaitingForResponse(false));
    }
    private async UniTaskVoid BuyWithIcrc(string actionId, IcrcTx icrcConstraint)
    {
        var principalResult = UserUtil.GetPrincipal();

        if (principalResult.Tag == UResultTag.Err)
        {
            principalResult.AsErr().Value.Error();
            return;
        }
        var principal = principalResult.AsOk().Value;

        BroadcastState.Invoke(new WaitingForResponse(true));

        var tokenSymbol = "ICRC";
        var userBalance = 0D;


        var tokenAndConfigsResult = TokenUtil.GetTokenDetails(principal, icrcConstraint.Canister);

        if (tokenAndConfigsResult.Tag == UResultTag.Ok)
        {
            var (token, tokenConfigs) = tokenAndConfigsResult.AsOk();

            tokenSymbol = tokenConfigs.symbol;
            userBalance = token.baseUnitAmount.ConvertToDecimal(tokenConfigs.decimals);
        }
        $"Required ICRC: {icrcConstraint.Amount} Balance: {userBalance}".Log();

        var actionResult = await ActionUtil.ProcessAction(actionId);

        //CHECK FOR ERR
        if (actionResult.Tag == UResultTag.Err)
        {
            var actionError = actionResult.AsErr();

            switch (actionError)
            {
                case ActionErrType.InsufficientBalance content:

                    Window infoPopup = null;
                    infoPopup = WindowManager.Instance.OpenWindow<InfoPopupWindow>(
                        new InfoPopupWindow.WindowData(
                    $"You don't have enough {tokenSymbol}",
                            $"Requirements:\n{$"{tokenSymbol} x {icrcConstraint.Amount}"}\n\nYou need to mint more \"{tokenSymbol}\"",
                            new(new($"Mint ${tokenSymbol}", () =>
                            {
                                if (infoPopup != null) infoPopup.Close();
                                Close();
                                WindowManager.Instance.OpenWindow<MintTestTokensWindow>(null, 1);
                            }
                    ))), 3);
                    break;
                case ActionErrType.ActionsPerInterval content:
                    WindowManager.Instance.OpenWindow<InfoPopupWindow>(new InfoPopupWindow.WindowData("Time constrain!", content.Content), 3);
                    break;
                default:
                    WindowManager.Instance.OpenWindow<InfoPopupWindow>(new InfoPopupWindow.WindowData("Other issue! " + actionResult.AsErr().GetType().Name, actionResult.AsErr().content), 3);
                    break;
            }

            BroadcastState.Invoke(new WaitingForResponse(false));
            return;
        }

        var resultAsOk = actionResult.AsOk();

        DisplayActionResponse(resultAsOk);

        BroadcastState.Invoke(new WaitingForResponse(false));
    }

    private void DisplayActionResponse(ProcessedActionResponse resonse)
    {
        List<string> inventoryElements = new();

        //NFTs
        Dictionary<string, int> collectionsToDisplay = new();

        if (resonse.callerOutcomes == null) return;


        resonse.callerOutcomes.nfts.Iterate(e =>
        {

            if (collectionsToDisplay.TryAdd(e.Canister, 1) == false) collectionsToDisplay[e.Canister] += 1;

        });

        collectionsToDisplay.Iterate(e =>
        {
            if (ConfigUtil.TryGetNftCollectionConfig(e.Key, out var collectionConfig) == false)
            {
                return;
            }

            inventoryElements.Add($"{(collectionConfig != null ? collectionConfig.name : "Name not Found")} x {e.Value}");
        });

        //Tokens
        resonse.callerOutcomes.tokens.Iterate(e =>
        {
            if (ConfigUtil.TryGetTokenConfig(e.Canister, out var tokenConfig) == false)
            {
                return;
            }

            inventoryElements.Add($"{(tokenConfig != null? tokenConfig.name : "ICRC")} x {e.Quantity}");
        });


        //ENTITIES
        resonse.callerOutcomes.entityOutcomes.Iterate(e =>
        {
            //NEW EDIT
            //if (e.Value.fields.Has(k =>
            //{
            //    if (k.Value is EntityFieldEdit.Numeric numericOutcome) return numericOutcome.NumericType_ == EntityFieldEdit.Numeric.NumericType.Increment;
            //    return false;
            //}) == false) return;

            if (!e.Value.TryGetOutcomeFieldAsDouble("quantity", out var quantity)) return;

            //NEW EDIT
            string displayValue = "";
            if (quantity.NumericType_ == EntityFieldEdit.Numeric.NumericType.Set) displayValue = $"{quantity.Value}";
            else if (quantity.NumericType_ == EntityFieldEdit.Numeric.NumericType.Increment) displayValue = $"+ {quantity.Value}";
            else displayValue = $"- {quantity.Value}";

            if (!ConfigUtil.TryGetConfigFieldAs<string>(BoomManager.Instance.WORLD_CANISTER_ID, e.Value.eid, "name", out var configName)) return;

            if (e.Value.TryGetConfig(BoomManager.Instance.WORLD_CANISTER_ID, out var config)) inventoryElements.Add($"{configName} {displayValue}");
            else inventoryElements.Add($"{e.Value.GetKey()} {displayValue}");
        });

        WindowManager.Instance.OpenWindow<InventoryPopupWindow>(new InventoryPopupWindow.WindowData("Earned Items", inventoryElements), 3);
    }
}