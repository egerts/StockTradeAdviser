import React from 'react';
import { useMsal } from '@azure/msal-react';
import { Navigate } from 'react-router-dom';
import { loginRequest } from '../config/auth';
import { useAuth } from '../contexts/AuthContext';

export const Login: React.FC = () => {
  const { instance } = useMsal();
  const { user, isLoading } = useAuth();

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading...</p>
        </div>
      </div>
    );
  }

  if (user) {
    return <Navigate to="/dashboard" replace />;
  }

  const handleLogin = () => {
    instance.loginRedirect(loginRequest);
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full space-y-8">
        <div>
          <div className="mx-auto h-12 w-12 flex items-center justify-center">
            <div className="w-12 h-12 bg-primary-600 rounded-lg flex items-center justify-center">
              <span className="text-white font-bold text-xl">S</span>
            </div>
          </div>
          <h2 className="mt-6 text-center text-3xl font-extrabold text-gray-900">
            Sign in to StockTradeAdviser
          </h2>
          <p className="mt-2 text-center text-sm text-gray-600">
            Get AI-powered stock recommendations based on your trading strategy
          </p>
        </div>
        <div>
          <button
            onClick={handleLogin}
            className="group relative w-full flex justify-center py-3 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500"
          >
            <span className="absolute left-0 inset-y-0 flex items-center pl-3">
              <svg
                className="h-5 w-5 text-primary-500 group-hover:text-primary-400"
                xmlns="http://www.w3.org/2000/svg"
                viewBox="0 0 20 20"
                fill="currentColor"
                aria-hidden="true"
              >
                <path
                  fillRule="evenodd"
                  d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z"
                  clipRule="evenodd"
                />
              </svg>
            </span>
            Sign in with Microsoft
          </button>
        </div>
        <div className="mt-6">
          <div className="relative">
            <div className="absolute inset-0 flex items-center">
              <div className="w-full border-t border-gray-300" />
            </div>
            <div className="relative flex justify-center text-sm">
              <span className="px-2 bg-gray-50 text-gray-500">Features</span>
            </div>
          </div>
          <div className="mt-6 grid grid-cols-1 gap-3">
            <div className="text-center text-sm text-gray-600">
              <ul className="space-y-2">
                <li className="flex items-center justify-center">
                  <span className="text-green-500 mr-2">✓</span>
                  AI-powered stock recommendations
                </li>
                <li className="flex items-center justify-center">
                  <span className="text-green-500 mr-2">✓</span>
                  Portfolio tracking and management
                </li>
                <li className="flex items-center justify-center">
                  <span className="text-green-500 mr-2">✓</span>
                  Technical and fundamental analysis
                </li>
                <li className="flex items-center justify-center">
                  <span className="text-green-500 mr-2">✓</span>
                  Personalized trading strategies
                </li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
