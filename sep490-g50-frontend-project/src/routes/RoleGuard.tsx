// /guards/RoleGuard.tsx
import React, { useContext } from "react";
import { Navigate, Outlet } from "react-router-dom";
import { AuthContext } from "../contexts/AuthContext";

interface RoleGuardProps {
  requiredRole: string;
}

const RoleGuard: React.FC<RoleGuardProps> = ({ requiredRole }) => {
    const authContext = useContext(AuthContext);
    const user = authContext?.user;

    // If user is not logged in, redirect to login page
    if (!user) {
        return <Navigate to="/login" />;
    }

    // If user does not have the required role, redirect to dashboard
    if (user.role !== requiredRole) {
        return <Navigate to="/dashboard" />;
    }


    // If user has the required role, render the child routes (protected content)
    return <Outlet />;
};

export default RoleGuard;
