import { useContext } from "react";
import { Navigate } from "react-router-dom";
import { AuthContext } from "../contexts/AuthContext";

const AuthGuard = ({ children }: { children: JSX.Element }) => {
  const authContext = useContext(AuthContext);
  
  // If no auth context is available
  if (!authContext) {
    return <Navigate to="/login" />;
  }
  
  // If user is not logged in
  if (!authContext.user) {
    return <Navigate to="/login" />;
  }
  
  // Check token validity on each navigation
  if (!authContext.isTokenValid()) {
    console.log("invalid is the token")
    authContext.logout();
    return <Navigate to="/login" />;
  }
  return children;
};

export default AuthGuard;