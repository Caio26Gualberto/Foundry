import React from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { useAuth } from "../contexts/Auth";
import { needsTenantSelection } from "../utils/authHelpers";
import { ProtectedRoute } from "./ProtectedRoute";
import { DashboardLayout } from "./Layout/DashboardLayout";
import { Login } from "../pages/Login";
import { TenantSelection } from "../pages/TenantSelection";
import { Dashboard } from "../pages/Dashboard";
import { ROUTES } from "../utils/constants";
import TenantSettings from "../pages/TenantSettings";
import Users from "../pages/Users";
import AcceptInvitation from "../pages/AcceptInvitation";

export const AppRouter: React.FC = () => {
  const { user, token, isLoading } = useAuth();


  // Show loading while auth is initializing
  if (isLoading) {
    return <div>Loading...</div>;
  }

  // Determine the default redirect based on user state
  const getDefaultRedirect = () => {
    if (!user || !token) {
      return ROUTES.LOGIN;
    }

    if (needsTenantSelection(user)) {
      return ROUTES.TENANT_SELECTION;
    }

    return ROUTES.DASHBOARD;
  };

  return (
    <BrowserRouter>
      <Routes>
          {/* Public Routes */}
          <Route
            path={ROUTES.LOGIN}
            element={
              user && token ? (
                <Navigate to={getDefaultRedirect()} replace />
              ) : (
                <Login />
              )
            }
          />

          {/* Accept Invitation Route */}
          <Route
            path="/security/acceptInvitation"
            element={<AcceptInvitation />}
          />

          {/* Tenant Selection Route */}
          <Route
            path={ROUTES.TENANT_SELECTION}
            element={
              <ProtectedRoute requireAuth={true}>
                <TenantSelection />
              </ProtectedRoute>
            }
          />

          {/* Protected Dashboard Routes */}
          <Route
            path="/dashboard/*"
            element={
              <ProtectedRoute requireAuth={true}>
                <DashboardLayout>
                  <Routes>
                    <Route index element={<Dashboard />} />
                    <Route path="tenant-settings" element={<TenantSettings />} />
                    <Route path="analytics" element={<Dashboard />} />
                    <Route path="users" element={<Users />} />
                    <Route path="reports" element={<Dashboard />} />
                    <Route path="security" element={<Dashboard />} />
                    <Route path="settings" element={<Dashboard />} />
                    <Route
                      path="*"
                      element={<Navigate to="/dashboard" replace />}
                    />
                  </Routes>
                </DashboardLayout>
              </ProtectedRoute>
            }
          />

          {/* Root redirect */}
          <Route
            path="/"
            element={<Navigate to={getDefaultRedirect()} replace />}
          />

          {/* Catch all - redirect to appropriate page */}
          <Route
            path="*"
            element={<Navigate to={getDefaultRedirect()} replace />}
          />
        </Routes>
    </BrowserRouter>
  );
};
