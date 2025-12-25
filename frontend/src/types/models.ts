export interface User {
  id: string;
  email: string;
  displayName: string;
  azureAdObjectId: string;
  createdAt: string;
  updatedAt: string;
  tradingStrategy: TradingStrategy;
  portfolios: Portfolio[];
}

export interface TradingStrategy {
  riskTolerance: 'Low' | 'Medium' | 'High';
  investmentHorizon: 'ShortTerm' | 'MediumTerm' | 'LongTerm';
  maxPortfolioSize: number;
  preferredSectors: string[];
  sellStrategy: SellStrategy;
}

export interface SellStrategy {
  takeProfitPercentage: number;
  stopLossPercentage: number;
  trailingStopEnabled: boolean;
  trailingStopPercentage: number;
}

export interface Portfolio {
  id: string;
  userId: string;
  name: string;
  description: string;
  createdAt: string;
  updatedAt: string;
  holdings: Holding[];
  totalValue: number;
  totalCost: number;
  totalGainLoss: number;
  totalGainLossPercentage: number;
}

export interface Holding {
  id: string;
  symbol: string;
  assetType: AssetType;
  quantity: number;
  averageCostPrice: number;
  currentPrice: number;
  lastUpdated: string;
  totalCost: number;
  currentValue: number;
  gainLoss: number;
  gainLossPercentage: number;
  transactions: Transaction[];
}

export interface Transaction {
  id: string;
  holdingId: string;
  type: TransactionType;
  quantity: number;
  price: number;
  timestamp: string;
  notes: string;
  fees: number;
}

export interface StockData {
  id: string;
  symbol: string;
  companyName: string;
  sector: string;
  industry: string;
  marketCap: number;
  price: number;
  priceChange: number;
  priceChangePercentage: number;
  volume: number;
  averageVolume: number;
  dayHigh: number;
  dayLow: number;
  week52High: number;
  week52Low: number;
  peRatio: number;
  dividendYield: number;
  beta: number;
  eps: number;
  timestamp: string;
  technicalIndicators: TechnicalIndicators;
  fundamentals: Fundamentals;
}

export interface TechnicalIndicators {
  rsi: number;
  macd: number;
  macdSignal: number;
  macdHistogram: number;
  sma20: number;
  sma50: number;
  sma200: number;
  ema12: number;
  ema26: number;
  bollingerUpper: number;
  bollingerMiddle: number;
  bollingerLower: number;
  volumeSma: number;
}

export interface Fundamentals {
  revenue: number;
  revenueGrowth: number;
  netIncome: number;
  grossMargin: number;
  operatingMargin: number;
  netMargin: number;
  debtToEquity: number;
  returnOnEquity: number;
  returnOnAssets: number;
  currentRatio: number;
  quickRatio: number;
  bookValuePerShare: number;
  priceToBook: number;
  priceToSales: number;
}

export interface Recommendation {
  id: string;
  userId: string;
  symbol: string;
  action: RecommendationAction;
  confidence: number;
  targetPrice: number;
  stopLoss: number;
  reasoning: string;
  keyFactors: string[];
  riskLevel: RiskLevel;
  timeHorizon: TimeHorizon;
  createdAt: string;
  validUntil: string;
  status: RecommendationStatus;
  technicalScore: number;
  fundamentalScore: number;
  sentimentScore: number;
  overallScore: number;
  actualAction?: RecommendationAction;
  actualPrice?: number;
  executedAt?: string;
}

export interface RecommendationHistory {
  id: string;
  recommendationId: string;
  userId: string;
  symbol: string;
  originalAction: RecommendationAction;
  originalPrice: number;
  actualAction?: RecommendationAction;
  actualPrice?: number;
  outcome?: RecommendationOutcome;
  profitLoss?: number;
  profitLossPercentage?: number;
  createdAt: string;
  closedAt?: string;
}

export enum AssetType {
  Stock = 'Stock',
  Option = 'Option',
  ETF = 'ETF',
  Bond = 'Bond',
  Crypto = 'Crypto',
  Commodity = 'Commodity'
}

export enum TransactionType {
  Buy = 'Buy',
  Sell = 'Sell',
  Dividend = 'Dividend',
  Split = 'Split'
}

export enum InvestmentHorizon {
  ShortTerm = 'ShortTerm',
  MediumTerm = 'MediumTerm',
  LongTerm = 'LongTerm'
}

export enum RecommendationAction {
  StrongBuy = 'StrongBuy',
  Buy = 'Buy',
  Hold = 'Hold',
  Sell = 'Sell',
  StrongSell = 'StrongSell'
}

export enum RiskLevel {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High',
  VeryHigh = 'VeryHigh'
}

export enum TimeHorizon {
  ShortTerm = 'ShortTerm',
  MediumTerm = 'MediumTerm',
  LongTerm = 'LongTerm'
}

export enum RecommendationStatus {
  Active = 'Active',
  Executed = 'Executed',
  Expired = 'Expired',
  Cancelled = 'Cancelled'
}

export enum RecommendationOutcome {
  Profitable = 'Profitable',
  Loss = 'Loss',
  Breakeven = 'Breakeven'
}

export enum RiskTolerance {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High'
}
