import React, { useState, useEffect, useCallback } from 'react';
import { Table, Input, Select, Button, message, Card, Space, Typography, Row, Col, Tag, Tooltip, Badge, Progress, Statistic } from 'antd';
import { SearchOutlined, ReloadOutlined, PlusOutlined, FilterOutlined, WarningOutlined, AreaChartOutlined, DeleteOutlined } from '@ant-design/icons';
import AxiosClient from '../../../configs/axiosConfig';
import { useNavigate } from 'react-router-dom';
import { ResponseStrategy, RiskCategory } from '../../../enum/enum';
import { addIndexToData } from '../../../utils/indexHelper';
import AssessRiskModal from './AssessRiskModal';
import { categoryOptions, RiskBeforeAssessModel, strategyOptions } from '../models/RiskModels';
import { getCategoryColor, getRiskColor, getRowColor, getStrategyColor } from '../../../utils/RiskMatrixColorUtils';
import ListViewContainer from '../../../components/layout/ListViewContainer';
import moment from 'moment';
import { SortOrder } from 'antd/es/table/interface';
import RiskDetailsModal from './RiskDetailModal';
import confirmDelete from '../../../components/common/popup-modals/ConfirmDeleteModal';
import { useTranslation } from 'react-i18next';
import FeatureGuard from '../../../routes/FeatureGuard';

const { Title, Text } = Typography;
const { Option } = Select;

