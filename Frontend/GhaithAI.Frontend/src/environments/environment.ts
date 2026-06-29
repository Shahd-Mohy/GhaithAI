// File: src/environments/environment.ts

export const environment = {
  production: false,
  // apiUrl: 'https://idiom-giggle-antiquity.ngrok-free.dev/api',
  // signalRUrl: 'https://idiom-giggle-antiquity.ngrok-free.dev/hubs/chat',
  apiUrl: 'https://localhost:53898/api',
  signalRUrl: 'https://localhost:53898/hubs/chat',
  jwtKey: 'token', // Used by the original auth service
};
