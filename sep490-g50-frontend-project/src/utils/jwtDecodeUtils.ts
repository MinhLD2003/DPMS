import { jwtDecode } from "jwt-decode";

interface DecodedToken {
  email?: string;
  feature?: string[];
}

export const getUserFeatures = (): string[] => {
  const token = localStorage.getItem("jwt"); // Adjust if using sessionStorage or cookies
  if (!token) return [];

  try {
    const decoded: DecodedToken = jwtDecode(token);
    return decoded.feature || [];
  } catch (error) {
    console.error("Invalid JWT", error);
    return [];
  }
};
export const hasFeaturePermission = (requiredFeature: string): boolean => {
  const userFeatures = getUserFeatures();
  return userFeatures.includes(requiredFeature);
};

/**
 * Checks if the user has any permission that matches a specific pattern
 * @param pattern - The pattern to match against permissions
 * @returns Boolean indicating if any matching permission exists
 */
export const hasMatchingPermission = (pattern: string): boolean => {
  const userFeatures = getUserFeatures();
  return userFeatures.some(feature => feature.includes(pattern));
};
export const getEmailFromToken = (): string => {
  try {
      const token = localStorage.getItem("jwt");
      if (!token) return '';
      
      const decoded = jwtDecode<DecodedToken>(token);
      return decoded.email || '';
  } catch (error) {
      console.error('Error decoding JWT:', error);
      return '';
  }
};
