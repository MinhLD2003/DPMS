import { useNavigate } from 'react-router-dom';
import {
  ArrowLeftOutlined,
} from '@ant-design/icons';
import { Button } from 'antd';

function MuiBackButton() {
  const navigate = useNavigate();

  const handleBack = () => {
    if (window.history.length > 1) {
      navigate(-1);
    } else {
      navigate('/'); // Fallback
    }
  };

  return (
    <Button className='m-4'
      type="link"
      icon={<ArrowLeftOutlined />}
      onClick={handleBack}
      style={{ borderRadius: '4px' }}
    />
  );
}

export default MuiBackButton;
