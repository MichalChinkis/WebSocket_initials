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
        if (!isValidName(name)) {
            errorMessage.textContent = 'אנא הכנס שם תקני בעברית בלבד.';
            return;
        }

        // בדיקת תקינות ראשי התיבות
        if (!isValidInitials(initials)) {
            errorMessage.textContent = 'אנא הכנס ראשי תיבות תקינים: 4 מילים שמתחילות ב- ת, ש, פ, ה';
            return;
        }
        const message = JSON.stringify({ Initials: initials, Name: name });
        ws.send(message);
    } else {
        errorMessage.textContent = 'אנא הכנס שם וראשי תיבות חוקיים.';
    }
}

function isValidInitials(initials) {
    const initialsRegex = /^ת[\u0590-\u05FF]*\sש[\u0590-\u05FF]*\sפ[\u0590-\u05FF]*\sה[\u0590-\u05FF]*$/;
    return initialsRegex.test(initials);
}


function isValidName(name) {
    const hebrewNameRegex = /^[\u0590-\u05FF\s]+$/; // ביטוי רגולרי לשם בעברית בלבד
    return hebrewNameRegex.test(name);
}
