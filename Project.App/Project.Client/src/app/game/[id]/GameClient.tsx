'use client';

import { useEffect, useState, useRef, FormEvent } from 'react';
import { useRouter } from 'next/navigation';
import { createSSEListener, GameStateSetters } from '@/lib/sse/GameEventHandler';
import { Room, User, RoomPlayer, GameConfig } from '@/lib/types/core';
import { ChatEventData } from '@/lib/types/events';
import { GameState } from '@/lib/types/game';
import { getGameHandler } from '@/lib/game/GameRegistry';

interface GameClientProps {
  roomId: string;
}

export default function GameClient({ roomId }: GameClientProps) {
  const router = useRouter();
  const [room, setRoom] = useState<Room | null>(null);
  const [user, setUser] = useState<User | null>(null);
  const [roomPlayers, setRoomPlayers] = useState<RoomPlayer[]>([]);
  const [gameState, setGameState] = useState<GameState | null>(null);
  const [gameConfig, setGameConfig] = useState<GameConfig | null>(null);
  const [messages, setMessages] = useState<ChatEventData[]>([]);
  const [chatMessage, setChatMessage] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const eventSourceRef = useRef<EventSource | null>(null);

  const API_URL = process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7069';

  // Fetch room players
  const fetchRoomPlayers = async () => {
    try {
      const playersRes = await fetch(`${API_URL}/api/room/${roomId}/players`, {
        credentials: 'include',
        cache: 'no-store', // Force fresh data, no caching
      });
      if (playersRes.ok) {
        const playersData = await playersRes.json();
        console.log('[GameClient] Players fetched:', playersData);
        console.log('[GameClient] Player count:', playersData.length);
        console.log(
          '[GameClient] Player balances:',
          playersData.map((p) => ({ id: p.userId, balance: p.balance })),
        );
        setRoomPlayers(playersData);
      }
    } catch (e) {
      console.error('Failed to fetch room players:', e);
    }
  };

  // Fetch initial room data and user info
  useEffect(() => {
    const fetchInitialData = async () => {
      try {
        // Fetch user
        const userRes = await fetch(`${API_URL}/api/user/me`, {
          credentials: 'include',
        });
        if (!userRes.ok) {
          router.replace('/login');
          return;
        }
        const userData = await userRes.json();
        setUser(userData);

        // Fetch room
        const roomRes = await fetch(`${API_URL}/api/room/${roomId}`, {
          credentials: 'include',
        });
        if (!roomRes.ok) {
          throw new Error('Failed to fetch room');
        }
        const roomData = await roomRes.json();
        setRoom(roomData);

        // Fetch room players
        await fetchRoomPlayers();

        console.log('[GameClient] Room loaded:', roomData);
        console.log('[GameClient] MaxPlayers:', roomData.maxPlayers);

        // Parse game state and config
        try {
          const parsedJson = JSON.parse(roomData.gameState);
          if (parsedJson) {
            // The server sends the full state object with camelCase
            console.log('[GameClient] Parsed game state:', parsedJson);
            console.log('[GameClient] Current stage:', parsedJson?.currentStage);
            console.log('[GameClient] Stage $type:', parsedJson?.currentStage?.$type);
            setGameState(parsedJson);
          }
        } catch (e) {
          console.error('Failed to parse game state:', e);
        }

        try {
          const parsedConfig = JSON.parse(roomData.gameConfig);
          setGameConfig(parsedConfig);
        } catch (e) {
          console.error('Failed to parse game config:', e);
        }

        setLoading(false);
      } catch (err) {
        console.error('Error fetching initial data:', err);
        setError(err.message);
        setLoading(false);
      }
    };

    fetchInitialData();
  }, [roomId, API_URL, router]);

  // Setup SSE connection
  useEffect(() => {
    if (!roomId || !user) return; // Wait for user data to be available

    const eventSource = new EventSource(`${API_URL}/api/room/${roomId}/events`, {
      withCredentials: true,
    });

    eventSource.onopen = () => {
      console.log('[SSE] Connection opened');
    };

    // Define the state setters and functions to pass to the event handler
    const setters: GameStateSetters = {
      setMessages,
      setRoomPlayers,
      setGameState,
      setRoom,
      fetchRoomPlayers,
      user,
    };

    // Create a single listener that will process all incoming game events
    const listener = createSSEListener(setters);

    // List of all event types we want to handle
    const eventTypes = [
      'chat',
      'game_state_update',
      'player_action',
      'player_join',
      'player_leave',
      'host_change',
      'player_reveal',
      'dealer_reveal',
    ];

    // Attach the listener to each event type
    eventTypes.forEach((type) => {
      eventSource.addEventListener(type, listener);
    });

    eventSource.onerror = (error) => {
      console.error('[SSE] Error:', error);
      eventSource.close();
    };

    eventSourceRef.current = eventSource;

    return () => {
      if (eventSourceRef.current) {
        // Remove all specific listeners before closing
        eventTypes.forEach((type) => {
          eventSourceRef.current.removeEventListener(type, listener);
        });
        eventSourceRef.current.close();
      }
    };
    // Add `user` to dependency array to ensure setters object has the latest user state
  }, [roomId, API_URL, user]);

  const handleStartGame = async () => {
    try {
      console.log('[StartGame] Starting game for room:', roomId);
      const response = await fetch(`${API_URL}/api/room/${roomId}/start`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify(null),
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || error.title || 'Failed to start game');
      }

      const updatedRoom = await response.json();
      console.log('[StartGame] Updated room received:', updatedRoom);
      setRoom(updatedRoom);

      if (updatedRoom.gameState) {
        const parsedJson = JSON.parse(updatedRoom.gameState);
        if (parsedJson) {
          // The server sends the full state object with camelCase
          console.log('[StartGame] Parsed game state:', parsedJson);
          console.log('[StartGame] Current stage:', parsedJson?.currentStage);
          console.log('[StartGame] Stage $type:', parsedJson?.currentStage?.$type);
          setGameState(parsedJson);
        }
      } else {
        console.warn('[StartGame] No game state in response');
      }

      console.log('[StartGame] Game started successfully');
    } catch (error) {
      console.error('[StartGame] Error starting game:', error);
      alert(`Failed to start game: ${error.message}`);
    }
  };

  const handlePlayerAction = async (action: string, data: Record<string, any> = {}) => {
    if (!user) return;

    try {
      console.log(`[PlayerAction] Performing action: ${action}`, data);
      const response = await fetch(`${API_URL}/api/room/${roomId}/player/${user.id}/action`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({ action, data }),
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || error.title || 'Failed to perform action');
      }

      console.log(`[PlayerAction] Action ${action} performed successfully`);

      // Immediately refresh player data to show updated balances
      // This supplements the SSE room_updated event for faster UI update
      await fetchRoomPlayers();
      console.log('[PlayerAction] Player data refreshed');
    } catch (error) {
      console.error('Error performing action:', error);
      alert(`Failed to perform action: ${error.message}`);
    }
  };


  const handleLeaveRoom = async () => {
    if (!user) return;

    try {
      const response = await fetch(`${API_URL}/api/room/${roomId}/leave`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({ userId: user.id }),
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || error.title || 'Failed to leave room');
      }

      router.push('/rooms');
    } catch (error) {
      console.error('Error leaving room:', error);
      alert(`Failed to leave room: ${error.message}`);
    }
  };

  const handleSendMessage = async (e: FormEvent) => {
    e.preventDefault();
    if (!chatMessage.trim()) return;

    try {
      await fetch(`${API_URL}/api/room/${roomId}/chat`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({ content: chatMessage }),
      });

      setChatMessage('');
    } catch (error) {
      console.error('Error sending message:', error);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-green-900 via-green-800 to-emerald-900 flex items-center justify-center">
        <div className="text-yellow-100 text-2xl">Loading game...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-green-900 via-green-800 to-emerald-900 flex items-center justify-center">
        <div className="bg-black/80 border-2 border-red-600 rounded-xl p-8 max-w-md">
          <h2 className="text-2xl font-bold text-red-400 mb-4">Error</h2>
          <p className="text-yellow-100 mb-4">{error}</p>
          <button
            onClick={() => router.push('/rooms')}
            className="px-4 py-2 bg-yellow-600 text-black font-bold rounded-lg hover:bg-yellow-700"
          >
            Back to Rooms
          </button>
        </div>
      </div>
    );
  }

  const isHost = user && room && user.id === room.hostId;
  const currentStage = gameState?.currentStage?.$type;

  // Game has not started if there's no stage or it's in 'not_started' stage
  const gameNotStarted = !currentStage || currentStage === 'not_started';

  // Get the appropriate game handler
  const gameHandler = room ? getGameHandler(room.gameMode) : null;

  // Debug logging
  console.log('[GameClient] Render - gameState:', gameState);
  console.log('[GameClient] Render - currentStage:', currentStage);
  console.log('[GameClient] Render - gameNotStarted:', gameNotStarted);
  console.log('[GameClient] Render - isHost:', isHost);
  console.log('[GameClient] Render - gameMode:', room?.gameMode);
  console.log('[GameClient] Render - gameHandler:', gameHandler);

  return (
    <div className="min-h-screen bg-gradient-to-br from-green-900 via-green-800 to-emerald-900 p-4 md:p-8">
      {/* Header */}
      <div className="bg-black/80 border-2 border-yellow-600 rounded-xl p-4 mb-4">
        <div className="flex items-center justify-between flex-wrap gap-4">
          <div>
            <h1 className="text-2xl md:text-3xl font-bold text-yellow-400">
              {room?.description || (gameHandler ? gameHandler.getGameTitle() : 'Game Table')}
            </h1>
            <p className="text-yellow-100/60 text-sm font-mono">Room ID: {roomId.substring(0, 8)}...</p>
          </div>
          <div className="flex items-center gap-4">
            <div
              className={`px-3 py-1 rounded text-sm font-semibold ${
                room?.isActive
                  ? 'bg-green-600/20 text-green-300 border border-green-600'
                  : 'bg-gray-600/20 text-gray-300 border border-gray-600'
              }`}
            >
              {room?.isActive ? 'Active' : 'Waiting'}
            </div>
            <button
              onClick={handleLeaveRoom}
              className="px-4 py-2 bg-red-600/80 text-white font-bold rounded-lg hover:bg-red-700 border-2 border-red-700"
            >
              Leave
            </button>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        {/* Main Game Area */}
      <div className="lg:col-span-2 space-y-4">
        {/* Game State - Use dynamic game handler if available */}
        {gameHandler && gameState && !gameNotStarted ? (
          gameHandler.renderGameArea(gameState, gameConfig, roomPlayers, user)
        ) : (
          <div className="bg-black/80 border-2 border-yellow-600 rounded-xl p-6">
            <h2 className="text-xl font-bold text-yellow-400 mb-4">Game Status</h2>
            <div className="grid grid-cols-2 gap-4 mb-4">
              <div>
                <p className="text-yellow-100/60 text-sm">Current Stage</p>
                <p className="text-yellow-200 font-bold capitalize">
                  {gameNotStarted ? 'Not Started' : currentStage?.replace(/_/g, ' ') || 'Unknown'}
                </p>
              </div>
              <div>
                <p className="text-yellow-100/60 text-sm">Your Balance</p>
                <p className="text-yellow-200 font-bold">${user?.balance || 0}</p>
              </div>
            </div>

            {gameConfig && (
              <div className="bg-green-900/30 border border-green-700 rounded-lg p-4 mb-4">
                <h3 className="text-yellow-300 font-bold mb-2">Game Config</h3>
                <div className="grid grid-cols-2 gap-2 text-sm">
                  <div>
                    <span className="text-yellow-100/60">Starting Balance:</span>
                    <span className="text-yellow-200 ml-2 font-bold">${gameConfig.startingBalance}</span>
                  </div>
                  <div>
                    <span className="text-yellow-100/60">Min Bet:</span>
                    <span className="text-yellow-200 ml-2 font-bold">${gameConfig.minBet}</span>
                  </div>
                </div>
              </div>
            )}

            {/* Host Controls */}
            {isHost && gameNotStarted && (
              <button
                onClick={handleStartGame}
                className="w-full py-3 bg-gradient-to-r from-green-400 via-green-500 to-green-600 text-black font-bold rounded-lg hover:from-green-500 hover:to-green-700 transition-all duration-200 border-2 border-green-700 shadow-md"
              >
                Start Game
              </button>
            )}

            {/* Waiting for Host to Start */}
            {!isHost && gameNotStarted && (
              <div className="bg-blue-900/20 border border-blue-700 rounded-lg p-4 text-center">
                <p className="text-blue-300">Waiting for host to start the game...</p>
              </div>
            )}
          </div>
        )}

          {/* Player Actions - Use dynamic game handler if available */}
          {room?.isActive && !gameNotStarted && (
            <div className="bg-black/80 border-2 border-yellow-600 rounded-xl p-6">
              <h2 className="text-xl font-bold text-yellow-400 mb-4">Player Actions</h2>
              {gameHandler && gameState ? (
                gameHandler.renderPlayerActions(gameState, gameConfig, handlePlayerAction, user, roomPlayers)
              ) : (
                <p className="text-yellow-100/60 text-center">
                  {gameHandler ? 'No actions available' : `Game mode "${room?.gameMode}" not supported`}
                </p>
              )}
            </div>
          )}
        </div>

        {/* Sidebar */}
        <div className="space-y-4">
          {/* Players List */}
          <div className="bg-black/80 border-2 border-yellow-600 rounded-xl p-4">
            <h2 className="text-xl font-bold text-yellow-400 mb-4">
              Players ({roomPlayers.length}/{room?.maxPlayers || '?'})
            </h2>
            <div className="space-y-2">
              {roomPlayers.length === 0 ? (
                <p className="text-yellow-100/40 text-sm text-center">No players yet</p>
              ) : (
                roomPlayers.map((player) => (
                  <div key={player.id} className="bg-black/60 rounded-lg p-3 border border-yellow-700/50">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-yellow-200 font-bold text-sm">
                          {player.userName}
                          {player.userId === user?.id && <span className="ml-2 text-xs text-yellow-400">(You)</span>}
                          {player.userId === room?.hostId && (
                            <span className="ml-2 text-xs text-green-400">(Host)</span>
                          )}
                        </p>
                        <p className="text-yellow-100/60 text-xs">{player.userEmail}</p>
                        <p className="text-yellow-100/40 text-xs font-mono">ID: {player.userId.substring(0, 8)}...</p>
                      </div>
                      <div className="text-right">
                        <p className="text-yellow-200 font-bold text-sm">${player.balance}</p>
                        <p
                          className={`text-xs font-semibold ${
                            player.status === 'Active' ? 'text-green-400' : 'text-gray-400'
                          }`}
                        >
                          {player.status}
                        </p>
                      </div>
                    </div>
                  </div>
                ))
              )}
            </div>
          </div>

          {/* Chat */}
          <div className="bg-black/80 border-2 border-yellow-600 rounded-xl p-4">
            <h2 className="text-xl font-bold text-yellow-400 mb-4">Chat</h2>
            <div className="bg-black/60 rounded-lg p-3 h-64 overflow-y-auto mb-4">
              {messages.length === 0 ? (
                <p className="text-yellow-100/40 text-sm text-center">No messages yet</p>
              ) : (
                messages.map((msg, idx) => (
                  <div key={idx} className="text-yellow-100 text-sm mb-2">
                    <span className="font-bold text-yellow-300">{msg.sender}: </span>
                    <span>{msg.content}</span>
                  </div>
                ))
              )}
            </div>
            <form onSubmit={handleSendMessage} className="flex gap-2">
              <input
                type="text"
                value={chatMessage}
                onChange={(e) => setChatMessage(e.target.value)}
                placeholder="Type a message..."
                className="flex-1 px-3 py-2 rounded bg-black/60 border border-yellow-700 text-yellow-100 text-sm focus:outline-none focus:ring-2 focus:ring-yellow-500"
              />
              <button
                type="submit"
                className="px-4 py-2 bg-yellow-600 text-black font-bold rounded-lg hover:bg-yellow-700"
              >
                Send
              </button>
            </form>
          </div>

          {/* Game State Debug */}
          {gameState && (
            <div className="bg-black/80 border-2 border-yellow-600 rounded-xl p-4">
              <h2 className="text-xl font-bold text-yellow-400 mb-4">Debug Info</h2>
              <pre className="text-yellow-100 text-xs overflow-auto max-h-64 bg-black/60 p-3 rounded">
                {JSON.stringify(gameState, null, 2)}
              </pre>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
