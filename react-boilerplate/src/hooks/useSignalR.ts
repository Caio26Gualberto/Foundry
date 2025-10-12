import { useEffect, useState } from "react";
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

export const useSignalR = () => {
  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);

  useEffect(() => {
    if (connection) return;

    const token = localStorage.getItem("auth_token");

    const newConnection = new HubConnectionBuilder()
      .withUrl("https://localhost:7265/hubs/systemNotification", {
        accessTokenFactory: () => token ?? "",
        withCredentials: true,
      })
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect()
      .build();

    setConnection(newConnection);

  }, [connection]);

  useEffect(() => {
    if (!connection) return;

    const startConnection = async () => {
      try {
        await connection.start();
        console.log("✅ Connected to SignalR hub");
        setIsConnected(true);
      } catch (err) {
        console.error("❌ Error connecting to hub:", err);
        setTimeout(startConnection, 5000);
      }
    };

    startConnection();

    connection.onclose(() => {
      console.log("⚠️ Connection closed");
      setIsConnected(false);
    });

    connection.on("NotificationsUpdated", () => {
      console.log("🔔 Notifications updated!");
    });

    return () => {
      connection.stop();
    };
  }, [connection]);

  return { connection, isConnected };
};
