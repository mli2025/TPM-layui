/**
 * Arbore Shell - 统一扫码入口
 *
 * 使用方式：
 *   ArboreShell.scan().then(code => { ... }).catch(err => { ... });
 *
 * 行为：
 *   - Android 壳内（window.AndroidBridge.scan 存在）走原生 ZXing 扫码
 *   - 其它环境：交给调用方提供的 onWebFallback；不提供则 reject NO_BRIDGE
 *
 * 错误 message 约定：
 *   CANCELLED            用户取消
 *   PERMISSION_REQUESTED 已申请摄像头权限，等用户授权后再点一次
 *   NOT_READY            壳未初始化完成
 *   NO_BRIDGE            非壳环境且未提供 fallback
 *
 * 同时注入 cordova.plugins.barcodeScanner.scan 兼容垫片，让旧 hybrid 代码
 * （`if ('_cordovaNative' in window) { cordova.plugins.barcodeScanner.scan(ok, err) }`）
 * 在 v3 壳里自动走原生扫码，无需逐处改写。
 */
(function () {
    if (window.ArboreShell && window.ArboreShell.scan) return;

    var ArboreShell = window.ArboreShell || {};
    window.ArboreShell = ArboreShell;

    var ua = navigator.userAgent || '';
    ArboreShell.isShell = /ArboreShell\//i.test(ua);
    ArboreShell.hasNativeScan = !!(window.AndroidBridge
        && typeof window.AndroidBridge.scan === 'function');

    var __resolvers = {};

    window.__arboreScanResult = function (token, code, err) {
        var r = __resolvers[token];
        if (!r) return;
        delete __resolvers[token];
        if (err) r.reject(new Error(err));
        else r.resolve(code || '');
    };

    function nativeScan() {
        return new Promise(function (resolve, reject) {
            if (!ArboreShell.hasNativeScan) {
                reject(new Error('NO_BRIDGE'));
                return;
            }
            var token = 's_' + Date.now() + '_' + Math.floor(Math.random() * 1e6);
            __resolvers[token] = { resolve: resolve, reject: reject };
            try { window.AndroidBridge.scan(token); }
            catch (e) {
                delete __resolvers[token];
                reject(e instanceof Error ? e : new Error(String(e)));
            }
        });
    }

    /**
     * 触发扫码。若在壳内，直接拉起原生 ZXing 取景框；否则调用 opts.onWebFallback。
     * @param {{ onWebFallback?: () => Promise<string> }} [opts]
     * @returns {Promise<string>} 扫码结果文本（去空白由调用方处理）
     */
    ArboreShell.scan = function (opts) {
        opts = opts || {};
        if (ArboreShell.hasNativeScan) return nativeScan();
        if (typeof opts.onWebFallback === 'function') {
            try { return Promise.resolve(opts.onWebFallback()); }
            catch (e) { return Promise.reject(e); }
        }
        return Promise.reject(new Error('NO_BRIDGE'));
    };

    // Cordova 兼容垫片：把 cordova.plugins.barcodeScanner.scan 接到原生桥上
    // 仅在壳内注入，避免污染浏览器环境
    if (ArboreShell.hasNativeScan) {
        // 旧代码用 'in window' 来探测 cordova 是否注入，这里给个最小标记
        try { Object.defineProperty(window, '_cordovaNative', { value: true, configurable: true }); }
        catch (_e) { window._cordovaNative = true; }

        window.cordova = window.cordova || {};
        window.cordova.plugins = window.cordova.plugins || {};
        if (!window.cordova.plugins.barcodeScanner) {
            window.cordova.plugins.barcodeScanner = {
                scan: function (onSuccess, onError /*, options */) {
                    nativeScan().then(function (code) {
                        if (typeof onSuccess === 'function') {
                            onSuccess({ text: code, format: '', cancelled: false });
                        }
                    }).catch(function (err) {
                        var msg = err && err.message ? err.message : 'SCAN_FAILED';
                        if (msg === 'CANCELLED') {
                            if (typeof onSuccess === 'function') {
                                onSuccess({ text: '', format: '', cancelled: true });
                            }
                            return;
                        }
                        if (typeof onError === 'function') onError(msg);
                    });
                }
            };
        }
    }
})();
