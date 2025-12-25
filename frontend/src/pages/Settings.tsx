import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { User, TradingStrategy, RiskTolerance, InvestmentHorizon } from '../types/models';
import { 
  CogIcon,
  UserIcon,
  ChartBarIcon,
  ShieldCheckIcon
} from '@heroicons/react/24/outline';

export const Settings: React.FC = () => {
  const { user, updateUser } = useAuth();
  const [activeTab, setActiveTab] = useState<'profile' | 'strategy' | 'notifications'>('profile');
  const [isSaving, setIsSaving] = useState(false);
  const [saveMessage, setSaveMessage] = useState('');

  const [profileData, setProfileData] = useState({
    displayName: '',
    email: '',
  });

  const [strategyData, setStrategyData] = useState<TradingStrategy>({
    riskTolerance: 'Medium',
    investmentHorizon: 'MediumTerm',
    maxPortfolioSize: 20,
    preferredSectors: [],
    sellStrategy: {
      takeProfitPercentage: 20,
      stopLossPercentage: 10,
      trailingStopEnabled: false,
      trailingStopPercentage: 5,
    },
  });

  useEffect(() => {
    if (user) {
      setProfileData({
        displayName: user.displayName,
        email: user.email,
      });
      setStrategyData(user.tradingStrategy);
    }
  }, [user]);

  const handleSaveProfile = async () => {
    if (!user) return;
    
    setIsSaving(true);
    try {
      const updatedUser = {
        ...user,
        ...profileData,
      };

      const response = await fetch('/api/auth/profile', {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(updatedUser),
      });

      if (response.ok) {
        const savedUser = await response.json();
        updateUser(savedUser);
        setSaveMessage('Profile updated successfully!');
        setTimeout(() => setSaveMessage(''), 3000);
      }
    } catch (error) {
      console.error('Error saving profile:', error);
      setSaveMessage('Error saving profile');
      setTimeout(() => setSaveMessage(''), 3000);
    } finally {
      setIsSaving(false);
    }
  };

  const handleSaveStrategy = async () => {
    if (!user) return;
    
    setIsSaving(true);
    try {
      const updatedUser = {
        ...user,
        tradingStrategy: strategyData,
      };

      const response = await fetch('/api/auth/profile', {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(updatedUser),
      });

      if (response.ok) {
        const savedUser = await response.json();
        updateUser(savedUser);
        setSaveMessage('Trading strategy updated successfully!');
        setTimeout(() => setSaveMessage(''), 3000);
      }
    } catch (error) {
      console.error('Error saving strategy:', error);
      setSaveMessage('Error saving strategy');
      setTimeout(() => setSaveMessage(''), 3000);
    } finally {
      setIsSaving(false);
    }
  };

  const handleSectorToggle = (sector: string) => {
    setStrategyData(prev => ({
      ...prev,
      preferredSectors: prev.preferredSectors.includes(sector)
        ? prev.preferredSectors.filter(s => s !== sector)
        : [...prev.preferredSectors, sector]
    }));
  };

  const sectors = [
    'Technology', 'Healthcare', 'Finance', 'Consumer', 'Energy',
    'Industrial', 'Materials', 'Utilities', 'Real Estate', 'Communication'
  ];

  if (!user) {
    return (
      <div className="p-6">
        <div className="text-center">
          <p>Loading user data...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Settings</h1>
        <p className="text-gray-600">Manage your account and trading preferences</p>
      </div>

      {/* Tabs */}
      <div className="border-b border-gray-200 mb-6">
        <nav className="-mb-px flex space-x-8">
          <button
            onClick={() => setActiveTab('profile')}
            className={`py-2 px-1 border-b-2 font-medium text-sm flex items-center space-x-2 ${
              activeTab === 'profile'
                ? 'border-primary-500 text-primary-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            <UserIcon className="h-4 w-4" />
            <span>Profile</span>
          </button>
          <button
            onClick={() => setActiveTab('strategy')}
            className={`py-2 px-1 border-b-2 font-medium text-sm flex items-center space-x-2 ${
              activeTab === 'strategy'
                ? 'border-primary-500 text-primary-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            <ChartBarIcon className="h-4 w-4" />
            <span>Trading Strategy</span>
          </button>
          <button
            onClick={() => setActiveTab('notifications')}
            className={`py-2 px-1 border-b-2 font-medium text-sm flex items-center space-x-2 ${
              activeTab === 'notifications'
                ? 'border-primary-500 text-primary-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            <ShieldCheckIcon className="h-4 w-4" />
            <span>Notifications</span>
          </button>
        </nav>
      </div>

      {/* Save Message */}
      {saveMessage && (
        <div className={`mb-4 p-4 rounded-md ${
          saveMessage.includes('Error') ? 'bg-red-50 text-red-800' : 'bg-green-50 text-green-800'
        }`}>
          {saveMessage}
        </div>
      )}

      {/* Profile Tab */}
      {activeTab === 'profile' && (
        <div className="card p-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Profile Information</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label className="label">Display Name</label>
              <input
                type="text"
                className="input"
                value={profileData.displayName}
                onChange={(e) => setProfileData({ ...profileData, displayName: e.target.value })}
              />
            </div>
            <div>
              <label className="label">Email</label>
              <input
                type="email"
                className="input"
                value={profileData.email}
                onChange={(e) => setProfileData({ ...profileData, email: e.target.value })}
              />
            </div>
          </div>
          <div className="mt-6 flex justify-end">
            <button
              onClick={handleSaveProfile}
              disabled={isSaving}
              className="btn btn-primary"
            >
              {isSaving ? 'Saving...' : 'Save Profile'}
            </button>
          </div>
        </div>
      )}

      {/* Trading Strategy Tab */}
      {activeTab === 'strategy' && (
        <div className="space-y-6">
          <div className="card p-6">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">Risk Preferences</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <label className="label">Risk Tolerance</label>
                <select
                  className="input"
                  value={strategyData.riskTolerance}
                  onChange={(e) => setStrategyData({ 
                    ...strategyData, 
                    riskTolerance: e.target.value as RiskTolerance 
                  })}
                >
                  <option value="Low">Low</option>
                  <option value="Medium">Medium</option>
                  <option value="High">High</option>
                </select>
              </div>
              <div>
                <label className="label">Investment Horizon</label>
                <select
                  className="input"
                  value={strategyData.investmentHorizon}
                  onChange={(e) => setStrategyData({ 
                    ...strategyData, 
                    investmentHorizon: e.target.value as InvestmentHorizon 
                  })}
                >
                  <option value="ShortTerm">Short Term (1-4 weeks)</option>
                  <option value="MediumTerm">Medium Term (1-6 months)</option>
                  <option value="LongTerm">Long Term (6+ months)</option>
                </select>
              </div>
            </div>
          </div>

          <div className="card p-6">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">Portfolio Settings</h2>
            <div className="mb-6">
              <label className="label">Maximum Portfolio Size</label>
              <input
                type="number"
                className="input"
                value={strategyData.maxPortfolioSize}
                onChange={(e) => setStrategyData({ 
                  ...strategyData, 
                  maxPortfolioSize: parseInt(e.target.value) || 20 
                })}
                min="1"
                max="100"
              />
              <p className="text-sm text-gray-500 mt-1">Maximum number of holdings in your portfolio</p>
            </div>

            <div>
              <label className="label">Preferred Sectors</label>
              <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
                {sectors.map((sector) => (
                  <label key={sector} className="flex items-center space-x-2">
                    <input
                      type="checkbox"
                      className="rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                      checked={strategyData.preferredSectors.includes(sector)}
                      onChange={() => handleSectorToggle(sector)}
                    />
                    <span className="text-sm text-gray-700">{sector}</span>
                  </label>
                ))}
              </div>
              <p className="text-sm text-gray-500 mt-2">
                Select sectors you prefer for investment recommendations
              </p>
            </div>
          </div>

          <div className="card p-6">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">Sell Strategy</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <label className="label">Take Profit Percentage (%)</label>
                <input
                  type="number"
                  step="0.1"
                  className="input"
                  value={strategyData.sellStrategy.takeProfitPercentage}
                  onChange={(e) => setStrategyData({
                    ...strategyData,
                    sellStrategy: {
                      ...strategyData.sellStrategy,
                      takeProfitPercentage: parseFloat(e.target.value) || 20
                    }
                  })}
                  min="0"
                  max="100"
                />
              </div>
              <div>
                <label className="label">Stop Loss Percentage (%)</label>
                <input
                  type="number"
                  step="0.1"
                  className="input"
                  value={strategyData.sellStrategy.stopLossPercentage}
                  onChange={(e) => setStrategyData({
                    ...strategyData,
                    sellStrategy: {
                      ...strategyData.sellStrategy,
                      stopLossPercentage: parseFloat(e.target.value) || 10
                    }
                  })}
                  min="0"
                  max="100"
                />
              </div>
            </div>

            <div className="mt-6">
              <label className="flex items-center space-x-2">
                <input
                  type="checkbox"
                  className="rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                  checked={strategyData.sellStrategy.trailingStopEnabled}
                  onChange={(e) => setStrategyData({
                    ...strategyData,
                    sellStrategy: {
                      ...strategyData.sellStrategy,
                      trailingStopEnabled: e.target.checked
                    }
                  })}
                />
                <span className="text-sm text-gray-700">Enable Trailing Stop</span>
              </label>
              
              {strategyData.sellStrategy.trailingStopEnabled && (
                <div className="mt-3">
                  <label className="label">Trailing Stop Percentage (%)</label>
                  <input
                    type="number"
                    step="0.1"
                    className="input"
                    value={strategyData.sellStrategy.trailingStopPercentage}
                    onChange={(e) => setStrategyData({
                      ...strategyData,
                      sellStrategy: {
                        ...strategyData.sellStrategy,
                        trailingStopPercentage: parseFloat(e.target.value) || 5
                      }
                    })}
                    min="0"
                    max="100"
                  />
                </div>
              )}
            </div>
          </div>

          <div className="flex justify-end">
            <button
              onClick={handleSaveStrategy}
              disabled={isSaving}
              className="btn btn-primary"
            >
              {isSaving ? 'Saving...' : 'Save Strategy'}
            </button>
          </div>
        </div>
      )}

      {/* Notifications Tab */}
      {activeTab === 'notifications' && (
        <div className="card p-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Notification Preferences</h2>
          <div className="space-y-4">
            <label className="flex items-center justify-between">
              <div>
                <p className="font-medium text-gray-900">Email Notifications</p>
                <p className="text-sm text-gray-500">Receive recommendations via email</p>
              </div>
              <input
                type="checkbox"
                className="rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                defaultChecked={true}
              />
            </label>
            
            <label className="flex items-center justify-between">
              <div>
                <p className="font-medium text-gray-900">Price Alerts</p>
                <p className="text-sm text-gray-500">Get notified when stocks hit target prices</p>
              </div>
              <input
                type="checkbox"
                className="rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                defaultChecked={true}
              />
            </label>
            
            <label className="flex items-center justify-between">
              <div>
                <p className="font-medium text-gray-900">Daily Summary</p>
                <p className="text-sm text-gray-500">Daily portfolio performance summary</p>
              </div>
              <input
                type="checkbox"
                className="rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                defaultChecked={false}
              />
            </label>
          </div>
          
          <div className="mt-6 flex justify-end">
            <button className="btn btn-primary">
              Save Preferences
            </button>
          </div>
        </div>
      )}
    </div>
  );
};