const FetchRiskList: React.FC = () => {
    const { t } = useTranslation();
    const [risks, setRisks] = useState<any[]>([]);
    const [loading, setLoading] = useState(false);
    const [currentPage, setCurrentPage] = useState(1);
    const [pageSize, setPageSize] = useState(20);
    const [total, setTotal] = useState(0);
    const [searchKey, setSearchKey] = useState("");
    const [selectedCategory, setSelectedCategory] = useState<number | undefined>();
    const [selectedStrategy, setSelectedStrategy] = useState<number | undefined>();
    const [selectedScore, setSelectedScore] = useState<number | undefined>();
    const [riskSummary, setRiskSummary] = useState({ extreme: 0, high: 0, medium: 0, low: 0, negligible: 0, total: 0 });
    const [sortField, setSortField] = useState<string>();
    const [sortOrder, setSortOrder] = useState<string | null>();
    const navigate = useNavigate();

    

    const buildFilterParams = () => {
        const filters: Record<string, string> = {};
        if (searchKey) filters['riskName'] = searchKey;
        if (selectedCategory !== undefined) filters['category'] = selectedCategory.toString();
        if (selectedStrategy !== undefined) filters['strategy'] = selectedStrategy.toString();
        return { pageNumber: currentPage, pageSize, filters, sortBy: sortField, sortDirection: sortOrder };
    };
    const getFilteredRisks = useCallback(() => {
        if (!selectedScore) return risks;

        return risks.filter((risk: any) => {
            const score = risk.score;
            switch (selectedScore) {
                case 40: // Extreme
                    return score >= 40 && score <= 80;
                case 20: // High
                    return score >= 20 && score <= 32;
                case 10: // Medium
                    return score >= 10 && score <= 16;
                case 3: // Low
                    return score >= 3 && score <= 8;
                case 1: // Negligible
                    return score >= 1 && score <= 2;
            }
        });
    }, [risks, selectedScore]);
    const fetchRisks = useCallback(async () => {
        setLoading(true);
        try {
            const filterParams = buildFilterParams();
            const response = await AxiosClient.get('/Risk', { params: filterParams });
            const indexingRisks = addIndexToData(response.data.data, currentPage, pageSize);
            const indexedRisks = indexingRisks.map((risk: any) => ({
                ...risk,
                score: risk.riskImpact * risk.riskProbability,
                scoreAfterMitigation: risk.riskImpactAfterMitigation * risk.riskProbabilityAfterMitigation,
            }));
            setRisks(indexedRisks);
            setTotal(response.data.totalRecords);

            // Calculate risk summary

            const summary = {
                extreme: indexedRisks.filter((risk: any) => risk.score >= 40).length,
                high: indexedRisks.filter((risk: any) => risk.score >= 20 && risk.score <= 32).length,
                medium: indexedRisks.filter((risk: any) => risk.score >= 10 && risk.score <= 16).length,
                low: indexedRisks.filter((risk: any) => risk.score >= 3 && risk.score <= 8).length,
                negligible: indexedRisks.filter((risk: any) => risk.score <= 2).length,
                total: indexedRisks.length
            };
            setRiskSummary(summary);
        } catch (error: any) {
            message.error(error.message || 'Failed to fetch risks');
        } finally {
            setLoading(false);
        }
    }, [searchKey, selectedCategory, selectedStrategy, selectedScore, currentPage, pageSize, sortField, sortOrder, t]);

    useEffect(() => {
        fetchRisks();
    }, [fetchRisks]);

    const handleTableChange = (pagination: any, filters: any, sorter: any) => {
        setCurrentPage(pagination.current);
        setPageSize(pagination.pageSize);

        if (sorter) {
            setSortField(sorter.field);
            if (sorter.order === 'ascend')
                setSortOrder('asc');
            else
                if (sorter.order === 'descend')
                    setSortOrder('desc');
                else
                    setSortOrder(null);
        }
    };
    const handleDelete = async (id: string) => {
        setLoading(true);
        try {
            await AxiosClient.delete(`/Risk/${id}`);
            setRisks(risks.filter(fic => fic.id !== id));
            message.success("Risk deleted successfully.");
        } catch (error) {
            message.error("Failed to delete risk.");
        }
        setLoading(false);
    };

    const resetFilters = () => {
        setSearchKey("");
        setSelectedCategory(undefined);
        setSelectedStrategy(undefined);
        setSelectedScore(undefined);
        setCurrentPage(1);
        setSortField(undefined);
        setSortOrder(undefined);
    };




    const mapToRiskBeforeAssessModel = (record: RiskBeforeAssessModel) => {
        return {
            id: record.id,
            riskImpact: record.riskImpact,
            riskProbability: record.riskProbability,
        };
    };
    const columns = [
        {
            title: '#',
            dataIndex: 'index',
            key: 'index',
            align: 'center' as 'center',
            render: (_: any, __: any, index: number) => {
                return ((currentPage - 1) * pageSize) + index + 1;
            },
        },
        {
            title: t('riskLevel'),
            dataIndex: 'score',
            key: 'riskLevel',
            sorter: (a: any, b: any) => a.score - b.score,
            defaultSortOrder: 'descend' as SortOrder,
            render: (score: number) => {
                let level = 'Low';
                if (40 <= score && score <= 80) level = 'Extreme';
                else if (20 <= score && score <= 32) level = 'High';
                else if (10 <= score && score <= 16) level = 'Medium';
                else if (3 <= score && score <= 8) level = 'Low';
                else if (score <= 2) level = 'Negligible';
                return (
                    <Badge
                        count={level}
                        style={{
                            backgroundColor: getRiskColor(score),
                            fontSize: '14px'
                        }}
                    />
                );
            },
        },
        {
            title: t('riskName'),
            dataIndex: 'riskName',
            key: 'riskName',
            // fixed: 'left' as 'left',
            render: (text: string, record: any) => (
                <div>
                    <Text strong>{text}</Text>
                    {record.score >= 40 && (
                        <Tooltip title="High Score Risk">
                            <WarningOutlined style={{ color: '#f5222d', marginLeft: 8 }} />
                        </Tooltip>
                    )}
                </div>
            ),
        },
        {
            title: t('category'),
            dataIndex: 'category',
            key: 'category',
            render: (value: number) => (
                <Tag color={getCategoryColor(value)} style={{ fontSize: '12px' }}>
                    {RiskCategory[value as unknown as keyof typeof RiskCategory] || 'Unknown'}
                </Tag>
            ),
        },
        {
            title: t('strategy'),
            dataIndex: 'strategy',
            key: 'strategy',
            render: (value: number) => (
                <Tag color={getStrategyColor(value)} style={{ fontSize: '12px' }}>
                    {ResponseStrategy[value as unknown as keyof typeof ResponseStrategy] || 'Unknown'}
                </Tag>
            ),
        },
        {
            title: t('dateEmerged'),
            dataIndex: 'raisedAt',
            key: 'raisedAt',
            render: (date: string) => (
                moment(date).format("DD/MM/YYYY")
            ),
            sorter: true,
        },

        {
            title: t('riskOwner'),
            dataIndex: 'riskOwner',
            key: 'riskOwner',
            render: (text: string) => <Text>{text}</Text>
        },
        {
            title: t('actions'),
            key: 'actions',
            // fixed: 'right' as 'right',
            render: (record: any) => (
                <Space>
                    <FeatureGuard requiredFeature='/api/Risk/resolve-risk/{id}_PUT'>
                        <AssessRiskModal
                            riskInfo={mapToRiskBeforeAssessModel(record)}
                            onUpdateSuccess={fetchRisks}
                            isDisabled={record.riskImpactAfterMitigation !== 0} />
                    </FeatureGuard>
                    <FeatureGuard requiredFeature='/api/Risk/{id}_GET'>
                        <RiskDetailsModal risk={record} />
                    </FeatureGuard>
                    <FeatureGuard requiredFeature='/api/Risk_DELETE'>
                        <Button
                            danger
                            type="primary"
                            size="small"
                            onClick={() =>
                                confirmDelete(
                                    () => handleDelete(record.id),
                                    t("confirmDelete.title", { action: t("delete"), itemName: t('viewRisk.thisRisk') }),
                                    t("confirmDelete.content"),
                                    t("confirmDelete.okText", { action: t("delete") }),
                                    t("confirmDelete.cancelText")
                                )
                            }
                            icon={<DeleteOutlined />}
                        />
                    </FeatureGuard>
                </Space>
            )
        }
    ];

    return (
        <ListViewContainer>
            <Row justify="space-between" align="middle" style={{ marginBottom: 24 }}>
                <Col>
                    <Title level={2} style={{ margin: 0 }}>{t('riskManagementDashboard')}</Title>
                </Col>
                <Col>
                    <FeatureGuard requiredFeature='/api/Risk_POST'>
                        <Button
                            type="primary"
                            icon={<PlusOutlined />}
                            onClick={() => navigate('new')}
                            size="middle"
                        >
                            {t('addRisk')}
                        </Button>
                    </FeatureGuard>
                </Col>
            </Row>

            <Row gutter={16} className="mb-4">
                <Col xs={18} sm={6}>
                    <Card className="shadow-sm" bordered={false}>
                        <Statistic
                            title={<Text strong style={{ fontSize: 16 }}>{t('criticalRisks')}</Text>}
                            value={riskSummary.extreme}
                            valueStyle={{ color: '#f5222d' }}
                            prefix={<WarningOutlined />}
                        />
                        <Progress
                            percent={Math.round((riskSummary.extreme / Math.max(riskSummary.total, 1)) * 100)}
                            strokeColor="#f5222d"
                            showInfo={false}
                        />
                    </Card>
                </Col>
                <Col xs={18} sm={6}>
                    <Card className="shadow-sm" bordered={false}>
                        <Statistic
                            title={<Text strong style={{ fontSize: 16 }}>{t('highScoreRisks')}</Text>}
                            value={riskSummary.high}
                            valueStyle={{ color: '#f5222d' }}
                            prefix={<WarningOutlined />}
                        />
                        <Progress
                            percent={Math.round((riskSummary.high / Math.max(riskSummary.total, 1)) * 100)}
                            strokeColor="#f5222d"
                            showInfo={false}
                        />
                    </Card>
                </Col>
                <Col xs={18} sm={6}>
                    <Card className="shadow-sm" bordered={false}>
                        <Statistic
                            title={<Text strong style={{ fontSize: 16 }}>{t('mediumScoreRisks')}</Text>}
                            value={riskSummary.medium}
                            valueStyle={{ color: '#faad14' }}
                            prefix={<AreaChartOutlined />}
                        />
                        <Progress
                            percent={Math.round((riskSummary.medium / Math.max(riskSummary.total, 1)) * 100)}
                            strokeColor="#faad14"
                            showInfo={false}
                        />
                    </Card>
                </Col>
                <Col xs={18} sm={6}>
                    <Card className="shadow-sm" bordered={false}>
                        <Statistic
                            title={<Text strong style={{ fontSize: 16 }}>{t('lowScoreRisks')}</Text>}
                            value={riskSummary.low}
                            valueStyle={{ color: '#52c41a' }}
                            prefix={<AreaChartOutlined />}
                        />
                        <Progress
                            percent={Math.round((riskSummary.low / Math.max(riskSummary.total, 1)) * 100)}
                            strokeColor="#52c41a"
                            showInfo={false}
                        />
                    </Card>
                </Col>
            </Row>

            <Card className="shadow-sm">
                <Space style={{ marginBottom: 16 }} wrap size="middle">
                    <Input
                        placeholder={t('searchRiskName')}
                        allowClear
                        value={searchKey}
                        onChange={(e) => setSearchKey(e.target.value)}
                        style={{ width: 200 }}
                        prefix={<SearchOutlined />}
                    />
                    <Select
                        style={{ width: 170 }}
                        placeholder={t('category')}
                        allowClear
                        value={selectedCategory}
                        onChange={setSelectedCategory}
                        suffixIcon={<FilterOutlined />}
                    >
                        {categoryOptions.map((label, index) => (
                            <Option key={index} value={index}>
                                <Tag color={getCategoryColor(index)} style={{ marginRight: 8 }}></Tag>
                                {label}
                            </Option>
                        ))}
                    </Select>
                    <Select
                        style={{ width: 170 }}
                        placeholder={t('strategy')}
                        allowClear
                        value={selectedStrategy}
                        onChange={setSelectedStrategy}
                        suffixIcon={<FilterOutlined />}
                    >
                        {strategyOptions.map((label, index) => (
                            <Option key={index} value={index}>
                                <Tag color={getStrategyColor(index)} style={{ marginRight: 8 }}></Tag>
                                {label}
                            </Option>
                        ))}
                    </Select>
                    {/* <Select
                        placeholder="Status(testing)"
                    >
                        <Option>Registered</Option>
                        <Option>Closed</Option>
                        <Option>Assigned To ...@gmail.com</Option>
                        <Option>In Progress</Option>
                        <Option>Done</Option>
                    </Select> */}
                    <Select
                        style={{ width: 150 }}
                        placeholder={t('score')}
                        allowClear
                        value={selectedScore}
                        onChange={setSelectedScore}
                        suffixIcon={<FilterOutlined />}
                    >
                        <Option value={40}>
                            <Badge count={t('extreme')} style={{ backgroundColor: 'red' }} />
                        </Option>
                        <Option value={20}>
                            <Badge count={t('high')} style={{ backgroundColor: 'orange' }} />
                        </Option>
                        <Option value={10}>
                            <Badge count={t('medium')} style={{ backgroundColor: 'yellow' }} />
                        </Option>
                        <Option value={3}>
                            <Badge count={t('low')} style={{ backgroundColor: 'green' }} />
                        </Option>
                        <Option value={1}>
                            <Badge count={t('negligible')} style={{ backgroundColor: 'cyan' }} />
                        </Option>
                    </Select>
                    <Button
                        onClick={resetFilters}
                        icon={<ReloadOutlined />}
                    >
                        {t('reset')}
                    </Button>
                </Space>

                <Table
                    columns={columns}
                    rowKey={'id'}
                    dataSource={getFilteredRisks()} // Change this line
                    loading={loading}
                    pagination={{
                        current: currentPage,
                        pageSize,
                        total,
                        showSizeChanger: true,
                        pageSizeOptions: ['10', '20', '50'],
                        showTotal: (total) => `${t('total')} ${total} risks`
                    }}
                    onChange={handleTableChange}
                    bordered
                    scroll={{ x: 1300 }}
                    size="middle"
                // rowClassName={(record) => {
                //     return getRowColor(record.score) //+ " hover:bg-gray-100 cursor-pointer"
                // }}
                />
            </Card>
        </ListViewContainer >
    );
};

export default FetchRiskList;