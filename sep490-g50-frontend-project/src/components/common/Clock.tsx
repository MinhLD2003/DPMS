import React, { useState, useEffect } from 'react';
import { Typography } from 'antd';

const { Text } = Typography;

const Clock: React.FC = () => {
  const [time, setTime] = useState(new Date());

  useEffect(() => {
    const timer = setInterval(() => {
      setTime(new Date());
    }, 1000);

    return () => clearInterval(timer);
  }, []);

  return (
    <Text className="text-lg">
      {time.toLocaleTimeString()}
    </Text>
  );
};

export default Clock;