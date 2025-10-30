// base types for game states

export interface GameStage {
  $type: string;
}

export interface GameState {
  currentStage: GameStage;
}

// abstract game handler interface
export interface GameHandler {
  renderGameArea: (gameState: GameState, gameConfig: any, roomPlayers: any[], user: any) => React.ReactElement;
  renderPlayerActions: (gameState: GameState, gameConfig: any, onAction: (action: string, data: any) => void, user: any, roomPlayers: any[]) => React.ReactElement;
  parseGameState: (rawState: string) => GameState;
  parseGameConfig: (rawConfig: string) => any;
  getSupportedActions: (gameState: GameState) => string[];
  getGameTitle: () => string;
}
