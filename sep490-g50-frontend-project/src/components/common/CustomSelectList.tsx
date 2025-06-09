import React, { useState, useEffect, useMemo } from "react";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import type { MenuProps } from "antd";
import { Button, Dropdown, Checkbox, Space, Input } from "antd";
import { CloseOutlined } from "@ant-design/icons";

interface CustomDropdownProps {
    items: string[];
    defaultLabel?: string;
    disabled?: boolean;
    danger?: boolean;
    onChange?: (values: string[]) => void;
    value?: string[];
    onBlur?: () => void;
}

const CustomSelectList: React.FC<CustomDropdownProps> = ({
    items,
    defaultLabel = "Select",
    disabled = false,
    danger = false,
    onChange,
    value = [],
}) => {
    const [selectedItems, setSelectedItems] = useState<string[]>(value || []);
    const [searchTerm, setSearchTerm] = useState("");

    useEffect(() => {
        if (value) {
            setSelectedItems(value);
        }
    }, [value]);

    const handleSelect = (item: string, checked: boolean) => {
        let newSelectedItems: string[];

        if (checked) {
            newSelectedItems = [...selectedItems, item];
        } else {
            newSelectedItems = selectedItems.filter((i) => i !== item);
        }

        setSelectedItems(newSelectedItems);

        if (onChange) {
            onChange(newSelectedItems);
        }
    };

    const removeItem = (item: string, e: React.MouseEvent) => {
        e.stopPropagation();
        const newSelectedItems = selectedItems.filter((i) => i !== item);
        setSelectedItems(newSelectedItems);

        if (onChange) {
            onChange(newSelectedItems);
        }
    };

    // Filtered items based on search
    const filteredItems = useMemo(
        () =>
            items.filter((item) =>
                item.toLowerCase().includes(searchTerm.toLowerCase())
            ),
        [items, searchTerm]
    );

    // Combine search input and filtered items into dropdown content
    const dropdownRender = () => (
        <div className="bg-white rounded-md shadow-md border border-gray-200 px-3 py-2 min-w-[200px]">
            <Input
                placeholder="Search..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="mb-2"
                allowClear
            />
            <div className="max-h-60 overflow-auto pr-1">
                {filteredItems.length ? (
                    filteredItems.map((item, index) => (
                        <div key={index} className="py-1">
                            <Checkbox
                                checked={selectedItems.includes(item)}
                                onChange={(e) =>
                                    handleSelect(item, e.target.checked)
                                }
                            >
                                {item}
                            </Checkbox>
                        </div>
                    ))
                ) : (
                    <div className="text-gray-400 text-sm px-2 py-1">
                        No results
                    </div>
                )}
            </div>
        </div>
    );

    return (
        <div className="w-full">
            <Dropdown
                dropdownRender={dropdownRender}
                trigger={["click"]}
            >
                <Button
                    disabled={disabled}
                    danger={danger}
                    className="w-full flex justify-between items-center"
                >
                    <span>
                        {selectedItems.length > 0
                            ? `${selectedItems.length} selected`
                            : defaultLabel}
                    </span>
                    <ExpandMoreIcon />
                </Button>
            </Dropdown>

            {/* Display selected tags */}
            {selectedItems.length > 0 && (
                <div className="mt-2 flex flex-wrap gap-2">
                    {selectedItems.map((item) => (
                        <div
                            key={item}
                            className="bg-gray-100 px-2 py-1 rounded-md flex items-center gap-1"
                        >
                            <span className="text-sm">{item}</span>
                            <CloseOutlined
                                className="cursor-pointer text-xs"
                                onClick={(e) => removeItem(item, e)}
                            />
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
};

export default CustomSelectList;
