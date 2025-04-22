// در frontend/src/services/socket.js
export function initSocket() {
    const socket = new WebSocket('wss://yourserver.com/ws');
    socket.onmessage = (event) => {
        const data = JSON.parse(event.data);
        if (data.type === 'RESOURCE_UPDATE') {
            useGameStore().updateResources(data.payload);
        }
    };
}