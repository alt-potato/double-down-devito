// for reference:
// Project.App/Project.Api/Models/User.cs
// Project.App/Project.Api/DTOs/RoomDTOs.cs
// Project.App/Project.Api/Models/RoomPlayer.cs
// Project.App/Project.Api/Models/Games/BlackjackConfig.cs
// Project.App/Project.Api/Models/Games/BlackjackState.cs

export interface User {
  id: string;
  name: string;
  email: string;
  balance: number;
  avatarUrl?: string;
}

export interface Room {
  id: string;
  hostId: string;
  isPublic: boolean;
  gameMode: string;
  gameState: string; // json string
  gameConfig: string; // json string
  description?: string;
  maxPlayers: number;
  minPlayers: number;
  deckId: string;
  createdAt: string;
  isActive: boolean;
}

export interface RoomPlayer {
  id: string;
  roomId: string;
  userId: string;
  role: 'Admin' | 'Moderator' | 'Player';
  status: 'Active' | 'Inactive' | 'Away' | 'Left';
  balance: number;
  balanceDelta: number;
  userName?: string; // for convenience
  userEmail?: string; // for convenience (TODO: check if necessary?)
}

export interface GameConfig {
  startingBalance: number;
  minBet: number;
  bettingTimeLimit: string; // Timespan as string
  turnTimeLimit: string; // Timespan as string
  allowBalanceReset: boolean;
}

export interface BlackjackInitStage {
  $type: 'init';
}

export interface BlackjackSetupStage {
  $type: 'setup';
}

export interface BlackjackBettingStage {
  $type: 'betting';
  deadline: string;
  bets: Record<string, number>;
}

export interface BlackjackDealingStage {
  $type: 'dealing';
}

export interface BlackjackPlayerActionStage {
  $type: 'player_action';
  deadline: string;
  playerIndex: number;
  handIndex: number;
}

export interface BlackjackFinishRoundStage {
  $type: 'finish_round';
}

export interface BlackjackTeardownStage {
  $type: 'teardown';
}

export type BlackjackStage =
  | BlackjackInitStage
  | BlackjackSetupStage
  | BlackjackBettingStage
  | BlackjackDealingStage
  | BlackjackPlayerActionStage
  | BlackjackFinishRoundStage
  | BlackjackTeardownStage;

export interface GameState {
  dealerHand: string;
  bets: Record<string, number>;
  currentStage: BlackjackStage;
}
