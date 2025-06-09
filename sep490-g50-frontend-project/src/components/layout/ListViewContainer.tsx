import React from 'react';
import { Card, Typography } from 'antd';
import { useMediaQuery } from 'react-responsive';

interface SectionContainerProps {
  children: React.ReactNode;
  title?: string;
}

const ListViewContainer: React.FC<SectionContainerProps> = ({
  children,
  title,
}) => {
  const isMobile = useMediaQuery({ maxWidth: 768 });
  const isTablet = useMediaQuery({ minWidth: 769, maxWidth: 1024 });
  
  return (
    <section className="overflow-x-auto">
        <Card 
          title={title}
          className="shadow-lg"
          style={{
            width: '100%',
            minWidth: isMobile ? '100%' : '768px',
            //minHeight: isMobile ? '100%' : '1080px',
            overflowX: 'auto'
          }}
          bodyStyle={{
            padding: isMobile ? '12px 8px' : isTablet ? '16px 12px' : '24px 20px',
          }}
        >
          {children}
        </Card>
    </section>
  );
};

export default ListViewContainer;