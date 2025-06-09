import React, { useState } from "react";
import { Button, Form, Input, Typography, message, Card } from "antd";
import { MailOutlined } from "@ant-design/icons";
import { authService } from "../apis/AuthAPIs";

const { Text, Link } = Typography;

const ForgotPassword: React.FC = () => {
  const [loading, setLoading] = useState(false);
  const handleSubmit = async (values: any) => {
    setLoading(true);
    try {
      await authService.forgotPassword(values.email);
      // Always show success message, regardless of whether user exists or not
      message.success("If an account exists with this email, you will receive a password reset link.");
    } catch (error: any) {
      // Only show error for server/network related issues
      if (error.response?.status >= 500) {
        message.error("Server error. Please try again later.");
      } else {
        // Still show success message for 404/400 errors (user not found, invalid email, etc)
        message.success("If an account exists with this email, you will receive a password reset link.");
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 flex justify-center align-center max-h-1/2">
      <Card title="Forgot Password" className="forgot-password-card">
        <Text type="secondary">Enter your email address to receive a password reset link.</Text>
        <Form
          name="reset_form"
          initialValues={{ remember: true }}
          onFinish={handleSubmit}
          layout="vertical"
          requiredMark="optional"
        >
          <Form.Item
            label=""
            name="email"
            rules={[{ type: "email", required: true, message: "Please input your Email!" }]}
          >
            <Input className="mt-8" prefix={<MailOutlined />} placeholder="Email" />
          </Form.Item>

          <Form.Item>
            <Button block type="primary" htmlType="submit" loading={loading}>
              {loading ? "Sending..." : "Send Verification Link"}
            </Button>
            <div className="sm:mt-4 lg:mt-8">
              <Text type="secondary">Remember your password?</Text> <Link href="/login"> Log in</Link>
            </div>
          </Form.Item>
        </Form>
      </Card>
    </div>
  );
};

export default ForgotPassword;
