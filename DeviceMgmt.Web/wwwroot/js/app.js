/* ================================================================
   arbore TPM · 全局 JS 工具
   - 依赖：jQuery 1.x（兼容性需要）/ AlpineJS（交互）/ Lucide（图标）
   ================================================================ */
(function (global) {
  "use strict";

  // ---------- Cookie ----------
  function getCookie(name) {
    var m = document.cookie.match(new RegExp("(?:^|; )" + name.replace(/([.$?*|{}()\[\]\\\/\+^])/g, "\\$1") + "=([^;]*)"));
    return m ? decodeURIComponent(m[1]) : null;
  }
  function setCookie(name, value, days) {
    var d = new Date();
    d.setTime(d.getTime() + (days || 1) * 24 * 60 * 60 * 1000);
    document.cookie = name + "=" + encodeURIComponent(value) + "; expires=" + d.toUTCString() + "; path=/";
  }
  function delCookie(name) {
    document.cookie = name + "=; expires=Thu, 01 Jan 1970 00:00:01 GMT; path=/";
  }

  // ---------- Query string ----------
  function getQuery(name) {
    var r = new RegExp("(^|[?&])" + name + "=([^&]*)").exec(window.location.search);
    return r ? decodeURIComponent(r[2]) : null;
  }

  // ---------- Token 注入到 jQuery ajax ----------
  function setupAjax() {
    if (typeof $ === "undefined" || !$.ajaxSetup) return;
    var token = getCookie("Token");
    $.ajaxSetup({ headers: token ? { Token: token } : {} });
  }

  // ---------- Toast ----------
  var toastStack = null;
  function ensureToastStack() {
    if (!toastStack) {
      toastStack = document.createElement("div");
      toastStack.className = "toast-stack";
      document.body.appendChild(toastStack);
    }
    return toastStack;
  }
  function toast(type, msg, duration) {
    var stack = ensureToastStack();
    var el = document.createElement("div");
    el.className = "toast toast-" + (type || "info");
    el.textContent = msg;
    stack.appendChild(el);
    setTimeout(function () { el.style.opacity = "0"; el.style.transition = "opacity .25s"; }, duration || 2500);
    setTimeout(function () { stack.removeChild(el); }, (duration || 2500) + 280);
  }

  // ---------- Theme ----------
  function initTheme() {
    var t = localStorage.getItem("theme") || "light";
    document.documentElement.setAttribute("data-theme", t);
  }
  function toggleTheme() {
    var cur = document.documentElement.getAttribute("data-theme") || "light";
    var next = cur === "light" ? "dark" : "light";
    document.documentElement.setAttribute("data-theme", next);
    localStorage.setItem("theme", next);
    return next;
  }

  // ---------- Lucide refresh ----------
  function refreshIcons() {
    if (global.lucide && typeof global.lucide.createIcons === "function") {
      global.lucide.createIcons();
    }
  }

  // ---------- 全局命名空间 ----------
  global.App = {
    getCookie: getCookie,
    setCookie: setCookie,
    delCookie: delCookie,
    getQuery: getQuery,
    setupAjax: setupAjax,
    toast: toast,
    success: function (m, d) { toast("success", m, d); },
    error:   function (m, d) { toast("error",   m, d); },
    warn:    function (m, d) { toast("warning", m, d); },
    info:    function (m, d) { toast("info",    m, d); },
    initTheme: initTheme,
    toggleTheme: toggleTheme,
    refreshIcons: refreshIcons
  };

  // 启动顺序：先设主题（避免闪烁） → 等 DOM 完成后挂 ajax token & 渲染图标
  initTheme();
  document.addEventListener("DOMContentLoaded", function () {
    setupAjax();
    refreshIcons();
  });
})(window);
