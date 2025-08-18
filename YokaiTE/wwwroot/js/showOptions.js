function ShowOptionsDocument(event, documentId) {
    var optionsContainer = document.getElementById("context-menu-modal");
    if (optionsContainer) {
        optionsContainer.style.display = "flex";
        var top = event.clientY + window.scrollY;
        var left = event.clientX + window.scrollX;
        left = left - optionsContainer.offsetWidth;
        if (left < 0) left = 0;
        if (top + optionsContainer.offsetHeight > window.innerHeight + window.scrollY)
            top = window.innerHeight + window.scrollY - optionsContainer.offsetHeight;
        if (window.innerWidth < 900) top -= 60;
        optionsContainer.style.top = top + "px";
        optionsContainer.style.left = left + "px";
        // Chame métodos .NET se necessário
    } else {
        window.alert('Element "options-container-document" not found');
    }
}

document.addEventListener('mousedown', function (event) {
    var optionsContainerDocument = document.getElementById("context-menu-modal");
    if (optionsContainerDocument && !optionsContainerDocument.contains(event.target)) {
        optionsContainerDocument.style.display = "none";
    }
});