// File: src/services/signalr.js
import * as signalR from '@microsoft/signalr';

class SignalRService {
    constructor() {
        this.connection = null;
        this.listeners = [];
    }

    async start() {
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            return;
        }

        const API_URL = process.env.REACT_APP_API_URL || 'https://localhost:7002';
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(`${API_URL}/hubs/comments`, {
                withCredentials: true,
                skipNegotiation: true,
                transport: signalR.HttpTransportType.WebSockets
            })
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        this.connection.on('CommentAdded', (comment) => {
            this.listeners.forEach(listener => listener.onCommentAdded?.(comment));
        });

        this.connection.on('CommentDeleted', (commentId) => {
            this.listeners.forEach(listener => listener.onCommentDeleted?.(commentId));
        });

        try {
            await this.connection.start();
            console.log('SignalR Connected');
        } catch (err) {
            console.error('SignalR Connection Error: ', err);
            setTimeout(() => this.start(), 5000);
        }
    }

    on(event, callback) {
        this.listeners.push({ [event]: callback });
    }

    off(event, callback) {
        this.listeners = this.listeners.filter(l => l[event] !== callback);
    }
}

export const signalRService = new SignalRService();