import { Button, Checkbox, Input, message, Select, Tooltip, Card, Spin, Layout } from 'antd';
import React, { useState, useCallback } from 'react';
import { EditOutlined, DeleteOutlined, SaveOutlined, CloseOutlined, PlusOutlined, FormOutlined } from '@ant-design/icons';
import { FICStructure, FormElement, SampleFICStructure } from '../models/FICPostModel';
import AxiosClient from '../../../configs/axiosConfig';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageHeader from '../../../components/common/PageHeader';

const { TextArea } = Input;

// Extend FormElement to include a unique ID
type ExtendedFormElement = FormElement & {
    id: string;
};

type ExtendedFICStructure = Omit<FICStructure, 'formElements'> & {
    formElements: ExtendedFormElement[];
};

const FICTemplateConfig = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const [loading, setLoading] = useState(false);

    // Function to generate a unique ID
    const generateUniqueId = useCallback((): string => {
        return crypto.randomUUID(); // Generates a universally unique identifier (UUID v4)
    }, []);

    // Function to add IDs to existing elements
    const addIdsToStructure = useCallback((structure: FICStructure): ExtendedFICStructure => {
        const addIdsToElements = (elements: FormElement[]): ExtendedFormElement[] => {
            return elements.map(elem => ({
                ...elem,
                id: generateUniqueId(),
                children: addIdsToElements(elem.children)
            }));
        };

        return {
            ...structure,
            formElements: addIdsToElements(structure.formElements)
        };
    }, [generateUniqueId]);

    // Initial structure with IDs
    const [structure, setStructure] = useState<ExtendedFICStructure>(addIdsToStructure(SampleFICStructure));

    // Form name state
    const [formName, setFormName] = useState(structure.name);

    // New element inputs
    const [newItemName, setNewItemName] = useState("");
    const [newItemNameError, setNewItemNameError] = useState("");
    const [selectedParentPath, setSelectedParentPath] = useState<string[]>([]);

    // Edit mode state
    const [editingElementPath, setEditingElementPath] = useState<string[]>([]);
    const [editingElementName, setEditingElementName] = useState("");
    const [editingElementNameError, setEditingElementNameError] = useState("");

    // Track validation state for leaf nodes missing data types
    const [invalidLeafNodes, setInvalidLeafNodes] = useState<Set<string>>(new Set());
    const [showValidationWarnings, setShowValidationWarnings] = useState(false);

    // Update form name
    const updateFormName = () => {
        if (formName.trim() === "") {
            message.error(t('ficTemplateConfig.emptyFormNameError'));
            return;
        }

        setStructure({
            ...structure,
            name: formName
        });
        message.success(t('ficTemplateConfig.formNameUpdated'));
    };

    // Validate element name
    const validateElementName = (name: string, parentPath: string[] = []): { isValid: boolean, error: string } => {
        if (name.trim() === "") {
            return { isValid: false, error: t('ficTemplateConfig.emptyElementNameError') };
        }

        // Check for duplicate names at the same level
        let currentLevel = structure.formElements;
        let parent: ExtendedFormElement | undefined;

        for (const id of parentPath) {
            parent = currentLevel.find(elem => elem.id === id);
            if (!parent) break;
            currentLevel = parent.children as ExtendedFormElement[];
        }

        const siblings = parentPath.length === 0 ? structure.formElements :
            (parent ? parent.children as ExtendedFormElement[] : []);

        // When editing, we need to exclude the current element from duplicate check
        const isEditing = editingElementPath.length > 0;
        const currentEditingId = isEditing ? editingElementPath[editingElementPath.length - 1] : "";

        const duplicateName = siblings.some(elem =>
            elem.name.toLowerCase() === name.trim().toLowerCase() &&
            (!isEditing || elem.id !== currentEditingId)
        );

        if (duplicateName) {
            return { isValid: false, error: t('ficTemplateConfig.duplicateElementNameError') };
        }

        return { isValid: true, error: "" };
    };

    // Add new element to the structure
    const addElement = () => {
        const validation = validateElementName(newItemName, selectedParentPath);
        if (!validation.isValid) {
            setNewItemNameError(validation.error);
            return;
        }

        setNewItemNameError("");

        // Create a copy of the structure
        const newStructure = { ...structure };

        // Create the new element with a unique ID
        const newElement: ExtendedFormElement = {
            id: generateUniqueId(),
            name: newItemName.trim(),
            dataType: null,
            orderIndex: 0,
            children: []
        };

        // If no parent is selected, add at the top level
        if (selectedParentPath.length === 0) {
            newElement.orderIndex = newStructure.formElements.length;
            newStructure.formElements.push(newElement);
        } else {
            // Navigate to the parent element
            let currentLevel = newStructure.formElements;
            let parent: ExtendedFormElement | undefined;

            for (const id of selectedParentPath) {
                parent = currentLevel.find(elem => elem.id === id);
                if (!parent) break;
                currentLevel = parent.children as ExtendedFormElement[];
            }

            if (parent) {
                newElement.orderIndex = parent.children.length;
                parent.children.push(newElement);
            }
        }

        setStructure(newStructure);
        setNewItemName("");
        message.success(t('ficTemplateConfig.elementAdded'));
    };

    // Toggle checkbox values for leaf nodes
    const toggleOption = (path: string[], optionIndex: number) => {
        const newStructure = { ...structure };

        // Navigate to the element
        let currentLevel = newStructure.formElements;
        let element: ExtendedFormElement | undefined;

        for (const id of path) {
            element = currentLevel.find(elem => elem.id === id);
            if (!element) return;
            currentLevel = element.children as ExtendedFormElement[];
        }

        if (element) {
            element.dataType = optionIndex === 0 ? 'boolean' : 'text';
            setStructure(newStructure);
        }
    };

    // Path selection for adding new elements
    const handlePathSelection = (value: string) => {
        const path = value ? value.split(",") : [];
        setSelectedParentPath(path);
        // Clear any previous errors when selecting a new parent
        setNewItemNameError("");
    };

    // Get all possible paths for dropdown
    const getAllPaths = () => {
        const paths: { display: string, path: string[] }[] = [
            { display: t('ficTemplateConfig.topLevel'), path: [] }
        ];

        const traverse = (elements: ExtendedFormElement[], currentPath: string[] = [], depth = 0) => {
            elements.forEach(elem => {
                const newPath = [...currentPath, elem.id];

                // Create a more readable display format
                // For depths > 0, add indentation and truncate long names
                let displayName = elem.name;
                if (displayName.length > 150) {
                    displayName = displayName.substring(0, 150) + '...';
                }

                // Add depth indicator for better hierarchy visualization
                const prefix = depth > 0 ? '└─ '.padStart((depth * 2) + 3, '  ') : '';

                paths.push({
                    display: `${prefix}${displayName}`,
                    path: newPath
                });

                traverse(elem.children as ExtendedFormElement[], newPath, depth + 1);
            });
        };

        traverse(structure.formElements);
        return paths;
    };

    const startEditing = (path: string[], name: string) => {
        setEditingElementPath(path);
        setEditingElementName(name);
        setEditingElementNameError("");
    };

    // Save edited element name
    const saveElementName = () => {
        if (editingElementPath.length === 0) return;

        // Get the parent path (all but the last element)
        const parentPath = editingElementPath.slice(0, -1);

        const validation = validateElementName(editingElementName, parentPath);
        if (!validation.isValid) {
            setEditingElementNameError(validation.error);
            return;
        }

        setEditingElementNameError("");

        const newStructure = { ...structure };
        let currentLevel = newStructure.formElements;
        let element: ExtendedFormElement | undefined;

        // Navigate to the element using path
        for (let i = 0; i < editingElementPath.length; i++) {
            const id = editingElementPath[i];
            element = currentLevel.find(elem => elem.id === id);
            if (!element) return;
            if (i < editingElementPath.length - 1) {
                currentLevel = element.children as ExtendedFormElement[];
            }
        }

        if (element) {
            element.name = editingElementName.trim();
            setStructure(newStructure);
            message.success(t('ficTemplateConfig.elementNameUpdated'));
        }

        // Reset editing state
        setEditingElementPath([]);
        setEditingElementName("");
    };

    // Cancel editing
    const cancelEditing = () => {
        setEditingElementPath([]);
        setEditingElementName("");
        setEditingElementNameError("");
    };

    // Delete an element
    const deleteElement = (path: string[]) => {
        if (path.length === 0) return;

        const newStructure = { ...structure };

        // Handle top-level deletion
        if (path.length === 1) {
            const elementId = path[0];
            newStructure.formElements = newStructure.formElements.filter(elem => elem.id !== elementId);
            // Update orderIndex for remaining elements
            newStructure.formElements.forEach((elem, idx) => {
                elem.orderIndex = idx;
            });
            setStructure(newStructure);
            message.success(t('ficTemplateConfig.elementDeleted'));
            return;
        }

        // For nested elements, navigate to the parent
        const parentPath = path.slice(0, -1);
        const elementId = path[path.length - 1];

        let currentLevel = newStructure.formElements;
        let parent: ExtendedFormElement | undefined;

        for (const id of parentPath) {
            parent = currentLevel.find(elem => elem.id === id);
            if (!parent) return;
            currentLevel = parent.children as ExtendedFormElement[];
        }

        if (parent) {
            // Remove the element from parent's children
            parent.children = parent.children.filter(elem => (elem as ExtendedFormElement).id !== elementId);
            // Update orderIndex for remaining children
            parent.children.forEach((elem, idx) => {
                elem.orderIndex = idx;
            });
            setStructure(newStructure);
            message.success(t('ficTemplateConfig.elementDeleted'));
        }
    };

    // Prepare structure for submission (remove IDs)
    const prepareForSubmission = (extendedStructure: ExtendedFICStructure): FICStructure => {
        const removeIds = (elements: ExtendedFormElement[]): FormElement[] => {
            return elements.map(elem => {
                const { id, ...rest } = elem;
                return {
                    ...rest,
                    children: removeIds(elem.children as ExtendedFormElement[])
                };
            });
        };

        return {
            systemId: extendedStructure.systemId,
            name: extendedStructure.name,
            status: 1, //draft
            formElements: removeIds(extendedStructure.formElements)
        };
    };

    const pathOptions = getAllPaths().map((option) => ({
        value: option.path.join(","),
        label: option.display,
    }));

    const isStructureEmpty = () => {
        return structure.formElements.length === 0;
    };

    // Validate leaf nodes have data type
    const validateLeafNodesHaveDataType = useCallback((): { isValid: boolean, invalidCount: number, invalidNodeIds: Set<string> } => {
        const invalidNodes = new Set<string>();

        const checkElements = (elements: ExtendedFormElement[]) => {
            elements.forEach(elem => {
                if (elem.children.length === 0) {
                    if (elem.dataType === null || elem.dataType === undefined) {
                        invalidNodes.add(elem.id);
                    }
                } else {
                    checkElements(elem.children as ExtendedFormElement[]);
                }
            });
        };

        checkElements(structure.formElements);
        setInvalidLeafNodes(invalidNodes);

        return {
            isValid: invalidNodes.size === 0,
            invalidCount: invalidNodes.size,
            invalidNodeIds: invalidNodes
        };
    }, [structure]);

    // Handle form submission
    const handleSubmit = async () => {
        try {
            if (isStructureEmpty()) {
                message.error(t('ficTemplateConfig.emptyFormError'));
                return;
            }

            const validation = validateLeafNodesHaveDataType();
            if (!validation.isValid) {
                setShowValidationWarnings(true);
                message.error(t('ficTemplateConfig.dataTypeError', { invalidCount: validation.invalidCount }));
                return;
            }

            setLoading(true);
            const submissionData = prepareForSubmission(structure);
            await AxiosClient.post("/Form/save", submissionData);
            message.success(t('ficTemplateConfig.formSaved'));
            setShowValidationWarnings(false);
            navigate(-1);
        } catch (error) {
            message.error(t('ficTemplateConfig.formSaveFailed'));
        } finally {
            setLoading(false);
        }
    };

    // Render the elements recursively
    const renderElements = (elements: ExtendedFormElement[], path: string[] = [], depth = 0) => {
        return elements.map(elem => {
            const currentPath = [...path, elem.id];
            const isLeaf = elem.children.length === 0;
            const isEditing = editingElementPath.length > 0 &&
                editingElementPath.join() === currentPath.join();
            const isInvalid = isLeaf && invalidLeafNodes.has(elem.id) && showValidationWarnings;

            return (
                <React.Fragment key={elem.id}>
                    <tr className={`
                        ${isLeaf ? "bg-white" : "bg-gray-100"} 
                        ${isInvalid ? "border-l-4 border-red-500" : ""}
                        hover:bg-gray-50 transition-colors
                    `}>
                        <td className="px-4 py-3">
                            <div className="flex items-center">
                                <div style={{ marginLeft: `${depth * 20}px` }} className="w-full">
                                    {isEditing ? (
                                        <div className="flex flex-col w-full">
                                            <div className="flex items-center space-x-2 w-full">
                                                <TextArea
                                                    allowClear
                                                    autoSize={{ minRows: 1, maxRows: 6 }}
                                                    value={editingElementName}
                                                    onChange={(e) => setEditingElementName(e.target.value)}
                                                    autoFocus
                                                    status={editingElementNameError ? "error" : ""}
                                                    className="flex-grow"
                                                />
                                                <Button
                                                    type="primary"
                                                    shape="circle"
                                                    icon={<SaveOutlined />}
                                                    onClick={saveElementName}
                                                    size="small"
                                                />
                                                <Button
                                                    type="default"
                                                    shape="circle"
                                                    icon={<CloseOutlined />}
                                                    onClick={cancelEditing}
                                                    size="small"
                                                    danger
                                                />
                                            </div>
                                            {editingElementNameError && (
                                                <div className="text-red-500 text-xs mt-1">{editingElementNameError}</div>
                                            )}
                                        </div>
                                    ) : (
                                        <div className="flex items-center">
                                            {!isLeaf && (
                                                <span className="text-gray-400 mr-2">
                                                    <FormOutlined />
                                                </span>
                                            )}
                                            <Tooltip title={elem.name} placement="topLeft">
                                                <span className={`
                                                    ${isLeaf ? "text-gray-700" : "font-medium text-gray-800"} 
                                                    truncate max-w-xl block
                                                `}>
                                                    {elem.name}
                                                </span>
                                            </Tooltip>
                                            {isInvalid && (
                                                <Tooltip title={t('ficTemplateConfig.dataTypeRequired')}>
                                                    <span className="text-red-500 ml-2">⚠️</span>
                                                </Tooltip>
                                            )}
                                        </div>
                                    )}
                                </div>
                            </div>
                        </td>
                        <td className="px-2 py-3 text-center">
                            {isLeaf && (
                                <Checkbox
                                    checked={elem.dataType === 'boolean'}
                                    onChange={() => toggleOption(currentPath, 0)}
                                    className={isInvalid ? "border-red-400" : ""}
                                />
                            )}
                        </td>
                        <td className="px-2 py-3 text-center">
                            {isLeaf && (
                                <Checkbox
                                    checked={elem.dataType === 'text'}
                                    onChange={() => toggleOption(currentPath, 1)}
                                    className={isInvalid ? "border-red-400" : ""}
                                />
                            )}
                        </td>
                        <td className="px-2 py-3">
                            <div className="flex justify-center space-x-1">
                                <Tooltip title={t('ficTemplateConfig.editTooltip')}>
                                    <Button
                                        type="text"
                                        icon={<EditOutlined />}
                                        onClick={() => startEditing(currentPath, elem.name)}
                                        size="small"
                                    />
                                </Tooltip>
                                <Tooltip title={t('ficTemplateConfig.deleteTooltip')}>
                                    <Button
                                        type="text"
                                        danger
                                        icon={<DeleteOutlined />}
                                        onClick={() => deleteElement(currentPath)}
                                        size="small"
                                    />
                                </Tooltip>
                            </div>
                        </td>
                    </tr>
                    {elem.children.length > 0 && renderElements(elem.children as ExtendedFormElement[], currentPath, depth + 1)}
                </React.Fragment>
            );
        });
    };

    return (
        <Spin spinning={loading} tip={t('processing')}>
            <Layout style={{ minHeight: '100vh', backgroundColor: '#f5f7fa', borderRadius:'10px' }}>
                <PageHeader
                    title={t('ficTemplateConfig.pageTitle')}
                />
                <div className='px-4 py-8'>
                    {/* <Card className='w-full shadow-xl rounded-2xl p-8 mx-auto mt-8'> */}
                        <Card className="mb-6 shadow-sm">
                            <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between mb-4">
                                <h1 className="text-2xl font-bold text-gray-800 mb-4 sm:mb-0">{t('ficTemplateConfig.pageTitle')}</h1>
                                <div className="flex w-full sm:w-auto">
                                    <Input
                                        value={formName}
                                        onChange={(e) => setFormName(e.target.value)}
                                        placeholder={t('ficTemplateConfig.formNamePlaceholder')}
                                        className="flex-grow mr-2"
                                        prefix={<FormOutlined className="text-gray-400" />}
                                    />
                                    <Button
                                        type="primary"
                                        onClick={updateFormName}
                                    >
                                        {t('ficTemplateConfig.update')}
                                    </Button>
                                </div>
                            </div>
                            <div className="bg-yellow-50 p-3 rounded-lg border border-blue-200 mb-4">
                                <h2 className="text-xl font-semibold text-blue-800">{t('ficTemplateConfig.currentForm')}: {structure.name}</h2>
                                <text className="text-blue-600 text-sm mt-1">
                                    {t('ficTemplateConfig.buildFormDescription')}
                                </text>
                            </div>
                        </Card>

                        {/* Add new element card */}
                        <Card title={t('ficTemplateConfig.addElementTitle')} className="mb-6 shadow-sm bg-green-50">
                            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                                <div className="md:col-span-1">
                                    <label className="block text-sm font-medium text-gray-700 mb-1">
                                        {t('ficTemplateConfig.parentElementLabel')}
                                    </label>
                                    <Select
                                        onChange={handlePathSelection}
                                        className="w-full"
                                        placeholder={t('ficTemplateConfig.parentElementPlaceholder')}
                                        options={pathOptions}
                                        defaultValue={pathOptions[0].value}
                                        showSearch
                                        optionFilterProp="label"
                                        optionRender={(option) => (
                                            <div className="font-mono whitespace-pre">{option.label}</div>
                                        )}
                                    />
                                </div>
                                <div className="md:col-span-2">
                                    <label className="block text-sm font-medium text-gray-700 mb-1">
                                        {t('ficTemplateConfig.elementNameLabel')}
                                    </label>
                                    <div className="flex space-x-2">
                                        <TextArea
                                            value={newItemName}
                                            autoSize={{ minRows: 1, maxRows: 3 }}
                                            onChange={(e) => setNewItemName(e.target.value)}
                                            placeholder={t('ficTemplateConfig.elementNamePlaceholder')}
                                            status={newItemNameError ? "error" : ""}
                                            className="flex-grow"
                                        />
                                        <Button
                                            type="primary"
                                            icon={<PlusOutlined />}
                                            onClick={addElement}
                                        >
                                            {t('ficTemplateConfig.add')}
                                        </Button>
                                    </div>
                                    {newItemNameError && (
                                        <div className="text-red-500 text-xs mt-1">{newItemNameError}</div>
                                    )}
                                </div>
                            </div>
                        </Card>

                        {/* Table */}
                        <Card className="mb-6 shadow-sm overflow-hidden">
                            <div className="overflow-x-auto">
                                <table className="min-w-full divide-y divide-gray-200">
                                    <thead className="bg-gray-50">
                                        <tr>
                                            <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                                {t('ficTemplateConfig.elementName')}
                                            </th>
                                            <th className="w-20 px-2 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                                                {t('ficTemplateConfig.boolean')}
                                            </th>
                                            <th className="w-20 px-2 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                                                {t('ficTemplateConfig.text')}
                                            </th>
                                            <th className="w-24 px-2 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                                                {t('actions')}
                                            </th>
                                        </tr>
                                    </thead>
                                    <tbody className="bg-white divide-y divide-gray-200">
                                        {structure.formElements.length > 0 ? (
                                            renderElements(structure.formElements)
                                        ) : (
                                            <tr>
                                                <td colSpan={4} className="px-6 py-8 text-center text-gray-500">
                                                    {t('ficTemplateConfig.noElementsAdded')}
                                                </td>
                                            </tr>
                                        )}
                                    </tbody>
                                </table>
                            </div>
                        </Card>

                        <div className="flex justify-center mt-6">
                            <Button
                                type="primary"
                                size="large"
                                icon={<SaveOutlined />}
                                onClick={handleSubmit}
                                disabled={isStructureEmpty()}
                                className="px-8"
                            >
                                {t('ficTemplateConfig.saveTemplate')}
                            </Button>
                        </div>
                    {/* </Card> */}
                </div>
            </Layout>
        </Spin>
    );
};

export default FICTemplateConfig;