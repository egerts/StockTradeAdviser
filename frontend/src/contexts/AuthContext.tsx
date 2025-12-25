import React, { createContext, useContext, useEffect, useState } from 'react';
import { useMsal } from '@azure/msal-react';
import { loginRequest } from '../config/auth';
import { User } from '../types/models';

interface AuthContextType {
  user: User | null;
  isLoading: boolean;
  login: () => void;
  logout: () => void;
  updateUser: (user: User) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

interface AuthProviderProps {
  children: React.ReactNode;
}

 export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const { instance, accounts, inProgress } = useMsal();
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const initializeAuth = async () => {
      if (inProgress === 'none') {
        try {
          const account = instance.getActiveAccount() ?? accounts[0];

          if (!account) {
            setUser(null);
            setIsLoading(false);
            return;
          }

          // Call backend API to get user profile
          const userProfile = await fetch('http://localhost:53133/api/auth/profile');

          if (userProfile.ok) {
            const userData = await userProfile.json();
            setUser(userData);
            setIsLoading(false);
          } else {
            // Fallback to MSAL account data if API fails
            const userData: User = {
              id: account.homeAccountId || account.localAccountId,
              azureAdObjectId: account.localAccountId,
              email: account.username || '',
              displayName: account.name || account.username || 'User',
              createdAt: new Date().toISOString(),
              updatedAt: new Date().toISOString(),
              portfolios: [],
              tradingStrategy: {
                riskTolerance: 'Medium',
                investmentHorizon: 'MediumTerm',
                maxPortfolioSize: 20,
                preferredSectors: [],
                sellStrategy: {
                  takeProfitPercentage: 20,
                  stopLossPercentage: 10,
                  trailingStopEnabled: false,
                  trailingStopPercentage: 5
                }
              },
              notificationPreferences: {
                emailNotifications: true,
                priceAlerts: true,
                dailySummary: false
              }
            };
            setUser(userData);
            setIsLoading(false);
          }
        } catch (error) {
          console.error('Error initializing auth:', error);
          setIsLoading(false);
        }
      }
    };

    initializeAuth();
  }, [instance, accounts, inProgress]);

  const login = () => {
    instance.loginRedirect(loginRequest);
  };

  const logout = () => {
    instance.logoutRedirect();
  };

  const updateUser = (updatedUser: User) => {
    setUser(updatedUser);
  };

  const value = {
    user,
    isLoading,
    login,
    logout,
    updateUser,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
