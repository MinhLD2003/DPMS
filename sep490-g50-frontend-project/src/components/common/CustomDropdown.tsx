import React, { useState, useMemo } from "react";
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import type { MenuProps } from "antd";
import { Button, Dropdown, Input, Menu } from "antd";

interface CustomDropdownProps {
    items: string[];
    value?: string;
    defaultLabel?: string;
    disabled?: boolean;
    danger?: boolean;
    onChange?: (value: string) => void;
    onBlur?: () => void;
}

const CustomDropdown: React.FC<CustomDropdownProps> = ({
    items,
    value,
    defaultLabel = null,
    disabled = false,
    danger = false,
    onChange,
}) => {
    const [search, setSearch] = useState("");
    const displayValue = value || defaultLabel;

    const filteredItems = useMemo(
        () => items.filter(item => item.toLowerCase().includes(search.toLowerCase())),
        [items, search]
    );

    const handleSelect = (item: string) => {
        if (onChange) {
            onChange(item);
        }
    };

    const menu = (
        <Menu>
            <div className="px-2 py-1">
                <Input
                    placeholder="Search email..."
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                    allowClear
                />
            </div>
            {filteredItems.map((item, index) => (
                <Menu.Item key={index} onClick={() => handleSelect(item)}>
                    {item}
                </Menu.Item>
            ))}
            {filteredItems.length === 0 && (
                <Menu.Item disabled>No results found</Menu.Item>
            )}
        </Menu>
    );

    return (
        <Dropdown overlay={menu} trigger={["click"]}>
            <Button disabled={disabled} danger={danger} className="w-full flex justify-between items-center">
                <span>{displayValue}</span>
                <ExpandMoreIcon />
            </Button>
        </Dropdown>
    );
};

export default CustomDropdown;
