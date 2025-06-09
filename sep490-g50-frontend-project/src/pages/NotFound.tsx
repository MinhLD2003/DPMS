import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Button } from 'antd';

const NotFound: React.FC = () => {
  const navigate = useNavigate();

  const handleGoBack = () => {
    navigate(-1); // Navigate to the previous page
  };

  return (
    <section style={{ textAlign: 'center', padding: '50px' }}>
      <h1>404 - Page Not Found</h1>
      <h5 className='mt-12 mb-12'>Oops! The page you are looking for does not exist.</h5>
      <div style={{ margin: '20px 0' }}>
        <Button type="primary" onClick={handleGoBack} style={{ marginRight: '10px' }}>
          Go Back
        </Button>
        <Link to="/">
          <Button type="default">Go Back Home</Button>
        </Link>
      </div>
    </section>
  );
};

export default NotFound;