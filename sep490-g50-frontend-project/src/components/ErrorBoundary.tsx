// components/ErrorBoundary.tsx
import React, { ErrorInfo, ReactNode } from 'react';
import { Button, Result } from 'antd';
import { useNavigate } from 'react-router-dom';

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
  error?: Error;
  errorInfo?: ErrorInfo;
}

class ErrorBoundary extends React.Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    this.setState({
      error: error,
      errorInfo: errorInfo
    });
    console.error('Error caught in ErrorBoundary:', error, errorInfo);
  }

  handleReload = () => {
    window.location.reload();
  };

  handleGoHome = () => {
    window.location.href = '/';
  };

  render() {
    if (this.state.hasError) {
      return (
        <div style={{ 
          height: '100vh',
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          backgroundColor: '#f5f5f5'
        }}>
          <Result
            status="error"
            title="Something went wrong"
            subTitle={this.state.error?.message || 'An unexpected error occurred.'}
            extra={[
              <Button type="primary" key="reload" onClick={this.handleReload}>
                Try Again
              </Button>,
              <Button key="home" onClick={this.handleGoHome}>
                Go Home
              </Button>,
            ]}
          />
          {process.env.NODE_ENV === 'development' && this.state.errorInfo && (
            <details style={{ 
              whiteSpace: 'pre-wrap',
              padding: '20px',
              backgroundColor: '#fff',
              borderRadius: '8px',
              marginTop: '20px'
            }}>
              <summary>Error Details</summary>
              <p>{this.state.error?.toString()}</p>
              <p>Component Stack:</p>
              <p>{this.state.errorInfo.componentStack}</p>
            </details>
          )}
        </div>
      );
    }

    return this.props.children;
  }
}

export default ErrorBoundary;
