'use client';

import React, { useState } from 'react';
import { GameHandler, GameState } from '@/lib/types/game';
import { BlackjackStage, BlackjackState } from '@/lib/types/blackjack';

interface BlackjackPlayerActionsProps {
  gameState: GameState;
  gameConfig: any;
  onAction: (action: string, data: any) => void;
  user: any;
  roomPlayers: any[];
}

// encapsulates the UI and state for player actions
const BlackjackPlayerActions: React.FC<BlackjackPlayerActionsProps> = ({
  gameState,
  gameConfig,
  onAction,
  user,
  roomPlayers,
}) => {
  const [betAmount, setBetAmount] = useState(gameConfig?.minBet || 10);
  const currentStage = gameState.currentStage as BlackjackStage;
  const stageType = currentStage?.$type;
  const playerBalance = roomPlayers.find((p) => p.userId === user?.id)?.balance || 0;

  if (stageType === 'betting') {
    return (
      <div className="space-y-4">
        {/* Player Balance */}
        {playerBalance > 0 && (
          <div className="bg-yellow-900/20 border border-yellow-700 rounded-lg p-3">
            <div className="flex justify-between items-center">
              <span className="text-yellow-100/80">Your Balance:</span>
              <span className="text-yellow-400 font-bold text-xl">${playerBalance}</span>
            </div>
          </div>
        )}

        {/* Betting Deadline Timer */}
        {currentStage.deadline && (
          <div className="bg-red-900/20 border border-red-700 rounded-lg p-2 text-center">
            <span className="text-red-300 text-sm">
              Betting closes: {new Date(currentStage.deadline).toLocaleTimeString()}
            </span>
          </div>
        )}

        {/* Bet Amount Input */}
        <div>
          <label className="block text-yellow-100 mb-2 font-semibold">
            Bet Amount (Min: ${gameConfig?.minBet || 10})
          </label>
          <input
            type="number"
            value={betAmount}
            onChange={(e) => setBetAmount(parseInt(e.target.value))}
            min={gameConfig?.minBet || 10}
            step="10"
            max={playerBalance}
            className="w-full px-4 py-2 rounded bg-black/60 border border-yellow-700 text-yellow-100 focus:outline-none focus:ring-2 focus:ring-yellow-500"
          />
        </div>

        {/* Quick Bet Buttons */}
        <div className="grid grid-cols-4 gap-2">
          <button
            onClick={() => setBetAmount(gameConfig?.minBet || 10)}
            className="py-2 px-3 bg-yellow-900/40 hover:bg-yellow-900/60 border border-yellow-700 text-yellow-300 rounded text-sm font-semibold transition"
          >
            Min
          </button>
          <button
            onClick={() => setBetAmount((gameConfig?.minBet || 10) * 2)}
            className="py-2 px-3 bg-yellow-900/40 hover:bg-yellow-900/60 border border-yellow-700 text-yellow-300 rounded text-sm font-semibold transition"
          >
            2x
          </button>
          <button
            onClick={() => setBetAmount((gameConfig?.minBet || 10) * 5)}
            className="py-2 px-3 bg-yellow-900/40 hover:bg-yellow-900/60 border border-yellow-700 text-yellow-300 rounded text-sm font-semibold transition"
          >
            5x
          </button>
          <button
            onClick={() => setBetAmount(playerBalance)}
            className="py-2 px-3 bg-yellow-900/40 hover:bg-yellow-900/60 border border-yellow-700 text-yellow-300 rounded text-sm font-semibold transition"
          >
            All In
          </button>
        </div>

        {/* Place Bet Button */}
        <button
          onClick={() => onAction('bet', { amount: betAmount })}
          className="w-full py-3 bg-gradient-to-r from-yellow-400 via-yellow-500 to-yellow-600 text-black font-bold rounded-lg hover:from-yellow-500 hover:to-yellow-700 transition-all duration-200 border-2 border-yellow-700 shadow-md"
        >
          Place Bet ${betAmount}
        </button>
      </div>
    );
  }

  if (stageType === 'player_action') {
    return (
      <div className="flex gap-4">
        <button
          onClick={() => onAction('hit', {})}
          className="flex-1 py-3 bg-gradient-to-r from-blue-400 via-blue-500 to-blue-600 text-white font-bold rounded-lg hover:from-blue-500 hover:to-blue-700 transition-all duration-200 border-2 border-blue-700 shadow-md"
        >
          Hit
        </button>
        <button
          onClick={() => onAction('stand', {})}
          className="flex-1 py-3 bg-gradient-to-r from-red-400 via-red-500 to-red-600 text-white font-bold rounded-lg hover:from-red-500 hover:to-red-700 transition-all duration-200 border-2 border-red-700 shadow-md"
        >
          Stand
        </button>
      </div>
    );
  }

  return <p className="text-yellow-100/60 text-center">Waiting for game to progress...</p>;
};

export const BlackjackGameHandler: GameHandler = {
  getGameTitle: () => 'Blackjack Table',

  parseGameState: (rawState: string): GameState => {
    try {
      const parsed = JSON.parse(rawState);
      return parsed as BlackjackState;
    } catch (e) {
      console.error('Failed to parse blackjack game state:', e);
      return { currentStage: { $type: 'init' } };
    }
  },

  parseGameConfig: (rawConfig: string) => {
    try {
      return JSON.parse(rawConfig);
    } catch (e) {
      console.error('Failed to parse blackjack game config:', e);
      return null;
    }
  },

  getSupportedActions: (gameState: GameState): string[] => {
    const stageType = gameState.currentStage?.$type;
    if (!stageType) return [];

    switch (stageType) {
      case 'betting':
        return ['bet'];
      case 'player_action':
        return ['hit', 'stand'];
      default:
        return [];
    }
  },

  renderGameArea: (gameState: GameState, gameConfig: any, roomPlayers: any[], user: any) => {
    const currentStage = gameState.currentStage as BlackjackStage;
    const stageType = currentStage?.$type || 'init';

    return (
      <div className="bg-black/80 border-2 border-yellow-600 rounded-xl p-6">
        <h2 className="text-xl font-bold text-yellow-400 mb-4">Game Status</h2>
        <div className="grid grid-cols-2 gap-4 mb-4">
          <div>
            <p className="text-yellow-100/60 text-sm">Current Stage</p>
            <p className="text-yellow-200 font-bold capitalize">
              {stageType === 'init' ? 'Not Started' : stageType.replace(/_/g, ' ')}
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

        {/* Betting stage specific UI */}
        {stageType === 'betting' && currentStage?.bets && Object.keys(currentStage.bets).length > 0 && (
          <div className="bg-green-900/20 border border-green-700 rounded-lg p-3">
            <p className="text-green-300 text-sm mb-2 font-semibold">Bets Placed:</p>
            <div className="space-y-1">
              {Object.entries(currentStage.bets).map(([playerId, amount]) => {
                const player = roomPlayers.find((p) => p.userId === playerId);
                return (
                  <div key={playerId} className="flex justify-between text-sm">
                    <span className="text-green-200">{player?.userName || 'Player'}</span>
                    <span className="text-green-400 font-bold">${Number(amount).toLocaleString()}</span>
                  </div>
                );
              })}
            </div>
          </div>
        )}
      </div>
    );
  },

  renderPlayerActions: (
    gameState: GameState,
    gameConfig: any,
    onAction: (action: string, data: any) => void,
    user: any,
    roomPlayers: any[],
  ) => {
    return (
      <BlackjackPlayerActions
        gameState={gameState}
        gameConfig={gameConfig}
        onAction={onAction}
        user={user}
        roomPlayers={roomPlayers}
      />
    );
  },
};
