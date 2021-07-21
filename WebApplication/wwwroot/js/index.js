document.getElementById("mainCheckbox").onclick = function checkAll(event) {
    let elements = document.getElementsByClassName("row-checkbox");
    for (var i = 0; i < elements.length; i++) {
        elements[i].checked = event.target.checked;
    }
};
