import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'

export function createTaskHubConnection(accessTokenFactory: () => string | Promise<string>) {
  return new HubConnectionBuilder()
    .withUrl('/hubs/tasks', { accessTokenFactory })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build()
}
