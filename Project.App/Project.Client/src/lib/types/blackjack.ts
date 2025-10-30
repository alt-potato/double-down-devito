import { GameState, GameStage } from './game';

export interface BlackjackState extends GameState {
  currentStage: BlackjackStage;
  // add properties as needed
}

export interface BlackjackStage extends GameStage {
  $type: 'not_started' | 'init' | 'setup' | 'betting' | 'dealing' | 'player_action' | 'finish_round' | 'teardown';
  deadline?: string;
  bets?: Record<string, number>;
  playerIndex?: number;
  handIndex?: number;
  // add stage properties as needed
}
