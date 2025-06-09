import React, { useState, useEffect } from 'react';
import { Alert, Layout, Typography, Form, Input, Select, Button, Checkbox, Table, Card, Upload, Space, Divider, Row, Col, Avatar, Tag, Tooltip, Steps, message, Tabs, DatePicker } from 'antd';
import { LeftOutlined, UploadOutlined, DeleteOutlined, UserOutlined, InfoCircleOutlined, PlusOutlined, FileTextOutlined, TeamOutlined, CheckCircleOutlined, ArrowLeftOutlined, EditOutlined, CrownOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import type { UploadProps, UploadFile } from 'antd/es/upload/interface';
import AxiosClient from '../../configs/axiosConfig';
import dayjs from 'dayjs';
import { useTranslation } from 'react-i18next';

const { Title, Text } = Typography;
const { Option } = Select;
const { TextArea } = Input;
const { Header, Content } = Layout;

interface TeamMember {
  id: string;
  key: number;
  no: number;
  fullName: string;
  email: string;
  groupNames: string[];
}

interface ResponsibilityAssignment {
  id: string;
  key: number;
  no: number;
  responsibility: string;
  responsibilityId: string;
  deadline: Date | null;
  assignees: Array<{
    id: string;
    fullName: string;
    email: string;
    isPic?: boolean;
  }>;
  picId?: string;
}

interface Responsibility {
  id: string;
  title: string;
}

interface System {
  id: string;
  name: string
  description: string
  status: string
  createdAt: string
  createdBy: string
}

const CreateDPIA: React.FC = () => {
  const { t } = useTranslation();

  const [form] = Form.useForm();
  const [assignmentForm] = Form.useForm();
  const [fileList, setFileList] = useState<UploadFile[]>([]);
  const navigate = useNavigate();
  const [currentStep, setCurrentStep] = useState(0);
  const [activeTab, setActiveTab] = useState('information');
  // Table data
  const [assignments, setAssignments] = useState<ResponsibilityAssignment[]>([]);
  const [availableMembers, setAvailableMembers] = useState<TeamMember[]>([]);
  const [responsibilities, setResponsibilities] = useState<Responsibility[]>([]);
  const [loading, setLoading] = useState(false);
  const [systems, setSystems] = useState<System[]>([]);
  const [selectedSystemId, setSelectedSystemId] = useState<string | null>(null);
  const [editingKey, setEditingKey] = useState<number | null>(null);
  const [dpiaDueDate, setDpiaDueDate] = useState<dayjs.Dayjs | null>(null);
  const [defaultResponsibilitiesAdded, setDefaultResponsibilitiesAdded] = useState(false);
  const MAX_FILE_SIZE = 25 * 1024 * 1024; // 25MB in bytes
  const ALLOWED_EXTENSIONS = ['.xlsx', '.docx', '.pdf', '.png', '.jpeg', '.jpg'];
  const dpiaTypeOptions = ['NewOrUpdatedSystem', 'PeriodicReview', 'DataBreach'];

  // Default responsibility titles
  const DEFAULT_RESPONSIBILITIES = [
    'Policy Compliance Assessment',
    'Information Security Assessment',
    'Technology Architecture Assessment'
  ];

  useEffect(() => {
    fetchSystems();
    fetchResponsibilities();
  }, []);

  useEffect(() => {
    if (selectedSystemId) {
      fetchSystemTeamMembers();
    } else {
      setAvailableMembers([]);
    }
  }, [selectedSystemId]);

  // Effect to handle default responsibilities when system and due date are selected
  useEffect(() => {
    if (selectedSystemId && form.getFieldValue('dueDate') && responsibilities.length > 0 && !defaultResponsibilitiesAdded) {
      addDefaultResponsibilities();
    }
  }, [selectedSystemId, form.getFieldValue('dueDate'), responsibilities, defaultResponsibilitiesAdded]);

  const fetchSystems = async () => {
    setLoading(true);
    try {
      const response = await AxiosClient.get('ExternalSystem');
      setSystems(response.data);
      setLoading(false);
    } catch (error) {
      message.error(t('failed_to_load_available_systems'));
      setLoading(false);
    }
  }

  const fetchResponsibilities = async () => {
    setLoading(true);
    try {
      const response = await AxiosClient.get('/responsibility');
      setResponsibilities(response.data);
      setLoading(false);
    } catch (error) {
      console.error('Error fetching responsibilities:', error);
      message.error(t('failed_to_load_responsibilities'));
      setLoading(false);
    }
  };

  const fetchSystemTeamMembers = async () => {
    setLoading(true);
    try {
      const response = await AxiosClient.get(`/Group/get-dpia-users`);
      setAvailableMembers(response.data);
      console.log(availableMembers);
      setLoading(false);
    } catch (error) {
      console.error('Error fetching system team members:', error);
      message.error(t('failed_to_load_team_members'));
      setLoading(false);
    }
  };


  const addDefaultResponsibilities = () => {
    const dpiaDueDate = form.getFieldValue('dueDate');
    if (!dpiaDueDate || !responsibilities.length) return;
    const defaultResponsibilityObjects = DEFAULT_RESPONSIBILITIES.map(title => {
      return responsibilities.find(r => r.title === title);
    }).filter(r => r !== undefined) as Responsibility[];

    if (defaultResponsibilityObjects.length === 0) {
      message.warning(t('default_responsibilities_not_found'));
      return;
    }
    // Create new assignments for each default responsibility
    const newAssignments: ResponsibilityAssignment[] = [];
    defaultResponsibilityObjects.forEach((resp, index) => {
      if (!resp) return;

      // Check if this responsibility is already assigned
      const exists = assignments.some(a => a.responsibilityId === resp.id);
      if (exists) return;

      // Create new assignment with today as placeholder
      const newAssignment: ResponsibilityAssignment = {
        id: "", // temporary id
        key: assignments.length + newAssignments.length + 1,
        no: assignments.length + newAssignments.length + 1,
        responsibility: resp.title,
        responsibilityId: resp.id,
        deadline: new Date(new Date().setDate(new Date().getDate() + 1)), // Use today as placeholder - user will need to change it
        assignees: [],
      };

      newAssignments.push(newAssignment);
    });

    if (newAssignments.length > 0) {
      setAssignments(prev => [...prev, ...newAssignments]);
      setDefaultResponsibilitiesAdded(true);
    }
  };

  const validateFile = (file: any) => {
    if (file.size > MAX_FILE_SIZE) {
      message.error(`${file.name} ${t('fileSizeError', { maxSize: '25MB' })}`);
      return false;
    }

    // Check file extension
    const extension = '.' + file.name.split('.').pop().toLowerCase();
    if (!ALLOWED_EXTENSIONS.includes(extension)) {
      message.error(`${file.name} ${t('fileTypeError', { types: ALLOWED_EXTENSIONS.join(', ') })}`);
      return false;
    }

    return true;
  };

  const handleFileChange: UploadProps['onChange'] = ({ fileList: newFileList }) => {
    setFileList(newFileList);
    form.setFieldsValue({ attachments: newFileList });
  };

  const handleSystemChange = (value: string) => {
    setSelectedSystemId(value);
    assignmentForm.resetFields();
    setDefaultResponsibilitiesAdded(false); // Reset flag when system changes
  };

  // Handle Due Date Change to trigger default responsibilities
  const handleDueDateChange = (date: dayjs.Dayjs | null) => {
    setDpiaDueDate(date);
    if (date && selectedSystemId && responsibilities.length > 0) {
      // Reset the flag to allow adding default responsibilities again
      setDefaultResponsibilitiesAdded(false);
    }
  };

  // Modified handleAssigneeChange to handle single assignee who is automatically PIC
  const handleAssigneeChange = (key: number, email: string) => {
    const updatedAssignments = assignments.map(assignment => {
      if (assignment.key === key) {
        // Find the selected member
        const selectedMember = availableMembers.find(m => m.email === email);

        if (selectedMember) {
          // Create assignees array with just one member who is also PIC
          const assignees = [{
            id: selectedMember.id,
            fullName: selectedMember.fullName,
            email: selectedMember.email,
            isPic: true // This person is automatically the PIC
          }];

          return {
            ...assignment,
            assignees,
            picId: selectedMember.id
          };
        } else {
          return {
            ...assignment,
            assignees: [],
            picId: undefined
          };
        }
      }
      return assignment;
    });

    setAssignments(updatedAssignments);
  };

  // Handler for changing due date directly in the table
  const handleDueDateChangeInTable = (key: number, date: dayjs.Dayjs | null) => {
    if (!date) {
      // Handle date removal - set deadline to null for Date type
      const updatedAssignments = assignments.map(assignment => {
        if (assignment.key === key) {
          return {
            ...assignment,
            deadline: new Date()  // Use null for Date type, not empty string
          };
        }
        return assignment;
      });
      setAssignments(updatedAssignments);
      return;
    }

    // Use dayjs directly instead of converting to Moment
    const today = dayjs().startOf('day');
    if (date.isBefore(today)) {
      message.error(t('deadline_date_cannot_be_past'));
      return;
    }

    // Update the assignment with an actual Date object
    const updatedAssignments = assignments.map(assignment => {
      if (assignment.key === key) {
        return {
          ...assignment,
          deadline: date.toDate() // Convert dayjs to actual Date object
        };
      }
      return assignment;
    });

    setAssignments(updatedAssignments);
  };

  // Updated to also check for missing deadlines
  const getRowClassName = (record: ResponsibilityAssignment, index: number) => {
    if (record.key === editingKey) {
      return 'editing-row';
    }
    if (record.assignees.length === 0 || !record.deadline) {
      return 'incomplete-row';
    }
    return index % 2 === 0 ? 'even-row' : 'odd-row';
  };

  // Table columns for assignments - Updated for single assignee
  const assignmentColumns = [
    {
      title: t('no'),
      dataIndex: 'no',
      key: 'no',
      width: 60,
      align: 'center' as const,
    },
    {
      title: t('responsibility'),
      dataIndex: 'responsibility',
      key: 'responsibility',
      width: 150,
      render: (text: string, record: ResponsibilityAssignment) => (
        <Text strong>{text}</Text>
      ),
    },
    {
      title: t('due_date'),
      dataIndex: 'deadline',
      key: 'deadline',
      width: 140,
      render: (_: string, record: ResponsibilityAssignment) => {
        return (
          <DatePicker
            // Convert Date to dayjs for DatePicker
            value={record.deadline ? dayjs(record.deadline) : undefined}
            format="DD/MM/YYYY"
            onChange={(newDate) => handleDueDateChangeInTable(record.key, newDate)}
            style={{ width: '100%' }}
            placeholder={t('select_due_date')}
            disabledDate={(current) => {
              const dpiaDueDate = form.getFieldValue('dueDate');
              return (current && current < dayjs().startOf('day')) ||
                (dpiaDueDate && current && current > dpiaDueDate);
            }}
          />
        );
      },
    },
    // Modified assignment column render function
    {
      title: t('assignment'),
      key: 'assignTo',
      width: 300,
      render: (_: any, record: ResponsibilityAssignment) => (
        <div>
          <Select
            value={record.assignees.length > 0 ? record.assignees[0].email : undefined}
            style={{ width: '100%' }}
            dropdownMatchSelectWidth={false}
            onChange={(value) => handleAssigneeChange(record.key, value)}
            placeholder={t('select_team_member')}
            optionLabelProp="label" // IMPORTANT!
          >
            {availableMembers.map(member => (
              <Option
                key={member.id}
                value={member.email}
                label={
                  <span>
                  
                    <span >{member.email}</span>
                    {member.groupNames && member.groupNames.length > 0 && (
                      <>
                        {" — "}
                        {member.groupNames.join(", ")}
                      </>
                    )}
                  </span>
                }
              >
                <div style={{ display: 'flex', alignItems: 'center' }}>
                  <Avatar size="small" icon={<UserOutlined />} style={{ marginRight: 8 }} />
                  <div>
                    <div>{member.email}</div>
                    {member.groupNames && member.groupNames.length > 0 && (
                      <div style={{ marginTop: 2 }}>
                        {member.groupNames.map((group, idx) => (
                          <span
                            key={idx}
                            style={{
                              background: '#e6f4ff',
                              color: '#1890ff',
                              borderRadius: 4,
                              fontSize: 11,
                              padding: '1px 6px',
                              marginRight: 4,
                              display: 'inline-block'
                            }}
                          >
                            {group}
                          </span>
                        ))}
                      </div>
                    )}
                  </div>
                </div>
              </Option>
            ))}
          </Select>

          {record.assignees.length > 0 && (
            <Tag color="success" style={{ marginTop: '4px' }}>
              {record.assignees[0].fullName || record.assignees[0].email} (PIC)
            </Tag>
          )}
        </div>
      ),
    },
    {
      title: t('actions'),
      key: 'action',
      width: 100,
      align: 'center' as const,
      render: (_: any, record: ResponsibilityAssignment) => (
        <Space>
          <Button
            type="text"
            icon={<EditOutlined />}
            style={{ color: '#1890ff' }}
            onClick={() => handleEditAssignment(record)}
          />
          <Button
            type="text"
            icon={<DeleteOutlined />}
            danger
            onClick={() => handleRemoveAssignment(record.key)}
          />
        </Space>
      ),
    },
  ];

  // Updated to add responsibility with date
  const handleAddAssignment = () => {
    const values = assignmentForm.getFieldsValue(['responsibility']);
    if (!values.responsibility) {
      message.error(t('please_select_responsibility'));
      return;
    }

    // Find the responsibility object
    const selectedResp = responsibilities.find(r => r.id === values.responsibility);
    if (!selectedResp) {
      message.error(t('selected_responsibility_not_found'));
      return;
    }

    // Check if this responsibility is already assigned
    const exists = assignments.some(a => a.responsibilityId === values.responsibility);
    if (exists) {
      message.error(t('this_responsibility_already_assigned'));
      return;
    }


    const newAssignment: ResponsibilityAssignment = {
      id: "", // temporary id
      key: assignments.length + 1,
      no: assignments.length + 1,
      responsibility: selectedResp.title,
      responsibilityId: selectedResp.id,
      deadline: new Date(new Date().setDate(new Date().getDate() + 1)),
      assignees: [],
    };

    setAssignments([...assignments, newAssignment]);
    assignmentForm.resetFields();
    message.success(t('responsibility_added'));
  };

  const handleEditAssignment = (record: ResponsibilityAssignment) => {
    assignmentForm.setFieldsValue({
      responsibility: record.responsibilityId,
    });
    setEditingKey(record.key);
  };

  // Modified handleUpdateAssignment function
  const handleUpdateAssignment = () => {
    const values = assignmentForm.getFieldsValue(['responsibility']);

    if (!values.responsibility) {
      message.error(t('please_select_responsibility'));
      return;
    }

    // Find the responsibility object
    const selectedResp = responsibilities.find(r => r.id === values.responsibility);
    if (!selectedResp) {
      message.error(t('selected_responsibility_not_found'));
      return;
    }

    // Check if this responsibility is already assigned to another row
    const isDuplicate = assignments.some(a =>
      a.responsibilityId === values.responsibility && a.key !== editingKey
    );

    if (isDuplicate) {
      message.error(t('this_responsibility_already_assigned'));
      return;
    }

    // Update the assignment - preserve existing assignees and deadline
    const updatedAssignments = assignments.map(assignment => {
      if (assignment.key === editingKey) {
        return {
          ...assignment,
          responsibility: selectedResp.title,
          responsibilityId: selectedResp.id,
        };
      }
      return assignment;
    });

    setAssignments(updatedAssignments);
    assignmentForm.resetFields();
    setEditingKey(null);
    message.success(t('assignment_updated'));
  };

  const handleRemoveAssignment = (key: number) => {
    setAssignments(assignments.filter(assignment => assignment.key !== key));
    message.success(t('assignment_removed'));
  };

  // Better normFile function with debugging
  const normFile = (e: any) => {
    console.log('normFile called with:', e);
    if (Array.isArray(e)) {
      return e;
    }
    return e?.fileList || [];
  };

  // Better file removal handler
  const handleRemoveFile = (file: UploadFile) => {
    console.log('Removing file:', file);
    const newFileList = fileList.filter(item => item.uid !== file.uid);
    setFileList(newFileList);
    return true;
  };

  const handleSubmit = async (values: any) => {
    setLoading(true);
    try {
      const incompleteAssignments = assignments.filter(a => a.assignees.length === 0);
      if (incompleteAssignments.length > 0) {
        message.error(t('please_assign_team_members'));
        setActiveTab('assignment');
        setCurrentStep(1);
        setLoading(false);
        return;
      }

      const missingDueDateAssignments = assignments.filter(a => !a.deadline);
      if (missingDueDateAssignments.length > 0) {
        message.error(t('please_set_due_dates_for_all_responsibilities'));
        setActiveTab('assignment');
        setCurrentStep(1);
        setLoading(false);
        return;
      }

      const formattedAssignments = assignments.map(ass => ({
        responsibilityId: ass.responsibilityId,
        userIds: [ass.assignees[0].id], // Single assignee
        picId: ass.assignees[0].id,     // Same person is the PIC
        dueDate: ass.deadline?.toISOString() // Convert Date object directly to ISO string
      }));

      // Create FormData for multipart/form-data request
      const formData = new FormData();

      // Add basic fields
      formData.append('Title', values.title);
      formData.append('ExternalSystemId', values.externalSystemId);
      formData.append('Description', values.description || '');
      formData.append('Type', dpiaTypeOptions.indexOf(values.type).toString());

      // Add DPIA due date
      if (values.dueDate) {
        formData.append('DueDate', values.dueDate.format('YYYY-MM-DD'));
      }

      // Add files from fileList to Documents with better error handling
      if (fileList.length > 0) {
        fileList.forEach(file => {
          // Access the actual File object from the UploadFile interface
          if (file.originFileObj) {
            formData.append('Documents', file.originFileObj);
            console.log('Appending file to formData:', file.name);
          } else {
            console.warn('File is missing originFileObj:', file);
          }
        });
      }

      // Add responsibilities with their PICs and due dates
      formattedAssignments.forEach((resp, index) => {
        formData.append(`Responsibilities[${index}].ResponsibilityId`, resp.responsibilityId);
        formData.append(`Responsibilities[${index}].DueDate`, resp.dueDate as string);
        formData.append(`Responsibilities[${index}].Pic`, resp.picId || '');
        formData.append(`Responsibilities[${index}].UserIds[0]`, resp.userIds[0]);
      });

      const response = await AxiosClient.post('/DPIA', formData, {
        headers: {
          'Content-Type': 'multipart/form-data'
        }
      });

      message.success(t('dpia_created_successfully'));
      navigate('../dpias');
    } catch (error: any) {
      console.error('Error creating DPIA:', error);
      message.error(error.apiSingleErrorMessage?.data || 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  const handleTabChange = (key: string) => {
    setActiveTab(key);

    // Update the step based on active tab
    if (key === 'information') {
      setCurrentStep(0);
    } else if (key === 'assignment') {
      setCurrentStep(1);
    } else if (key === 'review') {
      setCurrentStep(2);
    }
  };

  const handleStepChange = (current: number) => {
    setCurrentStep(current);

    // Update active tab based on step
    if (current === 0) {
      setActiveTab('information');
    } else if (current === 1) {
      setActiveTab('assignment');
    } else if (current === 2) {
      setActiveTab('review');
    }
  };

  const renderGeneralInformation = () => (
    <Card
      title={
        <Space>
          <FileTextOutlined />
          <Text strong>{t('dpia_general_information')}</Text>
        </Space>
      }
      bordered={false}
      style={{ borderRadius: 8, width: 900 }}
      bodyStyle={{ paddingBottom: 8 }}
    >
      <Form.Item
        label={t('dpia_title')}
        name="title"
        rules={[{ required: true, message: t('please_input_dpia_title') }]}
        tooltip={{ title: t('provide_clear_descriptive_title'), icon: <InfoCircleOutlined /> }}
      >
        <Input placeholder={t('eg_assessment_customer_data')} size="large" />
      </Form.Item>

      <Form.Item
        label={t('dpia_for_system')}
        name="externalSystemId"
        rules={[{ required: true, message: t('please_select_system') }]}
        tooltip={t('select_system_assessed')}
      >
        <Select
          placeholder={t('select_external_system')}
          size="large"
          dropdownMatchSelectWidth={false}
          onChange={handleSystemChange}
        >
          {systems.map(system => (
            <Option key={system.id} value={system.id}>{system.name}</Option>
          ))}
        </Select>
      </Form.Item>

      <Form.Item
        label={t('type_of_dpia')}
        name="type"
        rules={[{ required: true, message: t('please_select_dpia_type') }]}
      >
        <Select
          placeholder={t('select_dpia_type')}
          size="large"
        >
          {dpiaTypeOptions.map(option => (
            <Option key={option} value={option}>
              {option === 'NewOrUpdatedSystem' && <Tag color="green">NewOrUpdatedSystem</Tag>}
              {option === 'PeriodicReview' && <Tag color="blue">PeriodicReview</Tag>}
              {option === 'DataBreach' && <Tag color="purple">DataBreach</Tag>}
            </Option>
          ))}
        </Select>
      </Form.Item>

      {/* DPIA Due Date field */}
      <Form.Item
        label={t('dpia_due_date')}
        name="dueDate"
        rules={[
          { required: true, message: t('please_select_dpia_due_date') },
          {
            validator: (_, value) => {
              if (value && value.isBefore(dayjs().startOf('day'))) {
                return Promise.reject(t('due_date_cannot_be_past'));
              }
              return Promise.resolve();
            }
          }
        ]}
        tooltip={{
          title: t('final_due_date_entire_dpia'),
          icon: <InfoCircleOutlined />
        }}
      >
        <DatePicker
          style={{ width: '100%' }}
          format="DD/MM/YYYY"
          placeholder={t('select_dpia_due_date')}
          disabledDate={(current) => {
            return current && current < dayjs().startOf('day');
          }}
          onChange={handleDueDateChange}
        />
      </Form.Item>

      <Form.Item
        label={t('description')}
        name="description"
        rules={[{ required: true, message: t('please_provide_description') }]}
        tooltip={t('explain_reason_dpia_scope')}
      >
        <TextArea
          placeholder={t('provide_details_why_dpia_needed')}
          rows={5}
          showCount
          maxLength={500}
        />
      </Form.Item>

      <Form.Item
        label={t('upload_attachments')}
        name="attachments"
        valuePropName="fileList"
        getValueFromEvent={normFile}
      >
        <Upload
          fileList={fileList}
          onChange={handleFileChange}
          onRemove={handleRemoveFile}
          beforeUpload={(file) => {
            const isValid = validateFile(file);
            return isValid ? false : Upload.LIST_IGNORE;
          }}
          multiple
          listType="text"
          accept=".xlsx,.docx,.pdf,.png,.jpeg,.jpg"
        >
          <Button icon={<UploadOutlined />} type="dashed" block>
            {t('add_document')}
          </Button>
          <div style={{ marginTop: 8, color: '#888', fontSize: '12px' }}>
            {t('fileRestrictions', {
              maxSize: '25MB',
              types: ALLOWED_EXTENSIONS.join(', ')
            })}
          </div>
        </Upload>
      </Form.Item>
    </Card>
  );

  const renderAssignments = () => (
    <Card
      title={
        <Space>
          <TeamOutlined />
          <Text strong>{t('dpia_assignment')}</Text>
        </Space>
      }
      bordered={false}
      style={{ borderRadius: 8, width: 900 }}
      bodyStyle={{ padding: '16px 24px 24px' }}
    >
      {!selectedSystemId ? (
        <Alert
          message={t('system_selection_required')}
          description={t('select_system_view_team')}
          type="info"
          showIcon
          style={{ marginBottom: 16 }}
        />
      ) : !form.getFieldValue('dueDate') ? (
        <Alert
          message={t('dpia_due_date_required')}
          description={t('set_dpia_due_date_add_responsibilities')}
          type="warning"
          showIcon
          style={{
            marginBottom: 10,
            padding: '8px 12px', // reduce internal spacing
            fontSize: 11,        // smaller text
            lineHeight: 1.2      // tighter vertical spacing
          }}
        />
      ) : (
        <>
          <Divider>
            <Space>
              <TeamOutlined />
              <Text type="secondary">{t('responsibilities_assignment')}</Text>
            </Space>
          </Divider>

          <Alert
            message={t('deadline_constraint')}
            description={t('responsibilities_deadline_warning', {
              date: form.getFieldValue('dueDate').format('DD/MM/YYYY')
            })}
            type="info"
            showIcon
            style={{
              marginBottom: 10,
              padding: '8px 12px', // reduce internal spacing
              fontSize: 11,        // smaller text
              lineHeight: 1.2      // tighter vertical spacing
            }}
          />

          <div style={{ marginBottom: 24 }}>
            <Form
              form={assignmentForm}
              layout="horizontal"
            >
              <Row gutter={16} align="middle">
                <Col span={16}>
                  <Form.Item
                    name="responsibility"
                    label={t('responsibility')}
                    style={{ marginBottom: 30 }}
                  >
                    <Select
                      placeholder={t('input_responsibility')}
                      style={{ width: '100%' }}
                      dropdownMatchSelectWidth={false}
                    >
                      {responsibilities
                        .filter(resp => !assignments.some(a => a.responsibilityId === resp.id))
                        .map(resp => (
                          <Option key={resp.id} value={resp.id}>{resp.title}</Option>
                        ))}
                    </Select>
                  </Form.Item>
                </Col>
                <Col span={8} style={{ textAlign: 'right' }}>
                  <Button
                    type="primary"
                    onClick={editingKey !== null ? handleUpdateAssignment : handleAddAssignment}
                  >
                    {editingKey !== null ? t('update_responsibility') : t('add_responsibility')}
                  </Button>
                  {editingKey !== null && (
                    <Button
                      style={{ marginLeft: 8 }}
                      onClick={() => {
                        setEditingKey(null);
                        assignmentForm.resetFields();
                      }}
                    >
                      {t('cancel')}
                    </Button>
                  )}
                </Col>
              </Row>
            </Form>
          </div>
        </>
      )}

      {selectedSystemId && form.getFieldValue('dueDate') && (
        <div style={{ marginTop: 16 }}>
          <Alert
            message={t('due_date_selection_required')}
            description={t('all_responsibilities_must_have_due_dates')}
            type="info"
            showIcon
            style={{
              marginBottom: 10,
              padding: '8px 12px', // reduce internal spacing
              fontSize: 11,        // smaller text
              lineHeight: 1.2      // tighter vertical spacing
            }}
          />


          <Table
            columns={assignmentColumns}
            dataSource={assignments}
            pagination={false}
            size="middle"
            bordered
            rowClassName={getRowClassName}
            scroll={{ x: 800 }}
            loading={loading}
            rowKey="key"
            className="colored-table"
          />

          <style>
            {`
  .colored-table .ant-table-thead > tr > th {
    background-color: #f0f7ff !important;
    color: #1890ff;
    font-weight: 600;
  }
  .colored-table .even-row {
    background-color: #ffffff;
  }
  .colored-table .odd-row {
    background-color: #f9fbff;
  }
  .colored-table .editing-row {
    background-color: #fffbe6;
  }
  .colored-table .incomplete-row {
    background-color: #fff7f7;
  }
  .colored-table .ant-table-row:hover > td {
    background-color: #e6f7ff !important;
  }
`}
          </style>
        </div>
      )}
    </Card>
  );

  // Updated review section to show single assignee as PIC with proper date formatting
  const renderReviewInfo = () => (
    <Card
      title={
        <Space>
          <CheckCircleOutlined />
          <Text strong>{t('review_and_submit')}</Text>
        </Space>
      }
      bordered={false}
      style={{ borderRadius: 8, width: 900 }}
    >
      <Row gutter={24}>
        <Col span={12}>
          <Title level={5} style={{ color: '#1890ff' }}>
            <FileTextOutlined style={{ marginRight: 8 }} />
            {t('general_information')}
          </Title>
          <Divider style={{ margin: '12px 0', borderColor: '#1890ff' }} />
          <div style={{ marginBottom: 16 }}>
            <Text type="secondary">{t('dpia_title')}:</Text>
            <div><Text strong>{form.getFieldValue('title') || '-'}</Text></div>
          </div>
          <div style={{ marginBottom: 16 }}>
            <Text type="secondary">{t('system.name')}:</Text>
            <div><Text strong>{
              systems.find(s => s.id === form.getFieldValue('externalSystemId'))?.name || '-'
            }</Text></div>
          </div>
          <div style={{ marginBottom: 16 }}>
            <Text type="secondary">{t('dpia_type')}:</Text>
            <div>
              {form.getFieldValue('type') ? (
                <Tag color={
                  form.getFieldValue('type') === 'NewOrUpdatedSystem' ? 'green' :
                    form.getFieldValue('type') === 'PeriodicReview' ? 'blue' :
                      form.getFieldValue('type') === 'DataBreach' ? 'purple' : 'orange'
                }>
                  {form.getFieldValue('type')}
                </Tag>
              ) : '-'}
            </div>
          </div>
          <div style={{ marginBottom: 16 }}>
            <Text type="secondary">{t('dpia_due_date')}:</Text>
            <div>
              {form.getFieldValue('dueDate') ? (
                <Tag color="red">{form.getFieldValue('dueDate').format('DD/MM/YYYY')}</Tag>
              ) : '-'}
            </div>
          </div>
          <div style={{ marginBottom: 16 }}>
            <Text type="secondary">{t('attachments')}:</Text>
            <div>
              {fileList.length > 0 ? (
                fileList.map(file => (
                  <div key={file.uid} style={{ marginTop: 8 }}>
                    <Space>
                      <UploadOutlined style={{ color: '#1890ff' }} />
                      <Text>{file.name}</Text>
                    </Space>
                  </div>
                ))
              ) : <Text>{t('no_attachments')}</Text>}
            </div>
          </div>
        </Col>
        <Col span={12}>
          <Title level={5} style={{ color: '#1890ff' }}>
            <TeamOutlined style={{ marginRight: 8 }} />
            {t('responsibility_assignments')} ({assignments.length})
          </Title>
          <Divider style={{ margin: '12px 0', borderColor: '#1890ff' }} />

          {assignments.length > 0 ? (
            assignments.map((assignment, idx) => (
              <Card
                key={assignment.key}
                size="small"
                style={{
                  marginBottom: 16,
                  borderLeft: '3px solid #1890ff',
                  backgroundColor: idx % 2 === 0 ? '#f9fbff' : '#ffffff'
                }}
              >
                <div>
                  <Text strong>{assignment.responsibility}</Text>
                  <div>
                    <Tag color="blue">
                      Deadline: {assignment.deadline ? dayjs(assignment.deadline).format('DD/MM/YYYY') : 'Not set'}
                    </Tag>
                  </div>

                  <div style={{ marginTop: 8 }}>
                    <div style={{ display: 'flex', alignItems: 'center' }}>
                      <Tag color={assignment.assignees.length ? 'success' : 'warning'}>
                        {assignment.assignees.length
                          ? t('assigned_to')
                          : t('not_assigned')}
                      </Tag>
                    </div>

                    {assignment.assignees.length > 0 && (
                      <div style={{ marginTop: 4, marginLeft: 8 }}>
                        <div style={{ marginTop: 4 }}>
                          <Space>
                            <Avatar
                              size="small"
                              icon={<UserOutlined />}
                              style={{ backgroundColor: '#52c41a' }}
                            />
                            <Text>{assignment.assignees[0].email}</Text>
                            <Tag color="success">PIC</Tag>
                          </Space>
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              </Card>
            ))
          ) : <Text>No assignments added</Text>}
        </Col>
      </Row>
    </Card>
  );

  return (
    <Layout style={{ minHeight: '100vh', backgroundColor: '#f5f7fa' }}>
      {/* Header section */}
      <Header style={{ backgroundColor: 'white', padding: '0 24px', boxShadow: '0 2px 8px rgba(0,0,0,0.06)', display: 'flex', alignItems: 'center' }}>
        <Space align="center">
          <Button type="text" icon={<ArrowLeftOutlined />} style={{ borderRadius: "6px" }} onClick={() => {
            navigate('../dpias');
          }} />
          <Divider type="vertical" style={{ height: 24 }} />
          <Space>
            <Avatar style={{ backgroundColor: '#1890ff' }}>D</Avatar>
            <Text strong>{t('dpia')}</Text>
            <Text type="secondary" style={{ color: '#8c8c8c' }}>/</Text>
            <Text type="secondary">{t('create_dpia')}</Text>
          </Space>
        </Space>
      </Header>

      <Content style={{ padding: '32px', maxWidth: 1000, margin: '0 auto' }}>
        <Card style={{ marginBottom: 24, borderRadius: 8 }}>
          <Title level={3} style={{ marginBottom: 24 }}>{t('create_new_dpia')}</Title>

          <Steps
            current={currentStep}
            onChange={handleStepChange}
            items={[
              { title: t('information'), icon: <FileTextOutlined /> },
              { title: t('assignment'), icon: <TeamOutlined /> },
              { title: t('review'), icon: <CheckCircleOutlined /> }
            ]}
            style={{ marginBottom: 32 }}
          />

          <Form
            form={form}
            layout="vertical"
            onFinish={handleSubmit}
            initialValues={{
              sendNotify: true
            }}
          >
            <Tabs
              activeKey={activeTab}
              onChange={handleTabChange}
              items={[
                {
                  key: 'information',
                  label: t('general_information'),
                  children: renderGeneralInformation()
                },
                {
                  key: 'assignment',
                  label: t('assignment'),
                  children: renderAssignments()
                },
                {
                  key: 'review',
                  label: t('review'),
                  children: renderReviewInfo()
                }
              ]}
            />

            {/* Form buttons */}
            <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: 24 }}>
              <Space>
                {activeTab !== 'information' && (
                  <Button
                    size="large"
                    onClick={() => {
                      const prevTab = activeTab === 'assignment' ? 'information' : 'assignment';
                      setActiveTab(prevTab);
                      setCurrentStep(currentStep - 1);
                    }}
                  >
                    {t('previous')}
                  </Button>
                )}
              </Space>

              <Space>
                <Button size="large" onClick={() => {
                  form.resetFields();
                  assignmentForm.resetFields();
                  setAssignments([]);
                  setFileList([]);
                  setDefaultResponsibilitiesAdded(false);
                }}>
                  {t('reset')}
                </Button>

                {activeTab !== 'review' ? (
                  <Button
                    type="primary"
                    size="large"
                    onClick={() => {
                      const nextTab = activeTab === 'information' ? 'assignment' : 'review';
                      setActiveTab(nextTab);
                      setCurrentStep(currentStep + 1);
                    }}
                  >
                    {t('next')}
                  </Button>
                ) : (
                  <Button
                    type="primary"
                    size="large"
                    loading={loading}
                    onClick={() => {
                      form.validateFields().then(values => {
                        handleSubmit(values);
                      }).catch(err => {
                        message.error(t('please_complete_all_required_fields'));
                      });
                    }}
                  >
                    {t('create_dpia_button')}
                  </Button>
                )}
              </Space>
            </div>
          </Form>
        </Card>
      </Content>
    </Layout>
  );
};

export default CreateDPIA;
