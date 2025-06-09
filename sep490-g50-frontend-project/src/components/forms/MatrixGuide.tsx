import React from 'react';
import { Typography, Table } from 'antd';
import { useTranslation } from 'react-i18next';

const { Text } = Typography;

const MatrixGuide: React.FC = () => {
        const { t } = useTranslation();
    const probabilityData = [
        { probability: '1 - Rare', color: '#FFF9C4' },
        { probability: '2 - Unlikely', color: '#FFEB3B' },
        { probability: '3 - Possible', color: '#FBC02D' },
        { probability: '4 - Likely', color: '#F9A825' },
        { probability: '5 - Almost Certain', color: '#F57F17' },
    ];

    const impactData = [
        { impact: '1 - Insignificant', color: '#FFFFFF' },
        { impact: '2 - Minor', color: '#F5F5F5' },
        { impact: '4 - Moderate', color: '#E8E8E8' },
        { impact: '8 - Major', color: '#DCDCDC' },
        { impact: '16 - Severe', color: '#C0C0C0' },
    ];

    const levelScoreData = [
        { score: '1-2 Negligible', color: '#27ccff' },
        { score: '3-8 Low', color: '#5edf6f' },
        { score: '10-16 Medium', color: '#fadb14' },
        { score: '20-32 High', color: '#fa8c16' },
        { score: '40-80 Extreme', color: '#f5222d' },
    ];

    const guideColumns = [
        {
            title: t('probability'),
            dataIndex: 'probability',
            key: 'probability',
            width: '33%',
            align: 'center' as 'center',
        },
        {
            title: t('impact'),
            dataIndex: 'impact',
            key: 'impact',
            width: '33%',
            align: 'center' as 'center',
        },
        {
            title: t('priorityScore'),
            dataIndex: 'score',
            key: 'score',
            width: '33%',
            align: 'center' as 'center',
        },
    ];

    const dataSource = levelScoreData.map((item, index) => ({
        key: index.toString(),
        probability: probabilityData[index]?.probability || '-',
        impact: impactData[index]?.impact || '-',
        score: item.score,
        probabilityColor: probabilityData[index]?.color,
        impactColor: impactData[index]?.color,
        scoreColor: item.color,
    }));

    return (
        <section className='my-8'>
            <Text strong>Risk Matrix Guide:</Text>
            <Table
                dataSource={dataSource}
                columns={guideColumns}
                pagination={false}
                size="small"
                bordered
                rowKey="key"
                style={{ marginBottom: '16px',maxWidth: '50%' }}
                rowClassName={(record) => `matrix-row-${record.key}`}
            />
            <style>
                {dataSource.map((item, index) => `
                    .matrix-row-${index} td:nth-child(1),
                    .matrix-row-${index} td:nth-child(2),
                    .matrix-row-${index} td:nth-child(3) {
                        text-align: center !important;
                    }
                    .matrix-row-${index} td:nth-child(1) {
                        background-color: ${item.probabilityColor} !important;
                    }
                    .matrix-row-${index} td:nth-child(2) {
                        background-color: ${item.impactColor} !important;
                    }
                    .matrix-row-${index} td:nth-child(3) {
                        background-color: ${item.scoreColor} !important;
                    }
                `).join('\n')}
            </style>
        </section>
    );
};

export default MatrixGuide;
