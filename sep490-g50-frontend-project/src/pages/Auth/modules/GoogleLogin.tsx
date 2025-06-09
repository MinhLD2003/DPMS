import { useContext } from "react";
import { GoogleOAuthProvider, GoogleLogin, CredentialResponse } from "@react-oauth/google";
import { useNavigate } from "react-router-dom";
import { AuthContext } from "../../../contexts/AuthContext";
import AxiosClient from "../../../configs/axiosConfig";
import { message } from "antd";

const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID as string;

const LoginWithGoogle = () => {
  const authContext = useContext(AuthContext);
  const navigate = useNavigate();

  if (!authContext) {
    return null;
  }

  const { login } = authContext;

  const handleSuccess = async (response: CredentialResponse) => {
    if (!response.credential) return;

    try {
      const res = await AxiosClient.post("/Auth/google", {
        token: response.credential,
      });
      if (res) {
        login(res.data.token);  // Save JWT in context
        navigate("/dashboard");
      }
    }
    catch (error) {
      message.info("Your email is not registered with us. Please contact administrators for access.");
      return;
    };
  };

return (
  <GoogleOAuthProvider clientId={clientId}>
    <GoogleLogin onSuccess={handleSuccess} onError={() => console.log("Login Failed")} />
  </GoogleOAuthProvider>
);
};

export default LoginWithGoogle;
