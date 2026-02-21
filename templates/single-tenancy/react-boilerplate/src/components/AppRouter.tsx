import React from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { useAuth } from "../contexts/Auth";
import { ProtectedRoute } from "./ProtectedRoute";
import { DashboardLayout } from "./Layout/DashboardLayout";
import { Login } from "../pages/Login";
import { Dashboard } from "../pages/Dashboard";
import { ROUTES } from "../utils/constants";
import Users from "../pages/Users";
import ChangePassword from "../pages/ChangePassword";

export const AppRouter: React.FC = () => {
  const { user, token, isLoading } = useAuth();

  if (isLoading) {
    return <div>Loading...</div>;
  }

  const getDefaultRedirect = () => {
    if (!user || !token) {
      return ROUTES.LOGIN;
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

          {/* Change Password Route */}
          <Route
            path={ROUTES.CHANGE_PASSWORD}
            element={<ChangePassword />}
          />

          {/* Protected Dashboard Routes */}
          <Route
            path="/dashboard/*"
            element={
              <ProtectedRoute requireAuth={true}>
                <DashboardLayout>
                  <Routes>
                    <Route index element={<Dashboard />} />
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

          {/* Catch all */}
          <Route
            path="*"
            element={<Navigate to={getDefaultRedirect()} replace />}
          />
        </Routes>
    </BrowserRouter>
  );
};
