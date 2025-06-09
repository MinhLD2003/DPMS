import { GoogleGenAI } from '@google/genai';
import { Alert, Button, Card, Input, List, Spin, Avatar, Typography, Tooltip, Badge } from 'antd';
import React, { useEffect, useState, useRef } from 'react';
import { CloseOutlined, MessageOutlined, SendOutlined, DeleteOutlined, QuestionCircleOutlined } from '@ant-design/icons';
import { read, utils } from 'xlsx';
import { validateEnv } from '../../configs/env.config';

const { Text, Title } = Typography;

interface ChatMessage {
    id: string;
    role: 'user' | 'model';
    content: string;
    timestamp: Date;
}

interface SeedData {
    question: string;
    answer: string;
}

const NewChat: React.FC = () => {
    const [messages, setMessages] = useState<ChatMessage[]>([]);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [inputValue, setInputValue] = useState('');
    const [seedData, setSeedData] = useState<SeedData[]>([]);
    const [isOpen, setIsOpen] = useState(false);
    const [isSeedDataLoaded, setSeedDataLoaded] = useState(false);
    const messagesEndRef = useRef<HTMLDivElement>(null);
    const chatListRef = useRef<HTMLDivElement>(null);
    const MAX_CONTEXT_ITEMS = 30;

    const scrollToBottom = () => {
        messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    };

    useEffect(() => {
        scrollToBottom();
    }, [messages]);

    const generateMessageId = () => {
        return `msg-${Date.now()}-${Math.random().toString(36).substring(2, 9)}`;
    };

    const getRelevantContext = (question: string, seedData: SeedData[]) => {
        // Clean and prepare the question
        const cleanQuestion = question.toLowerCase().trim()
            .replace(/[.,?!;:(){}[\]]/g, ' ')  // Replace punctuation with spaces
            .replace(/\s+/g, ' ');            // Normalize spaces

        // Break into logical phrases (not just words)
        const phrases = cleanQuestion.split(/\b(and|or|but|because|when|how|what|why)\b/)
            .map(phrase => phrase.trim())
            .filter(phrase => phrase.length > 2 && !['and', 'or', 'but', 'because', 'when', 'how', 'what', 'why'].includes(phrase));

        // Extract potentially meaningful keywords (ignoring common stopwords)
        const stopwords = new Set(['the', 'a', 'an', 'is', 'are', 'was', 'were', 'be', 'have', 'has',
            'had', 'do', 'does', 'did', 'can', 'could', 'will', 'would', 'should', 'may', 'might',
            'must', 'to', 'of', 'in', 'on', 'at', 'by', 'for', 'with', 'about', 'from']);

        const keywords = cleanQuestion
            .split(' ')
            .filter(word => word.length > 2 && !stopwords.has(word));

        // Calculate relevance scores for each item in the seed data
        const scoredData = seedData.map(item => {
            const cleanQuestion = item.question.toLowerCase();
            const cleanAnswer = item.answer.toLowerCase();

            let score = (phrases.length > 0) ? 0 : 1; // Base score

            // Score based on phrases (more weight on phrase matches)
            phrases.forEach(phrase => {
                // Full phrase match is highly valuable
                if (cleanQuestion.includes(phrase)) score += 10;
                if (cleanAnswer.includes(phrase)) score += 5;

                // Partial phrase matches also have value
                const phraseWords = phrase.split(' ');
                if (phraseWords.length > 1) {
                    const partialMatches = phraseWords.filter(word =>
                        cleanQuestion.includes(word) || cleanAnswer.includes(word)
                    ).length;

                    score += (partialMatches / phraseWords.length) * 3;
                }
            });

            // Score based on individual keywords
            keywords.forEach(keyword => {
                if (cleanQuestion.includes(keyword)) score += 2;
                if (cleanAnswer.includes(keyword)) score += 1;

                // Bonus for matching at word boundaries (whole word match)
                const questionWordMatch = new RegExp(`\\b${keyword}\\b`).test(cleanQuestion);
                const answerWordMatch = new RegExp(`\\b${keyword}\\b`).test(cleanAnswer);

                if (questionWordMatch) score += 1;
                if (answerWordMatch) score += 0.5;
            });

            // Additional weights for question title matches
            // This prioritizes QA pairs where the core question matches
            const questionTitle = cleanQuestion.split('?')[0];
            if (questionTitle && cleanQuestion.includes(questionTitle)) {
                score += 3;
            }

            return { item, score };
        });

        // Sort by score (highest first) and return top N
        return scoredData
            .sort((a, b) => b.score - a.score)
            .slice(0, MAX_CONTEXT_ITEMS)
            .map(scored => scored.item);
    };

    const loadSeedData = async (file: File) => {
        try {
            const buffer = await file.arrayBuffer();
            const workbook = read(buffer);
            const worksheet = workbook.Sheets[workbook.SheetNames[0]];
            const jsonData = utils.sheet_to_json<SeedData>(worksheet);
            setSeedData(jsonData);
            setSeedDataLoaded(true);
            // console.log('Loaded seed data:', jsonData);

            return jsonData.map(item =>
                `Q: ${item.question}\nA: ${item.answer}`
            ).join('\n\n');
        } catch (error) {
            setError('Error loading seed data');
            return '';
        }
    };

    useEffect(() => {
        const loadInitialSeedData = async () => {
            try {
                const response = await fetch('/300QA.xlsx');
                const blob = await response.blob();
                const file = new File([blob], 'seed-data.xlsx', { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
                await loadSeedData(file);

                // Add welcome message after data is loaded
                setMessages([{
                    id: generateMessageId(),
                    role: 'model',
                    content: 'Hello! I\'m your AI assistant provided by DPMS to guide you through the system workflow. How can I help you today?',
                    timestamp: new Date()
                }]);
            } catch (error) {
                setError('Error loading initial seed data');
            }
        };

        loadInitialSeedData();
    }, []);


    const handleChat = async (userInput: string) => {
        if (!userInput.trim()) return;

        const userMessage: ChatMessage = {
            id: generateMessageId(),
            role: 'user',
            content: userInput,
            timestamp: new Date()
        };

        setMessages(prev => [...prev, userMessage]);

        try {
            setIsLoading(true);
            setError(null);

            const apiKey = validateEnv().geminiApiKey;
            if (!apiKey) {
                throw new Error('Google API key is not configured');
            }

            const ai = new GoogleGenAI({
                apiKey: apiKey
            });

            const relevantContext = getRelevantContext(userInput, seedData);

            const config = {
                responseMimeType: 'text/plain',
            };

            const model = 'gemini-1.5-flash';

            const contents = [
                {
                    role: 'user',
                    parts: [
                        {
                            text: relevantContext.length > 0
                                ? `Use this context for answering:\n${relevantContext.map(item =>
                                    `Q: ${item.question}\nA: ${item.answer}`
                                ).join('\n\n')}\n\nUser question: ${userInput}`
                                : userInput,
                        },
                    ],
                }
            ];

            // Stream the response
            const response = await ai.models.generateContentStream({
                model,
                config,
                contents,
            });

            let fullResponse = '';
            for await (const chunk of response) {
                fullResponse += chunk.text;
            }

            // Add AI response
            setMessages(prev => [
                ...prev,
                {
                    id: generateMessageId(),
                    role: 'model',
                    content: fullResponse,
                    timestamp: new Date()
                }
            ]);

        } catch (err) {
            setError(err instanceof Error ? err.message : 'An error occurred');
        } finally {
            setIsLoading(false);
        }
    };

    const clearChat = () => {
        setMessages([{
            id: generateMessageId(),
            role: 'model',
            content: 'Chat history cleared. How can I help you?',
            timestamp: new Date()
        }]);
    };

    const formatTime = (date: Date) => {
        return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    };

    return (
        <div style={{ position: 'fixed', bottom: '20px', right: '20px', zIndex: 1000 }}>
            {!isOpen ? (
                <Badge count={messages.length > 0 ? 1 : 0} dot>
                    <Button
                        type="primary"
                        shape="circle"
                        icon={<MessageOutlined />}
                        onClick={() => setIsOpen(true)}
                        size="large"
                        style={{ width: '60px', height: '60px', boxShadow: '0 4px 12px rgba(0,0,0,0.15)' }}
                    />
                </Badge>
            ) : (
                <Card
                    title={
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                                <Avatar style={{ backgroundColor: '#1890ff' }}>AI</Avatar>
                                <Title level={5} style={{ margin: 0 }}>AI Assistant</Title>
                            </div>
                            <div>
                                <Tooltip title="Clear conversation">
                                    <Button
                                        type="text"
                                        icon={<DeleteOutlined />}
                                        onClick={clearChat}
                                        size="small"
                                        style={{ marginRight: '8px' }}
                                    />
                                </Tooltip>
                                <Button
                                    type="text"
                                    icon={<CloseOutlined />}
                                    onClick={() => setIsOpen(false)}
                                    size="small"
                                />
                            </div>
                        </div>
                    }
                    style={{
                        width: '580px',
                        boxShadow: '0 6px 16px rgba(0,0,0,0.12)',
                        borderRadius: '12px',
                        minHeight: '400px',
                        maxHeight: '600px',
                        display: 'flex',
                        flexDirection: 'column'
                    }}
                    bodyStyle={{ padding: '12px', flex: 1, overflow: 'hidden', display: 'flex', flexDirection: 'column' }}
                >
                    <div
                        ref={chatListRef}
                        style={{
                            flexGrow: 1,
                            overflowY: 'auto',
                            marginBottom: '12px',
                            padding: '4px'
                        }}
                    >
                        <List
                            itemLayout="horizontal"
                            dataSource={messages}
                            renderItem={(message) => (
                                <List.Item style={{
                                    padding: '4px 0',
                                    border: 'none'
                                }}>
                                    <div style={{
                                        display: 'flex',
                                        flexDirection: 'column',
                                        alignItems: message.role === 'user' ? 'flex-end' : 'flex-start',
                                        width: '100%'
                                    }}>
                                        <div
                                            style={{
                                                backgroundColor: message.role === 'user' ? '#e6f7ff' : '#f5f5f5',
                                                padding: '10px 14px',
                                                borderRadius: message.role === 'user' ? '18px 18px 4px 18px' : '18px 18px 18px 4px',
                                                maxWidth: '85%',
                                                boxShadow: '0 1px 2px rgba(0,0,0,0.05)',
                                                marginBottom: '2px'
                                            }}
                                        >
                                            <div style={{ wordBreak: 'break-word' }}>
                                                {message.content}
                                            </div>
                                        </div>
                                        <Text type="secondary" style={{ fontSize: '11px', margin: '2px 4px' }}>
                                            {formatTime(message.timestamp)}
                                        </Text>
                                    </div>
                                </List.Item>
                            )}
                        />
                        <div ref={messagesEndRef} />
                    </div>

                    {isLoading && (
                        <div style={{ textAlign: 'center', padding: '6px' }}>
                            <Spin size="small" />
                            <Text type="secondary" style={{ display: 'block', fontSize: '12px', marginTop: '4px' }}>
                                Thinking...
                            </Text>
                        </div>
                    )}

                    {error && (
                        <Alert
                            message="Error"
                            description={error}
                            type="error"
                            showIcon
                            style={{ marginBottom: '12px' }}
                            closable
                        />
                    )}

                    <div style={{
                        display: 'flex',
                        gap: '8px',
                        padding: '8px',
                        borderTop: '1px solid #f0f0f0',
                        backgroundColor: '#fafafa'
                    }}>
                        <Input
                            value={inputValue}
                            onChange={(e) => setInputValue(e.target.value)}
                            placeholder="Type your message..."
                            disabled={isLoading}
                            onPressEnter={(e) => {
                                e.preventDefault();
                                handleChat(inputValue);
                                setInputValue('');
                            }}
                            style={{
                                flex: 1,
                                borderRadius: '18px',
                                padding: '8px 16px'
                            }}
                            autoFocus
                        />
                        <Button
                            type="primary"
                            icon={<SendOutlined />}
                            disabled={!inputValue.trim() || isLoading}
                            onClick={() => {
                                handleChat(inputValue);
                                setInputValue('');
                            }}
                            shape="circle"
                        />
                    </div>
                </Card>
            )}
        </div>
    );
};

export default NewChat;