import React from 'react';
import { Navigate } from 'react-router-dom';
import { hasFeaturePermission, hasMatchingPermission } from '../utils/jwtDecodeUtils';

interface FeatureRouteGuardProps {
  requiredFeature?: string;
  requiredPattern?: string;
  children: React.ReactNode;
  redirectPath?: string;
}

/**
 * A component that guards routes based on user feature permissions
 * @param requiredFeature - Exact feature permission required (e.g. "/api/Group_GET")
 * @param requiredPattern - Pattern to match in permissions (e.g. "/api/Group_")
 * @param children - Components to render if permission check passes
 * @param redirectPath - Path to redirect to if permission check fails (default: "/dashboard/access-denied")
 */
const FeatureRouteGuard: React.FC<FeatureRouteGuardProps> = ({
  requiredFeature,
  requiredPattern,
  children,
  redirectPath = '/dashboard/access-denied'
}) => {
  // Check if user has the required permission
  const hasPermission = 
    (requiredFeature && hasFeaturePermission(requiredFeature)) ||
    (requiredPattern && hasMatchingPermission(requiredPattern));

  if (!hasPermission) {
    console.log(`Access denied: Missing required permission ${requiredFeature || requiredPattern}`);
    return <Navigate to={redirectPath} replace />;
  }

  return <>{children}</>;
};

export default FeatureRouteGuard;