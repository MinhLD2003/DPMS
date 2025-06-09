import { jwtDecode } from "jwt-decode";
import { createContext, useState, useEffect, useMemo, ReactNode } from "react";

interface JwtPayload {
  sub?: string;
  exp?: number;
  role?: string;
}

interface UserType {
  token: string;
  sub?: string;
  role?: string;
  exp?: number;
}

interface AuthContextType {
  user: UserType | null;
  login: (jwtToken: string) => void;
  logout: () => void;
  consentChecked: boolean;
  setConsentChecked: (checked: boolean) => void;
  isTokenValid: () => boolean;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<UserType | null>(null);
  const [loading, setLoading] = useState(true);
  const [consentChecked, setConsentChecked] = useState(false);

  // Function to decode JWT (without verification, just base64 decoding)
  const decodeToken = (token: string): JwtPayload => {
    try {
      return jwtDecode<JwtPayload>(token);
    } catch (error) {
      console.error("Failed to decode token:", error);
      return {};
    }
  };

  // Function to check if token is expired
  const isTokenExpired = (exp?: number): boolean => {
    if (!exp) return true;
    // Convert exp to milliseconds and compare with current time
    return Date.now() >= exp * 1000;
  };

  // Function to check if current token is valid
  const isTokenValid = (): boolean => {
    if (!user || !user.exp) return false;
    return !isTokenExpired(user.exp);
  };

  const login = (jwtToken: string) => {
    localStorage.setItem("jwt", jwtToken);
    const decoded = decodeToken(jwtToken);
    console.log(decoded);
    const { sub, exp, role } = decoded;
    setUser({ token: jwtToken, sub, exp, role });
    setConsentChecked(false); 
  };

  const logout = () => {
    localStorage.removeItem("jwt");
    setConsentChecked(false); // Reset at logout
    setUser(null);
  };

  // On mount: Check stored token
  useEffect(() => {
    const token = localStorage.getItem("jwt");
    
    if (token) {
      const decoded = decodeToken(token);
      setUser({ token, ...decoded });
    }
    
    setLoading(false);
  }, []);

  const value = useMemo(() => ({
    user,
    login,
    logout,
    consentChecked,
    setConsentChecked,
    isTokenValid
  }), [user, consentChecked]);

  if (loading) {
    return <div>Loading...</div>;
  }

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};