import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { Portfolio, Holding, AssetType } from '../types/models';
import {
  ArrowTrendingUpIcon,
  ArrowTrendingDownIcon,
  PlusIcon,
  PencilIcon,
  TrashIcon,
  CurrencyDollarIcon
} from '@heroicons/react/24/outline';

export const Portfolios: React.FC = () => {
  const { user } = useAuth();
  const [portfolios, setPortfolios] = useState<Portfolio[]>([]);
  const [selectedPortfolio, setSelectedPortfolio] = useState<Portfolio | null>(null);
  const [selectedHolding, setSelectedHolding] = useState<Holding | null>(null);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isAddHoldingOpen, setIsAddHoldingOpen] = useState(false);
  const [isEditHoldingOpen, setIsEditHoldingOpen] = useState(false);
  const [isTransactionModalOpen, setIsTransactionModalOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState<string | null>(null);
  const [showDeleteHoldingConfirm, setShowDeleteHoldingConfirm] = useState(false);

  useEffect(() => {
    fetchPortfolios();
  }, []);

  const fetchPortfolios = async () => {
    try {
      const response = await fetch('http://localhost:53133/api/portfolios');
      const data: Portfolio[] = await response.json();
      setPortfolios(data);
    } catch (error) {
      console.error('Error fetching portfolios:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleCreatePortfolio = async (portfolioData: Partial<Portfolio>) => {
    try {
      const response = await fetch('http://localhost:53133/api/portfolios', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(portfolioData),
      });
      
      if (response.ok) {
        const newPortfolio: Portfolio = await response.json();
        setPortfolios([...portfolios, newPortfolio]);
        setIsCreateModalOpen(false);
      }
    } catch (error) {
      console.error('Error creating portfolio:', error);
    }
  };

  const handleUpdatePortfolio = async (portfolioData: Portfolio | Partial<Portfolio>) => {
    try {
      const response = await fetch(`http://localhost:53133/api/portfolios/${portfolioData.id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(portfolioData),
      });
      
      if (response.ok) {
        const updatedPortfolio: Portfolio = await response.json();
        setPortfolios(portfolios.map(p => p.id === updatedPortfolio.id ? updatedPortfolio : p));
        if (selectedPortfolio?.id === updatedPortfolio.id) {
          setSelectedPortfolio(updatedPortfolio);
        }
        setIsEditModalOpen(false);
      }
    } catch (error) {
      console.error('Error updating portfolio:', error);
    }
  };

  const handleDeletePortfolio = async (portfolioId: string) => {
    setShowDeleteConfirm(portfolioId);
  };

  const confirmDeletePortfolio = async () => {
    if (!showDeleteConfirm) return;
    
    try {
      const response = await fetch(`http://localhost:53133/api/portfolios/${showDeleteConfirm}`, {
        method: 'DELETE',
      });
      
      if (response.ok) {
        setPortfolios(portfolios.filter(p => p.id !== showDeleteConfirm));
        if (selectedPortfolio?.id === showDeleteConfirm) {
          setSelectedPortfolio(null);
        }
      }
    } catch (error) {
      console.error('Error deleting portfolio:', error);
    } finally {
      setShowDeleteConfirm(null);
    }
  };

  if (isLoading) {
    return (
      <div className="p-6">
        <div className="animate-pulse">
          <div className="h-8 bg-gray-200 rounded w-1/4 mb-6"></div>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {[1, 2, 3].map(i => (
              <div key={i} className="bg-white p-6 rounded-lg shadow">
                <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
                <div className="h-6 bg-gray-200 rounded w-1/2 mb-4"></div>
                <div className="space-y-2">
                  <div className="h-3 bg-gray-200 rounded"></div>
                  <div className="h-3 bg-gray-200 rounded w-5/6"></div>
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
          <h1 className="text-2xl font-bold text-gray-900">Portfolios</h1>
          <p className="text-gray-600">Manage your investment portfolios</p>
        </div>
        <button
          onClick={() => setIsCreateModalOpen(true)}
          className="btn btn-primary flex items-center space-x-2"
        >
          <PlusIcon className="h-5 w-5" />
          <span>Create Portfolio</span>
        </button>
      </div>

      {portfolios.length === 0 ? (
        <div className="card p-12 text-center">
          <div className="w-16 h-16 bg-gray-200 rounded-full flex items-center justify-center mx-auto mb-4">
            <CurrencyDollarIcon className="h-8 w-8 text-gray-400" />
          </div>
          <h3 className="text-lg font-medium text-gray-900 mb-2">No portfolios yet</h3>
          <p className="text-gray-500 mb-6">Create your first portfolio to start tracking your investments</p>
          <button
            onClick={() => setIsCreateModalOpen(true)}
            className="btn btn-primary"
          >
            Create Portfolio
          </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Portfolio List */}
          <div className="lg:col-span-1">
            <div className="space-y-4">
              {portfolios.map((portfolio: Portfolio) => (
                <div
                  key={portfolio.id}
                  className={`card p-4 cursor-pointer transition-all ${
                    selectedPortfolio?.id === portfolio.id
                      ? 'ring-2 ring-primary-500 border-primary-500'
                      : 'hover:shadow-md'
                  }`}
                  onClick={() => setSelectedPortfolio(portfolio)}
                >
                  <div className="flex justify-between items-start mb-2">
                    <h3 className="font-semibold text-gray-900">{portfolio.name}</h3>
                    <div className="flex space-x-1">
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          setSelectedPortfolio(portfolio);
                          setIsEditModalOpen(true);
                        }}
                        className="p-1 text-gray-400 hover:text-gray-600"
                      >
                        <PencilIcon className="h-4 w-4" />
                      </button>
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          handleDeletePortfolio(portfolio.id);
                        }}
                        className="p-1 text-gray-400 hover:text-red-600"
                      >
                        <TrashIcon className="h-4 w-4" />
                      </button>
                    </div>
                  </div>
                  <p className="text-sm text-gray-600 mb-3">{portfolio.description}</p>
                  <div className="space-y-1">
                    <div className="flex justify-between">
                      <span className="text-sm text-gray-500">Total Value:</span>
                      <span className="text-sm font-medium">
                        ${portfolio.totalValue.toLocaleString()}
                      </span>
                    </div>
                    <div className="flex justify-between items-center">
                      <span className="text-sm text-gray-500">Gain/Loss:</span>
                      <span className={`text-sm font-medium flex items-center ${
                        portfolio.totalGainLoss >= 0 ? 'text-green-600' : 'text-red-600'
                      }`}>
                        {portfolio.totalGainLoss >= 0 ? (
                          <ArrowTrendingUpIcon className="h-3 w-3 mr-1" />
                        ) : (
                          <ArrowTrendingDownIcon className="h-3 w-3 mr-1" />
                        )}
                        {portfolio.totalGainLoss >= 0 ? '+' : ''}{portfolio.totalGainLossPercentage.toFixed(2)}%
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-gray-500">Holdings:</span>
                      <span className="text-sm font-medium">{portfolio.holdings.length}</span>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Portfolio Details */}
          <div className="lg:col-span-2">
            {selectedPortfolio ? (
              <PortfolioDetails
                portfolio={selectedPortfolio}
                onUpdate={handleUpdatePortfolio}
                selectedHolding={selectedHolding}
                setSelectedHolding={setSelectedHolding}
                isEditHoldingOpen={isEditHoldingOpen}
                setIsEditHoldingOpen={setIsEditHoldingOpen}
                isTransactionModalOpen={isTransactionModalOpen}
                setIsTransactionModalOpen={setIsTransactionModalOpen}
                showDeleteHoldingConfirm={showDeleteHoldingConfirm}
                setShowDeleteHoldingConfirm={setShowDeleteHoldingConfirm}
              />
            ) : (
              <div className="card p-12 text-center">
                <div className="w-16 h-16 bg-gray-200 rounded-full flex items-center justify-center mx-auto mb-4">
                  <CurrencyDollarIcon className="h-8 w-8 text-gray-400" />
                </div>
                <h3 className="text-lg font-medium text-gray-900 mb-2">Select a portfolio</h3>
                <p className="text-gray-500">Choose a portfolio from the list to view details</p>
              </div>
            )}
          </div>
        </div>
      )}

      {/* Create Portfolio Modal */}
      {isCreateModalOpen && (
        <PortfolioModal
          portfolio={null}
          onClose={() => setIsCreateModalOpen(false)}
          onSave={handleCreatePortfolio}
        />
      )}

      {/* Edit Portfolio Modal */}
      {isEditModalOpen && selectedPortfolio && (
        <PortfolioModal
          portfolio={selectedPortfolio}
          onClose={() => {
            setIsEditModalOpen(false);
            setSelectedPortfolio(null);
          }}
          onSave={handleUpdatePortfolio}
        />
      )}

      {/* Delete Confirmation Modal */}
      {showDeleteConfirm && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg max-w-md w-full p-6">
            <div className="flex items-center mb-4">
              <div className="w-12 h-12 bg-red-100 rounded-full flex items-center justify-center mr-4">
                <TrashIcon className="h-6 w-6 text-red-600" />
              </div>
              <div>
                <h3 className="text-lg font-semibold text-gray-900">Delete Portfolio</h3>
                <p className="text-sm text-gray-500">This action cannot be undone</p>
              </div>
            </div>
            <p className="text-gray-600 mb-6">
              Are you sure you want to delete this portfolio? All associated holdings and transaction history will be permanently removed.
            </p>
            <div className="flex justify-end space-x-3">
              <button
                onClick={() => setShowDeleteConfirm(null)}
                className="btn btn-outline"
              >
                Cancel
              </button>
              <button
                onClick={confirmDeletePortfolio}
                className="btn btn-danger"
              >
                Delete Portfolio
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

interface PortfolioModalProps {
  portfolio: Portfolio | null;
  onClose: () => void;
  onSave: (portfolio: Portfolio | Partial<Portfolio>) => void;
}

interface CreatePortfolioModalProps {
  onClose: () => void;
  onSave: (portfolio: Partial<Portfolio>) => void;
}

const PortfolioModal: React.FC<PortfolioModalProps> = ({ portfolio, onClose, onSave }: PortfolioModalProps) => {
  const [formData, setFormData] = useState({
    name: portfolio?.name || '',
    description: portfolio?.description || '',
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (portfolio) {
      onSave({ ...portfolio, ...formData });
    } else {
      onSave(formData);
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg max-w-md w-full p-6">
        <h2 className="text-xl font-bold text-gray-900 mb-4">
          {portfolio ? 'Edit Portfolio' : 'Create Portfolio'}
        </h2>
        <form onSubmit={handleSubmit}>
          <div className="mb-4">
            <label className="label">Name</label>
            <input
              type="text"
              className="input"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              required
            />
          </div>
          <div className="mb-6">
            <label className="label">Description</label>
            <textarea
              className="input"
              rows={3}
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
            />
          </div>
          <div className="flex justify-end space-x-3">
            <button
              type="button"
              onClick={onClose}
              className="btn btn-outline"
            >
              Cancel
            </button>
            <button
              type="submit"
              className="btn btn-primary"
            >
              {portfolio ? 'Save Changes' : 'Create Portfolio'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

interface PortfolioDetailsProps {
  portfolio: Portfolio;
  onUpdate: (portfolio: Portfolio) => void;
  selectedHolding: Holding | null;
  setSelectedHolding: (holding: Holding | null) => void;
  isEditHoldingOpen: boolean;
  setIsEditHoldingOpen: (open: boolean) => void;
  isTransactionModalOpen: boolean;
  setIsTransactionModalOpen: (open: boolean) => void;
  showDeleteHoldingConfirm: boolean;
  setShowDeleteHoldingConfirm: (show: boolean) => void;
}

const PortfolioDetails: React.FC<PortfolioDetailsProps> = ({ 
  portfolio, 
  onUpdate, 
  selectedHolding,
  setSelectedHolding,
  isEditHoldingOpen,
  setIsEditHoldingOpen,
  isTransactionModalOpen,
  setIsTransactionModalOpen,
  showDeleteHoldingConfirm,
  setShowDeleteHoldingConfirm
}: PortfolioDetailsProps) => {
  const [isAddHoldingOpen, setIsAddHoldingOpen] = useState(false);

  const handleAddHolding = async (holdingData: Partial<Holding>) => {
    try {
      // Convert assetType to string for proper backend handling
      const payload = {
        holding: {
          ...holdingData,
          assetType: holdingData.assetType?.toString() || "Stock"
        }
      };
      
      const response = await fetch(`http://localhost:53133/api/portfolios/${portfolio.id}/holdings`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      });
      
      if (response.ok) {
        const updatedPortfolio: Portfolio = await response.json();
        onUpdate(updatedPortfolio);
        setIsAddHoldingOpen(false);
      }
    } catch (error) {
      console.error('Error adding holding:', error);
    }
  };

  const handleEditHolding = async (holdingData: Holding) => {
    if (!portfolio || !selectedHolding) return;
    
    try {
      // Convert assetType to string for proper backend handling
      const payload = {
        holding: {
          symbol: holdingData.symbol,
          quantity: holdingData.quantity,
          averageCostPrice: holdingData.averageCostPrice,
          currentPrice: holdingData.currentPrice,
          assetType: holdingData.assetType?.toString() || "Stock"
        }
      };
      
      const response = await fetch(`http://localhost:53133/api/portfolios/${portfolio.id}/holdings/${selectedHolding.id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      });
      
      if (response.ok) {
        const updatedPortfolio: Portfolio = await response.json();
        onUpdate(updatedPortfolio);
        setIsEditHoldingOpen(false);
        setSelectedHolding(null);
      }
    } catch (error) {
      console.error('Error editing holding:', error);
    }
  };

  const handleDeleteHolding = async () => {
    if (!portfolio || !selectedHolding) return;
    
    try {
      const response = await fetch(`http://localhost:53133/api/portfolios/${portfolio.id}/holdings/${selectedHolding.id}`, {
        method: 'DELETE',
      });
      
      if (response.ok) {
        const updatedPortfolio: Portfolio = await response.json();
        onUpdate(updatedPortfolio);
        setShowDeleteHoldingConfirm(false);
        setSelectedHolding(null);
      }
    } catch (error) {
      console.error('Error deleting holding:', error);
    }
  };

  return (
    <div className="card p-6">
      <div className="flex justify-between items-center mb-6">
        <div>
          <h2 className="text-xl font-bold text-gray-900">{portfolio.name}</h2>
          <p className="text-gray-600">{portfolio.description}</p>
        </div>
        <button
          onClick={() => setIsAddHoldingOpen(true)}
          className="btn btn-primary flex items-center space-x-2"
        >
          <PlusIcon className="h-5 w-5" />
          <span>Add Holding</span>
        </button>
      </div>

      {/* Portfolio Summary */}
      <div className="grid grid-cols-3 gap-4 mb-6">
        <div className="text-center p-4 bg-gray-50 rounded-lg">
          <p className="text-sm text-gray-600 mb-1">Total Value</p>
          <p className="text-xl font-bold text-gray-900">
            ${portfolio.totalValue.toLocaleString()}
          </p>
        </div>
        <div className="text-center p-4 bg-gray-50 rounded-lg">
          <p className="text-sm text-gray-600 mb-1">Total Cost</p>
          <p className="text-xl font-bold text-gray-900">
            ${portfolio.totalCost.toLocaleString()}
          </p>
        </div>
        <div className={`text-center p-4 rounded-lg ${
          portfolio.totalGainLoss >= 0 ? 'bg-green-50' : 'bg-red-50'
        }`}>
          <p className="text-sm text-gray-600 mb-1">Gain/Loss</p>
          <p className={`text-xl font-bold ${
            portfolio.totalGainLoss >= 0 ? 'text-green-600' : 'text-red-600'
          }`}>
            {portfolio.totalGainLoss >= 0 ? '+' : ''}${portfolio.totalGainLoss.toLocaleString()}
          </p>
          <p className={`text-sm ${
            portfolio.totalGainLoss >= 0 ? 'text-green-600' : 'text-red-600'
          }`}>
            {portfolio.totalGainLoss >= 0 ? '+' : ''}{portfolio.totalGainLossPercentage.toFixed(2)}%
          </p>
        </div>
      </div>

      {/* Holdings */}
      <div>
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Holdings</h3>
        {portfolio.holdings.length === 0 ? (
          <div className="text-center py-8 bg-gray-50 rounded-lg">
            <CurrencyDollarIcon className="h-12 w-12 text-gray-400 mx-auto mb-3" />
            <p className="text-gray-500">No holdings yet</p>
            <p className="text-sm text-gray-400">Add your first holding to get started</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Symbol
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Quantity
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Avg Cost
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Current Price
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Total Value
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Gain/Loss
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {portfolio.holdings.map((holding: Holding) => (
                  <tr key={holding.id}>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      {holding.symbol}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {holding.quantity}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      ${holding.averageCostPrice.toFixed(2)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      ${holding.currentPrice.toFixed(2)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      ${holding.currentValue.toLocaleString()}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm">
                      <div className={`font-medium ${
                        holding.gainLoss >= 0 ? 'text-green-600' : 'text-red-600'
                      }`}>
                        {holding.gainLoss >= 0 ? '+' : ''}${holding.gainLoss.toFixed(2)}
                      </div>
                      <div className={`text-xs ${
                        holding.gainLoss >= 0 ? 'text-green-600' : 'text-red-600'
                      }`}>
                        {holding.gainLoss >= 0 ? '+' : ''}{holding.gainLossPercentage.toFixed(2)}%
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      <div className="flex space-x-2">
                        <button
                          onClick={() => {
                            setSelectedHolding(holding);
                            setIsEditHoldingOpen(true);
                          }}
                          className="text-indigo-600 hover:text-indigo-900"
                          title="Edit holding"
                        >
                          <PencilIcon className="h-4 w-4" />
                        </button>
                        <button
                          onClick={() => {
                            setSelectedHolding(holding);
                            setShowDeleteHoldingConfirm(true);
                          }}
                          className="text-red-600 hover:text-red-900"
                          title="Delete holding"
                        >
                          <TrashIcon className="h-4 w-4" />
                        </button>
                        <button
                          onClick={() => {
                            setSelectedHolding(holding);
                            setIsTransactionModalOpen(true);
                          }}
                          className="text-green-600 hover:text-green-900"
                          title="Add transaction"
                        >
                          <PlusIcon className="h-4 w-4" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Add Holding Modal */}
      {isAddHoldingOpen && (
        <HoldingModal
          onClose={() => setIsAddHoldingOpen(false)}
          onSave={handleAddHolding}
        />
      )}

      {/* Edit Holding Modal */}
      {isEditHoldingOpen && selectedHolding && (
        <HoldingModal
          holding={selectedHolding}
          onClose={() => {
            setIsEditHoldingOpen(false);
            setSelectedHolding(null);
          }}
          onSave={handleAddHolding}
          onEdit={handleEditHolding}
        />
      )}

      {/* Delete Holding Confirmation Modal */}
      {showDeleteHoldingConfirm && selectedHolding && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg max-w-md w-full p-6">
            <div className="flex items-center mb-4">
              <div className="w-12 h-12 bg-red-100 rounded-full flex items-center justify-center mr-4">
                <TrashIcon className="h-6 w-6 text-red-600" />
              </div>
              <div>
                <h3 className="text-lg font-semibold text-gray-900">Delete Holding</h3>
                <p className="text-sm text-gray-500">This action cannot be undone</p>
              </div>
            </div>
            <p className="text-gray-600 mb-6">
              Are you sure you want to delete {selectedHolding.quantity} shares of {selectedHolding.symbol}? 
              All transaction history will be permanently removed.
            </p>
            <div className="flex justify-end space-x-3">
              <button
                onClick={() => {
                  setShowDeleteHoldingConfirm(false);
                  setSelectedHolding(null);
                }}
                className="btn btn-outline"
              >
                Cancel
              </button>
              <button
                onClick={handleDeleteHolding}
                className="btn btn-danger"
              >
                Delete Holding
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Transaction Modal */}
      {isTransactionModalOpen && selectedHolding && (
        <TransactionModal
          holding={selectedHolding}
          onClose={() => {
            setIsTransactionModalOpen(false);
            setSelectedHolding(null);
          }}
          onSave={async (transaction) => {
            try {
              const payload = {
                type: transaction.type,
                quantity: transaction.quantity,
                price: transaction.price,
                notes: transaction.notes || '',
                fees: transaction.fees || 0,
                timestamp: transaction.timestamp || new Date().toISOString()
              };

              const response = await fetch(`http://localhost:53133/api/portfolios/${portfolio.id}/holdings/${selectedHolding.id}/transactions`, {
                method: 'POST',
                headers: {
                  'Content-Type': 'application/json',
                },
                body: JSON.stringify(payload),
              });
              
              if (response.ok) {
                const updatedPortfolio = await response.json();
                console.log('Transaction created and portfolio updated:', updatedPortfolio);
                // Update the portfolio with the new transaction data
                onUpdate(updatedPortfolio);
                setIsTransactionModalOpen(false);
              } else {
                console.error('Failed to create transaction');
              }
            } catch (error) {
              console.error('Error creating transaction:', error);
            }
          }}
        />
      )}
    </div>
  );
};

interface HoldingModalProps {
  holding?: Holding | null;
  onClose: () => void;
  onSave: (holding: Partial<Holding>) => Promise<void> | void;
  onEdit?: (holding: Holding) => Promise<void> | void;
}

const HoldingModal: React.FC<HoldingModalProps> = ({ holding, onClose, onSave, onEdit }: HoldingModalProps) => {
  const [formData, setFormData] = useState({
    symbol: holding?.symbol || '',
    quantity: holding?.quantity || 0,
    averageCostPrice: holding?.averageCostPrice || 0,
    currentPrice: holding?.currentPrice || 0,
    assetType: holding?.assetType || AssetType.Stock,
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (holding && onEdit) {
      onEdit({ ...holding, ...formData });
    } else {
      onSave(formData);
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg max-w-md w-full p-6">
        <h2 className="text-xl font-bold text-gray-900 mb-4">
          {holding ? 'Edit Holding' : 'Add Holding'}
        </h2>
        <form onSubmit={handleSubmit}>
          <div className="mb-4">
            <label className="label">Symbol</label>
            <input
              type="text"
              className="input"
              value={formData.symbol}
              onChange={(e) => setFormData({ ...formData, symbol: e.target.value.toUpperCase() })}
              required
              disabled={!!holding} // Can't change symbol when editing
            />
          </div>
          <div className="mb-4">
            <label className="label">Quantity</label>
            <input
              type="number"
              step="0.01"
              className="input"
              value={formData.quantity}
              onChange={(e) => setFormData({ ...formData, quantity: parseFloat(e.target.value) || 0 })}
              required
            />
          </div>
          <div className="mb-4">
            <label className="label">Average Cost Price</label>
            <input
              type="number"
              step="0.01"
              className="input"
              value={formData.averageCostPrice}
              onChange={(e) => setFormData({ ...formData, averageCostPrice: parseFloat(e.target.value) || 0 })}
              required
            />
          </div>
          <div className="mb-4">
            <label className="label">Current Price</label>
            <input
              type="number"
              step="0.01"
              className="input"
              value={formData.currentPrice}
              onChange={(e) => setFormData({ ...formData, currentPrice: parseFloat(e.target.value) || 0 })}
              required
            />
          </div>
          <div className="mb-6">
            <label className="label">Asset Type</label>
            <select
              className="input"
              value={formData.assetType}
              onChange={(e) => setFormData({ ...formData, assetType: e.target.value as AssetType })}
            >
              {Object.values(AssetType).map((type) => (
                <option key={type} value={type}>{type}</option>
              ))}
            </select>
          </div>
          <div className="flex justify-end space-x-3">
            <button
              type="button"
              onClick={onClose}
              className="btn btn-outline"
            >
              Cancel
            </button>
            <button
              type="submit"
              className="btn btn-primary"
            >
              {holding ? 'Save Changes' : 'Add Holding'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

interface TransactionModalProps {
  holding: Holding;
  onClose: () => void;
  onSave: (transaction: any) => void;
}

const TransactionModal: React.FC<TransactionModalProps> = ({ holding, onClose, onSave }: TransactionModalProps) => {
  const [formData, setFormData] = useState({
    type: 'Buy' as 'Buy' | 'Sell' | 'Dividend',
    quantity: 0,
    price: 0,
    notes: '',
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const transaction = {
      id: crypto.randomUUID(),
      holdingId: holding.id,
      ...formData,
      timestamp: new Date().toISOString(),
    };
    onSave(transaction);
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg max-w-md w-full p-6">
        <h2 className="text-xl font-bold text-gray-900 mb-4">Add Transaction - {holding.symbol}</h2>
        <form onSubmit={handleSubmit}>
          <div className="mb-4">
            <label className="label">Transaction Type</label>
            <select
              className="input"
              value={formData.type}
              onChange={(e) => setFormData({ ...formData, type: e.target.value as any })}
            >
              <option value="Buy">Buy</option>
              <option value="Sell">Sell</option>
              <option value="Dividend">Dividend</option>
            </select>
          </div>
          <div className="mb-4">
            <label className="label">Quantity</label>
            <input
              type="number"
              step="0.01"
              className="input"
              value={formData.quantity}
              onChange={(e) => setFormData({ ...formData, quantity: parseFloat(e.target.value) || 0 })}
              required
            />
          </div>
          <div className="mb-4">
            <label className="label">Price</label>
            <input
              type="number"
              step="0.01"
              className="input"
              value={formData.price}
              onChange={(e) => setFormData({ ...formData, price: parseFloat(e.target.value) || 0 })}
              required
            />
          </div>
          <div className="mb-6">
            <label className="label">Notes</label>
            <textarea
              className="input"
              rows={3}
              value={formData.notes}
              onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
            />
          </div>
          <div className="flex justify-end space-x-3">
            <button
              type="button"
              onClick={onClose}
              className="btn btn-outline"
            >
              Cancel
            </button>
            <button
              type="submit"
              className="btn btn-primary"
            >
              Add Transaction
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
