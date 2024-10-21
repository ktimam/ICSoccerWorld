using Boom.UI;
using Boom.Utility;
using Boom.Values;
using TMPro;
using UnityEngine;
using Boom;
using Newtonsoft.Json;
using Candid;
using static Env;

public class BalanceWindow : Window
{
    string icpData;
    string icrcData;
    string nftData;

    [SerializeField] TextMeshProUGUI icpBalanceText;

    public class WindowData
    {
        //DATA
    }
    public override bool RequireUnlockCursor()
    {
        return false;//or true
    }

    public override void Setup(object data)
    {
        icpBalanceText.text = $"ICP: Loading...\nICRC: Loading...\nNFT Count: Loading...";

        UserUtil.AddListenerDataChangeSelf<DataTypes.Token>(UpdateWindow, new() { invokeOnRegistration = true });
        UserUtil.AddListenerDataChangeSelf<DataTypes.NftCollection>(UpdateWindow);
    }
    private void OnDestroy()
    {
        UserUtil.RemoveListenerDataChangeSelf<DataTypes.NftCollection>(UpdateWindow);
        UserUtil.RemoveListenerDataChangeSelf<DataTypes.Token>(UpdateWindow);
    }
    private void UpdateWindow(Data<DataTypes.NftCollection> obj)
    {

        if (UserUtil.IsDataValidSelf<DataTypes.Token>())
        {
           var tokensResult =  UserUtil.GetDataSelf<DataTypes.Token>();

            if (tokensResult.IsOk)
            {
                UpdateWindow(tokensResult.AsOk());
            }
        }
    }

    private void UpdateWindow(Data<DataTypes.Token> obj)
    {
        if (!UserUtil.IsLoggedIn(out var loginData))
        {
            icpBalanceText.text = $"ICP: {0}\nICRC: {0}\nNFT Count: {0}";

            return;
        }

        icpData = "ICP: Loading...";
        icrcData = "ICRC: Loading...";
        nftData = "NFT Count: Loading...";

        //
        var allTokenConfigsResult = ConfigUtil.GetAllTokenConfigs();

        if (allTokenConfigsResult.IsErr)
        {
            $"{allTokenConfigsResult.AsErr()}".Error();
            return;
        }

        var allTokenConfigsAsOk = allTokenConfigsResult.AsOk();

        allTokenConfigsAsOk.Iterate(tc =>
        {
            var tokenAndConfigsResult = TokenUtil.GetTokenAmountAsDecimal(loginData.principal, tc.canisterId);

            if (Env.CanisterIds.ICP_LEDGER == tc.canisterId)
            {
                icpData = $"ICP: {tokenAndConfigsResult.NotScientificNotation()}\n";

            }
            else
            {
                icrcData = $"{tc.name}: {tokenAndConfigsResult.NotScientificNotation()}\n";
            }
        });

        var nftCountResult = NftUtil.GetNftCount(loginData.principal, BoomManager.Instance.WORLD_COLLECTION_CANISTER_ID);

        if (nftCountResult.Tag == Boom.Values.UResultTag.Ok)
        {
            var nftResult = UserUtil.GetDataSelf<DataTypes.NftCollection>();

            nftData = $"NFT Count: {nftCountResult.AsOk()}";
        }


        icpBalanceText.text = $"{icpData}{icrcData}{nftData}";

    }
}