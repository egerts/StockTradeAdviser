import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { MsalProvider } from '@azure/msal-react';
import { EventType, PublicClientApplication, type AuthenticationResult } from '@azure/msal-browser';
import { msalConfig } from './config/auth';
import { AuthProvider } from './contexts/AuthContext';
import { Layout } from './components/Layout';
import { Login } from './pages/Login';
import { Dashboard } from './pages/Dashboard';
import { Portfolios } from './pages/Portfolios';
import { Recommendations } from './pages/Recommendations';
import { Settings } from './pages/Settings';
import { ProtectedRoute } from './components/ProtectedRoute';

const msalInstance = new PublicClientApplication(msalConfig);

const existingAccounts = msalInstance.getAllAccounts();
if (existingAccounts.length > 0) {
  msalInstance.setActiveAccount(existingAccounts[0]);
}

msalInstance.addEventCallback((event) => {
  if (event.eventType === EventType.LOGIN_SUCCESS && event.payload) {
    const payload = event.payload as AuthenticationResult;
    msalInstance.setActiveAccount(payload.account);
  }
});

function App() {
  return (
    <MsalProvider instance={msalInstance}>
      <AuthProvider>
        <Router>
          <Routes>
            <Route path="/login" element={<Login />} />
            <Route path="/" element={
              <ProtectedRoute>
                <Layout />
              </ProtectedRoute>
            }>
              <Route index element={<Navigate to="/dashboard" replace />} />
              <Route path="dashboard" element={<Dashboard />} />
              <Route path="portfolios" element={<Portfolios />} />
              <Route path="recommendations" element={<Recommendations />} />
              <Route path="settings" element={<Settings />} />
            </Route>
          </Routes>
        </Router>
      </AuthProvider>
    </MsalProvider>
  );
}

export default App;
