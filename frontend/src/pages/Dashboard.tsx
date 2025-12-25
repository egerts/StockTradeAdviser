import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { 
  ArrowTrendingUpIcon, 
  ArrowTrendingDownIcon, 
  BriefcaseIcon,
  LightBulbIcon,
  CurrencyDollarIcon,
  ChartBarIcon
} from '@heroicons/react/24/outline';
import { Portfolio, Recommendation, StockData } from '../types/models';

export const Dashboard: React.FC = () => {
  const { user } = useAuth();
  const [portfolios, setPortfolios] = useState<Portfolio[]>([]);
  const [recommendations, setRecommendations] = useState<Recommendation[]>([]);
  const [marketData, setMarketData] = useState<StockData[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchDashboardData = async () => {
      try {
        const [portfoliosRes, recommendationsRes, marketRes] = await Promise.all([
          fetch('/api/portfolios'),
          fetch('/api/recommendations/active'),
          fetch('/api/stocks/market-overview')
        ]);

        const portfoliosData = await portfoliosRes.json();
        const recommendationsData = await recommendationsRes.json();
        const marketDataData = await marketRes.json();

        setPortfolios(portfoliosData);
        setRecommendations(recommendationsData);
        setMarketData(marketDataData);
      } catch (error) {
        console.error('Error fetching dashboard data:', error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchDashboardData();
  }, []);

  const totalPortfolioValue = portfolios.reduce((sum: number, portfolio: Portfolio) => sum + portfolio.totalValue, 0);
  const totalGainLoss = portfolios.reduce((sum: number, portfolio: Portfolio) => sum + portfolio.totalGainLoss, 0);
  const totalGainLossPercentage = totalPortfolioValue > 0 ? (totalGainLoss / totalPortfolioValue) * 100 : 0;

  const activeRecommendations = recommendations.filter(r => r.status === 'Active');
  const buyRecommendations = activeRecommendations.filter((r: Recommendation) => r.action === 'Buy' || r.action === 'StrongBuy');

  if (isLoading) {
    return (
      <div className="p-6">
        <div className="animate-pulse">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
            {[1, 2, 3, 4].map(i => (
              <div key={i} className="bg-white p-6 rounded-lg shadow">
                <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
                <div className="h-8 bg-gray-200 rounded w-1/2"></div>
              </div>
            ))}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-900 mb-2">
          Welcome back, {user?.displayName}!
        </h1>
        <p className="text-gray-600">
          Here's your portfolio overview and latest recommendations.
        </p>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        <div className="card p-6">
          <div className="flex items-center">
            <div className="flex-shrink-0">
              <CurrencyDollarIcon className="h-8 w-8 text-green-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">Total Value</p>
              <p className="text-2xl font-bold text-gray-900">
                ${totalPortfolioValue.toLocaleString()}
              </p>
            </div>
          </div>
        </div>

        <div className="card p-6">
          <div className="flex items-center">
            <div className="flex-shrink-0">
              {totalGainLoss >= 0 ? (
                <ArrowTrendingUpIcon className="h-8 w-8 text-green-600" />
              ) : (
                <ArrowTrendingDownIcon className="h-8 w-8 text-red-600" />
              )}
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">Total Gain/Loss</p>
              <p className={`text-2xl font-bold ${totalGainLoss >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                {totalGainLoss >= 0 ? '+' : ''}${Math.abs(totalGainLoss).toLocaleString()}
              </p>
              <p className={`text-sm ${totalGainLoss >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                {totalGainLossPercentage >= 0 ? '+' : ''}{totalGainLossPercentage.toFixed(2)}%
              </p>
            </div>
          </div>
        </div>

        <div className="card p-6">
          <div className="flex items-center">
            <div className="flex-shrink-0">
              <BriefcaseIcon className="h-8 w-8 text-blue-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">Portfolios</p>
              <p className="text-2xl font-bold text-gray-900">{portfolios.length}</p>
            </div>
          </div>
        </div>

        <div className="card p-6">
          <div className="flex items-center">
            <div className="flex-shrink-0">
              <LightBulbIcon className="h-8 w-8 text-yellow-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">Active Recommendations</p>
              <p className="text-2xl font-bold text-gray-900">{activeRecommendations.length}</p>
              <p className="text-sm text-green-600">{buyRecommendations.length} Buy signals</p>
            </div>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Portfolio Overview */}
        <div className="card p-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-semibold text-gray-900">Portfolio Overview</h2>
            <ChartBarIcon className="h-5 w-5 text-gray-400" />
          </div>
          {portfolios.length > 0 ? (
            <div className="space-y-4">
              {portfolios.map((portfolio: Portfolio) => (
                <div key={portfolio.id} className="border-b border-gray-200 pb-3 last:border-0">
                  <div className="flex justify-between items-center">
                    <div>
                      <p className="font-medium text-gray-900">{portfolio.name}</p>
                      <p className="text-sm text-gray-500">{portfolio.holdings.length} holdings</p>
                    </div>
                    <div className="text-right">
                      <p className="font-medium text-gray-900">
                        ${portfolio.totalValue.toLocaleString()}
                      </p>
                      <p className={`text-sm ${portfolio.totalGainLoss >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                        {portfolio.totalGainLoss >= 0 ? '+' : ''}{portfolio.totalGainLossPercentage.toFixed(2)}%
                      </p>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-8">
              <BriefcaseIcon className="h-12 w-12 text-gray-400 mx-auto mb-3" />
              <p className="text-gray-500">No portfolios yet</p>
              <p className="text-sm text-gray-400">Create your first portfolio to get started</p>
            </div>
          )}
        </div>

        {/* Recent Recommendations */}
        <div className="card p-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-semibold text-gray-900">Recent Recommendations</h2>
            <LightBulbIcon className="h-5 w-5 text-gray-400" />
          </div>
          {activeRecommendations.length > 0 ? (
            <div className="space-y-4">
              {activeRecommendations.map((recommendation: Recommendation) => (
                <div key={recommendation.id} className="border-b border-gray-200 pb-3 last:border-0">
                  <div className="flex justify-between items-center">
                    <div>
                      <div className="flex items-center space-x-2">
                        <p className="font-medium text-gray-900">{recommendation.symbol}</p>
                        <span className={`px-2 py-1 text-xs font-medium rounded-full ${
                          recommendation.action === 'StrongBuy' || recommendation.action === 'Buy'
                            ? 'bg-green-100 text-green-800'
                            : recommendation.action === 'Sell' || recommendation.action === 'StrongSell'
                            ? 'bg-red-100 text-red-800'
                            : 'bg-yellow-100 text-yellow-800'
                        }`}>
                          {recommendation.action}
                        </span>
                      </div>
                      <p className="text-sm text-gray-500">
                        Target: ${recommendation.targetPrice} | Stop: ${recommendation.stopLoss}
                      </p>
                    </div>
                    <div className="text-right">
                      <p className="font-medium text-gray-900">
                        {Math.round(recommendation.confidence)}%
                      </p>
                      <p className="text-sm text-gray-500">confidence</p>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-8">
              <LightBulbIcon className="h-12 w-12 text-gray-400 mx-auto mb-3" />
              <p className="text-gray-500">No active recommendations</p>
              <p className="text-sm text-gray-400">Check back later for new recommendations</p>
            </div>
          )}
        </div>
      </div>

      {/* Market Overview */}
      {marketData.length > 0 && (
        <div className="card p-6 mt-8">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Market Overview</h2>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            {marketData.slice(0, 5).map((stock: StockData) => (
              <div key={stock.symbol} className="text-center">
                <p className="font-medium text-gray-900">{stock.symbol}</p>
                <p className="text-sm text-gray-900">${stock.price.toFixed(2)}</p>
                <p className={`text-sm font-medium ${
                  stock.priceChangePercentage >= 0 ? 'text-green-600' : 'text-red-600'
                }`}>
                  {stock.priceChangePercentage >= 0 ? '+' : ''}{stock.priceChangePercentage.toFixed(2)}%
                </p>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};
