var container;
document.addEventListener('DOMContentLoaded', () => {
    container = document.getElementById("container");
});

function addFloatTip(id, key, action, x, y){
    if(document.getElementById(id)) return;

    container.innerHTML += `
        <div class="float-tip" id="${id}" style="top:${y}px;left:${x}px;">
            <h1>${key}</h1>
            <p> - ${action}</p>
        </div>
    `;
}

function setScreenPosition(id, x, y){
    let doc = document.getElementById(id);
    doc.style.left = x + 'px';
    doc.style.top = y + 'px';
}