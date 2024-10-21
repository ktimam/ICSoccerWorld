namespace Boom.Mono {
    using System;
    using Boom.Utility;
    using Newtonsoft.Json;
    using UnityEngine;

    [RequireComponent(typeof(BoomActionTrigger))]
    public class BoomActionLog : MonoBehaviour
    {
        private void Awake()
        {
            BoomActionTrigger boomActionTrigger = GetComponent<BoomActionTrigger>();
            boomActionTrigger.OnActionResponseSuccess.AddListener(OnActionSuccessHandler);
            boomActionTrigger.OnActionResponseError.AddListener(OnActionErrorHandler);
        }

        private void OnActionSuccessHandler(ProcessedActionResponse arg0)
        {
            $"Action Success! Logs: {JsonConvert.SerializeObject(arg0)}".Log(typeof(BoomActionLog).Name);
        }

        private void OnActionErrorHandler(ActionErrType.Base arg0)
        {
            $"Action Failure! Logs: {arg0.content}".Error(typeof(BoomActionLog).Name);
        }

    }
}