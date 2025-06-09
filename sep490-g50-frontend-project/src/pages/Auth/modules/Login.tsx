import { Button, Form, Input, message } from "antd";
import { useContext, useState } from "react";
import { useNavigate } from "react-router-dom";
import LoginWithGoogle from "./GoogleLogin";
import { useFormValidation } from "../../../hooks/useFormValidation";
import { LoginPayload, loginSchema } from "../models/LoginPayload";
import { AuthContext } from "../../../contexts/AuthContext";
import { authService } from "../apis/AuthAPIs";
import SecondaryLayout from "../../../components/layout/SecondaryLayout";



export default function Login() {
  const [form] = Form.useForm<LoginPayload>();
  const [loading, setLoading] = useState<boolean>(false);
  const navigate = useNavigate();

  const { errors, validateForm, validateField } = useFormValidation<LoginPayload>(form, loginSchema, { validateOnChange: false });

  const authContext = useContext(AuthContext);
  if (!authContext) { return null; }
  const { login } = authContext;


  const handleSubmit = async () => {
    setLoading(true);
    try {
      if (validateForm()) {
        const values = form.getFieldsValue();
        const result = await authService.login(values);
        if (result.token) {
          login(result.token);  // Save JWT in context
          navigate("/dashboard");
          form.resetFields();
        } else {
          message.error('Failed to login.');
        }
      }
    } catch (errorInfo) {
      message.error('Invalid username or password.');
      console.log("Failed:", errorInfo);
    }
    setLoading(false);
  };
  const handleForgotPassword = async () => {
    navigate("/forgot-password");
  };

  return (
    <SecondaryLayout>
      <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2">
        <div className="login-box">
          <h2 className="text-center mb-4 text-black font-bold">Login to DPMS</h2>
          <div className="mb-12">
            <img
              src="/dpms.png"
              alt="logo"
              style={{ width: 60, height: 60, borderRadius: 10, }}
            />
          </div>
          <Form form={form}
            onFinish={handleSubmit}
          //onValuesChange={onValuesChange} // Attach here!
          >
            <Form.Item name="email"
              validateStatus={errors.email ? "error" : ""}
              help={errors.email}>
              <Input placeholder="Email" 
              onBlur={() => validateField('email')}
              />
            </Form.Item>
            <Form.Item name="password"
              validateStatus={errors.password ? "error" : ""}
              help={errors.password}>
              <Input.Password placeholder="Password"
              onBlur={() => validateField('password')}
              />
            </Form.Item>

            <Form.Item className="flex justify-center">
              <Button type="primary"
                htmlType="submit"
                loading={loading}>
                Login
              </Button>
            </Form.Item>

          </Form>
          <div className="flex justify-center text-blue-600 hover:underline cursor-pointer">
            <h6 onClick={handleForgotPassword}>Forgot Password</h6>
          </div>
          <div className="flex justify-center mt-4">
            <LoginWithGoogle />
          </div>

        </div>
      </div>
    </SecondaryLayout>

  );
}

