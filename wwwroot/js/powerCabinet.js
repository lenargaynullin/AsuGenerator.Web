// Хелперы конфигуратора силовых шкафов:
// копирование в буфер обмена (TSV для Excel) и скачивание файла (CSV с BOM).

window.powerCabinet = {
    copyText: function (text) {
        // Возвращает Promise<boolean> — флаг успеха.
        if (navigator.clipboard && window.isSecureContext) {
            return navigator.clipboard.writeText(text).then(
                function () { return true; },
                function () { return window.powerCabinet._legacyCopy(text); }
            );
        }
        return Promise.resolve(window.powerCabinet._legacyCopy(text));
    },

    _legacyCopy: function (text) {
        try {
            var ta = document.createElement('textarea');
            ta.value = text;
            ta.style.position = 'fixed';
            ta.style.opacity = '0';
            document.body.appendChild(ta);
            ta.focus();
            ta.select();
            var ok = document.execCommand('copy');
            document.body.removeChild(ta);
            return ok;
        } catch (e) { return false; }
    },

    downloadFile: function (fileName, mime, content) {
        // content передаётся как строка (для CSV с BOM или TSV).
        var blob = new Blob([content], { type: mime });
        var url = URL.createObjectURL(blob);
        var a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        setTimeout(function () { URL.revokeObjectURL(url); }, 1000);
        return true;
    }
};
