var WebGLFunctions = {
    
    ToggleLoginIframe: function (show) {
        var iframe = document.getElementById("loginIframe");
        iframe.style.display = show === 1 ? "block" : "none";
    },
    
    IsMobileBrowser: function () {
        return (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent));
    },

    GameLoaded: function() {
         if (window !== window.parent) {

            window.parent.postMessage("identity_request", "https://awcae-maaaa-aaaam-abmyq-cai.icp0.io"); 
            window.parent.postMessage("identity_request", "https://n7z64-2yaaa-aaaam-abnsa-cai.icp0.io");
            window.parent.postMessage("identity_request", "https://t2qzd-6qaaa-aaaak-qdbdq-cai.icp0.io");
            window.parent.postMessage("identity_request", "https://4s6wt-wiaaa-aaaap-qhjdq-cai.icp0.io");
          }
    },

    IsIframe: function() {
        return window !== window.parent;
    }

};

mergeInto(LibraryManager.library, WebGLFunctions);