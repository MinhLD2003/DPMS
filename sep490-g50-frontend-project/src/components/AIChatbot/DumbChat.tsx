import React, { useState } from 'react';
import { Input, Button, Spin, Alert, Card } from 'antd';
import { MessageOutlined, CloseOutlined } from '@ant-design/icons';
import AxiosClient from '../../configs/axiosConfig';

interface ChatMessage {
  sender: 'user' | 'ai';
  text: string;
}

const Chatbot: React.FC = () => {
  const [isOpen, setIsOpen] = useState(false);
  const [prompt, setPrompt] = useState('');
  const [chatHistory, setChatHistory] = useState<ChatMessage[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!prompt.trim()) return;

    const userMessage: ChatMessage = { sender: 'user', text: prompt };
    setChatHistory((prev) => [...prev, userMessage]);
    setPrompt('');
    setLoading(true);
    setError(null);

    try {
      const response = await AxiosClient.post('/OpenAI/Ask', prompt, {
        headers: { 'Content-Type': 'application/json' }
      });

      const aiMessage: ChatMessage = {
        sender: 'ai',
        text: response.data?.response?.toString() || 'No response received'
      };
      setChatHistory((prev) => [...prev, aiMessage]);
    } catch (error: any) {
      const errorMessage: ChatMessage = {
        sender: 'ai',
        text: error.response?.data?.message || 'Error: Could not get response'
      };
      setChatHistory((prev) => [...prev, errorMessage]);
      setError(error.response?.data?.message || 'Failed to get response from AI');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ position: 'fixed', bottom: '20px', right: '20px', zIndex: 1000 }}>
      {!isOpen ? (
        <Button
          type="primary"
          shape="circle"
          icon={<MessageOutlined />}
          onClick={() => setIsOpen(true)}
          size="large"
          style={{ width: '50px', height: '50px' }}
        />
      ) : (
        <Card
          title={
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <span>AI Assistant</span>
              <Button
                type="text"
                icon={<CloseOutlined />}
                onClick={() => setIsOpen(false)}
                size="small"
              />
            </div>
          }
          style={{
            width: '500px',
            boxShadow: '0 4px 8px rgba(0,0,0,0.1)',
            borderRadius: '8px',
            maxHeight: '500px',
            display: 'flex',
            flexDirection: 'column'
          }}
          bodyStyle={{ padding: '12px', flex: 1 }}
        >
          <div
            style={{
              height: '300px',
              overflowY: 'auto',
              marginBottom: '12px',
              padding: '8px'
            }}
          >
            {chatHistory.map((msg, index) => (
              <div
                key={index}
                style={{
                  textAlign: msg.sender === 'user' ? 'right' : 'left',
                  marginBottom: 8,
                  backgroundColor: msg.sender === 'user' ? '#e6f7ff' : '#f5f5f5',
                  padding: '8px 12px',
                  borderRadius: '12px',
                  maxWidth: '80%',
                  marginLeft: msg.sender === 'user' ? 'auto' : '0',
                  marginRight: msg.sender === 'ai' ? 'auto' : '0',
                }}
              >
                <div style={{ fontSize: '12px', color: '#888', marginBottom: '4px' }}>
                  {msg.sender === 'user' ? 'You' : 'AI'}
                </div>
                <div style={{ wordBreak: 'break-word' }}>
                  {msg.text}
                </div>
              </div>
            ))}
            {loading && (
              <div style={{ textAlign: 'center', padding: '10px' }}>
                <Spin size="small" />
              </div>
            )}
          </div>

          {error && (
            <Alert
              type="error"
              message={error}
              showIcon
              style={{ marginBottom: '12px' }}
            />
          )}

          <Input.Group compact style={{ display: 'flex' }}>
            <Input
              value={prompt}
              onChange={(e) => setPrompt(e.target.value)}
              onPressEnter={handleSubmit}
              placeholder="Ask something..."
              disabled={loading}
              style={{ flex: 1 }}
              size="middle"
            />
            <Button
              type="primary"
              onClick={handleSubmit}
              disabled={loading}
              size="middle"
            >
              Send
            </Button>
          </Input.Group>
        </Card>
      )}
    </div>
  );
};

export default Chatbot;
