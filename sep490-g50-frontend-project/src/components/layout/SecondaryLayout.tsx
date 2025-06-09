import React from "react";

interface SecondaryLayoutProps {
  title?: string;
  description?: string;
  children: React.ReactNode;
}

const SecondaryLayout: React.FC<SecondaryLayoutProps> = ({children }) => {
  return (
    <div
      style={{
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        height: "100vh",
        width: "100vw",
        backgroundImage: `url('/fpt1.jpg')`,
        backgroundSize: "cover",
        backgroundPosition: "center",
        padding: "20px",
      }}
    >
        {children}
    </div>
  );
};
export default SecondaryLayout;
