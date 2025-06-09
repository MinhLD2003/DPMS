import React from "react";
import { Table, Form, Checkbox, Input } from "antd";

interface FICFormTableProps {
    dataSource: any[];
    form: any;
}

const Text = Input;

const FICFormTable: React.FC<FICFormTableProps> = ({ dataSource, form }) => {
    const columns = [
        {
            title: "Nội dung khảo sát(Đánh giá hệ thống có thu thập, lưu trữ các Dữ liệu cá nhân sau không? Nếu có: √, Nếu không: x)",
            dataIndex: "name", key: "name",
            width: "80%"
        },

        {
            title: "Input",
            dataIndex: "input", key: "input",
            align: 'center' as 'center', width: "20%",
            render: (input: string | null, record: any) => {
                if (record.hasChildren) return null;
                if (record.type === "Boolean") {
                    return (
                        <Form.Item name={input || undefined} valuePropName="checked" initialValue={false} style={{ marginBottom: 0 }}>
                            <Checkbox />
                        </Form.Item>
                    );
                }
                if (record.type === "Text") {
                    return (
                        <Form.Item name={input || undefined} style={{ marginBottom: 0 }}>
                            <Text />
                        </Form.Item>
                    );
                }
                return null;
            }
        },
    ];

    return (
        <Table
            columns={columns}
            dataSource={dataSource}
            pagination={false}
            bordered
            rowClassName={(record) => (record.hasChildren ? "ml-12 font-bold bg-gray-200" : "italic")}
        />
    );
};

export default FICFormTable;
