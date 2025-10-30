// for reference:
// Project.App/Project.Api/Models/Games/BlackjackState.cs

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
