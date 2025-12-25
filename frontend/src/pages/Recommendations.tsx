import React, { useState, useEffect } from 'react';
import { 
  RecommendationAction, 
  RecommendationStatus, 
  RiskLevel, 
  TimeHorizon,
  Recommendation 
} from '../types/models';
import { 
  LightBulbIcon,
  ArrowTrendingUpIcon,
  ArrowTrendingDownIcon,
  ExclamationTriangleIcon,
  ClockIcon,
  CheckCircleIcon,
  XCircleIcon
} from '@heroicons/react/24/outline';

export const Recommendations: React.FC = () => {
  const [recommendations, setRecommendations] = useState<Recommendation[]>([]);
  const [activeRecommendations, setActiveRecommendations] = useState<Recommendation[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [selectedTab, setSelectedTab] = useState<'active' | 'all' | 'history'>('active');

  useEffect(() => {
    fetchRecommendations();
  }, []);

  const fetchRecommendations = async () => {
    try {
      const [allRes, activeRes] = await Promise.all([
        fetch('http://localhost:53133/api/recommendations'),
        fetch('http://localhost:53133/api/recommendations/active')
      ]);
      
      const allData = await allRes.json();
      const activeData = await activeRes.json();
      
      setRecommendations(allData);
      setActiveRecommendations(activeData);
    } catch (error) {
      console.error('Error fetching recommendations:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleGenerateRecommendations = async () => {
    try {
      const response = await fetch('http://localhost:53133/api/recommendations/generate', {
        method: 'POST'
      });
      
      if (response.ok) {
        const newRecommendations = await response.json();
        setRecommendations([...newRecommendations, ...recommendations]);
        setActiveRecommendations([...newRecommendations, ...activeRecommendations]);
      }
    } catch (error) {
      console.error('Error generating recommendations:', error);
    }
  };

  const handleExecuteRecommendation = async (recommendationId: string, action: RecommendationAction) => {
    try {
      const response = await fetch(`http://localhost:53133/api/recommendations/${recommendationId}/execute`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          action,
          price: 0, // This would be the actual execution price
          outcome: null
        })
      });
      
      if (response.ok) {
        const updatedRecommendation = await response.json();
        setRecommendations(recommendations.map((r: Recommendation) => 
          r.id === recommendationId ? updatedRecommendation : r
        ));
        setActiveRecommendations(activeRecommendations.filter((r: Recommendation) => r.id !== recommendationId));
        const buyRecommendations = activeRecommendations.filter((r: Recommendation) => r.action === 'Buy' || r.action === 'StrongBuy');
      }
    } catch (error: any) {
      console.error('Error executing recommendation:', error);
    }
  };

  const getActionColor = (action: RecommendationAction) => {
    switch (action) {
      case RecommendationAction.StrongBuy:
      case RecommendationAction.Buy:
        return 'bg-green-100 text-green-800';
      case RecommendationAction.Sell:
      case RecommendationAction.StrongSell:
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-yellow-100 text-yellow-800';
    }
  };

  const getRiskColor = (risk: RiskLevel) => {
    switch (risk) {
      case RiskLevel.Low:
        return 'bg-green-100 text-green-800';
      case RiskLevel.Medium:
        return 'bg-yellow-100 text-yellow-800';
      case RiskLevel.High:
        return 'bg-orange-100 text-orange-800';
      case RiskLevel.VeryHigh:
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusIcon = (status: RecommendationStatus) => {
    switch (status) {
      case RecommendationStatus.Active:
        return <LightBulbIcon className="h-5 w-5 text-yellow-500" />;
      case RecommendationStatus.Executed:
        return <CheckCircleIcon className="h-5 w-5 text-green-500" />;
      case RecommendationStatus.Expired:
        return <ClockIcon className="h-5 w-5 text-gray-500" />;
      case RecommendationStatus.Cancelled:
        return <XCircleIcon className="h-5 w-5 text-red-500" />;
      default:
        return null;
    }
  };

  const displayedRecommendations = selectedTab === 'active' 
    ? activeRecommendations 
    : selectedTab === 'all' 
    ? recommendations 
    : recommendations.filter((r: Recommendation) => r.status !== RecommendationStatus.Active);

  if (isLoading) {
    return (
      <div className="p-6">
        <div className="animate-pulse">
          <div className="h-8 bg-gray-200 rounded w-1/4 mb-6"></div>
          <div className="space-y-4">
            {[1, 2, 3, 4, 5].map(i => (
              <div key={i} className="bg-white p-6 rounded-lg shadow">
                <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
                <div className="h-3 bg-gray-200 rounded w-1/2 mb-4"></div>
                <div className="grid grid-cols-4 gap-4">
                  <div className="h-3 bg-gray-200 rounded"></div>
                  <div className="h-3 bg-gray-200 rounded"></div>
                  <div className="h-3 bg-gray-200 rounded"></div>
                  <div className="h-3 bg-gray-200 rounded"></div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Recommendations</h1>
          <p className="text-gray-600">AI-powered stock recommendations based on your strategy</p>
        </div>
        <button
          onClick={handleGenerateRecommendations}
          className="btn btn-primary flex items-center space-x-2"
        >
          <LightBulbIcon className="h-5 w-5" />
          <span>Generate New</span>
        </button>
      </div>

      {/* Tabs */}
      <div className="border-b border-gray-200 mb-6">
        <nav className="-mb-px flex space-x-8">
          <button
            onClick={() => setSelectedTab('active')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              selectedTab === 'active'
                ? 'border-primary-500 text-primary-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            Active ({activeRecommendations.length})
          </button>
          <button
            onClick={() => setSelectedTab('all')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              selectedTab === 'all'
                ? 'border-primary-500 text-primary-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            All ({recommendations.length})
          </button>
          <button
            onClick={() => setSelectedTab('history')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              selectedTab === 'history'
                ? 'border-primary-500 text-primary-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            History
          </button>
        </nav>
      </div>

      {displayedRecommendations.length === 0 ? (
        <div className="card p-12 text-center">
          <LightBulbIcon className="h-16 w-16 text-gray-400 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">
            {selectedTab === 'active' ? 'No active recommendations' : 'No recommendations found'}
          </h3>
          <p className="text-gray-500 mb-6">
            {selectedTab === 'active' 
              ? 'Generate new recommendations to get started'
              : 'Start by generating some recommendations'
            }
          </p>
          {selectedTab === 'active' && (
            <button
              onClick={handleGenerateRecommendations}
              className="btn btn-primary"
            >
              Generate Recommendations
            </button>
          )}
        </div>
      ) : (
        <div className="space-y-4">
          {displayedRecommendations.map((recommendation: Recommendation) => (
            <div key={recommendation.id} className="card p-6">
              <div className="flex justify-between items-start mb-4">
                <div className="flex items-center space-x-3">
                  {getStatusIcon(recommendation.status)}
                  <div>
                    <h3 className="text-lg font-semibold text-gray-900">
                      {recommendation.symbol}
                    </h3>
                    <p className="text-sm text-gray-500">
                      Created {new Date(recommendation.createdAt).toLocaleDateString()}
                    </p>
                  </div>
                </div>
                <div className="flex items-center space-x-2">
                  <span className={`px-3 py-1 text-xs font-medium rounded-full ${getActionColor(recommendation.action)}`}>
                    {recommendation.action}
                  </span>
                  <span className={`px-3 py-1 text-xs font-medium rounded-full ${getRiskColor(recommendation.riskLevel)}`}>
                    {recommendation.riskLevel} Risk
                  </span>
                </div>
              </div>

              <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-4">
                <div>
                  <p className="text-sm text-gray-500">Confidence</p>
                  <div className="flex items-center">
                    <div 
                      className="flex-1 bg-gray-200 rounded-full h-2 mr-2"
                      style={{ width: `${recommendation.confidence}%` }}
                    >
                      <div 
                        className="bg-primary-600 h-2 rounded-full"
                        style={{ width: `${recommendation.confidence}%` }}
                      ></div>
                    </div>
                    <span className="text-sm font-medium">{Math.round(recommendation.confidence)}%</span>
                  </div>
                </div>
                <div>
                  <p className="text-sm text-gray-500">Target Price</p>
                  <p className="text-lg font-semibold text-gray-900">${recommendation.targetPrice}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-500">Stop Loss</p>
                  <p className="text-lg font-semibold text-red-600">${recommendation.stopLoss}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-500">Time Horizon</p>
                  <p className="text-sm font-medium text-gray-900">{recommendation.timeHorizon}</p>
                </div>
              </div>

              <div className="mb-4">
                <p className="text-sm font-medium text-gray-900 mb-2">Reasoning</p>
                <p className="text-sm text-gray-600">{recommendation.reasoning}</p>
              </div>

              {recommendation.keyFactors.length > 0 && (
                <div className="mb-4">
                  <p className="text-sm font-medium text-gray-900 mb-2">Key Factors</p>
                  <div className="flex flex-wrap gap-2">
                    {recommendation.keyFactors.map((factor: string, index: number) => (
                      <span
                        key={index}
                        className="px-2 py-1 text-xs bg-gray-100 text-gray-700 rounded-full"
                      >
                        {factor}
                      </span>
                    ))}
                  </div>
                </div>
              )}

              <div className="grid grid-cols-3 gap-4 mb-4">
                <div className="text-center">
                  <p className="text-xs text-gray-500">Technical Score</p>
                  <p className="text-sm font-semibold text-gray-900">{recommendation.technicalScore}</p>
                </div>
                <div className="text-center">
                  <p className="text-xs text-gray-500">Fundamental Score</p>
                  <p className="text-sm font-semibold text-gray-900">{recommendation.fundamentalScore}</p>
                </div>
                <div className="text-center">
                  <p className="text-xs text-gray-500">Sentiment Score</p>
                  <p className="text-sm font-semibold text-gray-900">{recommendation.sentimentScore}</p>
                </div>
              </div>

              {recommendation.status === RecommendationStatus.Active && (
                <div className="flex justify-end space-x-3">
                  <button
                    onClick={() => handleExecuteRecommendation(recommendation.id, RecommendationAction.Buy)}
                    className="btn btn-success"
                  >
                    Execute Buy
                  </button>
                  <button
                    onClick={() => handleExecuteRecommendation(recommendation.id, RecommendationAction.Sell)}
                    className="btn btn-danger"
                  >
                    Execute Sell
                  </button>
                </div>
              )}

              {recommendation.status === RecommendationStatus.Executed && (
                <div className="flex justify-end items-center space-x-3">
                  <span className="text-sm text-gray-500">
                    Executed on {recommendation.executedAt ? new Date(recommendation.executedAt).toLocaleDateString() : 'Unknown'}
                  </span>
                  {recommendation.actualAction && (
                    <span className={`px-2 py-1 text-xs font-medium rounded-full ${getActionColor(recommendation.actualAction)}`}>
                      {recommendation.actualAction}
                    </span>
                  )}
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
