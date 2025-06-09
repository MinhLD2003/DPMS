import { Modal } from "antd";

const confirmDelete = (
  onConfirm: () => void,
  title: string,
  content: string,
  okText: string,
  cancelText: string
) => {
  Modal.confirm({
    title,
    content,
    okText,
    okType: "danger",
    cancelText,
    onOk: onConfirm,
  });
};

export default confirmDelete;
