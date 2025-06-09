// /components/LoginGuard.tsx
import React, { useContext } from "react";
import { Navigate } from "react-router-dom";
import { AuthContext } from "../contexts/AuthContext";  // Assuming you have this context to manage auth state

interface LoginGuardProps {
  children: React.ReactNode;
}

const LoginGuard: React.FC<LoginGuardProps> = ({ children }) => {
  const authContext = useContext(AuthContext);// Check if the user is logged in
  const user = authContext?.user;
  // If the user is logged in, redirect to the Dashboard
  if (user) {
    return <Navigate to="/dashboard" />;
  }

  // If the user is not logged in, allow access to Login page
  return <>{children}</>;
};

export default LoginGuard;
