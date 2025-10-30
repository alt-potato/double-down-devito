import { GameHandler } from '@/lib/types/game';
import { BlackjackGameHandler } from './handlers/BlackjackGameHandler';

// Game mode registry - maps game mode strings to their respective handlers
export const gameHandlers: Record<string, GameHandler> = {
  'blackjack': BlackjackGameHandler,
  // Add future game handlers here:
  // 'poker': PokerGameHandler,
  // 'roulette': RouletteGameHandler,
};

/**
 * Get the appropriate game handler for a given game mode
 * @param gameMode The game mode string from the room
 * @returns The game handler or null if not found
 */
export function getGameHandler(gameMode: string): GameHandler | null {
  return gameHandlers[gameMode] || null;
}

/**
 * Check if a game mode is supported
 * @param gameMode The game mode string to check
 * @returns True if the game mode is supported
 */
export function isGameModeSupported(gameMode: string): boolean {
  return gameMode in gameHandlers;
}

/**
 * Get a list of all supported game modes
 * @returns Array of supported game mode strings
 */
export function getSupportedGameModes(): string[] {
  return Object.keys(gameHandlers);
}
