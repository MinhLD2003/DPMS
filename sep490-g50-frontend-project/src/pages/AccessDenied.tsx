import React from "react";
import { Result, Button } from "antd";
import { useNavigate } from "react-router-dom";

const Custom403: React.FC = () => {
  const navigate = useNavigate();

  const handleBack = () => {
    navigate("/"); // Redirect to homepage or wherever you like
  };

  return (
    <div style={{ height: "100vh", display: "flex", alignItems: "center", justifyContent: "center" }}>
      <Result
        status="403"
        title="403"
        subTitle="Sorry, you are not authorized to access this page."
        extra={
          <Button type="primary" onClick={handleBack}>
            Back Home
          </Button>
        }
      />
    </div>
  );
};

export default Custom403;
