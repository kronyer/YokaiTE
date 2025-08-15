function exportPdfJs(title, content) {
    const { jsPDF } = window.jspdf;
    const doc = new jsPDF();
    doc.setFontSize(13);
    doc.text(content, 10, 40);
    doc.save(title + ".pdf");
}