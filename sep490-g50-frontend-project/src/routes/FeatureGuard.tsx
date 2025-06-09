import React from "react";
import { getUserFeatures } from "../utils/jwtDecodeUtils";

interface FeatureGuardProps {
  requiredFeature: string;
  children: React.ReactNode;
}

const FeatureGuard: React.FC<FeatureGuardProps> = ({ requiredFeature, children }) => {
  const userFeatures = getUserFeatures();
  return userFeatures.includes(requiredFeature) ? <>{children}</> : null;
};

export default FeatureGuard;
