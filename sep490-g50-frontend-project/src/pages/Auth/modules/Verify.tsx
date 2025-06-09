import React, { useState, useEffect } from "react";
import { Button, Card, Form, message, Result, Spin, Typography } from "antd";
import { useLocation, useNavigate } from "react-router-dom";
import { authService } from "../apis/AuthAPIs";

const Verify: React.FC = () => {
    const location = useLocation();
    const query = new URLSearchParams(location.search);
    const token = query.get("token");
    const [loading, setLoading] = useState(false);
    const [isValidToken, setIsValidToken] = useState(false);
    const navigate = useNavigate();
    const { Text, Link } = Typography;

    useEffect(() => {
        const verifyToken = async () => {
            if (!token) {
                message.error("Invalid token.");
                return;
            }
            setLoading(true);
            try {
                console.log("Sending request to verify token:", token);
                const response = await authService.verify(token);

                if (response) {
                    message.success("You have been verified.");
                    setIsValidToken(true);
                } else {
                    message.error("Something went wrong. Please try again.");
                    setIsValidToken(false);
                }
            } catch (error) {
                message.error("Network error. Please try again.");
                console.error("Error:", error);
                setIsValidToken(false);
            } finally {
                setLoading(false);
            }
        };

        verifyToken();
    }, [token]);

    const handleReset = () => {
        console.log('token:', token);
        navigate(`/reset-password?token=${token}`);
    };

    return (
        <div className="forgot-password-container">
            <Card title="Verify Token" className="forgot-password-card">
                {loading ? (
                    <Spin size="large" />
                ) : isValidToken ? (
                    <Result
                        status="success"
                        title="Verification Successful"
                        subTitle="Your token has been successfully verified. You can now reset your password."
                        extra={[
                            <Button type="primary" key="reset" onClick={handleReset}>
                                Reset Password
                            </Button>,
                        ]}
                    />
                ) : (
                    <Result
                        status="error"
                        title="Verification Failed"
                        subTitle="The token is invalid or has expired. Please try again."
                    />
                )}
                <Form.Item>
                    <div>
                        <Text type="secondary">Remember your password?</Text> <Link href="/login"> Log in</Link>
                    </div>
                </Form.Item>
            </Card>
        </div>
    );
};

export default Verify;