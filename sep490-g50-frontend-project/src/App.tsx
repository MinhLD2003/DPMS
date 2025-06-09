import { RouterProvider } from "react-router-dom";
import { AuthProvider } from "./contexts/AuthContext";
import router from "./routes/AppRoutes";
import { ConsentModalProvider } from "./contexts/ConsentModalContext";

const App = () => {
  return (
    <AuthProvider>
      <ConsentModalProvider>
        <RouterProvider router={router} />
      </ConsentModalProvider>

    </AuthProvider>
  );
};

export default App;
