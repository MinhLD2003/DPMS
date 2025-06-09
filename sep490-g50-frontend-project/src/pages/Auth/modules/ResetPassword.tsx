import React, { useState } from "react";
import { Button, Form, Input, message, Card } from "antd";
import { LockOutlined } from "@ant-design/icons";
import { useLocation, useNavigate } from "react-router-dom";
import { authService } from "../apis/AuthAPIs";
import { ResetPasswordPayload, resetPasswordSchema } from "../models/ResetPasswordPayload";
import { useFormValidation } from "../../../hooks/useFormValidation";


const ResetPassword: React.FC = () => {
  const location = useLocation();
  const query = new URLSearchParams(location.search);
  const navigate = useNavigate();

  const jwttoken = query.get("token") || "";
  const [loading, setLoading] = useState(false);
  const [resetSuccess, setResetSuccess] = useState(false);
  const [form] = Form.useForm();
  const { errors, validateField, validateForm } =
    useFormValidation<ResetPasswordPayload>(form, resetPasswordSchema, { validateOnChange: false });

  const handleSubmit = async () => {

    setLoading(true);
    try {
      if (validateForm()) {
        const values = form.getFieldsValue();
        const response = await authService.setPassword(jwttoken, values.new_password);
        if (response.success) {
          message.success("Password reset successfully.");
          setResetSuccess(true);
        } else {
          message.error("Something went wrong. Please try again.");
        }
      }
    } catch (error) {
      message.error("Network error. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  const handleBackToLogin = () => {
    navigate("/login");
  };

  return (
    <div className="forgot-password-container">
      <Card title="Reset Password" className="forgot-password-card">
        <Form
          form={form}
          name="reset_form"
          onFinish={handleSubmit}
          layout="vertical"
          requiredMark="optional"
        >
          <Form.Item name="new_password" validateStatus={errors.new_password ? "error" : ""} help={errors.new_password}>
            <Input.Password
              prefix={<LockOutlined />}
              placeholder="New Password"
              onBlur={() => validateField("new_password")}
            />
          </Form.Item>

          <Form.Item name="confirm_password" validateStatus={errors.confirm_password ? "error" : ""} help={errors.confirm_password}>
            <Input.Password 
            prefix={<LockOutlined />}
             placeholder="Confirm Password"
             onBlur={() => validateField("confirm_password")}
             />
          </Form.Item>

          <Form.Item>
            <Button block type="primary" htmlType="submit" loading={loading} disabled={resetSuccess}>
              {loading ? "Sending..." : "Reset Password"}
            </Button>

            {resetSuccess && (
              <Button block type="primary" onClick={handleBackToLogin}>
                Back to Login
              </Button>
            )}
          </Form.Item>
        </Form>
      </Card>
    </div>
  );
};

export default ResetPassword;
