const ws = new WebSocket('wss://localhost:7171/ws');

const initialsListElement = document.getElementById('initials-list');
const errorMessage = document.getElementById('error-message');

ws.onmessage = (event) => {
    const data = JSON.parse(event.data);
    initialsListElement.innerHTML = '';
    data.forEach(entry => {
        const listItem = document.createElement('li');
        listItem.textContent = `${entry.Name}: ${entry.Initials}`;
        initialsListElement.appendChild(listItem);
    });
    errorMessage.textContent = '';
};

function placeBid() {
    const name = document.getElementById('name').value;
    const initials = document.getElementById('bid').value;

    if (name && initials) {
        const message = JSON.stringify({ Initials: initials, Name: name });
        ws.send(message);
    } else {
        errorMessage.textContent = 'אנא הכנס שם וראשי תיבות חוקיים.';
    }
}
